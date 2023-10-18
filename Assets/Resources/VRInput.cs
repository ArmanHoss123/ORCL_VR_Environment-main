using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class VRInput : MonoBehaviour
{

    // [SteamVR_DefaultAction("Squeeze")]
    public SteamVR_Action_Single sqeezeAction;
    public SteamVR_Action_Vector2 joyStickAction;
    // Update is called once per frame
    void Update()
    {
        if(SteamVR_Actions._default.Teleport.GetStateDown(SteamVR_Input_Sources.Any)) {
            print("Teleport down");
        }

        if (SteamVR_Actions._default.GrabPinch.GetStateUp(SteamVR_Input_Sources.Any)) {
            print("Grab pinch up");
        }

        // Grab Action
        float triggerValue = sqeezeAction.GetAxis(SteamVR_Input_Sources.Any);
        if(triggerValue > 0.0f) {
            print(triggerValue);
        }

        // Joystick Movement option
        Vector2 joyStickValue = joyStickAction.GetAxis(SteamVR_Input_Sources.Any);
        if(joyStickValue != Vector2.zero) {
            print(joyStickValue);
        }
    }
}
