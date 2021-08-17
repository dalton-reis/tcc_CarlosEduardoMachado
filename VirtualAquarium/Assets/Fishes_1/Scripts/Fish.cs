using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fish : MonoBehaviour {
    Animator animator;
    float timer;
    public FStates state;
    public FStates lastState;
    List<Transform> playersAround = new List<Transform>();
    private FishArea fishArea;
    private Vector3 target;    
    private int totalRotate = 0;
    private float deadTime = 0;
    private float timeSwimmingAway = 0;
    private float timeStayed = 0;
    public float life;
    public float lifeTime;
    public enum FStates
    {
        Patrol, Stay, SwimAway, Feed, Die
    }

    public class FishComparer : IComparer<Fish>
    {
        public int Compare(Fish x, Fish y)
        {
            if (x.life == 0 || y.life == 0)
            {
                return 0;
            }             
            return x.life.CompareTo(y.life);

        }
    }

    private void Start()
    {
        life = 100;
    }

    private void lifeUpdate()
    {
        if (state != FStates.Die)
        {
            lifeTime += Time.deltaTime;
            if (life > 0)
            {
                if (lifeTime >= 1)
                {
                    life -= AquariumProperties.lifeLostPerHour / AquariumProperties.timeSpeedMultiplier + AquariumProperties.lossLifeCoefficient;
                    lifeTime = 0;
                }
                if (timeSwimmingAway >= 1)
                {
                    life -= AquariumProperties.lifeLostPerHour / AquariumProperties.timeSpeedMultiplier;
                    timeSwimmingAway = 0;
                }
                if (timeStayed >= 1)
                {
                    life += AquariumProperties.lifeLostPerHour / AquariumProperties.timeSpeedMultiplier;
                    timeStayed = 0;
                }
            }            
            else if (life <= 0)
            {                 
                target = new Vector3(transform.position.x, fishArea.transform.position.y + 2.3f, 0);
                animator.speed = 0.2f;
                lastState = state;
                state = FStates.Die;
            }
        }
    }    

    internal void goToFeed()
    {
        lastState = state;
        state = FStates.Feed;
        target = fishArea.feedPoint.foods[fishArea.feedPoint.foods.Count-1].transform.position;
    }

    void Feed()
    {        
        transform.position += transform.forward * Time.deltaTime * fishArea.speed * 1.5f;
        transform.forward = Vector3.MoveTowards(transform.forward, target - transform.position, Time.deltaTime * fishArea.rotationSpeed);
    }

    void Die()
    {
        transform.position = Vector3.MoveTowards(transform.position, target, Time.deltaTime * fishArea.speed * 0.5f);
        deadTime += Time.deltaTime;
        if (totalRotate < 180)
        {
            transform.Rotate(0, 0, 2);
            totalRotate += 2;
        } 
        if (transform.position == target && deadTime > 25)
        {
            fishArea.removeFish(this);
            gameObject.SetActive(false);
        }       
    }

    void Patrol()
    {
        animator.SetInteger("State", 1);
        Ray ray = new Ray(transform.position, transform.forward);
        var casts = Physics.RaycastAll(ray, fishArea.raycastDistance);
        foreach (var cast in casts)
        {
            if (cast.collider.transform != this.transform)
            {
                if (cast.collider.tag.Equals("Vase") || cast.collider.tag.Equals("Terrain") || lastState == FStates.Feed)
                {
                    do
                    {
                        target = fishArea.GetRandomPoint();
                    } while (target.y > 2);
                }

                timer = UnityEngine.Random.Range(0, 10f);                
                break;
            }
        }       
        transform.position += transform.forward * Time.deltaTime * fishArea.speed;
        transform.forward = Vector3.MoveTowards(transform.forward, target - transform.position, Time.deltaTime * fishArea.rotationSpeed);
        if ((transform.position - target).magnitude < fishArea.speed * Time.deltaTime * 3 || timer < 0f)
        {
            target = fishArea.GetRandomPoint();
            timer = UnityEngine.Random.Range(0, 10f);
            if (UnityEngine.Random.Range(0f, 1f) > 0.9f)
            {
                lastState = state;
                state = FStates.Stay;
            }
                
        }        
    }

    void SwimAway()
    {
        this.timeSwimmingAway += Time.deltaTime;        
        Vector3 runVector = Vector3.zero;
        foreach (var t in playersAround)
            runVector += (t.transform.position - transform.position).normalized;
        runVector.Normalize();
        transform.forward = Vector3.MoveTowards(transform.forward, -runVector, Time.deltaTime * fishArea.rotationSpeed * 10);
        transform.position += transform.forward * Time.deltaTime * fishArea.speed;
    }

    void Stay()
    {
        this.timeStayed += Time.deltaTime;
        transform.position += transform.forward * Time.deltaTime * fishArea.speed / 20f;
        animator.SetInteger("State", 0);
        if (timer < 0f)
        {
            if (UnityEngine.Random.Range(0f, 1f) < 0.9f)
            {
                lastState = state;
                state = FStates.Patrol;
            }
                
        }
    }

    internal void Move()
    {       
        timer -= Time.deltaTime;        
        lifeUpdate();
        switch (state)
        {
            case FStates.Patrol:
                Patrol();
                break;
            case FStates.Stay:
                Stay();
                break;
            case FStates.SwimAway:
                SwimAway();
                break;
            case FStates.Feed:
                Feed();
                break;
            case FStates.Die:
                Die();
                break;
        }
    }

    internal void Initialize(FishArea fishArea)
    {
        lastState = state;
        state = FStates.Patrol;
        this.fishArea = fishArea;
        animator = GetComponent<Animator>();
        target = fishArea.GetRandomPoint();
    }

    public void AddPlayer(Transform t)
    {
        playersAround.Add(t);
    }

    public void RemovePlayer(Transform t)
    {
        playersAround.Remove(t);
    }

    public void OnTriggerEnter(Collider other)
    {        
        if (other.tag == "Player" && state != FStates.Feed)
        {            
            AddPlayer(other.transform);
            lastState = state;
            state = FStates.SwimAway;
            if (animator != null)
            {
                animator.SetInteger("State", 1);
            }            
        } else if (other.tag == "Food" && state == FStates.Feed)
        {
            target = transform.position;
            fishArea.removeFood();
            life += UnityEngine.Random.Range(15, 30);
            lastState = state;
            state = FStates.Patrol;
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            RemovePlayer(other.transform);
            if (playersAround.Count == 0)
            {
                lastState = state;
                state = FStates.Patrol;
                target = fishArea.GetRandomPoint();
            }
        }
    }

    void OnDrawGizmos()
    {
        // Draw a yellow sphere at the transform's position
        Gizmos.color = Color.yellow;
        Ray ray = new Ray(transform.position, transform.forward);
        if (transform.parent == null)
            return;
        fishArea = transform.parent.GetComponent<FishArea>();
        if (fishArea == null)
            return;
        float raycastDistance = fishArea.raycastDistance;
        Gizmos.color = Color.black;
        Gizmos.DrawRay(transform.position, transform.forward * raycastDistance);
    }
}
