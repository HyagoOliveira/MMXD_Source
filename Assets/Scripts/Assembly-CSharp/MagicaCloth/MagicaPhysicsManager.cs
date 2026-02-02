using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Entities;
using UnityEngine;


namespace MagicaCloth
{
	[HelpURL("https://magicasoft.jp/magica-cloth-physics-manager/")]
	public class MagicaPhysicsManager : CreateSingleton<MagicaPhysicsManager>
	{
		[SerializeField]
		private UpdateTimeManager updateTime = new UpdateTimeManager();

		private PhysicsManagerParticleData particle = new PhysicsManagerParticleData();

		private PhysicsManagerBoneData bone = new PhysicsManagerBoneData();

		private PhysicsManagerMeshData mesh = new PhysicsManagerMeshData();

		private PhysicsManagerTeamData team = new PhysicsManagerTeamData();

		private PhysicsManagerWindData wind = new PhysicsManagerWindData();

		private PhysicsManagerComponent component = new PhysicsManagerComponent();

		private PhysicsManagerCompute compute = new PhysicsManagerCompute();

		private bool useDelay;

		private bool isActive = true;

		public UpdateTimeManager.UpdateCount UpdatePerSeccond
		{
			get
			{
				return (UpdateTimeManager.UpdateCount)UpdateTime.UpdatePerSecond;
			}
			set
			{
				UpdateTime.SetUpdatePerSecond(value);
			}
		}

		public UpdateTimeManager.UpdateMode UpdateMode
		{
			get
			{
				return UpdateTime.GetUpdateMode();
			}
			set
			{
				UpdateTime.SetUpdateMode(value);
			}
		}

		public float FuturePredictionRate
		{
			get
			{
				return UpdateTime.FuturePredictionRate;
			}
			set
			{
				UpdateTime.FuturePredictionRate = value;
			}
		}

		public UpdateTimeManager UpdateTime
		{
			get
			{
				return updateTime;
			}
		}

		public PhysicsManagerParticleData Particle
		{
			get
			{
				particle.SetParent(this);
				return particle;
			}
		}

		public PhysicsManagerBoneData Bone
		{
			get
			{
				bone.SetParent(this);
				return bone;
			}
		}

		public PhysicsManagerMeshData Mesh
		{
			get
			{
				mesh.SetParent(this);
				return mesh;
			}
		}

		public PhysicsManagerTeamData Team
		{
			get
			{
				team.SetParent(this);
				return team;
			}
		}

		public PhysicsManagerWindData Wind
		{
			get
			{
				wind.SetParent(this);
				return wind;
			}
		}

		public PhysicsManagerComponent Component
		{
			get
			{
				component.SetParent(this);
				return component;
			}
		}

		public PhysicsManagerCompute Compute
		{
			get
			{
				compute.SetParent(this);
				return compute;
			}
		}

		public bool IsDelay
		{
			get
			{
				return useDelay;
			}
		}

		public bool IsActive
		{
			get
			{
				return isActive;
			}
			set
			{
				base.enabled = value;
			}
		}

		public void SetGlobalTimeScale(float timeScale)
		{
			UpdateTime.TimeScale = Mathf.Clamp01(timeScale);
		}

		public float GetGlobalTimeScale()
		{
			return UpdateTime.TimeScale;
		}

		protected override void Awake()
		{
			base.Awake();
		}

		protected override void InitSingleton()
		{
			Component.Create();
			Particle.Create();
			Bone.Create();
			Mesh.Create();
			Team.Create();
			Wind.Create();
			Compute.Create();
		}

		protected override void DuplicateDetection(MagicaPhysicsManager duplicate)
		{
			UpdateMode = duplicate.UpdateMode;
			UpdatePerSeccond = duplicate.UpdatePerSeccond;
			FuturePredictionRate = duplicate.FuturePredictionRate;
		}

		protected void OnEnable()
		{
			if (!isActive)
			{
				isActive = true;
				Component.UpdateComponentStatus();
			}
		}

		protected void OnDisable()
		{
			if (isActive)
			{
				isActive = false;
				Component.UpdateComponentStatus();
			}
		}

		protected override void OnDestroy()
		{
			Compute.Dispose();
			Wind.Dispose();
			Team.Dispose();
			Mesh.Dispose();
			Bone.Dispose();
			Particle.Dispose();
			Component.Dispose();
			base.OnDestroy();
		}

		private void AfterUpdate()
		{
			Compute.InitJob();
			Compute.UpdateRestoreBone();
			Compute.CompleteJob();
		}

		private void BeforeLateUpdate()
		{
		}

		private void AfterLateUpdate()
		{
			if (useDelay != UpdateTime.IsDelay)
			{
				if (!useDelay)
				{
					Compute.UpdateSwapBuffer();
					Compute.UpdateSyncBuffer();
				}
				useDelay = UpdateTime.IsDelay;
			}
			if (!useDelay)
			{
				Compute.UpdateTeamAlways();
				Compute.UpdateReadBoneScale();
				Compute.InitJob();
				Compute.UpdateReadBone();
				Compute.UpdateStartSimulation(updateTime);
				Compute.UpdateWriteBone();
				Compute.UpdateCompleteSimulation();
				Compute.UpdateWriteMesh();
			}
		}

		private void PostLateUpdate()
		{
			if (useDelay)
			{
				Compute.UpdateTeamAlways();
				Compute.UpdateReadBoneScale();
				Compute.InitJob();
				Compute.UpdateReadWriteBone();
				Compute.UpdateStartSimulation(updateTime);
				Compute.ScheduleJob();
				Compute.UpdateWriteMesh();
			}
		}

		private void AfterRendering()
		{
			if (useDelay)
			{
				Compute.UpdateCompleteSimulation();
				Compute.UpdateSwapBuffer();
				Compute.UpdateSyncBuffer();
			}
		}

		[RuntimeInitializeOnLoadMethod]
		public static void InitCustomGameLoop()
		{
			UnityEngine.LowLevel.PlayerLoopSystem playerLoop = UnityEngine.LowLevel.PlayerLoop.GetCurrentPlayerLoop();

            if (playerLoop.subSystemList == null || playerLoop.subSystemList.Length == 0)
			{
				playerLoop = UnityEngine.LowLevel.PlayerLoop.GetDefaultPlayerLoop();
			}
			//if (!CheckRegist(ref playerLoop))
			//{
			//	SetCustomGameLoop(ref playerLoop);
			//	UnityEngine.LowLevel.PlayerLoop.SetPlayerLoop(playerLoop);
			//}
            SetCustomGameLoop(ref playerLoop);
            UnityEngine.LowLevel.PlayerLoop.SetPlayerLoop(playerLoop);
        }

		public static void SetCustomGameLoop(ref UnityEngine.LowLevel.PlayerLoopSystem playerLoop)
		{
			UnityEngine.LowLevel.PlayerLoopSystem playerLoopSystem = default(UnityEngine.LowLevel.PlayerLoopSystem);
			playerLoopSystem.type = typeof(MagicaPhysicsManager);
			playerLoopSystem.updateDelegate = delegate
			{
				if (CreateSingleton<MagicaPhysicsManager>.IsInstance())
				{
					CreateSingleton<MagicaPhysicsManager>.Instance.AfterUpdate();
				}
			};
			UnityEngine.LowLevel.PlayerLoopSystem item = playerLoopSystem;
			playerLoopSystem = default(UnityEngine.LowLevel.PlayerLoopSystem);
			playerLoopSystem.type = typeof(MagicaPhysicsManager);
			playerLoopSystem.updateDelegate = delegate
			{
				if (CreateSingleton<MagicaPhysicsManager>.IsInstance())
				{
					CreateSingleton<MagicaPhysicsManager>.Instance.BeforeLateUpdate();
				}
			};
			UnityEngine.LowLevel.PlayerLoopSystem item2 = playerLoopSystem;
			playerLoopSystem = default(UnityEngine.LowLevel.PlayerLoopSystem);
			playerLoopSystem.type = typeof(MagicaPhysicsManager);
			playerLoopSystem.updateDelegate = delegate
			{
				if (CreateSingleton<MagicaPhysicsManager>.IsInstance())
				{
					CreateSingleton<MagicaPhysicsManager>.Instance.AfterLateUpdate();
				}
			};
			UnityEngine.LowLevel.PlayerLoopSystem item3 = playerLoopSystem;
			playerLoopSystem = default(UnityEngine.LowLevel.PlayerLoopSystem);
			playerLoopSystem.type = typeof(MagicaPhysicsManager);
			playerLoopSystem.updateDelegate = delegate
			{
				if (CreateSingleton<MagicaPhysicsManager>.IsInstance())
				{
					CreateSingleton<MagicaPhysicsManager>.Instance.PostLateUpdate();
				}
			};
			UnityEngine.LowLevel.PlayerLoopSystem item4 = playerLoopSystem;
			playerLoopSystem = default(UnityEngine.LowLevel.PlayerLoopSystem);
			playerLoopSystem.type = typeof(MagicaPhysicsManager);
			playerLoopSystem.updateDelegate = delegate
			{
				if (CreateSingleton<MagicaPhysicsManager>.IsInstance())
				{
					CreateSingleton<MagicaPhysicsManager>.Instance.AfterRendering();
				}
			};
			UnityEngine.LowLevel.PlayerLoopSystem item5 = playerLoopSystem;
			int num = 0;
			int num2 = 0;
			num = Array.FindIndex(playerLoop.subSystemList, (UnityEngine.LowLevel.PlayerLoopSystem s) => s.type.Name == "Update");
			UnityEngine.LowLevel.PlayerLoopSystem playerLoopSystem2 = playerLoop.subSystemList[num];
			List<UnityEngine.LowLevel.PlayerLoopSystem> list = new List<UnityEngine.LowLevel.PlayerLoopSystem>(playerLoopSystem2.subSystemList);
			num2 = list.FindIndex((UnityEngine.LowLevel.PlayerLoopSystem h) => h.type.Name.Contains("ScriptRunBehaviourUpdate"));
			list.Insert(num2 + 1, item);
			playerLoopSystem2.subSystemList = list.ToArray();
			playerLoop.subSystemList[num] = playerLoopSystem2;
			num = Array.FindIndex(playerLoop.subSystemList, (UnityEngine.LowLevel.PlayerLoopSystem s) => s.type.Name == "PreLateUpdate");
			UnityEngine.LowLevel.PlayerLoopSystem playerLoopSystem3 = playerLoop.subSystemList[num];
			List<UnityEngine.LowLevel.PlayerLoopSystem> list2 = new List<UnityEngine.LowLevel.PlayerLoopSystem>(playerLoopSystem3.subSystemList);
			num2 = list2.FindIndex((UnityEngine.LowLevel.PlayerLoopSystem h) => h.type.Name.Contains("ScriptRunBehaviourLateUpdate"));
			list2.Insert(num2, item2);
			list2.Insert(num2 + 2, item3);
			playerLoopSystem3.subSystemList = list2.ToArray();
			playerLoop.subSystemList[num] = playerLoopSystem3;
			num = Array.FindIndex(playerLoop.subSystemList, (UnityEngine.LowLevel.PlayerLoopSystem s) => s.type.Name == "PostLateUpdate");
			UnityEngine.LowLevel.PlayerLoopSystem playerLoopSystem4 = playerLoop.subSystemList[num];
			List<UnityEngine.LowLevel.PlayerLoopSystem> list3 = new List<UnityEngine.LowLevel.PlayerLoopSystem>(playerLoopSystem4.subSystemList);
			num2 = list3.FindIndex((UnityEngine.LowLevel.PlayerLoopSystem h) => h.type.Name.Contains("ScriptRunDelayedDynamicFrameRate"));
			list3.Insert(num2 + 1, item4);
			playerLoopSystem4.subSystemList = list3.ToArray();
			playerLoop.subSystemList[num] = playerLoopSystem4;
			num = Array.FindIndex(playerLoop.subSystemList, (UnityEngine.LowLevel.PlayerLoopSystem s) => s.type.Name == "PostLateUpdate");
			UnityEngine.LowLevel.PlayerLoopSystem playerLoopSystem5 = playerLoop.subSystemList[num];
			List<UnityEngine.LowLevel.PlayerLoopSystem> list4 = new List<UnityEngine.LowLevel.PlayerLoopSystem>(playerLoopSystem5.subSystemList);
			num2 = list4.FindIndex((UnityEngine.LowLevel.PlayerLoopSystem h) => h.type.Name.Contains("FinishFrameRendering"));
			list4.Insert(num2 + 1, item5);
			playerLoopSystem5.subSystemList = list4.ToArray();
			playerLoop.subSystemList[num] = playerLoopSystem5;
		}

		private static bool CheckRegist(ref UnityEngine.LowLevel.PlayerLoopSystem playerLoop)
		{
			Type t = typeof(MagicaPhysicsManager);
			UnityEngine.LowLevel.PlayerLoopSystem[] subSystemList = playerLoop.subSystemList;
			for (int i = 0; i < subSystemList.Length; i++)
			{
				if (subSystemList[i].subSystemList.Any((UnityEngine.LowLevel.PlayerLoopSystem x) => x.type == t))
				{
					return true;
				}
			}
			return false;
		}
	}
}
