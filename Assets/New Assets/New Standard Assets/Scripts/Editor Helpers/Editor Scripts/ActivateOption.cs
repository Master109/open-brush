#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Extensions;

namespace EternityEngine
{
	[ExecuteInEditMode]
	public class ActivateOption : EditorScript
	{
		public Option option;

		public override void Do ()
		{
			if (option == null)
				option = GetComponent<Option>();
			_Do (option);
		}

		public static void _Do (Option option)
		{
#if USE_UNITY_EVENTS || USE_EVENTS
			if (option.activatable && option.gameObject.activeSelf)
#if USE_UNITY_EVENTS
				option.onStartActivateUnityEvent.Invoke(LogicModule.instance.leftHand);
#endif
#if USE_EVENTS
				option.onStartActivate.unityEvent.Invoke(LogicModule.instance.leftHand);
#endif
#endif
		}

		[MenuItem("Game/Activate selected Options %#a")]
		static void _Do ()
		{
			GameObject[] selectedGos = Selection.gameObjects;
			for (int i = 0; i < selectedGos.Length; i ++)
			{
				GameObject go = selectedGos[i];
				Option option = go.GetComponent<Option>();
				_Do (option);
			}
		}
	}
}
#else
namespace EternityEngine
{
	public class ActivateOption : EditorScript
	{
	}
}
#endif