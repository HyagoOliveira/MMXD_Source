using UnityEngine;

public class CH090_Kobun : CH037_Kobun
{
	protected override void DictionaryInit()
	{
		dictAnimateHash[HumanBase.AnimateId.ANI_WIN_POSE] = Animator.StringToHash("ch090_kobun_win");
		dictAnimateHash[HumanBase.AnimateId.ANI_TELEPORT_OUT_POSE] = Animator.StringToHash("ch090_kobun_logout");
		dictAnimateHash[HumanBase.AnimateId.ANI_LOGOUT2] = Animator.StringToHash("ch090_kobun_logout");
	}
}
