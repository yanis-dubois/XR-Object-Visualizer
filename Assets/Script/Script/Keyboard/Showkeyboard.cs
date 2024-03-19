using UnityEngine;
using TMPro;
using Microsoft.MixedReality.Toolkit.Experimental.UI;

public class KeyboardController : MonoBehaviour
{
    private TMP_InputField inputField;

    // Add listener to the keyboard
    void Start()
    {
        inputField = GetComponent<TMP_InputField>();
        inputField.onSelect.AddListener(x => OpenKeyboard());
    }

    // Link the keyboard to the selected input field and show it
    public void OpenKeyboard()
    {
        NonNativeKeyboard.Instance.InputField = inputField;
        NonNativeKeyboard.Instance.PresentKeyboard(inputField.text);
        SetCaretToEnd();
    }

    // sSet the cursor position to the end of the input field
    public void SetCaretToEnd()
    {
        inputField.caretPosition = inputField.text.Length;
    }
}

