#define RELEASE
using System;
using System.Collections;
using System.Collections.Generic;
using CallbackDefs;
using UnityEngine;

namespace StageLib
{
	public class StageOpenCommonTask
	{
		private CommonUI tCommonUI;

		public bool bOpen;

		public bool bNeedClose;

		private List<string> listCommonMsg = new List<string>();

		private List<Callback> listCommonCB = new List<Callback>();

		private Coroutine tOpenCommonUICoroutine;

		private Coroutine tClosetCommonUICoroutine;

		public void OpenCommon(string sTitle, string sContent, Callback ccb = null)
		{
			listCommonMsg.Add(sTitle);
			listCommonMsg.Add(sContent);
			Callback callback = ccb;
			callback = ((callback != null) ? ((Callback)Delegate.Combine(callback, new Callback(NullCommon))) : new Callback(NullCommon));
			listCommonCB.Add(callback);
			if (tOpenCommonUICoroutine == null)
			{
				tOpenCommonUICoroutine = StageResManager.GetStageUpdate().StartCoroutine(OpenCommonUICoroutine());
			}
		}

		private IEnumerator OpenCommonUICoroutine()
		{
			if (tCommonUI != null && StageResManager.GetStageUpdate().tEndCommonUI == tCommonUI)
			{
				yield break;
			}
			while (listCommonMsg.Count > 0)
			{
				while (tCommonUI != null)
				{
					if (tClosetCommonUICoroutine == null && tCommonUI != null)
					{
						tClosetCommonUICoroutine = StageResManager.GetStageUpdate().StartCoroutine(ClosetCommonUICoroutine());
					}
					if (tCommonUI != null && StageResManager.GetStageUpdate().tEndCommonUI == tCommonUI)
					{
						yield break;
					}
					yield return CoroutineDefine._waitForEndOfFrame;
				}
				while (MonoBehaviourSingleton<UIManager>.Instance.bLockTurtorial)
				{
					yield return CoroutineDefine._waitForEndOfFrame;
				}
				while (tClosetCommonUICoroutine != null)
				{
					yield return CoroutineDefine._waitForEndOfFrame;
				}
				bOpen = true;
				MonoBehaviourSingleton<UIManager>.Instance.LoadResourceUI("UI_CommonMsg", delegate(CommonUI ui)
				{
					bOpen = false;
					tCommonUI = ui;
					if (StageResManager.GetStageUpdate().IsEnd)
					{
						StageResManager.GetStageUpdate().tEndCommonUI = ui;
					}
					string p_title = "";
					string p_desc = "";
					Callback p_cb = null;
					while (listCommonMsg.Count > 0)
					{
						p_title = listCommonMsg[0];
						p_desc = listCommonMsg[1];
						p_cb = listCommonCB[0];
						listCommonMsg.RemoveAt(0);
						listCommonMsg.RemoveAt(0);
						listCommonCB.RemoveAt(0);
					}
					ui.CloseSE = SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL;
					ui.SetupConfirm(p_title, p_desc, MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_OK"), p_cb);
				});
				while (bOpen)
				{
					yield return CoroutineDefine._waitForEndOfFrame;
				}
				while (MonoBehaviourSingleton<UIManager>.Instance.bLockTurtorial)
				{
					yield return CoroutineDefine._waitForEndOfFrame;
				}
				while (tClosetCommonUICoroutine != null)
				{
					yield return CoroutineDefine._waitForEndOfFrame;
				}
				if (listCommonMsg.Count > 0 || !bNeedClose)
				{
					continue;
				}
				bNeedClose = false;
				while (tCommonUI != null)
				{
					if (tClosetCommonUICoroutine == null && tCommonUI != null)
					{
						tClosetCommonUICoroutine = StageResManager.GetStageUpdate().StartCoroutine(ClosetCommonUICoroutine());
					}
					if (tCommonUI != null && StageResManager.GetStageUpdate().tEndCommonUI == tCommonUI)
					{
						yield break;
					}
					yield return CoroutineDefine._waitForEndOfFrame;
				}
			}
			tOpenCommonUICoroutine = null;
		}

		private void NullCommon()
		{
			tCommonUI = null;
		}

		public void CloseCommon()
		{
			if (bOpen)
			{
				bNeedClose = true;
			}
			else if (tCommonUI != null)
			{
				if (tClosetCommonUICoroutine == null)
				{
					tClosetCommonUICoroutine = StageResManager.GetStageUpdate().StartCoroutine(ClosetCommonUICoroutine());
				}
				else if (listCommonMsg.Count > 0)
				{
					bNeedClose = true;
				}
			}
			else if (tOpenCommonUICoroutine != null && listCommonMsg.Count > 0)
			{
				bNeedClose = true;
			}
		}

		private IEnumerator ClosetCommonUICoroutine()
		{
			if (tCommonUI != null && StageResManager.GetStageUpdate().tEndCommonUI == tCommonUI)
			{
				tClosetCommonUICoroutine = null;
				yield break;
			}
			if (tCommonUI == null)
			{
				tClosetCommonUICoroutine = null;
				yield break;
			}
			while (MonoBehaviourSingleton<UIManager>.Instance.bLockTurtorial)
			{
				yield return CoroutineDefine._waitForEndOfFrame;
			}
			if (tCommonUI != null)
			{
				tCommonUI.OnClickCloseBtn();
			}
			if (tCommonUI != null)
			{
				Debug.LogError("OnClickCloseBtn No Set tCommonUI NULL");
				tCommonUI = null;
			}
			tClosetCommonUICoroutine = null;
		}
	}
}
