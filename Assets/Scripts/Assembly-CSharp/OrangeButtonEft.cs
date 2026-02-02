using UnityEngine;
using UnityEngine.EventSystems;

public class OrangeButtonEft : MonoBehaviour, IPointerClickHandler, IEventSystemHandler
{
	private Vector3 defaultScale;

	private void Awake()
	{
		defaultScale = base.transform.localScale;
	}

	public void OnPointerClick(PointerEventData pointerEventData)
	{
		if (pointerEventData.button == PointerEventData.InputButton.Left)
		{
			base.transform.localScale = defaultScale;
			if (!LeanTween.isTweening(base.gameObject))
			{
				LeanTween.scale(base.gameObject, new Vector2(1.15f, 1.15f), 0.08f).setEasePunch();
			}
		}
	}
}
