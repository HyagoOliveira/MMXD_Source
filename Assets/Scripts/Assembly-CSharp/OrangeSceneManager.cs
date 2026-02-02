#define RELEASE
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CallbackDefs;
using DragonBones;
using OrangeAudio;
using StageLib;
using UnityEngine;
using UnityEngine.SceneManagement;

public class OrangeSceneManager : MonoBehaviourSingleton<OrangeSceneManager>
{
	public enum LoadingType
	{
		DEFAULT = 0,
		BLACK = 1,
		WHITE = 2,
		STAGE = 3,
		PVP = 4,
		FULL = 5,
		TIP = 6,
		PATCH = 7
	}

	public const string SCENE_NAME_BOOTUP = "bootup";

	public const string SCENE_NAME_SWITCH = "switch";

	public const string SCENE_NAME_TITLE = "title";

	public const string SCENE_NAME_HOMETOP = "hometop";

	public const string SCENE_NAME_STAGE_TEST = "StageTest";

	public const string SCENE_NAME_GACHA = "Gacha";

	public const string SCENE_NAME_WORLD_VIEW = "WorldView";

	public const string SCENE_NAME_GUILD_MAIN = "GuildMain";

	private List<string> _addtiveSceneList = new List<string>();

	private Camera mainCamera;

	private List<string> listExtraLoadingAssets = new List<string>();

	private Callback ChangeSceneCallback;

	private LoadingType nowLoadingType;

	private BattleGUICamera cachedBattleGUICamera;

	private BattleFxCamera cachedBattleFxCamera;

	public string NowScene { get; private set; }

	public Scene Scene { get; private set; }

	public bool IsLoading { get; private set; }

	public bool IsBattleScene { get; private set; }

	public Camera MainCamera
	{
		get
		{
			if (mainCamera == null)
			{
				mainCamera = Camera.main;
			}
			return mainCamera;
		}
		set
		{
			mainCamera = value;
		}
	}

	public List<string> ListExtraLoadingAssets
	{
		get
		{
			if (listExtraLoadingAssets == null)
			{
				listExtraLoadingAssets = new List<string>();
			}
			return listExtraLoadingAssets;
		}
		set
		{
			listExtraLoadingAssets = value;
		}
	}

	private void Awake()
	{
		NowScene = string.Empty;
		IsLoading = false;
		IsBattleScene = false;
	}

	public void ChangeScene(string p_scene, LoadingType p_type = LoadingType.DEFAULT, Callback p_changeSceneCallback = null, bool p_clearSE = true)
	{
		if (NowScene == p_scene)
		{
			Debug.LogWarning("Current scene equal target sccene! ...SceneName:" + p_scene);
		}
		else
		{
			if (IsLoading)
			{
				return;
			}
			mainCamera = null;
			cachedBattleGUICamera = null;
			cachedBattleFxCamera = null;
			MonoBehaviourSingleton<OrangeBattleUtility>.Instance.OnChangeScene();
			MonoBehaviourSingleton<AudioManager>.Instance.StopBGM();
			if (p_clearSE)
			{
				MonoBehaviourSingleton<AudioManager>.Instance.Stop(AudioChannelType.Sound);
			}
			OrangeTimerManager.ReturnAll();
			IsLoading = true;
			bool bNeedCloaeAllUI = false;
			string nowScene = NowScene;
			if (!(nowScene == "StageTest"))
			{
				if (nowScene == "title")
				{
					bNeedCloaeAllUI = true;
				}
			}
			else
			{
				bNeedCloaeAllUI = true;
				MonoBehaviourSingleton<LegionManager>.Instance.callLight(true, 255);
				StageUpdate.bIsHost = true;
				MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.RecoveryNetGameData = string.Empty;
				MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.Save();
			}
			if (p_scene.Equals("switch"))
			{
				MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.BattleServerLogout();
				MonoBehaviourSingleton<OrangeMatchManager>.Instance.MatchServerLogout();
				MonoBehaviourSingleton<OrangeCommunityManager>.Instance.CommunityServerLogout();
				bNeedCloaeAllUI = true;
			}
			_addtiveSceneList.Clear();
			_addtiveSceneList.Add(p_scene);
			NowScene = p_scene;
			IsBattleScene = NowScene == "StageTest";
			nowLoadingType = p_type;
			MonoBehaviourSingleton<UIManager>.Instance.OpenLoadingUI(delegate
			{
				ChangeSceneCallback = p_changeSceneCallback;
				string nowScene2 = NowScene;
				if (!(nowScene2 == "hometop"))
				{
					if (nowScene2 == "StageTest")
					{
						bNeedCloaeAllUI = true;
						ReleaseSceneData(bNeedCloaeAllUI);
					}
					else
					{
						ReleaseSceneData(bNeedCloaeAllUI);
					}
				}
				else
				{
					ReleaseSceneData(bNeedCloaeAllUI);
				}
			}, nowLoadingType);
		}
	}

	private void ReleaseSceneData(bool bNeedCloseAllUI)
	{
		MonoBehaviourSingleton<InputManager>.Instance.DestroyVirtualPad();
		OrangeBattleUtility.Clear();
		if (bNeedCloseAllUI)
		{
			MonoBehaviourSingleton<UIManager>.Instance.CloseAllUI(delegate
			{
				Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.SWITCH_SCENE);
				MonoBehaviourSingleton<PoolManager>.Instance.AsyncClearPoolAll(delegate
				{
					MonoBehaviourSingleton<LocalizationManager>.Instance.ClearTextureCache(false);
					UnityFactory.factory.Clear();
					MonoBehaviourSingleton<AssetsBundleManager>.Instance.UnloadAllBundleCache(delegate
					{
						StartCoroutine(OnStartChangeScene());
					});
				});
			});
			return;
		}
		Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.SWITCH_SCENE);
		MonoBehaviourSingleton<PoolManager>.Instance.AsyncClearPoolAll(delegate
		{
			MonoBehaviourSingleton<LocalizationManager>.Instance.ClearTextureCache(false);
			UnityFactory.factory.Clear();
			MonoBehaviourSingleton<AssetsBundleManager>.Instance.UnloadAllBundleCache(delegate
			{
				StartCoroutine(OnStartChangeScene());
			});
		});
	}

	private IEnumerator OnStartChangeScene()
	{
		Resources.UnloadUnusedAssets();
		yield return CoroutineDefine._waitForEndOfFrame;
		GC.Collect();
		yield return CoroutineDefine._0_3sec;
		AsyncOperation ao = SceneManager.LoadSceneAsync(NowScene);
		ao.allowSceneActivation = false;
		float[] progressRate = new float[2] { 1f, 0f };
		if (ListExtraLoadingAssets.Count > 0)
		{
			progressRate[0] = 0.5f;
			progressRate[1] = 0.5f;
		}
		float progress = 0f;
		while (ao.progress < 0.9f)
		{
			if (ao.progress != progress)
			{
				progress = ao.progress;
				Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.UPDATE_SCENE_PROGRESS, progress * progressRate[0]);
			}
			yield return CoroutineDefine._waitForEndOfFrame;
		}
		Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.UPDATE_SCENE_PROGRESS, 0.9f * progressRate[0]);
		ao.allowSceneActivation = true;
		yield return CoroutineDefine._0_3sec;
		while (!ao.isDone)
		{
			if (ao.progress != progress)
			{
				progress = ao.progress;
				Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.UPDATE_SCENE_PROGRESS, progress * progressRate[0]);
			}
			yield return CoroutineDefine._waitForEndOfFrame;
		}
		Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.UPDATE_SCENE_PROGRESS, 1f * progressRate[0]);
		yield return CoroutineDefine._waitForEndOfFrame;
		Resources.UnloadUnusedAssets();
		yield return CoroutineDefine._waitForEndOfFrame;
		GC.Collect();
		yield return CoroutineDefine._0_3sec;
		Scene = SceneManager.GetActiveScene();
		if (ListExtraLoadingAssets.Count > 0)
		{
			Action<float> callback = delegate(float p_progress)
			{
				Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.UPDATE_SCENE_PROGRESS, progressRate[0] + p_progress * progressRate[1]);
			};
			Singleton<GenericEventManager>.Instance.AttachEvent(EventManager.ID.UPDATE_LOADING_PROGRESS, callback);
			MonoBehaviourSingleton<AssetsBundleManager>.Instance.LoadAssets(ListExtraLoadingAssets.ToArray(), delegate
			{
				Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.UPDATE_SCENE_PROGRESS, 1f);
				Singleton<GenericEventManager>.Instance.DetachEvent(EventManager.ID.UPDATE_LOADING_PROGRESS, callback);
				ListExtraLoadingAssets.Clear();
				callback = null;
				ChangeSceneComplete();
			}, AssetsBundleManager.AssetKeepMode.KEEP_IN_SCENE, false);
		}
		else
		{
			Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.UPDATE_SCENE_PROGRESS, 1f);
			ChangeSceneComplete();
		}
		yield return CoroutineDefine._waitForEndOfFrame;
	}

	private void ChangeSceneComplete()
	{
		if (!IsBattleScene)
		{
			EnemyControllerBase[] array = FindObjectsOfTypeCustom<EnemyControllerBase>();
			for (int i = 0; i < array.Length; i++)
			{
				UnityEngine.Object.Destroy(array[i].gameObject);
			}
		}
		else
		{
			MonoBehaviourSingleton<StageMaterialManager>.Instance.Create();
		}
		LoadingType loadingType = nowLoadingType;
		if ((uint)(loadingType - 1) <= 3u || (uint)(loadingType - 6) <= 1u)
		{
			ChangeSceneCallback.CheckTargetToInvoke();
			ChangeSceneCallback = null;
			IsLoading = false;
			Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.SCENE_INIT);
			return;
		}
		MonoBehaviourSingleton<UIManager>.Instance.CloseLoadingUI(delegate
		{
			ChangeSceneCallback.CheckTargetToInvoke();
			ChangeSceneCallback = null;
			IsLoading = false;
			Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.SCENE_INIT);
		});
	}

	public void AdditiveScene(string sceneName, Callback p_cb, params string[] extraLoadingAssets)
	{
		if (_addtiveSceneList.Contains(sceneName))
		{
			Debug.LogWarning("is Already AdditiveScene:" + sceneName);
		}
		else if (extraLoadingAssets != null && extraLoadingAssets.Length != 0)
		{
			Singleton<GenericEventManager>.Instance.AttachEvent<float>(EventManager.ID.UPDATE_LOADING_PROGRESS, OnUpdateLoadingProgress);
			MonoBehaviourSingleton<AssetsBundleManager>.Instance.LoadAssets(extraLoadingAssets, delegate
			{
				Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.UPDATE_SCENE_PROGRESS, 1f);
				Singleton<GenericEventManager>.Instance.DetachEvent<float>(EventManager.ID.UPDATE_LOADING_PROGRESS, OnUpdateLoadingProgress);
				StartCoroutine(OnStartAdditiveScene(sceneName, p_cb));
			}, AssetsBundleManager.AssetKeepMode.KEEP_IN_SCENE, false);
		}
		else
		{
			StartCoroutine(OnStartAdditiveScene(sceneName, p_cb));
		}
	}

	private IEnumerator OnStartAdditiveScene(string sceneName, Callback p_cb)
	{
		AsyncOperation ao = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
		while (!ao.isDone)
		{
			yield return CoroutineDefine._waitForEndOfFrame;
		}
		_addtiveSceneList.Add(sceneName);
		NowScene = _addtiveSceneList.Last();
		Scene = SceneManager.GetSceneByName(NowScene);
		SceneManager.SetActiveScene(Scene);
		Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.SCENE_INIT);
		p_cb.CheckTargetToInvoke();
	}

	private void OnUpdateLoadingProgress(float progress)
	{
		Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.UPDATE_SCENE_PROGRESS, progress);
	}

	public void UnloadScene(string sceneName, Callback p_cb)
	{
		if (!_addtiveSceneList.Contains(sceneName))
		{
			Debug.LogWarning("is No AdditiveScene:" + sceneName);
		}
		else
		{
			StartCoroutine(OnStartUnloadScene(sceneName, p_cb));
		}
	}

	private IEnumerator OnStartUnloadScene(string sceneName, Callback p_cb)
	{
		AsyncOperation ao = SceneManager.UnloadSceneAsync(sceneName);
		while (!ao.isDone)
		{
			yield return CoroutineDefine._waitForEndOfFrame;
		}
		_addtiveSceneList.Remove(sceneName);
		if (sceneName == NowScene)
		{
			NowScene = _addtiveSceneList.Last();
			Scene = SceneManager.GetSceneByName(NowScene);
			SceneManager.SetActiveScene(Scene);
		}
		Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.SCENE_INIT);
		p_cb.CheckTargetToInvoke();
	}

	public bool IsActiveScene(string p_sceneName)
	{
		return Scene.name == p_sceneName;
	}

	public BattleFxCamera GetBattleFxCamera()
	{
		if (cachedBattleFxCamera == null)
		{
			cachedBattleFxCamera = FindObjectOfTypeCustom<BattleFxCamera>();
		}
		return cachedBattleFxCamera;
	}

	public BattleGUICamera GetBattleGUICamera()
	{
		if (cachedBattleGUICamera == null)
		{
			cachedBattleGUICamera = FindObjectOfTypeCustom<BattleGUICamera>();
		}
		return cachedBattleGUICamera;
	}

	public static T[] FindObjectsOfTypeCustom<T>(bool includeInactive = false) where T : UnityEngine.Object
	{
		List<T> list = new List<T>();
		for (int i = 0; i < SceneManager.sceneCount; i++)
		{
			GameObject[] rootGameObjects = SceneManager.GetSceneAt(i).GetRootGameObjects();
			list.AddRange(rootGameObjects.SelectMany((GameObject go) => go.GetComponentsInChildren<T>(includeInactive)));
		}
		GameObject[] rootGameObjects2 = MonoBehaviourSingleton<UIManager>.Instance.gameObject.scene.GetRootGameObjects();
		list.AddRange(rootGameObjects2.SelectMany((GameObject go) => go.GetComponentsInChildren<T>(includeInactive)));
		return list.ToArray();
	}

	public static T FindObjectOfTypeCustom<T>(bool includeInactive = false) where T : UnityEngine.Object
	{
		return FindObjectsOfTypeCustom<T>(includeInactive).FirstOrDefault();
	}
}
