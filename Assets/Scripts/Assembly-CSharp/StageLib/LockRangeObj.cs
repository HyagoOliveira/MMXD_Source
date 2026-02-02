using UnityEngine;

namespace StageLib
{
	public class LockRangeObj : MonoBehaviour
	{
		public Vector2 vLockLR = new Vector2(-9999f, 9999f);

		public Vector2 vLockTB = new Vector2(-9999f, 9999f);

		public int nNoBack;

		public int nReserveBack;

		public Vector2 vReserveBackLR = new Vector2(-9999f, 9999f);

		public Vector2 vReserveBackTB = new Vector2(-9999f, 9999f);

		public float cameraHHalf;

		public float cameraWHalf;

		private Controller2D tTarget;

		public Vector3 halfsize = Vector3.zero;

		private const float fBios = 0.02f;

		public const int nUILayer = 67108864;

		private StageObjBase tSOB;

		private bool bOutCheckEnd;

		private bool bNeedUpdate;

		public void Init()
		{
			if (!(tTarget != null))
			{
				tTarget = base.transform.GetComponent<Controller2D>();
				tSOB = base.transform.GetComponent<StageObjBase>();
				if (tTarget != null && tSOB != null)
				{
					Singleton<GenericEventManager>.Instance.AttachEvent<EventManager.LockRangeParam>(EventManager.ID.LOCK_RANGE, EventLockRange);
					tTarget.MoveEndCall += CheckLogicPosCall;
				}
				cameraHHalf = ManagedSingleton<StageHelper>.Instance.fCameraHHalf;
				cameraWHalf = ManagedSingleton<StageHelper>.Instance.fCameraWHalf;
			}
		}

		public bool CheckOutRange(Controller2D tController2D, Vector3 vMove)
		{
			if (tSOB.bIsNpcCpy)
			{
				return false;
			}
			Vector3 vector = tController2D.LogicPosition.vec3 + vMove;
			Vector3 vector2 = tController2D.transform.localPosition + vMove;
			halfsize = tController2D.Collider2D.bounds.size;
			if (vLockLR.x - (vector.x - halfsize.x) > 0.001f)
			{
				return true;
			}
			if (vLockLR.y - (vector.x + halfsize.x) < -0.001f)
			{
				return true;
			}
			if (vLockTB.y - (vector.y + halfsize.y) < -0.001f)
			{
				return true;
			}
			if (vector2.x - halfsize.x < vLockLR.x)
			{
				return true;
			}
			if (vector2.x + halfsize.x > vLockLR.y)
			{
				return true;
			}
			if (vector2.y + halfsize.y > vLockTB.y)
			{
				return true;
			}
			return false;
		}

		private void CheckLogicPosCall(Controller2D tController2D)
		{
			CheckLogicPos(tController2D);
		}

		private bool CheckBoundsOutMainCamera(ref Bounds tBounds)
		{
			Vector3 position = MonoBehaviourSingleton<OrangeSceneManager>.Instance.MainCamera.transform.position;
			if (!(tBounds.max.y >= position.y - cameraHHalf) || !(tBounds.min.y <= position.y + cameraHHalf) || !(tBounds.max.x >= position.x - cameraWHalf) || !(tBounds.min.x <= position.x + cameraWHalf))
			{
				return true;
			}
			return false;
		}

		public bool CheckLogicPos(Controller2D tController2D, bool bCheckNoOutRange = false, bool bCheckBottom = false)
		{
			if (tSOB.bIsNpcCpy)
			{
				return false;
			}
			Vector3 vector = tController2D.LogicPosition.vec3;
			Vector3 vector2 = base.transform.localPosition;
			halfsize = tController2D.Collider2D.bounds.size;
			bNeedUpdate = false;
			if (vLockLR.x - (vector.x - halfsize.x) > 0.001f)
			{
				vector.x = vLockLR.x + halfsize.x;
				bNeedUpdate = true;
			}
			if (vLockLR.y - (vector.x + halfsize.x) < -0.001f)
			{
				vector.x = vLockLR.y - halfsize.x;
				bNeedUpdate = true;
			}
			if (bCheckBottom && vLockTB.x - vector.y > 0.001f)
			{
				vector.y = vLockTB.x;
				bNeedUpdate = true;
			}
			if (vLockTB.y - (vector.y + halfsize.y) < -0.001f)
			{
				vector.y = vLockTB.y - halfsize.y;
				bNeedUpdate = true;
			}
			if (vector2.x - halfsize.x < vLockLR.x)
			{
				vector2.x = vLockLR.x + halfsize.x;
			}
			if (vector2.x + halfsize.x > vLockLR.y)
			{
				vector2.x = vLockLR.y - halfsize.x;
			}
			if (bCheckBottom && vector.y < vLockTB.x)
			{
				vector.y = vLockTB.x;
			}
			if (vector2.y + halfsize.y > vLockTB.y)
			{
				vector2.y = vLockTB.y - halfsize.y;
			}
			if (bNeedUpdate || bCheckNoOutRange)
			{
				Vector3 position = MonoBehaviourSingleton<OrangeSceneManager>.Instance.MainCamera.transform.position;
				Bounds tBounds = tController2D.GetNewNowBounds();
				bool flag = CheckBoundsOutMainCamera(ref tBounds);
				float distance = halfsize.y * 0.8f;
				bool flag2 = false;
				Vector3 vector3 = vector;
				Vector3 vector4 = vector2;
				while (!flag2)
				{
					tController2D.LogicPosition = new VInt3(vector);
					tController2D.UpdateRaycastOrigins();
					Vector2 bottomLeft = tController2D._raycastOrigins.bottomLeft;
					Vector2 up = Vector2.up;
					Vector2 vector5 = Vector2.right * tController2D._verticalRaySpacing;
					flag2 = true;
					for (int i = 0; i < tController2D.VerticalRayCount; i++)
					{
						int num = LayerMask.GetMask("Block");
						if (tController2D.Collisions.below)
						{
							num |= LayerMask.GetMask("SemiBlock");
						}
						RaycastHit2D raycastHit2D = Physics2D.Raycast(bottomLeft, up, distance, num);
						if ((bool)raycastHit2D)
						{
							vector.y = vector.y + raycastHit2D.collider.bounds.max.y - raycastHit2D.point.y + raycastHit2D.distance + 0.02f;
							vector2.y = vector2.y + raycastHit2D.collider.bounds.max.y - raycastHit2D.point.y + raycastHit2D.distance + 0.02f;
							bottomLeft.y = bottomLeft.y + raycastHit2D.collider.bounds.max.y - raycastHit2D.point.y + raycastHit2D.distance + 0.02f;
							RaycastHit2D raycastHit2D2 = Physics2D.Raycast(bottomLeft, Vector3.down, float.PositiveInfinity, num);
							if ((bool)raycastHit2D2 && raycastHit2D2.distance > 0f)
							{
								vector.y = vector.y - raycastHit2D2.distance + 0.02f;
								vector2.y = vector2.y - raycastHit2D2.distance + 0.02f;
							}
							flag2 = false;
							bNeedUpdate = true;
							break;
						}
						bottomLeft += vector5;
					}
				}
				tBounds = tController2D.GetNewNowBounds();
				if (!flag && CheckBoundsOutMainCamera(ref tBounds))
				{
					flag2 = false;
					vector = vector3;
					vector2 = vector4;
					while (!flag2)
					{
						tController2D.LogicPosition = new VInt3(vector);
						tController2D.UpdateRaycastOrigins();
						Vector2 bottomLeft2 = tController2D._raycastOrigins.bottomLeft;
						Vector2 up2 = Vector2.up;
						Vector2 vector6 = Vector2.right * tController2D._verticalRaySpacing;
						flag2 = true;
						for (int j = 0; j < tController2D.VerticalRayCount; j++)
						{
							int num2 = LayerMask.GetMask("Block");
							if (tController2D.Collisions.below)
							{
								num2 |= LayerMask.GetMask("SemiBlock");
							}
							RaycastHit2D raycastHit2D3 = Physics2D.Raycast(bottomLeft2, up2, distance, num2);
							if ((bool)raycastHit2D3)
							{
								vector3 = vector;
								vector.y = raycastHit2D3.collider.bounds.min.y - 0.02f - tController2D.Collider2D.bounds.size.y;
								vector2.y += vector.y - vector3.y;
								flag2 = false;
								bNeedUpdate = true;
								break;
							}
							bottomLeft2 += vector6;
						}
					}
				}
				base.transform.localPosition = vector2;
				if (tSOB.GetSOBType() == 1)
				{
					(tSOB as OrangeCharacter).vLastMovePt = vector2;
				}
			}
			switch (nNoBack)
			{
			case 1:
				if (vLockLR.x < MonoBehaviourSingleton<OrangeSceneManager>.Instance.MainCamera.transform.position.x - cameraWHalf)
				{
					vLockLR.x = MonoBehaviourSingleton<OrangeSceneManager>.Instance.MainCamera.transform.position.x - cameraWHalf;
					if (vLockLR.y - vLockLR.x < cameraWHalf * 2f)
					{
						vLockLR.x = vLockLR.y - cameraWHalf * 2f;
					}
				}
				break;
			case 2:
				if (vLockLR.y > MonoBehaviourSingleton<OrangeSceneManager>.Instance.MainCamera.transform.position.x + cameraWHalf)
				{
					vLockLR.y = MonoBehaviourSingleton<OrangeSceneManager>.Instance.MainCamera.transform.position.x + cameraWHalf;
					if (vLockLR.y - vLockLR.x < cameraWHalf * 2f)
					{
						vLockLR.y = vLockLR.x + cameraWHalf * 2f;
					}
				}
				break;
			case 3:
				if (vLockTB.x < vector.y - cameraHHalf)
				{
					vLockTB.x = vector.y - cameraHHalf;
					if (vLockTB.y - vLockTB.x < cameraHHalf * 2f)
					{
						vLockTB.x = vLockTB.y - cameraHHalf * 2f;
					}
				}
				break;
			case 4:
				if (vLockTB.y > vector.y + cameraHHalf)
				{
					vLockTB.y = vector.y + cameraHHalf;
					if (vLockTB.y - vLockTB.x < cameraHHalf * 2f)
					{
						vLockTB.y = vLockTB.x + cameraHHalf * 2f;
					}
				}
				break;
			}
			if (vector.y + halfsize.y * 4f < vLockTB.x)
			{
				if (!bOutCheckEnd)
				{
					RaycastHit2D[] array = Physics2D.RaycastAll(tController2D.transform.position, Vector2.up, float.MaxValue, 67108864);
					if (array.Length != 0)
					{
						for (int k = 0; k < array.Length; k++)
						{
							DeadAreaEvent component = array[k].transform.GetComponent<DeadAreaEvent>();
							if (component != null)
							{
								if (component.bCheckPlayer && tSOB as OrangeCharacter != null)
								{
									component.OnEventPlayer(tSOB as OrangeCharacter);
								}
								else if (component.bCheckEnemy && tSOB as EnemyControllerBase != null)
								{
									component.OnEventEnemy(tSOB as EnemyControllerBase);
								}
								bOutCheckEnd = true;
								break;
							}
						}
					}
				}
			}
			else
			{
				bOutCheckEnd = false;
			}
			return bNeedUpdate;
		}

		private void OnDestroy()
		{
			if (tTarget != null)
			{
				tTarget.MoveEndCall -= CheckLogicPosCall;
			}
			Singleton<GenericEventManager>.Instance.DetachEvent<EventManager.LockRangeParam>(EventManager.ID.LOCK_RANGE, EventLockRange);
		}

		private void EventLockRange(EventManager.LockRangeParam tLockRangeParam)
		{
			OrangeCharacter orangeCharacter = GetComponent<OrangeCharacter>();
			if (orangeCharacter == null)
			{
				RideBaseObj component = GetComponent<RideBaseObj>();
				if ((bool)component)
				{
					orangeCharacter = component.MasterPilot;
				}
			}
			if (orangeCharacter == null || orangeCharacter.sPlayerID != MonoBehaviourSingleton<GameServerService>.Instance.PlayerIdentify)
			{
				return;
			}
			float fMinX = tLockRangeParam.fMinX;
			float fMaxX = tLockRangeParam.fMaxX;
			float fMinY = tLockRangeParam.fMinY;
			float fMaxY = tLockRangeParam.fMaxY;
			if (tLockRangeParam.nMode == 0)
			{
				Vector2 vector = vLockLR;
				Vector2 vector2 = vLockTB;
				vLockLR.Set(fMinX, fMaxX);
				vLockTB.Set(fMinY, fMaxY);
				int? num = tLockRangeParam.nNoBack;
				int num2 = nNoBack;
				nNoBack = num ?? 0;
				if (nNoBack == 5)
				{
					nNoBack = nReserveBack;
					switch (nNoBack)
					{
					case 1:
						if (vReserveBackLR.x < vector.x)
						{
							vReserveBackLR.x = vector.x;
						}
						break;
					case 2:
						if (vReserveBackLR.y > vector.y)
						{
							vReserveBackLR.y = vector.y;
						}
						break;
					case 3:
						if (vReserveBackTB.x < vector2.x)
						{
							vReserveBackTB.x = vector2.x;
						}
						break;
					case 4:
						if (vReserveBackTB.y > vector2.y)
						{
							vReserveBackTB.y = vector2.y;
						}
						break;
					}
					vLockLR = vReserveBackLR;
					vLockTB = vReserveBackTB;
				}
				else
				{
					nReserveBack = num2;
					vReserveBackLR = vector;
					vReserveBackTB = vector2;
				}
			}
			else if (tLockRangeParam.nMode == 1)
			{
				if (vLockLR.x > tLockRangeParam.fMinX)
				{
					vLockLR.x = tLockRangeParam.fMinX;
				}
				if (vLockLR.y < tLockRangeParam.fMaxX)
				{
					vLockLR.y = tLockRangeParam.fMaxX;
				}
				if (vLockTB.x > tLockRangeParam.fMinY)
				{
					vLockTB.x = tLockRangeParam.fMinY;
				}
				if (vLockTB.y < tLockRangeParam.fMaxY)
				{
					vLockTB.y = tLockRangeParam.fMaxY;
				}
			}
			else if (tLockRangeParam.nMode == 2)
			{
				Vector3 vector3 = tLockRangeParam.vDir ?? Vector3.zero;
				int num3 = tLockRangeParam.nNoBack ?? 0;
				if (vector3.x > 0f)
				{
					if (((uint)num3 & 2u) != 0 || (num3 & 0x1E) == 0)
					{
						vLockLR.x += vector3.x;
					}
					if (((uint)num3 & 4u) != 0)
					{
						vLockLR.y += vector3.x;
					}
					if (vLockLR.y - vLockLR.x <= cameraWHalf * 2f)
					{
						if ((num3 & 1) == 0)
						{
							vLockLR.x = vLockLR.y - cameraWHalf * 2f;
						}
						else
						{
							vLockLR.y = vLockLR.x + cameraWHalf * 2f;
						}
					}
				}
				else
				{
					if (((uint)num3 & 4u) != 0 || (num3 & 0x1E) == 0)
					{
						vLockLR.y += vector3.x;
					}
					if (((uint)num3 & 2u) != 0)
					{
						vLockLR.x += vector3.x;
					}
					if (vLockLR.y - vLockLR.x <= cameraWHalf * 2f)
					{
						if ((num3 & 1) == 0)
						{
							vLockLR.y = vLockLR.x + cameraWHalf * 2f;
						}
						else
						{
							vLockLR.x = vLockLR.y - cameraWHalf * 2f;
						}
					}
				}
				if (vector3.y > 0f)
				{
					if (((uint)num3 & 0x10u) != 0 || (num3 & 0x1E) == 0)
					{
						vLockTB.x += vector3.y;
					}
					if (((uint)num3 & 8u) != 0)
					{
						vLockTB.y += vector3.y;
					}
					if (vLockTB.y - vLockTB.x <= cameraHHalf * 2f)
					{
						if ((num3 & 1) == 0)
						{
							vLockTB.x = vLockTB.y - cameraHHalf * 2f;
						}
						else
						{
							vLockTB.y = vLockTB.x + cameraHHalf * 2f;
						}
					}
					return;
				}
				if (((uint)num3 & 8u) != 0 || (num3 & 0x1E) == 0)
				{
					vLockTB.y += vector3.y;
				}
				if (((uint)num3 & 0x10u) != 0)
				{
					vLockTB.x += vector3.y;
				}
				if (vLockTB.y - vLockTB.x <= cameraHHalf * 2f)
				{
					if ((num3 & 1) == 0)
					{
						vLockTB.y = vLockTB.x + cameraHHalf * 2f;
					}
					else
					{
						vLockTB.x = vLockTB.y - cameraHHalf * 2f;
					}
				}
			}
			else if (tLockRangeParam.nMode == 3)
			{
				cameraHHalf = ManagedSingleton<StageHelper>.Instance.fCameraHHalf;
				cameraWHalf = ManagedSingleton<StageHelper>.Instance.fCameraWHalf;
			}
		}
	}
}
