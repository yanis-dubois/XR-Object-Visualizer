using Dummiesman;
using System.IO;
using System.Text;
using TMPro;
using UnityEngine.Networking;
using System.Collections;
using UnityEngine;

public class ObjFromStream : MonoBehaviour {

    public GameObject interactableObjectPrefab;
    public GameObject objectSpawner; 

    public TextMeshProUGUI url_text;

	public void LoadObject () {
        string url = url_text.text.Substring(0, url_text.text.Length-1);

        StartCoroutine(GetRequest(url));
	}

    IEnumerator GetRequest(string uri)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
        {
            // Request and wait for the desired page.
            yield return webRequest.SendWebRequest();

            string[] pages = uri.Split('/');
            int page = pages.Length - 1;

            switch (webRequest.result)
            {
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                    Debug.LogError(pages[page] + ": Error: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.ProtocolError:
                    Debug.LogError(pages[page] + ": HTTP Error: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.Success:
                    Debug.Log(pages[page] + ":\nReceived: " + webRequest.downloadHandler.text);
                    byte[] results = webRequest.downloadHandler.data;
                    var stream = new MemoryStream(results);
                    var tmpObj = new OBJLoader().Load(stream);
                    OBJInstantiate.instantiate(interactableObjectPrefab, objectSpawner, tmpObj);
                    break;
            }
        }
    }
}
