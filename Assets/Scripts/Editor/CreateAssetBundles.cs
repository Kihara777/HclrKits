using System.IO;
using UnityEditor;
using UnityEngine;

public class CreateAssetBundles
{
    [MenuItem("HclrKits/Create Bundles")]
    static void Build()
    {
        string buildPath = Path.Combine(Application.streamingAssetsPath, "Bundles");
        if (!Directory.Exists(buildPath))
            Directory.CreateDirectory(buildPath);

        BuildPipeline.BuildAssetBundles
        (
            buildPath,
            BuildAssetBundleOptions.None,
            EditorUserBuildSettings.activeBuildTarget
        );
    }

    [MenuItem("HclrKits/Copy AOT metadata")]
    static void CopyMeta()
    {
        string metaPath = Path.Combine(Application.streamingAssetsPath, "AotMeta");
        if (!Directory.Exists(metaPath))
            Directory.CreateDirectory(metaPath);

        foreach (var file in Directory.GetFiles(Loader.MetaPath, "*.dll"))
        {
            var destFile = Path.GetFileName(file);
            var dest = Path.Combine(metaPath, $"{destFile}.bytes");
            File.Copy(file, dest, true);
        }
    }

    [MenuItem("HclrKits/Copy Hot Updates")]
    static void CopyData()
    {
        string dataPath = Path.Combine(Application.streamingAssetsPath, "HotData");
        if (!Directory.Exists(dataPath))
            Directory.CreateDirectory(dataPath);

        foreach (var file in Directory.GetFiles(Loader.DataPath, "*.dll"))
        {
            var destFile = Path.GetFileName(file);
            var dest = Path.Combine(dataPath, $"{destFile}.bytes");
            File.Copy(file, dest, true);
        }
    }
}