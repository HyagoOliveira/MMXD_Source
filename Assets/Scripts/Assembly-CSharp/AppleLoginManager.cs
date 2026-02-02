using CallbackDefs;

public class AppleLoginManager : MonoBehaviourSingleton<AppleLoginManager>
{
	private bool isInit;
    [System.Obsolete]
    public CallbackObjs OnLoginRetrieveInfoSuccess;

	public Callback OnLoginRetrieveInfoCancel;

	public bool IsSupportAppleLogin
	{
		get
		{
			return false;
		}
	}

	public void LoginWithInitialize()
	{
		OnLoginRetrieveInfoCancel.CheckTargetToInvoke();
	}
}
