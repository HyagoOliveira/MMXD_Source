using System.Collections.Generic;
using UnityEngine;

namespace StageLib
{
	public class MOECollisionUseData
	{
		public bool bNotFall;

		public bool bIsStop;

		public bool[] bPlaySE;

		public Vector2 dis;

		public float fTestLen;

		public float fTmpLen;

		public RaycastHit2D tSelfHit;

		public RaycastHit2D tTmpHit;

		public RaycastHit2D tCheckHitX;

		public Vector3 vTmpMove;

		public Vector3 vTmpMove2;

		public Vector2 nextdis;

		public Vector2 disnormal;

		public Vector2 vReflect;

		public List<Transform> tmpUse = new List<Transform>();

		public bool IsCheckEnd;

		public StageHurtObj tStageHurtObj;

		public bool bNeedAddTransList;

		public float fDirDotAns;

		public Vector2 fVBias;

		public float fTmp1;

		public bool bTmp1;

		public List<Vector3> listNormal = new List<Vector3>();

		public bool bHit;

		public int nHit;
	}
}
