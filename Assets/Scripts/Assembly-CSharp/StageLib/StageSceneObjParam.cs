#define RELEASE
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StageLib
{
	public class StageSceneObjParam : MonoBehaviour
	{
		private class TransformData
		{
			public Vector3 vPos;

			public Vector3 vScale;

			public Vector3 vRotate;
		}

		private enum MOVE_BIT_SET
		{
			NONE = 0,
			ICE = 1,
			IGNORE_EVENT = 2,
			USE_ANIMATION = 4
		}

		private enum BROKEN_BIT_PARAM
		{
			NONE = 0,
			SMALL_BROKEN = 1,
			AUTO_REBORN = 2
		}

		private class FlyCalcuData
		{
			public Transform tTrans;

			public Vector3 tFlyDir;

			public Vector3 tRotateAxie;

			public float tFlySpeed;

			public float fRotateAngle;
		}

		private BoxCollider2D[] SelfB2Ds;

		private bool bIsHidden;

		private Dictionary<int, Mesh> MeshMap = new Dictionary<int, Mesh>();

		private List<string> ListActiveEnemy = new List<string>();

		public float FlyTime = 1f;

		public float fG = 19f;

		public float fReduFactor = 0.95f;

		public float fSpeedMin = 5f;

		public float fSpeedMax = 50f;

		public float fWoundedMin = -5f;

		public float fWoundedMax = 5f;

		public float fRotateFactor = 1f;

		public float fFlyRandomX = 5f;

		public float fFlyRandomY = 5f;

		public int nMoveBitParam;

		public int nBrokenBitParam;

		public float fIceSliderParam = 0.8f;

		public float fAutoReBornTime = 1.5f;

		[HideInInspector]
		public float fContinueExplosion;

		[HideInInspector]
		public int nContinueExplosionGroup;

		public Vector3 vExplosionExtend = new Vector3(0.2f, 0.2f, 0f);

		private bool bInitMeshModel;

		private bool bInitMeshMaterial;

		private bool bInitMeshCenter;

		private int nInitMeshMaterialMode;

		private MeshRenderer[] tmeshRenderers;

		private Dictionary<int, TransformData> dictTransData = new Dictionary<int, TransformData>();

		private Coroutine tRestoreSceneObjCoroutine;

		private MaterialPropertyBlock mpb;

		public bool bCanPlayBrokenSE;

		public string sCustomBrokenSE = "";

		public float fRebornFXWaitTime;

		public string[] sRebornFXs;

		public string[] sWoundedFXs;

		public string[] sBrokenFXs;

		public string sBrokenAnimation;

		public bool bCanPlayRebornSE;

		public string sCustomRebornSE = "";

		private Transform FxRoot;

		public float SCENE_OBJ_MAX_DIS = 16f;

		private MaterialPropertyBlock mpbCheckAlpha;

		private Dictionary<MeshRenderer, List<string>> textureNameMap = new Dictionary<MeshRenderer, List<string>>();

		private MeshFilter[] alls;

		private void Awake()
		{
			MeshFilter[] componentsInChildren = GetComponentsInChildren<MeshFilter>();
			foreach (MeshFilter meshFilter in componentsInChildren)
			{
				MeshMap.Add(meshFilter.gameObject.GetInstanceID(), meshFilter.sharedMesh);
			}
			if (sRebornFXs != null && sRebornFXs.Length != 0)
			{
				for (int num = sRebornFXs.Length - 1; num >= 0; num--)
				{
					StageResManager.LoadFx(sRebornFXs[num]);
				}
			}
			if (sWoundedFXs != null && sWoundedFXs.Length != 0)
			{
				for (int num2 = sWoundedFXs.Length - 1; num2 >= 0; num2--)
				{
					StageResManager.LoadFx(sWoundedFXs[num2]);
				}
			}
			if (sBrokenFXs != null && sBrokenFXs.Length != 0)
			{
				for (int num3 = sBrokenFXs.Length - 1; num3 >= 0; num3--)
				{
					StageResManager.LoadFx(sBrokenFXs[num3]);
				}
			}
			if (sCustomBrokenSE != "")
			{
				string[] array = sCustomBrokenSE.Split(',');
				if (array.Length >= 1)
				{
					StageResManager.LoadSE(array[0]);
				}
			}
			if (sCustomRebornSE != "")
			{
				string[] array2 = sCustomRebornSE.Split(',');
				if (array2.Length >= 1)
				{
					StageResManager.LoadSE(array2[0]);
				}
			}
			FxRoot = base.transform.Find("fx_root");
			if (FxRoot == null)
			{
				FxRoot = base.transform;
			}
		}

		public bool IsIceBlock()
		{
			return (nMoveBitParam & 1) != 0;
		}

		public bool IsIgnoreEvent()
		{
			return (nMoveBitParam & 2) != 0;
		}

		public bool IsUseAnimationInsteadExplosion()
		{
			return (nMoveBitParam & 4) != 0;
		}

		public bool IsUseSmallBroken()
		{
			return (nBrokenBitParam & 1) != 0;
		}

		public bool IsUseAutoBorn()
		{
			return (nBrokenBitParam & 2) != 0;
		}

		public void AddBrokenActiveEnemyID(string sNetID)
		{
			if (!ListActiveEnemy.Contains(sNetID))
			{
				ListActiveEnemy.Add(sNetID);
			}
		}

		public void CheckAlphaMaterial()
		{
			if (bInitMeshMaterial && nInitMeshMaterialMode == 2)
			{
				return;
			}
			bInitMeshMaterial = true;
			nInitMeshMaterialMode = 2;
			if (tmeshRenderers == null)
			{
				tmeshRenderers = base.transform.GetComponentsInChildren<MeshRenderer>();
			}
			if (mpbCheckAlpha == null)
			{
				mpbCheckAlpha = new MaterialPropertyBlock();
				if (tmeshRenderers.Length != 0)
				{
					tmeshRenderers[0].GetPropertyBlock(mpbCheckAlpha);
				}
			}
			MeshRenderer[] array = tmeshRenderers;
			foreach (MeshRenderer meshRenderer in array)
			{
				Material[] sharedMaterials = meshRenderer.sharedMaterials;
				textureNameMap.ContainsAdd(meshRenderer, new List<string>());
				List<string> list = textureNameMap[meshRenderer];
				for (int j = 0; j < sharedMaterials.Length; j++)
				{
					list.Add(meshRenderer.name + "#" + j);
				}
				for (int k = 0; k < sharedMaterials.Length; k++)
				{
					Texture mainTexture = sharedMaterials[k].mainTexture;
					sharedMaterials[k] = StageMaterialManager.Get("StageLib/DiffuseAlpha");
					if (mainTexture != null)
					{
						StageMaterialManager.SetTexture(list[k], mainTexture);
					}
				}
				meshRenderer.materials = sharedMaterials;
				for (int l = 0; l < sharedMaterials.Length; l++)
				{
					Texture texture;
					StageMaterialManager.TryGetTexture(list[l], out texture);
					mpbCheckAlpha.SetTexture(MonoBehaviourSingleton<OrangeMaterialProperty>.Instance.i_MainTex, texture);
					meshRenderer.SetPropertyBlock(mpbCheckAlpha, l);
				}
			}
		}

		public void SetSceneObjAlpha(Color tColor)
		{
			CheckAlphaMaterial();
			mpbCheckAlpha.SetColor(MonoBehaviourSingleton<OrangeMaterialProperty>.Instance.i_Color, tColor);
			MeshRenderer[] array = tmeshRenderers;
			foreach (MeshRenderer meshRenderer in array)
			{
				List<string> list = textureNameMap[meshRenderer];
				Material[] sharedMaterials = meshRenderer.sharedMaterials;
				for (int j = 0; j < sharedMaterials.Length; j++)
				{
					Texture texture;
					if (StageMaterialManager.TryGetTexture(list[j], out texture))
					{
						mpbCheckAlpha.SetTexture(MonoBehaviourSingleton<OrangeMaterialProperty>.Instance.i_MainTex, texture);
					}
					meshRenderer.SetPropertyBlock(mpbCheckAlpha, j);
				}
			}
		}

		public void CheckSelfB2Ds()
		{
			if (SelfB2Ds == null)
			{
				SelfB2Ds = base.gameObject.GetComponentsInChildren<BoxCollider2D>();
			}
		}

		public bool CheckContainPoint(Vector3 tPos)
		{
			if (bIsHidden)
			{
				return false;
			}
			CheckSelfB2Ds();
			BoxCollider2D[] selfB2Ds = SelfB2Ds;
			for (int i = 0; i < selfB2Ds.Length; i++)
			{
				if (selfB2Ds[i].bounds.Contains(tPos))
				{
					return true;
				}
			}
			return false;
		}

		public bool CheckIntersectB2D(ref Bounds tCheckBounds)
		{
			if (bIsHidden)
			{
				return false;
			}
			CheckSelfB2Ds();
			BoxCollider2D[] selfB2Ds = SelfB2Ds;
			for (int i = 0; i < selfB2Ds.Length; i++)
			{
				Bounds tAB = selfB2Ds[i].bounds;
				if (StageResManager.CheckBoundsIntersectNoZEffect(ref tAB, ref tCheckBounds))
				{
					return true;
				}
			}
			return false;
		}

		public void HiddenStageSceneObj()
		{
			CheckSelfB2Ds();
			bIsHidden = true;
			BoxCollider2D[] selfB2Ds = SelfB2Ds;
			for (int i = 0; i < selfB2Ds.Length; i++)
			{
				selfB2Ds[i].enabled = false;
			}
			MeshRenderer[] array = tmeshRenderers;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].enabled = false;
			}
		}

		public void UnHiddenStageSceneObj()
		{
			CheckSelfB2Ds();
			bIsHidden = false;
			BoxCollider2D[] selfB2Ds = SelfB2Ds;
			for (int i = 0; i < selfB2Ds.Length; i++)
			{
				selfB2Ds[i].enabled = true;
			}
			MeshRenderer[] array = tmeshRenderers;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].enabled = true;
			}
		}

		public bool CheckContinueExplosion(Vector3 vSetExtend)
		{
			List<StageSceneObjParam> tOutList = new List<StageSceneObjParam>();
			List<StageHurtObj> list = new List<StageHurtObj>();
			StageUpdate stageUpdate = StageResManager.GetStageUpdate();
			int instanceID = base.gameObject.GetInstanceID();
			CheckSelfB2Ds();
			BoxCollider2D[] selfB2Ds = SelfB2Ds;
			foreach (BoxCollider2D tCheckC2D in selfB2Ds)
			{
				tOutList.Clear();
				stageUpdate.GetStageSceneInB2D(tCheckC2D, ref tOutList, vSetExtend);
				foreach (StageSceneObjParam item in tOutList)
				{
					if (instanceID != item.gameObject.GetInstanceID() && item.nContinueExplosionGroup == nContinueExplosionGroup)
					{
						StageHurtObj componentInChildren = item.transform.GetComponentInChildren<StageHurtObj>();
						if (componentInChildren != null && !list.Contains(componentInChildren))
						{
							list.Add(componentInChildren);
						}
					}
				}
			}
			foreach (StageHurtObj item2 in list)
			{
				item2.BrkoenAll();
			}
			return false;
		}

		public void InitMeshMaterial()
		{
			CheckMesh();
			CheckMaterial();
		}

		public void ReplaceByMeshMap(Mesh tSharedMesh, Mesh tMesh, Vector3 vMovePos)
		{
			if (MeshMap.Count <= 0 || !MeshMap.ContainsValue(tSharedMesh))
			{
				return;
			}
			CheckMesh();
			if (alls == null)
			{
				alls = GetComponentsInChildren<MeshFilter>();
			}
			MeshFilter[] array = alls;
			foreach (MeshFilter meshFilter in array)
			{
				if (meshFilter.sharedMesh == tSharedMesh)
				{
					meshFilter.mesh = tMesh;
					Vector3 vector = vMovePos;
					vector = meshFilter.transform.localRotation * vector;
					Vector3 localPosition = meshFilter.transform.localPosition;
					localPosition.x += vector.x;
					localPosition.y += vector.y;
					localPosition.z += vector.z;
					meshFilter.transform.localPosition = localPosition;
				}
			}
		}

		private void CheckMesh()
		{
			if (bInitMeshModel)
			{
				return;
			}
			bInitMeshModel = true;
			if (MeshMap.Count <= 0)
			{
				return;
			}
			if (alls == null)
			{
				alls = GetComponentsInChildren<MeshFilter>();
			}
			MeshFilter[] array = alls;
			foreach (MeshFilter meshFilter in array)
			{
				if (meshFilter.transform.childCount <= 0)
				{
					meshFilter.sharedMesh = MeshMap[meshFilter.gameObject.GetInstanceID()];
				}
			}
		}

		private void CheckMaterial()
		{
			if ((bInitMeshMaterial && nInitMeshMaterialMode == 1) || IsUseSmallBroken() || IsUseAnimationInsteadExplosion())
			{
				return;
			}
			bInitMeshMaterial = true;
			nInitMeshMaterialMode = 1;
			if (tmeshRenderers == null)
			{
				tmeshRenderers = base.transform.GetComponentsInChildren<MeshRenderer>();
			}
			if (tmeshRenderers.Length == 0)
			{
				return;
			}
			Texture2D assstSync = MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssstSync<Texture2D>("shader/orangecommon", "noise2");
			Texture2D assstSync2 = MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssstSync<Texture2D>("shader/orangecommon", "TCP2_Ramp_3Levels");
			mpb = new MaterialPropertyBlock();
			tmeshRenderers[0].GetPropertyBlock(mpb);
			MeshRenderer[] array = tmeshRenderers;
			foreach (MeshRenderer meshRenderer in array)
			{
				if (meshRenderer.sharedMaterial == null)
				{
					continue;
				}
				Material[] sharedMaterials = meshRenderer.sharedMaterials;
				Texture mainTexture;
				for (int j = 0; j < sharedMaterials.Length; j++)
				{
					mainTexture = sharedMaterials[j].mainTexture;
					if (mainTexture != null)
					{
						StageMaterialManager.SetTexture(meshRenderer.name + "#" + j, mainTexture);
					}
				}
				mainTexture = meshRenderer.sharedMaterial.mainTexture;
				meshRenderer.material = StageMaterialManager.Get("StageLib/StageStandardObj");
				if (mainTexture != null)
				{
					mpb.SetTexture(MonoBehaviourSingleton<OrangeMaterialProperty>.Instance.i_MainTex, mainTexture);
				}
				mpb.SetTexture(MonoBehaviourSingleton<OrangeMaterialProperty>.Instance.i_DissolveMap, assstSync);
				mpb.SetTexture(MonoBehaviourSingleton<OrangeMaterialProperty>.Instance.i_DissolveRamp, assstSync2);
				meshRenderer.SetPropertyBlock(mpb);
			}
		}

		private void CheckAllMeshCenter()
		{
			if (bInitMeshCenter)
			{
				return;
			}
			bInitMeshCenter = true;
			if (alls == null)
			{
				alls = GetComponentsInChildren<MeshFilter>();
			}
			MeshFilter[] array = alls;
			foreach (MeshFilter meshFilter in array)
			{
				if (meshFilter.transform.childCount <= 0)
				{
					CheckMeshCenter(meshFilter);
				}
			}
			if (tmeshRenderers == null)
			{
				tmeshRenderers = base.transform.GetComponentsInChildren<MeshRenderer>();
			}
			MeshRenderer[] array2 = tmeshRenderers;
			foreach (MeshRenderer meshRenderer in array2)
			{
				if (meshRenderer.transform.childCount == 0 && !dictTransData.ContainsKey(meshRenderer.transform.GetInstanceID()))
				{
					TransformData transformData = new TransformData();
					transformData.vPos = meshRenderer.transform.localPosition;
					transformData.vScale = meshRenderer.transform.localScale;
					transformData.vRotate = meshRenderer.transform.localRotation.eulerAngles;
					dictTransData.Add(meshRenderer.transform.GetInstanceID(), transformData);
				}
			}
		}

		private void CheckMeshCenter(MeshFilter tMF)
		{
			Mesh sharedMesh = tMF.sharedMesh;
			Vector3 zero = Vector3.zero;
			Vector3 zero2 = Vector3.zero;
			if (!sharedMesh.isReadable || sharedMesh.vertexCount <= 0)
			{
				return;
			}
			Vector3[] vertices = sharedMesh.vertices;
			if (vertices.Length < sharedMesh.vertexCount)
			{
				return;
			}
			for (int num = sharedMesh.vertexCount - 1; num >= 0; num--)
			{
				zero += vertices[num];
			}
			if (!(zero.magnitude < 0.001f))
			{
				Mesh mesh = tMF.mesh;
				zero /= (float)mesh.vertexCount;
				for (int num2 = mesh.vertexCount - 1; num2 >= 0; num2--)
				{
					vertices[num2] -= zero;
				}
				if (sharedMesh.GetInstanceID() != tMF.sharedMesh.GetInstanceID())
				{
					tMF.sharedMesh = sharedMesh;
				}
				StageSceneObjParam[] array = Resources.FindObjectsOfTypeAll<StageSceneObjParam>();
				for (int i = 0; i < array.Length; i++)
				{
					array[i].ReplaceByMeshMap(sharedMesh, mesh, zero);
				}
				mesh.vertices = vertices;
				mesh.RecalculateBounds();
			}
		}

		public void WoundedStageSceneObj()
		{
			CheckMesh();
			CheckAllMeshCenter();
			Vector3 position = base.transform.position;
			if (tmeshRenderers == null)
			{
				tmeshRenderers = base.transform.GetComponentsInChildren<MeshRenderer>();
			}
			MeshRenderer[] array = tmeshRenderers;
			foreach (MeshRenderer meshRenderer in array)
			{
				if (meshRenderer.transform.childCount == 0)
				{
					Vector3 normalized = (meshRenderer.bounds.center - position).normalized;
					Vector3 axis = Vector3.Cross(normalized, Vector3.up);
					meshRenderer.transform.RotateAround(meshRenderer.transform.position, axis, OrangeBattleUtility.Random(fWoundedMin, fWoundedMax));
				}
			}
			if (sWoundedFXs != null && sWoundedFXs.Length != 0)
			{
				for (int num = sWoundedFXs.Length - 1; num >= 0; num--)
				{
					MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(sWoundedFXs[num], FxRoot, Quaternion.Euler(0f, 0f, 0f), Array.Empty<object>());
				}
			}
		}

		public void SwitchAllFallingFloor(bool bSet = false)
		{
			FallingFloor[] componentsInChildren = GetComponentsInChildren<FallingFloor>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].enabled = bSet;
			}
		}

		public void BrokenStageSceneObj(float fWaitTime = 0f, int objType = 0)
		{
			if (bIsHidden)
			{
				Debug.LogError("BrokenStageSceneObj Two Times.");
				return;
			}
			bIsHidden = true;
			CheckMesh();
			CheckMaterial();
			CheckAllMeshCenter();
			if (IsUseAnimationInsteadExplosion())
			{
				if (sBrokenAnimation != "")
				{
					Animator component = base.transform.GetComponent<Animator>();
					if (component != null && base.gameObject.activeInHierarchy)
					{
						StartCoroutine(PlayAniAndWaitEnd(component));
					}
					else
					{
						base.gameObject.SetActive(false);
					}
				}
			}
			else if (base.gameObject.activeInHierarchy)
			{
				StartCoroutine(BrokenCoroutine(fWaitTime, objType));
			}
			else
			{
				base.gameObject.SetActive(false);
			}
		}

		public void SwitchB2DInStageSceneObj(bool bSet)
		{
			CheckSelfB2Ds();
			BoxCollider2D[] selfB2Ds = SelfB2Ds;
			for (int i = 0; i < selfB2Ds.Length; i++)
			{
				selfB2Ds[i].enabled = bSet;
			}
		}

		public void SwitchAnimatorInStageSceneObj(bool bSet)
		{
			Animator[] array = null;
			array = base.transform.GetComponentsInChildren<Animator>();
			int num = array.Length;
			for (int i = 0; i < num; i++)
			{
				array[i].enabled = bSet;
			}
		}

		private void ActiveEnemyList()
		{
			for (int num = ListActiveEnemy.Count - 1; num >= 0; num--)
			{
				StageUpdate.EnemyCtrlID enemyCtrlIDByNetSerialID = StageUpdate.GetEnemyCtrlIDByNetSerialID(ListActiveEnemy[num]);
				if (enemyCtrlIDByNetSerialID != null && (int)enemyCtrlIDByNetSerialID.mEnemy.Hp > 0 && !enemyCtrlIDByNetSerialID.mEnemy.Activate)
				{
					ObjInfoBar componentInChildren = enemyCtrlIDByNetSerialID.mEnemy.transform.GetComponentInChildren<ObjInfoBar>(true);
					if (componentInChildren != null)
					{
						componentInChildren.gameObject.SetActive(true);
					}
					enemyCtrlIDByNetSerialID.mEnemy.SetActive(true);
				}
			}
			ListActiveEnemy.Clear();
		}

		private IEnumerator ContinueExplosionCoroutine()
		{
			float tmpfCE = fContinueExplosion;
			float tmpfCE2 = tmpfCE * 0.5f;
			while (tmpfCE > 0f)
			{
				tmpfCE -= Time.deltaTime;
				tmpfCE2 -= Time.deltaTime;
				if (tmpfCE2 <= 0f)
				{
					SwitchB2DInStageSceneObj(true);
					CheckContinueExplosion(new Vector3(vExplosionExtend.x, 0f, 0f));
					SwitchB2DInStageSceneObj(false);
					tmpfCE2 = fContinueExplosion;
				}
				if (tmpfCE <= 0f)
				{
					SwitchB2DInStageSceneObj(true);
					CheckContinueExplosion(vExplosionExtend);
					SwitchB2DInStageSceneObj(false);
				}
				if (MonoBehaviourSingleton<OrangeGameManager>.Instance.IsGamePause)
				{
					yield return CoroutineDefine._waitForEndOfFrame;
				}
				else
				{
					yield return CoroutineDefine._waitForEndOfFrame;
				}
			}
		}

		private IEnumerator BrokenCoroutine(float fWaitTime, int objType)
		{
			while (fWaitTime > 0f)
			{
				yield return CoroutineDefine._waitForEndOfFrame;
				if (!MonoBehaviourSingleton<OrangeGameManager>.Instance.IsGamePause)
				{
					fWaitTime -= Time.deltaTime;
				}
			}
			StageUpdate tStageUpdate = StageResManager.GetStageUpdate();
			if (fContinueExplosion > 0f && tStageUpdate != null)
			{
				tStageUpdate.StartCoroutine(ContinueExplosionCoroutine());
			}
			SwitchAllFallingFloor();
			SwitchB2DInStageSceneObj(false);
			SwitchAnimatorInStageSceneObj(false);
			ActiveEnemyList();
			Vector3 position = base.transform.position;
			bool flag = false;
			List<FlyCalcuData> listFlyCalcuDatas = new List<FlyCalcuData>();
			MeshRenderer[] array = tmeshRenderers;
			foreach (MeshRenderer meshRenderer in array)
			{
				if (meshRenderer.transform.childCount == 0)
				{
					Vector3 normalized = (meshRenderer.bounds.center - position).normalized;
					normalized = Quaternion.Euler(OrangeBattleUtility.Random(0f - fFlyRandomX, fFlyRandomX), OrangeBattleUtility.Random(0f - fFlyRandomY, fFlyRandomY), 0f) * normalized;
					FlyCalcuData flyCalcuData = new FlyCalcuData();
					flyCalcuData.tTrans = meshRenderer.transform;
					flyCalcuData.tFlySpeed = OrangeBattleUtility.Random(fSpeedMin, fSpeedMax);
					flyCalcuData.tFlyDir = normalized * flyCalcuData.tFlySpeed;
					flyCalcuData.tRotateAxie = Vector3.Cross(normalized, Vector3.up);
					listFlyCalcuDatas.Add(flyCalcuData);
					if (meshRenderer.isVisible)
					{
						flag = true;
					}
				}
			}
			if (sCustomBrokenSE != "")
			{
				string[] array2 = sCustomBrokenSE.Split(',');
				if (array2.Length >= 1)
				{
					MonoBehaviourSingleton<AudioManager>.Instance.GetAPoint().Play(SCENE_OBJ_MAX_DIS, base.transform, array2[0], array2[1]);
					bCanPlayBrokenSE = false;
				}
			}
			if (bCanPlayBrokenSE && flag)
			{
				OrangeCriPoint aPoint = MonoBehaviourSingleton<AudioManager>.Instance.GetAPoint();
				switch (objType)
				{
				case 0:
					aPoint.Play(SCENE_OBJ_MAX_DIS, base.transform, "HitSE", "ht_dead01");
					break;
				case 1:
					aPoint.Play(SCENE_OBJ_MAX_DIS, base.transform, "BattleSE", "bt_wall02");
					break;
				}
			}
			if (sBrokenFXs != null && sBrokenFXs.Length != 0)
			{
				for (int num = sBrokenFXs.Length - 1; num >= 0; num--)
				{
					MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(sBrokenFXs[num], FxRoot, Quaternion.Euler(0f, 0f, 0f), Array.Empty<object>());
				}
			}
			float fFlyTime = FlyTime;
			float fStartValue = 0f;
			while (fFlyTime > 0f)
			{
				if (MonoBehaviourSingleton<OrangeGameManager>.Instance.IsGamePause)
				{
					yield return CoroutineDefine._waitForEndOfFrame;
					continue;
				}
				float deltaTime = Time.deltaTime;
				fFlyTime -= deltaTime;
				for (int j = 0; j < listFlyCalcuDatas.Count; j++)
				{
					Vector3 position2 = listFlyCalcuDatas[j].tTrans.position;
					position2 += listFlyCalcuDatas[j].tFlyDir * deltaTime;
					listFlyCalcuDatas[j].tFlyDir *= fReduFactor;
					listFlyCalcuDatas[j].tFlySpeed *= fReduFactor;
					listFlyCalcuDatas[j].tTrans.position = position2;
					listFlyCalcuDatas[j].tTrans.RotateAround(position2, listFlyCalcuDatas[j].tRotateAxie, listFlyCalcuDatas[j].fRotateAngle);
					listFlyCalcuDatas[j].fRotateAngle += deltaTime * listFlyCalcuDatas[j].tFlySpeed * fRotateFactor;
					if (listFlyCalcuDatas[j].fRotateAngle > 180f)
					{
						listFlyCalcuDatas[j].fRotateAngle = 0f;
					}
					listFlyCalcuDatas[j].tFlyDir.y -= fG * deltaTime;
				}
				if (fStartValue < 1f)
				{
					if (IsUseSmallBroken())
					{
						SetChildScale(1f - fStartValue);
					}
					else
					{
						SetDissolveVal(fStartValue);
					}
					fStartValue += deltaTime;
				}
				yield return CoroutineDefine._waitForEndOfFrame;
			}
			while (fStartValue < 1f)
			{
				if (MonoBehaviourSingleton<OrangeGameManager>.Instance.IsGamePause)
				{
					yield return CoroutineDefine._waitForEndOfFrame;
					continue;
				}
				if (IsUseSmallBroken())
				{
					SetChildScale(1f - fStartValue);
				}
				else
				{
					SetDissolveVal(fStartValue);
				}
				fStartValue += Time.deltaTime;
				yield return CoroutineDefine._waitForEndOfFrame;
			}
			FxBase[] componentsInChildren = base.transform.GetComponentsInChildren<FxBase>();
			for (int num2 = componentsInChildren.Length - 1; num2 >= 0; num2--)
			{
				componentsInChildren[num2].BackToPool();
			}
			base.gameObject.SetActive(false);
			if (IsUseAutoBorn() && tStageUpdate != null)
			{
				tStageUpdate.StartCoroutine(StageResManager.TweenFloatCoroutine(0f, 1f, fAutoReBornTime, null, delegate
				{
					RestoreSceneObj();
				}));
			}
		}

		public void SetRimMin(float fValue)
		{
			if (mpb != null)
			{
				OrangeMaterialProperty instance = MonoBehaviourSingleton<OrangeMaterialProperty>.Instance;
				mpb.SetFloat(instance.i_RimMin, fValue);
				UpdatePropertyBlock();
			}
		}

		private void UpdatePropertyBlock()
		{
			if (mpb != null)
			{
				MeshRenderer[] array = tmeshRenderers;
				for (int i = 0; i < array.Length; i++)
				{
					array[i].SetPropertyBlock(mpb);
				}
			}
		}

		public void SetDissolveVal(float fStartValue)
		{
			if (mpb != null)
			{
				mpb.SetFloat(MonoBehaviourSingleton<OrangeMaterialProperty>.Instance.i_DissolveValue, fStartValue);
				UpdatePropertyBlock();
			}
		}

		public void SetChildScale(float fScale)
		{
			MeshRenderer[] array = tmeshRenderers;
			foreach (MeshRenderer meshRenderer in array)
			{
				if (meshRenderer.transform.childCount == 0)
				{
					TransformData transformData = dictTransData[meshRenderer.transform.GetInstanceID()];
					meshRenderer.transform.localScale = transformData.vScale * fScale;
				}
			}
		}

		public void RestoreSceneObj()
		{
			if (tRestoreSceneObjCoroutine == null && bIsHidden)
			{
				base.gameObject.SetActive(true);
				tRestoreSceneObjCoroutine = StartCoroutine(RestoreSceneObjCoroutine());
			}
		}

		private IEnumerator RestoreSceneObjCoroutine()
		{
			CheckMesh();
			CheckAllMeshCenter();
			if (sRebornFXs != null && sRebornFXs.Length != 0)
			{
				for (int num = sRebornFXs.Length - 1; num >= 0; num--)
				{
					MonoBehaviourSingleton<FxManager>.Instance.Play<FxBase>(sRebornFXs[num], FxRoot, Quaternion.Euler(0f, 0f, 0f), Array.Empty<object>());
				}
			}
			float fWaitTime = fRebornFXWaitTime;
			while (fWaitTime > 0f)
			{
				if (!MonoBehaviourSingleton<OrangeGameManager>.Instance.IsGamePause)
				{
					fWaitTime -= Time.deltaTime;
				}
				yield return CoroutineDefine._waitForEndOfFrame;
			}
			if (sCustomRebornSE != "")
			{
				string[] array = sCustomRebornSE.Split(',');
				if (array.Length >= 1)
				{
					MonoBehaviourSingleton<AudioManager>.Instance.GetAPoint().Play(SCENE_OBJ_MAX_DIS, base.transform, array[0], array[1]);
				}
			}
			SwitchAllFallingFloor(true);
			SwitchB2DInStageSceneObj(true);
			MeshRenderer[] array2 = tmeshRenderers;
			foreach (MeshRenderer meshRenderer in array2)
			{
				if (meshRenderer.transform.childCount == 0)
				{
					TransformData transformData = dictTransData[meshRenderer.transform.GetInstanceID()];
					meshRenderer.transform.localPosition = transformData.vPos;
					meshRenderer.transform.localScale = transformData.vScale;
					meshRenderer.transform.localRotation = Quaternion.Euler(transformData.vRotate);
				}
			}
			if (!IsUseSmallBroken())
			{
				float fStartValue = 1f;
				while (fStartValue > 0f)
				{
					if (!MonoBehaviourSingleton<OrangeGameManager>.Instance.IsGamePause)
					{
						SetDissolveVal(fStartValue);
						fStartValue -= Time.deltaTime;
					}
					yield return CoroutineDefine._waitForEndOfFrame;
				}
				SetDissolveVal(0f);
			}
			bIsHidden = false;
			tRestoreSceneObjCoroutine = null;
		}

		public void NotifyEventID(int nID)
		{
			if (nID != 0)
			{
				EventManager.StageEventCall stageEventCall = new EventManager.StageEventCall();
				stageEventCall.nID = nID;
				stageEventCall.tTransform = null;
				Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.STAGE_EVENT_CALL, stageEventCall);
			}
		}

		public void NotifyEventByNameID()
		{
			int num = int.Parse(base.gameObject.name.Substring(base.gameObject.name.Length - 4));
			if (num != 0)
			{
				EventManager.StageEventCall stageEventCall = new EventManager.StageEventCall();
				stageEventCall.nID = num;
				stageEventCall.tTransform = null;
				Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.STAGE_EVENT_CALL, stageEventCall);
			}
		}

		public IEnumerator PlayAniAndWaitEnd(Animator tAnimator)
		{
			StageUpdate stageUpdate = StageResManager.GetStageUpdate();
			if (fContinueExplosion > 0f && stageUpdate != null)
			{
				stageUpdate.StartCoroutine(ContinueExplosionCoroutine());
			}
			tAnimator.Play(sBrokenAnimation);
			tAnimator.Update(0f);
			while (!(tAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime > 1f))
			{
				yield return CoroutineDefine._waitForEndOfFrame;
			}
			SwitchAllFallingFloor();
			SwitchB2DInStageSceneObj(false);
			SwitchAnimatorInStageSceneObj(false);
			ActiveEnemyList();
			FxBase[] componentsInChildren = base.transform.GetComponentsInChildren<FxBase>();
			for (int num = componentsInChildren.Length - 1; num >= 0; num--)
			{
				componentsInChildren[num].BackToPool();
			}
			base.gameObject.SetActive(false);
		}
	}
}
