using System;
using UnityEngine;
using System.Collections.Generic;

namespace EternityEngine
{
	[Serializable]
	public class CurveInstrument : Instrument
	{
		[SaveAndLoadValue]
		public float turnRate;
		[SaveAndLoadValue]
		public bool blockableAfterMinLength;

		public override void UpdateGraphics (LogicModule.Hand hand)
		{
			base.UpdateGraphics (hand);
			List<Vector3> positions = new List<Vector3>();
			Transform otherHandTrs;
			if (hand.isLeftHand)
				otherHandTrs = LogicModule.instance.rightHand.trs;
			else
				otherHandTrs = LogicModule.instance.leftHand.trs;
			Vector3 position = hand.trs.position;
			positions.Add(position);
			float sampleSeparation = length.max / (sampleCount - 1);
			Vector3 moveDirection = hand.trs.forward;
			float distanceRemaining = length.max;
			for (int i = 0; i < sampleCount - 1; i ++)
			{
				moveDirection = Vector3.RotateTowards(moveDirection, otherHandTrs.position - position, turnRate * sampleSeparation * Mathf.Deg2Rad, 0);
				position += moveDirection * sampleSeparation;
				positions.Add(position);
				distanceRemaining -= sampleSeparation;
				if (blockableAfterMinLength && length.max - distanceRemaining > length.min)
				{
					Option[] options = GetSelectedOptions(hand, position);
					if (options.Length > 0)
						break;
				}
			}
			lineRenderer.positionCount = positions.Count;
			lineRenderer.SetPositions(positions.ToArray());
		}

		public override Option[] GetSelectedOptions (LogicModule.Hand hand)
		{
			if (!trs.gameObject.activeInHierarchy)
				return new Option[0];
			List<Option> output = new List<Option>();
			Transform otherHandTrs;
			if (hand.isLeftHand)
				otherHandTrs = LogicModule.instance.rightHand.trs;
			else
				otherHandTrs = LogicModule.instance.leftHand.trs;
			Vector3[] positions = new Vector3[lineRenderer.positionCount];
			lineRenderer.GetPositions(positions);
			for (int i = 0; i < lineRenderer.positionCount; i ++)
			{
				Vector3 position = positions[i];
				Option[] options = GetSelectedOptions(hand, position);
				for (int i2 = 0; i2 < options.Length; i2 ++)
				{
					Option option = options[i2];
					if (!output.Contains(option))
						output.Add(option);
				}
			}
			return output.ToArray();
		}
	}
}