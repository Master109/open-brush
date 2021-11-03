using System;
using Extensions;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace EternityEngine
{
	public class SpawnOrientationOption : Option
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
		public EnumOption deleteCurrentBehaviourEnumOption;
		int activatedOrder;
		int spawnedOrder;
		static int mostRecentSpawnedOrder = -1;
		SortedDictionary<int, SpawnOrientationOption> activatedSpawnOrientationOptionsDict = new SortedDictionary<int, SpawnOrientationOption>();
		SortedDictionary<int, SpawnOrientationOption> spawnedSpawnOrientationOptionsDict = new SortedDictionary<int, SpawnOrientationOption>();

		public override void Init ()
		{
			base.Init ();
			activatedOrder = activatedSpawnOrientationOptionsDict.Count;
			activatedSpawnOrientationOptionsDict.Add(activatedOrder, this);
			mostRecentSpawnedOrder ++;
			spawnedOrder = mostRecentSpawnedOrder;
			spawnedSpawnOrientationOptionsDict.Add(spawnedOrder, this);
			if (spawnedSpawnOrientationOptionsDict.Count > 1)
				SetActivatable (true);
		}

		public void MakeCurrent ()
		{
			SetActivatable (false);
			LogicModule.instance.currentSpawnOrientationOption.SetActivatable (true);
			LogicModule.instance.currentSpawnOrientationOption = this;
			activatedSpawnOrientationOptionsDict.Remove(activatedOrder);
			activatedOrder = activatedSpawnOrientationOptionsDict.Count - 1;
			activatedSpawnOrientationOptionsDict.Add(activatedOrder, this);
		}

		public override void OnDespawned ()
		{
			base.OnDespawned ();
			activatedSpawnOrientationOptionsDict.Remove(activatedOrder);
			spawnedSpawnOrientationOptionsDict.Remove(spawnedOrder);
			DeleteCurrentBehaviour deleteCurrentBehaviour = (DeleteCurrentBehaviour) deleteCurrentBehaviourEnumOption.GetValue();
			if (deleteCurrentBehaviour == DeleteCurrentBehaviour.PreferUseMostRecentActiveThenMostRecentSpawned)
			{
				if (!TryMakeCurrentMostRecentInDictionary(activatedSpawnOrientationOptionsDict))
					TryMakeCurrentMostRecentInDictionary (spawnedSpawnOrientationOptionsDict);
			}
			else if (deleteCurrentBehaviour == DeleteCurrentBehaviour.PreferUseMostRecentActiveThenLeastRecentSpawned)
			{
				if (!TryMakeCurrentMostRecentInDictionary(activatedSpawnOrientationOptionsDict))
					TryMakeCurrentLeastRecentInDictionary (spawnedSpawnOrientationOptionsDict);
			}
			else if (deleteCurrentBehaviour == DeleteCurrentBehaviour.PreferUseLeastRecentActiveThenMostRecentSpawned)
			{
				if (!TryMakeCurrentLeastRecentInDictionary(activatedSpawnOrientationOptionsDict))
					TryMakeCurrentMostRecentInDictionary (spawnedSpawnOrientationOptionsDict);
			}
			else if (deleteCurrentBehaviour == DeleteCurrentBehaviour.PreferUseLeastRecentActiveThenLeastRecentSpawned)
			{
				if (!TryMakeCurrentLeastRecentInDictionary(activatedSpawnOrientationOptionsDict))
					TryMakeCurrentLeastRecentInDictionary (spawnedSpawnOrientationOptionsDict);
			}
			else if (deleteCurrentBehaviour == DeleteCurrentBehaviour.UseMostRecentSpawned)
			{
				TryMakeCurrentMostRecentInDictionary (spawnedSpawnOrientationOptionsDict);
			}
			else// if (deleteCurrentBehaviour == DeleteCurrentBehaviour.UseLeastRecentSpawned)
			{
				TryMakeCurrentLeastRecentInDictionary (spawnedSpawnOrientationOptionsDict);
			}
		}

		bool TryMakeCurrentLeastRecentInDictionary (SortedDictionary<int, SpawnOrientationOption> dict)
		{
			if (dict.Count == 0)
				return false;
			SpawnOrientationOption spawnOrientationOption;
			int spawnOrientationOptionIndex = 0;
			while (!dict.TryGetValue(spawnOrientationOptionIndex, out spawnOrientationOption))
			{
				spawnOrientationOptionIndex ++;
				if (spawnOrientationOptionIndex > mostRecentSpawnedOrder)
					return false;
			}
			spawnOrientationOption.MakeCurrent ();
			return true;
		}

		bool TryMakeCurrentMostRecentInDictionary (SortedDictionary<int, SpawnOrientationOption> dict)
		{
			if (dict.Count == 0)
				return false;
			SpawnOrientationOption spawnOrientationOption;
			int spawnOrientationOptionIndex = mostRecentSpawnedOrder;
			while (!dict.TryGetValue(spawnOrientationOptionIndex, out spawnOrientationOption))
			{
				spawnOrientationOptionIndex --;
				if (spawnOrientationOptionIndex == -1)
					return false;
			}
			spawnOrientationOption.MakeCurrent ();
			return true;
		}
		
		public override void SetData ()
		{
			if (_Data == null)
				_Data = new Data();
			base.SetData ();
			SetActivatedOrderOfData ();
			SetSpawnedOrderOfData ();
		}

		public void SetActivatedOrderOfData ()
		{
			_Data.activatedOrder = activatedOrder;
		}

		public void SetActivatedOrderFromData ()
		{
			activatedOrder = _Data.activatedOrder;
		}

		public void SetSpawnedOrderOfData ()
		{
			_Data.spawnedOrder = spawnedOrder;
		}

		public void SetSpawnedOrderFromData ()
		{
			spawnedOrder = _Data.spawnedOrder;
			if (spawnedOrder > mostRecentSpawnedOrder)
				mostRecentSpawnedOrder = spawnedOrder;
		}

		public enum DeleteCurrentBehaviour
		{
			PreferUseMostRecentActiveThenMostRecentSpawned,
			PreferUseMostRecentActiveThenLeastRecentSpawned,
			PreferUseLeastRecentActiveThenMostRecentSpawned,
			PreferUseLeastRecentActiveThenLeastRecentSpawned,
			UseMostRecentSpawned,
			UseLeastRecentSpawned
		}

		[Serializable]
		public class Data : Option.Data
		{
			[SaveAndLoadValue]
			public int activatedOrder;
			[SaveAndLoadValue]
			public int spawnedOrder;

			public override object MakeAsset ()
			{
				SpawnOrientationOption spawnOrientationOption = ObjectPool.instance.SpawnComponent<SpawnOrientationOption>(LogicModule.instance.spawnOrientationOptionPrefab.prefabIndex, parent:LogicModule.instance.sceneTrs);
				Apply (spawnOrientationOption);
				return spawnOrientationOption;
			}

			public override void Apply (Asset asset)
			{
				base.Apply (asset);
				SpawnOrientationOption spawnOrientationOption = (SpawnOrientationOption) asset;
				spawnOrientationOption._Data = this;
				spawnOrientationOption.SetActivatedOrderFromData ();
				spawnOrientationOption.SetSpawnedOrderFromData ();
			}
		}
	}
}