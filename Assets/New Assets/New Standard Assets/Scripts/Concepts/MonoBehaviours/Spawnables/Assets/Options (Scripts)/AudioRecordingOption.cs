using TMPro;
using System;
using System.IO;
using Extensions;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace EternityEngine
{
	public class AudioRecordingOption : Option
	{
		public new Data _Data
		{
			get
			{
				return (Data) data;
			}
			set
			{
				data = value;
			}
		}
		public string filePath;
		public AudioClip audioClip;
		public List<SoundOption> emitters = new List<SoundOption>();
		public Option deleteEmittersOption;

		public void Init (string filePath, AudioClip audioClip)
		{
			this.filePath = filePath;
			text.text = filePath;
			this.audioClip = audioClip;
		}

		public void DeleteEmitters ()
		{
			for (int i = 0; i < emitters.Count; i ++)
			{
				SoundOption emitter = emitters[i];
				emitter.Delete ();
			}
			emitters.Clear();
		}

		public void Delete ()
		{
			LogicModule.instance.DeleteFile (filePath);
			for (int i = 0; i < childOptionsParent.childCount; i ++)
			{
				Transform child = childOptionsParent.GetChild(i);
				child.SetParent(LogicModule.instance.sceneTrs);
			}
			ObjectPool.instance.Despawn (prefabIndex, gameObject, trs);
		}

		public void Rename (Option fileNameOption)
		{
			string filePath = fileNameOption.GetValue();
			File.Move(this.filePath, filePath);
			Init (filePath, audioClip);
			for (int i = 0; i < emitters.Count; i ++)
			{
				SoundOption emitter = emitters[i];
				emitter.UpdateTexts (filePath);
			}
		}

		public void MakeEmitter ()
		{
			SoundOption soundOption = ObjectPool.instance.SpawnComponent<SoundOption>(LogicModule.instance.soundOptionPrefab.prefabIndex, LogicModule.instance.currentSpawnOrientationOption.trs.position, LogicModule.instance.currentSpawnOrientationOption.trs.rotation, LogicModule.instance.sceneTrs);
			soundOption.trs.localScale = LogicModule.instance.currentSpawnOrientationOption.trs.localScale;
			soundOption.Init (this);
			emitters.Add(soundOption);
			deleteEmittersOption.SetActivatable (true);
		}
		
		public override void SetData ()
		{
			if (_Data == null)
				_Data = new Data();
			base.SetData ();
			SetAudioClipDataOfData ();
		}

		public void SetAudioClipDataOfData ()
		{
			if (audioClip != null)
				_Data.audioClipData = new AudioUtilities.AudioClipData(audioClip);
		}

		public void SetAudioClipDataFromData ()
		{
			if (!_Data.audioClipData.Equals(default(AudioUtilities.AudioClipData)))
				audioClip = _Data.audioClipData.ToAudioClip();
		}

		[Serializable]
		public class Data : Option.Data
		{
			[SaveAndLoadValue]
			public AudioUtilities.AudioClipData audioClipData;
			
			public override object MakeAsset ()
			{
				AudioRecordingOption recordingOption = ObjectPool.instance.SpawnComponent<AudioRecordingOption>(LogicModule.instance.audioRecordingOptionPrefab.prefabIndex, parent:LogicModule.instance.sceneTrs);
				Apply (recordingOption);
				return recordingOption;
			}

			public override void Apply (Asset asset)
			{
				base.Apply (asset);
				AudioRecordingOption recordingOption = (AudioRecordingOption) asset;
				recordingOption._Data = this;
				recordingOption.SetAudioClipDataFromData ();
			}
		}
	}
}