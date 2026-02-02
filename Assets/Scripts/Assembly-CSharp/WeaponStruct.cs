#define RELEASE
using System.Collections.Generic;
using UnityEngine;
using enums;

public class WeaponStruct
{
	public enum AbilityType
	{
		WEAPON = 0,
		SKILL = 1,
		FS = 2
	}

	public WEAPON_TABLE WeaponData;

	public SKILL_TABLE BulletData;

	public SKILL_TABLE[] FastBulletDatas;

	public bool ForceLock;

	public float MagazineRemain;

	public float MagazineRemainMax;

	public int[] ChargeTime;

	public sbyte ChargeLevel;

	public UpdateTimer ChargeTimer;

	public UpdateTimer LastUseTimer;

	public CharacterMaterial[] WeaponMesh;

	public Transform[] ShootTransform;

	public Transform[] ShootTransform2;

	public MeleeWeaponTrail WeaponTrial;

	public Transform SlashObject;

	public SlashEfx SlashEfxCmp;

	public ChipSystem ChipEfx;

	public bool chip_switch;

	public MeleeBullet MeleeBullet;

	public GatlingSpinner GatlingSpinner;

	public Sprite Icon;

	public Sprite BackupIcon;

	public WeaponStatus weaponStatus;

	public int SkillLV;

	public int EnhanceEXIndex;

	public int Reload_index;

	public ComboCheckData[] ComboCheckDatas;

	public void Initialize(AbilityType type, int idx, OrangeCharacter refCharacter)
	{
		Transform[] array = new Transform[1];
		ShootTransform = new Transform[10];
		WeaponMesh = new CharacterMaterial[10];
		switch (type)
		{
		case AbilityType.SKILL:
		{
			int key = ((idx == 0) ? refCharacter.CharacterData.n_SKILL1 : refCharacter.CharacterData.n_SKILL2);
			if (idx >= 10000 && idx < 100000)
			{
				key = idx;
			}
			BulletData = ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT[key].GetSkillTableByValue();
			refCharacter.tRefPassiveskill.ReCalcuSkill(ref BulletData);
			switch (BulletData.n_USE_DEFAULT_WEAPON)
			{
			case -1:
				array = null;
				break;
			case 0:
				array = OrangeBattleUtility.FindAllChildRecursive(refCharacter.transform, "SkillWeapon0");
				break;
			case 1:
				array = OrangeBattleUtility.FindAllChildRecursive(refCharacter.transform, "SkillWeapon1");
				break;
			}
			break;
		}
		case AbilityType.FS:
			if (BulletData == null)
			{
				BulletData = new SKILL_TABLE();
			}
			refCharacter.tRefPassiveskill.ReCalcuSkill(ref BulletData);
			switch (BulletData.n_USE_DEFAULT_WEAPON)
			{
			case -1:
				array = null;
				break;
			case 0:
				array = OrangeBattleUtility.FindAllChildRecursive(refCharacter.transform, "SkillWeapon0");
				break;
			case 1:
				array = OrangeBattleUtility.FindAllChildRecursive(refCharacter.transform, "SkillWeapon1");
				break;
			}
			break;
		default:
			if (WeaponData.n_SKILL == 0)
			{
				BulletData = new SKILL_TABLE();
			}
			else
			{
				BulletData = ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT[WeaponData.n_SKILL].GetSkillTableByValue();
			}
			array = OrangeBattleUtility.FindAllChildRecursive(refCharacter.transform, "NormalWeapon" + idx);
			refCharacter.tRefPassiveskill.ReCalcuSkill(ref BulletData);
			break;
		}
		if (WeaponData != null)
		{
			WeaponType weaponType = (WeaponType)WeaponData.n_TYPE;
			if (weaponType != WeaponType.Melee)
			{
				if (weaponType != WeaponType.Gatling)
				{
					if (weaponType == WeaponType.Launcher)
					{
						ShootTransform = OrangeBattleUtility.FindMultiChildRecursive(array, "ShootPoint");
						if (array[0] != null && refCharacter.CharacterID == 60 && WeaponData.n_ID == 104010)
						{
							array[0].localPosition += new Vector3(0f, 0.06f, 0f);
							ShootTransform[0].transform.localPosition = new Vector3(ShootTransform[0].transform.localPosition.x, 0.35f, ShootTransform[0].transform.localPosition.z);
						}
						goto IL_04d7;
					}
				}
				else
				{
					if (array[0] != null && refCharacter.CharacterID == 60)
					{
						array[0].localPosition += new Vector3(0f, 0.25f, 0f);
					}
					Transform transform = OrangeBattleUtility.FindChildRecursive(array[0], "_sub");
					if ((bool)transform)
					{
						GatlingSpinner = transform.gameObject.AddComponent<GatlingSpinner>();
					}
				}
				ShootTransform = OrangeBattleUtility.FindMultiChildRecursive(array, "ShootPoint");
			}
			else
			{
				WeaponTrial = array[0].GetComponentInChildren<MeleeWeaponTrail>();
				SlashObject = OrangeBattleUtility.FindChildRecursive(refCharacter.transform, "SlashEfx", true);
				if ((bool)SlashObject)
				{
					SlashObject.gameObject.name = "SlashEfx_used";
				}
				else
				{
					SlashObject = Object.Instantiate(new GameObject("SlashEfx_used")).transform;
					SlashObject.SetParent(refCharacter.transform, false);
					SlashObject.gameObject.AddComponent<SlashEfx>();
				}
				bool isMale = refCharacter.CharacterData.s_ANIMATOR.Substring(0, 4).Equals("male");
				if (refCharacter.CharacterDashType == OrangeCharacter.DASH_TYPE.CLASSIC)
				{
					isMale = true;
				}
				SlashEfxCmp = SlashObject.transform.GetComponent<SlashEfx>();
				SlashEfxCmp.InitSlashData(WeaponData.s_MODEL, isMale, refCharacter.transform, array[0].localScale);
				if (refCharacter.CharacterID == 60)
				{
					Transform transform2 = OrangeBattleUtility.FindChildRecursive(refCharacter.transform, "model");
					if ((bool)transform2)
					{
						PositionRateController component = transform2.GetComponent<PositionRateController>();
						if (component != null)
						{
							SlashEfxCmp.SetPositionRate(component.GetRate());
						}
					}
				}
				GameObject gameObject = new GameObject();
				MeleeBullet = gameObject.AddComponent<MeleeBullet>();
				MeleeBullet.UpdateWeaponData(BulletData, refCharacter.sPlayerName);
				MeleeBullet.transform.SetParent(refCharacter.transform);
				gameObject.transform.localPosition = Vector3.zero;
			}
			goto IL_04d7;
		}
		string s_USE_MOTION = BulletData.s_USE_MOTION;
		if (!(s_USE_MOTION == "SPECIAL_000"))
		{
			if (s_USE_MOTION == "MARINO_DARTS")
			{
				ShootTransform[0] = OrangeBattleUtility.FindChildRecursive(refCharacter.transform, "Bip L Finger1");
			}
		}
		else
		{
			ShootTransform[0] = refCharacter.transform;
			MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuse_fallen_000_f", 5);
		}
		ShootTransform2 = new Transform[1];
		goto IL_0561;
		IL_04d7:
		ShootTransform2 = OrangeBattleUtility.FindAllChildRecursive(refCharacter.transform, "Bip");
		goto IL_0561;
		IL_0561:
		int n_CHARGE_MAX_LEVEL = BulletData.n_CHARGE_MAX_LEVEL;
		ChargeTime = new int[n_CHARGE_MAX_LEVEL + 1];
		int num = n_CHARGE_MAX_LEVEL + 1;
		if (type == AbilityType.WEAPON)
		{
			WeaponType weaponType = (WeaponType)WeaponData.n_TYPE;
			if (weaponType == WeaponType.Melee && num < 20)
			{
				num = 20;
			}
		}
		List<SKILL_TABLE> list = new List<SKILL_TABLE>();
		list.Add(BulletData);
		for (int i = 1; i < num; i++)
		{
			SKILL_TABLE tSKILL_TABLE = ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT[list[0].n_ID + i].GetSkillTableByValue();
			refCharacter.tRefPassiveskill.ReCalcuSkill(ref tSKILL_TABLE);
			list.Add(tSKILL_TABLE);
		}
		List<ComboCheckData> list2 = new List<ComboCheckData>();
		for (int j = 0; j < num; j++)
		{
			if (list[j].n_COMBO_SKILL != 0)
			{
				SKILL_TABLE tSKILL_TABLE2 = list[j];
				while (tSKILL_TABLE2.n_COMBO_SKILL != 0)
				{
					ComboCheckData comboCheckData = new ComboCheckData();
					List<ComboCheckBuff> list3 = new List<ComboCheckBuff>();
					comboCheckData.nComboSkillID = tSKILL_TABLE2.n_COMBO_SKILL;
					comboCheckData.nTriggerSkillID = tSKILL_TABLE2.n_ID;
					if (tSKILL_TABLE2.s_COMBO != "null")
					{
						string[] array2 = tSKILL_TABLE2.s_COMBO.Split(',');
						for (int k = 0; k < int.Parse(array2[0]); k++)
						{
							ComboCheckBuff comboCheckBuff = new ComboCheckBuff();
							comboCheckBuff.nBuffID = int.Parse(array2[1 + k * 2]);
							comboCheckBuff.nBuffCount = int.Parse(array2[1 + k * 2 + 1]);
							list3.Add(comboCheckBuff);
						}
					}
					else
					{
						ComboCheckBuff comboCheckBuff2 = new ComboCheckBuff();
						comboCheckBuff2.nBuffID = -refCharacter.SkillComboBuffIndex;
						comboCheckBuff2.nBuffCount = 1;
						list3.Add(comboCheckBuff2);
						refCharacter.SkillComboBuffIndex++;
					}
					comboCheckData.ComboCheckBuffs = list3.ToArray();
					list2.Add(comboCheckData);
					SKILL_TABLE value;
					if (!ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT.TryGetValue(tSKILL_TABLE2.n_COMBO_SKILL, out value))
					{
						Debug.LogWarning(string.Format("Failed to get {0} of ComboSkill : {1} from Skill : {2}, break Combo", "SKILLSE_TABLE", tSKILL_TABLE2.n_COMBO_SKILL, tSKILL_TABLE2.n_ID));
						break;
					}
					tSKILL_TABLE2 = value.GetSkillTableByValue();
					refCharacter.tRefPassiveskill.ReCalcuSkill(ref tSKILL_TABLE2);
					list.Add(tSKILL_TABLE2);
					SKILL_TABLE tSKILL_TABLE3 = tSKILL_TABLE2;
					while (ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT.ContainsKey(tSKILL_TABLE3.n_LINK_SKILL))
					{
						tSKILL_TABLE3 = ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT[tSKILL_TABLE3.n_LINK_SKILL].GetSkillTableByValue();
						refCharacter.tRefPassiveskill.ReCalcuSkill(ref tSKILL_TABLE3);
						list.Add(tSKILL_TABLE3);
					}
				}
			}
			if (list[j].n_LINK_SKILL != 0)
			{
				SKILL_TABLE tSKILL_TABLE4 = list[j];
				while (ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT.ContainsKey(tSKILL_TABLE4.n_LINK_SKILL))
				{
					tSKILL_TABLE4 = ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT[tSKILL_TABLE4.n_LINK_SKILL].GetSkillTableByValue();
					refCharacter.tRefPassiveskill.ReCalcuSkill(ref tSKILL_TABLE4);
					list.Add(tSKILL_TABLE4);
				}
			}
		}
		FastBulletDatas = list.ToArray();
		ComboCheckDatas = list2.ToArray();
		for (int l = 0; l <= n_CHARGE_MAX_LEVEL; l++)
		{
			ChargeTime[l] = FastBulletDatas[l].n_CHARGE;
		}
		if (array != null)
		{
			for (int m = 0; m < array.Length; m++)
			{
				if (array[m] != null)
				{
					WeaponMesh[m] = array[m].GetComponent<CharacterMaterial>();
				}
			}
		}
		ForceLock = false;
		ChargeTimer = new UpdateTimer();
		LastUseTimer = new UpdateTimer();
		LastUseTimer.TimerStart();
		if (type == AbilityType.FS)
		{
			MagazineRemain = MagazineRemainMax;
		}
		else
		{
			MagazineRemain = BulletData.n_MAGAZINE;
		}
		if (ChipEfx != null)
		{
			ChipEfx.SetWeaponMesh(WeaponMesh);
		}
		CharacterMaterial[] weaponMesh = WeaponMesh;
		foreach (CharacterMaterial characterMaterial in weaponMesh)
		{
			if (characterMaterial != null)
			{
				float height = Mathf.Clamp(characterMaterial.GetDissolveModelHeight() * characterMaterial.transform.localScale.x, 1f, float.MaxValue);
				characterMaterial.ChangeDissolveModelModelHeight(height);
			}
		}
	}

	public void UpdateMagazine()
	{
		switch (BulletData.n_MAGAZINE_TYPE)
		{
		case 0:
		{
			SKILL_TABLE sKILL_TABLE = FastBulletDatas[Reload_index];
			if (MagazineRemain > 0f)
			{
				if ((float)LastUseTimer.GetMillisecond() >= (float)OrangeCharacter.AutoReloadDelay + (float)sKILL_TABLE.n_RELOAD * OrangeCharacter.AutoReloadPercent)
				{
					MagazineRemain = sKILL_TABLE.n_MAGAZINE;
				}
			}
			else if (LastUseTimer.GetMillisecond() >= sKILL_TABLE.n_RELOAD)
			{
				MagazineRemain = sKILL_TABLE.n_MAGAZINE;
			}
			break;
		}
		case 1:
			if (MagazineRemain < 0f)
			{
				ForceLock = true;
			}
			if (ForceLock)
			{
				if (LastUseTimer.GetMillisecond() >= FastBulletDatas[Reload_index].n_RELOAD)
				{
					ForceLock = false;
					MagazineRemain = BulletData.n_MAGAZINE;
				}
				break;
			}
			if (MagazineRemain < (float)BulletData.n_MAGAZINE)
			{
				MagazineRemain += GameLogicUpdateManager.m_fFrameLen * 10f * FastBulletDatas[0].f_ENERGY_RATE;
			}
			if (MagazineRemain > (float)BulletData.n_MAGAZINE)
			{
				MagazineRemain = BulletData.n_MAGAZINE;
			}
			break;
		}
	}

	public void ReCaluBulletData(int skillId, OrangeCharacter refCharacter)
	{
		if (BulletData != null && BulletData.n_ID == skillId)
		{
			BulletData = ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT[BulletData.n_ID].GetSkillTableByValue();
			refCharacter.tRefPassiveskill.ReCalcuSkill(ref BulletData);
		}
		for (int i = 0; i < FastBulletDatas.Length; i++)
		{
			if (FastBulletDatas[i] == null || FastBulletDatas[i].n_ID != skillId)
			{
				continue;
			}
			FastBulletDatas[i] = ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT[FastBulletDatas[i].n_ID].GetSkillTableByValue();
			refCharacter.tRefPassiveskill.ReCalcuSkill(ref FastBulletDatas[i]);
			if (FastBulletDatas[i].s_MODEL != "" && FastBulletDatas[i].s_MODEL != "null" && FastBulletDatas[i].s_MODEL != "DUMMY" && !MonoBehaviourSingleton<PoolManager>.Instance.IsPreload(FastBulletDatas[i].s_MODEL))
			{
				int index = i;
				MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad("prefab/bullet/" + FastBulletDatas[index].s_MODEL, FastBulletDatas[index].s_MODEL, delegate(GameObject obj)
				{
					Debug.Log("index = " + index + "   i = " + i);
					BulletBase component = obj.GetComponent<BulletBase>();
					MonoBehaviourSingleton<PoolManager>.Instance.CreatePoolBaseLocal<BulletBase>(Object.Instantiate(component), FastBulletDatas[index].s_MODEL, 5);
				});
			}
		}
	}
}
