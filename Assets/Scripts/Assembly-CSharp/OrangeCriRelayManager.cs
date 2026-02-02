using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using OrangeCriRelay;
using UnityEngine;

public class OrangeCriRelayManager : MonoBehaviourSingleton<OrangeCriRelayManager>
{
	private bool initOK;

	[NonSerialized]
	private List<CharacterParam> listCharacterParam = new List<CharacterParam>();

	public bool InitOK
	{
		get
		{
			return initOK;
		}
	}

	private void Awake()
	{
		initOK = false;
	}

	public void Init()
	{
		Load();
	}

	public List<CharacterParam> GetCharacterUpdateParam(string Controller)
	{
		return (from x in listCharacterParam
			where x.Controller == Controller
			where x.AudioType != OrangeCriRelay.AudioType.TELEPORT
			where x.AudioType != OrangeCriRelay.AudioType.CHARGE_SHOT
			where x.MainStatus != OrangeCharacter.MainStatus.NONE
			where x.SubStatus != OrangeCharacter.SubStatus.NONE
			select x).ToList();
	}

	public CharacterParam GetCharacterTeleportParam(string Controller)
	{
		return listCharacterParam.Where((CharacterParam x) => x.Controller == Controller).FirstOrDefault((CharacterParam x) => x.AudioType == OrangeCriRelay.AudioType.TELEPORT);
	}

	public CharacterParam GetCharacterChargeShotParam(string Controller)
	{
		return listCharacterParam.Where((CharacterParam x) => x.Controller == Controller).FirstOrDefault((CharacterParam x) => x.AudioType == OrangeCriRelay.AudioType.CHARGE_SHOT);
	}

	public CharacterParam GetCharacterCallPetParam(string Controller)
	{
		return (from x in listCharacterParam
			where x.Controller == Controller
			where x.PetSE != null
			select x).FirstOrDefault((CharacterParam x) => x.AudioType == OrangeCriRelay.AudioType.CALL_PET);
	}

	public void Load()
	{
		string loadPath = RelayDataPath.GetLoadPath();
		MonoBehaviourSingleton<OrangeWebRequestLoad>.Instance.Load(OrangeWebRequestLoad.LoadType.UNIQUE, loadPath, OrangeWebRequestLoad.LoadingFlg.TEXT_DEFAULT, delegate(byte[] param0, string param1)
		{
			initOK = true;
			if (param0 == null || param0.Length == 0)
			{
				listCharacterParam = new List<CharacterParam>();
			}
			else
			{
				string @string = Encoding.UTF8.GetString(param0);
				Deserialize(@string, out listCharacterParam);
			}
		});
	}

	private bool Deserialize(string text, out List<CharacterParam> listCharacterParam)
	{
		List<CharacterParam> list = JsonConvert.DeserializeObject<List<CharacterParam>>(AesCrypto.Decode(text));
		if (list != null && list.Count > 0)
		{
			for (int j = 0; j < list.Count; j++)
			{
				list[j].UpdateStatusByStr();
			}
			OrangeCriRelayParam[] componentsInChildren = GetComponentsInChildren<OrangeCriRelayParam>(true);
			for (int k = 0; k < componentsInChildren.Length; k++)
			{
				UnityEngine.Object.DestroyImmediate(componentsInChildren[k]);
			}
			string[] controllerName = list.Select((CharacterParam x) => x.Controller).Distinct().ToArray();
			int i;
			for (i = 0; i < controllerName.Length; i++)
			{
				IEnumerable<CharacterParam> source = list.Where((CharacterParam x) => x.Controller == controllerName[i]);
				base.gameObject.AddComponent<OrangeCriRelayParam>().UpdateParam(source.ToList());
			}
			listCharacterParam = list;
			return true;
		}
		listCharacterParam = new List<CharacterParam>();
		return false;
	}
}
