using UnityEngine;
using UnityEngine.AI;

public class WanderAI : MonoBehaviour
{
    public Transform[] patrolPoints;
   
    private int currentPointIndex = 0;
    private NavMeshAgent agent;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        agent.SetDestination(patrolPoints[currentPointIndex].position);

    }

    void Update()
    {
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            currentPointIndex = (currentPointIndex + 1) % patrolPoints.Length;
            agent.SetDestination(patrolPoints[currentPointIndex].position);
        }
    }
}
