using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Photon.Pun;


public class NetworkPlayer : MonoBehaviour
{

    public Transform position;
    private PhotonView photonView;
    private GameObject player;
    public Transform leftHand;
    public Transform rightHand;
    public Camera playerCamera;
    public Vector3 tempHandRightPosition;
    public static SteamVR_TrackedObject.EIndex[] indexList = new SteamVR_TrackedObject.EIndex[2];

    public SteamVR_Input_Sources rightHandSource;
    private const float _mMoveSpeed = 2.5f;

    public GameObject VRMesh;

    void Start() {
        photonView = GetComponent<PhotonView>();
        player = position.gameObject;
        indexList[0] = SteamVR_TrackedObject.EIndex.None;
        indexList[1] = SteamVR_TrackedObject.EIndex.None;

        /* If the player is not us, but rather is the networked player, then do everything in this if statement

            The following if-block disables controller scripts that would allow the Network Player to control 
            said objects.
        */
        if (!photonView.IsMine) {
            // disable SteamVR Behavior Pose
            leftHand.gameObject.GetComponent<SteamVR_Behaviour_Pose>().enabled = false;
            // disable SteamVR Render Model
            leftHand.GetChild(0).gameObject.SetActive(false);
            // enable Controller Model
            leftHand.GetChild(1).gameObject.SetActive(true);
            // disable SteamVR Behavior Pose
            rightHand.gameObject.GetComponent<SteamVR_Behaviour_Pose>().enabled = false;
            // disable SteamVR Render Model
            rightHand.GetChild(0).gameObject.SetActive(false);
            // enable Controller Model
            rightHand.GetChild(1).gameObject.SetActive(true);

            player.GetComponent<AudioListener>().enabled = false;
            player.transform.GetChild(0).GetComponent<SteamVR_PlayArea>().enabled = false;
            // playerCamera.gameObject.transform.GetChild(0).GetComponent<SteamVR_TrackedObject>().enabled = false;
            playerCamera.enabled = false;
            // Enables sphere Mesh Renderer for ONLY the network player   
            VRMesh.SetActive(true);
        } 

    }

    // Update is called once per frame
    void Update()
    {
        /*if (photonView.IsMine)
        {
            //MapPosition(playerCamera.gameObject.transform, "Camera");
            //MapPosition(rightHand, "Controller (left)");
            //MapPosition(leftHand, "Controller (right)");

        }*/
        if (photonView.IsMine)
        {
            // ***  This is Controlled Player   ***
            if (indexList[0] == SteamVR_TrackedObject.EIndex.None) {
                // grab the controller index number for the player and save it
                indexList[0] = leftHand.GetChild(0).gameObject.GetComponent<SteamVR_RenderModel>().index;
                indexList[1] = rightHand.GetChild(0).gameObject.GetComponent<SteamVR_RenderModel>().index;
            }

            // TODO: character has no collider, will go through ground/walls. -- NEED TO ADD BODY to character w/collider and check for collisions
            
            // grab the "TrackpadPosition" action (currently mapped to the Joystick)
            //SteamVR_Action_Vector2 action = SteamVR_Input.GetVector2ActionFromPath("/actions/default/in/TrackpadPosition");
            SteamVR_Action_Vector2 action_joy_right = SteamVR_Input.GetVector2ActionFromPath("/actions/default/in/TrackpadRight");
            SteamVR_Action_Vector2 action_joy_left = SteamVR_Input.GetVector2ActionFromPath("/actions/default/in/TrackpadLeft");

            // if action exceeds threshold - right stick movement
            if (action_joy_left.axis.magnitude > 0.1) {
                // get player orientation (current rotation) from playerCamera
                Quaternion orientation = playerCamera.transform.rotation;

                Debug.Log("Right Clicked");

                // calculate move direction using player orientation
                Vector3 moveDirection = orientation * Vector3.forward * action_joy_left.axis.y + orientation * Vector3.right * action_joy_left.axis.x;
                Vector3 pos = transform.position;
                pos.x += moveDirection.x * _mMoveSpeed * Time.deltaTime;
                pos.z += moveDirection.z * _mMoveSpeed * Time.deltaTime;
                transform.position = pos;
                //Debug.Log("move: " + action.axis.y);
            }
            // if left joy exceeds threshold - left stick rotation
            if (Mathf.Abs(action_joy_right.axis.x) > 0.1) {
                float rotationSpeed = 40f;

                transform.Rotate(-Vector3.up * rotationSpeed * -action_joy_right.axis.x * Time.deltaTime);
            }

            //Debug.Log("VR Action: " + action.delta.x + " " + action.delta.y);
        } /*else {
            // if we are the network player
            if ( (leftHand.GetChild(0).GetComponent<SteamVR_RenderModel>().index == SteamVR_TrackedObject.EIndex.None) &&
                 (indexList[0] != SteamVR_TrackedObject.EIndex.None))
            {
                // copy saved player indexes to the network player
                leftHand.GetChild(0).GetComponent<SteamVR_RenderModel>().index = indexList[0];
                rightHand.GetChild(0).GetComponent<SteamVR_RenderModel>().index = indexList[1];
                // update the controller mesh models
                leftHand.GetChild(0).GetComponent<SteamVR_RenderModel>().UpdateModel();
                rightHand.GetChild(0).GetComponent<SteamVR_RenderModel>().UpdateModel();
            }
        }*/
    }


    public void MapPosition(Transform target, string hand)
    {
        //player = GameObject.Find("Player").gameObject.transform.Find("[CameraRig]").gameObject;



        foreach (Transform child in player.transform.GetChild(0))
        {
            Debug.Log(child.name + child.position);



            if (hand == child.name)
            {
                if (child.name == "Controller (right)")
                {
                    Vector3 wPos = transform.TransformPoint(child.position.x, child.position.y, child.position.z);
                    //target.position = wPos - (GameObject.Find("Player").gameObject.transform.position + player.transform.Find("Camera").gameObject.transform.position - GameObject.Find("Player").gameObject.transform.position + new Vector3(0.12f, -0.3f, +0.3f));
                    target.position = wPos - (player.transform.position + playerCamera.gameObject.transform.position - player.transform.position + new Vector3(0.12f, -0.3f, +0.3f));
                    /*                    target.rotation = child.rotation;
                    */
                }
                else if (child.name == "Controller (left)")
                {
                    Vector3 wPos = transform.TransformPoint(child.position.x, child.position.y, child.position.z);
                    //target.position = wPos - (GameObject.Find("Player").gameObject.transform.position + player.transform.Find("Camera").gameObject.transform.position - GameObject.Find("Player").gameObject.transform.position + new Vector3(-0.15f, -0.3f, +0.3f));
                    target.position = wPos - (player.transform.position + playerCamera.gameObject.transform.position - player.transform.position + new Vector3(-0.15f, -0.3f, +0.3f));
                    /*                    target.rotation = child.rotation;
                    */
                }
                else
                {
                    //target.position = GameObject.Find("Player").gameObject.transform.position + player.transform.Find("Camera").gameObject.transform.position - GameObject.Find("Player").gameObject.transform.position + new Vector3(0f, 0f, 0.3f);
                    target.position = player.transform.position + playerCamera.gameObject.transform.position - player.transform.position + new Vector3(0f, 0f, 0.3f);
                }
            }

        }

    }
}


/*
public Transform position;
private PhotonView photonView;
private GameObject player;
public Transform leftHand;
public Transform rightHand;


// Start is called before the first frame update
void Start()
{
    photonView = GetComponent<PhotonView>();
}
// Update is called once per frame
void Update()
{
    if (photonView.IsMine)
    {
        MapPosition(position.gameObject.transform, "Camera");
        MapPosition(rightHand.gameObject.transform, "Controller (left)");
        MapPosition(leftHand.gameObject.transform, "Controller (right)");

    }
}


public void MapPosition(Transform target, string hand)
{
    player = GameObject.Find("Player").gameObject.transform.Find("[CameraRig]").gameObject;

    foreach (Transform child in player.transform)
    {
        Debug.Log("Child" + child.name);

        if (hand == child.name)
        {
            if(child.name != "Camera")
            {
                target.position = child.position;
                target.rotation = child.rotation;

            }

             else
            {
                target.position = new Vector3(child.position.x, 407.35f, child.position.z);
            }
        }

    }

}
*/

