using TMPro;
using System;
using Extensions;
using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

namespace EternityEngine
{
	public class ModulationTargetValueRangeOption : Option
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
		public FloatRange valueRange;

		public void SetValueRangeMin (Option valueRangeMinOption)
		{
			float valueRangeMin = float.Parse(valueRangeMinOption.GetValue());
			valueRange.min = valueRangeMin;
		}

		public void SetValueRangeMax (Option valueRangeMaxOption)
		{
			float valueRangeMax = float.Parse(valueRangeMaxOption.GetValue());
			valueRange.max = valueRangeMax;
		}
		
		public override void SetData ()
		{
			if (_Data == null)
				_Data = new Data();
			base.SetData ();
			SetValueRangeOfData ();
		}

		public void SetValueRangeOfData ()
		{
			_Data.valueRange = valueRange;
		}

		public void SetValueRangeFromData ()
		{
			valueRange = _Data.valueRange;
		}

		[Serializable]
		public class Data : Option.Data
		{
			public FloatRange valueRange;

			public override object MakeAsset ()
			{
				ModulationTargetValueRangeOption modulationTargetValueRangeOption = ObjectPool.instance.SpawnComponent<ModulationTargetValueRangeOption>(LogicModule.instance.modulationTargetValueRangeOptionPrefab.prefabIndex, parent:LogicModule.instance.sceneTrs);
				Apply (modulationTargetValueRangeOption);
				return modulationTargetValueRangeOption;
			}

			public override void Apply (Asset asset)
			{
				base.Apply (asset);
				ModulationTargetValueRangeOption modulationTargetValueRangeOption = (ModulationTargetValueRangeOption) asset;
				modulationTargetValueRangeOption._Data = this;
				modulationTargetValueRangeOption.SetValueRangeFromData ();
			}
		}
	}
}