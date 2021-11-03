using TMPro;
using System;
using Extensions;
using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Serialization;
#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif

namespace EternityEngine
{
	public class Option : Asset
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
		public virtual string nameToValueSeparator
		{
			get
			{
				return ":\n";
			}
		}
		public bool collidable;
		public bool activatable;
		public ColorOffset notActivatableColorOffset;
		public ColorOffset selectedColorOffset;
		public ColorOffset noCollisionColorOffset;
#if USE_UNITY_EVENTS
		public UnityEvent<LogicModule.Hand> onStartActivateUnityEvent;
		[HideInInspector]
		public int indexOfOnStartActivateUnityEvent;
#endif
#if USE_EVENTS
		public HandEvent onStartActivate;
		[HideInInspector]
		public int indexOfOnStartActivate;
#endif
		[HideInInspector]
		public Rigidbody rigid;
		[HideInInspector]
		public Collider collider;
		[HideInInspector]
		public Renderer renderer;
		[HideInInspector]
		public Transform uiTrs;
		[HideInInspector]
		public Transform childOptionsParent;
		[HideInInspector]
		public TMP_Text text;
		[HideInInspector]
		public bool justEnabled;
		[HideInInspector]
		public bool previousJustEnabled;
		[HideInInspector]
		public bool isActivated;
		// [HideInInspector]
		public List<Option> children = new List<Option>();
		[HideInInspector]
		public List<ParentingArrow> parentingArrows = new List<ParentingArrow>();
		[HideInInspector]
		public List<ControlFlowArrow> controlFlowArrows = new List<ControlFlowArrow>();
		[HideInInspector]
		public List<OptionConnectionArrow> optionConnectionArrows = new List<OptionConnectionArrow>();
		public delegate void OnAddChild(Option child);
		public event OnAddChild onAddChild;
		public delegate void OnRemoveChild(Option child);
		public event OnRemoveChild onRemoveChild;
		public delegate bool OnAboutToAddChild(Option child);
		public event OnAboutToAddChild onAboutToAddChild;
		public delegate bool OnAboutToRemoveChild(Option child);
		public event OnAboutToRemoveChild onAboutToRemoveChild;
		public delegate void OnSetCollidable(LogicModule.Hand hand);
		public event OnSetCollidable onSetCollidable;
		[HideInInspector]
		public bool recordingActivateInAutomationSequence;
		[HideInInspector]
		public bool isInitialized;
#if UNITY_EDITOR
		public bool addEvents;
		public bool removeEvents;
		List<Option> previousChildren = new List<Option>();
#endif
		public static List<Option> instances = new List<Option>();
		public AnimationEntry activateAnimationEntry;
		bool isDespawned;

		public override void Awake ()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
			{
				if (trs == null)
					trs = GetComponent<Transform>();
				if (rigid == null)
					rigid = GetComponent<Rigidbody>();
				if (collider == null)
					collider = GetComponent<Collider>();
				if (renderer == null)
					renderer = GetComponent<Renderer>();
				if (uiTrs == null)
					uiTrs = trs.Find("Canvas (World)");
				if (childOptionsParent == null)
					childOptionsParent = trs.Find("Children");
				if (text == null)
					text = uiTrs.Find("Text").GetComponent<TMP_Text>();
				if (activateAnimationEntry.Equals(default(AnimationEntry)))
				{
					activateAnimationEntry.animator = GetComponent<Animator>();
					activateAnimationEntry.animatorStateName = "Activate";
				}
				return;
			}
#endif
			if (!isInitialized && string.IsNullOrEmpty(SaveAndLoadManager.MostRecentSaveFileName))
				Init ();
		}

		void OnEnable ()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
			{
				SetActivatable (activatable);
				SetCollidable (null, collidable);
				if (trs.parent == LogicModule.Instance.sceneTrs)
					HandleNaming ();
				children.Clear();
				for (int i = 0; i < childOptionsParent.childCount; i ++)
				{
					Transform child = childOptionsParent.GetChild(i);
					Option option = child.GetComponent<Option>();
					children.Add(option);
				}
				return;
			}
#endif
			isDespawned = false;
			justEnabled = true;
			if (collidable)
				StartCoroutine(UpdateOptionConnectionArrowsRoutine ());
			instances.Add(this);
			for (int i = 0; i < parentingArrows.Count; i ++)
			{
				ParentingArrow parentingArrow = parentingArrows[i];
				parentingArrow.gameObject.SetActive(true);
			}
			for (int i = 0; i < children.Count; i ++)
			{
				Option child = children[i];
				if (child.gameObject.activeInHierarchy)
				{
					for (int i2 = 0; i2 < child.parentingArrows.Count; i2 ++)
					{
						ParentingArrow parentingArrow = child.parentingArrows[i2];
						if (parentingArrow.parent == this)
						{
							parentingArrow.gameObject.SetActive(true);
							break;
						}
					}
				}
			}
		}

		public virtual void Init ()
		{
			isInitialized = true;
			if (!activatable)
				renderer.material.color = notActivatableColorOffset.ApplyWithTransparency(renderer.material.color);
			HandleNaming ();
			for (int i = 0; i < children.Count; i ++)
			{
				Option child = children[i];
				ParentingArrow parentingArrow = ObjectPool.Instance.SpawnComponent<ParentingArrow>(LogicModule.instance.parentingArrowPrefab.prefabIndex, parent:trs);
				parentingArrow.pointsTo = child.trs;
				parentingArrow.child = child;
				parentingArrow.parent = this;
				parentingArrow.DoUpdate ();
				parentingArrow.gameObject.SetActive(child.gameObject.activeInHierarchy);
				child.parentingArrows.Add(parentingArrow);
			}
		}

		IEnumerator UpdateOptionConnectionArrowsRoutine ()
		{
			Vector3 previousPosition = trs.position;
			do
			{
				for (int i = 0; i < optionConnectionArrows.Count; i ++)
				{
					OptionConnectionArrow optionConnectionArrow = optionConnectionArrows[i];
					optionConnectionArrow.DoUpdate ();
				}
				previousPosition = trs.position;
				yield return new WaitForEndOfFrame();
			} while (trs.position != previousPosition);
		}

#if UNITY_EDITOR
		public virtual void OnValidate ()
		{
			if (addEvents)
			{
#if USE_UNITY_EVENTS
				if (!LogicModule.Instance.handUnityEvents.Contains(onStartActivateUnityEvent))
				{
					LogicModule.instance.handUnityEvents.Add(onStartActivateUnityEvent);
					indexOfOnStartActivateUnityEvent = LogicModule.instance.handUnityEvents.Count - 1;
				}
#endif
#if USE_EVENTS
				if (!LogicModule.Instance.handEvents.Contains(onStartActivate))
				{
					LogicModule.instance.handEvents.Add(onStartActivate);
					indexOfOnStartActivate = LogicModule.instance.handEvents.Count - 1;
				}
#endif
				addEvents = false;
			}
			if (removeEvents)
			{
#if USE_UNITY_EVENTS
				LogicModule.Instance.handUnityEvents.Remove(onStartActivateUnityEvent);
#endif
#if USE_EVENTS
				LogicModule.Instance.handEvents.Remove(onStartActivate);
#endif
				removeEvents = false;
			}
			if (Application.isPlaying)
			{
				if (children != previousChildren)
				{
					for (int i = 0; i < previousChildren.Count; i ++)
					{
						Option previousChild = previousChildren[i];
						if (previousChild != null && !children.Contains(previousChild))
						{
							RemoveChild (previousChild);
							for (int i2 = 0; i2 < previousChild.parentingArrows.Count; i2 ++)
							{
								ParentingArrow parentingArrow = previousChild.parentingArrows[i2];
								if (parentingArrow.parent == this)
								{
									previousChild.parentingArrows.RemoveAt(i);
									ObjectPool.Instance.Despawn (parentingArrow.prefabIndex, parentingArrow.gameObject, parentingArrow.trs);
								}
							}
						}
					}
					List<Option> addedChildren = new List<Option>();
					for (int i = 0; i < children.Count; i ++)
					{
						Option child = children[i];
						if (child != null && !previousChildren.Contains(child))
							addedChildren.Add(child);
					}
					previousChildren = new List<Option>(children);
					children.Clear();
					for (int i = 0; i < addedChildren.Count; i ++)
					{
						Option addedChild = addedChildren[i];
						AddChild (addedChild);
						ParentingArrow parentingArrow = ObjectPool.Instance.SpawnComponent<ParentingArrow>(LogicModule.instance.parentingArrowPrefab.prefabIndex, parent:trs);
						parentingArrow.pointsTo = addedChild.trs;
						parentingArrow.child = addedChild;
						parentingArrow.parent = this;
						parentingArrow.DoUpdate ();
						parentingArrow.gameObject.SetActive(addedChild.gameObject.activeInHierarchy);
						addedChild.parentingArrows.Add(parentingArrow);
					}
				}
			}
			enabled = !enabled;
			enabled = !enabled;
		}
#endif

		public void StartActivate (LogicModule.Hand hand)
		{
#if USE_UNITY_EVENTS
			onStartActivateUnityEvent.Invoke(hand);
#endif
#if USE_EVENTS
			onStartActivate.unityEvent.Invoke(hand);
#endif
			isActivated = true;
			for (int i = 0; i < controlFlowArrows.Count; i ++)
			{
				ControlFlowArrow controlFlowArrow = controlFlowArrows[i];
				controlFlowArrow.child.StartActivate (null);
			}
			activateAnimationEntry.Play(0);
		}

		public virtual void OnStartedActivate (LogicModule.Hand hand)
		{
			List<AutomationOption> automationOptions = AutomationOption.recordingStartActivateOptionsDict[this];
			for (int i = 0; i < automationOptions.Count; i ++)
			{
				AutomationOption automationOption = automationOptions[i];
				if (!automationOption.pauseRecordingBoolOption.value && ((automationOption.recordLeftHandBoolOption.value && hand.isLeftHand) || (automationOption.recordRightHandBoolOption.value && !hand.isLeftHand)))
				{
					AutomationSequence.SequenceEntry sequenceEntry = new AutomationSequence.SequenceEntry();
#if USE_UNITY_EVENTS
					onStartActivateUnityEvent.RemoveListener(OnStartedActivate);
#endif
#if USE_EVENTS
					onStartActivate.unityEvent.RemoveListener(OnStartedActivate);
#endif
					SetData ();
#if USE_UNITY_EVENTS
					onStartActivateUnityEvent.AddListener(OnStartedActivate);
#endif
#if USE_EVENTS
					onStartActivate.unityEvent.AddListener(OnStartedActivate);
#endif
					AutomationSequence.InvalidOptionReferenceBehaviour invalidOptionReferenceBehaviour = (AutomationSequence.InvalidOptionReferenceBehaviour) automationOption.invalidOptionReferenceBehaviourEnumOption.GetValue();
					sequenceEntry.activateOptionStart = new AutomationSequence.SequenceEntry.OptionEntry(_Data, invalidOptionReferenceBehaviour);
					sequenceEntry.time = Time.time - automationOption.recordingStartTime;
					automationOption.automationSequence.sequenceEntries.Add(sequenceEntry);
				}
			}
		}

		public void EndActivate (LogicModule.Hand hand)
		{
			isActivated = false;
		}

		public void SetActivatable (bool activatable)
		{
			if (activatable && !this.activatable)
			{
				renderer.material.color = notActivatableColorOffset.ApplyInverseWithTransparency(renderer.material.color);
				renderer.material.SetColor("_EmissionColor", notActivatableColorOffset.ApplyInverseWithTransparency(renderer.material.GetColor("_EmissionColor")));
			}
			else if (!activatable && this.activatable)
			{
				renderer.material.color = notActivatableColorOffset.ApplyWithTransparency(renderer.material.color);
				renderer.material.SetColor("_EmissionColor", notActivatableColorOffset.ApplyWithTransparency(renderer.material.GetColor("_EmissionColor")));
			}
			this.activatable = activatable;
		}

		public void SetCollidable (LogicModule.Hand hand, bool collidable)
		{
			collider.isTrigger = !collidable;
			if (collidable && !this.collidable)
			{
				renderer.material.color = noCollisionColorOffset.ApplyInverseWithTransparency(renderer.material.color);
				StartCoroutine(UpdateOptionConnectionArrowsRoutine ());
			}
			else if (!collidable && this.collidable)
				renderer.material.color = noCollisionColorOffset.ApplyWithTransparency(renderer.material.color);
			this.collidable = collidable;
			if (onSetCollidable != null)
				onSetCollidable (hand);
		}

		public virtual void OnCollidableSet (LogicModule.Hand hand)
		{
			List<AutomationOption> automationOptions = AutomationOption.recordingCollidableOptionsDict[this];
			for (int i = 0; i < automationOptions.Count; i ++)
			{
				AutomationOption automationOption = automationOptions[i];
				if (!automationOption.pauseRecordingBoolOption.value && ((automationOption.recordLeftHandBoolOption.value && hand.isLeftHand) || (automationOption.recordRightHandBoolOption.value && !hand.isLeftHand)))
				{
					AutomationSequence.SequenceEntry sequenceEntry = new AutomationSequence.SequenceEntry();
#if USE_UNITY_EVENTS
					if (recordingActivateInAutomationSequence)
						onStartActivateUnityEvent.RemoveListener(OnStartedActivate);
#endif
#if USE_EVENTS
					if (recordingActivateInAutomationSequence)
						onStartActivate.unityEvent.RemoveListener(OnStartedActivate);
#endif
					SetData ();
#if USE_UNITY_EVENTS
					if (recordingActivateInAutomationSequence)
						onStartActivateUnityEvent.AddListener(OnStartedActivate);
#endif
#if USE_EVENTS
					if (recordingActivateInAutomationSequence)
						onStartActivate.unityEvent.AddListener(OnStartedActivate);
#endif
					AutomationSequence.InvalidOptionReferenceBehaviour invalidOptionReferenceBehaviour = (AutomationSequence.InvalidOptionReferenceBehaviour) Enum.ToObject(typeof(AutomationSequence.InvalidOptionReferenceBehaviour), automationOption.invalidOptionReferenceBehaviourEnumOption.value);
					sequenceEntry.collidableOption = new AutomationSequence.SequenceEntry.OptionEntry(_Data, invalidOptionReferenceBehaviour);
					sequenceEntry.time = Time.time - automationOption.recordingStartTime;
					automationOption.automationSequence.sequenceEntries.Add(sequenceEntry);
				}
			}
		}

		public void HandleNaming ()
		{
			string nameAndValue = text.text;
			string newName = nameAndValue;
			int indexOfNameToValueSeparator = newName.IndexOf(nameToValueSeparator);
			if (indexOfNameToValueSeparator != -1)
				newName = newName.Remove(indexOfNameToValueSeparator);
#if UNITY_EDITOR
			if (!EditorSceneManager.IsPreviewScene(gameObject.scene) && LogicModule.Instance != null)
			{
#endif
				LogicModule.Instance.optionNamesDict.Remove(this);
				if (LogicModule.instance.optionNamesDict.ContainsValue(newName))
				{
					int nameOccuranceCount = 1;
					while (LogicModule.instance.optionNamesDict.ContainsValue(newName + " (" + nameOccuranceCount + ")"))
						nameOccuranceCount ++;
					newName += " (" + nameOccuranceCount + ")";
				}
				LogicModule.instance.optionNamesDict[this] = newName;
#if UNITY_EDITOR
			}
#endif
			name = newName;
			string newNameAndValue = "" + newName;
			string value = GetValue();
			if (value != null)
				newNameAndValue += nameToValueSeparator + value;
			text.text = newNameAndValue;
		}

		public virtual string GetValue ()
		{
			string output = null;
			int indexOfNameToValueSeparator = text.text.IndexOf(nameToValueSeparator);
			if (indexOfNameToValueSeparator != -1)
				output = text.text.Substring(indexOfNameToValueSeparator + nameToValueSeparator.Length);
			return output;
		}

		public void SetValue (string value)
		{
			string nameAndValue = text.text;
			int indexOfNameToValueSeparator = nameAndValue.IndexOf(nameToValueSeparator);
			if (indexOfNameToValueSeparator != -1)
				nameAndValue = nameAndValue.Remove(indexOfNameToValueSeparator);
			text.text = nameAndValue + nameToValueSeparator + value;
		}

		public void AddChild (Option childOption)
		{
			if (onAboutToAddChild != null && !onAboutToAddChild(childOption))
				return;
			children.Add(childOption);
			if (onAddChild != null)
				onAddChild (childOption);
		}

		public void RemoveChild (Option childOption)
		{
			if (onAboutToRemoveChild != null && !onAboutToRemoveChild(childOption))
				return;
			children.Remove(childOption);
			for (int i = 0; i < childOption.parentingArrows.Count; i ++)
			{
				ParentingArrow parentingArrow = childOption.parentingArrows[i];
				if (parentingArrow.parent == this)
				{
					ObjectPool.instance.Despawn (parentingArrow.prefabIndex, parentingArrow.gameObject, parentingArrow.trs);
					childOption.parentingArrows.RemoveAt(i);
					break;
				}
			}
			if (onRemoveChild != null)
				onRemoveChild (childOption);
		}

		public virtual void OnAddedOrRemovedChild (Option childOption)
		{
			LogicModule.Hand hand = LogicModule.instance.leftHand;
			if (hand.optionConnectionArrow.parent != this || hand.optionConnectionArrow.child != childOption)
				hand = LogicModule.instance.rightHand;
			List<AutomationOption> automationOptions = AutomationOption.recordingParentingOptionsDict[this];
			for (int i = 0; i < automationOptions.Count; i ++)
			{
				AutomationOption automationOption = automationOptions[i];
				if (!automationOption.pauseRecordingBoolOption.value && ((automationOption.recordLeftHandBoolOption.value && hand.isLeftHand) || (automationOption.recordRightHandBoolOption.value && !hand.isLeftHand)))
				{
					AutomationSequence.SequenceEntry sequenceEntry = new AutomationSequence.SequenceEntry();
#if USE_UNITY_EVENTS
					if (recordingActivateInAutomationSequence)
						onStartActivateUnityEvent.RemoveListener(OnStartedActivate);
#endif
#if USE_EVENTS
					if (recordingActivateInAutomationSequence)
						onStartActivate.unityEvent.RemoveListener(OnStartedActivate);
#endif
					SetData ();
#if USE_UNITY_EVENTS
					if (recordingActivateInAutomationSequence)
						onStartActivateUnityEvent.AddListener(OnStartedActivate);
#endif
#if USE_EVENTS
					if (recordingActivateInAutomationSequence)
						onStartActivate.unityEvent.AddListener(OnStartedActivate);
#endif
					AutomationSequence.InvalidOptionReferenceBehaviour invalidOptionReferenceBehaviour = (AutomationSequence.InvalidOptionReferenceBehaviour) automationOption.invalidOptionReferenceBehaviourEnumOption.GetValue();
					sequenceEntry.parentOption = new AutomationSequence.SequenceEntry.OptionEntry(_Data, invalidOptionReferenceBehaviour);
					sequenceEntry.childOption = new AutomationSequence.SequenceEntry.OptionEntry(childOption._Data, invalidOptionReferenceBehaviour);
					sequenceEntry.time = Time.time - automationOption.recordingStartTime;
					automationOption.automationSequence.sequenceEntries.Add(sequenceEntry);
				}
			}
		}

		public virtual void OnDisable ()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
				return;
#endif
			instances.Remove(this);
			if (trs.parent == ObjectPool.instance.trs)
				OnDespawned ();
			else if (!GameManager.isQuitting)
			{
				for (int i = 0; i < parentingArrows.Count; i ++)
				{
					ParentingArrow parentingArrow = parentingArrows[i];
					parentingArrow.gameObject.SetActive(false);
				}
			}
		}

		void OnDestroy ()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
				return;
#endif
			if (!isDespawned)
				OnDespawned ();
		}

		public virtual void OnDespawned ()
		{
			isDespawned = true;
			for (int i = 0; i < parentingArrows.Count; i ++)
			{
				ParentingArrow parentingArrow = parentingArrows[i];
				parentingArrow.parent.RemoveChild (this);
				i --;
			}
			if (LogicModule.Hand.grabbingOption == this)
				LogicModule.Hand.DropGrabbedTransform ();
			AutomationOption.recordingStartActivateOptionsDict.Remove(this);
			AutomationOption.recordingParentingOptionsDict.Remove(this);
			AutomationOption.recordingCollidableOptionsDict.Remove(this);
		}

		public static Option[] GetAllChildrenAndSelf (Option option)
		{
			List<Option> output = new List<Option>();
			List<Option> checkedOptions = new List<Option>() { option };
			List<Option> remainingOptions = new List<Option>() { option };
			while (remainingOptions.Count > 0)
			{
				Option option2 = remainingOptions[0];
				output.Add(option2);
				for (int i = 0; i < option2.children.Count; i ++)
				{
					Option connectedOption = option2.children[i];
					if (!checkedOptions.Contains(connectedOption))
					{
						checkedOptions.Add(connectedOption);
						remainingOptions.Add(connectedOption);
					}
				}
				remainingOptions.RemoveAt(0);
			}
			return output.ToArray();
		}

		public override void SetData ()
		{
			if (_Data == null)
				_Data = new Data();
			SetNameOfData ();
			SetTextOfData ();
			SetActiveOfData ();
			SetIsDespawnedOfData ();
#if USE_UNITY_EVENTS
			SetIndexOfOnStartActivateUnityEventOfData ();
#endif
#if USE_EVENTS
			SetIndexOfOnStartActivateOfData ();
#endif
			SetActivatableOfData ();
			SetPositionOfData ();
			SetEulerAnglesOfData ();
			SetSizeOfData ();
			SetChildNamesOfData ();
			SetCollidableOfData ();
		}

		public void SetNameOfData ()
		{
			_Data.name = name;
		}

		public void SetNameFromData ()
		{
			name = _Data.name;
		}

		public void SetTextOfData ()
		{
			_Data.text = text.text;
		}

		public void SetTextFromData ()
		{
			text.text = _Data.text;
		}

		public void SetActiveOfData ()
		{
			_Data.active = gameObject.activeSelf;
		}

		public void SetActiveFromData ()
		{
			gameObject.SetActive(_Data.active);
		}

		public void SetIsDespawnedOfData ()
		{
			_Data.isDespawned = isDespawned;
		}

		public void SetIsDespawnedFromData ()
		{
			if (_Data.isDespawned)
				ObjectPool.instance.Despawn (prefabIndex, gameObject, trs);
		}

#if USE_UNITY_EVENTS
		public void SetIndexOfOnStartActivateUnityEventOfData ()
		{
			_Data.indexOfOnStartActivateUnityEvent = indexOfOnStartActivateUnityEvent;
		}

		public void SetIndexOfOnStartActivateUnityEventFromData ()
		{
			indexOfOnStartActivateUnityEvent = _Data.indexOfOnStartActivateUnityEvent;
			if (indexOfOnStartActivateUnityEvent != -1)
				onStartActivateUnityEvent = LogicModule.instance.handUnityEvents[indexOfOnStartActivateUnityEvent];
		}
#endif

#if USE_EVENTS
		public void SetIndexOfOnStartActivateOfData ()
		{
			_Data.indexOfOnStartActivate = indexOfOnStartActivate;
		}

		public void SetIndexOfOnStartActivateFromData ()
		{
			indexOfOnStartActivate = _Data.indexOfOnStartActivate;
			if (indexOfOnStartActivate != -1)
				onStartActivate = LogicModule.instance.handEvents[indexOfOnStartActivate];
		}
#endif

		public void SetActivatableOfData ()
		{
			_Data.activatable = activatable;
		}

		public void SetActivatableFromData ()
		{
			SetActivatable (_Data.activatable);
		}

		public void SetPositionOfData ()
		{
			_Data.position = trs.position;
		}

		public void SetPositionFromData ()
		{
			trs.position = _Data.position;
		}

		public void SetEulerAnglesOfData ()
		{
			_Data.eulerAngles = trs.eulerAngles;
		}

		public void SetEulerAnglesFromData ()
		{
			trs.eulerAngles = _Data.eulerAngles;
		}

		public void SetSizeOfData ()
		{
			_Data.size = trs.lossyScale.x;
		}

		public void SetSizeFromData ()
		{
			trs.SetWorldScale (Vector3.one * _Data.size);
		}

		public void SetChildNamesOfData ()
		{
			_Data.childNames = new string[children.Count];
			for (int i = 0; i < children.Count; i ++)
			{
				Option child = children[i];
				_Data.childNames[i] = child.name;
			}
		}

		public void SetChildNamesFromData ()
		{
			children.Clear();
			for (int i = 0; i < _Data.childNames.Length; i ++)
			{
				string name = _Data.childNames[i];
				int indexOfName = LogicModule.instance.optionNamesDict.values.IndexOf(name);
				if (indexOfName != -1)
				{
					Option option = LogicModule.instance.optionNamesDict.keys[indexOfName];
					children.Add(option);
				}
				else
				{
					for (int i2 = 0; i2 < GameManager.instance.assetsData.Count; i2 ++)
					{
						Asset.Data data = GameManager.instance.assetsData[i2];
						if (data.name == name)
						{
							Option option = (Option) data.MakeAsset();
							children.Add(option);
							break;
						}
					}
				}
			}
		}

		public void SetCollidableOfData ()
		{
			_Data.collidable = collidable;
		}

		public void SetCollidableFromData ()
		{
			SetCollidable (null, _Data.collidable);
		}

#if USE_EVENTS
		[Serializable]
		public class Event
		{
			public UnityEvent unityEvent;
		}

		[Serializable]
		public class Event<T>
		{
			public UnityEvent<T> unityEvent;
		}

		[Serializable]
		public class HandEvent : Event<LogicModule.Hand>
		{
		}
#endif

		[Flags]
		public enum TriggerType
		{
			Always = 1,
			Input = 2,
			Startup = 4,
			Load = 8,
			Quit = 16,
			Save = 32,
			StartInteract = 64,
			EndInteract = 128,
			Delete = 256,
			AddChild = 512,
			RemoveChild = 1024,
			Hidden = 2048,
			Visible = 4096,
			Collidable = 8192,
			Uncollidable = 16384,
			NameChange = 32768,
			InvalidNameChangeAttempt = 65536,
			ValueChange = 131072,
			InvalidValueChangeAttempt = 262144,
			TypeCharacter = 524288,
			TypeBackspace = 1048576,
			ChangeTypingTarget = 2097152,
			Activatable = 4194304,
			Unactivatable = 8388608,
			Duplicate = 16777216,
			Grab = 33554432,
			Drop = 67108864,
			Activate = 134217728
		}

		[Serializable]
		public class Data : Asset.Data
		{
			[SaveAndLoadValue]
			public string text;
			[SaveAndLoadValue]
			public bool active;
			[SaveAndLoadValue]
			public bool isDespawned;
#if USE_UNITY_EVENTS
			[SaveAndLoadValue]
			public int indexOfOnStartActivateUnityEvent;
#endif
#if USE_EVENTS
			[SaveAndLoadValue]
			public int indexOfOnStartActivate;
#endif
			[SaveAndLoadValue]
			public bool activatable;
			[SaveAndLoadValue]
			public Vector3 position;
			[SaveAndLoadValue]
			public Vector3 eulerAngles;
			[SaveAndLoadValue]
			public float size;
			[SaveAndLoadValue]
			public string[] childNames = new string[0];
			[SaveAndLoadValue]
			public bool collidable;
			
			public override object MakeAsset ()
			{
				Option option = ObjectPool.instance.SpawnComponent<Option>(LogicModule.instance.optionPrefab.prefabIndex, parent:LogicModule.instance.sceneTrs);
				Apply (option);
				if (!option.isInitialized)
					option.Init ();
				return option;
			}

			public override void Apply (Asset asset)
			{
				Option option = (Option) asset;
				option._Data = this;
				option.SetNameFromData ();
				option.SetTextFromData ();
				option.SetActiveFromData ();
				option.SetIsDespawnedFromData ();
#if USE_UNITY_EVENTS
				option.SetIndexOfOnStartActivateUnityEventFromData ();
#endif
#if USE_EVENTS
				option.SetIndexOfOnStartActivateFromData ();
#endif
				option.SetActivatableFromData ();
				option.SetPositionFromData ();
				option.SetEulerAnglesFromData ();
				option.SetSizeFromData ();
				option.SetChildNamesFromData ();
				option.SetCollidableFromData ();
			}
		}
	}
}