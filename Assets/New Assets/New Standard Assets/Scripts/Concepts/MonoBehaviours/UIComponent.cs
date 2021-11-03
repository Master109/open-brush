using Extensions;
using UnityEngine;
using UnityEngine.Events;

[ExecuteInEditMode]
public class UIComponent : MonoBehaviour
{
	public Transform trs;
	public Collider collider;
	public Renderer renderer;
	public UnityEvent onSelect;
	public UnityEvent onDeselect;
	public UnityEvent onStartOperate;
	public UnityEvent onEndOperate;
	public ColorOffset selectedColorOffset;
	public ColorOffset operatingColorOffset;
	public bool endOperateOnDeselect;

	void Awake ()
	{
#if UNITY_EDITOR
		if (trs == null)
			trs = GetComponent<Transform>();
		if (collider == null)
			collider = GetComponent<Collider>();
		if (renderer == null)
			renderer = GetComponent<Renderer>();
#endif
	}

	public virtual void Select ()
	{
		renderer.material.color = selectedColorOffset.Apply(renderer.material.color);
		onSelect.Invoke();
	}

	public virtual void Deselect ()
	{
		renderer.material.color = selectedColorOffset.ApplyInverse(renderer.material.color);
		if (endOperateOnDeselect)
			EndOperate ();
		onDeselect.Invoke();
	}

	public virtual void StartOperate ()
	{
		renderer.material.color = operatingColorOffset.Apply(renderer.material.color);
		onStartOperate.Invoke();
	}

	public virtual void EndOperate ()
	{
		renderer.material.color = operatingColorOffset.ApplyInverse(renderer.material.color);
		onEndOperate.Invoke();
	}
}