using System;
using Extensions;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace EternityEngine
{
	public class ModelOption : TransformOption
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
		public BoolOption visibleBoolOption;
		public Renderer modelRenderer;

		public void Init (Renderer modelRenderer)
		{
			this.modelRenderer = modelRenderer;
			SetVisible ();
		}

		public void SetVisible ()
		{
			modelRenderer.enabled = visibleBoolOption.value;
		}
		
		public override void SetData ()
		{
			if (_Data == null)
				_Data = new Data();
			base.SetData ();
		}

		[Serializable]
		public class Data : TransformOption.Data
		{
			public override object MakeAsset ()
			{
				ModelOption modelOption = ObjectPool.instance.SpawnComponent<ModelOption>(LogicModule.instance.modelOptionPrefab.prefabIndex, parent:LogicModule.instance.sceneTrs);
				Apply (modelOption);
				return modelOption;
			}

			public override void Apply (Asset asset)
			{
				base.Apply (asset);
				ModelOption modelOption = (ModelOption) asset;
				modelOption._Data = this;
			}
		}
	}
}