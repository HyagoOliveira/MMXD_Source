#define RELEASE
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace StageLib
{
	[ExecuteInEditMode]
	public class StageOneWorkEvent : EventPointBase
	{
		public class StageOneWorkSL
		{
			public int mapEvent;

			[JsonConverter(typeof(FloatConverter))]
			public float B2DX;

			[JsonConverter(typeof(FloatConverter))]
			public float B2DY;

			[JsonConverter(typeof(FloatConverter))]
			public float B2DW = 1f;

			[JsonConverter(typeof(FloatConverter))]
			public float B2DH = 1f;

			public string param_save = "";

			public List<StageObjData> Datas = new List<StageObjData>();

			public List<bool> actives = new List<bool>();
		}

		private StageUpdate.MixBojSyncData tMixBojSyncData;

		public Dictionary<EventPointBase, bool> dictActives = new Dictionary<EventPointBase, bool>();

		private Dictionary<EventPointBase, bool>.Enumerator tEnumerator;

		private EventPointBase tEventPointBase;

		public override void OnLateUpdate()
		{
			tEnumerator = dictActives.GetEnumerator();
			tEventPointBase = null;
			while (tEnumerator.MoveNext())
			{
				if (tEnumerator.Current.Key.bCheckAble != tEnumerator.Current.Value)
				{
					tEventPointBase = tEnumerator.Current.Key;
					dictActives[tEventPointBase] = tEventPointBase.bCheckAble;
					break;
				}
			}
			if (tEventPointBase == null)
			{
				return;
			}
			KeyValuePair<EventPointBase, bool>[] array = dictActives.ToArray();
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i].Key != tEventPointBase)
				{
					dictActives[array[i].Key] = true;
					array[i].Key.bCheckAble = true;
				}
			}
		}

		public override bool IsNeedClip()
		{
			return false;
		}

		public override int GetTypeID()
		{
			return 14;
		}

		public override string GetTypeString()
		{
			return StageObjType.STAGEONEWORK_OBJ.ToString();
		}

		public override string GetSaveString()
		{
			return GetTypeString();
		}

		public override void LoadByString(string sLoad)
		{
			string text = sLoad.Substring(GetTypeString().Length);
			text = text.Replace(";" + text[0], ",");
			text = text.Substring(1);
			StageOneWorkSL stageOneWorkSL = JsonUtility.FromJson<StageOneWorkSL>(text);
			GameObject[] rootGameObjects = SceneManager.GetActiveScene().GetRootGameObjects();
			StageUpdate stageUpdate = null;
			for (int i = 0; i < rootGameObjects.Length; i++)
			{
				if (rootGameObjects[i].name == "StageUpdate")
				{
					stageUpdate = rootGameObjects[i].GetComponent<StageUpdate>();
					break;
				}
			}
			int count = stageOneWorkSL.Datas.Count;
			tMixBojSyncData = new StageUpdate.MixBojSyncData();
			if (stageUpdate != null)
			{
				stageUpdate.SetSyncStageFunc(sSyncID, tMixBojSyncData.OnSyncStageFunc);
				stageUpdate.AddMixBojSyncData(tMixBojSyncData);
				tMixBojSyncData.SetSyncMixStageFunc(sSyncID + "-0", OnSyncStageObj);
			}
			for (int j = 0; j < count; j++)
			{
				bool flag = (bool)stageUpdate;
				if (stageOneWorkSL.Datas[j].bunldepath == "")
				{
					Debug.LogError("prefab has no bundle path:" + stageOneWorkSL.Datas[j].path);
					continue;
				}
				int num = stageOneWorkSL.Datas[j].path.LastIndexOf("/");
				string text2 = stageOneWorkSL.Datas[j].path.Substring(num + 1);
				text2 = text2.Substring(0, text2.Length - 7);
				StageUpdate.LoadCallBackObj loadCallBackObj = new StageUpdate.LoadCallBackObj();
				loadCallBackObj.loadStageObjData = stageOneWorkSL.Datas[j];
				loadCallBackObj.i = ((stageOneWorkSL.actives.Count > j && stageOneWorkSL.actives[j]) ? 1 : 0);
				loadCallBackObj.lcb = StageLoadEndCall;
				loadCallBackObj.objParam0 = sSyncID + "-" + (j + 1);
				stageUpdate.AddSubLoadAB(loadCallBackObj);
				MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad<Object>(stageOneWorkSL.Datas[j].bunldepath, text2, loadCallBackObj.LoadCB);
			}
			sSyncID += "-0";
		}

		private void StageLoadEndCall(StageUpdate.LoadCallBackObj tObj, Object asset)
		{
			GameObject gameObject = Object.Instantiate(asset) as GameObject;
			StageObjData stageObjData = (StageObjData)tObj.loadStageObjData;
			gameObject.transform.parent = base.transform;
			gameObject.transform.localPosition = stageObjData.position;
			gameObject.transform.localScale = stageObjData.scale;
			gameObject.transform.localRotation = stageObjData.rotate;
			gameObject.name = stageObjData.name;
			int objtype = StageUpdate.LoadProperty(null, gameObject, stageObjData.property, null, tObj.objParam0 as string, tMixBojSyncData);
			if (tObj.i == 0)
			{
				gameObject.SetActive(false);
			}
			InitNewObj(objtype, gameObject);
		}

		private void InitNewObj(int objtype, GameObject newgobj)
		{
			EventPointBase component = newgobj.GetComponent<EventPointBase>();
			if (component != null)
			{
				dictActives.Add(component, component.bCheckAble);
			}
		}

		public override void OnSyncStageObj(string sIDKey, int nKey1, string smsg)
		{
			base.OnSyncStageObj(sIDKey, nKey1, smsg);
		}
	}
}
