using System;
using System.Threading.Tasks;
using UnityEngine;

public class VehicleDriveState : VehicleBaseState
{
    private float baseSpeed=0, rpm, curRandomSpeedMag=0, curSlopeSpeedMag=0;
    System.Random rand = new System.Random();
    bool driving = true;
    
    async void updateSpeedRandom(CarController vm, float sec) {
        while (driving && (vm != null)) {
            // get random speed range (between -randSpeedVariation and +randSpeedVariation)
            curRandomSpeedMag = ((float)rand.NextDouble()*2f-1.0f) * vm.randSpeedVariation;
            await Task.Delay(TimeSpan.FromSeconds(sec));
        }
    }

    async void updateSpeedSlope(CarController vm, float sec) {
        while (driving && (vm != null)) {
            // Calculate the speed multiplier of the car using the slope
            curSlopeSpeedMag = 
                Mathf.Tan(Mathf.Deg2Rad*vm.transform.localEulerAngles.x) * (vm.slopeSpeedMultiplier * 30f + 5f);
            
            // Clamp the speed to +/- 8
            curSlopeSpeedMag = Mathf.Clamp(curSlopeSpeedMag, -vm.maxSlopeSpeedGain, vm.maxSlopeSpeedGain);

            await Task.Delay(TimeSpan.FromSeconds(sec));
        }
    }

    async void updateSpeed(CarController vm, float sec) {
        while (driving && (vm != null)) {
            //Calculate the car's speed
            vm.speed = baseSpeed + curSlopeSpeedMag + curRandomSpeedMag;

            // calc rpm based on new speed
            //rpm = (vm.speed / 60f) * 63360f / 91.4202f;
            //rpm = (vm.speed / 60f) * 63360f / 91.4202f + ((vm.speed-vm.speedMPH)*100f);
            // use PID controller to calc rpm (accel) to match set speed to actual speed
            rpm = vm.pidController.Update(vm.speed,vm.speedMPH,sec);
            await Task.Delay(TimeSpan.FromSeconds(sec));
        }
    }

    public override void EnterState(CarController vm) {
        Debug.Log(vm.name + " - Enter Drive State");
        vm.curState = "Drive";
        driving = true;

        // store base vehicle speed (if not already stored)
        if (baseSpeed <= 0)
            baseSpeed = vm.speed;

        // calculate rpm using speed value from Base Controller
        rpm = (baseSpeed / 60f) * 63360f / 91.4202f;

        // calc the vehicle speed contribution (using random variation) every 5 seconds
        updateSpeedRandom(vm, 5f);

        // calc the vehicle speed contribution (based on slope) every 120ms
        updateSpeedSlope(vm, 0.12f);

        // update final vehicle speed using contributions every 75ms
        updateSpeed(vm, 0.075f);

        // Stop blinker if turn signals are activated
        vm.stopBlinker();
    }

    public override void UpdateState(CarController vm)
    {   
        vm.CheckWaypoint();
        
        //Raycast to detect the crosswalk
        bool rayCastCrosswalk = Physics.Raycast(vm.raycastTransform.position, vm.transform.forward, vm.crosswalkRaycastLength, LayerMask.GetMask("Crosswalk"));
        Debug.DrawRay(vm.raycastTransform.position, vm.transform.forward * vm.crosswalkRaycastLength, Color.red);

        if (vm.getForceBrake() || (vm.raycastTransform && vm.brakeDistance > 0f))
        {
            // Brake state for when vehicle is too close to another
            if (vm.ShouldBrake(vm.brakeDistance))
            {
                // Conditions met to enter the Brake State
                driving = false;
                vm.SwitchState(vm.vehicleBrakeState);
            }

            // Avoid State for when bike is nearby to vehicle
            else if (vm.rayCastDrivingBike()) {
                // Conditions met to enter the Avoid State
                driving = false;
                vm.SwitchState(vm.vehicleAvoidState);
            }

            // Intersection State for when vehicle enters intersections
            else if (rayCastCrosswalk) {
                // Conditions met to enter the Intersection State
                driving = false;
                vm.SwitchState(vm.vechicleIntersectionState);
            }
            // Default Drive State
            else
            {
                // Drive State Behavior
                float steer = vm.GetSteer();
                vm.simController.Move(steer, rpm, 0f, 0f);
                // rigidbody.velocity = rigidbody.velocity.normalized * speedInMetersPerSecond;
                vm.brakeVelocity = Vector3.zero;
                vm.velocityBeforeBrake = Vector3.zero;
            }
        }
    }


}
