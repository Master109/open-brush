using TMPro;
using Extensions;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace EternityEngine
{
	public class Modulator : UpdateWhileEnabled
	{
		public Transform trs;
		public LogicModule.Hand currentController;
		public ModulationOption modulationOption;
		public Transform minDistanceIndicatorTrs;
		public Transform maxDistanceIndicatorTrs;
		public Transform sliderIndicatorTrs;
		public Transform minPositionAngleIndicatorTrs;
		public Transform maxPositionAngleIndicatorTrs;
		public Transform minRotationAngleIndicatorTrs;
		public Transform maxRotationAngleIndicatorTrs;
		public ControlMethod controlMethod;
		public delegate void OnValueChanged(float value);
		public event OnValueChanged onValueChanged;
		public delegate void OnUncontrolled();
		public event OnUncontrolled onUncontrolled;
		public BehaviourWhenUncontrolled behaviourWhenUncontrolled;
		public ModulationCurve.KeyframeData potentialKeyframeDataTemplateWhenUncontrolled;
		public float? startTime;
		float? previousValue;
		float value;

		public override void OnEnable ()
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
				return;
#endif
			base.OnEnable ();
			previousValue = null;
			startTime = null;
		}

		public override void DoUpdate ()
		{
			if (currentController.secondaryButtonInput)
			{
				modulationOption.EndRecording ();
				return;
			}
			if (!currentController.triggerInput)
			{
				if (onUncontrolled != null)
					onUncontrolled ();
				return;
			}
			if (startTime == null)
				startTime = Time.time;
			if (controlMethod == ControlMethod.Distance)
			{
				value = Mathf.InverseLerp(minDistanceIndicatorTrs.lossyScale.x, maxDistanceIndicatorTrs.lossyScale.x, (currentController.trs.position - minDistanceIndicatorTrs.position).magnitude);
				value = Mathf.Clamp(value, minDistanceIndicatorTrs.lossyScale.x, maxDistanceIndicatorTrs.lossyScale.x);
			}
			else if (controlMethod == ControlMethod.Slider)
			{
				LineSegment3D lineSegment = new LineSegment3D(sliderIndicatorTrs.position - sliderIndicatorTrs.forward * sliderIndicatorTrs.lossyScale.z / 2, sliderIndicatorTrs.position + sliderIndicatorTrs.forward * sliderIndicatorTrs.lossyScale.z / 2);
				if ((sliderIndicatorTrs.position - currentController.trs.position).sqrMagnitude > sliderIndicatorTrs.lossyScale.z * sliderIndicatorTrs.lossyScale.z)
				{
					float directedDistance = lineSegment.GetDirectedDistanceAlongParallel(currentController.trs.position);
					if (directedDistance < sliderIndicatorTrs.lossyScale.z / 2)
						sliderIndicatorTrs.position += currentController.trs.position - lineSegment.start;
					else
						sliderIndicatorTrs.position += currentController.trs.position - lineSegment.end;
					sliderIndicatorTrs.forward = currentController.trs.position - sliderIndicatorTrs.position;
				}
				else
					sliderIndicatorTrs.position = currentController.trs.position - lineSegment.GetPointWithDirectedDistance(lineSegment.GetDirectedDistanceAlongParallel(currentController.trs.position));
				value = lineSegment.GetDirectedDistanceAlongParallel(currentController.trs.position) / sliderIndicatorTrs.lossyScale.z;
			}
			else if (controlMethod == ControlMethod.PositionAngle)
				value = LogicModule.instance.modulationPositionAngleRange.GetNormalized(Vector3.Angle(currentController.trs.position - minPositionAngleIndicatorTrs.position, minPositionAngleIndicatorTrs.forward));
			else// if (controlMethod == ControlMethod.RotationAngle)
				value = LogicModule.instance.modulationRotationAngleRange.GetNormalized(Vector3.Angle(currentController.trs.forward, minPositionAngleIndicatorTrs.forward));
			if ((previousValue == null || value != (float) previousValue) && onValueChanged != null)
				onValueChanged (value);
			previousValue = value;
		}
		
		public enum ControlMethod
		{
			Distance,
			Slider,
			PositionAngle,
			RotationAngle
		}

		public enum BehaviourWhenUncontrolled
		{
			DontAddKeyframe,
			AddDefaultKeyframe,
			AddLastKeyframe
		}
	}
}