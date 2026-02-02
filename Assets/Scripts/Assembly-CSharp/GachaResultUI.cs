using System;
using System.Collections;
using CallbackDefs;
using Coffee.UIExtensions;
using OrangeAudio;
using UnityEngine;
using UnityEngine.UI;
using enums;

public class GachaResultUI : OrangeUIBase
{
	private readonly string gachaRareBgPath = "texture/2d/icon/gachararebg";

	private readonly string gachaRareBgName = "icon_ch2_BG_{0}";

	[SerializeField]
	private Sprite[] sprRare;

	[SerializeField]
	private Image imgBgRare;

	[SerializeField]
	private OrangeText textName;

	[SerializeField]
	private Image imgRare;

	[SerializeField]
	private RawImage renderModel;

	[SerializeField]
	private GachaResultLight gachaResultLight;

	[SerializeField]
	private Transform stParent;

	private bool isShowComplete;

	private RenderTextureObj textureObj;

	[SerializeField]
	private ParticleSystem imgRareFx;

	[SerializeField]
	private UITransitionEffect transitionEffect;

	[SerializeField]
	private Canvas canvasBoard;

	[SerializeField]
	private Image imgSkl01;

	[SerializeField]
	private Image imgSkl02;

	[SerializeField]
	private Image imgCharcutBg;

	[SerializeField]
	private Image imgCharcut;

	[SerializeField]
	private Image imgNew;

	public bool isLastOne;

	protected override void Awake()
	{
		base.Awake();
		canvasBoard.enabled = false;
		Color clear = Color.clear;
		imgSkl01.color = clear;
		imgSkl02.color = clear;
		imgCharcutBg.color = clear;
		imgCharcut.color = clear;
		transitionEffect.effectFactor = 1f;
		float renderTextureRate = MonoBehaviourSingleton<OrangeGameManager>.Instance.GetRenderTextureRate();
		renderModel.transform.localScale = new Vector3(renderTextureRate, renderTextureRate, 1f);
	}

	public void Setup(NetRewardInfo netRewardInfo, bool isNew = false)
	{
		MonoBehaviourSingleton<OrangeSceneManager>.Instance.MainCamera.allowHDR = false;
		switch (netRewardInfo.RewardType)
		{
		case 3:
		{
			CHARACTER_TABLE table = ManagedSingleton<OrangeDataManager>.Instance.CHARACTER_TABLE_DICT[netRewardInfo.RewardID];
			string text = "st_" + table.s_ICON;
			renderModel.rectTransform.sizeDelta = new Vector2(1280f, 1280f);
			LoadSklIcon(table.n_SKILL1, imgSkl01);
			LoadSklIcon(table.n_SKILL2, imgSkl02);
			LoadCutInIconBg(table.n_RARITY, imgCharcutBg);
			LoadCutInIcon(table.s_ICON, imgCharcut);
			imgBgRare.sprite = sprRare[GetBgRareIdx(table.n_RARITY)];
			if (Background != null)
			{
				Background.GetComponent<Image>().sprite = sprRare[GetBgRareIdx(table.n_RARITY)];
			}
			MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad("prefab/RenderTextureObj", "RenderTextureObj", delegate(UnityEngine.Object model)
			{
				MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(string.Format(AssetBundleScriptableObject.Instance.m_dragonbones_chdb, table.s_ICON), table.s_ICON + "_db", delegate(GameObject objDb)
				{
					UnityEngine.Object.Instantiate(objDb, stParent, false);
					LeanTween.value(base.gameObject, 1f, 0.2f, 0.5f).setOnUpdate(delegate(float val)
					{
						transitionEffect.effectFactor = val;
					}).setDelay(0.2f)
						.setOnComplete((Action)delegate
						{
							string voiceID = AudioLib.GetVoice(ref table);
							MonoBehaviourSingleton<AudioManager>.Instance.PreloadAtomSource(voiceID, 3, delegate
							{
								MonoBehaviourSingleton<AudioManager>.Instance.Play(voiceID, 2);
							});
							MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_ShinyEffect", delegate(ShinyEffectUI ui)
							{
								ui.closeCB = (Callback)Delegate.Combine(ui.closeCB, (Callback)delegate
								{
									transitionEffect.effectFactor = 0f;
									stParent.GetComponent<Canvas>().enabled = false;
									StartCoroutine(PlayModel(model, table));
									canvasBoard.enabled = true;
									textName.text = ManagedSingleton<OrangeTextDataManager>.Instance.CHARATEXT_TABLE_DICT.GetL10nValue(table.w_NAME);
									SetRareFx(table.n_RARITY);
									AnimationGroup[1].PlayAnimation(delegate
									{
										imgRareFx.Play(true);
										imgNew.color = (isNew ? Color.white : Color.clear);
										isShowComplete = true;
									});
									MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_GACHA05);
								});
								ui.SetWhite(0.3f, 2f, LeanTweenType.easeInCubic);
							});
						});
				});
			});
			break;
		}
		case 2:
		{
			canvasBoard.enabled = false;
			transitionEffect.effectFactor = 0f;
			stParent.GetComponent<Canvas>().enabled = false;
			WEAPON_TABLE table2 = ManagedSingleton<OrangeDataManager>.Instance.WEAPON_TABLE_DICT[netRewardInfo.RewardID];
			imgBgRare.sprite = sprRare[GetBgRareIdx(table2.n_RARITY)];
			if (Background != null)
			{
				Background.GetComponent<Image>().sprite = sprRare[GetBgRareIdx(table2.n_RARITY)];
			}
			renderModel.rectTransform.sizeDelta = new Vector2(1280f, 720f);
			MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad("prefab/RenderTextureObj", "RenderTextureObj", delegate(GameObject obj)
			{
				textureObj = UnityEngine.Object.Instantiate(obj, Vector3.zero, Quaternion.identity).GetComponent<RenderTextureObj>();
				textureObj.AssignNewWeaponRender(table2, new Vector3(0f, 0.02f, 2f), renderModel);
				AnimationGroup[0].PlayAnimation(delegate
				{
					imgRareFx.Play(true);
					imgNew.color = (isNew ? Color.white : Color.clear);
				});
				textName.text = ManagedSingleton<OrangeTextDataManager>.Instance.WEAPONTEXT_TABLE_DICT.GetL10nValue(table2.w_NAME);
				SetRareFx(table2.n_RARITY);
				LeanTween.value(1f, 0f, 2f).setOnComplete((Action)delegate
				{
					isShowComplete = true;
				});
			});
			break;
		}
		default:
			isShowComplete = true;
			break;
		}
		MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_GACHA04);
	}

	private void SetRareFx(int rarity)
	{
		string iconRareWord = AssetBundleScriptableObject.Instance.GetIconRareWord(rarity);
		MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(AssetBundleScriptableObject.Instance.m_texture_ui_common, iconRareWord, delegate(Sprite spr)
		{
			if (!(null == imgRare))
			{
				imgRare.sprite = spr;
				imgRare.color = Color.white;
			}
		});
		gachaResultLight.Setup(rarity);
	}

	private int GetBgRareIdx(int rarity)
	{
		switch ((ItemRarity)(short)rarity)
		{
		case ItemRarity.S:
		case ItemRarity.SS:
			return 2;
		case ItemRarity.A:
			return 1;
		default:
			return 0;
		}
	}

	private void LoadSklIcon(int sklId, Image img)
	{
		SKILL_TABLE value = null;
		if (!ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT.TryGetValue(sklId, out value))
		{
			return;
		}
		MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(AssetBundleScriptableObject.Instance.GetIconSkill(value.s_ICON), value.s_ICON, delegate(Sprite spr)
		{
			if (spr != null)
			{
				img.sprite = spr;
				img.color = Color.white;
			}
			else
			{
				img.color = Color.clear;
			}
		});
	}

	private void LoadCutInIconBg(int rare, Image img)
	{
		string assetName = string.Format(gachaRareBgName, AssetBundleScriptableObject.Instance.RareIntToStr(rare));
		MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(gachaRareBgPath, assetName, delegate(Sprite spr)
		{
			if (spr != null)
			{
				img.sprite = spr;
				img.color = Color.white;
			}
			else
			{
				img.color = Color.clear;
			}
		});
	}

	private void LoadCutInIcon(string iconName, Image img)
	{
		MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(AssetBundleScriptableObject.Instance.GetIconCharacter2("icon_" + iconName), "icon_" + iconName, delegate(Sprite spr)
		{
			if (spr != null)
			{
				img.sprite = spr;
				img.color = Color.white;
			}
			else
			{
				img.color = Color.clear;
			}
		});
	}

	public override void OnClickCloseBtn()
	{
		if (!isShowComplete)
		{
			return;
		}
		renderModel.color = Color.clear;
		if (null != textureObj)
		{
			UnityEngine.Object.Destroy(textureObj.gameObject);
			MonoBehaviourSingleton<AudioManager>.Instance.Stop(AudioChannelType.Sound);
			MonoBehaviourSingleton<AudioManager>.Instance.Stop(AudioChannelType.Voice);
		}
		if (!isLastOne)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlaySystemSE(SystemSE.CRI_SYSTEMSE_SYS_CURSOR06);
		}
		MonoBehaviourSingleton<OrangeSceneManager>.Instance.MainCamera.allowHDR = true;
		GachaSkipUI uI = MonoBehaviourSingleton<UIManager>.Instance.GetUI<GachaSkipUI>("UI_GachaSkip");
		if (uI != null)
		{
			Camera componentInChildren = uI.GetComponentInChildren<Camera>();
			if ((bool)componentInChildren)
			{
				componentInChildren.allowHDR = false;
			}
		}
		base.OnClickCloseBtn();
	}

	private IEnumerator PlayModel(UnityEngine.Object model, CHARACTER_TABLE table)
	{
		textureObj = UnityEngine.Object.Instantiate((GameObject)model, Vector3.zero, Quaternion.identity).GetComponent<RenderTextureObj>();
		textureObj.OnlyDebut = true;
		yield return new WaitForSeconds(0.5f);
		textureObj.AssignNewRender(table, null, null, new Vector3(0f, -0.71f, 4.29f), renderModel);
	}
}
