using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Photon.Pun;


public class NetworkBicyclePlayer : MonoBehaviour
{
    public GameObject player;
    public GameObject wheels;
    public GameObject fitnessEquipmentDisplay;
    private PhotonView photonView;

    void Start() {
        photonView = GetComponent<PhotonView>();

        // If the player is not us, but rather is the networked player
        if (!photonView.IsMine) {
            //Destroy(player.transform.parent.GetComponent<Rigidbody>());
            player.transform.parent.GetComponent<Rigidbody>().isKinematic = true;
            player.GetComponentInParent<BicycleController>().enabled = false;
            player.SetActive(false);
            // disable wheel colliders for networked player, will get position from remote
            wheels.SetActive(false);
            fitnessEquipmentDisplay.SetActive(false);
        }
    }
}