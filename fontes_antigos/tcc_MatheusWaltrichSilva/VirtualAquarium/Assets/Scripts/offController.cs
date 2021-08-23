using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class offController : MonoBehaviour
{
    public GameObject Canvas,parede;
    internal GameController gameController;
    // Start is called before the first frame update
    void Start()
    {
        if(!gameController){
            gameController = GameObject.FindObjectOfType<GameController>();
        }
        if(!gameController.multi){
            Canvas.SetActive(true);
            parede.SetActive(true);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
