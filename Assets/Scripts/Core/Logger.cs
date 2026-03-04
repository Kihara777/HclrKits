using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class Logger : MonoBehaviour
{
    public Logger()
    {
        if (instance)
            return;

        instance = this;
    }

    private static Logger instance;

    [Header("Options")]
    [SerializeField] private int defaultFontSize = 42;

    private ConcurrentDictionary<Log, int> buffer = new();
    private Dictionary<KeyValuePair<Log, int>, bool> expand = new();
    private Vector2 scrollPos = new Vector2(0f, 1f);
    private int fontSize = -1;
    private bool visible = true;
    private GUIStyle btnStyle;

    [Serializable]
    private record Log
    {
        public string log;
        public string trace;
        public LogType type;
        public float stamp;
    }

    private void Awake()
    {
        if (instance != this)
            return;

        DontDestroyOnLoad(gameObject);
        Application.logMessageReceivedThreaded += Receiver;
    }

    private void OnDestroy()
    {
        if (instance != this)
            return;

        Application.logMessageReceivedThreaded -= Receiver;
    }

    private void Receiver(string log, string trace, LogType type)
    {
        var stamp = Time.time;
        var item = new Log
        {
            log = log,
            trace = trace,
            type = type,
            stamp = stamp
        };

        if (!buffer.ContainsKey(item))
            buffer[item] = 0;

        buffer[item]++;
    }

    private void Update()
    {
        if (Keyboard.current.backquoteKey.wasReleasedThisFrame)
        {
            visible = !visible;
        }

        if (visible)
        {
            if (Keyboard.current.numpadPlusKey.wasReleasedThisFrame)
                fontSize++;
            if (Keyboard.current.numpadMinusKey.wasReleasedThisFrame)
                fontSize--;

            fontSize = Mathf.Max(defaultFontSize, fontSize);
        }
    }

    private void OnGUI()
    {
        if (!visible || instance != this)
            return;

        btnStyle = new GUIStyle(GUI.skin.textField)
        {
            fontSize = fontSize,
            richText = true,
            wordWrap = true
        };
        btnStyle.onFocused.textColor = Color.grey;

        GUILayout.BeginArea(new Rect(0, 0, Screen.width, Screen.height));
        scrollPos = GUILayout.BeginScrollView(scrollPos);
        ;
        foreach (var kv in buffer.ToList().OrderBy(b => b.Key.stamp))
        {
            var item = kv.Key;
            string log = $"<size={fontSize + 4}>{kv.Value}</size> {item.log}";
            expand[kv] = expand.ContainsKey(kv) && expand[kv];
            if (expand[kv])
                log += $"\n<color=#808080><size={fontSize - 4}>[{item.type}] {item.trace}</size></color>";

            switch (item.type)
            {
                case LogType.Error:
                case LogType.Exception:
                    btnStyle.normal.textColor = Color.red;
                    break;
                case LogType.Warning:
                    btnStyle.normal.textColor = Color.yellow;
                    break;
                default:
                    btnStyle.normal.textColor = Color.white;
                    break;
            }

            expand[kv] = GUILayout.Toggle(expand[kv], log, btnStyle);
        }

        GUILayout.EndScrollView();
        GUILayout.EndArea();
    }
}
