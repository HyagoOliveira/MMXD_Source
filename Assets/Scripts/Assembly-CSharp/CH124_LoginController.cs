using System.Collections.Generic;
using Better;
using UnityEngine;

public class CH124_LoginController : MonoBehaviour
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
		dictAnimateHash[HumanBase.AnimateId.ANI_TELEPORT_IN_POSE] = Animator.StringToHash("ch124_login");
	}

	public void Play(HumanBase.AnimateId animateId)
	{
		if (!animator.isActiveAndEnabled)
		{
			animator.enabled = true;
		}
		if (lastAnimateId != animateId)
		{
			lastAnimateId = animateId;
			characterMaterial.Appear(null, 0f);
			animator.Play(dictAnimateHash[animateId]);
		}
	}

	public void UpdateMask(int idx, int val)
	{
		characterMaterial.UpdateMask(idx, val);
	}

	public void Disappear()
	{
		characterMaterial.Disappear(null, 0f);
		UpdateMask(-1, 0);
		if (animator.isActiveAndEnabled)
		{
			animator.enabled = false;
		}
	}
}
