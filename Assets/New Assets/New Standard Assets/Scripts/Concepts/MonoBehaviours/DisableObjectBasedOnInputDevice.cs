using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EternityEngine
{
	public class DisableObjectBasedOnInputDevice : MonoBehaviour
	{
		public bool disableIfUsing;
		public InputManager.InputDevice inputDevice;
		
		void Start ()
		{
			gameObject.SetActive(InputManager.Instance.inputDevice == inputDevice != disableIfUsing);
		}
	}
}