using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class animationStateTest : MonoBehaviour {

    Animator animator;
    Animator Nanim;
    public float FirstCogtimey;
    public float SecondCogtimey;
    public GameObject neighbor;
    public float delta;
	// Use this for initialization
	void Start () {
        animator = GetComponent<Animator>();
        Nanim = neighbor.GetComponent<Animator>();
	}
	
	// Update is called once per frame
	void Update () {
        AnimatorStateInfo currentState = animator.GetCurrentAnimatorStateInfo(0);
        AnimatorStateInfo NAcurrentState = Nanim.GetCurrentAnimatorStateInfo(0);

        FirstCogtimey = currentState.normalizedTime % 1;
        SecondCogtimey = NAcurrentState.normalizedTime % 1;
        delta = FirstCogtimey - SecondCogtimey;
        //Debug.Log(timey);
    }
}
