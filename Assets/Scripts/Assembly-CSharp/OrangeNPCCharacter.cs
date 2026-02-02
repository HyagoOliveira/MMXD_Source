#define RELEASE
internal class OrangeNPCCharacter : OrangeCharacter
{
	public override void SetLocalPlayer(bool isLocal)
	{
		Debug.LogError("錯誤的嘗試切換OrangeNPCCharacter的LocalPlayer身分!!");
	}

	public override void DieFromServer()
	{
	}

	protected override void Initialize()
	{
		base.Initialize();
		bIsNpcCpy = true;
	}

	protected override void TriggerDeadAfterHurt(string killer)
	{
		BroadcastPlayerDead(killer);
		PlayerDead();
	}

	private void BroadcastPlayerDead(string killerName)
	{
		MonoBehaviourSingleton<OrangeBattleServerManager>.Instance.ShowPvpReport(killerName, sPlayerName, base.sPlayerID);
	}
}
