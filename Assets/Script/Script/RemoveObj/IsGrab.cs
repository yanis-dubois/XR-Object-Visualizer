using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR;
using UnityEngine.InputSystem;

public class IsGrab : MonoBehaviour
{
    private XRGrabInteractable grabInteractable;
    public bool isGrab=true;

   

    private void Start()
    {
        isGrab=true;
        grabInteractable = GetComponent<XRGrabInteractable>();
    }
    // Update is called once per frame
    void Update()
    {
        if (grabInteractable.isSelected  ){
            isGrab=true;
        }
        else{
            isGrab=false;
        }
    }
    public bool GetIsGrab(){
        return isGrab;
    }

}
