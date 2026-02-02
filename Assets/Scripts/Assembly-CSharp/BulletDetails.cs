using CallbackDefs;
using UnityEngine;

public class BulletDetails
{
	public SKILL_TABLE bulletData;

	public WeaponStatus refWS;

	public Transform ShootTransform;

	public Vector3 ShootPosition;

	public Vector3? ShotDir;

	public int nRecordID;

	public int nBulletRecordID;

	public int nBulletLV;

	public bool useExtraCollider = true;

	public IAimTarget tAutoAimTarget;

	public int nDirect = 1;

	public bool ManualShoot;
    [System.Obsolete]
    public CallbackObj hitCallBack;
}
