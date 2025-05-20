using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InRoomTrigger : MonoBehaviour
{
    public FollowingGuard guard;  // Assign the guard in the Inspector
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player position is in");
            guard.SetPlayerInRoom(true, other.transform);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Guard is off");
            guard.SetPlayerInRoom(false, other.transform);
        }
    }
}
