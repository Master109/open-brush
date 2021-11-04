using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Extensions
{
	public static class CollectionExtensions 
	{
		public static List<T> ToList<T> (this T[] array)
		{
			return new List<T>(array);
		}

		public static T[] Add<T> (this T[] array, T element)
		{
			List<T> output = array.ToList();
			output.Add(element);
			return output.ToArray();
		}

		public static T[] Remove<T> (this T[] array, T element)
		{
			List<T> output = array.ToList();
			output.Remove(element);
			return output.ToArray();
		}

		public static T[] RemoveAt<T> (this T[] array, int index)
		{
			List<T> output = array.ToList();
			output.RemoveAt(index);
			return output.ToArray();
		}

		public static T[] AddRange<T> (this T[] array, IEnumerable<T> array2)
		{
			List<T> output = array.ToList();
			output.AddRange(array2);
			return output.ToArray();
		}

		public static bool Contains<T> (this T[] array, T element)
		{
			foreach (T obj in array)
			{
				if (obj == null)
				{
					if (element == null)
						return true;
				}
				else if (obj.Equals(element))
					return true;
			}
			return false;
		}

		public static int IndexOf<T> (this T[] array, T element)
		{
			for (int i = 0; i < array.Length; i ++)
			{
				if (array[i].Equals(element))
					return i;
			}
			return -1;
		}
		
		public static T[] Reverse<T> (this T[] array)
		{
			List<T> output = array.ToList();
			output.Reverse();
			return output.ToArray();
		}

		public static T[] AddArray<T> (this T[] array, Array array2)
		{
			List<T> output = array.ToList();
			for (int i = 0; i < array2.Length; i ++)
				output.Add((T) array2.GetValue(i));
			return output.ToArray();
		}

		public static string ToString<T> (this T[] array, string elementSeperator = ", ")
		{
            string output = "";
            foreach (T element in array)
                output += element.ToString() + elementSeperator;
			return output;
		}

		public static T[] RemoveEach<T> (this T[] array, IEnumerable<T> array2)
		{
			List<T> output = array.ToList();
			foreach (T element in array2)
				output.Remove(element);
			return output.ToArray();
		}

		public static T[] Insert<T> (this T[] array, T element, int index)
		{
			List<T> output = array.ToList();
			output.Insert(index, element);
			return output.ToArray();
		}

		public static int IndexOf<T> (this Array array, T element)
		{
			for (int index = 0; index < array.GetLength(0); index ++)
			{
				if (((T) array.GetValue(index)).Equals(element))
				{
					return index;
				}
			}
			return -1;
		}

		public static T[] _Sort<T> (this T[] array, IComparer<T> sorter)
		{
			List<T> output = array.ToList();
			output.Sort(sorter);
			return output.ToArray();
		}

		public static int Count (this IEnumerable enumerable)
		{
			int output = 0;
			IEnumerator enumerator = enumerable.GetEnumerator();
			while (enumerator.MoveNext())
				output ++;
			return output;
		}

		public static T Get<T> (this IEnumerable<T> enumerable, int index)
		{
			IEnumerator enumerator = enumerable.GetEnumerator();
			while (enumerator.MoveNext())
			{
				index --;
				if (index < 0)
					return (T) enumerator.Current;
			}
			return default(T);
		}

		public static float GetMin (this float[] array)
		{
			float min = array[0];
			for (int i = 1; i < array.Length; i ++)
			{
				if (array[i] < min)
					min = array[i];
			}
			return min;
		}

		public static float GetMax (this float[] array)
		{
			float max = array[0];
			for (int i = 1; i < array.Length; i ++)
			{
				if (array[i] > max)
					max = array[i];
			}
			return max;
		}

		public static List<T> _Add<T> (this List<T> list, T element)
		{
			list.Add(element);
			return list;
		}
		
		public static int Length<T> (this List<T> list)
		{
			return list.Count;
		}
		
		public static List<T> _TrimEnd<T> (this List<T> list, int count)
		{
			list.RemoveRange(list.Count - count, count);
			return list;
		}
		
		public static List<T> _RemoveAt<T> (this List<T> list, int index)
		{
			list.RemoveAt(index);
			return list;
		}
		
		public static List<T> _Remove<T> (this List<T> list, T element)
		{
			list.Remove(element);
			return list;
		}
		
		public static T[] _RemoveAt<T> (this T[] array, int index)
		{
			array = array.RemoveAt(index);
			return array;
		}
		
		public static T[] _Remove<T> (this T[] array, T element)
		{
			array = array.Remove(element);
			return array;
		}
		
		public static T[] _Add<T> (this T[] array, T element)
		{
			array = array.Add(element);
			return array;
		}

		public static T1[] GetKeys<T1, T2> (this Dictionary<T1, T2> dict)
		{
			List<T1> output = new List<T1>();
			IEnumerator keyEnumerator = dict.Keys.GetEnumerator();
			while (keyEnumerator.MoveNext())
				output.Add((T1) keyEnumerator.Current);
			return output.ToArray();
		}

		public static bool Contains_IList<T> (this IList<T> list, T element)
		{
			return list.Contains(element);
		}

		public static void RotateRight (IList list, int count)
		{
			object element = list[count - 1];
			list.RemoveAt(count - 1);
			list.Insert(0, element);
		}

		public static IEnumerable<IList> GetPermutations (this IList list, int count)
		{
			if (count == 1)
				yield return list;
			else
			{
				for (int i = 0; i < count; i ++)
				{
					foreach (IList permutation in GetPermutations(list, count - 1))
						yield return permutation;
					RotateRight (list, count);
				}
			}
		}

		public static IEnumerable<IList> GetPermutations (this IList list)
		{
			return list.GetPermutations(list.Count);
		}

		public static IEnumerable<IList> GetPermutations<T> (this T[] array, int count)
		{
			return array.ToList().GetPermutations(count);
		}

		public static IEnumerable<IList> GetPermutations<T> (this T[] array)
		{
			return array.GetPermutations(array.Length);
		}

		public static IEnumerable<IList>[] GetPermutations<T> (params T[][] arrays)
		{
			IList[] output;
			object[] temporary = new object[arrays.Length];
			output = new IList[temporary.GetPermutations().Count()];
			for (int i = 0; i < arrays.Length; i ++)
			{
				T[] array = arrays[i];
				output.AddRange(array.GetPermutations());
			}
			return (IEnumerable<IList>[]) output;
		}

		public static Vector2 ToVec2 (this float[] components)
		{
			return new Vector2(components[0], components[1]);
		}

		public static Vector3 ToVec3 (this float[] components)
		{
			return new Vector3(components[0], components[1], components[2]);
		}

		public static Vector2 ToVec2 (this int[] components)
		{
			return new Vector2(components[0], components[1]);
		}

		public static Vector3 ToVec3 (this int[] components)
		{
			return new Vector3(components[0], components[1], components[2]);
		}

		public static Vector2Int ToVec2Int (this int[] components)
		{
			return new Vector2Int(components[0], components[1]);
		}

		public static Vector3Int ToVec3Int (this int[] components)
		{
			return new Vector3Int(components[0], components[1], components[2]);
		}

		public static bool AreAllInstancesFound<T> (this List<T> list, T[] instances)
		{
			for (int i = 0; i < instances.Length; i ++)
			{
				T instance = instances[i];
				if (!list.Contains(instance))
					return false;
			}
			return true;
		}

		public static int InstanceOccuranceCount<T> (this List<T> list, T instance)
		{
			int output = 0;
			for (int i = 0; i < list.Count; i ++)
			{
				T value = list[i];
				if (value.Equals(instance))
					output ++;
			}
			return output;
		}

		public static bool AreAllInstancesFoundEqualNumberOfTimes<T> (this List<T> list, T[] instances)
		{
			int instanceOccuranceCount = list.InstanceOccuranceCount(instances[0]);
			for (int i = 1; i < instances.Length; i ++)
			{
				T instance = instances[i];
				if (list.InstanceOccuranceCount(instance) != instanceOccuranceCount)
					return false;
			}
			return true;
		}

		public static bool IsEveryInstanceFoundAfterEveryOther<T> (this List<T> list, T[] instances, T[] others)
		{
			int maxIndexOfInstance = 0;
			for (int i = 0; i < instances.Length; i ++)
			{
				T instance = instances[i];
				int indexOfInstance = list.IndexOf(instance);
				if (indexOfInstance == -1)
					return false;
				else if (indexOfInstance > maxIndexOfInstance)
					maxIndexOfInstance = indexOfInstance;
			}
			for (int i = 0; i < others.Length; i ++)
			{
				T other = others[i];
				int indexOfOther = list.IndexOf(other);
				if (indexOfOther <= maxIndexOfInstance)
					return false;
			}
			return true;
		}

		public static bool? IsEveryInstanceFoundAfterAllOthers<T> (this List<T> list, T[] instances, T[] others)
		{
			int maxIndexOfInstance = 0;
			for (int i = 0; i < instances.Length; i ++)
			{
				T instance = instances[i];
				int indexOfInstance = list.IndexOf(instance);
				if (indexOfInstance == -1)
					return false;
				else if (indexOfInstance > maxIndexOfInstance)
					maxIndexOfInstance = indexOfInstance;
			}
			int maxIndexOfOther = -1;
			for (int i = 0; i < others.Length; i ++)
			{
				T other = others[i];
				int indexOfOther = list.IndexOf(other);
				if (indexOfOther != -1)
				{
					if (indexOfOther <= maxIndexOfInstance)
						return false;
					if (indexOfOther > maxIndexOfOther)
						maxIndexOfOther = indexOfOther;
				}
			}
			if (maxIndexOfOther == -1)
				return null;
			return true;
		}

		public static bool? AreAllInstancesFoundAfterAllOthers<T> (this List<T> list, T[] instances, T[] others)
		{
			int maxIndexOfInstance = -1;
			for (int i = 0; i < instances.Length; i ++)
			{
				T instance = instances[i];
				int indexOfInstance = list.IndexOf(instance);
				if (indexOfInstance > maxIndexOfInstance)
					maxIndexOfInstance = indexOfInstance;
			}
			if (maxIndexOfInstance == -1)
				return null;
			int maxIndexOfOther = -1;
			for (int i = 0; i < others.Length; i ++)
			{
				T other = others[i];
				int indexOfOther = list.IndexOf(other);
				if (indexOfOther != -1)
				{
					if (indexOfOther <= maxIndexOfInstance)
						return false;
					if (indexOfOther > maxIndexOfOther)
						maxIndexOfOther = indexOfOther;
				}
			}
			if (maxIndexOfOther == -1)
				return null;
			return true;
		}

		public static bool? AreAllInstancesFoundAfterEveryOther<T> (this List<T> list, T[] instances, T[] others)
		{
			int maxIndexOfInstance = -1;
			for (int i = 0; i < instances.Length; i ++)
			{
				T instance = instances[i];
				int indexOfInstance = list.IndexOf(instance);
				if (indexOfInstance > maxIndexOfInstance)
					maxIndexOfInstance = indexOfInstance;
			}
			if (maxIndexOfInstance == -1)
				return null;
			for (int i = 0; i < others.Length; i ++)
			{
				T other = others[i];
				int indexOfOther = list.IndexOf(other);
				if (indexOfOther <= maxIndexOfInstance)
					return false;
			}
			return true;
		}

		public static int[] GetIndicesOf<T> (this List<T> list, T instance)
		{
			List<int> output = new List<int>();
			int indexOfInstance = 0;
			while (true)
			{
				indexOfInstance = list.IndexOf(instance, indexOfInstance);
				if (indexOfInstance != -1)
					output.Add(indexOfInstance);
				else
					break;
			}
			return output.ToArray();
		}
	}
}