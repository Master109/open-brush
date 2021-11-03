using TMPro;
using System;
using Extensions;
using UnityEngine;
using UnityEngine.Events;
#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif

namespace EternityEngine
{
	public class BoolOption : Option
	{
		public new Data _Data
		{
			get
			{
				return (Data) data;
			}
			set
			{
				data = value;
			}
		}
		public override string nameToValueSeparator
		{
			get
			{
				return ": ";
			}
		}
		public bool value;
		public string trueText;
		public string falseText;
#if USE_UNITY_EVENTS
		public UnityEvent onValueChangedUnityEvent;
		[HideInInspector]
		public int indexOfOnValueChangedUnityEvent;
#endif
#if USE_EVENTS
		public Event onValueChanged;
		[HideInInspector]
		public int indexOfOnValueChanged;
#endif

#if UNITY_EDITOR
		public override void OnValidate ()
		{
			if (addEvents)
			{
#if USE_UNITY_EVENTS
				if (!LogicModule.Instance.unityEvents.Contains(onValueChangedUnityEvent))
				{
					LogicModule.instance.unityEvents.Add(onValueChangedUnityEvent);
					indexOfOnValueChangedUnityEvent = LogicModule.instance.unityEvents.Count - 1;
				}
#endif
#if USE_EVENTS
				if (!LogicModule.Instance.events.Contains(onValueChanged))
				{
					LogicModule.instance.events.Add(onValueChanged);
					indexOfOnValueChanged = LogicModule.instance.events.Count - 1;
				}
#endif
			}
			if (removeEvents)
			{
#if USE_UNITY_EVENTS
				LogicModule.Instance.unityEvents.Remove(onValueChangedUnityEvent);
#endif
#if USE_EVENTS
				LogicModule.Instance.events.Remove(onValueChanged);
#endif
			}
			if (value)
				text.text = trueText;
			else
				text.text = falseText;
			if (!EditorSceneManager.IsPreviewScene(gameObject.scene))
				HandleNaming ();
			base.OnValidate ();
		}
#endif

		public void ToggleValue ()
		{
			SetValue (!value);
		}

		public void SetValue (bool value)
		{
			if (this.value == value)
				return;
			this.value = value;
			if (value)
				text.text = trueText;
			else
				text.text = falseText;
			HandleNaming ();
#if USE_UNITY_EVENTS
			onValueChangedUnityEvent.Invoke();
#endif
#if USE_EVENTS
			onValueChanged.unityEvent.Invoke();
#endif
		}
		
		public override void SetData ()
		{
			if (_Data == null)
				_Data = new Data();
			base.SetData ();
			SetValueOfData ();
		}

		public void SetValueOfData ()
		{
			value = _Data.value;
			if (value)
				text.text = trueText;
			else
				text.text = falseText;
		}

		public void SetValueFromData ()
		{
			SetValue (_Data.value);
		}

		[Serializable]
		public class Data : Option.Data
		{
			[SaveAndLoadValue]
			public bool value;
			
			public override object MakeAsset ()
			{
				BoolOption boolOption = ObjectPool.instance.SpawnComponent<BoolOption>(LogicModule.instance.boolOptionPrefab.prefabIndex, parent:LogicModule.instance.sceneTrs);
				Apply (boolOption);
				return boolOption;
			}

			public override void Apply (Asset asset)
			{
				base.Apply (asset);
				BoolOption boolOption = (BoolOption) asset;
				boolOption._Data = this;
				boolOption.SetValueFromData ();
			}
		}
	}
}