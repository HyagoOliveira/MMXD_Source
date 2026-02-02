using System;
using Newtonsoft.Json;
using UnityEngine;

namespace OrangeCriRelay
{
	[Serializable]
	public class CharacterParam
	{
		public string Controller = "";

		public AudioType AudioType = AudioType.SKILL;

		public string MainStatusString = "SKILL";

		public string SubStatusString = "SKILL0";

		public string CueName;

		public float Delay;

		public string ChargeSE_localPlayerString = string.Empty;

		public string ChargeSE_otherPlayerString = string.Empty;

		[JsonIgnore]
		[HideInInspector]
		public OrangeCharacter.MainStatus MainStatus = OrangeCharacter.MainStatus.NONE;

		[JsonIgnore]
		[HideInInspector]
		public OrangeCharacter.SubStatus SubStatus = OrangeCharacter.SubStatus.NONE;

		[JsonIgnore]
		public string[] ChargeSE_localPlayer;

		[JsonIgnore]
		public string[] ChargeSE_otherPlayer;

		[JsonIgnore]
		public string[] PetSE;

		public void UpdateStatusByStr()
		{
			MainStatus = OrangeCharacter.MainStatus.NONE;
			SubStatus = OrangeCharacter.SubStatus.NONE;
			Enum.TryParse<OrangeCharacter.MainStatus>(MainStatusString, out MainStatus);
			Enum.TryParse<OrangeCharacter.SubStatus>(SubStatusString, out SubStatus);
			if (!string.IsNullOrEmpty(ChargeSE_localPlayerString))
			{
				ChargeSE_localPlayer = ChargeSE_localPlayerString.Split(',');
			}
			if (!string.IsNullOrEmpty(ChargeSE_otherPlayerString))
			{
				ChargeSE_otherPlayer = ChargeSE_otherPlayerString.Split(',');
			}
			if (AudioType == AudioType.CALL_PET)
			{
				if (CueName.Split(',').Length == 2)
				{
					PetSE = CueName.Split(',');
				}
				else
				{
					PetSE = null;
				}
			}
			else
			{
				PetSE = null;
			}
		}
	}
}
