using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Reflection;
using System;
using SaveEntry = SaveAndLoadManager.SaveEntry;
using Extensions;

public class SaveAndLoadObject : MonoBehaviour
{
	public int uniqueId;
	public ISaveableAndLoadable[] saveables = new ISaveableAndLoadable[0];
	// public string typeId;
	public SaveEntry[] saveEntries = new SaveEntry[0];
	
	public virtual void Init ()
	{
		ISaveableAndLoadable[] _saveables = GetComponentsInChildren<ISaveableAndLoadable>();
		// SaveAndLoadObject sameTypeObj;
		// if (!SaveAndLoadManager.saveAndLoadObjectTypeDict.TryGetValue(typeId, out sameTypeObj))
		// {
			for (int i = 0; i < _saveables.Length; i ++)
			{
				ISaveableAndLoadable saveable = _saveables[i];
				AddSaveableAndLoadable (saveable);
			}
		// 	SaveAndLoadManager.saveAndLoadObjectTypeDict.Add(typeId, this);
		// }
		// else
		// {
		// 	saveEntries = sameTypeObj.saveEntries;
		// 	SaveEntry saveEntry;
		// 	for (int i = 0; i < saveEntries.Length; i ++)
		// 	{
		// 		saveEntry = saveEntries[i];
		// 		saveEntry.saveableAndLoadable = saveables[i];
		// 		saveEntry.saveableAndLoadObject = this;
		// 	}
		// }
	}

	public void AddSaveableAndLoadable (ISaveableAndLoadable saveable)
	{
		if (saveables.Contains(saveable))
			return;
		saveables = saveables.Add(saveable);
		SaveEntry saveEntry = new SaveEntry();
		saveEntry.saveableAndLoadObject = this;
		saveEntry.saveableAndLoadable = saveable;
		List<PropertyInfo> saveProperties = new List<PropertyInfo>();
		saveProperties.AddRange(saveEntry.saveableAndLoadable.GetType().GetProperties());
		for (int i = 0; i < saveProperties.Count; i ++)
		{
			PropertyInfo property = saveProperties[i];
			SaveAndLoadValueAttribute saveAndLoadValue = Attribute.GetCustomAttribute(property, typeof(SaveAndLoadValueAttribute)) as SaveAndLoadValueAttribute;
			if (saveAndLoadValue == null)
			{
				saveProperties.RemoveAt(i);
				i --;
			}
		}
		saveEntry.properties = saveProperties.ToArray();
		
		List<FieldInfo> saveFields = new List<FieldInfo>();
		saveFields.AddRange(saveEntry.saveableAndLoadable.GetType().GetFields());
		for (int i = 0; i < saveFields.Count; i ++)
		{
			FieldInfo field = saveFields[i];
			SaveAndLoadValueAttribute saveAndLoadValue = Attribute.GetCustomAttribute(field, typeof(SaveAndLoadValueAttribute)) as SaveAndLoadValueAttribute;
			if (saveAndLoadValue == null)
			{
				saveFields.RemoveAt(i);
				i --;
			}
		}
		saveEntry.fields = saveFields.ToArray();
		saveEntries = saveEntries.Add(saveEntry);
		SaveAndLoadManager.saveEntries = SaveAndLoadManager.saveEntries.Add(saveEntry);
	}
}
