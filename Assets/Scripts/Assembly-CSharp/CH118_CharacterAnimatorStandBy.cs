using enums;

public class CH118_CharacterAnimatorStandBy : CharacterAnimatorStandBy
{
	protected override void OverrideStandClips(WeaponType weaponType, ref string originalBundleName, ref string[] originalClips)
	{
		if (weaponType == WeaponType.Spray)
		{
			originalBundleName = originalBundleName.Replace("spray/m", "spray/c");
			SetPositionRate(1f);
		}
		else
		{
			SetPositionRate(0.82f);
		}
	}

	private void SetPositionRate(float val)
	{
		PositionRateController component = GetComponent<PositionRateController>();
		if ((bool)component)
		{
			component.Rate = val;
		}
	}
}
