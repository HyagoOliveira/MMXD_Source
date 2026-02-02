using UnityEngine;

public class PetWeaponStruct
{
	public SKILL_TABLE BulletData;

	public SKILL_TABLE[] FastBulletDatas;

	public float MagazineRemain;

	public int[] ChargeTime;

	public sbyte ChargeLevel;

	public UpdateTimer ChargeTimer;

	public UpdateTimer LastUseTimer;

	public Transform[] ShootTransform;

	public GatlingSpinner GatlingSpinner;

	public Sprite Icon;

	public Sprite Bacup_Icon;

	public WeaponStatus weaponStatus;

	public int SkillLV;
}
