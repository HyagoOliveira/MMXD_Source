public class VoiceChatManager : MonoBehaviourSingleton<VoiceChatManager>
{
	private string roomname = "test001";

	public string GetRoomID
	{
		get
		{
			return roomname;
		}
	}

	public void SetVoiceServerName(string servername)
	{
		roomname = servername;
	}
}
