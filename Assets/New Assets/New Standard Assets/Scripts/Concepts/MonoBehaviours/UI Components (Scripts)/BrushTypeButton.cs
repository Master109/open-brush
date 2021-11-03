namespace EternityEngine
{
	public class BrushTypeButton : UIComponent
	{
		public Brush brush;

		public override void StartOperate ()
		{
			base.StartOperate ();
			if (ArtModule.instance.menusInLeftHand)
				ArtModule.instance.rightHand.currrentBrush = brush;
			else
				ArtModule.instance.leftHand.currrentBrush = brush;
		}
	}
}