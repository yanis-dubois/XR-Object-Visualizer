using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Microsoft.MixedReality.Toolkit.Experimental.UI;

public class KeyboardController : MonoBehaviour
{
    private TMP_InputField inputField;

    void Start()
    {
        inputField=GetComponent<TMP_InputField>();
        inputField.onSelect.AddListener(x => OpenKeyboard());
    }

    public void OpenKeyboard()
    {
        NonNativeKeyboard.Instance.InputField=inputField;
        NonNativeKeyboard.Instance.PresentKeyboard(inputField.text);
        SetCaretToEnd();
    }

    public void SetCaretToEnd()
    {
        inputField.caretPosition = inputField.text.Length;
    }
}

