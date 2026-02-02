using System;
using Newtonsoft.Json;
using UnityEngine;

namespace StageLib
{
	[RequireComponent(typeof(BoxCollider2D))]
	public class StageBlockWall : StageSLBase
	{
		[Serializable]
		public class StageBlockWallData
		{
			[JsonConverter(typeof(FloatConverter))]
			public float B2DX;

			[JsonConverter(typeof(FloatConverter))]
			public float B2DY;

			[JsonConverter(typeof(FloatConverter))]
			public float B2DW = 1f;

			[JsonConverter(typeof(FloatConverter))]
			public float B2DH = 1f;

			public int nLayer = 11;

			public int nSemiBlockMode;
		}

		public BoxCollider2D EventB2D;

		public int SemiBlockMode;

		private void OnDrawGizmos()
		{
			if (EventB2D == null)
			{
				EventB2D = GetComponent<BoxCollider2D>();
			}
			Gizmos.color = new Color(0.6f, 0.6f, 0.6f);
			Vector3 position = base.transform.position;
			Vector3 lossyScale = base.transform.lossyScale;
			position.x += EventB2D.offset.x * lossyScale.x;
			position.y += EventB2D.offset.y * lossyScale.y;
			Gizmos.DrawSphere(position, 0.1f);
			Gizmos.DrawWireCube(position, new Vector3(EventB2D.size.x * lossyScale.x, EventB2D.size.y * lossyScale.y, 1.2f));
		}

		public override int GetTypeID()
		{
			return 11;
		}

		public override string GetTypeString()
		{
			return StageObjType.BLOCKWALL_OBJ.ToString();
		}

		public override bool IsMapDependObj()
		{
			return true;
		}

		public override bool IsNeedClip()
		{
			return false;
		}

		public override string GetSaveString()
		{
			string typeString = GetTypeString();
			string text = JsonConvert.SerializeObject(new StageBlockWallData
			{
				B2DX = EventB2D.offset.x,
				B2DY = EventB2D.offset.y,
				B2DW = EventB2D.size.x,
				B2DH = EventB2D.size.y,
				nLayer = base.gameObject.layer,
				nSemiBlockMode = SemiBlockMode
			}, Formatting.None, JsonHelper.IgnoreLoopSetting());
			int num = 1;
			Transform parent = base.transform;
			while (parent.parent != null)
			{
				parent = parent.parent;
				num++;
			}
			text = text.Replace(",", ";" + num);
			return typeString + num + text;
		}

		public override void LoadByString(string sLoad)
		{
			string text = sLoad.Substring(GetTypeString().Length);
			text = text.Replace(";" + text[0], ",");
			text = text.Substring(1);
			StageBlockWallData stageBlockWallData = JsonUtility.FromJson<StageBlockWallData>(text);
			EventB2D = GetComponent<BoxCollider2D>();
			EventB2D.offset = new Vector2(stageBlockWallData.B2DX, stageBlockWallData.B2DY);
			EventB2D.size = new Vector2(stageBlockWallData.B2DW, stageBlockWallData.B2DH);
			EventB2D = GetComponent<BoxCollider2D>();
			base.gameObject.layer = stageBlockWallData.nLayer;
			SemiBlockMode = stageBlockWallData.nSemiBlockMode;
		}

		public override void SyncNowStatus()
		{
		}

		public override void OnSyncStageObj(string sIDKey, int nKey1, string smsg)
		{
		}

		public bool isAllBlock()
		{
			return SemiBlockMode == 0;
		}

		public bool isBlockLeft()
		{
			return SemiBlockMode == -1;
		}

		public bool isBlockRight()
		{
			return SemiBlockMode == 1;
		}
	}
}
