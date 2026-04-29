using UnityEngine;
using UnityEngine.AI;

public class CPU : MonoBehaviour
{
    [SerializeField]
    private NavMeshAgent agent;
    [SerializeField]
    private Transform target;

    void Update()
    {
        agent.SetDestination(target.position);
    }
}
