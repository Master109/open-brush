using System;
using Extensions;
using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

namespace EternityEngine
{
	public class ActivationBehaviourOption : Option
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
				ActivationBehaviourOption activationBehaviourOption = ObjectPool.instance.SpawnComponent<ActivationBehaviourOption>(LogicModule.instance.activationBehaviourOptionPrefab.prefabIndex, parent:LogicModule.instance.sceneTrs);
				Apply (activationBehaviourOption);
				return activationBehaviourOption;
			}

			public override void Apply (Asset asset)
			{
				base.Apply (asset);
				ActivationBehaviourOption activationBehaviourOption = (ActivationBehaviourOption) asset;
				activationBehaviourOption._Data = this;
			}
		}
	}
}