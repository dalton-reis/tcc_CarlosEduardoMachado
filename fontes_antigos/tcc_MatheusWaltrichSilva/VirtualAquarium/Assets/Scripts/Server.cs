using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Mirror;
using Mirror.Discovery;
using UnityEngine;

public class Server : MonoBehaviour {
    NetworkManager manager;
    public GameObject ParedeJogo, ParedeDesenvolvimento;
    public List<Camera> cameras;
    internal GameController gameController;

    void Awake () {
        manager = GetComponent<NetworkManager> ();
    }

    // Start is called before the first frame update
    void Start () {
        if (!gameController) {
            gameController = GameObject.FindObjectOfType<GameController> ();
        }
        if (gameController.multi) {
            manager.StartHost ();
            if (gameController.CameraDesenvolvimento) {
                ParedeJogo.SetActive (false);
                ParedeDesenvolvimento.SetActive (true);
            } else {
                ParedeJogo.SetActive (true);
                ParedeDesenvolvimento.SetActive (false);
            }
        }
    }

    // Update is called once per frame
    void Update () {
        cameras = new List<Camera> (GameObject.FindObjectsOfType<Camera> ());
        for (int i = 0; i < cameras.Count; i++) {
            if (cameras[i].name != "Main Camera") {
                cameras[i].enabled = false;
            }
        }
    }
}