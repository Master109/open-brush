using System;
using UnityEngine;
using Extensions;
using UnityEngine.Events;

namespace EternityEngine
{
	public class CircularMenu : SingletonMonoBehaviour<CircularMenu>
	{
		public Transform trs;
		public Transform currentDirectionIndicator;
		public Option[] options = new Option[0];
		public static Option? currentSelected;

		public void DoUpdate (LogicModule.Hand hand)
		{
			currentDirectionIndicator.localScale = Vector3.one * hand.thumbstickInput.magnitude;
			currentDirectionIndicator.up = hand.thumbstickInput;
			for (int i = 0; i < options.Length; i ++)
			{
				Option option = options[i];
				if (!currentSelected.Equals(option) && option.degreeRange.Contains(hand.thumbstickInput.GetFacingAngle(), true, false))
				{
					if (currentSelected != null)
					{
						Option _currentSelected = (Option) currentSelected;
						_currentSelected.selectedIndicator.SetActive(false);
					}
					option.unityEvent.Invoke();
					option.selectedIndicator.SetActive(true);
					currentSelected = option;
					return;
				}
			}
		}

		[Serializable]
		public struct Option
		{
			public UnityEvent unityEvent;
			public GameObject selectedIndicator;
			public FloatRange degreeRange;
		}
	}
}