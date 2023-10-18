using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoatationWheelFr : MonoBehaviour
{
    private GameObject car;
    private float rotation;

    // Start is called before the first frame update
    void Start()
    {
        car = GameObject.Find("-----SimpleCar(Clone)");

    }

    // Update is called once per frame
    void Update()
    {


        rotation = car.GetComponent<LogitechSteeringWheel>().rotationSpeed;


        if(Mathf.Abs(rotation) <= 37642)
        {
            transform.Rotate(0, 0 , rotation / 37642 * 8f , Space.Self);

        }

    }
}
