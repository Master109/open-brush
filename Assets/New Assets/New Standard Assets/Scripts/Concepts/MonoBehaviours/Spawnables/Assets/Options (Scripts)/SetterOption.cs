using System;
using Extensions;
using UnityEngine;
using System.Collections.Generic;

namespace EternityEngine
{
	public class SetterOption : Option
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
		public EnumOption invalidChildBehaviourEnumOption;
		public EnumOption tooManyChildrenBehaviourEnumOption;
		public Option valuesToSetOptionsParent;
		public Option valueToSetToOptionParent;
		public Option runOption;
		List<Option> valuesToSetOptions = new List<Option>();
		Option valueToSetToOption;

		public override void Init ()
		{
			base.Init ();
			print(1);
			valuesToSetOptionsParent.onAddChild += OnAddChild_ValuesToSet;
			valuesToSetOptionsParent.onRemoveChild += OnRemoveChild_ValuesToSet;
			valuesToSetOptionsParent.onAboutToAddChild += OnAboutToAddChild_ValuesToSet;
			valueToSetToOptionParent.onAddChild += OnAddChild_ValueToSetTo;
			valueToSetToOptionParent.onAboutToAddChild += OnAboutToAddChild_ValueToSetTo;
			valueToSetToOptionParent.onRemoveChild += OnRemoveChild_ValueToSetTo;
			valueToSetToOptionParent.onAboutToRemoveChild += OnAboutToRemoveChild_ValueToSetTo;
		}

		void OnAddChild_ValuesToSet (Option child)
		{
			print("OnAddChild_ValuesToSet");
			SetValuesToSetOptions ();
			if (!runOption.activatable)
				runOption.SetActivatable (valuesToSetOptions.Count > 0 && valueToSetToOption != null);
		}
		
		void OnRemoveChild_ValuesToSet (Option child)
		{
			print("OnRemoveChild_ValuesToSet");
			valuesToSetOptions.Remove(child);
			if (runOption.activatable)
				runOption.SetActivatable (valuesToSetOptions.Count > 0 && valueToSetToOption != null);
		}

		bool OnAboutToAddChild_ValuesToSet (Option child)
		{
			print("OnAboutToAddChild_ValuesToSet");
			InvalidChildBehaviour invalidChildBehaviour = (InvalidChildBehaviour) invalidChildBehaviourEnumOption.GetValue();
			if (invalidChildBehaviour == InvalidChildBehaviour.Disallow && !IsValueToSetValid(child))
				return false;
			else
				return true;
		}

		void OnAddChild_ValueToSetTo (Option child)
		{
			print("OnAddChild_ValuesToSetTo");
			if (valueToSetToOptionParent.children.Count == 1)
				valueToSetToOption = child;
			else
				ApplyTooManyChildrenBehaviour ();
			SetValuesToSetOptions ();
			runOption.SetActivatable (valuesToSetOptions.Count > 0);
		}

		void OnRemoveChild_ValueToSetTo (Option child)
		{
			print("OnRemoveChild_ValueToSetTo");
			if (valueToSetToOptionParent.children.Count == 0)
			{
				valueToSetToOption = null;
				runOption.SetActivatable (false);
				return;
			}
			TooManyChildrenBehaviour tooManyChildrenBehaviour = (TooManyChildrenBehaviour) tooManyChildrenBehaviourEnumOption.GetValue();
			if (tooManyChildrenBehaviour == TooManyChildrenBehaviour.UseMostRecentChild)
				valueToSetToOption = valueToSetToOptionParent.children[valueToSetToOptionParent.children.Count - 1];
			else if (tooManyChildrenBehaviour == TooManyChildrenBehaviour.UseMostRecentChild)
				valueToSetToOption = valueToSetToOptionParent.children[0];
			else if (tooManyChildrenBehaviour == TooManyChildrenBehaviour.UnparentMostRecentChildren)
				valueToSetToOption = valueToSetToOptionParent.children[valueToSetToOptionParent.children.Count - 1];
			else// if (tooManyChildrenBehaviour == TooManyChildrenBehaviour.UnparentLeastRecentChildren)
				valueToSetToOption = valueToSetToOptionParent.children[0];
			SetValuesToSetOptions ();
			runOption.SetActivatable (valuesToSetOptions.Count > 0);
		}

		bool OnAboutToAddChild_ValueToSetTo (Option child)
		{
			print("OnAboutToAddChild_ValueToSetTo");
			TooManyChildrenBehaviour tooManyChildrenBehaviour = (TooManyChildrenBehaviour) tooManyChildrenBehaviourEnumOption.GetValue();
			if (tooManyChildrenBehaviour == TooManyChildrenBehaviour.Disallow && valuesToSetOptionsParent.children.Count == 1)
				return false;
			else
				return true;
		}

		bool OnAboutToRemoveChild_ValueToSetTo (Option child)
		{
			print("OnAboutToRemoveChild_ValueToSetTo");
			InvalidChildBehaviour invalidChildBehaviour = (InvalidChildBehaviour) invalidChildBehaviourEnumOption.GetValue();
			if (invalidChildBehaviour == InvalidChildBehaviour.Disallow)
			{
				if (valuesToSetOptions.Count == 1)
					return false;
				TooManyChildrenBehaviour tooManyChildrenBehaviour = (TooManyChildrenBehaviour) tooManyChildrenBehaviourEnumOption.GetValue();
				if (tooManyChildrenBehaviour == TooManyChildrenBehaviour.UseMostRecentChild)
				{
					for (int i = 0; i < valuesToSetOptions.Count; i ++)
					{
						Option valueToSetOption = valuesToSetOptions[i];
						if (!CanSetValue(valueToSetOption, valuesToSetOptions[valuesToSetOptions.Count - 1]))
							return false;
					}
					return true;
				}
				else if (tooManyChildrenBehaviour == TooManyChildrenBehaviour.UseLeastRecentChild)
				{
					for (int i = 0; i < valuesToSetOptions.Count; i ++)
					{
						Option valueToSetOption = valuesToSetOptions[i];
						if (!CanSetValue(valueToSetOption, valuesToSetOptions[0]))
							return false;
					}
					return true;
				}
				else
					return true;
			}
			else
				return true;
		}

		void SetValuesToSetOptions ()
		{
			List<Option> validChildren = new List<Option>(valuesToSetOptionsParent.children);
			int[] invalidChlidrenIndices = ApplyInvalidChildBehaviour();
			for (int i = 0; i < invalidChlidrenIndices.Length; i ++)
			{
				int invalidChildIndex = invalidChlidrenIndices[i];
				validChildren.RemoveAt(invalidChildIndex - i);
			}
			valuesToSetOptions = new List<Option>(validChildren);
		}

		public void _ApplyInvalidChildBehaviour ()
		{
			ApplyInvalidChildBehaviour ();
		}

		// Returns indices of remaining children that are still invalid
		int[] ApplyInvalidChildBehaviour ()
		{
			List<int> output = new List<int>();
			InvalidChildBehaviour invalidChildBehaviour = (InvalidChildBehaviour) invalidChildBehaviourEnumOption.GetValue();
			if (invalidChildBehaviour == InvalidChildBehaviour.Unparent)
			{
				for (int i = 0; i < valuesToSetOptionsParent.children.Count; i ++)
				{
					Option valueToSetOption = valuesToSetOptionsParent.children[i];
					if (!IsValueToSetValid(valueToSetOption))
					{
						valuesToSetOptionsParent.RemoveChild (valueToSetOption);
						i --;
					}
				}
			}
			else// if (invalidChildBehaviour == InvalidChildBehaviour.Ignore)
			{
				for (int i = 0; i < valuesToSetOptionsParent.children.Count; i ++)
				{
					Option valueToSetOption = valuesToSetOptionsParent.children[i];
					if (!IsValueToSetValid(valueToSetOption))
						output.Add(i);
				}
			}
			return output.ToArray();
		}

		public void ApplyTooManyChildrenBehaviour ()
		{
			if (valueToSetToOptionParent.children.Count < 2)
				return;
			TooManyChildrenBehaviour tooManyChildrenBehaviour = (TooManyChildrenBehaviour) tooManyChildrenBehaviourEnumOption.GetValue();
			if (tooManyChildrenBehaviour == TooManyChildrenBehaviour.UseMostRecentChild)
				valueToSetToOption = valuesToSetOptionsParent.children[valuesToSetOptionsParent.children.Count - 1];
			else if (tooManyChildrenBehaviour == TooManyChildrenBehaviour.UseMostRecentChild)
				valueToSetToOption = valuesToSetOptionsParent.children[0];
			else if (tooManyChildrenBehaviour == TooManyChildrenBehaviour.UnparentMostRecentChildren)
			{
				while (valueToSetToOptionParent.children.Count > 1)
					valueToSetToOptionParent.RemoveChild (valueToSetToOptionParent.children[valueToSetToOptionParent.children.Count - 1]);
				valueToSetToOption = valueToSetToOptionParent.children[0];
			}
			else if (tooManyChildrenBehaviour == TooManyChildrenBehaviour.UnparentLeastRecentChildren)
			{
				while (valueToSetToOptionParent.children.Count > 1)
					valueToSetToOptionParent.RemoveChild (valueToSetToOptionParent.children[0]);
				valueToSetToOption = valueToSetToOptionParent.children[0];
			}
		}

		bool IsValueToSetValid (Option valueToSetOption)
		{
			return CanSetValue(valueToSetOption, valueToSetToOption);
		}

		bool CanSetValue (Option valueToSetOption, Option valueToSetToOption)
		{
			TypingTargetOption typingTargetOptionToSet = valueToSetOption as TypingTargetOption;
			TypingTargetOption typingTargetOptionToSetTo = valueToSetToOption as TypingTargetOption;
			if (typingTargetOptionToSet != null && typingTargetOptionToSetTo != null)
			{
				if (typingTargetOptionToSet.type == typingTargetOptionToSetTo.type || (typingTargetOptionToSet.type == TypingTargetOption.Type.Integer && typingTargetOptionToSetTo.type == TypingTargetOption.Type.Float) || (typingTargetOptionToSet.type == TypingTargetOption.Type.Float && typingTargetOptionToSetTo.type == TypingTargetOption.Type.Integer) || (typingTargetOptionToSet.type == TypingTargetOption.Type.String && typingTargetOptionToSetTo.type == TypingTargetOption.Type.Integer) || (typingTargetOptionToSet.type == TypingTargetOption.Type.String && typingTargetOptionToSetTo.type == TypingTargetOption.Type.Float))
					return true;
			}
			else
			{
				BoolOption boolOptionToSet = valueToSetOption as BoolOption;
				BoolOption boolOptionToSetTo = valueToSetToOption as BoolOption;
				if (boolOptionToSet != null && boolOptionToSetTo != null)
					return true;
				else
				{
					EnumOption enumOptionToSet = valueToSetOption as EnumOption;
					EnumOption enumOptionToSetTo = valueToSetToOption as EnumOption;
					if (enumOptionToSet != null && enumOptionToSetTo != null)
						return enumOptionToSet.enumType == enumOptionToSetTo.enumType;
				}
			}
			return false;
		}

		public void DoUpdate ()
		{
			for (int i = 0; i < valuesToSetOptionsParent.children.Count; i ++)
			{
				Option valueToSetOption = valuesToSetOptionsParent.children[i];
				TypingTargetOption typingTargetOptionToSet = valueToSetOption as TypingTargetOption;
				TypingTargetOption typingTargetOptionToSetTo = valueToSetToOption as TypingTargetOption;
				print(typingTargetOptionToSet);
				print(typingTargetOptionToSetTo);
				if (typingTargetOptionToSet != null && typingTargetOptionToSetTo != null)
				{
					if (typingTargetOptionToSet.type == typingTargetOptionToSetTo.type)
						typingTargetOptionToSet.SetValue (typingTargetOptionToSetTo.GetValue());
					else if ((typingTargetOptionToSet.type == TypingTargetOption.Type.Integer && typingTargetOptionToSetTo.type == TypingTargetOption.Type.Float) || (typingTargetOptionToSet.type == TypingTargetOption.Type.Float && typingTargetOptionToSetTo.type == TypingTargetOption.Type.Integer))
						typingTargetOptionToSet.SetValue ("" + typingTargetOptionToSetTo.value);
					else if ((typingTargetOptionToSet.type == TypingTargetOption.Type.String && typingTargetOptionToSetTo.type == TypingTargetOption.Type.Integer) || (typingTargetOptionToSet.type == TypingTargetOption.Type.String && typingTargetOptionToSetTo.type == TypingTargetOption.Type.Float))
						typingTargetOptionToSet.SetValue ("" + typingTargetOptionToSetTo.value);
					// else if (typingTargetOptionToSet.type == TypingTargetOption.Type.Integer && typingTargetOptionToSetTo.type == TypingTargetOption.Type.String && typingTargetOptionToSetTo.)
					// 	typingTargetOptionToSet.SetValue ("" + typingTargetOptionToSetTo.value);
				}
			}
		}
		
		public override void SetData ()
		{
			if (_Data == null)
				_Data = new Data();
			base.SetData ();
		}

		public enum InvalidChildBehaviour
		{
			Disallow,
			Unparent,
			Ignore
		}

		public enum TooManyChildrenBehaviour
		{
			Disallow,
			UseMostRecentChild,
			UseLeastRecentChild,
			UnparentMostRecentChildren,
			UnparentLeastRecentChildren
		}

		[Serializable]
		public class Data : Option.Data
		{
			[SaveAndLoadValue]
			public bool value;
			
			public override object MakeAsset ()
			{
				SetterOption setterOption = ObjectPool.instance.SpawnComponent<SetterOption>(LogicModule.instance.setterOptionPrefab.prefabIndex, parent:LogicModule.instance.sceneTrs);
				Apply (setterOption);
				if (!setterOption.isInitialized)
					setterOption.Init ();
				return setterOption;
			}

			public override void Apply (Asset asset)
			{
				base.Apply (asset);
				SetterOption setterOption = (SetterOption) asset;
				setterOption._Data = this;
			}
		}
	}
}