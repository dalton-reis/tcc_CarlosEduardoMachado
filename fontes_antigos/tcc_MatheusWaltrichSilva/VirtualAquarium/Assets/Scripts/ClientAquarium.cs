using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ClientAquarium : NetworkBehaviour {
    public NetworkManager manager;
    public List<Camera> cameras;

    GameController gameController;

    GameObject parede;

    void Awake () {
        manager = GetComponent<NetworkManager> ();
    }

    // Start is called before the first frame update
    void Start () {
        if (!gameController) {
            gameController = GameObject.FindObjectOfType<GameController> ();
        }
        if (gameController.multi) {
            manager.networkAddress = gameController.server;
            manager.StartClient();
        }
        //manager.playerPrefab.GetComponent<PlayerController>().aquario = true;
    }

    void Update () {
        /*if(!manager.playerPrefab.GetComponent<PlayerController>().aquario){
            manager.playerPrefab.GetComponent<PlayerController>().aquario = true;
        }*/
        cameras = new List<Camera> (GameObject.FindObjectsOfType<Camera> ());
        for (int i = 0; i < cameras.Count; i++) {
            if (cameras[i].name != "Main Camera") {
                cameras[i].enabled = false;
            }
        }
    }
}