using CallbackDefs;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class StarConditionComponentUnit : MonoBehaviour, IPointerClickHandler, IEventSystemHandler
{
	public enum RewardState
	{
		NOT_REACHED = 0,
		REACH = 1,
		RECEIVED = 2
	}

	private const string iconName = "UI_SubCommon_dee{0}_00";

	private const string iconNameGlow = "UI_SubCommon_dee{0}_01";

	[HideInInspector]
	public RewardState rewardState;

	[SerializeField]
	private Image imgIcon;

	[SerializeField]
	private Text textStar;

	[SerializeField]
	private Image imgReachGlow;

	[SerializeField]
	private GameObject imgReachTip;

	private int idx;
    [System.Obsolete]
    private CallbackIdx m_cb;

	private int count;

    [System.Obsolete]
    public void Setup(int p_idx, int p_iconIdx, int p_star, RewardState p_rewardState, CallbackIdx p_cb)
	{
		idx = p_idx;
		m_cb = p_cb;
		textStar.text = p_star.ToString();
		UpdateRewardState(p_rewardState);
		MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(AssetBundleScriptableObject.Instance.m_texture_ui_sub_common, string.Format("UI_SubCommon_dee{0}_00", p_iconIdx.ToString("00")), delegate(Sprite obj)
		{
			imgIcon.sprite = obj;
			imgIcon.color = Color.white;
		});
		MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(AssetBundleScriptableObject.Instance.m_texture_ui_sub_common, string.Format("UI_SubCommon_dee{0}_01", p_iconIdx.ToString("00")), delegate(Sprite obj)
		{
			imgReachGlow.sprite = obj;
			imgReachGlow.color = Color.white;
		});
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		if (eventData.button == PointerEventData.InputButton.Left)
		{
			m_cb.CheckTargetToInvoke(idx);
		}
	}

	public void UpdateRewardState(RewardState p_rewardState)
	{
		rewardState = p_rewardState;
		switch (rewardState)
		{
		case RewardState.NOT_REACHED:
			imgReachGlow.gameObject.SetActive(false);
			imgReachTip.gameObject.SetActive(false);
			break;
		case RewardState.REACH:
			imgReachGlow.gameObject.SetActive(false);
			imgReachTip.gameObject.SetActive(true);
			DisplayReachEft();
			break;
		case RewardState.RECEIVED:
			imgReachGlow.gameObject.SetActive(true);
			imgReachTip.gameObject.SetActive(false);
			break;
		}
	}

	private void DisplayReachEft()
	{
		count++;
		if (count % 2 == 0)
		{
			LeanTween.rotateLocal(imgIcon.gameObject, new Vector3(0f, 0f, 10f), 0.2f).setLoopPingPong().setEaseInOutQuad()
				.setOnComplete(DisplayReachEft)
				.setLoopPingPong(1);
		}
		else
		{
			LeanTween.rotateLocal(imgIcon.gameObject, new Vector3(0f, 0f, -10f), 0.2f).setLoopPingPong().setEaseInOutQuad()
				.setOnComplete(DisplayReachEft)
				.setDelay(0.5f)
				.setLoopPingPong(1);
		}
	}
}
