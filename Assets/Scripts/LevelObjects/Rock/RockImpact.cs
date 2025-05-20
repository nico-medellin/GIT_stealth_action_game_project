using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class RockImpact : MonoBehaviour
{
    // public GameObject player;
    private Rigidbody rb;

    public float distractionRadius = 10f;
    private bool hasLandedOnFloor = false;


    private bool targetHit;    

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // rb.isKinematic = false; //make sure to make the item not kinematic so it can move
        // if (player.GetComponent<PlayerController>().playerInventory == 0)
        // {
        //     Destroy(gameObject);
        // }
    }
    
    
    // void OnTriggerEnter(Collider other)
    // {
    //     //  if (!other.gameObject.CompareTag("Player")) // Ensure enemies have the "Enemy" tag
    //     // {
    //     //     Destroy(gameObject); // Destroy the rock on impact
    //     // }

    //     if (other.gameObject.CompareTag("Enemy")) // Ensure enemies have the "Enemy" tag
    //     {
    //         Destroy(gameObject); // Destroy the rock on impact
    //     }
    // }

    void OnCollisionEnter(Collision collision)
    {
        // Check if the collision normal is pointing upward (floor collision)
        // Using a small threshold to account for slightly uneven surfaces
        if (collision.contacts[0].normal.y > 0.9f && !hasLandedOnFloor)
        {
            hasLandedOnFloor = true;
            CreateDistractionPoint();
            Destroy(gameObject, 6f); // destroy rock after distraction
        }
    }

    private void CreateDistractionPoint()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, distractionRadius);
        foreach (Collider col in hitColliders)
        {
            var guard1 = col.GetComponent<BasicGuardPath>();
            if (guard1 != null)
            {
                guard1.DistractToPoint(transform.position);
                continue;
            }

            var guard2 = col.GetComponent<SimpleGuard>();
            if (guard2 != null)
            {
                guard2.DistractToPoint(transform.position);
            }
        }
    }



    //If we pivot away from using the Rock as a trigger we can switch back to use the code below.
    // void OnCollisionEnter(Collision collision)
    // {

    //     // make sure only to stick to the first target you hit
    //     if (targetHit)
    //         return;
    //     else
    //         targetHit = true;



    //     if (collision.gameObject.CompareTag("Enemy")) // Ensure enemies have the "Enemy" tag
    //     {

    //         Destroy(gameObject); // Destroy the rock on impact
    //     }

    // }
}

