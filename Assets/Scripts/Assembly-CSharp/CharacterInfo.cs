using System.Collections.Generic;
using Better;
using enums;

public class CharacterInfo
{
	public NetCharacterInfo netInfo;

	public System.Collections.Generic.Dictionary<CharacterSkillSlot, NetCharacterSkillInfo> netSkillDic = new Better.Dictionary<CharacterSkillSlot, NetCharacterSkillInfo>();

	public List<int> netSkinList = new List<int>();

	public System.Collections.Generic.Dictionary<int, NetCharacterDNAInfo> netDNAInfoDic = new Better.Dictionary<int, NetCharacterDNAInfo>();

	public NetCharacterDNALinkInfo netDNALinkInfo;
}
