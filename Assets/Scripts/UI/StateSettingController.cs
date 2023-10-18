using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateSettingController : MonoBehaviour
{
    public enum PlayerType {Pedestrian_flat,Pedestrian,Bicyclist}; 
    public enum PlayerColor {red, blue, green, yellow};
    public static bool night = false;

    //Bike Object detection Varaibles
    public static float avoidMagnitude = 0.1f;
    public static float rayCastOneAngle = 80f;
    public static float raycastDistance = 3f;
    public static float test;

    //Car Speed variables
    public static float randSpeedVariation = 2f;
    public static float slopeSpeedMultiplier = 1.25f;

    //Player Type
    public static PlayerType playerType = PlayerType.Pedestrian_flat;
    public static PlayerColor playerColor = PlayerColor.red;


    /**************** DEFAULT VALUES *****************/
    public static bool def_night = false;
    public static float dev_avoidMagnitude = 0.1f;
    public static float dev_rayCastOneAngle = 80f;
    public static float dev_raycastDistance = 3f;
    public static float dev_randSpeedVariation = 2f;
    public static float dev_slopeSpeedMultiplier = 1.25f;
    public static PlayerType dev_playerType = PlayerType.Pedestrian_flat;
    public static PlayerColor dev_playerColor = PlayerColor.red;
}   

