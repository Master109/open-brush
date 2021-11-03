using System;
using UnityEngine;

namespace EternityEngine
{
	public class ArtModule : SingletonUpdateWhileEnabled<ArtModule>
	{
		public Hand leftHand;
		public Hand rightHand;
		public float minAngleToSwitchHands;
		public float maxDistanceToSwitchHands;
		public float minSpeedToSwitchHands;
		public bool menusInLeftHand;
		public ControllerActivationEffect swapHandsEffect;

		public override void DoUpdate ()
		{
			HandleSwitchHands ();
		}

		void HandleSwitchHands ()
		{
			Vector3 leftHandPosition = leftHand.trs.position;
			Vector3 rightHandPosition = rightHand.trs.position;
			float leftHandSpeed = (leftHandPosition - leftHand.previousPosition).magnitude / Time.deltaTime * Vector3.Dot(rightHandPosition - leftHandPosition, leftHandPosition - leftHand.previousPosition);
			float rightHandSpeed = (rightHandPosition - rightHand.previousPosition).magnitude / Time.deltaTime * Vector3.Dot(leftHandPosition - rightHandPosition, rightHandPosition - rightHand.previousPosition);
			if (Vector3.Angle(leftHand.trs.forward, rightHand.trs.forward) >= minAngleToSwitchHands && (leftHandPosition - rightHandPosition).sqrMagnitude <= maxDistanceToSwitchHands * maxDistanceToSwitchHands && leftHandSpeed + rightHandSpeed >= minSpeedToSwitchHands)
				SwitchHands ();
		}

		void SwitchHands ()
		{
			Brush previousLeftHandBrush = leftHand.currrentBrush;
			leftHand.currrentBrush = rightHand.currrentBrush;
			rightHand.currrentBrush = previousLeftHandBrush;
			menusInLeftHand = !menusInLeftHand;
			ObjectPool.instance.SpawnComponent<ControllerActivationEffect>(swapHandsEffect.prefabIndex, (leftHand.trs.position + rightHand.trs.position) / 2);
		}

		[Serializable]
		public class Hand : VRCameraRig.Hand
		{
			public Transform gripTrs;
			public Transform pointerParent;
			public Brush currrentBrush;
		}
	}
}