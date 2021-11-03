using System;
using Extensions;
using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;
using System.Collections.Generic;

namespace EternityEngine
{
	public class VRCameraRig : SingletonUpdateWhileEnabled<VRCameraRig>
	{
		public Camera camera;
		public Transform trackingSpaceTrs;
		public Transform eyesTrs;
		public Transform bothHandsAverageTrs;
		public Transform trs;
		public float lookRate;
		public Hand leftHand;
		public Hand rightHand;
		
		public override void OnEnable ()
		{
			trs.SetParent(null);
			base.OnEnable ();
		}

		public override void DoUpdate ()
		{
			leftHand.triggerInput = InputManager.LeftTriggerInput;
#if UNITY_EDITOR
			if (!leftHand.triggerInput)
				leftHand.triggerInput = Keyboard.current.leftAltKey.isPressed;
			if (!leftHand.primaryButtonInput)
				leftHand.primaryButtonInput = Keyboard.current.leftCtrlKey.isPressed;
#endif
			leftHand.gripInput = InputManager.LeftGripInput;
			rightHand.triggerInput = InputManager.RightTriggerInput;
#if UNITY_EDITOR
			if (!rightHand.triggerInput)
				rightHand.triggerInput = Keyboard.current.rightAltKey.isPressed;
#endif
			rightHand.gripInput = InputManager.RightGripInput;
			leftHand.thumbstickClickedInput = InputManager.LeftThumbstickClickedInput;
			rightHand.thumbstickClickedInput = InputManager.RightThumbstickClickedInput;
			leftHand.primaryButtonInput = InputManager.LeftPrimaryButtonInput;
#if UNITY_EDITOR
			if (!leftHand.primaryButtonInput)
				leftHand.primaryButtonInput = Keyboard.current.leftCtrlKey.isPressed;
#endif
			rightHand.primaryButtonInput = InputManager.RightPrimaryButtonInput;
#if UNITY_EDITOR
			if (!rightHand.primaryButtonInput)
				rightHand.primaryButtonInput = Keyboard.current.rightCtrlKey.isPressed;
#endif
			leftHand.secondaryButtonInput = InputManager.LeftSecondaryButtonInput;
			rightHand.secondaryButtonInput = InputManager.RightSecondaryButtonInput;
			// leftHand.Update ();
			// rightHand.Update ();
			leftHand.previousTriggerInput = leftHand.triggerInput;
			leftHand.previousGripInput = leftHand.gripInput;
			leftHand.previousThumbstickClickedInput = leftHand.thumbstickClickedInput;
			rightHand.previousThumbstickClickedInput = rightHand.thumbstickClickedInput;
			rightHand.previousTriggerInput = rightHand.triggerInput;
			rightHand.previousGripInput = rightHand.gripInput;
			leftHand.previousPrimaryButtonInput = leftHand.primaryButtonInput;
			rightHand.previousPrimaryButtonInput = rightHand.primaryButtonInput;
			leftHand.previousSecondaryButtonInput = leftHand.secondaryButtonInput;
			rightHand.previousSecondaryButtonInput = rightHand.secondaryButtonInput;
			leftHand.previousPosition = leftHand.trs.position;
			rightHand.previousPosition = rightHand.trs.position;
			if (InputManager.Instance.inputDevice == InputManager.InputDevice.VR)
				UpdateTransforms ();
			else
			{
				Vector2 rotaInput = Mouse.current.delta.ReadValue().FlipY() * lookRate * Time.unscaledDeltaTime;
				if (rotaInput != Vector2.zero)
				{
					trackingSpaceTrs.RotateAround(trackingSpaceTrs.position, trackingSpaceTrs.right, rotaInput.y);
					trackingSpaceTrs.RotateAround(trackingSpaceTrs.position, Vector3.up, rotaInput.x);
				}
			}
		}

		void UpdateTransforms ()
		{
			if (InputManager.HeadPosition != null)
				eyesTrs.localPosition = (Vector3) InputManager.HeadPosition;
			if (InputManager.HeadRotation != null)
				eyesTrs.localRotation = (Quaternion) InputManager.HeadRotation;
			if (InputManager.LeftHandPosition != null)
				leftHand.trs.localPosition = (Vector3) InputManager.LeftHandPosition;
			if (InputManager.LeftHandRotation != null)
				leftHand.trs.localRotation = (Quaternion) InputManager.LeftHandRotation;
			if (InputManager.RightHandPosition != null)
				rightHand.trs.localPosition = (Vector3) InputManager.RightHandPosition;
			if (InputManager.RightHandRotation != null)
				rightHand.trs.localRotation = (Quaternion) InputManager.RightHandRotation;
			bothHandsAverageTrs.position = (leftHand.trs.position + rightHand.trs.position) / 2;
			bothHandsAverageTrs.rotation = Quaternion.Slerp(leftHand.trs.rotation, rightHand.trs.rotation, 0.5f);
			bothHandsAverageTrs.SetWorldScale (Vector3.one * Vector3.Distance(leftHand.trs.position, rightHand.trs.position));
		}
		
		[Serializable]
		public class Hand : ISaveableAndLoadable
		{
			public bool isLeftHand;
			public Transform trs;
			public bool triggerInput;
			public bool previousTriggerInput;
			public bool gripInput;
			public bool previousGripInput;
			public bool primaryButtonInput;
			public bool previousPrimaryButtonInput;
			public bool secondaryButtonInput;
			public bool previousSecondaryButtonInput;
			public bool thumbstickClickedInput;
			public bool previousThumbstickClickedInput;
			public Vector2 thumbstickInput;
			public Vector2 previousThumbstickInput;
			public Vector3 previousPosition;

			public void Update ()
			{
				
			}
		}
	}
}