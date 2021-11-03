using TMPro;
using System;
using Extensions;
using UnityEngine;
using System.Collections.Generic;

namespace EternityEngine
{
	public class AutomationSequenceEntryOption : Option
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
		public AutomationSequence.SequenceEntry sequenceEntry;
		public List<AutomationOption> automationOptionsUsingMe = new List<AutomationOption>();
		public EnumOption madeByHandTypeEnumOption;
		public EnumOption invalidOptionReferenceBehaviourEnumOption;
		public Option sequenceEntryIndexOption;
		public Option timeOption;
		int sequenceEntryIndex;

		public void SetTime ()
		{
			float time = float.Parse(timeOption.GetValue());
			for (int i = 0; i < automationOptionsUsingMe.Count; i ++)
			{
				AutomationOption automationOption = automationOptionsUsingMe[i];
				AutomationSequence.SequenceEntry sequenceEntry = automationOption.automationSequence.sequenceEntries[sequenceEntryIndex];
				sequenceEntry.time = time;
				automationOption.automationSequence.sequenceEntries[sequenceEntryIndex] = sequenceEntry;
			}
		}

		public void SetSequenceEntryIndex ()
		{
			int newSequenceEntryIndex = int.Parse(sequenceEntryIndexOption.GetValue());
			for (int i = 0; i < automationOptionsUsingMe.Count; i ++)
			{
				AutomationOption automationOption = automationOptionsUsingMe[i];
				AutomationOption.DuplicateSequenceEntryIndexBehaviour duplicateSequenceEntryIndexBehaviour = (AutomationOption.DuplicateSequenceEntryIndexBehaviour) automationOption.duplicateSequenceEntryIndexBehaviourEnumOption.GetValue();
				if (duplicateSequenceEntryIndexBehaviour == AutomationOption.DuplicateSequenceEntryIndexBehaviour.Disallow)
				{
					if (automationOption.automationSequence.sequenceEntries.Count > sequenceEntryIndex)
					{
						sequenceEntryIndexOption.SetValue ("" + sequenceEntryIndex);
						return;
					}
				}
				else if (duplicateSequenceEntryIndexBehaviour == AutomationOption.DuplicateSequenceEntryIndexBehaviour.PreferSwitch)
				{
					automationOption.automationSequence.sequenceEntries[newSequenceEntryIndex] = automationOption.automationSequence.sequenceEntries[sequenceEntryIndex];
					automationOption.automationSequenceEntryOptions[newSequenceEntryIndex] = automationOption.automationSequenceEntryOptions[sequenceEntryIndex];
					automationOption.automationSequence.sequenceEntries[sequenceEntryIndex] = automationOption.automationSequence.sequenceEntries[newSequenceEntryIndex];
					automationOption.automationSequenceEntryOptions[sequenceEntryIndex] = automationOption.automationSequenceEntryOptions[newSequenceEntryIndex];
				}
				else// if (duplicateSequenceEntryIndexBehaviour == AutomationOption.DuplicateSequenceEntryIndexBehaviour.PreferInsert)
				{
					automationOption.automationSequence.sequenceEntries.Insert(newSequenceEntryIndex, sequenceEntry);
					automationOption.automationSequenceEntryOptions.Insert(newSequenceEntryIndex, this);
					for (int i2 = newSequenceEntryIndex + 1; i2 < automationOption.automationSequenceEntryOptions.Count; i2 ++)
						automationOption.automationSequenceEntryOptions[i2].sequenceEntryIndex ++;
				}
			}
			sequenceEntryIndex = newSequenceEntryIndex;
		}

		public override void SetData ()
		{
			if (_Data == null)
				_Data = new Data();
			base.SetData ();
			SetSequenceEntryOfData ();
			SetNamesOfAutomationOptionsUsingMeOfData ();
			SetSequenceEntryIndexOfData ();
		}

		public void SetSequenceEntryOfData ()
		{
			_Data.sequenceEntry = sequenceEntry;
		}

		public void SetSequenceEntryFromData ()
		{
			sequenceEntry = _Data.sequenceEntry;
		}

		public void SetNamesOfAutomationOptionsUsingMeOfData ()
		{
			_Data.namesOfAutomationOptionsUsingMe = new string[automationOptionsUsingMe.Count];
			for (int i = 0; i < automationOptionsUsingMe.Count; i ++)
			{
				AutomationOption automationOption = automationOptionsUsingMe[i];
				_Data.namesOfAutomationOptionsUsingMe[i] = automationOption.name;
			}
		}

		public void SetNamesOfAutomationOptionsUsingMeFromData ()
		{
			for (int i = 0; i < _Data.namesOfAutomationOptionsUsingMe.Length; i ++)
			{
				string name = _Data.namesOfAutomationOptionsUsingMe[i];
				int indexOfName = LogicModule.instance.optionNamesDict.values.IndexOf(name);
				if (indexOfName != -1)
				{
					Option option = LogicModule.instance.optionNamesDict.keys[indexOfName];
					automationOptionsUsingMe.Add(option as AutomationOption);
				}
				else
				{
					for (int i2 = 0; i2 < GameManager.instance.assetsData.Count; i2 ++)
					{
						Asset.Data data = GameManager.instance.assetsData[i2];
						if (data.name == name)
						{
							Option option = (Option) data.MakeAsset();
							automationOptionsUsingMe.Add(option as AutomationOption);
							break;
						}
					}
				}
			}
		}

		public void SetSequenceEntryIndexOfData ()
		{
			_Data.sequenceEntryIndex = sequenceEntryIndex;
		}

		public void SetSequenceEntryIndexFromData ()
		{
			sequenceEntryIndex = _Data.sequenceEntryIndex;
		}

		[Serializable]
		public class Data : Option.Data
		{
			public AutomationSequence.SequenceEntry sequenceEntry;
			public string[] namesOfAutomationOptionsUsingMe = new string[0];
			public int sequenceEntryIndex;

			public override object MakeAsset ()
			{
				AutomationSequenceEntryOption automationSequenceEntryOption = ObjectPool.instance.SpawnComponent<AutomationSequenceEntryOption>(LogicModule.instance.automationSequenceEntryOptionPrefab.prefabIndex, parent:LogicModule.instance.sceneTrs);
				Apply (automationSequenceEntryOption);
				return automationSequenceEntryOption;
			}

			public override void Apply (Asset asset)
			{
				base.Apply (asset);
				AutomationSequenceEntryOption automationSequenceEntryOption = (AutomationSequenceEntryOption) asset;
				automationSequenceEntryOption._Data = this;
				automationSequenceEntryOption.SetSequenceEntryFromData ();
				automationSequenceEntryOption.SetNamesOfAutomationOptionsUsingMeFromData ();
				automationSequenceEntryOption.SetSequenceEntryIndexFromData ();
			}
		}
	}
}