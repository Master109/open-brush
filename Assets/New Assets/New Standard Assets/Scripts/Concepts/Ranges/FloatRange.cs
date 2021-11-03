using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Extensions;
using System;

[Serializable]
public class FloatRange : Range<float>
{
	public FloatRange (float min, float max) : base (min, max)
	{
	}

	public bool DoesIntersect (FloatRange floatRange, bool containsMinAndMax = true)
	{
		if (containsMinAndMax)
			return (min >= floatRange.min && min <= floatRange.max) || (floatRange.min >= min && floatRange.min <= max) || (max <= floatRange.max && max >= floatRange.min) || (floatRange.max <= max && floatRange.max >= min);
		else
			return (min > floatRange.min && min < floatRange.max) || (floatRange.min > min && floatRange.min < max) || (max < floatRange.max && max > floatRange.min) || (floatRange.max < max && floatRange.max > min);
	}

	public bool Contains (float f, bool containsMinAndMax = true)
	{
		return Contains(f, containsMinAndMax, containsMinAndMax);
	}

	public bool Contains (float f, bool containsMin = true, bool containsMax = true)
	{
		bool greaterThanMin = min < f;
		if (containsMin)
			greaterThanMin |= min == f;
		bool lessThanMax = f < max;
		if (containsMax)
			lessThanMax |= f == max;
		return greaterThanMin && lessThanMax;
	}

	public bool GetIntersectionRange (FloatRange floatRange, out FloatRange intersectionRange, bool containsMinAndMax = true)
	{
		intersectionRange = new FloatRange(float.NaN, float.NaN);
		if (DoesIntersect(floatRange, containsMinAndMax))
			intersectionRange = new FloatRange(Mathf.Max(min, floatRange.min), Mathf.Min(max, floatRange.max));
		return intersectionRange != new FloatRange(float.NaN, float.NaN);
	}

	public override float Get (float normalizedValue)
	{
		return (max - min) * normalizedValue + min;
	}

	public override float GetNormalized (float value)
	{
		return Mathf.InverseLerp(min, max, value);
	}
}