#if UNITY_EDITOR
using UnityEditor;

namespace EternityEngine
{
	public class ToggleOptionsFaceEditorCamera : EditorScript
	{
		public static bool optionsFaceEditorCamera;

		public override void Do ()
		{
			_Do ();
		}

		[MenuItem("Game/Toggle Options facing editor Camera")]
		static void _Do ()
		{
			optionsFaceEditorCamera = !optionsFaceEditorCamera;
		}
	}
}
#else
namespace EternityEngine
{
	public class ToggleOptionsFaceEditorCamera : EditorScript
	{
		public override void Do ()
		{
		}
	}
}
#endif