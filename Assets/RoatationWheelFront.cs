using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;
using UnityEngine;

public class RotationWheelFront : MonoBehaviour
{
    public float rotation;
    public GameObject car;

    // Start is called before the first frame update
    void Start()
    {
        car = GameObject.Find("-----SimpleCar(Clone)");

    }

    // Update is called once per frame
    void Update()
    {


        rotation = car.GetComponent<LogitechSteeringWheel>().rotationSpeed;


        transform.Rotate( 0 , 0 , rotation / 37642 / 4, Space.Self);




    }
}