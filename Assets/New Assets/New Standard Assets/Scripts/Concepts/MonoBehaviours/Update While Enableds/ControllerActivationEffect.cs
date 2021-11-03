using UnityEngine;

namespace EternityEngine
{
	public class ControllerActivationEffect : UpdateWhileEnabled, ISpawnable
	{
		public int prefabIndex;
		public int PrefabIndex
		{
			get
			{
				return prefabIndex;
			}
		}
		public float duration;
		public Renderer renderer;
		float time;

		public override void DoUpdate ()
		{
			time += Time.deltaTime;
			float intensity = Mathf.Sin(time / duration * Mathf.PI);
			renderer.material.SetFloat("_Intensity", intensity);
			if (time > duration)
				Destroy(gameObject);
		}
	}
}
