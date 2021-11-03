#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Extensions;

namespace EternityEngine
{
	[ExecuteInEditMode]
	public class ParentOptions : EditorScript
	{
		public Option parent;
		public Option child;
		static Option _parent;
		static Option _child;

		public override void Do ()
		{
			if (parent == null)
				parent = GetComponent<Option>();
			_Do (parent, child);
		}

		public static void _Do (Option parent, Option child)
		{
			if (!parent.children.Contains(child))
				parent.AddChild (child);
			else
				parent.RemoveChild (child);
		}

		[MenuItem("Game/Set selected Option as parent %#p")]
		static void SetParent ()
		{
			GameObject[] selectedGos = Selection.gameObjects;
			if (selectedGos.Length > 0)
			{
				_parent = selectedGos[0].GetComponent<Option>();
				if (_child != null)
					_Do (_parent, _child);
			}
			else
				_parent = null;
		}

		[MenuItem("Game/Set selected Option as child %#c")]
		static void SetChlid ()
		{
			GameObject[] selectedGos = Selection.gameObjects;
			if (selectedGos.Length > 0)
			{
				_child = selectedGos[0].GetComponent<Option>();
				if (_parent != null)
					_Do (_parent, _child);
			}
			else
				_child = null;
		}
	}
}
#else
namespace EternityEngine
{
	public class ParentOptions : EditorScript
	{
	}
}
#endif