using HybridCLR;
#if UNITY_EDITOR
using HybridCLR.Editor.Settings;
#endif
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

public class Loader : MonoBehaviour
{
    public Loader()
    {
        if (instance)
            return;

        instance = this;
    }

    private static Loader instance;
    private static ConcurrentDictionary<string, byte[]> dataBytes = new();
    private static ConcurrentDictionary<string, byte[]> assetBytes = new();

    public static string MetaPath
    {
        get
        {
#if UNITY_EDITOR
            return Path.Combine
            (
                Path.GetDirectoryName(Application.dataPath),
                HybridCLRSettings.Instance.strippedAOTDllOutputRootDir,
                EditorUserBuildSettings.activeBuildTarget.ToString()
            );
#else
            return Path.Combine(Application.streamingAssetsPath, "AotMeta");
#endif
        }
    }

    public static string DataPath
    {
        get
        {
#if UNITY_EDITOR
            return Path.Combine
            (
                Path.GetDirectoryName(Application.dataPath),
                HybridCLRSettings.Instance.hotUpdateDllCompileOutputRootDir,
                EditorUserBuildSettings.activeBuildTarget.ToString()
            );
#else
            return Path.Combine(Application.streamingAssetsPath, "HotData");
#endif
        }
    }

    private static async Task ByteLoaded(string file, Action<string, byte[]> onLoaded, string log)
    {
        var result = await File.ReadAllBytesAsync(file);
        string fileName = Path.GetFileNameWithoutExtension(file);
        Debug.Log($"[Loader] {log} loaded {fileName}");

        onLoaded.Invoke(fileName, result);
    }

    private static async Task LoadBytes(string[] files, Action<string, byte[]> onLoaded, string tag)
    {
        var tasks = new List<Task>();

        foreach (var file in files)
        {
            if (!File.Exists(file))
            {
                Debug.LogWarning($"[Loader] File not found!\n{file}");
                continue;
            }
            tasks.Add(ByteLoaded(file, onLoaded, tag));
        }
        await Task.WhenAll(tasks);
    }

    private static Assembly HotAssembly(string name)
    {
        return System.AppDomain.CurrentDomain.GetAssemblies().First(a => a.GetName().Name == name);
    }

    public static void Invoke(string assembly, string type, string method, object obj = null, object[] param = null)
    {
        var hotAssembly = HotAssembly(assembly);
        var hotType = hotAssembly.GetType(type);
        var hotMethod = hotType.GetMethod(method);
        hotMethod.Invoke(obj, param);
    }

    public static void Instantiate(string bundle, string name)
    {
        var ab = AssetBundle.LoadFromMemory(assetBytes[bundle]);

        var prefab = ab.LoadAsset<GameObject>(name);
        Instantiate(prefab);
    }

    private async Task HotAwake()
    {
        #region AOT Meta DLLs
        HomologousImageMode mode = HomologousImageMode.SuperSet;

        if (!Directory.Exists(MetaPath))
        {
            Debug.Log($"[Loader] Path not found\n{MetaPath}");
            return;
        }
#if UNITY_EDITOR
        await LoadBytes(Directory.GetFiles(MetaPath, "*.dll"), (_, d) =>
#else
        await LoadBytes(Directory.GetFiles(MetaPath, "*.dll.bytes"), (_, d) =>
#endif
        {
            RuntimeApi.LoadMetadataForAOTAssembly(d, mode);
        }, "AOT metadata");
#endregion

        #region Hot Updates
#if !UNITY_EDITOR
        if (!Directory.Exists(DataPath))
        {
            Debug.Log($"[Loader] Path not found\n{DataPath}");
            return;
        }

        await LoadBytes(Directory.GetFiles(DataPath, "*.dll.bytes"), (n, d) =>
        {
            Assembly.Load(dataBytes[n] = d);
        }, "Hot updates");
#endif
#endregion

        #region AssetBundles
        string assetPath = Path.Combine(Application.streamingAssetsPath, "Bundles");
        if (!Directory.Exists(assetPath))
        {
            Debug.Log($"[Loader] Path not found\n{assetPath}");
            return;
        }

        await LoadBytes(Directory.GetFiles(assetPath, "*.ab"), (n, d) =>
        {
            assetBytes[n] = d;
        }, "AssetBundle");
        #endregion

        Debug.Log("[Loader] Initialized.");
        await Task.Delay(TimeSpan.FromSeconds(Time.unscaledDeltaTime));

        gameObject.BroadcastMessage("OnHotAwake", SendMessageOptions.DontRequireReceiver);
    }

    private void Awake()
    {
        if (instance != this)
            return;
        DontDestroyOnLoad(gameObject);

        _ = HotAwake();
    }
}
