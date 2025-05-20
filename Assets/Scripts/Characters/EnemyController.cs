using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour
{
    public Transform enemy;
    private Transform currentTarget;
    public Transform target1; //Player's transform
    public Transform target2; //Player's transform

    public float speed = 3f; //Movement Speed

    void Start()
    {
        currentTarget = target1;
    }

    void Update()
    {
        float distance = Vector3.Distance(currentTarget.position, transform.position);
        if (distance < 1 && currentTarget == target1)
        {
            enemy.SetPositionAndRotation(transform.position, new Quaternion(0, 180, 0, 0));
            currentTarget = target2;
        }
        else if (distance < 1 && currentTarget == target2)
        {
            enemy.SetPositionAndRotation(transform.position, new Quaternion(0, 0, 0, 0));
            currentTarget = target1;
        }
        Vector3 direction = (currentTarget.position - transform.position).normalized;
        transform.position += direction * speed * Time.deltaTime;
    }
}
