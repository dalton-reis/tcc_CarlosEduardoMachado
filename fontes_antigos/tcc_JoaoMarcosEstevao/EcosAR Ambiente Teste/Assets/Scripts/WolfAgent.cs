using UnityEngine;
using MLAgents;
using TMPro;
using System.Collections.Generic;

public class WolfAgent : Agent
{
    public float moveSpeed = 0f;
    public float turnSpeed = 0f;

    private SimulationController simulationController;

    private WolfAcademy wolfAcademy;
    new private Rigidbody rigidbody;

    Animator animator;

    GameObject closestPrey;

    private int deathTimer;
    private int hunger;
    private int thirst;

    private float drinkRadius = 0f;
    private float eatRadius = 0f;

    CurrentAnimalAction currentAction;

    void Start()
    {
        base.InitializeAgent();

        hunger = 0;
        thirst = 0;

        wolfAcademy = FindObjectOfType<WolfAcademy>();
        rigidbody = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        simulationController = FindObjectOfType<SimulationController>();

        closestPrey = null;

        currentAction = CurrentAnimalAction.idle;

        InvokeRepeating("UpdateControllers", 0, 1.0f);
    }

    private bool IsHungry()
    {
        return hunger >= 60;
    }

    private bool IsThirsty()
    {
        return thirst >= 50;
    }

    private void UpdateControllers()
    {
        hunger += 1;
        thirst += 5;

        if (hunger >= 200 || thirst >= 200)
            AddReward(-1f);
        else if (IsHungry() || IsThirsty())
            AddReward(-0.5f);
        else
            AddReward(0.5f);
    }

    public override void AgentAction(float[] vectorAction)
    {
        if (currentAction == CurrentAnimalAction.dead)
            return;

        currentAction = CurrentAnimalAction.idle;

        // Converte a primeira ação em movimento para frente
        float forwardAmount = vectorAction[0];
        if (forwardAmount > 0)
            currentAction = CurrentAnimalAction.walking;

        // Converte a secunda ação em virar pra esquerda ou pra direita
        float turnAmount = 0f;
        if (vectorAction[1] == 1f)
        {
            turnAmount = -1f;
        }
        else if (vectorAction[1] == 2f)
        {
            turnAmount = 1f;
        }

        if (vectorAction[2] == 1f)
        {
            Drink();
            ControlAnimation();
            return;
        }
        else if (vectorAction[2] == 2f)
        {
            Eat();
            ControlAnimation();
            return;
        }
        else if (vectorAction[2] == 3f)
        {
            Hunt();
            ControlAnimation();
            return;
        }

        // Aplica o movimento
        rigidbody.MovePosition(transform.position + transform.forward * forwardAmount * moveSpeed * Time.fixedDeltaTime);
        transform.Rotate(transform.up * turnAmount * turnSpeed * Time.fixedDeltaTime);

        // Aplica um a pequena recompensa negativa para encorajar o personagem a fazer uma ação
        AddReward(-1f / agentParameters.maxStep);

        ControlAnimation();
    }

    private string GetCurrentAction()
    {
        if (animator.GetBool("isWalking"))
            return "walking";

        if (animator.GetBool("isAttacking"))
            return "attacking";

        if (animator.GetBool("isDead"))
            return "dead";

        if (animator.GetBool("isRunning"))
            return "running";

        if (animator.GetBool("isHowling"))
            return "howling";

        return "idle";
    }

    private void ControlAnimation()
    {
        animator.SetBool("isWalking", false);
        animator.SetBool("isAttacking", false);
        animator.SetBool("isDead", false);
        animator.SetBool("isRunning", false);
        animator.SetBool("isHowling", false);

        switch (currentAction)
        {
            case CurrentAnimalAction.walking:
                animator.SetBool("isWalking", true);
                break;
            case CurrentAnimalAction.running:
                animator.SetBool("isRunning", true);
                break;
            case CurrentAnimalAction.attacking:
                animator.SetBool("isAttacking", true);
                break;
            case CurrentAnimalAction.dead:
                animator.SetBool("isDead", true);
                break;
        }
    }

    public override float[] Heuristic()
    {
        float forwardAction = 0f;
        float turnAction = 0f;
        float specialAction = 0f;

        if (Input.GetKey(KeyCode.W))
        {
            // move para frente
            forwardAction = 1f;
        }

        if (Input.GetKey(KeyCode.A))
        {
            // vira para esquerda
            turnAction = 1f;
        }
        else if (Input.GetKey(KeyCode.D))
        {
            // vira para direita
            turnAction = 2f;
        }

        if (Input.GetKey(KeyCode.C))
        {
            specialAction = 1;
        }
        else if (Input.GetKey(KeyCode.X))
        {
            specialAction = 2;
        }
        else if (Input.GetKey(KeyCode.Z))
        {
            specialAction = 3;
        }

        // Coloca as ações em um array e retorna (será processado pelo AgentAction posteriormente)
        return new float[] { forwardAction, turnAction, specialAction };
    }

    /// Reset the agent and terrain
    public override void AgentReset()
    {
        hunger = 0;
        thirst = 0;

        drinkRadius = 0.1f;
        eatRadius = 0.5f;
    }

    /// Collect all non-Raycast observations
    public override void CollectObservations()
    {
        // Se o lobo está com fome
        AddVectorObs(IsHungry());

        // Se o lobo está com sede
        AddVectorObs(IsThirsty());

        // A direção para qual o lobo está virado (1 Vector3 = 3 values)
        AddVectorObs(transform.forward);
    }

    private void Hunt()
    {
        GameObject[] rabbits = GameObject.FindGameObjectsWithTag("Rabbit");
        GameObject[] deer = GameObject.FindGameObjectsWithTag("Deer");

        var preyList = new List<GameObject>();
        preyList.AddRange(rabbits);
        preyList.AddRange(deer);

        if (closestPrey == null)
            closestPrey = GetClosestPrey(preyList);

        if (closestPrey == null || !IsHungry())
            return;

        transform.LookAt(closestPrey.transform);
        currentAction = CurrentAnimalAction.running;

        float huntingSpeed = moveSpeed + 0.02f;
        rigidbody.MovePosition(transform.position + transform.forward * 1 /*forwardAmount*/ * huntingSpeed * Time.fixedDeltaTime);
    }

    private void Eat()
    {
        GameObject[] deadPrey = null;

        try
        {
            deadPrey = GameObject.FindGameObjectsWithTag("DeadPrey");
        }
        catch (UnityException exception)
        {
            return;
        }

        if (deadPrey != null && deadPrey.Length > 0)
        {
            if (Vector3.Distance(transform.position, deadPrey[0].transform.position) < eatRadius)
            {
                currentAction = CurrentAnimalAction.eating;

                if (IsHungry())
                    AddReward(1f);
                else
                    AddReward(-0.5f);

                hunger -= 10;

                if (hunger < 0)
                    hunger = 0;
            }
        }
    }

    private GameObject GetClosestPrey(List<GameObject> preyList)
    {
        GameObject closest = null;
        float distance = Mathf.Infinity;
        Vector3 position = transform.position;
        foreach (GameObject prey in preyList)
        {
            Vector3 diff = prey.transform.position - position;
            float curDistance = diff.sqrMagnitude;
            if (curDistance < distance)
            {
                closest = prey;
                distance = curDistance;
            }
        }

        return closest;
    }

    private void Drink()
    {
        GameObject[] drinkSpots = GameObject.FindGameObjectsWithTag("DrinkSpot");

        if (drinkSpots.Length > 1)
        {
            if (Vector3.Distance(transform.position, drinkSpots[0].transform.position) < drinkRadius ||
                Vector3.Distance(transform.position, drinkSpots[1].transform.position) < drinkRadius)
            {
                currentAction = CurrentAnimalAction.drinking;

                if (IsThirsty())
                    AddReward(1f);
                else
                    AddReward(-0.5f);

                thirst -= 10;

                if (thirst < 0)
                    thirst = 0;
            }
        }
    }

    /// When the agent collides with something, take action
    /// <param name="collision">The collision info</param>
    private void OnCollisionEnter(Collision collision)
    {
        if (currentAction == CurrentAnimalAction.running)
        {
            if (collision.transform.CompareTag("Rabbit"))
            {
                currentAction = CurrentAnimalAction.attacking;
                RabbitAgent rabbit = collision.gameObject.GetComponent<RabbitAgent>();
                rabbit.Die();

                closestPrey = null;
            }
            else if (collision.transform.CompareTag("Deer"))
            {
                currentAction = CurrentAnimalAction.attacking;
                DeerAgent deer = collision.gameObject.GetComponent<DeerAgent>();
                deer.Die();

                closestPrey = null;
            }
        }
    }

    private void UpdateDeathTimer()
    {
        deathTimer++;

        if (deathTimer == 10)
        {
            Destroy(gameObject);
        }
    }

    public void Die()
    {
        currentAction = CurrentAnimalAction.dead;
        ControlAnimation();

        InvokeRepeating("UpdateDeathTimer", 0, 1.0f);
    }
}