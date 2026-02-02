#define RELEASE
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using CallbackDefs;
using UnityEngine;

public abstract class SocketClientEx<T> : MonoBehaviourSingleton<T> where T : MonoBehaviour
{
	private enum TcpClientState
	{
		Closed = 0,
		Connected = 1,
		Connecting = 2,
		Disconnecting = 3
	}

	public enum SocketEvent
	{
		ConnectEvt = 0,
		DisconnectEvt = 1,
		SocketIOErrEvt = 2
	}

	public enum SocketEventParam
	{
		EventType = 0,
		CallbackFunc = 1,
		CallbackFuncParam = 2,
		MAX = 3
	}

	protected SocketConnection connection;

	private volatile TcpClientState tcpState;

	private readonly int storedProtocolLimit = 32767;

	private readonly int packageSizeLimit = 52428800;

	private int syncCollectTime = 80;

	protected int asyncCollectTime = 16;

	private readonly int defaultThreadSleepTime = 1;

	private Thread networkThread;

	private readonly int busyWaitLimit = 30;

	private readonly int busyWaitMillisecond = 100;

	private Callback OnDisconnectedHandler;

	private Action<bool> OnSocketErrorHandler;

	protected TimeMeasurer timeMeasurer = new TimeMeasurer();

	private Queue<double> queueTime = new Queue<double>();

	protected int validSamplingTimes = 62;

	protected List<int> listAllowContinuousTransmission = new List<int>();

	private List<SendProtocol> listSendProtocol = new List<SendProtocol>();

	private List<RecvProtocol> listRecvProtocol = new List<RecvProtocol>();

	private Queue<object[]> queSocketEvent = new Queue<object[]>();

	public string Host { get; private set; }

	public int Port { get; private set; }

	public int PackageAverageTime
	{
		get
		{
			lock (queueTime)
			{
				if (queueTime.Count <= 0)
				{
					return 0;
				}
				return Math.Min(Convert.ToInt32(Math.Ceiling(queueTime.Sum() / (double)queueTime.Count)), 500);
			}
		}
	}

	public int NetworkAverageTime
	{
		get
		{
			if (connection != null && connection.Connected())
			{
				return connection.AverageTime();
			}
			return 0;
		}
	}

	public int SendBytes
	{
		get
		{
			if (connection != null && connection.Connected())
			{
				return connection.SendBytes;
			}
			return 0;
		}
	}

	public int RecvBytes
	{
		get
		{
			if (connection != null && connection.Connected())
			{
				return connection.RecvBytes;
			}
			return 0;
		}
	}

	public abstract void CreateConnection();

	public void PushElaspedTime(double elaspedTime)
	{
		lock (queueTime)
		{
			queueTime.Enqueue(elaspedTime);
			if (queueTime.Count > validSamplingTimes)
			{
				queueTime.Dequeue();
			}
		}
	}

	public void SetSyncNetworkFrequency(int freq)
	{
		syncCollectTime = freq;
	}

	public void SetAsyncNetworkFrequency(int freq)
	{
		asyncCollectTime = freq;
	}

	private void RegistSocketEvent(object[] evtParams)
	{
		lock (queSocketEvent)
		{
			if (evtParams.Length == 3 && evtParams[1] != null)
			{
				queSocketEvent.Enqueue(evtParams);
			}
		}
	}

	protected void ExecuteSocketEvent()
	{
		lock (queSocketEvent)
		{
			while (queSocketEvent.Count > 0)
			{
				object[] array = queSocketEvent.Dequeue();
				if (array.Length != 3)
				{
					continue;
				}
				switch ((SocketEvent)array[0])
				{
				case SocketEvent.ConnectEvt:
				case SocketEvent.SocketIOErrEvt:
				{
					Action<bool> obj2 = array[1] as Action<bool>;
					bool obj3 = Convert.ToBoolean(array[2]);
					if (obj2 != null)
					{
						obj2(obj3);
					}
					break;
				}
				case SocketEvent.DisconnectEvt:
				{
					Callback obj = array[1] as Callback;
					if (obj != null)
					{
						obj();
					}
					break;
				}
				}
			}
		}
	}

	public bool Connected()
	{
		if (connection != null)
		{
			if (connection.Connected())
			{
				return tcpState == TcpClientState.Connected;
			}
			return false;
		}
		return false;
	}

	public SocketClientEx()
	{
	}

	public virtual void Update()
	{
	}

	public void ConnectToServer(string host, int port, Action<bool> connectHandler, Callback disconnectHandler = null, Action<bool> socketErrorHandler = null)
	{
		BeginConnect(host, port, connectHandler, disconnectHandler, socketErrorHandler);
	}

	protected void BeginConnect(string host, int port, Action<bool> connectHandler = null, Callback disconnectHandler = null, Action<bool> socketErrorHandler = null)
	{
		Debug.LogWarning("[BeginConnect] ClientName = " + base.name);
		if (tcpState == TcpClientState.Connecting)
		{
			Debug.LogWarning("Duplicated BeginConnect call, already connecting.");
			RegistSocketEvent(new object[3]
			{
				SocketEvent.SocketIOErrEvt,
				socketErrorHandler,
				false
			});
			return;
		}
		if (tcpState == TcpClientState.Connected)
		{
			Disconnect();
		}
		int num = 0;
		while (tcpState != 0 || networkThread != null)
		{
			Thread.Sleep(busyWaitMillisecond);
			num++;
			if (num > busyWaitLimit)
			{
				Debug.LogWarning("Busy Waiting for networkThread set to null.");
				if (networkThread != null)
				{
					Debug.LogWarning("Call socketErrorHandler.");
					RegistSocketEvent(new object[3]
					{
						SocketEvent.SocketIOErrEvt,
						socketErrorHandler,
						false
					});
					return;
				}
				Debug.LogWarning("networkThread closed change state now.");
				ChangeTCPState(TcpClientState.Closed);
			}
		}
		Debug.Log(string.Format("BeginConnect to {0}:{1}......", host, port));
		ChangeTCPState(TcpClientState.Connecting);
		lock (queSocketEvent)
		{
			queSocketEvent.Clear();
		}
		CreateConnection();
		connection.BeginConnect(host, port, delegate(bool Connected)
		{
			if (Connected)
			{
				Host = host;
				Port = port;
				ChangeTCPState(TcpClientState.Connected);
				OnDisconnectedHandler = disconnectHandler;
				OnSocketErrorHandler = socketErrorHandler;
				listSendProtocol.Clear();
				listRecvProtocol.Clear();
				string text = GetType().Name;
				if (connection is SocketConnectionAsync)
				{
					networkThread = new Thread(AsyncNetworkThread);
				}
				else
				{
					networkThread = new Thread(NetworkThread);
				}
				networkThread.IsBackground = true;
				networkThread.Name = "NetThread-" + text;
				networkThread.Start();
			}
			else
			{
				ChangeTCPState(TcpClientState.Closed);
			}
			RegistSocketEvent(new object[3]
			{
				SocketEvent.ConnectEvt,
				connectHandler,
				Connected
			});
		});
	}

	public void Disconnect()
	{
		RegistSocketEvent(new object[3]
		{
			SocketEvent.DisconnectEvt,
			OnDisconnectedHandler,
			null
		});
		OnDisconnectedHandler = null;
		OnSocketErrorHandler = null;
		if (connection != null)
		{
			connection.Disconnect();
			connection = null;
		}
		ChangeTCPState((networkThread != null) ? TcpClientState.Disconnecting : TcpClientState.Closed);
		Host = string.Empty;
		Port = 0;
	}

	private void ChangeTCPState(TcpClientState state)
	{
		Debug.Log(string.Format("Change State To {0}", state));
		if (tcpState != state)
		{
			tcpState = state;
		}
	}

	private void AsyncNetworkThread()
	{
		while (tcpState == TcpClientState.Connected)
		{
			try
			{
				bool flag = false;
				byte[] sentData = null;
				if (connection.AsyncPackageSending(ref sentData))
				{
					if (sentData != null)
					{
						flag = true;
					}
					lock (listSendProtocol)
					{
						if (listSendProtocol.Count > storedProtocolLimit)
						{
							Debug.LogError("[SocketClient]Protocol count over limit.");
							RegistSocketEvent(new object[3]
							{
								SocketEvent.SocketIOErrEvt,
								OnSocketErrorHandler,
								true
							});
							break;
						}
						MemoryStream memoryStream = new MemoryStream();
						BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
						binaryWriter.Write(listSendProtocol.Count);
						foreach (SendProtocol item in listSendProtocol)
						{
							item.Write(binaryWriter);
						}
						byte[] array = memoryStream.ToArray();
						if (array.Length > packageSizeLimit)
						{
							Debug.LogError("[SocketClient]Package size over limit.");
							RegistSocketEvent(new object[3]
							{
								SocketEvent.SocketIOErrEvt,
								OnSocketErrorHandler,
								true
							});
							break;
						}
						if (connection.BeginWritePackage(array))
						{
							listSendProtocol.Clear();
						}
					}
				}
				byte[] rawData = null;
				do
				{
					if (!connection.AsyncPackageReading(ref rawData))
					{
						continue;
					}
					if (rawData != null)
					{
						lock (listRecvProtocol)
						{
							BinaryReader binaryReader = new BinaryReader(new MemoryStream(rawData));
							binaryReader.BaseStream.Position = 0L;
							if (binaryReader.ReadInt16() == 0)
							{
								connection.SecretKey = binaryReader.ReadASCIIString();
								connection.SecretIV = binaryReader.ReadASCIIString();
							}
							int num = binaryReader.ReadInt32();
							for (int i = 0; i < num; i++)
							{
								RecvProtocol recvProtocol = new RecvProtocol();
								recvProtocol.Read(binaryReader);
								listRecvProtocol.Add(recvProtocol);
							}
						}
					}
					connection.BeginReadPackage();
				}
				while (rawData != null);
				if (flag)
				{
					flag = false;
					Thread.Sleep(asyncCollectTime);
				}
				else
				{
					Thread.Sleep(defaultThreadSleepTime);
				}
				continue;
			}
			catch (Exception ex)
			{
				Debug.Log(string.Format("[SocketClient] NetStream error [{0}].", ex.Message));
				RegistSocketEvent(new object[3]
				{
					SocketEvent.SocketIOErrEvt,
					OnSocketErrorHandler,
					true
				});
			}
			break;
		}
		networkThread = null;
		Debug.LogWarning("NetworkThread set to null");
		Disconnect();
	}

	private void NetworkThread()
	{
		while (tcpState == TcpClientState.Connected)
		{
			timeMeasurer.Start();
			lock (listSendProtocol)
			{
				if (listSendProtocol.Count > storedProtocolLimit)
				{
					Debug.LogError("[SocketClient]Protocol count over limit.");
					RegistSocketEvent(new object[3]
					{
						SocketEvent.SocketIOErrEvt,
						OnSocketErrorHandler,
						true
					});
					break;
				}
				MemoryStream memoryStream = new MemoryStream();
				BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
				binaryWriter.Write(listSendProtocol.Count);
				foreach (SendProtocol item in listSendProtocol)
				{
					item.Write(binaryWriter);
				}
				byte[] array = memoryStream.ToArray();
				if (array.Length > packageSizeLimit)
				{
					Debug.LogError("[SocketClient]Package size over limit.");
					RegistSocketEvent(new object[3]
					{
						SocketEvent.SocketIOErrEvt,
						OnSocketErrorHandler,
						true
					});
					break;
				}
				try
				{
					connection.Send(array);
				}
				catch (Exception ex)
				{
					Debug.Log(string.Format("[SocketClient] NetStream error [{0}].", ex.Message));
					RegistSocketEvent(new object[3]
					{
						SocketEvent.SocketIOErrEvt,
						OnSocketErrorHandler,
						true
					});
					goto end_IL_001a;
				}
				listSendProtocol.Clear();
				goto IL_017a;
				end_IL_001a:;
			}
			break;
			IL_017a:
			byte[] array2 = null;
			try
			{
				array2 = connection.Recv();
			}
			catch (Exception ex2)
			{
				Debug.Log(string.Format("[SocketClient] NetStream error [{0}].", ex2.Message));
				RegistSocketEvent(new object[3]
				{
					SocketEvent.SocketIOErrEvt,
					OnSocketErrorHandler,
					true
				});
				break;
			}
			if (array2 != null)
			{
				lock (listRecvProtocol)
				{
					BinaryReader binaryReader = new BinaryReader(new MemoryStream(array2));
					binaryReader.BaseStream.Position = 0L;
					if (binaryReader.ReadInt16() == 0)
					{
						connection.SecretKey = binaryReader.ReadASCIIString();
						connection.SecretIV = binaryReader.ReadASCIIString();
					}
					int num = binaryReader.ReadInt32();
					for (int i = 0; i < num; i++)
					{
						RecvProtocol recvProtocol = new RecvProtocol();
						recvProtocol.Read(binaryReader);
						listRecvProtocol.Add(recvProtocol);
					}
				}
			}
			double num2 = timeMeasurer.Elapsed();
			lock (queueTime)
			{
				queueTime.Enqueue(num2);
				if (queueTime.Count > validSamplingTimes)
				{
					queueTime.Dequeue();
				}
			}
			int num3 = Convert.ToInt32(Math.Ceiling(num2));
			try
			{
				Thread.Sleep(Math.Max(syncCollectTime - num3, defaultThreadSleepTime));
			}
			catch (ThreadAbortException ex3)
			{
				Thread.ResetAbort();
				Debug.Log(string.Format("[SocketClient] NetStream error [{0}].", ex3.Message));
				RegistSocketEvent(new object[3]
				{
					SocketEvent.SocketIOErrEvt,
					OnSocketErrorHandler,
					true
				});
				break;
			}
		}
		networkThread = null;
		Debug.LogWarning("NetworkThread set to null");
		Disconnect();
	}

	public void RetrieveProtocol(List<RecvProtocol> list)
	{
		if (list == null)
		{
			return;
		}
		lock (listRecvProtocol)
		{
			list.Clear();
			list.AddRange(listRecvProtocol);
			listRecvProtocol.Clear();
		}
	}

	public bool SendProtocol(SendProtocol pro)
	{
		if (!Connected())
		{
			return false;
		}
		lock (listSendProtocol)
		{
			listSendProtocol.RemoveAll((SendProtocol x) => x.Type == pro.Type && !listAllowContinuousTransmission.Contains(pro.Type));
			listSendProtocol.Add(pro);
		}
		return true;
	}
}
