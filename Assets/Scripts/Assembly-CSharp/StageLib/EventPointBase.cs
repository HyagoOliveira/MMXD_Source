using System;
using UnityEngine;

namespace StageLib
{
	[RequireComponent(typeof(BoxCollider2D))]
	public class EventPointBase : StageSLBase
	{
		public BoxCollider2D EventB2D;

		protected bool bCheck = true;

		public int nSetID;

		protected bool bAddEvented;

		protected Action UpdateCall;

		public bool bUseBoxCollider2D = true;

		public bool bDifficultDepend;

		public int nDifficultSet;

		public Action<EventPointBase> SiCoroutineUpdate;

		[SerializeField]
		private OrangeCriSource _SSource;

		public bool bCheckAble
		{
			get
			{
				return bCheck;
			}
			set
			{
				bCheck = value;
			}
		}

		[SerializeField]
		public OrangeCriSource SoundSource
		{
			get
			{
				if (_SSource == null)
				{
					_SSource = base.gameObject.GetComponent<OrangeCriSource>();
					if (_SSource == null)
					{
						_SSource = base.gameObject.AddComponent<OrangeCriSource>();
						_SSource.UseRenderObj = false;
						_SSource.Initial(OrangeSSType.MAPOBJS);
					}
				}
				return _SSource;
			}
			set
			{
				OrangeCriSource component = base.gameObject.GetComponent<OrangeCriSource>();
				if (component != null)
				{
					UnityEngine.Object.Destroy(component);
				}
				_SSource = value;
			}
		}

		private void Start()
		{
			base.gameObject.layer = 26;
			EventB2D = GetComponent<BoxCollider2D>();
			UpdateCall = UpdateEvent;
			Init();
			if (!bUseBoxCollider2D)
			{
				EventB2D.enabled = false;
			}
			SoundSource.Initial(OrangeSSType.MAPOBJS);
			SoundSource.StopWhenOutside = true;
			StageUpdate.RegisterEventUpdate(this);
			bAddEvented = true;
			if (bDifficultDepend && StageUpdate.gDifficulty != nDifficultSet)
			{
				bCheck = false;
				EventB2D.enabled = false;
				base.enabled = false;
			}
		}

		private void OnEnable()
		{
			if (bAddEvented)
			{
				StageUpdate.RegisterEventUpdate(this);
			}
		}

		private void OnDisable()
		{
			StageUpdate.RemoveEventUpdate(this);
		}

		public void UpdateEventBase()
		{
			if (UpdateCall != null)
			{
				UpdateCall();
			}
			if (SiCoroutineUpdate != null)
			{
				SiCoroutineUpdate(this);
			}
		}

		public virtual void StopEvent()
		{
		}

		public virtual void LockAnimator(bool bLock)
		{
		}

		protected virtual void UpdateEvent()
		{
			if (!StageUpdate.bIsHost)
			{
				return;
			}
			if (bCheck)
			{
				Vector3 max = EventB2D.bounds.max;
				Vector3 min = EventB2D.bounds.min;
				for (int num = StageUpdate.runPlayers.Count - 1; num >= 0; num--)
				{
					if (StageUpdate.runPlayers[num].UsingVehicle)
					{
						RideArmorController component = StageUpdate.runPlayers[num].transform.root.GetComponent<RideArmorController>();
						Vector3 position = component.transform.position;
						if (max.x > position.x && max.y > position.y && min.x < position.x && min.y < position.y)
						{
							OnEvent(component.transform);
							if (!bCheck)
							{
								return;
							}
						}
					}
					else
					{
						Vector3 position2 = StageUpdate.runPlayers[num].transform.position;
						if (max.x > position2.x && max.y > position2.y && min.x < position2.x && min.y < position2.y)
						{
							OnEvent(StageUpdate.runPlayers[num].transform);
							if (!bCheck)
							{
								return;
							}
						}
					}
				}
			}
			OnLateUpdate();
		}

		public virtual void Init()
		{
		}

		public virtual void OnEvent(Transform TriggerTransform)
		{
		}

		public virtual void OnLateUpdate()
		{
		}

		public override int GetTypeID()
		{
			return 0;
		}

		public override string GetTypeString()
		{
			return StageObjType.PREFAB_OBJ.ToString();
		}

		public override string GetSaveString()
		{
			return "";
		}

		public override void LoadByString(string sLoad)
		{
		}

		public override void SyncNowStatus()
		{
		}

		public override void OnSyncStageObj(string sIDKey, int nKey1, string smsg)
		{
		}
	}
}
