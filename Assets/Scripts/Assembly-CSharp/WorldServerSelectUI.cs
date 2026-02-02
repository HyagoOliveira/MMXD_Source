using UnityEngine;
using UnityEngine.UI;

public class WorldServerSelectUI : OrangeUIBase
{
	[SerializeField]
	private Canvas canvasWindows;

	[SerializeField]
	private Button btnServerAsia;

	[SerializeField]
	private Button btnServerJP;

	private bool visible = true;

	protected override void Awake()
	{
		base.Awake();
		btnServerJP.onClick.RemoveAllListeners();
		btnServerAsia.onClick.RemoveAllListeners();
	}

	public void Setup()
	{
		visible = true;
		btnServerAsia.onClick.AddListener(delegate
		{
			ManagedSingleton<ServerConfig>.Instance.NowServer = ServerConfig.WorldServer.ASIA;
			SetBtnSlectCB();
		});
		btnServerJP.onClick.AddListener(delegate
		{
			ManagedSingleton<ServerConfig>.Instance.NowServer = ServerConfig.WorldServer.JP;
			SetBtnSlectCB();
		});
		ManagedSingleton<ServerConfig>.Instance.NowServer = ServerConfig.WorldServer.ASIA;
		SetBtnSlectCB();
	}

	private void SetBtnSlectCB()
	{
		ServerConfig.WorldServer nowServer = ManagedSingleton<ServerConfig>.Instance.NowServer;
		btnServerAsia.interactable = nowServer != ServerConfig.WorldServer.ASIA;
		btnServerJP.interactable = nowServer != ServerConfig.WorldServer.JP;
	}

	public override void OnClickCloseBtn()
	{
		MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.SelectWorld = ManagedSingleton<ServerConfig>.Instance.NowServer.ToString();
		MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.Save();
		base.OnClickCloseBtn();
	}

	public void OnClickVisable()
	{
		visible = !visible;
		canvasWindows.enabled = visible;
	}
}
