using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ZombieAI : MonoBehaviour
{
    public ZombieState currentState = ZombieState.Idle;
    public float roamRadius = 10f;
    public float roamInterval = 5f;
    public float timeSpentIdle = 10f;
    
    private NavMeshAgent agent;
    private Animator animator;

    private float roamTimer;
    private float idleTimer = 0;
    private bool hasPatrollingDestination;

    public enum ZombieState
    {
        Idle,
        Patrolling,
        Chasing,
        Attacking,
        Dead
    }

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        roamTimer = roamInterval;
    }

    private void Update()
    {
        switch (currentState)
        {
            case ZombieState.Idle:
                HandleIdle();
                break;
            case ZombieState.Patrolling:
                HandlePatrolling();
                break;
            case ZombieState.Chasing:
                HandleChasing();
                break;
            case ZombieState.Attacking:
                HandleAttacking();
                break;
            case ZombieState.Dead:
                HandleDead();
                break;
        }
    }

    private void HandleDead()
    {
        throw new System.NotImplementedException();
    }

    private void HandleAttacking()
    {
        throw new System.NotImplementedException();
    }

    private void HandleChasing()
    {
        throw new System.NotImplementedException();
    }

    private void HandlePatrolling()
    {
        Debug.Log("Zombie is patrolling.");
        animator.SetBool("Running", true);
        if (hasPatrollingDestination == false)
        {
            Vector3 newPos = RandomNavSphere(transform.position, roamRadius, -1);
            agent.SetDestination(newPos);
            hasPatrollingDestination = true;
        }

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            currentState = ZombieState.Idle;
            hasPatrollingDestination = false;
        }

    }

    private void HandleIdle()
    {
        Debug.Log("Zombie is idle.");

        idleTimer += Time.deltaTime;
        animator.SetBool("Running", false);
        animator.SetBool("Idle", true);

        if (idleTimer >= timeSpentIdle)
        {
            animator.SetBool("Idle", false);
            currentState = ZombieState.Patrolling;
            idleTimer = 0;
        }

    }

    public static Vector3 RandomNavSphere(Vector3 origin, float dist, int layermask)
    {
        Vector3 randDirection = Random.insideUnitSphere * dist;
        randDirection += origin;

        NavMeshHit navHit;

        NavMesh.SamplePosition(randDirection, out navHit, dist, layermask);

        return navHit.position;
    }
}
