using System.Collections;
using CallbackDefs;
using UnityEngine;
using enums;

public class EnemyHumanBuilder : MonoBehaviour
{
	private const int MAX_WEAPON = 2;

	private const int PRECREATE_WEAPON_COUNT = 3;

	public int[] WeaponList = new int[2];

	public int EnemyID = 1;

	public int AvatarID = 1;

	private MOB_TABLE EnemyData;

	private CHARACTER_TABLE characterTable;

	private EnemyHumanPoolObject[] SetupWeapons(int weaponID, EnemyHumanPoolObject bodyInstance, int id)
	{
		string text = "NormalWeapon";
		WEAPON_TABLE wEAPON_TABLE = ManagedSingleton<OrangeDataManager>.Instance.WEAPON_TABLE_DICT[weaponID];
		GameObject[] weapon = MonoBehaviourSingleton<EnemyHumanResourceManager>.Instance.GetWeapon(weaponID);
		EnemyHumanPoolObject[] array = new EnemyHumanPoolObject[weapon.Length];
		int num = 0;
		Transform[] target = bodyInstance.transform.GetComponentsInChildren<Transform>(true);
		for (int i = 0; i < weapon.Length; i++)
		{
			if (!(weapon[i] == null))
			{
				if (!MonoBehaviourSingleton<PoolManager>.Instance.ExistsInPool<EnemyHumanPoolObject>(wEAPON_TABLE.s_MODEL))
				{
					GameObject gameObject = Object.Instantiate(weapon[i], Vector3.zero, Quaternion.identity);
					gameObject.AddComponent<EnemyHumanPoolObject>().poolName = wEAPON_TABLE.s_MODEL;
					MonoBehaviourSingleton<PoolManager>.Instance.CreatePoolBaseLocal<EnemyHumanPoolObject>(gameObject.GetComponent<EnemyHumanPoolObject>(), wEAPON_TABLE.s_MODEL, 3);
				}
				array[i] = MonoBehaviourSingleton<PoolManager>.Instance.GetPoolObj<EnemyHumanPoolObject>(wEAPON_TABLE.s_MODEL);
				array[i].gameObject.name = text + id;
				switch ((WeaponType)(short)wEAPON_TABLE.n_TYPE)
				{
				case WeaponType.Buster:
				case WeaponType.Spray:
				case WeaponType.SprayHeavy:
					array[i].transform.SetParent(OrangeBattleUtility.FindChildRecursive(ref target, "L BusterPoint"));
					array[i].transform.localEulerAngles = Vector3.zero;
					break;
				case WeaponType.Melee:
					array[i].transform.SetParent(OrangeBattleUtility.FindChildRecursive(ref target, "R WeaponPoint"));
					array[i].transform.localEulerAngles = new Vector3(0f, -90f, 90f);
					break;
				case WeaponType.MGun:
					array[i].transform.SetParent(OrangeBattleUtility.FindChildRecursive(ref target, "R WeaponPoint"));
					array[i].transform.localEulerAngles = Vector3.zero;
					break;
				case WeaponType.Gatling:
				case WeaponType.Launcher:
					array[i].transform.SetParent(OrangeBattleUtility.FindChildRecursive(ref target, "L WeaponPoint"));
					array[i].transform.localEulerAngles = Vector3.zero;
					break;
				case WeaponType.DualGun:
					array[i].transform.SetParent((num == 0) ? OrangeBattleUtility.FindChildRecursive(ref target, "L WeaponPoint") : OrangeBattleUtility.FindChildRecursive(ref target, "R WeaponPoint"));
					array[i].transform.localEulerAngles = Vector3.zero;
					break;
				}
				array[i].transform.localPosition = Vector3.zero;
				num++;
			}
		}
		return array;
	}

    [System.Obsolete]
    public void CreateHumanEnemy(CallbackObjs pCallBack = null)
	{
		EnemyData = ManagedSingleton<OrangeDataManager>.Instance.MOB_TABLE_DICT[EnemyID];
		AvatarID = EnemyData.n_AVATAR;
		characterTable = ManagedSingleton<OrangeDataManager>.Instance.CHARACTER_TABLE_DICT[AvatarID];
		WeaponList[0] = ManagedSingleton<OrangeDataManager>.Instance.CHARACTER_TABLE_DICT[AvatarID].n_INIT_WEAPON1;
		WeaponList[1] = ManagedSingleton<OrangeDataManager>.Instance.CHARACTER_TABLE_DICT[AvatarID].n_INIT_WEAPON2;
		StartCoroutine(Build(WeaponList, pCallBack));
	}

    [System.Obsolete]
    private IEnumerator Build(int[] _weaponlist, CallbackObjs pCallBack)
	{
		EnemyHumanController enemy = base.gameObject.GetComponent<EnemyHumanController>();
		EnemyHumanPoolObject poolObj = MonoBehaviourSingleton<PoolManager>.Instance.GetPoolObj<EnemyHumanPoolObject>(AvatarID.ToString());
		if (enemy == null)
		{
			yield break;
		}
		poolObj.name = "model";
		poolObj.transform.SetParent(base.transform);
		poolObj.transform.eulerAngles = new Vector3(0f, 90f, 0f);
		poolObj.transform.localPosition = Vector3.zero;
		enemy.CurrentEnemyHumanModel = poolObj;
		enemy.CurrentEnemyHumanWeapon = SetupWeapons(_weaponlist[0], poolObj, 0);
		enemy.PlayerWeapons = new WeaponStruct[2];
		for (int i = 0; i < 2; i++)
		{
			if (_weaponlist[i] != -1)
			{
				enemy.PlayerWeapons[i] = new WeaponStruct();
				enemy.PlayerWeapons[i].WeaponData = ManagedSingleton<OrangeDataManager>.Instance.WEAPON_TABLE_DICT[_weaponlist[i]];
			}
		}
		enemy.AvatarID = AvatarID;
		yield return CoroutineDefine._waitForEndOfFrame;
		pCallBack.CheckTargetToInvoke(enemy);
		Object.Destroy(this);
	}
}
