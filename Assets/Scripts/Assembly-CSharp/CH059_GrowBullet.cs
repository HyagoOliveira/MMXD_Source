using UnityEngine;

public class CH059_GrowBullet : BasicBullet
{
	public override void UpdateBulletData(SKILL_TABLE pData, string owner = "", int nInRecordID = 0, int nNetID = 0, int nDirection = 1)
	{
		base.UpdateBulletData(pData, owner, nInRecordID, nNetID);
		if (nDirection == 1)
		{
			_transform.localScale = Vector3.one;
		}
		else
		{
			_transform.localScale = new Vector3(1f, -1f, -1f);
		}
	}
}
