// code inspired from
// https://stackoverflow.com/questions/67704820/how-do-i-print-unitys-debug-log-to-the-screen-gui
using UnityEngine;
using System.Collections;
using TMPro;

public class ZLog : MonoBehaviour
{
    public uint qsize = 15;
    private Queue myLogQueue = new Queue();
    public TMP_Text logText;

    void Start() {
        Debug.Log("Started up logging.");
    }

    void OnEnable() {
        Application.logMessageReceived += HandleLog;
    }

    void OnDisable() {
        Application.logMessageReceived -= HandleLog;
    }

    void Update() {
        if (logText != null && myLogQueue.Count > 0)
            logText.text = string.Join("\n", myLogQueue.ToArray());
    }

    void HandleLog(string logString, string stackTrace, LogType type) {
        myLogQueue.Enqueue("[" + type + "] : " + logString);
        if (type == LogType.Exception)
            myLogQueue.Enqueue(stackTrace);

        while (myLogQueue.Count > qsize)
            myLogQueue.Dequeue();

        // update text
        if (logText != null)
            logText.text = string.Join("\n", myLogQueue.ToArray());
    }
}