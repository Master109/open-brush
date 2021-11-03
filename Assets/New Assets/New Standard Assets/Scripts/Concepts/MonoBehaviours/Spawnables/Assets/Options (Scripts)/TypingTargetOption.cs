using System;
using Extensions;
using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

namespace EternityEngine
{
	public class TypingTargetOption : Option
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
		public Type type;
		public FloatRange validNumberRange;
		public float? value;
#if USE_UNITY_EVENTS
		public UnityEvent onValueChangedUnityEvent;
		[HideInInspector]
		public int indexOfOnValueChangedUnityEvent;
#endif
#if USE_EVENTS
		public Event onValueChanged;
		[HideInInspector]
		public int indexOfOnValueChanged;
#endif
		public static TypingTargetOption currentActive;
		public static int typingCursorLocation;
		public static char[] DIGITS = new char[10] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };

		public override void Init ()
		{
			base.Init ();
			value = GetNumberValue(GetValue());
		}

		public void AddText (string text)
		{
			this.text.text = this.text.text.Insert(typingCursorLocation, text);
			if (type == Type.Integer)
			{
				value = GetNumberValue(text);
				if (value == null)
				{
					this.text.color = Color.red;
					return;
				}
				else
					this.text.color = Color.black;
			}
			else if (type == Type.Float)
			{
				
			}
#if USE_UNITY_EVENTS
			onValueChangedUnityEvent.Invoke();
#endif
#if USE_EVENTS
			onValueChanged.unityEvent.Invoke();
#endif
		}

		public void SetText (string text)
		{
			if (text.Equals(this.text.text))
				return;
			this.text.text = this.text.text.Remove(this.text.text.IndexOf(nameToValueSeparator)) + text;
#if USE_UNITY_EVENTS
			onValueChangedUnityEvent.Invoke();
#endif
#if USE_EVENTS
			onValueChanged.unityEvent.Invoke();
#endif
		}

		float? GetNumberValue (string text)
		{
			float output = 0;
			while (!float.TryParse(text, out output))
			{
				int indexOfLeftParenthesis = text.IndexOf("(");
				if (indexOfLeftParenthesis != -1)
				{
					if (indexOfLeftParenthesis == text.Length - 1)
						return null;
					text = GetSimplifiedExponentOperation(text, indexOfLeftParenthesis + 1);
					if (text == null)
						return null;
					
				}
				else
				{
					
				}
			}
			return output;
		}

		TreeNode<MathFloatExpression> GetExpressionTree ()
		{
			TreeNode<MathFloatExpression> output = new TreeNode<MathFloatExpression>(null);
			return output;
		}

		string GetSimplifiedExponentOperation (string text, int startIndex = 0)
		{
			int indexOfExponent = text.IndexOf("^", startIndex);
			if (indexOfExponent != -1)
			{
				if (indexOfExponent == 0 || indexOfExponent == text.Length - 1)
					return null;
				int firstNumberIndex = indexOfExponent - 1;
				string currentNumberStr = "";
				while (firstNumberIndex >= 0)
				{
					char c = text[firstNumberIndex];
					if (DIGITS.Contains(c) || c == '.' || c == '-')
					{
						currentNumberStr = c + currentNumberStr;
						firstNumberIndex --;
					}
					else
						break;
				}
				float firstNumber = float.Parse(currentNumberStr);
				int secondNumberIndex = indexOfExponent + 1;
				currentNumberStr = "";
				while (secondNumberIndex < text.Length)
				{
					char c = text[secondNumberIndex];
					if (DIGITS.Contains(c) || c == '.' || c == '-')
					{
						currentNumberStr += c;
						secondNumberIndex ++;
					}
					else
						break;
				}
				float secondNumber = float.Parse(currentNumberStr);
				text = text.RemoveStartEnd(firstNumberIndex, secondNumberIndex);
				text = text.Insert(firstNumberIndex, "" + Mathf.Pow(firstNumber, secondNumber));
			}
			return text;
		}

		public void DeleteText ()
		{
			if (typingCursorLocation < text.text.Length)
			{
				text.text = text.text.Remove(typingCursorLocation, 1);
#if USE_UNITY_EVENTS
				onValueChangedUnityEvent.Invoke();
#endif
#if USE_EVENTS
				onValueChanged.unityEvent.Invoke();
#endif
			}
		}

		public void BackspaceText ()
		{
			if (typingCursorLocation >= text.text.IndexOf(nameToValueSeparator) + nameToValueSeparator.Length)
			{
				text.text = text.text.Remove(typingCursorLocation - 1, 1);
				typingCursorLocation --;
#if USE_UNITY_EVENTS
				onValueChangedUnityEvent.Invoke();
#endif
#if USE_EVENTS
				onValueChanged.unityEvent.Invoke();
#endif
			}
		}

#if UNITY_EDITOR
		public override void OnValidate ()
		{
			if (addEvents)
			{
#if USE_UNITY_EVENTS
				if (!LogicModule.Instance.unityEvents.Contains(onValueChangedUnityEvent))
				{
					LogicModule.instance.unityEvents.Add(onValueChangedUnityEvent);
					indexOfOnValueChangedUnityEvent = LogicModule.instance.unityEvents.Count - 1;
				}
#endif
#if USE_EVENTS
				if (!LogicModule.Instance.events.Contains(onValueChanged))
				{
					LogicModule.instance.events.Add(onValueChanged);
					indexOfOnValueChanged = LogicModule.instance.events.Count - 1;
				}
#endif
			}
			if (removeEvents)
#if USE_UNITY_EVENTS
				LogicModule.Instance.unityEvents.Remove(onValueChangedUnityEvent);
#endif
#if USE_EVENTS
				LogicModule.Instance.events.Remove(onValueChanged);
#endif
			base.OnValidate ();
		}
#endif

		public override void SetData ()
		{
			if (_Data == null)
				_Data = new Data();
			base.SetData ();
			SetTypeOfData ();
			SetValidNumberRangeOfData ();
#if USE_UNITY_EVENTS
			SetIndexOfOnValueChangedUnityEventOfData ();
#endif
#if USE_EVENTS
			SetIndexOfOnValueChangedOfData ();
#endif
		}

		public void SetTypeOfData ()
		{
			_Data.type = type;
		}

		public void SetTypeFromData ()
		{
			type = _Data.type;
		}

		public void SetValidNumberRangeOfData ()
		{
			_Data.validNumberRange = validNumberRange;
		}

		public void SetValidNumberRangeFromData ()
		{
			validNumberRange = _Data.validNumberRange;
		}

#if USE_UNITY_EVENTS
		public void SetIndexOfOnValueChangedUnityEventOfData ()
		{
			_Data.indexOfOnValueChangedUnityEvent = indexOfOnValueChangedUnityEvent;
		}

		public void SetIndexOfOnValueChangedUnityEventFromData ()
		{
			indexOfOnValueChangedUnityEvent = _Data.indexOfOnValueChangedUnityEvent;
			onValueChangedUnityEvent = LogicModule.instance.unityEvents[indexOfOnValueChangedUnityEvent];
		}
#endif
#if USE_EVENTS
		public void SetIndexOfOnValueChangedOfData ()
		{
			_Data.indexOfOnValueChanged = indexOfOnValueChanged;
		}

		public void SetIndexOfOnValueChangedFromData ()
		{
			indexOfOnValueChanged = _Data.indexOfOnValueChanged;
			onValueChanged = LogicModule.instance.events[indexOfOnValueChanged];
		}
#endif

		public enum Type
		{
			Integer,
			Float,
			String
		}

		[Serializable]
		public class Data : Option.Data
		{
			[SaveAndLoadValue]
			public Type type;
			[SaveAndLoadValue]
			public FloatRange validNumberRange;
#if USE_UNITY_EVENTS
			[SaveAndLoadValue]
			public int indexOfOnValueChangedUnityEvent;
#endif
#if USE_EVENTS
			[SaveAndLoadValue]
			public int indexOfOnValueChanged;
#endif
			
			public override object MakeAsset ()
			{
				TypingTargetOption typingTargetOption = ObjectPool.instance.SpawnComponent<TypingTargetOption>(LogicModule.instance.typingTargetOptionPrefab.prefabIndex, parent:LogicModule.instance.sceneTrs);
				Apply (typingTargetOption);
				if (!typingTargetOption.isInitialized)
					typingTargetOption.Init ();
				return typingTargetOption;
			}

			public override void Apply (Asset asset)
			{
				base.Apply (asset);
				TypingTargetOption typingTargetOption = (TypingTargetOption) asset;
				typingTargetOption._Data = this;
				typingTargetOption.SetTypeFromData ();
				typingTargetOption.SetValidNumberRangeFromData ();
#if USE_UNITY_EVENTS
				typingTargetOption.SetIndexOfOnValueChangedUnityEventFromData ();
#endif
#if USE_EVENTS
				typingTargetOption.SetIndexOfOnValueChangedFromData ();
#endif
			}
		}
	}
}