#define RELEASE
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PvpLoadingUI : MonoBehaviour, ILoadingState
{
	[SerializeField]
	private PvpLoadingUIUnit unitPlayerInfo;

	[SerializeField]
	private Image[] ImgVs;

	[SerializeField]
	private RawImage whiteMask;

	[SerializeField]
	private Transform[] rootCharacterImg;

	[SerializeField]
	private VerticalLayoutGroup layoutGroupLeft;

	[SerializeField]
	private VerticalLayoutGroup layoutGroupRight;

	[SerializeField]
	private Canvas bottomCanvas;

	[SerializeField]
	private OrangeText[] textPlayerName;

	[SerializeField]
	private CommonIconBase[] playerWeapons;

	[SerializeField]
	private PvpPlayerLoadingProgress[] playerProgress;

	[SerializeField]
	private Transform leftPlayerStarsRoot;

	[SerializeField]
	private Image[] leftPlayerStarImgs;

	[SerializeField]
	private Transform rightPlayerStarsRoot;

	[SerializeField]
	private Image[] rightPlayerStarImgs;

	[SerializeField]
	private Transform[] PlayerSignRoot;

	[SerializeField]
	private GameObject SignObject;

	private float[] ch_moveFrom = new float[2] { -1000f, 1000f };

	private int loadingCount = int.MaxValue;

	public bool IsComplete { get; set; }

	public object[] Params { get; set; }

	public void SetPlayerSignIcon(Transform SignRoot, string PlayerID)
	{
		if (SignRoot != null && SignObject != null)
		{
			int childCount = SignRoot.childCount;
			for (int i = 0; i < childCount; i++)
			{
				UnityEngine.Object.Destroy(SignRoot.GetChild(i).gameObject);
			}
			GameObject gameObject = UnityEngine.Object.Instantiate(SignObject, SignRoot.position, new Quaternion(0f, 0f, 0f, 0f));
			gameObject.transform.SetParent(SignRoot);
			gameObject.transform.localScale = new Vector3(1f, 1f, 1f);
			if (PlayerID == MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify && ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo != null)
			{
				gameObject.GetComponent<CommonSignBase>().SetupSign(ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.TitleID);
			}
			else
			{
				gameObject.GetComponent<CommonSignBase>().SetupSignFromHUD(PlayerID);
			}
		}
	}

	private void Awake()
	{
		IsComplete = false;
	}

	private void Start()
	{
		List<MemberInfo> listMemberInfo = MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.ListMemberInfo;
		bool flag = false;
		if (listMemberInfo.Count > 0)
		{
			flag = listMemberInfo[0].netSealBattleSettingInfo.CharacterList.Count == 3;
			Debug.LogWarning("[istMemberInfo[0].netSealBattleSettingInfo.CharacterList]" + listMemberInfo[0].netSealBattleSettingInfo.CharacterList.Count);
		}
		if (flag)
		{
			SeasonPvpSetup(ref listMemberInfo);
		}
		else
		{
			NormalPvpSetup(ref listMemberInfo);
		}
		StartCoroutine(OnDisplayTween());
	}

	private void SeasonPvpSetup(ref List<MemberInfo> listMemberInfo)
	{
		layoutGroupLeft.transform.localPosition = new Vector3(-525f, 0f, 0f);
		layoutGroupRight.transform.localPosition = new Vector3(525f, 0f, 0f);
		loadingCount = listMemberInfo.Count;
		for (int i = 0; i < listMemberInfo.Count; i++)
		{
			MemberInfo memberInfo = listMemberInfo[i];
			Transform parent = ((memberInfo.Team == 0) ? layoutGroupLeft.transform : layoutGroupRight.transform);
			for (int j = 0; j < memberInfo.netSealBattleSettingInfo.CharacterList.Count; j++)
			{
				PvpLoadingUIUnit pvpLoadingUIUnit = UnityEngine.Object.Instantiate(unitPlayerInfo);
				CHARACTER_TABLE characterTable = ManagedSingleton<OrangeTableHelper>.Instance.GetCharacterTable(memberInfo.netSealBattleSettingInfo.CharacterList[j].CharacterID);
				int skin = memberInfo.netSealBattleSettingInfo.CharacterList[j].Skin;
				int star = memberInfo.netSealBattleSettingInfo.CharacterList[j].Star;
				pvpLoadingUIUnit.Setup(characterTable, skin, star, delegate
				{
					loadingCount--;
				});
				pvpLoadingUIUnit.transform.SetParent(parent, false);
			}
		}
		SetBottomInfo(ref listMemberInfo);
	}

	private void NormalPvpSetup(ref List<MemberInfo> listMemberInfo)
	{
		layoutGroupLeft.transform.localPosition = new Vector3(-525f, -80f, 0f);
		layoutGroupRight.transform.localPosition = new Vector3(525f, -80f, 0f);
		loadingCount = listMemberInfo.Count;
		if ((bool)leftPlayerStarsRoot)
		{
			leftPlayerStarsRoot.gameObject.SetActive(true);
		}
		if ((bool)rightPlayerStarsRoot)
		{
			rightPlayerStarsRoot.gameObject.SetActive(true);
		}
		if (loadingCount == 2)
		{
			float[] ch_moveTo = new float[2];
			for (int i = 0; i < listMemberInfo.Count; i++)
			{
				int selectIdx = listMemberInfo[i].Team;
				string empty = string.Empty;
				NetCharacterInfo netCharacterInfo = listMemberInfo[i].netSealBattleSettingInfo.CharacterList[0];
				empty = ((netCharacterInfo.Skin <= 0) ? ManagedSingleton<OrangeDataManager>.Instance.CHARACTER_TABLE_DICT[netCharacterInfo.CharacterID].s_ICON : ManagedSingleton<OrangeDataManager>.Instance.SKIN_TABLE_DICT[netCharacterInfo.Skin].s_ICON);
				if ((bool)leftPlayerStarsRoot && (bool)rightPlayerStarsRoot && leftPlayerStarImgs.Length == 5 && rightPlayerStarImgs.Length == 5)
				{
					Image[] array = ((selectIdx == 0) ? leftPlayerStarImgs : rightPlayerStarImgs);
					for (int j = 0; j < 5; j++)
					{
						array[j].gameObject.SetActive(j < netCharacterInfo.Star);
					}
				}
				Debug.Log(empty);
				MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(string.Format(AssetBundleScriptableObject.Instance.m_dragonbones_chdb, empty), empty + "_db", delegate(GameObject obj)
				{
					if (obj == null)
					{
						loadingCount--;
					}
					else
					{
						Transform transform = rootCharacterImg[selectIdx];
						UnityEngine.Object.Instantiate(obj).transform.SetParent(transform, false);
						ch_moveTo[selectIdx] = transform.transform.localPosition.x;
						transform.transform.localPosition = new Vector3(transform.transform.localPosition.x + ch_moveFrom[selectIdx], transform.transform.localPosition.y, transform.transform.localPosition.z);
						LeanTween.moveLocalX(transform.gameObject, ch_moveTo[selectIdx], 0.3f).setOnComplete((Action)delegate
						{
							loadingCount--;
						});
					}
				});
			}
			SetBottomInfo(ref listMemberInfo);
			return;
		}
		for (int k = 0; k < listMemberInfo.Count; k++)
		{
			Transform parent = ((listMemberInfo[k].Team == 0) ? layoutGroupLeft.transform : layoutGroupRight.transform);
			PvpLoadingUIUnit pvpLoadingUIUnit = UnityEngine.Object.Instantiate(unitPlayerInfo);
			pvpLoadingUIUnit.Setup(k, delegate
			{
				loadingCount--;
			});
			pvpLoadingUIUnit.transform.SetParent(parent, false);
		}
	}

	private void SetBottomInfo(ref List<MemberInfo> listMemberInfo)
	{
		if (listMemberInfo.Count >= 2)
		{
			for (int i = 0; i < 2; i++)
			{
				MemberInfo memberInfo = listMemberInfo[i];
				textPlayerName[memberInfo.Team].text = memberInfo.Nickname;
				playerProgress[memberInfo.Team].Setup(memberInfo.PlayerId);
				int num = memberInfo.Team * 2;
				SetWeapon(memberInfo.netSealBattleSettingInfo.MainWeaponInfo, ref playerWeapons[num], memberInfo.netSealBattleSettingInfo.WeaponExpertList);
				SetWeapon(memberInfo.netSealBattleSettingInfo.SubWeaponInfo, ref playerWeapons[num + 1], memberInfo.netSealBattleSettingInfo.WeaponExpertList);
				SetPlayerSignIcon(PlayerSignRoot[memberInfo.Team], memberInfo.PlayerId);
			}
			bottomCanvas.enabled = true;
		}
	}

	private void SetWeapon(NetWeaponInfo weaponInfo, ref CommonIconBase weaponIcon, List<NetWeaponExpertInfo> tWeaponExpertList)
	{
		if (weaponInfo.WeaponID != 0)
		{
			int num = 0;
			foreach (NetWeaponExpertInfo tWeaponExpert in tWeaponExpertList)
			{
				if (tWeaponExpert.WeaponID == weaponInfo.WeaponID)
				{
					num += tWeaponExpert.ExpertLevel;
				}
			}
			WEAPON_TABLE wEAPON_TABLE = ManagedSingleton<OrangeDataManager>.Instance.WEAPON_TABLE_DICT[weaponInfo.WeaponID];
			weaponIcon.Setup(0, AssetBundleScriptableObject.Instance.m_iconWeapon, wEAPON_TABLE.s_ICON);
			weaponIcon.SetOtherInfo(weaponInfo, CommonIconBase.WeaponEquipType.UnEquip, true, num, false);
			weaponIcon.EnableLevel(false);
			weaponIcon.EnableWeaponRank(false);
		}
		else
		{
			weaponIcon.gameObject.SetActive(false);
		}
	}

	private IEnumerator OnDisplayTween()
	{
		while (loadingCount > 0)
		{
			yield return CoroutineDefine._waitForEndOfFrame;
		}
		DisplayVsEft();
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_RESULT01);
	}

	private void DisplayVsEft()
	{
		LeanTween.moveLocalY(ImgVs[0].gameObject, 0f, 0.5f);
		LeanTween.moveLocalY(ImgVs[1].gameObject, 0f, 0.5f).setOnComplete((Action)delegate
		{
			whiteMask.color = Color.white;
			ImgVs[1].transform.localScale = new Vector3(2f, 2f, 1f);
			ImgVs[1].color = new Color(1f, 1f, 1f, 0.6f);
			LeanTween.color(whiteMask.rectTransform, Color.clear, 1f);
			LeanTween.color(ImgVs[1].rectTransform, Color.clear, 1f);
			LeanTween.scale(ImgVs[1].gameObject, new Vector3(3.5f, 3.5f, 1f), 1f).setOnComplete((Action)delegate
			{
				IsComplete = true;
			});
			MonoBehaviourSingleton<AudioManager>.Instance.PlayBGM("BGM01", 43);
		});
	}

	private void OnDestroy()
	{
		Resources.UnloadUnusedAssets();
		GC.Collect();
	}
}
