using UnityEngine;
using System.Collections;

public class ConsoleToCanvasText : MonoBehaviour {

    public UnityEngine.UI.Text textConsole;

    string output = "";
    string stack = "";

    string outputLog;

    void OnEnable()
    {
        Application.logMessageReceived += HandleLog;
    }
    void OnDisable()
    {
        Application.logMessageReceived -= HandleLog;
    }
    void HandleLog(string logString, string stackTrace, LogType type)
    {
        output = logString;
        stack = stackTrace;

        outputLog += output + "\n";

        if (textConsole)
        {
            textConsole.text = outputLog;
        }

    }
}
