using UnityEngine;
using Extensions;

namespace EternityEngine
{
	public class OptionConnectionArrow : Spawnable
	{
		public LineRenderer lineRenderer;
		public Transform pointsTo;
		public Option parent;
		public Option child;
		public float optionPenetrationFraction;
		public float textureScaleMultiplier;
		public float lineRendererWidthMultiplier;

		void Awake ()
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

		public void DoUpdate ()
		{
			Vector3 direction = pointsTo.position - parent.trs.position;
			trs.forward = direction;
			float offsetDistanceFromParent = parent.trs.lossyScale.x / 2 * (1 - optionPenetrationFraction);
			trs.position = parent.trs.position + trs.forward * offsetDistanceFromParent;
			float length = direction.magnitude - offsetDistanceFromParent;
			if (child != null)
				length -= child.trs.lossyScale.x / 2 * (1 - optionPenetrationFraction);
			else if (pointsTo == null)
				ObjectPool.instance.Despawn (prefabIndex, gameObject, trs);
			trs.SetWorldScale (Vector3.forward * length);
			lineRenderer.widthMultiplier = lineRendererWidthMultiplier * trs.parent.lossyScale.x;
			int mainTextureScaleX = (int) Mathf.Round(length / lineRenderer.widthMultiplier * textureScaleMultiplier);
			mainTextureScaleX = (int) Mathf.Clamp(mainTextureScaleX, 1, Mathf.Infinity);
			lineRenderer.material.mainTextureScale = new Vector2(mainTextureScaleX, 1);
		}
	}
}