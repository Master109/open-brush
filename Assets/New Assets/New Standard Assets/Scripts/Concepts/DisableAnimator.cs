using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableAnimator : StateMachineBehaviour
{
	public override void OnStateEnter (Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		animator.enabled = false;
	}
}
