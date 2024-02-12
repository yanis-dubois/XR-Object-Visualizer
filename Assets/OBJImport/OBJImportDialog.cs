using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleFileBrowser;

public class OBJImportDialog : MonoBehaviour
{
    public string selectedFilePathText;

    private void Start()
    {
        // Assurez-vous d'appeler cette méthode dans la méthode Start ou ailleurs au moment approprié
        FileBrowser.SetFilters(true, new FileBrowser.Filter("Text Files", ".obj"));

        // Ouvre la boîte de dialogue de navigation de fichiers
        bool isDialogShown = FileBrowser.ShowLoadDialog(OpenFileCallback, null, FileBrowser.PickMode.Files, false, null, "Sélectionner un fichier", "Sélectionner");

        Debug.Log("isDialogShown " + isDialogShown);
    }

    public void OpenFileBrowser()
    {
        
    }

    private void OpenFileCallback(string[] paths)
    {
        if (paths.Length > 0)
        {
            // Faites quelque chose avec le chemin du fichier sélectionné (par exemple, affichez-le dans un texte)
            selectedFilePathText = "Chemin du fichier sélectionné : " + paths[0];
        }
        else
        {
            // Aucun fichier sélectionné
            selectedFilePathText = "Aucun fichier sélectionné";
        }
        Debug.Log(selectedFilePathText);
    }
}
