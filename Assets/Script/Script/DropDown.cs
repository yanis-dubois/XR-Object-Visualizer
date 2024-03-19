using TMPro;
using UnityEngine;

public class DropDown : MonoBehaviour
{
    public TextMeshProUGUI dropDownValue; 
    public GameObject smbDialog;
    public GameObject urlDialog;

    public void OnValueChange() {
        Debug.Log(dropDownValue.text);

        switch (dropDownValue.text) {
            case "SMB":
                smbDialog.SetActive(true);
                urlDialog.SetActive(false);
                break;
            case "URL":
                smbDialog.SetActive(false);
                urlDialog.SetActive(true);
                break;
            default:
                smbDialog.SetActive(false);
                urlDialog.SetActive(false);
                break;
        }
    }
}
