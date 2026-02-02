using UnityEngine;
using UnityEngine.UI;

public class HexUICell : MonoBehaviour
{
	public RectTransform rectTransform;

	[SerializeField]
	private Text textRewardAmount;

	[SerializeField]
	private Image imgBg;

	[SerializeField]
	private Image imgRewardBg;

	[SerializeField]
	private Image imgReward;

	[SerializeField]
	private Image imgMaskMoveable;

	[SerializeField]
	private Image imgMaskFinished;

	[SerializeField]
	private Image imgTempMove;

	[SerializeField]
	private RectTransform playerInfoRect;

	private int cacheRewardId = -1;

	private RectTransform rectPlayer;

	public Image ImgBg
	{
		get
		{
			return imgBg;
		}
	}

	public void UpdateFlag(HexCell.CellStatus status)
	{
		LeanTween.cancel(imgMaskMoveable.gameObject);
		LeanTween.cancel(imgMaskFinished.gameObject);
		if (status.HasFlag(HexCell.CellStatus.Available))
		{
			imgBg.color = Color.white;
			if (status.HasFlag(HexCell.CellStatus.CurrentPoint))
			{
				imgMaskMoveable.color = Color.clear;
				imgMaskFinished.color = Color.clear;
				ChkFinished(status);
			}
			else if (status.HasFlag(HexCell.CellStatus.Moveable))
			{
				TweenColor(imgMaskMoveable, Color.white);
				TweenColor(imgMaskFinished, Color.clear);
				ChkFinished(status);
			}
			else if (ChkFinished(status))
			{
				imgMaskFinished.color = Color.white;
				imgMaskMoveable.color = Color.clear;
			}
			else
			{
				imgMaskFinished.color = Color.clear;
				imgMaskMoveable.color = Color.clear;
			}
		}
		else
		{
			Clear();
		}
	}

	public void UpdateTempMove(HexCell.CellStatus status)
	{
		if (status.HasFlag(HexCell.CellStatus.TempMove))
		{
			imgTempMove.color = Color.white;
		}
		else
		{
			imgTempMove.color = Color.clear;
		}
	}

	private void TweenColor(Image img, Color targetColor)
	{
		Color color = img.color;
		if (!(color == targetColor))
		{
			LeanTween.value(img.gameObject, delegate(Color c)
			{
				img.color = c;
			}, color, targetColor, 0.2f);
		}
	}

	private bool ChkFinished(HexCell.CellStatus status)
	{
		bool num = status.HasFlag(HexCell.CellStatus.Finished);
		if (num)
		{
			ClearReweard();
			textRewardAmount.text = string.Empty;
			ClearHoldPlayer();
		}
		return num;
	}

	public void SetReward(GACHA_TABLE gachaTable)
	{
		if (gachaTable == null)
		{
			ClearReweard();
		}
		else
		{
			if (cacheRewardId == gachaTable.n_ID)
			{
				return;
			}
			cacheRewardId = gachaTable.n_ID;
			textRewardAmount.text = string.Format("x{0}", gachaTable.n_AMOUNT_MAX);
			string bundlePath;
			string assetPath;
			if (!MonoBehaviourSingleton<OrangeGameManager>.Instance.GetRewardSpritePath(gachaTable, out bundlePath, out assetPath))
			{
				ClearReweard();
				return;
			}
			MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad(bundlePath, assetPath, delegate(Sprite spr)
			{
				if ((bool)imgReward)
				{
					imgRewardBg.color = Color.black;
					imgReward.sprite = spr;
					imgReward.color = Color.white;
				}
			});
		}
	}

	private void ClearReweard()
	{
		imgRewardBg.color = Color.clear;
		imgReward.color = Color.clear;
	}

	private void Clear()
	{
		Color clear = Color.clear;
		imgBg.color = clear;
		imgMaskMoveable.color = clear;
		imgMaskFinished.color = clear;
		imgRewardBg.color = clear;
		imgReward.color = clear;
		imgTempMove.color = clear;
		textRewardAmount.text = string.Empty;
	}

	public void AddPlayerInfo(RectTransform rectTransform)
	{
		ClearHoldPlayer();
		rectTransform.localScale = Vector3.one;
		rectTransform.SetParent(playerInfoRect, false);
		rectPlayer = rectTransform;
	}

	private void ClearHoldPlayer()
	{
		if (rectPlayer != null)
		{
			rectPlayer.gameObject.SetActive(false);
			rectPlayer = null;
		}
	}
}
