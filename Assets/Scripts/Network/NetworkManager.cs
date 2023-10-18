using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.XR;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    private GameObject localPlayer;
    private Room currentRoom;

    // Start is called before the first frame update
    void Start()
    {
        try{
            Debug.Log("Server: Connecting to Server");
            ConnectToServer();
        }
        catch{
            Debug.Log("Server: Was not able to connect to server");
        }
    }

    void ConnectToServer(){
        PhotonNetwork.ConnectUsingSettings();
        Debug.Log("Server: Trying to Server");
    }

    public override void OnConnectedToMaster(){

        Debug.Log(
        "Server: Connected to the server"
        );
        base.OnConnectedToMaster();
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = 10;
        roomOptions.IsVisible = true;
        roomOptions.IsOpen = true;
        PhotonNetwork.JoinOrCreateRoom("Room 1", roomOptions , TypedLobby.Default);
    }

    public override void OnJoinedRoom(){
        Debug.Log("Server: Player Joining Room");
        base.OnJoinedRoom();

        /*
        GameObject playerObject = GameObject.Find("Player");
        Vector3 position = playerObject.transform.position;
        position.z = position.z;
        position.x = position.x;
        Vector3 temp = new Vector3((float)415.0, (float)407.2, (float)450.0);*/
        //Vector3 temp = new Vector3((float)389.87, (float)406.02, (float)448.85);
        Vector3 randVec = new Vector3(Random.Range(0,1.5f),0,Random.Range(-0.1f,0.1f));
        Vector3 pedestrianSpawn = new Vector3((float)478, (float)407, (float)448.85);
        Vector3 bikeSpawn = new Vector3((float)478.75, (float)407, (float)448.85);
        Debug.Log("New Player Type: " + StateSettingController.playerType);

        Color playerColor = getPlayerColor(StateSettingController.playerColor);

        // setup PUN2 Hashtable for custom properties
        ExitGames.Client.Photon.Hashtable colorProperties = new ExitGames.Client.Photon.Hashtable() {
            ["color"] = (int)StateSettingController.playerColor,
            ["type"] = (int)StateSettingController.playerType
        };

        // set this local players properties
        PhotonNetwork.SetPlayerCustomProperties(colorProperties);

        switch (StateSettingController.playerType) {
            case StateSettingController.PlayerType.Pedestrian : // VR Pedestrian
                XRSettings.enabled=true;
                Debug.Log("Spawn VR Pedestrian");
                localPlayer = PhotonNetwork.Instantiate("NetworkPlayerTest" , pedestrianSpawn+randVec, transform.rotation);
                localPlayer.transform.Find("Colliders").GetChild(0).GetChild(0).GetComponent<Renderer>().material.color = playerColor;
                break;
            case StateSettingController.PlayerType.Pedestrian_flat : // Flatscreen Pedestrian
                XRSettings.enabled=true;
                Debug.Log("Spawn Flatscreen Pedestrian");
                // localPlayer = PhotonNetwork.Instantiate("PedestrianPlayerFlat" , pedestrianSpawn+randVec, transform.rotation);
                localPlayer = PhotonNetwork.Instantiate("-----SimpleCar", new Vector3(550.7f, 405.95f, 455f), transform.rotation);
                localPlayer.transform.Rotate(0, -90, 0);
                localPlayer.transform.Rotate(0, 180, 0);
                

                XRSettings.enabled=true;
                Debug.Log("Spawn VR Pedestrian");
                localPlayer.transform.Find("Colliders").GetChild(0).GetChild(0).GetComponent<Renderer>().material.color = playerColor;
                //break;
                // localPlayer.GetComponent<Renderer>().material.color = playerColor;
                break;


                
            case StateSettingController.PlayerType.Bicyclist:
                XRSettings.enabled=true;
                // instantiate bike here
                Debug.Log("Spawn Bicycle");
                localPlayer = PhotonNetwork.Instantiate("-----SimpleBike" , bikeSpawn+randVec, transform.rotation);
                break;
        }

        // store player gameObject as this players character in Player.TagObject
        PhotonNetwork.LocalPlayer.TagObject = localPlayer;
    }

    public override void OnLeftRoom(){
        Debug.Log("Servier: Player Leaving Room");

        base.OnLeftRoom();
        PhotonNetwork.Destroy(localPlayer);
    }

    // Called when remote player enters room
    public override void OnPlayerEnteredRoom(Player player){
        // doc for Player object: https://doc-api.photonengine.com/en/pun/v2/class_photon_1_1_realtime_1_1_player.html
        // this is where prob have to change network player color
        Debug.Log("A new player joined the room");
        base.OnPlayerEnteredRoom(player);

        // get color of this network player based on its custom properties
        StateSettingController.PlayerColor remotePlayerColor = (StateSettingController.PlayerColor)player.CustomProperties["color"];

        // get player type based on its custom properties
        StateSettingController.PlayerType remotePlayerType = (StateSettingController.PlayerType)player.CustomProperties["type"];

        // get the gameObject that represents this remote player
        GameObject remotePlayer = (GameObject) player.TagObject;

        Color playerColor = getPlayerColor(remotePlayerColor);

        switch (remotePlayerType) {
            case StateSettingController.PlayerType.Pedestrian:
                remotePlayer.transform.Find("Colliders").GetChild(0).GetChild(0).GetComponent<Renderer>().material.color = playerColor;
                break;
            case StateSettingController.PlayerType.Pedestrian_flat:
                remotePlayer.GetComponent<Renderer>().material.color = playerColor;
                break;       
        }
    }

    // Utility function to get player color
    private Color getPlayerColor(StateSettingController.PlayerColor color) {
        Color playerColor = new Color (1f, 0f, 0f, 1f);
        switch(color) {
            case StateSettingController.PlayerColor.blue :
                playerColor = new Color (0f, 0f, 1f, 1f);
                break;
            case StateSettingController.PlayerColor.green:
                playerColor = new Color (0f, 1f, 0f, 1f);
                break;
            case StateSettingController.PlayerColor.yellow:
                playerColor = new Color (1f, 1f, 0f, 1f);
                break;
            default:
                playerColor = new Color (1f, 0f, 0f, 1f);
                break;
        }

        return playerColor;
    }

}