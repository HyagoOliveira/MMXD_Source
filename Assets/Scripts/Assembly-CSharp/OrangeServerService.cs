#define RELEASE
using System;
using System.Text;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;

public abstract class OrangeServerService<T> : ServerService<T> where T : MonoBehaviour
{
	public bool IsSending { get; private set; }

	protected override void SendingRequest(bool isSending)
	{
		IsSending = isSending;
		MonoBehaviourSingleton<UIManager>.Instance.Connecting(isSending);
	}

	public override void SendRequest<U>(IRequest req, Action<U> cb = null, bool errorRtnTitle = false)
	{
		base.SendRequest(req, cb, errorRtnTitle);
	}

	protected override void WWWNetworkError(RequestCommand cmd)
	{
		if (cmd.autoRetryCount < HttpSetting.RetryLimit)
		{
			cmd.autoRetryCount++;
			StartCoroutine(BeginCommand(cmd));
			return;
		}
		MonoBehaviourSingleton<OrangeGameManager>.Instance.ShowRetryMsg(delegate
		{
			StartCoroutine(BeginCommand(cmd));
		}, delegate
		{
			MonoBehaviourSingleton<UIManager>.Instance.CloseAllUI(delegate
			{
				MonoBehaviourSingleton<OrangeSceneManager>.Instance.ChangeScene("switch", OrangeSceneManager.LoadingType.DEFAULT, null, false);
			});
		});
	}

	protected override bool WWWRequestError(UnityWebRequest www, RequestCommand cmd)
	{
		try
		{
			byte[] data = www.downloadHandler.data;
			Code code = (Code)Convert.ToInt32(JObject.Parse(Encoding.ASCII.GetString(data))["_b"]);
			switch (code)
			{
			case Code.SYSTEM_INVALID_VERSION:
				ManagedSingleton<StoreHelper>.Instance.OpenMarket();
				break;
			case Code.SYSTEM_INVALID_REQUEST_TIME:
				MonoBehaviourSingleton<OrangeGameManager>.Instance.ShowRetryMsg(delegate
				{
					StartCoroutine(BeginCommand(cmd));
				}, delegate
				{
					MonoBehaviourSingleton<UIManager>.Instance.CloseAllUI(delegate
					{
						MonoBehaviourSingleton<OrangeSceneManager>.Instance.ChangeScene("switch", OrangeSceneManager.LoadingType.DEFAULT, null, false);
					});
				});
				break;
			case Code.SYSTEM_SEQID_NOT_FOUND:
			case Code.SYSTEM_SEQID_NOT_CONTINUOUS:
				MonoBehaviourSingleton<OrangeGameManager>.Instance.ShowMessageAndReturnTitle("SEQUENCE_INVALID");
				break;
			case Code.SYSTEM_TOKEN_NOT_FOUND:
			case Code.SYSTEM_INVALID_TOKEN:
			case Code.SYSTEM_TOKEN_EXPIRE:
				MonoBehaviourSingleton<OrangeGameManager>.Instance.ShowMessageAndReturnTitle("AUTHENTICATION_EXPIRED");
				break;
			case Code.SYSTEM_IN_MAINTENANCE:
				MonoBehaviourSingleton<OrangeGameManager>.Instance.ShowMessageAndReturnTitle("SYSTEM_SERVER_MAINTENANCE");
				break;
			case Code.SYSTEM_OPERATION_DEDUCT_RELOGIN:
				MonoBehaviourSingleton<OrangeGameManager>.Instance.ShowMessageAndReturnTitle("SYSTEM_PATCHVER_CHANGED");
				break;
			case Code.SYSTEM_INVALID_REGION:
				MonoBehaviourSingleton<OrangeGameManager>.Instance.ShowMessageAndReturnTitle("NETWORK_NOT_REACHABLE_TITLE");
				break;
			case Code.SYSTEM_CHANGEDAY_PROCESSING:
				SeqID++;
				if (MonoBehaviourSingleton<UIManager>.Instance.IsLoading)
				{
					MonoBehaviourSingleton<UIManager>.Instance.CloseLoadingUI(null);
				}
				MonoBehaviourSingleton<UIManager>.Instance.LoadResourceUI("UI_CommonMsg", delegate(CommonUI ui)
				{
					ui.SetupConfirmByKey("COMMON_TIP", "CROSS_REFRESH", "COMMON_OK", delegate
					{
						MonoBehaviourSingleton<UIManager>.Instance.BackToHometop();
					});
				}, false, true);
				break;
			case Code.GACHA_DRAW_NO_EVENT:
			case Code.SHOP_PURCHASE_NO_SUCH_ITEM:
			case Code.SHOP_PURCHASE_INVALID_COUNT:
				MonoBehaviourSingleton<OrangeGameManager>.Instance.ShowMessageAndReturnTitle("SYSTEM_PATCHVER_CHANGED");
				break;
			case Code.GUILD_DATA_LOCKED:
				CommonUIHelper.ShowCommonTipUI("SYSTEM_BUSY");
				break;
			default:
				MonoBehaviourSingleton<OrangeGameManager>.Instance.ShowErrorMsg(code, cmd.errorReturnTitle);
				if (!cmd.errorReturnTitle)
				{
					SeqID++;
					return true;
				}
				break;
			}
		}
		catch (Exception ex)
		{
			Debug.LogError(ex.Message);
			MonoBehaviourSingleton<UIManager>.Instance.CloseAllUI(delegate
			{
				MonoBehaviourSingleton<OrangeSceneManager>.Instance.ChangeScene("switch");
			});
		}
		return false;
	}

	protected override void WWWDecryptError(RequestCommand cmd)
	{
		MonoBehaviourSingleton<OrangeGameManager>.Instance.ShowMessageAndReturnTitle("SEQUENCE_INVALID");
	}
}
