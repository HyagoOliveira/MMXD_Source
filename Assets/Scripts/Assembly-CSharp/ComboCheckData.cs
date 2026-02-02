public class ComboCheckData
{
	public int nTriggerSkillID;

	public int nComboSkillID;

	public ComboCheckBuff[] ComboCheckBuffs;

	public bool CheckHasAllBuff(PerBuffManager tPerBuffManager)
	{
		for (int i = 0; i < ComboCheckBuffs.Length; i++)
		{
			if (!tPerBuffManager.CheckHasEffectByCONDITIONID(ComboCheckBuffs[i].nBuffID, ComboCheckBuffs[i].nBuffCount))
			{
				return false;
			}
		}
		return true;
	}

	public void RemoveComboBuff(PerBuffManager tPerBuffManager)
	{
		for (int i = 0; i < ComboCheckBuffs.Length; i++)
		{
			tPerBuffManager.RemoveBuffByCONDITIONID(ComboCheckBuffs[i].nBuffID);
		}
	}

	public void AddComboBuff(PerBuffManager tPerBuffManager)
	{
		for (int i = 0; i < ComboCheckBuffs.Length; i++)
		{
			tPerBuffManager.AddBuff(ComboCheckBuffs[i].nBuffID, 0, 0, 0);
		}
	}
}
