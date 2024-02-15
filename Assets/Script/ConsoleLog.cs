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
    // Calcul de la position de départ pour centrer la zone
    float zoneWidth = 400;
    float zoneHeight = Screen.height * 0.5f; // Ajustez la hauteur selon vos besoins
    float startX = (Screen.width - zoneWidth) / 2;
    float startY = (Screen.height - zoneHeight) / 2;

    // Utilisation des valeurs calculées pour positionner la zone de GUI
    GUILayout.BeginArea(new Rect(startX, startY, zoneWidth, zoneHeight));

    // Affichage du texte de log
    GUILayout.Label("\n" + string.Join("\n", myLogQueue.ToArray()));

    // Fermeture de la zone de GUI
    GUILayout.EndArea();
    }
}
