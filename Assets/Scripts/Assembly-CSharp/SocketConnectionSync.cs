using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Threading;

public sealed class SocketConnectionSync : SocketConnection
{
	private NetworkStream netStream;

	private BinaryWriter binWriter;

	private bool isWaitingResponse;

	private TimeMeasurer timeMeasurer = new TimeMeasurer();

	private Queue<double> queueTime = new Queue<double>();

	public override int AverageTime()
	{
		if (queueTime.Count <= 0)
		{
			return 0;
		}
		return Convert.ToInt32(Math.Ceiling(queueTime.Sum() / (double)queueTime.Count));
	}

	public override void Connect(string host, int port)
	{
		IsCompleted = false;
		isWaitingResponse = false;
		tcpSocket = new TcpClient
		{
			NoDelay = true,
			SendTimeout = socketTimeout,
			ReceiveTimeout = socketTimeout
		};
		tcpSocket.Connect(SocketConnection.ParseSpecificServerIP(host), port);
		netStream = new NetworkStream(tcpSocket.Client);
		binWriter = new BinaryWriter(netStream);
	}

	public override void BeginConnect(string host, int port, Action<bool> handler)
	{
		IsCompleted = false;
		isWaitingResponse = false;
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
			if (tcpSocket != null && tcpSocket.Connected)
			{
				netStream = tcpSocket.GetStream();
				binWriter = new BinaryWriter(netStream);
			}
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
			isWaitingResponse = false;
		}
	}

	public override bool AsyncPackageSending(ref byte[] sentData)
	{
		throw new SocketException(-1);
	}

	public override bool BeginWritePackage(byte[] rawData)
	{
		throw new SocketException(-1);
	}

	public override bool AsyncPackageReading(ref byte[] rawData)
	{
		throw new SocketException(-1);
	}

	public override void BeginReadPackage()
	{
		throw new SocketException(-1);
	}

	public override void Send(byte[] rawData)
	{
		if (!isWaitingResponse)
		{
			byte[] array = EncryptDataWithHeaderTail(rawData);
			timeMeasurer.Start();
			totalSendBytes += array.Length;
			try
			{
				binWriter.Write(array);
			}
			catch (Exception ex)
			{
				throw ex;
			}
			isWaitingResponse = true;
		}
	}

	public override byte[] Recv()
	{
		if (!isWaitingResponse)
		{
			return null;
		}
		byte[] array = null;
		byte[] array3;
		try
		{
			byte[] array2 = RecvLimitedSize(4);
			if (array2 == null)
			{
				return null;
			}
			int num = BitConverter.ToInt32(array2, 0);
			array3 = RecvLimitedSize(num);
			if (array3 == null)
			{
				return null;
			}
			array = RecvLimitedSize(4);
			if (array == null)
			{
				return null;
			}
			totalRecvBytes += 4 + num + 4;
		}
		catch (Exception ex)
		{
			throw ex;
		}
		double item = timeMeasurer.Elapsed();
		queueTime.Enqueue(item);
		if (queueTime.Count > ValidSamplingTimes)
		{
			queueTime.Dequeue();
		}
		uint num2 = Crc32.Compute(array3);
		uint num3 = BitConverter.ToUInt32(array, 0);
		if (num2 != num3)
		{
			return null;
		}
		byte[] result = DecryptData(array3);
		isWaitingResponse = false;
		return result;
	}

	public byte[] RecvLimitedSize(int limitedSize)
	{
		byte[] array = new byte[limitedSize];
		byte[] array2 = new byte[limitedSize];
		int num = 0;
		int num2 = 0;
		while (tcpSocket != null && tcpSocket.Connected)
		{
			try
			{
				int num3 = netStream.Read(array2, 0, limitedSize - num);
				Buffer.BlockCopy(array2, 0, array, num, num3);
				num += num3;
				if (num == limitedSize)
				{
					break;
				}
				Thread.Sleep(1);
				num2++;
				if (num2 >= 1000)
				{
					throw new Exception("Reach retry limit");
				}
				continue;
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}
		return array;
	}
}
