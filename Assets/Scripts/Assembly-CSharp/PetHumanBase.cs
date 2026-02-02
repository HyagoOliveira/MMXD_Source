public class PetHumanBase
{
	public enum PetAnimateId : uint
	{
		ANI_STAND = 0u,
		ANI_TELEPORT_IN_POSE = 1u,
		ANI_WIN_POSE = 2u,
		ANI_TELEPORT_OUT_POSE = 3u,
		ANI_SKILL_START = 4u,
		ANI_SKILL_END = 34u,
		ANI_BTSKILL_START = 35u,
		ANI_BTSKILL_END = 50u,
		MAX_ANI = 51u
	}

	public enum MainStatus
	{
		NONE = -1,
		IDLE = 0,
		MAX_STATUS = 1
	}

	public enum SubStatus
	{
		NONE = -1,
		IDLE = 0,
		SKILL0 = 1,
		SKILL1 = 31
	}

	public enum FollowType
	{
		NONE = -1,
		Aerial = 0,
		Ground = 1
	}
}
