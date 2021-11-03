using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Extensions
{
	public static class StringExtensions
	{
		public static string SubstringStartEnd (this string str, int startIndex, int endIndex)
		{
			return str.Substring(startIndex, endIndex - startIndex + 1);
		}
		
		public static string RemoveEach (this string str, string remove)
		{
			return str.Replace(remove, "");
		}
		
		public static string StartAfter (this string str, string startAfter)
		{
			return str.Substring(str.IndexOf(startAfter) + startAfter.Length);
		}

		public static string RemoveStartEnd (this string str, int startIndex, int endIndex)
		{
			return str.Remove(startIndex, endIndex - startIndex + 1);
		}

		public static string RemoveStartAt (this string str, string remove)
		{
			return str.Remove(str.IndexOf(remove));
		}

		public static string RemoveAfter (this string str, string remove)
		{
			return str.Remove(str.IndexOf(remove) + remove.Length);
		}

		public static string RemoveStartEnd (this string str, string startString, string endString)
		{
			string output = str;
			int indexOfStartString = str.IndexOf(startString);
			if (indexOfStartString != -1)
			{
				string startOfStr = str.Substring(0, indexOfStartString);
				str = str.Substring(indexOfStartString + startString.Length);
				output = startOfStr + str.RemoveStartEnd(0, str.IndexOf(endString) + endString.Length);
			}
			return output;
		}

		public static bool ContainsOnlyInstancesOf (this string str, string instance)
		{
			for (int i = 0; i < str.Length; i ++)
			{
				if (str.StartsWith(instance))
					str = str.Remove(0, instance.Length);
			}
			return str.Length > 0;
		}
	}
}