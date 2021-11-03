#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace Extensions
{
	public class SelectionExtensions
	{
		public static T[] GetSelected<T> () where T : Object
		{
			List<T> output = new List<T>();
			for (int i = 0; i < Selection.gameObjects.Length; i ++)
			{
				GameObject go = Selection.gameObjects[i];
				T obj = go.GetComponent<T>();
				if (obj != null)
					output.Add(obj);
			}
			return output.ToArray();
		}
	}
}
#endif