using CodeStage.AntiCheat.Detectors;
using UnityEngine;

public class ACTkManager : MonoBehaviourSingleton<ACTkManager>
{
	private void Start()
	{
		ObscuredCheatingDetector.StartDetection(OnDetected);
	}

	private void OnDetected()
	{
		ManagedSingleton<PlayerNetManager>.Instance.RetrieveResetTimeReq(delegate
		{
			Application.Quit();
		}, 1);
	}

	public void SetDetected()
	{
		OnDetected();
	}
}
