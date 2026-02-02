using UnityEngine;
using UnityEngine.UI;

public class DeepRecordChallengeResultUI : OrangeUIBase
{
	[SerializeField]
	private Animator animator;

	[SerializeField]
	private Image[] imgCharacters;

	public void Setup()
	{
		canvasGroup.alpha = 0f;
		animator.speed = 1f;
		if (ManagedSingleton<DeepRecordHelper>.Instance.PlayerInfo.CharacterList.Count > 0)
		{
			int key = ManagedSingleton<DeepRecordHelper>.Instance.PlayerInfo.CharacterList[0];
			CHARACTER_TABLE value;
			if (ManagedSingleton<OrangeDataManager>.Instance.CHARACTER_TABLE_DICT.TryGetValue(key, out value))
			{
				LoadCutInIcon(value.s_ICON);
			}
		}
		base.CloseSE = SystemSE.NONE;
	}

	private void LoadCutInIcon(string iconName)
	{
		MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(AssetBundleScriptableObject.Instance.GetIconCharacter2("icon_" + iconName), "icon_" + iconName, delegate(Sprite spr)
		{
			if (spr != null)
			{
				Image[] array = imgCharacters;
				foreach (Image obj in array)
				{
					obj.sprite = spr;
					obj.color = Color.white;
				}
			}
			else
			{
				Image[] array = imgCharacters;
				for (int i = 0; i < array.Length; i++)
				{
					array[i].color = Color.clear;
				}
			}
			animator.enabled = true;
			canvasGroup.alpha = 1f;
		});
	}

	public void PlayCloseSE()
	{
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WINDOW_CL);
	}

	public void AnimationComplete()
	{
		animator.speed = 1f;
		OnClickCloseBtn();
	}

	public void OnClickSkip()
	{
		if (animator.isActiveAndEnabled && animator.speed == 1f)
		{
			animator.speed = 5f;
		}
	}

	public void PlayBattleSE()
	{
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_BATTLE);
	}

	public void PlayWinSE()
	{
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_WIN01);
	}

	public void PlayLoseSE()
	{
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_LOSE01);
	}

	public void PlayEffectSE()
	{
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_EFFECT04);
	}
}
