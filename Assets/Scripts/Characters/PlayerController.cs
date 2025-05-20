using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

using DG.Tweening;
public class PlayerController : MonoBehaviour
{
    // Button & Door Interaction
    public float interactionDistance = 2f; // Distance to check for button
    public string buttonTag = "Button"; // Tag for buttons
    public string doorTag = "Door"; // Tag for doors
    
    private DoorController doorController;
    private Transform buttonTarget = null; // Button that the hand will move to
    private bool isPressingButton = false; // Track hand movement
    private float currentIKWeight = 0f;
    public float ikWeightSpeed = 2f;  // How fast the hand moves (adjust in Inspector)
    // laptop interaction
    public string laptopTag = "Laptop";
    private Transform laptopTarget = null; // Laptop that hands will move to
     private float currentIKLaptopWeight = 0f;
     public float ikLaptopWeightSpeed = 2f;  // How fast the hand moves (adjust in Inspector)
    private bool isPressingLaptop = false;
    // Menu
    public GameObject goalNotFinishedText;
    public GameObject doorText;
    public GameObject nextLevelText;
    public GameObject quitGameText;
    private int currentScene;

    // Rock Pickup
    public GameObject pickupText;
    public GameObject throwableText;
    private GameObject toBeDestroyed;
    private const int MAX_ROCK_THROWABLES = 2;
    private const int MAX_STUN_THROWABLES = 2;
    private const float pickupRange = 6f;
    private VGDPixelPioneersProject defaultInput;

    private InputAction m_FireAction;


    // Rock Throw
    // public GameObject rock;
    public Transform rightHandTarget; //IK target for the right hand

    // Prisoner
    public TextMeshProUGUI prisonerCount;
    public int prisonersRemaining;

    // Character Setup
    public Transform spawnPoint;
    public GameObject playerHand;
    public int playerInventory;
    public int playerStunInventory;
    public TextMeshProUGUI inventoryCount;
    public TextMeshProUGUI inventoryStunCount;
    public Camera playerCamera;
    public LayerMask rockItemLayer;
    public LayerMask stunItemLayer;
    private CharacterController controller;
    private Animator charAnim;
    private bool groundedPlayer;
    private float gravityValue = -9.81f;
    private Vector3 playerVelocity;
    private AudioSource rockAudioSource;
    private AudioSource stunAudioSource;
    public bool gameOver = false;

    // Vaulting animation
    public Transform jumpPosition;
    private float vaultAnimDuration;
    private bool isVaulting = false;
    private Vector3 vaultDirection;
  
    [SerializeField] private Transform landingTarget;
    [SerializeField] private AnimationClip vaultClip;
    [SerializeField] private LayerMask vaultableLayer;
    [SerializeField] private float vaultCheckDistance = 2f;

    // Start is called before the first frame update.
    void Start()
    {
        // Get and store the Rigidbody component attached to the player.
        controller = GetComponent<CharacterController>();
        charAnim = GetComponent<Animator>();
        AudioSource[] audioSources = GetComponents<AudioSource>();
        rockAudioSource = audioSources[0];
        stunAudioSource = audioSources[1];

        ResetScreen();

        // For pickup (F or A button)
        defaultInput = new VGDPixelPioneersProject();
        m_FireAction = defaultInput.Player.Fire;
        defaultInput.Player.Fire.performed += c => StartCoroutine(PickupAction());
        defaultInput.Player.Fire.canceled += c => StopAllCoroutines(); // ensure a single coroutine is running even if fire is re-performed within nextFire seconds
        defaultInput.Enable();

        currentScene = SceneManager.GetActiveScene().buildIndex;
    

        //For vaulting
        vaultAnimDuration = vaultClip.length;
        Debug.Log("Vault animation duration: " + vaultAnimDuration);
        if (landingTarget == null)
        {
            GameObject landingObj = new GameObject("LandingTarget");
            landingTarget = landingObj.transform;
        }
    }

    public void ResetScreen()
    {
        pickupText.SetActive(false);
        throwableText.SetActive(false);
        nextLevelText.SetActive(false);
        goalNotFinishedText.SetActive(false);
        doorText.SetActive(false);
        quitGameText.SetActive(false);

        inventoryCount.text = "Throwing Rocks: 0";
        playerInventory = 0;
        inventoryStunCount.text = "Stun Grenades: 0";
        playerStunInventory = 0;
        prisonerCount.text = "Prisoners Remaining: " + prisonersRemaining;
    }

    // FixedUpdate is called once per fixed frame-rate frame.
    private void Update() 
    {
        groundedPlayer = controller.isGrounded;
        if (groundedPlayer && playerVelocity.y < 0)
        {
            playerVelocity.y = 0f;
        }

        charAnim.SetFloat("X", Input.GetAxis("Horizontal"));
        charAnim.SetFloat("Y", Input.GetAxis("Vertical"));
        if (!Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.W))
        {
            charAnim.SetFloat("Y", 0.5f);
        }
        transform.Rotate(Vector3.up, Input.GetAxis("Horizontal") * 100f * Time.deltaTime);

        playerVelocity.y += gravityValue * Time.deltaTime;
        controller.Move(playerVelocity * Time.deltaTime);

        Pickup();
        inventoryCount.text = "Throwing Rocks: " + playerInventory.ToString();
        inventoryStunCount.text = "Stun Grenades: " + playerStunInventory.ToString();
        if (toBeDestroyed == null)
        {
            pickupText.SetActive(false);
        }

        CheckForButtonInteraction();
       
        Vault();

        CheckForLaptopInteraction();
    }


    void AlignToVaultPoint(Transform jump_position)
    {
        Vector3 toJump = jump_position.position - transform.position;
        toJump.y = 0f;
        Vector3 vaultDir = toJump.normalized;

        // Don't change position � just rotate
        transform.rotation = Quaternion.LookRotation(vaultDir, Vector3.up);

        // Store direction for arc
        vaultDirection = vaultDir;

    }


    private Transform FindNearestVaultable()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, vaultCheckDistance, vaultableLayer);

        Transform closest = null;
        float closestDist = Mathf.Infinity;

        foreach (var hit in hits)
        {
            float dist = Vector3.Distance(transform.position, hit.transform.position);
            if (dist < closestDist)
            {
                closest = hit.transform;
                closestDist = dist;
            }
        }

        return closest;
    }

    //This is for troubleshooting
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, vaultCheckDistance);


    }

    //This is for troubleshooting vaulting issues for landing and jumpPosition(height)
    private void OnDrawGizmos()
    {
        if (jumpPosition != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawSphere(jumpPosition.position, 0.05f);

            Gizmos.color = Color.green;
            Gizmos.DrawRay(jumpPosition.position, jumpPosition.up * 0.2f);

            Gizmos.color = Color.blue;
            Gizmos.DrawRay(jumpPosition.position, jumpPosition.forward * 0.2f);
        }
        if (landingTarget != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(landingTarget.position, 0.1f); // visualize the landing point

            // Optional: draw a line from jumpPosition to landingTarget
            if (jumpPosition != null)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawLine(jumpPosition.position, landingTarget.position);
            }
        }
    }
    void Vault()
    {

        if ((Input.GetKeyDown(KeyCode.Space) || Input.GetButtonDown("Jump"))&& !isVaulting)
        {
            //transform.DoMove(transform.position + controller.forward * 2, 1f).SetEase(Ease.InOutCubic);
            Transform vaultTarget = FindNearestVaultable();


            if (vaultTarget != null)
            {
                Transform handTarget = vaultTarget.Find("vault_hand_target");
                Transform heightTarget = vaultTarget.Find("vaultheight");
                if (handTarget == null || heightTarget == null)
                {
                    Debug.LogWarning("Missing jump_position or vaultheight on " + vaultTarget.name);
                    return;
                }

                jumpPosition = handTarget; // For hand IK


                AlignToVaultPoint(handTarget); // rotate to face vault direction
                isVaulting = true;

                // Trigger animation
                charAnim.SetTrigger("Vault");

                // Get vault animation duration
                float animDuration = vaultClip.length;

                // Calculate timing based on animation
                float peakTime = 62f / vaultClip.frameRate;
                float jumpUpTime = peakTime;   // to peak
                float jumpDownTime = animDuration - peakTime; // to land

                // Final target position
                Vector3 vaultTargetPos = heightTarget.position;

                // Arc height offset
                float jumpHeight = Mathf.Max(0.2f, vaultTargetPos.y - transform.position.y + 0.5f);

                // Build jump sequence
                DG.Tweening.Sequence vaultSequence = DOTween.Sequence();

                // Optional: slow time for dramatic effect
                //Time.timeScale = 0.5f;
                //Time.fixedDeltaTime = 0.02f * Time.timeScale;

                // Go up + forward
                vaultSequence.Append(transform.DOMove(heightTarget.position, .6f)); 

                // Then come down
                CalculateLandingTarget(); // dynamic landing point
               
                vaultSequence.Append(transform.DOMove(landingTarget.position, .3f));//jumpDownTime));//.SetEase(Ease.InSine));
                //vaultSequence.Append(transform.DOMoveY(vaultTargetPos.y, jumpDownTime).SetEase(Ease.InSine));

                // Disable controller while vaulting
                vaultSequence.OnStart(() => controller.enabled = false);

                vaultSequence.OnComplete(() =>
                {
                    controller.enabled = true;
                    isVaulting = false;
                    //Time.timeScale = 1f;
                    //Time.fixedDeltaTime = 0.02f;
                    Debug.Log("Vault complete");
                });

            }
        }

    }

    void LateUpdate()
    {
        AnimatorStateInfo state = charAnim.GetCurrentAnimatorStateInfo(0);

        if (isVaulting && !state.IsName("Vault"))
        {
            isVaulting = false;
        }
    }

    void OnTriggerEnter(Collider other) 
    {
        if (other.gameObject.CompareTag("Enemy")) 
        {   

        // Try to get any type of guard component
        SimpleGuard simpleGuard = other.gameObject.GetComponentInParent<SimpleGuard>();
        FollowingGuard followingGuard = other.gameObject.GetComponentInParent<FollowingGuard>();
        EnemyController enemyController = other.gameObject.GetComponentInParent<EnemyController>();

        // Check SimpleGuard
        if (simpleGuard != null && simpleGuard.currentstate == SimpleGuard.AIState.Walking)
        {
            Transform guardEye = simpleGuard.transform; // You can change this to a head transform if needed
            Vector3 directionToPlayer = (transform.position - guardEye.position).normalized;
            float distanceToPlayer = Vector3.Distance(guardEye.position, transform.position);

            // Check for obstacles between guard and player
            if (Physics.Raycast(guardEye.position, directionToPlayer, out RaycastHit hit, distanceToPlayer))
            {
                if (hit.collider.CompareTag("Player"))
                {
                    Debug.Log($"Caught by SimpleGuard (LOS confirmed). State: {simpleGuard.currentstate}");
                    gameOver = true;
                    return;
                }
                else
                {
                    Debug.Log("Player is hidden behind an object. No LOS.");
                }
            }
        }


        // Check FollowingGuard
        if (followingGuard != null && (followingGuard.currentState == FollowingGuard.GuardState.Idle
                                       || followingGuard.currentState == FollowingGuard.GuardState.Chasing))
        {
            Debug.Log($"Caught by FollowingGuard in state: {followingGuard.currentState}");
            gameOver = true;
            return;
        }

            // // First try to get the SimpleGuard component
            // SimpleGuard guard = other.gameObject.GetComponent<SimpleGuard>();

            // if (guard != null && guard.currentstate != SimpleGuard.AIState.KnockedOut)
            // {
            //     Debug.Log($"Collided with guard in state: {guard.currentstate}");
            //     gameOver = true;
            // }


            // First try to get the SimpleGuard component from this object
            // SimpleGuard guard = other.gameObject.GetComponent<SimpleGuard>();
            
            // If not found, try to get it from the parent
            // if (guard == null)
            // {
            //     guard = other.gameObject.GetComponentInParent<SimpleGuard>();
            // }

            // if (guard != null && guard.currentstate != SimpleGuard.AIState.Walking)
            // {
            //     Debug.Log($"Collided with guard's field of vision. Guard state: {guard.currentstate}");
            // }

            // if (guard != null && guard.currentstate == SimpleGuard.AIState.Walking)
            // {
            //     Debug.Log($"Collided with guard's field of vision. Guard state: {guard.currentstate}");
            //     gameOver = true;
            // }

        }

        if (other.gameObject.CompareTag("FinishLine") && prisonersRemaining == 0) 
        {
            DisplayFinishLineText(true);
        }

        if (other.gameObject.CompareTag("FinishLine") && prisonersRemaining > 0) 
        {
            DisplayNotDoneYet(true);
        }

        if (other.gameObject.CompareTag("Door")) {
            DisplayDoorText(true);
        }
    
    
    }

    void OnTriggerExit(Collider other) 
    {
        if (other.gameObject.CompareTag("FinishLine") && prisonersRemaining > 0) 
        {
            DisplayNotDoneYet(false);
        }
    }

    void Pickup()
    {
        Ray ray = playerCamera.ScreenPointToRay(new Vector2(Screen.width/2, Screen.height/2));
        RaycastHit rayHit;
        if (Physics.Raycast(ray, out rayHit, pickupRange, rockItemLayer))
        {
            if (rayHit.collider.CompareTag("Rock") && rayHit.collider.transform.position.y < 0.5)
            {
                toBeDestroyed = rayHit.collider.gameObject;
                if (rayHit.collider != null)
                {
                    if (playerInventory >= MAX_ROCK_THROWABLES)
                    {
                        pickupText.SetActive(false);
                        throwableText.SetActive(true);
                    }
                    else
                    {
                        pickupText.SetActive(true);
                        throwableText.SetActive(false);
                    }
                }
            }
        }
        else if (Physics.Raycast(ray, out rayHit, pickupRange, stunItemLayer))
        {
            if (rayHit.collider.CompareTag("Stun") && rayHit.collider.transform.position.y < 0.5)
            {
                // //Attempting to debug an invisible stun grenade
                // Debug.Log($"Detected stun grenade: {rayHit.collider.gameObject.name} at position {rayHit.collider.transform.position}");
                // //The issue was that the stun grenade had a massive radius and was triggering the player through a wall.
                // //Return the players current location
                // Debug.Log($"Player position: {transform.position}");


                toBeDestroyed = rayHit.collider.gameObject;
                if (rayHit.collider != null)
                {
                    if (playerStunInventory >= MAX_STUN_THROWABLES)
                    {
                        pickupText.SetActive(false);
                        throwableText.SetActive(true);
                    }
                    else
                    {
                        pickupText.SetActive(true);
                        throwableText.SetActive(false);
                    }
                }
            }
        }
        else
        {
            toBeDestroyed = null;
            pickupText.SetActive(false);
            throwableText.SetActive(false);
        }
    }

    void CalculateLandingTarget()
    {
        float vaultDistance = 2f; // How far forward from the obstacle to land

        // Step 1: Estimate the forward landing point from obstacle
        Vector3 rawLandingPoint = jumpPosition.position + vaultDirection * vaultDistance;
        //Vector3 rawLandingPoint = jumpPosition.position + jumpPosition.forward * vaultDistance;

        // Step 2: Raycast downward to find the ground
        RaycastHit hit;
        Vector3 rayStart = rawLandingPoint + Vector3.up * 1f; // Start above ground

        if (Physics.Raycast(rayStart, Vector3.down, out hit, 5f))
        {
            landingTarget.position = hit.point;
        }
        else
        {
            // Fallback: Just use raw point at obstacle height
            landingTarget.position = rawLandingPoint;
        }
    }

    private IEnumerator PickupAction()
    {
        while (defaultInput.Player.Fire.IsPressed() && toBeDestroyed != null && toBeDestroyed.transform.position.y < 0.2)
        {
            if (playerInventory < MAX_ROCK_THROWABLES && toBeDestroyed.CompareTag("Rock"))
            {
                charAnim.SetTrigger("PickUp");
                toBeDestroyed.GetComponent<Rigidbody>().isKinematic = true;
                // Used RightHandMiddle4 to make it seem like the rock is less apart of his hand
                toBeDestroyed.transform.position = playerHand.transform.position;
                toBeDestroyed.transform.parent = playerHand.transform;
                rockAudioSource.Play();
                playerInventory++;
                Destroy(toBeDestroyed, 1.5f);
                yield return new WaitForSeconds(5);
            }
            else if (playerInventory >= MAX_ROCK_THROWABLES && toBeDestroyed.CompareTag("Rock"))
            {
                throwableText.SetActive(true);
                pickupText.SetActive(false);
                yield return new WaitForSeconds(5);
            }
            else if (playerStunInventory < MAX_STUN_THROWABLES && toBeDestroyed.CompareTag("Stun"))
            {
                charAnim.SetTrigger("PickUp");
                toBeDestroyed.GetComponent<Rigidbody>().isKinematic = true;
                // Used RightHandMiddle4 to make it seem like the rock is less apart of his hand
                toBeDestroyed.transform.position = playerHand.transform.position;
                toBeDestroyed.transform.parent = playerHand.transform;
                stunAudioSource.Play();
                playerStunInventory++;
                Destroy(toBeDestroyed, 1.5f);
                yield return new WaitForSeconds(5);
            }
            else if (playerStunInventory >= MAX_STUN_THROWABLES && toBeDestroyed.CompareTag("Stun"))
            {
                throwableText.SetActive(true);
                pickupText.SetActive(false);
                yield return new WaitForSeconds(5);
            }
        }
    }

    // private void ResetPlayerPosition()
    // {
    //     Vector3 resetSpawn = new Vector3(18, 0, -20);
    //     controller.enabled = false;
    //     transform.position = resetSpawn;
    //     controller.enabled = true;
    // }

    void CheckForButtonInteraction()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, interactionDistance);

        bool foundButton = false;
        foreach (Collider hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag(buttonTag)) // Found a button
            {
                foundButton = true;
                buttonTarget = hitCollider.transform; // Store the button transform

                if (m_FireAction.WasPressedThisFrame()) // Press "F" to interact
                {
                    //Debug.Log("F Key Pressed - Moving hand to button");
                    isPressingButton = true; // Start IK movement
                }
                return;
            }
        }
        if (foundButton != true) {
            buttonTarget = null;
        }
    }

    void OnAnimatorIK(int layerIndex)
    {

        // Move hands to button for door
        if (charAnim && buttonTarget != null)
        {
            laptopTarget = null;
            float targetWeight = isPressingButton ? 1f : 0f;

            // Smoothly interpolate the IK weight
            currentIKWeight = Mathf.MoveTowards(currentIKWeight, targetWeight, Time.deltaTime * ikWeightSpeed);

            charAnim.SetIKPositionWeight(AvatarIKGoal.RightHand, currentIKWeight);
            charAnim.SetIKRotationWeight(AvatarIKGoal.RightHand, currentIKWeight);

            // Only move the hand if there's some weight
            if (currentIKWeight > 0f)
            {
                Debug.Log("Moving hand to button");
                charAnim.SetIKPosition(AvatarIKGoal.RightHand, buttonTarget.position);
                charAnim.SetIKRotation(AvatarIKGoal.RightHand, buttonTarget.rotation);
            }

            // Trigger button only when fully extended
            if (isPressingButton && currentIKWeight >= 1f &&
                Vector3.Distance(charAnim.GetIKPosition(AvatarIKGoal.RightHand), buttonTarget.position) < 0.1f)
            {
                Debug.Log("button pressed");
                isPressingButton = false; // Done pressing
                buttonTarget = null;

                // Trigger the door or whatever action
                TriggerNearbyDoor();
            }
        }
        // Move hands to laptop
        if (charAnim && laptopTarget != null) 
        {
            buttonTarget = null;
            float targetWeight = isPressingLaptop ? 1f : 0f;

            // Smoothly interpolate the IK weight
            currentIKLaptopWeight = Mathf.MoveTowards(currentIKLaptopWeight, targetWeight, Time.deltaTime * ikLaptopWeightSpeed);

            charAnim.SetIKPositionWeight(AvatarIKGoal.RightHand, currentIKLaptopWeight);
            //charAnim.SetIKRotationWeight(AvatarIKGoal.RightHand, currentIKWeight);

            // Only move the hand if there's some weight
            if (currentIKLaptopWeight > 0f)
            {
                Debug.Log("Moving hand to laptop");
                charAnim.SetIKPosition(AvatarIKGoal.RightHand, laptopTarget.position);
                //charAnim.SetIKRotation(AvatarIKGoal.RightHand, laptopTarget.rotation);
            }

            // Trigger laptop only when fully extended
            // might need to play with this condition a bit
            if (isPressingLaptop && currentIKLaptopWeight >= 1f &&
                Vector3.Distance(charAnim.GetIKPosition(AvatarIKGoal.RightHand), laptopTarget.position) < .1f)
            {
                Debug.Log("Hand has moved to laptop");
                isPressingLaptop = false; // Done pressing
                laptopTarget = null;

                // Trigger whatever action
                TriggerNearbyLaptop();
            }
        }

    }



    void TriggerNearbyDoor()
    {
        //Debug.Log("TriggerNearbyDoor() called - Checking for nearby doors...");

        Collider[] nearbyObjects = Physics.OverlapSphere(transform.position, interactionDistance);

        if (nearbyObjects.Length == 1)
        {
            Debug.Log("No objects found within interaction distance.");
        }

        foreach (Collider obj in nearbyObjects)
        {
            //Debug.Log("Found object: " + obj.gameObject.name);

            if (obj.CompareTag(doorTag)) // Found a door
            {
                //Debug.Log("Door detected: " + obj.gameObject.name);

                DoorController door = obj.GetComponent<DoorController>();

                if (door != null)
                {
                   // Debug.Log("Calling OpenDoor() on " + obj.gameObject.name);
                    door.OpenDoor(); // Call the door's OpenDoor() function
                }
                else
                {
                    Debug.LogError("DoorController script NOT found on " + obj.gameObject.name);
                }
                return; // Stop checking after finding the first door
            }
            else
            {
                //Debug.Log(obj.gameObject.name + " is NOT tagged as 'Door'.");
            }
        }
    }

    public void DisplayFinishLineText(bool displayText)
    {
        if (SceneManager.GetActiveScene().buildIndex == SceneManager.sceneCountInBuildSettings - 1)
        {
            quitGameText.SetActive(displayText);
        }
        else
        {
            nextLevelText.SetActive(displayText);
        }

        Time.timeScale = 0f;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void DisplayNotDoneYet(bool displayText)
    {
        goalNotFinishedText.SetActive(displayText);
    }

    // This gives a prompt to the user to press F to open doors.
    public void DisplayDoorText(bool displayText) 
    {
        doorText.SetActive(displayText);
    }

    public void updatePrisonerCount()
    {
        Debug.Log("prisoner count decreased by 1");
        prisonersRemaining -= 1;
        prisonerCount.text = "Prisoners Remaining: " + prisonersRemaining.ToString();
    }

    void TriggerNearbyLaptop()
    {
        Debug.Log("TriggerNearbyLaptop() called - Checking for nearby laptops...");

        Collider[] nearbyObjects = Physics.OverlapSphere(transform.position, interactionDistance);

        if (nearbyObjects.Length == 1)
        {
            Debug.Log("No objects found within interaction distance.");
        }

        foreach (Collider obj in nearbyObjects)
        {
            //Debug.Log("Found object: " + obj.gameObject.name);

            if (obj.CompareTag(laptopTag)) // Found a door
            {
                //Debug.Log("Door detected: " + obj.gameObject.name);

                LaptopController laptop = obj.GetComponent<LaptopController>();

                if (laptop != null)
                {
                   // Debug.Log("Calling OpenDoor() on " + obj.gameObject.name);
                    laptop.DisableGuard(); // Call the door's OpenDoor() function
                }
                else
                {
                    Debug.LogError("LaptopController script NOT found on " + obj.gameObject.name);
                }
                return; // Stop checking after finding the first door
            }
            else
            {
                //Debug.Log(obj.gameObject.name + " is NOT tagged as 'Door'.");
            }
        }
    }

    // To setup a laptop, attach the laptop controller script the laptop object, add a small sphere collider, and set the guard
    // to be disabled in the inspector for the laptop controller script. Also tag the laptop as "Laptop"
    void CheckForLaptopInteraction()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, interactionDistance);

        bool foundLaptop = false;
        foreach (Collider hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag(laptopTag)) // Found a laptop
            {
                foundLaptop = true;
                laptopTarget = hitCollider.transform; // Store the laptop transform

                if (m_FireAction.WasPressedThisFrame()) // Press "F" to interact
                {
                    Debug.Log("F Key Pressed - Moving hand to laptop");
                    isPressingLaptop = true; // Start IK movement
                }
                return;
            }
        }

        if (foundLaptop != true) {
            laptopTarget = null;
        }
    }
}