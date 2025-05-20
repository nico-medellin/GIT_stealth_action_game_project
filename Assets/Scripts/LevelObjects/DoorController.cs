using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DoorController : MonoBehaviour
{
    private Animator doorAnimator;
    public string openTriggerName = "Open"; // Make sure this matches the Animator parameter
    public bool isOpen = false;
    public UnityEvent onDoorOpened;

    void Start()
    {
        doorAnimator = GetComponent<Animator>();

        if (doorAnimator == null)
        {
            Debug.LogError(" Door Animator is missing on " + gameObject.name);
        }
    }

    public void OpenDoor()
    {
        isOpen = true;
        onDoorOpened?.Invoke();
        if (doorAnimator != null)
        {
            //Debug.Log(" OpenDoor() called on " + gameObject.name);
            
            if(HasParameter(doorAnimator,openTriggerName))
            {
                //Debug.Log("Setting Animator Trigger: " + openTriggerName);
                doorAnimator.SetTrigger(openTriggerName);
            }
            else
            {
                Debug.LogError("The parameter '" + openTriggerName + "' does NOT exist in the Animator!");
            }
            // Alternative: Directly play animation if trigger fails
            doorAnimator.Play("Open", 0, 0.0f);
            //Debug.Log("Attempting to play 'Open' animation directly.");
        }
        else
        {
            Debug.LogError(" OpenDoor() called but no Animator found on " + gameObject.name);
        }
    }

    bool HasParameter(Animator animator, string paramName)
    {
        foreach (AnimatorControllerParameter param in animator.parameters)
        {
            if (param.name == paramName)
            {
                return true;
            }
        }
        return false;
    }
}
