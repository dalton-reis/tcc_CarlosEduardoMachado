using UnityEngine;
using MLAgents;
using TMPro;

public class RabbitAgent : Agent
{
    public float moveSpeed = 0f;
    public float turnSpeed = 0f;

    private SimulationController simulationController;

    private RabbitAcademy rabbitAcademy;
    new private Rigidbody rigidbody;

    Animator animator;

    private int deathTimer;
    private int hunger;
    private int thirst;

    private float drinkRadius = 0f;

    CurrentAnimalAction currentAction;

    void Start()
    {
        base.InitializeAgent();

        hunger = 0;
        thirst = 0;

        rabbitAcademy = FindObjectOfType<RabbitAcademy>();
        rigidbody = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        simulationController = FindObjectOfType<SimulationController>();

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
        hunger += 5;
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

        //Converte a terceira ação para a chamada do método correspondente (1 para beber, 2 para comer)
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

        // Aplica o movimento
        rigidbody.MovePosition(transform.position + transform.forward * forwardAmount * moveSpeed * Time.fixedDeltaTime);
        transform.Rotate(transform.up * turnAmount * turnSpeed * Time.fixedDeltaTime);

        // Aplica um a pequena recompensa negativa para encorajar o personagem a fazer uma ação
        AddReward(-1f / agentParameters.maxStep);

        ControlAnimation();
    }

    private string GetCurrentAnimalAction()
    {
        if (currentAction == CurrentAnimalAction.walking)
            return "walking";

        if (currentAction == CurrentAnimalAction.dead)
            return "dead";

        if (currentAction == CurrentAnimalAction.running)
            return "running";

        if (currentAction == CurrentAnimalAction.drinking)
            return "drinking";

        if (currentAction == CurrentAnimalAction.eating)
            return "eating";

        return "idle";
    }

    private void ControlAnimation()
    {
        animator.SetBool("isJumping", false);
        animator.SetBool("isJumping_Up", false);
        animator.SetBool("isDead_1", false);
        animator.SetBool("isDead_0", false);
        animator.SetBool("isRunning", false);
        animator.SetBool("isLookingOut", false);

        switch (currentAction)
        {
            case CurrentAnimalAction.walking:
                animator.SetBool("isJumping", true);
                break;
            case CurrentAnimalAction.running:
                animator.SetBool("isRunning", true);
                break;
            case CurrentAnimalAction.dead:
                animator.SetBool("isDead_1", true);
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

        // Coloca as ações em um array e retorna (será processado pelo AgentAction posteriormente)
        return new float[] { forwardAction, turnAction, specialAction };
    }

    /// Reset the agent and terrain
    public override void AgentReset()
    {
        hunger = 0;
        thirst = 0;

        drinkRadius = 0.1f;
    }

    /// Collect all non-Raycast observations
    public override void CollectObservations()
    {
        // Se o coelho está com fome
        AddVectorObs(IsHungry());

        // Se o coelho está com sede
        AddVectorObs(IsThirsty());

        // A direção para qual o coelho está virado (1 Vector3 = 3 values)
        AddVectorObs(transform.forward);
    }

    private void Eat()
    {
        if (simulationController.GetSceneState() != SceneState.Favorable)
            return;

        currentAction = CurrentAnimalAction.eating;

        if (IsHungry())
            AddReward(1f);
        else
            AddReward(-0.5f);

        hunger -= 10;

        if (hunger < 0)
            hunger = 0;
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

    private void UpdateDeathTimer()
    {
        deathTimer++;

        if (deathTimer == 30)
        {          
            Destroy(gameObject);
        }
    }

    public void Die()
    {
        gameObject.tag = "DeadPrey";

        currentAction = CurrentAnimalAction.dead;
        ControlAnimation();

        InvokeRepeating("UpdateDeathTimer", 0, 1.0f);
    }
}