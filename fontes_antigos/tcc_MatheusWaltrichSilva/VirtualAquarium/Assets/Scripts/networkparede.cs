using System.Collections;
using System.Collections.Generic;
using Mirror;
using Mirror.Discovery;
using UnityEngine;

public class networkparede : NetworkManager {

    public GameObject parede;
    public GameObject canvas;
    public GameObject player;
    public GameController gameController;

    bool iniciadoParede = false;

    /// <summary>
    /// Start is called on the frame when a script is enabled just before
    /// any of the Update methods is called the first time.
    /// </summary>
    public override void Start () {
        if (!gameController) {
            gameController = GameObject.FindObjectOfType<GameController> ();
        }
    }

    public override void OnServerAddPlayer (NetworkConnection conn) {
        Transform start = base.GetStartPosition ();
        player = Instantiate (playerPrefab, start.position, start.rotation);
        NetworkServer.AddPlayerForConnection (conn, player);

        // spawn parede e Canvas
        if (!iniciadoParede) {
            if (gameController.CameraDesenvolvimento) {
                parede = Instantiate (spawnPrefabs.Find (prefab => prefab.name == "cameraParede"));
                NetworkServer.Spawn (parede);
                canvas = Instantiate (spawnPrefabs.Find (prefab => prefab.name == "Canvas"));
                NetworkServer.Spawn (canvas);
            } else {
                parede = Instantiate (spawnPrefabs.Find (prefab => prefab.name == "cameraParedeJogo"));
                NetworkServer.Spawn (parede);
                canvas = Instantiate (spawnPrefabs.Find (prefab => prefab.name == "Canvas"));
                NetworkServer.Spawn (canvas);
            }

            iniciadoParede = true;
        }
    }

    // Update is called once per frame
    void Update () {

    }
}