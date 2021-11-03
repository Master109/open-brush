using System;
using UnityEngine;
using System.Collections.Generic;

namespace EternityEngine
{
	[ExecuteInEditMode]
	[RequireComponent(typeof(LineRenderer))]
	public class ArcsLineRenderer : UpdateWhileEnabled
	{
		public ArcEntry[] arcEntries = new ArcEntry[0];
		public LineRenderer lineRenderer;
#if UNITY_EDITOR
		public bool update;
#endif

		void Start ()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
			{
				if (lineRenderer == null)
					lineRenderer = GetComponent<LineRenderer>();
				return;
			}
#endif
		}

#if UNITY_EDITOR
		void OnValidate ()
		{
			if (update)
			{
				update = false;
				DoUpdate ();
			}
		}
#endif

		public override void DoUpdate ()
		{
			List<Vector3> points = new List<Vector3>();
			for (int i = 0; i < arcEntries.Length; i ++)
			{
				ArcEntry arcEntry = arcEntries[i];
				for (int i2 = 0; i2 < arcEntry.pointCount; i2 ++)
					points.Add(arcEntry.arc.GetPointAlongPerimeterNormalized(1f / (arcEntry.pointCount - 1) * i2));
			}
			lineRenderer.positionCount = points.Count;
			lineRenderer.SetPositions(points.ToArray());
		}

		[Serializable]
		public struct ArcEntry
		{
			public Arc3D arc;
			public int pointCount;
		}
	}
}