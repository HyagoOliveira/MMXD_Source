namespace MagicaCloth
{
	public struct ChunkData
	{
		public int chunkNo;

		public int startIndex;

		public int dataLength;

		public int useLength;

		public void Clear()
		{
			chunkNo = 0;
			startIndex = 0;
			dataLength = 0;
			useLength = 0;
		}

		public bool IsValid()
		{
			return dataLength > 0;
		}

		public override string ToString()
		{
			string empty = string.Empty;
			return empty + "[chunkNo=" + chunkNo + ",startIndex=" + startIndex + ",dataLength=" + dataLength + ",useLength=" + useLength + "\n";
		}
	}
}
