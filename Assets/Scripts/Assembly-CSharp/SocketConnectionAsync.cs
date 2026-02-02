using System;
using System.Collections.Generic;
using System.Net.Sockets;

public sealed class SocketConnectionAsync : SocketConnection
{
	private enum RecvPackageState
	{
		NONE = 0,
		HEADER = 1,
		BODY = 2,
		CRC = 3
	}

	private TimeMeasurer recvTimeMeasurer = new TimeMeasurer();

	private TimeMeasurer sendTimeMeasurer = new TimeMeasurer();

	private Queue<double> recvQueueTime = new Queue<double>();

	private Queue<double> sendQueueTime = new Queue<double>();

	private int lastSentSize;

	private int totalSentSize;

	private bool dataSending;

	private byte[] encrypedWriteData;

	private readonly int recvHeaderSize = 4;

	private readonly int recvCrcSize = 4;

	private int recvTargetBodySize;

	private int lastReceivedSize;

	private int totalReceivedSize;

	private int targetReceiveSize;

	private bool dataReceiving;

	private byte[] encrypedReadData;

	private byte[] encrypedBodyData;

	private byte[] remainUnpackedData;

	private RecvPackageState recvPackageState;

	private readonly int defaultRecvBufferSize = 4096;

	public override int AverageTime()
	{
		if (recvQueueTime.Count <= 0 || sendQueueTime.Count <= 0)
		{
			return 0;
		}
		double num = Math.Max(Math.Min(sendQueueTime.Sum() / (double)sendQueueTime.Count, 5000.0), 1.0);
		double val = Math.Min(recvQueueTime.Sum() / (double)recvQueueTime.Count, 5000.0);
		val = Math.Max(val, 1.0);
		return Convert.ToInt32(num + val) / 2;
	}

	public override void Connect(string host, int port)
	{
		ClearReadPackage();
		ClearWritePackage();
		IsCompleted = false;
		tcpSocket = new TcpClient
		{
			NoDelay = true,
			SendTimeout = socketTimeout,
			ReceiveTimeout = socketTimeout
		};
		tcpSocket.Connect(SocketConnection.ParseSpecificServerIP(host), port);
	}

	public override void BeginConnect(string host, int port, Action<bool> handler)
	{
		ClearReadPackage();
		ClearWritePackage();
		IsCompleted = false;
		tcpSocket = null;
		TcpClient tcpClient = new TcpClient
		{
			NoDelay = true,
			SendTimeout = socketTimeout,
			ReceiveTimeout = socketTimeout
		};
		tcpClient.BeginConnect(SocketConnection.ParseSpecificServerIP(host), port, delegate(IAsyncResult ar)
		{
			tcpSocket = ar.AsyncState as TcpClient;
			IsCompleted = true;
			if (handler != null)
			{
				handler(tcpSocket.Connected);
			}
		}, tcpClient);
	}

	public override void Disconnect()
	{
		if (tcpSocket != null)
		{
			tcpSocket.Close();
			tcpSocket = null;
			IsCompleted = false;
		}
		ClearReadPackage();
		ClearWritePackage();
	}

	public override void Send(byte[] rawData)
	{
		throw new SocketException(-1);
	}

	public override byte[] Recv()
	{
		throw new SocketException(-1);
	}

	private void ClearWritePackage()
	{
		totalSentSize = 0;
		lastSentSize = 0;
		dataSending = false;
		encrypedWriteData = null;
	}

	public override bool AsyncPackageSending(ref byte[] sentData)
	{
		if (encrypedWriteData == null)
		{
			return true;
		}
		if (dataSending)
		{
			return false;
		}
		if (lastSentSize > 0)
		{
			totalSentSize += lastSentSize;
			lastSentSize = 0;
			if (totalSentSize >= encrypedWriteData.Length)
			{
				sentData = encrypedWriteData;
				ClearWritePackage();
				return true;
			}
		}
		try
		{
			dataSending = true;
			int size = Math.Max(encrypedWriteData.Length - totalSentSize, 0);
			tcpSocket.Client.BeginSend(encrypedWriteData, totalSentSize, size, SocketFlags.None, delegate(IAsyncResult result)
			{
				int num = tcpSocket.Client.EndSend(result);
				if (num < 0)
				{
					ClearWritePackage();
					throw new SocketException(10054);
				}
				lastSentSize = num;
				dataSending = false;
			}, 0);
		}
		catch (Exception ex)
		{
			ClearWritePackage();
			throw ex;
		}
		return false;
	}

	public override bool BeginWritePackage(byte[] rawData)
	{
		if (dataSending || encrypedWriteData != null)
		{
			return false;
		}
		encrypedWriteData = EncryptDataWithHeaderTail(rawData);
		totalSendBytes += encrypedWriteData.Length;
		double item = sendTimeMeasurer.Elapsed();
		sendQueueTime.Enqueue(item);
		if (sendQueueTime.Count > ValidSamplingTimes)
		{
			sendQueueTime.Dequeue();
		}
		sendTimeMeasurer.Start();
		return true;
	}

	public override byte[] EncryptData(byte[] rawData)
	{
		return LZ4Helper.EncodeWithHeader(rawData);
	}

	public override byte[] DecryptData(byte[] encryptedData)
	{
		return LZ4Helper.DecodeWithHeader(encryptedData);
	}

	private void ChangeRecvState(RecvPackageState state)
	{
		recvPackageState = state;
	}

	private void ClearReadPackage()
	{
		recvTargetBodySize = 0;
		lastReceivedSize = 0;
		totalReceivedSize = 0;
		targetReceiveSize = 0;
		dataReceiving = false;
		encrypedReadData = null;
		encrypedBodyData = null;
		ChangeRecvState(RecvPackageState.NONE);
	}

	public override bool AsyncPackageReading(ref byte[] rawData)
	{
		rawData = null;
		if (encrypedReadData == null && targetReceiveSize == 0)
		{
			return true;
		}
		if (dataReceiving)
		{
			return false;
		}
		if (lastReceivedSize > 0)
		{
			totalReceivedSize += lastReceivedSize;
			lastReceivedSize = 0;
			while (totalReceivedSize >= targetReceiveSize)
			{
				switch (recvPackageState)
				{
				default:
					throw new SocketException(-1);
				case RecvPackageState.HEADER:
					recvTargetBodySize = BitConverter.ToInt32(encrypedReadData, 0);
					if (recvHeaderSize + recvTargetBodySize + recvCrcSize > encrypedReadData.Length)
					{
						Array.Resize(ref encrypedReadData, recvHeaderSize + recvTargetBodySize + recvCrcSize);
					}
					targetReceiveSize += recvTargetBodySize;
					ChangeRecvState(RecvPackageState.BODY);
					break;
				case RecvPackageState.BODY:
					targetReceiveSize += recvCrcSize;
					encrypedBodyData = new byte[recvTargetBodySize];
					Buffer.BlockCopy(encrypedReadData, recvHeaderSize, encrypedBodyData, 0, recvTargetBodySize);
					ChangeRecvState(RecvPackageState.CRC);
					break;
				case RecvPackageState.CRC:
				{
					uint num = Crc32.Compute(encrypedBodyData);
					uint num2 = BitConverter.ToUInt32(encrypedReadData, recvHeaderSize + recvTargetBodySize);
					if (num != num2)
					{
						throw new SocketException(-1);
					}
					try
					{
						rawData = DecryptData(encrypedBodyData);
					}
					catch (Exception ex)
					{
						throw ex;
					}
					if (totalReceivedSize > recvHeaderSize + recvTargetBodySize + recvCrcSize)
					{
						int num3 = totalReceivedSize - (recvHeaderSize + recvTargetBodySize + recvCrcSize);
						remainUnpackedData = new byte[num3];
						Buffer.BlockCopy(encrypedReadData, recvHeaderSize + recvTargetBodySize + recvCrcSize, remainUnpackedData, 0, num3);
					}
					ClearReadPackage();
					totalReceivedSize += recvHeaderSize + recvTargetBodySize + recvCrcSize;
					return true;
				}
				}
			}
		}
		try
		{
			dataReceiving = true;
			tcpSocket.Client.BeginReceive(encrypedReadData, totalReceivedSize, encrypedReadData.Length - totalReceivedSize, SocketFlags.None, delegate(IAsyncResult result)
			{
				int num4 = tcpSocket.Client.EndReceive(result);
				if (num4 < 0)
				{
					ClearReadPackage();
					throw new SocketException(10054);
				}
				lastReceivedSize = num4;
				dataReceiving = false;
			}, 0);
		}
		catch (Exception ex2)
		{
			ClearReadPackage();
			throw ex2;
		}
		return false;
	}

	public override void BeginReadPackage()
	{
		if (!dataSending && encrypedReadData == null && encrypedBodyData == null && recvPackageState == RecvPackageState.NONE)
		{
			totalReceivedSize = 0;
			targetReceiveSize = recvHeaderSize;
			encrypedReadData = new byte[defaultRecvBufferSize];
			if (remainUnpackedData != null)
			{
				lastReceivedSize = remainUnpackedData.Length;
				Buffer.BlockCopy(remainUnpackedData, 0, encrypedReadData, 0, remainUnpackedData.Length);
				remainUnpackedData = null;
			}
			ChangeRecvState(RecvPackageState.HEADER);
			double item = recvTimeMeasurer.Elapsed();
			recvQueueTime.Enqueue(item);
			if (recvQueueTime.Count > ValidSamplingTimes)
			{
				recvQueueTime.Dequeue();
			}
			recvTimeMeasurer.Start();
		}
	}
}
