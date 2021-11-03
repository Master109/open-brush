using UnityEngine;
using System;

namespace EternityEngine
{
	public class Asset : Spawnable
	{
		public object data;
		public Data _Data
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

		public virtual void Awake ()
		{
			if (_Data == null)
				_Data = new Data();
		}

		public virtual void SetData ()
		{
			throw new NotImplementedException();
		}

		[Serializable]
		public class Data : ISaveableAndLoadable
		{
			[SaveAndLoadValue]
			public string name;

			public virtual object MakeAsset ()
			{
				throw new NotImplementedException();
			}

			public virtual void Apply (Asset asset)
			{
				throw new NotImplementedException();
			}
		}
	}
}