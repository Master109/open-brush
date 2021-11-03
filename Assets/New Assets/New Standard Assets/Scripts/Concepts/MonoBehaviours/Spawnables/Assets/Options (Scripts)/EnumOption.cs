using System;
using UnityEngine;
using UnityEngine.Events;
#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif

namespace EternityEngine
{
	public class EnumOption : Option
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
		public string enumTypeName;
		public Type enumType;
		public int value;
		public bool hasFlags;
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
		public bool updateChildren;
#endif

		public override void Init ()
		{
			base.Init ();
			enumType = Type.GetType(enumTypeName);
		}

		public override void Awake ()
		{
			base.Awake ();
#if UNITY_EDITOR
			if (!Application.isPlaying)
			{
				enumType = Type.GetType(enumTypeName);
				return;
			}
#endif
			children[value].SetActivatable (false);
			for (int i = 0; i < children.Count; i ++)
			{
				Option child = children[i];
#if USE_UNITY_EVENTS
				child.onStartActivateUnityEvent.RemoveAllListeners();
				if (hasFlags)
					child.onStartActivateUnityEvent.AddListener((LogicModule.Hand hand) => { ToggleFlagsValue (children.IndexOf(child)); });
				else
					child.onStartActivateUnityEvent.AddListener((LogicModule.Hand hand) => { SetValue (children.IndexOf(child)); });
#endif
#if USE_EVENTS
				child.onStartActivate.unityEvent.RemoveAllListeners();
				if (hasFlags)
					child.onStartActivate.unityEvent.AddListener((LogicModule.Hand hand) => { ToggleFlagsValue (children.IndexOf(child)); });
				else
					child.onStartActivate.unityEvent.AddListener((LogicModule.Hand hand) => { SetValue (children.IndexOf(child)); });
#endif
			}
		}

#if UNITY_EDITOR
		public override void OnValidate ()
		{
			if (addEvents)
			{
#if USE_UNITY_EVENTS
				for (int i = 0; i < children.Count; i ++)
				{
					Option child = children[i];
					child.text.text = Enum.GetName(enumType, i);
					child.onStartActivateUnityEvent.RemoveAllListeners();
					if (hasFlags)
						child.onStartActivateUnityEvent.AddListener((LogicModule.Hand hand) => { ToggleFlagsValue (children.IndexOf(child)); });
					else
						child.onStartActivateUnityEvent.AddListener((LogicModule.Hand hand) => { SetValue (children.IndexOf(child)); });
				}
				if (!LogicModule.Instance.unityEvents.Contains(onValueChangedUnityEvent))
				{
					LogicModule.instance.unityEvents.Add(onValueChangedUnityEvent);
					indexOfOnValueChangedUnityEvent = LogicModule.instance.unityEvents.Count - 1;
				}
#endif
#if USE_EVENTS
				for (int i = 0; i < children.Count; i ++)
				{
					Option child = children[i];
					child.text.text = Enum.GetName(enumType, i);
					child.onStartActivate.unityEvent.RemoveAllListeners();
					if (hasFlags)
						child.onStartActivate.unityEvent.AddListener((LogicModule.Hand hand) => { ToggleFlagsValue (children.IndexOf(child)); });
					else
						child.onStartActivate.unityEvent.AddListener((LogicModule.Hand hand) => { SetValue (children.IndexOf(child)); });
				}
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
			enumType = Type.GetType(enumTypeName);
			if (updateChildren)
			{
				int newChildCount = Enum.GetValues(enumType).Length;
				int changeInChildCount = newChildCount - children.Count;
				for (int i = 0; i < -changeInChildCount; i ++)
				{
					Option child = children[0];
					if (child != null)
						GameManager.DestroyOnNextEditorUpdate (child.gameObject);
					children.RemoveAt(0);
				}
				hasFlags = enumType.GetCustomAttributes(typeof(FlagsAttribute), false).Length > 0;
				if (hasFlags)
				{
					if (!EditorSceneManager.IsPreviewScene(gameObject.scene))
					{
						for (int i = 0; i < changeInChildCount; i ++)
							children.Add(ObjectPool.Instance.SpawnComponent<BoolOption>(LogicModule.Instance.boolOptionPrefab.prefabIndex, parent:childOptionsParent));
					}
					Array enumNames = Enum.GetNames(enumType);
					Array enumValues = Enum.GetValues(enumType);
					Enum enumValue = GetValue();
					for (int i = 0; i < children.Count; i ++)
					{
						BoolOption child = (BoolOption) children[i];
						string enumName = (string) enumNames.GetValue(i);
						child.trueText = enumName + ": True";
						child.falseText = enumName + ": False";
						Enum flagValue = (Enum) enumValues.GetValue(i);
						child.value = enumValue.HasFlag(flagValue);
#if USE_UNITY_EVENTS
						child.onStartActivateUnityEvent.RemoveAllListeners();
						child.onStartActivateUnityEvent.AddListener((LogicModule.Hand hand) => { ToggleFlagsValue (child.trs.GetSiblingIndex()); });
#endif
#if USE_EVENTS
						child.onStartActivate.unityEvent.RemoveAllListeners();
						child.onStartActivate.unityEvent.AddListener((LogicModule.Hand hand) => { ToggleFlagsValue (child.trs.GetSiblingIndex()); });
#endif
						child.HandleNaming ();
					}
				}
				else
				{
					for (int i = 0; i < changeInChildCount; i ++)
						children.Add(ObjectPool.Instance.SpawnComponent<Option>(LogicModule.Instance.optionPrefab.prefabIndex, parent:childOptionsParent));
					for (int i = 0; i < children.Count; i ++)
					{
						Option child = children[i];
						child.text.text = Enum.GetName(enumType, i);
#if USE_UNITY_EVENTS
						child.onStartActivateUnityEvent.RemoveAllListeners();
						child.onStartActivateUnityEvent.AddListener((LogicModule.Hand hand) => { SetValue (child.trs.GetSiblingIndex()); });
#endif
#if USE_EVENTS
						child.onStartActivate.unityEvent.RemoveAllListeners();
						child.onStartActivate.unityEvent.AddListener((LogicModule.Hand hand) => { SetValue (child.trs.GetSiblingIndex()); });
#endif
						child.HandleNaming ();
					}
				}
				updateChildren = false;
			}
			base.OnValidate ();
		}
#endif

		void SetValue (int value)
		{
			if (this.value == value)
				return;
			children[value].SetActivatable (false);
			children[this.value].SetActivatable (true);
			this.value = value;
#if USE_UNITY_EVENTS
			onValueChangedUnityEvent.Invoke();
#endif
#if USE_EVENTS
			onValueChanged.unityEvent.Invoke();
#endif
		}

		void ToggleFlagsValue (int value)
		{
			Array enumValues = Enum.GetValues(enumType);
			Enum flagValue = (Enum) enumValues.GetValue(value);
			Enum enumValue = (Enum) Enum.ToObject(enumType, this.value);
			if (enumValue.HasFlag(flagValue))
				this.value &= ~flagValue.GetHashCode();
			else
				this.value |= flagValue.GetHashCode();
#if USE_UNITY_EVENTS
			onValueChangedUnityEvent.Invoke();
#endif
#if USE_EVENTS
			onValueChanged.unityEvent.Invoke();
#endif
		}

		public Enum GetValue ()
		{
			return (Enum) Enum.ToObject(enumType, value);
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
			_Data.value = value;
		}

		public void SetValueFromData ()
		{
			if (hasFlags)
				value = _Data.value;
			else if (value != _Data.value)
			{
				children[_Data.value].SetActivatable (false);
				children[value].SetActivatable (true);
				value = _Data.value;
			}
		}

		[Serializable]
		public class Data : Option.Data
		{
			[SaveAndLoadValue]
			public int value;
			
			public override object MakeAsset ()
			{
				EnumOption enumOption = ObjectPool.instance.SpawnComponent<EnumOption>(LogicModule.instance.enumOptionPrefab.prefabIndex, parent:LogicModule.instance.sceneTrs);
				Apply (enumOption);
				if (!enumOption.isInitialized)
					enumOption.Init ();
				return enumOption;
			}

			public override void Apply (Asset asset)
			{
				base.Apply (asset);
				EnumOption enumOption = (EnumOption) asset;
				enumOption._Data = this;
				enumOption.SetValueFromData ();
			}
		}
	}
}