using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuppetAnim : MonoBehaviour
{
    private Animator animator;
    
    void Start()
    {
        animator = GetComponent<Animator>();

        animator.SetTrigger("StartScreen");
    }

    // Update is called once per frame
    void Update()
    {
    }
}
