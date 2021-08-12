using System.Collections;
using System.Collections.Generic;
using UnityEngine;

enum CurrentAnimalAction
{
    idle,
    walking,
    drinking,
    eating,
    running,
    attacking,
    dead
};

public enum SceneState
{
    Favorable,
    Unfavorable
}

public class SimulationController : MonoBehaviour
{
    private SceneState _currentSceneState;

    // Start is called before the first frame update
    void Start()
    {
        _currentSceneState = SceneState.Favorable;

        InvokeRepeating("VerifyScene", 3, 3.0f);
    }

    void VerifyScene()
    {
        GameObject[] wolfs = GameObject.FindGameObjectsWithTag("Wolf");
        GameObject[] rabbits = GameObject.FindGameObjectsWithTag("Rabbit");
        GameObject[] deer = GameObject.FindGameObjectsWithTag("Deer");

        var animalsList = new List<GameObject>();
        animalsList.AddRange(wolfs);
        animalsList.AddRange(rabbits);
        animalsList.AddRange(deer);

        if (animalsList.Count == 0)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name); ;
        }
    }

    public SceneState GetSceneState()
    {
        return _currentSceneState;
    }
}
