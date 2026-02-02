using UnityEngine;

public class CH035_FxSkill1 : FxBase
{
	[SerializeField]
	private Transform[] fxKeepDirections;

	public override void Active(params object[] p_params)
	{
		if (base.transform.localRotation == OrangeCharacter.ReversedQuaternion)
		{
			for (int i = 0; i < fxKeepDirections.Length; i++)
			{
				fxKeepDirections[i].localRotation = OrangeCharacter.NormalQuaternion;
			}
		}
		else
		{
			for (int j = 0; j < fxKeepDirections.Length; j++)
			{
				fxKeepDirections[j].localRotation = OrangeCharacter.ReversedQuaternion;
			}
		}
		base.transform.localRotation = Quaternion.identity;
		base.Active(p_params);
	}
}
