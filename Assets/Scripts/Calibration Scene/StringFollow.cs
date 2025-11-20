using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StringFollow : MonoBehaviour
{
    public Transform transformToFollow;
    
    // Update is called once per frame
    void Update()
    {
        transform.position = transformToFollow.position;
    }
}
