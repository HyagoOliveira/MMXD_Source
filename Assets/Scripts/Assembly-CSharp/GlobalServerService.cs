public class GlobalServerService : OrangeServerService<GlobalServerService>
{
	public new void Awake()
	{
		base.Awake();
	}

	protected override void ParseServerResponse(RequestCommand cmd, IResponse res)
	{
		if (cmd.callbackEvent != null)
		{
			cmd.callbackEvent(res);
		}
	}
}
