using System;
using Extensions;
using UnityEngine;
using System.Collections.Generic;

namespace EternityEngine
{
	public class AutomationOption : Option
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
		public Option recordStartActivateOptionsParent;
		public Option recordParentingOptionsParent;
		public Option recordCollidableOptionsParent;
		public Option automationSequenceEntriesParent;
		public Option timeMultiplierOption;
		public BoolOption pauseRecordingBoolOption;
		public BoolOption referenceNewOptionsBoolOption;
		public BoolOption recordLeftHandBoolOption;
		public BoolOption recordRightHandBoolOption;
		public BoolOption recordInvalidHandBoolOption;
		public BoolOption pausePlaybackBoolOption;
		public EnumOption invalidOptionReferenceBehaviourEnumOption;
		public EnumOption duplicateSequenceEntryIndexBehaviourEnumOption;
		public AutomationSequence automationSequence;
		public float recordingStartTime;
		public List<AutomationSequenceEntryOption> automationSequenceEntryOptions = new List<AutomationSequenceEntryOption>();
		// public static List<AutomationOption> currentRecording = new List<AutomationOption>();
		public static Dictionary<Option, List<AutomationOption>> recordingStartActivateOptionsDict = new Dictionary<Option, List<AutomationOption>>();
		public static Dictionary<Option, List<AutomationOption>> recordingParentingOptionsDict = new Dictionary<Option, List<AutomationOption>>();
		public static Dictionary<Option, List<AutomationOption>> recordingCollidableOptionsDict = new Dictionary<Option, List<AutomationOption>>();
		PlaybackHandler playbackHandler;

		public void Init (string name)
		{
			text.text = "\"" + name + "\" Automation Sequence";
			automationSequenceEntriesParent.onAddChild += OnAddChild;
			automationSequenceEntriesParent.onRemoveChild += OnRemoveChild;
		}

		public void StartRecording ()
		{
			recordingStartTime = Time.time;
			// currentRecording.Add(this);
			for (int i = 0; i < recordStartActivateOptionsParent.children.Count; i ++)
			{
				Option option = recordStartActivateOptionsParent.children[i];
				RegisterStartActivateOption (option);
			}
			recordStartActivateOptionsParent.onAddChild += RegisterStartActivateOption;
			recordStartActivateOptionsParent.onRemoveChild += UnregisterStartActivateOption;
			for (int i = 0; i < recordParentingOptionsParent.children.Count; i ++)
			{
				Option option = recordParentingOptionsParent.children[i];
				RegisterParentingOption (option);
			}
			recordParentingOptionsParent.onAddChild += RegisterParentingOption;
			recordParentingOptionsParent.onRemoveChild += UnregisterParentingOption;
			for (int i = 0; i < recordCollidableOptionsParent.children.Count; i ++)
			{
				Option option = recordCollidableOptionsParent.children[i];
				RegisterCollidableOption (option);
			}
			recordCollidableOptionsParent.onAddChild += RegisterCollidableOption;
			recordCollidableOptionsParent.onRemoveChild += UnregisterCollidableOption;
		}
		
		public void EndRecording ()
		{
			// currentRecording.Remove(this);
			foreach (KeyValuePair<Option, List<AutomationOption>> keyValuePair in recordingStartActivateOptionsDict)
				UnregisterStartActivateOption (keyValuePair.Key);
			recordStartActivateOptionsParent.onAddChild -= RegisterStartActivateOption;
			recordStartActivateOptionsParent.onRemoveChild -= UnregisterStartActivateOption;
			foreach (KeyValuePair<Option, List<AutomationOption>> keyValuePair in recordingParentingOptionsDict)
				UnregisterParentingOption (keyValuePair.Key);
			recordParentingOptionsParent.onAddChild -= RegisterParentingOption;
			recordParentingOptionsParent.onRemoveChild -= UnregisterParentingOption;
			foreach (KeyValuePair<Option, List<AutomationOption>> keyValuePair in recordingCollidableOptionsDict)
				UnregisterCollidableOption (keyValuePair.Key);
			recordCollidableOptionsParent.onAddChild -= RegisterCollidableOption;
			recordCollidableOptionsParent.onRemoveChild -= UnregisterCollidableOption;
		}

		public void StartPlaying ()
		{
			playbackHandler = new PlaybackHandler();
			playbackHandler.Init (this);
		}

		public void PausePlaying (bool pause)
		{
			if (pause)
				GameManager.updatables = GameManager.updatables.Remove(playbackHandler);
			else
				GameManager.updatables = GameManager.updatables.Add(playbackHandler);
		}

		public void EndPlayback ()
		{
			playbackHandler.End ();
		}

		public void SetTimeMultiplier ()
		{
			float timeMultiplier = float.Parse(timeMultiplierOption.GetValue());
			automationSequence.timeMultiplier = timeMultiplier;
		}

		void RegisterStartActivateOption (Option option)
		{
			if (!recordingStartActivateOptionsDict.ContainsKey(option))
			{
				recordingStartActivateOptionsDict.Add(option, new List<AutomationOption>() { this });
#if USE_UNITY_EVENTS
				option.onStartActivateUnityEvent.AddListener(option.OnStartedActivate);
#endif
#if USE_EVENTS
				option.onStartActivate.unityEvent.AddListener(option.OnStartedActivate);
#endif
				option.recordingActivateInAutomationSequence = true;
			}
			else
				recordingStartActivateOptionsDict[option].Add(this);
		}

		void UnregisterStartActivateOption (Option option)
		{
			recordingStartActivateOptionsDict[option].Remove(this);
			if (recordingStartActivateOptionsDict[option].Count == 0)
			{
#if USE_UNITY_EVENTS
				option.onStartActivateUnityEvent.RemoveListener(option.OnStartedActivate);
#endif
#if USE_EVENTS
				option.onStartActivate.unityEvent.RemoveListener(option.OnStartedActivate);
#endif
				recordingStartActivateOptionsDict.Remove(option);
				option.recordingActivateInAutomationSequence = false;
			}
		}

		void RegisterParentingOption (Option option)
		{
			if (!recordingParentingOptionsDict.ContainsKey(option))
			{
				recordingParentingOptionsDict.Add(option, new List<AutomationOption>() { this });
				option.onAddChild += option.OnAddedOrRemovedChild;
				option.onRemoveChild += option.OnAddedOrRemovedChild;
			}
			else
				recordingParentingOptionsDict[option].Add(this);
		}

		void UnregisterParentingOption (Option option)
		{
			recordingParentingOptionsDict[option].Remove(this);
			if (recordingParentingOptionsDict[option].Count == 0)
			{
				option.onAddChild -= option.OnAddedOrRemovedChild;
				option.onRemoveChild -= option.OnAddedOrRemovedChild;
				recordingParentingOptionsDict.Remove(option);
			}
		}

		void RegisterCollidableOption (Option option)
		{
			if (!recordingCollidableOptionsDict.ContainsKey(option))
			{
				recordingCollidableOptionsDict.Add(option, new List<AutomationOption>() { this });
				option.onSetCollidable += option.OnCollidableSet;
			}
			else
				recordingCollidableOptionsDict[option].Add(this);
		}

		void UnregisterCollidableOption (Option option)
		{
			recordingCollidableOptionsDict[option].Remove(this);
			if (recordingCollidableOptionsDict[option].Count == 0)
			{
				option.onSetCollidable -= option.OnCollidableSet;
				recordingCollidableOptionsDict.Remove(option);
			}
		}

		void OnAddChild (Option option)
		{
			AutomationSequenceEntryOption sequenceEntryOption = option as AutomationSequenceEntryOption;
			if (sequenceEntryOption != null)
			{
				int sequenceEntryIndex = int.Parse(sequenceEntryOption.sequenceEntryIndexOption.GetValue());
				if (sequenceEntryIndex >= 0 && sequenceEntryIndex < automationSequenceEntryOptions.Count)
				{
					DuplicateSequenceEntryIndexBehaviour duplicateSequenceEntryIndexBehaviour = (DuplicateSequenceEntryIndexBehaviour) duplicateSequenceEntryIndexBehaviourEnumOption.GetValue();
					if (duplicateSequenceEntryIndexBehaviour == DuplicateSequenceEntryIndexBehaviour.PreferSwitch)
					{
						AutomationSequenceEntryOption mySequenceEntryOption = automationSequenceEntryOptions[sequenceEntryIndex];
						mySequenceEntryOption.sequenceEntry = sequenceEntryOption.sequenceEntry;
						automationSequence.sequenceEntries[sequenceEntryIndex] = sequenceEntryOption.sequenceEntry;
						mySequenceEntryOption.automationOptionsUsingMe.Remove(this);
						int indexOfMe = sequenceEntryOption.automationOptionsUsingMe.IndexOf(this);
						mySequenceEntryOption.automationOptionsUsingMe.Add(sequenceEntryOption.automationOptionsUsingMe[indexOfMe]);
					}
					else// if (duplicateSequenceEntryIndexBehaviour == DuplicateSequenceEntryIndexBehaviour.PreferInsert)
					{
						automationSequence.sequenceEntries.Insert(sequenceEntryIndex, sequenceEntryOption.sequenceEntry);
						automationSequenceEntryOptions.Insert(sequenceEntryIndex, sequenceEntryOption);
						for (int i = sequenceEntryIndex; i < automationSequenceEntryOptions.Count; i ++)
						{
							AutomationSequenceEntryOption mySequenceEntryOption = automationSequenceEntryOptions[i];
							mySequenceEntryOption.sequenceEntryIndexOption.SetValue ("" + (i + 1));
						}
					}
					sequenceEntryOption.automationOptionsUsingMe.Add(this);
					automationSequenceEntryOptions.Add(sequenceEntryOption);
				}
			}
		}

		void OnRemoveChild (Option option)
		{
			AutomationSequenceEntryOption sequenceEntryOption = option as AutomationSequenceEntryOption;
			if (sequenceEntryOption != null)
			{
				int sequenceEntryIndex = int.Parse(sequenceEntryOption.sequenceEntryIndexOption.GetValue());
				for (int i = 0; i < sequenceEntryOption.automationOptionsUsingMe.Count; i ++)
				{
					AutomationOption automationOption = sequenceEntryOption.automationOptionsUsingMe[i];
					sequenceEntryOption.automationOptionsUsingMe.RemoveAt(i);
					automationOption.automationSequenceEntryOptions.RemoveAt(i);
					automationSequence.sequenceEntries.RemoveAt(sequenceEntryIndex);
					for (int i2 = sequenceEntryIndex; i2 < automationSequenceEntryOptions.Count; i2 ++)
					{
						AutomationSequenceEntryOption sequenceEntryOption2 = automationSequenceEntryOptions[i2];
						sequenceEntryOption2.sequenceEntryIndexOption.SetValue ("" + (i2 - 1));
					}
					i --;
				}
			}
		}

		public override void SetData ()
		{
			if (_Data == null)
				_Data = new Data();
			base.SetData ();
			SetAutomationSequenceOfData ();
		}

		public void SetAutomationSequenceOfData ()
		{
			_Data.automationSequence = automationSequence;
		}

		public void SetAutomationSequenceFromData ()
		{
			automationSequence = _Data.automationSequence;
		}

		public class PlaybackHandler : IUpdatable
		{
			public AutomationOption automationOption;
			int sequenceEntryIndex;
			float playbackTime;

			public void Init (AutomationOption automationOption)
			{
				this.automationOption = automationOption;
			}

			public void Start ()
			{
				playbackTime = 0;
				GameManager.updatables = GameManager.updatables.Add(this);
			}

			public void DoUpdate ()
			{
				AutomationSequence.SequenceEntry sequenceEntry = automationOption.automationSequence.sequenceEntries[sequenceEntryIndex];
				while (playbackTime >= sequenceEntry.time * automationOption.automationSequence.timeMultiplier)
				{
					if (!sequenceEntry.activateOptionStart.Equals(null))
					{
						Option option = sequenceEntry.activateOptionStart.GetOption();
						if (option != null)
							option.StartActivate (sequenceEntry.GetHandThatMadeMe());
						else
						{
							AutomationSequence.InvalidOptionReferenceBehaviour invalidOptionReferenceBehaviour = sequenceEntry.activateOptionStart.invalidOptionReferenceBehaviour;
							if (invalidOptionReferenceBehaviour == AutomationSequence.InvalidOptionReferenceBehaviour.StopSequence)
							{
								End ();
								return;
							}
							else if (invalidOptionReferenceBehaviour == AutomationSequence.InvalidOptionReferenceBehaviour.ContinueSequence)
							{
								sequenceEntryIndex ++;
								if (sequenceEntryIndex >= automationOption.automationSequence.sequenceEntries.Count)
									break;
								sequenceEntry = automationOption.automationSequence.sequenceEntries[sequenceEntryIndex];
								continue;
							}
							else if (invalidOptionReferenceBehaviour == AutomationSequence.InvalidOptionReferenceBehaviour.DeleteSequenceEntryAndContinueSequence)
							{
								automationOption.automationSequence.sequenceEntries.RemoveAt(sequenceEntryIndex);
								if (sequenceEntryIndex >= automationOption.automationSequence.sequenceEntries.Count)
									break;
								sequenceEntry = automationOption.automationSequence.sequenceEntries[sequenceEntryIndex];
								continue;
							}
							else if (invalidOptionReferenceBehaviour == AutomationSequence.InvalidOptionReferenceBehaviour.DeleteSequenceEntryAndRestartSequence)
							{
								automationOption.automationSequence.sequenceEntries.RemoveAt(sequenceEntryIndex);
								End ();
								Start ();
								return;
							}
							else if (invalidOptionReferenceBehaviour == AutomationSequence.InvalidOptionReferenceBehaviour.DeleteSequenceEntryAndPauseSequence)
							{
								automationOption.automationSequence.sequenceEntries.RemoveAt(sequenceEntryIndex);
								automationOption.pausePlaybackBoolOption.SetValue (true);
								return;
							}
							else if (invalidOptionReferenceBehaviour == AutomationSequence.InvalidOptionReferenceBehaviour.RestartSequence)
							{
								End ();
								Start ();
								return;
							}
							else// if (invalidOptionReferenceBehaviour == AutomationSequence.InvalidOptionReferenceBehaviour.PauseSequence)
							{
								automationOption.pausePlaybackBoolOption.SetValue (true);
								return;
							}
						}
					}
					else if (!sequenceEntry.activateOptionEnd.Equals(null))
					{
						Option option = sequenceEntry.activateOptionEnd.GetOption();
						if (option != null)
							option.EndActivate (sequenceEntry.GetHandThatMadeMe());
						else
						{
							AutomationSequence.InvalidOptionReferenceBehaviour invalidOptionReferenceBehaviour = sequenceEntry.activateOptionEnd.invalidOptionReferenceBehaviour;
							if (invalidOptionReferenceBehaviour == AutomationSequence.InvalidOptionReferenceBehaviour.StopSequence)
							{
								End ();
								return;
							}
							else if (invalidOptionReferenceBehaviour == AutomationSequence.InvalidOptionReferenceBehaviour.ContinueSequence)
							{
								sequenceEntryIndex ++;
								if (sequenceEntryIndex >= automationOption.automationSequence.sequenceEntries.Count)
									break;
								sequenceEntry = automationOption.automationSequence.sequenceEntries[sequenceEntryIndex];
								continue;
							}
							else if (invalidOptionReferenceBehaviour == AutomationSequence.InvalidOptionReferenceBehaviour.DeleteSequenceEntryAndContinueSequence)
							{
								automationOption.automationSequence.sequenceEntries.RemoveAt(sequenceEntryIndex);
								if (sequenceEntryIndex >= automationOption.automationSequence.sequenceEntries.Count)
									break;
								sequenceEntry = automationOption.automationSequence.sequenceEntries[sequenceEntryIndex];
								continue;
							}
							else if (invalidOptionReferenceBehaviour == AutomationSequence.InvalidOptionReferenceBehaviour.DeleteSequenceEntryAndRestartSequence)
							{
								automationOption.automationSequence.sequenceEntries.RemoveAt(sequenceEntryIndex);
								End ();
								Start ();
								return;
							}
							else if (invalidOptionReferenceBehaviour == AutomationSequence.InvalidOptionReferenceBehaviour.DeleteSequenceEntryAndPauseSequence)
							{
								automationOption.automationSequence.sequenceEntries.RemoveAt(sequenceEntryIndex);
								automationOption.pausePlaybackBoolOption.SetValue (true);
								return;
							}
							else if (invalidOptionReferenceBehaviour == AutomationSequence.InvalidOptionReferenceBehaviour.RestartSequence)
							{
								End ();
								Start ();
								return;
							}
							else// if (invalidOptionReferenceBehaviour == AutomationSequence.InvalidOptionReferenceBehaviour.PauseSequence)
							{
								automationOption.pausePlaybackBoolOption.SetValue (true);
								return;
							}
						}
					}
					else if (!sequenceEntry.parentOption.Equals(null))
					{
						Option parent = sequenceEntry.parentOption.GetOption();
						if (parent != null)
						{
							Option child = sequenceEntry.childOption.GetOption();
							if (child != null)
							{
								if (parent.children.Contains(child))
									parent.RemoveChild (child);
							}
							else
							{
								AutomationSequence.InvalidOptionReferenceBehaviour invalidOptionReferenceBehaviour = sequenceEntry.childOption.invalidOptionReferenceBehaviour;
								if (invalidOptionReferenceBehaviour == AutomationSequence.InvalidOptionReferenceBehaviour.StopSequence)
								{
									End ();
									return;
								}
								else if (invalidOptionReferenceBehaviour == AutomationSequence.InvalidOptionReferenceBehaviour.ContinueSequence)
								{
									sequenceEntryIndex ++;
									if (sequenceEntryIndex >= automationOption.automationSequence.sequenceEntries.Count)
										break;
									sequenceEntry = automationOption.automationSequence.sequenceEntries[sequenceEntryIndex];
									continue;
								}
								else if (invalidOptionReferenceBehaviour == AutomationSequence.InvalidOptionReferenceBehaviour.DeleteSequenceEntryAndContinueSequence)
								{
									automationOption.automationSequence.sequenceEntries.RemoveAt(sequenceEntryIndex);
									if (sequenceEntryIndex >= automationOption.automationSequence.sequenceEntries.Count)
										break;
									sequenceEntry = automationOption.automationSequence.sequenceEntries[sequenceEntryIndex];
									continue;
								}
								else if (invalidOptionReferenceBehaviour == AutomationSequence.InvalidOptionReferenceBehaviour.DeleteSequenceEntryAndRestartSequence)
								{
									automationOption.automationSequence.sequenceEntries.RemoveAt(sequenceEntryIndex);
									End ();
									Start ();
									return;
								}
								else if (invalidOptionReferenceBehaviour == AutomationSequence.InvalidOptionReferenceBehaviour.DeleteSequenceEntryAndPauseSequence)
								{
									automationOption.automationSequence.sequenceEntries.RemoveAt(sequenceEntryIndex);
									automationOption.pausePlaybackBoolOption.SetValue (true);
									return;
								}
								else if (invalidOptionReferenceBehaviour == AutomationSequence.InvalidOptionReferenceBehaviour.RestartSequence)
								{
									End ();
									Start ();
									return;
								}
								else// if (invalidOptionReferenceBehaviour == AutomationSequence.InvalidOptionReferenceBehaviour.PauseSequence)
								{
									automationOption.pausePlaybackBoolOption.SetValue (true);
									return;
								}
							}
						}
						else
						{
							AutomationSequence.InvalidOptionReferenceBehaviour invalidOptionReferenceBehaviour = sequenceEntry.parentOption.invalidOptionReferenceBehaviour;
							if (invalidOptionReferenceBehaviour == AutomationSequence.InvalidOptionReferenceBehaviour.StopSequence)
							{
								End ();
								return;
							}
							else if (invalidOptionReferenceBehaviour == AutomationSequence.InvalidOptionReferenceBehaviour.ContinueSequence)
							{
								sequenceEntryIndex ++;
								if (sequenceEntryIndex >= automationOption.automationSequence.sequenceEntries.Count)
									break;
								sequenceEntry = automationOption.automationSequence.sequenceEntries[sequenceEntryIndex];
								continue;
							}
							else if (invalidOptionReferenceBehaviour == AutomationSequence.InvalidOptionReferenceBehaviour.DeleteSequenceEntryAndContinueSequence)
							{
								automationOption.automationSequence.sequenceEntries.RemoveAt(sequenceEntryIndex);
								if (sequenceEntryIndex >= automationOption.automationSequence.sequenceEntries.Count)
									break;
								sequenceEntry = automationOption.automationSequence.sequenceEntries[sequenceEntryIndex];
								continue;
							}
							else if (invalidOptionReferenceBehaviour == AutomationSequence.InvalidOptionReferenceBehaviour.DeleteSequenceEntryAndRestartSequence)
							{
								automationOption.automationSequence.sequenceEntries.RemoveAt(sequenceEntryIndex);
								End ();
								Start ();
								return;
							}
							else if (invalidOptionReferenceBehaviour == AutomationSequence.InvalidOptionReferenceBehaviour.DeleteSequenceEntryAndPauseSequence)
							{
								automationOption.automationSequence.sequenceEntries.RemoveAt(sequenceEntryIndex);
								automationOption.pausePlaybackBoolOption.SetValue (true);
								return;
							}
							else if (invalidOptionReferenceBehaviour == AutomationSequence.InvalidOptionReferenceBehaviour.RestartSequence)
							{
								End ();
								Start ();
								return;
							}
							else// if (invalidOptionReferenceBehaviour == AutomationSequence.InvalidOptionReferenceBehaviour.PauseSequence)
							{
								automationOption.pausePlaybackBoolOption.SetValue (true);
								return;
							}
						}
					}
					sequenceEntryIndex ++;
					if (sequenceEntryIndex >= automationOption.automationSequence.sequenceEntries.Count)
						break;
					sequenceEntry = automationOption.automationSequence.sequenceEntries[sequenceEntryIndex];
				}
				playbackTime += Time.deltaTime;
			}

			public void End ()
			{
				GameManager.updatables = GameManager.updatables.Remove(this);
			}
		}

		public enum DuplicateSequenceEntryIndexBehaviour
		{
			Disallow,
			PreferSwitch,
			PreferInsert
		}

		[Serializable]
		public class Data : Option.Data
		{
			[SaveAndLoadValue]
			public AutomationSequence automationSequence;
			
			public override object MakeAsset ()
			{
				AutomationOption automationOption = ObjectPool.instance.SpawnComponent<AutomationOption>(LogicModule.instance.automationOptionPrefab.prefabIndex, parent:LogicModule.instance.sceneTrs);
				Apply (automationOption);
				return automationOption;
			}

			public override void Apply (Asset asset)
			{
				base.Apply (asset);
				AutomationOption automationOption = (AutomationOption) asset;
				automationOption._Data = this;
				automationOption.SetAutomationSequenceFromData ();
			}
		}
	}
}