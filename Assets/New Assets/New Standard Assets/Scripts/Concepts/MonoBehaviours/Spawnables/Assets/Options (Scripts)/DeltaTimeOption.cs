using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace EternityEngine
{
	public class DeltaTimeOption : Option
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
			SetValue ("" + Time.deltaTime);
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
				DeltaTimeOption deltaTimeOption = ObjectPool.instance.SpawnComponent<DeltaTimeOption>(LogicModule.instance.deltaTimeOptionPrefab.prefabIndex, parent:LogicModule.instance.sceneTrs);
				Apply (deltaTimeOption);
				return deltaTimeOption;
			}

			public override void Apply (Asset asset)
			{
				base.Apply (asset);
				DeltaTimeOption deltaTimeOption = (DeltaTimeOption) asset;
				deltaTimeOption._Data = this;
			}
		}
	}
}