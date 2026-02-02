#define RELEASE
using System;
using System.Collections.Generic;
using FlatBuffers;
using OrangeSocket;
using cb;

internal class CBSocketClient : SocketClientEx<CBSocketClient>
{
	private DateTime lastHeartTime = new DateTime(2050, 1, 1);

	private List<RecvProtocol> list = new List<RecvProtocol>();

	private IFlatbufferObject entity;

	public CBSocketClient()
	{
		listAllowContinuousTransmission.Add(28);
		listAllowContinuousTransmission.Add(30);
		listAllowContinuousTransmission.Add(32);
		MonoBehaviourSingleton<SignalDispatcher>.Instance.AddHandler(CB.RSHeartBeat, HeartBeat);
	}

	protected override void OnDestroy()
	{
		MonoBehaviourSingleton<SignalDispatcher>.Instance.RemoveHandler(CB.RSHeartBeat, HeartBeat);
	}

	public void StartBeating()
	{
		if (asyncCollectTime != 0)
		{
			lastHeartTime = DateTime.Now.AddMinutes(1.0);
			timeMeasurer.Start();
			SendProtocol(FlatBufferCBHelper.CreateRQHeartBeat());
		}
	}

	public void HeartBeat(object res)
	{
		PushElaspedTime(timeMeasurer.Elapsed());
		Invoke("StartBeating", 0.5f);
	}

	public override void CreateConnection()
	{
		lastHeartTime = new DateTime(2050, 1, 1);
		validSamplingTimes = 10;
		if (asyncCollectTime == 0)
		{
			connection = new SocketConnectionSync();
		}
		else
		{
			connection = new SocketConnectionAsync();
		}
	}

	public override void Update()
	{
		if (DateTime.Now > lastHeartTime)
		{
			StartBeating();
		}
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
			entity = FlatBufferCBDeserializer.Deserialize((CB)item.Type, item.Data);
			if (entity == null)
			{
				continue;
			}
			try
			{
				if (MonoBehaviourSingleton<SignalDispatcher>.Instance.ContainListener((CB)item.Type))
				{
					MonoBehaviourSingleton<SignalDispatcher>.Instance.Dispatch((CB)item.Type, entity);
				}
				else
				{
					Debug.LogWarning(string.Format("[1P] {0} Message missed !!", (CB)item.Type));
				}
			}
			catch (Exception)
			{
			}
		}
	}

	public void OnApplicationQuit()
	{
		Disconnect();
	}
}
