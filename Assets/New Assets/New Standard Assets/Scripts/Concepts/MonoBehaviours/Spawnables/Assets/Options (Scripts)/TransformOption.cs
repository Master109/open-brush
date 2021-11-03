using System;
using Extensions;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace EternityEngine
{
	public class TransformOption : Option
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
		public Transform otherTrs;
		public Option positionXOption;
		public Option positionYOption;
		public Option positionZOption;
		public Option eulerAnglesXOption;
		public Option eulerAnglesYOption;
		public Option eulerAnglesZOption;
		public Option sizeXOption;
		public Option sizeYOption;
		public Option sizeZOption;

		public void Init (Transform otherTrs)
		{
			this.otherTrs = otherTrs;
		}

		public void SetPositionX ()
		{
			otherTrs.position = otherTrs.position.SetX(float.Parse(positionXOption.GetValue()));
		}

		public void SetPositionY ()
		{
			otherTrs.position = otherTrs.position.SetY(float.Parse(positionYOption.GetValue()));
		}

		public void SetPositionZ ()
		{
			otherTrs.position = otherTrs.position.SetZ(float.Parse(positionZOption.GetValue()));
		}

		public void SetEulerAnglesX ()
		{
			otherTrs.eulerAngles = otherTrs.eulerAngles.SetX(float.Parse(eulerAnglesXOption.GetValue()));
		}

		public void SetEulerAnglesY ()
		{
			otherTrs.eulerAngles = otherTrs.eulerAngles.SetY(float.Parse(eulerAnglesYOption.GetValue()));
		}

		public void SetEulerAnglesZ ()
		{
			otherTrs.eulerAngles = otherTrs.eulerAngles.SetZ(float.Parse(eulerAnglesZOption.GetValue()));
		}

		public void SetSizeX ()
		{
			Vector3 size = otherTrs.lossyScale;
			size.x = float.Parse(sizeXOption.GetValue());
			otherTrs.SetWorldScale (size);
		}

		public void SetSizeY ()
		{
			Vector3 size = otherTrs.lossyScale;
			size.y = float.Parse(sizeYOption.GetValue());
			otherTrs.SetWorldScale (size);
		}

		public void SetSizeZ ()
		{
			Vector3 size = otherTrs.lossyScale;
			size.z = float.Parse(sizeZOption.GetValue());
			otherTrs.SetWorldScale (size);
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
				TransformOption transformOption = ObjectPool.instance.SpawnComponent<TransformOption>(LogicModule.instance.transformOptionPrefab.prefabIndex, parent:LogicModule.instance.sceneTrs);
				Apply (transformOption);
				return transformOption;
			}

			public override void Apply (Asset asset)
			{
				base.Apply (asset);
				TransformOption transformOption = (TransformOption) asset;
				transformOption._Data = this;
			}
		}
	}
}