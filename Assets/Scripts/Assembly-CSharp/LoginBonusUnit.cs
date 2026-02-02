using System;
using DragonBones;
using UnityEngine;
using UnityEngine.UI;

public class LoginBonusUnit : MonoBehaviour
{
	[SerializeField]
	private ItemIconWithAmount itemIcon;

	[SerializeField]
	private OrangeText textDay;

	[SerializeField]
	private Image imgBg;

	[SerializeField]
	private Image imgDayBg;

	[SerializeField]
	private Image stamp;

	[SerializeField]
	private Image imgPiece;

	[SerializeField]
	private Image imgCardType;

	public void Setup(int day, int itemId, int amount, bool isGot)
	{
		textDay.text = day.ToString();
		ITEM_TABLE itemTable = null;
		if (ManagedSingleton<OrangeTableHelper>.Instance.GetItem(itemId, out itemTable))
		{
			if (itemTable.n_TYPE == 5 && itemTable.n_TYPE_X == 1 && (int)itemTable.f_VALUE_Y > 0)
			{
				CARD_TABLE value = null;
				if (ManagedSingleton<OrangeDataManager>.Instance.CARD_TABLE_DICT.TryGetValue((int)itemTable.f_VALUE_Y, out value))
				{
					string p_bundleName = AssetBundleScriptableObject.Instance.m_iconCard + string.Format(AssetBundleScriptableObject.Instance.m_icon_card_s_format, value.n_PATCH);
					itemIcon.Setup(itemId, p_bundleName, value.s_ICON, OnClickItem);
					string cardTypeAssetName = ManagedSingleton<OrangeTableHelper>.Instance.GetCardTypeAssetName(value.n_TYPE);
					if (imgCardType != null)
					{
						imgCardType.sprite = MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssstSync<Sprite>(AssetBundleScriptableObject.Instance.m_texture_ui_common, cardTypeAssetName);
						imgCardType.color = Color.white;
					}
				}
			}
			else
			{
				itemIcon.Setup(0, AssetBundleScriptableObject.Instance.GetIconItem(itemTable.s_ICON), itemTable.s_ICON, delegate
				{
					MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_ItemInfo", delegate(ItemInfoUI ui)
					{
						ui.CanShowHow2Get = false;
						ui.Setup(itemTable);
					});
				});
				if (imgCardType != null)
				{
					imgCardType.color = Color.clear;
				}
			}
			itemIcon.SetRare(itemTable.n_RARE);
			itemIcon.SetAmount(amount);
			if (imgPiece != null)
			{
				imgPiece.color = ((itemTable.n_TYPE == 4) ? Color.white : Color.clear);
			}
		}
		if (isGot)
		{
			SetGotState();
		}
	}

	public void OverridBg(string sprName1, string sprName2)
	{
		Sprite assstSync = MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssstSync<Sprite>(AssetBundleScriptableObject.Instance.m_texture_ui_sub_common, sprName1);
		if (assstSync != null)
		{
			imgBg.sprite = assstSync;
		}
		assstSync = MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssstSync<Sprite>(AssetBundleScriptableObject.Instance.m_texture_ui_sub_common, sprName2);
		if (assstSync != null)
		{
			imgDayBg.sprite = assstSync;
		}
	}

	private void SetGotState()
	{
		stamp.GetComponent<RectTransform>().localScale = new Vector2(0.8f, 0.8f);
		stamp.color = Color.white;
	}

	public float PlayStampEft(UnityArmatureComponent exEft)
	{
		exEft.transform.SetParent(base.transform, false);
		exEft.transform.localScale = new Vector2(100f, 100f);
		float num = 0.3f;
		float timeB = 0.1f;
		LeanTween.color(stamp.rectTransform, Color.white, num);
		LeanTween.scale(stamp.rectTransform, new Vector2(0.8f, 0.8f), num).setEaseOutCubic().setOnComplete((Action)delegate
		{
			exEft._armature.animation.Play(null, 1);
			exEft.GetComponent<CanvasGroup>().alpha = 1f;
			LeanTween.moveLocalZ(base.gameObject, 20f, timeB).setLoopPingPong(1);
		});
		return num + timeB * 2f;
	}

	private void OnClickItem(int p_idx)
	{
		ITEM_TABLE item = null;
		if (!ManagedSingleton<OrangeTableHelper>.Instance.GetItem(p_idx, out item))
		{
			return;
		}
		MonoBehaviourSingleton<UIManager>.Instance.LoadUI("UI_ItemInfo", delegate(ItemInfoUI ui)
		{
			if (item.n_TYPE == 5 && item.n_TYPE_X == 1 && (int)item.f_VALUE_Y > 0)
			{
				CARD_TABLE value = null;
				if (ManagedSingleton<OrangeDataManager>.Instance.CARD_TABLE_DICT.TryGetValue((int)item.f_VALUE_Y, out value))
				{
					ui.CanShowHow2Get = false;
					ui.Setup(value, item);
				}
			}
			else
			{
				ui.CanShowHow2Get = false;
				ui.Setup(item);
			}
		});
	}
}
