#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace EternityEngine
{
	public class SetInputDevice : EditorScript
	{
		const string PATH_TO_INPUT_MANAGER = "Assets/Prefabs/Managers (Prefabs)/Input Manager.prefab";

		[MenuItem("Game/Use keyboard and mouse")]
		static void SetToKeyboardAndMouse ()
		{
			InputManager inputManager = (InputManager) AssetDatabase.LoadAssetAtPath(PATH_TO_INPUT_MANAGER, typeof(InputManager));
			inputManager.inputDevice = InputManager.InputDevice.KeyboardAndMouse;
			PrefabUtility.SavePrefabAsset(inputManager.gameObject);
		}

		[MenuItem("Game/Use VR")]
		static void SetToVR ()
		{
			InputManager inputManager = (InputManager) AssetDatabase.LoadAssetAtPath(PATH_TO_INPUT_MANAGER, typeof(InputManager));
			inputManager.inputDevice = InputManager.InputDevice.VR;
			PrefabUtility.SavePrefabAsset(inputManager.gameObject);
		}
	}
}
#else
namespace EternityEngine
{
	public class SetInputDevice : EditorScript
	{
	}
}
#endif