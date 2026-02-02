using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(InputField))]
public class OrangeInputFieldSoundHelper : MonoBehaviour, IPointerClickHandler, IEventSystemHandler
{
	[SerializeField]
	public bool PointClickSE;

	public void OnPointerClick(PointerEventData eventData)
	{
		if (eventData.button == PointerEventData.InputButton.Left && PointClickSE)
		{
			PlaySE("SystemSE,sys_ok01");
		}
	}

	public void PlaySE(string strCue)
	{
		string[] array = strCue.Split(',');
		if (array.Length == 2)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.Play(array[0], array[1]);
		}
	}
}
