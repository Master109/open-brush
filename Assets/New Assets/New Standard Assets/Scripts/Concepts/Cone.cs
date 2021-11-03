using System;
using Extensions;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[Serializable]
public struct Cone
{
	public Vector3 basePosition;
	public Vector3 upAxis;
	public float baseRadius;
	public float baseAngle;

	public Cone (Vector3 basePosition, Vector3 upAxis, float baseRadius, float baseAngle)
	{
		this.basePosition = basePosition;
		this.upAxis = upAxis;
		this.baseRadius = baseRadius;
		this.baseAngle = baseAngle;
	}

	public Mesh ToMesh (int basePointCount, FaceType faceType, float angleToFirstBasePoint = 0)
	{
		Mesh output = new Mesh();
		output.name = "Cone (Generated)";
		Circle2D circle = new Circle2D(baseRadius);
		Vector2[] basePoints = circle.GetPointsAlongOutside(360f / basePointCount, angleToFirstBasePoint);
		float height = Mathf.Tan(baseAngle * Mathf.Deg2Rad) * baseRadius;
		Vector3[] vertices = new Vector3[basePointCount + 2];
		for (int i = 0; i < basePointCount; i ++)
		{
			Vector3 basePoint = basePoints[i];
			vertices[i + 2] = basePosition + Quaternion.FromToRotation(Vector3.up, upAxis) * basePoint.XYToXZ();
		}
		vertices[0] = basePosition;
		vertices[1] = basePosition + upAxis.normalized * height;
		List<int> triangles = new List<int>();
		bool makeInsideFaces = faceType == FaceType.Inside || faceType == FaceType.Both;
		bool makeOutsideFaces = faceType == FaceType.Outside || faceType == FaceType.Both;
		for (int i = 2; i <= basePointCount; i ++)
		{
			if (makeInsideFaces)
			{
				triangles.Add(0);
				triangles.Add(i + 1);
				triangles.Add(i);
				triangles.Add(1);
				triangles.Add(i);
				triangles.Add(i + 1);
			}
			if (makeOutsideFaces)
			{
				triangles.Add(0);
				triangles.Add(i);
				triangles.Add(i + 1);
				triangles.Add(1);
				triangles.Add(i + 1);
				triangles.Add(i);
			}
		}
		if (makeInsideFaces)
		{
			triangles.Add(0);
			triangles.Add(2);
			triangles.Add(vertices.Length - 1);
			triangles.Add(1);
			triangles.Add(vertices.Length - 1);
			triangles.Add(2);
		}
		if (makeOutsideFaces)
		{
			triangles.Add(0);
			triangles.Add(vertices.Length - 1);
			triangles.Add(2);
			triangles.Add(1);
			triangles.Add(2);
			triangles.Add(vertices.Length - 1);
		}
		output.vertices = vertices;
		output.triangles = triangles.ToArray();
		output.RecalculateNormals();
		return output;
	}

	public MeshRenderer MakeMeshRenderer (int basePointCount, FaceType faceType, float angleToFirstBasePoint = 0)
	{
		GameObject go = new GameObject();
		MeshRenderer meshRenderer = go.AddComponent<MeshRenderer>();
		MeshFilter meshFilter = go.AddComponent<MeshFilter>();
		meshFilter.sharedMesh = ToMesh(basePointCount, faceType, 0);
		return meshRenderer;
	}

	public enum FaceType
	{
		Inside,
		Outside,
		Both
	}
}