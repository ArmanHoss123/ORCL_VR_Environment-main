using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;
using Valve.VR;
using System.Linq;
#if UNITY_EDITOR
    using UnityEditor;
    using System.IO;
    using System.Text.RegularExpressions;
#endif
[ExecuteInEditMode]
public class BicycleController : Controller
{
    public bool useKeyboard = true;
    [ShowOnVariable("useKeyboard", 1)]
    public float turnAmountOnKeyboard = 0.5f;
    [ShowOnVariable("useKeyboard", 1)]
    public float speedIncrement = 1f;
    [ShowOnVariable("useKeyboard", 1)]
    public Transform cameraTransform;
    [ShowOnVariable("useKeyboard", 1)]
    public Vector3 cameraOffset;
    [ShowOnVariable("useKeyboard", 1)]
    public Vector3 maxRotation = Mathf.Infinity * Vector3.one;

    [Header("Speed & Brake")]
    public float speedMultiplier = 0.5f;
    public float maxAcceleration = 2f;
    public float largestBrakeSpeed = 2f;

    [Header("Turning")]
    public float turnMultiplier = 1f;
    public float turnOffsetPositiveSlope = 0.05f;
    public float turnOffsetNegativeSlope = 0.05f;
    public bool multiplyBySpeed = true;

    [Header("Slope")]
    public bool useCurrentSpeed;
    public float frontDistanceCheck = 2f;
    public float frontDistanceOffset = 6f;
    public float bikeSlopeElevationMultiplier = 1f;
    public float bikeSlopeResistanceMultiplier = 1f;

    [Header("Set By Default")]
    public FitnessEquipmentDisplay fitnessEquipment;
    public WheelCollider turnWheel;
    public WheelCollider backWheel;
    public bool hideHands = true;
    public SteamVR_Action_Single brakeAction = SteamVR_Input.GetAction<SteamVR_Action_Single>("default", "squeeze");


    [Header("Average Speed")]
    public TextAsset averageSpeedFile;
    [EditorButton("Clear File", "ClearTextFile")]
    public bool clear;

    [Header("Calibration")]
    public Transform calibrateHandTransform;
    public float lowestTurnPosition;
    public float middleTurnPosition;
    public float highestTurnPosition;
    [EditorButton("Calibrate Lowest Position", "CalibrateLowest")]
    public bool calLow;
    [EditorButton("Calibrate Middle Position", "CalibrateMiddle")]
    public bool calMid;
    [EditorButton("Calibrate Highest Position", "CalibrateHighest")]
    public bool calHigh;


    private float turnAmount;
    private Vector3 startEulerAngles;
    private Vector3 startMousePosition;
    private float deceleration;
    private float originalSpeed;
    private float lastSpeed;
    private float averageSpeed;
    private int averageTimesChanged;
    private float slopeAngleElevation;
    private float slopeAngleDrift;

    // GameObject References
    private GameObject steamVRObjects;
    private GameObject bikeMesh;
    private Camera vrCamera;

    float velocity;
    Vector3 location;


    // Start is called before the first frame update
    protected override void Start()
    {
        if (Application.isPlaying)
        {
            base.Start();
            startMousePosition = Input.mousePosition;

            Transform transformUse = cameraTransform ? cameraTransform : transform;
            startEulerAngles = transformUse.localEulerAngles;
            if (hideHands)
            {
                StartCoroutine(HideHandsAfterWaiting());
            }
            originalSpeed = speed;
        }
        steamVRObjects = GameObject.Find("SteamVRObjects");
        bikeMesh = GameObject.Find("Cykel1");
        vrCamera = GameObject.Find("VRCamera").GetComponent<Camera>();
    }

    IEnumerator HideHandsAfterWaiting() {
        for(int i = 0; i < 10; i ++)
            yield return new WaitForEndOfFrame();
        HideHands();
    }

    public void CalibrateLowest()
    {
        lowestTurnPosition = calibrateHandTransform.localPosition.z;
        PlayerPrefs.SetFloat("Lowest Turn Position", lowestTurnPosition);
        PlayerPrefs.Save();
    }

    public void CalibrateMiddle()
    {
        middleTurnPosition = calibrateHandTransform.localPosition.z;
        PlayerPrefs.SetFloat("Middle Turn Position", middleTurnPosition);
        PlayerPrefs.Save();
    }

    public void CalibrateHighest()
    {
        highestTurnPosition = calibrateHandTransform.localPosition.z;
        PlayerPrefs.SetFloat("Highest Turn Position", highestTurnPosition);
        PlayerPrefs.Save();
    }

    public void SetElevationSlope(float slope)
    {
        slopeAngleElevation = slope;
        if(fitnessEquipment && !useKeyboard)
            fitnessEquipment.SetTrainerSlope((int)(slope * bikeSlopeElevationMultiplier));
    }

    public void SetDriftSlope(float slope)
    {
        slopeAngleDrift = slope;
    }


    // Update is called once per frame
    protected override void Update()
    {
        if (Application.isPlaying)
        {
            CheckForKeyboardInput();
            CheckForFitnessEquipment();
            CheckForBrake();
            CalculateSlope();
            CheckForTurn();
            UpdateAverageSpeed();
            if(turnWheel)
            {
                turnWheel.steerAngle = turnAmount;
                turnWheel.motorTorque = speed;
            }
            else
                base.Update();
            velocity = rigidbody.velocity.magnitude * 2.23693629f;
            location = transform.position;
        }
#if UNITY_EDITOR
        CheckForSavedVariables();
#endif
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        PlayerPrefs.SetFloat("Average Speed", averageSpeed);
        PlayerPrefs.Save();
    }

    void CalculateSlope()
    {
        RaycastHit hit;
        float velocity = speed;
        Debug.DrawRay(transform.position + transform.forward * (useCurrentSpeed ? velocity * frontDistanceCheck : frontDistanceCheck), Vector3.down);
        if (Physics.Raycast(transform.position + transform.forward * (useCurrentSpeed ? velocity * frontDistanceCheck : frontDistanceCheck), Vector3.down, out hit))
        {
            slopeAngleElevation = Vector3.Angle(hit.normal, transform.forward) - 90f;
            if(!useKeyboard && fitnessEquipment)
            {
                fitnessEquipment.SetTrainerSlope((int)(slopeAngleElevation * bikeSlopeElevationMultiplier));
                fitnessEquipment.SetTrainerResistance((int)(slopeAngleElevation * bikeSlopeResistanceMultiplier));
            }
        }

        if (Physics.Raycast(transform.position + transform.forward * frontDistanceOffset, Vector3.down, out hit))
        {
            slopeAngleDrift = Vector3.Angle(hit.normal, transform.forward) - 90f;
        }
    }

    void UpdateAverageSpeed()
    {
        if(lastSpeed != speed)
        {
            UpdateCumulativeMovingAverageSpeed(speed);
        }
    }

    void UpdateCumulativeMovingAverageSpeed(float newSpeed)
    {
        averageTimesChanged++;
        averageSpeed += (newSpeed - averageSpeed) / averageTimesChanged;
    }

    void CheckForBrake()
    {
        if(deceleration <= 0f)
        {
            speed = originalSpeed;
        }

        float brakePercentage = useKeyboard ? (IsDownPressed() ? largestBrakeSpeed : 0f) : brakeAction.GetAxis(SteamVR_Input_Sources.Any);       
        float currentDeceleration = Mathf.Lerp(0f, largestBrakeSpeed, brakePercentage);
        if(currentDeceleration < deceleration)
        {
            deceleration -= (useKeyboard ? maxAcceleration : Mathf.Min(maxAcceleration, originalSpeed)) * Time.deltaTime;
            if (deceleration < currentDeceleration)
                deceleration = currentDeceleration;
            speed = originalSpeed - deceleration;
        }
        else
        {
            deceleration = currentDeceleration;
            speed -= deceleration * Time.deltaTime;
        }
        
        if (speed < 0f)
            speed = 0f;
    }


    void CheckForTurn()
    {
        if (lowestTurnPosition != highestTurnPosition && calibrateHandTransform && !useKeyboard)
        {
            float offset = slopeAngleElevation < 0f ? turnOffsetNegativeSlope : turnOffsetPositiveSlope;
            float position = calibrateHandTransform.localPosition.z + offset * slopeAngleElevation;
            float dir = (position - middleTurnPosition) * turnMultiplier;
            if (multiplyBySpeed)
                dir *= speed;
            float amount = dir > 0f ? Mathf.InverseLerp(lowestTurnPosition, highestTurnPosition, position) : Mathf.InverseLerp(highestTurnPosition, lowestTurnPosition, position);
            Turn(dir, amount);
        }
    }

    void CheckForFitnessEquipment()
    {
        if(!useKeyboard && fitnessEquipment)
        {
            originalSpeed = fitnessEquipment.speed * speedMultiplier;
        }
    }

    void CheckForKeyboardInput()
    {
        if (useKeyboard)
        {
            if (IsUpPressed())
            {
                speed = speedIncrement;
            }
          
            if (IsLeftPressed())
            {
                turnAmount += turnAmountOnKeyboard * Time.deltaTime * -1f;
            }
            
            if (IsRightPressed())
            {
                turnAmount += turnAmountOnKeyboard * Time.deltaTime;
            }
            
            if(Mathf.Abs(turnAmount) > turnAmountOnKeyboard)
                turnAmount = turnAmountOnKeyboard * Mathf.Sign(turnAmount);
            originalSpeed = speed;
        }
        else if (vrCamera.enabled) 
        {
            if (steamVRObjects) {
                // in VR mode, use arrow keys to adjust VR position
                if (IsUpJustPressed()) {
                    Vector3 tempVec = new Vector3(0,0,0.025f);
                    steamVRObjects.transform.position += tempVec;
                }
                if (IsDownJustPressed()) {
                    Vector3 tempVec = new Vector3(0,0,-0.025f);
                    steamVRObjects.transform.position += tempVec;
                }
                if (IsLeftJustPressed()) {
                    Vector3 tempVec = new Vector3(-0.025f,0,0);
                    steamVRObjects.transform.position += tempVec;
                }
                if (IsRightJustPressed()) {
                    Vector3 tempVec = new Vector3(0.025f,0,0);
                    steamVRObjects.transform.position += tempVec;
                }
                if (IsRaiseJustPressed()) {
                    Vector3 tempVec = new Vector3(0,0.025f,0);
                    steamVRObjects.transform.position += tempVec;
                }
                if (IsLowerJustPressed()) {
                    Vector3 tempVec = new Vector3(0,-0.025f,0);
                    steamVRObjects.transform.position += tempVec;
                }
                if (IsSpaceJustPressed()) {
                    // toggle bike mesh on or off
                    bikeMesh.SetActive(!bikeMesh.activeSelf);
                }
            }
        }
    }

    public void Turn(float direction, float amount)
    {
        directionMove = initialTransform.TransformDirection(new Vector3(direction * amount, 0f, 1f));
        turnAmount = amount * direction;
    }

    protected override void CollidedWith(Controller otherController)
    {
        if(otherController.GetType() == typeof(CarController))
        {
            RestartDemo.RestartAndEnable();
        }
    }

    #region Data Export 
    protected override string FileName()
    {
        return "bicycle.csv";
    }

    protected override string Headers()
    {
        return "Timestamp,Speed,Location";
    }

    protected override string WriteString()
    {
        string locationText = location.x + ";" + location.y + ";" + location.z;

        return string.Format("\n{0},{1},{2}",
            Utils.MiliTime(), velocity, locationText);
    }
    #endregion

#if UNITY_EDITOR
    void CheckForSavedVariables()
    {
        if (Application.isPlaying)
            return;
        bool shouldSave = false;
        if (PlayerPrefs.HasKey("Lowest Turn Position"))
        {
            lowestTurnPosition = PlayerPrefs.GetFloat("Lowest Turn Position");
            PlayerPrefs.DeleteKey("Lowest Turn Position");
            shouldSave = true;
        }

        if (PlayerPrefs.HasKey("Middle Turn Position"))
        {
            middleTurnPosition = PlayerPrefs.GetFloat("Middle Turn Position");
            PlayerPrefs.DeleteKey("Middle Turn Position");
            shouldSave = true;
        }

        if (PlayerPrefs.HasKey("Highest Turn Position"))
        {
            highestTurnPosition = PlayerPrefs.GetFloat("Highest Turn Position");
            PlayerPrefs.DeleteKey("Highest Turn Position");
            shouldSave = true;
        }

        if(PlayerPrefs.HasKey("Average Speed"))
        {
            float speed = PlayerPrefs.GetFloat("Average Speed");
            PlayerPrefs.DeleteKey("Average Speed");
            PlayerPrefs.Save();
            WriteSpeedToTextFile(speed);
            shouldSave = true;
        }
        if (shouldSave)
            PlayerPrefs.Save();
    }

    public void ClearTextFile()
    {
        if (!averageSpeedFile)
            return;
        string path = FilePath();
        File.WriteAllText(path, "");

        EditorUtility.SetDirty(averageSpeedFile);
        //Re-import the file to update the reference in the editor
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
    }

    void WriteSpeedToTextFile(float newSpeed)
    {
        if (!averageSpeedFile)
            return;
        string path = FilePath();
        string[] lines = averageSpeedFile.text.Split(new string[] { "\n" }, System.StringSplitOptions.RemoveEmptyEntries);

        float currentAverage = 0;
        int currentTotal = 0;
        string totalAverageStr = "Total Average Speed: ";

        if (lines.Length > 0 && !string.IsNullOrEmpty(lines[0]))
        {
            string averageStr = lines[lines.Length - 1];
            Debug.Log("Average string is: " + averageStr);
            currentAverage = float.Parse(averageStr.Replace(totalAverageStr, ""));
            currentTotal = lines.Length - 1;
        }

        currentTotal++;
        currentAverage = (currentAverage + newSpeed) / currentTotal;
        if(lines.Length == 0)
        {
            System.Array.Resize(ref lines, 1);
        }

        lines[lines.Length - 1] = "Average Speed Test #" + currentTotal + ": " + newSpeed;
        System.Array.Resize(ref lines, lines.Length + 1);
        lines[lines.Length - 1] = totalAverageStr + currentAverage;

        File.WriteAllText(path, string.Join("\n", lines));

        EditorUtility.SetDirty(averageSpeedFile);
        //Re-import the file to update the reference in the editor
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
    }

    string FilePath()
    {
        string path = Application.dataPath.Replace("Assets", "") + AssetDatabase.GetAssetPath(averageSpeedFile.GetInstanceID());
        return path;
    }
#endif

    void HideHands()
    {
        if (Player.instance == null)
            return;
        for (int handIndex = 0; handIndex < Player.instance.hands.Length; handIndex++)
        {
            var hand = Player.instance.hands[handIndex];
            if (hand != null)
            {
                hand.HideController(true);
            }
        }
    }

    #region Input Check
    bool IsUpPressed() {
        return Input.GetKey( KeyCode.W ) || Input.GetKey( KeyCode.UpArrow );
    }
     
    bool IsDownPressed() {
        return Input.GetKey( KeyCode.S ) || Input.GetKey( KeyCode.DownArrow );
    }

    bool IsRightPressed() {
        return Input.GetKey( KeyCode.D ) || Input.GetKey( KeyCode.RightArrow );
    }

    bool IsLeftPressed() {
         return Input.GetKey( KeyCode.A ) || Input.GetKey( KeyCode.LeftArrow );
    }
    
    bool IsUpJustPressed() {
        return Input.GetKeyDown( KeyCode.W ) || Input.GetKeyDown( KeyCode.UpArrow );
    }
     
    bool IsDownJustPressed() {
        return Input.GetKeyDown( KeyCode.S ) || Input.GetKeyDown( KeyCode.DownArrow );
    }

    bool IsRightJustPressed() {
        return Input.GetKeyDown( KeyCode.D ) || Input.GetKeyDown( KeyCode.RightArrow );
    }

    bool IsLeftJustPressed() {
         return Input.GetKeyDown( KeyCode.A ) || Input.GetKeyDown( KeyCode.LeftArrow );
    }

    bool IsRaiseJustPressed() {
        return Input.GetKeyDown( KeyCode.Q );
    }
    
    bool IsLowerJustPressed() {
        return Input.GetKeyDown( KeyCode.Z );
    }

    bool IsSpaceJustPressed() {
        return Input.GetKeyDown( KeyCode.Space );
    }
    #endregion
}
