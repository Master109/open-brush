using System;
using Extensions;
using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

namespace EternityEngine
{
	public class Vector2Option : Option
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
		public TypingTargetOption xTypingTargetOption;
		public TypingTargetOption yTypingTargetOption;
		public Vector2 value;
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
			base.OnValidate ();
		}
#endif
		
		public override void SetData ()
		{
			if (_Data == null)
				_Data = new Data();
			base.SetData ();
		}

		public void SetX ()
		{
			SetX (float.Parse(xTypingTargetOption.GetValue()));
		}

		public void SetX (float x)
		{
			SetValue (new Vector2(x, value.y));
		}

		public void SetY ()
		{
			SetY (float.Parse(yTypingTargetOption.GetValue()));
		}

		public void SetY (float y)
		{
			SetValue (new Vector2(value.x, y));
		}

		public void SetValue (Vector2 value)
		{
			if (this.value != value)
				return;
			this.value = value;
			xTypingTargetOption.SetValue ("" + value.x);
			yTypingTargetOption.SetValue ("" + value.y);
#if USE_UNITY_EVENTS
			onValueChangedUnityEvent.Invoke();
#endif
#if USE_EVENTS
			onValueChanged.unityEvent.Invoke();
#endif
		}

		[Serializable]
		public class Data : Option.Data
		{
			public override object MakeAsset ()
			{
				Vector2Option vector2Option = ObjectPool.instance.SpawnComponent<Vector2Option>(LogicModule.instance.vector2OptionPrefab.prefabIndex, parent:LogicModule.instance.sceneTrs);
				Apply (vector2Option);
				return vector2Option;
			}

			public override void Apply (Asset asset)
			{
				base.Apply (asset);
				Vector2Option vector2Option = (Vector2Option) asset;
				vector2Option._Data = this;
			}
		}
	}
}