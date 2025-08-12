using System.Collections.Generic;
using Unity.FPS.Game;
using UnityEngine;
using UnityEngine.AI;

public class ZombieAI : MonoBehaviour
{
    public GameObject player;
    public ZombieState currentState = ZombieState.Idle;
    public float roamRadius = 10f;
    public float roamInterval = 5f;
    public float timeSpentIdle = 10f;
    
    private NavMeshAgent agent;
    private Animator animator;

    private string currentAnimState = "Idle";
    private float roamTimer;
    private float idleTimer = 0;
    private float AttackTimer = 0;
    private bool hasPatrollingDestination;
    private float raycastChaseDistance = 10;
    private float attackRange = 4f;

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
        LineOfSightCheck();
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
        Debug.Log("Zombie is dead.");
        throw new System.NotImplementedException();
    }

    private void HandleAttacking()
    {
        Debug.Log("Zombie is attacking.");

        // Face the player at the start of attack
        Vector3 lookDir = (player.transform.position - transform.position).normalized;
        lookDir.y = 0;
        transform.rotation = Quaternion.LookRotation(lookDir);

        /*float distance = Vector3.Distance(transform.position, player.transform.position);

        if (distance > attackRange)
        {
            // Player moved out of range → chase again
            currentState = ZombieState.Chasing;
            return;
        }*/

        agent.speed = 0;

        animator.SetBool("Running", false);
        animator.SetBool("Attacking", true);

        

        AttackTimer += Time.deltaTime; 
        if (AttackTimer >= 2.18f)
        {
            player.GetComponent<Damageable>().InflictDamage(20, false, player);
            AttackTimer = 0;
        }
    }

    private void HandleChasing()
    {
        Debug.Log("Zombie is chasing.");

        animator.SetBool("Idle", false);
        animator.SetBool("Attacking", false);
        animator.SetBool("Running", true);

        agent.speed = 3.5f;
        agent.SetDestination(player.transform.position);

        float distance = Vector3.Distance(transform.position, player.transform.position);

        if (distance <= attackRange)
        {
            currentState = ZombieState.Attacking;
        }

    }

    private void HandlePatrolling()
    {
        Debug.Log("Zombie is patrolling.");
        
        animator.SetBool("Running", true);

        LineOfSightCheck();

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

    private void LineOfSightCheck()
    {
        Vector3 origin = transform.position + Vector3.up * 1f; // offset up a bit

        Ray ray = new Ray(origin, transform.forward);
        RaycastHit hit;

        Debug.DrawRay(ray.origin, ray.direction * raycastChaseDistance, Color.red);

        if (Physics.Raycast(ray, out hit, raycastChaseDistance))
        {

            if (hit.collider.gameObject == player)
            {
                Debug.Log("Player has been hit");

                currentState = ZombieState.Chasing;
            }

        }
    }
}
