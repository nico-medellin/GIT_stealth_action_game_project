using UnityEngine;

public class IKFootPlacement : MonoBehaviour
{
    private Animator animator;
    public LayerMask groundLayer;
    public Transform leftFoot, rightFoot;
    public float footOffset = 0.1f;
    
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void OnAnimatorIK(int layerIndex)
    {
        if (animator)
        {
            SetFootIK(AvatarIKGoal.LeftFoot, leftFoot);
            SetFootIK(AvatarIKGoal.RightFoot, rightFoot);
        }
    }

    void SetFootIK(AvatarIKGoal foot, Transform footTransform)
    {
        RaycastHit hit;
        if (Physics.Raycast(footTransform.position + Vector3.up, Vector3.down, out hit, 1f, groundLayer))
        {
            animator.SetIKPositionWeight(foot, 1);
            animator.SetIKRotationWeight(foot, 1);
            animator.SetIKPosition(foot, hit.point + Vector3.up * footOffset);
            animator.SetIKRotation(foot, Quaternion.LookRotation(transform.forward, hit.normal));
        }
        else
        {
            animator.SetIKPositionWeight(foot, 0);
            animator.SetIKRotationWeight(foot, 0);
        }
    }
}
