using System.Collections.Generic;
using FlatBuffers;
using OrangeSocket;
using cc;

internal class CCSocketClient : SocketClientEx<CCSocketClient>
{
	private List<RecvProtocol> list = new List<RecvProtocol>();

	public CCSocketClient()
	{
		listAllowContinuousTransmission.Add(48);
		listAllowContinuousTransmission.Add(50);
		listAllowContinuousTransmission.Add(65);
		listAllowContinuousTransmission.Add(14);
		listAllowContinuousTransmission.Add(16);
		listAllowContinuousTransmission.Add(9);
		listAllowContinuousTransmission.Add(33);
		listAllowContinuousTransmission.Add(31);
		listAllowContinuousTransmission.Add(60);
		listAllowContinuousTransmission.Add(82);
		listAllowContinuousTransmission.Add(92);
	}

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
			IFlatbufferObject flatbufferObject = FlatBufferCCDeserializer.Deserialize((CC)item.Type, item.Data);
			if (flatbufferObject != null)
			{
				MonoBehaviourSingleton<SignalDispatcher>.Instance.Dispatch((CC)item.Type, flatbufferObject);
			}
		}
		list.Clear();
	}

	public void OnApplicationQuit()
	{
		Disconnect();
	}
}
