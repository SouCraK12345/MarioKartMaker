using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.AI;

public class CPU : MonoBehaviour
{
    // [SerializeField]
    // private NavMeshAgent agent;
    [SerializeField]
    private Transform target;
    public PlayerController playerController;
    [SerializeField]
    private List<Vector3> targetPosition;
    public int phase = 0;
    private bool hasReset = false;
    public float acceleration = 20f;
    public float speedDivisor = 1.033f;
    public float speed = 0;
    public float rotationSpeed;
    private Rigidbody rb;
    private NavMeshPath path;
    public bool autoDriving = false;
    public bool autoCamera = false;
    private Camera MainCamera;
    void Start()
    {
        path = new NavMeshPath();
    }
    void Update()
    {
        if (!autoDriving) return;
        if (autoCamera)
        {
            
        }
        // agent.SetDestination(target.position);
        rb = GetComponent<Rigidbody>();
        // if (agent.remainingDistance < 10 && hasReset)
        if (GetPathRemainingDistance() < 10f && hasReset)
        {
            phase++;
            hasReset = false;
        }
        else
        {
            hasReset = true;
        }
        if (targetPosition.Count != 0)
        {
            target.position = targetPosition[phase % targetPosition.Count];
        }
    }
    void FixedUpdate()
    {
        if (!autoDriving) return;
        speed++;
        speed /= speedDivisor;
        if (rb != null)
        {
            move(transform.forward.normalized * speed);
        }
        // Vector3 targetDir = (agent.steeringTarget - transform.position).normalized;
        Vector3 targetDir = getTargetDir();

        if (targetDir != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(targetDir);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }
    }

    void move(Vector3 targetVelocity)
    {
        Vector3 currentVelocity = rb.linearVelocity;
        Vector3 horizontalVelocity = Vector3.Lerp(
            new Vector3(currentVelocity.x, 0f, currentVelocity.z),
            new Vector3(targetVelocity.x, 0f, targetVelocity.z),
            0.1f
        );

        rb.linearVelocity = new Vector3(horizontalVelocity.x, currentVelocity.y, horizontalVelocity.z);
    }

    Vector3 getTargetDir()
    {
        if (NavMesh.CalculatePath(transform.position, target.position, NavMesh.AllAreas, path))
        {
            // 経路のコーナーが2つ以上あれば、次の曲がり角への方向を出す
            if (path.corners.Length > 1)
            {
                Vector3 nextCorner = path.corners[1];
                Vector3 direction = nextCorner - transform.position;
                direction.y = 0;
                return direction.normalized;
            }
        }
        Vector3 direction2 = target.position - transform.position;
        direction2.y = 0;
        return direction2.normalized;
    }

    float GetPathRemainingDistance()
    {
        Vector3 diff = transform.position - target.transform.position;
        return diff.magnitude;
    }
}
