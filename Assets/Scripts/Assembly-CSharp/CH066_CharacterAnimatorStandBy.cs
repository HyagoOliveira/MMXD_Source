using enums;

public class CH066_CharacterAnimatorStandBy : CharacterAnimatorStandBy
{
	protected override void OverrideStandClips(WeaponType weaponType, ref string originalBundleName, ref string[] originalClips)
	{
		if (weaponType == WeaponType.Gatling)
		{
			for (int i = 0; i < originalClips.Length; i++)
			{
				originalClips[i] = originalClips[i].Replace((i + 1).ToString(), "4");
			}
		}
	}
}
