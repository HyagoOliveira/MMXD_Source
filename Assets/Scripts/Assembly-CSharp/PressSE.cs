using UnityEngine;

public class PressSE : MonoBehaviour
{
	protected bool visible;

	private bool IsLoopSEPPlayed;

	private Renderer meshRenderer;

	private void Awake()
	{
		meshRenderer = OrangeGameUtility.AddOrGetRenderer<MeshRenderer>(base.gameObject);
	}

	public void PlayPress_Down_SE()
	{
		PlayBattleSE(BattleSE.CRI_BATTLESE_BT_PRESS01);
	}

	public void PlayPress_DownEnd_SE()
	{
		PlayBattleSE(BattleSE.CRI_BATTLESE_BT_PRESS02);
	}

	public void PlayPress_Up_SE()
	{
		PlayBattleSE(BattleSE.CRI_BATTLESE_BT_PRESS03_LP);
		if (visible)
		{
			IsLoopSEPPlayed = true;
		}
	}

	public void PlayPress_UpEnd_SE()
	{
		if (IsLoopSEPPlayed)
		{
			PlayBattleSE(BattleSE.CRI_BATTLESE_BT_PRESS03_STOP, true);
			IsLoopSEPPlayed = false;
		}
		else
		{
			PlayBattleSE(BattleSE.CRI_BATTLESE_BT_PRESS03_STOP);
		}
	}

	private void Update()
	{
	}

	protected void OnDestroy()
	{
		PlayBattleSE(BattleSE.CRI_BATTLESE_BT_PRESS03_STOP, true);
	}

	public void PlayBattleSE(BattleSE cueId, bool ForceTrigger = false)
	{
		if (!MonoBehaviourSingleton<UIManager>.Instance.IsLoading && (visible || ForceTrigger))
		{
			MonoBehaviourSingleton<AudioManager>.Instance.PlayBattleSE(cueId);
		}
	}

	private void OnBecameVisible()
	{
		visible = true;
	}

	private void OnBecameInvisible()
	{
		visible = false;
	}
}
