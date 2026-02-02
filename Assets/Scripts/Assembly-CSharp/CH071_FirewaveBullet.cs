using UnityEngine;

public class CH071_FirewaveBullet : CollideBullet
{
	public override void UpdateBulletData(SKILL_TABLE pData, string owner = "", int nInRecordID = 0, int nInNetID = 0, int nDirection = 1)
	{
		base.UpdateBulletData(pData, owner, nInRecordID, nInNetID, nDirection);
		_transform.eulerAngles = Vector3.zero;
	}
}
