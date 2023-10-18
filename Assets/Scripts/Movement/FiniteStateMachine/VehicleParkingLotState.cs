using System;
using System.Threading.Tasks;
using UnityEngine;

public class VehicleParkingLotState : VehicleBaseState
{
    private float baseSpeed=0, rpm;
    System.Random rand = new System.Random();
    bool driving = true;
    bool pause = true;
    bool detecting = false;
    bool rayCastExit = false;

    private const float rayCastOneRadius = 2.0f;
    RaycastHit hitInfo;
    public static event Action<GameObject> exitedParkingLot;

    async void updateSpeed(CarController vm, float sec) {
        while (driving) {
            //Calculate the car's speed
            vm.speed = baseSpeed;

            // calc rpm based on new speed
            // rpm = (vm.speed / 60f) * 63360f / 91.4202f;
            // rpm = (vm.speed / 60f) * 63360f / 91.4202f + ((vm.speed-vm.speedMPH)*100f);
            // use PID controller to calc rpm (accel) to match set speed to actual speed
            rpm = vm.pidController.Update(vm.speed,vm.speedMPH,sec);
            await Task.Delay(TimeSpan.FromSeconds(sec));
        }
    }
    /*
    Pauses the exiting parking Lot vehicle before the intersection
    Also activates blinker and calls detectCars(CarController, float)
    */
    async void pauseVehicle(CarController vm, float sec) {
        Debug.Log(vm.name + "Hit merging intersection");
        vm.startBlinker(true);
        vm.simController.Move(0f, 0f, -1f, 1f);
        await Task.Delay(TimeSpan.FromSeconds(sec));
        detecting = true;
        detectCars(vm, 0.25f);
    }

    /*
    After waiting for an alloted time, detectCars(CarController, float) is called
    in order to detect oncoming traffic. the exiting parking lot vehicle will only
    merge into traffic if no cars are detected by its spherecasts.
    */
    async void detectCars(CarController vm, float sec) {
        while(detecting) {
            await Task.Delay(TimeSpan.FromSeconds(sec));

            Debug.DrawRay(vm.raycastTransform.position, Quaternion.AngleAxis((45), Vector3.up) * -vm.transform.right * 20f, Color.blue);
            // Car detection spherecast
            bool bCarsDetected = Physics.SphereCast(vm.raycastTransform.position, rayCastOneRadius*2.0f, Quaternion.AngleAxis((45), Vector3.up) * -vm.transform.right, out hitInfo, 20f, LayerMask.GetMask("Car"));
    
            if(!bCarsDetected) {
                // Sends an event to the CarSpawner to route a new path for the car to follow
                exitedParkingLot?.Invoke(vm.gameObject);
                vm.speed = baseSpeed*4;
                // Calculates the nearest node (the next point the car must follow) to the car
                vm.calcCurrentNode();
                detecting = false;
                vm.SwitchState(vm.vehicleDriveState);
            }
        }
    }

    public override void EnterState(CarController vm) {
        Debug.Log(vm.name + " - Enter ParkingLot State");
        vm.curState = "ParkingLot";
        driving = true;

        // store base vehicle speed (if not already stored)
        if (baseSpeed <= 0)
            baseSpeed = vm.speed/4;

        // calculate rpm using speed value from Base Controller
        rpm = (baseSpeed / 60f) * 63360f / 91.4202f;

        // update final vehicle speed using contributions every 75ms
        updateSpeed(vm, 0.075f);
    }

    public override void UpdateState(CarController vm)
    {   
        vm.CheckParkedCarWaypoint(1f);
        
        //Raycast to detect the crosswalk
        if (!rayCastExit) {
            rayCastExit = Physics.Raycast(vm.raycastTransform.position, vm.transform.forward, 0.5f, LayerMask.GetMask("ParkingLotExit"));
            Debug.DrawRay(vm.raycastTransform.position, vm.transform.forward * 0.5f, Color.red);
        }

        if (vm.getForceBrake() || (vm.raycastTransform && vm.brakeDistance > 0f))
        {
            // Brake state for when vehicle is too close to another
            if (vm.ShouldBrake(vm.brakeDistance / 5f))
            {
                // driving = false;
                // Conditions met to enter the Brake State
                vm.simController.Move(0f, 0f, -1f, 1f);
            }

            // Avoid State for when bike is nearby to vehicle
            else if (vm.rayCastDrivingBike()) {
                // Conditions met to enter the Avoid State
                vm.SwitchState(vm.vehicleAvoidState);
            }
            // Intersection State for when vehicle enters intersections
            else if (rayCastExit) {
                // Conditions met to enter the Intersection State
                driving = false;
                // stop
                vm.simController.Move(0f, 0f, -1f, 1f);

                // check for cars, then enter drive state
                if(pause) {
                    pauseVehicle(vm, 2.0f);
                    pause = false;
                }
            }
            // Default Drive State
            else
            {
                driving = true;
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
