using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;
using System.Threading;
using UnityEngine;

public class RotatingWheel : MonoBehaviour
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


        if (Mathf.Abs(rotation) <= 37642)
        {
            transform.Rotate(rotation / 37642 * 10 * Time.deltaTime , 0, 0, Space.Self);

        }

    }
}