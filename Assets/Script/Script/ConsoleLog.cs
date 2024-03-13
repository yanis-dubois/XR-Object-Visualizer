// code from
// https://stackoverflow.com/questions/67704820/how-do-i-print-unitys-debug-log-to-the-screen-gui
using UnityEngine;
using System.Collections;

public class ZzzLog : MonoBehaviour
{
    uint qsize = 15;  // number of messages to keep
    Queue myLogQueue = new Queue();

    void Start() {
        Debug.Log("Started up logging.");
    }

    void OnEnable() {
        Application.logMessageReceived += HandleLog;
    }

    void OnDisable() {
        Application.logMessageReceived -= HandleLog;
    }

    void HandleLog(string logString, string stackTrace, LogType type) {
        myLogQueue.Enqueue("[" + type + "] : " + logString);
        if (type == LogType.Exception)
            myLogQueue.Enqueue(stackTrace);
        while (myLogQueue.Count > qsize)
            myLogQueue.Dequeue();
    }

    void OnGUI() {
        float zoneWidth = 400;
        float zoneHeight = Screen.height * 0.5f;
        float startX = (Screen.width - zoneWidth) / 2;
        float startY = (Screen.height - zoneHeight) / 2;

        GUILayout.BeginArea(new Rect(startX, startY, zoneWidth, zoneHeight));
        GUILayout.Label("\n" + string.Join("\n", myLogQueue.ToArray()));
        GUILayout.EndArea();
    }
}
