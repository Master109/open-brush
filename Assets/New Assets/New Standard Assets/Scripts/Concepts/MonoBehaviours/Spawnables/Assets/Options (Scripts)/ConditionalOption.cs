using System;
using Extensions;
using UnityEngine;

namespace EternityEngine
{
	public class ConditionalOption : Option
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
		public EnumOption comparisonTypeEnumOption;
		public EnumOption tooManyChildrenBehaviourEnumOption;
		public Option trueOption;
		public Option falseOption;
		public Option value1OptionParent;
		public Option value2OptionParent;
		public Option runOption;
		ComparisonType comparisonType;
		Option value1Option;
		Option value2Option;

		public override void Init ()
		{
			base.Init ();
			if (!comparisonTypeEnumOption.isInitialized)
				comparisonTypeEnumOption.Init ();
			SetComparisonType ();
			value1OptionParent.onAddChild += (Option child) => { OnAddChild (value1OptionParent, child); };
			value1OptionParent.onRemoveChild += (Option child) => { OnRemoveChild (value1OptionParent, child); };
			value1OptionParent.onAboutToAddChild += OnAboutToAddChild_Value1;
			value2OptionParent.onAddChild += (Option child) => { OnAddChild (value2OptionParent, child); };
			value2OptionParent.onRemoveChild += (Option child) => { OnRemoveChild (value2OptionParent, child); };
			value2OptionParent.onAboutToAddChild += OnAboutToAddChild_Value2;
		}

		void OnAddChild (Option parent, Option child)
		{
			if (parent.children.Count > 1)
			{
				TooManyChildrenBehaviour tooManyChildrenBehaviour = (TooManyChildrenBehaviour) tooManyChildrenBehaviourEnumOption.GetValue();
				if (tooManyChildrenBehaviour == TooManyChildrenBehaviour.UnparentOldChildren)
				{
					while (parent.children.Count > 1)
						parent.RemoveChild (parent.children[0]);
				}
			}
			if (parent == value1OptionParent)
				value1Option = child;
			else
				value2Option = child;
			runOption.SetActivatable (ShouldRunOptionBeActivatable());
		}

		void OnRemoveChild (Option parent, Option child)
		{
			if (parent.children.Count == 0)
			{
				if (parent == value1OptionParent)
					value1Option = null;
				else
					value2Option = null;
				runOption.SetActivatable (false);
			}
			else
			{
				ApplyTooManyChildrenBehaviour (parent);
				runOption.SetActivatable (ShouldRunOptionBeActivatable());
			}
		}

		bool OnAboutToAddChild_Value1 (Option child)
		{
			TooManyChildrenBehaviour tooManyChildrenBehaviour = (TooManyChildrenBehaviour) tooManyChildrenBehaviourEnumOption.GetValue();
			return tooManyChildrenBehaviour == TooManyChildrenBehaviour.Disallow && value1OptionParent.children.Count > 0;
		}

		bool OnAboutToAddChild_Value2 (Option child)
		{
			TooManyChildrenBehaviour tooManyChildrenBehaviour = (TooManyChildrenBehaviour) tooManyChildrenBehaviourEnumOption.GetValue();
			return tooManyChildrenBehaviour == TooManyChildrenBehaviour.Disallow && value2OptionParent.children.Count > 0;
		}

		public void ApplyTooManyChildrenBehaviour (Option parent)
		{
			if (parent.children.Count < 2)
				return;
			TooManyChildrenBehaviour tooManyChildrenBehaviour = (TooManyChildrenBehaviour) tooManyChildrenBehaviourEnumOption.GetValue();
			if (tooManyChildrenBehaviour == TooManyChildrenBehaviour.UseMostRecentChild)
			{
				if (parent == value1OptionParent)
					value1Option = value1OptionParent.children[value1OptionParent.children.Count - 1];
				else
					value2Option = value2OptionParent.children[value2OptionParent.children.Count - 1];
			}
			else if (tooManyChildrenBehaviour == TooManyChildrenBehaviour.UseMostRecentChild)
			{
				if (parent == value1OptionParent)
					value1Option = value1OptionParent.children[0];
				else
					value2Option = value2OptionParent.children[0];
			}
			else if (tooManyChildrenBehaviour == TooManyChildrenBehaviour.UnparentOldChildren)
			{
				while (parent.children.Count > 1)
					parent.RemoveChild (parent.children[0]);
				if (parent == value1OptionParent)
					value1Option = value1OptionParent.children[0];
				else
					value2Option = value2OptionParent.children[0];
			}
		}

		public void SetComparisonType ()
		{
			comparisonType = (ComparisonType) comparisonTypeEnumOption.GetValue();
			runOption.SetActivatable (ShouldRunOptionBeActivatable());
		}

		bool ShouldRunOptionBeActivatable ()
		{
			bool output = true;
			TypingTargetOption value1TypingTargetOption = value1Option as TypingTargetOption;
			TypingTargetOption value2TypingTargetOption = value2Option as TypingTargetOption;
			if (value1TypingTargetOption != null)
			{
				if (value2TypingTargetOption != null)
				{
					float? numberValue1 = value1TypingTargetOption.value;
					float? numberValue2 = value2TypingTargetOption.value;
					if (numberValue1 != null)
					{
						if (numberValue2 == null && comparisonType != ComparisonType.EqualTo && comparisonType != ComparisonType.NotEqualTo)
							output = false;
					}
					else if (comparisonType != ComparisonType.EqualTo && comparisonType != ComparisonType.NotEqualTo)
						output = false;
				}
				else if (comparisonType != ComparisonType.EqualTo && comparisonType != ComparisonType.NotEqualTo)
					output = false;
			}
			else if (comparisonType != ComparisonType.EqualTo && comparisonType != ComparisonType.NotEqualTo)
				output = false;
			return output;
		}

		public void Evaluate ()
		{
			bool isTrue = false;
			TypingTargetOption value1TypingTargetOption = value1Option as TypingTargetOption;
			TypingTargetOption value2TypingTargetOption = value2Option as TypingTargetOption;
			if (value1TypingTargetOption != null)
			{
				if (value2TypingTargetOption != null)
				{
					float? numberValue1 = value1TypingTargetOption.value;
					float? numberValue2 = value2TypingTargetOption.value;
					if (numberValue1 != null)
					{
						if (numberValue2 != null)
						{
							if (comparisonType == ComparisonType.EqualTo)
								isTrue = numberValue1 == numberValue2;
							else if (comparisonType == ComparisonType.NotEqualTo)
								isTrue = numberValue1 != numberValue2;
							else if (comparisonType == ComparisonType.GreaterThan)
								isTrue = numberValue1 > numberValue2;
							else if (comparisonType == ComparisonType.LessThan)
								isTrue = numberValue1 < numberValue2;
							else if (comparisonType == ComparisonType.GreaterThanOrEqualTo)
								isTrue = numberValue1 >= numberValue2;
							else// if (comparisonType == ComparisonType.LessThanOrEqualTo)
								isTrue = numberValue1 <= numberValue2;
						}
						else
						{
							if (comparisonType == ComparisonType.EqualTo)
								isTrue = numberValue1 == numberValue2;
							else// if (comparisonType == ComparisonType.NotEqualTo)
								isTrue = numberValue1 != numberValue2;
						}
					}
					else
					{
						if (comparisonType == ComparisonType.EqualTo)
							isTrue = numberValue1 == numberValue2;
						else// if (comparisonType == ComparisonType.NotEqualTo)
							isTrue = numberValue1 != numberValue2;
					}
				}
				else
				{
					if (comparisonType == ComparisonType.EqualTo)
						isTrue = value1Option == value2Option;
					else// if (comparisonType == ComparisonType.NotEqualTo)
						isTrue = value1Option != value2Option;
				}
			}
			else
			{
				if (comparisonType == ComparisonType.EqualTo)
					isTrue = value1Option == value2Option;
				else// if (comparisonType == ComparisonType.NotEqualTo)
					isTrue = value1Option != value2Option;
			}
			if (isTrue)
				trueOption.StartActivate (null);
			else
				falseOption.StartActivate (null);
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

		public enum ComparisonType
		{
			EqualTo,
			NotEqualTo,
			GreaterThan,
			LessThan,
			GreaterThanOrEqualTo,
			LessThanOrEqualTo
		}

		[Serializable]
		public class Data : Option.Data
		{
			public override object MakeAsset ()
			{
				ConditionalOption conditionalOption = ObjectPool.instance.SpawnComponent<ConditionalOption>(LogicModule.instance.conditionalOptionPrefab.prefabIndex, parent:LogicModule.instance.sceneTrs);
				Apply (conditionalOption);
				if (!conditionalOption.isInitialized)
					conditionalOption.Init ();
				return conditionalOption;
			}

			public override void Apply (Asset asset)
			{
				base.Apply (asset);
				ConditionalOption conditionalOption = (ConditionalOption) asset;
				conditionalOption._Data = this;
			}
		}
	}
}