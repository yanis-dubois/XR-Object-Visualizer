using Dummiesman;
using System.IO;
using TMPro;
using UnityEngine.Networking;
using UnityEngine;
using System.Threading.Tasks;

public class ObjFromStream : MonoBehaviour {

    public GameObject interactableObjectPrefab;
    public GameObject objectSpawner; 

    public TextMeshProUGUI url_text;

	public async void LoadObject () {
        string url = url_text.text.Substring(0, url_text.text.Length-1);
        byte[] results = await DownloadObject(url);
        var stream = new MemoryStream(results);
        var tmpObj = new OBJLoader().Load(stream);
        OBJInstantiate.instantiate(interactableObjectPrefab, objectSpawner, tmpObj);
	}

    private async Task<byte[]> DownloadObject(string url)
    {
        var request = UnityWebRequest.Get(url);
        request.SendWebRequest();
        while (!request.isDone) await Task.Yield(); // wait 1 frame until request done

        if (request.result == UnityWebRequest.Result.ConnectionError || 
            request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError("Error: " + request.error);
            return null;
        }

        return request.downloadHandler.data;
    }
}
