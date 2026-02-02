public class CloseFxByEvent : FxBase
{
	public override void Active(params object[] p_params)
	{
		base.Active(p_params);
		Singleton<GenericEventManager>.Instance.AttachEvent(EventManager.ID.CLOSE_FX, CloseFX);
	}

	public override void BackToPool()
	{
		Singleton<GenericEventManager>.Instance.DetachEvent(EventManager.ID.CLOSE_FX, CloseFX);
		base.BackToPool();
	}

	private void CloseFX()
	{
		BackToPool();
	}
}
