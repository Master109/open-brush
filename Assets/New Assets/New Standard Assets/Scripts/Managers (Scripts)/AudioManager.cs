﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Extensions;

namespace EternityEngine
{
	public class AudioManager : SingletonMonoBehaviour<AudioManager>
	{
		public static float Volume
		{
			get
			{
				return PlayerPrefs.GetFloat("Volume", 1);
			}
			set
			{
				AudioListener.volume = value;
				PlayerPrefs.SetFloat("Volume", value);
			}
		}
		public static bool Mute
		{
			get
			{
				return PlayerPrefsExtensions.GetBool("Mute");
			}
			set
			{
				AudioListener.pause = value;
				PlayerPrefsExtensions.SetBool("Mute", value);
			}
		}
	}
}