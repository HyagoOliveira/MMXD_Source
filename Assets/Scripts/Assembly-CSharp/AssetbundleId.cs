using Newtonsoft.Json;

public class AssetbundleId
{
	public string name;

	public string hash;

	public uint crc;

	public long size;

	[JsonIgnore]
	public byte[] Keys { get; private set; }

	public AssetbundleId(string name, string hash, uint crc, long size)
	{
		this.name = name;
		this.hash = hash;
		this.crc = crc;
		this.size = size;
	}

	public void SetKeys()
	{
		Keys = AssetBundleStream.GenerateBytes(crc);
	}
}
