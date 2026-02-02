#define RELEASE
using System;
using UnityEngine;
using enums;

public class CharacterAnimatorStandByEx : CharacterAnimatorStandBy
{
	[Flags]
	public enum ReplaceNumber
	{
		none = 0,
		one = 1,
		two = 2,
		three = 4,
		one2 = 3,
		one3 = 5,
		two3 = 6
	}

	[SerializeField]
	private WeaponType[] replaceWeaponTypes = new WeaponType[0];

	public ReplaceNumber[] replaceNumbers = new ReplaceNumber[0];

	protected override void OverrideStandClips(WeaponType weaponType, ref string originalBundleName, ref string[] originalClips)
	{
		if (replaceWeaponTypes.Length != replaceNumbers.Length)
		{
			return;
		}
		for (int i = 0; i < replaceWeaponTypes.Length; i++)
		{
			if (weaponType != replaceWeaponTypes[i])
			{
				continue;
			}
			for (int j = 0; j < originalClips.Length; j++)
			{
				string text = originalClips[j];
				ReplaceNumber replaceNumber = replaceNumbers[i];
				ReplaceNumber nowNumber = GetNowNumber(j);
				if (replaceNumber.HasFlag(nowNumber))
				{
					originalClips[j] = originalClips[j].Replace((j + 1).ToString(), GetReplaceNumber(replaceNumber).ToString());
					Debug.Log(text + "=>" + originalClips[j]);
				}
			}
			break;
		}
	}

	private ReplaceNumber GetNowNumber(int j)
	{
		switch (j)
		{
		case 0:
			return ReplaceNumber.one;
		case 1:
			return ReplaceNumber.two;
		case 2:
			return ReplaceNumber.three;
		default:
			return ReplaceNumber.none;
		}
	}

	private int GetReplaceNumber(ReplaceNumber replaceNumber)
	{
		switch (replaceNumber)
		{
		case ReplaceNumber.one:
			return 2;
		case ReplaceNumber.two:
			return 3;
		case ReplaceNumber.three:
			return 1;
		case ReplaceNumber.one2:
			return 3;
		case ReplaceNumber.one3:
			return 2;
		case ReplaceNumber.two3:
			return 1;
		default:
			return 1;
		}
	}
}
