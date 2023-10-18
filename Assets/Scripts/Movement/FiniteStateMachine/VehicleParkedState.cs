using System;
using System.Threading.Tasks;
using UnityEngine;

public class VehicleParkedState : VehicleBaseState
{
    const float rayCastOneRadius = 2.0f;
    private float baseSpeed=0, rpm;
    bool driving = true;
    bool bPlayerDetected = false;
    bool finishedMerging = false;
    bool[] arrays;
    RaycastHit hitInfo;

    public override void EnterState(CarController vm) {
        Debug.Log(vm.name + " - Enter Parked State");
        vm.curState = "Parked";

        // store base vehicle speed (if not already stored)
        if (baseSpeed <= 0)
            baseSpeed = vm.speed;

        rpm = 0; // parked

        // update final vehicle speed using contributions every 75ms
        updateSpeed(vm, 0.075f);

        //Array of raycasts from CarController
        arrays = new bool[4];
    }

    async void updateSpeed(CarController vm, float sec) {
        while (driving) {
            //Calculate the car's speed
            vm.speed = baseSpeed;

            // use PID controller to calc rpm (accel) to match set speed to actual speed
            rpm = vm.pidController.Update(vm.speed,vm.speedMPH,sec);
            await Task.Delay(TimeSpan.FromSeconds(sec));
        }
    }

    public override void UpdateState(CarController vm)
    {  
        vm.CheckWaypoint();

        if (!bPlayerDetected) {
            bPlayerDetected = Physics.SphereCast(vm.raycastTransform.position, rayCastOneRadius, Quaternion.AngleAxis((-75), Vector3.up) * -vm.transform.right, out hitInfo, 15f, LayerMask.GetMask("Player"));         
            Debug.DrawRay(vm.raycastTransform.position,  Quaternion.AngleAxis((-75), Vector3.up) * -vm.transform.right * 15f, Color.green);
        }

        vm.simController.Move(0f, 0f, -1f, 1f);  // apply brakes

        if (vm.getForceBrake() || (vm.raycastTransform && vm.brakeDistance > 0f))
        {
            // Avoid State for when bike is nearby to vehicle
            if (arrays[0]) {
                bPlayerDetected = true;
                Debug.Log(vm.name + " -> detected bike from " +  vm.curState);
            }
            if(bPlayerDetected) {
                //flips side of raycast origin
                vm.bikeDetectorTransform.localPosition = new Vector3(-1.081f, vm.bikeDetectorTransform.localPosition.y, vm.bikeDetectorTransform.localPosition.z);
                //vm.raycastTransform.localPosition = new Vector3(vm.raycastTransform.localPosition.x, vm.raycastTransform.localPosition.y, -vm.raycastTransform.localPosition.z);

                // Check for cars before merging
                Debug.DrawRay(vm.bikeDetectorTransform.position, Quaternion.AngleAxis((-45), Vector3.up) * -vm.transform.right * 15f, Color.red);
                bool bCarsDetected = Physics.SphereCast(vm.bikeDetectorTransform.position, rayCastOneRadius*2.0f, Quaternion.AngleAxis((-45), Vector3.up) * -vm.transform.right, out hitInfo, 15f, LayerMask.GetMask("Car"));
                if(!bCarsDetected) {
                    // Start merging
                    mergeTimer((float)0.75);
                    
                    //Gets set to true after allotted time
                    if(!finishedMerging) {
                        float steer = vm.GetSteer();
                        //hardcoded "steer" variable to be -0.35 for merging cars
                        vm.simController.Move((float)-0.35, rpm, 0f, 0f);
                        vm.brakeVelocity = Vector3.zero;
                        vm.velocityBeforeBrake = Vector3.zero;
                    } else {
                        driving = true;
                        Debug.Log(vm.name + "No cars detected, exiting" +  vm.curState);
                        // flip bikeDetector Transform back to original position
                        vm.bikeDetectorTransform.localPosition = new Vector3(1.081f, vm.bikeDetectorTransform.localPosition.y, vm.bikeDetectorTransform.localPosition.z);
                        vm.SwitchState(vm.vehicleDriveState);
                    }
                    
                }
            }
        }

    }

    async void mergeTimer(float time) {
        await Task.Delay(TimeSpan.FromSeconds(time));
        finishedMerging = true;
    }
}
