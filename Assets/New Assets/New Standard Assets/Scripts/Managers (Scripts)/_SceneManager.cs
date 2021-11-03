using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EternityEngine
{
	public class _SceneManager : SingletonMonoBehaviour<_SceneManager>//, ISaveableAndLoadable
	{
		public float transitionRate;
		[SaveAndLoadValue]
		public string mostRecentSceneName;
		public static bool isLoading;
		public static Scene CurrentScene
		{
			get
			{
				return SceneManager.GetActiveScene();
			}
		}

		public override void Awake ()
		{
			base.Awake ();
			isLoading = false;
		}
		
		public void LoadSceneWithTransition (string sceneName)
		{
			if (Instance != this)
			{
				instance.LoadSceneWithTransition (sceneName);
				return;
			}
			isLoading = true;
			StartCoroutine (SceneTransition (sceneName));
		}
		
		public void LoadSceneWithoutTransition (string sceneName)
		{
			isLoading = true;
			SceneManager.LoadScene(sceneName);
		}
		
		public void LoadSceneWithTransition (int sceneId)
		{
			if (Instance != this)
			{
				instance.LoadSceneWithTransition (sceneId);
				return;
			}
			isLoading = true;
			StartCoroutine (SceneTransition (sceneId));
		}
		
		public void LoadSceneWithoutTransition (int sceneId)
		{
			isLoading = true;
			SceneManager.LoadScene(sceneId);
		}
		
		public void LoadSceneAdditiveWithTransition (string sceneName)
		{
			if (Instance != this)
			{
				Instance.LoadSceneAdditiveWithTransition (sceneName);
				return;
			}
			isLoading = true;
			StartCoroutine (SceneTransition (sceneName, LoadSceneMode.Additive));
		}
		
		public void LoadSceneAdditiveWithoutTransition (string sceneName)
		{
			isLoading = true;
			SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
		}

		public void LoadSceneAdditiveWithTransition (int sceneId)
		{
			if (Instance != this)
			{
				Instance.LoadSceneAdditiveWithTransition (sceneId);
				return;
			}
			isLoading = true;
			StartCoroutine (SceneTransition (sceneId, LoadSceneMode.Additive));
		}
		
		public void LoadSceneAdditiveWithoutTransition (int sceneId)
		{
			isLoading = true;
			SceneManager.LoadScene(sceneId, LoadSceneMode.Additive);
		}
		
		public AsyncOperation LoadSceneAsyncAdditiveWithoutTransition (string sceneName)
		{
			isLoading = true;
			return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
		}
		
		public AsyncOperation LoadSceneAsyncAdditiveWithoutTransition (int sceneId)
		{
			isLoading = true;
			return SceneManager.LoadSceneAsync(sceneId, LoadSceneMode.Additive);
		}
		
		public void RestartSceneWithTransition ()
		{
			LoadSceneWithTransition (CurrentScene.name);
		}
		
		public void RestartSceneWithoutTransition ()
		{
			LoadSceneWithoutTransition (CurrentScene.name);
		}
		
		public void NextSceneWithTransition ()
		{
			LoadSceneWithTransition (CurrentScene.buildIndex + 1);
		}
		
		public void NextSceneWithoutTransition ()
		{
			LoadSceneWithoutTransition (CurrentScene.buildIndex + 1);
		}
		
		public void OnSceneLoaded (Scene scene = new Scene(), LoadSceneMode loadMode = LoadSceneMode.Single)
		{
			Camera.main.rect = new Rect(.5f, .5f, 0, 0);
			StartCoroutine(SceneTransition (null));
			SceneManager.sceneLoaded -= OnSceneLoaded;
			isLoading = false;
			mostRecentSceneName = scene.name;
		}
		
		public IEnumerator SceneTransition (string sceneName = null, LoadSceneMode loadMode = LoadSceneMode.Single)
		{
			bool transitioningIn = string.IsNullOrEmpty(sceneName);
			float transitionRateMultiplier = 1;
			if (transitioningIn)
				transitionRateMultiplier *= -1;
			while ((Camera.main.rect.size.x > 0 && !transitioningIn) || (Camera.main.rect.size.x < 1 && transitioningIn))
			{
				Rect cameraRect = Camera.main.rect;
				cameraRect.size -= Vector2.one * transitionRate * transitionRateMultiplier * Time.unscaledDeltaTime;
				cameraRect.center += Vector2.one * transitionRate * transitionRateMultiplier * Time.unscaledDeltaTime / 2;
				Camera.main.rect = cameraRect;
				yield return new WaitForEndOfFrame();
			}
			if (transitioningIn)
				Camera.main.rect = new Rect(0, 0, 1, 1);
			else
			{
				Camera.main.rect = new Rect(.5f, .5f, 0, 0);
				SceneManager.sceneLoaded += OnSceneLoaded;
				if (!string.IsNullOrEmpty(sceneName))
					SceneManager.LoadScene(sceneName, loadMode);
			}
		}

		public IEnumerator SceneTransition (int sceneId = -1, LoadSceneMode loadMode = LoadSceneMode.Single)
		{
			yield return StartCoroutine(SceneTransition (SceneManager.GetSceneByBuildIndex(sceneId).name, loadMode));
		}
	}
}