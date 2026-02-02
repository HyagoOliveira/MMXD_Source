using UnityEngine;
using UnityEngine.UI;

public class HexPlayerCell : MonoBehaviour
{
	[SerializeField]
	private RectTransform rect;

	[SerializeField]
	private Image imgframe;

	[SerializeField]
	private PlayerIconBase playerIcon;

	public RectTransform Rect
	{
		get
		{
			return rect;
		}
	}

	public string PlayerId { get; private set; }

	public void Setup(bool enableFrame, string p_playerId, int characterID, Transform uiCanvans)
	{
		rect.SetParent(uiCanvans, false);
		rect.anchoredPosition = new Vector2(base.transform.position.x, base.transform.position.z);
		PlayerId = p_playerId;
		imgframe.color = (enableFrame ? Color.white : Color.clear);
		CHARACTER_TABLE value;
		if (ManagedSingleton<OrangeDataManager>.Instance.CHARACTER_TABLE_DICT.TryGetValue(characterID, out value))
		{
			playerIcon.SetupByUnlockID(value.n_UNLOCK_ID);
		}
		else
		{
			playerIcon.SetupByUnlockID(-1);
		}
	}

	public void Move(HexCell nextCell)
	{
		Vector3 position = HexMetrics.GetPosition(nextCell.CoordinateInfo.X, nextCell.CoordinateInfo.Y);
		base.transform.localPosition = position;
		rect.anchoredPosition = new Vector2(base.transform.position.x, base.transform.position.z);
	}

	public void SetGlow()
	{
		Image glow1 = Object.Instantiate(imgframe, imgframe.transform.parent);
		Image glow2 = Object.Instantiate(imgframe, imgframe.transform.parent);
		glow1.color = Color.clear;
		glow2.color = Color.clear;
		LeanTween.scale(glow1.gameObject, new Vector3(2f, 2f), 4f).setLoopClamp();
		LeanTween.value(glow1.gameObject, 1f, 0f, 4f).setOnUpdate(delegate(float alpha)
		{
			glow1.color = new Color(1f, 1f, 1f, alpha);
		}).setLoopClamp();
		LeanTween.scale(glow2.gameObject, new Vector3(2f, 2f), 4f).setLoopClamp().setDelay(2f);
		LeanTween.value(glow2.gameObject, 1f, 0f, 4f).setOnUpdate(delegate(float alpha)
		{
			glow2.color = new Color(1f, 1f, 1f, alpha);
		}).setLoopClamp()
			.setDelay(2f);
	}

	public void Resize(Vector3 size)
	{
		if (Rect.childCount > 0)
		{
			rect.GetChild(0).localScale = size;
		}
	}
}
