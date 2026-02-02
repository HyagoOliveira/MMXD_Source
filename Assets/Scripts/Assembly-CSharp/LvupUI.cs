using DragonBones;
using NaughtyAttributes;
using UnityEngine;

public class LvupUI : OrangeUIBase
{
	private readonly string animationName1 = "newAnimation";

	private readonly string animationName2 = "newAnimation_1";

	private readonly string event_move_top = "move_top";

	private readonly string event_move_stop = "move_stop";

	private float bgY = 610f;

	[SerializeField]
	private RectTransform rtBg;

	[SerializeField]
	private CanvasGroup canvasGroupInfo;

	[SerializeField]
	private OrangeText[] textLV = new OrangeText[2];

	[SerializeField]
	private OrangeText[] textEnergy = new OrangeText[2];

	[SerializeField]
	private OrangeText[] textAtk = new OrangeText[2];

	[SerializeField]
	private OrangeText[] textHp = new OrangeText[2];

	[SerializeField]
	private OrangeText[] textDef = new OrangeText[2];

	[SerializeField]
	private OrangeText lvupText;

	[SerializeField]
	private UnityArmatureComponent armature;

	[BoxGroup("Sound")]
	[SerializeField]
	private SystemSE m_finishSE;

	[BoxGroup("Sound")]
	[SerializeField]
	private SystemSE m_levelUPSE;

	private bool isPlaying = true;

	public void Play()
	{
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_PROGRESS04_STOP);
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(m_levelUPSE);
		armature.AddEventListener("frameEvent", FrameEvent);
		armature.animation.Play(animationName1, 1);
		float[] rgb = new float[3]
		{
			lvupText.color.r,
			lvupText.color.g,
			lvupText.color.b
		};
		LeanTween.value(lvupText.gameObject, 0f, 1f, 0.3f).setOnUpdate(delegate(float alpha)
		{
			lvupText.color = new Color(rgb[0], rgb[1], rgb[2], alpha);
		});
		base._EscapeEvent = EscapeEvent.CLOSE_UI;
	}

	private void FrameEvent(string type, EventObject e)
	{
		if (e.name == event_move_top)
		{
			LeanTween.value(base.gameObject, 0f, bgY, 0.3f).setOnUpdate(delegate(float val)
			{
				rtBg.sizeDelta = new Vector2(1550f, val);
			}).setOnComplete(SetInfo);
		}
		else if (e.name == event_move_stop)
		{
			armature.animation.Play(animationName2, 0);
		}
	}

	private void SetInfo()
	{
		int lV = ManagedSingleton<PlayerHelper>.Instance.GetLV();
		EXP_TABLE eXP_TABLE = ManagedSingleton<OrangeDataManager>.Instance.EXP_TABLE_DICT[lV];
		EXP_TABLE eXP_TABLE2 = ManagedSingleton<OrangeDataManager>.Instance.EXP_TABLE_DICT[lV - 1];
		textLV[0].text = (lV - 1).ToString();
		textLV[1].text = lV.ToString();
		textEnergy[0].text = eXP_TABLE2.n_STAMINA.ToString();
		textEnergy[1].text = eXP_TABLE.n_STAMINA.ToString();
		textAtk[0].text = eXP_TABLE2.n_RANK_ATK.ToString();
		textAtk[1].text = eXP_TABLE.n_RANK_ATK.ToString();
		textHp[0].text = eXP_TABLE2.n_RANK_HP.ToString();
		textHp[1].text = eXP_TABLE.n_RANK_HP.ToString();
		textDef[0].text = eXP_TABLE2.n_RANK_DEF.ToString();
		textDef[1].text = eXP_TABLE.n_RANK_DEF.ToString();
		canvasGroupInfo.alpha = 1f;
		isPlaying = false;
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(m_finishSE);
	}

	public override void OnClickCloseBtn()
	{
		if (!isPlaying)
		{
			base.OnClickCloseBtn();
		}
	}
}
