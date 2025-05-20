using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SimpleGuard : MonoBehaviour
{
    public enum AIState { Walking, KnockedOut, StandingUp, ResetBone, Distraction}
    public AIState currentstate = AIState.Walking;
    private Rigidbody[] knockedOutRigidbodies;
    private Collider[] knockedOutColliders;
    public GameObject fieldOfVision;
    private Collider fovCollider;

    public GameObject[] waypoints;
    private int currentWaypointIndex = 0;
    public float agentSpeed = 2.0f;
    private NavMeshAgent agent;
    private Animator animator;
    private CharacterController characterController;

    private float knockoutTime;
    private Transform hipBone;

    [SerializeField]
    public string standupState;

    [SerializeField]
    public string standupClip;

    public float minKnockoutTime = 5f;
    public float maxKnockoutTime = 15f;

    private Transform[] bones;
    private Vector3[] standupBonesPosition;
    private Quaternion[] standupBonesRotation;
    private Vector3[] knockedOutBonesPosition;
    private Quaternion[] knockedOutBonesRotation;

    private float boneResetTime;

    private Vector3 distractionPoint;
    private float distractionTimer;
    private float distractionDuration = 3f;


    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        characterController = GetComponent<CharacterController>();
        knockedOutRigidbodies = GetComponentsInChildren<Rigidbody>();
        knockedOutColliders = GetComponentsInChildren<Collider>();
        hipBone = animator.GetBoneTransform(HumanBodyBones.Hips);
        bones = hipBone.GetComponentsInChildren<Transform>();
        standupBonesPosition = new Vector3[bones.Length];
        knockedOutBonesPosition = new Vector3[bones.Length];
        if (fieldOfVision != null)
        {
            fovCollider = fieldOfVision.GetComponent<Collider>();
        }
        for (int i = 0; i < bones.Length; i++)
        {
            standupBonesPosition[i] = new Vector3();
            knockedOutBonesPosition[i] = new Vector3();

        }

        standupBonesRotation = new Quaternion[bones.Length];
        knockedOutBonesRotation = new Quaternion[bones.Length];
        for (int i = 0; i < bones.Length; i++)
        {
            standupBonesRotation[i] = new Quaternion();
            knockedOutBonesRotation[i] = new Quaternion();

        }

        FindAnimationBonePosition(standupClip, standupBonesPosition, standupBonesRotation);

        DisableKnockedOut();
    }
    // Start is called before the first frame update
    void Start()
    {
        agent.updatePosition = false;
        agent.updateRotation = true;
        animator.applyRootMotion = true;
        // if (waypoints.Length > 0)
        // {
        //     Debug.Log("Waypoints assigned: " + waypoints.Length);
        //     agent.SetDestination(waypoints[currentWaypointIndex].transform.position);
            
        // }
        if (waypoints.Length > 0) 
        {
            Vector3 waypointPos = waypoints[currentWaypointIndex].transform.position;
            NavMeshHit hit;
            if (NavMesh.SamplePosition(waypointPos, out hit, 2.0f, NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);
            }
            else
            {
                Debug.LogWarning("Waypoint not reachable on NavMesh.");
            }
        }

        else
        {
            Debug.LogError("No waypoints assigned!");
        }
    }

    // Update is called once per frame
    void Update()
    {
        switch (currentstate)
        {
            case AIState.Walking: // Handles the Character Walking
                HandleWalking();
                break;
            case AIState.KnockedOut: // Handles when the character is knocked out
                HandleKnockedOut();
                break;
            case AIState.ResetBone: // Tries to reset the position for the character from knocked out to stand up animation
                ResetBones();
                break;
            case AIState.StandingUp: // Handles when the character is trying to stand up
                HandleStandingUp();
                break;
            case AIState.Distraction:
                HandleDistraction();
                break;
            
        }
    }

    private void HandleDistraction()
    {
        // animator.applyRootMotion = false; // <- disable root motion here

        if (agent.remainingDistance < 0.5f)
        {
            distractionTimer -= Time.deltaTime;
            if (distractionTimer <= 0)
            {
                currentstate = AIState.Walking;
                // agent.SetDestination(waypoints[currentWaypointIndex].transform.position); //Old walking logic
                Vector3 waypointPos = waypoints[currentWaypointIndex].transform.position;
                NavMeshHit hit;
                if (NavMesh.SamplePosition(waypointPos, out hit, 2.0f, NavMesh.AllAreas))
                {
                    agent.SetDestination(hit.position);
                }
                else
                {
                    Debug.LogWarning("Waypoint not reachable on NavMesh.");
                }
            }
        }
    }

    public void DistractToPoint(Vector3 point)
    {
        if (currentstate == AIState.Walking) // or currentstate for SimpleGuard
        {
            NavMeshHit hit;
            if (NavMesh.SamplePosition(point, out hit, 2.0f, NavMesh.AllAreas))
            {
                
                distractionPoint = hit.position;
                Debug.DrawLine(transform.position, distractionPoint, Color.yellow, 2f);

                agent.SetDestination(distractionPoint);
                distractionTimer = distractionDuration;
                currentstate = AIState.Distraction; // or currentstate = AIState.Distraction;
            }
            else
            {
                Debug.LogWarning("Distraction point not reachable on NavMesh.");
            }
        }


        // if (currentstate == AIState.Walking)
        // {
        //     distractionPoint = point;
        //     distractionTimer = distractionDuration;
        //     agent.SetDestination(distractionPoint);
        //     currentstate = AIState.Distraction;
        // }
    }
    private void OnAnimatorMove()
    {
        Vector3 rootPosition = animator.rootPosition;
        rootPosition.y = agent.nextPosition.y;
        transform.position = rootPosition;
        agent.nextPosition = rootPosition;
    }
    private void OnTriggerEnter(Collider c)
    
    {
        if(c.gameObject.CompareTag("Stun"))
        {
            Rigidbody rb = c.attachedRigidbody;

            if (rb != null)
            {
                Vector3 force = rb.mass * rb.velocity;
                Rigidbody hitHead = null;
                foreach (Rigidbody parts in knockedOutRigidbodies)
                {
                    if (parts.gameObject.name.Contains("Head"))
                    {
                        hitHead = parts;
                        break;
                    }
                }
                Vector3 hitpoint = c.ClosestPoint(hitHead.position);
 

                if (hitHead != null)
                {
                    Debug.Log("Hit in the head");
                    hitHead.AddForceAtPosition(force, hitpoint, ForceMode.Impulse);
                }
                
            }

            EnableKnockedOut();
            currentstate = AIState.KnockedOut;
            Debug.Log("Stun Hit");
            knockoutTime = Random.Range(minKnockoutTime, maxKnockoutTime);
        }

        if(c.gameObject.CompareTag("Player"))
        {
            Debug.Log("Player Hit!");
        }
    }

    private void DisableKnockedOut()
    {
        foreach (var col in knockedOutColliders)
        {
            col.enabled = true;
        }
        fovCollider.enabled = true;
        /*
        foreach (var rigidbody in knockedOutRigidbodies)
        {
            rigidbody.isKinematic = true;
        }*/
        animator.enabled = true;
        agent.enabled = true;
        characterController.enabled = true;
    }

    private void EnableKnockedOut()
    {
        animator.enabled = false;
        foreach (var col in knockedOutColliders)
        {
            col.enabled = false;
        }
        fovCollider.enabled = false;
        /*
        foreach (var rigidbody in knockedOutRigidbodies)
        {
            rigidbody.isKinematic = false;
        }*/

        agent.enabled = false;
        characterController.enabled = false;

    }

    private void HandleWalking()
    {
        // animator.applyRootMotion = false; // <- disable root motion here

        /*Vector3 velocity = animator.deltaPosition / Time.deltaTime;
        agent.velocity = velocity;*/
        animator.speed = 1.2f;
        agent.speed = agentSpeed;
        
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
            // agent.SetDestination(waypoints[currentWaypointIndex].transform.position);
            Vector3 waypointPos = waypoints[currentWaypointIndex].transform.position;
            NavMeshHit hit;
            if (NavMesh.SamplePosition(waypointPos, out hit, 2.0f, NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);
            }
            else
            {
                Debug.LogWarning("Waypoint not reachable on NavMesh.");
            }
        }

        if (agent.velocity.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(agent.velocity.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
        }
        animator.SetFloat("Speed", agent.velocity.magnitude);

    }

    private void HandleKnockedOut()
    {
        knockoutTime -= Time.deltaTime;
        if(knockoutTime <= 0)
        {
            AlignPosToHips();

            FindBonePosition(knockedOutBonesPosition, knockedOutBonesRotation);

            currentstate = AIState.ResetBone;
            boneResetTime = 0;
        }
    }

    private void HandleStandingUp()
    {
        if (animator.GetCurrentAnimatorStateInfo(0).IsName(standupState) == false)
        {
            currentstate = AIState.Walking;
        }
    }

    private void ResetBones()
    {
        boneResetTime += Time.deltaTime;

        float resetProgress = boneResetTime / 1;
        
        for (int i = 0; i < bones.Length; i++)
        {
            bones[i].localPosition = Vector3.Lerp(knockedOutBonesPosition[i], standupBonesPosition[i], resetProgress);
            bones[i].localRotation = Quaternion.Lerp(knockedOutBonesRotation[i], standupBonesRotation[i], resetProgress);
        }

        if (resetProgress >= 1)
        {
            currentstate = AIState.StandingUp;
            DisableKnockedOut();
            // animator.applyRootMotion = true; // <- only needed for stand-up animation

            animator.Play(standupState);
        }

    }

    private void AlignPosToHips()
    {
        Vector3 originalHips = hipBone.position;
        transform.position = hipBone.position;

        Vector3 offset = standupBonesPosition[0];
        offset.y = 0;
        offset = transform.rotation * offset;
        transform.position -= offset;

        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hitInfo))
        {
            transform.position = new Vector3(transform.position.x, hitInfo.point.y, transform.position.z);
        }

        hipBone.position = originalHips;
    }

    private void FindBonePosition(Vector3[] bonePositions, Quaternion[] boneRotations)
    {
        for (int i = 0; i < bones.Length; i++)
        {
            bonePositions[i] = bones[i].localPosition;
            boneRotations[i] = bones[i].localRotation;
        }
    }

    private void FindAnimationBonePosition(string clipname, Vector3[] bonePositions, Quaternion[] boneRotations)
    {
        Vector3 positionBeforeSample = transform.position;
        Quaternion rotationBeforeSample = transform.rotation;
        foreach (var clip in animator.runtimeAnimatorController.animationClips)
        {
            if (clip.name == clipname)
            {
                clip.SampleAnimation(gameObject, 0);
                FindBonePosition(bonePositions, boneRotations);
                break;
            }
        }
        transform.position = positionBeforeSample;
        transform.rotation = rotationBeforeSample;
    }
}

