using System;
using System.Collections.Generic;
using System.Linq;
using CallbackDefs;
using NaughtyAttributes;
using OrangeAudio;
using UnityEngine;
using enums;

public class GachaSceneController : MonoBehaviour
{
	[SerializeField]
	private GachaCapsule gachaCapsule;

	[SerializeField]
	private GachaReward rewardCharacter;

	[SerializeField]
	private GachaReward rewardWeapon;

	[SerializeField]
	private GachaDoorEvent gachaDoorEvent;

	[SerializeField]
	private GachaCMEvent GachaCMEvent;

	[BoxGroup("Sound")]
	[SerializeField]
	private SystemSE m_backSE;

	private int nowIdx = -1;

	private NetRewardsEntity rewardsEntity;

	private List<GachaRewardVisualInfo> rewardList = new List<GachaRewardVisualInfo>();

	private GachaRewardVisualInfo netGachaRewardInfo;

	private int[] rewardTypes;

	private string bundlePath = string.Empty;

	private string assetPath = string.Empty;

	private int rare;

	private int bestRare;

	private bool isDoorSpChange;

	private bool isCapsuleSpChange;

	private int lastNewOne = -1;

	private readonly int SpChangeRate = 10;

	private bool isCeiling;

	private bool skipAll;

	private List<GachaRareVisualInfo> listGachaRareVisualInfo = new List<GachaRareVisualInfo>();

	public void Init(NetRewardsEntity p_rewardsEntity, bool p_isCeiling = false)
	{
		skipAll = false;
		nowIdx = -1;
		isCeiling = p_isCeiling;
		rewardsEntity = p_rewardsEntity;
		List<NetCharacterInfo> list = new List<NetCharacterInfo>(rewardsEntity.CharacterList);
		List<NetWeaponInfo> list2 = new List<NetWeaponInfo>(rewardsEntity.WeaponList);
		foreach (NetRewardInfo rewardInfo in rewardsEntity.RewardList)
		{
			if (rewardInfo.IsGachaBonus == 1)
			{
				continue;
			}
			bool isNew = false;
			switch ((RewardType)rewardInfo.RewardType)
			{
			case RewardType.Character:
			{
				NetCharacterInfo netCharacterInfo = list.FirstOrDefault((NetCharacterInfo x) => x.CharacterID == rewardInfo.RewardID);
				if (netCharacterInfo != null)
				{
					list.Remove(netCharacterInfo);
					isNew = true;
				}
				break;
			}
			case RewardType.Weapon:
			{
				NetWeaponInfo netWeaponInfo = list2.FirstOrDefault((NetWeaponInfo x) => x.WeaponID == rewardInfo.RewardID);
				if (netWeaponInfo != null)
				{
					list2.Remove(netWeaponInfo);
					isNew = true;
				}
				break;
			}
			}
			GachaRewardVisualInfo item = new GachaRewardVisualInfo
			{
				NetRewardInfo = rewardInfo,
				IsNew = isNew
			};
			rewardList.Add(item);
		}
		SetBestRare();
		gachaDoorEvent.SetDoorInfo(bestRare, isDoorSpChange, isCapsuleSpChange);
		MonoBehaviourSingleton<UIManager>.Instance.CloseLoadingUI(delegate
		{
			gachaDoorEvent.GachaSceneController = this;
			gachaDoorEvent.PlayAnim();
		});
		MonoBehaviourSingleton<AudioManager>.Instance.PlayBGM("BGM01", 38);
	}

	public void OnDoorAnimStopped()
	{
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI<GachaSkipUI>("UI_GachaSkip", delegate
		{
			PlayNext();
		});
	}

	private void PlayNext()
	{
		nowIdx++;
		if (nowIdx >= rewardList.Count)
		{
			PlayEnd();
			return;
		}
		if (skipAll)
		{
			LoopDisplayNewReward(nowIdx, PlayEnd);
			return;
		}
		netGachaRewardInfo = rewardList[nowIdx];
		float num = 1f;
		if (listGachaRareVisualInfo[nowIdx].IsSpChange)
		{
			num += 5f;
		}
		bundlePath = string.Empty;
		assetPath = string.Empty;
		rare = 0;
		rewardTypes = MonoBehaviourSingleton<OrangeGameManager>.Instance.GetRewardSpritePath(netGachaRewardInfo.NetRewardInfo, ref bundlePath, ref assetPath, ref rare);
		RewardType rewardType = (RewardType)netGachaRewardInfo.NetRewardInfo.RewardType;
		if (rewardType == RewardType.Weapon || rewardType != RewardType.Character)
		{
			rewardWeapon.ShowResult(rare, num);
		}
		else
		{
			rewardCharacter.ShowResult(rare, num);
		}
		gachaCapsule.Init(listGachaRareVisualInfo[nowIdx], ShowResult);
	}

	private void ShowResult()
	{
		GachaReward gachaReward = null;
		int num = rewardTypes[0];
		if (num == 2 || num != 3)
		{
			gachaReward = rewardWeapon;
		}
		else
		{
			gachaReward = rewardCharacter;
		}
		gachaReward.SetVisable();
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_ShinyEffect", delegate(ShinyEffectUI ui)
		{
			ui.SetWhite(delegate
			{
				MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_GachaResult", delegate(GachaResultUI ui2)
				{
					ui2.closeCB = (Callback)Delegate.Combine(ui2.closeCB, new Callback(PlayNext));
					ui2.Setup(netGachaRewardInfo.NetRewardInfo, netGachaRewardInfo.IsNew);
					ui2.isLastOne = nowIdx == rewardList.Count - 1;
					gachaReward.SetVisable(0);
					ui.OnClickCloseBtn();
				});
			}, 0.2f, 0f, LeanTweenType.easeInCubic);
		});
	}

	private void PlayEnd()
	{
		ManagedSingleton<StageHelper>.Instance.nStageEndGoUI = StageHelper.STAGE_END_GO.GACHA;
		ManagedSingleton<StageHelper>.Instance.nStageEndParam = new object[2] { rewardsEntity, isCeiling };
		MonoBehaviourSingleton<OrangeSceneManager>.Instance.ChangeScene("hometop", OrangeSceneManager.LoadingType.TIP, null, false);
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(m_backSE);
	}

	private void SkipGacha(int count)
	{
		if (gachaCapsule.IsBlock)
		{
			if (count < 0)
			{
				skipAll = true;
			}
			return;
		}
		gachaCapsule.IsBlock = true;
		MonoBehaviourSingleton<AudioManager>.Instance.Stop(AudioChannelType.Sound);
		if (count < 0)
		{
			skipAll = true;
			CancelInvoke();
			gachaCapsule.SkipEft(false);
			LoopDisplayNewReward(nowIdx, PlayEnd);
			for (int i = 0; i < rewardList.Count; i++)
			{
				if (rewardList[i].IsNew)
				{
					lastNewOne = i;
				}
			}
			if (lastNewOne != -1)
			{
				MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_GLOW03);
			}
		}
		else
		{
			gachaCapsule.SkipEft();
			if (nowIdx < rewardList.Count)
			{
				MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_CURSOR06);
			}
		}
	}

	private void LoopDisplayNewReward(int idx, Callback p_completeCB)
	{
		if (rewardList.Count <= idx)
		{
			p_completeCB.CheckTargetToInvoke();
			return;
		}
		if (!rewardList[idx].IsNew)
		{
			idx++;
			LoopDisplayNewReward(idx, p_completeCB);
			return;
		}
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_ShinyEffect", delegate(ShinyEffectUI ui)
		{
			ui.SetWhite(delegate
			{
				MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_GachaResult", delegate(GachaResultUI ui2)
				{
					ui2.closeCB = (Callback)Delegate.Combine(ui2.closeCB, (Callback)delegate
					{
						idx++;
						LoopDisplayNewReward(idx, p_completeCB);
					});
					ui2.Setup(rewardList[idx].NetRewardInfo, true);
					ui2.isLastOne = idx == lastNewOne;
					ui.OnClickCloseBtn();
				});
			}, 0.2f, 0f, LeanTweenType.easeInCubic);
		});
	}

	private void OnEnable()
	{
		Singleton<GenericEventManager>.Instance.AttachEvent<int>(EventManager.ID.GACHA_SKIP, SkipGacha);
	}

	private void OnDisable()
	{
		Singleton<GenericEventManager>.Instance.DetachEvent<int>(EventManager.ID.GACHA_SKIP, SkipGacha);
	}

	private void OnDestroy()
	{
		rewardList.Clear();
		rewardList = null;
	}

	public void SetDoorOpen()
	{
		GachaCMEvent.PlayCM();
	}

	public void SetBestRare()
	{
		int num = 5;
		bestRare = 1;
		int num2 = 0;
		foreach (GachaRewardVisualInfo reward in rewardList)
		{
			int num3 = 1;
			bool flag = false;
			switch ((RewardType)reward.NetRewardInfo.RewardType)
			{
			case RewardType.Character:
				num3 = ManagedSingleton<OrangeDataManager>.Instance.CHARACTER_TABLE_DICT[reward.NetRewardInfo.RewardID].n_RARITY;
				break;
			case RewardType.Weapon:
				num3 = ManagedSingleton<OrangeDataManager>.Instance.WEAPON_TABLE_DICT[reward.NetRewardInfo.RewardID].n_RARITY;
				break;
			}
			if (num3 > bestRare)
			{
				bestRare = num3;
				if (bestRare == num)
				{
					isDoorSpChange = SpChangeRate >= UnityEngine.Random.Range(0, 100);
					flag = !isDoorSpChange && SpChangeRate >= UnityEngine.Random.Range(0, 100);
					if (flag)
					{
						bestRare = 4;
					}
				}
			}
			if (!isCapsuleSpChange)
			{
				isCapsuleSpChange = flag;
			}
			GachaRareVisualInfo item = new GachaRareVisualInfo
			{
				Idx = num2,
				IsNew = true,
				IsSpChange = flag,
				Rare = num3
			};
			listGachaRareVisualInfo.Add(item);
			num2++;
		}
		gachaCapsule.SetRareFx(ref listGachaRareVisualInfo);
	}
}
