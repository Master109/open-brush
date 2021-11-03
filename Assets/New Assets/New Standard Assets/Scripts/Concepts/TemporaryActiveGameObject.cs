using UnityEngine;
using System.Collections;
using System;

[Serializable]
public class TemporaryActiveGameObject
{
	public GameObject go;
	public float duration;
	public bool realtime;
	
	public virtual IEnumerator DoRoutine ()
	{
		go.SetActive(true);
		if (realtime)
			yield return new WaitForSecondsRealtime(duration);
		else
			yield return new WaitForSeconds(duration);
		go.SetActive(false);
	}
}