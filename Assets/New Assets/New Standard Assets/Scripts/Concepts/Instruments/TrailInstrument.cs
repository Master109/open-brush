using System;
using UnityEngine;
using System.Collections.Generic;

namespace EternityEngine
{
	[Serializable]
	public class TrailInstrument : Instrument
	{
		[SaveAndLoadValue]
		public float dissapearDelay;
		[SaveAndLoadValue]
		public bool resetDissapearDelayAtMinLength;
		[SaveAndLoadValue]
		public bool resetDissapearDelayAtMaxLength;
		[HideInInspector]
		public LineRenderer temporaryLineRenderer;
		List<Point> points = new List<Point>();
		float lineRendererLength;

		public override void UpdateGraphics (LogicModule.Hand hand)
		{
			base.UpdateGraphics (hand);
			float distanceToPreviousPoint = (hand.trs.position - hand.previousPosition).magnitude;
			points.Add(new Point(hand.trs.position, Time.time, distanceToPreviousPoint));
			lineRendererLength += distanceToPreviousPoint;
			float distanceOvershoot = lineRendererLength - length.max;
			if (distanceOvershoot >= 0)
			{
				while (distanceOvershoot > 0 && points.Count > 1)
				{
					Vector3 toSecondPoint = points[1].position - points[0].position;
					Vector3 offset = Vector3.ClampMagnitude(toSecondPoint, distanceOvershoot);
					points[0].position += offset;
					distanceOvershoot -= offset.magnitude;
					if (distanceOvershoot >= 0 && !TryToRemoveTailPoint())
						break;
				}
			}
			if (hand.triggerInput)
			{
				if (!hand.previousTriggerInput)
				{
					temporaryLineRenderer = GameManager.Instantiate(lineRenderer, hand.trs);
					Transform temporaryLineRendererTrs = temporaryLineRenderer.GetComponent<Transform>();
					temporaryLineRendererTrs.SetParent(LogicModule.instance.sceneTrs);
					temporaryLineRendererTrs.position = Vector3.zero;
					temporaryLineRendererTrs.eulerAngles = Vector3.zero;
					temporaryLineRenderer.useWorldSpace = false;
				}
			}
			else if (hand.previousTriggerInput)
				GameManager.DestroyImmediate(temporaryLineRenderer.gameObject);
			if (!((lineRendererLength <= length.min && resetDissapearDelayAtMinLength) || (lineRendererLength >= length.max && resetDissapearDelayAtMaxLength)))
			{
				while (points.Count > 1 && Time.time - points[0].time > dissapearDelay)
				{
					if (!TryToRemoveTailPoint())
						break;
				}
			}
			Vector3[] positions = new Vector3[points.Count];
			for (int i = 0; i < points.Count; i ++)
			{
				Point point = points[i];
				positions[i] = point.position;
			}
			lineRenderer.positionCount = points.Count;
			lineRenderer.SetPositions(positions);
		}

		bool TryToRemoveTailPoint ()
		{
			float distanceToPreviousPoint = points[0].distanceToPreviousPoint;
			if (lineRendererLength - distanceToPreviousPoint >= length.min)
			{
				lineRendererLength -= distanceToPreviousPoint;
				points.RemoveAt(0);
				return true;
			}
			return false;
		}

		public override Option[] GetSelectedOptions (LogicModule.Hand hand)
		{
			if (!trs.gameObject.activeInHierarchy)
				return new Option[0];
			List<Option> output = new List<Option>();
			float distance = 0;
			int pointIndex = 1;
			List<Point> _points = new List<Point>(points);
			_points.Reverse();
			Point previousPoint = _points[0];
			float sampleSeparation = lineRendererLength / (sampleCount - 1);
			for (int i = 0; i < sampleCount; i ++)
			{
				float sampleDistance = sampleSeparation * i;
				for (pointIndex = pointIndex; pointIndex < _points.Count; pointIndex ++)
				{
					Point point = _points[pointIndex];
					float distanceToPreviousPoint = previousPoint.distanceToPreviousPoint;
					distance += distanceToPreviousPoint;
					float distanceOvershoot = distance - sampleDistance;
					if (distanceOvershoot >= 0)
					{
						LineSegment3D lineSegment = new LineSegment3D(previousPoint.position, point.position);
						Option[] options = GetSelectedOptions(hand, lineSegment.GetPointWithDirectedDistance(distanceToPreviousPoint - distanceOvershoot));
						for (int i2 = 0; i2 < options.Length; i2 ++)
						{
							Option option = options[i2];
							if (!output.Contains(option))
								output.Add(option);
						}
						previousPoint = point;
						pointIndex ++;
						break;
					}
					previousPoint = point;
				}
			}
			return output.ToArray();
		}

		public class Point
		{
			public Vector3 position;
			public float time;
			public float distanceToPreviousPoint;

			public Point (Vector3 position, float time, float distanceToPreviousPoint)
			{
				this.position = position;
				this.time = time;
				this.distanceToPreviousPoint = distanceToPreviousPoint;
			}
		}
	}
}