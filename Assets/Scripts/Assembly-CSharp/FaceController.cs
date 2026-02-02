using UnityEngine;
using UnityEngine.UI;

public class FaceController : MonoBehaviour
{
	private bool isFaceChange;

	[SerializeField]
	private Image imageEye;

	[SerializeField]
	private Sprite[] sprEyes;

	private void Awake()
	{
		isFaceChange = imageEye != null && sprEyes.Length != 0;
	}

	public void UpdateState(int pEyes)
	{
		if (isFaceChange)
		{
			imageEye.sprite = sprEyes[pEyes];
		}
	}
}
