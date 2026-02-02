public class OrangeScrollSePlayerHorizontal : OrangeScrollSePlayer
{
	protected override float GetDirection()
	{
		return base.transform.localPosition.x;
	}
}
