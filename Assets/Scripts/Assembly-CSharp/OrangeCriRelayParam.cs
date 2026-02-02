using System.Collections.Generic;
using OrangeCriRelay;
using UnityEngine;

public class OrangeCriRelayParam : MonoBehaviour
{
	[SerializeField]
	private List<CharacterParam> listCharacterParam;

	public List<CharacterParam> ListCharacterParam
	{
		get
		{
			return listCharacterParam;
		}
	}

	public void UpdateParam(List<CharacterParam> newList)
	{
		listCharacterParam = newList;
	}
}
