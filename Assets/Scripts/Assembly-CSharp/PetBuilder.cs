using System.Collections;
using System.Collections.Generic;
using CallbackDefs;
using StageLib;
using UnityEngine;

public class PetBuilder : MonoBehaviour
{
	private enum PetParts
	{
		PetObject = 0,
		PetMesh = 1,
		PetMotion = 2,
		MAX_PARTS = 3
	}

	public int PetID;

	public int follow_skill_id;

	private Object[] _loadedParts;

	private List<StageUpdate.LoadCallBackObj> listLoads = new List<StageUpdate.LoadCallBackObj>();

	private StageUpdate.LoadCallBackObj GetNewLoadCbObj()
	{
		StageUpdate.LoadCallBackObj loadCallBackObj = new StageUpdate.LoadCallBackObj();
		listLoads.Add(loadCallBackObj);
		return loadCallBackObj;
	}

	public void CreatePet<T>(Callback<T> pCallBack = null) where T : PetControllerBase
	{
		StageUpdate.LoadCallBackObj loadCallBackObj = null;
		_loadedParts = new Object[3];
		PET_TABLE pET_TABLE = ManagedSingleton<OrangeDataManager>.Instance.PET_TABLE_DICT[PetID];
		loadCallBackObj = GetNewLoadCbObj();
		loadCallBackObj.lcb = delegate(StageUpdate.LoadCallBackObj tLCB, Object obj)
		{
			_loadedParts[0] = obj;
		};
		MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad<Object>("model/pet/petbase", "PetBase", loadCallBackObj.LoadCB);
		loadCallBackObj = GetNewLoadCbObj();
		loadCallBackObj.lcb = delegate(StageUpdate.LoadCallBackObj tLCB, Object obj)
		{
			_loadedParts[1] = obj;
		};
		MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad<Object>("model/pet/" + pET_TABLE.s_MODEL, pET_TABLE.s_MODEL + "_G.prefab", loadCallBackObj.LoadCB);
		loadCallBackObj = GetNewLoadCbObj();
		loadCallBackObj.lcb = delegate(StageUpdate.LoadCallBackObj tLCB, Object obj)
		{
			string text = ((RuntimeAnimatorController)obj).name;
			AnimatorOverrideController animatorOverrideController = new AnimatorOverrideController
			{
				runtimeAnimatorController = (RuntimeAnimatorController)obj,
				name = text
			};
			_loadedParts[2] = animatorOverrideController;
		};
		MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad<RuntimeAnimatorController>("model/animator/petemptycontroller", "PetEmptyController", loadCallBackObj.LoadCB);
		loadCallBackObj = GetNewLoadCbObj();
		MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad<Object>("model/animation/pet/" + pET_TABLE.s_MODEL, pET_TABLE.s_MODEL, loadCallBackObj.LoadCB);
		StartCoroutine(Build(pCallBack));
	}

	private IEnumerator Build<T>(Callback<T> pCallBack) where T : PetControllerBase
	{
		bool flag = false;
		while (!flag)
		{
			yield return CoroutineDefine._waitForEndOfFrame;
			flag = true;
			for (int i = 0; i < listLoads.Count; i++)
			{
				if (!listLoads[i].bLoadEnd)
				{
					flag = false;
					break;
				}
			}
		}
		PET_TABLE pET_TABLE = ManagedSingleton<OrangeDataManager>.Instance.PET_TABLE_DICT[PetID];
		GameObject gameObject = Object.Instantiate((GameObject)_loadedParts[0], base.transform.position, Quaternion.identity);
		GameObject obj = Object.Instantiate((GameObject)_loadedParts[1], Vector3.zero, Quaternion.identity);
		obj.name = "model";
		obj.transform.SetParent(gameObject.transform);
		obj.transform.eulerAngles = new Vector3(0f, 90f, 0f);
		obj.transform.localPosition = Vector3.zero;
		Animator component = obj.GetComponent<Animator>();
		AnimatorOverrideController animatorOverrideController = (AnimatorOverrideController)_loadedParts[2];
		T val = gameObject.AddComponent<T>();
		val.PetID = PetID;
		val.follow_skill_id = follow_skill_id;
		string[] source;
		string[] target;
		val.GetUniqueMotion(out source, out target);
		if (source.Length != 0 && source[0] != "null")
		{
			for (int j = 0; j < target.Length; j++)
			{
				AnimationClip assstSync = MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssstSync<AnimationClip>("model/animation/pet/" + pET_TABLE.s_MODEL, target[j]);
				animatorOverrideController[source[j]] = assstSync;
			}
		}
		string[] petDependAnimations = val.GetPetDependAnimations();
		if (petDependAnimations != null)
		{
			for (int k = 0; k < petDependAnimations.Length; k++)
			{
				AnimationClip assstSync2 = MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssstSync<AnimationClip>("model/animation/pet/" + pET_TABLE.s_MODEL, petDependAnimations[k]);
				animatorOverrideController["skillclip" + k] = assstSync2;
			}
		}
		if ((bool)component)
		{
			component.runtimeAnimatorController = animatorOverrideController;
		}
		pCallBack.CheckTargetToInvoke(val);
		AnimatorSoundHelper[] componentsInChildren = val.gameObject.GetComponentsInChildren<AnimatorSoundHelper>();
		for (int l = 0; l < componentsInChildren.Length; l++)
		{
			componentsInChildren[l].SoundSource = val.SoundSource;
		}
		Object.Destroy(base.gameObject);
	}
}
