using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Random = UnityEngine.Random;

namespace EternityEngine
{
	public class RandomValueOption : Option
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

		public override void Init ()
		{
			base.Init ();
			DoUpdate ();
		}

		public override string GetValue ()
		{
			DoUpdate ();
			return base.GetValue();
		}
		
		public void DoUpdate ()
		{
			SetValue ("" + Random.value);
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
				RandomValueOption randomValueOption = ObjectPool.instance.SpawnComponent<RandomValueOption>(LogicModule.instance.randomValueOptionPrefab.prefabIndex, parent:LogicModule.instance.sceneTrs);
				Apply (randomValueOption);
				return randomValueOption;
			}

			public override void Apply (Asset asset)
			{
				base.Apply (asset);
				RandomValueOption randomValueOption = (RandomValueOption) asset;
				randomValueOption._Data = this;
			}
		}
	}
}