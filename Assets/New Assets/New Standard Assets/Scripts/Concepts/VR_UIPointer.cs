using UnityEngine;

namespace EternityEngine
{
	public class VR_UIPointer : SingletonUpdateWhileEnabled<VR_UIPointer>
	{
		public Transform trs;
		public bool attachedToLeftHand;
		UIComponent selectedUIComponent;
		UIComponent previousSelectedUIComponent;
		bool operateInput;
		bool previousOperateInput;
		
		public override void DoUpdate ()
		{
			if (attachedToLeftHand)
				operateInput = InputManager.LeftTriggerInput;
			else
				operateInput = InputManager.RightTriggerInput;
			RaycastHit hit;
			selectedUIComponent = null;
			if (Physics.Raycast(trs.position, trs.forward, out hit))
			{
				selectedUIComponent = hit.collider.GetComponent<UIComponent>();
				if (selectedUIComponent != previousSelectedUIComponent)
				{
					selectedUIComponent.Select ();
					if (previousSelectedUIComponent != null)
						previousSelectedUIComponent.Deselect ();
				}
				if (operateInput && !previousOperateInput)
					selectedUIComponent.StartOperate ();
				else if (!operateInput && previousOperateInput)
					selectedUIComponent.EndOperate ();
			}
			previousSelectedUIComponent = selectedUIComponent;
			previousOperateInput = operateInput;
		}
	}
}