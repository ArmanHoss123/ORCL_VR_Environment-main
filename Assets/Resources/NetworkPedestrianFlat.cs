using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;


public class NetworkPedestrianFlat : MonoBehaviour
{

    public Transform position;
    private PhotonView photonView;
    private GameObject player;
    public Camera playerCamera;
    private const float _mMoveSpeed = 2.5f;

    void Start() {
        photonView = GetComponent<PhotonView>();
        player = position.gameObject;

        // If the player is not us, but rather is the networked player
        if (!photonView.IsMine) {
            // disable character controller script
            player.GetComponent<FirstPersonController>().enabled = false;
            // disable audio
            //playerCamera.GetComponent<AudioListener>().enabled = false;
            // disable camera
            //playerCamera.GetComponent<Camera>().enabled = false;
            // disable entire player gameobject (disabled both audio listner and camera)
            playerCamera.gameObject.SetActive(false);
        }

    }
}