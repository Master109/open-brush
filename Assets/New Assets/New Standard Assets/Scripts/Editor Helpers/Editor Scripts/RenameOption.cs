#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Extensions;

namespace EternityEngine
{
	[ExecuteInEditMode]
	public class RenameOption : EditorScript
	{
		public Option option;

		public override void Do ()
		{
			if (option == null)
				option = GetComponent<Option>();
			_Do (option);
		}

		public static void _Do (Option option)
		{
			string name = option.text.text;
			int indexOfSpaceAndLeftParenthesis = name.LastIndexOf(" (");
			if (indexOfSpaceAndLeftParenthesis != -1)
			{
				int indexOfRightParenthesis = name.LastIndexOf(")");
				name = name.RemoveStartEnd(indexOfSpaceAndLeftParenthesis, indexOfRightParenthesis);
			}
			option.text.text = name;
			option.enabled = !option.enabled;
			option.enabled = !option.enabled;
		}

		[MenuItem("Game/Rename selected Options")]
		static void _DoToSelected ()
		{
			GameObject[] selectedGos = Selection.gameObjects;
			for (int i = 0; i < selectedGos.Length; i ++)
			{
				GameObject go = selectedGos[i];
				Option option = go.GetComponent<Option>();
				_Do (option);
			}
		}

		[MenuItem("Game/Rename all Options")]
		static void _DoToAll ()
		{
			Option[] options = FindObjectsOfType<Option>(true);
			for (int i = 0; i < options.Length; i ++)
			{
				Option option = options[i];
				_Do (option);
			}
		}
	}
}
#else
namespace EternityEngine
{
	public class RenameOption : EditorScript
	{
	}
}
#endif