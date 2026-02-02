#define RELEASE
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace StageLib
{
	[ExecuteInEditMode]
	public class MapCollisionEvent : EventPointBase
	{
		public class MapCollisionSL
		{
			public int nSetID;

			public int mapEvent;

			public bool bS = true;

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
		}

		public enum MapCollisionEnum
		{
			NONE = 0,
			COLLISION_TRACK = 1,
			COLLISION_BUMPOBJ = 2,
			COLLISION_DESTRUCTIBLE = 3,
			COLLISION_KICKOBJ = 4,
			COLLISION_BUMPOBJ2 = 5,
			COLLISION_QUICKSAND = 6,
			MAX_NUM = 7
		}

		private enum SYNC_TYPE
		{
			SYNC_EVENT = 0,
			SYNC_RECONNECT = 1
		}

		public MapCollisionEnum mapEvent;

		public bool bStartAtInit = true;

		public float fParam0;

		public float fParam1;

		public float fParam2;

		public float fParam3;

		public float fParam4;

		public int nParam0;

		public int nParam1;

		public int nParam2;

		public List<int> listnParam0 = new List<int>();

		public List<int> listnParam1 = new List<int>();

		public List<float> listfParam0 = new List<float>();

		public Dictionary<int, int> dicKeyIntParam0 = new Dictionary<int, int>();

		public Dictionary<float, int> dicKeyIntParam1 = new Dictionary<float, int>();

		public Dictionary<float, int> dicKeyIntParam2 = new Dictionary<float, int>();

		public List<string> listsParam0 = new List<string>();

		private float fNowStatus0;

		private Vector3 originPos = Vector3.zero;

		private int nNowStep;

		private float fLeftTime;

		private float fTmpParam0;

		private float fTmpParam1;

		private Color tTmpColor;

		private bool bStartUpdtate;

		private bool bUseSE;

		private Action[] actRunFunction;

		private List<Transform> allCollisions = new List<Transform>();

		private List<Transform> ListHitTmpCollisions = new List<Transform>();

		private List<Controller2D> allController2Ds = new List<Controller2D>();

		private StageUpdate.MixBojSyncData tMixBojSyncData;

		private Bounds tmpBoundA;

		private Bounds tmpBoundB;

		private void OnDestroy()
		{
			Singleton<GenericEventManager>.Instance.DetachEvent<EventManager.StageEventCall>(EventManager.ID.STAGE_EVENT_CALL, CallOnByID);
		}

		public override void Init()
		{
			Singleton<GenericEventManager>.Instance.AttachEvent<EventManager.StageEventCall>(EventManager.ID.STAGE_EVENT_CALL, CallOnByID);
			StartCoroutine(InitCollisionDataCoroutine());
			originPos = base.transform.localPosition;
			bCheck = false;
		}

		private IEnumerator InitCollisionDataCoroutine()
		{
			while (!StageUpdate.gbStageReady)
			{
				yield return CoroutineDefine._waitForEndOfFrame;
			}
			InitCollisionData();
			fNowStatus0 = 0f;
		}

		public void CallOnByID(EventManager.StageEventCall tStageEventCall)
		{
			int nID = tStageEventCall.nID;
			if (nID == 0 || nSetID != nID)
			{
				return;
			}
			if (!bStartUpdtate)
			{
				bStartUpdtate = true;
				switch (mapEvent)
				{
				case MapCollisionEnum.COLLISION_TRACK:
					if (base.transform.childCount > 0)
					{
						FindName(base.transform, "UVscroll", AddCollider);
					}
					break;
				case MapCollisionEnum.COLLISION_BUMPOBJ:
				case MapCollisionEnum.COLLISION_BUMPOBJ2:
					fLeftTime = 0f;
					nNowStep = 0;
					break;
				case MapCollisionEnum.COLLISION_DESTRUCTIBLE:
				case MapCollisionEnum.COLLISION_KICKOBJ:
					break;
				}
				return;
			}
			switch (mapEvent)
			{
			case MapCollisionEnum.COLLISION_TRACK:
				bStartUpdtate = false;
				break;
			case MapCollisionEnum.COLLISION_BUMPOBJ:
			case MapCollisionEnum.COLLISION_BUMPOBJ2:
				fLeftTime = 0f;
				nNowStep = 3;
				listnParam0.Clear();
				listnParam0.Add(1);
				break;
			case MapCollisionEnum.COLLISION_DESTRUCTIBLE:
				if (!(fParam0 <= 0f))
				{
					fTmpParam1 = fParam0;
					fParam0 = 0f;
					StageObjBase stageObjBase = null;
					if (tStageEventCall.tTransform != null)
					{
						stageObjBase = tStageEventCall.tTransform.GetComponent<StageObjBase>();
					}
					HurtCPCheck(stageObjBase);
					if (stageObjBase == null)
					{
						StageUpdate.SyncStageObj(sSyncID, 0, "," + fParam0.ToString("0.000"), true);
					}
					else
					{
						StageUpdate.SyncStageObj(sSyncID, 0, stageObjBase.sNetSerialID + "," + fParam0.ToString("0.000"), true);
					}
				}
				break;
			case MapCollisionEnum.COLLISION_KICKOBJ:
				break;
			}
		}

		protected override void UpdateEvent()
		{
			OnLateUpdate();
		}

		public override void OnLateUpdate()
		{
			if (!base.gameObject.activeSelf || !bStartUpdtate)
			{
				return;
			}
			if (mapEvent == MapCollisionEnum.COLLISION_TRACK)
			{
				for (int num = StageUpdate.runPlayers.Count - 1; num >= 0; num--)
				{
					if (StageUpdate.runPlayers[num].UsingVehicle)
					{
						RideArmorController component = StageUpdate.runPlayers[num].transform.root.GetComponent<RideArmorController>();
						if ((bool)StageResManager.ObjMoveCollisionWithBoxCheck(component.Controller, Vector2.down, 0.03f, LayerMask.GetMask("Block"), null, allCollisions))
						{
							component.AddForce(new VInt3(new Vector3(fParam0 * 0.1f, 0f, 0f)));
							break;
						}
					}
					else if ((bool)StageResManager.ObjMoveCollisionWithBoxCheck(StageUpdate.runPlayers[num].Controller, Vector2.down, 0.03f, LayerMask.GetMask("Block"), null, allCollisions))
					{
						StageUpdate.runPlayers[num].AddForce(new VInt3(new Vector3(fParam0, 0f, 0f)));
					}
				}
				for (int num2 = StageUpdate.runEnemys.Count - 1; num2 >= 0; num2--)
				{
					if ((bool)StageResManager.ObjMoveCollisionWithBoxCheck(StageUpdate.runEnemys[num2].mEnemy.Controller, Vector2.down, 0.03f, LayerMask.GetMask("Block"), null, allCollisions))
					{
						StageUpdate.runEnemys[num2].mEnemy.AddForce(new VInt3(new Vector3(fParam0, 0f, 0f)));
					}
				}
			}
			else if (mapEvent == MapCollisionEnum.COLLISION_BUMPOBJ || mapEvent == MapCollisionEnum.COLLISION_BUMPOBJ2)
			{
				actRunFunction[nNowStep]();
			}
			else if (mapEvent == MapCollisionEnum.COLLISION_KICKOBJ)
			{
				tmpBoundA = EventB2D.bounds;
				for (int num3 = StageUpdate.runPlayers.Count - 1; num3 >= 0; num3--)
				{
					OrangeCharacter orangeCharacter = StageUpdate.runPlayers[num3];
					if (!(orangeCharacter.refRideBaseObj != null) && !orangeCharacter.bIsNpcCpy && (int)orangeCharacter.Hp > 0 && orangeCharacter.Controller.Collider2D.enabled)
					{
						tmpBoundB = orangeCharacter.Controller.Collider2D.bounds;
						if (StageResManager.CheckBoundsContainNoZEffect(ref tmpBoundA, ref tmpBoundB) && (orangeCharacter.CheckActStatusEvt(4, -1) || orangeCharacter.CheckActStatusEvt(6, -1) || Mathf.Abs(orangeCharacter.Velocity.x) > listnParam0[3]))
						{
							RunUpdateClass runUpdateClass = new RunUpdateClass();
							runUpdateClass.tUpdateCB = (Action<RunUpdateClass>)Delegate.Combine(runUpdateClass.tUpdateCB, new Action<RunUpdateClass>(UpdateFlyObj));
							runUpdateClass.fParams = new float[5];
							runUpdateClass.oParams = new object[4];
							if (orangeCharacter.CheckActStatusEvt(6, -1))
							{
								runUpdateClass.fParams[0] = fParam2 * Mathf.Sign(base.transform.position.x - tmpBoundB.center.x);
								runUpdateClass.fParams[1] = fParam3;
								runUpdateClass.fParams[4] = listfParam0[1];
								runUpdateClass.oParams[3] = listnParam0[0];
							}
							else
							{
								runUpdateClass.fParams[0] = fParam0 * Mathf.Sign(base.transform.position.x - tmpBoundB.center.x);
								runUpdateClass.fParams[1] = fParam1;
								runUpdateClass.fParams[4] = listfParam0[2];
								runUpdateClass.oParams[3] = listnParam0[1];
							}
							runUpdateClass.fParams[2] = 0f;
							runUpdateClass.fParams[3] = listfParam0[0];
							Quaternion[] array = new Quaternion[allCollisions.Count];
							runUpdateClass.oParams[0] = array;
							BoxCollider2D[][] array2 = new BoxCollider2D[allCollisions.Count][];
							runUpdateClass.oParams[1] = array2;
							runUpdateClass.oParams[2] = base.transform.GetComponentsInChildren<StageSceneObjParam>();
							for (int i = 0; i < allCollisions.Count; i++)
							{
								array[i] = allCollisions[i].transform.rotation;
								array2[i] = allCollisions[i].transform.GetComponentsInChildren<BoxCollider2D>();
							}
							if (((uint)nParam0 & (true ? 1u : 0u)) != 0 && listsParam0.Count >= 2 && listsParam0[1] != "")
							{
								base.SoundSource.PlaySE(listsParam0[0], listsParam0[1]);
							}
							SiCoroutineUpdate = (Action<EventPointBase>)Delegate.Combine(SiCoroutineUpdate, new Action<EventPointBase>(runUpdateClass.UpdateCall));
							bStartUpdtate = false;
							tTmpColor = Color.white;
						}
					}
				}
			}
			else
			{
				if (mapEvent != MapCollisionEnum.COLLISION_QUICKSAND)
				{
					return;
				}
				tmpBoundA = EventB2D.bounds;
				for (int num4 = StageUpdate.runPlayers.Count - 1; num4 >= 0; num4--)
				{
					OrangeCharacter orangeCharacter2 = StageUpdate.runPlayers[num4];
					if (!(orangeCharacter2.refRideBaseObj != null) && !orangeCharacter2.bIsNpcCpy && (int)orangeCharacter2.Hp > 0 && orangeCharacter2.Controller.Collider2D.enabled)
					{
						tmpBoundB = orangeCharacter2.Controller.Collider2D.bounds;
						if (StageResManager.CheckBoundsContainNoZEffect(ref tmpBoundA, ref tmpBoundB))
						{
							if (!orangeCharacter2.listQuickSand.Contains(sSyncID))
							{
								orangeCharacter2.listQuickSand.Add(sSyncID);
								if (orangeCharacter2.listQuickSand.Count() > 0)
								{
									orangeCharacter2.bQuickSand = true;
								}
								orangeCharacter2.fQuickSand = fParam0;
								if (nParam0 == 0)
								{
									nParam0 = 1;
									ChangeScrollXSpeed(fParam2);
									ChangeScrollYSpeed(fParam4);
									if (listsParam0.Count >= 2 && listsParam0[0] != "" && listsParam0[1] != "")
									{
										base.SoundSource.PlaySE(listsParam0[0], listsParam0[1]);
									}
								}
							}
						}
						else if (orangeCharacter2.listQuickSand.Contains(sSyncID))
						{
							orangeCharacter2.listQuickSand.Remove(sSyncID);
							if (orangeCharacter2.listQuickSand.Count() == 0)
							{
								orangeCharacter2.bQuickSand = false;
							}
							if (nParam0 == 1)
							{
								ChangeScrollXSpeed(fParam1);
								ChangeScrollYSpeed(fParam3);
								nParam0 = 0;
								if (listsParam0.Count >= 4 && listsParam0[2] != "" && listsParam0[3] != "")
								{
									base.SoundSource.PlaySE(listsParam0[2], listsParam0[3]);
								}
							}
						}
					}
				}
			}
		}

		private void ChangeScrollXSpeed(float fSpeed)
		{
			for (int i = 0; i < base.transform.childCount; i++)
			{
				Renderer component = base.transform.GetChild(i).GetComponent<Renderer>();
				if (!(component != null))
				{
					continue;
				}
				Material[] materials = component.materials;
				foreach (Material material in materials)
				{
					if (material.shader.name.Contains("UVScroll"))
					{
						material.SetFloat("_ScrollX", fSpeed);
					}
				}
			}
		}

		private void ChangeScrollYSpeed(float fSpeed)
		{
			for (int i = 0; i < base.transform.childCount; i++)
			{
				Renderer component = base.transform.GetChild(i).GetComponent<Renderer>();
				if (!(component != null))
				{
					continue;
				}
				Material[] materials = component.materials;
				foreach (Material material in materials)
				{
					if (material.shader.name.Contains("UVScroll"))
					{
						material.SetFloat("_ScrollY", fSpeed);
					}
				}
			}
		}

		private void UpdateFlyObj(RunUpdateClass tRUC)
		{
			LayerMask mask = LayerMask.GetMask("Block", "SemiBlock");
			for (int i = 0; i < allCollisions.Count; i++)
			{
				Vector2 vector = new Vector2(tRUC.fParams[0] * 0.016f, tRUC.fParams[1] * 0.016f);
				float num = vector.magnitude;
				vector /= num;
				BoxCollider2D[] array = (tRUC.oParams[1] as BoxCollider2D[][])[i];
				bool flag = false;
				Vector2 inNormal = Vector2.zero;
				BoxCollider2D[] array2 = array;
				for (int j = 0; j < array2.Length; j++)
				{
					RaycastHit2D raycastHit2D = StageResManager.ObjMoveCollisionWithBoxCheckX(array2[j], Vector3.zero, vector, num, mask, ListHitTmpCollisions);
					if ((bool)raycastHit2D)
					{
						Vector2.Dot(vector, raycastHit2D.normal);
						num = raycastHit2D.distance;
						flag = true;
						inNormal = raycastHit2D.normal;
						if (num <= 0f)
						{
							break;
						}
					}
				}
				vector *= num;
				allCollisions[i].transform.position = allCollisions[i].transform.position + new Vector3(vector.x, vector.y, 0f);
				if (flag)
				{
					vector = new Vector2(tRUC.fParams[0], tRUC.fParams[1]);
					vector = Vector2.Reflect(vector, inNormal);
					tRUC.fParams[0] = vector.x;
					tRUC.fParams[1] = vector.y;
					if (((uint)nParam0 & (true ? 1u : 0u)) != 0)
					{
						if (listsParam0.Count == 2)
						{
							base.SoundSource.PlaySE(listsParam0[0], listsParam0[1]);
						}
						if (listsParam0.Count == 3)
						{
							base.SoundSource.PlaySE(listsParam0[0], listsParam0[2]);
						}
					}
				}
				allCollisions[i].transform.rotation = ((Quaternion[])tRUC.oParams[0])[i];
				allCollisions[i].transform.RotateAround(allCollisions[i].transform.position, Vector3.back, tRUC.fParams[2] + tRUC.fParams[0] * 45f * 0.016f);
				bool flag2 = false;
				array2 = array;
				for (int j = 0; j < array2.Length; j++)
				{
					if ((bool)StageResManager.ObjOverlapWithBoxCheckX(array2[j], Vector3.zero, mask, ListHitTmpCollisions))
					{
						flag2 = true;
						break;
					}
				}
				if (!flag2)
				{
					tRUC.fParams[2] += tRUC.fParams[0] * 45f * 0.016f;
					continue;
				}
				allCollisions[i].transform.rotation = ((Quaternion[])tRUC.oParams[0])[i];
				allCollisions[i].transform.RotateAround(allCollisions[i].transform.position, Vector3.back, tRUC.fParams[2]);
			}
			tRUC.fParams[0] = tRUC.fParams[0] * 0.995f;
			tRUC.fParams[1] -= fParam4 * 0.016f;
			tRUC.fParams[3] -= 0.016f;
			if (tRUC.fParams[4] >= 0f)
			{
				tRUC.fParams[4] -= 0.016f;
				if (tRUC.fParams[4] < 0f && (int)tRUC.oParams[3] != 0)
				{
					EventManager.StageEventCall stageEventCall = new EventManager.StageEventCall();
					stageEventCall.nID = (int)tRUC.oParams[3];
					stageEventCall.tTransform = StageUpdate.GetMainPlayerTrans();
					Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.STAGE_EVENT_CALL, stageEventCall);
				}
			}
			if (tRUC.fParams[3] <= 0f)
			{
				tRUC.bIsEnd = true;
				if (((uint)nParam0 & (true ? 1u : 0u)) != 0 && listsParam0.Count >= 3 && listsParam0[2] != "")
				{
					base.SoundSource.PlaySE(listsParam0[0], listsParam0[2]);
				}
				StageSceneObjParam[] array3 = tRUC.oParams[2] as StageSceneObjParam[];
				for (int j = 0; j < array3.Length; j++)
				{
					array3[j].SwitchB2DInStageSceneObj(false);
				}
				if (listnParam0[2] != 0)
				{
					EventManager.StageEventCall stageEventCall2 = new EventManager.StageEventCall();
					stageEventCall2.nID = listnParam0[2];
					stageEventCall2.tTransform = StageUpdate.GetMainPlayerTrans();
					Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.STAGE_EVENT_CALL, stageEventCall2);
				}
			}
			else if (tRUC.fParams[3] <= 1f)
			{
				tTmpColor.a = tRUC.fParams[3];
				StageSceneObjParam[] array3 = tRUC.oParams[2] as StageSceneObjParam[];
				for (int j = 0; j < array3.Length; j++)
				{
					array3[j].SetSceneObjAlpha(tTmpColor);
				}
			}
		}

		private void WaitTime()
		{
			fLeftTime += 0.016f;
			if (fLeftTime >= fParam0)
			{
				fLeftTime = 0f;
				fNowStatus0 = 0f;
				nNowStep++;
				if (bUseSE)
				{
					base.SoundSource.PlaySE(listsParam0[4], listsParam0[5]);
				}
			}
		}

		private float fMoveToB2DLeft(BoxCollider2D tBoxCollider2D, RideArmorController tRAC, float fDefault)
		{
			float num = 0f;
			if (tBoxCollider2D != null)
			{
				num = tBoxCollider2D.bounds.min.x - (tRAC.transform.position.x + tRAC.Controller.Collider2D.bounds.size.x);
				if (num > 0f)
				{
					num = 0f;
				}
			}
			else
			{
				num = fDefault;
			}
			return num;
		}

		private float fMoveToB2DRight(BoxCollider2D tBoxCollider2D, RideArmorController tRAC, float fDefault)
		{
			float num = 0f;
			if (tBoxCollider2D != null)
			{
				num = tBoxCollider2D.bounds.max.x - (tRAC.transform.position.x - tRAC.Controller.Collider2D.bounds.size.x);
				if (num < 0f)
				{
					num = 0f;
				}
			}
			else
			{
				num = fDefault;
			}
			return num;
		}

		private float fMoveToB2DLeft(BoxCollider2D tBoxCollider2D, OrangeCharacter tOC, float fDefault)
		{
			float num = 0f;
			if (tBoxCollider2D != null)
			{
				num = tBoxCollider2D.bounds.min.x - (tOC.transform.position.x + tOC.Controller.Collider2D.bounds.size.x);
				if (num > 0f)
				{
					num = 0f;
				}
			}
			else
			{
				num = fDefault;
			}
			return num;
		}

		private float fMoveToB2DRight(BoxCollider2D tBoxCollider2D, OrangeCharacter tOC, float fDefault)
		{
			float num = 0f;
			if (tBoxCollider2D != null)
			{
				num = tBoxCollider2D.bounds.max.x - (tOC.transform.position.x - tOC.Controller.Collider2D.bounds.size.x);
				if (num < 0f)
				{
					num = 0f;
				}
			}
			else
			{
				num = fDefault;
			}
			return num;
		}

		private void MoveCheckHitBlock()
		{
			fNowStatus0 += fParam1;
			Vector3 vector = default(Vector3);
			switch (nParam0)
			{
			case 0:
				vector = new Vector3(0f, (0f - fNowStatus0) * GameLogicUpdateManager.m_fFrameLen, 0f);
				break;
			case 1:
				vector = new Vector3(0f, fNowStatus0 * GameLogicUpdateManager.m_fFrameLen, 0f);
				break;
			case 2:
				vector = new Vector3((0f - fNowStatus0) * GameLogicUpdateManager.m_fFrameLen, 0f, 0f);
				break;
			case 3:
				vector = new Vector3(fNowStatus0 * GameLogicUpdateManager.m_fFrameLen, 0f, 0f);
				break;
			}
			bool flag = false;
			float num = 0f;
			switch (nParam0)
			{
			case 0:
				num = Mathf.Abs(vector.y);
				break;
			case 1:
				num = Mathf.Abs(vector.y);
				break;
			case 2:
				num = Mathf.Abs(vector.x);
				break;
			case 3:
				num = Mathf.Abs(vector.x);
				break;
			}
			Vector2 zero3 = Vector2.zero;
			Vector2 vDir = Vector2.down;
			Vector2 zero4 = Vector2.zero;
			ListHitTmpCollisions.Clear();
			RaycastHit2D outhit;
			for (int num2 = StageUpdate.runPlayers.Count - 1; num2 >= 0; num2--)
			{
				if (StageUpdate.runPlayers[num2].UsingVehicle)
				{
					RideArmorController component = StageUpdate.runPlayers[num2].transform.root.GetComponent<RideArmorController>();
					Controller2D controller = component.Controller;
					while (CheckObjBir(nParam0, component.Controller, num, out outhit, ListHitTmpCollisions, allCollisions))
					{
						if (mapEvent == MapCollisionEnum.COLLISION_BUMPOBJ)
						{
							Vector3 zero = Vector3.zero;
							switch (nParam0)
							{
							case 0:
							case 1:
								if (component.ModelTransform.localScale.z == 1f)
								{
									BoxCollider2D component2 = outhit.transform.GetComponent<BoxCollider2D>();
									zero.x = fMoveToB2DLeft(component2, component, 0f - fParam4);
									LockRangeObj component3 = component.GetComponent<LockRangeObj>();
									if ((bool)component3 && component3.CheckOutRange(component.Controller, zero))
									{
										zero.x = fMoveToB2DRight(component2, component, fParam4);
									}
								}
								else
								{
									BoxCollider2D component2 = outhit.transform.GetComponent<BoxCollider2D>();
									zero.x = fMoveToB2DLeft(component2, component, fParam4);
									LockRangeObj component4 = component.GetComponent<LockRangeObj>();
									if ((bool)component4 && component4.CheckOutRange(component.Controller, zero))
									{
										zero.x = fMoveToB2DRight(component2, component, 0f - fParam4);
									}
								}
								break;
							case 2:
							case 3:
							{
								BoxCollider2D component2 = outhit.transform.GetComponent<BoxCollider2D>();
								if (component2 != null)
								{
									zero.y = component2.bounds.min.y - (component.ModelTransform.position.y + component.Controller.Collider2D.bounds.size.y);
									if (zero.y > 0f)
									{
										zero.y = 0f;
									}
								}
								else
								{
									zero.y = 0f - fParam4;
								}
								break;
							}
							}
							component.transform.localPosition = component.transform.localPosition + zero;
							component.Controller.LogicPosition = new VInt3(component.Controller.LogicPosition.vec3 + zero);
						}
						EventManager.StageSkillAtkTargetParam stageSkillAtkTargetParam = new EventManager.StageSkillAtkTargetParam();
						stageSkillAtkTargetParam.tTrans = component.transform;
						stageSkillAtkTargetParam.nSkillID = nParam1;
						stageSkillAtkTargetParam.bAtkNoCast = true;
						stageSkillAtkTargetParam.tPos = component.transform.position;
						stageSkillAtkTargetParam.tDir = component.transform.position;
						stageSkillAtkTargetParam.bBuff = false;
						Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.STAGE_SKILL_ATK_TARGET, stageSkillAtkTargetParam);
						ListHitTmpCollisions.Add(outhit.transform);
					}
				}
				else
				{
					Controller2D controller2 = StageUpdate.runPlayers[num2].Controller;
					while (CheckObjBir(nParam0, StageUpdate.runPlayers[num2].Controller, num, out outhit, ListHitTmpCollisions, allCollisions))
					{
						if (mapEvent == MapCollisionEnum.COLLISION_BUMPOBJ)
						{
							Vector3 zero2 = Vector3.zero;
							switch (nParam0)
							{
							case 0:
							case 1:
								if (StageUpdate.runPlayers[num2].ModelTransform.localScale.z == 1f)
								{
									BoxCollider2D component2 = outhit.transform.GetComponent<BoxCollider2D>();
									zero2.x = fMoveToB2DLeft(component2, StageUpdate.runPlayers[num2], 0f - fParam4);
									LockRangeObj component5 = StageUpdate.runPlayers[num2].GetComponent<LockRangeObj>();
									if ((bool)component5 && component5.CheckOutRange(StageUpdate.runPlayers[num2].Controller, zero2))
									{
										zero2.x = fMoveToB2DRight(component2, StageUpdate.runPlayers[num2], fParam4);
									}
								}
								else
								{
									BoxCollider2D component2 = outhit.transform.GetComponent<BoxCollider2D>();
									zero2.x = fMoveToB2DRight(component2, StageUpdate.runPlayers[num2], fParam4);
									LockRangeObj component6 = StageUpdate.runPlayers[num2].GetComponent<LockRangeObj>();
									if ((bool)component6 && component6.CheckOutRange(StageUpdate.runPlayers[num2].Controller, zero2))
									{
										zero2.x = fMoveToB2DLeft(component2, StageUpdate.runPlayers[num2], 0f - fParam4);
									}
								}
								break;
							case 2:
							case 3:
							{
								BoxCollider2D component2 = outhit.transform.GetComponent<BoxCollider2D>();
								if (component2 != null)
								{
									zero2.y = component2.bounds.min.y - (StageUpdate.runPlayers[num2].ModelTransform.position.y + StageUpdate.runPlayers[num2].Controller.Collider2D.bounds.size.y);
									if (zero2.y > 0f)
									{
										zero2.y = 0f;
									}
								}
								else
								{
									zero2.y = 0f - fParam4;
								}
								break;
							}
							}
							StageUpdate.runPlayers[num2].mapCollisionCB.CheckTargetToInvoke(mapEvent);
							StageUpdate.runPlayers[num2].transform.localPosition = StageUpdate.runPlayers[num2].transform.localPosition + zero2;
							StageUpdate.runPlayers[num2].Controller.LogicPosition = new VInt3(StageUpdate.runPlayers[num2].Controller.LogicPosition.vec3 + zero2);
						}
						else if (mapEvent == MapCollisionEnum.COLLISION_BUMPOBJ2)
						{
							StageUpdate.runPlayers[num2].mapCollisionCB.CheckTargetToInvoke(mapEvent);
							Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.STAGE_REBORNEVENT, StageUpdate.runPlayers[num2], StageUpdate.runPlayers[num2].transform.position);
						}
						EventManager.StageSkillAtkTargetParam stageSkillAtkTargetParam2 = new EventManager.StageSkillAtkTargetParam();
						stageSkillAtkTargetParam2.tTrans = StageUpdate.runPlayers[num2].transform;
						stageSkillAtkTargetParam2.nSkillID = nParam1;
						stageSkillAtkTargetParam2.bAtkNoCast = true;
						stageSkillAtkTargetParam2.tPos = StageUpdate.runPlayers[num2].transform.position;
						stageSkillAtkTargetParam2.tDir = StageUpdate.runPlayers[num2].transform.position;
						stageSkillAtkTargetParam2.bBuff = false;
						Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.STAGE_SKILL_ATK_TARGET, stageSkillAtkTargetParam2);
						ListHitTmpCollisions.Add(outhit.transform);
					}
				}
			}
			if ((nParam2 & 1) == 0)
			{
				for (int num3 = StageUpdate.runEnemys.Count - 1; num3 >= 0; num3--)
				{
					if (CheckObjBir(nParam0, StageUpdate.runEnemys[num3].mEnemy.Controller, num, out outhit, null, allCollisions))
					{
						StageUpdate.runEnemys[num3].mEnemy.Hp = 0;
						StageUpdate.runEnemys[num3].mEnemy.Hurt(new HurtPassParam());
					}
				}
			}
			switch (nParam0)
			{
			case 0:
				vDir = Vector2.down;
				break;
			case 1:
				vDir = Vector2.up;
				break;
			case 2:
				vDir = Vector2.left;
				break;
			case 3:
				vDir = Vector2.right;
				break;
			}
			for (int i = 0; i < allController2Ds.Count; i++)
			{
				Controller2D controller2D = allController2Ds[i];
				switch (nParam0)
				{
				case 0:
				{
					Controller2D.RaycastOrigins raycastOrigin = controller2D._raycastOrigins;
					Vector2 vector2 = Vector2.right * controller2D._verticalRaySpacing;
					break;
				}
				case 1:
				{
					Controller2D.RaycastOrigins raycastOrigin2 = controller2D._raycastOrigins;
					Vector2 vector3 = Vector2.right * controller2D._verticalRaySpacing;
					break;
				}
				case 2:
				{
					Controller2D.RaycastOrigins raycastOrigin3 = controller2D._raycastOrigins;
					Vector2 vector4 = Vector2.up * controller2D._horizontalRaySpacing;
					break;
				}
				case 3:
				{
					Controller2D.RaycastOrigins raycastOrigin4 = controller2D._raycastOrigins;
					Vector2 vector5 = Vector2.up * controller2D._horizontalRaySpacing;
					break;
				}
				}
				outhit = StageResManager.ObjMoveCollisionWithBoxCheck(controller2D, vDir, num, LayerMask.GetMask("Block"), allCollisions);
				if ((bool)outhit)
				{
					switch (nParam0)
					{
					case 0:
						vector.y = 0f - outhit.distance;
						break;
					case 1:
						vector.y = outhit.distance;
						break;
					case 2:
						vector.x = 0f - outhit.distance;
						break;
					case 3:
						vector.x = outhit.distance;
						break;
					}
					nNowStep++;
					Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.CAMERA_SHAKE, 1f, false);
					Animation[] componentsInChildren = base.transform.GetComponentsInChildren<Animation>();
					foreach (Animation animation in componentsInChildren)
					{
						foreach (AnimationState item in animation)
						{
							if (item.name.ToLower().Contains("down"))
							{
								animation.Play(item.name);
								break;
							}
						}
					}
					flag = true;
				}
				if (flag)
				{
					if (bUseSE)
					{
						base.SoundSource.PlaySE(listsParam0[6], listsParam0[7]);
					}
					break;
				}
			}
			base.transform.localPosition = base.transform.localPosition + vector;
			for (int k = 0; k < allController2Ds.Count; k++)
			{
				Controller2D controller2D2 = allController2Ds[k];
				controller2D2.LogicPosition = new VInt3(controller2D2.transform.localPosition);
				controller2D2.UpdateRaycastOrigins();
			}
		}

		private bool CheckObjBir(int nDir, Controller2D tController2D, float moveLen, out RaycastHit2D outhit, List<Transform> listIgnoreSelf, List<Transform> listCheck)
		{
			Vector2 zero = Vector2.zero;
			Vector2 vDir = Vector2.down;
			Vector2 zero2 = Vector2.zero;
			tController2D.UpdateRaycastOrigins();
			switch (nDir)
			{
			case 0:
				vDir = Vector2.up;
				break;
			case 1:
				vDir = Vector2.down;
				break;
			case 2:
				vDir = Vector2.right;
				break;
			case 3:
				vDir = Vector2.left;
				break;
			}
			if ((bool)(outhit = StageResManager.ObjMoveCollisionWithBoxCheck(tController2D, vDir, moveLen, tController2D.collisionMask, listIgnoreSelf, listCheck)))
			{
				return true;
			}
			return false;
		}

		private void WaitTimeToReturn()
		{
			fLeftTime += 0.016f;
			if (!(fLeftTime >= fParam3))
			{
				return;
			}
			fLeftTime = 0f;
			nNowStep++;
			if (bUseSE)
			{
				base.SoundSource.PlaySE(listsParam0[0], listsParam0[1]);
			}
			Animation[] componentsInChildren = base.transform.GetComponentsInChildren<Animation>();
			foreach (Animation animation in componentsInChildren)
			{
				foreach (AnimationState item in animation)
				{
					if (item.name.ToLower().Contains("up"))
					{
						animation.Play(item.name);
						break;
					}
				}
			}
		}

		private void ReturnToPos()
		{
			fNowStatus0 = fParam2;
			Vector3 vector = Vector3.zero;
			switch (nParam0)
			{
			case 0:
				vector = new Vector3(0f, fNowStatus0 * GameLogicUpdateManager.m_fFrameLen, 0f);
				break;
			case 1:
				vector = new Vector3(0f, (0f - fNowStatus0) * GameLogicUpdateManager.m_fFrameLen, 0f);
				break;
			case 2:
				vector = new Vector3(fNowStatus0 * GameLogicUpdateManager.m_fFrameLen, 0f, 0f);
				break;
			case 3:
				vector = new Vector3((0f - fNowStatus0) * GameLogicUpdateManager.m_fFrameLen, 0f, 0f);
				break;
			}
			float moveLen = 0f;
			switch (nParam0)
			{
			case 0:
				if (originPos.y - (base.transform.localPosition.y + vector.y) < 0f)
				{
					vector.y = originPos.y - base.transform.localPosition.y;
					nNowStep = 0;
					if (listnParam0.Count > 0 && listnParam0[0] == 1)
					{
						bStartUpdtate = false;
						listnParam0.Clear();
					}
				}
				moveLen = Mathf.Abs(vector.y);
				break;
			case 1:
				if (originPos.y - (base.transform.localPosition.y + vector.y) > 0f)
				{
					vector.y = originPos.y - base.transform.localPosition.y;
					nNowStep = 0;
					if (listnParam0.Count > 0 && listnParam0[0] == 1)
					{
						bStartUpdtate = false;
						listnParam0.Clear();
					}
				}
				moveLen = Mathf.Abs(vector.y);
				break;
			case 2:
				if (originPos.x - (base.transform.localPosition.x + vector.x) < 0f)
				{
					vector.x = originPos.x - base.transform.localPosition.x;
					nNowStep = 0;
					if (listnParam0.Count > 0 && listnParam0[0] == 1)
					{
						bStartUpdtate = false;
						listnParam0.Clear();
					}
				}
				moveLen = Mathf.Abs(vector.x);
				break;
			case 3:
				if (originPos.x - (base.transform.localPosition.x + vector.x) > 0f)
				{
					vector.x = originPos.x - base.transform.localPosition.x;
					nNowStep = 0;
					if (listnParam0.Count > 0 && listnParam0[0] == 1)
					{
						bStartUpdtate = false;
						listnParam0.Clear();
					}
				}
				moveLen = Mathf.Abs(vector.x);
				break;
			}
			if (bUseSE && nNowStep == 0)
			{
				base.SoundSource.PlaySE(listsParam0[2], listsParam0[3]);
			}
			Vector2 zero = Vector2.zero;
			for (int num = StageUpdate.runPlayers.Count - 1; num >= 0; num--)
			{
				OrangeCharacter orangeCharacter = StageUpdate.runPlayers[num];
				RaycastHit2D outhit;
				if (orangeCharacter.UsingVehicle)
				{
					RideArmorController component = orangeCharacter.transform.root.GetComponent<RideArmorController>();
					Controller2D controller = component.Controller;
					if (CheckObjBir(1, controller, 0.03f, out outhit, null, allCollisions))
					{
						BoxCollider2D boxCollider2D = outhit.collider as BoxCollider2D;
						Vector3 vector2 = vector;
						if (CheckObjBir(nParam0, controller, moveLen, out outhit, allCollisions, null))
						{
							if (!Physics2D.Raycast(controller.transform.position, Vector3.right, 3f, LayerMask.GetMask("Block", "SemiBlock", "BlockPlayer")))
							{
								vector2.x = boxCollider2D.bounds.max.x - (component.ModelTransform.position.x - component.Controller.Collider2D.bounds.size.x);
							}
							else
							{
								vector2.x = boxCollider2D.bounds.min.x - (component.ModelTransform.position.x + component.Controller.Collider2D.bounds.size.x);
							}
						}
						component.transform.localPosition = component.transform.localPosition + vector2;
						component.Controller.LogicPosition = new VInt3(component.Controller.LogicPosition.vec3 + vector2);
					}
					else if (CheckObjBir(2, controller, 0.03f, out outhit, null, allCollisions))
					{
						if (CheckObjBir(nParam0, controller, moveLen, out outhit, allCollisions, null))
						{
							switch (nParam0)
							{
							case 0:
								vector.y = outhit.distance;
								break;
							case 1:
								vector.y = 0f - outhit.distance;
								break;
							case 2:
								vector.x = outhit.distance;
								break;
							case 3:
								vector.x = 0f - outhit.distance;
								break;
							}
						}
						component.transform.localPosition = component.transform.localPosition + vector;
						component.Controller.LogicPosition = new VInt3(component.Controller.LogicPosition.vec3 + vector);
					}
					else if (CheckObjBir(3, controller, 0.03f, out outhit, null, allCollisions))
					{
						if (CheckObjBir(nParam0, controller, moveLen, out outhit, allCollisions, null))
						{
							switch (nParam0)
							{
							case 0:
								vector.y = outhit.distance;
								break;
							case 1:
								vector.y = 0f - outhit.distance;
								break;
							case 2:
								vector.x = outhit.distance;
								break;
							case 3:
								vector.x = 0f - outhit.distance;
								break;
							}
						}
						component.transform.localPosition = component.transform.localPosition + vector;
						component.Controller.LogicPosition = new VInt3(component.Controller.LogicPosition.vec3 + vector);
					}
				}
				else
				{
					Controller2D controller2 = orangeCharacter.Controller;
					if (CheckObjBir(1, controller2, 0.03f, out outhit, null, allCollisions))
					{
						BoxCollider2D boxCollider2D2 = outhit.collider as BoxCollider2D;
						Vector3 vector3 = vector;
						if (CheckObjBir(nParam0, controller2, moveLen, out outhit, allCollisions, null))
						{
							if (!Physics2D.Raycast(controller2.transform.position, Vector3.right, 3f, LayerMask.GetMask("Block", "SemiBlock", "BlockPlayer")))
							{
								vector3.x = boxCollider2D2.bounds.max.x - (orangeCharacter.ModelTransform.position.x - orangeCharacter.Controller.Collider2D.bounds.size.x);
							}
							else
							{
								vector3.x = boxCollider2D2.bounds.min.x - (orangeCharacter.ModelTransform.position.x + orangeCharacter.Controller.Collider2D.bounds.size.x);
							}
						}
						orangeCharacter.transform.localPosition = orangeCharacter.transform.localPosition + vector3;
						orangeCharacter.Controller.LogicPosition = new VInt3(orangeCharacter.Controller.LogicPosition.vec3 + vector3);
					}
					else if (IsOCCanReturnMove(orangeCharacter) && CheckObjBir(2, controller2, 0.03f, out outhit, null, allCollisions))
					{
						if (CheckObjBir(nParam0, controller2, moveLen, out outhit, allCollisions, null))
						{
							switch (nParam0)
							{
							case 0:
								vector.y = outhit.distance;
								break;
							case 1:
								vector.y = 0f - outhit.distance;
								break;
							case 2:
								vector.x = outhit.distance;
								break;
							case 3:
								vector.x = 0f - outhit.distance;
								break;
							}
						}
						orangeCharacter.transform.localPosition = orangeCharacter.transform.localPosition + vector;
						orangeCharacter.Controller.LogicPosition = new VInt3(orangeCharacter.Controller.LogicPosition.vec3 + vector);
					}
					else if (IsOCCanReturnMove(orangeCharacter) && CheckObjBir(3, controller2, 0.03f, out outhit, null, allCollisions))
					{
						if (CheckObjBir(nParam0, controller2, moveLen, out outhit, allCollisions, null))
						{
							switch (nParam0)
							{
							case 0:
								vector.y = outhit.distance;
								break;
							case 1:
								vector.y = 0f - outhit.distance;
								break;
							case 2:
								vector.x = outhit.distance;
								break;
							case 3:
								vector.x = 0f - outhit.distance;
								break;
							}
						}
						orangeCharacter.transform.localPosition = orangeCharacter.transform.localPosition + vector;
						orangeCharacter.Controller.LogicPosition = new VInt3(orangeCharacter.Controller.LogicPosition.vec3 + vector);
					}
				}
			}
			base.transform.localPosition = base.transform.localPosition + vector;
		}

		private bool IsOCCanReturnMove(OrangeCharacter tOC)
		{
			if (tOC.CheckActStatus(5, -1))
			{
				return false;
			}
			if (tOC.CheckActStatus(9, -1) && tOC.WallSlideGravity == 0)
			{
				return false;
			}
			return true;
		}

		public void InitCollisionData()
		{
			if (bStartAtInit)
			{
				bStartUpdtate = true;
			}
			allCollisions.Clear();
			switch (mapEvent)
			{
			case MapCollisionEnum.COLLISION_QUICKSAND:
				if (listsParam0.Any((string p) => p != ""))
				{
					bUseSE = true;
					base.SoundSource.Initial(OrangeSSType.MAPOBJS);
				}
				break;
			case MapCollisionEnum.COLLISION_TRACK:
			{
				if (base.transform.childCount <= 0)
				{
					break;
				}
				FindName(base.transform, "UVscroll", AddCollider);
				if ((nParam0 & 1) == 0)
				{
					break;
				}
				BoxCollider2D[] componentsInChildren4 = base.transform.GetComponentsInChildren<BoxCollider2D>();
				for (int m = 0; m < componentsInChildren4.Length; m++)
				{
					if (!(componentsInChildren4[m].transform == base.transform) && !allCollisions.Contains(componentsInChildren4[m].transform))
					{
						allCollisions.Add(componentsInChildren4[m].transform);
					}
				}
				break;
			}
			case MapCollisionEnum.COLLISION_BUMPOBJ:
			case MapCollisionEnum.COLLISION_BUMPOBJ2:
			{
				BoxCollider2D[] componentsInChildren5 = base.transform.GetComponentsInChildren<BoxCollider2D>();
				if (componentsInChildren5.Length > 1)
				{
					for (int n = 0; n < componentsInChildren5.Length; n++)
					{
						if (componentsInChildren5[n].gameObject.GetInstanceID() != base.gameObject.GetInstanceID())
						{
							allCollisions.Add(componentsInChildren5[n].transform);
						}
					}
				}
				Controller2D[] componentsInChildren6 = base.transform.GetComponentsInChildren<Controller2D>();
				for (int num = 0; num < componentsInChildren6.Length; num++)
				{
					allController2Ds.Add(componentsInChildren6[num]);
				}
				if (listsParam0.Any((string p) => p != ""))
				{
					bUseSE = true;
					base.SoundSource.Initial(OrangeSSType.ENEMY);
				}
				if (listsParam0[8] != "")
				{
					base.SoundSource.MaxDistance = int.Parse(listsParam0[8]);
				}
				actRunFunction = new Action[4];
				actRunFunction[0] = WaitTime;
				actRunFunction[1] = MoveCheckHitBlock;
				actRunFunction[2] = WaitTimeToReturn;
				actRunFunction[3] = ReturnToPos;
				break;
			}
			case MapCollisionEnum.COLLISION_DESTRUCTIBLE:
			{
				BoxCollider2D[] array = null;
				Animation[] array2 = null;
				if (dicKeyIntParam0.Count > 0 && dicKeyIntParam0.ContainsKey(StageUpdate.gDifficulty))
				{
					fParam0 = dicKeyIntParam0[StageUpdate.gDifficulty];
				}
				fTmpParam0 = fParam0;
				StageSceneObjParam[] componentsInChildren3 = base.transform.GetComponentsInChildren<StageSceneObjParam>(true);
				array = base.transform.GetComponentsInChildren<BoxCollider2D>(true);
				base.transform.GetComponentsInChildren<MeshRenderer>(true);
				array2 = base.transform.GetComponentsInChildren<Animation>(true);
				for (int k = 0; k < componentsInChildren3.Length; k++)
				{
					componentsInChildren3[k].InitMeshMaterial();
					if (fParam3 > 0f)
					{
						componentsInChildren3[k].fContinueExplosion = fParam3;
						componentsInChildren3[k].nContinueExplosionGroup = nParam2;
					}
				}
				for (int l = 0; l < array.Length; l++)
				{
					allCollisions.Add(array[l].transform);
					if (!(array[l].transform.GetComponent<EventPointBase>() != null) && array[l].transform.gameObject.GetInstanceID() != base.gameObject.GetInstanceID())
					{
						StageHurtObj stageHurtObj = array[l].gameObject.AddComponent<StageHurtObj>();
						stageHurtObj.nBitParam = nParam1;
						stageHurtObj.HurtCB += Hurt;
						stageHurtObj.listSkillID = listnParam0;
						stageHurtObj.tSSOPs = componentsInChildren3;
						stageHurtObj.tAnimations = array2;
						stageHurtObj.tMapCollisionEvent = this;
						stageHurtObj.nMaxHP = (int)fTmpParam0;
					}
				}
				break;
			}
			case MapCollisionEnum.COLLISION_KICKOBJ:
			{
				if (base.transform.childCount <= 0)
				{
					break;
				}
				StageSceneObjParam[] componentsInChildren = base.transform.GetComponentsInChildren<StageSceneObjParam>();
				for (int i = 0; i < componentsInChildren.Length; i++)
				{
					componentsInChildren[i].CheckAlphaMaterial();
				}
				for (int j = 0; j < base.transform.childCount; j++)
				{
					Transform child = base.transform.GetChild(j);
					allCollisions.Add(child);
					ListHitTmpCollisions.Add(child);
					BoxCollider2D[] componentsInChildren2 = child.GetComponentsInChildren<BoxCollider2D>();
					foreach (BoxCollider2D boxCollider2D in componentsInChildren2)
					{
						if (!ListHitTmpCollisions.Contains(boxCollider2D.transform))
						{
							ListHitTmpCollisions.Add(boxCollider2D.transform);
						}
					}
				}
				break;
			}
			}
		}

		private void FindName(Transform tTrans, string name, Action<Transform> cb)
		{
			int childCount = tTrans.childCount;
			for (int i = 0; i < childCount; i++)
			{
				Transform child = tTrans.GetChild(i);
				if (child.gameObject.name == name)
				{
					if (cb != null)
					{
						cb(child);
					}
				}
				else if (child.childCount > 0)
				{
					FindName(child, name, cb);
				}
			}
		}

		private void AddCollider(Transform tTrans)
		{
			BoxCollider2D[] componentsInChildren = tTrans.GetComponentsInChildren<BoxCollider2D>();
			Renderer component = tTrans.GetComponent<Renderer>();
			if (component != null)
			{
				if (bStartUpdtate)
				{
					if (component.transform.parent != null && component.transform.parent.gameObject.name.Contains("INVERT"))
					{
						component.material.SetFloat("_ScrollX", fParam0 / 0.052f);
					}
					else
					{
						component.material.SetFloat("_ScrollX", fParam0 / -0.052f);
					}
				}
				else
				{
					component.material.SetFloat("_ScrollX", 0f);
				}
			}
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				if (!allCollisions.Contains(componentsInChildren[i].transform))
				{
					allCollisions.Add(componentsInChildren[i].transform);
				}
			}
		}

		private void HurtCPCheck(StageObjBase tShotSOB, Vector3? vDir = null)
		{
			foreach (KeyValuePair<float, int> item in dicKeyIntParam1)
			{
				if (fTmpParam0 * item.Key >= fParam0 && fTmpParam0 * item.Key < fTmpParam1)
				{
					EventManager.StageEventCall stageEventCall = new EventManager.StageEventCall();
					stageEventCall.nID = item.Value;
					if (tShotSOB == null)
					{
						stageEventCall.tTransform = null;
					}
					else
					{
						stageEventCall.tTransform = tShotSOB.transform;
					}
					Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.STAGE_EVENT_CALL, stageEventCall);
				}
			}
			foreach (KeyValuePair<float, int> item2 in dicKeyIntParam2)
			{
				if (fTmpParam0 * item2.Key >= fParam0 && fTmpParam0 * item2.Key < fTmpParam1 && item2.Value != 0)
				{
					StageResManager.LoadStageItemModel(item2.Value, null, base.transform.position, vDir, allCollisions);
				}
			}
			if (fParam0 <= 0f)
			{
				fParam0 = 0f;
				StageSceneObjParam[] componentsInChildren = GetComponentsInChildren<StageSceneObjParam>();
				bool flag = false;
				StageSceneObjParam[] array = componentsInChildren;
				foreach (StageSceneObjParam stageSceneObjParam in array)
				{
					if (!flag)
					{
						flag = (stageSceneObjParam.bCanPlayBrokenSE = true);
					}
					stageSceneObjParam.BrokenStageSceneObj();
				}
				if (nParam0 != 0)
				{
					EventManager.StageEventCall stageEventCall2 = new EventManager.StageEventCall();
					stageEventCall2.nID = nParam0;
					if (tShotSOB == null)
					{
						stageEventCall2.tTransform = null;
					}
					else
					{
						stageEventCall2.tTransform = tShotSOB.transform;
					}
					Singleton<GenericEventManager>.Instance.NotifyEvent(EventManager.ID.STAGE_EVENT_CALL, stageEventCall2);
				}
			}
			else if (fTmpParam0 * 0.3f >= fParam0 && fTmpParam0 * 0.3f < fTmpParam1)
			{
				StageSceneObjParam[] array = GetComponentsInChildren<StageSceneObjParam>();
				foreach (StageSceneObjParam obj in array)
				{
					obj.SwitchAnimatorInStageSceneObj(false);
					obj.WoundedStageSceneObj();
				}
			}
		}

		private int Hurt(int dmg, Vector3 vDir, int nSkillID, StageObjBase tShotSOB)
		{
			if (fParam0 <= 0f)
			{
				return 0;
			}
			fTmpParam1 = fParam0;
			fParam4 = dmg;
			if (fParam1 != 0f)
			{
				fParam4 = fParam4 * (100f - fParam1) * 0.01f;
			}
			if (fParam2 > 0f && fParam2 < fParam4)
			{
				fParam4 = fParam2;
			}
			fParam0 -= fParam4;
			if (listnParam1.Contains(nSkillID))
			{
				fParam0 = 0f;
			}
			if (((uint)nParam1 & 4u) != 0 && fParam0 <= 0f)
			{
				fParam0 = 1f;
			}
			HurtCPCheck(tShotSOB, vDir);
			if (tShotSOB == null)
			{
				StageUpdate.SyncStageObj(sSyncID, 0, "," + fParam0.ToString("0.000"), true);
			}
			else
			{
				StageUpdate.SyncStageObj(sSyncID, 0, tShotSOB.sNetSerialID + "," + fParam0.ToString("0.000"), true);
			}
			return (int)fParam0;
		}

		private IEnumerator MeshExplosion(Mesh[] allMeshs)
		{
			float fTime = 2f;
			while (fTime >= 0f)
			{
				foreach (Mesh mesh in allMeshs)
				{
					Vector3[] vertices = mesh.vertices;
					Vector3[] normals = mesh.normals;
					for (int j = 0; j < vertices.Length; j++)
					{
						vertices[j] += normals[j] * Time.deltaTime;
					}
					fTime -= Time.deltaTime;
					mesh.vertices = vertices;
				}
				yield return CoroutineDefine._waitForEndOfFrame;
			}
		}

		private void OnDrawGizmos()
		{
			Gizmos.color = new Color(1f, 0.5f, 0.7f);
			Gizmos.DrawSphere(base.transform.position, 0.1f);
			Gizmos.DrawWireCube(base.transform.position, new Vector3(1.2f, 1.2f, 1.2f));
		}

		public override int GetTypeID()
		{
			return 13;
		}

		public override string GetTypeString()
		{
			return StageObjType.STAGECOLLISION_OBJ.ToString();
		}

		public override bool IsCanAddChild()
		{
			return true;
		}

		public override bool IsMapDependObj()
		{
			return true;
		}

		public override string GetSaveString()
		{
			return GetTypeString();
		}

		public override void LoadByString(string sLoad)
		{
			string text = sLoad.Substring(GetTypeString().Length);
			int result = 0;
			if (int.TryParse(text.Substring(0, 1), out result))
			{
				text = text.Replace(";" + text[0], ",");
				text = text.Substring(1);
			}
			else
			{
				text = text.Replace(";", ",");
			}
			MapCollisionSL mapCollisionSL = JsonUtility.FromJson<MapCollisionSL>(text);
			EventB2D = GetComponent<BoxCollider2D>();
			EventB2D.offset = new Vector2(mapCollisionSL.B2DX, mapCollisionSL.B2DY);
			EventB2D.size = new Vector2(mapCollisionSL.B2DW, mapCollisionSL.B2DH);
			mapEvent = (MapCollisionEnum)mapCollisionSL.mapEvent;
			bStartAtInit = mapCollisionSL.bS;
			nSetID = mapCollisionSL.nSetID;
			string[] array = mapCollisionSL.param_save.Split(',');
			int nIndex = 0;
			int result2;
			switch (mapEvent)
			{
			case MapCollisionEnum.COLLISION_TRACK:
				float.TryParse(array[nIndex++], out fParam0);
				if (array.Length > nIndex)
				{
					int.TryParse(array[nIndex++], out nParam0);
				}
				break;
			case MapCollisionEnum.COLLISION_BUMPOBJ:
			case MapCollisionEnum.COLLISION_BUMPOBJ2:
			{
				int.TryParse(array[nIndex++], out nParam0);
				int.TryParse(array[nIndex++], out nParam1);
				float.TryParse(array[nIndex++], out fParam0);
				float.TryParse(array[nIndex++], out fParam1);
				float.TryParse(array[nIndex++], out fParam2);
				float.TryParse(array[nIndex++], out fParam3);
				float.TryParse(array[nIndex++], out fParam4);
				int num = 0;
				while (array.Length > nIndex)
				{
					listsParam0.Add(array[nIndex++]);
					num++;
					if (num >= 8)
					{
						break;
					}
				}
				while (num < 8)
				{
					num++;
					listsParam0.Add("");
				}
				if (array.Length > nIndex)
				{
					int.TryParse(array[nIndex++], out nParam2);
				}
				if (array.Length > nIndex)
				{
					num++;
					listsParam0.Add(array[nIndex++]);
				}
				while (num < 9)
				{
					num++;
					listsParam0.Add("");
				}
				break;
			}
			case MapCollisionEnum.COLLISION_DESTRUCTIBLE:
				float.TryParse(array[nIndex++], out fParam0);
				float.TryParse(array[nIndex++], out fParam1);
				int.TryParse(array[nIndex++], out nParam0);
				if (array.Length > nIndex)
				{
					int.TryParse(array[nIndex++], out nParam1);
				}
				if (array.Length > nIndex)
				{
					int.TryParse(array[nIndex++], out result2);
					for (int k = 0; k < result2; k++)
					{
						int result5;
						int.TryParse(array[nIndex++], out result5);
						listnParam0.Add(result5);
					}
				}
				if (array.Length > nIndex)
				{
					int.TryParse(array[nIndex++], out result2);
					for (int l = 0; l < result2; l++)
					{
						int result6;
						int.TryParse(array[nIndex++], out result6);
						int result7;
						int.TryParse(array[nIndex++], out result7);
						dicKeyIntParam0.Add(result6, result7);
					}
				}
				if (array.Length > nIndex)
				{
					int.TryParse(array[nIndex++], out result2);
					for (int m = 0; m < result2; m++)
					{
						int result8;
						int.TryParse(array[nIndex++], out result8);
						listnParam1.Add(result8);
					}
				}
				if (array.Length > nIndex)
				{
					int.TryParse(array[nIndex++], out result2);
					for (int n = 0; n < result2; n++)
					{
						float result9;
						float.TryParse(array[nIndex++], out result9);
						int result10;
						int.TryParse(array[nIndex++], out result10);
						dicKeyIntParam1.Add(result9, result10);
					}
				}
				if (array.Length > nIndex)
				{
					int.TryParse(array[nIndex++], out result2);
					for (int num2 = 0; num2 < result2; num2++)
					{
						float result11;
						float.TryParse(array[nIndex++], out result11);
						int result12;
						int.TryParse(array[nIndex++], out result12);
						dicKeyIntParam2.Add(result11, result12);
						StageResManager.LoadStageItemModel(result12);
					}
				}
				if (array.Length > nIndex)
				{
					float.TryParse(array[nIndex++], out fParam2);
				}
				if (array.Length > nIndex)
				{
					float.TryParse(array[nIndex++], out fParam3);
				}
				if (array.Length > nIndex)
				{
					int.TryParse(array[nIndex++], out nParam2);
				}
				break;
			case MapCollisionEnum.COLLISION_KICKOBJ:
				float.TryParse(array[nIndex++], out fParam0);
				float.TryParse(array[nIndex++], out fParam1);
				float.TryParse(array[nIndex++], out fParam2);
				float.TryParse(array[nIndex++], out fParam3);
				float.TryParse(array[nIndex++], out fParam4);
				if (array.Length > nIndex)
				{
					int.TryParse(array[nIndex++], out result2);
					for (int i = 0; i < result2; i++)
					{
						int result3;
						int.TryParse(array[nIndex++], out result3);
						listnParam0.Add(result3);
					}
				}
				if (array.Length > nIndex)
				{
					int.TryParse(array[nIndex++], out result2);
					for (int j = 0; j < result2; j++)
					{
						float result4;
						float.TryParse(array[nIndex++], out result4);
						listfParam0.Add(result4);
					}
				}
				if (listnParam0.Count < 4)
				{
					while (listnParam0.Count < 4)
					{
						listnParam0.Add(0);
					}
					if (listnParam0[3] < 8000)
					{
						listnParam0[3] = 8000;
					}
				}
				while (listfParam0.Count < 3)
				{
					listfParam0.Add(0f);
				}
				int.TryParse(array[nIndex++], out nParam0);
				if (((uint)nParam0 & (true ? 1u : 0u)) != 0)
				{
					listsParam0.Add(array[nIndex++]);
					listsParam0.Add(array[nIndex++]);
					listsParam0.Add(array[nIndex++]);
				}
				break;
			case MapCollisionEnum.COLLISION_QUICKSAND:
			{
				TryLoadFloat(array, ref nIndex, out fParam0);
				TryLoadFloat(array, ref nIndex, out fParam1);
				TryLoadFloat(array, ref nIndex, out fParam2);
				TryLoadFloat(array, ref nIndex, out fParam3);
				TryLoadFloat(array, ref nIndex, out fParam4);
				int num = 0;
				while (array.Length > nIndex)
				{
					listsParam0.Add(array[nIndex++]);
					num++;
					if (num >= 4)
					{
						break;
					}
				}
				while (listsParam0.Count < 4)
				{
					listsParam0.Add("");
				}
				break;
			}
			}
			GameObject[] rootGameObjects = SceneManager.GetActiveScene().GetRootGameObjects();
			StageUpdate stageUpdate = null;
			for (int num3 = 0; num3 < rootGameObjects.Length; num3++)
			{
				if (rootGameObjects[num3].name == "StageUpdate")
				{
					stageUpdate = rootGameObjects[num3].GetComponent<StageUpdate>();
					break;
				}
			}
			result2 = mapCollisionSL.Datas.Count;
			tMixBojSyncData = new StageUpdate.MixBojSyncData();
			if (stageUpdate != null)
			{
				stageUpdate.SetSyncStageFunc(sSyncID, tMixBojSyncData.OnSyncStageFunc);
				stageUpdate.AddMixBojSyncData(tMixBojSyncData);
				tMixBojSyncData.SetSyncMixStageFunc(sSyncID + "-0", OnSyncStageObj);
			}
			for (int num4 = 0; num4 < result2; num4++)
			{
				bool flag = (bool)stageUpdate;
				if (mapCollisionSL.Datas[num4].bunldepath == "")
				{
					Debug.LogError("prefab has no bundle path:" + mapCollisionSL.Datas[num4].path);
					continue;
				}
				int num5 = mapCollisionSL.Datas[num4].path.LastIndexOf("/");
				string text2 = mapCollisionSL.Datas[num4].path.Substring(num5 + 1);
				text2 = text2.Substring(0, text2.Length - 7);
				StageUpdate.LoadCallBackObj loadCallBackObj = new StageUpdate.LoadCallBackObj();
				loadCallBackObj.loadStageObjData = mapCollisionSL.Datas[num4];
				loadCallBackObj.i = num4;
				loadCallBackObj.lcb = StageLoadEndCall;
				loadCallBackObj.objParam0 = sSyncID + "-" + (num4 + 1);
				stageUpdate.AddSubLoadAB(loadCallBackObj);
				MonoBehaviourSingleton<AssetsBundleManager>.Instance.GetAssetAndAsyncLoad<UnityEngine.Object>(mapCollisionSL.Datas[num4].bunldepath, text2, loadCallBackObj.LoadCB);
			}
			sSyncID += "-0";
		}

		private void StageLoadEndCall(StageUpdate.LoadCallBackObj tObj, UnityEngine.Object asset)
		{
			int i = tObj.i;
			GameObject gameObject = UnityEngine.Object.Instantiate(asset) as GameObject;
			StageObjData stageObjData = (StageObjData)tObj.loadStageObjData;
			gameObject.transform.parent = base.transform;
			gameObject.transform.localPosition = stageObjData.position;
			gameObject.transform.localScale = stageObjData.scale;
			gameObject.transform.localRotation = stageObjData.rotate;
			gameObject.name = stageObjData.name;
			StageUpdate.LoadProperty(null, gameObject, stageObjData.property, null, tObj.objParam0 as string, tMixBojSyncData);
		}

		public override void SyncNowStatus()
		{
			base.SyncNowStatus();
			string text = "";
			MapCollisionEnum mapCollisionEnum = mapEvent;
			if (mapCollisionEnum == MapCollisionEnum.COLLISION_DESTRUCTIBLE)
			{
				text += fParam0;
			}
			if (text != "")
			{
				StageUpdate.SyncStageObj(sSyncID, 1, text);
			}
		}

		public override void OnSyncStageObj(string sIDKey, int nKey1, string smsg)
		{
			base.OnSyncStageObj(sIDKey, nKey1, smsg);
			if (!StageUpdate.gbStageReady)
			{
				return;
			}
			string[] array = smsg.Split(',');
			switch (nKey1)
			{
			case 0:
			{
				MapCollisionEnum mapCollisionEnum = mapEvent;
				if (mapCollisionEnum == MapCollisionEnum.COLLISION_DESTRUCTIBLE && !(fParam0 <= 0f))
				{
					fTmpParam1 = fParam0;
					fParam0 = float.Parse(array[1]);
					HurtCPCheck(StageResManager.GetStageUpdate().GetSOBByNetSerialID(array[0]));
				}
				break;
			}
			case 1:
			{
				MapCollisionEnum mapCollisionEnum = mapEvent;
				if (mapCollisionEnum == MapCollisionEnum.COLLISION_DESTRUCTIBLE && !(fParam0 <= 0f))
				{
					fTmpParam1 = fParam0;
					fParam0 = float.Parse(array[0]);
					HurtCPCheck(null);
				}
				break;
			}
			}
		}
	}
}
