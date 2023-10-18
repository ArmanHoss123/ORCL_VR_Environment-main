using System;
using System.Threading.Tasks;
using UnityEngine;

public class VehicleAvoidState : VehicleBaseState
{
    private float rpm;
    bool driving = true;
    bool avoiding = true;
    float steer = 0f;
    float raycastOneLength;

    async void avoidTimer(CarController vm) {
        Debug.Log(vm.name + " -> Avoiding = "+ avoiding);
        vm.simController.Move((vm.avoidMagnitude *-1f), rpm, 0f, 0f);
        await Task.Delay(TimeSpan.FromSeconds(.25));

        avoiding = false;
        Debug.Log(vm.name + " -> Avoiding = "+ avoiding);
    }
    
    public override void EnterState(CarController vm) {
        Debug.Log(vm.name + " - Enter Avoid State");
        vm.curState = "Avoid";
        avoiding = true;

        // Calculate length of RaycastOne
        raycastOneLength = vm.raycastDistance/Mathf.Cos((Mathf.PI/180f) * vm.rayCastOneAngle);

        // maintain current speed (update every 0.75ms)
        updateSpeed(vm, 0.075f);
    }

    async void updateSpeed(CarController vm, float sec) {
        while (avoiding) {
            // use PID controller to calc rpm (accel) to match set speed to actual speed, slow down to 75% of set speed
            rpm = vm.pidController.Update(vm.speed*0.75f,vm.speedMPH,sec);
            await Task.Delay(TimeSpan.FromSeconds(sec));
        }
    }

    public override void UpdateState(CarController vm)
    {
        vm.CheckWaypoint();

        if (vm.getForceBrake() || (vm.raycastTransform && vm.brakeDistance > 0f))
        {
            if (vm.ShouldBrake(vm.brakeDistance))
            {
                // Conditions met to enter the Brake State
                driving = false;
                avoiding = false;
                vm.SwitchState(vm.vehicleBrakeState);
            }
            else if (!vm.rayCastDrivingBike() && !avoiding) {
                vm.SwitchState(vm.vehicleDriveState);
            }
            else if (Physics.Raycast(vm.raycastTransform.position,  Quaternion.AngleAxis(vm.rayCastOneAngle, Vector3.up) * vm.transform.right, raycastOneLength, LayerMask.GetMask("Crosswalk"))) {
                avoiding = false;
                vm.SwitchState(vm.vechicleIntersectionState);
            }
            else
            {
                //Avoid for a set amount of time and go back to drive state
                avoidTimer(vm);
            }
        }
    }


}
