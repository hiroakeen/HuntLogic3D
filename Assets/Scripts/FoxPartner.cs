using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class FoxPartner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private float followDistance = 5f;
    [SerializeField] private float detectionRadius = 30f;
    [SerializeField] private float orbitDistance = 3f;
    [SerializeField] private float orbitSpeed = 2f;
    [SerializeField] private float runThreshold = 15f;
    [SerializeField] private float walkThreshold = 7f;
    [SerializeField] private float maxGuideDistance = 20f;
    [SerializeField] private float stopDistanceFromAnimal = 1.5f;

    private NavMeshAgent agent;
    private Animator animator;
    private Transform nearestAnimal;
    private float orbitAngle = 0f;
    private float idleTimer = 0f;
    private bool hasStartedGuiding = false;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        HandleInvalidAnimal();

        animator.SetFloat("MoveSpeed", agent.velocity.magnitude);

        nearestAnimal = FindNearestAnimal();
        if (!hasStartedGuiding && nearestAnimal != null)
        {
            HandleIdleBeforeGuide();
            return;
        }

        DecideBehavior();
    }



    private void GuideToAnimal()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        if (distanceToPlayer > maxGuideDistance)
        {
            FollowPlayerIdle();
            return;
        }

        if (nearestAnimal != null)
        {
            float distanceToAnimal = Vector3.Distance(transform.position, nearestAnimal.position);

            if (distanceToAnimal > runThreshold)
            {
                agent.speed = 6f;
            }
            else if (distanceToAnimal > walkThreshold)
            {
                agent.speed = 3f;
            }
            else if (distanceToAnimal <= stopDistanceFromAnimal)
            {
                agent.ResetPath();
                return;
            }

            // 動物の方へ向かう
            Vector3 targetPos = Vector3.Lerp(player.position, nearestAnimal.position, 0.85f);
            agent.SetDestination(targetPos);
        }
    }


    private void FollowPlayerIdle()
    {
        float distance = Vector3.Distance(transform.position, player.position);
        if (distance > followDistance)
        {
            agent.SetDestination(player.position);
            agent.speed = 3f;
        }
        else
        {
            agent.ResetPath();
        }
    }

    private void OrbitAroundAnimal()
    {
        orbitAngle += orbitSpeed * Time.deltaTime;
        Vector3 offset = new Vector3(Mathf.Cos(orbitAngle), 0, Mathf.Sin(orbitAngle)) * orbitDistance;
        Vector3 orbitTarget = nearestAnimal.position + offset;

        if (NavMesh.SamplePosition(orbitTarget, out var hit, 2f, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
            agent.speed = 2f;
        }
        else
        {
            agent.ResetPath();
        }
    }

    private void OrbitAroundPlayer()
    {
        orbitAngle += orbitSpeed * Time.deltaTime;
        Vector3 offset = new Vector3(Mathf.Cos(orbitAngle), 0, Mathf.Sin(orbitAngle)) * followDistance;
        Vector3 orbitTarget = player.position + offset;

        if (NavMesh.SamplePosition(orbitTarget, out var hit, 2f, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
            agent.speed = 2f;
        }
        else
        {
            agent.ResetPath();
        }
    }

    private Transform FindNearestAnimal()
    {
        GameObject[] animals = GameObject.FindGameObjectsWithTag("Animal");
        Transform closest = null;
        float minDistance = detectionRadius;

        foreach (GameObject animal in animals)
        {
            if (!animal.activeInHierarchy) continue;

            float dist = Vector3.Distance(transform.position, animal.transform.position);
            if (dist < minDistance)
            {
                minDistance = dist;
                closest = animal.transform;
            }
        }
        return closest;
    }

    private void HandleInvalidAnimal()
    {
        if (nearestAnimal != null && !nearestAnimal.gameObject.activeInHierarchy)
        {
            nearestAnimal = null;
            hasStartedGuiding = false;
        }
    }

    private void HandleIdleBeforeGuide()
    {
        idleTimer += Time.deltaTime;
        if (idleTimer >= 2f)
        {
            hasStartedGuiding = true;
        }
        else
        {
            agent.ResetPath();
        }
    }

    private void DecideBehavior()
    {
        if (nearestAnimal == null)
        {
            OrbitAroundPlayer();
            return;
        }

        float distanceToAnimal = Vector3.Distance(transform.position, nearestAnimal.position);
        if (distanceToAnimal < orbitDistance + 1f)
        {
            OrbitAroundAnimal();
        }
        else
        {
            GuideToAnimal();
        }
    }

}
