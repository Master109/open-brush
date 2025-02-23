using UnityEngine;
using System.Collections.Generic;
using System.Collections;

namespace Extensions
{
	public static class MatrixExtensions
	{
		public static Vector3 GetPosition (this Matrix4x4 matrix)
		{
			return new Vector3(matrix[0, 3], matrix[1, 3], matrix[2, 3]);
		}
	}
}