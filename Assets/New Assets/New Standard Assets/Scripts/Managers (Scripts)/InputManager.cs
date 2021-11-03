using UnityEngine;
using Unity.XR.Oculus.Input;
using UnityEngine.InputSystem;

namespace EternityEngine
{
	public class InputManager : SingletonMonoBehaviour<InputManager>
	{
		public InputDevice inputDevice;
		public InputSettings settings;
		public static bool UsingMouse
		{
			get
			{
				return Mouse.current != null;
			}
		}
		public static bool UsingKeyboard
		{
			get
			{
				return Keyboard.current != null;
			}
		}
		public static bool LeftClickInput
		{
			get
			{
				if (UsingMouse)
					return Mouse.current.leftButton.isPressed;
				else
					return false;
			}
		}
		public static bool RightClickInput
		{
			get
			{
				if (UsingMouse)
					return Mouse.current.rightButton.isPressed;
				else
					return false;
			}
		}
		public static Vector2? MousePosition
		{
			get
			{
				if (UsingMouse)
					return Mouse.current.position.ReadValue();
				else
					return null;
			}
		}
		public static Vector2? LeftThumbstick
		{
			get
			{
				if (LeftTouchController != null)
					return Vector2.ClampMagnitude(LeftTouchController.thumbstick.ReadValue(), 1);
				else
					return null;
			}
		}
		public static Vector2? RightThumbstick
		{
			get
			{
				if (RightTouchController != null)
					return Vector2.ClampMagnitude(RightTouchController.thumbstick.ReadValue(), 1);
				else
					return null;
			}
		}
		public static bool LeftGripInput
		{
			get
			{
				return LeftTouchController != null && LeftTouchController.gripPressed.isPressed;
			}
		}
		public static bool RightGripInput
		{
			get
			{
				return RightTouchController != null && RightTouchController.gripPressed.isPressed;
			}
		}
		public static bool LeftTriggerInput
		{
			get
			{
				return LeftTouchController != null && LeftTouchController.triggerPressed.isPressed;
			}
		}
		public static bool RightTriggerInput
		{
			get
			{
				return RightTouchController != null && RightTouchController.triggerPressed.isPressed;
			}
		}
		public static bool LeftPrimaryButtonInput
		{
			get
			{
				return LeftTouchController != null && LeftTouchController.primaryButton.isPressed;
			}
		}
		public static bool RightPrimaryButtonInput
		{
			get
			{
				return RightTouchController != null && RightTouchController.primaryButton.isPressed;
			}
		}
		public static bool LeftSecondaryButtonInput
		{
			get
			{
				return LeftTouchController != null && LeftTouchController.secondaryButton.isPressed;
			}
		}
		public static bool RightSecondaryButtonInput
		{
			get
			{
				return RightTouchController != null && RightTouchController.secondaryButton.isPressed;
			}
		}
		public static bool LeftThumbstickClickedInput
		{
			get
			{
				return LeftTouchController != null && LeftTouchController.thumbstickClicked.isPressed;
			}
		}
		public static bool RightThumbstickClickedInput
		{
			get
			{
				return RightTouchController != null && RightTouchController.thumbstickClicked.isPressed;
			}
		}
		public static Vector3? HeadPosition
		{
			get
			{
				if (Hmd != null)
					return Hmd.devicePosition.ReadValue();
				else
					return null;
			}
		}
		public static Quaternion? HeadRotation
		{
			get
			{
				if (Hmd != null)
					return Hmd.deviceRotation.ReadValue();
				else
					return null;
			}
		}
		public static Vector3? LeftHandPosition
		{
			get
			{
				if (LeftTouchController != null)
					return LeftTouchController.devicePosition.ReadValue();
				else
					return null;
			}
		}
		public static Quaternion? LeftHandRotation
		{
			get
			{
				if (LeftTouchController != null)
					return LeftTouchController.deviceRotation.ReadValue();
				else
					return null;
			}
		}
		public static Vector3? RightHandPosition
		{
			get
			{
				if (RightTouchController != null)
					return RightTouchController.devicePosition.ReadValue();
				else
					return null;
			}
		}
		public static Quaternion? RightHandRotation
		{
			get
			{
				if (RightTouchController != null)
					return RightTouchController.deviceRotation.ReadValue();
				else
					return null;
			}
		}
		public static OculusHMD Hmd
		{
			get
			{
				return InputSystem.GetDevice<OculusHMD>();
			}
		}
		public static OculusTouchController LeftTouchController
		{
			get
			{
				return (OculusTouchController) OculusTouchController.leftHand;
			}
		}
		public static OculusTouchController RightTouchController
		{
			get
			{
				return (OculusTouchController) OculusTouchController.rightHand;
			}
		}

		public static float GetAxis (InputControl<float> positiveButton, InputControl<float> negativeButton)
		{
			return positiveButton.ReadValue() - negativeButton.ReadValue();
		}

		public static Vector2 GetAxis2D (InputControl<float> positiveXButton, InputControl<float> negativeXButton, InputControl<float> positiveYButton, InputControl<float> negativeYButton)
		{
			Vector2 output = new Vector2();
			output.x = positiveXButton.ReadValue() - negativeXButton.ReadValue();
			output.y = positiveYButton.ReadValue() - negativeYButton.ReadValue();
			output = Vector2.ClampMagnitude(output, 1);
			return output;
		}
		
		public enum InputDevice
		{
			KeyboardAndMouse,
			VR
		}
	}
}