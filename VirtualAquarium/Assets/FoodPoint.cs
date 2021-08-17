using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodPoint : MonoBehaviour {

    private int foodCount = 0;
    public List<GameObject> foods;

    // Use this for initialization
    void Start () {
        foods = new List<GameObject>();
    }    

    // Update is called once per frame
    void Update () {
		
	}

    public void addFood()
    {
        this.foodCount++;
    }

    public void removeFood()
    {
        this.foodCount--;
    }

    public int totalFood()
    {
        return this.foodCount;
    }
}
