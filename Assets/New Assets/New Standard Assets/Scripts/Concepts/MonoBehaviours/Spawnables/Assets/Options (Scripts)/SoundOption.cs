using TMPro;
using System;
using Extensions;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace EternityEngine
{
	public class SoundOption : Option
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
		public AudioClip audioClip;
		public BoolOption loopBoolOption;
		public TMP_Text emitText;
		public List<SoundEntry> soundEntries = new List<SoundEntry>();
		public Sound soundPrefab;
		public AudioRecordingOption recordingOption;

		public void Init (AudioRecordingOption recordingOption)
		{
			UpdateTexts (recordingOption.filePath);
			this.recordingOption = recordingOption;
		}

		public void UpdateTexts (string filePath)
		{
			text.text = "\"" + filePath + "\" Emitter";
			emitText.text = "Emit \"" + filePath + "\"";
		}

		public void StartEmit (LogicModule.Hand hand)
		{
			Sound sound = ObjectPool.instance.SpawnComponent<Sound>(soundPrefab.prefabIndex, trs.position, trs.rotation, trs);
			sound.audioSource.clip = recordingOption.audioClip;
			sound.audioSource.loop = loopBoolOption.value;
			sound.audioSource.Play();
			ObjectPool.DelayedDespawn delayedDespawn = null;
			if (!loopBoolOption.value)
				delayedDespawn = ObjectPool.instance.DelayDespawn(sound.prefabIndex, sound.gameObject, sound.trs, recordingOption.audioClip.length);
			SoundEntry soundEntry = new SoundEntry(sound, delayedDespawn, hand.isLeftHand);
			soundEntries.Add(soundEntry);
		}

		public void EndEmit (LogicModule.Hand hand)
		{
			for (int i = 0; i < soundEntries.Count; i ++)
			{
				SoundEntry soundEntry = soundEntries[i];
				if (soundEntry.madeByLeftHand == hand.isLeftHand)
				{
					soundEntries.RemoveAt(i);
					ObjectPool.DelayedDespawn delayedDespawn = soundEntry.delayedDespawn;
					if (delayedDespawn != null)
						ObjectPool.instance.CancelDelayedDespawn (delayedDespawn);
					Sound sound = soundEntry.sound;
					sound.audioSource.loop = false;
					ObjectPool.instance.Despawn (sound.prefabIndex, sound.gameObject, sound.trs);
					i --;
				}
			}
		}

		public void SetVolume (Option volumeOption)
		{
			float volume = float.Parse(volumeOption.GetValue());
			for (int i = 0; i < soundEntries.Count; i ++)
			{
				SoundEntry soundEntry = soundEntries[i];
				soundEntry.sound.audioSource.volume = volume;
			}
		}

		public void SetPan (Option panOption)
		{
			float pan = float.Parse(panOption.GetValue());
			for (int i = 0; i < soundEntries.Count; i ++)
			{
				SoundEntry soundEntry = soundEntries[i];
				soundEntry.sound.audioSource.panStereo = pan;
			}
		}

		public void SetPitch (Option pitchOption)
		{
			float pitch = float.Parse(pitchOption.GetValue());
			for (int i = 0; i < soundEntries.Count; i ++)
			{
				SoundEntry soundEntry = soundEntries[i];
				soundEntry.sound.audioSource.pitch = pitch;
			}
		}

		public void Delete ()
		{
			for (int i = 0; i < childOptionsParent.childCount; i ++)
			{
				Transform child = childOptionsParent.GetChild(i);
				child.SetParent(LogicModule.instance.sceneTrs);
			}
			ObjectPool.instance.Despawn (prefabIndex, gameObject, trs);
			recordingOption.emitters.Remove(this);
			if (recordingOption.emitters.Count == 0)
				recordingOption.deleteEmittersOption.SetActivatable (false);
		}

		public override void OnDisable ()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
				return;
#endif
			base.OnDisable ();
			if (trs.parent == ObjectPool.instance.trs)
			{
				for (int i = 0; i < soundEntries.Count; i ++)
				{
					SoundEntry soundEntry = soundEntries[i];
					ObjectPool.DelayedDespawn delayedDespawn = soundEntry.delayedDespawn;
					if (delayedDespawn != null)
						ObjectPool.instance.CancelDelayedDespawn (delayedDespawn);
					Sound sound = soundEntry.sound;
					sound.audioSource.loop = false;
					ObjectPool.instance.Despawn (sound.prefabIndex, sound.gameObject, sound.trs);
				}
				soundEntries.Clear();
			}
		}
		
		public override void SetData ()
		{
			if (_Data == null)
				_Data = new Data();
			base.SetData ();
			SetAudioRecordingOptionNameOfData ();
		}

		public void SetAudioRecordingOptionNameOfData ()
		{
			if (recordingOption != null)
				_Data.audioRecordingOptionName = recordingOption.name;
		}

		public void SetAudioRecordingOptionNameFromData ()
		{
			if (_Data.audioRecordingOptionName != null)
			{
				for (int i = 0; i < GameManager.instance.assetsData.Count; i ++)
				{
					Asset.Data assetData = GameManager.instance.assetsData[i];
					if (assetData.name == _Data.audioRecordingOptionName)
					{
						for (int i2 = 0; i2 < LogicModule.instance.optionNamesDict.Count; i2 ++)
						{
							Option option = LogicModule.instance.optionNamesDict.keys[i2];
							if (option.name == assetData.name)
							{
								recordingOption = option as AudioRecordingOption;
								return;
							}
						}
						Asset asset = (Asset) assetData.MakeAsset();
						recordingOption = asset as AudioRecordingOption;
						return;
					}
				}
			}
		}

		public struct SoundEntry
		{
			public Sound sound;
			public ObjectPool.DelayedDespawn delayedDespawn;
			public bool madeByLeftHand;

			public SoundEntry (Sound sound, ObjectPool.DelayedDespawn delayedDespawn, bool madeByLeftHand)
			{
				this.sound = sound;
				this.delayedDespawn = delayedDespawn;
				this.madeByLeftHand = madeByLeftHand;
			}
		}

		[Serializable]
		public class Data : Option.Data
		{
			[SaveAndLoadValue]
			public string audioRecordingOptionName = null;
			
			public override object MakeAsset ()
			{
				SoundOption soundOption = ObjectPool.instance.SpawnComponent<SoundOption>(LogicModule.instance.soundOptionPrefab.prefabIndex, parent:LogicModule.instance.sceneTrs);
				Apply (soundOption);
				return soundOption;
			}

			public override void Apply (Asset asset)
			{
				base.Apply (asset);
				SoundOption soundOption = (SoundOption) asset;
				soundOption._Data = this;
				soundOption.SetAudioRecordingOptionNameFromData ();
			}
		}
	}
}