using UnityEngine;

public class GateSE : MonoBehaviour
{
	private OrangeCharacter oc;

	public bool noPlaySE;

	[SerializeField]
	public OrangeCriSource SoundSource
	{
		get
		{
			return base.gameObject.AddOrGetComponent<OrangeCriSource>();
		}
	}

	public void PlayBossGate_Unlock_SE()
	{
		if (!noPlaySE)
		{
			PlaySE(BattleSE.CRI_BATTLESE_BT_BOSS_DOOR_UNLOCK);
		}
	}

	public void PlayBossGate_Open_SE()
	{
		if (!noPlaySE)
		{
			PlaySE(BattleSE.CRI_BATTLESE_BT_BOSS_DOOR_OPEN);
		}
	}

	public void PlayBossGate_Close_SE()
	{
		if (!noPlaySE)
		{
			PlaySE(BattleSE.CRI_BATTLESE_BT_BOSS_DOOR_CLOSE);
		}
	}

	private void PlaySE(BattleSE se)
	{
		SoundSource.PlaySE("BattleSE", (int)se);
	}
}
