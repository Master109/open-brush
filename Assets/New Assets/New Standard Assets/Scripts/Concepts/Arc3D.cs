using System;
using Extensions;
using UnityEngine;

[Serializable]
public struct Arc3D
{
	public FloatRange degreeRange;
	public float radius;
	public Vector3 center;
	public Vector3 rotation;

	public Arc3D (FloatRange degreeRange, float radius, Vector3 center, Vector3 rotation)
	{
		this.degreeRange = degreeRange;
		this.radius = radius;
		this.center = center;
		this.rotation = rotation;
	}

	public Vector3 GetPointAlongPerimeter (float distance)
	{
		return center + (Quaternion.Euler(rotation) * ((Vector3) VectorExtensions.FromFacingAngle(degreeRange.Get(degreeRange.GetNormalized(distance))) * radius));
	}

	public Vector3 GetPointAlongPerimeterNormalized (float normalizedDistance)
	{
		return center + (Quaternion.Euler(rotation) * ((Vector3) VectorExtensions.FromFacingAngle(degreeRange.Get(normalizedDistance)) * radius));
	}
}