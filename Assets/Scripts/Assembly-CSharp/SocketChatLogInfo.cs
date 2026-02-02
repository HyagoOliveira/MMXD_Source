using System;

public class SocketChatLogInfo
{
	public int Channel;

	public string PlayerID = string.Empty;

	public string TargetID = string.Empty;

	public DateTime UpdateTime = DateTime.Now;

	public string MessageInfo = string.Empty;

	public bool RequestedHUD;
}
