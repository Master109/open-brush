using System;
using UnityEngine;
using System.Collections.Generic;

namespace EternityEngine
{
	[Serializable]
	public class Instrument : ISaveableAndLoadable
	{
		[SaveAndLoadValue]
		public bool Active
		{
			get
			{
				return trs.gameObject.activeSelf;
			}
			set
			{
				trs.gameObject.SetActive(value);
			}
		}
		[SaveAndLoadValue]
		public ModulationCurve volumeCurveOnPenetrateEmitter;
		[SaveAndLoadValue]
		public ModulationCurve volumeCurveOnDepenetrateEmitter;
		[SaveAndLoadValue]
		public ModulationCurve panCurveOnPenetrateEmitter;
		[SaveAndLoadValue]
		public ModulationCurve panCurveOnDepenetrateEmitter;
		[SaveAndLoadValue]
		public ModulationCurve pitchCurveOnPenetrateEmitter;
		[SaveAndLoadValue]
		public ModulationCurve pitchCurveOnDepenetrateEmitter;
		public Transform trs;
		[SaveAndLoadValue]
		public FloatRange length;
		[SaveAndLoadValue]
		public float radius;
		[SaveAndLoadValue]
		public int sampleCount;
		[SaveAndLoadValue]
		public bool showMinAndMaxLength;
		[SaveAndLoadValue]
		public bool showSamples;
		public LineRenderer lineRenderer;

		public virtual void UpdateGraphics (LogicModule.Hand hand)
		{
			lineRenderer.widthMultiplier = radius * 2;
			trs.localScale = Vector3.one * radius * 2;
		}

		public virtual Option[] GetSelectedOptions (LogicModule.Hand hand)
		{
			throw new NotImplementedException();
		}

		public virtual Option[] GetSelectedOptions (LogicModule.Hand hand, Vector3 position)
		{
			SortedDictionary<float, Option> selectedOptionsDict = new SortedDictionary<float, Option>();
			for (int i = 0; i < Option.instances.Count; i ++)
			{
				Option option = Option.instances[i];
				Bounds optionBounds = option.collider.bounds;
				float distanceToPositionSqr = (position - optionBounds.center).sqrMagnitude;
				if (distanceToPositionSqr <= (optionBounds.extents.x + radius) * (optionBounds.extents.x + radius))
					selectedOptionsDict[Mathf.Sqrt(distanceToPositionSqr) - optionBounds.extents.x] = option;
			}
			Option[] output = new Option[selectedOptionsDict.Count];
			selectedOptionsDict.Values.CopyTo(output, 0);
			return output;
		}
	}
}