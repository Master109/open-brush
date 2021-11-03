using UnityEngine;
using System.Collections;
using System;
using TMPro;

[Serializable]
public class TemporaryActiveText : TemporaryActiveGameObject
{
	public TMP_Text text;
	public float durationPerCharacter;
	
	public override IEnumerator DoRoutine ()
	{
		duration = text.text.Length * durationPerCharacter;
		yield return base.DoRoutine ();
	}
}