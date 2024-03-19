using TMPro;
using UnityEngine.Networking;
using UnityEngine;
using System.Threading.Tasks;

public class ObjFromUrl : MonoBehaviour {
    public TextMeshProUGUI url_text;
    public GameObject validButton;
    public GameObject loadingAnimation;

	public async Task<byte[]> LoadObject () {
        string url = url_text.text.Substring(0, url_text.text.Length-1);
        byte[] result = await DownloadObject(url);
        return result;
	}

    // Download object from a web stream
    private async Task<byte[]> DownloadObject(string url)
    {
        var request = UnityWebRequest.Get(url);
        request.SendWebRequest();
        while (!request.isDone) await Task.Yield();

        if (request.result == UnityWebRequest.Result.ConnectionError || 
            request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError("Error: " + request.error);
            return null;
        }

        return request.downloadHandler.data;
    }
}
