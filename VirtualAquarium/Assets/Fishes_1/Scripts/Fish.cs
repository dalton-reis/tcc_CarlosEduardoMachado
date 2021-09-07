using System;
using System.Collections;
using System.Collections.Generic;
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
    private Vector3 target;
    public Camera camerap;
    new private Rigidbody rigidbody;
    public float moveSpeed = 0f;
    public float turnSpeed = 0f;
    private float eatRadius = 2f;

    public List<Camera> cameras;

    public string id;

    public NetworkIdentity identity;

    public bool iniciado = false;
    private int totalRotate = 0;
    private float deadTime = 0;
    private float timeSwimmingAway = 0;
    private float timeStayed = 0;
    public float life;
    public float lifeTime;

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

        if (!gameController)
            gameController = GameObject.FindObjectOfType<GameController>();
        fishArea = GameObject.FindObjectOfType<FishArea>();
        rigidbody = GetComponent<Rigidbody>();



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

        if (life <= 40)
            AddReward(-1f);
        else
            AddReward(0.5f);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = -Input.GetAxis("Horizontal");
        continuousActionsOut[1] = Input.GetAxis("Vertical");
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float[] vectorAction = actions.ContinuousActions.Array;
        Debug.Log(string.Join("|", new List<float>(vectorAction).ConvertAll<string>((value) => value.ToString()).ToArray()));


        state = FStates.Stay;

        // Converte a primeira ação em movimento para frente
        float forwardAmount = vectorAction[0];
        if (forwardAmount > 0)
            state = FStates.Patrol;

        // Converte a secunda ação em virar pra esquerda ou pra direita
        float turnAmount = vectorAction[1];

        //Converte a terceira ação para a chamada do método correspondente (1 para beber, 2 para comer)
        if (vectorAction[2] != 0)
        {
            Feed();
        }

        // Aplica o movimento
        Debug.Log(turnAmount * turnSpeed);
        transform.Rotate(transform.up, turnAmount * turnSpeed);
        if (!CheckInvalidPosition(transform.forward * Time.fixedDeltaTime * moveSpeed * forwardAmount)) { 
            transform.position += transform.forward * Time.fixedDeltaTime * moveSpeed * forwardAmount;
        } else
        {
            AddReward(-1f / MaxStep);
        }

        // Aplica um a pequena recompensa negativa para encorajar o personagem a fazer uma ação
        AddReward(-1f / MaxStep);
    }


    public override void OnEpisodeBegin()
    {
        life = UnityEngine.Random.Range(50, 100);
        fishArea.feedPoint.clearFood();
        GameObject food = GameObject.FindGameObjectsWithTag("Food")[0];
        food.SetActive(true);

        transform.position = fishArea.GetRandomPoint();

        food.transform.position = new Vector3(UnityEngine.Random.value * 12 - 6,
                                           UnityEngine.Random.value * 4 - 2,
                                           UnityEngine.Random.value * 2 - 1);

        if (food.activeSelf)
        {
            fishArea.feedPoint.addFood(food);
        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(this.transform.localPosition);
        GameObject[] foods = GameObject.FindGameObjectsWithTag("Food");
        GameObject food = foods[0];
        sensor.AddObservation(food.activeSelf);
        sensor.AddObservation(food.transform.position);
        sensor.AddObservation((int)life);

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

    void Feed()
    {
        if (fishArea.feedPoint.foods.Count > 0)
        {
            foreach (GameObject food in fishArea.feedPoint.foods)
            {
                if (Vector3.Distance(transform.position, food.transform.position) < eatRadius)
                {
                    state = FStates.Feed;

                    if (life <= 40)
                        AddReward(1f);
                    else
                        AddReward(-0.5f);

                    fishArea.removeFood(food);
                    life += UnityEngine.Random.Range(15, 30);
                }
            }
        }
    }

    void Die()
    {
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

    private bool CheckInvalidPosition(Vector3 _positionToCheck)
    {
        Bounds bounds = fishArea.GetComponent<BoxCollider>().bounds;

        if (_positionToCheck.x < bounds.min.x || _positionToCheck.x > bounds.max.x) { return false; }
        if (_positionToCheck.y < bounds.min.y || _positionToCheck.y > bounds.max.y) { return false; }
        if (_positionToCheck.z < bounds.min.z || _positionToCheck.z > bounds.max.z) { return false; }
        return true;
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
                //Patrol();
                break;
            case FStates.Stay:
                //Stay();
                break;
            case FStates.SwimAway:
                ///SwimAway();
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
        }
        else if (other.tag == "Food" && state == FStates.Feed)
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