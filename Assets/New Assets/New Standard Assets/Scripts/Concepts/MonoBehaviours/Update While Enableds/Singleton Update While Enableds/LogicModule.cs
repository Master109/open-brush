using TMPro;
using System;
using System.IO;
using Extensions;
using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using TriLibCore;
using TriLibCore.General;
using TriLibCore.Utils;
using UnityEngine.Networking;
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine.InputSystem;
#endif

namespace EternityEngine
{
	[ExecuteInEditMode]
	public class LogicModule : SingletonUpdateWhileEnabled<LogicModule>, ISaveableAndLoadable
	{
		public AudioSource audioSource;
		public int recordingLength;
		public int recordingFrequency;
		public CurveInstrument leftHandCurveInstrument;
		public TrailInstrument leftHandTrailInstrument;
		public CurveInstrument rightHandCurveInstrument;
		public TrailInstrument rightHandTrailInstrument;
		public Transform sceneTrs;
		public SpawnOrientationOption currentSpawnOrientationOption;
		public Option renameChildrenOption;
		public Option modulationNameOption;
		public Option automationSequenceNameOption;
		public Option optionPrefab;
		public TypingTargetOption typingTargetOptionPrefab;
		public InstrumentOption instrumentOptionPrefab;
		public AudioRecordingOption audioRecordingOptionPrefab;
		public SoundOption soundOptionPrefab;
		public ModulationOption modulationOptionPrefab;
		public ModulationTargetValueRangeOption modulationTargetValueRangeOptionPrefab;
		public BoolOption boolOptionPrefab;
		public EnumOption enumOptionPrefab;
		public CodeCommandOption codeCommandOptionPrefab;
		public AutomationOption automationOptionPrefab;
		public AutomationSequenceEntryOption automationSequenceEntryOptionPrefab;
		public ConditionalOption conditionalOptionPrefab;
		public DeltaTimeOption deltaTimeOptionPrefab;
		public RandomValueOption randomValueOptionPrefab;
		public TransformOption transformOptionPrefab;
		public ModelOption modelOptionPrefab;
		public SpawnOrientationOption spawnOrientationOptionPrefab;
		public SetterOption setterOptionPrefab;
		public Vector2Option vector2OptionPrefab;
		public Vector3Option vector3OptionPrefab;
		public ArithmeticOption arithmeticOptionPrefab;
		public ActivationBehaviourOption activationBehaviourOptionPrefab;
		public ControlFlowArrow controlFlowArrowPrefab;
		public ParentingArrow parentingArrowPrefab;
		public Hand leftHand;
		public Hand rightHand;
		public TypingOptionsGroup[] typingOptionsGroups = new TypingOptionsGroup[0];
		public SerializableDictionary<Option, string> optionNamesDict = new SerializableDictionary<Option, string>();
#if USE_UNITY_EVENTS
		public List<UnityEvent> unityEvents = new List<UnityEvent>();
		public List<UnityEvent<Hand>> handUnityEvents = new List<UnityEvent<Hand>>();
#endif
#if USE_EVENTS
		public List<Option.Event> events = new List<Option.Event>();
		public List<Option.HandEvent> handEvents = new List<Option.HandEvent>();
#endif
		public static Instrument[] leftHandInstruments = new Instrument[0];
		public static Instrument[] rightHandInstruments = new Instrument[0];
		[SaveAndLoadValue]
		public FloatRange modulationDistanceRange;
		[SaveAndLoadValue]
		public float modulationSliderLength;
		[SaveAndLoadValue]
		public FloatRange modulationPositionAngleRange;
		[SaveAndLoadValue]
		public FloatRange modulationRotationAngleRange;
		public int minModulationPositionAngleIndicatorConeBasePointCount;
		public int maxModulationPositionAngleIndicatorConeBasePointCount;
		public Material minModulationPositionAngleIndicatorMaterial;
		public Material maxModulationPositionAngleIndicatorMaterial;
		public int minModulationRotationAngleIndicatorConeBasePointCount;
		public int maxModulationRotationAngleIndicatorConeBasePointCount;
		public Material minModulationRotationAngleIndicatorMaterial;
		public Material maxModulationRotationAngleIndicatorMaterial;
		[SaveAndLoadValue]
		public Modulator.ControlMethod modulationControlMethod;
		[SaveAndLoadValue]
		public Modulator.BehaviourWhenUncontrolled modulationBehaviourWhenUncontrolled;
		[SaveAndLoadValue]
		public ModulationCurve.KeyframeData modulationKeyframeDataTemplateWhenControlled;
		[SaveAndLoadValue]
		public ModulationCurve.KeyframeData potentialModulationKeyframeDataTemplateWhenUncontrolled;
		public BoolOption useSeparateTweakingDisplayersForModulationCurvesBoolOption;
		public const int VERSION_INDEX = 1;
		public const string SCRIPTING_DEFINE_SYMBOLS_SEPARATOR = ";";
		public const string USE_UNITY_EVENTS_SCRIPTING_DEFINE_SYMBOL = "USE_UNITY_EVENTS";
		public const string USE_EVENTS_SCRIPTING_DEFINE_SYMBOL = "USE_EVENTS";
		Dictionary<TypingTargetOption.Type, Option[]> typingOptionsDict = new Dictionary<TypingTargetOption.Type, Option[]>();

		public override void OnEnable ()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
			{
				optionNamesDict.Init ();
				leftHand.circularMenuEntriesDict.Init ();
				rightHand.circularMenuEntriesDict.Init ();
				return;
			}
#endif
			if (PlayerPrefs.GetInt("Version", 0) != VERSION_INDEX)
			{
				PlayerPrefs.DeleteAll();
				PlayerPrefs.SetInt("Version", VERSION_INDEX);
			}
			leftHandInstruments = new Instrument[2];
			leftHandInstruments[0] = leftHandCurveInstrument;
			leftHandInstruments[1] = leftHandTrailInstrument;
			rightHandInstruments = new Instrument[2];
			rightHandInstruments[0] = rightHandCurveInstrument;
			rightHandInstruments[1] = rightHandTrailInstrument;
			leftHand.circularMenuEntriesDict.Init ();
			rightHand.circularMenuEntriesDict.Init ();
			leftHand.HandleUpdateInstrument ();
			rightHand.HandleUpdateInstrument ();
			optionNamesDict.Init ();
			typingOptionsDict.Clear();
			for (int i = 0; i < typingOptionsGroups.Length; i ++)
			{
				TypingOptionsGroup typingOptionsGroup = typingOptionsGroups[i];
				typingOptionsDict.Add(typingOptionsGroup.type, typingOptionsGroup.options);
			}
			for (int i = 0; i < optionNamesDict.Count; i ++)
			{
				Option option = optionNamesDict.keys[i];
				option.gameObject.SetActive(option.gameObject.activeInHierarchy);
				option.trs.SetParent(sceneTrs);
				option.childOptionsParent.gameObject.SetActive(true);
			}
			base.OnEnable ();
		}

		public override void DoUpdate ()
		{
			leftHand.triggerInput = InputManager.LeftTriggerInput;
#if UNITY_EDITOR
			if (!leftHand.triggerInput)
				leftHand.triggerInput = Keyboard.current.leftAltKey.isPressed;
#endif
			leftHand.gripInput = InputManager.LeftGripInput;
			rightHand.triggerInput = InputManager.RightTriggerInput;
#if UNITY_EDITOR
			if (!rightHand.triggerInput)
				rightHand.triggerInput = Keyboard.current.rightAltKey.isPressed;
#endif
			rightHand.gripInput = InputManager.RightGripInput;
			leftHand.thumbstickClickedInput = InputManager.LeftThumbstickClickedInput;
			rightHand.thumbstickClickedInput = InputManager.RightThumbstickClickedInput;
			leftHand.primaryButtonInput = InputManager.LeftPrimaryButtonInput;
			rightHand.primaryButtonInput = InputManager.RightPrimaryButtonInput;
			leftHand.secondaryButtonInput = InputManager.LeftSecondaryButtonInput;
			rightHand.secondaryButtonInput = InputManager.RightSecondaryButtonInput;
			leftHand.Update ();
			rightHand.Update ();
			leftHand.previousTriggerInput = leftHand.triggerInput;
			leftHand.previousGripInput = leftHand.gripInput;
			leftHand.previousThumbstickClickedInput = leftHand.thumbstickClickedInput;
			rightHand.previousThumbstickClickedInput = rightHand.thumbstickClickedInput;
			rightHand.previousTriggerInput = rightHand.triggerInput;
			rightHand.previousGripInput = rightHand.gripInput;
			leftHand.previousPrimaryButtonInput = leftHand.primaryButtonInput;
			rightHand.previousPrimaryButtonInput = rightHand.primaryButtonInput;
			leftHand.previousSecondaryButtonInput = leftHand.secondaryButtonInput;
			rightHand.previousSecondaryButtonInput = rightHand.secondaryButtonInput;
			leftHand.previousSelectedOptions = new Option[leftHand.selectedOptions.Length];
			leftHand.selectedOptions.CopyTo(leftHand.previousSelectedOptions, 0);
			rightHand.previousSelectedOptions = new Option[rightHand.selectedOptions.Length];
			rightHand.selectedOptions.CopyTo(rightHand.previousSelectedOptions, 0);
			leftHand.previousPosition = leftHand.trs.position;
			rightHand.previousPosition = rightHand.trs.position;
			for (int i = 0; i < Option.instances.Count; i ++)
			{
				Option option = Option.instances[i];
				Transform trsToLookAt = VRCameraRig.instance.eyesTrs;
#if UNITY_EDITOR
				if (ToggleOptionsFaceEditorCamera.optionsFaceEditorCamera)
				{
					SceneView sceneView = SceneView.currentDrawingSceneView;
					if (sceneView == null)
						sceneView = SceneView.lastActiveSceneView;
					trsToLookAt = sceneView.camera.GetComponent<Transform>();
				}
#endif
				option.uiTrs.LookAt(trsToLookAt);
				option.previousJustEnabled = option.justEnabled;
				option.justEnabled = false;
			}
		}

		public void SetInstrument (Hand hand, int instrumentIndex)
		{
			TrailInstrument trailInstrument = hand.currentInstrument as TrailInstrument;
			if (trailInstrument != null && trailInstrument.temporaryLineRenderer != null)
				DestroyImmediate(trailInstrument.temporaryLineRenderer.gameObject);
			hand.currentInstrumentIndex = instrumentIndex;
			hand.HandleUpdateInstrument ();
		}

		public void UpdateMostRecentInstrument (Hand hand)
		{
			if (hand.mostRecentInstrumentIndex != null)
			{
				if (hand.isLeftHand)
					leftHandInstruments[(int) hand.mostRecentInstrumentIndex] = hand.currentInstrument;
				else
					rightHandInstruments[(int) hand.mostRecentInstrumentIndex] = hand.currentInstrument;
			}
		}

		public void StartRecording ()
		{
			if (instance != this)
			{
				instance.StartRecording ();
				return;
			}
			StartCoroutine(RecordingRoutine ());
		}

		public void SaveRecording (Option fileNameOption)
		{
			if (instance != this)
			{
				instance.SaveRecording (fileNameOption);
				return;
			}
			string filePath = fileNameOption.GetValue();
			if (!filePath.ToLower().EndsWith(".wav"))
				filePath += ".wav";
			bool filePreviouslyExists = File.Exists(filePath);
			audioSource.clip = AudioUtilities.TrimSilence(audioSource.clip, 0);
			if (AudioUtilities.SaveWavFile(filePath, audioSource.clip))
			{
				if (filePreviouslyExists)
				{
					
					return;
				}
				AudioRecordingOption audioRecordingOption = ObjectPool.instance.SpawnComponent<AudioRecordingOption>(audioRecordingOptionPrefab.prefabIndex, currentSpawnOrientationOption.trs.position, currentSpawnOrientationOption.trs.rotation, sceneTrs);
				audioRecordingOption.trs.SetWorldScale (LogicModule.instance.currentSpawnOrientationOption.trs.lossyScale);
				audioRecordingOption.Init (filePath, audioSource.clip);
			}
		}

		public void StopRecording ()
		{
			if (instance != this)
			{
				instance.StopRecording ();
				return;
			}
			StopCoroutine(RecordingRoutine ());
			Microphone.End("");
		}

		public void SetTypingTargetOption (TypingTargetOption typingTargetOption)
		{
			if (instance != this)
			{
				instance.SetTypingTargetOption (typingTargetOption);
				return;
			}
			TypingTargetOption.currentActive = typingTargetOption;
			TypingTargetOption.typingCursorLocation = typingTargetOption.text.text.Length;
			if (typingTargetOption.type != TypingTargetOption.Type.String)
			{
				Option[] typingOptions = typingOptionsDict[TypingTargetOption.Type.String];
				for (int i = 0; i < typingOptions.Length; i ++)
				{
					Option option = typingOptions[i];
					option.SetActivatable (false);
				}
			}
			Option[] _typingOptions = typingOptionsDict[typingTargetOption.type];
			for (int i = 0; i < _typingOptions.Length; i ++)
			{
				Option option = _typingOptions[i];
				option.SetActivatable (true);
			}
		}
		
		public void AddTextToTypingTargetOption (string text)
		{
			if (TypingTargetOption.currentActive != null)
				TypingTargetOption.currentActive.AddText (text);
		}

		public void DeleteTextFromTypingTargetOption ()
		{
			if (TypingTargetOption.currentActive != null)
				TypingTargetOption.currentActive.DeleteText ();
		}

		public void BackspaceTextFromTypingTargetOption ()
		{
			if (TypingTargetOption.currentActive != null)
				TypingTargetOption.currentActive.BackspaceText ();
		}

		IEnumerator RecordingRoutine ()
		{
			yield return Application.RequestUserAuthorization(UserAuthorization.Microphone);
			if (Application.HasUserAuthorization(UserAuthorization.Microphone))
			{
				audioSource.clip = Microphone.Start("", true, recordingLength, recordingFrequency);
				audioSource.loop = true;
				while (Microphone.GetPosition("") <= 0)
				{
				}
				// audioSource.Play();
			}
			else
				print("Microphone not authorized for user");
		}

		public void DeleteFile (string filePath)
		{
			if (instance != this)
			{
				instance.DeleteFile (filePath);
				return;
			}
			File.Delete(filePath);
		}

		public void RenameChildren (Option nameOption)
		{
			if (instance != this)
			{
				instance.RenameChildren (nameOption);
				return;
			}
			string name = nameOption.GetValue();
			int nameOccuranceCount = 1;
			for (int i = 0; i < renameChildrenOption.children.Count; i ++)
			{
				Option option = renameChildrenOption.children[i];
				if (optionNamesDict.ContainsValue(name))
				{
					while (optionNamesDict.ContainsValue(name + " (" + nameOccuranceCount + ")"))
						nameOccuranceCount ++;
					name += " (" + nameOccuranceCount + ")";
				}
				optionNamesDict[option] = name;
				string nameAndValue = name;
				string value = option.GetValue();
				if (value != null)
					nameAndValue += option.nameToValueSeparator + value;
				option.text.text = nameAndValue;
			}
		}

		public void DuplicateChildren (Option option)
		{
			if (instance != this)
			{
				instance.DuplicateChildren (option);
				return;
			}
			for (int i = 0; i < option.children.Count; i ++)
			{
				Option childOption = option.children[i];
				for (int i2 = 0; i2 < childOption.childOptionsParent.childCount; i2 ++)
				{
					Transform child = childOption.childOptionsParent.GetChild(i2);
					child.SetParent(sceneTrs);
				}
				Instantiate(childOption, currentSpawnOrientationOption.trs.position, currentSpawnOrientationOption.trs.rotation, sceneTrs);
			}
		}

		public void HideOptionAndChildren (Option option)
		{
			if (instance != this)
			{
				instance.HideOptionAndChildren (option);
				return;
			}
			option.gameObject.SetActive(false);
			for (int i = 0; i < option.children.Count; i ++)
			{
				Option childOption = option.children[i];
				for (int i2 = 0; i2 < childOption.childOptionsParent.childCount; i2 ++)
				{
					Transform child = childOption.childOptionsParent.GetChild(i2);
					child.SetParent(sceneTrs);
				}
				childOption.gameObject.SetActive(false);
			}
		}

		public void ShowOptionAndChildren (Option option)
		{
			if (instance != this)
			{
				instance.ShowOptionAndChildren (option);
				return;
			}
			option.gameObject.SetActive(true);
			for (int i = 0; i < option.children.Count; i ++)
			{
				Option childOption = option.children[i];
				childOption.gameObject.SetActive(true);
			}
		}

		public void SaveProject (Option fileNameOption)
		{
			if (instance != this)
			{
				instance.SaveProject (fileNameOption);
				return;
			}
			SaveAndLoadManager.instance.Save (fileNameOption.GetValue());
		}

		public void LoadProject (Option fileNameOption)
		{
			if (instance != this)
			{
				instance.LoadProject (fileNameOption);
				return;
			}
			SaveAndLoadManager.instance.Load (fileNameOption.GetValue());
		}

		public void NewProject ()
		{
			if (instance != this)
			{
				instance.NewProject ();
				return;
			}
			SaveAndLoadManager.MostRecentSaveFileName = null;
			_SceneManager.instance.RestartSceneWithoutTransition ();
		}

		public void DespawnChildren (Option option)
		{
			for (int i = 0; i < option.children.Count; i ++)
			{
				Option childOption = option.children[i];
				for (int i2 = 0; i2 < childOption.childOptionsParent.childCount; i2 ++)
				{
					Transform child = childOption.childOptionsParent.GetChild(i2);
					child.SetParent(sceneTrs);
				}
				ObjectPool.Instance.Despawn (childOption.prefabIndex, childOption.gameObject, childOption.trs);
				// DestroyImmediate(childOption.gameObject);
			}
		}

		public void ToggleChildren (Option option)
		{
			for (int i = 0; i < option.children.Count; i ++)
			{
				Option childOption = option.children[i];
				for (int i2 = 0; i2 < childOption.childOptionsParent.childCount; i2 ++)
				{
					Transform child = childOption.childOptionsParent.GetChild(i2);
					child.SetParent(sceneTrs);
				}
				childOption.gameObject.SetActive(!childOption.gameObject.activeSelf);
			}
		}

		public void SetModulationControlMethod (int controlMethodIndex)
		{
			modulationControlMethod = (Modulator.ControlMethod) Enum.ToObject(typeof(Modulator.ControlMethod), controlMethodIndex);
		}

		public void SetModulationBehaviourWhenUncontrolled (int behaviourWhenUncontrolledIndex)
		{
			modulationBehaviourWhenUncontrolled = (Modulator.BehaviourWhenUncontrolled) Enum.ToObject(typeof(Modulator.BehaviourWhenUncontrolled), behaviourWhenUncontrolledIndex);
		}

		public void StartModulation (Hand hand)
		{
			ModulationOption modulationOption = ObjectPool.instance.SpawnComponent<ModulationOption>(modulationOptionPrefab.prefabIndex, currentSpawnOrientationOption.trs.position, currentSpawnOrientationOption.trs.rotation, sceneTrs);
			modulationOption.trs.SetWorldScale (LogicModule.instance.currentSpawnOrientationOption.trs.lossyScale);
			string modulationName = modulationNameOption.GetValue();
			modulationOption.Init (modulationControlMethod, modulationBehaviourWhenUncontrolled, modulationKeyframeDataTemplateWhenControlled, potentialModulationKeyframeDataTemplateWhenUncontrolled, hand.isLeftHand, modulationName);
		}

		public void EndModulation (Hand hand)
		{
			ModulationOption modulationOption = ObjectPool.instance.SpawnComponent<ModulationOption>(modulationOptionPrefab.prefabIndex, currentSpawnOrientationOption.trs.position, currentSpawnOrientationOption.trs.rotation, sceneTrs);
			modulationOption.trs.SetWorldScale (currentSpawnOrientationOption.trs.lossyScale);
			string modulationName = modulationNameOption.GetValue();
			modulationOption.Init (modulationControlMethod, modulationBehaviourWhenUncontrolled, modulationKeyframeDataTemplateWhenControlled, potentialModulationKeyframeDataTemplateWhenUncontrolled, hand.isLeftHand, modulationName);
		}

		public void NewCodeCommand ()
		{
			CodeCommandOption codeCommandOption = ObjectPool.instance.SpawnComponent<CodeCommandOption>(codeCommandOptionPrefab.prefabIndex, currentSpawnOrientationOption.trs.position, currentSpawnOrientationOption.trs.rotation, sceneTrs);
			codeCommandOption.trs.SetWorldScale (currentSpawnOrientationOption.trs.lossyScale);
		}

		public void NewAutomationSequence ()
		{
			AutomationOption automationOption = ObjectPool.instance.SpawnComponent<AutomationOption>(automationOptionPrefab.prefabIndex, currentSpawnOrientationOption.trs.position, currentSpawnOrientationOption.trs.rotation, sceneTrs);
			automationOption.trs.SetWorldScale (currentSpawnOrientationOption.trs.lossyScale);
			string sequenceName = automationSequenceNameOption.GetValue();
			automationOption.Init (sequenceName);
		}

		public void ImportModelFileAsyncWithFileBrowser ()
		{
			LoadModelFileAsyncWithFileBrowser (new GameObject(), OnModelLoaded, OnModelMaterialsLoaded, OnModelLoadProgressed, onError:OnModelLoadError);
		}

		public void LoadModelFileAsyncWithFileBrowser (GameObject loadIntoGo, Action<AssetLoaderContext> onLoad = null, Action<AssetLoaderContext> onMaterialsLoad = null, Action<AssetLoaderContext, float> onProgress = null, Action<bool> onBeginLoad = null, Action<IContextualizedError> onError = null, AssetLoaderOptions assetLoaderOptions = null)
		{
			AssetLoaderFilePicker filePickerAssetLoader = AssetLoaderFilePicker.Create();
			filePickerAssetLoader.LoadModelFromFilePickerAsync("Select Model File", onLoad, onMaterialsLoad, onProgress, onBeginLoad, onError, loadIntoGo, assetLoaderOptions);
		}

		public void LoadModelAtURL (UnityWebRequest request, string fileExtension, GameObject loadIntoGo, Action<AssetLoaderContext> onLoad = null, Action<AssetLoaderContext> onMaterialsLoad = null, Action<AssetLoaderContext, float> onProgress = null, Action<IContextualizedError> onError = null, AssetLoaderOptions assetLoaderOptions = null, object customData = null)
		{
			fileExtension = fileExtension.ToLowerInvariant();
			bool isZipFile = fileExtension == "zip" || fileExtension == ".zip";
			AssetDownloader.LoadModelFromUri(request, onLoad, onMaterialsLoad, onProgress, onError, loadIntoGo, assetLoaderOptions, customData, isZipFile ? null : fileExtension, isZipFile);
		}

		public void ImportAudioFromFile (Option fileNameOption)
		{
			string fileName = fileNameOption.GetValue();
			AudioClip audioClip = AudioUtilities.GetAudioClipFromWavFile(fileName);
			AudioRecordingOption audioRecordingOption = ObjectPool.instance.SpawnComponent<AudioRecordingOption>(audioRecordingOptionPrefab.prefabIndex, currentSpawnOrientationOption.trs.position, currentSpawnOrientationOption.trs.rotation, sceneTrs);
			audioRecordingOption.trs.SetWorldScale (LogicModule.instance.currentSpawnOrientationOption.trs.lossyScale);
			audioRecordingOption.Init (fileName, audioClip);
		}

		public void Undo ()
		{
			throw new NotImplementedException();
		}

		public void Redo ()
		{
			throw new NotImplementedException();
		}

		void OnModelLoadError (IContextualizedError obj)
		{
		}

		void OnModelLoadProgressed (AssetLoaderContext assetLoaderContext, float progress)
		{
		}

		void OnModelMaterialsLoaded (AssetLoaderContext assetLoaderContext)
		{
		}

		void OnModelLoaded (AssetLoaderContext assetLoaderContext)
		{
		}

#if UNITY_EDITOR
		[MenuItem("Game/Reset optionNamesDict, readd all Option events, apply prefab changes, and clear data")]
		static void UpdateValuesAndClearData ()
		{
			ResetOptionNamesDict ();
			ReaddAllOptionEvents._Do ();
			Option[] options = FindObjectsOfType<Option>(true);
			for (int i = 0; i < options.Length; i ++)
			{
				Option option = options[i];
				if (option.trs.parent == null)
				{
					option.gameObject.SetActive(true);
					PrefabUtility.ApplyPrefabInstance(option.gameObject, InteractionMode.UserAction);
					option.gameObject.SetActive(false);
				}
			}
			PlayerPrefs.DeleteAll();
		}

		[MenuItem("Game/Reset optionNamesDict")]
		static void ResetOptionNamesDict ()
		{
			instance.optionNamesDict.Clear ();
			Option[] options = FindObjectsOfType<Option>(true);
			for (int i = 0; i < options.Length; i ++)
			{
				Option option = options[i];
				// option.text.text = option.text.text.Replace(":\n:\n", ":\n");
				// option.text.text = option.text.text.RemoveEach("22");
				option.HandleNaming ();
			}
			instance.enabled = !instance.enabled;
			instance.enabled = !instance.enabled;
		}

#if USE_UNITY_EVENTS
		[MenuItem("Game/Use Events")]
		static void UseEvents ()
		{
			BuildTargetGroup buildTargetGroup = BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget);
			string scriptingDefineSymbolsString = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);
			List<string> scriptingDefineSymbols = new List<string>(scriptingDefineSymbolsString.Split(new string[1] { SCRIPTING_DEFINE_SYMBOLS_SEPARATOR }, StringSplitOptions.None));
			if (!scriptingDefineSymbols.Contains(USE_EVENTS_SCRIPTING_DEFINE_SYMBOL))
				scriptingDefineSymbols.Add(USE_EVENTS_SCRIPTING_DEFINE_SYMBOL);
			scriptingDefineSymbols.Remove(USE_UNITY_EVENTS_SCRIPTING_DEFINE_SYMBOL);
			PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, scriptingDefineSymbols.ToArray());
		}
#endif

#if USE_EVENTS
		[MenuItem("Game/Use UnityEvents")]
		static void UseUnityEvents ()
		{
			BuildTargetGroup buildTargetGroup = BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget);
			string scriptingDefineSymbolsString = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);
			List<string> scriptingDefineSymbols = new List<string>(scriptingDefineSymbolsString.Split(new string[1] { SCRIPTING_DEFINE_SYMBOLS_SEPARATOR }, StringSplitOptions.None));
			if (!scriptingDefineSymbols.Contains(USE_UNITY_EVENTS_SCRIPTING_DEFINE_SYMBOL))
				scriptingDefineSymbols.Add(USE_UNITY_EVENTS_SCRIPTING_DEFINE_SYMBOL);
			scriptingDefineSymbols.Remove(USE_EVENTS_SCRIPTING_DEFINE_SYMBOL);
			PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, scriptingDefineSymbols.ToArray());
		}
#endif

#if USE_UNITY_EVENTS && USE_EVENTS
		[MenuItem("Game/Set UnityEvents to Events")]
		static void SetUnityEventsToEvents ()
		{
			Option[] options = SelectionExtensions.GetSelected<Option>();
			for (int i = 0; i < options.Length; i ++)
			{
				Option option = options[i];
				Option[] optionAndAllChildren = Option.GetAllChildrenAndSelf(option);
				for (int i2 = 0; i2 < optionAndAllChildren.Length; i2 ++)
				{
					Option option2 = optionAndAllChildren[i2];
					option2.onStartActivateUnityEvent = option2.onStartActivate.unityEvent;
					option2.onEndActivateUnityEvent = option2.onEndActivate.unityEvent;
					BoolOption boolOption = option2 as BoolOption;
					if (boolOption != null)
						boolOption.onValueChangedUnityEvent = boolOption.onValueChanged.unityEvent;
					else
					{
						EnumOption enumOption = option2 as EnumOption;
						if (enumOption != null)
							enumOption.onValueChangedUnityEvent = enumOption.onValueChanged.unityEvent;
						else
						{
							TypingTargetOption typingTargetOption = option2 as TypingTargetOption;
							if (typingTargetOption != null)
								typingTargetOption.onValueChangedUnityEvent = typingTargetOption.onValueChanged.unityEvent;
						}
					}
				}
			}
		}
		
		[MenuItem("Game/Set Events to UnityEvents")]
		static void SetEventsToUnityEvents ()
		{
			Option[] options = SelectionExtensions.GetSelected<Option>();
			for (int i = 0; i < options.Length; i ++)
			{
				Option option = options[i];
				Option[] optionAndAllChildren = Option.GetAllChildrenAndSelf(option);
				for (int i2 = 0; i2 < optionAndAllChildren.Length; i2 ++)
				{
					Option option2 = optionAndAllChildren[i2];
					option2.onStartActivate.unityEvent = option2.onStartActivateUnityEvent;
					option2.onEndActivate.unityEvent = option2.onEndActivateUnityEvent;
					BoolOption boolOption = option2 as BoolOption;
					if (boolOption != null)
						boolOption.onValueChanged.unityEvent = boolOption.onValueChangedUnityEvent;
					else
					{
						EnumOption enumOption = option2 as EnumOption;
						if (enumOption != null)
							enumOption.onValueChanged.unityEvent = enumOption.onValueChangedUnityEvent;
						else
						{
							TypingTargetOption typingTargetOption = option2 as TypingTargetOption;
							if (typingTargetOption != null)
								typingTargetOption.onValueChanged.unityEvent = typingTargetOption.onValueChangedUnityEvent;
						}
					}
				}
			}
		}
#else
		[MenuItem("Game/Use UnityEvents and Events")]
		static void UseUnityEventsAndEvents ()
		{
			BuildTargetGroup buildTargetGroup = BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget);
			string scriptingDefineSymbolsString = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);
			List<string> scriptingDefineSymbols = new List<string>(scriptingDefineSymbolsString.Split(new string[1] { SCRIPTING_DEFINE_SYMBOLS_SEPARATOR }, StringSplitOptions.None));
			if (!scriptingDefineSymbols.Contains(USE_UNITY_EVENTS_SCRIPTING_DEFINE_SYMBOL))
				scriptingDefineSymbols.Add(USE_UNITY_EVENTS_SCRIPTING_DEFINE_SYMBOL);
			if (!scriptingDefineSymbols.Contains(USE_EVENTS_SCRIPTING_DEFINE_SYMBOL))
				scriptingDefineSymbols.Add(USE_EVENTS_SCRIPTING_DEFINE_SYMBOL);
			PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, scriptingDefineSymbols.ToArray());
		}
#endif
#endif

		[Serializable]
		public struct TypingOptionsGroup
		{
			public TypingTargetOption.Type type;
			public Option[] options;
		}

		[Serializable]
		public class Hand : VRCameraRig.Hand, ISaveableAndLoadable
		{
			[SaveAndLoadValue]
			public int currentInstrumentIndex;
			public int? mostRecentInstrumentIndex;
			public Instrument currentInstrument;
			public Option[] selectedOptions;
			public Option[] previousSelectedOptions;
			public Modulator modulator;
			public OptionConnectionArrow optionConnectionArrow;
			public bool optionConnectionArrowIsParentingArrow;
			public Mode mode;
			public SerializableDictionary<Mode, CircularMenuEntry> circularMenuEntriesDict = new SerializableDictionary<Mode, CircularMenuEntry>();
			public CircularMenu currentCircularMenu;
			public static Option grabbingOption;
			public static Option[] grabbingOptionAndAllChildren = new Option[0];
			static Transform grabbingTrs;

			public void Update ()
			{
				currentInstrument.UpdateGraphics (this);
				HandleSelectOptions ();
				HandleActivateOptions ();
				HandleOptionsCollision ();
				HandleGrabTransforms ();
				HandleParentingOptions ();
				HandleModeSettings ();
			}

			void HandleSelectOptions ()
			{
				selectedOptions = currentInstrument.GetSelectedOptions(this);
				for (int i = 0; i < selectedOptions.Length; i ++)
				{
					Option option = selectedOptions[i];
					option.renderer.material.color = option.selectedColorOffset.ApplyWithTransparency(option.renderer.material.color);
				}
				for (int i = 0; i < previousSelectedOptions.Length; i ++)
				{
					Option option = previousSelectedOptions[i];
					if (option != null)
						option.renderer.material.color = option.selectedColorOffset.ApplyInverseWithTransparency(option.renderer.material.color);
				}
			}

			void HandleActivateOptions ()
			{
				if (triggerInput)
				{
					for (int i = 0; i < previousSelectedOptions.Length; i ++)
					{
						Option option = previousSelectedOptions[i];
						if (option.isActivated && !selectedOptions.Contains(option))
							option.EndActivate (this);
					}
					for (int i = 0; i < selectedOptions.Length; i ++)
					{
						Option option = selectedOptions[i];
						if (option.activatable && !option.justEnabled && !option.previousJustEnabled && (!previousTriggerInput || !previousSelectedOptions.Contains(option)))
							option.StartActivate (this);
					}
				}
				else
				{
					for (int i = 0; i < previousSelectedOptions.Length; i ++)
					{
						Option option = previousSelectedOptions[i];
						if (option.isActivated)
							option.EndActivate (this);
					}
					for (int i = 0; i < selectedOptions.Length; i ++)
					{
						Option option = selectedOptions[i];
						if (option.isActivated)
							option.EndActivate (this);
					}
				}
			}

			void HandleOptionsCollision ()
			{
				if (secondaryButtonInput)
				{
					for (int i = 0; i < selectedOptions.Length; i ++)
					{
						Option option = selectedOptions[i];
						if (!previousSecondaryButtonInput || !previousSelectedOptions.Contains(option))
							option.SetCollidable (this, !option.collidable);
					}
				}
			}

			void HandleGrabTransforms ()
			{
				Hand leftHand = LogicModule.instance.leftHand;
				Hand rightHand = LogicModule.instance.rightHand;
				if (grabbingTrs == null && grabbingOption == null && gripInput && selectedOptions.Length > 0)
					grabbingOption = selectedOptions[0];
				if (grabbingOption != null)
					grabbingTrs = grabbingOption.trs;
				else if (gripInput)
					grabbingTrs = LogicModule.instance.sceneTrs;
				bool isGrabbingWithLeft = leftHand.gripInput;
				bool isGrabbingWithRight = rightHand.gripInput;
				if (gripInput)
				{
					if (grabbingOption != null)
					{
						grabbingOption.rigid.isKinematic = true;
						if (!previousGripInput)
							UpdateGrabbingOptionAndAllChildren ();
						for (int i = 0; i < grabbingOptionAndAllChildren.Length; i ++)
						{
							Option option = grabbingOptionAndAllChildren[i];
							option.trs.SetParent(grabbingOption.childOptionsParent);
							for (int i2 = 0; i2 < option.optionConnectionArrows.Count; i2 ++)
							{
								OptionConnectionArrow optionConnectionArrow = option.optionConnectionArrows[i2];
								optionConnectionArrow.DoUpdate ();
							}
						}
					}
					if (isGrabbingWithLeft && !isGrabbingWithRight)
						grabbingTrs.SetParent(leftHand.trs);
					else if (!isGrabbingWithLeft && isGrabbingWithRight)
						grabbingTrs.SetParent(rightHand.trs);
					else if (isGrabbingWithLeft && isGrabbingWithRight)
					{
						grabbingTrs.SetParent(VRCameraRig.instance.bothHandsAverageTrs);
						for (int i = 0; i < Option.instances.Count; i ++)
						{
							Option option = Option.instances[i];
							for (int i2 = 0; i2 < option.children.Count; i2 ++)
							{
								Option childOption = option.children[i2];
								for (int i3 = 0; i3 < childOption.optionConnectionArrows.Count; i3 ++)
								{
									OptionConnectionArrow optionConnectionArrow = childOption.optionConnectionArrows[i3];
									optionConnectionArrow.DoUpdate ();
								}
							}
						}
					}
				}
				else if (grabbingTrs != null && ((!isGrabbingWithLeft && !isGrabbingWithRight) || previousGripInput))
				{
					if (isLeftHand)
					{
						if (isGrabbingWithRight)
							grabbingTrs.SetParent(rightHand.trs);
						else
							DropGrabbedTransform ();
					}
					else
					{
						if (isGrabbingWithLeft)
							grabbingTrs.SetParent(leftHand.trs);
						else
							DropGrabbedTransform ();
					}
				}
			}

			public static void UpdateGrabbingOptionAndAllChildren ()
			{
				for (int i = 0; i < grabbingOptionAndAllChildren.Length; i ++)
				{
					Option option = grabbingOptionAndAllChildren[i];
					option.onAddChild -= (Option child) => { UpdateGrabbingOptionAndAllChildren (); };
					option.onRemoveChild -= (Option child) => { UpdateGrabbingOptionAndAllChildren (); };
				}
				grabbingOptionAndAllChildren = Option.GetAllChildrenAndSelf(grabbingOption);
				for (int i = 0; i < grabbingOptionAndAllChildren.Length; i ++)
				{
					Option option = grabbingOptionAndAllChildren[i];
					option.onAddChild += (Option child) => { UpdateGrabbingOptionAndAllChildren (); };
					option.onRemoveChild += (Option child) => { UpdateGrabbingOptionAndAllChildren (); };
				}
			}

			public static void DropGrabbedTransform ()
			{
				if (grabbingOption != null)
				{
					grabbingTrs.SetParent(LogicModule.instance.sceneTrs);
					grabbingOption.rigid.isKinematic = false;
				}
				else
					grabbingTrs.SetParent(null);
				grabbingOption = null;
				grabbingTrs = null;
				for (int i = 0; i < grabbingOptionAndAllChildren.Length; i ++)
				{
					Option option = grabbingOptionAndAllChildren[i];
					option.onAddChild -= (Option child) => { UpdateGrabbingOptionAndAllChildren (); };
					option.onRemoveChild -= (Option child) => { UpdateGrabbingOptionAndAllChildren (); };
				}
				grabbingOptionAndAllChildren = new Option[0];
			}

			void HandleModeSettings ()
			{
				if (thumbstickInput.sqrMagnitude > InputManager.instance.settings.defaultDeadzoneMin * InputManager.instance.settings.defaultDeadzoneMin)
				{
					if (grabbingOption != null)
						SetMode (Mode.GrabbingOption);
					else if (modulator.gameObject.activeSelf)
						SetMode (Mode.Modulating);
					else
						SetMode (Mode.Default);
					currentCircularMenu.DoUpdate (this);
				}
				else
					currentCircularMenu.gameObject.SetActive(false);
			}

			void HandleParentingOptions ()
			{
				if (thumbstickClickedInput)
				{
					if (!previousThumbstickClickedInput && selectedOptions.Length > 0)
					{
						Option option = selectedOptions[0];
						if (option != null)
						{
							if (optionConnectionArrowIsParentingArrow)
								optionConnectionArrow = ObjectPool.instance.SpawnComponent<ParentingArrow>(LogicModule.instance.parentingArrowPrefab.prefabIndex, parent:option.trs);
							else
								optionConnectionArrow = ObjectPool.instance.SpawnComponent<ControlFlowArrow>(LogicModule.instance.controlFlowArrowPrefab.prefabIndex, parent:option.trs);
							optionConnectionArrow.parent = option;
							UpdateOptionConnectionArrow ();
						}
					}
					else if (optionConnectionArrow != null)
						UpdateOptionConnectionArrow ();
				}
				else if (previousThumbstickClickedInput && optionConnectionArrow != null)
				{
					UpdateOptionConnectionArrow ();
					if (optionConnectionArrow.child == optionConnectionArrow.parent || optionConnectionArrow.child == null)
						ObjectPool.instance.Despawn (optionConnectionArrow.prefabIndex, optionConnectionArrow.gameObject, optionConnectionArrow.trs);
					else if (optionConnectionArrowIsParentingArrow)
					{
						if (optionConnectionArrow.parent.children.Contains(optionConnectionArrow.child))
						{
							optionConnectionArrow.parent.RemoveChild (optionConnectionArrow.child);
							ObjectPool.instance.Despawn (optionConnectionArrow.prefabIndex, optionConnectionArrow.gameObject, optionConnectionArrow.trs);
						}
						else
						{
							optionConnectionArrow.parent.AddChild (optionConnectionArrow.child);
							optionConnectionArrow.child.optionConnectionArrows.Add(optionConnectionArrow);
							optionConnectionArrow.trs.SetParent(optionConnectionArrow.pointsTo);
						}
					}
					else
					{
						bool deleteControlFlowArrow = false;
						for (int i = 0; i < optionConnectionArrow.parent.controlFlowArrows.Count; i ++)
						{
							ControlFlowArrow controlFlowArrow = optionConnectionArrow.parent.controlFlowArrows[i];
							if (controlFlowArrow.parent == optionConnectionArrow.parent && controlFlowArrow.child == optionConnectionArrow.child)
							{
								deleteControlFlowArrow = true;
								break;
							}
						}
						if (deleteControlFlowArrow)
						{
							ObjectPool.instance.Despawn (optionConnectionArrow.prefabIndex, optionConnectionArrow.gameObject, optionConnectionArrow.trs);
							optionConnectionArrow.parent.optionConnectionArrows.Remove(optionConnectionArrow);
							optionConnectionArrow.parent.controlFlowArrows.Remove(optionConnectionArrow as ControlFlowArrow);
						}
					}
					optionConnectionArrow = null;
				}
			}

			void UpdateOptionConnectionArrow ()
			{
				if (selectedOptions.Length > 0)
				{
					Option option = selectedOptions[0];
					optionConnectionArrow.pointsTo = option.trs;
					optionConnectionArrow.child = option;
				}
				else
				{
					optionConnectionArrow.pointsTo = trs;
					optionConnectionArrow.child = null;
				}
				optionConnectionArrow.DoUpdate ();
			}

			public void HandleUpdateInstrument ()
			{
				if (currentInstrumentIndex != mostRecentInstrumentIndex)
				{
					currentInstrument.trs.gameObject.SetActive(false);
					LogicModule.instance.UpdateMostRecentInstrument (this);
					if (isLeftHand)
						currentInstrument = LogicModule.leftHandInstruments[currentInstrumentIndex];
					else
						currentInstrument = LogicModule.rightHandInstruments[currentInstrumentIndex];
					currentInstrument.trs.gameObject.SetActive(true);
					mostRecentInstrumentIndex = currentInstrumentIndex;
				}
			}

			void SetMode (Mode mode)
			{
				currentCircularMenu.gameObject.SetActive(false);
				this.mode = mode;
				currentCircularMenu = circularMenuEntriesDict[mode].circularMenu;
				currentCircularMenu.gameObject.SetActive(true);
			}

			[Serializable]
			public struct CircularMenuEntry
			{
				public CircularMenu circularMenu;
				public Mode mode;
			}

			public enum Mode
			{
				GrabbingOption,
				Modulating,
				Default
			}

			public enum Type
			{
				Left,
				Right,
				Invalid
			}
		}
	}
}