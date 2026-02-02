using UnityEngine;

public class CH111_Kobun : CH037_Kobun
{
	protected override void DictionaryInit()
	{
		dictAnimateHash[HumanBase.AnimateId.ANI_TELEPORT_IN_POSE] = Animator.StringToHash("ch111_koubu_login");
		dictAnimateHash[HumanBase.AnimateId.ANI_WIN_POSE] = Animator.StringToHash("ch111_koubu_win");
	}
}
