using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

public abstract class SocketConnection
{
	protected TcpClient tcpSocket;

	protected int totalSendBytes;

	protected int totalRecvBytes;

	protected string secretKey = AesCrypto.encryptKey;

	protected string secretIV = AesCrypto.iv;

	protected readonly int socketTimeout = 30000;

	public bool IsCompleted;

	protected readonly int ValidSamplingTimes = 60;

	public int SendBytes
	{
		get
		{
			return totalSendBytes;
		}
	}

	public int RecvBytes
	{
		get
		{
			return totalRecvBytes;
		}
	}

	public string SecretKey
	{
		set
		{
			secretKey = value;
		}
	}

	public string SecretIV
	{
		set
		{
			secretIV = value;
		}
	}

	public bool Connected()
	{
		if (!IsCompleted)
		{
			return false;
		}
		if (tcpSocket == null)
		{
			return false;
		}
		return tcpSocket.Connected;
	}

	public abstract int AverageTime();

	public abstract void Connect(string host, int port);

	public abstract void BeginConnect(string host, int port, Action<bool> handler);

	public abstract void Disconnect();

	public byte[] EncryptDataWithHeaderTail(byte[] rawData)
	{
		byte[] array = EncryptData(rawData);
		uint value = Crc32.Compute(array);
		int num = array.Length;
		byte[] array2 = new byte[4 + num + 4];
		Buffer.BlockCopy(BitConverter.GetBytes(num), 0, array2, 0, 4);
		Buffer.BlockCopy(array, 0, array2, 4, num);
		Buffer.BlockCopy(BitConverter.GetBytes(value), 0, array2, 4 + num, 4);
		return array2;
	}

	public virtual byte[] EncryptData(byte[] rawData)
	{
		return AesCrypto.Encode(LZ4Helper.EncodeWithHeader(rawData), secretKey, secretIV);
	}

	public virtual byte[] DecryptData(byte[] encryptedData)
	{
		return LZ4Helper.DecodeWithHeader(AesCrypto.Decode(encryptedData, secretKey, secretIV));
	}

	public abstract void Send(byte[] rawData);

	public abstract byte[] Recv();

	public abstract bool AsyncPackageSending(ref byte[] sentData);

	public abstract bool BeginWritePackage(byte[] rawData);

	public abstract bool AsyncPackageReading(ref byte[] rawData);

	public abstract void BeginReadPackage();

	public static IPAddress[] ParseSpecificServerIP(string domainName, AddressFamily type = AddressFamily.InterNetwork)
	{
		if (string.IsNullOrEmpty(domainName))
		{
			return null;
		}
		IPAddress[] hostAddresses = Dns.GetHostAddresses(domainName);
		if (hostAddresses.Length == 0)
		{
			return null;
		}
		List<IPAddress> list = new List<IPAddress>();
		IPAddress[] array = hostAddresses;
		foreach (IPAddress iPAddress in array)
		{
			if (iPAddress.AddressFamily == type)
			{
				list.Add(iPAddress);
			}
		}
		return list.ToArray();
	}
}
