using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrosswayCollider : MonoBehaviour
{
    public enum ColliderType { Close = 0, Far = 1, Pedestrian = 2}
    public ColliderType colliderType;
    public float carDirectionStop;
    public Crossway crossway;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        CarController controller = other.GetComponent<CarController>();
        if (controller)
        {
            Vector3 direction =  controller.transform.rotation * Vector3.forward;
            if (Mathf.Sign(direction.x) != Mathf.Sign(carDirectionStop))
                return;

            if (colliderType == ColliderType.Close)
                crossway.TriggeredCloseDistance(controller, true);
            else if (colliderType == ColliderType.Far)
                crossway.TriggeredFarDistance(controller, true);
        }
        else if(colliderType == ColliderType.Pedestrian)
        {
            PedestrianController ped = other.GetComponent<PedestrianController>();
            if (ped)
                crossway.PedestrianEnteredOrExitedCrossway(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        CarController controller = other.GetComponent<CarController>();
        if (controller)
        {
            if (colliderType == ColliderType.Close)
                crossway.TriggeredCloseDistance(controller, false);
            else if (colliderType == ColliderType.Far)
                crossway.TriggeredFarDistance(controller, false);
        }
        else if (colliderType == ColliderType.Pedestrian)
        {
            PedestrianController ped = other.GetComponent<PedestrianController>();
            if (ped)
                crossway.PedestrianEnteredOrExitedCrossway(false);
        }
    }
}
