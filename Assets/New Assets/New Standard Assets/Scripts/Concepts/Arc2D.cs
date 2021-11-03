using System;
using Extensions;
using UnityEngine;

[Serializable]
public struct Arc2D
{
	public FloatRange degreeRange;
	public float radius;
	public Vector2 center;

	public Arc2D (FloatRange degreeRange, float radius, Vector2 center)
	{
		this.degreeRange = degreeRange;
		this.radius = radius;
		this.center = center;
	}

	public Vector2 GetPointAlongPerimeter (float distance)
	{
		return center + VectorExtensions.FromFacingAngle(degreeRange.Get(degreeRange.GetNormalized(distance))) * radius;
	}

	public Vector2 GetPointAlongPerimeterNormalized (float normalizedDistance)
	{
		return center + VectorExtensions.FromFacingAngle(degreeRange.Get(normalizedDistance)) * radius;
	}
}