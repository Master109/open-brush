using TMPro;
using System;
using Extensions;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace EternityEngine
{
	public class CodeCommandOption : Option
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
		public Option nameOption;
		public Option contentsOption;
		public BoolOption runRepeatedlyBoolOption;
		public string previousName;
		
		public void Run ()
		{
			CodeRunner.RunCodeCommandOnce (nameOption.GetValue(), contentsOption.GetValue());
		}

		public void SetRunRepeatedly ()
		{
			if (runRepeatedlyBoolOption.value)
				CodeRunner.StartRunningCodeCommand (nameOption.GetValue(), contentsOption.GetValue());
			else
				CodeRunner.StopRunningCodeCommand (nameOption.GetValue());
		}

		public void Rename ()
		{
			string newName = nameOption.GetValue();
			CodeRunner.RenameRunningCodeCommand (previousName, newName);
			previousName = newName;
			text.text = "\"" + newName + "\" Code Command";
		}

		public void SetContents ()
		{
			if (runRepeatedlyBoolOption.value)
			{
				string name = nameOption.GetValue();
				string contents = contentsOption.GetValue();
				CodeRunner.StopRunningCodeCommand (name);
				CodeRunner.StartRunningCodeCommand (name, contents);
			}
		}
		
		public override void SetData ()
		{
			if (_Data == null)
				_Data = new Data();
			base.SetData ();
		}

		[Serializable]
		public class Data : Option.Data
		{
			public override object MakeAsset ()
			{
				CodeCommandOption codeCommandOption = ObjectPool.instance.SpawnComponent<CodeCommandOption>(LogicModule.instance.codeCommandOptionPrefab.prefabIndex, parent:LogicModule.instance.sceneTrs);
				Apply (codeCommandOption);
				return codeCommandOption;
			}

			public override void Apply (Asset asset)
			{
				base.Apply (asset);
				CodeCommandOption codeCommandOption = (CodeCommandOption) asset;
				codeCommandOption._Data = this;
			}
		}
	}
}