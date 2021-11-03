#if UNITY_EDITOR
using UnityEngine;
using Extensions;

namespace EternityEngine
{
	public class GridTransform : EditorScript
	{
		public Transform trs;

		void Start ()
		{
			if (!Application.isPlaying)
			{
				if (trs == null)
					trs = GetComponent<Transform>();
				return;
			}
		}

		public override void Do ()
		{
			if (trs == null)
				return;
			BoundsInt bounds = trs.GetBounds().ToBoundsInt(MathfExtensions.RoundingMethod.HalfOrLessRoundsDown, MathfExtensions.RoundingMethod.HalfOrMoreRoundsUp);
			trs.position = bounds.center;
			trs.SetWorldScale (bounds.size);
		}
	}
}
#else
namespace EternityEngine
{
	public class GridTransform : EditorScript
	{
	}
}
#endif