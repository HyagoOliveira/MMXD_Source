using UnityEngine;

public class SCH027_EggTrap_Controller : SCH016Controller
{
	public delegate void DeactiveCallBack(Vector3 pos);

	public DeactiveCallBack _cbDeactive;

	protected override void AfterDeactive()
	{
		if (_cbDeactive != null && _nDeactiveType == 1)
		{
			_cbDeactive(_transform.position);
		}
		base.AfterDeactive();
	}

	protected override void DestructToPool()
	{
		base.DestructToPool();
		_cbDeactive = null;
	}

	public void UpdateAimRange(OrangeCharacter mainOC)
	{
		SKILL_TABLE tSKILL_TABLE = PetWeapons[0].BulletData;
		mainOC.tRefPassiveskill.ReCalcuSkill(ref tSKILL_TABLE);
		_autoAim.UpdateAimRange(tSKILL_TABLE.f_DISTANCE);
	}
}
