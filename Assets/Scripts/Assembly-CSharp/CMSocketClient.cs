#define RELEASE
using System.Collections.Generic;
using FlatBuffers;
using OrangeSocket;
using cm;

internal class CMSocketClient : SocketClientEx<CMSocketClient>
{
	private List<RecvProtocol> list = new List<RecvProtocol>();

	public override void CreateConnection()
	{
		connection = new SocketConnectionSync();
	}

	public override void Update()
	{
		OnReceivedProtocol();
		base.Update();
		ExecuteSocketEvent();
	}

	public void OnReceivedProtocol()
	{
		RetrieveProtocol(list);
		if (list.Count <= 0)
		{
			return;
		}
		foreach (RecvProtocol item in list)
		{
			IFlatbufferObject flatbufferObject = FlatBufferCMDeserializer.Deserialize((CM)item.Type, item.Data);
			if (flatbufferObject != null)
			{
				Debug.Log(string.Format("[1P] CM Received {0}!", (CM)item.Type));
				if (MonoBehaviourSingleton<SignalDispatcher>.Instance.ContainListener((CM)item.Type))
				{
					MonoBehaviourSingleton<SignalDispatcher>.Instance.Dispatch((CM)item.Type, flatbufferObject);
				}
				else
				{
					Debug.LogWarning(string.Format("[1P] {0} Message missed !!", (CM)item.Type));
				}
			}
		}
		list.Clear();
	}

	public void OnApplicationQuit()
	{
		Disconnect();
	}
}
