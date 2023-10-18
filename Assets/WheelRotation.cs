using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class WheelRotation : MonoBehaviour
{
    float speed = 2f;
    public float rotation;

    private GameObject car;

    float tempRotation = 0;



    // Start is called before the first frame update
    void Start()
    {

        car = GameObject.Find("-----SimpleCar(Clone)");

        //MeshFilter m = this.GetComponent<MeshFilter>();
        //float min = float.MaxValue;
        //float max = float.MinValue;
        //for (int i = 0; i < m.mesh.vertexCount; i++)
        //{
        //    if (min > m.mesh.vertices[i].z)
        //    {
        //        min = m.mesh.vertices[i].z;
        //    }

        //    if (max < m.mesh.vertices[i].z)
        //    {
        //        max = m.mesh.vertices[i].z;
        //    }
        //}

        //diameter = max - min;
        //Debug.Log("Diameter = " + diameter);
    }

    // Update is called once per frame
    void Update()

    {

        rotation = car.GetComponent<LogitechSteeringWheel>().rotationSpeed;


        if (this.name == "Wheel_LF" || this.name == "Wheel_RF")
        {

            transform.Rotate((float)(36000 * Time.deltaTime * speed * 0.63f) / Mathf.PI, 0f, 0f, Space.Self);

            if (tempRotation != rotation)
            {

                transform.Rotate(0f, 0f, -rotation / 37642  / Mathf.PI * 10f, Space.Self);
                tempRotation = rotation;

            }
    

            //transform.rotation = transform.rotation * Quaternion.Euler((float)(36000 * Time.deltaTime * speed * 0.63f) / Mathf.PI, 0f , rotation / 37642 * 10f);

            // transform.rotation = Quaternion.Euler((float)(36000 * Time.deltaTime * speed * 0.63f) / Mathf.PI, -rotation / 37642 * 5f, 0f);
            //transform.Rotate((float)(36000 * Time.deltaTime * speed * 0.63f) / Mathf.PI, -rotation / 37642 * 5f, 0f, Space.Self);

        }
        else
        {

            transform.Rotate((float)(36000 * Time.deltaTime * speed * 0.63f) / Mathf.PI, 0f, 0f, Space.Self);

        }
    }
}