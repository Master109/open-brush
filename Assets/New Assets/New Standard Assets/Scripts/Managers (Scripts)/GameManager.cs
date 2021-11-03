using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using TMPro;
using Object = UnityEngine.Object;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace EternityEngine
{
	public class GameManager : SingletonMonoBehaviour<GameManager>, ISaveableAndLoadable
	{
		[SaveAndLoadValue]
		public List<Asset.Data> assetsData = new List<Asset.Data>();
		public SaveAndLoadObject saveAndLoadObject;
		// public GameObject[] registeredGos = new GameObject[0];
		// [SaveAndLoadValue]
		// static string enabledGosString = "";
		// [SaveAndLoadValue]
		// static string disabledGosString = "";
		// [SaveAndLoadValue]
		// public GameModifier[] gameModifiers = new GameModifier[0];
		public TMP_Text notificationText;
		// public static Dictionary<string, GameModifier> gameModifierDict = new Dictionary<string, GameModifier>();
		public static bool paused;
		public static IUpdatable[] updatables = new IUpdatable[0];
		public static int framesSinceLevelLoaded;
		public static bool isQuitting;
		public static float pausedTime;
		public static float TimeSinceLevelLoad
		{
			get
			{
				return Time.timeSinceLevelLoad - pausedTime;
			}
		}

		public override void Awake ()
		{
			base.Awake ();
			if (instance != this)
				return;
			// gameModifierDict.Clear();
			// for (int i = 0; i < gameModifiers.Length; i ++)
			// {
			// 	GameModifier gameModifier = gameModifiers[i];
			// 	gameModifierDict.Add(gameModifier.name, gameModifier);
			// }
			SceneManager.sceneLoaded += OnSceneLoaded;
		}

		void OnDestroy ()
		{
			if (instance == this)
				SceneManager.sceneLoaded -= OnSceneLoaded;
		}
		
		void OnSceneLoaded (Scene scene = new Scene(), LoadSceneMode loadMode = LoadSceneMode.Single)
		{
			StopAllCoroutines();
			framesSinceLevelLoaded = 0;
			pausedTime = 0;
		}

		void Update ()
		{
			for (int i = 0; i < updatables.Length; i ++)
			{
				IUpdatable updatable = updatables[i];
				updatable.DoUpdate ();
			}
			// if (Time.deltaTime > 0)
			// 	Physics.Simulate(Time.deltaTime);
			if (ObjectPool.Instance != null && ObjectPool.instance.enabled)
				ObjectPool.instance.DoUpdate ();
			InputSystem.Update ();
			framesSinceLevelLoaded ++;
			if (paused)
				pausedTime += Time.unscaledDeltaTime;
		}

		public void Quit ()
		{
			Application.Quit();
		}

		void OnApplicationQuit ()
		{
			isQuitting = true;
			// PlayerPrefs.DeleteAll();
			SaveAndLoadManager.instance.Save ("Auto-Save");
		}

		public void ToggleGameObject (GameObject go)
		{
			go.SetActive(!go.activeSelf);
		}

		public void DestroyChildren (Transform trs)
		{
			for (int i = 0; i < trs.childCount; i ++)
				Destroy(trs.GetChild(i).gameObject);
		}

		public void DestroyChildrenImmediate (Transform trs)
		{
			for (int i = 0; i < trs.childCount; i ++)
				DestroyImmediate(trs.GetChild(i).gameObject);
		}

		public static void Log (object obj)
		{
			print(obj);
		}
		
#if UNITY_EDITOR
		public static void DestroyOnNextEditorUpdate (Object obj)
		{
			EditorApplication.update += () => { if (obj == null) return; DestroyObject (obj); };
		}

		static void DestroyObject (Object obj)
		{
			if (obj == null)
				return;
			EditorApplication.update -= () => { DestroyObject (obj); };
			DestroyImmediate(obj);
		}
#endif
		
		// public static bool ModifierExistsAndIsActive (string name)
		// {
		// 	GameModifier gameModifier;
		// 	if (gameModifierDict.TryGetValue(name, out gameModifier))
		// 		return gameModifier.isActive;
		// 	else
		// 		return false;
		// }

		// public static bool ModifierIsActive (string name)
		// {
		// 	return gameModifierDict[name].isActive;
		// }

		// public static bool ModifierExists (string name)
		// {
		// 	return gameModifierDict.ContainsKey(name);
		// }

		// [Serializable]
		// public class GameModifier
		// {
		// 	public string name;
		// 	public bool isActive;
		// }
	}
}