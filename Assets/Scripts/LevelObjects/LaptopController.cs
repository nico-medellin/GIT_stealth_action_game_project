using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaptopController : MonoBehaviour
{
    public FollowingGuard linkedGuard;

    public void DisableGuard() {
        if (linkedGuard != null) {
            Debug.Log("Disabling guard from laptopControlle");
            linkedGuard.EnableKnockedOut();
        } else {
            Debug.LogError("linkedGuard not set");
        }
    }
}
