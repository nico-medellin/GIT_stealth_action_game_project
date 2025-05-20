using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class FollowingGuard : MonoBehaviour
{
    public enum GuardState { Idle, Chasing, Capture, KnockedOut, StandingUp, ResetBone }
    public GuardState currentState = GuardState.Idle;

    private NavMeshAgent agent;
    private Animator animator;
    private Transform player;
    private CharacterController characterController;
    private bool playerInRoom = false;
    private bool isCapturing = false;

    public GameObject[] waypoints;
    private int currentWaypointIndex = 0;
    public float agentSpeed = 2.0f;

    private Rigidbody[] knockedOutRigidbodies;
    private Collider[] knockedOutColliders;

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

    void Start()
    {
        agent.updatePosition = true;
        agent.updateRotation = true;
        animator.applyRootMotion = false;

        if (waypoints.Length > 0)
        {
            agent.SetDestination(waypoints[currentWaypointIndex].transform.position);
        }

        SetState(GuardState.Idle);
        player = null;
    }

    void Update()
    {
        if (isCapturing) return;

        switch (currentState)
        {
            case GuardState.Idle:
                HandleIdleState();
                break;

            case GuardState.Chasing:
                HandleChasingState();
                break;
            case GuardState.KnockedOut: // Handles when the character is knocked out
                HandleKnockedOut();
                break;
            case GuardState.ResetBone: // Tries to reset the position for the character from knocked out to stand up animation
                ResetBones();
                break;
            case GuardState.StandingUp: // Handles when the character is trying to stand up
                HandleStandingUp();
                break;
        }

        animator.SetFloat("Speed", agent.velocity.magnitude);
    }
    private void OnAnimatorMove()
    {
        if (currentState == GuardState.Capture) return;
        if (currentState == GuardState.Idle) return;
        Vector3 rootMotionMovement = animator.deltaPosition; // Get movement from animation
        agent.nextPosition = transform.position + rootMotionMovement;
    }

    private void HandleIdleState()
    {
        if (playerInRoom)
        {
            SetState(GuardState.Chasing);
        }
    }

    private void HandleChasingState()
    {
        if (agent.enabled)
        {
            agent.nextPosition = transform.position;

            if (playerInRoom == false)
            {
                if (!agent.pathPending && agent.remainingDistance < 0.5f)
                {
                    SetState(GuardState.Idle);
                }
                else
                {
                    currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
                    agent.SetDestination(waypoints[currentWaypointIndex].transform.position);
                }

            }
            else
            {
                agent.SetDestination(player.position);
            }

            if (agent.velocity.magnitude > 0.1f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(agent.velocity.normalized);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
            }
        }
    }

    private void SetState(GuardState newState)
    {
        if (currentState == newState) return;

        currentState = newState;

        switch (newState)
        {
            case GuardState.Idle:
                animator.SetBool("IsChasing", false);
                animator.SetBool("IsCapturing", false);
                agent.speed = agentSpeed / 2;
                break;

            case GuardState.Chasing:
                animator.SetBool("IsChasing", true);
                animator.SetBool("IsCapturing", false);
                agent.speed = agentSpeed;
                break;

            case GuardState.Capture:
                animator.SetBool("IsChasing", false);
                animator.SetBool("IsCapturing", true);
                agent.isStopped = true;
                isCapturing = true;
                StartCoroutine(EndCapture());
                break;
        }
    }

    private void DisableKnockedOut()
    {
        Debug.Log("Calling disable knocked out on guard");
        foreach (var col in knockedOutColliders)
        {
            col.enabled = true;
        }
        /*
        foreach (var rigidbody in knockedOutRigidbodies)
        {
            rigidbody.isKinematic = true;
        }*/
        animator.enabled = true;
        agent.enabled = true;
        characterController.enabled = true;
    }

    public void EnableKnockedOut()
    {
        Debug.Log("Calling enable knocked out on guard");
        animator.enabled = false;
        foreach (var col in knockedOutColliders)
        {
            col.enabled = false;
        }/*
        foreach (var rigidbody in knockedOutRigidbodies)
        {
            rigidbody.isKinematic = false;
        }*/

        agent.enabled = false;
        characterController.enabled = false;

    }

    private void OnTriggerEnter(Collider c)
    {

        
        if (c.gameObject.CompareTag("Stun"))
        {
            Rigidbody rb = c.attachedRigidbody;

            if (rb != null)
            {
                Vector3 force = rb.mass * rb.velocity;
                Rigidbody hitHead = null;
                foreach (Rigidbody parts in knockedOutRigidbodies)
                {
                    if (parts.gameObject.name.Contains("head"))
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
            currentState = GuardState.KnockedOut;
            Debug.Log("Stun Grenade Hit");
            knockoutTime = Random.Range(minKnockoutTime, maxKnockoutTime);
        }
        if (c.gameObject.CompareTag("Player"))
        {
            //player = c.transform;
            //playerInRoom = true;
            SetState(GuardState.Capture);
        }
    }
    private void HandleKnockedOut()
    {
        knockoutTime -= Time.deltaTime;
        if (knockoutTime <= 0)
        {
            AlignPosToHips();

            FindBonePosition(knockedOutBonesPosition, knockedOutBonesRotation);

            currentState = GuardState.ResetBone;
            boneResetTime = 0;
        }
    }

    private void HandleStandingUp()
    {
        if (animator.GetCurrentAnimatorStateInfo(0).IsName(standupState) == false)
        {
            SetState(GuardState.Idle);
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
            currentState = GuardState.StandingUp;
            DisableKnockedOut();

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

    public void SetPlayerInRoom(bool isInside, Transform p)
    {
        player = p;
        playerInRoom = isInside;
        /*if (isInside)
        {
            SetState(GuardState.Chasing);
        }
        else
        {
            SetState(GuardState.Idle);
        }*/
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player") && !isCapturing)
        {
            SetState(GuardState.Capture);
        }
    }

    private IEnumerator EndCapture()
    {
        yield return new WaitForSeconds(2f); // Adjust to match capture animation duration
        isCapturing = false;
        agent.isStopped = false;
        SetState(GuardState.Idle);
    }
}

