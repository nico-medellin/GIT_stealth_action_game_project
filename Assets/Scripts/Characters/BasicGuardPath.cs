using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BasicGuardPath : MonoBehaviour
{
    public enum AIState {Walking, Falling, KnockedOut, GetUp, Distraction}
    public AIState currentState = AIState.Walking;

    //Distraction variables
    private Vector3 distractionPoint;
    private float distractionDuration = 3f;
    private float distractionTimer = 0f;

    public GameObject[] waypoints;
    private int currentWaypointIndex = 0;
    private NavMeshAgent agent;
    private Animator animator;
    private Rigidbody rb;

    public float knockedOutTime = 10f;
    private float knockoutTimer = 0f;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
        
        agent.updatePosition = false; // Prevent agent from moving manually
        agent.updateRotation = false; // Let animation control rotation

        if (waypoints.Length > 0)
        {
            agent.SetDestination(waypoints[currentWaypointIndex].transform.position);
        }
    }

    void Update()
    {
        switch(currentState)
        {
            case AIState.Walking:
                HandleWalking();
                break;
            case AIState.KnockedOut:
                HandleKnockOut();
                break;
            case AIState.Distraction:
                HandleDistraction();
                break;
        }

        // Set the animation speed based on NavMeshAgent movement
        animator.SetFloat("Speed", agent.velocity.magnitude);
        //Debug.Log("Current State: " + currentState);
    }

    void HandleWalking()
    {
        animator.SetBool("IsKnocked", false);
        rb.isKinematic = true;
        agent.enabled = true;
        rb.useGravity = false;
        agent.isStopped = false;
        
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
            agent.SetDestination(waypoints[currentWaypointIndex].transform.position);
        }

        if (agent.velocity.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(agent.velocity.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
        }
    }

    void HandleDistraction()
{
    if (agent.remainingDistance < 0.5f)
    {
        distractionTimer -= Time.deltaTime;
        if (distractionTimer <= 0)
        {
            currentState = AIState.Walking;
            agent.SetDestination(waypoints[currentWaypointIndex].transform.position);
        }
    }
}

    public void DistractToPoint(Vector3 point)
{
    if (currentState == AIState.Walking)
    {
        distractionPoint = point;
        agent.SetDestination(distractionPoint);
        distractionTimer = distractionDuration;
        currentState = AIState.Distraction;
    }
}


    void HandleKnockOut()
    {
        animator.SetBool("IsKnocked", true);
        agent.enabled = false;
        rb.isKinematic = false;
        rb.useGravity = true;
        

        knockoutTimer -= Time.deltaTime;

        if (knockoutTimer <= 0)
        {
            rb.isKinematic = true;
            agent.enabled = true;
            rb.useGravity = false;
            animator.applyRootMotion = true;
            currentState = AIState.Walking;
            agent.SetDestination(waypoints[currentWaypointIndex].transform.position);
            Debug.Log("Getting Up");
        }
    }

    void OnTriggerEnter(Collider c)
    {
        if(c.CompareTag("Rock"))
        {
            GetKnockedOut();
        }
    }
    public void GetKnockedOut()
    {
        if (currentState == AIState.Walking)
        {
            animator.applyRootMotion = false;
            currentState = AIState.KnockedOut;
            knockoutTimer = knockedOutTime;
        }
    }
    void OnAnimatorMove()
    {
        // Apply the root motion movement from the animation to the character
        transform.position = agent.nextPosition;
    }
    private bool AnimationFinished(string animationName)
    {
        //Debug.Log(animator.GetCurrentAnimatorStateInfo(0).IsName(animationName));
        return animator.GetCurrentAnimatorStateInfo(0).IsName(animationName) && animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f;
    }
}
