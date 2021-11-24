using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VirtualAquarium
{
    public class EggFish : MonoBehaviour
    {

        public EggState state = EggState.Layed;
        public float timeSinceFertilized = 0;
        public float timeSinceLayed = 0;
        public Fish mother, father, prefab;
        public FishArea fishArea;

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
                    rigidbody.angularVelocity = Vector3.zero;
                    GetComponent<SphereCollider>().isTrigger = false;
                    Debug.Log("Entrou terreno");
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
                egg.prefab = fish.prefab.GetComponent<Fish>();
                egg.fishArea = fish.fishArea;
                return true;
            }
            return false;
        }

        public bool fertilize(Fish fish)
        {
            if (state == EggState.Layed && fish.gender == Gender.male && fish.specie == prefab.specie)
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
                    GameObject newFishObj = fishArea.SpawnFish(prefab.gameObject, Gender.random);
                    Fish newFish = newFishObj.GetComponent<Fish>();
                    newFish.Initialize(fishArea, true);
                    fishArea.fishes.Add(newFish);

                    Destroy(gameObject, 1);
                    state = EggState.Born;
                }
            }
            else if (state == EggState.Layed)
            {
                timeSinceLayed += Time.deltaTime / AquariumProperties.timeSpeedMultiplier;
                if (timeSinceLayed > 100)
                {
                    Destroy(gameObject, 1);
                }
            }

        }
    }
}