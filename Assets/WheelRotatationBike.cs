using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WheelRotatationBike : MonoBehaviour
{
    float speed = 0.5f;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {   
        
        transform.Rotate(0f, (float)(36000 * Time.deltaTime * speed * 0.63f) / Mathf.PI, 0f, Space.Self);

    }
}
