using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crossway : MonoBehaviour
{
    public bool pedestrianOnCrossway;
    public bool crosswayActive;
    public bool CrosswayActive {
        get
        {
            return crosswayActive;
        }
        set
        {
            if (crosswayActive != value)
            {
                crosswayActive = value;
              //  CheckActive();
            }
        }
    }

    [EditorButton("Change Pedestrian Active", "ChangePedestrianActive")]
    public bool changePed;

    [EditorButton("Change Crossway Active", "ChangeActive")]
    public bool change;

    public void ChangePedestrianActive()
    {
        PedestrianEnteredOrExitedCrossway(!pedestrianOnCrossway);
    }

    public void ChangeActive()
    {
        CrosswayActive = !CrosswayActive;
    }

    private HashSet<CarController> closeCars = new HashSet<CarController>();
    private HashSet<CarController> farCars = new HashSet<CarController>();

    void Start()
    {

    }

    public void PedestrianEnteredOrExitedCrossway(bool didEnter)
    {
        pedestrianOnCrossway = didEnter;
        CheckActive();
    }

    public void TriggeredCloseDistance(CarController controller, bool entered)
    {
        if (entered)
            closeCars.Add(controller);
        else
        {
            controller.ForceBrake(false);
            closeCars.Remove(controller);
        }
        CheckActive();
    }


    public void TriggeredFarDistance(CarController controller, bool entered)
    {
        if (entered)
            farCars.Add(controller);
        else
        {
            controller.ForceBrake(false);
            farCars.Remove(controller);
        }
        CheckActive();
    }

    void CheckActive()
    {
        foreach(CarController controller in closeCars)
        {
            if(pedestrianOnCrossway)
            {
                Debug.Log("Force car to brake!");
                controller.ForceBrake(true);
            } else
            {
                controller.ForceBrake(false);
            }
        }

        foreach (CarController controller in farCars)
        {
            if (IsActive())
            {
                Debug.Log("Force car to brake!");
                controller.ForceBrake(true);
            }
            else
            {
                controller.ForceBrake(false);
            }
        }
    }

    bool IsActive()
    {
        return CrosswayActive || pedestrianOnCrossway;
    }
}
