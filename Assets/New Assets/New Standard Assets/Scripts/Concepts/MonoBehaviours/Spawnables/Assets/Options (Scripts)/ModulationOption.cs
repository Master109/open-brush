using TMPro;
using System;
using Extensions;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace EternityEngine
{
	public class ModulationOption : Option
	{
		public new Data _Data
		{
			get
			{
				return (Data) data;
			}
			set
			{
				data = value;
			}
		}
		public Option modulateChildrenOption;
		public Modulator.ControlMethod controlMethod;
		public Modulator.BehaviourWhenUncontrolled behaviourWhenUncontrolled;
		public ModulationCurve modulationCurve;
		public ModulationCurve.KeyframeData keyframeDataTemplateWhenControlled;
		public ModulationCurve.KeyframeData potentialKeyframeDataTemplateWhenUncontrolled;
		public bool isForLeftHand;
		public List<ModulationTarget> modulateTargets = new List<ModulationTarget>();
		public Dictionary<TypingTargetOption, ModulationTarget> modulateTargetsDict = new Dictionary<TypingTargetOption, ModulationTarget>();
		public LineRenderer localDisplayerLineRenderer;
		public Transform tweakingDisplayerTrs;
		public LineRenderer tweakingDisplayerLineRenderer;
		public EnumOption tweakingTimeDisplayAxisEnumOption;
		public EnumOption tweakingValueDisplayAxisEnumOption;
		public static List<ModulationOption> displayingModulationOptionCurves = new List<ModulationOption>();
		public static int localDisplaySampleCount = 50;
		public static int tweakingDisplaySampleCount = 200;
		Keyframe lastKeyframe;
		Modulator modulator;

		public void Init (Modulator.ControlMethod controlMethod, Modulator.BehaviourWhenUncontrolled behaviourWhenUncontrolled, ModulationCurve.KeyframeData keyframeDataTemplateWhenControlled, ModulationCurve.KeyframeData potentialKeyframeDataTemplateWhenUncontrolled, bool isForLeftHand, string name)
		{
			this.controlMethod = controlMethod;
			this.behaviourWhenUncontrolled = behaviourWhenUncontrolled;
			this.keyframeDataTemplateWhenControlled = keyframeDataTemplateWhenControlled;
			this.potentialKeyframeDataTemplateWhenUncontrolled = potentialKeyframeDataTemplateWhenUncontrolled;
			this.isForLeftHand = isForLeftHand;
			text.text = "\"" + name + "\" Modulation Curve";
			modulateChildrenOption.onAddChild += OnAddChildToModulate;
			modulateChildrenOption.onRemoveChild += OnRemoveChildToModulate;
		}

		public void StartRecording ()
		{
			if (isForLeftHand)
				StartRecording (LogicModule.instance.leftHand);
			else
				StartRecording (LogicModule.instance.rightHand);
		}

		void StartRecording (LogicModule.Hand hand)
		{
			hand.currentInstrument.trs.gameObject.SetActive(false);
			modulator = hand.modulator;
			modulator.controlMethod = controlMethod;
			if (controlMethod == Modulator.ControlMethod.Distance)
			{
				modulator.minDistanceIndicatorTrs.SetWorldScale (Vector3.one * LogicModule.instance.modulationDistanceRange.min);
				modulator.maxDistanceIndicatorTrs.SetWorldScale (Vector3.one * LogicModule.instance.modulationDistanceRange.max);
				modulator.minDistanceIndicatorTrs.parent.gameObject.SetActive(true);
			}
			if (controlMethod == Modulator.ControlMethod.Slider)
			{
				modulator.sliderIndicatorTrs.SetWorldScale (modulator.sliderIndicatorTrs.lossyScale.SetZ(LogicModule.instance.modulationSliderLength));
				modulator.sliderIndicatorTrs.gameObject.SetActive(true);
			}
			else if (controlMethod == Modulator.ControlMethod.PositionAngle)
			{
				float coneBaseAngle = (360f - LogicModule.instance.modulationPositionAngleRange.min) / 2;
				Cone cone = new Cone(modulator.minPositionAngleIndicatorTrs.position, modulator.minPositionAngleIndicatorTrs.forward, modulator.minPositionAngleIndicatorTrs.lossyScale.x / 2, coneBaseAngle);
				MeshRenderer minPositionIndicator = cone.MakeMeshRenderer(LogicModule.instance.minModulationPositionAngleIndicatorConeBasePointCount, Cone.FaceType.Inside);
				minPositionIndicator.sharedMaterial = LogicModule.instance.minModulationPositionAngleIndicatorMaterial;
				coneBaseAngle = (360f - LogicModule.instance.modulationPositionAngleRange.max) / 2;
				cone = new Cone(modulator.maxPositionAngleIndicatorTrs.position, modulator.maxPositionAngleIndicatorTrs.forward, modulator.maxPositionAngleIndicatorTrs.lossyScale.x / 2, coneBaseAngle);
				MeshRenderer maxPositionIndicator = cone.MakeMeshRenderer(LogicModule.instance.maxModulationPositionAngleIndicatorConeBasePointCount, Cone.FaceType.Inside);
				maxPositionIndicator.sharedMaterial = LogicModule.instance.maxModulationPositionAngleIndicatorMaterial;
				modulator.minPositionAngleIndicatorTrs.parent.gameObject.SetActive(true);
			}
			else// if (controlMethod == Modulator.ControlMethod.RotationAngle)
			{
				float coneBaseAngle = (360f - LogicModule.instance.modulationRotationAngleRange.min) / 2;
				Cone cone = new Cone(modulator.minRotationAngleIndicatorTrs.position, modulator.minRotationAngleIndicatorTrs.forward, modulator.minRotationAngleIndicatorTrs.lossyScale.x / 2, coneBaseAngle);
				MeshRenderer minRotationIndicator = cone.MakeMeshRenderer(LogicModule.instance.minModulationRotationAngleIndicatorConeBasePointCount, Cone.FaceType.Inside);
				minRotationIndicator.sharedMaterial = LogicModule.instance.minModulationRotationAngleIndicatorMaterial;
				coneBaseAngle = (360f - LogicModule.instance.modulationRotationAngleRange.max) / 2;
				cone = new Cone(modulator.maxRotationAngleIndicatorTrs.position, modulator.maxRotationAngleIndicatorTrs.forward, modulator.maxRotationAngleIndicatorTrs.lossyScale.x / 2, coneBaseAngle);
				MeshRenderer maxRotationIndicator = cone.MakeMeshRenderer(LogicModule.instance.maxModulationRotationAngleIndicatorConeBasePointCount, Cone.FaceType.Inside);
				maxRotationIndicator.sharedMaterial = LogicModule.instance.maxModulationRotationAngleIndicatorMaterial;
				modulator.minRotationAngleIndicatorTrs.parent.gameObject.SetActive(true);
			}
			modulator.onUncontrolled += OnModulatorUncontrolled;
			modulator.onValueChanged += OnModulatorValueChanged;
			modulator.trs.SetParent(null);
			modulator.modulationOption = this;
			modulator.gameObject.SetActive(true);
		}

		public void EndRecording ()
		{
			if (isForLeftHand)
				EndRecording (LogicModule.instance.leftHand);
			else
				EndRecording (LogicModule.instance.rightHand);
		}

		void EndRecording (LogicModule.Hand hand)
		{
			if (controlMethod == Modulator.ControlMethod.Distance)
				modulator.minDistanceIndicatorTrs.parent.gameObject.SetActive(false);
			if (controlMethod == Modulator.ControlMethod.Slider)
				modulator.sliderIndicatorTrs.gameObject.SetActive(false);
			else if (controlMethod == Modulator.ControlMethod.PositionAngle)
				modulator.minPositionAngleIndicatorTrs.parent.gameObject.SetActive(false);
			else// if (controlMethod == Modulator.ControlMethod.RotationAngle)
				modulator.minRotationAngleIndicatorTrs.parent.gameObject.SetActive(false);
			modulator.gameObject.SetActive(false);
			modulator.onUncontrolled -= OnModulatorUncontrolled;
			modulator.onValueChanged -= OnModulatorValueChanged;
			hand.currentInstrument.trs.gameObject.SetActive(true);
			modulator.trs.SetParent(hand.trs);
			modulator.trs.localPosition = Vector3.zero;
			Display (localDisplayerLineRenderer, 0, 1, false);
		}

		void OnModulatorUncontrolled ()
		{
			if (modulator.startTime == null)
				return;
			if (behaviourWhenUncontrolled == Modulator.BehaviourWhenUncontrolled.AddDefaultKeyframe)
			{
				lastKeyframe = keyframeDataTemplateWhenControlled.ToKeyframe();
				lastKeyframe.time = Time.time - (float) modulator.startTime;
				lastKeyframe.value = potentialKeyframeDataTemplateWhenUncontrolled.value;
				modulationCurve.normalizedValueOverTimeCurve.AddKey(lastKeyframe);
			}
			else if (behaviourWhenUncontrolled == Modulator.BehaviourWhenUncontrolled.AddLastKeyframe)
			{
				lastKeyframe.time = Time.time - (float) modulator.startTime;
				modulationCurve.normalizedValueOverTimeCurve.AddKey(lastKeyframe);
			}
		}

		void OnModulatorValueChanged (float value)
		{
			lastKeyframe = keyframeDataTemplateWhenControlled.ToKeyframe();
			lastKeyframe.time = Time.time - (float) modulator.startTime;
			lastKeyframe.value = value;
			modulationCurve.normalizedValueOverTimeCurve.AddKey(lastKeyframe);
			ModulateTypingTargetOptions ();
		}

		void OnAddChildToModulate (Option option)
		{
			TypingTargetOption typingTargetOption = option as TypingTargetOption;
			if (typingTargetOption != null)
			{
				ModulationTarget modulationTarget = new ModulationTarget();
				modulationTarget.typingTargetOption = typingTargetOption;
				ModulationTargetValueRangeOption modulationTargetValueRangeOption = ObjectPool.instance.SpawnComponent<ModulationTargetValueRangeOption>(LogicModule.instance.modulationTargetValueRangeOptionPrefab.prefabIndex, LogicModule.instance.currentSpawnOrientationOption.trs.position, LogicModule.instance.currentSpawnOrientationOption.trs.rotation, LogicModule.instance.sceneTrs);
				modulationTargetValueRangeOption.trs.SetWorldScale (LogicModule.instance.currentSpawnOrientationOption.trs.lossyScale);
				modulationTargetValueRangeOption.text.text = typingTargetOption.text.text.RemoveStartAt(nameToValueSeparator);
				modulationTarget.valueRangeOption = modulationTargetValueRangeOption;
				modulateTargets.Add(modulationTarget);
				modulateTargetsDict.Add(typingTargetOption, modulationTarget);
			}
		}

		void OnRemoveChildToModulate (Option option)
		{
			TypingTargetOption typingTargetOption = option as TypingTargetOption;
			if (typingTargetOption != null)
			{
				ModulationTarget modulationTarget = modulateTargetsDict[typingTargetOption];
				DestroyImmediate(modulationTarget.valueRangeOption.gameObject);
				modulateTargets.Remove(modulationTarget);
				modulateTargetsDict.Remove(typingTargetOption);
			}
		}

		void ModulateTypingTargetOptions ()
		{
			for (int i = 0; i < modulateTargets.Count; i ++)
			{
				ModulationTarget modulationTarget = modulateTargets[i];
				modulationTarget.typingTargetOption.SetText ("" + modulationTarget.valueRangeOption.valueRange.GetNormalized(lastKeyframe.value));
			}
		}

		public void StartDisplayingTweakingDisplayer ()
		{
			if (!LogicModule.instance.useSeparateTweakingDisplayersForModulationCurvesBoolOption.value)
			{
				DisplayAxis displayValueAxis = DisplayAxis.Y;
				if (displayingModulationOptionCurves.Count > 0)
					displayValueAxis = DisplayAxis.Z;
				Display (tweakingDisplayerLineRenderer, tweakingTimeDisplayAxisEnumOption.value, tweakingValueDisplayAxisEnumOption.value, false);
			}
			else
				Display (displayingModulationOptionCurves[0].tweakingDisplayerLineRenderer, tweakingTimeDisplayAxisEnumOption.value, tweakingValueDisplayAxisEnumOption.value, true);
			displayingModulationOptionCurves.Add(this);
		}

		public void StopDisplayingTweakingDisplayer ()
		{
			tweakingDisplayerLineRenderer.enabled = false;
			if (displayingModulationOptionCurves.Count > 1)
			{
				if (this == displayingModulationOptionCurves[0])
				{
					if (displayingModulationOptionCurves[1].tweakingDisplayerLineRenderer.enabled)
						displayingModulationOptionCurves[1].Display (displayingModulationOptionCurves[1].tweakingDisplayerLineRenderer, tweakingTimeDisplayAxisEnumOption.value, tweakingValueDisplayAxisEnumOption.value, false);
				}
				else if (displayingModulationOptionCurves[0].tweakingDisplayerLineRenderer.enabled)
					displayingModulationOptionCurves[0].Display (displayingModulationOptionCurves[0].tweakingDisplayerLineRenderer, tweakingTimeDisplayAxisEnumOption.value, tweakingValueDisplayAxisEnumOption.value, false);
			}
			displayingModulationOptionCurves.Remove(this);
		}

		void Display (LineRenderer lineRenderer, int timeDisplayAxisIndex, int valueDisplayAxisIndex, bool isAlreadyDisplayingAValue)
		{
			Keyframe lastKeyframe = modulationCurve.normalizedValueOverTimeCurve.keys[modulationCurve.normalizedValueOverTimeCurve.keys.Length - 1];
			float endTime = lastKeyframe.time;
			Vector3[] positions = new Vector3[tweakingDisplaySampleCount];
			if (isAlreadyDisplayingAValue)
				lineRenderer.GetPositions(positions);
			for (int i = 0; i < tweakingDisplaySampleCount; i ++)
			{
				float time = (float) i / (tweakingDisplaySampleCount - 1) * endTime;
				float value = modulationCurve.normalizedValueOverTimeCurve.Evaluate(time);
				Vector3 position = positions[i];
				float[] positionComponents = position.ToArray();
				positionComponents[timeDisplayAxisIndex] = time;
				positionComponents[valueDisplayAxisIndex] = value;
				positions[i] = positionComponents.ToVec3();
			}
			lineRenderer.positionCount = tweakingDisplaySampleCount;
			lineRenderer.SetPositions(positions);
			lineRenderer.enabled = true;
		}

		public override void OnDespawned ()
		{
			base.OnDespawned ();
			modulateChildrenOption.onAddChild -= OnAddChildToModulate;
			modulateChildrenOption.onRemoveChild -= OnRemoveChildToModulate;
			modulator.onUncontrolled -= OnModulatorUncontrolled;
			modulator.onValueChanged -= OnModulatorValueChanged;
			displayingModulationOptionCurves.Remove(this);
		}
		
		public override void SetData ()
		{
			if (_Data == null)
				_Data = new Data();
			base.SetData ();
			SetModulationCurveOfData ();
		}

		public void SetModulationCurveOfData ()
		{
			_Data.modulationCurve = modulationCurve;
		}

		public void SetModulationCurveFromData ()
		{
			modulationCurve = _Data.modulationCurve;
		}

		public enum DisplayAxis
		{
			X,
			Y,
			Z
		}

		[Serializable]
		public struct ModulationTarget
		{
			public TypingTargetOption typingTargetOption;
			public ModulationTargetValueRangeOption valueRangeOption;
		}

		[Serializable]
		public class Data : Option.Data
		{
			[SaveAndLoadValue]
			public ModulationCurve modulationCurve;
			
			public override object MakeAsset ()
			{
				ModulationOption modulationOption = ObjectPool.instance.SpawnComponent<ModulationOption>(LogicModule.instance.modulationOptionPrefab.prefabIndex, parent:LogicModule.instance.sceneTrs);
				Apply (modulationOption);
				return modulationOption;
			}

			public override void Apply (Asset asset)
			{
				base.Apply (asset);
				ModulationOption modulationOption = (ModulationOption) asset;
				modulationOption._Data = this;
				modulationOption.SetModulationCurveFromData ();
			}
		}
	}
}