#if UNITY_EDITOR
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Extensions;
using UnityEditor;

namespace EternityEngine
{
	public class MakeTypingOptions : EditorScript
	{
		public Option optionPrefab;
		public string characters;
		public Option parentOption;
		
		public override void Do ()
		{
			foreach (char character in characters)
			{
				Option option = Instantiate(optionPrefab);
				option.name = "Option_" + character;
				option.text.text = "" + character;
#if USE_UNITY_EVENTS
				option.onStartActivateUnityEvent.RemoveAllListeners();
				option.onStartActivateUnityEvent.AddListener((LogicModule.Hand hand) => { LogicModule.instance.AddTextToTypingTargetOption ("" + character); });
#endif
#if USE_EVENTS
				option.onStartActivate.unityEvent.RemoveAllListeners();
				option.onStartActivate.unityEvent.AddListener((LogicModule.Hand hand) => { LogicModule.instance.AddTextToTypingTargetOption ("" + character); });
#endif
				Transform optionTrs = parentOption.childOptionsParent.Find(option.name);
				option.trs.position = optionTrs.position;
				option.trs.SetParent(parentOption.childOptionsParent);
				GameManager.DestroyOnNextEditorUpdate (optionTrs.gameObject);
			}
		}
	}
}
#else
namespace EternityEngine
{
	public class MakeTypingOptions : EditorScript
	{
	}
}
#endif