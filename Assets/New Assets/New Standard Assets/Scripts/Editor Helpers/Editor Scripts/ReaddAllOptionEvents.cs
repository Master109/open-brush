#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Extensions;

namespace EternityEngine
{
	[ExecuteInEditMode]
	public class ReaddAllOptionEvents : EditorScript
	{
		public GameObject go;

		public override void Do ()
		{
			_Do ();
		}

		[MenuItem("Game/Readd all Option events")]
		public static void _Do ()
		{
#if USE_UNITY_EVENTS
			LogicModule.instance.unityEvents.Clear();
			LogicModule.instance.handUnityEvents.Clear();
#endif
#if USE_EVENTS
			LogicModule.instance.events.Clear();
			LogicModule.instance.handEvents.Clear();
#endif
			Option[] options = FindObjectsOfType<Option>(true);
			for (int i = 0; i < options.Length; i ++)
			{
				Option option = options[i];
				option.addEvents = true;
				option.OnValidate ();
			}
		}
	}
}
#else
namespace EternityEngine
{
	public class ReaddAllOptionEvents : EditorScript
	{
	}
}
#endif