using System.Collections;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;


public class RemoveObject : MonoBehaviour
{
    /// <summary>
    /// Reference to the InputActionAsset for the remove button.
    /// </summary>
    [SerializeField] 
    private InputActionReference removeButtonAction;

    /// <summary>
    /// Called when the script instance is being loaded.
    /// </summary>
    private void OnEnable()
    {
        if (removeButtonAction != null)
        {
            // Register the callback for the performed event
            removeButtonAction.action.performed += OnRemoveButtonPressed;
            removeButtonAction.action.Enable();
        }
        else
        {
            Debug.LogWarning("InputActionReference is not assigned in the Inspector.");
        }
    }
    
    /// <summary>
    /// Called when the script instance is being destroyed.
    /// </summary>
    private void OnDisable()
    {
        if (removeButtonAction != null)
        {
            // Unregister the callback for the performed event
            removeButtonAction.action.performed -= OnRemoveButtonPressed;
            removeButtonAction.action.Disable();
        }
        else
        {
            Debug.LogWarning("InputActionReference is not assigned in the Inspector.");
        }
    }

    /// <summary>
    /// Called when the remove button is pressed.
    /// </summary>
    /// <param name="context">The callback context for the input action.</param>
    private void OnRemoveButtonPressed(InputAction.CallbackContext context)
    {       
        if(FindObjectsOfType<IsGrab>()!=null )
        {
            IsGrab[] monScripts = FindObjectsOfType<IsGrab>();
            foreach (IsGrab monScript in monScripts)
            {
                if(monScript.isGrab)
                {
                    Destroy(monScript.gameObject);
                }
            }
        }
    }
}
