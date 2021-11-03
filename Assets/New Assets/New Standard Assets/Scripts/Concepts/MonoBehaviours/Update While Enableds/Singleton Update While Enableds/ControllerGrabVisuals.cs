using TiltBrush;
using UnityEngine;

namespace EternityEngine
{
	public class ControllerGrabVisuals : SingletonUpdateWhileEnabled<ControllerGrabVisuals>
	{
		public Transform lineTrs;
		public Transform lineOutlineTrs;
		public float lineHorizontalOffset = 0.75f;
		public float lineOutlineWidth = 0.1f;
		public float lineBaseWidth = 0.025f;
		public float m_HintIntensity = 0.75f;
		public float m_DrawInDuration = 0.3f;
		public Transform animalRulerTrs;
		public Renderer animalRulerRenderer;
		public float animalRulerUZeroPoint;
		public float animalRulerUExtent;
		public Vector2 animalRulerSquishRange;
		public Vector2 animalRulerShrinkRange;
		public float animalRulerScaleSpeed = 8.0f;
		public Renderer lineRenderer;
		public Renderer lineOutlineRenderer;
		public Renderer[] animalRulerRenderers = new Renderer[0];
		float animalRulerTextureRatio;
		float intensity = 1;
		float lineDrawInTime = 0.0f;
		float lineLerpValue;

		void Start ()
		{
			lineRenderer.enabled = false;
			lineOutlineRenderer.enabled = false;
			animalRulerTrs.gameObject.SetActive(false);
			animalRulerRenderers = animalRulerTrs.GetComponentsInChildren<Renderer>();
			SetAnimalRulerScale (0);
			Texture animalsTexture = animalRulerRenderer.material.mainTexture;
			animalRulerTextureRatio = (float) animalsTexture.height / animalsTexture.width;
		}

		public override void DoUpdate ()
		{
			if (ArtModule.instance.leftHand.gripInput)
			{
				if (!ArtModule.instance.rightHand.gripInput)
					UpdateLine ();
				UpdateVisuals ();
			}
			else if (ArtModule.instance.rightHand.gripInput)
			{
				UpdateLine ();
				UpdateVisuals ();
			}
		}

		void UpdateLine ()
		{
			if (lineLerpValue < 1)
			{
				lineLerpValue = Mathf.SmoothStep(0, 1, Mathf.Clamp01(lineDrawInTime / m_DrawInDuration));
				lineDrawInTime += Time.deltaTime;
			}
		}

		Vector3 GetControllerAttachPosition (bool isLeftHand)
		{
			if (isLeftHand)
				return ArtModule.instance.leftHand.gripTrs.position;
			else
				return ArtModule.instance.rightHand.gripTrs.position;
		}

		void UpdateVisuals ()
		{
			Vector3 leftHandAttachPosition = GetControllerAttachPosition(true);
			Vector3 rightHandAttachPosition = GetControllerAttachPosition(false);
			leftHandAttachPosition = Vector3.Lerp(rightHandAttachPosition, leftHandAttachPosition, lineLerpValue);
			float lineLength = (leftHandAttachPosition - rightHandAttachPosition).magnitude - lineHorizontalOffset;
			if (lineLength > 0)
			{
				Vector3 brush_to_wand = (leftHandAttachPosition - rightHandAttachPosition).normalized;
				Vector3 centerpoint = leftHandAttachPosition - (leftHandAttachPosition - rightHandAttachPosition) / 2;
				transform.position = centerpoint;
				lineTrs.position = centerpoint;
				lineTrs.up = brush_to_wand;
				lineOutlineTrs.position = centerpoint;
				lineOutlineTrs.up = brush_to_wand;
				Vector3 temp = Vector3.one * lineBaseWidth * intensity;
				temp.y = lineLength / 2;
				lineTrs.localScale = temp;
				temp.y = lineLength / 2 + lineOutlineWidth * Mathf.Min(1, 1 / lineLength) * intensity;
				temp.x += lineOutlineWidth;
				temp.z += lineOutlineWidth;
				lineOutlineTrs.localScale = temp;
			}
			else
			{
				lineTrs.localScale = Vector3.zero;
				lineOutlineTrs.localScale = Vector3.zero;
				animalRulerTrs.gameObject.SetActive(false);
			}
			lineRenderer.material.color = SketchControlsScript.m_Instance.m_GrabHighlightActiveColor;
			UpdateAnimalRuler ();
		}

		void UpdateAnimalRuler ()
		{
			Vector3 vBrushPos = GetControllerAttachPosition(true);
			Vector3 vWandPos = GetControllerAttachPosition(false);
			Vector3 vHeadToBrush = vBrushPos - VRCameraRig.instance.eyesTrs.position;
			Vector3 vHeadToWand = vWandPos - VRCameraRig.instance.eyesTrs.position;
			Vector3 vHeadToBrushTransformed = VRCameraRig.instance.eyesTrs.InverseTransformDirection(vHeadToBrush);
			Vector3 vHeadToWandTransformed = VRCameraRig.instance.eyesTrs.InverseTransformDirection(vHeadToWand);
			Vector3 vControllerSpan = Vector3.zero;
			if (vHeadToBrushTransformed.x < vHeadToWandTransformed.x)
				vControllerSpan = vWandPos - vBrushPos;
			else
				vControllerSpan = vBrushPos - vWandPos;
			float fControllerSpanMag = vControllerSpan.magnitude - lineHorizontalOffset;
			if (fControllerSpanMag < animalRulerShrinkRange.x)
				animalRulerTrs.gameObject.SetActive(false);
			else
			{
				animalRulerTrs.gameObject.SetActive(true);
				animalRulerTrs.rotation = Quaternion.LookRotation(-vControllerSpan.normalized, ViewpointScript.Head.up);
				float fQuadWidth = Mathf.Clamp(fControllerSpanMag, animalRulerSquishRange.x, animalRulerSquishRange.y);
				float quadWidthU = fQuadWidth * animalRulerTextureRatio;
				animalRulerRenderer.transform.localScale = new Vector3(fQuadWidth, 1, 1);
				animalRulerRenderer.material.SetTextureScale("_MainTex", new Vector2(quadWidthU, 1));
				float logUserSize = -Mathf.Log(App.Scene.Pose.scale, 10.0f);
				float quadLeftU = animalRulerUZeroPoint + (logUserSize * animalRulerUExtent) - (quadWidthU * 0.5f);
				animalRulerRenderer.material.SetTextureOffset("_MainTex", new Vector2(quadLeftU, 0));
				float rulerScale = (fControllerSpanMag > animalRulerShrinkRange.y) ? 1 : ((fControllerSpanMag - animalRulerShrinkRange.x) / (animalRulerShrinkRange.y - animalRulerShrinkRange.x));
				float fCurrentScale = animalRulerTrs.localScale.x;
				float fScaleDiff = rulerScale - fCurrentScale;
				if (fScaleDiff != 0)
				{
					float fScaleStep = animalRulerScaleSpeed * Time.deltaTime;
					if (fCurrentScale < rulerScale)
						fCurrentScale = Mathf.Min(fCurrentScale + fScaleStep, rulerScale);
					else if (fCurrentScale > rulerScale)
						fCurrentScale = Mathf.Max(fCurrentScale - fScaleStep, rulerScale);
					SetAnimalRulerScale(fCurrentScale);
				}
			}
			for (int i = 0; i < animalRulerRenderers.Length; i++)
				animalRulerRenderers[i].material.color = SketchControlsScript.m_Instance.m_GrabHighlightActiveColor;
		}

		void SetAnimalRulerScale (float fScale)
		{
			animalRulerTrs.localScale = Vector3.one * fScale;
		}
	}
}