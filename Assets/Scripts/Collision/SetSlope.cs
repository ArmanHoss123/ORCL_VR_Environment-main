using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetSlope : MonoBehaviour
{
    public enum SlopeType
    {
        Drift = 0,
        Elevation = 1,
        Both = 2
    }

    public enum Direction
    {
        BikeBehind = -1,
        BikeInFront = 1,
        Any = 0
    }
    public float slopeAngle;
    public SlopeType slopeType;
    public Direction directionEnter;
    public Transform transformDirectionCheck;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        BicycleController bike = collision.GetComponent<BicycleController>();
        if (bike)
        {
            float dir = Mathf.Sign(bike.transform.position.z - transformDirectionCheck.position.z);
            if(directionEnter == Direction.Any || (int)dir == (int)directionEnter)
            {
                if (slopeType == SlopeType.Drift || slopeType == SlopeType.Both)
                    bike.SetDriftSlope(slopeAngle);
                if(slopeType == SlopeType.Elevation || slopeType == SlopeType.Both)
                    bike.SetElevationSlope(slopeAngle);
            }
        }
    }
}
