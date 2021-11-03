#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace EternityEngine
{
	public class MakeAsset : EditorScript
	{
		public Object obj;
		public string assetPath;

		public override void Do ()
		{
			_Do (obj, assetPath);
		}

		public static void _Do (Object obj, string assetPath)
		{
			AssetDatabase.DeleteAsset(assetPath);
			AssetDatabase.CreateAsset(obj, assetPath);
		}
	}
}
#else
namespace EternityEngine
{
	public class MakeAsset : EditorScript
	{
	}
}
#endif