#define RELEASE
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CallbackDefs;
using OrangeAudio;
using StageLib;
using UnityEngine;
using enums;

public class PlayerBuilder : MonoBehaviour
{
	private enum PlayerParts
	{
		PlayerObject = 0,
		BodyMesh = 1,
		SpeedLineParticle = 2,
		ChargeLV1Particle = 3,
		ChargeLV2Particle = 4,
		ChargeLV3Particle = 5,
		DustParticle = 6,
		ThrusterParticle = 7,
		Animator = 8,
		WallkickParticle = 9,
		LandParticle = 10,
		JumpUpParticle = 11,
		JumpLeftParticle = 12,
		JumpRightParticle = 13,
		DashSmokeParticle = 14,
		Voice = 15,
		CharaSE = 16,
		SkillSE = 17,
		ChargeStartParticle = 18,
		ChargeLV1Particle2 = 19,
		ChargeLV2Particle2 = 20,
		ChargeLV3Particle2 = 21,
		ChargeStartParticle2 = 22,
		MAX_PARTS = 23
	}

	public class PlayerBuildParam
	{
		public string sPlayerID = "";

		public int CharacterID = 1;

		public int CharacterSkinID;

		public int[] WeaponList = new int[2];

		public int[] WeaponChipList = new int[2];

		public int[] SkillLv = new int[2];

		public int[] EnhanceEXIndex = new int[2];

		public ChargeData tChargeData;

		public FinalStrikeInfo[] FSkillList = new FinalStrikeInfo[2];

		public string sPlayerName = "";

		public WeaponStatus mainWStatus;

		public WeaponStatus subWStatus;

		public WeaponStatus chipStatus;

		public PlayerStatus tPlayerStatus;

		public RefPassiveskill tRefPassiveskill;

		public CharacterDirection tSetCharacterDir = CharacterDirection.LEFT;

		public NetControllerSetting netControllerSetting;
	}

	private const string _wallKickSparkFx = "OBJ_WALLKICK_SPARK";

	private const string _landFx = "OBJ_LAND";

	private const string _jumpUpFx = "OBJ_JUMP_UP";

	private const string _jumpLeftFx = "OBJ_JUMP_LEFT";

	private const string _jumpRightFx = "OBJ_JUMP_RIGHT";

	private const string _dashSmokeFx = "OBJ_DASH_SMOKE";

	private const int MaxSkill = 2;

	private const int MaxFSkill = 2;

	private const int MaxWeapon = 2;

	private const int MaxDefaultWeapon = 2;

	public const int MaxSubWeapon = 10;

	private const float fConstWaitNetTimeOut = 10f;

	private UnityEngine.Object[] _loadedParts;

	private Sprite[] _loadedSkillIcons;

	private Sprite[] _loadedFSkillIcons;

	private UnityEngine.Object[][] _loadedWeapons;

	private Sprite[] _loadedWeaponIcons;

	private UnityEngine.Object[][] _loadedDefaultWeapons;

	private UnityEngine.Object _loadfxDummy;

	private int[] SkillList = new int[2];

	private int[] DefaultWeaponList = new int[2];

	private string _playerModel;

	private string _playerAnimator;

	private string _playTeleportInFx;

	private string _playTeleportOutFx;

	public bool CreateAtStart = true;

	public bool IsLocalPlayer = true;

	public bool IsSceneInitialized;

	public bool IsReadyGoEnd;

	public bool IsWaitNetCtrl;

	private bool bNeedLockInput;

	public bool IsJustNPC;

	[HideInInspector]
	public string uid = "";

	public PlayerBuildParam SetPBP = new PlayerBuildParam();

	[HideInInspector]
	public bool bShowStartEffect = true;

	private List<StageUpdate.LoadCallBackObj> listLoads = new List<StageUpdate.LoadCallBackObj>();

	private void Start()
	{
		if (CreateAtStart)
		{
			CreatePlayer();
		}
	}

	private void OnDestroy()
	{
		Singleton<GenericEventManager>.Instance.DetachEvent(EventManager.ID.SCENE_INIT, SceneInitialized);
		Singleton<GenericEventManager>.Instance.DetachEvent(EventManager.ID.PLAYERBUILD_PLAYER_SPAWN, ReadyGoEndNotify);
		Singleton<GenericEventManager>.Instance.DetachEvent<string, bool>(EventManager.ID.PLAYERBUILD_PLAYER_NETCTRLON, NetCtrlNotify);
	}

	public void SceneInitialized()
	{
		IsSceneInitialized = true;
	}

	public void ReadyGoEndNotify()
	{
		IsReadyGoEnd = true;
	}

	public void NetCtrlNotify(string suid, bool bNeedInputLock)
	{
		if (suid == uid)
		{
			IsWaitNetCtrl = false;
			if (bNeedInputLock)
			{
				bNeedLockInput = true;
			}
		}
	}

	private void LoadWeapons(int[] WeaponList, UnityEngine.Object[][] targetWeaponList)
	{
		for (int i = 0; i < WeaponList.Length; i++)
		{
			if (WeaponList[i] == -1)
			{
				continue;
			}
			WEAPON_TABLE wEAPON_TABLE = ManagedSingleton<OrangeDataManager>.Instance.WEAPON_TABLE_DICT[WeaponList[i]];
			int num = 0;
			targetWeaponList[i] = new UnityEngine.Object[10];
			while (true)
			{
				int i2 = i;
				int num2 = num;
				StageUpdate.LoadCallBackObj newLoadCbObj = GetNewLoadCbObj();
				newLoadCbObj.i = i2;
				newLoadCbObj.objParam0 = num2;
				newLoadCbObj.lcb = delegate(StageUpdate.LoadCallBackObj tLCB, UnityEngine.Object obj)
				{
					Debug.Log("Loaded Weapon Model" + tLCB.i + "Sub" + (int)tLCB.objParam0);
					targetWeaponList[tLCB.i][(int)tLCB.objParam0] = obj;
				};
				MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad<UnityEngine.Object>(AssetBundleScriptableObject.Instance.m_newmodel_weapon + wEAPON_TABLE.s_MODEL, wEAPON_TABLE.s_MODEL + "_G.prefab", newLoadCbObj.LoadCB);
				if (wEAPON_TABLE.n_SUB_LINK == -1)
				{
					break;
				}
				wEAPON_TABLE = ManagedSingleton<OrangeDataManager>.Instance.WEAPON_TABLE_DICT[wEAPON_TABLE.n_SUB_LINK];
				num++;
			}
		}
	}

	private void InstantiateWeapons(int[] WeaponList, ref Transform[] bodyInstance, bool isSkill, float weaponSize = 1f)
	{
		string text = (isSkill ? "SkillWeapon" : "NormalWeapon");
		WEAPON_TABLE[] array = new WEAPON_TABLE[2];
		int num = 0;
		UnityEngine.Object[][] array2 = (isSkill ? _loadedDefaultWeapons : _loadedWeapons);
		foreach (UnityEngine.Object[] array3 in array2)
		{
			if (array3 == null)
			{
				continue;
			}
			int num2 = 0;
			UnityEngine.Object[] array4 = array3;
			foreach (UnityEngine.Object @object in array4)
			{
				if (!(@object == null))
				{
					GameObject gameObject = UnityEngine.Object.Instantiate((GameObject)@object, Vector3.zero, Quaternion.identity);
					gameObject.name = text + num;
					array[num] = ManagedSingleton<OrangeDataManager>.Instance.WEAPON_TABLE_DICT[WeaponList[num]];
					if (IsLocalPlayer)
					{
						OrangeBattleUtility.ChangeLayersRecursively<Transform>(gameObject.transform, ManagedSingleton<OrangeLayerManager>.Instance.RenderPlayer);
					}
					switch ((WeaponType)(short)array[num].n_TYPE)
					{
					case WeaponType.Buster:
					case WeaponType.Spray:
					case WeaponType.SprayHeavy:
						gameObject.transform.SetParent(OrangeBattleUtility.FindChildRecursive(ref bodyInstance, "L BusterPoint"));
						gameObject.transform.localEulerAngles = Vector3.zero;
						break;
					case WeaponType.Melee:
						gameObject.transform.SetParent(OrangeBattleUtility.FindChildRecursive(ref bodyInstance, "R WeaponPoint"));
						gameObject.transform.localEulerAngles = new Vector3(0f, -90f, 90f);
						break;
					case WeaponType.MGun:
						gameObject.transform.SetParent(OrangeBattleUtility.FindChildRecursive(ref bodyInstance, "R WeaponPoint"));
						gameObject.transform.localEulerAngles = Vector3.zero;
						break;
					case WeaponType.Gatling:
					case WeaponType.Launcher:
						gameObject.transform.SetParent(OrangeBattleUtility.FindChildRecursive(ref bodyInstance, "L WeaponPoint"));
						gameObject.transform.localEulerAngles = Vector3.zero;
						break;
					case WeaponType.DualGun:
						gameObject.transform.SetParent((num2 == 0) ? OrangeBattleUtility.FindChildRecursive(ref bodyInstance, "L WeaponPoint") : OrangeBattleUtility.FindChildRecursive(ref bodyInstance, "R WeaponPoint"));
						gameObject.transform.localEulerAngles = Vector3.zero;
						break;
					}
					gameObject.transform.localScale = new Vector3(weaponSize, weaponSize, weaponSize);
					gameObject.transform.localPosition = Vector3.zero;
					num2++;
				}
			}
			num++;
		}
	}

	private void LoadWeaponBullet(WEAPON_TABLE weaponTable)
	{
		if (weaponTable.n_SKILL == 0)
		{
			return;
		}
		SKILL_TABLE sKILL_TABLE = ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT[weaponTable.n_SKILL];
		switch ((WeaponType)(short)weaponTable.n_TYPE)
		{
		case WeaponType.Buster:
		case WeaponType.Spray:
		case WeaponType.SprayHeavy:
		case WeaponType.DualGun:
		case WeaponType.MGun:
		case WeaponType.Gatling:
		case WeaponType.Launcher:
		{
			for (int i = 0; i <= sKILL_TABLE.n_CHARGE_MAX_LEVEL; i++)
			{
				SKILL_TABLE sKILL_TABLE2 = ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT[weaponTable.n_SKILL + i];
				StageUpdate.LoadCallBackObj newLoadCbObj = GetNewLoadCbObj();
				newLoadCbObj.i = i;
				newLoadCbObj.loadStageObjData = sKILL_TABLE2;
				newLoadCbObj.lcb = delegate(StageUpdate.LoadCallBackObj tLCB, UnityEngine.Object obj)
				{
					SKILL_TABLE sKILL_TABLE3 = tLCB.loadStageObjData as SKILL_TABLE;
					switch ((BulletType)(short)sKILL_TABLE3.n_TYPE)
					{
					case BulletType.Continuous:
					{
						ContinuousBullet component5 = ((GameObject)obj).GetComponent<ContinuousBullet>();
						MonoBehaviourSingleton<PoolManager>.Instance.CreatePoolBaseLocal<ContinuousBullet>(UnityEngine.Object.Instantiate(component5), sKILL_TABLE3.s_MODEL, 5);
						break;
					}
					case BulletType.Spray:
					{
						SprayBullet component4 = ((GameObject)obj).GetComponent<SprayBullet>();
						MonoBehaviourSingleton<PoolManager>.Instance.CreatePoolBaseLocal<SprayBullet>(UnityEngine.Object.Instantiate(component4), sKILL_TABLE3.s_MODEL, 5);
						break;
					}
					case BulletType.Collide:
					{
						CollideBullet component3 = ((GameObject)obj).GetComponent<CollideBullet>();
						MonoBehaviourSingleton<PoolManager>.Instance.CreatePoolBaseLocal<CollideBullet>(UnityEngine.Object.Instantiate(component3), sKILL_TABLE3.s_MODEL, 5);
						break;
					}
					case BulletType.LrColliderBulle:
					{
						LrColliderBullet component2 = ((GameObject)obj).GetComponent<LrColliderBullet>();
						MonoBehaviourSingleton<PoolManager>.Instance.CreatePoolBaseLocal<LrColliderBullet>(UnityEngine.Object.Instantiate(component2), sKILL_TABLE3.s_MODEL, 5);
						break;
					}
					default:
					{
						BulletBase component = ((GameObject)obj).GetComponent<BulletBase>();
						MonoBehaviourSingleton<PoolManager>.Instance.CreatePoolBaseLocal<BasicBullet>(UnityEngine.Object.Instantiate(component), sKILL_TABLE3.s_MODEL, 5);
						break;
					}
					}
				};
				MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad<UnityEngine.Object>("prefab/bullet/" + sKILL_TABLE2.s_MODEL, sKILL_TABLE2.s_MODEL, newLoadCbObj.LoadCB);
			}
			break;
		}
		default:
			Debug.Log("Unexpected type :" + weaponTable.n_TYPE);
			break;
		case WeaponType.Melee:
			break;
		}
		if (sKILL_TABLE.n_CONDITION_ID != 0)
		{
			CONDITION_TABLE value = null;
			if (ManagedSingleton<OrangeDataManager>.Instance.CONDITION_TABLE_DICT.TryGetValue(sKILL_TABLE.n_CONDITION_ID, out value) && value.s_DURING_FX != null && value.n_EFFECT != 6 && value.n_EFFECT != 7)
			{
				LoadBuffSound(value.s_DURING_FX);
			}
		}
	}

	private void LoadSkillBullet(SKILL_TABLE bulletTable)
	{
		int num;
		switch (bulletTable.n_USE_DEFAULT_WEAPON)
		{
		case -1:
			num = -1;
			break;
		case 0:
			num = ManagedSingleton<OrangeDataManager>.Instance.CHARACTER_TABLE_DICT[SetPBP.CharacterID].n_INIT_WEAPON1;
			break;
		default:
			num = ManagedSingleton<OrangeDataManager>.Instance.CHARACTER_TABLE_DICT[SetPBP.CharacterID].n_INIT_WEAPON2;
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
				MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxduring_novastrike_000", 2);
				MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxduring_novastrike_001", 10);
				MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxduring_novastrike_002", 2);
				break;
			case "MARINO_DASH":
				MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuse_dash_000", 2);
				MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuse_dashdx_000", 2);
				break;
			case "AXL_ROLL":
				MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("obj_fxuse_rolling_000", 2);
				MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuse_axlskill1", 2);
				MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuse_axlskill2", 5);
				break;
			case "MARINO_DARTS":
			{
				MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuse_darts_000a", 2);
				StageUpdate.LoadCallBackObj newLoadCbObj = GetNewLoadCbObj();
				newLoadCbObj.loadStageObjData = bulletTable.s_MODEL;
				newLoadCbObj.lcb = delegate(StageUpdate.LoadCallBackObj tLCB, UnityEngine.Object obj)
				{
					BulletBase component = ((GameObject)obj).GetComponent<BulletBase>();
					MonoBehaviourSingleton<PoolManager>.Instance.CreatePoolBaseLocal<BulletBase>(UnityEngine.Object.Instantiate(component), tLCB.loadStageObjData as string, 5);
				};
				MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad<UnityEngine.Object>("prefab/bullet/" + bulletTable.s_MODEL, bulletTable.s_MODEL, newLoadCbObj.LoadCB);
				break;
			}
			default:
			{
				if (bulletTable.s_MODEL == "null")
				{
					Debug.LogWarning("子彈為NULL，正常不該這樣");
					break;
				}
				if (bulletTable.n_CHARGE_MAX_LEVEL != 0)
				{
					LoadBulletBySkillTable(bulletTable);
					break;
				}
				if (bulletTable.n_COMBO_SKILL != 0)
				{
					SKILL_TABLE value = null;
					if (ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT.TryGetValue(bulletTable.n_COMBO_SKILL, out value))
					{
						LoadBulletBySkillTable(value);
					}
					break;
				}
				StageUpdate.LoadCallBackObj newLoadCbObj = GetNewLoadCbObj();
				newLoadCbObj.loadStageObjData = bulletTable.s_MODEL;
				newLoadCbObj.lcb = delegate(StageUpdate.LoadCallBackObj tLCB, UnityEngine.Object obj)
				{
					BulletBase component2 = ((GameObject)obj).GetComponent<BulletBase>();
					MonoBehaviourSingleton<PoolManager>.Instance.CreatePoolBaseLocal<BulletBase>(UnityEngine.Object.Instantiate(component2), tLCB.loadStageObjData as string, 5);
				};
				MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad<UnityEngine.Object>("prefab/bullet/" + bulletTable.s_MODEL, bulletTable.s_MODEL, newLoadCbObj.LoadCB);
				if (bulletTable.n_TYPE == 1 && bulletTable.n_SHOTLINE == 5)
				{
					CheckSkill(bulletTable);
				}
				break;
			}
			case "AXL_SPRAYSHOT":
			case "FIRST_X_SKILL2":
				break;
			}
		}
		StageResManager.LoadBuff(bulletTable.n_CONDITION_ID);
	}

	public void LoadBuffSound(string sName)
	{
		if (!(sName == "null") && sName != null)
		{
			StageUpdate.LoadCallBackObj newLoadCbObj = GetNewLoadCbObj();
			MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(sName, 10, newLoadCbObj.LoadCBNoParam);
		}
	}

	public void LoadBulletBySkillTable(SKILL_TABLE bulletTable)
	{
		for (int i = 0; i <= bulletTable.n_CHARGE_MAX_LEVEL; i++)
		{
			SKILL_TABLE sKILL_TABLE = ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT[bulletTable.n_ID + i];
			if (sKILL_TABLE.s_MODEL == "null" || sKILL_TABLE.s_MODEL == "DUMMY")
			{
				continue;
			}
			StageUpdate.LoadCallBackObj newLoadCbObj = GetNewLoadCbObj();
			newLoadCbObj.loadStageObjData = sKILL_TABLE;
			newLoadCbObj.lcb = delegate(StageUpdate.LoadCallBackObj tLCB, UnityEngine.Object obj)
			{
				SKILL_TABLE sKILL_TABLE3 = tLCB.loadStageObjData as SKILL_TABLE;
				switch ((BulletType)(short)sKILL_TABLE3.n_TYPE)
				{
				case BulletType.Continuous:
				{
					ContinuousBullet component10 = ((GameObject)obj).GetComponent<ContinuousBullet>();
					MonoBehaviourSingleton<PoolManager>.Instance.CreatePoolBaseLocal<ContinuousBullet>(UnityEngine.Object.Instantiate(component10), sKILL_TABLE3.s_MODEL, 5);
					break;
				}
				case BulletType.Spray:
				{
					SprayBullet component9 = ((GameObject)obj).GetComponent<SprayBullet>();
					MonoBehaviourSingleton<PoolManager>.Instance.CreatePoolBaseLocal<SprayBullet>(UnityEngine.Object.Instantiate(component9), sKILL_TABLE3.s_MODEL, 5);
					break;
				}
				case BulletType.Collide:
				{
					CollideBullet component8 = ((GameObject)obj).GetComponent<CollideBullet>();
					MonoBehaviourSingleton<PoolManager>.Instance.CreatePoolBaseLocal<CollideBullet>(UnityEngine.Object.Instantiate(component8), sKILL_TABLE3.s_MODEL, 5);
					break;
				}
				case BulletType.LrColliderBulle:
				{
					LrColliderBullet component7 = ((GameObject)obj).GetComponent<LrColliderBullet>();
					MonoBehaviourSingleton<PoolManager>.Instance.CreatePoolBaseLocal<LrColliderBullet>(UnityEngine.Object.Instantiate(component7), sKILL_TABLE3.s_MODEL, 5);
					break;
				}
				default:
				{
					BulletBase component6 = ((GameObject)obj).GetComponent<BulletBase>();
					MonoBehaviourSingleton<PoolManager>.Instance.CreatePoolBaseLocal<BasicBullet>(UnityEngine.Object.Instantiate(component6), sKILL_TABLE3.s_MODEL, 5);
					break;
				}
				}
			};
			MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad<UnityEngine.Object>("prefab/bullet/" + sKILL_TABLE.s_MODEL, sKILL_TABLE.s_MODEL, newLoadCbObj.LoadCB);
			while (sKILL_TABLE.n_COMBO_SKILL != 0)
			{
				sKILL_TABLE = ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT[sKILL_TABLE.n_COMBO_SKILL];
				newLoadCbObj = GetNewLoadCbObj();
				newLoadCbObj.loadStageObjData = sKILL_TABLE;
				newLoadCbObj.lcb = delegate(StageUpdate.LoadCallBackObj tLCB, UnityEngine.Object obj)
				{
					SKILL_TABLE sKILL_TABLE2 = tLCB.loadStageObjData as SKILL_TABLE;
					switch ((BulletType)(short)sKILL_TABLE2.n_TYPE)
					{
					case BulletType.Continuous:
					{
						ContinuousBullet component5 = ((GameObject)obj).GetComponent<ContinuousBullet>();
						MonoBehaviourSingleton<PoolManager>.Instance.CreatePoolBaseLocal<ContinuousBullet>(UnityEngine.Object.Instantiate(component5), sKILL_TABLE2.s_MODEL, 5);
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
						CollideBullet component3 = ((GameObject)obj).GetComponent<CollideBullet>();
						MonoBehaviourSingleton<PoolManager>.Instance.CreatePoolBaseLocal<CollideBullet>(UnityEngine.Object.Instantiate(component3), sKILL_TABLE2.s_MODEL, 5);
						break;
					}
					case BulletType.LrColliderBulle:
					{
						LrColliderBullet component2 = ((GameObject)obj).GetComponent<LrColliderBullet>();
						MonoBehaviourSingleton<PoolManager>.Instance.CreatePoolBaseLocal<LrColliderBullet>(UnityEngine.Object.Instantiate(component2), sKILL_TABLE2.s_MODEL, 5);
						break;
					}
					default:
					{
						BulletBase component = ((GameObject)obj).GetComponent<BulletBase>();
						MonoBehaviourSingleton<PoolManager>.Instance.CreatePoolBaseLocal<BasicBullet>(UnityEngine.Object.Instantiate(component), sKILL_TABLE2.s_MODEL, 5);
						break;
					}
					}
				};
				MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad<UnityEngine.Object>("prefab/bullet/" + sKILL_TABLE.s_MODEL, sKILL_TABLE.s_MODEL, newLoadCbObj.LoadCB);
				CheckSkill(sKILL_TABLE);
			}
			CheckSkill(ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT[bulletTable.n_ID + i]);
		}
		StageResManager.LoadBuff(bulletTable.n_CONDITION_ID);
	}

	private void CheckSkill(SKILL_TABLE chargeBulletTable)
	{
		while (ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT.ContainsKey(chargeBulletTable.n_LINK_SKILL))
		{
			chargeBulletTable = ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT[chargeBulletTable.n_LINK_SKILL];
			StageUpdate.LoadCallBackObj newLoadCbObj = GetNewLoadCbObj();
			newLoadCbObj.loadStageObjData = chargeBulletTable;
			newLoadCbObj.lcb = delegate(StageUpdate.LoadCallBackObj tLCB, UnityEngine.Object obj)
			{
				SKILL_TABLE sKILL_TABLE = tLCB.loadStageObjData as SKILL_TABLE;
				switch ((BulletType)(short)sKILL_TABLE.n_TYPE)
				{
				case BulletType.Continuous:
				{
					ContinuousBullet component5 = ((GameObject)obj).GetComponent<ContinuousBullet>();
					MonoBehaviourSingleton<PoolManager>.Instance.CreatePoolBaseLocal<ContinuousBullet>(UnityEngine.Object.Instantiate(component5), sKILL_TABLE.s_MODEL, 5);
					break;
				}
				case BulletType.Spray:
				{
					SprayBullet component4 = ((GameObject)obj).GetComponent<SprayBullet>();
					MonoBehaviourSingleton<PoolManager>.Instance.CreatePoolBaseLocal<SprayBullet>(UnityEngine.Object.Instantiate(component4), sKILL_TABLE.s_MODEL, 5);
					break;
				}
				case BulletType.Collide:
				{
					CollideBullet component3 = ((GameObject)obj).GetComponent<CollideBullet>();
					MonoBehaviourSingleton<PoolManager>.Instance.CreatePoolBaseLocal<CollideBullet>(UnityEngine.Object.Instantiate(component3), sKILL_TABLE.s_MODEL, 5);
					break;
				}
				case BulletType.LrColliderBulle:
				{
					LrColliderBullet component2 = ((GameObject)obj).GetComponent<LrColliderBullet>();
					MonoBehaviourSingleton<PoolManager>.Instance.CreatePoolBaseLocal<LrColliderBullet>(UnityEngine.Object.Instantiate(component2), sKILL_TABLE.s_MODEL, 5);
					break;
				}
				default:
				{
					BulletBase component = ((GameObject)obj).GetComponent<BulletBase>();
					MonoBehaviourSingleton<PoolManager>.Instance.CreatePoolBaseLocal<BasicBullet>(UnityEngine.Object.Instantiate(component), sKILL_TABLE.s_MODEL, 5);
					break;
				}
				}
			};
			MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad<UnityEngine.Object>("prefab/bullet/" + chargeBulletTable.s_MODEL, chargeBulletTable.s_MODEL, newLoadCbObj.LoadCB);
		}
	}

	private StageUpdate.LoadCallBackObj GetNewLoadCbObj()
	{
		StageUpdate.LoadCallBackObj loadCallBackObj = new StageUpdate.LoadCallBackObj();
		listLoads.Add(loadCallBackObj);
		return loadCallBackObj;
	}

    [Obsolete]
    public void CreatePlayer(CallbackObjs pCallBack = null)
	{
		Singleton<GenericEventManager>.Instance.AttachEvent(EventManager.ID.SCENE_INIT, SceneInitialized);
		Singleton<GenericEventManager>.Instance.AttachEvent(EventManager.ID.PLAYERBUILD_PLAYER_SPAWN, ReadyGoEndNotify);
		Singleton<GenericEventManager>.Instance.AttachEvent<string, bool>(EventManager.ID.PLAYERBUILD_PLAYER_NETCTRLON, NetCtrlNotify);
		StageUpdate.LoadCallBackObj loadCallBackObj = null;
		_loadedWeapons = new UnityEngine.Object[SetPBP.WeaponList.Length][];
		_loadedWeaponIcons = new Sprite[SetPBP.WeaponList.Length];
		_loadedDefaultWeapons = new UnityEngine.Object[2][];
		_loadedSkillIcons = new Sprite[2];
		_loadedFSkillIcons = new Sprite[2];
		_loadedParts = new UnityEngine.Object[23];
		CHARACTER_TABLE character = ManagedSingleton<OrangeDataManager>.Instance.CHARACTER_TABLE_DICT[SetPBP.CharacterID];
		DefaultWeaponList[0] = character.n_INIT_WEAPON1;
		DefaultWeaponList[1] = character.n_INIT_WEAPON2;
		LoadWeapons(DefaultWeaponList, _loadedDefaultWeapons);
		for (int j = 0; j < 2; j++)
		{
			if (SetPBP.FSkillList[j] != null && SetPBP.FSkillList[j].netFinalStrikeInfo.FinalStrikeID > 0)
			{
				SKILL_TABLE fS_SkillTable = ManagedSingleton<OrangeTableHelper>.Instance.getFS_SkillTable(SetPBP.FSkillList[j].netFinalStrikeInfo.FinalStrikeID, SetPBP.FSkillList[j].netFinalStrikeInfo.Level, SetPBP.FSkillList[j].netFinalStrikeInfo.Star);
				if (!fS_SkillTable.s_MODEL.Equals("null"))
				{
					LoadSkillBullet(fS_SkillTable);
				}
				loadCallBackObj = GetNewLoadCbObj();
				loadCallBackObj.i = j;
				loadCallBackObj.lcb = delegate(StageUpdate.LoadCallBackObj tLCB, UnityEngine.Object obj)
				{
					_loadedFSkillIcons[tLCB.i] = obj as Sprite;
				};
				MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad<Sprite>(AssetBundleScriptableObject.Instance.GetIconSkill(fS_SkillTable.s_ICON), fS_SkillTable.s_ICON, loadCallBackObj.LoadCB);
			}
		}
		SkillList[0] = character.n_SKILL1;
		SkillList[1] = character.n_SKILL2;
		for (int k = 0; k < 2; k++)
		{
			int i1 = k;
			SKILL_TABLE sKILL_TABLE = ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT[SkillList[k]];
			LoadSkillBullet(sKILL_TABLE);
			loadCallBackObj = GetNewLoadCbObj();
			loadCallBackObj.lcb = delegate(StageUpdate.LoadCallBackObj tLCB, UnityEngine.Object obj)
			{
				_loadedSkillIcons[i1] = obj as Sprite;
			};
			MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad<Sprite>(AssetBundleScriptableObject.Instance.GetIconSkill(sKILL_TABLE.s_ICON), sKILL_TABLE.s_ICON, loadCallBackObj.LoadCB);
		}
		if (IsLocalPlayer)
		{
			CharacterInfo value = null;
			if (ManagedSingleton<PlayerNetManager>.Instance.dicCharacter.TryGetValue(SetPBP.CharacterID, out value))
			{
				if (value.netSkillDic.ContainsKey(CharacterSkillSlot.ActiveSkill1))
				{
					SetPBP.EnhanceEXIndex[0] = value.netSkillDic[CharacterSkillSlot.ActiveSkill1].Extra;
				}
				if (value.netSkillDic.ContainsKey(CharacterSkillSlot.ActiveSkill2))
				{
					SetPBP.EnhanceEXIndex[1] = value.netSkillDic[CharacterSkillSlot.ActiveSkill2].Extra;
				}
			}
		}
		LoadWeapons(SetPBP.WeaponList, _loadedWeapons);
		for (int l = 0; l < 2; l++)
		{
			WEAPON_TABLE wEAPON_TABLE = ManagedSingleton<OrangeDataManager>.Instance.WEAPON_TABLE_DICT[SetPBP.WeaponList[l]];
			LoadWeaponBullet(wEAPON_TABLE);
			loadCallBackObj = GetNewLoadCbObj();
			loadCallBackObj.i = l;
			loadCallBackObj.lcb = delegate(StageUpdate.LoadCallBackObj tLCB, UnityEngine.Object obj)
			{
				Debug.Log("Loaded Weapon Icon" + tLCB.i);
				_loadedWeaponIcons[tLCB.i] = obj as Sprite;
			};
			MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad<Sprite>(AssetBundleScriptableObject.Instance.m_iconWeapon, wEAPON_TABLE.s_ICON, loadCallBackObj.LoadCB);
		}
		loadCallBackObj = GetNewLoadCbObj();
		loadCallBackObj.lcb = delegate(StageUpdate.LoadCallBackObj tLCB, UnityEngine.Object obj)
		{
			_loadedParts[0] = obj;
		};
		MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad<UnityEngine.Object>("prefab/player", "player", loadCallBackObj.LoadCB);
		_playerModel = character.s_MODEL;
		if (SetPBP.CharacterSkinID > 0)
		{
			SKIN_TABLE value2 = null;
			if (ManagedSingleton<OrangeDataManager>.Instance.SKIN_TABLE_DICT.TryGetValue(SetPBP.CharacterSkinID, out value2))
			{
				_playerModel = value2.s_MODEL;
			}
		}
		loadCallBackObj = GetNewLoadCbObj();
		loadCallBackObj.lcb = delegate(StageUpdate.LoadCallBackObj tLCB, UnityEngine.Object obj)
		{
			_loadedParts[1] = obj;
		};
		MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad<UnityEngine.Object>("model/character/" + _playerModel, _playerModel + "_G.prefab", loadCallBackObj.LoadCB);
		loadCallBackObj = GetNewLoadCbObj();
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("OBJ_WALLKICK_SPARK", 5, loadCallBackObj.LoadCBNoParam);
		loadCallBackObj = GetNewLoadCbObj();
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("OBJ_LAND", 5, loadCallBackObj.LoadCBNoParam);
		loadCallBackObj = GetNewLoadCbObj();
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("OBJ_JUMP_UP", 5, loadCallBackObj.LoadCBNoParam);
		loadCallBackObj = GetNewLoadCbObj();
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("OBJ_JUMP_LEFT", 5, loadCallBackObj.LoadCBNoParam);
		loadCallBackObj = GetNewLoadCbObj();
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("OBJ_JUMP_RIGHT", 5, loadCallBackObj.LoadCBNoParam);
		loadCallBackObj = GetNewLoadCbObj();
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("OBJ_DASH_SMOKE", 5, loadCallBackObj.LoadCBNoParam);
		if (MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.tChargeDataScriptObj == null)
		{
			MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.tChargeDataScriptObj = new ChargeDataScriptObj();
			MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.tChargeDataScriptObj.InitDefaultData();
		}
		string text = _playerModel;
		if (text == "ch035_000")
		{
			switch (SetPBP.EnhanceEXIndex[0])
			{
			case 1:
				text += "1";
				break;
			case 2:
				text += "2";
				break;
			case 3:
				text += "3";
				break;
			}
		}
		ChargeData chargeData = MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.tChargeDataScriptObj.GetChargeData(text);
		SetPBP.tChargeData = chargeData;
		LoadChargeParts(loadCallBackObj, chargeData);
		loadCallBackObj = GetNewLoadCbObj();
		loadCallBackObj.lcb = delegate(StageUpdate.LoadCallBackObj tLCB, UnityEngine.Object obj)
		{
			_loadfxDummy = obj;
		};
		MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad<UnityEngine.Object>("prefab/fx/fxdummy", "fxDummy", loadCallBackObj.LoadCB);
		loadCallBackObj = GetNewLoadCbObj();
		loadCallBackObj.lcb = delegate(StageUpdate.LoadCallBackObj tLCB, UnityEngine.Object obj)
		{
			_loadedParts[2] = obj;
		};
		MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad<UnityEngine.Object>("prefab/fx/fx_dash_speedline", "fx_dash_speedline", loadCallBackObj.LoadCB);
		loadCallBackObj = GetNewLoadCbObj();
		loadCallBackObj.lcb = delegate(StageUpdate.LoadCallBackObj tLCB, UnityEngine.Object obj)
		{
			_loadedParts[6] = obj;
		};
		MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad<UnityEngine.Object>("prefab/fx/rockmandust", "RockmanDust", loadCallBackObj.LoadCB);
		loadCallBackObj = GetNewLoadCbObj();
		loadCallBackObj.lcb = delegate(StageUpdate.LoadCallBackObj tLCB, UnityEngine.Object obj)
		{
			_loadedParts[7] = obj;
		};
		if (character.n_ID == 15 || character.n_ID == 16)
		{
			MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad<UnityEngine.Object>("prefab/fx/fx_dash_air", "fx_dash_air", loadCallBackObj.LoadCB);
		}
		else
		{
			MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad<UnityEngine.Object>("prefab/fx/OBJ_DASH_SPARK", "OBJ_DASH_SPARK", loadCallBackObj.LoadCB);
		}
		_playerAnimator = character.s_ANIMATOR;
		_playerAnimator = "newemptycontroller";
		loadCallBackObj = GetNewLoadCbObj();
		loadCallBackObj.lcb = delegate(StageUpdate.LoadCallBackObj tLCB, UnityEngine.Object obj)
		{
			string text2 = ((RuntimeAnimatorController)obj).name;
			AnimatorOverrideController animatorOverrideController = new AnimatorOverrideController
			{
				runtimeAnimatorController = (RuntimeAnimatorController)obj,
				name = text2
			};
			_loadedParts[8] = animatorOverrideController;
		};
		MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad<RuntimeAnimatorController>("model/animator/" + _playerAnimator, _playerAnimator, loadCallBackObj.LoadCB);
		loadCallBackObj = GetNewLoadCbObj();
		HumanBase.LoadMotion(loadCallBackObj, character.s_ANIMATOR, WeaponType.Dummy);
		loadCallBackObj = GetNewLoadCbObj();
		HumanBase.LoadMotion(loadCallBackObj, character.s_ANIMATOR, (WeaponType)ManagedSingleton<OrangeDataManager>.Instance.WEAPON_TABLE_DICT[SetPBP.WeaponList[0]].n_TYPE);
		loadCallBackObj = GetNewLoadCbObj();
		HumanBase.LoadMotion(loadCallBackObj, character.s_ANIMATOR, (WeaponType)ManagedSingleton<OrangeDataManager>.Instance.WEAPON_TABLE_DICT[SetPBP.WeaponList[1]].n_TYPE);
		int[] array = new int[2] { character.n_INIT_WEAPON1, character.n_INIT_WEAPON2 };
		foreach (int num in array)
		{
			if (num != -1)
			{
				WEAPON_TABLE wEAPON_TABLE2 = ManagedSingleton<OrangeDataManager>.Instance.WEAPON_TABLE_DICT[num];
				loadCallBackObj = GetNewLoadCbObj();
				HumanBase.LoadMotion(loadCallBackObj, character.s_ANIMATOR, (WeaponType)wEAPON_TABLE2.n_TYPE);
			}
		}
		switch (_playerModel)
		{
		case "ch021_000":
		case "ch043_000":
		case "ch035_000":
		case "ch062_000":
		case "ch067_000":
		case "ch072_000":
		case "ch074_000":
		case "ch077_000":
		case "ch078_000":
		case "ch081_000":
		case "ch093_000":
		case "ch118_000":
		case "ch120_000":
		case "ch124_000":
		case "ch131_000":
		case "ch128_000":
		case "ch136_000":
		case "ch139_000":
		case "ch142_000":
			loadCallBackObj = GetNewLoadCbObj();
			HumanBase.LoadMotion(loadCallBackObj, character.s_ANIMATOR, WeaponType.Buster);
			break;
		}
		switch (character.n_ID)
		{
		default:
			_playTeleportInFx = "FX_TELEPORT_IN";
			_playTeleportOutFx = "FX_TELEPORT_OUT";
			break;
		case 101:
			_playTeleportInFx = "fxdemo_valstrax_005";
			_playTeleportOutFx = "fxdemo_valstrax_006";
			break;
		case 102:
			_playTeleportInFx = "fxdemo_zinogre_003";
			_playTeleportOutFx = "fxdemo_zinogre_004";
			break;
		case 104:
			_playTeleportInFx = "fxdemo_loveico_004";
			_playTeleportOutFx = "fxdemo_loveico_005";
			break;
		case 103:
			_playTeleportInFx = "fxdemo_loverico_004";
			_playTeleportOutFx = "fxdemo_loverico_005";
			break;
		case 129:
			_playTeleportInFx = "fxdemo_HalloweenZero_003";
			_playTeleportOutFx = "fxdemo_HalloweenZero_004";
			break;
		case 141:
			_playTeleportInFx = "fxdemo_spx_003";
			_playTeleportOutFx = "fxdemo_spx_003";
			break;
		}
		loadCallBackObj = GetNewLoadCbObj();
		MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad<UnityEngine.Object>("model/animation/character/" + character.s_MODEL, character.s_MODEL, loadCallBackObj.LoadCB);
		loadCallBackObj = GetNewLoadCbObj();
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(_playTeleportInFx, 2, loadCallBackObj.LoadCBNoParam);
		loadCallBackObj = GetNewLoadCbObj();
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("FX_TELEPORT_IN2", 2, loadCallBackObj.LoadCBNoParam);
		loadCallBackObj = GetNewLoadCbObj();
		MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(_playTeleportOutFx, 2, loadCallBackObj.LoadCBNoParam);
		loadCallBackObj = GetNewLoadCbObj();
		_loadedParts[15] = null;
		AudioLib.LoadVoice(ref character, loadCallBackObj.LoadCBNoParam);
		loadCallBackObj = GetNewLoadCbObj();
		_loadedParts[16] = null;
		AudioLib.LoadCharaSE(ref character, loadCallBackObj.LoadCBNoParam);
		loadCallBackObj = GetNewLoadCbObj();
		loadCallBackObj.lcb = delegate
		{
			_loadedParts[17] = null;
		};
		if (character.s_NAME == "Y")
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PreloadAtomSource("SkillSE_X", 2);
			MonoBehaviourSingleton<AudioManager>.Instance.PreloadAtomSource("SkillSE_UltimateX", 2);
			AudioLib.LoadSkillSE(ref character, loadCallBackObj.LoadCBNoParam);
		}
		else
		{
			AudioLib.LoadSkillSE(ref character, loadCallBackObj.LoadCBNoParam);
		}
		StartCoroutine(Build(pCallBack));
	}

    [Obsolete]
    private IEnumerator Build(CallbackObjs pCallBack)
	{
		bool flag = false;
		while (!flag)
		{
			yield return CoroutineDefine._waitForEndOfFrame;
			flag = true;
			for (int i = 0; i < listLoads.Count; i++)
			{
				if (!listLoads[i].bLoadEnd)
				{
					flag = false;
					break;
				}
			}
		}
		CHARACTER_TABLE characterTable = ManagedSingleton<OrangeDataManager>.Instance.CHARACTER_TABLE_DICT[SetPBP.CharacterID];
		float[] characterTableExtraSize = ManagedSingleton<OrangeTableHelper>.Instance.GetCharacterTableExtraSize(characterTable.s_ModelExtraSize);
		RaycastHit2D raycastHit2D = Physics2D.Raycast(base.transform.position, Vector2.down, 10f, LayerMask.GetMask("Block", "SemiBlock"));
		base.transform.position = base.transform.position + Vector3.down * raycastHit2D.distance;
		GameObject playerInstance = UnityEngine.Object.Instantiate((GameObject)_loadedParts[0], base.transform.position, Quaternion.identity);
		GameObject gameObject = UnityEngine.Object.Instantiate((GameObject)_loadedParts[1], Vector3.zero, Quaternion.identity);
		GameObject gameObject2 = UnityEngine.Object.Instantiate((GameObject)_loadedParts[6], Vector3.zero, Quaternion.identity);
		GameObject gameObject3 = UnityEngine.Object.Instantiate((GameObject)_loadedParts[2], Vector3.zero, Quaternion.identity);
		GameObject gameObject4 = UnityEngine.Object.Instantiate((GameObject)_loadedParts[7], Vector3.zero, Quaternion.identity);
		GameObject gameObject5 = UnityEngine.Object.Instantiate((GameObject)_loadedParts[7], Vector3.zero, Quaternion.identity);
		GameObject gameObject6 = UnityEngine.Object.Instantiate((GameObject)_loadedParts[3], Vector3.zero, Quaternion.identity);
		GameObject gameObject7 = UnityEngine.Object.Instantiate((GameObject)_loadedParts[4], Vector3.zero, Quaternion.identity);
		GameObject gameObject8 = UnityEngine.Object.Instantiate((GameObject)_loadedParts[5], Vector3.zero, Quaternion.identity);
		GameObject gameObject9 = gameObject6;
		GameObject obj2 = UnityEngine.Object.Instantiate((GameObject)_loadfxDummy);
		if (_loadedParts[18] != null)
		{
			gameObject9 = UnityEngine.Object.Instantiate((GameObject)_loadedParts[18], Vector3.zero, Quaternion.identity);
		}
		GameObject gameObject10 = obj2;
		if (_loadedParts[19] != null)
		{
			gameObject10 = UnityEngine.Object.Instantiate((GameObject)_loadedParts[19], Vector3.zero, Quaternion.identity);
		}
		GameObject gameObject11 = obj2;
		if (_loadedParts[20] != null)
		{
			gameObject11 = UnityEngine.Object.Instantiate((GameObject)_loadedParts[20], Vector3.zero, Quaternion.identity);
		}
		GameObject gameObject12 = obj2;
		if (_loadedParts[21] != null)
		{
			gameObject12 = UnityEngine.Object.Instantiate((GameObject)_loadedParts[21], Vector3.zero, Quaternion.identity);
		}
		GameObject gameObject13 = obj2;
		if (_loadedParts[22] != null)
		{
			gameObject13 = UnityEngine.Object.Instantiate((GameObject)_loadedParts[22], Vector3.zero, Quaternion.identity);
		}
		playerInstance.SetActive(false);
		gameObject.name = "model";
		Transform transform = gameObject.transform;
		transform.SetParent(playerInstance.transform);
		transform.eulerAngles = new Vector3(0f, 90f, 0f);
		transform.localPosition = Vector3.zero;
		gameObject.AddComponent<AnimatorSoundHelper>();
		Transform[] target = playerInstance.transform.GetComponentsInChildren<Transform>(true);
		Transform parent = OrangeBattleUtility.FindChildRecursive(ref target, "Bip");
		ChargeData.ChargeFXRoot fXRoot = SetPBP.tChargeData.FXRoot;
		if (fXRoot == ChargeData.ChargeFXRoot.Model)
		{
			gameObject6.transform.SetParent(transform, false);
			gameObject7.transform.SetParent(transform, false);
			gameObject8.transform.SetParent(transform, false);
			gameObject9.transform.SetParent(transform, false);
			gameObject10.transform.SetParent(transform, false);
			gameObject11.transform.SetParent(transform, false);
			gameObject12.transform.SetParent(transform, false);
			gameObject13.transform.SetParent(transform, false);
		}
		else
		{
			gameObject6.transform.SetParent(parent, false);
			gameObject7.transform.SetParent(parent, false);
			gameObject8.transform.SetParent(parent, false);
			gameObject9.transform.SetParent(parent, false);
			gameObject10.transform.SetParent(parent, false);
			gameObject11.transform.SetParent(parent, false);
			gameObject12.transform.SetParent(parent, false);
			gameObject13.transform.SetParent(parent, false);
		}
		Transform parent2 = OrangeBattleUtility.FindChildRecursive(ref target, "Footsteps");
		gameObject2.transform.SetParent(parent2, false);
		gameObject3.transform.SetParent(gameObject.transform, false);
		Transform parent3 = OrangeBattleUtility.FindChildRecursive(ref target, "L_Thruster");
		gameObject4.transform.SetParent(parent3, false);
		Transform parent4 = OrangeBattleUtility.FindChildRecursive(ref target, "R_Thruster");
		gameObject5.transform.SetParent(parent4, false);
		InstantiateWeapons(DefaultWeaponList, ref target, true, characterTableExtraSize[0]);
		InstantiateWeapons(SetPBP.WeaponList, ref target, false, characterTableExtraSize[0]);
		OrangeCharacter player;
		CharacterControlBase component;
		if (IsJustNPC)
		{
			player = playerInstance.AddComponent(typeof(OrangeNPCCharacter)) as OrangeCharacter;
			player.CreateCharacterControl(CharacterControlFactory.GetCharacterControlType((EControlCharacter)SetPBP.CharacterID, SetPBP.CharacterSkinID));
			component = player.GetComponent<CharacterControlBase>();
			if ((bool)component)
			{
				string teleportInExtraEffect = component.GetTeleportInExtraEffect();
				if (!string.IsNullOrEmpty(teleportInExtraEffect))
				{
					MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(teleportInExtraEffect);
				}
			}
		}
		else
		{
			player = playerInstance.AddComponent(IsLocalPlayer ? typeof(OrangeConsoleCharacter) : typeof(OrangeNetCharacter)) as OrangeCharacter;
			player.CreateCharacterControl(CharacterControlFactory.GetCharacterControlType((EControlCharacter)SetPBP.CharacterID, SetPBP.CharacterSkinID));
			component = player.GetComponent<CharacterControlBase>();
			if ((bool)component)
			{
				string teleportInExtraEffect2 = component.GetTeleportInExtraEffect();
				if (!string.IsNullOrEmpty(teleportInExtraEffect2))
				{
					MonoBehaviourSingleton<FxManager>.Instance.PreloadFx(teleportInExtraEffect2);
				}
			}
		}
		player.SoundSource.Initial(IsLocalPlayer ? OrangeSSType.PLAYER : OrangeSSType.PVP1);
		player.SetupChargeComponent(gameObject6.GetComponent<ParticleSystem>(), gameObject7.GetComponent<ParticleSystem>(), gameObject8.GetComponent<ParticleSystem>(), gameObject9.GetComponent<ParticleSystem>(), SetPBP.tChargeData);
		player.SetupChargeComponent(gameObject10.GetComponent<ParticleSystem>(), gameObject11.GetComponent<ParticleSystem>(), gameObject12.GetComponent<ParticleSystem>(), gameObject13.GetComponent<ParticleSystem>(), SetPBP.tChargeData, 1);
		Animator component2 = gameObject.GetComponent<Animator>();
		AnimatorOverrideController animator = (AnimatorOverrideController)_loadedParts[8];
		string bundleName;
		string[] motionName;
		string[] uniqueBattlePose = HumanBase.GetUniqueBattlePose(characterTable.s_MODEL, out bundleName, out motionName);
		for (int j = 0; j < uniqueBattlePose.Length; j++)
		{
			AnimationClip assstSync = MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssstSync<AnimationClip>(bundleName, uniqueBattlePose[j]);
			if (assstSync == null)
			{
				switch ((HumanBase.UniqueBattlePose)j)
				{
				case HumanBase.UniqueBattlePose.dive_trigger_jump_start:
					assstSync = MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssstSync<AnimationClip>(bundleName, uniqueBattlePose[3]);
					break;
				case HumanBase.UniqueBattlePose.dive_trigger_jump_end:
					assstSync = MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssstSync<AnimationClip>(bundleName, uniqueBattlePose[4]);
					break;
				case HumanBase.UniqueBattlePose.dive_trigger_crouch_start:
					assstSync = MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssstSync<AnimationClip>(bundleName, uniqueBattlePose[3]);
					break;
				case HumanBase.UniqueBattlePose.dive_trigger_crouch_end:
					assstSync = MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssstSync<AnimationClip>(bundleName, uniqueBattlePose[4]);
					break;
				case HumanBase.UniqueBattlePose.logout2:
					assstSync = MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssstSync<AnimationClip>(bundleName, uniqueBattlePose[1]);
					break;
				}
			}
			animator[motionName[j]] = assstSync;
		}
		if ((bool)component)
		{
			string[] characterDependAnimations = component.GetCharacterDependAnimations();
			if (characterDependAnimations != null)
			{
				for (int k = 0; k < characterDependAnimations.Length; k++)
				{
					AnimationClip assstSync2 = MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssstSync<AnimationClip>(bundleName, characterDependAnimations[k]);
					animator["skillclip" + k] = assstSync2;
				}
			}
			string[] characterDependBlendAnimations = component.GetCharacterDependBlendAnimations();
			if (characterDependBlendAnimations != null)
			{
				for (int l = 0; l < characterDependBlendAnimations.Length; l++)
				{
					AnimationClip assstSync3 = MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssstSync<AnimationClip>(bundleName, characterDependBlendAnimations[l]);
					animator["blendskill" + l] = assstSync3;
				}
			}
			string[][] characterDependAnimationsBlendTree = component.GetCharacterDependAnimationsBlendTree();
			if (characterDependAnimationsBlendTree != null)
			{
				int num = 0;
				for (int m = 0; m < characterDependAnimationsBlendTree.Length; m++)
				{
					for (int n = 0; n < 3; n++)
					{
						AnimationClip assstSync4 = MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssstSync<AnimationClip>(bundleName, characterDependAnimationsBlendTree[m][n]);
						animator["bskillclip" + num] = assstSync4;
						num++;
					}
				}
			}
		}
		HumanBase.SetMotion(ref animator, characterTable.s_ANIMATOR, WeaponType.Dummy);
		HumanBase.SetMotion(ref animator, characterTable.s_ANIMATOR, (WeaponType)ManagedSingleton<OrangeDataManager>.Instance.WEAPON_TABLE_DICT[SetPBP.WeaponList[0]].n_TYPE);
		HumanBase.SetMotion(ref animator, characterTable.s_ANIMATOR, (WeaponType)ManagedSingleton<OrangeDataManager>.Instance.WEAPON_TABLE_DICT[SetPBP.WeaponList[1]].n_TYPE);
		int[] obj3 = new int[2] { characterTable.n_INIT_WEAPON1, characterTable.n_INIT_WEAPON2 };
		player.vLastMovePt = player.transform.position;
		if (characterTable.s_ANIMATOR.Contains("classic"))
		{
			player.CharacterDashType = OrangeCharacter.DASH_TYPE.CLASSIC;
		}
		int[] array = obj3;
		foreach (int num3 in array)
		{
			if (num3 != -1)
			{
				WEAPON_TABLE wEAPON_TABLE = ManagedSingleton<OrangeDataManager>.Instance.WEAPON_TABLE_DICT[num3];
				HumanBase.SetMotion(ref animator, characterTable.s_ANIMATOR, (WeaponType)wEAPON_TABLE.n_TYPE);
			}
		}
		if ((bool)component)
		{
			int uniqueWeaponType = component.GetUniqueWeaponType();
			if (uniqueWeaponType > 0)
			{
				HumanBase.SetMotion(ref animator, characterTable.s_ANIMATOR, (WeaponType)uniqueWeaponType);
			}
			HumanBase.SetUniqueWeaponMotion(ref animator, characterTable.s_ANIMATOR, (WeaponType)ManagedSingleton<OrangeDataManager>.Instance.WEAPON_TABLE_DICT[SetPBP.WeaponList[0]].n_TYPE, component.GetUniqueWeaponMotion);
			HumanBase.SetUniqueWeaponMotion(ref animator, characterTable.s_ANIMATOR, (WeaponType)ManagedSingleton<OrangeDataManager>.Instance.WEAPON_TABLE_DICT[SetPBP.WeaponList[1]].n_TYPE, component.GetUniqueWeaponMotion);
			HumanBase.SetUniqueMotion(ref animator, bundleName, component.GetUniqueMotion);
		}
		component2.runtimeAnimatorController = animator;
		player.WallKickSparkFx = "OBJ_WALLKICK_SPARK";
		player.LandFx = "OBJ_LAND";
		player.JumpUpFx = "OBJ_JUMP_UP";
		player.JumpLeftFx = "OBJ_JUMP_LEFT";
		player.JumpRightFx = "OBJ_JUMP_RIGHT";
		player.DashSmokeFx = "OBJ_DASH_SMOKE";
		player.TeleportInFx = _playTeleportInFx;
		player.TeleportOutFx = _playTeleportOutFx;
		player.SpeedLineParticleSystem = gameObject3.GetComponent<ParticleSystem>();
		player.DustParticleSystem = gameObject2.GetComponent<ParticleSystem>();
		player.LThrusterParticleSystem = gameObject4.GetComponentInChildren<ParticleSystem>();
		player.RThrusterParticleSystem = gameObject5.GetComponentInChildren<ParticleSystem>();
		player.PlayerFSkills = new WeaponStruct[2];
		player.PlayerFSkillsID = new int[2];
		for (int num4 = 0; num4 < 2; num4++)
		{
			player.PlayerFSkills[num4] = new WeaponStruct();
			if ((ManagedSingleton<StageHelper>.Instance.nLastStageRuleID_Status <= 0 || ManagedSingleton<StageHelper>.Instance.StageRuleFS(num4)) && SetPBP.FSkillList[num4] != null && SetPBP.FSkillList[num4].netFinalStrikeInfo.Level > 0)
			{
				player.PlayerFSkills[num4].Icon = _loadedFSkillIcons[num4];
				player.PlayerFSkills[num4].SkillLV = SetPBP.FSkillList[num4].netFinalStrikeInfo.Level;
				player.PlayerFSkills[num4].MagazineRemainMax = 1f;
				player.PlayerFSkills[num4].BulletData = ManagedSingleton<OrangeTableHelper>.Instance.getFS_SkillTable(SetPBP.FSkillList[num4].netFinalStrikeInfo.FinalStrikeID, SetPBP.FSkillList[num4].netFinalStrikeInfo.Level, SetPBP.FSkillList[num4].netFinalStrikeInfo.Star);
				player.PlayerFSkillsID[num4] = SetPBP.FSkillList[num4].netFinalStrikeInfo.FinalStrikeID;
			}
		}
		player.PlayerSkills = new WeaponStruct[2];
		for (int num5 = 0; num5 < 2; num5++)
		{
			player.PlayerSkills[num5] = new WeaponStruct();
			player.PlayerSkills[num5].Icon = _loadedSkillIcons[num5];
			player.PlayerSkills[num5].SkillLV = SetPBP.SkillLv[num5];
			player.PlayerSkills[num5].EnhanceEXIndex = SetPBP.EnhanceEXIndex[num5];
			if (ManagedSingleton<StageHelper>.Instance.nLastStageRuleID_Status > 0)
			{
				player.PlayerSkills[num5].SkillLV = ManagedSingleton<StageHelper>.Instance.StatusCorrection(player.PlayerSkills[num5].SkillLV, StageHelper.STAGE_RULE_STATUS.SKILL_LV);
			}
			SKILL_TABLE sKILL_TABLE = ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT[SkillList[num5]];
			if (sKILL_TABLE.n_USE_DEFAULT_WEAPON != -1 && DefaultWeaponList[sKILL_TABLE.n_USE_DEFAULT_WEAPON] != -1)
			{
				player.PlayerSkills[num5].WeaponData = ManagedSingleton<OrangeDataManager>.Instance.WEAPON_TABLE_DICT[DefaultWeaponList[sKILL_TABLE.n_USE_DEFAULT_WEAPON]];
			}
		}
		player.PlayerWeapons = new WeaponStruct[2];
		for (int num6 = 0; num6 < 2; num6++)
		{
			player.PlayerWeapons[num6] = new WeaponStruct();
			player.PlayerWeapons[num6].WeaponData = ManagedSingleton<OrangeDataManager>.Instance.WEAPON_TABLE_DICT[SetPBP.WeaponList[num6]];
			player.PlayerWeapons[num6].Icon = _loadedWeaponIcons[num6];
		}
		if (SetPBP.chipStatus == null)
		{
			SetPBP.chipStatus = ManagedSingleton<StatusHelper>.Instance.GetAllChipStatus();
		}
		if (SetPBP.mainWStatus == null)
		{
			SetPBP.mainWStatus = ManagedSingleton<StatusHelper>.Instance.GetWeaponStatus(SetPBP.WeaponList[0]);
		}
		if (SetPBP.subWStatus == null)
		{
			SetPBP.subWStatus = ManagedSingleton<StatusHelper>.Instance.GetWeaponStatus(SetPBP.WeaponList[1]);
		}
		if (SetPBP.tPlayerStatus == null)
		{
			SetPBP.tPlayerStatus = ManagedSingleton<StatusHelper>.Instance.GetPlayerStatusWithEquip();
		}
		if (SetPBP.sPlayerID == "" && ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo != null)
		{
			SetPBP.sPlayerID = MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify;
		}
		if (SetPBP.sPlayerName == "" && ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo != null)
		{
			SetPBP.sPlayerName = ManagedSingleton<PlayerNetManager>.Instance.playerInfo.netPlayerInfo.Nickname;
		}
		if (SetPBP.tRefPassiveskill == null)
		{
			SetPBP.tRefPassiveskill = new RefPassiveskill();
			WeaponInfo value = null;
			if (ManagedSingleton<PlayerNetManager>.Instance.dicWeapon.TryGetValue(SetPBP.WeaponList[0], out value))
			{
				if (value.netSkillInfos != null)
				{
					for (int num7 = 0; num7 < value.netSkillInfos.Count; num7++)
					{
						int num8 = value.netSkillInfos[num7].Level;
						if (ManagedSingleton<StageHelper>.Instance.nLastStageRuleID_Status > 0)
						{
							num8 = ManagedSingleton<StageHelper>.Instance.StatusCorrection(num8, StageHelper.STAGE_RULE_STATUS.SKILL_LV);
						}
						SetPBP.tRefPassiveskill.AddPassivesSkill(value.netSkillInfos[num7], 4, num8);
					}
				}
				if (value.netDiveSkillInfo != null)
				{
					SetPBP.tRefPassiveskill.AddPassivesSkill(value.netDiveSkillInfo.SkillID, 4);
				}
				ChipInfo value2 = null;
				if (!IsJustNPC && ManagedSingleton<PlayerNetManager>.Instance.dicChip.TryGetValue(value.netInfo.Chip, out value2))
				{
					SetPBP.tRefPassiveskill.AddPassivesSkill(value2.netChipInfo, 4, 1, true);
					if (!SetPBP.tRefPassiveskill.tMainPassiveskillStatus.tSKILL_TABLE.s_USE_MOTION.Equals("null"))
					{
						MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad("prefab/fx/chipeffect/" + SetPBP.tRefPassiveskill.tMainPassiveskillStatus.tSKILL_TABLE.s_USE_MOTION, SetPBP.tRefPassiveskill.tMainPassiveskillStatus.tSKILL_TABLE.s_USE_MOTION, delegate(GameObject obj)
						{
							GameObject gameObject17 = UnityEngine.Object.Instantiate(obj);
							player.PlayerWeapons[0].ChipEfx = gameObject17.GetComponent<ChipSystem>();
							Transform parent8 = OrangeBattleUtility.FindChildRecursive(player.transform, "model");
							player.PlayerWeapons[0].ChipEfx.transform.SetParent(parent8, false);
							player.PlayerWeapons[0].chip_switch = true;
							if (SetPBP.CharacterID == 60)
							{
								player.PlayerWeapons[0].ChipEfx.SetActivePosition(new Vector3(0f, 0.35f, 0f));
							}
							else
							{
								player.PlayerWeapons[0].ChipEfx.SetActivePosition(new Vector3(0f, 1f, 0f));
							}
						});
					}
				}
			}
			if (ManagedSingleton<PlayerNetManager>.Instance.dicWeapon.TryGetValue(SetPBP.WeaponList[1], out value))
			{
				if (value.netSkillInfos != null)
				{
					for (int num9 = 0; num9 < value.netSkillInfos.Count; num9++)
					{
						int num10 = value.netSkillInfos[num9].Level;
						if (ManagedSingleton<StageHelper>.Instance.nLastStageRuleID_Status > 0)
						{
							num10 = ManagedSingleton<StageHelper>.Instance.StatusCorrection(num10, StageHelper.STAGE_RULE_STATUS.SKILL_LV);
						}
						SetPBP.tRefPassiveskill.AddPassivesSkill(value.netSkillInfos[num9], 8, num10);
					}
				}
				if (value.netDiveSkillInfo != null)
				{
					SetPBP.tRefPassiveskill.AddPassivesSkill(value.netDiveSkillInfo.SkillID, 8);
				}
				ChipInfo value3 = null;
				if (!IsJustNPC && ManagedSingleton<PlayerNetManager>.Instance.dicChip.TryGetValue(value.netInfo.Chip, out value3))
				{
					SetPBP.tRefPassiveskill.AddPassivesSkill(value3.netChipInfo, 8, 1, true);
					if (!SetPBP.tRefPassiveskill.tSubPassiveskillStatus.tSKILL_TABLE.s_USE_MOTION.Equals("null"))
					{
						MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad("prefab/fx/chipeffect/" + SetPBP.tRefPassiveskill.tSubPassiveskillStatus.tSKILL_TABLE.s_USE_MOTION, SetPBP.tRefPassiveskill.tSubPassiveskillStatus.tSKILL_TABLE.s_USE_MOTION, delegate(GameObject obj)
						{
							GameObject gameObject16 = UnityEngine.Object.Instantiate(obj);
							player.PlayerWeapons[1].ChipEfx = gameObject16.GetComponent<ChipSystem>();
							Transform parent7 = OrangeBattleUtility.FindChildRecursive(player.transform, "model");
							player.PlayerWeapons[1].ChipEfx.transform.SetParent(parent7, false);
							player.PlayerWeapons[1].chip_switch = true;
							if (SetPBP.CharacterID == 60)
							{
								player.PlayerWeapons[1].ChipEfx.SetActivePosition(new Vector3(0f, 0.35f, 0f));
							}
							else
							{
								player.PlayerWeapons[1].ChipEfx.SetActivePosition(new Vector3(0f, 1f, 0f));
							}
						});
					}
				}
			}
			CharacterInfo value4 = null;
			if (ManagedSingleton<PlayerNetManager>.Instance.dicCharacter.TryGetValue(SetPBP.CharacterID, out value4))
			{
				Dictionary<CharacterSkillSlot, NetCharacterSkillInfo>.Enumerator enumerator = value4.netSkillDic.GetEnumerator();
				while (enumerator.MoveNext())
				{
					SetPBP.tRefPassiveskill.AddPassivesSkill(enumerator.Current.Value);
				}
				CHARACTER_TABLE cHARACTER_TABLE = ManagedSingleton<OrangeDataManager>.Instance.CHARACTER_TABLE_DICT[value4.netInfo.CharacterID];
				SetPBP.tRefPassiveskill.AddPassivesSkill(cHARACTER_TABLE.n_INITIAL_SKILL1);
				SetPBP.tRefPassiveskill.AddPassivesSkill(cHARACTER_TABLE.n_INITIAL_SKILL2);
				SetPBP.tRefPassiveskill.AddPassivesSkill(cHARACTER_TABLE.n_INITIAL_SKILL3);
				List<int> charactertCardSkillList = MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.GetCharactertCardSkillList(value4.netInfo.CharacterID);
				if (charactertCardSkillList != null)
				{
					for (int num11 = 0; num11 < charactertCardSkillList.Count; num11++)
					{
						SetPBP.tRefPassiveskill.AddPassivesSkill(charactertCardSkillList[num11]);
					}
				}
				List<int> characterDNASkillIDList = ManagedSingleton<PlayerNetManager>.Instance.GetCharacterDNASkillIDList(value4.netInfo.CharacterID);
				for (int num12 = 0; num12 < characterDNASkillIDList.Count; num12++)
				{
					SetPBP.tRefPassiveskill.AddPassivesSkill(characterDNASkillIDList[num12]);
				}
			}
			Dictionary<int, SUIT_TABLE>.Enumerator enumerator2 = ManagedSingleton<OrangeDataManager>.Instance.SUIT_TABLE_DICT.GetEnumerator();
			KeyValuePair<int, EquipInfo>[] array2 = ManagedSingleton<PlayerNetManager>.Instance.dicEquip.Where((KeyValuePair<int, EquipInfo> obj) => obj.Value.netEquipmentInfo.Equip != 0).ToArray();
			while (enumerator2.MoveNext())
			{
				int num13 = 0;
				int[] array3 = new int[6]
				{
					enumerator2.Current.Value.n_EQUIP_1,
					enumerator2.Current.Value.n_EQUIP_2,
					enumerator2.Current.Value.n_EQUIP_3,
					enumerator2.Current.Value.n_EQUIP_4,
					enumerator2.Current.Value.n_EQUIP_5,
					enumerator2.Current.Value.n_EQUIP_6
				};
				if (ManagedSingleton<StageHelper>.Instance.nLastStageRuleID_Status > 0)
				{
					break;
				}
				for (int num14 = 0; num14 < array2.Length; num14++)
				{
					EquipInfo value5 = array2[num14].Value;
					array = array3;
					for (int num2 = 0; num2 < array.Length; num2++)
					{
						if (array[num2] == value5.netEquipmentInfo.EquipItemID)
						{
							num13++;
						}
					}
				}
				if (num13 >= enumerator2.Current.Value.n_SUIT_1)
				{
					SetPBP.tRefPassiveskill.AddPassivesSkill(enumerator2.Current.Value.n_EFFECT_1, 64);
				}
				if (num13 >= enumerator2.Current.Value.n_SUIT_2)
				{
					SetPBP.tRefPassiveskill.AddPassivesSkill(enumerator2.Current.Value.n_EFFECT_2, 64);
				}
				if (num13 >= enumerator2.Current.Value.n_SUIT_3)
				{
					SetPBP.tRefPassiveskill.AddPassivesSkill(enumerator2.Current.Value.n_EFFECT_3, 64);
				}
			}
			if (ManagedSingleton<StageHelper>.Instance.nLastStageRuleID_Status > 0)
			{
				STAGE_RULE_TABLE value6 = null;
				if (ManagedSingleton<OrangeDataManager>.Instance.STAGE_RULE_TABLE_DICT.TryGetValue(ManagedSingleton<StageHelper>.Instance.nLastStageRuleID_Status, out value6))
				{
					SetPBP.tRefPassiveskill.AddPassivesSkill(value6.n_PASSIVE_SKILL1);
					SetPBP.tRefPassiveskill.AddPassivesSkill(value6.n_PASSIVE_SKILL2);
					SetPBP.tRefPassiveskill.AddPassivesSkill(value6.n_PASSIVE_SKILL3);
				}
			}
			SetPBP.tRefPassiveskill.ReCalcuPassiveskillSelf();
			SetPBP.tRefPassiveskill.PreloadAllPassiveskill(LoadBulletBySkillTable, LoadBuffSound);
		}
		else
		{
			if (SetPBP.tRefPassiveskill.tMainPassiveskillStatus != null && !SetPBP.tRefPassiveskill.tMainPassiveskillStatus.tSKILL_TABLE.s_USE_MOTION.Equals("null"))
			{
				MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad("prefab/fx/chipeffect/" + SetPBP.tRefPassiveskill.tMainPassiveskillStatus.tSKILL_TABLE.s_USE_MOTION, SetPBP.tRefPassiveskill.tMainPassiveskillStatus.tSKILL_TABLE.s_USE_MOTION, delegate(GameObject obj)
				{
					GameObject gameObject15 = UnityEngine.Object.Instantiate(obj);
					player.PlayerWeapons[0].ChipEfx = gameObject15.GetComponent<ChipSystem>();
					Transform parent6 = OrangeBattleUtility.FindChildRecursive(player.transform, "model");
					player.PlayerWeapons[0].ChipEfx.transform.SetParent(parent6, false);
					player.PlayerWeapons[0].chip_switch = true;
					if (SetPBP.CharacterID == 60)
					{
						player.PlayerWeapons[0].ChipEfx.SetActivePosition(new Vector3(0f, 0.35f, 0f));
					}
					else
					{
						player.PlayerWeapons[0].ChipEfx.SetActivePosition(new Vector3(0f, 1f, 0f));
					}
				});
			}
			if (SetPBP.tRefPassiveskill.tSubPassiveskillStatus != null && !SetPBP.tRefPassiveskill.tSubPassiveskillStatus.tSKILL_TABLE.s_USE_MOTION.Equals("null"))
			{
				MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad("prefab/fx/chipeffect/" + SetPBP.tRefPassiveskill.tSubPassiveskillStatus.tSKILL_TABLE.s_USE_MOTION, SetPBP.tRefPassiveskill.tSubPassiveskillStatus.tSKILL_TABLE.s_USE_MOTION, delegate(GameObject obj)
				{
					GameObject gameObject14 = UnityEngine.Object.Instantiate(obj);
					player.PlayerWeapons[1].ChipEfx = gameObject14.GetComponent<ChipSystem>();
					Transform parent5 = OrangeBattleUtility.FindChildRecursive(player.transform, "model");
					player.PlayerWeapons[1].ChipEfx.transform.SetParent(parent5, false);
					player.PlayerWeapons[1].chip_switch = true;
					if (SetPBP.CharacterID == 60)
					{
						player.PlayerWeapons[1].ChipEfx.SetActivePosition(new Vector3(0f, 0.35f, 0f));
					}
					else
					{
						player.PlayerWeapons[1].ChipEfx.SetActivePosition(new Vector3(0f, 1f, 0f));
					}
				});
			}
			SetPBP.tRefPassiveskill.PreloadAllPassiveskill(LoadBulletBySkillTable, LoadBuffSound);
			if (ManagedSingleton<StageHelper>.Instance.nLastStageRuleID_Status > 0)
			{
				STAGE_RULE_TABLE value7 = null;
				if (ManagedSingleton<OrangeDataManager>.Instance.STAGE_RULE_TABLE_DICT.TryGetValue(ManagedSingleton<StageHelper>.Instance.nLastStageRuleID_Status, out value7))
				{
					SetPBP.tRefPassiveskill.AddPassivesSkill(value7.n_PASSIVE_SKILL1);
					SetPBP.tRefPassiveskill.AddPassivesSkill(value7.n_PASSIVE_SKILL2);
					SetPBP.tRefPassiveskill.AddPassivesSkill(value7.n_PASSIVE_SKILL3);
				}
			}
		}
		STAGE_TABLE nowStageTable = BattleInfoUI.Instance.NowStageTable;
		AddPassiveSkillOfCrusade(nowStageTable);
		AddPassiveSkillOfPowerPillarBuff(player, nowStageTable);
		WeaponStatus weaponStatus = new WeaponStatus
		{
			nATK = (int)SetPBP.mainWStatus.nATK + (int)SetPBP.subWStatus.nATK + (int)SetPBP.chipStatus.nATK + (int)SetPBP.tPlayerStatus.nATK,
			nHP = (int)SetPBP.mainWStatus.nHP + (int)SetPBP.subWStatus.nHP
		};
		if (ManagedSingleton<StageHelper>.Instance.nLastStageRuleID_Status > 0)
		{
			weaponStatus.nATK = ManagedSingleton<StageHelper>.Instance.StatusCorrection(weaponStatus.nATK, StageHelper.STAGE_RULE_STATUS.ATK);
		}
		STAGE_TABLE value8;
		if (IsLocalPlayer && ManagedSingleton<OrangeDataManager>.Instance.STAGE_TABLE_DICT.TryGetValue(ManagedSingleton<StageHelper>.Instance.nLastStageID, out value8))
		{
			switch (value8.n_TYPE)
			{
			case 9:
				weaponStatus.nATK = (int)((float)(int)weaponStatus.nATK * ((float)ManagedSingleton<PlayerHelper>.Instance.GetRaidBossBounes() * 0.0001f + 1f));
				break;
			case 10:
				weaponStatus.nATK = (int)((float)(int)weaponStatus.nATK * ((float)Singleton<CrusadeSystem>.Instance.GetCrusadeBonus() * 0.0001f + 1f));
				break;
			}
		}
		weaponStatus.nATK = MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.PowerCorrection(weaponStatus.nATK);
		weaponStatus.nHP = MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.PowerCorrection(weaponStatus.nHP);
		player.PlayerWeapons[0].weaponStatus = new WeaponStatus();
		player.PlayerWeapons[1].weaponStatus = new WeaponStatus();
		player.PlayerWeapons[0].weaponStatus.CopyWeaponStatus(SetPBP.mainWStatus, 4, SetPBP.mainWStatus.nWeaponType);
		player.PlayerWeapons[1].weaponStatus.CopyWeaponStatus(SetPBP.subWStatus, 8, SetPBP.subWStatus.nWeaponType);
		player.PlayerWeapons[0].weaponStatus.nATK = weaponStatus.nATK;
		player.PlayerWeapons[1].weaponStatus.nATK = weaponStatus.nATK;
		player.PlayerWeapons[0].weaponStatus.nHP = weaponStatus.nHP;
		player.PlayerWeapons[1].weaponStatus.nHP = weaponStatus.nHP;
		if (ManagedSingleton<StageHelper>.Instance.nLastStageRuleID_Status > 0)
		{
			player.PlayerWeapons[0].weaponStatus.nHIT = ManagedSingleton<StageHelper>.Instance.StatusCorrection(player.PlayerWeapons[0].weaponStatus.nHIT, StageHelper.STAGE_RULE_STATUS.HIT);
			player.PlayerWeapons[1].weaponStatus.nHIT = ManagedSingleton<StageHelper>.Instance.StatusCorrection(player.PlayerWeapons[1].weaponStatus.nHIT, StageHelper.STAGE_RULE_STATUS.HIT);
			player.PlayerWeapons[0].weaponStatus.nReduceCriPercent = ManagedSingleton<StageHelper>.Instance.StatusCorrection(player.PlayerWeapons[0].weaponStatus.nReduceCriPercent, StageHelper.STAGE_RULE_STATUS.CRIDMG_RESIST);
			player.PlayerWeapons[1].weaponStatus.nReduceCriPercent = ManagedSingleton<StageHelper>.Instance.StatusCorrection(player.PlayerWeapons[1].weaponStatus.nReduceCriPercent, StageHelper.STAGE_RULE_STATUS.CRIDMG_RESIST);
			player.PlayerWeapons[0].weaponStatus.nReduceBlockPercent = ManagedSingleton<StageHelper>.Instance.StatusCorrection(player.PlayerWeapons[0].weaponStatus.nReduceBlockPercent, StageHelper.STAGE_RULE_STATUS.PARRY_RESIST);
			player.PlayerWeapons[1].weaponStatus.nReduceBlockPercent = ManagedSingleton<StageHelper>.Instance.StatusCorrection(player.PlayerWeapons[1].weaponStatus.nReduceBlockPercent, StageHelper.STAGE_RULE_STATUS.PARRY_RESIST);
		}
		WeaponStatus weaponStatus2 = new WeaponStatus();
		WeaponStatus weaponStatus3 = new WeaponStatus();
		weaponStatus2.CopyWeaponStatus(player.PlayerWeapons[0].weaponStatus, 1);
		weaponStatus3.CopyWeaponStatus(player.PlayerWeapons[0].weaponStatus, 2);
		player.PlayerSkills[0].weaponStatus = weaponStatus2;
		player.PlayerSkills[1].weaponStatus = weaponStatus3;
		WeaponStatus weaponStatus4 = new WeaponStatus();
		WeaponStatus weaponStatus5 = new WeaponStatus();
		if (player.PlayerFSkills[0] != null)
		{
			weaponStatus4.CopyWeaponStatus(player.PlayerWeapons[0].weaponStatus, 16);
			player.PlayerFSkills[0].weaponStatus = weaponStatus4;
		}
		if (player.PlayerFSkills[1] != null)
		{
			weaponStatus5.CopyWeaponStatus(player.PlayerWeapons[0].weaponStatus, 32);
			player.PlayerFSkills[1].weaponStatus = weaponStatus5;
		}
		player.SetPBP = SetPBP;
		player.sPlayerName = SetPBP.sPlayerName;
		player.sPlayerID = SetPBP.sPlayerID;
		player.tRefPassiveskill = SetPBP.tRefPassiveskill;
		if (ManagedSingleton<StageHelper>.Instance.nLastStageRuleID_Status > 0)
		{
			int pow = (int)SetPBP.mainWStatus.nHP + (int)SetPBP.subWStatus.nHP + (int)SetPBP.tPlayerStatus.nHP + (int)SetPBP.chipStatus.nHP;
			pow = ManagedSingleton<StageHelper>.Instance.StatusCorrection(pow, StageHelper.STAGE_RULE_STATUS.HP);
			float ratioStatus = SetPBP.tRefPassiveskill.GetRatioStatus(1, 12);
			int addStatus = SetPBP.tRefPassiveskill.GetAddStatus(1, 12);
			player.Hp = Mathf.FloorToInt((float)pow * ratioStatus) + addStatus;
		}
		else
		{
			int num15 = (int)SetPBP.mainWStatus.nHP + (int)SetPBP.subWStatus.nHP + (int)SetPBP.tPlayerStatus.nHP + (int)SetPBP.chipStatus.nHP;
			float ratioStatus2 = SetPBP.tRefPassiveskill.GetRatioStatus(1, 12);
			int addStatus2 = SetPBP.tRefPassiveskill.GetAddStatus(1, 12);
			player.Hp = Mathf.FloorToInt((float)num15 * ratioStatus2) + addStatus2;
		}
		if (MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.IsPvp)
		{
			player.Hp = (int)player.Hp * (100 + OrangeConst.PVP_HP_MODIFY);
			player.Hp = (int)player.Hp / 100;
		}
		player.MaxHp = player.Hp;
		if (IsJustNPC)
		{
			IsLocalPlayer = false;
			OrangeBattleUtility.ChangeLayersRecursively(gameObject.transform, ManagedSingleton<OrangeLayerManager>.Instance.RenderPlayer, false);
		}
		if (IsLocalPlayer)
		{
			ManagedSingleton<StageHelper>.Instance.nLastOCPower = ManagedSingleton<PlayerHelper>.Instance.GetBattlePower(SetPBP.mainWStatus, SetPBP.subWStatus, SetPBP.tPlayerStatus + SetPBP.chipStatus);
			CameraControl component3 = MonoBehaviourSingleton<OrangeSceneManager>.Instance.MainCamera.GetComponent<CameraControl>();
			component3.Target = playerInstance.GetComponent<Controller2D>();
			component3.Init();
			Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.LOCAL_PLAYER_SPWAN, player);
			OrangeBattleUtility.ChangeLayersRecursively(gameObject.transform, ManagedSingleton<OrangeLayerManager>.Instance.RenderPlayer, false);
		}
		else
		{
			if (!IsJustNPC)
			{
				IsWaitNetCtrl = true;
			}
			player.bNeedUpdateAlways = true;
		}
		OrangeBattleUtility.ChangeLayersRecursively(gameObject6.transform, ManagedSingleton<OrangeLayerManager>.Instance.FxLayer);
		OrangeBattleUtility.ChangeLayersRecursively(gameObject7.transform, ManagedSingleton<OrangeLayerManager>.Instance.FxLayer);
		OrangeBattleUtility.ChangeLayersRecursively(gameObject8.transform, ManagedSingleton<OrangeLayerManager>.Instance.FxLayer);
		Setting setting = ((!IsLocalPlayer) ? new Setting(true) : MonoBehaviourSingleton<OrangePlayerLocalData>.Instance.SaveData.Setting);
		if (SetPBP.netControllerSetting != null)
		{
			setting.JumpClassic = SetPBP.netControllerSetting.JumpClassic;
			setting.AutoCharge = SetPBP.netControllerSetting.AutoCharge;
			setting.AutoAim = SetPBP.netControllerSetting.AutoAim;
			setting.AimFirst = SetPBP.netControllerSetting.AimFirst;
			setting.SlashClassic = SetPBP.netControllerSetting.SlashClassic;
			setting.ShootClassic = SetPBP.netControllerSetting.ShootClassic;
			setting.AimManual = SetPBP.netControllerSetting.ManualAim;
			setting.AimLine = SetPBP.netControllerSetting.AimLine;
			setting.DoubleTapDash = SetPBP.netControllerSetting.DClickDash;
		}
		player.PlayerSetting = setting;
		Vector2 size = new Vector2(player._normalHitboxSize.x, characterTableExtraSize[1]);
		player._normalHitboxSize = new Vector2(size.x, size.y);
		player._halfHitboxSize = new Vector2(size.x, size.y / 2f);
		player._normalHitboxOffset = new Vector2(0f, size.y / 2f);
		player._halfHitboxOffset = new Vector2(0f, size.y / 4f);
		AnimatorBase component4 = player.GetComponent<AnimatorBase>();
		component4._modelShift.y = characterTableExtraSize[2];
		component4._defaultModelshiftY = characterTableExtraSize[2];
		BoxCollider2D component5 = playerInstance.GetComponent<BoxCollider2D>();
		if ((bool)component5)
		{
			component5.size = size;
		}
		player.CharacterID = SetPBP.CharacterID;
		player.UserID = (IsLocalPlayer ? MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify : uid);
		player._characterDirection = SetPBP.tSetCharacterDir;
		Debug.Log("Build player success");
		if (!IsJustNPC)
		{
			IsReadyGoEnd = false;
			MonoBehaviourSingleton<StageSyncManager>.Instance.LoadPlayerEnd(player.sPlayerID);
			while (!IsReadyGoEnd)
			{
				yield return CoroutineDefine._waitForEndOfFrame;
			}
		}
		float fWaitNetTimeOut = 10f;
		while (IsWaitNetCtrl)
		{
			yield return CoroutineDefine._waitForEndOfFrame;
			fWaitNetTimeOut -= Time.deltaTime;
			if (fWaitNetTimeOut <= 0f)
			{
				IsWaitNetCtrl = false;
				player.EventLockInputingNet = true;
				if (StageUpdate.bIsHost)
				{
					StageUpdate.SyncStageObj(3, 11, player.sPlayerID);
				}
			}
		}
		if (IsLocalPlayer)
		{
			StageUpdate.SyncStageObj(3, 17, player.sPlayerID, true, true);
		}
		if (bNeedLockInput)
		{
			player.EventLockInputingNet = true;
		}
		if (bShowStartEffect)
		{
			FxBase teleportParticleSystem = MonoBehaviourSingleton<FxManager>.Instance.PlayReturn<FxBase>(_playTeleportInFx, base.transform.position, Quaternion.identity, Array.Empty<object>());
			player.SoundSource.UpdateDistanceCall();
			player.SoundSource.ForcePlaySE(AudioLib.GetCharaSE(ref characterTable), 8);
			while (teleportParticleSystem.pPS.time < 0.3f && !teleportParticleSystem.IsEnd)
			{
				yield return CoroutineDefine._waitForEndOfFrame;
			}
		}
		ShadowBuilder.CreateShadow(player.transform);
		pCallBack.CheckTargetToInvoke(player);
		playerInstance.SetActive(true);
		UnityEngine.Object.Destroy(base.gameObject);
	}

	private void AddPassiveSkillOfCrusade(STAGE_TABLE stageTable)
	{
		if (!IsLocalPlayer)
		{
			return;
		}
		int n_TYPE = stageTable.n_TYPE;
		if (n_TYPE == 10)
		{
			int passiveSkillID = Singleton<CrusadeSystem>.Instance.PassiveSkillID;
			if (StageUpdate.StageMode == StageMode.Contribute && passiveSkillID > 0)
			{
				SetPBP.tRefPassiveskill.AddPassivesSkill(passiveSkillID, 64);
			}
		}
	}

	private void AddPassiveSkillOfPowerPillarBuff(OrangeCharacter player, STAGE_TABLE stageTable)
	{
		if (!IsLocalPlayer)
		{
			return;
		}
		foreach (PowerPillarInfoData effectivePowerPillarInfoData in Singleton<PowerTowerSystem>.Instance.GetEffectivePowerPillarInfoDataList(stageTable))
		{
			foreach (SKILL_TABLE skillAttrData in effectivePowerPillarInfoData.OreInfo.SkillAttrDataList)
			{
				int n_ID = skillAttrData.n_ID;
				switch (skillAttrData.n_EFFECT)
				{
				case 101:
				{
					WeaponStruct[] playerFSkills = player.PlayerFSkills;
					for (int i = 0; i < playerFSkills.Length; i++)
					{
						playerFSkills[i].MagazineRemainMax += skillAttrData.f_EFFECT_X;
					}
					break;
				}
				default:
					SetPBP.tRefPassiveskill.AddPassivesSkill(n_ID, 64);
					break;
				}
			}
		}
	}

	protected void LoadChargeParts(StageUpdate.LoadCallBackObj tNewLoad, ChargeData tChargeData)
	{
		tNewLoad = GetNewLoadCbObj();
		tNewLoad.lcb = delegate(StageUpdate.LoadCallBackObj tLCB, UnityEngine.Object obj)
		{
			_loadedParts[3] = obj;
		};
		MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad<UnityEngine.Object>(tChargeData.sABDlPath, tChargeData.sChargeLV1FX, tNewLoad.LoadCB);
		tNewLoad = GetNewLoadCbObj();
		tNewLoad.lcb = delegate(StageUpdate.LoadCallBackObj tLCB, UnityEngine.Object obj)
		{
			_loadedParts[4] = obj;
		};
		MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad<UnityEngine.Object>(tChargeData.sABDlPath, tChargeData.sChargeLV2FX, tNewLoad.LoadCB);
		if (tChargeData.sChargeLV3FX == "")
		{
			tChargeData.sChargeLV3FX = tChargeData.sChargeLV2FX;
		}
		tNewLoad = GetNewLoadCbObj();
		tNewLoad.lcb = delegate(StageUpdate.LoadCallBackObj tLCB, UnityEngine.Object obj)
		{
			_loadedParts[5] = obj;
		};
		MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad<UnityEngine.Object>(tChargeData.sABDlPath, tChargeData.sChargeLV3FX, tNewLoad.LoadCB);
		if (tChargeData.sChargeStartFX != "")
		{
			tNewLoad = GetNewLoadCbObj();
			tNewLoad.lcb = delegate(StageUpdate.LoadCallBackObj tLCB, UnityEngine.Object obj)
			{
				_loadedParts[18] = obj;
			};
			MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad<UnityEngine.Object>(tChargeData.sABDlPath, tChargeData.sChargeStartFX, tNewLoad.LoadCB);
		}
		if (tChargeData.sChargeLV1FX2 != "")
		{
			tNewLoad = GetNewLoadCbObj();
			tNewLoad.lcb = delegate(StageUpdate.LoadCallBackObj tLCB, UnityEngine.Object obj)
			{
				_loadedParts[19] = obj;
			};
			MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad<UnityEngine.Object>(tChargeData.sABDlPath, tChargeData.sChargeLV1FX2, tNewLoad.LoadCB);
		}
		if (tChargeData.sChargeLV2FX2 != "")
		{
			tNewLoad = GetNewLoadCbObj();
			tNewLoad.lcb = delegate(StageUpdate.LoadCallBackObj tLCB, UnityEngine.Object obj)
			{
				_loadedParts[20] = obj;
			};
			MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad<UnityEngine.Object>(tChargeData.sABDlPath, tChargeData.sChargeLV2FX2, tNewLoad.LoadCB);
		}
		if (tChargeData.sChargeLV3FX2 != "")
		{
			tNewLoad = GetNewLoadCbObj();
			tNewLoad.lcb = delegate(StageUpdate.LoadCallBackObj tLCB, UnityEngine.Object obj)
			{
				_loadedParts[21] = obj;
			};
			MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad<UnityEngine.Object>(tChargeData.sABDlPath, tChargeData.sChargeLV3FX2, tNewLoad.LoadCB);
		}
		if (tChargeData.sChargeStartFX2 != "")
		{
			tNewLoad = GetNewLoadCbObj();
			tNewLoad.lcb = delegate(StageUpdate.LoadCallBackObj tLCB, UnityEngine.Object obj)
			{
				_loadedParts[22] = obj;
			};
			MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad<UnityEngine.Object>(tChargeData.sABDlPath, tChargeData.sChargeStartFX2, tNewLoad.LoadCB);
		}
	}
}
