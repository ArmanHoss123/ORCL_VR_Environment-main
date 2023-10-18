using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AirSimUnity;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
public class CarController : Controller
{
    [Header("Vehicle State")]
    [Tooltip("Actual speed of vehicle in MPH")]
    public float speedMPH;
    [Tooltip("Current AI State of Vehicle")]
    public string curState = "None";
    [Header("For slowing down and speeding up if anything in front")]
    public float brakeDistance = 5f;
    public float brakeSpeed = 2f;

    [Header("For checking for objects in front")]
    public Transform raycastTransform;
    public int carLayer = 9;
    public int playerLayer = 10;
    public int crosswayLayer = 13;
    public float maxSteerAngle = 90f;
    [Header("For checking for objects in Bike-lane")]
    public Transform bikeDetectorTransform;
    [Tooltip("How much the car will try to avoid a bicycle")]
    [Range(0.00f, 0.50f)]
    public float avoidMagnitude = StateSettingController.avoidMagnitude;
    [Tooltip("The angle of the raycast used to detect the bike at a distance")]
    [Range(-90f, 90f)]
    public float rayCastOneAngle = StateSettingController.rayCastOneAngle;
    [Tooltip("Distance to cast the main ray used to detect bicycles at a distance")]
    [Range(0.1f,8f)]
    public float raycastDistance = StateSettingController.raycastDistance;
    [Header("For speed variation behaviors")]
    [Tooltip("Amount of variation in the regular driving speed")]
    [Range(0f,5f)]
    public float randSpeedVariation = StateSettingController.randSpeedVariation;
    [Tooltip("Multiplier for speed variation based on slope")]
    [Range(0f,10f)]
    public float slopeSpeedMultiplier = 1.25f;
    [Tooltip("Clamp max slope speed gain or loss to this value")]
    [Range(0f,20f)]
    public float maxSlopeSpeedGain = 8f;
    [Tooltip("Distance to detect crosswalks")]
    [Range(0f,25f)]
    public float crosswalkRaycastLength = 13f;

    [Header("Wheels will rotate depending on car speed")]
    public AirSimCarController simController;
    
    [Header("For Size")]
    public BoxCollider sizeCollider;
    public bool constantSpeed;

    public Transform[] Nodes;

    public int CarId { get; set; }
    public string FolderLocation { get; set; }
    public int Model { get; set; }
    public Light rightRearBlinker;
    private bool blinking = false;

    public System.Action Destroyed;
    private RaycastHit[] castsHit;
    private float slopeAngleElevation;
    private bool forceBrake;
    public Vector3 velocityBeforeBrake;
    private bool brakingDueToCrosswalk;
    private float distanceCheckCrossway = 0.9f;
    private string fileName;
    [SerializeField]
    public int currentNode;
    private const float minError = 5f;
    private float directionForNodes;

    public Vector3 brakeVelocity;
    private float velocity;
    private float Velocity { get { return velocity; } set { velocity = value; } }
    public float position;
    public float positiony;
    public float positionz;

    // Stores light value of the traffic light in front of the car
    public TrafficManager.lightColor trafficLight;
    [SerializeField]
    public float trafficLightDistance = -1f;
    public float trafficBreakDistance = 15f;

    TrafficManager tm = null;

    public float Position { get { return position; } }
    private bool shouldExecuteThread;
    private bool setSpeedOnce;
    public bool ShouldExecuteThread
    
    
    {
        get
        {
            return shouldExecuteThread;
        }
    }

    Thread encoderThread;

    // State Management:
    VehicleBaseState currentState;
    public VehicleDriveState vehicleDriveState = new VehicleDriveState();
    public VehicleBrakeState vehicleBrakeState = new VehicleBrakeState();
    public VehicleAvoidState vehicleAvoidState = new VehicleAvoidState();
    public VehicleParkedState vehicleParkedState = new VehicleParkedState();
    public VehicleIntersectionState vechicleIntersectionState = new VehicleIntersectionState();
    public VehicleParkingLotState vehicleParkingLotState = new VehicleParkingLotState();

    [HideInInspector]
    // PID Controller
    public PID pidController = new PID(45.0f,3.5f,0.25f);

    protected override void Start()
    {
        avoidMagnitude = StateSettingController.avoidMagnitude;
        rayCastOneAngle = StateSettingController.rayCastOneAngle;
        raycastDistance = StateSettingController.raycastDistance;
        rightRearBlinker.intensity = 0;
    
        base.Start();
        if (!simController)
            simController = GetComponent<AirSimCarController>();
        // this appears to be incorrect, limiting the maxspeed to the start speed of 26mph
        //simController.MaxSpeed = speed;
        simController.MaxSpeed = 40f; // limit maxspeed of vehicle to 40mph
        gameObject.layer = carLayer;
        tm = GameObject.FindWithTag("TrafficManager").GetComponent<TrafficManager>();
        rigidbody = GetComponentInChildren<Rigidbody>();

        //directionForNodes = Mathf.Sign(Nodes[1].position.x - Nodes[0].position.x);
        
        // initial state - BrakeState
        //SwitchState(vehicleBrakeState);
        SwitchState(currentState);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        shouldExecuteThread = false;
        if(Destroyed != null)
            Destroyed();
    }

    protected override void Update()
    {
        //CheckWaypoint();

        // run update in the current state (fsm)
        currentState.UpdateState(this);

        // update velocity & position
        velocity = rigidbody.velocity.magnitude * 2.23693629f;
        position = transform.position.x;
        positiony = transform.position.y;
        positionz = transform.position.z;

        speedMPH = simController.CurrentSpeed;
    }   

    public void initNodes() {
        directionForNodes = Mathf.Sign(Nodes[1].position.x - Nodes[0].position.x);
        calcCurrentNode();
    }

    public void SwitchState(VehicleBaseState state) {
        currentState = state;
        currentState.EnterState(this);
    }

    public float GetSteer()
    {
        Vector3 relativeVector = transform.InverseTransformPoint(Nodes[currentNode].position);
        return (relativeVector.x / relativeVector.magnitude);
    }

    public void CheckWaypoint()
    {
        if(Mathf.Sign(transform.position.x - Nodes[currentNode].position.x) == directionForNodes)
        {
            if (currentNode == Nodes.Length - 1)
                currentNode = 0;
            else
                currentNode++;
        }
    }

    public void CheckParkedCarWaypoint(float minError) {
        //Finds the distance between the car and next node
        float dist = Vector3.Distance(transform.position, Nodes[currentNode].position);
        if(dist <= minError) {
            if (currentNode == Nodes.Length - 1)
                currentNode = 0;
            else
                currentNode++;
        }
    }

    public void calcCurrentNode() {
        int tempCurNode = 0;
        float minDist = 99f;
        for(int i =0; i < Nodes.Length; i++) {
            float dist = Vector3.Distance(transform.position, Nodes[i].position);
            if(dist < minDist) {
                tempCurNode = i;
                minDist = dist;
            }
        }
        currentNode = tempCurNode;
        directionForNodes = Mathf.Sign(Nodes[1].position.x - Nodes[0].position.x);
    }

    public void startBlinker(bool rightBlinker) {
        blinking = true;
        if (rightBlinker)
            blink(rightRearBlinker, 0.3f);
        else{
            // pass in left blinker
            return;
        }
    }

    public void stopBlinker() {
        stopBlink(2f);
    }

    public void ForceBrake(bool brake)
    {
        forceBrake = brake;
    }

    public bool getForceBrake() 
    {
        return forceBrake;
    }

    public void ApplyLocalPositionToVisuals(WheelCollider collider)
    {
        if (collider.transform.childCount == 0)
        {
            return;
        }

        Transform visualWheel = collider.transform.GetChild(0);

        Vector3 position;
        Quaternion rotation;
        collider.GetWorldPose(out position, out rotation);

        visualWheel.transform.position = position;
        visualWheel.transform.rotation = rotation;
    }

    // TODO: Candidate function - Move logic to individual states
    public bool ShouldBrake(float distance)
    {
        if (forceBrake)
            return true;
        /*if (Physics.Raycast(raycastTransform.position, transform.forward, distance, LayerMask.GetMask("Car")))
        {
            brakingDueToCrosswalk = false;
            return true;
        }*/

        // Check for raycast hit to crossway layer
        RaycastHit[] castsHit = Physics.RaycastAll(raycastTransform.position - transform.forward * distanceCheckCrossway, transform.forward, distance + distanceCheckCrossway, 1 << crosswayLayer, QueryTriggerInteraction.Collide);
        foreach(RaycastHit hit in castsHit)
        {
            GameObject objHit = hit.collider.gameObject;
            if (objHit.GetComponentInParent<Crossway>().CrosswayActive)
            {
                Vector3 posDiff = objHit.transform.position - transform.position;
                float dot = Vector3.Dot(posDiff, transform.forward);
                if (brakingDueToCrosswalk || dot > 0f)
                {
                    brakingDueToCrosswalk = true;
                    return true;
                }
            }
        }

        // check to see if car RigidBody would hit player
        RaycastHit[] hits = rigidbody.SweepTestAll(transform.forward, (raycastTransform.position - rigidbody.position).z + distance, QueryTriggerInteraction.Collide);
        foreach(RaycastHit rHit in hits)
        {
            if (rHit.collider.gameObject.layer == playerLayer)
            {
                brakingDueToCrosswalk = false;
                return true;
            }
            if (rHit.collider.gameObject.layer == carLayer)
            {
                brakingDueToCrosswalk = false;
                return true;
            }
        }

        brakingDueToCrosswalk = false;
        return false;
    }

    // Raycast a sphere to detect if near the player while driving
    public bool rayCastDrivingBike() {
        float raycastOneLength = raycastDistance/Mathf.Cos((Mathf.PI/180f) * (rayCastOneAngle * -1f));
        const float rayCastOneRadius = 2.5f;
 
        RaycastHit hitInfo;

        Debug.DrawRay(bikeDetectorTransform.position,  Quaternion.AngleAxis((rayCastOneAngle * -1f), Vector3.up) * transform.right * raycastOneLength, Color.green);
        if (Physics.SphereCast(bikeDetectorTransform.position, rayCastOneRadius, Quaternion.AngleAxis((rayCastOneAngle * -1f), Vector3.up) * transform.right, out hitInfo, raycastOneLength, LayerMask.GetMask("Player"))) {
            if (hitInfo.transform.gameObject.tag == "Player")
                return true;
        }
        return false;
    }

    // Stores the lightState of the traffic light and distance from the car
    private void OnTriggerStay(Collider other) {
        //TrafficManager lamp = other.gameObject.GetComponent<TrafficManager>();
        if(other.CompareTag("StreetLightNS") || other.CompareTag("StreetLightEW")) {
            trafficLight = tm.lightState;
            trafficLightDistance = Vector3.Distance(raycastTransform.position, other.transform.position);
        }
    }

    //If trafficLightDistance is -1, we know that car is not in the traffic Light trigger collider
    private void OnTriggerExit(Collider other) {
        trafficLight = TrafficManager.lightColor.green;
        trafficLightDistance = -1f;
    }

    private async void blink(Light rearBlinker, float sec) {
        while(blinking) {
            rearBlinker.intensity = 1;
            await Task.Delay(TimeSpan.FromSeconds(sec));
            rearBlinker.intensity = 0;
            await Task.Delay(TimeSpan.FromSeconds(sec));
        }
    }

    private async void stopBlink(float sec) {
        await Task.Delay(TimeSpan.FromSeconds(sec));
        blinking = false;
    }


    #region Data Export

    public override void ExportData()
    {
        Debug.Log("Calling export data id: " + CarId);
        base.ExportData();
    }

    protected override string FileName()
    {
        string carData = Path.Combine(Utils.GetOrCreateDataFolder(), "Car Data");
        if (!Directory.Exists(carData))
            Directory.CreateDirectory(carData);

        return Path.Combine(carData, "carspawner_" + CarId + ".csv");
    }

    protected override string Headers()
    {
        return "Timestamp,DateTime,Speed,LocationX,LocationY,LocationZ,Model";
    }

    protected override string WriteString()
    {
        return string.Format("\n{0},{1},{2},{3},{4},{5},{6}",
                Utils.MiliTime(), DateTime.Now.ToString("yyyyMMddTHHmmssffffff"), velocity, position, positiony, positionz, Model);
    }

    #endregion
}
