using System.Collections.Generic;
using System.Linq;
using DragonBones;
using UnityEngine;

public class ReplaceSkin : BaseDemo
{
	private UnityArmatureComponent _bodyArmatureComp;

	private int _replaceSuitIndex;

	private Dictionary<string, List<string>> _suitConfigs = new Dictionary<string, List<string>>();

	private readonly List<string> _replaceSuitParts = new List<string>();

	private void Awake()
	{
		_bodyArmatureComp = null;
		_replaceSuitIndex = 0;
		_suitConfigs.Clear();
		_replaceSuitParts.Clear();
		List<string> list = new List<string>();
		list.Add("2010600a");
		list.Add("2010600a_1");
		list.Add("20208003");
		list.Add("20208003_1");
		list.Add("20208003_2");
		list.Add("20208003_3");
		list.Add("20405006");
		list.Add("20509005");
		list.Add("20703016");
		list.Add("20703016_1");
		list.Add("2080100c");
		list.Add("2080100e");
		list.Add("2080100e_1");
		list.Add("20803005");
		list.Add("2080500b");
		list.Add("2080500b_1");
		_suitConfigs.Add("suit1", list);
		List<string> list2 = new List<string>();
		list2.Add("20106010");
		list2.Add("20106010_1");
		list2.Add("20208006");
		list2.Add("20208006_1");
		list2.Add("20208006_2");
		list2.Add("20208006_3");
		list2.Add("2040600b");
		list2.Add("2040600b_1");
		list2.Add("20509007");
		list2.Add("20703020");
		list2.Add("20703020_1");
		list2.Add("2080b003");
		list2.Add("20801015");
		_suitConfigs.Add("suit2", list2);
	}

	protected override void OnStart()
	{
		_LoadData("you_xin/body/body_ske", "you_xin/body/body_tex");
		string text = "";
		string text2 = "";
		foreach (string key in _suitConfigs.Keys)
		{
			foreach (string item in _suitConfigs[key])
			{
				text = "you_xin/" + key + "/" + item + "/" + item + "_ske";
				text2 = "you_xin/" + key + "/" + item + "/" + item + "_tex";
				_LoadData(text, text2);
			}
		}
		_bodyArmatureComp = UnityFactory.factory.BuildArmatureComponent("body");
		_bodyArmatureComp.CloseCombineMeshs();
		_bodyArmatureComp.transform.localScale = new Vector3(0.5f, 0.5f, 1f);
		_bodyArmatureComp.transform.localPosition = new Vector3(0f, -4f, 0f);
		_bodyArmatureComp.AddDBEventListener("loopComplete", _OnFrameEventHandler);
		_bodyArmatureComp.animation.Play("idle", 0);
		int num = 0;
		string[] array = _suitConfigs.Keys.ToArray();
		foreach (string item2 in _suitConfigs[array[num]])
		{
			ArmatureData armatureData = UnityFactory.factory.GetArmatureData(item2);
			UnityFactory.factory.ReplaceSkin(_bodyArmatureComp.armature, armatureData.defaultSkin);
		}
		num = (_replaceSuitIndex = num + 1);
		_replaceSuitParts.AddRange(_suitConfigs[array[num]]);
	}

	protected override void OnUpdate()
	{
		if (Input.GetMouseButtonDown(0))
		{
			RandomReplaceSkin();
		}
	}

	private void _LoadData(string dragonBonesJSONPath, string textureAtlasJSONPath)
	{
		UnityFactory.factory.LoadDragonBonesData(dragonBonesJSONPath);
		UnityFactory.factory.LoadTextureAtlasData(textureAtlasJSONPath);
	}

	private void _OnFrameEventHandler(string type, EventObject eventObject)
	{
		if (type == "loopComplete")
		{
			int index = Random.Range(0, _bodyArmatureComp.animation.animationNames.Count);
			string animationName = _bodyArmatureComp.animation.animationNames[index];
			_bodyArmatureComp.animation.FadeIn(animationName, 0.3f, 0);
		}
	}

	private void RandomReplaceSkin()
	{
		if (_replaceSuitParts.Count == 0)
		{
			_replaceSuitIndex++;
			string[] array = _suitConfigs.Keys.ToArray();
			if (_replaceSuitIndex >= array.Length)
			{
				_replaceSuitIndex = 0;
			}
			_replaceSuitParts.AddRange(_suitConfigs[array[_replaceSuitIndex]]);
		}
		int index = Random.Range(0, _replaceSuitParts.Count);
		string text = _replaceSuitParts[index];
		ArmatureData armatureData = UnityFactory.factory.GetArmatureData(text);
		UnityFactory.factory.ReplaceSkin(_bodyArmatureComp.armature, armatureData.defaultSkin);
		_replaceSuitParts.RemoveAt(index);
	}
}
