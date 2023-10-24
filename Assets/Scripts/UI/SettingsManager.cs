using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using UnityEditor;
using UnityEngine.XR;


public class SettingsManager : MonoBehaviour
{
    [SerializeField] Dropdown sceneDropdown,playerTypeDropdown, colorDropdown;  // scene dropdown ui
    
    [SerializeField] Dropdown envDropdown; // environment dropdown ui
    [SerializeField] InputField avoidMagField, raycastOneAngleField, raycastDistField, speedVariationField, slopeMultField;
    [SerializeField] Text statusText;
    void Start() {
        XRSettings.enabled=false;

        //Add listener for when the value of the Dropdown changes, to take action
        playerTypeDropdown.onValueChanged.AddListener(delegate {
            PlayerDropdownValueChanged(playerTypeDropdown);
        });

        // populate dropdown UI with all the scenes available
        sceneDropdown.ClearOptions();
        List<string> m_DropOptions = new List<string>();
        if (SceneManager.sceneCountInBuildSettings > 0) {
            for (int n = 1; n < SceneManager.sceneCountInBuildSettings; ++n)
            {
                string path = SceneUtility.GetScenePathByBuildIndex(n);
                string sceneName = path.Substring(0, path.Length - 6).Substring(path.LastIndexOf('/') + 1);
                m_DropOptions.Add(sceneName);
                Debug.Log(sceneName);
            }
        }
        sceneDropdown.AddOptions(m_DropOptions);
    }

    public void switchScenes() {
        // Grab the current settings and store it into the StateSettingController static object
        // Environment (Night/Day)
        if (envDropdown.value == 0) {
            // day is selected
            StateSettingController.night = false;
        } else {
            StateSettingController.night = true;
        }
        // Grab values from UI Fields
        StateSettingController.avoidMagnitude = (float)Convert.ToDouble(avoidMagField.text);
        StateSettingController.rayCastOneAngle = (float)Convert.ToDouble(raycastOneAngleField.text);
        StateSettingController.raycastDistance = (float)Convert.ToDouble(raycastDistField.text);
        StateSettingController.randSpeedVariation = (float)Convert.ToDouble(speedVariationField.text);
        StateSettingController.slopeSpeedMultiplier = (float)Convert.ToDouble(slopeMultField.text);
        StateSettingController.playerType = (StateSettingController.PlayerType)playerTypeDropdown.value;
        StateSettingController.playerColor = (StateSettingController.PlayerColor)colorDropdown.value;
        
        statusText.text = "Simulation Starting. Please wait...";

        // use coroutine so that UI can update the status text before starting the new scene.
        StartCoroutine(StartScene());
    }
    
    //Ouput the new value of the Dropdown into Text
    void PlayerDropdownValueChanged(Dropdown change)
    {
       if ((change.value == 0) || (change.value == 1)) {
            colorDropdown.interactable = true;
       } else {
            colorDropdown.interactable = false;
       }
    }

    private IEnumerator StartScene() {
        yield return null;
        // Load the selected scene
        SceneManager.LoadScene(sceneDropdown.value+1);
    }

    public void resetDefaults() {
        envDropdown.value = 0;  // reset to default day
        sceneDropdown.value = 0; // reset to first scene
        avoidMagField.text = Convert.ToString(StateSettingController.dev_avoidMagnitude);
        raycastOneAngleField.text = Convert.ToString(StateSettingController.dev_rayCastOneAngle);
        raycastDistField.text = Convert.ToString(StateSettingController.dev_raycastDistance);
        speedVariationField.text = Convert.ToString(StateSettingController.dev_randSpeedVariation);
        slopeMultField.text = Convert.ToString(StateSettingController.dev_slopeSpeedMultiplier);
        playerTypeDropdown.value = (int)StateSettingController.dev_playerType;
        colorDropdown.value = (int)StateSettingController.dev_playerColor;
    }

    public void changeAvoidMag(InputField arg) {
        Debug.Log(arg.text);
        try 
        {
            var temp = Convert.ToDouble(arg.text);
            StateSettingController.avoidMagnitude = (float)temp;
        }
        catch (System.Exception)
        {
            Debug.Log("AvoidMag Error - Resetting to default");
            
            avoidMagField.text = Convert.ToString(StateSettingController.dev_avoidMagnitude);
            throw;
        }
    }

    public void changeRaycastDist(InputField arg) {
        Debug.Log(arg.text);
        try 
        {
            var temp = Convert.ToDouble(arg.text);
            StateSettingController.raycastDistance = (float)temp;
        }
        catch (System.Exception)
        {
            Debug.Log("ChangeRay Distance Error");
            raycastDistField.text = Convert.ToString(StateSettingController.dev_raycastDistance);
            throw;
        }
    }

    public void changeRaycastAng(InputField arg) {
        Debug.Log(arg.text);
        try 
        {
            var temp = Convert.ToDouble(arg.text);
            StateSettingController.rayCastOneAngle = (float)temp;
        }
        catch (System.Exception)
        {
            Debug.Log("ChangeRay Angle Error");
            raycastOneAngleField.text = Convert.ToString(StateSettingController.dev_rayCastOneAngle);
            throw;
        }
    }

    public void changeSpeedVariation(InputField arg) {
        Debug.Log(arg.text);
        try 
        {
            var temp = Convert.ToDouble(arg.text);
            StateSettingController.randSpeedVariation = (float)temp;
        }
        catch (System.Exception)
        {
            Debug.Log("Speed Variation Error");
            speedVariationField.text = Convert.ToString(StateSettingController.dev_randSpeedVariation);
            throw;
        }
    }

    public void changeSlopeMultiplier(InputField arg) {
        Debug.Log(arg.text);
        try 
        {
            var temp = Convert.ToDouble(arg.text);
            StateSettingController.slopeSpeedMultiplier = (float)temp;
        }
        catch (System.Exception)
        {
            Debug.Log("Speed Variation Error");
            slopeMultField.text = Convert.ToString(StateSettingController.dev_slopeSpeedMultiplier);
            throw;
        }
    }

}