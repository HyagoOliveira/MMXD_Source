using System.Collections.Generic;
using Better;
using UnityEngine;

public class CH037_Kobun : MonoBehaviour
{
	protected System.Collections.Generic.Dictionary<HumanBase.AnimateId, int> dictAnimateHash = new Better.Dictionary<HumanBase.AnimateId, int>();

	[SerializeField]
	private Animator animator;

	[SerializeField]
	private CharacterMaterial characterMaterial;

	private HumanBase.AnimateId lastAnimateId = HumanBase.AnimateId.MAX_ANI;

	private void Awake()
	{
		dictAnimateHash.Clear();
		DictionaryInit();
	}

	protected virtual void DictionaryInit()
	{
		dictAnimateHash[HumanBase.AnimateId.ANI_TELEPORT_IN_POSE] = Animator.StringToHash("ch037_kobun_login");
		dictAnimateHash[HumanBase.AnimateId.ANI_WIN_POSE] = Animator.StringToHash("ch037_kobun_win");
		dictAnimateHash[HumanBase.AnimateId.ANI_TELEPORT_OUT_POSE] = Animator.StringToHash("ch037_kobun_logout");
		dictAnimateHash[HumanBase.AnimateId.ANI_LOGOUT2] = Animator.StringToHash("ch037_kobun_logout");
	}

	public void Play(HumanBase.AnimateId animateId)
	{
		if (lastAnimateId != animateId)
		{
			lastAnimateId = animateId;
			characterMaterial.Appear(null, 0.01f);
			animator.Play(dictAnimateHash[animateId]);
		}
	}
}
