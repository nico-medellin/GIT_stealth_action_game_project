using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

using TMPro;

public class ThrowRock : MonoBehaviour
{
    public Transform cam; //transform of the camera
    public GameObject rockPrefab; // Assign the rock prefab in the Inspector
    public Transform throwPoint;  // A transform in front of the player to spawn the rock
    

    public GameObject player;
    private int playerInventory; // Added declaration for playerInventory

    public Animator charAnim; //Store the animator for the player


    //Throw variables
    public float throwForce = 20f;
    public float throwUpwardForce;
    public KeyCode throwKey = KeyCode.F;

    bool readyToThrow;
    public TextMeshProUGUI inventoryCount;
    private VGDPixelPioneersProject controls;
    private InputAction m_RockThrow;

    private void Awake()
    {
        controls = new VGDPixelPioneersProject();
        m_RockThrow = controls.Player.ThrowRock;
        
    }
    void Start() 
    {
        playerInventory = player.GetComponent<PlayerController>().playerInventory;
        readyToThrow = true;
        charAnim = GetComponent<Animator>();
        // Initialize the UI text
        inventoryCount.text = "Throwing Rocks: " + playerInventory.ToString();

        

      
    }

    void Update()
    {
        // playerInventory = player.GetComponent<PlayerController>().playerInventory;
        Debug.DrawRay(throwPoint.transform.position, throwPoint.transform.forward * 5, Color.red, 2f);
        
        playerInventory = player.GetComponent<PlayerController>().playerInventory;
        // Update UI text
        inventoryCount.text = "Throwing Rocks: " + playerInventory.ToString();



        ThrowRockTrigger();
    }

    private void OnEnable()
    {
        controls.Enable();  // Turn on all input actions
    }

    private void OnDisable()
    {
        controls.Disable(); // Turn them off when the object is disabled or destroyed
    }

    private void ThrowRockTrigger()
    {
        if (m_RockThrow.WasPressedThisFrame() && playerInventory > 0 && readyToThrow) // Change key as needed
        {
            readyToThrow = false;
            charAnim.SetTrigger("Throw");
            Debug.Log("Throw button pressed!");
            readyToThrow = true; // Reset readyToThrow after throwing
            return;

        }
    }

    public void ThrowRockMotion()
    {
        // Add debug logging
        Debug.Log($"Rock prefab reference: {(rockPrefab != null ? "Valid" : "NULL")}");
        Debug.Log($"Throw point reference: {(throwPoint != null ? "Valid" : "NULL")}");

        if (rockPrefab != null && throwPoint != null && playerInventory > 0)
        {
            GameObject projectile = Instantiate(rockPrefab, throwPoint.position, Quaternion.identity);
            
            if (projectile != null)
            {
                Rigidbody projectileRb = projectile.GetComponent<Rigidbody>();
                if (projectileRb != null)
                {
                    Vector3 baseDirection = throwPoint.transform.forward;
                    Vector3 offsetDirection = Quaternion.Euler(0, -30, 0) * baseDirection;
                    Vector3 forceToAdd = offsetDirection * throwForce + transform.up * throwUpwardForce;
                    projectileRb.AddForce(forceToAdd, ForceMode.Impulse);
                }
                else
                {
                    Debug.LogError("Projectile has no Rigidbody component!");
                }
            }
            else
            {
                Debug.LogError("Failed to instantiate rock prefab!");
            }

            playerInventory--;
            player.GetComponent<PlayerController>().playerInventory = playerInventory;
            inventoryCount.text = "Throwing Rocks: " + playerInventory.ToString();
        }
        else
        {
            Debug.LogError($"Missing references - Rock Prefab: {rockPrefab}, Throw Point: {throwPoint}");
        }
    }
}
