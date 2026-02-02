using UnityEngine;

namespace StageLib
{
	[ExecuteInEditMode]
	public class StageRideableObj : StageSLBase
	{
		public class SROSL
		{
			public int nRideID;
		}

		public int nRideID;

		private int LastnRideID = -1;

		public bool bBack;

		private bool bLastBack;

		private bool bNeedSpawnGG;

		private Object vehicleInstance;

		private Object vehicleModel;

		private void LateUpdate()
		{
			UpdateModel();
			if (bNeedSpawnGG)
			{
				SpawnGG();
			}
		}

		public void UpdateModel()
		{
			if (StageUpdate.gbStageReady && StageUpdate.bIsHost && LastnRideID != nRideID)
			{
				LastnRideID = nRideID;
				while (base.transform.childCount > 0)
				{
					Object.Destroy(base.transform.GetChild(0).gameObject);
				}
				bNeedSpawnGG = true;
			}
		}

		public void SpawnGG()
		{
			if (!(MonoBehaviourSingleton<OrangeSceneManager>.Instance.MainCamera == null))
			{
				bNeedSpawnGG = false;
				CallRideSpawn();
			}
		}

		private void CallRideSpawn()
		{
			bLastBack = bBack;
			GameObject gameObject = Object.Instantiate((GameObject)vehicleInstance, base.transform.position, Quaternion.identity);
			GameObject obj = Object.Instantiate((GameObject)vehicleModel, Vector3.zero, Quaternion.identity);
			gameObject.SetActive(true);
			obj.name = "model";
			obj.transform.SetParent(gameObject.transform);
			obj.transform.localEulerAngles = new Vector3(0f, 90f, 0f);
			obj.transform.localPosition = Vector3.zero;
			RideBaseObj component = gameObject.GetComponent<RideBaseObj>();
			component.nRideID = LastnRideID;
			component.sNetSerialID = sSyncID;
			component.Activate = true;
			StageResManager.GetStageUpdate().RegisterStageObjBase(component);
			if (bLastBack)
			{
				component._characterDirection = CharacterDirection.LEFT;
			}
			else
			{
				component._characterDirection = CharacterDirection.RIGHT;
			}
		}

		public override int GetTypeID()
		{
			return 16;
		}

		public override string GetTypeString()
		{
			return StageObjType.RIDEABLE_OBJ.ToString();
		}

		public override string GetSaveString()
		{
			string typeString = GetTypeString();
			string text = JsonUtility.ToJson(new SROSL
			{
				nRideID = nRideID
			});
			text = text.Replace(",", ";");
			return typeString + text;
		}

		public override void LoadByString(string sLoad)
		{
			sLoad.Substring(GetTypeString().Length);
			SROSL sROSL = JsonUtility.FromJson<SROSL>(sLoad.Substring(GetTypeString().Length).Replace(";", ","));
			nRideID = sROSL.nRideID;
			VEHICLE_TABLE value;
			if (ManagedSingleton<OrangeDataManager>.Instance.VEHICLE_TABLE_DICT.TryGetValue(nRideID, out value))
			{
				StageUpdate stageUpdate = StageResManager.GetStageUpdate();
				StageUpdate.LoadCallBackObj loadCallBackObj = new StageUpdate.LoadCallBackObj
				{
					lcb = delegate(StageUpdate.LoadCallBackObj tLCB, Object tObj)
					{
						vehicleInstance = tObj;
						((GameObject)vehicleInstance).SetActive(false);
					}
				};
				stageUpdate.AddSubLoadAB(loadCallBackObj);
				if (nRideID != 4)
				{
					MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad<Object>("prefab/ridearmorbase", "ridearmorbase", loadCallBackObj.LoadCB);
				}
				else
				{
					MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad<Object>("prefab/noactarmorbase", "noactarmorbase", loadCallBackObj.LoadCB);
				}
				loadCallBackObj = new StageUpdate.LoadCallBackObj
				{
					lcb = delegate(StageUpdate.LoadCallBackObj tLCB, Object tObj)
					{
						vehicleModel = tObj;
					}
				};
				stageUpdate.AddSubLoadAB(loadCallBackObj);
				MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad<Object>("model/vehicle/" + value.s_MODEL, value.s_MODEL, loadCallBackObj.LoadCB);
				loadCallBackObj = new StageUpdate.LoadCallBackObj();
				stageUpdate.AddSubLoadAB(loadCallBackObj);
				MonoBehaviourSingleton<FxManager>.Instance.PreloadFx("fxuse_headcrush_001", 2, loadCallBackObj.LoadCBNoParam);
				SKILL_TABLE value2;
				if (ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT.TryGetValue(value.n_SKILL_1, out value2))
				{
					StageResManager.LoadBulletBySkillTable(value2);
				}
				if (ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT.TryGetValue(value.n_SKILL_2, out value2))
				{
					StageResManager.LoadBulletBySkillTable(value2);
				}
				if (ManagedSingleton<OrangeDataManager>.Instance.SKILL_TABLE_DICT.TryGetValue(value.n_SKILL_3, out value2))
				{
					StageResManager.LoadBulletBySkillTable(value2);
				}
			}
		}

		public override void SyncNowStatus()
		{
		}

		public override void OnSyncStageObj(string sIDKey, int nKey1, string smsg)
		{
		}
	}
}
