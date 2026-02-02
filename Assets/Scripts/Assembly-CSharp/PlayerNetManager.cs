using System;
using System.Collections.Generic;
using System.Linq;
using Better;
using CallbackDefs;
using OrangeApi;
using StageLib;
using UnityEngine;
using enums;

internal class PlayerNetManager : ManagedSingleton<PlayerNetManager>
{
	public AccountInfo AccountInfo = new AccountInfo();

	public PlayerInfo playerInfo = new PlayerInfo();

	public System.Collections.Generic.Dictionary<int, CharacterInfo> dicCharacter = new Better.Dictionary<int, CharacterInfo>();

	public System.Collections.Generic.Dictionary<int, WeaponInfo> dicWeapon = new Better.Dictionary<int, WeaponInfo>();

	public System.Collections.Generic.Dictionary<int, EquipInfo> dicEquip = new Better.Dictionary<int, EquipInfo>();

	public System.Collections.Generic.Dictionary<EquipPartType, EquipEnhanceInfo> dicEquipEnhance = new Better.Dictionary<EquipPartType, EquipEnhanceInfo>();

	public System.Collections.Generic.Dictionary<int, StageInfo> dicStage = new Better.Dictionary<int, StageInfo>();

	public MultiMap<int, NetTowerBossInfo> mmapTowerBossInfoMap = new MultiMap<int, NetTowerBossInfo>();

	public System.Collections.Generic.Dictionary<int, ItemInfo> dicItem = new Better.Dictionary<int, ItemInfo>();

	public System.Collections.Generic.Dictionary<int, ChipInfo> dicChip = new Better.Dictionary<int, ChipInfo>();

	public GalleryInfo galleryInfo = new GalleryInfo();

	public System.Collections.Generic.Dictionary<int, GachaInfo> dicGacha = new Better.Dictionary<int, GachaInfo>();

	public System.Collections.Generic.Dictionary<int, int> dicGachaGuaranteeRecord = new Better.Dictionary<int, int>();

	public MultiMap<MultiPlayGachaType, NetMultiPlayerGachaInfo> mmapMultiPlayGachaRecord = new MultiMap<MultiPlayGachaType, NetMultiPlayerGachaInfo>();

	public System.Collections.Generic.Dictionary<int, ShopInfo> dicShop = new Better.Dictionary<int, ShopInfo>();

	public System.Collections.Generic.Dictionary<int, FinalStrikeInfo> dicFinalStrike = new Better.Dictionary<int, FinalStrikeInfo>();

	public System.Collections.Generic.Dictionary<int, MailInfo> dicMail = new System.Collections.Generic.Dictionary<int, MailInfo>();

	public System.Collections.Generic.Dictionary<int, MailInfo> dicReservedMail = new System.Collections.Generic.Dictionary<int, MailInfo>();

	public System.Collections.Generic.Dictionary<int, System.Collections.Generic.Dictionary<Language, SystemContext>> dicSystemContext = new System.Collections.Generic.Dictionary<int, System.Collections.Generic.Dictionary<Language, SystemContext>>();

	public System.Collections.Generic.Dictionary<int, MissionInfo> dicMission = new System.Collections.Generic.Dictionary<int, MissionInfo>();

	public System.Collections.Generic.Dictionary<int, MissionProgress> dicMissionProgress = new System.Collections.Generic.Dictionary<int, MissionProgress>();

	public ResearchInfo researchInfo = new ResearchInfo();

	public List<int> TutorialList = new List<int>();

	public System.Collections.Generic.Dictionary<int, ChargeInfo> dicCharge = new System.Collections.Generic.Dictionary<int, ChargeInfo>();

	public GuildInfo guildInfo = new GuildInfo();

	public System.Collections.Generic.Dictionary<int, ServiceInfo> dicService = new System.Collections.Generic.Dictionary<int, ServiceInfo>();

	public MultiMap<int, NetCharacterSkinInfo> mmapSkin = new MultiMap<int, NetCharacterSkinInfo>();

	public IAPReceiptInfo IAPReceipt = new IAPReceiptInfo();

	public System.Collections.Generic.Dictionary<int, EventExInfo> dicEventEx = new System.Collections.Generic.Dictionary<int, EventExInfo>();

	public System.Collections.Generic.Dictionary<int, GachaEventExInfo> dicGachaEventEx = new System.Collections.Generic.Dictionary<int, GachaEventExInfo>();

	public System.Collections.Generic.Dictionary<int, GachaExInfo> dicGachaEx = new System.Collections.Generic.Dictionary<int, GachaExInfo>();

	public System.Collections.Generic.Dictionary<int, ShopExInfo> dicShopEx = new System.Collections.Generic.Dictionary<int, ShopExInfo>();

	public System.Collections.Generic.Dictionary<int, ItemExInfo> dicItemEx = new System.Collections.Generic.Dictionary<int, ItemExInfo>();

	public System.Collections.Generic.Dictionary<int, BoxGachaStatus> dicBoxGachaStatus = new System.Collections.Generic.Dictionary<int, BoxGachaStatus>();

	public System.Collections.Generic.Dictionary<int, BenchInfo> dicBenchWeaponInfo = new System.Collections.Generic.Dictionary<int, BenchInfo>();

	public System.Collections.Generic.Dictionary<int, BossRushInfo> dicBossRushInfo = new System.Collections.Generic.Dictionary<int, BossRushInfo>();

	public System.Collections.Generic.Dictionary<int, CardInfo> dicCard = new Better.Dictionary<int, CardInfo>();

	public System.Collections.Generic.Dictionary<int, System.Collections.Generic.Dictionary<int, NetCharacterCardSlotInfo>> dicCharacterCardSlotInfo = new System.Collections.Generic.Dictionary<int, System.Collections.Generic.Dictionary<int, NetCharacterCardSlotInfo>>();

	public System.Collections.Generic.Dictionary<int, BannerExInfo> dicBannerEx = new System.Collections.Generic.Dictionary<int, BannerExInfo>();

	private string rawExData = string.Empty;

	public List<string> listAllowExTable = new List<string>();

	public System.Collections.Generic.Dictionary<int, System.Collections.Generic.Dictionary<int, NetCardDeployInfo>> dicCardDeployInfo = new System.Collections.Generic.Dictionary<int, System.Collections.Generic.Dictionary<int, NetCardDeployInfo>>();

	public System.Collections.Generic.Dictionary<int, string> dicCardDeployNameInfo = new System.Collections.Generic.Dictionary<int, string>();

	public int nRaidBossSocre;

	public int nTWSuppressScore;

	public int nTWLightningScore;

	public int nTWCrusadeScore;

	public int nTotalFSLv;

	public bool bIsAnyFinalStrikeCanStrengthen;

	private GameServerService WebService;

	public string RawExtData
	{
		get
		{
			return rawExData;
		}
	}

	public override void Initialize()
	{
	}

	public override void Reset()
	{
		base.Reset();
		playerInfo = new PlayerInfo();
		dicCharacter.Clear();
		dicWeapon.Clear();
		dicEquip.Clear();
		dicEquipEnhance.Clear();
		dicStage.Clear();
		mmapTowerBossInfoMap.Clear();
		dicItem.Clear();
		dicChip.Clear();
		galleryInfo = new GalleryInfo();
		dicGacha.Clear();
		dicGachaGuaranteeRecord.Clear();
		mmapMultiPlayGachaRecord.Clear();
		dicShop.Clear();
		dicFinalStrike.Clear();
		dicMail.Clear();
		dicReservedMail.Clear();
		dicSystemContext.Clear();
		dicMission.Clear();
		dicMissionProgress.Clear();
		researchInfo = new ResearchInfo();
		TutorialList.Clear();
		dicCharge.Clear();
		guildInfo = new GuildInfo();
		dicService.Clear();
		mmapSkin.Clear();
		IAPReceipt = new IAPReceiptInfo();
		dicGachaEx.Clear();
		dicGachaEventEx.Clear();
		dicEventEx.Clear();
		dicShopEx.Clear();
		dicItemEx.Clear();
		dicBoxGachaStatus.Clear();
		dicBenchWeaponInfo.Clear();
		dicBossRushInfo.Clear();
		dicBannerEx.Clear();
		listAllowExTable.Clear();
		rawExData = string.Empty;
		dicCard.Clear();
		dicCharacterCardSlotInfo.Clear();
		dicCardDeployInfo.Clear();
		dicCardDeployNameInfo.Clear();
	}

	public override void Dispose()
	{
	}

	public void CrusadeRetrieveCrusadeInfo(Action<RetrieveCrusadeInfoRes> p_cb = null)
	{
		WebService.SendRequest(new RetrieveCrusadeInfoReq(), p_cb);
	}

	public void CrusadeCheckCrusadeInfo(Action<CheckCrusadeInfoRes> p_cb = null)
	{
		WebService.SendRequest(new CheckCrusadeInfoReq(), p_cb);
	}

	public void CrusadeStart(int stageId, Action<CrusadeStartRes> p_cb = null)
	{
		WebService.SendRequest(new CrusadeStartReq
		{
			StageID = stageId
		}, p_cb);
	}

	public void CrusadeEnd(int stageId, sbyte result, sbyte star, int score, int power, int duration, short resolutionW, short resolutionH, Action<CrusadeEndRes> p_cb = null)
	{
		WebService.SendRequest(new CrusadeEndReq
		{
			StageID = stageId,
			Result = result,
			Star = star,
			Score = score,
			Power = power,
			Duration = duration,
			ResolutionW = resolutionW,
			ResolutionH = resolutionH
		}, delegate(CrusadeEndRes res)
		{
			UpdateStageInfo(res.StageInfo);
			UpdateWeaponInfoList(res.WeaponInfoList);
			UpdateItemInfoList(res.ItemList);
			UpdateCharacterInfoList(res.CharacterInfoList);
			UpdatePlayerInfo(res.PlayerInfo);
			if (res.RewardEntities != null)
			{
				ComposeRewardEntities(res.RewardEntities);
			}
			Action<CrusadeEndRes> action = p_cb;
			if (action != null)
			{
				action(res);
			}
		});
	}

	public void CrusadeLimitResetReq(Action<CrusadeLimitResetRes> p_cb = null)
	{
		WebService.SendRequest(new CrusadeLimitResetReq(), delegate(CrusadeLimitResetRes res)
		{
			UpdateItemInfoList(res.ItemList);
			UpdateChargeInfo(res.ChargeInfo);
			Action<CrusadeLimitResetRes> action = p_cb;
			if (action != null)
			{
				action(res);
			}
		});
	}

	public void CrusadeRetrievePersonalEventRankingReq(int eventId, Action<RetrievePersonalCrusadeEventRankingRes> p_cb = null)
	{
		WebService.SendRequest(new RetrievePersonalCrusadeEventRankingReq
		{
			EventID = eventId
		}, p_cb);
	}

	public void CrusadeRetrieveEventRankingReq(int eventId, int rankingStart, int rankingEnd, Action<RetrieveCrusadeEventRankingRes> p_cb = null)
	{
		WebService.SendRequest(new RetrieveCrusadeEventRankingReq
		{
			EventID = eventId,
			StartRanking = rankingStart,
			StopRanking = rankingEnd
		}, p_cb);
	}

	public void SetupWebService()
	{
		WebService = MonoBehaviourSingleton<GameServerService>.Instance;
	}

	public void LoginToGameService(string serverUrl, enums.DeviceType deviceType, Action<LoginToServerRes> p_cb = null)
	{
		WebService.SeqID = 0;
		WebService.ServerUrl = serverUrl;
		rawExData = string.Empty;
		int num = ((deviceType == enums.DeviceType.IOS) ? ApiCommon.ProtocolVersionIOS : ApiCommon.ProtocolVersionAndroid);
		LoginToServerReq req = new LoginToServerReq
		{
			AppVersion = ManagedSingleton<ServerConfig>.Instance.APP_VERSION,
			Account = AccountInfo.ID,
			Password = AccountInfo.Secret,
			SourceType = (sbyte)AccountInfo.SourceType,
			DeviceType = (sbyte)deviceType,
			ProtocolVersion = (short)num,
			BirthTime = MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.Birth,
			Region = MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.Locate,
			HashKey = CapUtility.EncryptStringToBase64(DeviceHelper.GetDeviceName(), AccountInfo.ID),
			AuthKey = DeviceHelper.GetActivityName()
		};
		WebService.SendRequest(req, delegate(LoginToServerRes res)
		{
			WebService.SeqID = 1;
			WebService.ServiceToken = res.Token;
			WebService.PlayerIdentify = res.PlayerID;
			WebService.ServiceZoneID = res.ServerID;
			listAllowExTable.Clear();
			listAllowExTable.AddRange(res.ValidTableList);
			ManagedSingleton<InputStorage>.Instance.AddInputData(WebService.PlayerIdentify);
			if (p_cb != null)
			{
				p_cb(res);
			}
		});
	}

	public void CreateNewPlayer(string nickName, Callback p_cb = null)
	{
		WebService.SendRequest(new CreatePlayerReq
		{
			Nickname = nickName
		}, delegate(CreatePlayerRes res)
		{
			if (res.Code == 10100)
			{
				p_cb.CheckTargetToInvoke();
			}
		});
	}

	public void DeleteAccount(Action<DeleteAccountRes> p_cb = null)
	{
		WebService.SendRequest(new DeleteAccountReq
		{
			Account = AccountInfo.ID
		}, delegate(DeleteAccountRes res)
		{
			p_cb.CheckTargetToInvoke(res);
		});
	}

	public void RetrievePlayerInfo(Action<RetrievePlayerInfoRes> p_cb = null)
	{
		WebService.SendRequest(new RetrievePlayerInfoReq(), delegate(RetrievePlayerInfoRes res)
		{
			if (res.PlayerInfo != null)
			{
				playerInfo.netPlayerInfo = res.PlayerInfo;
			}
			if (p_cb != null)
			{
				p_cb(res);
			}
		});
	}

	public void RetrieveResetTimeReq(Action<RetrieveResetTimeRes> p_cb = null, sbyte p_timeZone = 0)
	{
		WebService.SendRequest(new RetrieveResetTimeReq
		{
			TimeZone = p_timeZone
		}, delegate(RetrieveResetTimeRes res)
		{
			p_cb.CheckTargetToInvoke(res);
		});
	}

	public void LoginRetrieveCollector1Req(NetDeviceInfo deviceInfo, Callback p_cb = null)
	{
		WebService.SendRequest(new LoginRetrieveCollector1Req
		{
			HasDeviceInfo = (sbyte)((deviceInfo != null) ? 1 : 0),
			DeviceInfo = deviceInfo
		}, delegate(LoginRetrieveCollector1Res res)
		{
			dicItem.Clear();
			foreach (NetItemInfo item in res.ItemList)
			{
				dicItem.Value(item.ItemID).netItemInfo = item;
			}
			dicCharacter.Clear();
			foreach (NetCharacterInfo character in res.CharacterList)
			{
				dicCharacter.Value(character.CharacterID).netInfo = character;
			}
			foreach (NetCharacterSkillInfo characterSkill in res.CharacterSkillList)
			{
				dicCharacter.Value(characterSkill.CharacterID).netSkillDic.ContainsAdd((CharacterSkillSlot)characterSkill.Slot, characterSkill);
			}
			foreach (NetCharacterDNAInfo characterDNAInfo in res.CharacterDNAInfoList)
			{
				dicCharacter.Value(characterDNAInfo.CharacterID).netDNAInfoDic.ContainsAdd(characterDNAInfo.SlotID, characterDNAInfo);
			}
			foreach (NetCharacterDNALinkInfo characterDNALinkInfo in res.CharacterDNALinkInfoList)
			{
				dicCharacter.Value(characterDNALinkInfo.CharacterID).netDNALinkInfo = characterDNALinkInfo;
			}
			dicCard.Clear();
			foreach (NetCardInfo card in res.CardList)
			{
				dicCard.Value(card.CardSeqID).netCardInfo = card;
			}
			dicCharacterCardSlotInfo.Clear();
			foreach (NetCharacterCardSlotInfo characterCardSlotInfo in res.CharacterCardSlotInfoList)
			{
				dicCharacterCardSlotInfo.Value(characterCardSlotInfo.CharacterID).Value(characterCardSlotInfo.CharacterCardSlot).CardSeqID = characterCardSlotInfo.CardSeqID;
				dicCharacterCardSlotInfo.Value(characterCardSlotInfo.CharacterID).Value(characterCardSlotInfo.CharacterCardSlot).CharacterCardSlot = characterCardSlotInfo.CharacterCardSlot;
				dicCharacterCardSlotInfo.Value(characterCardSlotInfo.CharacterID).Value(characterCardSlotInfo.CharacterCardSlot).CharacterID = characterCardSlotInfo.CharacterID;
			}
			dicCardDeployInfo.Clear();
			foreach (NetCardDeployInfo cardDeployInfo in res.CardDeployInfoList)
			{
				dicCardDeployInfo.Value(cardDeployInfo.DeployId).Value(cardDeployInfo.CardSlot).CardSeqID = cardDeployInfo.CardSeqID;
				dicCardDeployInfo.Value(cardDeployInfo.DeployId).Value(cardDeployInfo.CardSlot).CardSlot = cardDeployInfo.CardSlot;
				dicCardDeployInfo.Value(cardDeployInfo.DeployId).Value(cardDeployInfo.CardSlot).DeployId = cardDeployInfo.DeployId;
			}
			dicCardDeployNameInfo.Clear();
			foreach (NetCardDeployNameInfo cardDeployNameInfo in res.CardDeployNameInfoList)
			{
				if (dicCardDeployNameInfo.ContainsKey(cardDeployNameInfo.DeployId))
				{
					dicCardDeployNameInfo[cardDeployNameInfo.DeployId] = cardDeployNameInfo.SlotName;
				}
				else
				{
					dicCardDeployNameInfo.Add(cardDeployNameInfo.DeployId, cardDeployNameInfo.SlotName);
				}
			}
			mmapSkin.Clear();
			foreach (NetCharacterSkinInfo characterSkin in res.CharacterSkinList)
			{
				mmapSkin.Add(characterSkin.CharacterID, characterSkin);
				dicCharacter.Value(characterSkin.CharacterID).netSkinList.Add(characterSkin.SkinId);
			}
			rawExData += res.RawExtData;
			p_cb.CheckTargetToInvoke();
		});
	}

	public void LoginRetrieveCollector2Req(Callback p_cb = null)
	{
		WebService.SendRequest(new LoginRetrieveCollector2Req(), delegate(LoginRetrieveCollector2Res res)
		{
			dicWeapon.Clear();
			foreach (NetWeaponInfo weapon in res.WeaponList)
			{
				dicWeapon.Value(weapon.WeaponID).netInfo = weapon;
			}
			foreach (NetWeaponExpertInfo weaponExpert in res.WeaponExpertList)
			{
				dicWeapon.Value(weaponExpert.WeaponID).AddNetWeaponExpertInfo(weaponExpert);
			}
			foreach (NetWeaponSkillInfo weaponSkill in res.WeaponSkillList)
			{
				dicWeapon.Value(weaponSkill.WeaponID).AddNetWeaponSkillInfo(weaponSkill);
			}
			foreach (NetWeaponDiVESkillInfo weaponDiVESkill in res.WeaponDiVESkillList)
			{
				dicWeapon.Value(weaponDiVESkill.WeaponID).netDiveSkillInfo = weaponDiVESkill;
			}
			dicEquip.Clear();
			foreach (NetEquipmentInfo equipment in res.EquipmentList)
			{
				dicEquip.Value(equipment.EquipmentID).netEquipmentInfo = equipment;
			}
			dicEquipEnhance.Clear();
			foreach (NetPlayerEquipInfo playerEquip in res.PlayerEquipList)
			{
				dicEquipEnhance.Value((EquipPartType)playerEquip.Slot).netPlayerEquipInfo = playerEquip;
			}
			dicFinalStrike.Clear();
			foreach (NetFinalStrikeInfo fS in res.FSList)
			{
				dicFinalStrike.Value(fS.FinalStrikeID).netFinalStrikeInfo = fS;
			}
			dicChip.Clear();
			foreach (NetChipInfo chip in res.ChipList)
			{
				dicChip.Value(chip.ChipID).netChipInfo = chip;
			}
			dicCharge.Clear();
			foreach (NetChargeInfo chargeInfo in res.ChargeInfoList)
			{
				dicCharge.Value(chargeInfo.ChargeType).netChargeInfo = chargeInfo;
			}
			dicService.Clear();
			foreach (NetPlayerServiceInfo playerServiceInfo in res.PlayerServiceInfoList)
			{
				dicService.Value(playerServiceInfo.ServiceID).netServiceInfo = playerServiceInfo;
			}
			dicBenchWeaponInfo.Clear();
			foreach (NetBenchInfo benchWeaponInfo in res.BenchWeaponInfoList)
			{
				dicBenchWeaponInfo.Value(benchWeaponInfo.BenchSlot).netBenchInfo = benchWeaponInfo;
			}
			TutorialList.Clear();
			TutorialList = res.TutorialList;
			TutorialList.Sort();
			rawExData += res.RawExtData;
			p_cb.CheckTargetToInvoke();
		});
	}

	public void LoginRetrieveCollector3Req(Callback p_cb = null)
	{
		WebService.SendRequest(new LoginRetrieveCollector3Req(), delegate(LoginRetrieveCollector3Res res)
		{
			dicStage.Clear();
			foreach (NetStageInfo stageClear in res.StageClearList)
			{
				dicStage.Value(stageClear.StageID).netStageInfo = stageClear;
			}
			galleryInfo = new GalleryInfo();
			galleryInfo.GalleryExpList = res.GalleryExpList;
			galleryInfo.GalleryList = res.GalleryList;
			galleryInfo.GalleryCardList = res.GalleryCardList;
			dicMail.Clear();
			if (res.MailListCount > 0)
			{
				dicMail.Add(0, new MailInfo());
			}
			dicReservedMail.Clear();
			foreach (NetMailInfo reservedMail in res.ReservedMailList)
			{
				dicReservedMail.Value(reservedMail.MailID).netMailInfo = reservedMail;
			}
			dicSystemContext.Clear();
			foreach (NetExtraSystemContext systemContext in res.SystemContextList)
			{
				dicSystemContext.Value(systemContext.SystemTextID).Value((Language)systemContext.Language).netSystemContext = systemContext;
			}
			dicMission.Clear();
			foreach (NetMissionInfo completedMission in res.CompletedMissionList)
			{
				dicMission.Value(completedMission.MissionID).netMissionInfo = completedMission;
			}
			researchInfo = new ResearchInfo();
			researchInfo.listFreeResearch = res.FreeResearchInfoList;
			researchInfo.listResearchRecord = res.ResearchRecordList;
			researchInfo.dicResearch.Clear();
			foreach (NetResearchInfo researchInfo in res.ResearchInfoList)
			{
				this.researchInfo.dicResearch.ContainsAdd(researchInfo.Slot, researchInfo);
			}
			if (res.StageSecretList != null)
			{
				foreach (NetStageSecretInfo stageSecret in res.StageSecretList)
				{
					dicStage.Value(stageSecret.StageID).AddStageSecretToList(stageSecret.SecretID);
				}
			}
			mmapTowerBossInfoMap.Clear();
			if (res.TowerBossInfoList != null)
			{
				foreach (NetTowerBossInfo towerBossInfo in res.TowerBossInfoList)
				{
					mmapTowerBossInfoMap.Add(towerBossInfo.TowerStageID, towerBossInfo);
				}
			}
			rawExData += res.RawExtData;
			p_cb.CheckTargetToInvoke();
		});
	}

	public void LoginRetrieveCollector4Req(string IDFA, string pushToken, Callback p_cb = null)
	{
		WebService.SendRequest(new LoginRetrieveCollector4Req
		{
			IDFA = IDFA,
			PushToken = pushToken
		}, delegate(LoginRetrieveCollector4Res res)
		{
			dicGacha.Clear();
			foreach (NetGachaEventRecord gachaEventRecord in res.GachaEventRecordList)
			{
				dicGacha.Value(gachaEventRecord.GachaEventID).netGachaEventRecord = gachaEventRecord;
			}
			dicGachaGuaranteeRecord.Clear();
			foreach (NetGachaGuaranteeRecord gachaGuaranteeRecord in res.GachaGuaranteeRecordList)
			{
				dicGachaGuaranteeRecord.ContainsAdd(gachaGuaranteeRecord.GroupID, gachaGuaranteeRecord.Value);
			}
			mmapMultiPlayGachaRecord.Clear();
			foreach (NetMultiPlayerGachaInfo multiPlayGachaRecord in res.MultiPlayGachaRecordList)
			{
				mmapMultiPlayGachaRecord.Add((MultiPlayGachaType)multiPlayGachaRecord.MultiPlayGachaType, multiPlayGachaRecord);
			}
			dicShop.Clear();
			foreach (NetShopRecord shopRecord in res.ShopRecordList)
			{
				dicShop.Value(shopRecord.ShopID).netShopRecord = shopRecord;
			}
			dicShopEx.Clear();
			foreach (NetShopInfoEx exShopInfo in res.ExShopInfoList)
			{
				dicShopEx.Value(exShopInfo.n_ID).netShopExInfo = exShopInfo;
			}
			dicBannerEx.Clear();
			foreach (NetBannerEx exBanner in res.ExBannerList)
			{
				dicBannerEx.Value(exBanner.n_ID).netBannerExInfo = exBanner;
			}
			dicGachaEx.Clear();
			foreach (NetGachaInfoEx exGachaInfo in res.ExGachaInfoList)
			{
				dicGachaEx.Value(exGachaInfo.n_ID).netGachaExInfo = exGachaInfo;
			}
			dicGachaEventEx.Clear();
			foreach (NetGachaEventInfoEx exGachaEventInfo in res.ExGachaEventInfoList)
			{
				dicGachaEventEx.Value(exGachaEventInfo.n_ID).netGachaEventExInfo = exGachaEventInfo;
			}
			dicEventEx.Clear();
			foreach (NetEventEx exEvent in res.ExEventList)
			{
				dicEventEx.Value(exEvent.n_ID).netEventExInfo = exEvent;
			}
			dicItemEx.Clear();
			foreach (NetItemInfoEx exItem in res.ExItemList)
			{
				dicItemEx.Value(exItem.n_ID).netItemExInfo = exItem;
			}
			dicBoxGachaStatus.Clear();
			foreach (NetBoxGachaStatus boxGachaStatus in res.BoxGachaStatusList)
			{
				dicBoxGachaStatus.Value(boxGachaStatus.EventID).netBoxGachaStatus = boxGachaStatus;
			}
			dicBossRushInfo.Clear();
			foreach (NetBREventInfo bREventInfo in res.BREventInfoList)
			{
				dicBossRushInfo.Value(bREventInfo.EventID).netBRInfo = bREventInfo;
			}
			rawExData += res.RawExtData;
			p_cb.CheckTargetToInvoke();
		});
	}

	public void ItemListReq(Callback p_cb = null)
	{
		WebService.SendRequest(new ItemListReq(), delegate(ItemListRes res)
		{
			foreach (NetItemInfo item in res.ItemList)
			{
				dicItem.Value(item.ItemID).netItemInfo = item;
			}
			p_cb.CheckTargetToInvoke();
		});
	}

	public void RetrievePlayerEquipInfoReq(Callback p_cb = null)
	{
		WebService.SendRequest(new RetrievePlayerEquipInfoReq(), delegate(RetrievePlayerEquipInfoRes res)
		{
			foreach (NetPlayerEquipInfo playerEquip in res.PlayerEquipList)
			{
				dicEquipEnhance.Value((EquipPartType)playerEquip.Slot).netPlayerEquipInfo = playerEquip;
			}
			p_cb.CheckTargetToInvoke();
		});
	}

	public void RetrieveCharacterListReq(Callback p_cb = null)
	{
		WebService.SendRequest(new RetrieveCharacterListReq(), delegate(RetrieveCharacterListRes res)
		{
			dicCharacter.Clear();
			foreach (NetCharacterInfo character in res.CharacterList)
			{
				dicCharacter.Value(character.CharacterID).netInfo = character;
			}
			foreach (NetCharacterSkillInfo characterSkill in res.CharacterSkillList)
			{
				dicCharacter.Value(characterSkill.CharacterID).netSkillDic.ContainsAdd((CharacterSkillSlot)characterSkill.Slot, characterSkill);
			}
			p_cb.CheckTargetToInvoke();
		});
	}

	public void RetrieveWeaponListReq(Callback p_cb = null)
	{
		WebService.SendRequest(new RetrieveWeaponListReq(), delegate(RetrieveWeaponListRes res)
		{
			foreach (NetWeaponInfo weapon in res.WeaponList)
			{
				dicWeapon.Value(weapon.WeaponID).netInfo = weapon;
			}
			foreach (NetWeaponExpertInfo weaponExpert in res.WeaponExpertList)
			{
				dicWeapon.Value(weaponExpert.WeaponID).AddNetWeaponExpertInfo(weaponExpert);
			}
			foreach (NetWeaponSkillInfo weaponSkill in res.WeaponSkillList)
			{
				dicWeapon.Value(weaponSkill.WeaponID).AddNetWeaponSkillInfo(weaponSkill);
			}
			p_cb.CheckTargetToInvoke();
		});
	}

	public void RetrieveChipListReq(Callback p_cb = null)
	{
		WebService.SendRequest(new RetrieveChipListReq(), delegate(RetrieveChipListRes res)
		{
			foreach (NetChipInfo chip in res.ChipList)
			{
				dicChip.Value(chip.ChipID).netChipInfo = chip;
			}
			p_cb.CheckTargetToInvoke();
		});
	}

	public void ChangeNickNameReq(string name, Callback p_cb = null)
	{
		WebService.SendRequest(new ChangeNicknameReq
		{
			Nickname = name
		}, delegate(ChangeNicknameRes res)
		{
			playerInfo.netPlayerInfo.Nickname = res.Nickname;
			p_cb.CheckTargetToInvoke();
		});
	}

	public void RetrieveGiverListReq(Action<List<string>> p_cb = null)
	{
		WebService.SendRequest(new RetrieveGiverListReq(), delegate(RetrieveGiverListRes res)
		{
			if (res.FriendPlaerIDList != null)
			{
				p_cb.CheckTargetToInvoke(res.FriendPlaerIDList);
			}
		});
	}

	public void RetrieveFriendGiftAPReq(List<string> listPlayerID, Callback p_cb = null)
	{
		WebService.SendRequest(new RetrieveFriendGiftAPReq
		{
			FriendPlaerIDList = listPlayerID
		}, delegate(RetrieveFriendGiftAPRes res)
		{
			if (res.PlayerInfo != null)
			{
				playerInfo.netPlayerInfo = res.PlayerInfo;
			}
			p_cb.CheckTargetToInvoke();
		});
	}

	public void GiveAPToFriendReq(List<string> listPlayerID, Callback p_cb = null)
	{
		WebService.SendRequest<GiveFriendAPRes>(new GiveFriendAPReq
		{
			FriendIDList = listPlayerID
		}, delegate
		{
			p_cb.CheckTargetToInvoke();
		});
	}

	public void ChargeEnergyReq(ChargeType type, Callback p_cb = null)
	{
		WebService.SendRequest(new ChargeEnergyReq
		{
			ChargeType = (sbyte)type
		}, delegate(ChargeEnergyRes res)
		{
			if (res.PlayerInfo != null)
			{
				playerInfo.netPlayerInfo = res.PlayerInfo;
			}
			foreach (NetItemInfo item in res.ItemList)
			{
				dicItem.Value(item.ItemID).netItemInfo = item;
			}
			dicCharge.Value(res.ChargeInfo.ChargeType).netChargeInfo = res.ChargeInfo;
			p_cb.CheckTargetToInvoke();
		});
	}

	public void SetPortraitReq(int portraitItemID, Callback p_cb = null)
	{
		WebService.SendRequest(new SetPortraitReq
		{
			PortraitItemID = portraitItemID
		}, delegate(SetPortraitRes res)
		{
			if (res.PlayerInfo != null)
			{
				playerInfo.netPlayerInfo = res.PlayerInfo;
				p_cb.CheckTargetToInvoke();
			}
		});
	}

	public void SetTitleReq(int titleItemID, Callback p_cb = null)
	{
		WebService.SendRequest(new SetTitleReq
		{
			TitleItemID = titleItemID
		}, delegate(SetTitleRes res)
		{
			if (res.PlayerInfo != null)
			{
				playerInfo.netPlayerInfo = res.PlayerInfo;
				p_cb.CheckTargetToInvoke();
			}
		});
	}

	public void CharacterStandbyReq(int p_characterId, Callback p_cb = null)
	{
		WebService.SendRequest(new CharacterStandbyReq
		{
			CharacterId = p_characterId
		}, delegate(CharacterStandbyRes res)
		{
			if (res.PlayerInfo != null)
			{
				playerInfo.netPlayerInfo = res.PlayerInfo;
			}
			p_cb.CheckTargetToInvoke();
		});
	}

	public void CharacterUnlockSkillSlotReq(int p_characterId, CharacterSkillSlot p_characterSkillSlot, Callback p_cb = null)
	{
		WebService.SendRequest(new CharacterUnlockSkillSlotReq
		{
			CharacterId = p_characterId,
			SkillSlot = (sbyte)p_characterSkillSlot
		}, delegate(CharacterUnlockSkillSlotRes res)
		{
			if (res.CharacterSkillInfo != null)
			{
				dicCharacter.Value(res.CharacterSkillInfo.CharacterID).netSkillDic.ContainsAdd((CharacterSkillSlot)res.CharacterSkillInfo.Slot, res.CharacterSkillInfo);
			}
			foreach (NetItemInfo item in res.ItemList)
			{
				dicItem.Value(item.ItemID).netItemInfo = item;
			}
			p_cb.CheckTargetToInvoke();
		});
	}

	public void CharacterUpgradeStarReq(int p_characterId, int p_upgradeId, Callback p_cb = null)
	{
		WebService.SendRequest(new CharacterUpgradeStarReq
		{
			CharacterId = p_characterId,
			UpgradeId = p_upgradeId
		}, delegate(CharacterUpgradeStarRes res)
		{
			if (res.CharacterInfo != null)
			{
				dicCharacter.Value(res.CharacterInfo.CharacterID).netInfo = res.CharacterInfo;
			}
			foreach (NetItemInfo item in res.ItemList)
			{
				dicItem.Value(item.ItemID).netItemInfo = item;
			}
			p_cb.CheckTargetToInvoke();
		});
	}

	public void CharacterSetEnhanceSkillReq(int p_characterId, CharacterSkillSlot p_skillSlot, CharacterSkillEnhanceSlot p_skillEnhanceSlot, Callback p_cb = null)
	{
		WebService.SendRequest(new CharacterSetEnhanceSkillReq
		{
			CharacterId = p_characterId,
			SkillSlot = (sbyte)p_skillSlot,
			EnhanceSlot = (sbyte)p_skillEnhanceSlot
		}, delegate(CharacterSetEnhanceSkillRes res)
		{
			if (res.CharacterSkillInfo != null)
			{
				dicCharacter.Value(res.CharacterSkillInfo.CharacterID).netSkillDic.ContainsAdd((CharacterSkillSlot)res.CharacterSkillInfo.Slot, res.CharacterSkillInfo);
			}
			p_cb.CheckTargetToInvoke();
		});
	}

	public void CharacterUpgradeSkillReq(int p_characterId, CharacterSkillSlot p_skillSlot, int nTargetSkillLV, Callback p_cb = null)
	{
		WebService.SendRequest(new CharacterUpgradeSkillReq
		{
			CharacterId = p_characterId,
			SkillSlot = (sbyte)p_skillSlot,
			TargetSkillLevel = (sbyte)nTargetSkillLV
		}, delegate(CharacterUpgradeSkillRes res)
		{
			if (res.CharacterSkillInfo != null)
			{
				dicCharacter.Value(res.CharacterSkillInfo.CharacterID).netSkillDic.ContainsAdd((CharacterSkillSlot)res.CharacterSkillInfo.Slot, res.CharacterSkillInfo);
			}
			foreach (NetItemInfo item in res.ItemList)
			{
				dicItem.Value(item.ItemID).netItemInfo = item;
			}
			p_cb.CheckTargetToInvoke();
		});
	}

	public void CharacterUnlockReq(int p_characterId, Callback p_cb = null)
	{
		WebService.SendRequest(new CharacterUnlockReq
		{
			CharacterId = p_characterId
		}, delegate(CharacterUnlockRes res)
		{
			if (res.CharacterInfo != null)
			{
				dicCharacter.Value(res.CharacterInfo.CharacterID).netInfo = res.CharacterInfo;
			}
			foreach (NetCharacterSkillInfo characterSkill in res.CharacterSkillList)
			{
				dicCharacter.Value(characterSkill.CharacterID).netSkillDic.ContainsAdd((CharacterSkillSlot)characterSkill.Slot, characterSkill);
			}
			foreach (NetItemInfo item in res.ItemList)
			{
				dicItem.Value(item.ItemID).netItemInfo = item;
			}
			p_cb.CheckTargetToInvoke();
		});
	}

	public void CharacterSkinSetReq(int p_characterId, int p_skinId, Action<NetCharacterInfo> p_cb = null)
	{
		WebService.SendRequest(new CharacterSkinSetReq
		{
			CharacterId = p_characterId,
			SkinId = p_skinId
		}, delegate(CharacterSkinSetRes res)
		{
			if (res.CharacterInfo != null)
			{
				dicCharacter.Value(res.CharacterInfo.CharacterID).netInfo = res.CharacterInfo;
			}
			p_cb.CheckTargetToInvoke(res.CharacterInfo);
		});
	}

	public void CharacterUnlockSkinReq(int p_characterId, int p_skinId, Action<NetCharacterSkinInfo> p_cb = null)
	{
		WebService.SendRequest(new CharacterUnlockSkinReq
		{
			CharacterId = p_characterId,
			SkinId = p_skinId
		}, delegate(CharacterUnlockSkinRes res)
		{
			mmapSkin.Add(res.CharacterSkinInfo.CharacterID, res.CharacterSkinInfo);
			CharacterInfo value = null;
			if (dicCharacter.TryGetValue(p_characterId, out value))
			{
				value.netSkinList.Add(p_skinId);
			}
			foreach (NetItemInfo item in res.ItemList)
			{
				dicItem.Value(item.ItemID).netItemInfo = item;
			}
			p_cb.CheckTargetToInvoke(res.CharacterSkinInfo);
		});
	}

	public void CharacterDNAAnalyzeReq(int p_DNAId, Action<NetCharacterDNAInfo> p_cb = null)
	{
		WebService.SendRequest(new CharacterDNAAnalyzeReq
		{
			DNAId = p_DNAId
		}, delegate(CharacterDNAAnalyzeRes res)
		{
			if (res.Code == 108000)
			{
				if (res.CharacterDNAInfo != null)
				{
					dicCharacter.Value(res.CharacterDNAInfo.CharacterID).netDNAInfoDic.ContainsAdd(res.CharacterDNAInfo.SlotID, res.CharacterDNAInfo);
				}
				foreach (NetItemInfo item in res.ItemList)
				{
					dicItem.Value(item.ItemID).netItemInfo = item;
				}
				p_cb.CheckTargetToInvoke(res.CharacterDNAInfo);
			}
		});
	}

	public void CharacterDNALinkReq(int characterId, int targetCharacterId, List<sbyte> useSPItemSlotId, Action<NetCharacterDNALinkInfo> p_cb = null)
	{
		WebService.SendRequest(new CharacterDNALinkReq
		{
			CharacterId = characterId,
			BeLinkCharacterId = targetCharacterId,
			UseSPItemSlotId = useSPItemSlotId
		}, delegate(CharacterDNALinkRes res)
		{
			if (res.CharacterDNALinkInfo != null)
			{
				dicCharacter.Value(res.CharacterDNALinkInfo.CharacterID).netDNALinkInfo = res.CharacterDNALinkInfo;
			}
			foreach (NetItemInfo item in res.ItemList)
			{
				dicItem.Value(item.ItemID).netItemInfo = item;
			}
			p_cb.CheckTargetToInvoke(res.CharacterDNALinkInfo);
		});
	}

	public void DNAChangeSkill(int cid = 0, int slot = 0, bool bChange = false, Action<NetCharacterDNAInfo> cb = null)
	{
		WebService.SendRequest(new ConfirmDNAAnalyzeReq
		{
			CharacterId = cid,
			SlotId = slot,
			ConfirmUseNewSkill = (sbyte)(bChange ? 1 : 0)
		}, delegate(ConfirmDNAAnalyzeRes res)
		{
			if (res.CharacterDNAInfo != null)
			{
				dicCharacter.Value(res.CharacterDNAInfo.CharacterID).netDNAInfoDic.ContainsAdd(res.CharacterDNAInfo.SlotID, res.CharacterDNAInfo);
			}
			cb.CheckTargetToInvoke(res.CharacterDNAInfo);
		});
	}

	public void CharacterFavoriteChange(int cid, Callback p_cb = null)
	{
		WebService.SendRequest(new FavoriteCharacterReq
		{
			CharacterId = cid
		}, delegate(FavoriteCharacterRes res)
		{
			dicCharacter.Value(res.CharacterInfo.CharacterID).netInfo = res.CharacterInfo;
			p_cb.CheckTargetToInvoke();
		});
	}

	public void WeaponWieldReq(int p_weaponID, WeaponWieldType p_wieldPart, Action<WeaponWieldRes> p_cb = null)
	{
		WebService.SendRequest(new WeaponWieldReq
		{
			WeaponID = p_weaponID,
			WieldPart = (sbyte)p_wieldPart
		}, delegate(WeaponWieldRes res)
		{
			if (res.PlayerInfo != null)
			{
				playerInfo.netPlayerInfo = res.PlayerInfo;
			}
			foreach (NetBenchInfo benchWeaponInfo in res.BenchWeaponInfoList)
			{
				ManagedSingleton<PlayerNetManager>.Instance.dicBenchWeaponInfo.Value(benchWeaponInfo.BenchSlot).netBenchInfo = benchWeaponInfo;
			}
			if (p_cb != null)
			{
				p_cb(res);
			}
		});
	}

	public void WeaponUnlockReq(int p_weaponID, Callback p_cb = null)
	{
		WebService.SendRequest(new WeaponUnlockReq
		{
			WeaponID = p_weaponID
		}, delegate(WeaponUnlockRes res)
		{
			if (res.WeaponInfo != null)
			{
				dicWeapon.Value(res.WeaponInfo.WeaponID).netInfo = res.WeaponInfo;
			}
			foreach (NetWeaponExpertInfo weaponExpert in res.WeaponExpertList)
			{
				dicWeapon.Value(weaponExpert.WeaponID).AddNetWeaponExpertInfo(weaponExpert);
			}
			foreach (NetItemInfo item in res.ItemList)
			{
				dicItem.Value(item.ItemID).netItemInfo = item;
			}
			p_cb.CheckTargetToInvoke();
		});
	}

	public void WeaponAddExpReq(int p_weaponID, List<ItemConsumptionInfo> p_itemConsumptionList, Callback p_cb = null)
	{
		WebService.SendRequest(new WeaponAddExpReq
		{
			WeaponID = p_weaponID,
			ItemConsumptionList = p_itemConsumptionList
		}, delegate(WeaponAddExpRes res)
		{
			if (res.WeaponInfo != null)
			{
				dicWeapon.Value(res.WeaponInfo.WeaponID).netInfo = res.WeaponInfo;
			}
			foreach (NetItemInfo item in res.ItemList)
			{
				dicItem.Value(item.ItemID).netItemInfo = item;
			}
			p_cb.CheckTargetToInvoke();
		});
	}

	public void WeaponUpgradeStarReq(int p_weaponID, int p_upgradeId, Callback p_cb = null)
	{
		WebService.SendRequest(new WeaponUpgradeStarReq
		{
			WeaponID = p_weaponID,
			UpgradeId = p_upgradeId
		}, delegate(WeaponUpgradeStarRes res)
		{
			if (res.WeaponInfo != null)
			{
				dicWeapon.Value(res.WeaponInfo.WeaponID).netInfo = res.WeaponInfo;
			}
			foreach (NetItemInfo item in res.ItemList)
			{
				dicItem.Value(item.ItemID).netItemInfo = item;
			}
			p_cb.CheckTargetToInvoke();
		});
	}

	public void WeaponUnlockSkillSlotReq(int p_weaponID, WeaponSkillSlot p_weaponSkillSlot, Callback p_cb = null)
	{
		WebService.SendRequest(new WeaponUnlockSkillSlotReq
		{
			WeaponID = p_weaponID,
			SkillSlot = (sbyte)p_weaponSkillSlot
		}, delegate(WeaponUnlockSkillSlotRes res)
		{
			if (res.WeaponSkillInfo != null)
			{
				dicWeapon.Value(res.WeaponSkillInfo.WeaponID).AddNetWeaponSkillInfo(res.WeaponSkillInfo);
			}
			foreach (NetItemInfo item in res.ItemList)
			{
				dicItem.Value(item.ItemID).netItemInfo = item;
			}
			p_cb.CheckTargetToInvoke();
		});
	}

	public void WeaponExpertUpReq(int p_weaponID, WeaponExpertType p_weaponExpertType, List<int> listupgradeId, Callback p_cb = null)
	{
		WebService.SendRequest(new WeaponExpertUpReq
		{
			WeaponID = p_weaponID,
			ExpertType = (sbyte)p_weaponExpertType,
			UpgradeIDList = listupgradeId
		}, delegate(WeaponExpertUpRes res)
		{
			if (res.WeaponInfo != null && res.WeaponExpertInfo != null)
			{
				WeaponInfo weaponInfo = dicWeapon.Value(res.WeaponInfo.WeaponID);
				weaponInfo.netInfo = res.WeaponInfo;
				weaponInfo.AddNetWeaponExpertInfo(res.WeaponExpertInfo);
			}
			foreach (NetItemInfo item in res.ItemList)
			{
				dicItem.Value(item.ItemID).netItemInfo = item;
			}
			p_cb.CheckTargetToInvoke();
		});
	}

	public void WeaponExpertTransferReq(int p_weaponID, int p_transferAmount, Callback p_cb = null)
	{
		WebService.SendRequest(new WeaponExpertTransferReq
		{
			WeaponID = p_weaponID,
			TransferAmount = p_transferAmount
		}, delegate(WeaponExpertTransferRes res)
		{
			if (res.WeaponInfo != null)
			{
				dicWeapon.Value(res.WeaponInfo.WeaponID).netInfo = res.WeaponInfo;
			}
			foreach (NetItemInfo item in res.ItemList)
			{
				dicItem.Value(item.ItemID).netItemInfo = item;
			}
			p_cb.CheckTargetToInvoke();
		});
	}

	public void WeaponSkillUpReq(int p_weaponID, WeaponSkillSlot p_weaponSkillSlot, int targetSkillLevel, Callback p_cb = null)
	{
		WebService.SendRequest(new WeaponSkillUpReq
		{
			WeaponID = p_weaponID,
			SkillSlot = (sbyte)p_weaponSkillSlot,
			TargetSkillLevel = (sbyte)targetSkillLevel
		}, delegate(WeaponSkillUpRes res)
		{
			if (res.WeaponSkillInfo != null)
			{
				dicWeapon.Value(res.WeaponSkillInfo.WeaponID).AddNetWeaponSkillInfo(res.WeaponSkillInfo);
			}
			foreach (NetItemInfo item in res.ItemList)
			{
				dicItem.Value(item.ItemID).netItemInfo = item;
			}
			p_cb.CheckTargetToInvoke();
		});
	}

	public void WeaponChipSetReq(int p_weaponID, int p_chipID, ChipSetState nSetState, Callback p_cb = null)
	{
		WebService.SendRequest(new WeaponChipSetReq
		{
			WeaponID = p_weaponID,
			ChipID = (sbyte)p_chipID,
			SetState = (sbyte)nSetState
		}, delegate(WeaponChipSetRes res)
		{
			if (res.WeaponInfo != null)
			{
				dicWeapon.Value(res.WeaponInfo.WeaponID).netInfo = res.WeaponInfo;
			}
			p_cb.CheckTargetToInvoke();
		});
	}

	public void WeaponChipSetReqs(int[] parray_AddWeaponID, int p_chipID, int[] parray_RemoveWeaponID, Callback p_cb = null)
	{
		List<NetWeaponIDInfo> list = new List<NetWeaponIDInfo>();
		for (int i = 0; i < parray_AddWeaponID.Length; i++)
		{
			list.Add(new NetWeaponIDInfo
			{
				WeaponID = parray_AddWeaponID[i]
			});
		}
		List<NetWeaponIDInfo> list2 = new List<NetWeaponIDInfo>();
		for (int j = 0; j < parray_RemoveWeaponID.Length; j++)
		{
			list2.Add(new NetWeaponIDInfo
			{
				WeaponID = parray_RemoveWeaponID[j]
			});
		}
		WebService.SendRequest(new EquipChipReq
		{
			AddWeaponIDList = list,
			RemoveWeaponIDList = list2,
			ChipID = (sbyte)p_chipID
		}, delegate(EquipChipRes res)
		{
			for (int k = 0; k < res.WeaponList.Count; k++)
			{
				dicWeapon.Value(res.WeaponList[k].WeaponID).netInfo = res.WeaponList[k];
			}
			p_cb.CheckTargetToInvoke();
		});
	}

	public void WeaponResetReq(int p_weaponID, WeaponResetType wrType, Action<List<NetRewardInfo>> p_cb = null)
	{
		WebService.SendRequest(new WeaponResetReq
		{
			WeaponID = p_weaponID,
			ResetType = (sbyte)wrType
		}, delegate(WeaponResetRes res)
		{
			if (res.WeaponInfo != null)
			{
				dicWeapon.Value(res.WeaponInfo.WeaponID).netInfo = res.WeaponInfo;
			}
			foreach (NetWeaponExpertInfo weaponExpert in res.WeaponExpertList)
			{
				dicWeapon.Value(weaponExpert.WeaponID).AddNetWeaponExpertInfo(weaponExpert);
			}
			foreach (NetWeaponSkillInfo weaponSkill in res.WeaponSkillList)
			{
				dicWeapon.Value(weaponSkill.WeaponID).AddNetWeaponSkillInfo(weaponSkill);
			}
			foreach (NetItemInfo item in res.ItemList)
			{
				dicItem.Value(item.ItemID).netItemInfo = item;
			}
			if (res.RewardEntities != null)
			{
				ComposeRewardEntities(res.RewardEntities);
			}
			p_cb.CheckTargetToInvoke(res.RewardEntities.RewardList);
		});
	}

	public void WeaponRandomRSkill(int p_weaponID, bool bUseSpecialItem, Callback p_cb = null)
	{
		WebService.SendRequest(new PullWeaponDiVESkillReq
		{
			WeaponID = p_weaponID,
			UseSpecialItem = (sbyte)(bUseSpecialItem ? 1 : 0)
		}, delegate(PullWeaponDiVESkillRes res)
		{
			dicWeapon.Value(res.DiVESkillInfo.WeaponID).netDiveSkillInfo = res.DiVESkillInfo;
			foreach (NetItemInfo item in res.ItemList)
			{
				dicItem.Value(item.ItemID).netItemInfo = item;
			}
			p_cb.CheckTargetToInvoke();
		});
	}

	public void WeaponChangeRSkill(int p_weaponID, bool newSkill, Callback p_cb = null)
	{
		WebService.SendRequest(new ConfirmWeaponDiVESkillReq
		{
			WeaponID = p_weaponID,
			ConfirmUseNewSkill = (sbyte)(newSkill ? 1 : 0)
		}, delegate(ConfirmWeaponDiVESkillRes res)
		{
			dicWeapon.Value(res.DiVESkillInfo.WeaponID).netDiveSkillInfo = res.DiVESkillInfo;
			p_cb.CheckTargetToInvoke();
		});
	}

	public void WeaponFavoriteChange(int p_weaponID, Callback p_cb = null)
	{
		WebService.SendRequest(new FavoriteWeaponReq
		{
			WeaponID = p_weaponID
		}, delegate(FavoriteWeaponRes res)
		{
			dicWeapon.Value(res.WeaponInfo.WeaponID).netInfo = res.WeaponInfo;
			p_cb.CheckTargetToInvoke();
		});
	}

	public void GalleryListReq(Callback p_cb = null)
	{
		WebService.SendRequest(new GalleryListReq(), delegate(GalleryListRes res)
		{
			galleryInfo.GalleryExpList = res.GalleryExpList;
			galleryInfo.GalleryList = res.GalleryList;
			p_cb.CheckTargetToInvoke();
		});
	}

	public void GalleryCardListReq(Callback p_cb = null)
	{
		WebService.SendRequest(new GalleryCardListReq(), delegate(GalleryCardListRes res)
		{
			galleryInfo.GalleryCardList = res.GalleryList;
			p_cb.CheckTargetToInvoke();
		});
	}

	public void GalleryUnlockReq(List<int> l_galleryIdList, Callback p_cb = null)
	{
		WebService.SendRequest(new GalleryUnlockReq
		{
			GalleryIDList = l_galleryIdList
		}, delegate(GalleryUnlockRes res)
		{
			if (res.GalleryList != null)
			{
				res.GalleryList.ForEach(delegate(NetGalleryInfo info)
				{
					galleryInfo.GalleryList.Add(info);
				});
			}
			NetGalleryExpInfo netGalleryExpInfo = null;
			if (res.GalleryExpInfo != null)
			{
				netGalleryExpInfo = galleryInfo.GalleryExpList.Find((NetGalleryExpInfo x) => x.GalleryType == res.GalleryExpInfo.GalleryType);
			}
			if (netGalleryExpInfo != null)
			{
				netGalleryExpInfo.Exp = res.GalleryExpInfo.Exp;
			}
			else
			{
				galleryInfo.GalleryExpList.Add(res.GalleryExpInfo);
			}
			p_cb.CheckTargetToInvoke();
		});
	}

	public void GalleryCardUnlockReq(List<NetGalleryMainIdInfo> l_galleryList, Callback p_cb = null)
	{
		WebService.SendRequest(new GalleryCardUnlockReq
		{
			UnlockGalleryList = l_galleryList
		}, delegate(GalleryCardUnlockRes res)
		{
			if (res.GalleryList != null)
			{
				res.GalleryList.ForEach(delegate(NetGalleryMainIdInfo info)
				{
					NetGalleryMainIdInfo item = galleryInfo.GalleryCardList.Find((NetGalleryMainIdInfo p) => p.GalleryMainID == info.GalleryMainID);
					if (item != null)
					{
						info.GalleryIDList.ForEach(delegate(int cardid)
						{
							item.GalleryIDList.Add(cardid);
						});
					}
					else
					{
						galleryInfo.GalleryCardList.Add(info);
					}
				});
			}
			NetGalleryExpInfo netGalleryExpInfo = null;
			if (res.GalleryExpInfo != null)
			{
				netGalleryExpInfo = galleryInfo.GalleryExpList.Find((NetGalleryExpInfo x) => x.GalleryType == res.GalleryExpInfo.GalleryType);
			}
			if (netGalleryExpInfo != null)
			{
				netGalleryExpInfo.Exp = res.GalleryExpInfo.Exp;
			}
			else
			{
				galleryInfo.GalleryExpList.Add(res.GalleryExpInfo);
			}
			p_cb.CheckTargetToInvoke();
		});
	}

	public void GalleryCompletionRateReq(GalleryType p_type, int p_mainID, Callback p_cb = null)
	{
	}

	public void RetrieveStageClearListReq(StageType p_stageType, Callback p_cb = null)
	{
		WebService.SendRequest(new RetrieveStageClearListReq
		{
			StageType = (sbyte)p_stageType
		}, delegate(RetrieveStageClearListRes res)
		{
			foreach (NetStageInfo stageClear in res.StageClearList)
			{
				dicStage.Value(stageClear.StageID).netStageInfo = stageClear;
			}
			p_cb.CheckTargetToInvoke();
		});
	}

	private string GetRoomID()
	{
		string strRoomID = MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.strRoomID;
		string text = CapUtility.GetSubString("PVP[", "]", strRoomID, false).Trim();
		if (text.Length < 1)
		{
			text = CapUtility.GetSubString("PVE[", "]", strRoomID, false).Trim();
		}
		MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.strRoomID = "";
		return text;
	}

	public void StageStartReq(int p_stageID, string p_stageKey, uint p_stageCrc, Callback<string> p_cb = null)
	{
		MonoBehaviourSingleton<OrangeCommunityManager>.Instance.OnSetBusy(UserStatus.Fighting);
		string roomID = GetRoomID();
		WebService.SendRequest(new StageStartReq
		{
			StageID = p_stageID,
			StageKey = p_stageKey,
			RoomID = roomID,
			StageCRC = p_stageCrc
		}, delegate(StageStartRes res)
		{
			if (res.PlayerInfo != null)
			{
				playerInfo.netPlayerInfo = res.PlayerInfo;
			}
			if (res.Code == 11000)
			{
				p_cb.CheckTargetToInvoke(res.StageKey);
			}
		});
	}

	public void StageStartReq(int p_stageID, string p_stageKey, uint p_stageCrc, int epRatio, Callback<string> p_cb = null)
	{
		MonoBehaviourSingleton<OrangeCommunityManager>.Instance.OnSetBusy(UserStatus.Fighting);
		string roomID = GetRoomID();
		WebService.SendRequest(new StageStartReq
		{
			StageID = p_stageID,
			StageKey = p_stageKey,
			RoomID = roomID,
			StageCRC = p_stageCrc,
			EPRatio = (sbyte)epRatio
		}, delegate(StageStartRes res)
		{
			if (res.PlayerInfo != null)
			{
				playerInfo.netPlayerInfo = res.PlayerInfo;
			}
			if (res.Code == 11000)
			{
				p_cb.CheckTargetToInvoke(res.StageKey);
			}
		});
	}

	public void StageEndReq(StageEndReq tStageEndReq, Action<StageEndRes> p_cb = null)
	{
		MonoBehaviourSingleton<OrangeCommunityManager>.Instance.OnSetBusy(UserStatus.Online);
		StageType stageType = StageType.None;
		STAGE_TABLE value;
		if (ManagedSingleton<OrangeDataManager>.Instance.STAGE_TABLE_DICT.TryGetValue(tStageEndReq.StageID, out value))
		{
			switch (value.n_TYPE)
			{
			case 9:
				stageType = StageType.RaidBoss;
				break;
			case 10:
				stageType = StageType.Crusade;
				break;
			case 11:
			case 12:
			case 13:
				stageType = (StageType)value.n_TYPE;
				break;
			}
		}
		switch (stageType)
		{
		case StageType.TWSuppress:
		case StageType.TWLightning:
		case StageType.TWCrusade:
		{
			TotalWarEndReq totalWarEndReq = new TotalWarEndReq();
			totalWarEndReq.ResolutionW = (short)Screen.width;
			totalWarEndReq.ResolutionH = (short)Screen.height;
			totalWarEndReq.StageID = tStageEndReq.StageID;
			totalWarEndReq.Result = tStageEndReq.Result;
			totalWarEndReq.Score = tStageEndReq.Score;
			totalWarEndReq.Power = tStageEndReq.Power;
			totalWarEndReq.Duration = tStageEndReq.Duration;
			totalWarEndReq.KillCount = tStageEndReq.KillCount;
			WebService.SendRequest(totalWarEndReq, delegate(TotalWarEndRes res)
			{
				if (res.StageInfo != null)
				{
					dicStage.Value(res.StageInfo.StageID).netStageInfo = res.StageInfo;
				}
				foreach (NetWeaponInfo weaponInfo in res.WeaponInfoList)
				{
					dicWeapon.Value(weaponInfo.WeaponID).netInfo = weaponInfo;
				}
				foreach (NetItemInfo item in res.ItemList)
				{
					dicItem.Value(item.ItemID).netItemInfo = item;
				}
				foreach (NetCharacterInfo characterInfo in res.CharacterInfoList)
				{
					dicCharacter.Value(characterInfo.CharacterID).netInfo = characterInfo;
				}
				if (res.PlayerInfo != null)
				{
					playerInfo.netPlayerInfo = res.PlayerInfo;
				}
				if (res.RewardEntities != null)
				{
					ComposeRewardEntities(res.RewardEntities);
				}
				StageEndRes stageEndRes = new StageEndRes
				{
					WeaponInfoList = res.WeaponInfoList,
					ItemList = res.ItemList,
					CharacterInfoList = res.CharacterInfoList,
					PlayerInfo = res.PlayerInfo,
					RewardEntities = res.RewardEntities,
					DisplayExp = res.DisplayExp,
					DisplayMoney = res.DisplayMoney,
					DisplayProf = res.DisplayProf,
					Code = res.Code
				};
				StageUpdate.AddPerGameSavaData("FinalScore" + res.FinalScore);
				StageUpdate.AddPerGameSavaData("RecordFlag" + res.RecordFlag);
				if (stageEndRes.RewardEntities == null)
				{
					stageEndRes.RewardEntities = new NetRewardsEntity();
					stageEndRes.RewardEntities.RewardList = new List<NetRewardInfo>();
				}
				p_cb.CheckTargetToInvoke(stageEndRes);
			});
			return;
		}
		case StageType.RaidBoss:
		{
			RaidBossEndReq raidBossEndReq = new RaidBossEndReq();
			raidBossEndReq.ResolutionW = (short)Screen.width;
			raidBossEndReq.ResolutionH = (short)Screen.height;
			raidBossEndReq.StageID = tStageEndReq.StageID;
			raidBossEndReq.Result = tStageEndReq.Result;
			raidBossEndReq.Star = tStageEndReq.Star;
			raidBossEndReq.Score = tStageEndReq.Score;
			raidBossEndReq.Power = tStageEndReq.Power;
			raidBossEndReq.Duration = tStageEndReq.Duration;
			WebService.SendRequest(raidBossEndReq, delegate(RaidBossEndRes res)
			{
				if (res.StageInfo != null)
				{
					dicStage.Value(res.StageInfo.StageID).netStageInfo = res.StageInfo;
				}
				foreach (NetWeaponInfo weaponInfo2 in res.WeaponInfoList)
				{
					dicWeapon.Value(weaponInfo2.WeaponID).netInfo = weaponInfo2;
				}
				foreach (NetItemInfo item2 in res.ItemList)
				{
					dicItem.Value(item2.ItemID).netItemInfo = item2;
				}
				foreach (NetCharacterInfo characterInfo2 in res.CharacterInfoList)
				{
					dicCharacter.Value(characterInfo2.CharacterID).netInfo = characterInfo2;
				}
				if (res.PlayerInfo != null)
				{
					playerInfo.netPlayerInfo = res.PlayerInfo;
				}
				if (res.RewardEntities != null)
				{
					ComposeRewardEntities(res.RewardEntities);
				}
				StageEndRes stageEndRes2 = new StageEndRes
				{
					WeaponInfoList = res.WeaponInfoList,
					ItemList = res.ItemList,
					CharacterInfoList = res.CharacterInfoList,
					PlayerInfo = res.PlayerInfo,
					RewardEntities = res.RewardEntities,
					DisplayExp = res.DisplayExp,
					DisplayMoney = res.DisplayMoney,
					DisplayProf = res.DisplayProf,
					Code = res.Code
				};
				if (stageEndRes2.RewardEntities == null)
				{
					stageEndRes2.RewardEntities = new NetRewardsEntity();
					stageEndRes2.RewardEntities.RewardList = new List<NetRewardInfo>();
				}
				p_cb.CheckTargetToInvoke(stageEndRes2);
			});
			return;
		}
		case StageType.Crusade:
		{
			Action<CrusadeEndRes> value2 = delegate(CrusadeEndRes res)
			{
				StageEndRes p_param = new StageEndRes
				{
					WeaponInfoList = res.WeaponInfoList,
					ItemList = res.ItemList,
					CharacterInfoList = res.CharacterInfoList,
					PlayerInfo = res.PlayerInfo,
					RewardEntities = (res.RewardEntities ?? new NetRewardsEntity
					{
						RewardList = new List<NetRewardInfo>()
					}),
					DisplayExp = res.DisplayExp,
					DisplayMoney = res.DisplayMoney,
					DisplayProf = res.DisplayProf,
					Code = res.Code
				};
				p_cb.CheckTargetToInvoke(p_param);
			};
			Singleton<CrusadeSystem>.Instance.OnCrusadeEndOnceEvent += value2;
			Singleton<CrusadeSystem>.Instance.EndCrusade(tStageEndReq.StageID, tStageEndReq.Result, tStageEndReq.Star, tStageEndReq.Score, tStageEndReq.Power, tStageEndReq.Duration, (short)Screen.width, (short)Screen.height);
			return;
		}
		}
		tStageEndReq.ResolutionW = (short)Screen.width;
		tStageEndReq.ResolutionH = (short)Screen.height;
		WEAPON_TABLE weaponTable = ManagedSingleton<OrangeTableHelper>.Instance.GetWeaponTable(playerInfo.netPlayerInfo.MainWeaponID);
		if (weaponTable != null)
		{
			SKILL_TABLE value3 = null;
			if (ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT.TryGetValue(weaponTable.n_SKILL, out value3))
			{
				tStageEndReq.Magazine = (short)value3.n_MAGAZINE;
			}
		}
		WebService.SendRequest(tStageEndReq, delegate(StageEndRes res)
		{
			if (res.StageInfo != null)
			{
				dicStage.Value(res.StageInfo.StageID).netStageInfo = res.StageInfo;
			}
			foreach (NetWeaponInfo weaponInfo3 in res.WeaponInfoList)
			{
				dicWeapon.Value(weaponInfo3.WeaponID).netInfo = weaponInfo3;
			}
			foreach (NetItemInfo item3 in res.ItemList)
			{
				dicItem.Value(item3.ItemID).netItemInfo = item3;
			}
			foreach (NetCharacterInfo characterInfo3 in res.CharacterInfoList)
			{
				dicCharacter.Value(characterInfo3.CharacterID).netInfo = characterInfo3;
			}
			if (res.PlayerInfo != null)
			{
				playerInfo.netPlayerInfo = res.PlayerInfo;
			}
			if (res.RewardEntities != null)
			{
				ComposeRewardEntities(res.RewardEntities);
			}
			if (res.StageSecretList != null)
			{
				foreach (NetStageSecretInfo stageSecret in res.StageSecretList)
				{
					dicStage.Value(stageSecret.StageID).AddStageSecretToList(stageSecret.SecretID);
				}
			}
			if (res.TowerBossInfoList != null && res.TowerBossInfoList.Count > 0)
			{
				ManagedSingleton<PlayerNetManager>.Instance.mmapTowerBossInfoMap.Replace(res.TowerBossInfoList[0].TowerStageID, res.TowerBossInfoList);
			}
			if (res.BREventInfo != null)
			{
				dicBossRushInfo.Value(res.BREventInfo.EventID).netBRInfo = res.BREventInfo;
			}
			p_cb.CheckTargetToInvoke(res);
		});
	}

	public void StageSweepReq(int p_stageID, int p_sweepCount, Action<NetRewardsEntity> p_cb = null, Action<StageSweepRes> p_cb2 = null)
	{
		WebService.SendRequest(new StageSweepReq
		{
			StageID = p_stageID,
			SweepCount = p_sweepCount
		}, delegate(StageSweepRes res)
		{
			if (res.StageInfo != null)
			{
				dicStage.Value(res.StageInfo.StageID).netStageInfo = res.StageInfo;
			}
			foreach (NetWeaponInfo weaponInfo in res.WeaponInfoList)
			{
				dicWeapon.Value(weaponInfo.WeaponID).netInfo = weaponInfo;
			}
			foreach (NetItemInfo item3 in res.ItemList)
			{
				dicItem.Value(item3.ItemID).netItemInfo = item3;
			}
			foreach (NetCharacterInfo characterInfo in res.CharacterInfoList)
			{
				dicCharacter.Value(characterInfo.CharacterID).netInfo = characterInfo;
			}
			if (res.PlayerInfo != null)
			{
				playerInfo.netPlayerInfo = res.PlayerInfo;
			}
			if (res.DisplayMoney > 0)
			{
				NetRewardInfo item = new NetRewardInfo
				{
					RewardID = OrangeConst.ITEMID_MONEY,
					Amount = res.DisplayMoney,
					RewardType = 1
				};
				res.RewardEntities.RewardList.Add(item);
			}
			if (res.DisplayExp > 0)
			{
				NetRewardInfo item2 = new NetRewardInfo
				{
					RewardID = OrangeConst.ITEMID_PLAYER_EXP,
					Amount = res.DisplayExp,
					RewardType = 1
				};
				res.RewardEntities.RewardList.Add(item2);
			}
			if (res.RewardEntities != null)
			{
				ComposeRewardEntities(res.RewardEntities);
				p_cb.CheckTargetToInvoke(res.RewardEntities);
			}
			p_cb2.CheckTargetToInvoke(res);
		});
	}

	public void StageContinueReq(int stageID, Callback p_cb = null)
	{
		WebService.SendRequest(new ContinueStageReq
		{
			StageID = stageID
		}, delegate(ContinueStageRes res)
		{
			foreach (NetItemInfo item in res.ItemList)
			{
				dicItem.Value(item.ItemID).netItemInfo = item;
			}
			p_cb.CheckTargetToInvoke();
		});
	}

	public void BattlePrepareReq(int p_mainWeaponID, int p_subWeaponID, int p_characterID, Callback p_cb = null)
	{
		WebService.SendRequest(new BattlePrepareReq
		{
			MainWeaponID = p_mainWeaponID,
			SubWeaponID = p_subWeaponID,
			CharacterId = p_characterID
		}, delegate(BattlePrepareRes res)
		{
			if (res.PlayerInfo != null)
			{
				playerInfo.netPlayerInfo = res.PlayerInfo;
			}
			p_cb.CheckTargetToInvoke();
		});
	}

	public void StageLimitResetReq(int stageID, Callback p_cb = null)
	{
		WebService.SendRequest(new StageLimitResetReq
		{
			StageID = stageID
		}, delegate(StageLimitResetRes res)
		{
			if (res.StageInfo != null)
			{
				dicStage.Value(res.StageInfo.StageID).netStageInfo = res.StageInfo;
			}
			foreach (NetItemInfo item in res.ItemList)
			{
				dicItem.Value(item.ItemID).netItemInfo = item;
			}
			p_cb.CheckTargetToInvoke();
		});
	}

	public List<NetWeaponInfo> GetBenchWeaponInfoList()
	{
		List<NetWeaponInfo> list = new List<NetWeaponInfo>();
		List<NetBenchInfo> list2 = dicBenchWeaponInfo.Values.Select((BenchInfo x) => x.netBenchInfo).ToList();
		for (int i = 0; i < list2.Count; i++)
		{
			int weaponID = list2[i].WeaponID;
			if (weaponID > 0 && dicWeapon.ContainsKey(weaponID))
			{
				WeaponInfo value = null;
				dicWeapon.TryGetValue(weaponID, out value);
				list.Add(value.netInfo);
			}
		}
		return list;
	}

	public int[] GetEquipWeapons(int nMainWeaponID, int nSubWeaponID)
	{
		List<int> list = new List<int>();
		list.Add(nMainWeaponID);
		list.Add(nSubWeaponID);
		List<NetBenchInfo> list2 = dicBenchWeaponInfo.Values.Select((BenchInfo x) => x.netBenchInfo).ToList();
		for (int i = 0; i < list2.Count; i++)
		{
			int weaponID = list2[i].WeaponID;
			if (weaponID > 0 && dicWeapon.ContainsKey(weaponID))
			{
				list.Add(weaponID);
			}
		}
		return list.ToArray();
	}

	public void SealBattleSettingReq(int characterId, Action<string> p_cb = null, int nMainWeaponID = 0, int nSubWeaponID = 0)
	{
		SealBattleSettingReq(new List<int> { characterId }, MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.GetControllerSetting(), p_cb, nMainWeaponID, nSubWeaponID);
	}

	public void SealBattleSettingReq(List<int> p_characterIds, Action<string> p_cb = null, int nMainWeaponID = 0, int nSubWeaponID = 0)
	{
		SealBattleSettingReq(p_characterIds, MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.GetControllerSetting(), p_cb, nMainWeaponID, nSubWeaponID);
	}

	public NetSealBattleSettingInfo GetNetSealBattleSettingInfo(List<int> p_characterIds, NetControllerSetting netController, int nMainWeaponID = 0, int nSubWeaponID = 0, bool bGenerate = false)
	{
		int[] equipWeapons = ManagedSingleton<PlayerNetManager>.Instance.GetEquipWeapons(nMainWeaponID, nSubWeaponID);
		NetSealBattleSettingInfo netSealBattleSettingInfo = new NetSealBattleSettingInfo();
		netSealBattleSettingInfo.CharacterList = GetNetCharacterInfos(p_characterIds.ToArray(), bGenerate);
		WeaponInfo value = null;
		WeaponInfo value2 = null;
		if (!dicWeapon.TryGetValue(equipWeapons[0], out value) && bGenerate)
		{
			value = new WeaponInfo();
			value.netInfo = new NetWeaponInfo();
			value.netInfo.WeaponID = equipWeapons[0];
		}
		if (!dicWeapon.TryGetValue(equipWeapons[1], out value2) && bGenerate)
		{
			value2 = new WeaponInfo();
			value2.netInfo = new NetWeaponInfo();
			value2.netInfo.WeaponID = equipWeapons[0];
		}
		netSealBattleSettingInfo.MainWeaponInfo = ((value == null) ? new NetWeaponInfo() : value.netInfo);
		netSealBattleSettingInfo.SubWeaponInfo = ((value2 == null) ? new NetWeaponInfo() : value2.netInfo);
		netSealBattleSettingInfo.EquipmentList = (from x in dicEquip.Values
			select x.netEquipmentInfo into x
			where x.Equip > 0
			select x).ToList();
		netSealBattleSettingInfo.CharacterSkillList = GetNetCharacterAllSkillInfos(p_characterIds.ToArray());
		netSealBattleSettingInfo.ExtraPassiveSkillInfoList = ManagedSingleton<PlayerNetManager>.Instance.GetExtraPassiveSkillInfoList(p_characterIds.ToArray());
		netSealBattleSettingInfo.WeaponExpertList = GetNetWeaponExpertInfos(equipWeapons);
		netSealBattleSettingInfo.WeaponSkillList = GetNetWeaponSkillInfos(equipWeapons);
		netSealBattleSettingInfo.WeaponDiVESkillList = GetNetWeaponDiVESkillInfos(equipWeapons);
		netSealBattleSettingInfo.PlayerInfo = playerInfo.netPlayerInfo;
		netSealBattleSettingInfo.ControllerInfo = netController;
		netSealBattleSettingInfo.TotalCharacterSkinList = GetNetCharacterSkins();
		netSealBattleSettingInfo.TotalFSList = dicFinalStrike.Values.Select((FinalStrikeInfo x) => x.netFinalStrikeInfo).ToList();
		netSealBattleSettingInfo.TotalChipList = dicChip.Values.Select((ChipInfo x) => x.netChipInfo).ToList();
		netSealBattleSettingInfo.GalleryLevelList = new List<int>
		{
			0,
			ManagedSingleton<GalleryHelper>.Instance.GalleryGetCharactersExp().m_lv,
			ManagedSingleton<GalleryHelper>.Instance.GalleryGetWeaponsExp().m_lv,
			ManagedSingleton<GalleryHelper>.Instance.GalleryGetCardsExp().m_lv
		};
		netSealBattleSettingInfo.PlayerEquipList = dicEquipEnhance.Values.Select((EquipEnhanceInfo x) => x.netPlayerEquipInfo).ToList();
		netSealBattleSettingInfo.BenchSlotInfoList = dicBenchWeaponInfo.Values.Select((BenchInfo x) => x.netBenchInfo).ToList();
		netSealBattleSettingInfo.BenchWeaponInfoList.Clear();
		for (int i = 0; i < netSealBattleSettingInfo.BenchSlotInfoList.Count; i++)
		{
			NetBenchInfo netBenchInfo = netSealBattleSettingInfo.BenchSlotInfoList[i];
			if (dicWeapon.ContainsKey(netBenchInfo.WeaponID))
			{
				netSealBattleSettingInfo.BenchWeaponInfoList.Add(dicWeapon[netBenchInfo.WeaponID].netInfo);
			}
		}
		List<NetCharacterCardSlotInfo> list = new List<NetCharacterCardSlotInfo>();
		for (int j = 0; j < netSealBattleSettingInfo.CharacterList.Count; j++)
		{
			int characterID = netSealBattleSettingInfo.CharacterList[j].CharacterID;
			System.Collections.Generic.Dictionary<int, NetCharacterCardSlotInfo> value3 = null;
			if (dicCharacterCardSlotInfo.TryGetValue(characterID, out value3))
			{
				List<NetCharacterCardSlotInfo> collection = value3.Values.ToList();
				list.AddRange(collection);
			}
		}
		netSealBattleSettingInfo.CharacterCardSlotInfoList = list;
		netSealBattleSettingInfo.CardInfoList.Clear();
		for (int k = 0; k < netSealBattleSettingInfo.CharacterCardSlotInfoList.Count; k++)
		{
			NetCharacterCardSlotInfo netCharacterCardSlotInfo = netSealBattleSettingInfo.CharacterCardSlotInfoList[k];
			if (dicCard.ContainsKey(netCharacterCardSlotInfo.CardSeqID))
			{
				netSealBattleSettingInfo.CardInfoList.Add(dicCard[netCharacterCardSlotInfo.CardSeqID].netCardInfo);
			}
		}
		string p = "device";
		netSealBattleSettingInfo.HashKey = CapUtility.EncryptStringToBase64(p, AccountInfo.ID);
		netSealBattleSettingInfo.PVPScore = MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.CurrentScore;
		return netSealBattleSettingInfo;
	}

	public void SealBattleSettingReq(List<int> p_characterIds, NetControllerSetting netController, Action<string> p_cb = null, int nMainWeaponID = 0, int nSubWeaponID = 0)
	{
		if (playerInfo != null && playerInfo.netPlayerInfo != null)
		{
			if (nMainWeaponID == 0)
			{
				nMainWeaponID = playerInfo.netPlayerInfo.MainWeaponID;
				nSubWeaponID = playerInfo.netPlayerInfo.SubWeaponID;
			}
			NetSealBattleSettingInfo netSealBattleSettingInfo = GetNetSealBattleSettingInfo(p_characterIds, netController, nMainWeaponID, nSubWeaponID);
			WebService.SendRequest(new SealBattleSettingReq
			{
				BattleSetting = netSealBattleSettingInfo
			}, delegate(SealBattleSettingRes res)
			{
				p_cb.CheckTargetToInvoke(res.SealedBattleSetting);
			});
		}
	}

	public List<NetCharacterInfo> GetNetCharacterInfos(int[] characterIds, bool bGenerate = false)
	{
		List<NetCharacterInfo> list = new List<NetCharacterInfo>();
		foreach (int num in characterIds)
		{
			CharacterInfo value = null;
			if (dicCharacter.TryGetValue(num, out value))
			{
				list.Add(value.netInfo);
			}
			else if (bGenerate)
			{
				value = new CharacterInfo();
				value.netInfo = new NetCharacterInfo();
				value.netInfo.CharacterID = num;
				list.Add(value.netInfo);
			}
		}
		return list;
	}

	public List<NetCharacterSkinInfo> GetNetCharacterSkins()
	{
		List<NetCharacterSkinInfo> list = new List<NetCharacterSkinInfo>();
		foreach (int key in dicCharacter.Keys)
		{
			if (!mmapSkin.ContainKey(key))
			{
				continue;
			}
			foreach (NetCharacterSkinInfo item in mmapSkin[key])
			{
				list.Add(item);
			}
		}
		return list;
	}

	public List<NetCharacterSkillInfo> GetNetCharacterSkillInfos(int[] characterIds)
	{
		List<NetCharacterSkillInfo> list = new List<NetCharacterSkillInfo>();
		foreach (int key in characterIds)
		{
			CharacterInfo value = null;
			if (dicCharacter.TryGetValue(key, out value))
			{
				list.AddRange(value.netSkillDic.Values.Where((NetCharacterSkillInfo x) => x.Slot == 1 || x.Slot == 2).ToList());
			}
		}
		return list;
	}

	public List<NetCharacterSkillInfo> GetNetCharacterAllSkillInfos(int[] characterIds)
	{
		List<NetCharacterSkillInfo> list = new List<NetCharacterSkillInfo>();
		foreach (int key in characterIds)
		{
			CharacterInfo value = null;
			if (dicCharacter.TryGetValue(key, out value))
			{
				list.AddRange(value.netSkillDic.Values.ToList());
			}
		}
		return list;
	}

	public List<int> GetCharacterDNASkillIDList(int nCharacterID)
	{
		List<int> list = new List<int>();
		CharacterInfo value = null;
		if (ManagedSingleton<PlayerNetManager>.Instance.dicCharacter.TryGetValue(nCharacterID, out value))
		{
			System.Collections.Generic.Dictionary<int, NetCharacterDNAInfo>.Enumerator enumerator = value.netDNAInfoDic.GetEnumerator();
			while (enumerator.MoveNext())
			{
				list.Add(enumerator.Current.Value.SkillID);
			}
			if (value.netDNALinkInfo != null)
			{
				CharacterInfo value2 = null;
				if (ManagedSingleton<PlayerNetManager>.Instance.dicCharacter.TryGetValue(value.netDNALinkInfo.LinkedCharacterID, out value2))
				{
					foreach (sbyte item in value.netDNALinkInfo.LinkedSlotID)
					{
						list.Add(value2.netDNAInfoDic[item].SkillID);
					}
				}
			}
		}
		return list;
	}

	public List<NetCharacterPassiveSkillInfo> GetExtraPassiveSkillInfoList(int[] characterIds)
	{
		List<NetCharacterPassiveSkillInfo> list = new List<NetCharacterPassiveSkillInfo>();
		foreach (int num in characterIds)
		{
			CharacterInfo value = null;
			if (dicCharacter.TryGetValue(num, out value))
			{
				List<int> characterDNASkillIDList = GetCharacterDNASkillIDList(num);
				for (int j = 0; j < characterDNASkillIDList.Count; j++)
				{
					NetCharacterPassiveSkillInfo netCharacterPassiveSkillInfo = new NetCharacterPassiveSkillInfo();
					netCharacterPassiveSkillInfo.CharacterID = num;
					netCharacterPassiveSkillInfo.SkillID = characterDNASkillIDList[j];
					netCharacterPassiveSkillInfo.Level = 1;
					list.Add(netCharacterPassiveSkillInfo);
				}
			}
		}
		return list;
	}

	public List<NetWeaponExpertInfo> GetNetWeaponExpertInfos(int[] weaponIds)
	{
		List<NetWeaponExpertInfo> list = new List<NetWeaponExpertInfo>();
		foreach (int key in weaponIds)
		{
			WeaponInfo value = null;
			if (dicWeapon.TryGetValue(key, out value))
			{
				for (int j = 0; j < value.netExpertInfos.Count; j++)
				{
					list.Add(value.netExpertInfos[j]);
				}
			}
		}
		return list;
	}

	public List<NetWeaponSkillInfo> GetNetWeaponSkillInfos(int[] weaponIds)
	{
		List<NetWeaponSkillInfo> list = new List<NetWeaponSkillInfo>();
		foreach (int key in weaponIds)
		{
			WeaponInfo value = null;
			if (dicWeapon.TryGetValue(key, out value) && value.netSkillInfos != null)
			{
				for (int j = 0; j < value.netSkillInfos.Count; j++)
				{
					list.Add(value.netSkillInfos[j]);
				}
			}
		}
		return list;
	}

	public List<NetWeaponDiVESkillInfo> GetNetWeaponDiVESkillInfos(int[] weaponIds)
	{
		List<NetWeaponDiVESkillInfo> list = new List<NetWeaponDiVESkillInfo>();
		foreach (int key in weaponIds)
		{
			WeaponInfo value = null;
			if (dicWeapon.TryGetValue(key, out value) && value.netDiveSkillInfo != null)
			{
				list.Add(value.netDiveSkillInfo);
			}
		}
		return list;
	}

	public List<NetChipInfo> GetEquipChipList(int[] weaponIds)
	{
		List<NetChipInfo> list = new List<NetChipInfo>();
		foreach (int key in weaponIds)
		{
			WeaponInfo value = null;
			if (dicWeapon.TryGetValue(key, out value))
			{
				ChipInfo value2 = null;
				if (dicChip.TryGetValue(value.netInfo.Chip, out value2))
				{
					list.Add(value2.netChipInfo);
				}
			}
		}
		return list;
	}

	public List<NetCharacterDNAInfo> GetNetCharacterDNAInfo(int[] characterIds)
	{
		List<NetCharacterDNAInfo> result = new List<NetCharacterDNAInfo>();
		for (int i = 0; i < characterIds.Length; i++)
		{
			int num = characterIds[i];
		}
		return result;
	}

	public void RetrievePVPRecordReq(int PVPType, Action<RetrievePVPRecordRes> p_cb = null)
	{
		new List<NetPVPRecord>();
		WebService.SendRequest(new RetrievePVPRecordReq
		{
			PVPType = PVPType
		}, delegate(RetrievePVPRecordRes res)
		{
			p_cb.CheckTargetToInvoke(res);
		});
	}

	public void RetrieveSeasonInfoReq(Action<RetrieveSeasonInfoRes> p_cb = null)
	{
		WebService.SendRequest(new RetrieveSeasonInfoReq(), delegate(RetrieveSeasonInfoRes res)
		{
			p_cb.CheckTargetToInvoke(res);
		});
	}

	public void SeasonBattleStartReq(NetSealBattleSettingInfo PlayerBattleSetting, NetSealBattleSettingInfo RivalBattleSetting, Action<SeasonBattleStartRes> p_cb = null)
	{
		string roomID = GetRoomID();
		WebService.SendRequest(new SeasonBattleStartReq
		{
			PlayerBattleSetting = PlayerBattleSetting,
			RivalBattleSetting = RivalBattleSetting,
			RoomID = roomID
		}, delegate(SeasonBattleStartRes res)
		{
			p_cb.CheckTargetToInvoke(res);
		});
	}

	public void SeasonBattleEndReq(int Result, Action<SeasonBattleEndRes> p_cb = null)
	{
		MonoBehaviourSingleton<OrangeCommunityManager>.Instance.OnSetBusy(UserStatus.Online);
		WebService.SendRequest(new SeasonBattleEndReq
		{
			Result = (sbyte)Result
		}, delegate(SeasonBattleEndRes res)
		{
			p_cb.CheckTargetToInvoke(res);
		});
	}

	public void SeasonBattleInfoReq(string PlayerID, int CharacterId, int HP, int KillCount, int BeKilledCount, Action<SeasonBattleInfoRes> p_cb = null)
	{
		WebService.SendRequest(new SeasonBattleInfoReq
		{
			PlayerID = PlayerID,
			CharacterId = CharacterId,
			HP = HP,
			KillCount = KillCount,
			BeKilledCount = BeKilledCount,
			ResolutionW = (short)Screen.width,
			ResolutionH = (short)Screen.height
		}, delegate(SeasonBattleInfoRes res)
		{
			p_cb.CheckTargetToInvoke(res);
		});
	}

	public void RetrieveGachaRecordReq(Callback p_cb = null)
	{
		WebService.SendRequest(new RetrieveGachaRecordReq(), delegate(RetrieveGachaRecordRes res)
		{
			foreach (NetGachaEventRecord gachaEventRecord in res.GachaEventRecordList)
			{
				dicGacha.Value(gachaEventRecord.GachaEventID).netGachaEventRecord = gachaEventRecord;
			}
			foreach (NetGachaGuaranteeRecord gachaGuaranteeRecord in res.GachaGuaranteeRecordList)
			{
				dicGachaGuaranteeRecord.ContainsAdd(gachaGuaranteeRecord.GroupID, gachaGuaranteeRecord.Value);
			}
			p_cb.CheckTargetToInvoke();
		});
	}

	public void RetrieveExGachaSettingReq(Action<List<NetGachaEventInfoEx>> p_cb = null)
	{
		WebService.SendRequest(new RetrieveExGachaSettingReq(), delegate(RetrieveExGachaSettingRes res)
		{
			foreach (NetGachaInfoEx exGachaInfo in res.ExGachaInfoList)
			{
				dicGachaEx.Value(exGachaInfo.n_ID).netGachaExInfo = exGachaInfo;
			}
			p_cb.CheckTargetToInvoke(res.ExGachaEventInfoList);
		});
	}

	public void GachaReq(int p_gachaEventID, Action<GachaRes> p_cb = null)
	{
		WebService.SendRequest(new GachaReq
		{
			GachaEventID = p_gachaEventID
		}, delegate(GachaRes res)
		{
			if (res.Code == 19000)
			{
				foreach (NetItemInfo item in res.ItemList)
				{
					dicItem.Value(item.ItemID).netItemInfo = item;
				}
				if (res.GachaEventRecord != null)
				{
					dicGacha.Value(res.GachaEventRecord.GachaEventID).netGachaEventRecord = res.GachaEventRecord;
				}
				if (res.RewardEntities != null)
				{
					ComposeRewardEntities(res.RewardEntities);
				}
				p_cb.CheckTargetToInvoke(res);
			}
		});
	}

	public void SetupGachaReq(int p_gachaEventID, int p_gachaID, Action<SetupGachaRes> p_cb = null)
	{
		WebService.SendRequest(new SetupGachaReq
		{
			GachaEventID = p_gachaEventID,
			GachaID = p_gachaID
		}, delegate(SetupGachaRes res)
		{
			if (res.Code == 102000)
			{
				if (res.GachaEventRecord != null)
				{
					dicGacha.Value(res.GachaEventRecord.GachaEventID).netGachaEventRecord = res.GachaEventRecord;
				}
				if (res.RewardEntities != null)
				{
					ComposeRewardEntities(res.RewardEntities);
				}
				p_cb.CheckTargetToInvoke(res);
			}
		});
	}

	public void RetrieveMultiPlayGachaRecordReq(MultiPlayGachaType p_multiPlayGachaType, Callback p_cb)
	{
		WebService.SendRequest(new RetrieveMultiPlayGachaRecordReq
		{
			MultiPlayGachaType = (sbyte)p_multiPlayGachaType
		}, delegate(RetrieveMultiPlayGachaRecordRes res)
		{
			if (res.MultiPlayGachaRecordList.Count > 0)
			{
				mmapMultiPlayGachaRecord.Replace((MultiPlayGachaType)res.MultiPlayGachaRecordList[0].MultiPlayGachaType, res.MultiPlayGachaRecordList);
			}
			p_cb.CheckTargetToInvoke();
		});
	}

	public void MultiPlayGachaReq(sbyte p_multiPlayGachaType, int p_gachaCount, Action<int, List<NetMultiPlayerGachaInfo>, List<NetRewardInfo>> p_cb)
	{
		WebService.SendRequest(new MultiPlayGachaReq
		{
			MultiPlayGachaType = p_multiPlayGachaType,
			GachaCount = (sbyte)p_gachaCount
		}, delegate(MultiPlayGachaRes res)
		{
			if (res.RewardEntities != null)
			{
				ComposeRewardEntities(res.RewardEntities);
			}
			p_cb.CheckTargetToInvoke(res.Code, res.MultiPlayGachaRecord, res.RewardEntities.RewardList);
		});
	}

	public void RetrieveBoxGachaRecordReq(int eventID, int currentBoxGachaID, Action<int, List<NetBoxGachaRecord>> p_cb)
	{
		WebService.SendRequest(new RetrieveBoxGachaRecordReq
		{
			EventID = eventID,
			CurrentBoxGachaID = currentBoxGachaID
		}, delegate(RetrieveBoxGachaRecordRes res)
		{
			p_cb.CheckTargetToInvoke(res.Code, res.BoxGachaRecordList);
		});
	}

	public void BoxGachaReq(int count, int boxGachaID, int eventID, Action<int, NetRewardsEntity> p_cb)
	{
		WebService.SendRequest(new BoxGachaReq
		{
			Count = count,
			CurrentBoxGachaID = boxGachaID,
			EventID = eventID
		}, delegate(BoxGachaRes res)
		{
			if (res.RewardEntities != null)
			{
				ComposeRewardEntities(res.RewardEntities);
			}
			foreach (NetItemInfo item in res.ItemList)
			{
				dicItem.Value(item.ItemID).netItemInfo = item;
			}
			p_cb.CheckTargetToInvoke(res.Code, res.RewardEntities);
		});
	}

	public void ResetBoxGachaStatusReq(int eventID, Action<int> p_cb)
	{
		WebService.SendRequest(new ResetBoxGachaStatusReq
		{
			EventID = eventID
		}, delegate(ResetBoxGachaStatusRes res)
		{
			dicBoxGachaStatus.Value(res.BoxGachaStatus.EventID).netBoxGachaStatus = res.BoxGachaStatus;
			p_cb.CheckTargetToInvoke(res.Code);
		});
	}

	public void RetrieveShopRecordReq(Callback p_cb = null)
	{
		WebService.SendRequest(new RetrieveShopRecordReq(), delegate(RetrieveShopRecordRes res)
		{
			foreach (NetShopRecord shopRecord in res.ShopRecordList)
			{
				dicShop.Value(shopRecord.ShopID).netShopRecord = shopRecord;
			}
			p_cb.CheckTargetToInvoke();
		});
	}

	public void RetrieveExShopSettingReq(Callback p_cb = null)
	{
		WebService.SendRequest(new RetrieveExShopSettingReq(), delegate(RetrieveExShopSettingRes res)
		{
			foreach (NetShopInfoEx exShopInfo in res.ExShopInfoList)
			{
				dicShop.Value(exShopInfo.n_ID).netShopInfoEx = exShopInfo;
			}
			p_cb.CheckTargetToInvoke();
		});
	}

	public void PurchaseItemReq(int p_shopItemID, int p_count, Action<List<NetRewardInfo>, NetShopRecord, short> p_cb = null)
	{
		WebService.SendRequest(new PurchaseItemReq
		{
			ShopItemID = p_shopItemID,
			Count = (short)p_count
		}, delegate(PurchaseItemRes res)
		{
			if (res.ShopRecord != null)
			{
				dicShop.Value(res.ShopRecord.ShopID).netShopRecord = res.ShopRecord;
			}
			foreach (NetItemInfo item in res.ItemList)
			{
				dicItem.Value(item.ItemID).netItemInfo = item;
			}
			if (res.RewardEntities != null)
			{
				ComposeRewardEntities(res.RewardEntities);
				p_cb.CheckTargetToInvoke(res.RewardEntities.RewardList, res.ShopRecord, res.Count);
			}
		});
	}

	public void RetrieveIAPProductReq(Action<List<NetIAPProductInfo>> p_cb)
	{
		WebService.SendRequest(new RetrieveIAPProductReq
		{
			Store = (sbyte)MonoBehaviourSingleton<OrangeIAP>.Instance.Store
		}, delegate(RetrieveIAPProductRes res)
		{
			p_cb.CheckTargetToInvoke(res.IAPProductList);
		});
	}

	public void StartSteamPurchaseReq(string oneTimeTicket, int shopItemID, string productID, string description, Action<StartSteamPurchaseRes> p_cb)
	{
		WebService.SendRequest(new StartSteamPurchaseReq
		{
			SteamTicket = oneTimeTicket,
			ShopItemID = shopItemID,
			ProductID = productID,
			ShopItemDescription = description
		}, delegate(StartSteamPurchaseRes res)
		{
			p_cb.CheckTargetToInvoke(res);
		});
	}

	public void CancelSteamPurchaseReq(string orderId, Action<CancelSteamPurchaseRes> p_cb)
	{
		WebService.SendRequest(new CancelSteamPurchaseReq
		{
			OrderID = orderId
		}, delegate(CancelSteamPurchaseRes res)
		{
			p_cb.CheckTargetToInvoke(res);
		});
	}

	public void IAPExchangReq(IAPReceipt iapReceipt, Action<int, List<NetPlayerServiceInfo>, NetRewardsEntity, string> p_cb = null)
	{
		WebService.SendRequest(new IAPExchangReq
		{
			ShopItemID = iapReceipt.ShopItemID,
			ProductID = iapReceipt.ProductID,
			Store = (sbyte)iapReceipt.StoreType,
			TransactionID = iapReceipt.TransactionID,
			ReceiptPayload = iapReceipt.Payload
		}, delegate(IAPExchangRes res)
		{
			bool flag = false;
			string p_param = string.Empty;
			switch ((IAPReceiptStatus)res.IAPReceipt.Status)
			{
			case IAPReceiptStatus.New:
				flag = false;
				p_param = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("IAP_FAIL_7_UNKNOWN");
				break;
			case IAPReceiptStatus.InvalidData:
			case IAPReceiptStatus.Expired:
				flag = true;
				p_param = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("IAP_FAIL_2_PRODUCT_UNAVAILABLE");
				break;
			case IAPReceiptStatus.StoreAuthSuccess:
			case IAPReceiptStatus.StoreAuthFailed:
				flag = true;
				p_param = MonoBehaviourSingleton<LocalizationManager>.Instance.GetStr("IAP_FAIL_7_UNKNOWN");
				break;
			case IAPReceiptStatus.Exchanged:
				flag = true;
				p_param = string.Empty;
				IAPReceipt.ListNetIAPReceiptInfo.Add(res.IAPReceipt);
				MonoBehaviourSingleton<KochavaEventManager>.Instance.SendEvent_Purchase(iapReceipt.ShopItemID);
				break;
			}
			if (flag)
			{
				string key = res.IAPReceipt.StoreType + iapReceipt.TransactionID;
				if (MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.DicNewReceipt.ContainsKey(key))
				{
					MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.DicNewReceipt.Remove(key);
					MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.Save();
				}
			}
			if (res.ShopRecord != null)
			{
				dicShop.Value(res.ShopRecord.ShopID).netShopRecord = res.ShopRecord;
			}
			foreach (NetPlayerServiceInfo playerServiceInfo in res.PlayerServiceInfoList)
			{
				dicService.Value(playerServiceInfo.ServiceID).netServiceInfo = playerServiceInfo;
			}
			if (res.RewardEntities != null)
			{
				ComposeRewardEntities(res.RewardEntities);
			}
			Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.UPDATE_SHOP);
			p_cb.CheckTargetToInvoke(iapReceipt.ShopItemID, res.PlayerServiceInfoList, res.RewardEntities, p_param);
		});
	}

	public void RetrieveIAPReceiptReq(Callback p_cb)
	{
		WebService.SendRequest(new RetrieveIAPReceiptReq(), delegate(RetrieveIAPReceiptRes res)
		{
			if (res.IAPReceiptList != null)
			{
				IAPReceipt.ListNetIAPReceiptInfo = res.IAPReceiptList;
			}
			else if (IAPReceipt.ListNetIAPReceiptInfo == null)
			{
				IAPReceipt.ListNetIAPReceiptInfo = new List<NetIAPReceiptInfo>();
			}
			p_cb.CheckTargetToInvoke();
		});
	}

	public void RetrieveFSListReq(Callback p_cb = null)
	{
		WebService.SendRequest(new RetrieveFSListReq(), delegate(RetrieveFSListRes res)
		{
			foreach (NetFinalStrikeInfo fS in res.FSList)
			{
				dicFinalStrike.Value(fS.FinalStrikeID).netFinalStrikeInfo = fS;
			}
			p_cb.CheckTargetToInvoke();
		});
	}

	public void FSSetupReq(int p_finalStrikeID, WeaponWieldType p_wieldPart, Callback p_cb = null)
	{
		WebService.SendRequest(new FSSetupReq
		{
			FinalStrikeID = p_finalStrikeID,
			WieldPart = (sbyte)p_wieldPart
		}, delegate(FSSetupRes res)
		{
			if (res.PlayerInfo != null)
			{
				playerInfo.netPlayerInfo = res.PlayerInfo;
			}
			p_cb.CheckTargetToInvoke();
		});
	}

	public void FSUnlockReq(int p_tableDataID, Callback p_cb = null)
	{
		WebService.SendRequest(new FSUnlockReq
		{
			TableDataID = p_tableDataID
		}, delegate(FSUnlockRes res)
		{
			if (res.FinalStrikeInfo != null)
			{
				dicFinalStrike.Value(res.FinalStrikeInfo.FinalStrikeID).netFinalStrikeInfo = res.FinalStrikeInfo;
			}
			foreach (NetItemInfo item in res.ItemList)
			{
				dicItem.Value(item.ItemID).netItemInfo = item;
			}
			p_cb.CheckTargetToInvoke();
		});
	}

	public void FSUpgradeStarReq(int p_finalStrikeID, int p_upgradeId, Callback p_cb = null)
	{
		WebService.SendRequest(new FSUpgradeStarReq
		{
			FinalStrikeID = p_finalStrikeID,
			UpgradeId = p_upgradeId
		}, delegate(FSUpgradeStarRes res)
		{
			if (res.FinalStrikeInfo != null)
			{
				dicFinalStrike.Value(res.FinalStrikeInfo.FinalStrikeID).netFinalStrikeInfo = res.FinalStrikeInfo;
			}
			foreach (NetItemInfo item in res.ItemList)
			{
				dicItem.Value(item.ItemID).netItemInfo = item;
			}
			p_cb.CheckTargetToInvoke();
		});
	}

	public void FSLevelUpReq(int p_tableDataID, Callback p_cb = null)
	{
		WebService.SendRequest(new FSLevelUpReq
		{
			TableDataID = p_tableDataID
		}, delegate(FSLevelUpRes res)
		{
			if (res.FinalStrikeInfo != null)
			{
				dicFinalStrike.Value(res.FinalStrikeInfo.FinalStrikeID).netFinalStrikeInfo = res.FinalStrikeInfo;
			}
			foreach (NetItemInfo item in res.ItemList)
			{
				dicItem.Value(item.ItemID).netItemInfo = item;
			}
			p_cb.CheckTargetToInvoke();
		});
	}

	public void EquipmentListReq(Callback p_cb = null)
	{
		WebService.SendRequest(new EquipmentListReq(), delegate(EquipmentListRes res)
		{
			foreach (NetEquipmentInfo equipment in res.EquipmentList)
			{
				dicEquip.Value(equipment.EquipmentID).netEquipmentInfo = equipment;
			}
			p_cb.CheckTargetToInvoke();
		});
	}

	public void ComposeEquipmentReq(int p_equipItemID, Action<NetEquipmentInfo> p_cb = null)
	{
		WebService.SendRequest(new ComposeEquipmentReq
		{
			EquipItemID = p_equipItemID
		}, delegate(ComposeEquipmentRes res)
		{
			if (res.EquipmentInfo != null)
			{
				dicEquip.Value(res.EquipmentInfo.EquipmentID).netEquipmentInfo = res.EquipmentInfo;
			}
			foreach (NetItemInfo item in res.ItemList)
			{
				dicItem.Value(item.ItemID).netItemInfo = item;
			}
			p_cb.CheckTargetToInvoke(res.EquipmentInfo);
		});
	}

	public void EquipEquipmentReq(int p_equipmentID, Callback p_cb = null)
	{
		WebService.SendRequest(new EquipEquipmentReq
		{
			EquipmentID = p_equipmentID
		}, delegate(EquipEquipmentRes res)
		{
			foreach (NetEquipmentInfo equipment in res.EquipmentList)
			{
				dicEquip.Value(equipment.EquipmentID).netEquipmentInfo = equipment;
			}
			p_cb.CheckTargetToInvoke();
		});
	}

	public void PowerUpEquipmentReq(int p_equipmentID, short enhanceLv, Callback p_cb = null)
	{
		WebService.SendRequest(new PowerUpEquipmentReq
		{
			EquipmentID = p_equipmentID,
			EnhanceLv = enhanceLv
		}, delegate(PowerUpEquipmentRes res)
		{
			foreach (NetPlayerEquipInfo playerEquip in res.PlayerEquipList)
			{
				dicEquipEnhance.Value((EquipPartType)playerEquip.Slot).netPlayerEquipInfo = playerEquip;
			}
			foreach (NetItemInfo item in res.ItemList)
			{
				dicItem.Value(item.ItemID).netItemInfo = item;
			}
			p_cb.CheckTargetToInvoke();
		});
	}

	public void DecomposeEquipmentReq(List<int> p_equipmentIDList, Action<NetRewardsEntity> p_cb = null)
	{
		WebService.SendRequest(new DecomposeEquipmentReq
		{
			EquipmentIDList = p_equipmentIDList
		}, delegate(DecomposeEquipmentRes res)
		{
			foreach (NetEquipmentInfo equipment in res.EquipmentList)
			{
				if (dicEquip.ContainsKey(equipment.EquipmentID))
				{
					dicEquip.Remove(equipment.EquipmentID);
				}
			}
			if (res.RewardEntities != null)
			{
				ComposeRewardEntities(res.RewardEntities);
				p_cb.CheckTargetToInvoke(res.RewardEntities);
			}
		});
	}

	public void UnlockChipReq(int p_chipID, Callback p_cb = null)
	{
		WebService.SendRequest(new UnlockChipReq
		{
			ChipID = p_chipID
		}, delegate(UnlockChipRes res)
		{
			if (res.ChipInfo != null)
			{
				dicChip.Value(res.ChipInfo.ChipID).netChipInfo = res.ChipInfo;
			}
			foreach (NetItemInfo item in res.ItemList)
			{
				dicItem.Value(item.ItemID).netItemInfo = item;
			}
			p_cb.CheckTargetToInvoke();
		});
	}

	public void UpgradeLevelChipReq(int p_chipID, List<ItemConsumptionInfo> p_itemConsumptionList, Callback p_cb = null)
	{
		WebService.SendRequest(new UpgradeLevelChipReq
		{
			ChipID = p_chipID,
			ItemConsumptionList = p_itemConsumptionList
		}, delegate(UpgradeLevelChipRes res)
		{
			if (res.ChipInfo != null)
			{
				dicChip.Value(res.ChipInfo.ChipID).netChipInfo = res.ChipInfo;
			}
			foreach (NetItemInfo item in res.ItemList)
			{
				dicItem.Value(item.ItemID).netItemInfo = item;
			}
			p_cb.CheckTargetToInvoke();
		});
	}

	public void UpgradeStarChipReq(int p_chipID, Callback p_cb = null)
	{
		WebService.SendRequest(new UpgradeStarChipReq
		{
			ChipID = p_chipID
		}, delegate(UpgradeStarChipRes res)
		{
			if (res.ChipInfo != null)
			{
				dicChip.Value(res.ChipInfo.ChipID).netChipInfo = res.ChipInfo;
			}
			foreach (NetItemInfo item in res.ItemList)
			{
				dicItem.Value(item.ItemID).netItemInfo = item;
			}
			p_cb.CheckTargetToInvoke();
		});
	}

	public void ChipAnalyseReq(int p_chipID, Callback p_cb = null)
	{
		WebService.SendRequest(new AnalyseChipReq
		{
			ChipID = p_chipID
		}, delegate(AnalyseChipRes res)
		{
			if (res.ChipInfo != null)
			{
				dicChip.Value(res.ChipInfo.ChipID).netChipInfo = res.ChipInfo;
			}
			foreach (NetItemInfo item in res.ItemList)
			{
				dicItem.Value(item.ItemID).netItemInfo = item;
			}
			p_cb.CheckTargetToInvoke();
		});
	}

	public void RetrieveNewMailCountReq(Callback p_cb = null)
	{
		WebService.SendRequest(new RetrieveNewMailCountReq(), delegate(RetrieveNewMailCountRes res)
		{
			dicMail.Clear();
			if (res.Count > 0)
			{
				dicMail.Add(0, new MailInfo());
			}
			p_cb.CheckTargetToInvoke();
		});
	}

	public void RetrieveMailListReq(Callback p_cb = null, Callback p_cb2 = null)
	{
		WebService.SendRequest(new RetrieveMailListReq(), delegate(RetrieveMailListRes res)
		{
			if (res.Code == 23052)
			{
				p_cb2.CheckTargetToInvoke();
			}
			else
			{
				dicMail.Clear();
				foreach (NetMailInfo mail in res.MailList)
				{
					dicMail.Value(mail.MailID).netMailInfo = mail;
				}
				foreach (NetExtraSystemContext systemContext in res.SystemContextList)
				{
					dicSystemContext.Value(systemContext.SystemTextID).Value((Language)systemContext.Language).netSystemContext = systemContext;
				}
				p_cb.CheckTargetToInvoke();
			}
		});
	}

	public void RetrieveMailItemListReq(List<int> listMailID, Action<List<NetRewardInfo>> p_cb = null, Callback p_cb2 = null)
	{
		WebService.SendRequest(new RetrieveMailItemReq
		{
			MailIDList = listMailID
		}, delegate(RetrieveMailItemRes res)
		{
			if (res.Code == 23052)
			{
				p_cb2.CheckTargetToInvoke();
			}
			else
			{
				foreach (int mailID in res.MailIDList)
				{
					if (dicMail.ContainsKey(mailID))
					{
						MailInfo mailInfo = dicMail[mailID];
						dicMail.Remove(mailID);
						mailInfo.netMailInfo.ReservedTime = res.ReservedTime;
						if (!dicReservedMail.ContainsKey(mailID))
						{
							dicReservedMail.Add(mailID, mailInfo);
						}
					}
				}
				if (res.PlayerInfo != null)
				{
					playerInfo.netPlayerInfo = res.PlayerInfo;
				}
				if (res.RewardEntities != null)
				{
					ComposeRewardEntities(res.RewardEntities);
					p_cb.CheckTargetToInvoke(res.RewardEntities.RewardList);
				}
			}
		});
	}

	public void UnlockResearchSlotReq(ResearchSlot slot, Callback p_cb = null)
	{
		WebService.SendRequest(new UnlockResearchSlotReq
		{
			Slot = (int)slot
		}, delegate(UnlockResearchSlotRes res)
		{
			if (res.Code == 21100)
			{
				if (res.ResearchInfo != null)
				{
					researchInfo.dicResearch.ContainsAdd(res.ResearchInfo.Slot, res.ResearchInfo);
				}
				foreach (NetItemInfo item in res.ItemList)
				{
					dicItem.Value(item.ItemID).netItemInfo = item;
				}
				p_cb.CheckTargetToInvoke();
			}
		});
	}

	public void RetrieveFreeResearch(int researchID, Action<List<NetRewardInfo>> p_cb = null)
	{
		WebService.SendRequest(new ReceiveFreeResearchReq
		{
			ResearchID = researchID
		}, delegate(ReceiveFreeResearchRes res)
		{
			if (res.Code == 21000)
			{
				foreach (NetItemInfo item in res.ItemList)
				{
					dicItem.Value(item.ItemID).netItemInfo = item;
				}
				if (res.PlayerInfo != null)
				{
					playerInfo.netPlayerInfo = res.PlayerInfo;
				}
				if (res.FreeResearchInfo != null)
				{
					researchInfo.listFreeResearch.RemoveAll((NetFreeResearchInfo x) => x.ResearchID == res.FreeResearchInfo.ResearchID);
					researchInfo.listFreeResearch.Add(res.FreeResearchInfo);
				}
				if (res.RewardEntities != null)
				{
					ComposeRewardEntities(res.RewardEntities);
					p_cb.CheckTargetToInvoke(res.RewardEntities.RewardList);
				}
			}
		});
	}

	public void ResearchStart(int researchID, ResearchSlot slot, Callback p_cb = null)
	{
		WebService.SendRequest(new ResearchStartReq
		{
			ResearchID = researchID,
			Slot = (sbyte)slot
		}, delegate(ResearchStartRes res)
		{
			if (res.Code == 21200)
			{
				foreach (NetItemInfo item in res.ItemList)
				{
					dicItem.Value(item.ItemID).netItemInfo = item;
				}
				if (res.ResearchInfo != null)
				{
					researchInfo.dicResearch.ContainsAdd(res.ResearchInfo.Slot, res.ResearchInfo);
				}
				if (res.ResearchRecord != null)
				{
					researchInfo.listResearchRecord.RemoveAll((NetResearchRecord x) => x.ResearchID == res.ResearchRecord.ResearchID);
					researchInfo.listResearchRecord.Add(res.ResearchRecord);
				}
				p_cb.CheckTargetToInvoke();
			}
		});
	}

	public void ReceiveResearch(ResearchSlot slot, bool isBoost, Action<List<NetRewardInfo>> p_cb = null)
	{
		WebService.SendRequest(new ReceiveResearchReq
		{
			Slot = (sbyte)slot,
			IsBoost = (sbyte)(isBoost ? 1 : 0)
		}, delegate(ReceiveResearchRes res)
		{
			if (res.Code == 21300)
			{
				foreach (NetItemInfo item in res.ItemList)
				{
					dicItem.Value(item.ItemID).netItemInfo = item;
				}
				if (res.ResearchInfo != null)
				{
					researchInfo.dicResearch.ContainsAdd(res.ResearchInfo.Slot, res.ResearchInfo);
				}
				if (res.RewardEntities != null)
				{
					ComposeRewardEntities(res.RewardEntities);
					p_cb.CheckTargetToInvoke(res.RewardEntities.RewardList);
				}
			}
		});
	}

	public void ComposeRewardEntities(NetRewardsEntity entities)
	{
		foreach (NetItemInfo item in entities.ItemList)
		{
			dicItem.Value(item.ItemID).netItemInfo = item;
		}
		foreach (NetWeaponInfo weapon in entities.WeaponList)
		{
			dicWeapon.Value(weapon.WeaponID).netInfo = weapon;
		}
		foreach (NetWeaponExpertInfo weaponExpert in entities.WeaponExpertList)
		{
			dicWeapon.Value(weaponExpert.WeaponID).AddNetWeaponExpertInfo(weaponExpert);
		}
		foreach (NetCharacterInfo character in entities.CharacterList)
		{
			dicCharacter.Value(character.CharacterID).netInfo = character;
		}
		foreach (NetCharacterSkillInfo characterSkill in entities.CharacterSkillList)
		{
			dicCharacter.Value(characterSkill.CharacterID).netSkillDic.ContainsAdd((CharacterSkillSlot)characterSkill.Slot, characterSkill);
		}
		foreach (NetEquipmentInfo equipment in entities.EquipmentList)
		{
			dicEquip.Value(equipment.EquipmentID).netEquipmentInfo = equipment;
		}
		foreach (NetChipInfo chip in entities.ChipList)
		{
			dicChip.Value(chip.ChipID).netChipInfo = chip;
		}
		foreach (NetCardInfo card in entities.CardList)
		{
			dicCard.Value(card.CardSeqID).netCardInfo = card;
		}
		Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.UPDATE_PLAYER_BOX);
	}

	public void RetrieveOperationDeliveryItemReq(Action<List<NetRewardInfo>> p_cb)
	{
		WebService.SendRequest(new RetrieveOperationDeliveryItemReq(), delegate(RetrieveOperationDeliveryItemRes res)
		{
			List<NetRewardInfo> obj = null;
			if (res.RewardEntities != null)
			{
				ComposeRewardEntities(res.RewardEntities);
				if (res.RewardEntities.RewardList != null)
				{
					obj = res.RewardEntities.RewardList;
				}
			}
			p_cb(obj);
		});
	}

	public void ItemUseReq(int p_itemID, int p_amount, int p_chosenGachaID, Action<List<NetRewardInfo>> p_cb = null)
	{
		WebService.SendRequest(new ItemUseReq
		{
			ItemID = p_itemID,
			Amount = p_amount,
			ChosenGachaID = p_chosenGachaID
		}, delegate(ItemUseRes res)
		{
			foreach (NetItemInfo item in res.ItemList)
			{
				dicItem.Value(item.ItemID).netItemInfo = item;
			}
			if (res.RewardEntities != null)
			{
				ComposeRewardEntities(res.RewardEntities);
				p_cb.CheckTargetToInvoke(res.RewardEntities.RewardList);
			}
		});
	}

	public void ItemSellReq(int p_itemID, int p_amount, Action<List<NetRewardInfo>> p_cb = null)
	{
		WebService.SendRequest(new ItemSellReq
		{
			ItemID = p_itemID,
			Amount = p_amount
		}, delegate(ItemSellRes res)
		{
			foreach (NetItemInfo item in res.ItemList)
			{
				dicItem.Value(item.ItemID).netItemInfo = item;
			}
			if (res.RewardEntities != null)
			{
				ComposeRewardEntities(res.RewardEntities);
				p_cb.CheckTargetToInvoke(res.RewardEntities.RewardList);
			}
		});
	}

	public void TransferDNAReq(List<NetDNATransferUnit> listNetDNATransferUnit, Action<List<NetRewardInfo>> p_cb = null)
	{
		WebService.SendRequest(new TransferDNAReq
		{
			TransferDNAList = listNetDNATransferUnit
		}, delegate(TransferDNARes res)
		{
			if (res.ItemList != null)
			{
				foreach (NetItemInfo item in res.ItemList)
				{
					dicItem.Value(item.ItemID).netItemInfo = item;
				}
			}
			if (res.RewardEntities != null)
			{
				ComposeRewardEntities(res.RewardEntities);
				p_cb.CheckTargetToInvoke(res.RewardEntities.RewardList);
			}
		});
	}

	public void RetrieveCompletedMissionReq(Callback p_cb = null)
	{
		WebService.SendRequest(new RetrieveCompletedMissionReq(), delegate(RetrieveCompletedMissionRes res)
		{
			foreach (NetMissionInfo completedMission in res.CompletedMissionList)
			{
				dicMission.Value(completedMission.MissionID).netMissionInfo = completedMission;
			}
			p_cb.CheckTargetToInvoke();
		});
	}

	public void ReceiveMissionRewardReq(List<int> listMissionID, Action<List<NetRewardInfo>> p_cb = null)
	{
		WebService.SendRequest(new ReceiveMissionRewardReq
		{
			MissionIDList = listMissionID
		}, delegate(ReceiveMissionRewardRes res)
		{
			if (res.PlayerInfo != null)
			{
				playerInfo.netPlayerInfo = res.PlayerInfo;
			}
			foreach (NetMissionInfo completedMission in res.CompletedMissionList)
			{
				dicMission.Value(completedMission.MissionID).netMissionInfo = completedMission;
			}
			if (res.RewardEntities != null)
			{
				ComposeRewardEntities(res.RewardEntities);
				p_cb.CheckTargetToInvoke(res.RewardEntities.RewardList);
			}
		});
	}

	public void SetMissionProgress(OrangeGameResponse res)
	{
		foreach (NetMissionProgressInfo missionProgress in res.MissionProgressList)
		{
			dicMissionProgress.Value(missionProgress.CounterID).netMissionProgressInfo = missionProgress;
		}
	}

	public void TurtorialFlagRq(int tutorialFlag, Callback p_cb = null)
	{
		if (TutorialList.Contains(tutorialFlag))
		{
			p_cb.CheckTargetToInvoke();
			return;
		}
		WebService.SendRequest(new SetTutorialFlagReq
		{
			Flag = tutorialFlag
		}, delegate(SetTutorialFlagRes res)
		{
			if (!TutorialList.Contains(res.Flag))
			{
				TutorialList.Add(res.Flag);
				TutorialList.Sort();
			}
			p_cb.CheckTargetToInvoke();
		});
	}

	private void a(IRequest tIRequest)
	{
		bool flag = tIRequest is SetTutorialFlagReq;
	}

	public void RetrieveTutorialItemReq(int tutorialFlag, Callback p_cb = null)
	{
		if (TutorialList.Contains(tutorialFlag))
		{
			p_cb.CheckTargetToInvoke();
			return;
		}
		WebService.SendRequest(new RetrieveTutorialItemReq
		{
			Flag = tutorialFlag
		}, delegate(RetrieveTutorialItemRes res)
		{
			foreach (NetItemInfo item in res.ItemList)
			{
				dicItem.Value(item.ItemID).netItemInfo = item;
			}
			p_cb.CheckTargetToInvoke();
		});
	}

	public void RetrieveUseChatItemReq(int ChannelType, Action<RetrieveUseChatItemRes> p_cb = null)
	{
		WebService.SendRequest(new RetrieveUseChatItemReq
		{
			Channel = ChannelType
		}, delegate(RetrieveUseChatItemRes res)
		{
			foreach (NetItemInfo item in res.ItemList)
			{
				dicItem.Value(item.ItemID).netItemInfo = item;
			}
			p_cb.CheckTargetToInvoke(res);
		});
	}

	public void ComposeLaboEventReq(int typ, List<int> cntList, Action<ComposeLaboEventRes> p_cb = null)
	{
		WebService.SendRequest(new ComposeLaboEventReq
		{
			LaboEventItemType = typ,
			LaboEventItemCountList = cntList
		}, delegate(ComposeLaboEventRes res)
		{
			foreach (NetItemInfo item in res.ItemList)
			{
				dicItem.Value(item.ItemID).netItemInfo = item;
			}
			if (res.RewardEntities != null)
			{
				ComposeRewardEntities(res.RewardEntities);
				p_cb.CheckTargetToInvoke(res);
			}
		});
	}

	public void GetPersonnelLaboEventRankingReq(int _id, Action<GetPersonnelLaboEventRankingRes> p_cb = null)
	{
		WebService.SendRequest(new GetPersonnelLaboEventRankingReq
		{
			LaboEventID = _id
		}, delegate(GetPersonnelLaboEventRankingRes res)
		{
			p_cb.CheckTargetToInvoke(res);
		});
	}

	public void GetLaboEventRankingReq(int _id, int _start, int _stop, Action<GetLaboEventRankingRes> p_cb = null)
	{
		WebService.SendRequest(new GetLaboEventRankingReq
		{
			LaboEventID = _id,
			RankingStart = _start,
			RankingStop = _stop
		}, delegate(GetLaboEventRankingRes res)
		{
			p_cb.CheckTargetToInvoke(res);
		});
	}

	public void RetrieveEventRankingReq(int eventID, int startRanking, int stopRanking, Action<List<EventRankingInfo>> p_cb = null)
	{
		WebService.SendRequest(new RetrieveEventRankingReq
		{
			EventID = eventID,
			StartRanking = startRanking,
			StopRanking = stopRanking
		}, delegate(RetrieveEventRankingRes res)
		{
			p_cb.CheckTargetToInvoke(res.EventRankingList);
		});
	}

	public void RetrievePersonnelEventRankingReq(int eventID, Action<EventRankingInfo> p_cb = null)
	{
		WebService.SendRequest(new RetrievePersonnelEventRankingReq
		{
			EventID = eventID
		}, delegate(RetrievePersonnelEventRankingRes res)
		{
			p_cb.CheckTargetToInvoke(res.EventRanking);
		});
	}

	public void TransferBackupItemReq(Action<List<NetRewardInfo>> p_cb = null)
	{
		WebService.SendRequest(new TransferBackupItemReq(), delegate(TransferBackupItemRes res)
		{
			if (res.ItemList != null)
			{
				foreach (NetItemInfo item in res.ItemList)
				{
					dicItem.Value(item.ItemID).netItemInfo = item;
				}
				if (res.RewardEntities != null)
				{
					ComposeRewardEntities(res.RewardEntities);
					p_cb.CheckTargetToInvoke(res.RewardEntities.RewardList);
				}
			}
		});
	}

	public void UpgradeBenchSlotReq(int _bid, Action<UpgradeBenchSlotRes> p_cb = null)
	{
		WebService.SendRequest(new UpgradeBenchSlotReq
		{
			BenchID = _bid
		}, delegate(UpgradeBenchSlotRes res)
		{
			if (res.ItemList != null)
			{
				foreach (NetItemInfo item in res.ItemList)
				{
					dicItem.Value(item.ItemID).netItemInfo = item;
				}
			}
			if (res.BenchWeaponInfo != null)
			{
				dicBenchWeaponInfo.Value(res.BenchWeaponInfo.BenchSlot).netBenchInfo = res.BenchWeaponInfo;
			}
			p_cb.CheckTargetToInvoke(res);
		});
	}

	public void SetBenchWeaponReq(int _slt, int _wid, Action<SetBenchWeaponRes> p_cb = null)
	{
		WebService.SendRequest(new SetBenchWeaponReq
		{
			BenchSlot = _slt,
			WeaponID = _wid
		}, delegate(SetBenchWeaponRes res)
		{
			foreach (NetBenchInfo benchWeaponInfo in res.BenchWeaponInfoList)
			{
				dicBenchWeaponInfo.Value(benchWeaponInfo.BenchSlot).netBenchInfo = benchWeaponInfo;
			}
			p_cb.CheckTargetToInvoke(res);
		});
	}

	public void RetrieveTAStageInfoReq(Action<RetrieveTAStageInfoRes> p_cb = null)
	{
		WebService.SendRequest(new RetrieveTAStageInfoReq(), delegate(RetrieveTAStageInfoRes res)
		{
			p_cb.CheckTargetToInvoke(res);
		});
	}

	public void RetrieveRaidBossInfoReq(Action<RetrieveRaidBossInfoRes> p_cb = null)
	{
		WebService.SendRequest(new RetrieveRaidBossInfoReq(), delegate(RetrieveRaidBossInfoRes res)
		{
			if (res.RewardEntities != null)
			{
				ComposeRewardEntities(res.RewardEntities);
			}
			p_cb.CheckTargetToInvoke(res);
		});
	}

	public void CheckRaidBossInfoReq(Action<CheckRaidBossInfoRes> p_cb = null)
	{
		WebService.SendRequest(new CheckRaidBossInfoReq(), delegate(CheckRaidBossInfoRes res)
		{
			if (p_cb != null)
			{
				p_cb(res);
			}
		});
	}

	public void RaidBossStartReq(int nStageID, Action p_cb)
	{
		WebService.SendRequest<RaidBossStartRes>(new RaidBossStartReq
		{
			StageID = nStageID
		}, delegate
		{
			if (p_cb != null)
			{
				p_cb();
			}
		});
	}

	public void RaidBossLimitResetReq(Action<RaidBossLimitResetRes> p_cb)
	{
		WebService.SendRequest(new RaidBossLimitResetReq(), delegate(RaidBossLimitResetRes res)
		{
			foreach (NetItemInfo item in res.ItemList)
			{
				dicItem.Value(item.ItemID).netItemInfo = item;
			}
			if (ManagedSingleton<PlayerNetManager>.Instance.dicCharge.ContainsKey(res.ChargeInfo.ChargeType))
			{
				ManagedSingleton<PlayerNetManager>.Instance.dicCharge[res.ChargeInfo.ChargeType].netChargeInfo = res.ChargeInfo;
			}
			else
			{
				ManagedSingleton<PlayerNetManager>.Instance.dicCharge.Add(res.ChargeInfo.ChargeType, new ChargeInfo());
				ManagedSingleton<PlayerNetManager>.Instance.dicCharge[res.ChargeInfo.ChargeType].netChargeInfo = res.ChargeInfo;
			}
			p_cb.CheckTargetToInvoke(res);
		});
	}

	public void RetrieveTotalWarInfoReq(Action<RetrieveTotalWarInfoRes> p_cb = null)
	{
		WebService.SendRequest(new RetrieveTotalWarInfoReq(), delegate(RetrieveTotalWarInfoRes res)
		{
			p_cb.CheckTargetToInvoke(res);
		});
	}

	public void TotalWarRecordReplaceReq(bool bReplace, Action<TotalWarRecordReplaceRes> p_cb = null)
	{
		WebService.SendRequest(new TotalWarRecordReplaceReq
		{
			Replaced = (sbyte)(bReplace ? 1 : 0)
		}, delegate(TotalWarRecordReplaceRes res)
		{
			p_cb.CheckTargetToInvoke(res);
		});
	}

	public void ExpandCardStorageReq(int Amount, Callback p_cb = null)
	{
		WebService.SendRequest(new ExpandCardStorageReq
		{
			Amount = (sbyte)Amount
		}, delegate(ExpandCardStorageRes res)
		{
			if (res.ItemList != null)
			{
				foreach (NetItemInfo item in res.ItemList)
				{
					dicItem.Value(item.ItemID).netItemInfo = item;
				}
			}
			p_cb.CheckTargetToInvoke();
		});
	}

	public void ProtectedCardReq(int CardSeqID, int SetStatus, Callback p_cb = null)
	{
		WebService.SendRequest(new ProtectedCardReq
		{
			CardSeqID = CardSeqID,
			SetStatus = (sbyte)SetStatus
		}, delegate(ProtectedCardRes res)
		{
			if (res.CardInfo != null)
			{
				dicCard.Value(res.CardInfo.CardSeqID).netCardInfo = res.CardInfo;
			}
			p_cb.CheckTargetToInvoke();
		});
	}

	public void FavoriteCardReq(int CardSeqID, int SetStatus, Callback p_cb = null)
	{
		WebService.SendRequest(new FavoriteCardReq
		{
			CardSeqID = CardSeqID,
			SetStatus = (sbyte)SetStatus
		}, delegate(FavoriteCardRes res)
		{
			if (res.CardInfo != null)
			{
				dicCard.Value(res.CardInfo.CardSeqID).netCardInfo = res.CardInfo;
			}
			p_cb.CheckTargetToInvoke();
		});
	}

	public void CardSellReq(List<int> CardSeqIDList, Action<List<NetRewardInfo>> p_cb = null)
	{
		WebService.SendRequest(new CardSellReq
		{
			CardSeqIDList = CardSeqIDList
		}, delegate(CardSellRes res)
		{
			if (res.CardSeqIDList != null)
			{
				foreach (int cardSeqID in res.CardSeqIDList)
				{
					dicCard.Remove(cardSeqID);
				}
			}
			if (res.ItemInfo != null && res.ItemInfo.ItemID == OrangeConst.ITEMID_MONEY)
			{
				int stack = dicItem[OrangeConst.ITEMID_MONEY].netItemInfo.Stack;
				int stack2 = res.ItemInfo.Stack;
				dicItem.Value(res.ItemInfo.ItemID).netItemInfo = res.ItemInfo;
				List<NetRewardInfo> p_param = new List<NetRewardInfo>
				{
					new NetRewardInfo
					{
						RewardID = OrangeConst.ITEMID_MONEY,
						Amount = stack2 - stack,
						RewardType = 1
					}
				};
				p_cb.CheckTargetToInvoke(p_param);
			}
		});
	}

	public void CardFusionReq(int CardSeqID, List<int> CardSeqIDList, Callback p_cb = null)
	{
		WebService.SendRequest(new CardFusionReq
		{
			CardSeqID = CardSeqID,
			MaterialCardSeqID = CardSeqIDList
		}, delegate(CardFusionRes res)
		{
			if (res.CardSeqIDList != null)
			{
				foreach (int cardSeqID in res.CardSeqIDList)
				{
					dicCard.Remove(cardSeqID);
				}
			}
			if (res.CardInfo != null)
			{
				dicCard.Value(res.CardInfo.CardSeqID).netCardInfo = res.CardInfo;
			}
			p_cb.CheckTargetToInvoke();
		});
	}

	public void CardEnhanceByItemReq(int CardSeqID, List<ItemConsumptionInfo> p_itemConsumptionList, Callback p_cb = null)
	{
		WebService.SendRequest(new CardEnhanceByItemReq
		{
			CardSeqID = CardSeqID,
			ItemConsumptionList = p_itemConsumptionList
		}, delegate(CardEnhanceByItemRes res)
		{
			foreach (NetItemInfo item in res.ItemList)
			{
				dicItem.Value(item.ItemID).netItemInfo = item;
			}
			if (res.CardInfo != null)
			{
				dicCard.Value(res.CardInfo.CardSeqID).netCardInfo = res.CardInfo;
			}
			p_cb.CheckTargetToInvoke();
		});
	}

	public void EquipCardReq(int CharacterID, int CharacterCardSlot, int CardSeqID, Callback p_cb = null)
	{
		WebService.SendRequest(new EquipCardReq
		{
			CharacterID = CharacterID,
			CharacterCardSlot = (sbyte)CharacterCardSlot,
			CardSeqID = CardSeqID
		}, delegate(EquipCardRes res)
		{
			if (res.CharacterCardSlotInfoList != null)
			{
				foreach (NetCharacterCardSlotInfo characterCardSlotInfo in res.CharacterCardSlotInfoList)
				{
					dicCharacterCardSlotInfo.Value(characterCardSlotInfo.CharacterID).Value(characterCardSlotInfo.CharacterCardSlot).CardSeqID = characterCardSlotInfo.CardSeqID;
					dicCharacterCardSlotInfo.Value(characterCardSlotInfo.CharacterID).Value(characterCardSlotInfo.CharacterCardSlot).CharacterCardSlot = characterCardSlotInfo.CharacterCardSlot;
					dicCharacterCardSlotInfo.Value(characterCardSlotInfo.CharacterID).Value(characterCardSlotInfo.CharacterCardSlot).CharacterID = characterCardSlotInfo.CharacterID;
				}
			}
			p_cb.CheckTargetToInvoke();
		});
	}

	public void SetCardDeployReq(int DeployId, List<int> CardSeqIDList, Callback p_cb = null)
	{
		WebService.SendRequest(new SetCardDeployReq
		{
			DeployId = DeployId,
			CardSeqIDList = CardSeqIDList
		}, delegate(SetCardDeployRes res)
		{
			if (res.CardDeployInfoList != null)
			{
				foreach (NetCardDeployInfo cardDeployInfo in res.CardDeployInfoList)
				{
					dicCardDeployInfo.Value(cardDeployInfo.DeployId).Value(cardDeployInfo.CardSlot).DeployId = cardDeployInfo.DeployId;
					dicCardDeployInfo.Value(cardDeployInfo.DeployId).Value(cardDeployInfo.CardSlot).CardSlot = cardDeployInfo.CardSlot;
					dicCardDeployInfo.Value(cardDeployInfo.DeployId).Value(cardDeployInfo.CardSlot).CardSeqID = cardDeployInfo.CardSeqID;
				}
			}
			p_cb.CheckTargetToInvoke();
		});
	}

	public void SetCardDeployNameReq(int DeployId, string SlotName, Callback p_cb = null)
	{
		WebService.SendRequest(new SetCardDeployNameReq
		{
			DeployId = DeployId,
			SlotName = SlotName
		}, delegate(SetCardDeployNameRes res)
		{
			if (res.SlotName != null)
			{
				if (dicCardDeployNameInfo.ContainsKey(res.DeployId))
				{
					dicCardDeployNameInfo[res.DeployId] = res.SlotName;
				}
				else
				{
					dicCardDeployNameInfo.Add(res.DeployId, res.SlotName);
				}
			}
			p_cb.CheckTargetToInvoke();
		});
	}

	public void ExpandCardDeploySlotReq(int Amount, Callback p_cb = null)
	{
		WebService.SendRequest(new ExpandCardDeploySlotReq
		{
			Amount = (sbyte)Amount
		}, delegate(ExpandCardDeploySlotRes res)
		{
			if (res.ItemList != null)
			{
				foreach (NetItemInfo item in res.ItemList)
				{
					dicItem.Value(item.ItemID).netItemInfo = item;
				}
			}
			p_cb.CheckTargetToInvoke();
		});
	}

	public void EquipCardDeployReq(int CharacterID, List<int> CardSeqIDList, Callback p_cb = null)
	{
		WebService.SendRequest(new EquipCardDeployReq
		{
			CharacterID = CharacterID,
			CardSeqIDList = CardSeqIDList
		}, delegate(EquipCardDeployRes res)
		{
			if (res.CharacterCardSlotInfoList != null)
			{
				foreach (NetCharacterCardSlotInfo characterCardSlotInfo in res.CharacterCardSlotInfoList)
				{
					dicCharacterCardSlotInfo.Value(characterCardSlotInfo.CharacterID).Value(characterCardSlotInfo.CharacterCardSlot).CardSeqID = characterCardSlotInfo.CardSeqID;
					dicCharacterCardSlotInfo.Value(characterCardSlotInfo.CharacterID).Value(characterCardSlotInfo.CharacterCardSlot).CharacterCardSlot = characterCardSlotInfo.CharacterCardSlot;
					dicCharacterCardSlotInfo.Value(characterCardSlotInfo.CharacterID).Value(characterCardSlotInfo.CharacterCardSlot).CharacterID = characterCardSlotInfo.CharacterID;
				}
			}
			p_cb.CheckTargetToInvoke();
		});
	}

	public void CardResetReq(List<int> CardSeqIDList, Action<List<NetRewardInfo>> p_cb = null)
	{
		WebService.SendRequest(new CardResetReq
		{
			CardSeqIDList = CardSeqIDList
		}, delegate(CardResetRes res)
		{
			if (res.ItemList != null)
			{
				foreach (NetItemInfo item in res.ItemList)
				{
					dicItem.Value(item.ItemID).netItemInfo = item;
				}
			}
			if (res.CardInfoList != null)
			{
				foreach (NetCardInfo cardInfo in res.CardInfoList)
				{
					dicCard.Value(cardInfo.CardSeqID).netCardInfo = cardInfo;
				}
			}
			if (res.RewardEntities != null)
			{
				ComposeRewardEntities(res.RewardEntities);
			}
			p_cb.CheckTargetToInvoke(res.RewardEntities.RewardList);
		});
	}

	public void RetrieveRecordGridInfoReq(Callback<RetrieveRecordGridInfoRes> p_cb)
	{
		WebService.SendRequest(new RetrieveRecordGridInfoReq(), delegate(RetrieveRecordGridInfoRes res)
		{
			p_cb(res);
		});
	}

	public void InitializeRecordGrid(List<int> CharacterList, List<int> WeaponList, Callback<InitializeRecordGridRes> p_cb)
	{
		WebService.SendRequest(new InitializeRecordGridReq
		{
			CharacterList = CharacterList,
			WeaponList = WeaponList
		}, delegate(InitializeRecordGridRes res)
		{
			p_cb(res);
		});
	}

	public void ChallengeRecordGridReq(List<NetCommonCoordinateInfo> listMovePoint, Callback<ChallengeRecordGridRes> p_cb)
	{
		WebService.SendRequest(new ChallengeRecordGridReq
		{
			TargetPosition = listMovePoint
		}, delegate(ChallengeRecordGridRes res)
		{
			p_cb(res);
		});
	}

	public void ChallengeMultiRecordGridReq(List<NetCommonCoordinateInfo> listMovePoint, Callback<ChallengeMultiRecordGridRes> p_cb)
	{
		WebService.SendRequest(new ChallengeMultiRecordGridReq
		{
			TargetPosition = listMovePoint
		}, delegate(ChallengeMultiRecordGridRes res)
		{
			p_cb(res);
		});
	}

	public void RetrieveRecordGridBattleLogReq(int p_offeset, Callback<RetrieveRecordGridBattleLogRes> p_cb)
	{
		WebService.SendRequest(new RetrieveRecordGridBattleLogReq
		{
			Offset = p_offeset
		}, delegate(RetrieveRecordGridBattleLogRes res)
		{
			p_cb(res);
		});
	}

	public void RetrieveRecordGridAbilityLogReq(int p_offeset, Callback<RetrieveRecordGridAbilityLogRes> p_cb)
	{
		WebService.SendRequest(new RetrieveRecordGridAbilityLogReq
		{
			Offset = p_offeset
		}, delegate(RetrieveRecordGridAbilityLogRes res)
		{
			p_cb(res);
		});
	}

	public void RetrieveRecordGridRandomLogReq(int p_offeset, Callback<RetrieveRecordGridRandomLogRes> p_cb)
	{
		WebService.SendRequest(new RetrieveRecordGridRandomLogReq
		{
			Offset = p_offeset
		}, delegate(RetrieveRecordGridRandomLogRes res)
		{
			p_cb(res);
		});
	}

	public void GuildReqCheckGuildState(Action<GuildCheckGuildStateRes> p_cb = null)
	{
		WebService.SendRequest(new GuildCheckGuildStateReq(), p_cb);
	}

	public void GuildReqGetGuildInfo(Action<GuildGetGuildInfoRes> p_cb = null)
	{
		WebService.SendRequest(new GuildGetGuildInfoReq(), p_cb);
	}

	public void GuildReqGetMemberInfoList(Action<GuildGetGuildMemberListRes> p_cb = null)
	{
		WebService.SendRequest(new GuildGetGuildMemberListReq(), p_cb);
	}

	public void GuildReqGetInvitePlayerList(Action<GuildGetInvitePlayerListRes> p_cb = null)
	{
		WebService.SendRequest(new GuildGetInvitePlayerListReq(), p_cb);
	}

	public void GuildReqGetApplyPlayerList(Action<GuildGetApplyPlayerListRes> p_cb = null)
	{
		WebService.SendRequest(new GuildGetApplyPlayerListReq(), p_cb);
	}

	public void GuildReqAgreePlayerApply(List<string> playerIdList, Action<GuildAgreePlayerJoinRes> p_cb = null)
	{
		WebService.SendRequest(new GuildAgreePlayerJoinReq
		{
			PlayerIDList = playerIdList
		}, p_cb);
	}

	public void GuildReqRefusePlayerApply(List<string> playerIdList, Action<GuildRefusePlayerJoinRes> p_cb = null)
	{
		WebService.SendRequest(new GuildRefusePlayerJoinReq
		{
			PlayerIDList = playerIdList
		}, p_cb);
	}

	public void GuildReqEditBadge(int badgeIndex, int badgeColor, Action<GuildEditBadgeRes> p_cb = null)
	{
		WebService.SendRequest(new GuildEditBadgeReq
		{
			Badge = badgeIndex,
			BadgeColor = badgeColor
		}, delegate(GuildEditBadgeRes res)
		{
			UpdateItemInfoList(res.ItemList);
			Action<GuildEditBadgeRes> action = p_cb;
			if (action != null)
			{
				action(res);
			}
		});
	}

	public void GuildReqCreateGuild(string guildName, string introduction, string board, int badgeIndex, int badgeColor, int applyType, int powerDemand, Action<GuildCreateGuildRes> p_cb = null)
	{
		WebService.SendRequest(new GuildCreateGuildReq
		{
			GuildName = guildName,
			Introduction = introduction,
			Board = board,
			Badge = badgeIndex,
			BadgeColor = badgeColor,
			ApplyType = applyType,
			PowerDemand = powerDemand
		}, delegate(GuildCreateGuildRes res)
		{
			UpdateItemInfoList(res.ItemList);
			Action<GuildCreateGuildRes> action = p_cb;
			if (action != null)
			{
				action(res);
			}
		});
	}

	public void GuildReqJoinGuild(int guildId, int power, string applyMsg, Action<GuildJoinGuildRes> p_cb = null)
	{
		WebService.SendRequest(new GuildJoinGuildReq
		{
			GuildID = guildId,
			TotalPower = power,
			ApplyMessage = applyMsg
		}, p_cb);
	}

	public void GuildReqCancelJoinGuild(int guildId, Action<GuildCancelJoinGuildRes> p_cb = null)
	{
		WebService.SendRequest(new GuildCancelJoinGuildReq
		{
			GuildID = guildId
		}, p_cb);
	}

	public void GuildReqRankupGuild(Action<GuildRankUpRes> p_cb = null)
	{
		WebService.SendRequest(new GuildRankUpReq(), p_cb);
	}

	public void GuildReqLeaveGuild(Action<GuildLeaveGuildRes> p_cb = null)
	{
		WebService.SendRequest(new GuildLeaveGuildReq(), p_cb);
	}

	public void GuildReqRemoveGuild(Action<GuildRemoveGuildRes> p_cb = null)
	{
		WebService.SendRequest(new GuildRemoveGuildReq(), p_cb);
	}

	public void GuildReqSearchGuild(string nam, int offset, int maxPower, Action<GuildSearchGuildRes> p_cb = null)
	{
		WebService.SendRequest(new GuildSearchGuildReq
		{
			SearchName = nam,
			Offset = offset,
			TotalPower = maxPower
		}, p_cb);
	}

	public void GuildReqRefusePlayerApply(Action<GuildRefusePlayerJoinRes> p_cb = null)
	{
		WebService.SendRequest(new GuildRefusePlayerJoinReq(), p_cb);
	}

	public void GuildReqChangeGuildName(string guildName, Action<GuildChangeNameRes> p_cb = null)
	{
		WebService.SendRequest(new GuildChangeNameReq
		{
			GuildName = guildName
		}, delegate(GuildChangeNameRes res)
		{
			UpdateItemInfoList(res.ItemList);
			Action<GuildChangeNameRes> action = p_cb;
			if (action != null)
			{
				action(res);
			}
		});
	}

	public void GuildReqChangeGuildPowerDemand(int powerDemand, Action<GuildChangePowerDemandRes> p_cb = null)
	{
		WebService.SendRequest(new GuildChangePowerDemandReq
		{
			PowerDemand = powerDemand
		}, p_cb);
	}

	public void GuildReqChangeApplyType(int applyType, Action<GuildChangeApplyTypeRes> p_cb = null)
	{
		WebService.SendRequest(new GuildChangeApplyTypeReq
		{
			ApplyType = applyType
		}, p_cb);
	}

	public void GuildReqChangeAnnouncement(string announcement, Action<GuildChangeBoardRes> p_cb = null)
	{
		WebService.SendRequest(new GuildChangeBoardReq
		{
			Board = announcement
		}, p_cb);
	}

	public void GuildReqChangeIntroduction(string introduction, Action<GuildChangeIntroductionRes> p_cb = null)
	{
		WebService.SendRequest(new GuildChangeIntroductionReq
		{
			Introduction = introduction
		}, p_cb);
	}

	public void GuildReqGetApplyGuildList(Action<GuildGetApplyGuildListRes> p_cb = null)
	{
		WebService.SendRequest(new GuildGetApplyGuildListReq(), p_cb);
	}

	public void GuildReqKickMember(string playerId, Action<GuildKickMemberRes> p_cb = null)
	{
		WebService.SendRequest(new GuildKickMemberReq
		{
			PlayerID = playerId
		}, p_cb);
	}

	public void GuildReqChangeMemberPrivilege(string playerId, int privilege, Action<GuildChangePrivilegeRes> p_cb = null)
	{
		WebService.SendRequest(new GuildChangePrivilegeReq
		{
			PlayerID = playerId,
			Privilege = privilege
		}, p_cb);
	}

	public void GuildReqChangeHeaderPower(int headerPower, Action<GuildChangeHeaderPowerRes> p_cb = null)
	{
		WebService.SendRequest(new GuildChangeHeaderPowerReq
		{
			HeaderPower = headerPower
		}, p_cb);
	}

	public void GuildReqInvitePlayer(string playerId, string inviteMsg, Action<GuildInvitePlayerRes> p_cb = null)
	{
		WebService.SendRequest(new GuildInvitePlayerReq
		{
			PlayerID = playerId,
			InviteMessage = inviteMsg
		}, p_cb);
	}

	public void GuildReqCancelInvitePlayer(List<string> playerIdList, Action<GuildCancelInvitePlayerRes> p_cb = null)
	{
		WebService.SendRequest(new GuildCancelInvitePlayerReq
		{
			PlayerIDList = playerIdList
		}, p_cb);
	}

	public void GuildReqGetInviteGuildList(Action<GuildGetInviteGuildListRes> p_cb = null)
	{
		WebService.SendRequest(new GuildGetInviteGuildListReq(), p_cb);
	}

	public void GuildReqAgreeGuildInvite(int guildId, Action<GuildAgreeGuildInviteRes> p_cb = null)
	{
		WebService.SendRequest(new GuildAgreeGuildInviteReq
		{
			GuildID = guildId
		}, p_cb);
	}

	public void GuildReqRefuseGuildInvite(int guildId, Action<GuildRefuseGuildInviteRes> p_cb = null)
	{
		WebService.SendRequest(new GuildRefuseGuildInviteReq
		{
			GuildID = guildId
		}, p_cb);
	}

	public void GuildReqGetLog(Action<GuildGetLogRes> p_cb = null)
	{
		WebService.SendRequest(new GuildGetLogReq(), p_cb);
	}

	public void GuildReqDonate(int amount, Action<GuildDonateRes> p_cb = null)
	{
		WebService.SendRequest(new GuildDonateReq
		{
			MoneyCount = amount
		}, delegate(GuildDonateRes res)
		{
			UpdateItemInfoList(res.ItemList);
			Action<GuildDonateRes> action = p_cb;
			if (action != null)
			{
				action(res);
			}
		});
	}

	public void GuildReqEddieDonate(int amount, Action<GuildEddieDonateRes> p_cb = null)
	{
		WebService.SendRequest(new GuildEddieDonateReq
		{
			MoneyCount = amount
		}, delegate(GuildEddieDonateRes res)
		{
			UpdateItemInfoList(res.ItemList);
			Action<GuildEddieDonateRes> action = p_cb;
			if (action != null)
			{
				action(res);
			}
		});
	}

	public void GuildReqRetrieveEddieBoxGachaRecord(Action<GuildRetrieveEddieBoxGachaRecordRes> p_cb = null)
	{
		WebService.SendRequest(new GuildRetrieveEddieBoxGachaRecordReq(), p_cb);
	}

	public void GuildReqReceiveEddieReward(Action<GuildReceiveEddieRewardRes> p_cb = null)
	{
		WebService.SendRequest(new GuildReceiveEddieRewardReq(), delegate(GuildReceiveEddieRewardRes res)
		{
			if (res.RewardEntities != null)
			{
				ComposeRewardEntities(res.RewardEntities);
			}
			Action<GuildReceiveEddieRewardRes> action = p_cb;
			if (action != null)
			{
				action(res);
			}
		});
	}

	public void GuildReqCheckReplaceLeader(Action<GuildCheckReplaceLeaderRes> p_cb = null)
	{
		WebService.SendRequest(new GuildCheckReplaceLeaderReq(), p_cb);
	}

	public void GuildReqGetGuildRankRead(Action<GuildGetReadGuildRankRes> p_cb = null)
	{
		WebService.SendRequest(new GuildGetReadGuildRankReq(), p_cb);
	}

	public void GuildReqSetGuildRankRead(int rank, Action<GuildSetReadGuildRankRes> p_cb = null)
	{
		WebService.SendRequest(new GuildSetReadGuildRankReq
		{
			ReadGuildRank = rank
		}, p_cb);
	}

	public void PowerTowerReqGetPowerPillarInfo(Action<GuildGetPowerPillarInfoRes> p_cb = null)
	{
		WebService.SendRequest(new GuildGetPowerPillarInfoReq(), p_cb);
	}

	public void PowerTowerReqOpenPowerPillar(int pillarId, int oreGroup, int oreLv, Action<GuildPowerPillarOpenRes> p_cb = null)
	{
		WebService.SendRequest(new GuildPowerPillarOpenReq
		{
			PillarID = pillarId,
			OreGroupID = oreGroup,
			OreLevel = oreLv
		}, delegate(GuildPowerPillarOpenRes res)
		{
			Action<GuildPowerPillarOpenRes> action = p_cb;
			if (action != null)
			{
				action(res);
			}
		});
	}

	public void PowerTowerReqClosePowerPillar(int oreId, Action<GuildPowerPillarCloseRes> p_cb = null)
	{
		WebService.SendRequest(new GuildPowerPillarCloseReq
		{
			OreID = oreId
		}, p_cb);
	}

	public void PowerTowerReqGetOreInfo(Action<GuildGetOreInfoRes> p_cb = null)
	{
		WebService.SendRequest(new GuildGetOreInfoReq(), p_cb);
	}

	public void PowerTowerReqOreLevelUp(int oreGroup, int oreLv, Action<GuildOreLevelUpRes> p_cb = null)
	{
		WebService.SendRequest(new GuildOreLevelUpReq
		{
			OreGroupID = oreGroup,
			OreLevel = oreLv
		}, delegate(GuildOreLevelUpRes res)
		{
			Action<GuildOreLevelUpRes> action = p_cb;
			if (action != null)
			{
				action(res);
			}
		});
	}

	public void PowerTowerReqRankup(Action<GuildPowerTowerRankUpRes> p_cb = null)
	{
		WebService.SendRequest(new GuildPowerTowerRankUpReq(), delegate(GuildPowerTowerRankUpRes res)
		{
			Action<GuildPowerTowerRankUpRes> action = p_cb;
			if (action != null)
			{
				action(res);
			}
		});
	}

	private void UpdatePlayerInfo(NetPlayerInfo netPlayerInfo)
	{
		if (netPlayerInfo != null)
		{
			playerInfo.netPlayerInfo = netPlayerInfo;
		}
	}

	private void UpdateItemInfoList(List<NetItemInfo> itemInfoList)
	{
		if (itemInfoList == null)
		{
			return;
		}
		foreach (NetItemInfo itemInfo in itemInfoList)
		{
			dicItem[itemInfo.ItemID].netItemInfo = itemInfo;
		}
	}

	private void UpdateCharacterInfoList(List<NetCharacterInfo> characterInfoList)
	{
		if (characterInfoList == null)
		{
			return;
		}
		foreach (NetCharacterInfo characterInfo in characterInfoList)
		{
			dicCharacter.Value(characterInfo.CharacterID).netInfo = characterInfo;
		}
	}

	private void UpdateWeaponInfoList(List<NetWeaponInfo> weaponInfoList)
	{
		if (weaponInfoList == null)
		{
			return;
		}
		foreach (NetWeaponInfo weaponInfo in weaponInfoList)
		{
			dicWeapon.Value(weaponInfo.WeaponID).netInfo = weaponInfo;
		}
	}

	private void UpdateStageInfo(NetStageInfo stageInfo)
	{
		if (stageInfo != null)
		{
			dicStage.Value(stageInfo.StageID).netStageInfo = stageInfo;
		}
	}

	private void UpdateChargeInfo(NetChargeInfo chargeInfo)
	{
		if (chargeInfo != null)
		{
			dicCharge.Value(chargeInfo.ChargeType).netChargeInfo = chargeInfo;
		}
	}

	public void WantedReqRetrieveWantedInfo(Action<RetrieveWantedInfoRes> p_cb = null)
	{
		WebService.SendRequest(new RetrieveWantedInfoReq(), p_cb);
	}

	public void WantedReqStart(sbyte slot, List<int> characterIdList, NetWantedHelpInfo helpInfo, Action<WantedStartRes> p_cb = null)
	{
		WebService.SendRequest(new WantedStartReq
		{
			Slot = slot,
			CharacterIDs = characterIdList,
			WantedHelpInfo = helpInfo
		}, delegate(WantedStartRes res)
		{
			UpdatePlayerInfo(res.PlayerInfo);
			Action<WantedStartRes> action = p_cb;
			if (action != null)
			{
				action(res);
			}
		});
	}

	public void WantedReqReceiveWanted(sbyte slot, sbyte isBoost, Action<ReceiveWantedRes> p_cb = null)
	{
		WebService.SendRequest(new ReceiveWantedReq
		{
			Slot = slot,
			IsBoost = isBoost
		}, delegate(ReceiveWantedRes res)
		{
			UpdateItemInfoList(res.ItemList);
			ComposeRewardEntities(res.RewardEntities);
			Action<ReceiveWantedRes> action = p_cb;
			if (action != null)
			{
				action(res);
			}
		});
	}

	public void WantedReqRefreshWantedSlot(sbyte slot, Action<RefreshWantedSlotRes> p_cb = null)
	{
		WebService.SendRequest(new RefreshWantedSlotReq
		{
			Slot = slot
		}, delegate(RefreshWantedSlotRes res)
		{
			UpdateItemInfoList(res.ItemList);
			Action<RefreshWantedSlotRes> action = p_cb;
			if (action != null)
			{
				action(res);
			}
		});
	}
}
