using System;
using UnityEngine;

[Serializable]
public struct ModulationCurve
{
	public AnimationCurve normalizedValueOverTimeCurve;

	[Serializable]
	public struct KeyframeData
	{
		public float time;
		public float value;
		public WeightedMode weightedMode;
		public float inTangent;
		public float inWeight;
		public float outTangent;
		public float outWeight;

		public Keyframe ToKeyframe ()
		{
			Keyframe output = new Keyframe(time, value, inTangent, outTangent, inWeight, outWeight);
			output.weightedMode = weightedMode;
			return output;
		}
	}
}