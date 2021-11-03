using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using Extensions;
using FullSerializer;
using System;
using System.IO;
using EternityEngine;
using Random = UnityEngine.Random;

[ExecuteInEditMode]
public class SaveAndLoadManager : SingletonMonoBehaviour<SaveAndLoadManager>
{
	public List<SaveAndLoadObject> saveAndLoadObjects = new List<SaveAndLoadObject>();
	public TemporaryActiveText displayOnSave;
	// public List<string> assetNames = new List<string>();
	public static fsSerializer serializer = new fsSerializer();
	public static SaveEntry[] saveEntries = new SaveEntry[0];
	public static string MostRecentSaveFileName
	{
		get
		{
			return PlayerPrefs.GetString("Most recent save file name", null);
		}
		set
		{
			PlayerPrefs.SetString("Most recent save file name", value);
		}
	}
	// public static Dictionary<string, SaveAndLoadObject> saveAndLoadObjectTypeDict = new Dictionary<string, SaveAndLoadObject>();
	// [HideInInspector]

#if UNITY_EDITOR
	public void OnEnable ()
	{
		if (Application.isPlaying)
			return;
		// saveAndLoadObjects.Clear();
		// saveAndLoadObjects.AddRange(FindObjectsOfType<SaveAndLoadObject>());
		List<int> uniqueIds = new List<int>();
		for (int i = 0; i < saveAndLoadObjects.Count; i ++)
		{
			SaveAndLoadObject saveAndLoadObject = saveAndLoadObjects[i];
			while (uniqueIds.Contains(saveAndLoadObject.uniqueId))
				saveAndLoadObject.uniqueId ++;
			uniqueIds.Add(saveAndLoadObject.uniqueId);
		}
	}
#endif

	public override void Awake ()
	{
#if UNITY_EDITOR
		if (!Application.isPlaying)
			return;
#endif
		for (int i = 0; i < saveAndLoadObjects.Count; i ++)
		{
			SaveAndLoadObject saveAndLoadObject = saveAndLoadObjects[i];
			saveAndLoadObject.Init ();
		}
		base.Awake ();
	}
	
	public void Start ()
	{
#if UNITY_EDITOR
		if (!Application.isPlaying)
			return;
#endif
		// saveAndLoadObjectTypeDict.Clear();
		if (!string.IsNullOrEmpty(MostRecentSaveFileName))
			LoadMostRecent ();
	}

	void OnAboutToSave ()
	{
		// LogicModule.instance.UpdateMostRecentInstrument (LogicModule.instance.leftHand);
		// LogicModule.instance.UpdateMostRecentInstrument (LogicModule.instance.rightHand);
		Asset[] assets = FindObjectsOfType<Asset>(true);
		GameManager.instance.assetsData.Clear();
		for (int i = 0; i < assets.Length; i ++)
		{
			Asset asset = assets[i];
			asset.SetData ();
			// if (!assetNames.Contains(asset._Data.name))
			// 	print(asset._Data.name);
			GameManager.instance.assetsData.Add(asset._Data);
		}
		// GameManager.instance.saveAndLoadObject.AddSaveableAndLoadable (LogicModule.instance.leftHandCurveInstrument);
		// GameManager.instance.saveAndLoadObject.AddSaveableAndLoadable (LogicModule.instance.leftHandTrailInstrument);
		// GameManager.instance.saveAndLoadObject.AddSaveableAndLoadable (LogicModule.instance.rightHandCurveInstrument);
		// GameManager.instance.saveAndLoadObject.AddSaveableAndLoadable (LogicModule.instance.rightHandCurveInstrument);
		// GameManager.instance.saveAndLoadObject.AddSaveableAndLoadable (LogicModule.instance.leftHand);
		// GameManager.instance.saveAndLoadObject.AddSaveableAndLoadable (LogicModule.instance.rightHand);
	}
	
	public void Save (string fileName)
	{
		if (instance != this)
		{
			instance.Save (fileName);
			return;
		}
		OnAboutToSave ();
		List<string> fileLines = new List<string>();
		for (int i = 0; i < saveEntries.Length; i ++)
		{
			SaveEntry saveEntry = saveEntries[i];
			fileLines.AddRange(saveEntry.GetData());
		}
		File.WriteAllLines(fileName, fileLines.ToArray());
		if (displayOnSave.go != null)
			StartCoroutine(displayOnSave.DoRoutine ());
		MostRecentSaveFileName = fileName;
	}

	void OnAboutToLoad ()
	{
		// for (int i = 0; i < LogicModule.instance.sceneTrs.childCount; i ++)
		// {
		// 	Transform trs = LogicModule.instance.sceneTrs.GetChild(i);
		// 	DestroyImmediate(trs.gameObject);
		// 	i --;
		// }
		// LogicModule.instance.optionNamesDict.Clear();

		// GameManager.instance.saveAndLoadObject.AddSaveableAndLoadable (LogicModule.instance.leftHandCurveInstrument);
		// GameManager.instance.saveAndLoadObject.AddSaveableAndLoadable (LogicModule.instance.leftHandTrailInstrument);
		// GameManager.instance.saveAndLoadObject.AddSaveableAndLoadable (LogicModule.instance.rightHandCurveInstrument);
		// GameManager.instance.saveAndLoadObject.AddSaveableAndLoadable (LogicModule.instance.rightHandCurveInstrument);
		// GameManager.instance.saveAndLoadObject.AddSaveableAndLoadable (LogicModule.instance.leftHand);
		// GameManager.instance.saveAndLoadObject.AddSaveableAndLoadable (LogicModule.instance.rightHand);
	}
	
	public void Load (string fileName)
	{
		if (instance != this)
		{
			instance.Load (fileName);
			return;
		}
		OnAboutToLoad ();
		string[] allFileLines = File.ReadAllLines(fileName);
		int currentFileLineIndex = 0;
		for (int i = 0; i < saveEntries.Length; i ++)
		{
			SaveEntry saveEntry = saveEntries[i];
			string[] fileLines = new string[saveEntry.properties.Length + saveEntry.fields.Length];
			for (int i2 = 0; i2 < fileLines.Length; i2 ++)
				fileLines[i2] = allFileLines[i2 + currentFileLineIndex];
			currentFileLineIndex += fileLines.Length;
			saveEntry.Load (fileLines);
		}
		OnLoaded ();
		MostRecentSaveFileName = fileName;
	}

	void OnLoaded ()
	{
		for (int i = 0; i < GameManager.instance.assetsData.Count; i ++)
		{
			Asset.Data assetData = GameManager.instance.assetsData[i];
			// assetNames.Add(assetData.name);
			Option correspondingOption = null;
			for (int i2 = 0; i2 < LogicModule.instance.optionNamesDict.Count; i2 ++)
			{
				Option option = LogicModule.instance.optionNamesDict.keys[i2];
				if (assetData.name == option.name)
				{
					correspondingOption = option;
					break;
				}
			}
			if (correspondingOption == null)
				assetData.MakeAsset ();
			else
				assetData.Apply (correspondingOption);
			LogicModule.instance.leftHand.HandleUpdateInstrument ();
			LogicModule.instance.rightHand.HandleUpdateInstrument ();
		}
	}
	
	public void LoadMostRecent ()
	{
		Load (MostRecentSaveFileName);
	}

	public static string Serialize (object value, Type type)
	{
		fsData data;
		serializer.TrySerialize(type, value, out data).AssertSuccessWithoutWarnings();
		return fsJsonPrinter.CompressedJson(data);
	}
	
	public static object Deserialize (string serializedState, Type type)
	{
		fsData data = fsJsonParser.Parse(serializedState);
		object deserialized = null;
		serializer.TryDeserialize(data, type, ref deserialized).AssertSuccessWithoutWarnings();
		return deserialized;
	}
	
	public class SaveEntry
	{
		public SaveAndLoadObject saveableAndLoadObject;
		public ISaveableAndLoadable saveableAndLoadable;
		public PropertyInfo[] properties = new PropertyInfo[0];
		public FieldInfo[] fields = new FieldInfo[0];
		
		public string[] GetData ()
		{
			string[] data = new string[properties.Length + fields.Length];
			for (int i = 0; i < properties.Length; i ++)
			{
				PropertyInfo property = properties[i];
				data[i] = Serialize(property.GetValue(saveableAndLoadable, null), property.PropertyType);
			}
			for (int i = 0; i < fields.Length; i ++)
			{
				FieldInfo field = fields[i];
				data[i + properties.Length] = Serialize(field.GetValue(saveableAndLoadable), field.FieldType);
			}
			return data;
		}
		
		public void Load (string[] fileLines)
		{
			object value;
			for (int i = 0; i < properties.Length; i ++)
			{
				PropertyInfo property = properties[i];
				value = Deserialize(fileLines[i], property.PropertyType);
				property.SetValue(saveableAndLoadable, value, null);
			}
			for (int i = 0; i < fields.Length; i ++)
			{
				FieldInfo field = fields[i];
				value = Deserialize(fileLines[i + properties.Length], field.FieldType);
				field.SetValue(saveableAndLoadable, value);
			}
		}
	}
}
