using UnityEngine;
using UnityEngine.UI;

public class GachaUIPageDot : MonoBehaviour
{
	[SerializeField]
	private Image imgActive;

	private int dotPage = -1;

	public void Setup(int p_dotPage)
	{
		dotPage = p_dotPage;
	}

	public void SetNowPage(int p_nowPage)
	{
		imgActive.color = ((dotPage == p_nowPage) ? Color.white : Color.clear);
	}
}
