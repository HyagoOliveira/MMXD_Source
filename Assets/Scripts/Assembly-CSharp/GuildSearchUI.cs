public class GuildSearchUI : GuildListUIBase<GuildSearchUI, GuildSearchGuildCell>
{
	private GuildCell<GuildSearchUI> _lastReqCell;

	protected override void OnEnable()
	{
		base.OnEnable();
	}

	protected override void OnDisable()
	{
		base.OnDisable();
	}

	public override void Setup()
	{
		base.Setup();
		SearchGuild();
	}
}
