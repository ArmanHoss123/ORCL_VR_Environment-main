using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VehicleBrakeState : VehicleBaseState
{
    public override void EnterState(CarController vm) {
        Debug.Log(vm.name + " - Enter Brake State");
        vm.curState = "Brake";
    }

    public override void UpdateState(CarController vm)
    {
        vm.CheckWaypoint();
        
        if (vm.velocityBeforeBrake == Vector3.zero)
        {
            vm.velocityBeforeBrake = vm.rigidbody.velocity;
        }

        Vector3 newVelocity = vm.velocityBeforeBrake - vm.brakeVelocity;
        if (newVelocity.x < 0f && vm.velocityBeforeBrake.x > 0f)
            newVelocity.x = 0f;
        else if (newVelocity.x > 0f && vm.velocityBeforeBrake.x < 0f)
            newVelocity.x = 0f;

        if (newVelocity.y < 0f && vm.velocityBeforeBrake.y > 0f)
            newVelocity.y = 0f;
        else if (newVelocity.y > 0f && vm.velocityBeforeBrake.y < 0f)
            newVelocity.y = 0f;

        if (newVelocity.z < 0f && vm.velocityBeforeBrake.z > 0f)
            newVelocity.z = 0f;
        else if (newVelocity.z > 0f && vm.velocityBeforeBrake.z < 0f)
            newVelocity.z = 0f;

        if (newVelocity.x == 0f)
            vm.simController.Move(0f, 0f, -1f, 1f);
        else
            vm.simController.Move(0f, 0f, 0f, 0f);

        vm.rigidbody.velocity = newVelocity;
        vm.brakeVelocity += vm.brakeSpeed * vm.velocityBeforeBrake.normalized * Time.deltaTime;

        //Assuming we've entered break state via the Intersection state, then if the traffic light is red or yellow, then we must remain in brake
        if (!vm.ShouldBrake(vm.brakeDistance) && (vm.trafficLight != TrafficManager.lightColor.red && vm.trafficLight != TrafficManager.lightColor.yellow)) {
            Debug.Log(vm.name + " - Switching back to drive state from break (1)");
            vm.SwitchState(vm.vehicleDriveState);
        }

        //If there are no cars in front and the light is green, exit out of brake
        if (!vm.ShouldBrake(vm.brakeDistance) && vm.trafficLight == TrafficManager.lightColor.green) {
            // Conditions met to enter the Drive State
            Debug.Log(vm.name + " - Green light, switching back to Drive");
            vm.SwitchState(vm.vehicleDriveState);
        }
    }
}
