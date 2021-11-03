using UnityEngine;

namespace Reaktion
{
	public class _Light : UpdateWhileEnabled
	{
		public Light light;
		public ReaktorLink reaktor;
		public Modifier intensity;
		public bool enableColor;
		public Gradient colorGradient;
		public bool enableBeatAccumulatedColor;
		public float accumulatedColorRate;

		void Awake ()
		{
			reaktor.Initialize(this);
			UpdateLight (0,0);
		}

		public override void DoUpdate ()
		{
			UpdateLight (reaktor.Output, (reaktor.OutputAccumulated * accumulatedColorRate) % 1.0f);
		}

		void UpdateLight (float param, float param2)
		{
			if (intensity.enabled)
				light.intensity = intensity.Evaluate(param);
			if (enableColor)
				light.color = colorGradient.Evaluate(param);
			if (enableBeatAccumulatedColor)
				light.color = colorGradient.Evaluate(param2);
		}
	}
}