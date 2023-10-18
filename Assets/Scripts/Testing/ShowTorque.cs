using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowTorque : MonoBehaviour
{
    public float currentTorque;
    WheelCollider w_collider;
    // Start is called before the first frame update
    void Start()
    {
        w_collider = GetComponent<WheelCollider>();
    }

    // Update is called once per frame
    void LateUpdate()
    {
        currentTorque = w_collider.motorTorque;
      //  Debug.Log("Torque is: " + currentTorque + " brake torque is: " + collider.brakeTorque);
    }
}
