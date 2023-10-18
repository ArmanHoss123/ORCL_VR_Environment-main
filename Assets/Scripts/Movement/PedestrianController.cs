using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;
using System.IO;
using System.Threading;
public class PedestrianController : Controller
{
    public float sprintSpeed;
    public bool canFly;
    public bool disablePhone;
    public Transform cameraTransform;
    public GameObject[] controllerObjects;
    public GameObject handWithPhone;
    public bool activateCrosswalks;
    [ShowOnVariable("activateCrosswalks", 1)]
    public float rangeCanActivateCrosswalk = 5f;
    [ShowOnVariable("activateCrosswalks", 1)]
    public float crosswayActiveTime = 1f;

    public SteamVR_Action_Vector2 moveAction = SteamVR_Input.GetAction<SteamVR_Action_Vector2>("platformer", "Move");
    public SteamVR_Action_Boolean moveClick = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("platformer", "Sprint");
    public SteamVR_Action_Boolean sprintAction = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("platformer", "Sprint");
    public SteamVR_Action_Boolean phoneTap = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("platformer", "MoveClick");

    public SteamVR_Input_Sources inputSource = SteamVR_Input_Sources.Any;

    Vector2 movement;
    Animator screenAnimator;
    Crossway currentCrossway;
    string curTrigger;
    bool inActivation;
    float velocity;
    Vector3 location;

    protected override void Start()
    {
        base.Start();
        ExportData();
        StartCoroutine(WaitThenHideHands());
    }

    protected override void Update()
    {
        bool sprint = sprintAction.GetState(SteamVR_Input_Sources.LeftHand) && sprintAction.GetState(SteamVR_Input_Sources.RightHand);
        movement = moveAction.GetAxis(inputSource);

        if (sprint)
            speed = sprintSpeed;
        else
            speed = startSpeed;
        if (moveClick.GetState(inputSource))
        {
            Transform transformUse = cameraTransform ? cameraTransform : initialTransform;
            directionMove = cameraTransform.TransformDirection(new Vector3(movement.x, 0f, movement.y));
            //stay grounded
            if(!canFly)
                directionMove.y = 0f;
        }
        else
        {
            UpdateKeyboardMovement();
        }

        if(activateCrosswalks && !inActivation && !disablePhone)
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, rangeCanActivateCrosswalk, 1 << 13, QueryTriggerInteraction.Collide);
            float minDistance = Mathf.Infinity;
            currentCrossway = null;
            foreach (Collider col in colliders)
            {
                Crossway crossway = col.GetComponentInParent<Crossway>();
                if(crossway)
                {
                    float distance = Vector3.Distance(col.transform.position, transform.position);
                    if(distance < minDistance)
                    {
                        minDistance = distance;
                        currentCrossway = crossway;
                    }
                }
            }
         
            if(currentCrossway)
            {
                curTrigger = "Ready";
                if(phoneTap.GetStateDown(SteamVR_Input_Sources.RightHand))
                {
                    StartCoroutine(ActivateCrosswalk(currentCrossway));
                }
            }
            else
            {
                curTrigger = "Idle";
            }

            if (!inActivation && screenAnimator)
                screenAnimator.SetTrigger(curTrigger);
        }

        velocity = rigidbody.velocity.magnitude * 2.23693629f;
        location = transform.position;

        base.Update();
    }

    IEnumerator ActivateCrosswalk(Crossway crosswalk)
    {
        crosswalk.CrosswayActive = true;
        screenAnimator.SetTrigger("Activated");
        inActivation = true;
        yield return new WaitForSeconds(crosswayActiveTime);
        crosswalk.CrosswayActive = false;
        inActivation = false;
        screenAnimator.SetTrigger("Idle");
    }

    protected void UpdateKeyboardMovement()
    {
        float forward = 0.0f;
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
        {
            forward += 1.0f;
        }
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
        {
            forward -= 1.0f;
        }

        float right = 0.0f;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            right += 1.0f;
        }
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            right -= 1.0f;
        }

        if(Input.GetKey(KeyCode.LeftShift))
        {
            speed = sprintSpeed;
        }

        directionMove = new Vector3(forward, 0.0f, right * -1f);
    }

    protected override void CollidedWith(Controller otherController)
    {
        if (otherController.GetType() == typeof(CarController))
        {
            RestartDemo.RestartAndEnable();
        }
    }

    IEnumerator WaitThenHideHands()
    {
        for (int i = 0; i < 10; i++)
            yield return new WaitForEndOfFrame();
        bool controllerSet = false;
        while (!controllerSet)
        {
            foreach (GameObject controller in controllerObjects)
            {
                controllerSet = SetControllerVisible(controller, false);
            }
            yield return new WaitForEndOfFrame();
        }

        FindHandWithPhone(handWithPhone);
        if (disablePhone && screenAnimator)
            screenAnimator.transform.parent.gameObject.SetActive(false);
    }

    void FindHandWithPhone(GameObject curHand)
    {
        if(curHand.name == "Iphone Version Test")
        {
            screenAnimator = curHand.GetComponentInChildren<Animator>();
            return;
        }

        foreach (Transform t in curHand.transform)
            FindHandWithPhone(t.gameObject);
    }

    bool SetControllerVisible(GameObject controller, bool visible)
    {
        if(controller.name.Contains("controller"))
        {
            controller.SetActive(visible);
            return true;
        }

        foreach (Transform t in controller.transform)
        {
            if (SetControllerVisible(t.gameObject, visible))
                return true;
        }
        return false;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
    }

    #region Data Export
    protected override string FileName()
    {
        return "pedestrian.csv";
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
}
