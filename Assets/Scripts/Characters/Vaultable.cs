//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class Vaultable : MonoBehaviour
//{
//    public Transform vaultPoint;
//    // Start is called before the first frame update
//    void Start()
//    {
        
//    }

//    // Update is called once per frame
//    void OnTriggerEnter(Collider col)
//    {
//        PlayerController c = col.GetComponent<PlayerController>();
//        if (c != null)
//        {
//            c.Vaultable = this;
//        }
 
//    }
//    void OnTriggerExit(Collider col)
//    {
//        PlayerController c = col.GetComponent<PlayerController>();
//        if (c != null)
//        {
//            c.Vaultable = null;
//        }
//    }

//    public Transform GetVaultPoint()
//    {
//        return vaultPoint.transform;
//    }
//}
