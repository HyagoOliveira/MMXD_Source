namespace MagicaCloth
{
	public interface IDataVerify
	{
		int GetVersion();

		void CreateVerifyData();

		Define.Error VerifyData();

		string GetInformation();
	}
}
