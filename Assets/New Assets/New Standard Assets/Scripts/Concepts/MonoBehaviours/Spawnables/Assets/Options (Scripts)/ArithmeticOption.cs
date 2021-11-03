using System;
using Extensions;
using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

namespace EternityEngine
{
	public class ArithmeticOption : Option
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
		public EnumOption operationTypeEnumOption;
		public EnumOption tooManyChildrenBehaviourEnumOption;
		public Option invalidAnswerOption;
		public Option value1TypingTargetOptionParent;
		public Option value2TypingTargetOptionParent;
		public Option runOption;
		TooManyChildrenBehaviour tooManyChildrenBehaviour;
		OperationType operationType;
		TypingTargetOption value1TypingTargetOption;
		TypingTargetOption value2TypingTargetOption;

		public override void Init ()
		{
			base.Init ();
			if (!operationTypeEnumOption.isInitialized)
				operationTypeEnumOption.Init ();
			SetOperationType ();
			tooManyChildrenBehaviour = (TooManyChildrenBehaviour) tooManyChildrenBehaviourEnumOption.GetValue();
			value1TypingTargetOptionParent.onAddChild += (Option child) => { OnAddChild (value1TypingTargetOptionParent, child); };
			value1TypingTargetOptionParent.onRemoveChild += (Option child) => { OnRemoveChild (value1TypingTargetOptionParent, child); };
			value1TypingTargetOptionParent.onAboutToAddChild += OnAboutToAddChild_value1TypingTarget;
			value2TypingTargetOptionParent.onAddChild += (Option child) => { OnAddChild (value2TypingTargetOptionParent, child); };
			value2TypingTargetOptionParent.onRemoveChild += (Option child) => { OnRemoveChild (value2TypingTargetOptionParent, child); };
			value2TypingTargetOptionParent.onAboutToAddChild += OnAboutToAddChild_value2TypingTarget;
		}

		void OnAddChild (Option parent, Option child)
		{
			if (parent.children.Count > 1)
			{
				if (tooManyChildrenBehaviour == TooManyChildrenBehaviour.UnparentOldChildren)
				{
					while (parent.children.Count > 1)
						parent.RemoveChild (parent.children[0]);
				}
			}
			if (parent == value1TypingTargetOptionParent)
				value1TypingTargetOption = child as TypingTargetOption;
			else
				value2TypingTargetOption = child as TypingTargetOption;
			runOption.SetActivatable (ShouldRunOptionBeActivatable());
		}

		void OnRemoveChild (Option parent, Option child)
		{
			if (parent.children.Count == 0)
			{
				if (parent == value1TypingTargetOptionParent)
					value1TypingTargetOption = null;
				else
					value2TypingTargetOption = null;
				runOption.SetActivatable (false);
			}
			else
			{
				ApplyTooManyChildrenBehaviour (parent);
				runOption.SetActivatable (ShouldRunOptionBeActivatable());
			}
		}

		bool OnAboutToAddChild_value1TypingTarget (Option child)
		{
			return tooManyChildrenBehaviour == TooManyChildrenBehaviour.Disallow && value1TypingTargetOptionParent.children.Count >= 1;
		}

		bool OnAboutToAddChild_value2TypingTarget (Option child)
		{
			return tooManyChildrenBehaviour == TooManyChildrenBehaviour.Disallow && value2TypingTargetOptionParent.children.Count >= 1;
		}

		public void ApplyTooManyChildrenBehaviour (Option parent)
		{
			if (parent.children.Count < 2)
				return;
			tooManyChildrenBehaviour = (TooManyChildrenBehaviour) tooManyChildrenBehaviourEnumOption.GetValue();
			if (tooManyChildrenBehaviour == TooManyChildrenBehaviour.UseMostRecentChild)
			{
				if (parent == value1TypingTargetOptionParent)
					value1TypingTargetOption = value1TypingTargetOptionParent.children[value1TypingTargetOptionParent.children.Count - 1] as TypingTargetOption;
				else
					value2TypingTargetOption = value2TypingTargetOptionParent.children[value2TypingTargetOptionParent.children.Count - 1] as TypingTargetOption;
			}
			else if (tooManyChildrenBehaviour == TooManyChildrenBehaviour.UseMostRecentChild)
			{
				if (parent == value1TypingTargetOptionParent)
					value1TypingTargetOption = value1TypingTargetOptionParent.children[0] as TypingTargetOption;
				else
					value2TypingTargetOption = value2TypingTargetOptionParent.children[0] as TypingTargetOption;
			}
			else if (tooManyChildrenBehaviour == TooManyChildrenBehaviour.UnparentOldChildren)
			{
				while (parent.children.Count > 1)
					parent.RemoveChild (parent.children[0]);
				if (parent == value1TypingTargetOptionParent)
					value1TypingTargetOption = value1TypingTargetOptionParent.children[0] as TypingTargetOption;
				else
					value2TypingTargetOption = value2TypingTargetOptionParent.children[0] as TypingTargetOption;
			}
		}

		public void SetOperationType ()
		{
			operationType = (OperationType) operationTypeEnumOption.GetValue();
			runOption.SetActivatable (ShouldRunOptionBeActivatable());
		}

		bool ShouldRunOptionBeActivatable ()
		{
			return value1TypingTargetOption != null && value1TypingTargetOption.value != null && value2TypingTargetOption != null && value2TypingTargetOption.value != null;
		}

		public void DoUpdate ()
		{
			print(value1TypingTargetOption);
			print(value2TypingTargetOption);
			float value1 = (float) value1TypingTargetOption.value;
			float value2 = (float) value2TypingTargetOption.value;
			if (operationType == OperationType.Add)
				value1TypingTargetOption.SetValue ("" + (value1 + value2));
			else if (operationType == OperationType.Subtract)
				value1TypingTargetOption.SetValue ("" + (value1 - value2));
			else if (operationType == OperationType.Multiply)
				value1TypingTargetOption.SetValue ("" + (value1 * value2));
			else if (operationType == OperationType.Divide)
				value1TypingTargetOption.SetValue ("" + (value1 / value2));
			else if (operationType == OperationType.Exponent)
				value1TypingTargetOption.SetValue ("" + Mathf.Pow(value1, value2));
		}
		
		public override void SetData ()
		{
			if (_Data == null)
				_Data = new Data();
			base.SetData ();
		}

		public enum TooManyChildrenBehaviour
		{
			Disallow,
			UseMostRecentChild,
			UseLeastRecentChild,
			UnparentOldChildren
		}

		public enum OperationType
		{
			Add,
			Subtract,
			Multiply,
			Divide,
			Exponent
		}

		[Serializable]
		public class Data : Option.Data
		{
			public override object MakeAsset ()
			{
				ArithmeticOption arithmeticOption = ObjectPool.instance.SpawnComponent<ArithmeticOption>(LogicModule.instance.arithmeticOptionPrefab.prefabIndex, parent:LogicModule.instance.sceneTrs);
				Apply (arithmeticOption);
				return arithmeticOption;
			}

			public override void Apply (Asset asset)
			{
				base.Apply (asset);
				ArithmeticOption arithmeticOption = (ArithmeticOption) asset;
				arithmeticOption._Data = this;
			}
		}
	}
}