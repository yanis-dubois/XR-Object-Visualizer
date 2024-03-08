using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TextFieldManager : MonoBehaviour
{
    public TMP_InputField tmpInputField;

    private void Start()
    {
        tmpInputField.onEndEdit.AddListener(OnEndEdit);
    }

    private void OnEndEdit(string text)
    {
        Debug.Log("Texte finalisé : " + text);
    }

    public void OpenKeyboard()
    {
        // Activez le champ de texte, ce qui devrait ouvrir le clavier virtuel
        tmpInputField.ActivateInputField();
    }
}

