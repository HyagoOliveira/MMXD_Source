#define RELEASE
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OrangeAudio;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using enums;

namespace StageLib
{
	public class StageResManager : MonoBehaviour
	{
		private static Dictionary<Type, List<object>> StagePoolDic = new Dictionary<Type, List<object>>();

		private static List<StageUpdate.LoadCallBackObj> loadCallBackObjs = new List<StageUpdate.LoadCallBackObj>();

		private static List<TriggerLockEventData> listTriggerLockEvent = new List<TriggerLockEventData>();

		public static int loadingCbMax = 0;

		private static Coroutine tLockRangeCoroutine = null;

		private static RaycastHit2D[] hitArray = new RaycastHit2D[30];

		private static Collider2D[] overlapC2Ds = new Collider2D[30];

		private static StageUpdate stageUpdate;

		public static T GetObjFromPool<T>() where T : new()
		{
			Type typeFromHandle = typeof(T);
			List<object> value;
			if (StagePoolDic.TryGetValue(typeFromHandle, out value))
			{
				if (value.Count > 0)
				{
					T result = (T)value[0];
					value.RemoveAt(0);
					return result;
				}
				return new T();
			}
			StagePoolDic.Add(typeFromHandle, new List<object>());
			return new T();
		}

		public static void BackObjToPool<T>(T tBack)
		{
			Type typeFromHandle = typeof(T);
			List<object> value;
			if (StagePoolDic.TryGetValue(typeFromHandle, out value))
			{
				value.Add(tBack);
				return;
			}
			StagePoolDic.Add(typeFromHandle, new List<object>());
			StagePoolDic[typeFromHandle].Add(tBack);
		}

		public static void RegisterLockEvent(TriggerLockEventData tTriggerLockEventData)
		{
			StageUpdate stageUpdate = GetStageUpdate();
			if (listTriggerLockEvent.Count == 0)
			{
				listTriggerLockEvent.Add(tTriggerLockEventData);
				if (stageUpdate != null)
				{
					if (tLockRangeCoroutine != null)
					{
						stageUpdate.StopCoroutine(tLockRangeCoroutine);
					}
					tLockRangeCoroutine = stageUpdate.StartCoroutine(CheckLockEventCoroutine());
				}
			}
			else
			{
				listTriggerLockEvent.Add(tTriggerLockEventData);
			}
		}

		public static void RemoveAllLockEvent()
		{
			StageUpdate stageUpdate = GetStageUpdate();
			if (stageUpdate != null)
			{
				if (tLockRangeCoroutine != null)
				{
					stageUpdate.StopCoroutine(tLockRangeCoroutine);
				}
				tLockRangeCoroutine = null;
			}
			listTriggerLockEvent.Clear();
		}

		private static IEnumerator CheckLockEventCoroutine()
		{
			yield return CoroutineDefine._waitForEndOfFrame;
			bool bFirstLock = false;
			while (!StageUpdate.bMainPlayerOK)
			{
				bFirstLock = true;
				yield return CoroutineDefine._waitForEndOfFrame;
			}
			StageUpdate tStageUpdate = GetStageUpdate();
			OrangeCharacter tOC = StageUpdate.GetMainPlayerOC();
			int i = 0;
			int j = 0;
			while (i < listTriggerLockEvent.Count)
			{
				float fWaitTime = 0f;
				int num;
				while (i < listTriggerLockEvent.Count)
				{
					if (listTriggerLockEvent[i].fWaitLockTime > 0f)
					{
						fWaitTime = listTriggerLockEvent[i].fWaitLockTime;
						break;
					}
					num = i + 1;
					i = num;
				}
				if (i == listTriggerLockEvent.Count)
				{
					i--;
				}
				tStageUpdate.sLastLockRangeSyncID = listTriggerLockEvent[i].sSyncID;
				bool bMoveRightNow = false;
				if (i < listTriggerLockEvent.Count && fWaitTime > 0f)
				{
					TriggerLockEventData tTLockEventData2 = listTriggerLockEvent[i];
					bMoveRightNow = true;
					while (fWaitTime > 0f)
					{
						while (i + 1 != listTriggerLockEvent.Count && i + i < listTriggerLockEvent.Count)
						{
							int k;
							for (k = i + 1; k < listTriggerLockEvent.Count && !(listTriggerLockEvent[k].fWaitLockTime > 0f); k++)
							{
							}
							if (k != listTriggerLockEvent.Count)
							{
								k = i + 1;
								while (listTriggerLockEvent.Count != i + 1)
								{
									if (listTriggerLockEvent[k].fWaitLockTime == 0f)
									{
										listTriggerLockEvent.RemoveAt(k);
										continue;
									}
									listTriggerLockEvent.RemoveAt(k);
									break;
								}
								continue;
							}
							for (k = i + 1; k < listTriggerLockEvent.Count; k++)
							{
								TriggerLockEventData triggerLockEventData = listTriggerLockEvent[k];
								EventManager.LockRangeParam lockRangeParam = new EventManager.LockRangeParam();
								lockRangeParam.fMinX = triggerLockEventData.fMin;
								lockRangeParam.fMaxX = triggerLockEventData.fMax;
								lockRangeParam.fMinY = triggerLockEventData.fBtn;
								lockRangeParam.fMaxY = triggerLockEventData.fTop;
								lockRangeParam.nNoBack = triggerLockEventData.nType;
								lockRangeParam.fSpeed = triggerLockEventData.fSpeed;
								lockRangeParam.fOY = triggerLockEventData.fOY;
								lockRangeParam.bSetFocus = triggerLockEventData.bSetFocus;
								lockRangeParam.bSlowWhenMove = triggerLockEventData.bSlowWhenMove;
								Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.LOCK_RANGE, lockRangeParam);
							}
							k = i + 1;
							while (listTriggerLockEvent.Count != i + 1)
							{
								listTriggerLockEvent.RemoveAt(k);
							}
						}
						if (!LockRangeEvent.CheckPlayerPosition(tOC, null, tTLockEventData2.fMax, tTLockEventData2.fMin, tTLockEventData2.fTop, tTLockEventData2.fBtn))
						{
							break;
						}
						fWaitTime -= Time.deltaTime;
						if (BattleInfoUI.Instance != null)
						{
							BattleInfoUI.Instance.ShowTeleportCD(fWaitTime, tTLockEventData2.vTriggerPos);
						}
						yield return CoroutineDefine._waitForEndOfFrame;
					}
					if (BattleInfoUI.Instance != null)
					{
						BattleInfoUI.Instance.ShowTeleportCD(-1f, null);
					}
				}
				if (bMoveRightNow)
				{
					EventManager.RemoveDeadAreaEvent objFromPool = GetObjFromPool<EventManager.RemoveDeadAreaEvent>();
					objFromPool.tOC = tOC;
					Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.REMOVE_DEAD_AREA_EVENT, objFromPool);
				}
				tStageUpdate.sLastLockRangeSyncID = listTriggerLockEvent[i].sSyncID;
				while (j < i + 1 && j < listTriggerLockEvent.Count)
				{
					TriggerLockEventData tTLockEventData2 = listTriggerLockEvent[j];
					if (j == i || !(tTLockEventData2.sSyncID != "") || !(tTLockEventData2.sSyncID == tStageUpdate.sLastLockRangeSyncID))
					{
						EventManager.LockRangeParam lockRangeParam2 = new EventManager.LockRangeParam();
						lockRangeParam2.fMinX = tTLockEventData2.fMin;
						lockRangeParam2.fMaxX = tTLockEventData2.fMax;
						lockRangeParam2.fMinY = tTLockEventData2.fBtn;
						lockRangeParam2.fMaxY = tTLockEventData2.fTop;
						lockRangeParam2.nNoBack = tTLockEventData2.nType;
						lockRangeParam2.fSpeed = tTLockEventData2.fSpeed;
						lockRangeParam2.fOY = tTLockEventData2.fOY;
						lockRangeParam2.bSetFocus = tTLockEventData2.bSetFocus;
						lockRangeParam2.bSlowWhenMove = tTLockEventData2.bSlowWhenMove;
						Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.LOCK_RANGE, lockRangeParam2);
						if (j == i && LockRangeEvent.CheckPlayerPosition(tOC, tTLockEventData2.vTriggerPos, tTLockEventData2.fMax, tTLockEventData2.fMin, tTLockEventData2.fTop, tTLockEventData2.fBtn) && bMoveRightNow)
						{
							EventManager.StageCameraFocus stageCameraFocus = new EventManager.StageCameraFocus();
							stageCameraFocus.bLock = true;
							stageCameraFocus.bRightNow = true;
							Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.STAGE_CAMERA_FOCUS, stageCameraFocus);
						}
					}
					num = j + 1;
					j = num;
				}
				num = i + 1;
				i = num;
			}
			if (bFirstLock)
			{
				EventManager.StageCameraFocus stageCameraFocus2 = new EventManager.StageCameraFocus();
				stageCameraFocus2.nMode = 0;
				stageCameraFocus2.bLock = true;
				stageCameraFocus2.bRightNow = true;
				Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.STAGE_CAMERA_FOCUS, stageCameraFocus2);
			}
			listTriggerLockEvent.Clear();
			tLockRangeCoroutine = null;
		}

		public static void InitStageBattleUI(Action loadendcb)
		{
			StageUpdate.LoadCallBackObj loadCallBackObj = new StageUpdate.LoadCallBackObj();
			loadCallBackObj.lcb = delegate(StageUpdate.LoadCallBackObj tObj, UnityEngine.Object asset)
			{
				loadCallBackObjs.Remove(tObj);
			};
			AddCb(loadCallBackObj);
			TurtorialUI.LoadTurtorialUIByAB(loadCallBackObj.LoadCBNoParam);
			if (BattleInfoUI.Instance != null && (bool)BattleInfoUI.Instance.gameObject)
			{
				BattleInfoUI.Instance.OnClickCloseBtn();
			}
			loadCallBackObj = new StageUpdate.LoadCallBackObj();
			loadCallBackObj.loadStageObjData = loadendcb;
			loadCallBackObj.lcb = delegate(StageUpdate.LoadCallBackObj tObj, UnityEngine.Object asset)
			{
				loadCallBackObjs.Remove(tObj);
				BattleInfoUI battleInfoUI = (BattleInfoUI)asset;
				if (ManagedSingleton<StageHelper>.Instance.nLastStageID != 0)
				{
					battleInfoUI.InitEnemyInfo(ManagedSingleton<StageHelper>.Instance.nLastStageID);
				}
			};
			AddCb(loadCallBackObj);
			MonoBehaviourSingleton<UIManager>.Instance.LoadUI<BattleInfoUI>("UI_BattleInfo", loadCallBackObj.LoadUIComplete);
			if (MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.tChargeDataScriptObj == null)
			{
				loadCallBackObj = new StageUpdate.LoadCallBackObj();
				loadCallBackObj.loadStageObjData = loadendcb;
				loadCallBackObj.lcb = delegate(StageUpdate.LoadCallBackObj tObj, UnityEngine.Object asset)
				{
					loadCallBackObjs.Remove(tObj);
					MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.tChargeDataScriptObj = (ChargeDataScriptObj)asset;
				};
				AddCb(loadCallBackObj);
				MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad<ChargeDataScriptObj>("prefab/scriptdatas", "ChargeData", loadCallBackObj.LoadCB);
			}
			if (MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.tStageUiDataScriptObj == null)
			{
				loadCallBackObj = new StageUpdate.LoadCallBackObj();
				loadCallBackObj.loadStageObjData = loadendcb;
				loadCallBackObj.lcb = delegate(StageUpdate.LoadCallBackObj tObj, UnityEngine.Object asset)
				{
					loadCallBackObjs.Remove(tObj);
					MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.tStageUiDataScriptObj = (StageUiDataScriptObj)asset;
				};
				AddCb(loadCallBackObj);
				MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad<StageUiDataScriptObj>("prefab/scriptdatas", "StageUiData", loadCallBackObj.LoadCB);
			}
			STAGE_TABLE value;
			if (MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.IsPvp && ManagedSingleton<OrangeDataManager>.Instance.STAGE_TABLE_DICT.TryGetValue(ManagedSingleton<StageHelper>.Instance.nLastStageID, out value) && value.n_MAIN == 90001 && value.n_SUB == 1)
			{
				loadCallBackObj = new StageUpdate.LoadCallBackObj();
				loadCallBackObj.lcb = delegate(StageUpdate.LoadCallBackObj tObj, UnityEngine.Object asset)
				{
					loadCallBackObjs.Remove(tObj);
				};
				AddCb(loadCallBackObj);
				MonoBehaviourSingleton<UIManager>.Instance.PreloadUI("UI_PvpBar", loadCallBackObj.LoadCBNoParam);
			}
			if (!MonoBehaviourSingleton<PoolManager>.Instance.IsPreload("PoolColliderBullet"))
			{
				CollideBullet collideBullet = new GameObject().AddComponent<CollideBullet>();
				collideBullet.bNeedBackPoolColliderBullet = true;
				MonoBehaviourSingleton<PoolManager>.Instance.CreatePoolBaseLocal<CollideBullet>(collideBullet, "PoolColliderBullet");
			}
			loadCallBackObj = new StageUpdate.LoadCallBackObj();
			loadCallBackObj.lcb = delegate(StageUpdate.LoadCallBackObj tObj, UnityEngine.Object asset)
			{
				loadCallBackObjs.Remove(tObj);
			};
			AddCb(loadCallBackObj);
			MonoBehaviourSingleton<UIManager>.Instance.PreloadUI("UI_BattleSetting", loadCallBackObj.LoadCBNoParam);
			loadCallBackObj = new StageUpdate.LoadCallBackObj();
			loadCallBackObj.lcb = delegate(StageUpdate.LoadCallBackObj tObj, UnityEngine.Object asset)
			{
				loadCallBackObjs.Remove(tObj);
			};
			AddCb(loadCallBackObj);
			MonoBehaviourSingleton<UIManager>.Instance.PreloadUI("UI_GamePause", loadCallBackObj.LoadCBNoParam);
			StageUpdate stageUpdate = GetStageUpdate();
			if (stageUpdate != null)
			{
				stageUpdate.StartCoroutine(LoadEndRunCB(loadendcb));
			}
		}

		private static IEnumerator LoadEndRunCB(Action loadendcb)
		{
			yield return CoroutineDefine._waitForEndOfFrame;
			while (loadCallBackObjs.Count > 0)
			{
				yield return CoroutineDefine._waitForEndOfFrame;
			}
			if (loadendcb != null)
			{
				loadendcb();
			}
		}

		private static void InitPad(object p_param)
		{
			MonoBehaviour monoBehaviour = p_param as MonoBehaviour;
			if (!(monoBehaviour == null))
			{
				Transform transform = monoBehaviour.transform.Find("VirtualGamepad");
				transform = transform.Find("Radius");
				transform = transform.Find("Stick");
				transform = monoBehaviour.transform.Find("VirtualButton_Attack");
				transform = monoBehaviour.transform.Find("VirtualButton_Jump");
				LoadIconCallBack loadIconCallBack = new LoadIconCallBack();
				loadIconCallBack.TargetImage = transform.GetComponent<Image>();
				loadIconCallBack.TargetImage.color = Color.white;
				MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad<UnityEngine.Object>("texture/prototype/battle", "Orange_UI_battle_Button_Jump", loadIconCallBack.LoadCB);
				transform = monoBehaviour.transform.Find("VirtualButton_Skill0");
				loadIconCallBack.TargetImage = transform.GetComponent<Image>();
				loadIconCallBack.TargetImage.color = Color.white;
				MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad<UnityEngine.Object>("texture/prototype/battle", "Orange_UI_battle_Button_Sprint", loadIconCallBack.LoadCB);
				transform = monoBehaviour.transform.Find("VirtualButton_Skill1");
				transform.gameObject.AddComponent<StageSkillBtn>().ReChange(3f);
				transform = monoBehaviour.transform.Find("VirtualButton_Switch");
			}
		}

		public static void LoadInfoBar()
		{
			StageUpdate.LoadCallBackObj loadCallBackObj = new StageUpdate.LoadCallBackObj();
			loadCallBackObj.lcb = delegate(StageUpdate.LoadCallBackObj tObj, UnityEngine.Object asset)
			{
				loadCallBackObjs.Remove(tObj);
			};
			AddCb(loadCallBackObj);
			MonoBehaviourSingleton<PoolManager>.Instance.CreatePoolBase<ObjInfoBar>("prefab/ObjInfoBar", "ObjInfoBar", 10, loadCallBackObj.LoadCBNoParam);
		}

		public static MOB_TABLE GetEnemy(int nGroupID)
		{
			MOB_TABLE[] mobArrayFromGroup = ManagedSingleton<OrangeTableHelper>.Instance.GetMobArrayFromGroup(nGroupID);
			for (int i = 0; i < mobArrayFromGroup.Length; i++)
			{
				if (mobArrayFromGroup[i].n_DIFFICULTY == StageUpdate.gDifficulty)
				{
					return mobArrayFromGroup[i];
				}
			}
			return null;
		}

		public static void LoadEnemy(int nGroupID)
		{
			MOB_TABLE enemy = GetEnemy(nGroupID);
			if (enemy != null)
			{
				LoadEnemy(enemy);
			}
		}

		public static void LoadStageItemModel(int nSkillID, Transform tTrans = null, Vector3? vPos = null, Vector3? vDir = null, List<Transform> listignoretrans = null)
		{
			SKILL_TABLE tSKILL_TABLE;
			if (!ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT.TryGetValue(nSkillID, out tSKILL_TABLE) || tSKILL_TABLE.s_MODEL == "DUMMY" || tSKILL_TABLE.s_MODEL == "null")
			{
				return;
			}
			StageUpdate.LoadCallBackObj loadCallBackObj = new StageUpdate.LoadCallBackObj();
			loadCallBackObj.loadStageObjData = tSKILL_TABLE;
			loadCallBackObj.lcb = delegate(StageUpdate.LoadCallBackObj tObj, UnityEngine.Object asset)
			{
				if (asset == null)
				{
					SKILL_TABLE sKILL_TABLE = tObj.loadStageObjData as SKILL_TABLE;
					Debug.LogError("SKILL_TABLE id = " + sKILL_TABLE.n_ID + " s_MODEL = " + sKILL_TABLE.s_MODEL + " no model");
				}
				else if (tTrans != null)
				{
					BulletBase component = (UnityEngine.Object.Instantiate(asset, tTrans) as GameObject).GetComponent<BulletBase>();
					if (component != null)
					{
						component.UpdateFx();
					}
				}
				else if (vPos.HasValue)
				{
					GameObject obj = UnityEngine.Object.Instantiate(asset) as GameObject;
					BulletBase component2 = obj.GetComponent<BulletBase>();
					obj.transform.position = vPos ?? Vector3.zero;
					ItemBullet itemBullet = component2 as ItemBullet;
					if (itemBullet != null)
					{
						if (listignoretrans == null)
						{
							listignoretrans = new List<Transform>();
						}
						itemBullet.listignorec2dtrans = listignoretrans;
						component2.UpdateBulletData(tSKILL_TABLE);
						component2.SetBulletAtk(new WeaponStatus(), new PerBuffManager.BuffStatus());
						component2.Active(vPos ?? Vector3.zero, vDir ?? Vector3.right, (int)BulletScriptableObject.Instance.BulletLayerMaskPlayer | (int)BulletScriptableObject.Instance.BulletLayerMaskPvpPlayer);
					}
					else if (component2 != null)
					{
						component2.UpdateFx();
					}
				}
				loadCallBackObjs.Remove(tObj);
			};
			AddCb(loadCallBackObj);
			MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad<UnityEngine.Object>("prefab/bullet/" + tSKILL_TABLE.s_MODEL, tSKILL_TABLE.s_MODEL, loadCallBackObj.LoadCB);
		}

		public static void LoadBullet(int nID)
		{
			SKILL_TABLE value;
			if (ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT.TryGetValue(nID, out value))
			{
				LoadBulletBySkillTable(value);
			}
		}

		public static void LoadFx(string fxpath, int nCount = 5)
		{
			if (fxpath != "null")
			{
				StageUpdate.LoadCallBackObj loadCallBackObj = new StageUpdate.LoadCallBackObj();
				loadCallBackObj.lcb = delegate(StageUpdate.LoadCallBackObj tObj, UnityEngine.Object asset)
				{
					loadCallBackObjs.Remove(tObj);
				};
				AddCb(loadCallBackObj);
				MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(fxpath, nCount, loadCallBackObj.LoadCBNoParam);
			}
		}

		public static void LoadSE(string fxpath)
		{
			if (fxpath != "null")
			{
				StageUpdate.LoadCallBackObj loadCallBackObj = new StageUpdate.LoadCallBackObj();
				loadCallBackObj.lcb = delegate(StageUpdate.LoadCallBackObj tObj, UnityEngine.Object asset)
				{
					loadCallBackObjs.Remove(tObj);
				};
				AddCb(loadCallBackObj);
				MonoBehaviourSingleton<AudioManager>.Instance.PreloadAtomSource(fxpath, loadCallBackObj.LoadCBNoParam);
			}
		}

		public static void LoadScenario(int nScenarioID)
		{
			if (nScenarioID != 0)
			{
				Queue<SCENARIO_TABLE> queue = new Queue<SCENARIO_TABLE>(ManagedSingleton<OrangeTableHelper>.Instance.GetScenarioGroupData(nScenarioID));
				while (queue.Count > 0)
				{
					SCENARIO_TABLE sCENARIO_TABLE = queue.Dequeue();
					string format = ((sCENARIO_TABLE.n_HEAD_TYPE != 0) ? AssetBundleScriptableObject.Instance.m_texture_2d_stand_st : AssetBundleScriptableObject.Instance.m_texture_scenario);
					string[] array = sCENARIO_TABLE.s_HEAD.Split(',');
					string empty = string.Empty;
					LoadObject(assetName: (array.Length <= 1) ? array[0] : array[1], bundleName: string.Format(format, array[0]));
				}
				LoadUI("UI_Dialog");
			}
		}

		public static void LoadUI(string uiname)
		{
			if (uiname != null && !(uiname == ""))
			{
				StageUpdate.LoadCallBackObj loadCallBackObj = new StageUpdate.LoadCallBackObj();
				loadCallBackObj.lcb = delegate(StageUpdate.LoadCallBackObj tObj, UnityEngine.Object asset)
				{
					loadCallBackObjs.Remove(tObj);
				};
				AddCb(loadCallBackObj);
				MonoBehaviourSingleton<UIManager>.Instance.PreloadUI(uiname, loadCallBackObj.LoadCBNoParam);
			}
		}

		public static void LoadObject(string bundleName, string assetName)
		{
			if (bundleName != "null")
			{
				StageUpdate.LoadCallBackObj loadCallBackObj = new StageUpdate.LoadCallBackObj();
				loadCallBackObj.lcb = delegate(StageUpdate.LoadCallBackObj tObj, UnityEngine.Object asset)
				{
					loadCallBackObjs.Remove(tObj);
				};
				AddCb(loadCallBackObj);
				MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad<UnityEngine.Object>(bundleName, assetName, loadCallBackObj.LoadCB);
			}
		}

		public static void LoadIcon(string iconpath)
		{
			if (iconpath != "null")
			{
				StageUpdate.LoadCallBackObj loadCallBackObj = new StageUpdate.LoadCallBackObj();
				loadCallBackObj.lcb = delegate(StageUpdate.LoadCallBackObj tObj, UnityEngine.Object asset)
				{
					loadCallBackObjs.Remove(tObj);
				};
				AddCb(loadCallBackObj);
				MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad<Sprite>(AssetBundleScriptableObject.Instance.GetIconSkill(iconpath), iconpath, loadCallBackObj.LoadCB);
			}
		}

		public static void LoadBuff(int nBuffID)
		{
			CONDITION_TABLE value;
			if (ManagedSingleton<OrangeDataManager>.Instance.CONDITION_TABLE_DICT.TryGetValue(nBuffID, out value))
			{
				LoadIcon(value.s_ICON);
				LoadFx(value.s_HIT_FX);
				LoadFx(value.s_DURING_FX);
				if (value.n_MAX_TRIGGER != 0)
				{
					LoadBulletBySkillTable(ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT[value.n_MAX_TRIGGER]);
				}
			}
		}

		public static bool LoadEnemy(MOB_TABLE tMob)
		{
			StageUpdate.LoadCallBackObj loadCallBackObj = null;
			bool result = true;
			switch (tMob.s_MODEL)
			{
			case "enemy_human":
			case "enemy_shield_human":
			case "enemy_sword_human":
			case "event_human":
			case "event2_human_1":
			case "event2_human_2":
			case "event2_human_3":
			case "event3_human_1":
			case "event4_human_1":
			case "event4_human_2":
			case "event5_human_1":
			case "event5_human_2":
			case "event6_human_1":
			case "event7_human_1":
			{
				int n_AVATAR = tMob.n_AVATAR;
				Debug.Log("偵測到人型怪，將進行特殊處理 - 人型怪模型:" + n_AVATAR);
				MonoBehaviourSingleton<EnemyHumanResourceManager>.Instance.LoadEnemyHuman(tMob.n_ID);
				break;
			}
			}
			if (!MonoBehaviourSingleton<PoolManager>.Instance.IsPreload(tMob.s_MODEL))
			{
				loadCallBackObj = new StageUpdate.LoadCallBackObj();
				loadCallBackObj.loadStageObjData = tMob;
				loadCallBackObj.lcb = delegate(StageUpdate.LoadCallBackObj tObj, UnityEngine.Object asset)
				{
					if (asset != null)
					{
						EnemyControllerBase component = ((GameObject)asset).GetComponent<EnemyControllerBase>();
						MOB_TABLE mOB_TABLE = (MOB_TABLE)tObj.loadStageObjData;
						MonoBehaviourSingleton<PoolManager>.Instance.CreatePoolBaseLocal<EnemyControllerBase>(UnityEngine.Object.Instantiate(component), mOB_TABLE.s_MODEL, ManagedSingleton<OrangeTableHelper>.Instance.IsBoss(mOB_TABLE) ? 1 : 3);
					}
					loadCallBackObjs.Remove(tObj);
				};
				AddCb(loadCallBackObj);
				MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad<UnityEngine.Object>(AssetBundleScriptableObject.Instance.m_prefabEnemy + tMob.s_MODEL, tMob.s_MODEL, loadCallBackObj.LoadCB);
				result = false;
			}
			List<SKILL_TABLE> list = new List<SKILL_TABLE>();
			list.AddRange(ManagedSingleton<OrangeTableHelper>.Instance.GetEnemyAllSkillData(tMob));
			int[] array = new int[3] { tMob.n_INITIAL_SKILL1, tMob.n_INITIAL_SKILL2, tMob.n_INITIAL_SKILL3 };
			for (int i = 0; i < array.Length; i++)
			{
				SKILL_TABLE value;
				if (ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT.TryGetValue(array[i], out value))
				{
					list.Add(value);
				}
			}
			for (int j = 0; j < list.Count; j++)
			{
				if (LoadBulletBySkillTable(list[j]))
				{
					result = false;
				}
				if (list[j].n_EFFECT == 22 || list[j].n_EFFECT == 11)
				{
					LoadEnemy((int)list[j].f_EFFECT_X);
				}
			}
			return result;
		}

		public static void LoadSkillBullet(SKILL_TABLE bulletTable, int nWID1, int nWID2)
		{
			int num;
			switch (bulletTable.n_USE_DEFAULT_WEAPON)
			{
			case -1:
				num = -1;
				break;
			case 0:
				num = nWID1;
				break;
			default:
				num = nWID2;
				break;
			}
			if (num != -1)
			{
				WEAPON_TABLE wEAPON_TABLE = ManagedSingleton<OrangeDataManager>.Instance.WEAPON_TABLE_DICT[num];
				switch ((WeaponType)(short)wEAPON_TABLE.n_TYPE)
				{
				case WeaponType.Buster:
				case WeaponType.Spray:
				case WeaponType.SprayHeavy:
				case WeaponType.DualGun:
				case WeaponType.MGun:
				case WeaponType.Gatling:
				case WeaponType.Launcher:
					LoadBulletBySkillTable(bulletTable);
					break;
				case WeaponType.Melee:
					if (bulletTable.s_MODEL != "null" && bulletTable.s_MODEL != "DUMMY")
					{
						LoadBulletBySkillTable(bulletTable);
					}
					break;
				default:
					Debug.Log("Unexpected type :" + wEAPON_TABLE.n_TYPE);
					break;
				}
			}
			else
			{
				switch (bulletTable.s_USE_MOTION)
				{
				case "ULTIMATE_NOVASTRIKE":
					LoadFx("fxduring_novastrike_000", 2);
					LoadFx("fxduring_novastrike_001", 10);
					LoadFx("fxduring_novastrike_002", 2);
					break;
				case "MARINO_DASH":
					LoadFx("fxuse_dash_000", 2);
					LoadFx("fxuse_dashdx_000", 2);
					break;
				case "AXL_ROLL":
					LoadFx("obj_fxuse_rolling_000", 2);
					LoadFx("fxuse_axlskill1", 2);
					LoadFx("fxuse_axlskill2");
					break;
				case "MARINO_DARTS":
					LoadFx("fxuse_darts_000a", 2);
					LoadBulletModel(bulletTable);
					break;
				default:
					if (bulletTable.s_MODEL == "null")
					{
						Debug.LogWarning("子彈為NULL，正常不該這樣");
						break;
					}
					if (bulletTable.n_COMBO_SKILL != 0 || bulletTable.n_CHARGE_MAX_LEVEL != 0)
					{
						LoadBulletBySkillTable(bulletTable);
						break;
					}
					LoadBulletModel(bulletTable);
					if (bulletTable.n_TYPE == 1 && bulletTable.n_SHOTLINE == 5)
					{
						CheckLinkSkill(bulletTable);
					}
					break;
				case "AXL_SPRAYSHOT":
				case "FIRST_X_SKILL2":
					break;
				}
			}
			LoadBuff(bulletTable.n_CONDITION_ID);
		}

		public static void LoadBulletBySkillTableVoid(SKILL_TABLE bulletTable)
		{
			LoadBulletBySkillTable(bulletTable);
		}

		public static bool LoadBulletBySkillTable(SKILL_TABLE bulletTable)
		{
			bool result = false;
			for (int i = 0; i <= bulletTable.n_CHARGE_MAX_LEVEL; i++)
			{
				SKILL_TABLE sKILL_TABLE = ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT[bulletTable.n_ID + i];
				if (LoadBulletModel(sKILL_TABLE))
				{
					result = true;
				}
				if (sKILL_TABLE.n_EFFECT == 16)
				{
					int key = (int)sKILL_TABLE.f_EFFECT_X;
					float f_EFFECT_Z = sKILL_TABLE.f_EFFECT_Z;
					PET_TABLE pET_TABLE = ManagedSingleton<OrangeDataManager>.Instance.PET_TABLE_DICT[key];
					StageUpdate.LoadCallBackObj loadCallBackObj = new StageUpdate.LoadCallBackObj();
					AddCb(loadCallBackObj);
					MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad<UnityEngine.Object>("model/pet/petbase", "PetBase", loadCallBackObj.LoadCB);
					loadCallBackObj = new StageUpdate.LoadCallBackObj();
					AddCb(loadCallBackObj);
					MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad<UnityEngine.Object>("model/pet/" + pET_TABLE.s_MODEL, pET_TABLE.s_MODEL + "_G.prefab", loadCallBackObj.LoadCB);
					loadCallBackObj = new StageUpdate.LoadCallBackObj();
					AddCb(loadCallBackObj);
					MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad<RuntimeAnimatorController>("model/animator/petemptycontroller", "PetEmptyController", loadCallBackObj.LoadCB);
					loadCallBackObj = new StageUpdate.LoadCallBackObj();
					AddCb(loadCallBackObj);
					MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad<UnityEngine.Object>("model/animation/pet/" + pET_TABLE.s_MODEL, pET_TABLE.s_MODEL, loadCallBackObj.LoadCB);
					LoadFx("fxuse_ballskill_000", 2);
				}
				if (CheckLinkSkill(sKILL_TABLE))
				{
					result = true;
				}
				LoadBuff(sKILL_TABLE.n_CONDITION_ID);
				while (sKILL_TABLE.n_COMBO_SKILL != 0)
				{
					sKILL_TABLE = ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT[sKILL_TABLE.n_COMBO_SKILL];
					if (LoadBulletModel(sKILL_TABLE))
					{
						result = true;
					}
					if (CheckLinkSkill(sKILL_TABLE))
					{
						result = true;
					}
				}
			}
			return result;
		}

		private static bool CheckLinkSkill(SKILL_TABLE tLinkSkill)
		{
			bool result = false;
			while (ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT.ContainsKey(tLinkSkill.n_LINK_SKILL))
			{
				tLinkSkill = ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT[tLinkSkill.n_LINK_SKILL];
				if (LoadBulletModel(tLinkSkill))
				{
					result = true;
				}
			}
			return result;
		}

		private static bool LoadBulletModel(SKILL_TABLE tBulletTable)
		{
			bool result = false;
			if (tBulletTable.s_MODEL != "" && tBulletTable.s_MODEL != "null" && tBulletTable.s_MODEL != "DUMMY" && !MonoBehaviourSingleton<PoolManager>.Instance.IsPreload(tBulletTable.s_MODEL))
			{
				result = true;
				StageUpdate.LoadCallBackObj loadCallBackObj = new StageUpdate.LoadCallBackObj();
				loadCallBackObj.loadStageObjData = tBulletTable;
				loadCallBackObj.lcb = delegate(StageUpdate.LoadCallBackObj tLCB, UnityEngine.Object obj)
				{
					SKILL_TABLE sKILL_TABLE2 = tLCB.loadStageObjData as SKILL_TABLE;
					switch ((BulletType)(short)sKILL_TABLE2.n_TYPE)
					{
					case BulletType.Continuous:
					{
						ContinuousBullet component3 = ((GameObject)obj).GetComponent<ContinuousBullet>();
						MonoBehaviourSingleton<PoolManager>.Instance.CreatePoolBaseLocal<ContinuousBullet>(UnityEngine.Object.Instantiate(component3), sKILL_TABLE2.s_MODEL, 5);
						break;
					}
					case BulletType.Spray:
					{
						SprayBullet component4 = ((GameObject)obj).GetComponent<SprayBullet>();
						MonoBehaviourSingleton<PoolManager>.Instance.CreatePoolBaseLocal<SprayBullet>(UnityEngine.Object.Instantiate(component4), sKILL_TABLE2.s_MODEL, 5);
						break;
					}
					case BulletType.Collide:
					{
						CollideBullet component6 = ((GameObject)obj).GetComponent<CollideBullet>();
						if (component6 == null)
						{
							BulletBase component7 = ((GameObject)obj).GetComponent<BulletBase>();
							MonoBehaviourSingleton<PoolManager>.Instance.CreatePoolBaseLocal<BulletBase>(UnityEngine.Object.Instantiate(component7), sKILL_TABLE2.s_MODEL, 5);
						}
						else
						{
							MonoBehaviourSingleton<PoolManager>.Instance.CreatePoolBaseLocal<CollideBullet>(UnityEngine.Object.Instantiate(component6), sKILL_TABLE2.s_MODEL, 5);
						}
						break;
					}
					case BulletType.LrColliderBulle:
					{
						LrColliderBullet component5 = ((GameObject)obj).GetComponent<LrColliderBullet>();
						MonoBehaviourSingleton<PoolManager>.Instance.CreatePoolBaseLocal<LrColliderBullet>(UnityEngine.Object.Instantiate(component5), sKILL_TABLE2.s_MODEL, 5);
						break;
					}
					default:
					{
						if (obj == null)
						{
							Debug.LogError(sKILL_TABLE2.s_MODEL + "出了點問題");
						}
						BulletBase component2 = ((GameObject)obj).GetComponent<BulletBase>();
						MonoBehaviourSingleton<PoolManager>.Instance.CreatePoolBaseLocal<BulletBase>(UnityEngine.Object.Instantiate(component2), sKILL_TABLE2.s_MODEL, 5);
						break;
					}
					}
					loadCallBackObjs.Remove(tLCB);
				};
				AddCb(loadCallBackObj);
				MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad<UnityEngine.Object>("prefab/bullet/" + tBulletTable.s_MODEL, tBulletTable.s_MODEL, loadCallBackObj.LoadCB);
			}
			int n_EFFECT = tBulletTable.n_EFFECT;
			if (n_EFFECT == 11)
			{
				MOB_TABLE mOB_TABLE = ManagedSingleton<OrangeDataManager>.Instance.MOB_TABLE_DICT[(int)tBulletTable.f_EFFECT_X];
				if (!MonoBehaviourSingleton<PoolManager>.Instance.IsPreload(mOB_TABLE.s_MODEL))
				{
					string bundleName = AssetBundleScriptableObject.Instance.m_prefabEnemy + mOB_TABLE.s_MODEL;
					result = true;
					StageUpdate.LoadCallBackObj loadCallBackObj2 = new StageUpdate.LoadCallBackObj();
					loadCallBackObj2.loadStageObjData = tBulletTable;
					loadCallBackObj2.lcb = delegate(StageUpdate.LoadCallBackObj tObj, UnityEngine.Object asset)
					{
						EnemyControllerBase component = ((GameObject)asset).GetComponent<EnemyControllerBase>();
						SKILL_TABLE sKILL_TABLE = (SKILL_TABLE)tObj.loadStageObjData;
						MOB_TABLE mOB_TABLE2 = ManagedSingleton<OrangeDataManager>.Instance.MOB_TABLE_DICT[(int)sKILL_TABLE.f_EFFECT_X];
						MonoBehaviourSingleton<PoolManager>.Instance.CreatePoolBaseLocal<EnemyControllerBase>(UnityEngine.Object.Instantiate(component), mOB_TABLE2.s_MODEL, (int)sKILL_TABLE.f_EFFECT_Y);
						loadCallBackObjs.Remove(tObj);
						MonoBehaviourSingleton<AssetsBundleManager>.Instance.UnloadSingleBundleCache(bundleName);
					};
					AddCb(loadCallBackObj2);
					MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad<UnityEngine.Object>(bundleName, mOB_TABLE.s_MODEL, loadCallBackObj2.LoadCB);
				}
			}
			LoadFx(tBulletTable.s_USE_FX);
			LoadFx(tBulletTable.s_HIT_FX);
			LoadFx(tBulletTable.s_VANISH_FX);
			return result;
		}

		public static ObjInfoBar CreateInfoBar(Transform tFollow, BoxCollider2D tB2d, bool bCaluHeigh = false, float fadd = 0.5f)
		{
			ObjInfoBar poolObj = MonoBehaviourSingleton<PoolManager>.Instance.GetPoolObj<ObjInfoBar>("ObjInfoBar");
			float y = 0f;
			Transform transform = OrangeBattleUtility.FindChildRecursive(tFollow, "InfoBar", true);
			if (transform == null)
			{
				if (tB2d != null)
				{
					if (!tB2d.enabled)
					{
						tB2d.enabled = true;
						y = tB2d.bounds.size.y + fadd;
						tB2d.enabled = false;
					}
					else
					{
						y = tB2d.bounds.size.y + fadd;
					}
				}
				if (tB2d == null || bCaluHeigh)
				{
					y = GetTransformHeigh(tFollow, tFollow.transform.position.y) + fadd;
				}
				transform = tFollow;
			}
			poolObj.transform.localRotation = Quaternion.identity;
			poolObj.transform.SetParent(transform, false);
			poolObj.InitBuff(new Vector3(0f, y, 0f));
			return poolObj;
		}

		public static void CreateHpBarToPlayer(OrangeCharacter tPlayer)
		{
			if (!tPlayer.UsingVehicle)
			{
				ObjInfoBar objInfoBar = CreateInfoBar(tPlayer.transform, tPlayer.Controller.Collider2D);
				ObjInfoBar.BAR_COLOR barColor = ObjInfoBar.BAR_COLOR.GREEN_BAR;
				tPlayer.HurtActions += objInfoBar.HurtCB;
				tPlayer.selfBuffManager.UpdateBuffBar += objInfoBar.UpdateBuffCB;
				int playerTeam = MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.GetPlayerTeam(tPlayer);
				ObjInfoBar.BAR_COLOR[] array = new ObjInfoBar.BAR_COLOR[3]
				{
					ObjInfoBar.BAR_COLOR.GREEN_BAR,
					ObjInfoBar.BAR_COLOR.BLUE_BAR,
					ObjInfoBar.BAR_COLOR.ORANGE_BAR
				};
				if (tPlayer.gameObject.layer == ManagedSingleton<OrangeLayerManager>.Instance.PvpPlayerLayer)
				{
					barColor = array[playerTeam];
				}
				else if (!tPlayer.IsLocalPlayer)
				{
					barColor = ((!StageUpdate.gbGeneratePvePlayer) ? array[playerTeam] : array[1]);
				}
				tPlayer.objInfoBar = objInfoBar;
				objInfoBar.SetPlayBar(tPlayer.MaxHp, tPlayer.Hp, tPlayer.sPlayerName, tPlayer.SetPBP.tPlayerStatus.nLV, barColor);
				objInfoBar.UpdateBuffCB(tPlayer.selfBuffManager);
			}
		}

		public static void CreateHpBarToRideArmor(RideBaseObj tPlayer)
		{
			ObjInfoBar objInfoBar = CreateInfoBar(tPlayer.transform, tPlayer.Controller.Collider2D, false, 1.25f);
			ObjInfoBar.BAR_COLOR barColor = ObjInfoBar.BAR_COLOR.ORANGE_BAR;
			tPlayer.HurtActions += objInfoBar.HurtCB;
			tPlayer.selfBuffManager.UpdateBuffBar += objInfoBar.UpdateBuffCB;
			int playerTeam = MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.GetPlayerTeam(tPlayer.MasterPilot);
			ObjInfoBar.BAR_COLOR[] array = new ObjInfoBar.BAR_COLOR[3]
			{
				ObjInfoBar.BAR_COLOR.GREEN_BAR,
				ObjInfoBar.BAR_COLOR.BLUE_BAR,
				ObjInfoBar.BAR_COLOR.ORANGE_BAR
			};
			if (tPlayer.gameObject.layer == ManagedSingleton<OrangeLayerManager>.Instance.PvpPlayerLayer)
			{
				barColor = array[playerTeam];
			}
			else if (!tPlayer.MasterPilot.IsLocalPlayer)
			{
				barColor = ((!StageUpdate.gbGeneratePvePlayer) ? array[playerTeam] : array[1]);
			}
			tPlayer.tObjInfoBar = objInfoBar;
			objInfoBar.SetPlayBar(tPlayer.MaxHp, tPlayer.Hp, tPlayer.MasterPilot.sPlayerName, tPlayer.MasterPilot.SetPBP.tPlayerStatus.nLV, barColor);
		}

		public static void HurtAddObjInfoBar(StageObjBase tSOB)
		{
			OrangeCharacter orangeCharacter = tSOB as OrangeCharacter;
			orangeCharacter.HurtActions -= HurtAddObjInfoBar;
			ObjInfoBar objInfoBar = CreateInfoBar(orangeCharacter.transform, orangeCharacter.Controller.Collider2D);
			ObjInfoBar.BAR_COLOR barColor = ObjInfoBar.BAR_COLOR.GREEN_BAR;
			tSOB.HurtActions += objInfoBar.HurtCB;
			tSOB.selfBuffManager.UpdateBuffBar += objInfoBar.UpdateBuffCB;
			int playerTeam = MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.GetPlayerTeam(orangeCharacter);
			ObjInfoBar.BAR_COLOR[] array = new ObjInfoBar.BAR_COLOR[3]
			{
				ObjInfoBar.BAR_COLOR.GREEN_BAR,
				ObjInfoBar.BAR_COLOR.BLUE_BAR,
				ObjInfoBar.BAR_COLOR.RED_BAR
			};
			if (orangeCharacter.gameObject.layer == ManagedSingleton<OrangeLayerManager>.Instance.PvpPlayerLayer)
			{
				barColor = array[playerTeam];
			}
			else if (!orangeCharacter.IsLocalPlayer)
			{
				barColor = ((!StageUpdate.gbGeneratePvePlayer) ? array[playerTeam] : array[1]);
			}
			orangeCharacter.objInfoBar = objInfoBar;
			objInfoBar.SetPlayBar(orangeCharacter.MaxHp, orangeCharacter.Hp, orangeCharacter.sPlayerName, orangeCharacter.SetPBP.tPlayerStatus.nLV, barColor);
		}

		public static float GetTransformHeigh(Transform transform, float fbottom)
		{
			float num = 0f;
			MeshRenderer component = transform.GetComponent<MeshRenderer>();
			if (component != null)
			{
				num = component.bounds.max.y - fbottom;
			}
			SkinnedMeshRenderer component2 = transform.GetComponent<SkinnedMeshRenderer>();
			if (component2 != null)
			{
				num = component2.bounds.max.y - fbottom;
			}
			if (transform.childCount > 0)
			{
				for (int i = 0; i < transform.childCount; i++)
				{
					float transformHeigh = GetTransformHeigh(transform.GetChild(i), fbottom);
					if (transformHeigh > num)
					{
						num = transformHeigh;
					}
				}
			}
			return num;
		}

		public static void RemoveInfoBar(ObjInfoBar tObjInfoBar)
		{
			MonoBehaviourSingleton<PoolManager>.Instance.BackToPool(tObjInfoBar, "ObjInfoBar");
		}

		public static EnemyControllerBase CreateEnemy(int nMobGroupID)
		{
			return CreateEnemyByMob(GetEnemy(nMobGroupID));
		}

		public static EnemyControllerBase CreateEnemyByMob(MOB_TABLE tMOB_TABLE)
		{
			if (tMOB_TABLE == null)
			{
				return null;
			}
			if (!LoadEnemy(tMOB_TABLE))
			{
				return null;
			}
			int n_ID = tMOB_TABLE.n_ID;
			MOB_TABLE enemy = GetEnemy(n_ID);
			if (enemy != null)
			{
				n_ID = enemy.n_ID;
				tMOB_TABLE = enemy;
			}
			EnemyControllerBase poolObj = MonoBehaviourSingleton<PoolManager>.Instance.GetPoolObj<EnemyControllerBase>(tMOB_TABLE.s_MODEL);
			poolObj.UpdateEnemyID(n_ID);
			poolObj.bNeedDead = false;
			BoxCollider2D component = poolObj.gameObject.GetComponent<BoxCollider2D>();
			poolObj.HealHp = 0;
			poolObj.DmgHp = 0;
			if (tMOB_TABLE.n_TYPE == 2 || tMOB_TABLE.n_TYPE == 5)
			{
				BattleInfoUI.Instance.InitBar(poolObj, tMOB_TABLE.n_HP_STEP, tMOB_TABLE.n_HP, tMOB_TABLE.n_HP, tMOB_TABLE.s_ICON);
				poolObj.selfBuffManager.ClearBuff();
				if (tMOB_TABLE.n_TYPE == 5)
				{
					BattleInfoUI.Instance.SetHiddenBossBar(true);
				}
			}
			else if (tMOB_TABLE.n_TYPE == 3)
			{
				poolObj.selfBuffManager.ClearBuff();
			}
			else if (tMOB_TABLE.n_TYPE == 4)
			{
				BattleInfoUI.Instance.InitBar(poolObj, tMOB_TABLE.n_HP_STEP, tMOB_TABLE.n_HP, tMOB_TABLE.n_HP, tMOB_TABLE.s_ICON, false);
				poolObj.selfBuffManager.ClearBuff();
			}
			else
			{
				ObjInfoBar objInfoBar = CreateInfoBar(poolObj.transform, component, true);
				objInfoBar.SetEnemyBar(tMOB_TABLE.n_HP, tMOB_TABLE.n_HP, tMOB_TABLE.s_NAME);
				poolObj.HurtActions += objInfoBar.HurtCB;
				poolObj.selfBuffManager.UpdateBuffBar += objInfoBar.UpdateBuffCB;
				poolObj.selfBuffManager.ClearBuff();
			}
			return poolObj;
		}

		public static void RemoveEnemy(EnemyControllerBase tEnemyControllerBase)
		{
		}

		public static float CheckLoadEnd()
		{
			if (loadCallBackObjs.Count > 0)
			{
				return 1f - (float)loadCallBackObjs.Count / (float)loadingCbMax;
			}
			return 1f;
		}

		public static string GetPlayerNameByID(int nID)
		{
			if (ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo != null)
			{
				return ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.Nickname;
			}
			return "NoName";
		}

		public static void RemoveLCB(StageUpdate.LoadCallBackObj tLCB)
		{
			if (loadCallBackObjs.Contains(tLCB))
			{
				loadCallBackObjs.Remove(tLCB);
			}
		}

		private static void AddCb(StageUpdate.LoadCallBackObj p_cb)
		{
			loadCallBackObjs.Add(p_cb);
			loadingCbMax++;
		}

		public static void LoadWeaponAB(int nWeaponID)
		{
			WEAPON_TABLE value;
			if (!ManagedSingleton<OrangeDataManager>.Instance.WEAPON_TABLE_DICT.TryGetValue(nWeaponID, out value))
			{
				return;
			}
			string text = "";
			SKILL_TABLE value2;
			if (ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT.TryGetValue(value.n_SKILL, out value2))
			{
				switch ((WeaponType)(short)value.n_TYPE)
				{
				case WeaponType.Buster:
				case WeaponType.Spray:
				case WeaponType.SprayHeavy:
				case WeaponType.DualGun:
				case WeaponType.MGun:
				case WeaponType.Gatling:
				case WeaponType.Launcher:
				{
					for (int i = 0; i <= value2.n_CHARGE_MAX_LEVEL; i++)
					{
						SKILL_TABLE sKILL_TABLE = ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT[value.n_SKILL + i];
						StageUpdate.LoadCallBackObj loadCallBackObj = new StageUpdate.LoadCallBackObj();
						AddCb(loadCallBackObj);
						MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad<UnityEngine.Object>("prefab/bullet/" + sKILL_TABLE.s_MODEL, sKILL_TABLE.s_MODEL, loadCallBackObj.LoadCB);
						LoadBulletBySkillTable(sKILL_TABLE);
					}
					break;
				}
				case WeaponType.Melee:
				{
					text = value.s_MODEL;
					StageUpdate.LoadCallBackObj loadCallBackObj = new StageUpdate.LoadCallBackObj();
					AddCb(loadCallBackObj);
					MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad<GameObject>("prefab/fx/slash/" + text + "_efx_sla", text + "_efx_sla", loadCallBackObj.LoadCB);
					loadCallBackObj = new StageUpdate.LoadCallBackObj();
					AddCb(loadCallBackObj);
					MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad<GameObject>("prefab/fx/slash/" + text + "_efx_jpsla", text + "_efx_jpsla", loadCallBackObj.LoadCB);
					loadCallBackObj = new StageUpdate.LoadCallBackObj();
					AddCb(loadCallBackObj);
					MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad<GameObject>("prefab/fx/slash/" + text + "_efx_wsla1", text + "_efx_wsla1", loadCallBackObj.LoadCB);
					loadCallBackObj = new StageUpdate.LoadCallBackObj();
					AddCb(loadCallBackObj);
					MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad<GameObject>("prefab/fx/slash/" + text + "_efx_wsla2", text + "_efx_wsla2", loadCallBackObj.LoadCB);
					LoadFx("fxhit_slash_000");
					break;
				}
				}
			}
			if (value.s_ICON != "" && value.s_ICON != "null")
			{
				StageUpdate.LoadCallBackObj loadCallBackObj = new StageUpdate.LoadCallBackObj();
				AddCb(loadCallBackObj);
				MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad<Sprite>(AssetBundleScriptableObject.Instance.m_iconWeapon, value.s_ICON, loadCallBackObj.LoadCB);
			}
			do
			{
				StageUpdate.LoadCallBackObj loadCallBackObj = new StageUpdate.LoadCallBackObj();
				AddCb(loadCallBackObj);
				MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad<UnityEngine.Object>(AssetBundleScriptableObject.Instance.m_newmodel_weapon + value.s_MODEL, value.s_MODEL + "_G.prefab", loadCallBackObj.LoadCB);
			}
			while (value.n_SUB_LINK != -1 && ManagedSingleton<OrangeDataManager>.Instance.WEAPON_TABLE_DICT.TryGetValue(value.n_SUB_LINK, out value));
		}

		public static void LoadMotion(int nCharacterID, int nWeaponID1, int nWeaponID2)
		{
			CHARACTER_TABLE value;
			if (ManagedSingleton<OrangeDataManager>.Instance.CHARACTER_TABLE_DICT.TryGetValue(nCharacterID, out value))
			{
				StageUpdate.LoadCallBackObj loadCallBackObj = new StageUpdate.LoadCallBackObj();
				AddCb(loadCallBackObj);
				HumanBase.LoadMotion(loadCallBackObj, value.s_ANIMATOR, WeaponType.Dummy);
				WEAPON_TABLE value2;
				if (ManagedSingleton<OrangeDataManager>.Instance.WEAPON_TABLE_DICT.TryGetValue(nWeaponID1, out value2))
				{
					StageUpdate.LoadCallBackObj loadCallBackObj2 = new StageUpdate.LoadCallBackObj();
					AddCb(loadCallBackObj2);
					HumanBase.LoadMotion(loadCallBackObj2, value.s_ANIMATOR, (WeaponType)value2.n_TYPE);
				}
				if (ManagedSingleton<OrangeDataManager>.Instance.WEAPON_TABLE_DICT.TryGetValue(nWeaponID2, out value2))
				{
					StageUpdate.LoadCallBackObj loadCallBackObj3 = new StageUpdate.LoadCallBackObj();
					AddCb(loadCallBackObj3);
					HumanBase.LoadMotion(loadCallBackObj3, value.s_ANIMATOR, (WeaponType)value2.n_TYPE);
				}
			}
		}

		private static int GetCharactPassiveSkillID(CHARACTER_TABLE tCHARACTER_TABLE, sbyte slot, int extra)
		{
			switch (slot)
			{
			case 1:
				switch ((CharacterSkillEnhanceSlot)(short)extra)
				{
				case CharacterSkillEnhanceSlot.EX_SKILL1:
					return tCHARACTER_TABLE.n_SKILL1_EX1;
				case CharacterSkillEnhanceSlot.EX_SKILL2:
					return tCHARACTER_TABLE.n_SKILL1_EX2;
				case CharacterSkillEnhanceSlot.EX_SKILL3:
					return tCHARACTER_TABLE.n_SKILL1_EX3;
				}
				break;
			case 2:
				switch ((CharacterSkillEnhanceSlot)(short)extra)
				{
				case CharacterSkillEnhanceSlot.EX_SKILL1:
					return tCHARACTER_TABLE.n_SKILL2_EX1;
				case CharacterSkillEnhanceSlot.EX_SKILL2:
					return tCHARACTER_TABLE.n_SKILL2_EX2;
				case CharacterSkillEnhanceSlot.EX_SKILL3:
					return tCHARACTER_TABLE.n_SKILL2_EX3;
				}
				break;
			case 3:
				return tCHARACTER_TABLE.n_PASSIVE_1;
			case 4:
				return tCHARACTER_TABLE.n_PASSIVE_2;
			case 5:
				return tCHARACTER_TABLE.n_PASSIVE_3;
			case 6:
				return tCHARACTER_TABLE.n_PASSIVE_4;
			case 7:
				return tCHARACTER_TABLE.n_PASSIVE_5;
			case 8:
				return tCHARACTER_TABLE.n_PASSIVE_6;
			}
			return 0;
		}

		public static void LoadPlayerAB(int nCharacterID, int nSkin, List<NetCharacterSkillInfo> listNetCharacterSkillInfo = null)
		{
			CHARACTER_TABLE value;
			if (!ManagedSingleton<OrangeDataManager>.Instance.CHARACTER_TABLE_DICT.TryGetValue(nCharacterID, out value))
			{
				return;
			}
			LoadWeaponAB(value.n_INIT_WEAPON1);
			LoadWeaponAB(value.n_INIT_WEAPON2);
			SKILL_TABLE tSKILL_TABLE = ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT[value.n_SKILL1];
			SKILL_TABLE tSKILL_TABLE2 = ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT[value.n_SKILL2];
			if (listNetCharacterSkillInfo != null)
			{
				foreach (NetCharacterSkillInfo item in listNetCharacterSkillInfo)
				{
					if (nCharacterID == item.CharacterID)
					{
						int charactPassiveSkillID = GetCharactPassiveSkillID(value, item.Slot, item.Extra);
						if (ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT.ContainsKey(charactPassiveSkillID))
						{
							SKILL_TABLE refSKILL_TABLE = ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT[charactPassiveSkillID];
							RefPassiveskill.RecalcuSkillBySkill(ref tSKILL_TABLE, refSKILL_TABLE);
							RefPassiveskill.RecalcuSkillBySkill(ref tSKILL_TABLE2, refSKILL_TABLE);
						}
					}
				}
			}
			LoadSkillBullet(tSKILL_TABLE, value.n_INIT_WEAPON1, value.n_INIT_WEAPON2);
			LoadSkillBullet(tSKILL_TABLE2, value.n_INIT_WEAPON1, value.n_INIT_WEAPON2);
			LoadObject("prefab/player", "player");
			string s_MODEL = value.s_MODEL;
			if (nSkin > 0)
			{
				s_MODEL = ManagedSingleton<OrangeDataManager>.Instance.SKIN_TABLE_DICT[nSkin].s_MODEL;
			}
			LoadObject("model/character/" + s_MODEL, s_MODEL + "_G.prefab");
			LoadFx("OBJ_WALLKICK_SPARK");
			LoadFx("OBJ_LAND");
			LoadFx("OBJ_JUMP_UP");
			LoadFx("OBJ_JUMP_LEFT");
			LoadFx("OBJ_JUMP_RIGHT");
			LoadFx("OBJ_DASH_SMOKE");
			LoadFx("obj_player_die");
			LoadFx("DistortionFx", 3);
			LoadFx("fxuse_bloody_000", 2);
			LoadObject("prefab/fx/chargefx", "fxduring_chargeshot_000_start");
			LoadObject("prefab/fx/chargefx", "fxduring_chargeshot_001_loop");
			LoadObject("prefab/fx/fx_dash_speedline", "fx_dash_speedline");
			LoadObject("prefab/fx/rockmandust", "RockmanDust");
			LoadObject("prefab/fx/OBJ_DASH_SPARK", "OBJ_DASH_SPARK");
			LoadObject("model/animator/newemptycontroller", "newemptycontroller");
			LoadFx("FX_TELEPORT_IN", 2);
			LoadFx("FX_TELEPORT_IN2", 2);
			LoadFx("FX_TELEPORT_OUT", 2);
			StageUpdate.LoadCallBackObj loadCallBackObj = new StageUpdate.LoadCallBackObj();
			AddCb(loadCallBackObj);
			AudioLib.LoadVoice(ref value, loadCallBackObj.LoadCBNoParam);
			loadCallBackObj = new StageUpdate.LoadCallBackObj();
			AddCb(loadCallBackObj);
			AudioLib.LoadCharaSE(ref value, loadCallBackObj.LoadCBNoParam);
			if (value.n_ID != 5 && value.n_ID != 10 && value.n_ID != 11)
			{
				loadCallBackObj = new StageUpdate.LoadCallBackObj();
				AddCb(loadCallBackObj);
				AudioLib.LoadSkillSE(ref value, loadCallBackObj.LoadCBNoParam);
			}
			string bundleName;
			string[] motionName;
			string[] uniqueBattlePose = HumanBase.GetUniqueBattlePose(value.s_MODEL, out bundleName, out motionName);
			foreach (string assetName in uniqueBattlePose)
			{
				LoadObject(bundleName, assetName);
			}
			LoadObject("prefab/shadowprojector", "shadowprojector");
			LoadObject("prefab/aimicon2", "aimicon2");
			LoadObject("prefab/virtualpadsystem4", "VirtualPadSystem4");
			LoadObject(AssetBundleScriptableObject.Instance.GetIconCharacter("icon_" + value.s_ICON), "icon_" + value.s_ICON);
		}

		public static void LoadBuffSound(string sName)
		{
			if (!(sName == "null") && sName != null)
			{
				StageUpdate.LoadCallBackObj loadCallBackObj = new StageUpdate.LoadCallBackObj();
				loadCallBackObj.lcb = delegate(StageUpdate.LoadCallBackObj tObj, UnityEngine.Object asset)
				{
					loadCallBackObjs.Remove(tObj);
				};
				AddCb(loadCallBackObj);
				MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(sName, 10, loadCallBackObj.LoadCBNoParam);
			}
		}

		public static RefPassiveskill LoadAllPassSkill(StageHelper.StageCharacterStruct tStageCharacterStruct, MemberInfo tMemberInfo = null)
		{
			RefPassiveskill refPassiveskill = new RefPassiveskill();
			WeaponInfo value = null;
			Debug.Log("LoadAllPassSkill 0");
			if (tMemberInfo != null)
			{
				if (tMemberInfo.netSealBattleSettingInfo.CharacterList.Count > tMemberInfo.nNowCharacterID)
				{
					tStageCharacterStruct.StandbyChara = tMemberInfo.netSealBattleSettingInfo.CharacterList[tMemberInfo.nNowCharacterID].CharacterID;
				}
				else
				{
					tStageCharacterStruct.StandbyChara = tMemberInfo.netSealBattleSettingInfo.CharacterList[tMemberInfo.netSealBattleSettingInfo.CharacterList.Count - 1].CharacterID;
				}
				foreach (NetWeaponSkillInfo weaponSkill in tMemberInfo.netSealBattleSettingInfo.WeaponSkillList)
				{
					if (StageUpdate.gbRegisterPvpPlayer)
					{
						weaponSkill.Level = 1;
					}
					if (weaponSkill.WeaponID == tMemberInfo.netSealBattleSettingInfo.MainWeaponInfo.WeaponID)
					{
						refPassiveskill.AddPassivesSkill(weaponSkill, 4, weaponSkill.Level);
					}
					else if (weaponSkill.WeaponID == tMemberInfo.netSealBattleSettingInfo.SubWeaponInfo.WeaponID)
					{
						refPassiveskill.AddPassivesSkill(weaponSkill, 8, weaponSkill.Level);
					}
				}
				foreach (NetChipInfo totalChip in tMemberInfo.netSealBattleSettingInfo.TotalChipList)
				{
					if (totalChip.ChipID == tMemberInfo.netSealBattleSettingInfo.MainWeaponInfo.Chip)
					{
						refPassiveskill.AddPassivesSkill(totalChip, 4, 1, true);
					}
					else if (totalChip.ChipID == tMemberInfo.netSealBattleSettingInfo.SubWeaponInfo.Chip)
					{
						refPassiveskill.AddPassivesSkill(totalChip, 8, 1, true);
					}
				}
			}
			else
			{
				if (ManagedSingleton<PlayerNetManager>.Instance.dicWeapon.TryGetValue(tStageCharacterStruct.MainWeaponID, out value))
				{
					if (value.netSkillInfos != null)
					{
						for (int i = 0; i < value.netSkillInfos.Count; i++)
						{
							refPassiveskill.AddPassivesSkill(value.netSkillInfos[i], 4, value.netSkillInfos[i].Level);
						}
					}
					ChipInfo value2 = null;
					if (ManagedSingleton<PlayerNetManager>.Instance.dicChip.TryGetValue(value.netInfo.Chip, out value2))
					{
						refPassiveskill.AddPassivesSkill(value2.netChipInfo, 4, 1, true);
						if (!refPassiveskill.tMainPassiveskillStatus.tSKILL_TABLE.s_USE_MOTION.Equals("null"))
						{
							StageUpdate.LoadCallBackObj loadCallBackObj = new StageUpdate.LoadCallBackObj();
							AddCb(loadCallBackObj);
							MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad<GameObject>("prefab/fx/chipeffect/" + refPassiveskill.tMainPassiveskillStatus.tSKILL_TABLE.s_USE_MOTION, refPassiveskill.tMainPassiveskillStatus.tSKILL_TABLE.s_USE_MOTION, loadCallBackObj.LoadCB);
							LoadFx("Fx_ChipActive", 3);
						}
					}
				}
				if (ManagedSingleton<PlayerNetManager>.Instance.dicWeapon.TryGetValue(tStageCharacterStruct.SubWeaponID, out value))
				{
					if (value.netSkillInfos != null)
					{
						for (int j = 0; j < value.netSkillInfos.Count; j++)
						{
							refPassiveskill.AddPassivesSkill(value.netSkillInfos[j], 8, value.netSkillInfos[j].Level);
						}
					}
					ChipInfo value3 = null;
					if (ManagedSingleton<PlayerNetManager>.Instance.dicChip.TryGetValue(value.netInfo.Chip, out value3))
					{
						refPassiveskill.AddPassivesSkill(value3.netChipInfo, 8, 1, true);
						if (!refPassiveskill.tSubPassiveskillStatus.tSKILL_TABLE.s_USE_MOTION.Equals("null"))
						{
							StageUpdate.LoadCallBackObj loadCallBackObj2 = new StageUpdate.LoadCallBackObj();
							AddCb(loadCallBackObj2);
							MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad<GameObject>("prefab/fx/chipeffect/" + refPassiveskill.tSubPassiveskillStatus.tSKILL_TABLE.s_USE_MOTION, refPassiveskill.tSubPassiveskillStatus.tSKILL_TABLE.s_USE_MOTION, loadCallBackObj2.LoadCB);
							LoadFx("Fx_ChipActive", 3);
						}
					}
				}
			}
			Debug.Log("LoadAllPassSkill 1");
			CharacterInfo value4 = null;
			if (tMemberInfo != null)
			{
				foreach (NetCharacterSkillInfo characterSkill in tMemberInfo.netSealBattleSettingInfo.CharacterSkillList)
				{
					if (characterSkill.CharacterID == tStageCharacterStruct.StandbyChara)
					{
						if (StageUpdate.gbRegisterPvpPlayer)
						{
							characterSkill.Level = 1;
						}
						refPassiveskill.AddPassivesSkill(characterSkill);
					}
				}
			}
			else if (ManagedSingleton<PlayerNetManager>.Instance.dicCharacter.TryGetValue(tStageCharacterStruct.StandbyChara, out value4))
			{
				Dictionary<CharacterSkillSlot, NetCharacterSkillInfo>.Enumerator enumerator4 = value4.netSkillDic.GetEnumerator();
				while (enumerator4.MoveNext())
				{
					refPassiveskill.AddPassivesSkill(enumerator4.Current.Value);
				}
			}
			Debug.Log("LoadAllPassSkill 2");
			CHARACTER_TABLE cHARACTER_TABLE = ManagedSingleton<OrangeDataManager>.Instance.CHARACTER_TABLE_DICT[tStageCharacterStruct.StandbyChara];
			refPassiveskill.AddPassivesSkill(cHARACTER_TABLE.n_INITIAL_SKILL1);
			refPassiveskill.AddPassivesSkill(cHARACTER_TABLE.n_INITIAL_SKILL2);
			refPassiveskill.AddPassivesSkill(cHARACTER_TABLE.n_INITIAL_SKILL3);
			List<int> charactertCardSkillList = MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.GetCharactertCardSkillList(cHARACTER_TABLE.n_ID);
			if (charactertCardSkillList != null)
			{
				for (int k = 0; k < charactertCardSkillList.Count; k++)
				{
					refPassiveskill.AddPassivesSkill(charactertCardSkillList[k]);
				}
			}
			List<int> list = new List<int>();
			Debug.Log("LoadAllPassSkill 3");
			if (tMemberInfo != null)
			{
				if (tMemberInfo.netSealBattleSettingInfo.EquipmentList.Count > 0)
				{
					for (int l = 0; l < tMemberInfo.netSealBattleSettingInfo.EquipmentList.Count; l++)
					{
						EquipInfo equipInfo = new EquipInfo();
						equipInfo.netEquipmentInfo = tMemberInfo.netSealBattleSettingInfo.EquipmentList[l];
						list.Add(equipInfo.netEquipmentInfo.EquipItemID);
					}
				}
			}
			else
			{
				IEnumerator<KeyValuePair<int, EquipInfo>> enumerator5 = ManagedSingleton<PlayerNetManager>.Instance.dicEquip.Where((KeyValuePair<int, EquipInfo> obj) => obj.Value.netEquipmentInfo.Equip != 0).GetEnumerator();
				while (enumerator5.MoveNext())
				{
					list.Add(enumerator5.Current.Value.netEquipmentInfo.EquipItemID);
				}
			}
			Debug.Log("LoadAllPassSkill 4");
			Dictionary<int, SUIT_TABLE>.Enumerator enumerator6 = ManagedSingleton<OrangeDataManager>.Instance.SUIT_TABLE_DICT.GetEnumerator();
			int num = 0;
			while (enumerator6.MoveNext())
			{
				num = 0;
				int[] array = new int[6]
				{
					enumerator6.Current.Value.n_EQUIP_1,
					enumerator6.Current.Value.n_EQUIP_2,
					enumerator6.Current.Value.n_EQUIP_3,
					enumerator6.Current.Value.n_EQUIP_4,
					enumerator6.Current.Value.n_EQUIP_5,
					enumerator6.Current.Value.n_EQUIP_6
				};
				for (int num2 = list.Count - 1; num2 >= 0; num2--)
				{
					int[] array2 = array;
					for (int m = 0; m < array2.Length; m++)
					{
						if (array2[m] == list[num2])
						{
							num++;
						}
					}
				}
				if (num >= enumerator6.Current.Value.n_SUIT_1)
				{
					refPassiveskill.AddPassivesSkill(enumerator6.Current.Value.n_EFFECT_1, 64);
				}
				if (num >= enumerator6.Current.Value.n_SUIT_2)
				{
					refPassiveskill.AddPassivesSkill(enumerator6.Current.Value.n_EFFECT_2, 64);
				}
				if (num >= enumerator6.Current.Value.n_SUIT_3)
				{
					refPassiveskill.AddPassivesSkill(enumerator6.Current.Value.n_EFFECT_3, 64);
				}
			}
			Debug.Log("LoadAllPassSkill 5");
			if (ManagedSingleton<StageHelper>.Instance.nLastStageRuleID_Status > 0)
			{
				STAGE_RULE_TABLE value5 = null;
				if (ManagedSingleton<OrangeDataManager>.Instance.STAGE_RULE_TABLE_DICT.TryGetValue(ManagedSingleton<StageHelper>.Instance.nLastStageRuleID_Status, out value5))
				{
					refPassiveskill.AddPassivesSkill(value5.n_PASSIVE_SKILL1);
					refPassiveskill.AddPassivesSkill(value5.n_PASSIVE_SKILL2);
					refPassiveskill.AddPassivesSkill(value5.n_PASSIVE_SKILL3);
				}
			}
			Debug.Log("LoadAllPassSkill 6");
			refPassiveskill.ReCalcuPassiveskillSelf();
			refPassiveskill.PreloadAllPassiveskill(LoadBulletBySkillTableVoid, LoadBuffSound);
			return refPassiveskill;
		}

		public static RaycastHit2D ObjMoveCollisionWithBoxCheckX(BoxCollider2D tB2D, Vector2 vDir, float fTestLen, LayerMask mask, List<Transform> listIgnoreCollisions = null, List<Transform> listCheckCollisions = null)
		{
			Vector3 vector = tB2D.offset * tB2D.transform.lossyScale;
			Quaternion rotation = tB2D.transform.rotation;
			vector = rotation * vector + tB2D.transform.position;
			Vector2 size = tB2D.size * tB2D.transform.lossyScale;
			size.x = Mathf.Abs(size.x);
			size.y = Mathf.Abs(size.y);
			return ObjMoveCollisionWithBoxCheck(vector, size, 0f - rotation.eulerAngles.z, vDir, fTestLen, mask, listIgnoreCollisions, listCheckCollisions);
		}

		public static RaycastHit2D ObjMoveCollisionWithBoxCheckX(BoxCollider2D tB2D, Vector3 offset, Vector2 vDir, float fTestLen, LayerMask mask, List<Transform> listIgnoreCollisions = null, List<Transform> listCheckCollisions = null)
		{
			Vector3 vector = tB2D.offset * tB2D.transform.lossyScale;
			Quaternion rotation = tB2D.transform.rotation;
			vector = rotation * vector + tB2D.transform.position;
			Vector2 size = tB2D.size * tB2D.transform.lossyScale;
			size.x = Mathf.Abs(size.x);
			size.y = Mathf.Abs(size.y);
			return ObjMoveCollisionWithBoxCheck(vector + offset, size, 0f - rotation.eulerAngles.z, vDir, fTestLen, mask, listIgnoreCollisions, listCheckCollisions);
		}

		public static int ObjMoveCollisionNumWithBoxCheckX(BoxCollider2D tB2D, Vector3 offset, Vector2 vDir, float fTestLen, LayerMask mask)
		{
			Vector3 vector = tB2D.offset * tB2D.transform.lossyScale;
			Quaternion rotation = tB2D.transform.rotation;
			vector = rotation * vector + tB2D.transform.position;
			Vector2 size = tB2D.size * tB2D.transform.lossyScale;
			size.x = Mathf.Abs(size.x);
			size.y = Mathf.Abs(size.y);
			return ObjMoveCollisionNumWithBoxCheck(vector + offset, size, 0f - rotation.eulerAngles.z, vDir, fTestLen, mask);
		}

		public static RaycastHit2D ObjMoveCollisionWithBoxCheck(Controller2D tC2D, Vector2 vDir, float fTestLen, LayerMask mask, List<Transform> listIgnoreCollisions = null, List<Transform> listCheckCollisions = null)
		{
			Bounds newNowBounds = tC2D.GetNewNowBounds();
			return ObjMoveCollisionWithBoxCheck(newNowBounds.center, newNowBounds.size, 0f, vDir, fTestLen, mask, listIgnoreCollisions, listCheckCollisions);
		}

		public static RaycastHit2D ObjMoveCollisionWithBoxCheck(Controller2D tC2D, Vector3 offset, Vector2 vDir, float fTestLen, LayerMask mask, List<Transform> listIgnoreCollisions = null, List<Transform> listCheckCollisions = null)
		{
			Bounds newNowBounds = tC2D.GetNewNowBounds();
			return ObjMoveCollisionWithBoxCheck(newNowBounds.center + offset, newNowBounds.size, 0f, vDir, fTestLen, mask, listIgnoreCollisions, listCheckCollisions);
		}

		public static RaycastHit2D ObjMoveCollisionWithBoxCheck(BoxCollider2D tB2D, Vector2 vDir, float fTestLen, LayerMask mask, List<Transform> listIgnoreCollisions = null, List<Transform> listCheckCollisions = null)
		{
			Bounds bounds = tB2D.bounds;
			return ObjMoveCollisionWithBoxCheck(bounds.center, bounds.size, 0f, vDir, fTestLen, mask, listIgnoreCollisions, listCheckCollisions);
		}

		public static RaycastHit2D ObjMoveCollisionWithBoxCheck(BoxCollider2D tB2D, Vector3 offset, Vector2 vDir, float fTestLen, LayerMask mask, List<Transform> listIgnoreCollisions = null, List<Transform> listCheckCollisions = null)
		{
			Bounds bounds = tB2D.bounds;
			return ObjMoveCollisionWithBoxCheck(bounds.center + offset, bounds.size, 0f, vDir, fTestLen, mask, listIgnoreCollisions, listCheckCollisions);
		}

		public static RaycastHit2D[] GetHitArray()
		{
			return hitArray;
		}

		public static RaycastHit2D ObjMoveCollisionWithBoxCheck(Vector2 center, Vector2 size, float fAngle, Vector2 vDir, float fTestLen, LayerMask mask, List<Transform> listIgnoreCollisions = null, List<Transform> listCheckCollisions = null)
		{
			RaycastHit2D result = default(RaycastHit2D);
			float num = 9999f;
			if (fTestLen == 0f)
			{
				return result;
			}
			int num2 = Physics2D.BoxCastNonAlloc(center, size, fAngle, vDir, hitArray, fTestLen, mask);
			for (int i = 0; i < num2; i++)
			{
				if (listIgnoreCollisions != null)
				{
					if (!listIgnoreCollisions.Contains(hitArray[i].transform) && (listCheckCollisions == null || listCheckCollisions.Contains(hitArray[i].transform)) && num > hitArray[i].distance && Vector2.Dot(vDir, hitArray[i].normal) < 0f)
					{
						num = hitArray[i].distance;
						result = hitArray[i];
					}
				}
				else if ((listCheckCollisions == null || listCheckCollisions.Contains(hitArray[i].transform)) && num > hitArray[i].distance && Vector2.Dot(vDir, hitArray[i].normal) < 0f)
				{
					num = hitArray[i].distance;
					result = hitArray[i];
				}
			}
			return result;
		}

		public static int ObjMoveCollisionNumWithBoxCheck(Vector2 center, Vector2 size, float fAngle, Vector2 vDir, float fTestLen, LayerMask mask)
		{
			if (fTestLen == 0f)
			{
				return 0;
			}
			return Physics2D.BoxCastNonAlloc(center, size, fAngle, vDir, hitArray, fTestLen, mask);
		}

		public static Collider2D ObjOverlapWithBoxCheckX(BoxCollider2D tB2D, Vector3 tmpMove, LayerMask mask, List<Transform> listIgnoreCollisions = null, List<Transform> listCheckCollisions = null)
		{
			Vector3 vector = tB2D.offset * tB2D.transform.lossyScale;
			Quaternion rotation = tB2D.transform.rotation;
			vector = rotation * vector + tB2D.transform.position;
			Vector2 size = tB2D.size * tB2D.transform.lossyScale;
			size.x = Mathf.Abs(size.x);
			size.y = Mathf.Abs(size.y);
			return ObjOverlapWithBoxCheck(vector + tmpMove, size, 0f - rotation.eulerAngles.z, mask, listIgnoreCollisions, listCheckCollisions);
		}

		public static Collider2D ObjOverlapWithBoxCheck(Vector2 center, Vector2 size, float fAngle, LayerMask mask, List<Transform> listIgnoreCollisions = null, List<Transform> listCheckCollisions = null)
		{
			int num = Physics2D.OverlapBoxNonAlloc(center, size, fAngle, overlapC2Ds, mask);
			for (int i = 0; i < num; i++)
			{
				if (listIgnoreCollisions != null)
				{
					if (!listIgnoreCollisions.Contains(overlapC2Ds[i].transform) && (listCheckCollisions == null || listCheckCollisions.Contains(overlapC2Ds[i].transform)))
					{
						return overlapC2Ds[i];
					}
				}
				else if (listCheckCollisions == null || listCheckCollisions.Contains(overlapC2Ds[i].transform))
				{
					return overlapC2Ds[i];
				}
			}
			return null;
		}

		public static void DebugDrawBoxX(Vector2 point, Vector2 size, float angle, Color color, float duration)
		{
			Quaternion quaternion = Quaternion.Euler(0f, 0f, angle);
			Vector2 vector = quaternion * Vector2.right * size.x / 2f;
			Vector2 vector2 = quaternion * Vector2.up * size.y / 2f;
			Vector2 vector3 = point + vector2 - vector;
			Vector2 vector4 = point + vector2 + vector;
			Vector2 vector5 = point - vector2 + vector;
			Vector2 vector6 = point - vector2 - vector;
			Debug.DrawLine(vector3, vector4, color, duration);
			Debug.DrawLine(vector4, vector5, color, duration);
			Debug.DrawLine(vector5, vector6, color, duration);
			Debug.DrawLine(vector6, vector3, color, duration);
		}

		public static bool CheckBoundsIntersectNoZEffect(ref Bounds tAB, ref Bounds tBB)
		{
			Vector3 vector = tAB.center - tBB.center;
			Vector3 vector2 = tAB.extents + tBB.extents;
			if (Mathf.Abs(vector.x) < vector2.x && Mathf.Abs(vector.y) < vector2.y)
			{
				return true;
			}
			return false;
		}

		public static bool CheckBoundsContainNoZEffect(ref Bounds tAB, ref Vector3 tP)
		{
			Vector3 vector = tAB.center - tP;
			Vector3 extents = tAB.extents;
			if (Mathf.Abs(vector.x) < extents.x && Mathf.Abs(vector.y) < extents.y)
			{
				return true;
			}
			return false;
		}

		public static bool CheckBoundsContainNoZEffect(ref Bounds tAB, ref Bounds tBB)
		{
			if (tAB.max.x < tBB.max.x || tAB.max.y < tBB.max.y || tAB.min.x > tBB.min.x || tAB.min.y > tBB.min.y)
			{
				return false;
			}
			return true;
		}

		public static void ResetStageUpdate()
		{
			stageUpdate = null;
		}

		public static StageUpdate GetStageUpdate()
		{
			if ((object)stageUpdate != null)
			{
				return stageUpdate;
			}
			stageUpdate = null;
			GameObject[] rootGameObjects = SceneManager.GetActiveScene().GetRootGameObjects();
			for (int i = 0; i < rootGameObjects.Length; i++)
			{
				if (rootGameObjects[i].name == "StageUpdate")
				{
					stageUpdate = rootGameObjects[i].GetComponent<StageUpdate>();
					break;
				}
			}
			return stageUpdate;
		}

		public static T[] GetAllTypeOfObjs<T>()
		{
			List<T> list = new List<T>();
			GameObject[] rootGameObjects = SceneManager.GetActiveScene().GetRootGameObjects();
			for (int i = 0; i < rootGameObjects.Length; i++)
			{
				list.AddRange(rootGameObjects[i].GetComponentsInChildren<T>(true));
			}
			return list.ToArray();
		}

		public static IEnumerator TweenFloatCoroutine(float fStart, float fEnd, float fTime, Action<float> updatecb, Action endcb)
		{
			float fD = (fEnd - fStart) / fTime;
			float fNowTime = fTime;
			float fTmpStart = fStart;
			yield return CoroutineDefine._waitForEndOfFrame;
			while (fNowTime > 0f)
			{
				float num = Time.deltaTime;
				if (num > fNowTime)
				{
					num = fNowTime;
				}
				fTmpStart += num * fD;
				fNowTime -= num;
				if (updatecb != null)
				{
					updatecb(fTmpStart);
				}
				yield return CoroutineDefine._waitForEndOfFrame;
				while (MonoBehaviourSingleton<OrangeGameManager>.Instance.IsGamePause)
				{
					yield return CoroutineDefine._waitForEndOfFrame;
				}
			}
			if (endcb != null)
			{
				endcb();
			}
		}

		public static IEnumerator TweenVc3Coroutine(Vector3 vStart, Vector3 vEnd, float fTime, Action<Vector3> updatecb, Action endcb)
		{
			Vector3 fD = (vEnd - vStart) / fTime;
			float fNowTime = fTime;
			Vector3 vTmpStart = vStart;
			yield return CoroutineDefine._waitForEndOfFrame;
			while (fNowTime > 0f)
			{
				float num = Time.deltaTime;
				if (num > fNowTime)
				{
					num = fNowTime;
				}
				vTmpStart += num * fD;
				fNowTime -= num;
				if (updatecb != null)
				{
					updatecb(vTmpStart);
				}
				yield return CoroutineDefine._waitForEndOfFrame;
				while (MonoBehaviourSingleton<OrangeGameManager>.Instance.IsGamePause)
				{
					yield return CoroutineDefine._waitForEndOfFrame;
				}
			}
			if (endcb != null)
			{
				endcb();
			}
		}

		public static IEnumerator TweenObjScale(GameObject tGO, Vector3 vScaleEnd, float fTime)
		{
			Vector3 vScaleStart = tGO.transform.localScale;
			Vector3 vScaleDIff = vScaleEnd - vScaleStart;
			for (float fScaleTime = 0f; fScaleTime < fTime; fScaleTime += Time.deltaTime)
			{
				tGO.transform.localScale = vScaleStart + (1f - (fTime - fScaleTime) / fTime) * vScaleDIff;
				yield return CoroutineDefine._waitForEndOfFrame;
				while (MonoBehaviourSingleton<OrangeGameManager>.Instance.IsGamePause)
				{
					yield return CoroutineDefine._waitForEndOfFrame;
				}
			}
		}

		public static string[] GetAllFileInPath(string path, string searchpattern = "*.prefab")
		{
			List<string> list = new List<string>();
			string[] array = searchpattern.Split(';');
			for (int i = 0; i < array.Length; i++)
			{
				string[] files = Directory.GetFiles(path, array[i]);
				foreach (string text in files)
				{
					list.Add(text.Replace("\\", "/"));
				}
				files = Directory.GetDirectories(path);
				foreach (string text2 in files)
				{
					if (!text2.Contains("Materials"))
					{
						string[] allFileInPath = GetAllFileInPath(text2, array[i]);
						foreach (string text3 in allFileInPath)
						{
							list.Add(text3.Replace("\\", "/"));
						}
					}
				}
			}
			return list.ToArray();
		}

		public static float[] GetObjMinx(Transform tTrans)
		{
			float[] array = new float[2];
			array[0] = (array[1] = tTrans.position.x);
			Renderer component = tTrans.GetComponent<Renderer>();
			if (component != null)
			{
				if (component.bounds.min.x < array[0])
				{
					array[0] = component.bounds.min.x;
				}
				if (component.bounds.max.x > array[1])
				{
					array[1] = component.bounds.max.x;
				}
			}
			int childCount = tTrans.childCount;
			for (int i = 0; i < childCount; i++)
			{
				float[] objMinx = GetObjMinx(tTrans.GetChild(i));
				if (objMinx[0] < array[0])
				{
					array[0] = objMinx[0];
				}
				if (objMinx[1] > array[1])
				{
					array[1] = objMinx[1];
				}
			}
			return array;
		}
	}
}
