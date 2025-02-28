using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class HotOutputScreen : MonoBehaviour
{
    [SerializeField]
    int maxLines = 50;
    [SerializeField]
    int maxLineLength = 100;
    [SerializeField]
    int fontSize = 15;

    string m_logFileSavePath = string.Empty;

    private string _logStr = string.Empty;
    private readonly List<string> _lines = new List<string>();

    private void Awake()
    {
        m_logFileSavePath = string.Format("{0}/output_{1}.log", Directory.GetParent(Application.dataPath));
        Debug.Log(m_logFileSavePath);
    }
    private void OnEnable()
    {
        Application.logMessageReceived += Log;
    }

    private void OnDisable()
    {
        Application.logMessageReceived -= Log;
    }

    public void Log(string logString, string stackTrace, LogType type)
    {
        foreach (string line in logString.Split('\n'))
        {
            if(line.Length <= maxLineLength)
            {
                _lines.Add(line);
                continue;
            }

            int lineCount = Mathf.CeilToInt(line.Length / maxLineLength + 1);
            for(int i = 0; i < lineCount; i++)
            {
                if ((i + 1) * maxLineLength <= line.Length)
                {
                    _lines.Add(line.Substring(i * maxLineLength, maxLineLength));
                }
                else
                {
                    _lines.Add(line.Substring(i * maxLineLength, line.Length - i * maxLineLength));
                }
            }
        }

        if (_lines.Count > maxLines)
        {
            _lines.RemoveRange(0, _lines.Count - maxLines);
        }

        _logStr = string.Join("\n", _lines);

        if (!File.Exists(m_logFileSavePath))
        {
            var fs = File.Create(m_logFileSavePath);
            fs.Close();
        }

        using (var sw = File.AppendText(m_logFileSavePath))
        {
            sw.WriteLine(_logStr.ToString());
        }

        _logStr.Remove(0, _logStr.Length);
    }

    void OnGUI()
    {
        GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity,
            new Vector3(Screen.width / 1200.0f, Screen.height / 800.0f, 1.0f));
        GUI.Label(new Rect(10, 10, 800, 370), _logStr, new GUIStyle { fontSize = 15 });
    }
}
