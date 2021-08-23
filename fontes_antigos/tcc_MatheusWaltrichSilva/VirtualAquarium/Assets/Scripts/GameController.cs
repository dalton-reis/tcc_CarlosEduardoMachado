using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public bool multi, iot, CameraDesenvolvimento;
    public string server;

    public static GameController gameController;
    void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

    void Awake() {
        if (gameController == null) {
            gameController = this;
        }
        else if (gameController != this) {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(gameObject);    
    }
    
}
