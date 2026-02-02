public abstract class CharacterControllerProxyBaseGen4 : CharacterControllerProxyBaseGen3
{
	public override void PlayerPressSkillCharacterCall(int skillID)
	{
		OnPlayerPressSkillCharacterCall((SkillID)skillID);
	}

	protected sealed override bool CanPlayerPressSkill(int skillID)
	{
		return true;
	}
}
