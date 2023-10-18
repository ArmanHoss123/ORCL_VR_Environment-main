using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkyBoxSwitcher : MonoBehaviour
{
    // Start is called before the first frame update
    public Material daySky;
    public Material nightSky;
    private List<Light> lightArray = new List<Light>();
    bool night;

    void daySwap() {
        night = false;
        StateSettingController.night = false;
            Debug.Log("Switching to Day");
            RenderSettings.skybox = daySky;
            for (int i = 0; i < lightArray.Count; i++) {
                ((Light)lightArray[i]).intensity *= 2f;
            }
            RenderSettings.fogColor = new Color(0.72f, .89f, .98f, 1);
    }
    void nightSwap() {
        night = true;
        Debug.Log("Switching to Night");
        StateSettingController.night = true;
        RenderSettings.skybox = nightSky;
        for (int i = 0; i < lightArray.Count; i++) {
            ((Light)lightArray[i]).intensity *= 0.5f;
        }
        RenderSettings.fogColor = new Color(0.1f, 0.1f, 0.15f, 1);
    }
    
    void Start()
    {
        night = StateSettingController.night;
        for(int i = 0; i < 7; i++) {
            GameObject lightObject = this.transform.GetChild(i).gameObject;
            if (lightObject.activeSelf) {
                Debug.Log(lightObject.GetComponent<Light>());
                lightArray.Add(lightObject.GetComponent<Light>());
            }
        }

        if(night == false)
            daySwap();
        else
            nightSwap();

    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Alpha1) && night) {
            daySwap();
        }
        if(Input.GetKeyDown(KeyCode.Alpha2) && !night) {
            nightSwap();
        }
    }
}
