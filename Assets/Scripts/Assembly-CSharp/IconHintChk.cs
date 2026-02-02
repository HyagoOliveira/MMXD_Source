using System.Collections;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class IconHintChk : MonoBehaviour
{
	public enum HINT_TYPE
	{
		MAIL = 0,
		RESEARCH = 1,
		MISSION = 2,
		FRIEND = 3,
		PVP_REWARD = 4,
		GUIDE = 5,
		OPERATION_EVENT = 6,
		CHARACTER = 7,
		GALLERY = 8,
		EQUIP = 9,
		CHIP = 10,
		WEAPON = 11,
		FSSKILL = 12,
		RESEARCHALL = 13,
		ALL = 14
	}

	public enum HINT_SUB_TYPE
	{
		NORMAL = 0,
		SUGGEST = 1
	}

	private class EventReddot
	{
		public string result;
	}

	[SerializeField]
	private HINT_TYPE hintType = HINT_TYPE.ALL;

	private Color visible = Color.white;

	private Color invisible = Color.clear;

	private Image imgHint;

	private HINT_SUB_TYPE currentSubType;

	private bool isChecking;

	private void Awake()
	{
		imgHint = GetComponent<Image>();
	}

	private void OnEnable()
	{
		Singleton<GenericEventManager>.Instance.AttachEvent<HINT_TYPE>(EventManager.ID.UPDATE_HOMETOP_HINT, OnUpdateHintIcon);
		OnUpdateHintIcon(hintType);
	}

	private void OnDisable()
	{
		Singleton<GenericEventManager>.Instance.DetachEvent<HINT_TYPE>(EventManager.ID.UPDATE_HOMETOP_HINT, OnUpdateHintIcon);
	}

	public void SetImage(HINT_SUB_TYPE type)
	{
		if (currentSubType == type)
		{
			return;
		}
		currentSubType = type;
		string assetName = "UI_Common_notice";
		if (type == HINT_SUB_TYPE.SUGGEST)
		{
			assetName = "UI_Common_notice03";
		}
		MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(AssetBundleScriptableObject.Instance.m_texture_ui_common, assetName, delegate(Sprite obj)
		{
			if ((bool)obj)
			{
				imgHint.sprite = obj;
			}
		});
	}

	private void OnUpdateHintIcon(HINT_TYPE notifyType)
	{
		if (notifyType != HINT_TYPE.ALL && notifyType != hintType)
		{
			return;
		}
		bool flag = false;
		switch (hintType)
		{
		case HINT_TYPE.MAIL:
			flag = ManagedSingleton<MailHelper>.Instance.DisplayHint;
			break;
		case HINT_TYPE.RESEARCH:
			flag = ManagedSingleton<ResearchHelper>.Instance.DisplayHint;
			break;
		case HINT_TYPE.MISSION:
			flag = ManagedSingleton<MissionHelper>.Instance.DisplayHint;
			if (!flag)
			{
				flag = ManagedSingleton<MissionHelper>.Instance.DisplayDailySuggest;
				if (flag)
				{
					SetImage(HINT_SUB_TYPE.SUGGEST);
				}
			}
			else
			{
				SetImage(HINT_SUB_TYPE.NORMAL);
			}
			break;
		case HINT_TYPE.FRIEND:
			flag = ManagedSingleton<FriendHelper>.Instance.OnGetFriendDisplayHint();
			break;
		case HINT_TYPE.PVP_REWARD:
			flag = ManagedSingleton<HintHelper>.Instance.DisplayPvpRewardHint;
			break;
		case HINT_TYPE.GUIDE:
			flag = ManagedSingleton<HintHelper>.Instance.DisplayGuideHint;
			break;
		case HINT_TYPE.OPERATION_EVENT:
			CheckOperationEvent();
			break;
		case HINT_TYPE.CHARACTER:
			CheckCharacterEvent();
			break;
		case HINT_TYPE.GALLERY:
			ManagedSingleton<GalleryHelper>.Instance.BuildGalleryInfo();
			flag = ManagedSingleton<GalleryHelper>.Instance.DisplayHint;
			break;
		case HINT_TYPE.EQUIP:
			flag = ManagedSingleton<EquipHelper>.Instance.IsAnyWeaponCanUpgradeStar() || ManagedSingleton<EquipHelper>.Instance.IsAnyChipCanUp();
			break;
		case HINT_TYPE.CHIP:
			flag = ManagedSingleton<EquipHelper>.Instance.IsAnyChipCanUp();
			break;
		case HINT_TYPE.WEAPON:
			flag = ManagedSingleton<EquipHelper>.Instance.IsAnyWeaponCanUpgradeStar();
			break;
		case HINT_TYPE.FSSKILL:
			flag = ManagedSingleton<HintHelper>.Instance.IsAnyFinalStrikeCanStrengthen();
			break;
		case HINT_TYPE.RESEARCHALL:
			flag = ManagedSingleton<ResearchHelper>.Instance.DisplayHint || ManagedSingleton<HintHelper>.Instance.IsAnyFinalStrikeCanStrengthen();
			break;
		}
		if (hintType != HINT_TYPE.CHARACTER)
		{
			imgHint.color = (flag ? visible : invisible);
		}
	}

	private void CheckOperationEvent()
	{
		if (!isChecking)
		{
			isChecking = true;
			StartCoroutine(OnStartCheckOperationEvent());
		}
	}

    [System.Obsolete]
    private IEnumerator OnStartCheckOperationEvent()
	{
		string eventReddotUrl = ManagedSingleton<ServerConfig>.Instance.GetEventReddotUrl();
		using (UnityWebRequest www = UnityWebRequest.Get(eventReddotUrl))
		{
			www.SendWebRequest();
			while (!www.isDone)
			{
				yield return CoroutineDefine._waitForEndOfFrame;
			}
			if (!www.isNetworkError || !www.isHttpError)
			{
				string text = www.downloadHandler.text;
				if (string.IsNullOrEmpty(text))
				{
					imgHint.color = invisible;
				}
				else
				{
					try
					{
						EventReddot eventReddot = JsonConvert.DeserializeObject<EventReddot>(text);
						int result = 0;
						int.TryParse(eventReddot.result, out result);
						imgHint.color = ((result == 1) ? visible : invisible);
					}
					catch
					{
						imgHint.color = invisible;
					}
				}
			}
			else
			{
				imgHint.color = invisible;
			}
			www.Dispose();
			isChecking = false;
		}
		yield return null;
	}

	private void CheckCharacterEvent()
	{
		imgHint.color = invisible;
		StartCoroutine(OnStartCheckCharacterEvent());
	}

	private IEnumerator OnStartCheckCharacterEvent()
	{
		while (ManagedSingleton<CharacterHelper>.Instance.IsUpgradeChecking())
		{
			imgHint.color = (ManagedSingleton<CharacterHelper>.Instance.IsUpgradeAvailable() ? visible : invisible);
			yield return null;
		}
		imgHint.color = (ManagedSingleton<CharacterHelper>.Instance.IsUpgradeAvailable() ? visible : invisible);
	}
}
