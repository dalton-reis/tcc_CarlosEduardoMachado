using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EggFish : MonoBehaviour
{

    public EggState state = EggState.Layed;
    public float timeSinceFertilized = 0;
    public float timeSinceLayed = 0;
    public Fish mother, father;

    GameController gameController;

    new private Rigidbody rigidbody;
    public void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.tag == "Terrain")
        {
            if (rigidbody)
            {
                rigidbody.useGravity = false;
                rigidbody.velocity = Vector3.zero;
                GetComponent<SphereCollider>().isTrigger = false;
            }
        }
    }
    public enum EggState
    {
        Layed,
        Fertilized,
        Died,
        Born
    }
    // Start is called before the first frame update
    void Start()
    {
        gameController = GameObject.FindObjectOfType<GameController>();
        tag = "Egg";
        rigidbody = GetComponentInChildren<Rigidbody>();
       // rigidbody.AddForce(0, -1, 0);
    }


    public static bool layEgg(Fish fish)
    {
        if (fish.gender == Gender.female)
        {
            EggFish egg = Instantiate(fish.fishArea.eggFish, fish.transform.position, fish.transform.rotation, fish.fishArea.transform);
            egg.mother = fish;
            return true;
        }
        return false;
    }

    public bool fertilize(Fish fish)
    {
        if (state == EggState.Layed && fish.gender == Gender.male)
        {
            state = EggState.Fertilized;
            father = fish;
            return true;
        }
        return false;
    }

    // Update is called once per frame
    void Update()
    {
        if (state == EggState.Fertilized)
        {
            timeSinceFertilized += Time.deltaTime / AquariumProperties.timeSpeedMultiplier;

            if (timeSinceFertilized > 100)
            {
                mother.AddReward(1f / mother.MaxStep);
                father.AddReward(1f / mother.MaxStep);
                
                Fish newFish = Instantiate(mother, transform.position, transform.rotation, mother.fishArea.transform);
                newFish.Initialize(mother.fishArea, true);

                Destroy(gameObject, 1);
                state = EggState.Born;
            }
        } else if (state == EggState.Layed)
        {
            timeSinceLayed += Time.deltaTime / AquariumProperties.timeSpeedMultiplier;
            if (timeSinceLayed > 100)
            {
                mother.AddReward(-1f / mother.MaxStep);
                Destroy(gameObject, 1);
            }
        }
        
    }
}
