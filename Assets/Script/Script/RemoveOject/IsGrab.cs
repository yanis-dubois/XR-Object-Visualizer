using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class IsGrab : MonoBehaviour
{
    private XRGrabInteractable grabInteractable;
    public bool isGrab = false;

    private void Start()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
    }

    void Update()
    {
        if (grabInteractable.isSelected) {
            isGrab = true;
        }
        else {
            isGrab = false;
        }
    }
    
    public bool GetIsGrab(){
        return isGrab;
    }
}
