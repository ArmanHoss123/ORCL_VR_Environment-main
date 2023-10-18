//======= Copyright (c) Valve Corporation, All rights reserved. ===============
//
// Purpose: Demonstrates how to create a simple interactable object
//
//=============================================================================

using UnityEngine;
using System.Collections;
using Valve.VR.InteractionSystem;
using Valve.VR;

//-------------------------------------------------------------------------
public class SignPost : MonoBehaviour
{
    public float lightsTotalActiveTime = 1f;
    public float buttonFullPressTime = 0.1f;
    public Vector3 buttonMoveAmount = new Vector3(0f, -1f, 0f);
    public float buttonMoveTime = 0.2f;

    public GameObject lights;
    public Crossway crossway;
    public Transform button;
    public SignPost connectedSignPost;

    private bool buttonPressed;
    public bool IsButtonPressed
    {
        get
        {
            return buttonPressed;
        }
    }
    private float buttonPressTime;
    private bool activatedLights;
    private Vector3 finalButtonMovePosition;
    private Vector3 origButtonPosition;

    [EditorButton("Force Button Down", "ForceButtonDown")]
    public bool forceDown;

    [EditorButton("Force Button Up", "ForceButtonUp")]
    public bool forceUp;

    void Awake()
    {
        origButtonPosition = button.localPosition;
        finalButtonMovePosition = button.localPosition + buttonMoveAmount;
    }
   
    public void ForceButtonDown()
    {
        OnButtonDown(null);
    }

    public void ForceButtonUp()
    {
        OnButtonUp(null);
    }

    public void OnButtonDown(Hand fromHand)
    {
        Debug.Log("BUTTON DOWN!");
        buttonPressed = true;
        if(fromHand != null)
            fromHand.TriggerHapticPulse(1000);
        ActivateLights();

        if (connectedSignPost && !connectedSignPost.IsButtonPressed)
            connectedSignPost.ForceButtonDown();
    }

    public void OnButtonUp(Hand fromHand)
    {
        Debug.Log("BUTTON UP!");
        activatedLights = true;
        buttonPressed = false;

        if (connectedSignPost && connectedSignPost.IsButtonPressed)
            connectedSignPost.ForceButtonUp();
    }

    private void Update()
    {
        button.localPosition = Vector3.Lerp(origButtonPosition, buttonMoveAmount, buttonPressTime / buttonMoveTime);
        buttonPressTime += Time.deltaTime * (buttonPressed ? 1f : -1f);
        if (buttonPressTime < 0f)
            buttonPressTime = 0f;
        else if (buttonPressTime > buttonMoveTime)
            buttonPressTime = buttonMoveTime;
    }


    void ActivateLights()
    {
        if (activatedLights)
            return;
        Debug.Log("Lights activated");
        if (crossway)
           crossway.CrosswayActive = true;
        activatedLights = true;
        if(lights)
            lights.SetActive(true);
        StartCoroutine(DeactivateLights());
    }

    IEnumerator DeactivateLights()
    {
        yield return new WaitForSeconds(lightsTotalActiveTime);
        Debug.Log("Lights deactivated");
        activatedLights = false;
        if(lights)
            lights.SetActive(false);
        if (crossway)
            crossway.CrosswayActive = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Trigger entered: " + other.gameObject.name);
        if(other.gameObject.name.Contains("Pedestrian Hand Collider"))
        {
            OnButtonDown(other.gameObject.name.Contains("Right") ? Player.instance.rightHand : Player.instance.leftHand);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log("Trigger exited: " + other.gameObject.name);
        if (other.gameObject.name.Contains("Pedestrian Hand Collider"))
        {
            OnButtonUp(other.gameObject.name.Contains("Right") ? Player.instance.rightHand : Player.instance.leftHand);
        }
    }
}

