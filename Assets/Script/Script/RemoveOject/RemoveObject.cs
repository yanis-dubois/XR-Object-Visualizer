using UnityEngine;
using UnityEngine.InputSystem;

public class RemoveObject : MonoBehaviour
{

    [SerializeField] 
    private InputActionReference removeButtonAction;


    private void OnEnable(){
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
    
    private void OnDisable(){
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

    private void OnRemoveButtonPressed(InputAction.CallbackContext context)
    {       
        if (FindObjectsOfType<IsGrab>()!=null) {
    
            IsGrab[] monScripts = FindObjectsOfType<IsGrab>();
            foreach (IsGrab monScript in monScripts)
            {
                if (monScript.isGrab) {
                    Destroy(monScript.gameObject);
                }
            }
        }
    }
}
