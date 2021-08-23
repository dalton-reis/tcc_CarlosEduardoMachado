using Mirror;
using UnityEngine;

public class Client : MonoBehaviour {

    public NetworkManager manager;
    GameController gameController;
    public GameObject ParedeJogo, ParedeDesenvolvimento;

    void Awake () {
        manager = GetComponent<NetworkManager> ();
        if (!gameController) {
            gameController = GameObject.FindObjectOfType<GameController> ();
        }
    }

    // Start is called before the first frame update
    void Start () {

        if (gameController.multi) {
            manager.networkAddress = gameController.server;
            manager.StartClient ();
            if (gameController.CameraDesenvolvimento) {
                ParedeJogo.SetActive (false);
                ParedeDesenvolvimento.SetActive (true);
            } else {
                ParedeJogo.SetActive (true);
                ParedeDesenvolvimento.SetActive (false);
            }
        }
    }
    void Update () { }
}