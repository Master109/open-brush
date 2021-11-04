﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Extensions;
using System;

[Serializable]
public class LineSegment2D
{
	public Vector2 start;
	public Vector2 end;

	public LineSegment2D ()
	{
	}

	public LineSegment2D (Vector2 start, Vector2 end)
	{
		this.start = start;
		this.end = end;
	}

	public override string ToString ()
	{
		return "[" + start + "], [" + end + "]";
	}
	
	public float GetSlope ()
	{
		return (end.y - start.y) / (end.x - start.x);
	}
	
	public float GetFacingAngle ()
	{
		return (end - start).GetFacingAngle();
	}

	public bool DoIIntersectWithLineSegment (LineSegment2D other, bool shouldIncludeEndPoints)
	{
		bool output = false;
		float denominator = (other.end.y - other.start.y) * (end.x - start.x) - (other.end.x - other.start.x) * (end.y - start.y);
		if (denominator != 0f)
		{
			float u_a = ((other.end.x - other.start.x) * (start.y - other.start.y) - (other.end.y - other.start.y) * (start.x - other.start.x)) / denominator;
			float u_b = ((end.x - start.x) * (start.y - other.start.y) - (end.y - start.y) * (start.x - other.start.x)) / denominator;
			if (shouldIncludeEndPoints)
			{
				if (u_a >= 0f && u_a <= 1f && u_b >= 0f && u_b <= 1f)
					output = true;
			}
			else
			{
				if (u_a > 0f && u_a < 1f && u_b > 0f && u_b < 1f)
					output = true;
			}
		}
		return output;
	}

	public bool DoIIntersectWithCircle (Vector2 center, float radius)
	{
		return Vector2.Distance(ClosestPoint(center), center) <= radius;
	}

	// public bool DoIIntersectWithCircle (Vector2 center, float radius)
	// {
	// 	return Vector2.Distance(GetPointWithDirectedDistance(GetDirectedDistanceAlongParallel(center)), center) <= radius;
	// }

	// public bool DoIIntersectWithCircle (Vector2 center, float radius)
	// {
	// 	Vector2 lineDirection = GetDirection();
	// 	Vector2 centerToLineStart = start - center;
	// 	float a = Vector2.Dot(lineDirection, lineDirection);
	// 	float b = 2 * Vector2.Dot(centerToLineStart, lineDirection);
	// 	float c = Vector2.Dot(centerToLineStart, centerToLineStart) - radius * radius;
	// 	float discriminant = b * b - 4 * a * c;
	// 	if (discriminant >= 0)
	// 	{
	// 		discriminant = Mathf.Sqrt(discriminant);
	// 		float t1 = (-b - discriminant) / (2 * a);
	// 		float t2 = (-b + discriminant) / (2 * a);
	// 		if (t1 >= 0 && t1 <= 1 || t2 >= 0 && t2 <= 1)
	// 			return true;
	// 	}
	// 	return false;
	// }
	
	public bool ContainsPoint (Vector2 point)
	{
		return Vector2.Distance(point, start) + Vector2.Distance(point, end) == Vector2.Distance(start, end);
	}
	
	public LineSegment2D Move (Vector2 movement)
	{
		return new LineSegment2D(start + movement, end + movement);
	}
	
	public LineSegment2D Rotate (Vector2 pivotPoint, float degrees)
	{
		LineSegment2D output;
		Vector2 outputStart = start.Rotate(pivotPoint, degrees);
		Vector2 outputEnd = end.Rotate(pivotPoint, degrees);
		output = new LineSegment2D(outputStart, outputEnd);
		return output;
	}

	public Vector2 ClosestPoint (Vector2 point)
	{
		Vector2 output;
		float directedDistanceAlongParallel = GetDirectedDistanceAlongParallel(point);
		if (directedDistanceAlongParallel > 0 && directedDistanceAlongParallel < GetLength())
			output = GetPointWithDirectedDistance(directedDistanceAlongParallel);
		else if (directedDistanceAlongParallel >= GetLength())
			output = end;
		else
			output = start;
		return output;
	}

	public LineSegment2D GetPerpendicular (bool rotateClockwise = false)
	{
		if (rotateClockwise)
			return Rotate(GetMidpoint(), -90);
		else
			return Rotate(GetMidpoint(), 90);
	}

	public Vector2 GetMidpoint ()
	{
		return (start + end) / 2;
	}

	public float GetDirectedDistanceAlongParallel (Vector2 point)
	{
		float rotate = -GetFacingAngle();
		LineSegment2D rotatedLine = Rotate(Vector2.zero, rotate);
		point = point.Rotate(rotate);
		return point.x - rotatedLine.start.x;
	}

	public Vector2 GetPointWithDirectedDistance (float directedDistance)
	{
		return start + (GetDirection() * directedDistance);
	}

	public float GetLength ()
	{
		return Vector2.Distance(start, end);
	}

	public Vector2 GetDirection ()
	{
		return (end - start).normalized;
	}
	
	public LineSegment2D Flip ()
	{
		return new LineSegment2D(end, start);
	}
}