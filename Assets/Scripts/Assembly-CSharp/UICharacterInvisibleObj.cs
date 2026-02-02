using UnityEngine;

public class UICharacterInvisibleObj : MonoBehaviour
{
	[SerializeField]
	private GameObject[] invisibleObjs;

	public void Awake()
	{
		GameObject[] array = invisibleObjs;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetActive(false);
		}
	}
}
