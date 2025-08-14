using Codice.Client.Common.GameUI;
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

    private CapsuleCollider agentCollider; 
    private NavMeshAgent agent;
    private Animator animator;
    private Health health;

    private float roamTimer;
    private float idleTimer = 0;
    private bool hasPatrollingDestination;
    private bool deathAnimPlayed = false;
    private float raycastChaseDistance = 10;
    private float attackRange = 2f;
    private float damage = 20;
    private float destroyZombietimer = 0; 
    private float destroyTimer = 5;


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
        agentCollider = GetComponent<CapsuleCollider>();
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        roamTimer = roamInterval;
        health = GetComponent<Health>();
        
    }

    void Start()
    {
        health.OnDie += OnDeath;
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
                HandleDeath();
                break;
        }
    }

    private void OnDeath()
    {
        currentState = ZombieState.Dead;
    }

    private void HandleDeath()
    {
        Debug.Log("Zombie is dead.");

        if (deathAnimPlayed == true && destroyZombietimer >= destroyTimer)
        {
            Destroy(gameObject);
        }

        agent.speed = 0;
        agent.enabled = false;
        animator.applyRootMotion = true;
        agentCollider.enabled = false;

        animator.SetBool("Idle", false);
        animator.SetBool("Attacking", false);
        animator.SetBool("Running", false);
        animator.SetBool("Death", true);

        deathAnimPlayed = true;
        destroyZombietimer += Time.deltaTime;
    }

    private void HandleAttacking()
    {
        Debug.Log("Zombie is attacking.");

        float distance = Vector3.Distance(transform.position, player.transform.position);

        if (distance > attackRange)
        {
            // Player moved out of range → chase again
            currentState = ZombieState.Chasing;
            return;
        }

        // Face the player at the start of attack
        Vector3 lookDir = (player.transform.position - transform.position).normalized;
        lookDir.y = 0;
        transform.rotation = Quaternion.LookRotation(lookDir);

        agent.speed = 0;

        animator.SetBool("Running", false);
        animator.SetBool("Attacking", true);
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

        LineOfSightCheck();
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

        LineOfSightCheck();
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

    public void DealDamage()
    {
        // You can refine this with range checks, raycasts, etc.
        float distance = Vector3.Distance(transform.position, player.transform.position);
        if (distance <= attackRange)
        {
            Debug.Log("Zombie hit the player!");
            player.GetComponent<Damageable>().InflictDamage(damage, false, player);
        }
    }

    public void DestoryZombie()
    {

    }
}
