using UnityEngine;


public class TriggerParticleEffect : MonoBehaviour
{
    [Header("Assign your particle system prefab here")]
    public GameObject particlePrefab;

    [Header("Auto destroy particle effect after seconds")]
    public float destroyAfterSeconds = 2f;

    [Header("Audio Source")]
    AudioSource explosionSound; 
    private bool hasExploded = false;

    

    void Start()
    {
        explosionSound = GetComponent<AudioSource>();
        
    }

    private void OnTriggerEnter(Collider other) //Trigger event for when it hits the enemy
    {
        
        if (hasExploded) return; // Prevent multiple triggers

        // Check for both Rigidbody and Enemy tag
        Rigidbody rb = other.GetComponent<Rigidbody>();
        if (rb != null && other.CompareTag("Enemy")) 
        {
            hasExploded = true; // Set the flag to true to prevent re-triggering


            // Instantiate the effect at this object's position and rotation
            GameObject effect = Instantiate(
                particlePrefab,
                transform.position,
                Quaternion.identity
            );

            explosionSound.Play(); // Play the explosion sound


            // Optional: parent to the current object (if you want it to move with it)
            // effect.transform.SetParent(transform);

            // Auto-destroy to keep scene clean
            Destroy(effect, destroyAfterSeconds);

            // destroy the grenade itself
            Destroy(gameObject,destroyAfterSeconds);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Only explode once
        if (hasExploded) return;

        // Check Y position and that it's NOT the player
        if (transform.position.y <= 0.5f && !collision.collider.CompareTag("Player"))
        {
            hasExploded = true;

            // Spawn explosion effect at grenade's position
            GameObject effect = Instantiate(
                particlePrefab,
                transform.position,
                Quaternion.identity
            );

            explosionSound.Play(); // Play the explosion sound


            Destroy(effect, destroyAfterSeconds);

            // Optional: destroy the grenade itself
            Destroy(gameObject,destroyAfterSeconds);
        }
    }

}
