#define RELEASE
using UnityEngine;
using enums;

namespace StageLib
{
	[ExecuteInEditMode]
	public class StageStartPoint : StageSLBase
	{
		public int StartID;

		public bool bShowStartEffect = true;

		public bool bMoveCamera;

		public bool bShowReadyGo = true;

		public bool bLookDir;

		public int nOpeningTalkID;

		public bool bOnlyGenerateOne;

		private int LastStartID = -1;

		private bool bOpeningTalkEnd;

		private bool bIsTalking;

		private int[] SkillLv = new int[2];

		private int SkinID;

		private void Start()
		{
		}

		private void LateUpdate()
		{
			if (!StageUpdate.gbStageReady)
			{
				return;
			}
			STAGE_TABLE value;
			ManagedSingleton<OrangeDataManager>.Instance.STAGE_TABLE_DICT.TryGetValue(ManagedSingleton<StageHelper>.Instance.nLastStageID, out value);
			if ((StageUpdate.AllStageCtrlEvent || (!ManagedSingleton<PlayerNetManager>.Instance.dicStage.ContainsKey(ManagedSingleton<StageHelper>.Instance.nLastStageID) && (value == null || value.n_DIFFICULTY <= 1))) && nOpeningTalkID != 0 && !bOpeningTalkEnd)
			{
				if (!bIsTalking)
				{
					MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_Dialog", delegate(DialogUI ui)
					{
						ui.Setup(nOpeningTalkID, TalkEndCB);
					});
					bIsTalking = true;
				}
				return;
			}
			base.gameObject.SetActive(false);
			if (StageUpdate.gbRegisterPvpPlayer)
			{
				if (LastStartID != StartID)
				{
					MonoBehaviourSingleton<StageSyncManager>.Instance.bIgnoreReadyGo = !bShowReadyGo;
					Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.STAGE_REGISTER_PVP_SPAWNPOS, base.transform.position, StartID, bLookDir);
					LastStartID = StartID;
				}
				return;
			}
			if (MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.bIsReConnect)
			{
				LastStartID = StartID;
				return;
			}
			if (StageUpdate.gbGeneratePvePlayer)
			{
				if (LastStartID != StartID)
				{
					MonoBehaviourSingleton<StageSyncManager>.Instance.bIgnoreReadyGo = !bShowReadyGo;
					EventManager.StageGeneratePlayer stageGeneratePlayer = new EventManager.StageGeneratePlayer();
					if (bOnlyGenerateOne)
					{
						stageGeneratePlayer.nMode = 2;
					}
					else
					{
						stageGeneratePlayer.nMode = 0;
					}
					stageGeneratePlayer.nID = StartID;
					stageGeneratePlayer.vPos = base.transform.position;
					stageGeneratePlayer.bLookDir = bLookDir;
					Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.STAGE_GENERATE_PVE_PLAYER, stageGeneratePlayer);
					LastStartID = StartID;
				}
				return;
			}
			if (!StageUpdate.bIsHost)
			{
				Debug.Log("[StageStartPoint] Not Host.");
				return;
			}
			SkinID = 0;
			if (ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo != null)
			{
				StartID = ManagedSingleton<StageHelper>.Instance.GetStageCharacterStruct().StandbyChara;
				if (ManagedSingleton<PlayerNetManager>.Instance.dicCharacter.ContainsKey(StartID))
				{
					CharacterInfo characterInfo = ManagedSingleton<PlayerNetManager>.Instance.dicCharacter[StartID];
					if (characterInfo.netSkillDic.ContainsKey(CharacterSkillSlot.ActiveSkill1))
					{
						SkillLv[0] = characterInfo.netSkillDic[CharacterSkillSlot.ActiveSkill1].Level;
					}
					if (characterInfo.netSkillDic.ContainsKey(CharacterSkillSlot.ActiveSkill2))
					{
						SkillLv[1] = characterInfo.netSkillDic[CharacterSkillSlot.ActiveSkill2].Level;
					}
					SkinID = characterInfo.netInfo.Skin;
				}
			}
			if (LastStartID == StartID)
			{
				return;
			}
			LastStartID = StartID;
			while (base.transform.childCount > 0)
			{
				Object.Destroy(base.transform.GetChild(0).gameObject);
			}
			MonoBehaviourSingleton<StageSyncManager>.Instance.bIgnoreReadyGo = !bShowReadyGo;
			if (LastStartID != 0)
			{
				GameObject obj = new GameObject();
				PlayerBuilder playerBuilder = obj.AddComponent<PlayerBuilder>();
				playerBuilder.SetPBP.CharacterID = LastStartID;
				playerBuilder.SetPBP.CharacterSkinID = SkinID;
				playerBuilder.SetPBP.SkillLv = SkillLv;
				playerBuilder.bShowStartEffect = bShowStartEffect;
				if (bLookDir)
				{
					playerBuilder.SetPBP.tSetCharacterDir = CharacterDirection.LEFT;
				}
				else
				{
					playerBuilder.SetPBP.tSetCharacterDir = CharacterDirection.RIGHT;
				}
				if (ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo != null)
				{
					StageHelper.StageCharacterStruct stageCharacterStruct = ManagedSingleton<StageHelper>.Instance.GetStageCharacterStruct();
					playerBuilder.SetPBP.WeaponList[0] = stageCharacterStruct.MainWeaponID;
					playerBuilder.SetPBP.WeaponList[1] = stageCharacterStruct.SubWeaponID;
					playerBuilder.SetPBP.WeaponChipList[0] = stageCharacterStruct.MainWeaponChipID;
					playerBuilder.SetPBP.WeaponChipList[1] = stageCharacterStruct.SubWeaponChipID;
					ManagedSingleton<PlayerNetManager>.Instance.dicFinalStrike.TryGetValue(stageCharacterStruct.MainWeaponFSID, out playerBuilder.SetPBP.FSkillList[0]);
					ManagedSingleton<PlayerNetManager>.Instance.dicFinalStrike.TryGetValue(stageCharacterStruct.SubWeaponFSID, out playerBuilder.SetPBP.FSkillList[1]);
				}
				obj.transform.position = base.transform.position;
				obj.transform.localScale = base.transform.lossyScale;
				obj.transform.rotation = base.transform.rotation;
			}
		}

		private void OnDrawGizmos()
		{
			Gizmos.color = new Color(1f, 0f, 0f);
			Gizmos.DrawSphere(base.transform.position, 0.1f);
			Gizmos.DrawWireCube(base.transform.position, new Vector3(1.2f, 1.2f, 1.2f));
		}

		private void TalkEndCB()
		{
			bOpeningTalkEnd = true;
		}

		public override bool IsNeedClip()
		{
			return false;
		}

		public override int GetTypeID()
		{
			return 1;
		}

		public override string GetTypeString()
		{
			return StageObjType.START_OBJ.ToString();
		}

		public override string GetSaveString()
		{
			return string.Concat(string.Concat(string.Concat(string.Concat(string.Concat(string.Concat(GetTypeString() + StartID, ";", GetBoolSaveStr(bShowStartEffect)), ";", GetBoolSaveStr(bMoveCamera)), ";", GetBoolSaveStr(bShowReadyGo)), ";", GetBoolSaveStr(bLookDir)), ";", nOpeningTalkID), ";", GetBoolSaveStr(bOnlyGenerateOne));
		}

		public override void LoadByString(string sLoad)
		{
			if (sLoad.StartsWith(GetTypeString()))
			{
				string[] array = sLoad.Substring(GetTypeString().Length).Split(';');
				StartID = int.Parse(array[0]);
				if (array.Length > 1)
				{
					bShowStartEffect = GetBoolBySaveStr(array[1]);
				}
				if (array.Length > 2)
				{
					bMoveCamera = GetBoolBySaveStr(array[2]);
				}
				if (array.Length > 3)
				{
					bShowReadyGo = GetBoolBySaveStr(array[3]);
				}
				if (array.Length > 4)
				{
					bLookDir = GetBoolBySaveStr(array[4]);
				}
				else
				{
					bLookDir = false;
				}
				if (array.Length > 5)
				{
					nOpeningTalkID = int.Parse(array[5]);
				}
				else
				{
					nOpeningTalkID = 0;
				}
				if (array.Length > 6)
				{
					bOnlyGenerateOne = GetBoolBySaveStr(array[6]);
				}
				else
				{
					bOnlyGenerateOne = false;
				}
			}
			else
			{
				StartID = int.Parse(sLoad);
			}
		}

		public override void SyncNowStatus()
		{
		}

		public override void OnSyncStageObj(string sIDKey, int nKey1, string smsg)
		{
			LastStartID = StartID;
		}
	}
}
