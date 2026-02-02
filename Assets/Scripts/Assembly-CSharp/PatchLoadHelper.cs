using System.Collections;
using System.Collections.Generic;
using OrangeAudio;

public class PatchLoadHelper : MonoBehaviourSingleton<PatchLoadHelper>
{
	private long keepSize;

    [System.Obsolete]
    public bool NeedLoadPatchData()
	{
		long totalFileSize = 0L;
		if (keepSize > 0)
		{
			totalFileSize = keepSize;
		}
		else
		{
			long fileSize = MonoBehaviourSingleton<AudioManager>.Instance.GetFileSize("BGM01");
			fileSize += MonoBehaviourSingleton<AudioManager>.Instance.GetFileSize("BGM02");
			long sizeNotyetDownload = MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetSizeNotyetDownload();
			totalFileSize = fileSize + sizeNotyetDownload;
			keepSize = totalFileSize;
		}
		if (totalFileSize > 0)
		{
			totalFileSize /= 1048576L;
			MonoBehaviourSingleton<UIManager>.Instance.LoadResourceUI("UI_CommonMsg", delegate(CommonUI ui)
			{
				string text = string.Format(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("NETWORK_DOWNLOAD_DESC_1"), totalFileSize.ToString("F2"));
				if (totalFileSize > 50)
				{
					text = string.Format("{0}\n{1}", text, MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("NETWORK_DOWNLOAD_DESC_2"));
				}
				ui.SetupYesNO(MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("NETWORK_DOWNLOAD_TITLE"), text, MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_OK"), MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("COMMON_CANCEL"), delegate
				{
					MonoBehaviourSingleton<AudioManager>.Instance.Stop(AudioChannelType.BGM);
					MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_OK17);
					MonoBehaviourSingleton<UIManager>.Instance.OpenLoadingUI(delegate
					{
						MonoBehaviourSingleton<UIManager>.Instance.CloseAllUI(delegate
						{
							MonoBehaviourSingleton<AssetsBundleManager>.Instance.UnloadAllBundleCache(delegate
							{
								StartCoroutine(OnStartCheckWebProgress());
								MonoBehaviourSingleton<AudioManager>.Instance.PreloadAtomSource(new string[2] { "BGM01", "BGM02" }, new int[2] { 1, 1 }, delegate
								{
									List<string> listNotyetDownload = MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetListNotyetDownload();
									MonoBehaviourSingleton<EventManager>.Instance.NotifyEvent(EventManager.ID.UPDATE_SCENE_PROGRESS, 1f);
									StopAllCoroutines();
									MonoBehaviourSingleton<AssetsBundleManager>.Instance.LoadAssets(listNotyetDownload.ToArray(), delegate
									{
										MonoBehaviourSingleton<PatchLoadHelper>.Instance.keepSize = 0L;
										MonoBehaviourSingleton<EventManager>.Instance.NotifyEvent(EventManager.ID.UPDATE_LOADING_PROGRESS, 1f);
										ManagedSingleton<StageHelper>.Instance.nStageEndGoUI = StageHelper.STAGE_END_GO.LOGIN_BONUS;
										MonoBehaviourSingleton<UIManager>.Instance.CloseLoadingUI(delegate
										{
											MonoBehaviourSingleton<OrangeSceneManager>.Instance.ChangeScene("hometop", OrangeSceneManager.LoadingType.TIP);
										}, 0.01f);
									}, AssetsBundleManager.AssetKeepMode.KEEP_NO, false);
								}, false, false);
							});
						});
					}, OrangeSceneManager.LoadingType.PATCH);
				});
			}, true);
			return true;
		}
		return false;
	}

	private IEnumerator OnStartCheckWebProgress()
	{
		float nowProgress = 0f;
		while (true)
		{
			yield return CoroutineDefine._waitForEndOfFrame;
			if (MonoBehaviourSingleton<OrangeWebRequestLoad>.Instance.DownloadProgress != nowProgress)
			{
				nowProgress = MonoBehaviourSingleton<OrangeWebRequestLoad>.Instance.DownloadProgress;
				Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.UPDATE_SCENE_PROGRESS, nowProgress);
			}
		}
	}
}
