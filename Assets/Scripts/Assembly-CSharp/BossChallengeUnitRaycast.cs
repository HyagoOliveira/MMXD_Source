using UnityEngine;
using UnityEngine.UI;

public class BossChallengeUnitRaycast : MonoBehaviour
{
	[SerializeField]
	private Image image;

	private void Awake()
	{
		image.alphaHitTestMinimumThreshold = 0.1f;
	}
}
