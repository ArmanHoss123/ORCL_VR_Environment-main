using System;
using System.Threading.Tasks;
using UnityEngine;

public class VehicleIntersectionState : VehicleBaseState
{
    private float rpm, brakeForce=0;
    bool intersectDetected = true;

    async void updateSpeed(CarController vm, float sec) {
        while (intersectDetected) {
            // use PID controller to calc rpm (accel) - simulate slightly letting off the gas (70%)
            rpm = vm.pidController.Update(vm.speed*.70f,vm.speedMPH,sec);
            await Task.Delay(TimeSpan.FromSeconds(sec));
        }
    }

    async void applyMomentaryBrake(float sec) {
        // we apply a momentary brake as car approaches the intersection
        brakeForce = 0.01f;
        await Task.Delay(TimeSpan.FromSeconds(sec));
        brakeForce = 0f;
    }

    // Start is called before the first frame update
    public override void EnterState(CarController vm){
        Debug.Log(vm.name + " - Enter Intersection State");
        vm.curState = "Intersection";
        intersectDetected = true;
        
        // update final vehicle speed using contributions every 75ms
        updateSpeed(vm, 0.075f);

        // apply momentary brake force for 500ms 
        applyMomentaryBrake(0.5f);
    }

    public override void UpdateState(CarController vm){
        vm.CheckWaypoint();
        
        //Only need to cast one raycast in front of car to detect if crosswalk is ahead
        intersectDetected = Physics.Raycast(vm.raycastTransform.position, vm.transform.forward, vm.crosswalkRaycastLength, LayerMask.GetMask("Crosswalk"));
        Debug.DrawRay(vm.raycastTransform.position, vm.transform.forward * vm.crosswalkRaycastLength, Color.red);

        //execute original drive behavior with reduced rpm 
        if(vm.trafficLight == TrafficManager.lightColor.red && vm.trafficLightDistance >= vm.trafficBreakDistance/2) {
            Debug.Log(vm.name + "braking - detected red light from " + vm.curState);
            vm.SwitchState(vm.vehicleBrakeState);
        }
        if(vm.trafficLight == TrafficManager.lightColor.yellow && vm.trafficLightDistance >= vm.trafficBreakDistance) {
                Debug.Log(vm.name + "braking - detected distant yellow light " + vm.curState);
                vm.SwitchState(vm.vehicleBrakeState);
        }
        if(vm.ShouldBrake(vm.brakeDistance)) {
            Debug.Log(vm.name + "braking - detected vehicle from " + vm.curState);
            vm.SwitchState(vm.vehicleBrakeState);
        }
        //After applying a momentary break, the vechicle coasts through the intersection until it exits and returns to drive
        if(intersectDetected) {
            // Drive State Behavior
            float steer = vm.GetSteer();
            vm.simController.Move(steer, rpm, brakeForce, 0f);
            // rigidbody.velocity = rigidbody.velocity.normalized * speedInMetersPerSecond;
            vm.brakeVelocity = Vector3.zero;
            vm.velocityBeforeBrake = Vector3.zero;
        //swap out of VehicleIntersectionState if raycast no longer detects crosswalk
        } else {
            vm.SwitchState(vm.vehicleDriveState);
        }
        
    }

}
