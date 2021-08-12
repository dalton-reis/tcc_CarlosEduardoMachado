using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleFood : MonoBehaviour {

    public ParticleSystem part;
    List<ParticleSystem.Particle> enter = new List<ParticleSystem.Particle>();
    public FoodPoint foodPoint;
    public GameObject sphereFood;

    // Use this for initialization
    void Start () {
        part = GetComponent<ParticleSystem>();
    }

    void OnParticleTrigger()
    {
        int numEnter = part.GetTriggerParticles(ParticleSystemTriggerEventType.Enter, enter);        
        for (int i = 0; i < numEnter; i++)
        {            
            ParticleSystem.Particle p = enter[i];            
            Debug.Log("P Position: x: " + p.position.x + "; y: " + p.position.x + "; z: " + p.position.z);
            GameObject sphere = Instantiate(sphereFood, gameObject.transform.TransformPoint(p.position), Quaternion.identity);
            sphere.tag = "Food";
            foodPoint.foods.Add(sphere);
            Debug.Log("Sphere Position: x: " + sphere.transform.position.x + "; y: " + sphere.transform.position.x + "; z: " + sphere.transform.position.z);
            foodPoint.addFood();            
            enter[i] = p;            
        }
        part.SetTriggerParticles(ParticleSystemTriggerEventType.Enter, enter);
    }
}
