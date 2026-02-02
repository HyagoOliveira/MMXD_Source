using UnityEngine;

public class TramSE : MonoBehaviour
{
	private Renderer meshRenderer;

	private bool isPlaySe;

	private void Awake()
	{
		meshRenderer = OrangeGameUtility.AddOrGetRenderer<MeshRenderer>(base.gameObject);
	}

	public void PlayTramSE()
	{
		MonoBehaviourSingleton<AudioManager>.Instance.PlayBattleSE(BattleSE.CRI_BATTLESE_BT_TRAM01_LP);
		isPlaySe = true;
	}

	private void Update()
	{
		if ((bool)meshRenderer && !meshRenderer.isVisible && isPlaySe)
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlayBattleSE(BattleSE.CRI_BATTLESE_BT_TRAM01_STOP);
			isPlaySe = false;
		}
	}
}
