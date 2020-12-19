using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckConnection : MonoBehaviour
{
    void Start()
    {
        
    }

    public bool tryConnect;

    void Update()
    {
        switch (Application.internetReachability)
        {
            case NetworkReachability.NotReachable:
                break;
            case NetworkReachability.ReachableViaCarrierDataNetwork:
                break;
            case NetworkReachability.ReachableViaLocalAreaNetwork:
                break;
            default:
                break;
        }
    }
}
