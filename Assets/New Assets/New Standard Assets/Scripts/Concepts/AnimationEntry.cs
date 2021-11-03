using System;
using UnityEngine;

namespace EternityEngine
{
	[Serializable]
	public struct AnimationEntry
	{
		public string animatorStateName;
		public int layer;
		public Animator animator;

		public void Play ()
		{
			animator.enabled = true;
			animator.Play(animatorStateName, layer);
		}

		public void Play (float normalizedTime)
		{
			animator.enabled = true;
			animator.Play(animatorStateName, layer, normalizedTime);
		}
	}
}