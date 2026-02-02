#define RELEASE
using System;
using System.Collections.Generic;
using System.Text;
using CallbackDefs;
using OrangeApi;
using UnityEngine;
using enums;

public class SNSHelper : ManagedSingleton<SNSHelper>
{
	public class SnsLinkData
	{
		public sbyte AccountSourceType;

		public string UserID;

		public string AccessToken;
	}

	public SnsLinkData MyLinkData { get; set; }

	public override void Initialize()
	{
	}

	public override void Dispose()
	{
	}

	public void SelectBestZone()
	{
		List<GameServerGameInfo> game = ManagedSingleton<ServerConfig>.Instance.ServerSetting.Game;
		List<GameServerZoneInfo> list = new List<GameServerZoneInfo>();
		foreach (GameServerGameInfo item in game)
		{
			foreach (GameServerZoneInfo item2 in item.Zone)
			{
				if (item2.Best == 1)
				{
					list.Add(item2);
				}
			}
		}
		if (list.Count > 0)
		{
			int index = UnityEngine.Random.Range(0, list.Count);
			MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.LastChoseServiceZoneID = list[index].ID;
		}
	}

    [Obsolete]
    public void Login(SNS_TYPE snsType, CallbackObj p_cb)
	{
		MyLinkData = null;
		switch (snsType)
		{
		default:
			ManagedSingleton<PlayerNetManager>.Instance.AccountInfo.ID = MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.UID;
			ManagedSingleton<PlayerNetManager>.Instance.AccountInfo.Secret = SystemInfo.deviceUniqueIdentifier;
			ManagedSingleton<PlayerNetManager>.Instance.AccountInfo.SourceType = AccountSourceType.Unity;
			p_cb.CheckTargetToInvoke(true);
			break;
		case SNS_TYPE.INHERIT:
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_AccountInherit", delegate(AccountInherit ui)
			{
				ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
				ui._EscapeEvent = OrangeUIBase.EscapeEvent.CLOSE_UI;
				MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_OP);
				ui.OnAccountInheritSuccess = delegate(object[] p_params)
				{
					ManagedSingleton<PlayerNetManager>.Instance.AccountInfo.ID = p_params[0] as string;
					ManagedSingleton<PlayerNetManager>.Instance.AccountInfo.Secret = p_params[1] as string;
					ManagedSingleton<PlayerNetManager>.Instance.AccountInfo.SourceType = AccountSourceType.Unity;
					p_cb.CheckTargetToInvoke(true);
				};
				ui.closeCB = (Callback)Delegate.Combine(ui.closeCB, (Callback)delegate
				{
					p_cb.CheckTargetToInvoke(false);
				});
				ui.Setup();
			});
			break;
		case SNS_TYPE.FACEBOOK:
			ManagedSingleton<FacebookManager>.Instance.OnLoginRetrieveInfoCancel = delegate
			{
				p_cb.CheckTargetToInvoke(false);
			};
			ManagedSingleton<FacebookManager>.Instance.OnLoginRetrieveInfoSuccess = delegate(FacebookUser player)
			{
				SnsLoginSuccess(AccountSourceType.Facebook, player.Identify, player.AccessToken, p_cb);
			};
			ManagedSingleton<FacebookManager>.Instance.LoginWithInitialize();
			break;
		case SNS_TYPE.LINE:
			ManagedSingleton<LINEManager>.Instance.OnLoginRetrieveInfoCancel = delegate
			{
				p_cb.CheckTargetToInvoke(false);
			};
			ManagedSingleton<LINEManager>.Instance.OnLoginRetrieveInfoSuccess = delegate(LINEUser player)
			{
				SnsLoginSuccess(AccountSourceType.Line, player.UserID, player.AccessToken, p_cb);
			};
			ManagedSingleton<LINEManager>.Instance.LoginWithInitialize();
			break;
		case SNS_TYPE.TWITTER:
			ManagedSingleton<TwitterManager>.Instance.OnLoginRetrieveInfoCancel = delegate
			{
				p_cb.CheckTargetToInvoke(false);
			};
			ManagedSingleton<TwitterManager>.Instance.OnLoginRetrieveInfoSuccess = delegate(TwitterUser player)
			{
				SnsLoginSuccess(AccountSourceType.Twitter, player.AccessToken, player.AccessSecret, p_cb);
			};
			ManagedSingleton<TwitterManager>.Instance.LoginWithInitialize();
			break;
		case SNS_TYPE.APPLE:
			MonoBehaviourSingleton<AppleLoginManager>.Instance.OnLoginRetrieveInfoCancel = delegate
			{
				p_cb.CheckTargetToInvoke(false);
			};
			MonoBehaviourSingleton<AppleLoginManager>.Instance.OnLoginRetrieveInfoSuccess = delegate(object[] p_param)
			{
				byte[] bytes = (byte[])p_param[1];
				string @string = Encoding.ASCII.GetString(bytes);
				Debug.Log("Identity token in UTF8: " + Encoding.UTF8.GetString(bytes));
				SnsLoginSuccess(AccountSourceType.Apple, (string)p_param[0], @string, p_cb);
			};
			MonoBehaviourSingleton<AppleLoginManager>.Instance.LoginWithInitialize();
			break;
		case SNS_TYPE.STEAM:
			if (!MonoBehaviourSingleton<SteamManager>.Instance.TrustedAccount)
			{
				p_cb.CheckTargetToInvoke(false);
			}
			else
			{
				SnsLoginSuccess(AccountSourceType.Steam, MonoBehaviourSingleton<SteamManager>.Instance.GetUserSteamID(), MonoBehaviourSingleton<SteamManager>.Instance.GetUserTicket(), p_cb);
			}
			break;
		}
	}

    [Obsolete]
    private void SnsLoginSuccess(AccountSourceType p_accountSourceType, string p_userID, string p_accessToken, CallbackObj p_cb)
	{
		if (string.IsNullOrEmpty(p_accessToken))
		{
			p_cb.CheckTargetToInvoke(false);
			return;
		}
		RetrieveSNSLinkUIDReq(p_userID, p_accessToken, p_accountSourceType, delegate(object p_param)
		{
			RetrieveSNSLinkUIDRes retrieveSNSLinkUIDRes = (RetrieveSNSLinkUIDRes)p_param;
			Debug.Log(string.Format("RetrieveSNSLinkUIDReq: Account = {0}, Password = {1}, UID = {2}, res.Code = {3}.", p_userID, p_accessToken, retrieveSNSLinkUIDRes.UniqueIdentifier, retrieveSNSLinkUIDRes.Code));
			switch ((Code)retrieveSNSLinkUIDRes.Code)
			{
			case Code.ACCOUNT_SNSLINK_SUCCESS:
				MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.UID = retrieveSNSLinkUIDRes.UniqueIdentifier;
				MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.Birth = retrieveSNSLinkUIDRes.BirthTime;
				MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.Locate = retrieveSNSLinkUIDRes.Region;
				MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.Save();
				p_cb.CheckTargetToInvoke(true);
				break;
			case Code.ACCOUNT_SNSLINK_CANNOT_RETRIEVE_SNS_INFO:
				MyLinkData = new SnsLinkData
				{
					AccountSourceType = (sbyte)p_accountSourceType,
					UserID = p_userID,
					AccessToken = p_accessToken
				};
				p_cb.CheckTargetToInvoke(true);
				break;
			default:
				p_cb.CheckTargetToInvoke(false);
				break;
			}
		});
	}

    [Obsolete]
    public void GetInheritCode(string newPassword, CallbackObj p_cb = null)
	{
		MonoBehaviourSingleton<GlobalServerService>.Instance.SendRequest(new AuthorizeInheritCodeReq
		{
			NewPassword = newPassword,
			UniqueIdentifier = MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.UID,
			BirthTime = MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.Birth,
			Region = MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.Locate
		}, delegate(AuthorizeInheritCodeRes res)
		{
			if (res.Code == 40451)
			{
				MonoBehaviourSingleton<UIManager>.Instance.LoadResourceUI("UI_CommonMsg", delegate(CommonUI ui)
				{
					ui.OpenSE = SystemSE.CRI_SYSTEMSE_SYS_ERROR;
					ui.SetupConfirm(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_TIP"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("INHERIT_TRY_AGAIN"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_OK"), delegate
					{
						MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL);
					});
				});
			}
			else
			{
				p_cb.CheckTargetToInvoke(res.InheritCode);
			}
		});
	}

    [Obsolete]
    public void RetrieveSNSLinkUIDReq(string account, string password, AccountSourceType p_sourceType, CallbackObj p_cb)
	{
		MonoBehaviourSingleton<GlobalServerService>.Instance.SendRequest(new RetrieveSNSLinkUIDReq
		{
			Account = account,
			Password = password,
			SourceType = (sbyte)p_sourceType
		}, delegate(RetrieveSNSLinkUIDRes res)
		{
			p_cb.CheckTargetToInvoke(res);
		});
	}

    [Obsolete]
    public void RetrieveSNSLinkInfoReq(string uid, CallbackObj p_cb = null)
	{
		MonoBehaviourSingleton<GlobalServerService>.Instance.SendRequest(new RetrieveSNSLinkInfoReq
		{
			UniqueIdentifier = uid
		}, delegate(RetrieveSNSLinkInfoRes res)
		{
			p_cb.CheckTargetToInvoke(res.SNSLinkInfoList);
		});
	}

    [Obsolete]
    public void SNSLinkReq(string account, string password, sbyte sourceType, string newPassword, CallbackObj cbSuccess = null, CallbackObj cbFail = null)
	{
		SaveData saveData = MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData;
		Debug.Log(string.Format("SNSLinkReq: Account = {0}, Password = {1}, UID = {2}, NewPassword = {3}.", account, password, saveData.UID, newPassword));
		MonoBehaviourSingleton<GlobalServerService>.Instance.SendRequest(new SNSLinkReq
		{
			Account = account,
			Password = password,
			SourceType = sourceType,
			UniqueIdentifier = saveData.UID,
			NewPassword = newPassword,
			BirthTime = saveData.Birth,
			Region = saveData.Locate
		}, delegate(SNSLinkRes res)
		{
			if (res.Code == 40200)
			{
				cbSuccess.CheckTargetToInvoke(res.SNSName);
			}
			else
			{
				Debug.Log(string.Format("SNSLinkReq failed, error: {0}", res.Result));
				cbFail.CheckTargetToInvoke(res.Code);
			}
		});
	}

	public void CancelSNSLinkReq(sbyte sourceType, string uid, Callback p_cb = null)
	{
		MonoBehaviourSingleton<GlobalServerService>.Instance.SendRequest(new CancelSNSLinkReq
		{
			SourceType = sourceType,
			UniqueIdentifier = uid
		}, delegate(CancelSNSLinkRes res)
		{
			p_cb.CheckTargetToInvoke();
			if (res.Code == 40300)
			{
				switch ((SNS_TYPE)sourceType)
				{
				case SNS_TYPE.FACEBOOK:
					ManagedSingleton<FacebookManager>.Instance.Logout();
					break;
				case SNS_TYPE.TWITTER:
					ManagedSingleton<TwitterManager>.Instance.Logout();
					break;
				case SNS_TYPE.LINE:
					ManagedSingleton<LINEManager>.Instance.Logout();
					break;
				case SNS_TYPE.APPLE:
				case SNS_TYPE.STEAM:
					break;
				}
			}
		});
	}
}
