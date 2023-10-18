using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
public class TrafficManager : MonoBehaviour
{
    private GameObject[] listNS;
    private GameObject[] listEW;

    ArrayList yellowListNS = new ArrayList();
    ArrayList greenListNS = new ArrayList();
    ArrayList redListNS = new ArrayList();
    ArrayList yellowListEW = new ArrayList();
    ArrayList greenListEW = new ArrayList();
    ArrayList redListEW = new ArrayList();

    public enum lightColor {yellow, green, red};
    public lightColor lightState = lightColor.yellow;


    public Vector2 timeGreen = new Vector2(6f, 10f);
    public Vector2 timeYellow = new Vector2(3f, 8.5f);
    public Vector2 timeRed = new Vector2(3f, 8.5f);
    void Start()
    {
        listNS = GameObject.FindGameObjectsWithTag("StreetLightNS");
        listEW = GameObject.FindGameObjectsWithTag("StreetLightEW");

        // grab all the NS lights
        foreach(GameObject parent in listNS) 
        {
            foreach (Transform child in parent.transform)
            {
                try {
                    child.transform.gameObject.GetComponent<Light>().intensity = 0f;

                    if (child.name == "Yellow light") {
                        yellowListNS.Add(child.gameObject.GetComponent<Light>());
                    }
                    else if (child.name == "Red light") {
                        redListNS.Add(child.gameObject.GetComponent<Light>());
                    }
                    else if (child.name == "Green light") {
                        greenListNS.Add(child.gameObject.GetComponent<Light>());
                    }
                }
                catch (MissingComponentException)
                {
                    
                }
            }        // grab all the NS lights
        }
        foreach(GameObject parent in listEW) {
            foreach (Transform child in parent.transform)
            {
                try {
                    child.gameObject.GetComponent<Light>().intensity = 0f;

                    if (child.name == "Yellow light") {
                        yellowListEW.Add(child.gameObject.GetComponent<Light>());
                    }
                    else if (child.name == "Red light") {
                        redListEW.Add(child.gameObject.GetComponent<Light>());
                    }
                    else if (child.name == "Green light") {
                        greenListEW.Add(child.gameObject.GetComponent<Light>());
                    }
                }
                catch (MissingComponentException)
                {
                    
                }
            }
        }
        cycleLights(timeGreen, timeYellow, timeRed);
    }

    private void toggleLightsEW(lightColor color, float intensity) {
        ArrayList onList;
        if(color == lightColor.red) {
            onList = redListEW;
        } else if(color == lightColor.yellow) {
            onList = yellowListEW;
        } else {
            onList = greenListEW;
        }
        // turn on the correct lights
        foreach (Light light in onList) {
            light.intensity = intensity;
        }
    }

    private void toggleLightsNS(lightColor color, float intensity) {
        ArrayList onList;
        if(color == lightColor.red) {
            onList = redListNS;
        } else if(color == lightColor.yellow) {
            onList = yellowListNS;
        } else {
            onList = greenListNS;
        }
        // turn on the correct lights
        foreach (Light light in onList) {
            light.intensity = intensity;
        }
    }

    async void cycleLights(Vector2 green, Vector2 yellow, Vector2 red) {
        while(true) {
            float cycleTime = UnityEngine.Random.Range(green[0], green[1]);
            
            // NS = RED  ::  EW = GREEN
            toggleLightsEW(lightColor.green, 2.5f);
            toggleLightsNS(lightColor.red, 2.5f);
            lightState = lightColor.green;
            await Task.Delay(TimeSpan.FromSeconds(cycleTime));
            toggleLightsEW(lightColor.green, 0f);
            
 
            // NS = RED  ::  EW = YELLOW
            cycleTime = UnityEngine.Random.Range(yellow[0], yellow[1]);
            toggleLightsEW(lightColor.yellow, 2.5f);
            lightState = lightColor.yellow;
            await Task.Delay(TimeSpan.FromSeconds(cycleTime));
            toggleLightsEW(lightColor.yellow, 0f);
            toggleLightsNS(lightColor.red, 0f);

            // NS = GREEN  ::  EW = RED
            cycleTime = UnityEngine.Random.Range(red[0], red[1]);
            toggleLightsEW(lightColor.red, 2.5f);
            toggleLightsNS(lightColor.green, 2.5f);
            lightState = lightColor.red;
            await Task.Delay(TimeSpan.FromSeconds(cycleTime));
            toggleLightsNS(lightColor.green, 0f);

            
            // NS = YELLOW  ::  EW = RED
            cycleTime = UnityEngine.Random.Range(red[0], red[1]);
            toggleLightsEW(lightColor.red, 2.5f);
            toggleLightsNS(lightColor.yellow, 2.5f);
            lightState = lightColor.red;
            await Task.Delay(TimeSpan.FromSeconds(cycleTime));
            toggleLightsEW(lightColor.red, 0f);
            toggleLightsNS(lightColor.yellow, 0f);
        }
    }
}
