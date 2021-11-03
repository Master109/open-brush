using TMPro;
using System;
using Extensions;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace EternityEngine
{
	public class InstrumentOption : Option
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
		public int instrumentIndex;
		public bool isForLeftHand;

		public void SwitchToInstrument ()
		{
			if (isForLeftHand)
				LogicModule.instance.SetInstrument (LogicModule.instance.leftHand, instrumentIndex);
			else
				LogicModule.instance.SetInstrument (LogicModule.instance.rightHand, instrumentIndex);
		}

		public void SetRadius (Option radiusOption)
		{
			float radius = float.Parse(radiusOption.GetValue());
			if (isForLeftHand)
			{
				if (LogicModule.instance.leftHand.currentInstrumentIndex == instrumentIndex)
					LogicModule.instance.leftHand.currentInstrument.radius = radius;
				else
					LogicModule.leftHandInstruments[instrumentIndex].radius = radius;
			}
			else
			{
				if (LogicModule.instance.rightHand.currentInstrumentIndex == instrumentIndex)
					LogicModule.instance.rightHand.currentInstrument.radius = radius;
				else
					LogicModule.rightHandInstruments[instrumentIndex].radius = radius;
			}
		}

		public void SetMinLength (Option minLenghOption)
		{
			float minLength = float.Parse(minLenghOption.GetValue());
			if (isForLeftHand)
			{
				 if (LogicModule.instance.leftHand.currentInstrumentIndex == instrumentIndex)
					LogicModule.instance.leftHand.currentInstrument.length.min = minLength;
				else
					LogicModule.leftHandInstruments[instrumentIndex].radius = minLength;
			}
			else
			{
				if (LogicModule.instance.rightHand.currentInstrumentIndex == instrumentIndex)
					LogicModule.instance.rightHand.currentInstrument.length.min = minLength;
				else
					LogicModule.rightHandInstruments[instrumentIndex].length.min = minLength;
			}
		}

		public void SetMaxLength (Option maxLengthOption)
		{
			float maxLength = float.Parse(maxLengthOption.GetValue());
			if (isForLeftHand)
			{
				if (LogicModule.instance.leftHand.currentInstrumentIndex == instrumentIndex)
					LogicModule.instance.leftHand.currentInstrument.length.max = maxLength;
				else
					LogicModule.leftHandInstruments[instrumentIndex].radius = maxLength;
			}
			else
			{
				if (LogicModule.instance.rightHand.currentInstrumentIndex == instrumentIndex)
					LogicModule.instance.rightHand.currentInstrument.length.max = maxLength;
				else
					LogicModule.rightHandInstruments[instrumentIndex].length.max = maxLength;
			}
		}

		public void SetSampleCount (Option sampleCountOption)
		{
			int sampleCount = int.Parse(sampleCountOption.GetValue());
			if (isForLeftHand)
			{
				if (LogicModule.instance.leftHand.currentInstrumentIndex == instrumentIndex)
					LogicModule.instance.leftHand.currentInstrument.sampleCount = sampleCount;
				else
					LogicModule.leftHandInstruments[instrumentIndex].radius = sampleCount;
			}
			else
			{
				if (LogicModule.instance.rightHand.currentInstrumentIndex == instrumentIndex)
					LogicModule.instance.rightHand.currentInstrument.sampleCount = sampleCount;
				else
					LogicModule.rightHandInstruments[instrumentIndex].sampleCount = sampleCount;
			}
		}

		public void SetData ()
		{
			if (_Data == null)
				_Data = new Data();
			base.SetData ();
			SetInstrumentIndexOfData ();
			SetIsForLeftHandOfData ();
		}
		
		public void SetInstrumentIndexOfData ()
		{
			_Data.instrumentIndex = instrumentIndex;
		}

		public void SetInstrumentIndexFromData ()
		{
			instrumentIndex = _Data.instrumentIndex;
		}
		
		public void SetIsForLeftHandOfData ()
		{
			_Data.isForLeftHand = isForLeftHand;
		}

		public void SetIsForLeftHandFromData ()
		{
			isForLeftHand = _Data.isForLeftHand;
		}

		[Serializable]
		public class Data : Option.Data
		{
			[SaveAndLoadValue]
			public int instrumentIndex;
			[SaveAndLoadValue]
			public bool isForLeftHand;
			
			public override object MakeAsset ()
			{
				InstrumentOption instrumentOption = ObjectPool.instance.SpawnComponent<InstrumentOption>(LogicModule.instance.instrumentOptionPrefab.prefabIndex, parent:LogicModule.instance.sceneTrs);
				Apply (instrumentOption);
				return instrumentOption;
			}

			public override void Apply (Asset asset)
			{
				base.Apply (asset);
				InstrumentOption instrumentOption = (InstrumentOption) asset;
				instrumentOption._Data = this;
				instrumentOption.SetInstrumentIndexFromData ();
				instrumentOption.SetIsForLeftHandFromData ();
			}
		}
	}
}