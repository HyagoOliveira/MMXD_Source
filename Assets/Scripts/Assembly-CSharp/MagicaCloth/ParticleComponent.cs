using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace MagicaCloth
{
	public abstract class ParticleComponent : MonoBehaviour, IDataHash
	{
		protected Dictionary<int, ChunkData> particleDict = new Dictionary<int, ChunkData>();

		protected RuntimeStatus status = new RuntimeStatus();

		public RuntimeStatus Status
		{
			get
			{
				return status;
			}
		}

		public int CenterParticleIndex
		{
			get
			{
				if (particleDict.ContainsKey(0))
				{
					return particleDict[0].startIndex;
				}
				return -1;
			}
		}

		public abstract int GetDataHash();

		protected virtual void Start()
		{
			Init();
		}

		public virtual void OnEnable()
		{
			status.SetEnable(true);
			status.UpdateStatus();
		}

		public virtual void OnDisable()
		{
			status.SetEnable(false);
			status.UpdateStatus();
		}

		protected virtual void OnDestroy()
		{
			OnDispose();
			status.SetDispose();
		}

		private void Init()
		{
			status.updateStatusAction = OnUpdateStatus;
			if (status.IsInitComplete || status.IsInitStart)
			{
				return;
			}
			status.SetInitStart();
			if (!VerifyData())
			{
				status.SetInitError();
				return;
			}
			OnInit();
			if (!status.IsInitError)
			{
				status.SetInitComplete();
				status.UpdateStatus();
			}
		}

		protected void OnUpdateStatus()
		{
			if (status.IsActive)
			{
				OnActive();
			}
			else
			{
				OnInactive();
			}
		}

		public virtual bool VerifyData()
		{
			return true;
		}

		protected virtual void OnInit()
		{
		}

		protected virtual void OnDispose()
		{
			if (CreateSingleton<MagicaPhysicsManager>.IsInstance())
			{
				RemoveParticle();
			}
		}

		protected virtual void OnUpdate()
		{
		}

		protected virtual void OnActive()
		{
			EnableParticle();
		}

		protected virtual void OnInactive()
		{
			DisableParticle();
		}

		protected void EnableParticle()
		{
			foreach (int key in particleDict.Keys)
			{
				EnableTeamParticle(key);
			}
		}

		protected void DisableParticle()
		{
			if (!CreateSingleton<MagicaPhysicsManager>.IsInstance())
			{
				return;
			}
			foreach (int key in particleDict.Keys)
			{
				DisableTeamParticle(key);
			}
		}

		protected void EnableTeamParticle(int teamId)
		{
			ChunkData c = particleDict[teamId];
			CreateSingleton<MagicaPhysicsManager>.Instance.Particle.SetEnable(c, true, UserTransform, UserTransformLocalPosition, UserTransformLocalRotation);
		}

		protected void DisableTeamParticle(int teamId)
		{
			ChunkData c = particleDict[teamId];
			CreateSingleton<MagicaPhysicsManager>.Instance.Particle.SetEnable(c, false, UserTransform, UserTransformLocalPosition, UserTransformLocalRotation);
		}

		protected ChunkData CreateParticle(uint flag, int teamId, float depth, float3 radius, float3 localPos)
		{
			Transform transform = base.transform;
			ChunkData chunkData = CreateSingleton<MagicaPhysicsManager>.Instance.Particle.CreateParticle(flag, teamId, transform.position, transform.rotation, depth, radius, localPos);
			particleDict.Add(teamId, chunkData);
			DisableTeamParticle(teamId);
			return chunkData;
		}

		protected void RemoveTeamParticle(int teamId)
		{
			if (CreateSingleton<MagicaPhysicsManager>.IsInstance())
			{
				DisableTeamParticle(teamId);
				ChunkData c = particleDict[teamId];
				CreateSingleton<MagicaPhysicsManager>.Instance.Particle.RemoveParticle(c);
				particleDict.Remove(teamId);
			}
		}

		protected void RemoveParticle()
		{
			if (CreateSingleton<MagicaPhysicsManager>.IsInstance())
			{
				foreach (int key in particleDict.Keys)
				{
					RemoveTeamParticle(key);
				}
			}
			particleDict.Clear();
		}

		protected Transform UserTransform(int vindex)
		{
			return base.transform;
		}

		protected float3 UserTransformLocalPosition(int vindex)
		{
			return base.transform.localPosition;
		}

		protected quaternion UserTransformLocalRotation(int vindex)
		{
			return base.transform.localRotation;
		}
	}
}
