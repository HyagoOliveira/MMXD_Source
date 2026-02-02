using CodeStage.AntiCheat.ObscuredTypes;
using Newtonsoft.Json;
using UnityEngine;
using enums;

public class HurtPassParam
{
	public ObscuredInt dmg = 0;

	public int nSubPartID;

	public string owner = "";

	public WeaponType wpnType = WeaponType.Buster;

	public Vector2 vBulletDis = Vector2.zero;

	public Vector3 S_Direction = Vector3.zero;

	public int Skill_Type;

	public int LVMax = 1;

	public BulletBase.BulletFlag BulletFlg;

	[JsonIgnore]
	public bool IsThrough
	{
		get
		{
			return (BulletFlg & BulletBase.BulletFlag.Through) == BulletBase.BulletFlag.Through;
		}
	}

	[JsonIgnore]
	public bool IsSplash
	{
		get
		{
			return (BulletFlg & BulletBase.BulletFlag.Splash) == BulletBase.BulletFlag.Splash;
		}
	}

	[JsonIgnore]
	public bool IsHitShield
	{
		get
		{
			return (BulletFlg & BulletBase.BulletFlag.HitShield) == BulletBase.BulletFlag.HitShield;
		}
	}

	[JsonIgnore]
	public bool IsBreak
	{
		get
		{
			return (BulletFlg & BulletBase.BulletFlag.Break) == BulletBase.BulletFlag.Break;
		}
	}

	[JsonIgnore]
	public bool IsPlayer
	{
		get
		{
			return (BulletFlg & BulletBase.BulletFlag.IsPlayer) == BulletBase.BulletFlag.IsPlayer;
		}
	}

	public void SetIsPlayer(bool bSet)
	{
		if (bSet)
		{
			BulletFlg |= BulletBase.BulletFlag.IsPlayer;
		}
		else
		{
			BulletFlg &= ~BulletBase.BulletFlag.IsPlayer;
		}
	}
}
