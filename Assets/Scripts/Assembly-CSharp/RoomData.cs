#define RELEASE
public class RoomData
{
	public string RoomId;

	public string Leader;

	public string Condition;

	public string RoomName;

	public short Capacity;

	public short Current;

	public string Ip;

	public int Port;

	public RoomData(string RoomId, string Leader, string Condition, string RoomName, short Capacity, short Current, string Ip, int Port)
	{
		this.RoomId = RoomId;
		this.Leader = Leader;
		this.Condition = Condition;
		this.RoomName = RoomName;
		this.Capacity = Capacity;
		this.Current = Current;
		this.Ip = Ip;
		this.Port = Port;
		Debug.Log(RoomId + "\n" + Leader + "\n" + Condition + "\n" + this.RoomName + "\n" + Capacity + "\n" + Current + "\n" + Ip + "\n" + Port);
	}
}
