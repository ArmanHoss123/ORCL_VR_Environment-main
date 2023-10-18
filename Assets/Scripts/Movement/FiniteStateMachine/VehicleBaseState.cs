using UnityEngine;

public abstract class VehicleBaseState
{
    public abstract void EnterState(CarController vm);

    public abstract void UpdateState(CarController vm);

}
