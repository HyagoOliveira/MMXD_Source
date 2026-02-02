using System.Collections.Generic;
using UnityEngine;

namespace StageLib
{
	[ExecuteInEditMode]
	public class StageEnemy : StageSLBase
	{
		private enum StageEnemyEnum
		{
			SPAWN = 0,
			SYNC_HP = 1,
			DEAD = 2,
			RqSync = 3
		}

		public int GroupID;

		private int LastGroupID = -1;

		public bool bBack;

		private bool bLastBack;

		public bool bReBorn;

		public float fTimeReBorn = 1f;

		public bool bBornAtStart = true;

		public int nSCEID;

		public int nStickWall;

		public float fAiRange;

		public float fAiRangeY;

		public float fAiOffsetX;

		public float fAiOffsetY;

		public int _iPatrolPathId;

		private bool _bIsLoop;

		private int _nMoveSpeed;

		private List<Vector3> _lsPatrolPath = new List<Vector3>();

		public int _nSummonEventId = 999;

		public int _nStageCustomType;

		public int[] _nStageCustomParams = new int[3];

		public EnemyControllerBase enemySpawn;

		private bool bNeedSpawnGG;

		private float fWaitTime;

		private void LateUpdate()
		{
			UpdateModel();
			if (bNeedSpawnGG)
			{
				SpawnGG();
			}
		}

		public static int GetEnemyIDByGroupID(int inGroupID)
		{
			MOB_TABLE[] allMob = ManagedSingleton<OrangeTableHelper>.Instance.GetAllMob();
			for (int i = 0; i < allMob.Length; i++)
			{
				if (inGroupID == allMob[i].n_GROUP)
				{
					return allMob[i].n_ID;
				}
			}
			return -1;
		}

		public static int GetStageCustomParamsType(string modelName)
		{
			int result = 0;
			switch (modelName)
			{
			case "enemy_em056":
			case "enemy_em183":
				result = 1;
				break;
			case "event4_human_1":
				result = 2;
				break;
			}
			return result;
		}

		public void UpdateModel()
		{
			if (StageUpdate.gbStageReady && LastGroupID != GroupID && GroupID != 0)
			{
				LastGroupID = GroupID;
				while (base.transform.childCount > 0)
				{
					Object.Destroy(base.transform.GetChild(0).gameObject);
				}
				bNeedSpawnGG = true;
			}
		}

		public void HurtCB(StageObjBase tSOB)
		{
			if ((int)tSOB.Hp <= 0)
			{
				if (enemySpawn != null)
				{
					StageUpdate.RemoveEnemy(enemySpawn);
					enemySpawn = null;
				}
				if (bReBorn)
				{
					bNeedSpawnGG = true;
				}
				tSOB.HurtActions -= HurtCB;
			}
		}

		private void CallEnemySpawn()
		{
			bLastBack = bBack;
			if (!(enemySpawn != null))
			{
				int num = 0;
				if (((uint)nStickWall & 0x20u) != 0)
				{
					num |= 4;
				}
				if (((uint)nStickWall & 0x100u) != 0)
				{
					num |= 8;
				}
				enemySpawn = StageUpdate.StageSpawnEnemy(LastGroupID, sSyncID, num, nSCEID, fAiRange, fAiRangeY, fAiOffsetX, fAiOffsetY);
				if (enemySpawn != null)
				{
					Vector3 position = base.transform.position;
					enemySpawn.SetPositionAndRotation(position, bLastBack);
					enemySpawn.HurtActions += HurtCB;
					enemySpawn.SetActive(true);
					StageUpdate.SyncStageObj(sSyncID, 0, "");
					CheckEnemyPosition(position);
					enemySpawn.SetPatrolPath(_bIsLoop, _nMoveSpeed, _lsPatrolPath.ToArray());
					enemySpawn.SetSummonEventID(_nSummonEventId);
					enemySpawn.SetStageCustomParams(_nStageCustomType, _nStageCustomParams);
				}
			}
		}

		private void CheckEnemyPosition(Vector3 tPosition)
		{
			if (nStickWall == 0)
			{
				return;
			}
			float num = 9999f;
			Vector2[] array = new Vector2[4]
			{
				Vector2.down,
				Vector2.up,
				Vector2.left,
				Vector2.right
			};
			float z = 0f;
			float[] array2 = new float[4] { 0f, 180f, 270f, 90f };
			for (int i = 0; i < 4; i++)
			{
				if ((nStickWall & (1 << i)) == 0)
				{
					continue;
				}
				RaycastHit2D[] array3 = Physics2D.RaycastAll(base.transform.position, array[i], float.PositiveInfinity, LayerMask.GetMask("Block", "SemiBlock"));
				for (int j = 0; j < array3.Length; j++)
				{
					RaycastHit2D raycastHit2D = array3[j];
					if (raycastHit2D.transform != enemySpawn.Controller.Collider2D.transform && raycastHit2D.distance < num)
					{
						tPosition = raycastHit2D.point;
						num = raycastHit2D.distance;
						z = array2[i];
					}
				}
			}
			enemySpawn.SetPositionAndRotation(tPosition, bLastBack);
			if (bBack)
			{
				enemySpawn.transform.localRotation = enemySpawn.transform.localRotation * Quaternion.Euler(0f, 0f, z);
			}
			else
			{
				enemySpawn.transform.localRotation = enemySpawn.transform.localRotation * Quaternion.Euler(0f, 0f, z);
			}
		}

		public void SpawnGG()
		{
			if (MonoBehaviourSingleton<OrangeSceneManager>.Instance.MainCamera == null)
			{
				return;
			}
			if (!bBornAtStart)
			{
				fWaitTime += Time.deltaTime;
				if (fWaitTime < fTimeReBorn)
				{
					return;
				}
			}
			if (!StageUpdate.bIsHost)
			{
				StageUpdate.SyncStageObj(sSyncID, 3, "", true);
				return;
			}
			fWaitTime = 0f;
			bBornAtStart = false;
			bNeedSpawnGG = false;
			CallEnemySpawn();
		}

		public override int GetTypeID()
		{
			return 3;
		}

		public override string GetTypeString()
		{
			return StageObjType.ENEMY_OBJ.ToString();
		}

		public override string GetSaveString()
		{
			string typeString = GetTypeString();
			typeString += GroupID;
			typeString = ((!bBack) ? (typeString + ":0") : (typeString + ":1"));
			typeString = ((!bReBorn) ? (typeString + ":0") : (typeString + ":1"));
			typeString = typeString + ":" + fTimeReBorn.ToString("0.0000");
			typeString = ((!bBornAtStart) ? (typeString + ":0") : (typeString + ":1"));
			typeString = typeString + ":" + nSCEID;
			typeString = typeString + ":" + nStickWall;
			typeString = typeString + ":" + fAiRange.ToString("0.0000");
			updatePatrolPathData();
			typeString = typeString + ":" + _iPatrolPathId;
			typeString = ((!_bIsLoop) ? (typeString + ":0") : (typeString + ":1"));
			typeString += ":";
			for (int i = 0; i < _lsPatrolPath.Count; i++)
			{
				typeString = typeString + "v" + _lsPatrolPath[i].x.ToString("0.0000");
				typeString = typeString + "v" + _lsPatrolPath[i].y.ToString("0.0000");
				typeString = typeString + "v" + _lsPatrolPath[i].z.ToString("0.0000");
			}
			typeString = typeString + ":" + _nMoveSpeed;
			typeString = typeString + ":" + _nSummonEventId;
			typeString = typeString + ":" + fAiRangeY.ToString("0.0000");
			typeString = typeString + ":" + fAiOffsetX.ToString("0.0000");
			typeString = typeString + ":" + fAiOffsetY.ToString("0.0000");
			typeString = typeString + ":" + _nStageCustomType;
			typeString = typeString + ":" + _nStageCustomParams[0];
			typeString = typeString + ":" + _nStageCustomParams[1];
			return typeString + ":" + _nStageCustomParams[2];
		}

		private void updatePatrolPathData()
		{
		}

		public override void LoadByString(string sLoad)
		{
			string[] array = sLoad.Substring(GetTypeString().Length).Split(':');
			GroupID = int.Parse(array[0]);
			int num = 1;
			if (array.Length > num)
			{
				bBack = false;
				if (int.Parse(array[num]) == 1)
				{
					bBack = true;
				}
			}
			else
			{
				bBack = false;
			}
			num++;
			if (array.Length > num)
			{
				bReBorn = false;
				if (int.Parse(array[num]) == 1)
				{
					bReBorn = true;
				}
			}
			else
			{
				bReBorn = false;
			}
			num++;
			if (array.Length > num)
			{
				fTimeReBorn = float.Parse(array[num]);
			}
			num++;
			if (array.Length > num)
			{
				bBornAtStart = false;
				if (int.Parse(array[num]) == 1)
				{
					bBornAtStart = true;
				}
			}
			else
			{
				bBornAtStart = false;
			}
			num++;
			if (array.Length > num)
			{
				nSCEID = int.Parse(array[num]);
			}
			num++;
			if (array.Length > num)
			{
				nStickWall = int.Parse(array[num]);
			}
			else
			{
				nStickWall = 0;
			}
			num++;
			if (array.Length > num)
			{
				fAiRange = float.Parse(array[num]);
			}
			else
			{
				fAiRange = 0f;
			}
			num++;
			_iPatrolPathId = ((array.Length > num) ? int.Parse(array[num]) : 0);
			num++;
			if (array.Length > num)
			{
				_bIsLoop = int.Parse(array[num]) == 1;
			}
			else
			{
				_bIsLoop = false;
			}
			num++;
			if (array.Length > num)
			{
				string[] array2 = array[num].Split('v');
				for (int i = 1; i < array2.Length; i += 3)
				{
					float x = float.Parse(array2[i]);
					float y = float.Parse(array2[i + 1]);
					float z = float.Parse(array2[i + 2]);
					_lsPatrolPath.Add(new Vector3(x, y, z));
				}
			}
			num++;
			if (array.Length > num)
			{
				_nMoveSpeed = int.Parse(array[num]);
			}
			num++;
			if (array.Length > num)
			{
				_nSummonEventId = int.Parse(array[num]);
			}
			num++;
			if (array.Length > num)
			{
				fAiRangeY = float.Parse(array[num]);
			}
			else
			{
				fAiRangeY = 0f;
			}
			num++;
			if (array.Length > num)
			{
				fAiOffsetX = float.Parse(array[num]);
			}
			else
			{
				fAiOffsetX = 0f;
			}
			num++;
			if (array.Length > num)
			{
				fAiOffsetY = float.Parse(array[num]);
			}
			else
			{
				fAiOffsetY = 0f;
			}
			num++;
			_nStageCustomType = ((array.Length > num) ? int.Parse(array[num]) : 0);
			num++;
			_nStageCustomParams[0] = ((array.Length > num) ? int.Parse(array[num]) : 0);
			num++;
			_nStageCustomParams[1] = ((array.Length > num) ? int.Parse(array[num]) : 0);
			num++;
			_nStageCustomParams[2] = ((array.Length > num) ? int.Parse(array[num]) : 0);
			StageResManager.LoadEnemy(GroupID);
		}

		public override void SyncNowStatus()
		{
			if (LastGroupID != GroupID)
			{
				return;
			}
			if (enemySpawn != null)
			{
				string text = enemySpawn.Hp.ToString();
				text = text + "," + enemySpawn.HealHp;
				text = text + "," + enemySpawn.DmgHp;
				for (int i = 0; i < enemySpawn.PartHp.Length; i++)
				{
					text = text + "," + enemySpawn.PartHp[i];
				}
				Vector3 position = enemySpawn.transform.position;
				StageUpdate.SyncStageObj(sSyncID, 1, position.x + "," + position.y + "," + text);
			}
			else
			{
				StageUpdate.SyncStageObj(sSyncID, 2, "");
			}
		}

		public override void OnSyncStageObj(string sIDKey, int nKey1, string smsg)
		{
			switch (nKey1)
			{
			case 0:
				LastGroupID = GroupID;
				bLastBack = bBack;
				fWaitTime = 0f;
				bBornAtStart = false;
				bNeedSpawnGG = false;
				CallEnemySpawn();
				break;
			case 1:
				LastGroupID = GroupID;
				bLastBack = bBack;
				fWaitTime = 0f;
				bBornAtStart = false;
				bNeedSpawnGG = false;
				if (enemySpawn == null)
				{
					int num = 0;
					if (((uint)nStickWall & 0x20u) != 0)
					{
						num |= 4;
					}
					if (((uint)nStickWall & 0x100u) != 0)
					{
						num |= 8;
					}
					enemySpawn = StageUpdate.StageSpawnEnemy(LastGroupID, sSyncID, num, nSCEID, fAiRange, fAiRangeY, fAiOffsetX, fAiOffsetY);
					if (enemySpawn != null)
					{
						string[] array = smsg.Split(',');
						Vector3 zero = Vector3.zero;
						int num2 = 0;
						zero.x = float.Parse(array[num2++]);
						zero.y = float.Parse(array[num2++]);
						enemySpawn.SetPositionAndRotation(zero, bLastBack);
						enemySpawn.Hp = int.Parse(array[num2++]);
						enemySpawn.HealHp = int.Parse(array[num2++]);
						enemySpawn.DmgHp = int.Parse(array[num2++]);
						enemySpawn.UpdateHurtAction();
						for (int i = num2; i < array.Length; i++)
						{
							enemySpawn.PartHp[i] = int.Parse(array[i]);
						}
						enemySpawn.HurtActions += HurtCB;
						CheckEnemyPosition(zero);
						enemySpawn.SetPatrolPath(_bIsLoop, _nMoveSpeed, _lsPatrolPath.ToArray());
						enemySpawn.SetSummonEventID(_nSummonEventId);
						enemySpawn.SetStageCustomParams(_nStageCustomType, _nStageCustomParams);
						enemySpawn.SetActive(true);
					}
				}
				else
				{
					string[] array2 = smsg.Split(',');
					Vector3 zero2 = Vector3.zero;
					int num3 = 0;
					zero2.x = float.Parse(array2[num3++]);
					zero2.y = float.Parse(array2[num3++]);
					enemySpawn.SetPositionAndRotation(zero2, bLastBack);
					enemySpawn.Hp = int.Parse(array2[num3++]);
					enemySpawn.HealHp = int.Parse(array2[num3++]);
					enemySpawn.DmgHp = int.Parse(array2[num3++]);
					enemySpawn.UpdateHurtAction();
					for (int j = num3; j < array2.Length; j++)
					{
						enemySpawn.PartHp[j] = int.Parse(array2[j]);
					}
					CheckEnemyPosition(zero2);
				}
				break;
			case 2:
				LastGroupID = GroupID;
				bLastBack = bBack;
				if (enemySpawn != null)
				{
					enemySpawn.Hp = 0;
					enemySpawn.Hurt(new HurtPassParam());
				}
				break;
			case 3:
				SyncNowStatus();
				break;
			}
		}
	}
}
