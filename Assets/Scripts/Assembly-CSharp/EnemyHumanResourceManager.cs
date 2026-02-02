using System;
using System.Collections;
using System.Collections.Generic;
using Better;
using StageLib;
using UnityEngine;
using enums;

public class EnemyHumanResourceManager : MonoBehaviourSingleton<EnemyHumanResourceManager>
{
	private const int MAX_SUB_WWEAPONS = 10;

	private const string LAND_FX_NAME = "OBJ_LAND";

	private bool _landFxLoaded;

	private CHARACTER_TABLE _characterTable;

	private readonly System.Collections.Generic.Dictionary<WeaponType, int> dictWeaponTypeIdx = new Better.Dictionary<WeaponType, int>();

	private readonly System.Collections.Generic.Dictionary<int, GameObject> _modelDictionary = new Better.Dictionary<int, GameObject>();

	private readonly System.Collections.Generic.Dictionary<string, RuntimeAnimatorController> _animatorDictionary = new Better.Dictionary<string, RuntimeAnimatorController>();

	private readonly System.Collections.Generic.Dictionary<int, GameObject[]> _weaponDictionary = new Better.Dictionary<int, GameObject[]>();

	private readonly System.Collections.Generic.Dictionary<string, bool[]> _weaponMotionDictionary = new Better.Dictionary<string, bool[]>();

	private readonly List<StageUpdate.LoadCallBackObj> _listLoads = new List<StageUpdate.LoadCallBackObj>();

	private StageUpdate.LoadCallBackObj GetNewLoadCbObj()
	{
		StageUpdate.LoadCallBackObj loadCallBackObj = new StageUpdate.LoadCallBackObj();
		_listLoads.Add(loadCallBackObj);
		return loadCallBackObj;
	}

	public void Initialize()
	{
		_listLoads.Clear();
		_modelDictionary.Clear();
		_animatorDictionary.Clear();
		_weaponDictionary.Clear();
		_weaponMotionDictionary.Clear();
		DictWeaponTypeIndexInit();
	}

	private void DictWeaponTypeIndexInit()
	{
		dictWeaponTypeIdx.Clear();
		int num = 0;
		foreach (WeaponType value in Enum.GetValues(typeof(WeaponType)))
		{
			dictWeaponTypeIdx.Add(value, num);
			num++;
		}
	}

	public bool IsLoadDone()
	{
		foreach (StageUpdate.LoadCallBackObj listLoad in _listLoads)
		{
			if (!listLoad.bLoadEnd)
			{
				return false;
			}
		}
		return true;
	}

	public GameObject[] GetWeapon(int weaponID)
	{
		if (_weaponDictionary.ContainsKey(weaponID))
		{
			return _weaponDictionary[weaponID];
		}
		LoadWeapon(weaponID);
		return null;
	}

	private RuntimeAnimatorController GetAnimator(string targetAnimator)
	{
		if (!_animatorDictionary.ContainsKey(targetAnimator))
		{
			return null;
		}
		return _animatorDictionary[targetAnimator];
	}

	public void LoadLandFx()
	{
		if (!_landFxLoaded)
		{
			_landFxLoaded = true;
			StageUpdate.LoadCallBackObj newLoadCbObj = GetNewLoadCbObj();
			MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("OBJ_LAND", 5, newLoadCbObj.LoadCBNoParam);
		}
	}

	public void LoadAvatar(int id, string model)
	{
		if (_modelDictionary.ContainsKey(id))
		{
			return;
		}
		_modelDictionary.Add(id, null);
		_characterTable = ManagedSingleton<OrangeDataManager>.Instance.CHARACTER_TABLE_DICT[id];
		string s_MODEL = _characterTable.s_MODEL;
		StageUpdate.LoadCallBackObj newLoadCbObj = GetNewLoadCbObj();
		newLoadCbObj.lcb = delegate(StageUpdate.LoadCallBackObj tLCB, UnityEngine.Object obj)
		{
			_modelDictionary[id] = obj as GameObject;
		};
		MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad<UnityEngine.Object>("model/character/" + s_MODEL, s_MODEL + "_G.prefab", newLoadCbObj.LoadCB);
		LoadAnimator(_characterTable.s_ANIMATOR);
		newLoadCbObj = GetNewLoadCbObj();
		newLoadCbObj.lcb = delegate
		{
			if (!_weaponMotionDictionary.ContainsKey(_characterTable.s_ANIMATOR))
			{
				_weaponMotionDictionary[_characterTable.s_ANIMATOR] = new bool[dictWeaponTypeIdx.Count];
			}
			_weaponMotionDictionary[_characterTable.s_ANIMATOR][dictWeaponTypeIdx[WeaponType.Dummy]] = true;
		};
		HumanBase.LoadMotion(newLoadCbObj, _characterTable.s_ANIMATOR, WeaponType.Dummy);
		StartCoroutine(SetAnimator(_characterTable.s_ANIMATOR, id, model));
	}

	public void LoadAnimator(string targetAnimator)
	{
		if (!_animatorDictionary.ContainsKey(targetAnimator))
		{
			_animatorDictionary.Add(targetAnimator, null);
			StageUpdate.LoadCallBackObj newLoadCbObj = GetNewLoadCbObj();
			newLoadCbObj.lcb = delegate(StageUpdate.LoadCallBackObj tLCB, UnityEngine.Object obj)
			{
				_animatorDictionary[targetAnimator] = (RuntimeAnimatorController)obj;
			};
			string text = "newemptycontroller";
			MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad<RuntimeAnimatorController>("model/animator/" + text, text, newLoadCbObj.LoadCB);
		}
	}

	private IEnumerator SetAnimator(string targetAnimator, int targetID, string s_Model = "")
	{
		CHARACTER_TABLE temptable = ManagedSingleton<OrangeDataManager>.Instance.CHARACTER_TABLE_DICT[targetID];
		WEAPON_TABLE targetWeaponTable1 = ManagedSingleton<OrangeDataManager>.Instance.WEAPON_TABLE_DICT[temptable.n_INIT_WEAPON1];
		WEAPON_TABLE targetWeaponTable2 = null;
		WeaponType targetWeaponType1 = (WeaponType)targetWeaponTable1.n_TYPE;
		string animatorName = temptable.s_ANIMATOR;
		if (temptable.n_INIT_WEAPON2 != -1)
		{
			targetWeaponTable2 = ManagedSingleton<OrangeDataManager>.Instance.WEAPON_TABLE_DICT[temptable.n_INIT_WEAPON2];
		}
		if (targetWeaponTable2 == null)
		{
			while (!_animatorDictionary.ContainsKey(targetAnimator) || _animatorDictionary[targetAnimator] == null || !_modelDictionary.ContainsKey(targetID) || _modelDictionary[targetID] == null || !_weaponMotionDictionary.ContainsKey(animatorName) || !_weaponMotionDictionary[animatorName][dictWeaponTypeIdx[targetWeaponType1]])
			{
				yield return CoroutineDefine._waitForEndOfFrame;
			}
		}
		else
		{
			WeaponType targetWeaponType2 = (WeaponType)targetWeaponTable2.n_TYPE;
			while (!_animatorDictionary.ContainsKey(targetAnimator) || _animatorDictionary[targetAnimator] == null || !_modelDictionary.ContainsKey(targetID) || _modelDictionary[targetID] == null || !_weaponMotionDictionary.ContainsKey(animatorName) || !_weaponMotionDictionary[animatorName][dictWeaponTypeIdx[targetWeaponType1]] || !_weaponMotionDictionary[animatorName][dictWeaponTypeIdx[targetWeaponType2]])
			{
				yield return CoroutineDefine._waitForEndOfFrame;
			}
		}
		string bundleName = "model/animation/character/" + temptable.s_MODEL;
		string extrabundle = "model/animation/event/";
		AnimatorOverrideController targetOverrideController = new AnimatorOverrideController();
		RuntimeAnimatorController animator = GetAnimator(temptable.s_ANIMATOR);
		targetOverrideController.runtimeAnimatorController = animator;
		_modelDictionary[targetID].GetComponent<Animator>().runtimeAnimatorController = targetOverrideController;
		HumanBase.SetMotion(ref targetOverrideController, temptable.s_ANIMATOR, WeaponType.Dummy);
		HumanBase.SetMotion(ref targetOverrideController, temptable.s_ANIMATOR, (WeaponType)targetWeaponTable1.n_TYPE);
		if (targetWeaponTable2 != null)
		{
			HumanBase.SetMotion(ref targetOverrideController, temptable.s_ANIMATOR, (WeaponType)targetWeaponTable2.n_TYPE);
		}
		string[] ExtraAnimations = new string[0];
		string[] array = new string[0];
		switch (s_Model)
		{
		case "event2_human_1":
			ExtraAnimations = EnemyHumanAnimationHelper.GetHumanDependAnimations(EnemyHumanAnimationHelper.ControllerType.Valentine2021HumanSwordController);
			break;
		case "event2_human_2":
			ExtraAnimations = EnemyHumanAnimationHelper.GetHumanDependAnimations(EnemyHumanAnimationHelper.ControllerType.Valentine2021HumanController);
			break;
		case "event4_human_2":
			ExtraAnimations = EnemyHumanAnimationHelper.GetHumanDependAnimations(EnemyHumanAnimationHelper.ControllerType.Event4_Human2Controller);
			break;
		case "enemy_shield_human":
			array = EnemyHumanAnimationHelper.GetHumanDependAnimations(EnemyHumanAnimationHelper.ControllerType.EnemyHumanShieldController);
			break;
		case "enemy_sword_human":
			ExtraAnimations = EnemyHumanAnimationHelper.GetHumanDependAnimations(EnemyHumanAnimationHelper.ControllerType.EnemyHumanSwordController);
			break;
		case "event5_human_1":
		case "event5_human_2":
			ExtraAnimations = EnemyHumanAnimationHelper.GetHumanDependAnimations(EnemyHumanAnimationHelper.ControllerType.Event5_HumanController);
			break;
		case "event6_human_1":
			ExtraAnimations = EnemyHumanAnimationHelper.GetHumanDependAnimations(EnemyHumanAnimationHelper.ControllerType.Event6_NPC_Controller);
			break;
		case "event7_human_1":
			ExtraAnimations = EnemyHumanAnimationHelper.GetHumanDependAnimations(EnemyHumanAnimationHelper.ControllerType.Event7_NPC_Controller);
			break;
		default:
			ExtraAnimations = EnemyHumanAnimationHelper.GetHumanDependAnimations(EnemyHumanAnimationHelper.ControllerType.EnemyHumanController);
			break;
		}
		if (array != null && array.Length != 0)
		{
			for (int i = 0; i < array.Length; i++)
			{
				AnimationClip assstSync = MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssstSync<AnimationClip>(bundleName, array[i]);
				targetOverrideController[array[i]] = assstSync;
			}
		}
		if (ExtraAnimations != null && ExtraAnimations.Length != 0)
		{
			string modelbundle = "";
			switch (s_Model)
			{
			case "event2_human_1":
			case "event2_human_2":
				modelbundle = "event2_human";
				break;
			case "event5_human_1":
			case "event5_human_2":
				modelbundle = "event5_human";
				break;
			case "event4_human_2":
				modelbundle = "event4_npc";
				break;
			case "event6_human_1":
				modelbundle = "event6_npc";
				break;
			case "event7_human_1":
				modelbundle = "event7_npc";
				break;
			}
			StageUpdate.LoadCallBackObj newLoadCbObj = GetNewLoadCbObj();
			newLoadCbObj.lcb = delegate
			{
				if (modelbundle != "")
				{
					for (int j = 0; j < ExtraAnimations.Length; j++)
					{
						AnimationClip assstSync2 = MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssstSync<AnimationClip>(extrabundle + modelbundle, ExtraAnimations[j]);
						targetOverrideController["skillclip" + j] = assstSync2;
					}
				}
			};
			MonoBehaviourSingleton<AssetsBundleManager>.Instance.LoadAssets(new string[1] { extrabundle + modelbundle }, newLoadCbObj.LoadCBNoParam, AssetsBundleManager.AssetKeepMode.KEEP_IN_SCENE, false);
		}
		EnemyHumanPoolObject enemyHumanPoolObject = _modelDictionary[targetID].AddComponent<EnemyHumanPoolObject>();
		enemyHumanPoolObject.poolName = ManagedSingleton<OrangeDataManager>.Instance.CHARACTER_TABLE_DICT[targetID].n_ID.ToString();
		EnemyHumanPoolObject enemyHumanPoolObject2 = UnityEngine.Object.Instantiate(enemyHumanPoolObject);
		MonoBehaviourSingleton<PoolManager>.Instance.CreatePoolBaseLocal<EnemyHumanPoolObject>(enemyHumanPoolObject2.GetComponent<EnemyHumanPoolObject>(), ManagedSingleton<OrangeDataManager>.Instance.CHARACTER_TABLE_DICT[targetID].n_ID.ToString(), 3);
	}

	public void LoadWeapon(int weaponID, string s_Animator = "")
	{
		WEAPON_TABLE targetWeaponTable = ManagedSingleton<OrangeDataManager>.Instance.WEAPON_TABLE_DICT[weaponID];
		if (s_Animator == "")
		{
			s_Animator = _characterTable.s_ANIMATOR;
		}
		if (weaponID == -1 || (_weaponDictionary.ContainsKey(weaponID) && _weaponMotionDictionary.ContainsKey(s_Animator) && _weaponMotionDictionary[s_Animator][dictWeaponTypeIdx[(WeaponType)targetWeaponTable.n_TYPE]]))
		{
			return;
		}
		if (!_weaponDictionary.ContainsKey(weaponID))
		{
			_weaponDictionary.Add(weaponID, null);
		}
		GameObject[] targetWeaponList = new GameObject[10];
		int num = 0;
		StageUpdate.LoadCallBackObj newLoadCbObj;
		while (true)
		{
			newLoadCbObj = GetNewLoadCbObj();
			newLoadCbObj.objParam0 = num;
			newLoadCbObj.lcb = delegate(StageUpdate.LoadCallBackObj tLCB, UnityEngine.Object obj)
			{
				targetWeaponList[(int)tLCB.objParam0] = (GameObject)obj;
			};
			MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad<UnityEngine.Object>(AssetBundleScriptableObject.Instance.m_newmodel_weapon + targetWeaponTable.s_MODEL, targetWeaponTable.s_MODEL + "_G.prefab", newLoadCbObj.LoadCB);
			if (targetWeaponTable.n_SUB_LINK == -1)
			{
				break;
			}
			targetWeaponTable = ManagedSingleton<OrangeDataManager>.Instance.WEAPON_TABLE_DICT[targetWeaponTable.n_SUB_LINK];
			num++;
		}
		_weaponDictionary[weaponID] = targetWeaponList;
		newLoadCbObj = GetNewLoadCbObj();
		newLoadCbObj.lcb = delegate
		{
			if (!_weaponMotionDictionary.ContainsKey(s_Animator))
			{
				_weaponMotionDictionary[s_Animator] = new bool[dictWeaponTypeIdx.Count];
			}
			_weaponMotionDictionary[s_Animator][dictWeaponTypeIdx[(WeaponType)targetWeaponTable.n_TYPE]] = true;
		};
		HumanBase.LoadMotion(newLoadCbObj, s_Animator, (WeaponType)targetWeaponTable.n_TYPE);
		LoadWeaponBullet(weaponID);
	}

	public void LoadWeaponBullet(int weaponID)
	{
		if (weaponID == 0)
		{
			return;
		}
		WEAPON_TABLE wEAPON_TABLE = ManagedSingleton<OrangeDataManager>.Instance.WEAPON_TABLE_DICT[weaponID];
		if (wEAPON_TABLE.n_SKILL == 0)
		{
			return;
		}
		SKILL_TABLE sKILL_TABLE = ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT[wEAPON_TABLE.n_SKILL];
		WeaponType weaponType = (WeaponType)wEAPON_TABLE.n_TYPE;
		if (weaponType <= WeaponType.Melee)
		{
			if ((uint)(weaponType - 1) > 1u && weaponType != WeaponType.SprayHeavy)
			{
				int num = 8;
				return;
			}
		}
		else
		{
			switch (weaponType)
			{
			default:
				return;
			case WeaponType.DualGun:
			case WeaponType.MGun:
			case WeaponType.Gatling:
			case WeaponType.Launcher:
				break;
			}
		}
		for (int i = 0; i <= sKILL_TABLE.n_CHARGE_MAX_LEVEL; i++)
		{
			SKILL_TABLE sKILL_TABLE2 = ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT[wEAPON_TABLE.n_SKILL + i];
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
	}

	public void LoadEnemyHuman(int EnemyID)
	{
		MOB_TABLE mOB_TABLE = ManagedSingleton<OrangeDataManager>.Instance.MOB_TABLE_DICT[EnemyID];
		int n_AVATAR = mOB_TABLE.n_AVATAR;
		LoadLandFx();
		LoadAvatar(n_AVATAR, mOB_TABLE.s_MODEL);
		LoadWeapon(ManagedSingleton<OrangeDataManager>.Instance.CHARACTER_TABLE_DICT[n_AVATAR].n_INIT_WEAPON1, ManagedSingleton<OrangeDataManager>.Instance.CHARACTER_TABLE_DICT[n_AVATAR].s_ANIMATOR);
		LoadWeapon(ManagedSingleton<OrangeDataManager>.Instance.CHARACTER_TABLE_DICT[n_AVATAR].n_INIT_WEAPON2, ManagedSingleton<OrangeDataManager>.Instance.CHARACTER_TABLE_DICT[n_AVATAR].s_ANIMATOR);
	}
}
