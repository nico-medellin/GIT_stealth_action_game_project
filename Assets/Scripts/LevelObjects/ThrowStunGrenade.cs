using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

using TMPro;

public class ThrowStunGrenade : MonoBehaviour
{
    public Transform cam; //transform of the camera
    public GameObject stunGrenadePrefab; // Assign the rock prefab in the Inspector
    public Transform throwPoint;  // A transform in front of the player to spawn the rock
    

    public GameObject player;
    private int grenadeInventory; // Added declaration for grenadeInventory

    public Animator charAnim; //Store the animator for the player


    //Throw variables
    public float throwForce = 20f;
    public float throwUpwardForce;
    public KeyCode throwKey = KeyCode.G; //G for grenade

    bool readyToThrow;
    public TextMeshProUGUI grenadeCount;
    private VGDPixelPioneersProject controls;
    private InputAction m_Grenade;

    private void Awake()
    {
        controls = new VGDPixelPioneersProject();
        m_Grenade = controls.Player.ThrowGrenade;  
    }

    private void OnEnable()
    {
        controls.Enable();
    }

    private void OnDisable()
    {
        controls.Disable();
    }
    void Start() 
    {
        grenadeInventory = player.GetComponent<PlayerController>().playerStunInventory;
        readyToThrow = true;
        charAnim = GetComponent<Animator>();
        //initialize the grenade count text
        grenadeCount.text = "Stun Grenades: " + grenadeInventory.ToString();


        

      
    }

    void Update()
    {
        // grenadeInventory = player.GetComponent<PlayerController>().grenadeInventory;
        Debug.DrawRay(throwPoint.transform.position, throwPoint.transform.forward * 5, Color.red, 2f);
        
        grenadeInventory = player.GetComponent<PlayerController>().playerStunInventory;
        // Update UI text
        grenadeCount.text = "Stun Grenades: " + grenadeInventory.ToString();


        if (m_Grenade.WasPressedThisFrame() && grenadeInventory > 0 && readyToThrow) // Change key as needed
        {
            readyToThrow = false;
            charAnim.SetTrigger("GrenadeThrow");
            Debug.Log("Grenade Throw button pressed!");
            readyToThrow = true; // Reset readyToThrow after throwing
            // Throw();
            
        }
    }

    public void ThrowStunGrenadeMotion()
    {
        if (stunGrenadePrefab != null && throwPoint != null && grenadeInventory > 0)
        {
            

            GameObject projectile = Instantiate(stunGrenadePrefab, throwPoint.position,Quaternion.identity); //remove rotation

            // Reset throwPoint back to original rotation
            // throwPoint.rotation = originalRotation; 
            Rigidbody projectileRb = projectile.GetComponent<Rigidbody>();

            //Attempting to add an offset of force to the projectile
            // Get the base forward direction from the throw point
            Vector3 baseDirection = throwPoint.transform.forward;

            // Vector3 offsetDirection = Quaternion.Euler(0, 0, 0) * baseDirection;


            // Vector3 forceToAdd = baseDirection * throwForce + transform.up * throwUpwardForce;

            // projectileRb.AddForce(forceToAdd, ForceMode.Impulse);


        //Only necessary if the animation effects the orientation of the throwPoint

            // Rotate the direction 30 degrees to the left around the up axis
            Vector3 offsetDirection = Quaternion.Euler(0, -20, 0) * baseDirection;
            
            // Calculate the final force with the rotated direction
            Vector3 forceToAdd = offsetDirection * throwForce + transform.up * throwUpwardForce;
            
            // Apply the force to the projectile
        
            projectileRb.AddForce(forceToAdd, ForceMode.Impulse);

        //End of the offset code


    

            grenadeInventory --; // Decrease grenadeInventory by 1
            player.GetComponent<PlayerController>().playerStunInventory = grenadeInventory; // Update grenadeInventory in PlayerController
            grenadeCount.text = "Stun Grenades: " + grenadeInventory.ToString();

        }
    }
}

