using CallbackDefs;

public class BannerInfo
{
	public Callback Callback;

	public string SpriteName = string.Empty;

	public BannerInfo(string SpriteName, Callback Callback = null)
	{
		this.SpriteName = SpriteName;
		this.Callback = Callback;
	}
}
