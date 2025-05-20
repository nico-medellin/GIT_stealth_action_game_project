using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class PrisonerController : MonoBehaviour
{
    public GameObject[] waypoints;
    NavMeshAgent agent;
    bool freedByPlayer = false;
    private Animator animator;
    private int currentWaypointIndex = 0;
    public GameObject exitWaypoint;

    public enum AIState
    {
        Freed,
        WaitingForPlayer
    }

    public AIState aiState;

    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        agent.updateRotation = true;
        aiState = AIState.WaitingForPlayer;

        animator.applyRootMotion = true;
        if (waypoints.Length > 0)
        {
            Debug.Log("Waypoints assigned: " + waypoints.Length);
            agent.SetDestination(waypoints[currentWaypointIndex].transform.position);
        }
        else
        {
            Debug.LogError("No waypoints assigned!");
        }
    }

    // Update is called once per frame
    void Update()
    {
        // keep wandering around until the player talks to them.
        if (freedByPlayer) {
            // update state
            aiState = AIState.Freed;
        } else {
            // walk back and forth
            aiState = AIState.WaitingForPlayer;
        }

        switch (aiState) {
            case AIState.Freed:
                agent.SetDestination(exitWaypoint.transform.position);
                HandleFreed();
                break;
            case AIState.WaitingForPlayer:
                HandleWalking();
                break;
        }
    }

    public void OnDoorOpened()
    {
        freedByPlayer = true;
    }

    private void HandleWalking()
    {
        animator.speed = .6f;

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

    private void HandleFreed()
    {
        animator.speed = 2.0f;
        
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            gameObject.SetActive(false);
        }

        if (agent.velocity.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(agent.velocity.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
        }
    }
}
