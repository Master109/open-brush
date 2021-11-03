using System;
using Extensions;
using UnityEngine;
using System.Collections.Generic;

namespace EternityEngine
{
	[Serializable]
	public struct AutomationSequence
	{
		public List<SequenceEntry> sequenceEntries;
		public float timeMultiplier;

		public AutomationSequence (List<SequenceEntry> sequenceEntries, float timeMultiplier)
		{
			this.sequenceEntries = sequenceEntries;
			this.timeMultiplier = timeMultiplier;
		}

		[Serializable]
		public struct SequenceEntry
		{
			public OptionEntry activateOptionStart;
			public OptionEntry activateOptionEnd;
			public OptionEntry parentOption;
			public OptionEntry childOption;
			public OptionEntry collidableOption;
			public OptionEntry inputOrientationOption;
			public InputType inputType;
			public LogicModule.Hand.Type madeByHandType;
			public bool addChildAfterRecord;
			public bool removeChildAfterRecord;
			public float time;

			public SequenceEntry (OptionEntry activateOptionStart, OptionEntry activateOptionEnd, OptionEntry parentOption, OptionEntry childOption, OptionEntry collidableOption, OptionEntry inputOrientationOption, InputType inputType, LogicModule.Hand.Type madeByHandType, bool addChildAfterRecord, bool removeChildAfterRecord, float time)
			{
				this.activateOptionStart = activateOptionStart;
				this.activateOptionEnd = activateOptionEnd;
				this.parentOption = parentOption;
				this.childOption = childOption;
				this.collidableOption = collidableOption;
				this.inputOrientationOption = inputOrientationOption;
				this.inputType = inputType;
				this.madeByHandType = madeByHandType;
				this.addChildAfterRecord = addChildAfterRecord;
				this.removeChildAfterRecord = removeChildAfterRecord;
				this.time = time;
			}

			public LogicModule.Hand GetHandThatMadeMe ()
			{
				LogicModule.Hand output = null;
				if (madeByHandType == LogicModule.Hand.Type.Left)
					output = LogicModule.instance.leftHand;
				else if (madeByHandType == LogicModule.Hand.Type.Right)
					output = LogicModule.instance.rightHand;
				return output;
			}

			[Serializable]
			public struct OptionEntry
			{
				public Option.Data data;
				public InvalidOptionReferenceBehaviour invalidOptionReferenceBehaviour;

				public OptionEntry (Option.Data data, InvalidOptionReferenceBehaviour invalidOptionReferenceBehaviour)
				{
					this.data = data;
					this.invalidOptionReferenceBehaviour = invalidOptionReferenceBehaviour;
				}

				public Option GetOption ()
				{
					for (int i = 0; i < LogicModule.instance.optionNamesDict.Count; i ++)
					{
						Option option = LogicModule.instance.optionNamesDict.keys[i];
						if (option.name == data.name)
							return option;
					}
					if (invalidOptionReferenceBehaviour == InvalidOptionReferenceBehaviour.Spawn)
						return (Option) data.MakeAsset();
					else
						return null;
				}
			}
			
			public enum InputType
			{
				ThumbstickTouched,
				ThumbstickPressed,
				PrimaryButtonTouched,
				PrimaryButtonPressed,
				SecondaryButtonTouched,
				SecondaryButtonPressed,
				TriggerTouched,
				TriggerPressed,
				GripTouched,
				GripPressed,
				MenuButtonPressed,
			}
		}

		public enum InvalidOptionReferenceBehaviour
		{
			Spawn,
			StopSequence,
			ContinueSequence,
			DeleteSequenceEntryAndContinueSequence,
			DeleteSequenceEntryAndRestartSequence,
			DeleteSequenceEntryAndPauseSequence,
			RestartSequence,
			PauseSequence
		}
	}
}