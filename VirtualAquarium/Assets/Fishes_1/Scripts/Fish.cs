using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Mirror;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class Fish : Agent
{
    Animator animator;
    float timer;
    public FStates state;
    public FStates lastState;
    List<Transform> playersAround = new List<Transform>();
    public FishArea fishArea;
    public Vector3 target;
    public Camera camerap;
    new private Rigidbody rigidbody;
    public float moveSpeed = 0f;
    public float turnSpeed = 0f;
    private float eatRadius = 2f;
    private Bounds bounds;
    private Vector3 rotateTarget, position, direction, velocity;

    public List<Camera> cameras;

    public string id;

    public NetworkIdentity identity;

    public bool iniciado = false;
    private int totalRotate = 0;
    private float deadTime = 0;
    private float timeSwimmingAway = 0;
    public float timeSinceFeed = 10;
    private float timeStayed = 0;
    public float life;
    public float energy;
    public float lifeTime;
    public float distanceFromTarget;
    public float distanceFromFood;
    public Vector3 heuristic;
    private static Mutex mutexFeed = new Mutex();

    [System.NonSerialized]
    public float changeTarget = 0f, changeAnim = 0f, timeSinceTarget = 0f, timeSinceAnim = 0f, prevAnim, currentAnim = 0f, prevSpeed, speed, zturn, prevz,
    turnSpeedBackup;
    private Quaternion lookRotation;

    private float rechargingEnergy;

    GameController gameController;
    public enum FStates
    {
        Patrol,
        Stay,
        SwimAway,
        Feed,
        Die
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
        energy = 100;

        if (!gameController)
            gameController = GameObject.FindObjectOfType<GameController>();
        fishArea = GameObject.FindObjectOfType<FishArea>();
        rigidbody = GetComponent<Rigidbody>();
        bounds = fishArea.GetComponent<BoxCollider>().bounds;
        state = FStates.Stay;


        if (gameController.multi)
        {
            identity = GetComponent<NetworkIdentity>();
            if (identity.isClient)
            {
                id = identity.netId.ToString();
                camerap.name = "camera_peixe" + id;
            }
        }

        InvokeRepeating("UpdateControllers", 0, 1.0f);
    }

    private void UpdateControllers()
    {

    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = -heuristic.x;
        continuousActionsOut[1] = heuristic.y;
        continuousActionsOut[2] = heuristic.z;

        actionsOut.DiscreteActions.Array[0] = 1;
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float[] vectorAction = actions.ContinuousActions.Array;


        //Converte a terceira ação para a chamada do método correspondente (1 para beber, 2 para comer)
        if (actions.DiscreteActions[0] == 1)
        {
            Feed();
        }
        if (state == FStates.Stay && HasEnergy())
        {
            state = FStates.Patrol;

            target = new Vector3(vectorAction[0] * fishArea.GetComponent<BoxCollider>().size.x / 2 * fishArea.transform.localScale.x * 0.9f,
                                 vectorAction[1] * fishArea.GetComponent<BoxCollider>().size.y / 2 * fishArea.transform.localScale.y * 0.9f,
                                 vectorAction[2] * fishArea.GetComponent<BoxCollider>().size.z / 2 * fishArea.transform.localScale.z * 0.9f) + fishArea.transform.position;
            //Debug.Log(string.Join("|", new List<float>(vectorAction).ConvertAll<string>((value) => value.ToString()).ToArray()));
            //Debug.Log("y" + target.y);
            if (Vector3.Magnitude(target - rigidbody.position) < 1)
            {
                AddReward(-1f / MaxStep);
            }
        }
    }


    public override void OnEpisodeBegin()
    {
        if (!gameController)
            gameController = GameObject.FindObjectOfType<GameController>();
        if (gameController.Simulador)
        {
            life = UnityEngine.Random.Range(50, 100);
            energy = UnityEngine.Random.Range(50, 100);

            transform.position = fishArea.GetRandomPoint();
            state = FStates.Stay;

            while (fishArea.feedPoint.foods.Count > 0)
                fishArea.removeFood();

            fishArea.particleFood.Play();
        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.forward);
        sensor.AddObservation(IsLowLife());
        sensor.AddObservation((int)state);
        sensor.AddObservation(IsSuperLife());
        sensor.AddObservation((int)distanceFromFood);
    }

    private bool IsLowLife()
    {
        return life <= 40;
    }
    private bool IsSuperLife()
    {
        return life > 100;
    }

    private bool HasEnergy()
    {
        return energy >= 50;
    }

    private void Update()
    {
        if (gameController.multi)
        {
            if (identity.isClient)
            {
                if (identity.isLocalPlayer)
                {
                    cameras = new List<Camera>(GameObject.FindObjectsOfType<Camera>());
                    for (int i = 0; i < cameras.Count; i++)
                    {
                        if (cameras[i].enabled)
                        {
                            if ((cameras[i].name != "camera_peixe" + id) && (cameras[i].name != "Main Camera"))
                            {
                                cameras[i].enabled = false;
                            }
                        }
                    }
                }
            }
        }

    }

    private void lifeUpdate()
    {
        if (state != FStates.Die)
        {
            lifeTime += Time.deltaTime;
            timeSinceFeed += Time.deltaTime;
            if (life > 0)
            {
                if (!IsLowLife() && !IsSuperLife())
                    AddReward(100f / MaxStep);


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
                if (animator)
                    animator.speed = 0.2f;
                lastState = state;
                state = FStates.Die;
            }
        }
    }

    void Feed()
    {
        if (fishArea.feedPoint.foods.Count > 0)
        {

            GameObject food = fishArea.feedPoint.foods[0];
            distanceFromFood = Vector3.Distance(transform.position, food.transform.position);
            if (Vector3.Distance(transform.position, food.transform.position) < eatRadius)
            {
                mutexFeed.WaitOne();
                if (timeSinceFeed >= 10)
                {
                    timeSinceFeed = 0;
                    Debug.Log("Comeu");

                    if (IsLowLife()) AddReward(1f);
                    //if (IsSuperLife()) AddReward(-100f / MaxStep);

                    life += UnityEngine.Random.Range(15, 30);
                }
                mutexFeed.ReleaseMutex();
            }

        }
    }

    void Die()
    {
        AddReward(-300f);
        EndEpisode();
        return;
        transform.position = Vector3.MoveTowards(transform.position, target, Time.deltaTime * fishArea.speed * 0.5f);
        deadTime += Time.deltaTime;
        if (totalRotate < 180)
        {
            transform.Rotate(0, 0, 2);
            totalRotate += 2;
        }
        if (transform.position == target && deadTime > 25)
        {
            gameObject.SetActive(false);
            fishArea.removeFish(this);
        }


    }

    private bool CheckValidPosition(Vector3 _positionToCheck)
    {
        if (_positionToCheck.x < bounds.min.x || _positionToCheck.x > bounds.max.x) { return false; }
        if (_positionToCheck.y < bounds.min.y || _positionToCheck.y > bounds.max.y) { return false; }
        if (_positionToCheck.z < bounds.min.z || _positionToCheck.z > bounds.max.z) { return false; }
        return true;
    }

    void Patrol()
    {
        // Calculate distances
        distanceFromTarget = Vector3.Magnitude(target - rigidbody.position);
        rotateTarget = target;
        // Allow drastic turns close to base to ensure target can be reached
        if (distanceFromTarget < 2f)
        {
            if (turnSpeed != 10f && rigidbody.velocity.magnitude != 0f)
            {
                turnSpeedBackup = turnSpeed;
                turnSpeed = 10f;
            }
            else if (distanceFromTarget <= 0.5f)
            {
                rigidbody.velocity = Vector3.zero;
                turnSpeed = turnSpeedBackup;

                lastState = state;
                state = FStates.Stay;
                //AddReward(-1f);
                return;
            }
        }
        if (rigidbody.transform.position.y < bounds.min.y + 1f ||
            rigidbody.transform.position.y > bounds.max.y - 1f)
        {
            if (rigidbody.transform.position.y < bounds.center.x + 1f) rotateTarget.y = 0.1f; else rotateTarget.y = -0.1f;
        }
        // Update times
        changeAnim -= Time.fixedDeltaTime;
        changeTarget -= Time.fixedDeltaTime;
        timeSinceTarget += Time.fixedDeltaTime;
        timeSinceAnim += Time.fixedDeltaTime;

        // Rotate towards target
        Vector3 targetDirection = target - transform.position;
        float singleStep = turnSpeed * Time.deltaTime;
        Vector3 newDirection = Vector3.RotateTowards(transform.forward, targetDirection, singleStep, 0.0f);
        transform.rotation = Quaternion.LookRotation(newDirection);

        // Move flyer
        direction = Quaternion.Euler(transform.eulerAngles) * Vector3.forward;
        rigidbody.velocity = Mathf.Lerp(prevSpeed, moveSpeed, Mathf.Clamp(timeSinceAnim / 3, 0f, 1f)) * direction;
        // Hard-limit the height, in case the limit is breached despite of the turnaround attempt
        if (rigidbody.transform.position.y < bounds.min.y || rigidbody.transform.position.y > bounds.max.y)
        {
            position = rigidbody.transform.position;
            position.y = Mathf.Clamp(position.y, bounds.min.y, bounds.max.y);
            rigidbody.transform.position = position;
        }
    }

    // Select a new animation speed randomly
    private float ChangeAnim(float currentAnim)
    {
        return 0;
    }

    // Select a new direction to fly in randomly

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
        animator?.SetInteger("State", 0);
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
                RequestDecision();
                break;
            case FStates.SwimAway:
                SwimAway();
                break;
            case FStates.Feed:
                //Feed();
                break;
            case FStates.Die:
                Die();
                break;
        }
    }

    internal void Initialize(FishArea fishArea)
    {
        fishArea = GameObject.FindObjectOfType<FishArea>();
        lastState = state;
        state = FStates.Patrol;
        this.iniciado = true;
        this.fishArea = fishArea;
        animator = GetComponent<Animator>();
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
            animator?.SetInteger("State", 1);
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
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