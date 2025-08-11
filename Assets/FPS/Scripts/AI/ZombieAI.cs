using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ZombieAI : MonoBehaviour
{
    public Transform player;
    public float roamRadius = 10f;
    public float roamInterval = 5f;

    private NavMeshAgent agent;
    private float roamTimer;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        roamTimer = roamInterval;
    }

    void Update()
    {
        roamTimer += Time.deltaTime;

        if (roamTimer >= roamInterval && agent.remainingDistance < 0.5f)
        {
            Vector3 newPos = RandomNavSphere(transform.position, roamRadius, -1);
            agent.SetDestination(newPos);
            roamTimer = 0;
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
