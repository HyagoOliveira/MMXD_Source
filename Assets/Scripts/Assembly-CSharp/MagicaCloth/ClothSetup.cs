#define RELEASE
using System;
using System.Collections.Generic;
using UnityEngine;

namespace MagicaCloth
{
	public class ClothSetup
	{
		private int teamBoneIndex;

		private float distanceBlendRatio = 1f;

		public float DistanceBlendRatio
		{
			get
			{
				return distanceBlendRatio;
			}
			set
			{
				distanceBlendRatio = value;
			}
		}

		public void ClothInit(PhysicsTeam team, MeshData meshData, ClothData clothData, ClothParams param, Func<int, uint> funcUserFlag)
		{
			MagicaPhysicsManager instance = CreateSingleton<MagicaPhysicsManager>.Instance;
			PhysicsManagerCompute compute = instance.Compute;
			instance.Team.SetMass(team.TeamId, param.GetMass());
			instance.Team.SetGravity(team.TeamId, param.GetGravity());
			instance.Team.SetDrag(team.TeamId, param.GetDrag());
			instance.Team.SetMaxVelocity(team.TeamId, param.GetMaxVelocity());
			instance.Team.SetFriction(team.TeamId, param.Friction);
			instance.Team.SetExternalForce(team.TeamId, param.MassInfluence, param.WindInfluence, param.WindRandomScale);
			instance.Team.SetWorldInfluence(team.TeamId, param.MaxMoveSpeed, param.GetWorldMoveInfluence(), param.GetWorldRotationInfluence(), param.UseResetTeleport, param.TeleportDistance, param.TeleportRotation, param.ResetStabilizationTime);
			Debug.Assert(clothData.VertexUseCount > 0);
			Debug.Assert(clothData.useVertexList.Count > 0);
			ChunkData chunk = team.CreateParticle(team.TeamId, clothData.useVertexList.Count, delegate(int i)
			{
				bool num12 = clothData.IsFixedVertex(i) || clothData.IsExtendVertex(i);
				uint num13 = 0u;
				if (funcUserFlag != null)
				{
					num13 = funcUserFlag(i);
				}
				if (num12)
				{
					num13 |= 0x1000002u;
				}
				if (clothData.IsFlag(i, 131072u))
				{
					num13 |= 0x400u;
				}
				return num13 | 0x2000000u;
			}, null, null, (int i) => clothData.vertexDepthList[i], delegate(int i)
			{
				float depth = clothData.vertexDepthList[i];
				return param.GetRadius(depth);
			}, null);
			instance.Team.SetParticleChunk(team.TeamId, chunk);
			if (param.UseSpring)
			{
				int num = compute.Spring.AddGroup(team.TeamId, param.UseSpring, param.GetSpringPower());
				PhysicsManagerTeamData.TeamData value = instance.Team.teamDataList[team.TeamId];
				value.springGroupIndex = (short)num;
				instance.Team.teamDataList[team.TeamId] = value;
			}
			if (param.UseClampPositionLength)
			{
				int num2 = compute.ClampPosition.AddGroup(team.TeamId, param.UseClampPositionLength, param.GetClampPositionLength(), param.ClampPositionAxisRatio, param.ClampPositionVelocityInfluence);
				PhysicsManagerTeamData.TeamData value2 = instance.Team.teamDataList[team.TeamId];
				value2.clampPositionGroupIndex = (short)num2;
				instance.Team.teamDataList[team.TeamId] = value2;
			}
			if (param.UseClampDistanceRatio && clothData.ClampDistanceConstraintCount > 0)
			{
				int num3 = compute.ClampDistance.AddGroup(team.TeamId, param.UseClampDistanceRatio, param.ClampDistanceMinRatio, param.ClampDistanceMaxRatio, param.ClampDistanceVelocityInfluence, clothData.rootDistanceDataList, clothData.rootDistanceReferenceList);
				PhysicsManagerTeamData.TeamData value3 = instance.Team.teamDataList[team.TeamId];
				value3.clampDistanceGroupIndex = (short)num3;
				instance.Team.teamDataList[team.TeamId] = value3;
			}
			if (clothData.StructDistanceConstraintCount > 0 || clothData.BendDistanceConstraintCount > 0 || clothData.NearDistanceConstraintCount > 0)
			{
				int num4 = compute.RestoreDistance.AddGroup(team.TeamId, param.GetMass(), param.RestoreDistanceVelocityInfluence, param.GetStructDistanceStiffness(), clothData.structDistanceDataList, clothData.structDistanceReferenceList, param.UseBendDistance, param.GetBendDistanceStiffness(), clothData.bendDistanceDataList, clothData.bendDistanceReferenceList, param.UseNearDistance, param.GetNearDistanceStiffness(), clothData.nearDistanceDataList, clothData.nearDistanceReferenceList);
				PhysicsManagerTeamData.TeamData value4 = instance.Team.teamDataList[team.TeamId];
				value4.restoreDistanceGroupIndex = (short)num4;
				instance.Team.teamDataList[team.TeamId] = value4;
			}
			if (param.UseRestoreRotation && clothData.RestoreRotationConstraintCount > 0)
			{
				int num5 = compute.RestoreRotation.AddGroup(team.TeamId, param.UseRestoreRotation, param.GetRotationPower(), param.RestoreRotationVelocityInfluence, clothData.restoreRotationDataList, clothData.restoreRotationReferenceList);
				PhysicsManagerTeamData.TeamData value5 = instance.Team.teamDataList[team.TeamId];
				value5.restoreRotationGroupIndex = (short)num5;
				instance.Team.teamDataList[team.TeamId] = value5;
			}
			if (param.UseClampRotation && clothData.ClampRotationConstraintDataCount > 0)
			{
				int num6 = compute.ClampRotation.AddGroup(team.TeamId, param.UseClampRotation, param.GetClampRotationAngle(), param.ClampRotationVelocityInfluence, clothData.clampRotationDataList, clothData.clampRotationRootInfoList);
				PhysicsManagerTeamData.TeamData value6 = instance.Team.teamDataList[team.TeamId];
				value6.clampRotationGroupIndex = (short)num6;
				instance.Team.teamDataList[team.TeamId] = value6;
			}
			if (param.UseTriangleBend && clothData.TriangleBendConstraintCount > 0)
			{
				int num7 = compute.TriangleBend.AddGroup(team.TeamId, param.UseTriangleBend, param.GetTriangleBendStiffness(), clothData.triangleBendDataList, clothData.triangleBendReferenceList, clothData.triangleBendWriteBufferCount);
				PhysicsManagerTeamData.TeamData value7 = instance.Team.teamDataList[team.TeamId];
				value7.triangleBendGroupIndex = (short)num7;
				instance.Team.teamDataList[team.TeamId] = value7;
			}
			if (param.UseCollision)
			{
				PhysicsManagerTeamData.TeamData value8 = instance.Team.teamDataList[team.TeamId];
				value8.SetFlag(262144u, param.KeepInitialShape);
				value8.SetFlag(32u, param.UseCollision);
				instance.Team.teamDataList[team.TeamId] = value8;
			}
			if (param.UsePenetration && clothData.PenetrationCount > 0)
			{
				int num8 = compute.Penetration.AddGroup(team.TeamId, param.UsePenetration, clothData.penetrationMode, param.GetPenetrationDistance(), param.GetPenetrationRadius(), clothData.penetrationDataList, clothData.penetrationReferenceList);
				PhysicsManagerTeamData.TeamData value9 = instance.Team.teamDataList[team.TeamId];
				value9.penetrationGroupIndex = (short)num8;
				instance.Team.teamDataList[team.TeamId] = value9;
			}
			if (team is MagicaBoneSpring || team is MagicaMeshSpring)
			{
				int num9 = compute.AdjustRotationWorker.AddGroup(team.TeamId, true, (int)param.AdjustRotationMode, param.AdjustRotationVector, clothData.adjustRotationDataList);
				PhysicsManagerTeamData.TeamData value10 = instance.Team.teamDataList[team.TeamId];
				value10.adjustRotationGroupIndex = (short)num9;
				instance.Team.teamDataList[team.TeamId] = value10;
			}
			if (clothData.lineRotationDataList != null && clothData.lineRotationDataList.Length != 0)
			{
				int num10 = compute.LineWorker.AddGroup(team.TeamId, param.UseLineAvarageRotation, clothData.lineRotationDataList, clothData.lineRotationRootInfoList);
				PhysicsManagerTeamData.TeamData value11 = instance.Team.teamDataList[team.TeamId];
				value11.lineWorkerGroupIndex = (short)num10;
				instance.Team.teamDataList[team.TeamId] = value11;
			}
			if (clothData.triangleRotationDataList != null && clothData.triangleRotationDataList.Length != 0)
			{
				int num11 = compute.TriangleWorker.AddGroup(team.TeamId, clothData.triangleRotationDataList, clothData.triangleRotationIndexList);
				PhysicsManagerTeamData.TeamData value12 = instance.Team.teamDataList[team.TeamId];
				value12.triangleWorkerGroupIndex = (short)num11;
				instance.Team.teamDataList[team.TeamId] = value12;
			}
			instance.Team.SetFlag(team.TeamId, 4u, param.UseFixedNonRotation);
		}

		public void ClothDispose(PhysicsTeam team)
		{
			if (CreateSingleton<MagicaPhysicsManager>.IsInstance())
			{
				CreateSingleton<MagicaPhysicsManager>.Instance.Compute.RemoveTeam(team.TeamId);
				team.RemoveAllParticle();
			}
		}

		public void ClothActive(PhysicsTeam team, ClothParams param, ClothData clothData)
		{
			MagicaPhysicsManager instance = CreateSingleton<MagicaPhysicsManager>.Instance;
			Transform transform = (param.GetInfluenceTarget() ? param.GetInfluenceTarget() : team.transform);
			teamBoneIndex = instance.Bone.AddBone(transform);
			instance.Team.SetBoneIndex(team.TeamId, teamBoneIndex, clothData.initScale);
			team.InfluenceTarget = transform;
		}

		public void ClothInactive(PhysicsTeam team)
		{
			if (CreateSingleton<MagicaPhysicsManager>.IsInstance())
			{
				MagicaPhysicsManager instance = CreateSingleton<MagicaPhysicsManager>.Instance;
				instance.Bone.RemoveBone(teamBoneIndex);
				instance.Team.SetBoneIndex(team.TeamId, -1, Vector3.zero);
			}
		}

		public void ReplaceBone(PhysicsTeam team, ClothParams param, Dictionary<Transform, Transform> boneReplaceDict)
		{
			Transform influenceTarget = param.GetInfluenceTarget();
			if ((bool)influenceTarget && boneReplaceDict.ContainsKey(influenceTarget))
			{
				param.SetInfluenceTarget(boneReplaceDict[influenceTarget]);
			}
		}

		public void ChangeData(PhysicsTeam team, ClothParams param)
		{
			if (!Application.isPlaying || !CreateSingleton<MagicaPhysicsManager>.IsInstance() || team == null)
			{
				return;
			}
			MagicaPhysicsManager instance = CreateSingleton<MagicaPhysicsManager>.Instance;
			PhysicsManagerCompute compute = instance.Compute;
			bool flag = false;
			if (param.ChangedParam(ClothParams.ParamType.Radius))
			{
				for (int i = 0; i < team.ParticleChunk.dataLength; i++)
				{
					int index = team.ParticleChunk.startIndex + i;
					float depth = instance.Particle.depthList[index];
					float radius = param.GetRadius(depth);
					instance.Particle.SetRadius(index, radius);
				}
			}
			if (param.ChangedParam(ClothParams.ParamType.Mass))
			{
				instance.Team.SetMass(team.TeamId, param.GetMass());
				flag = true;
			}
			if (param.ChangedParam(ClothParams.ParamType.Gravity))
			{
				instance.Team.SetGravity(team.TeamId, param.GetGravity());
			}
			if (param.ChangedParam(ClothParams.ParamType.Drag))
			{
				instance.Team.SetDrag(team.TeamId, param.GetDrag());
			}
			if (param.ChangedParam(ClothParams.ParamType.MaxVelocity))
			{
				instance.Team.SetMaxVelocity(team.TeamId, param.GetMaxVelocity());
			}
			if (param.ChangedParam(ClothParams.ParamType.ExternalForce))
			{
				instance.Team.SetExternalForce(team.TeamId, param.MassInfluence, param.WindInfluence, param.WindRandomScale);
			}
			if (param.ChangedParam(ClothParams.ParamType.ColliderCollision))
			{
				instance.Team.SetFriction(team.TeamId, param.Friction);
			}
			if (param.ChangedParam(ClothParams.ParamType.WorldInfluence))
			{
				instance.Team.SetWorldInfluence(team.TeamId, param.MaxMoveSpeed, param.GetWorldMoveInfluence(), param.GetWorldRotationInfluence(), param.UseResetTeleport, param.TeleportDistance, param.TeleportRotation, param.ResetStabilizationTime);
			}
			if (param.ChangedParam(ClothParams.ParamType.RestoreDistance) || flag)
			{
				compute.RestoreDistance.ChangeParam(team.TeamId, param.GetMass(), param.RestoreDistanceVelocityInfluence, param.GetStructDistanceStiffness(), param.UseBendDistance, param.GetBendDistanceStiffness(), param.UseNearDistance, param.GetNearDistanceStiffness());
			}
			if (param.ChangedParam(ClothParams.ParamType.TriangleBend))
			{
				compute.TriangleBend.ChangeParam(team.TeamId, param.UseTriangleBend, param.GetTriangleBendStiffness());
			}
			if (param.ChangedParam(ClothParams.ParamType.ClampDistance))
			{
				compute.ClampDistance.ChangeParam(team.TeamId, param.UseClampDistanceRatio, param.ClampDistanceMinRatio, param.ClampDistanceMaxRatio, param.ClampDistanceVelocityInfluence);
			}
			if (param.ChangedParam(ClothParams.ParamType.ClampPosition))
			{
				compute.ClampPosition.ChangeParam(team.TeamId, param.UseClampPositionLength, param.GetClampPositionLength(), param.ClampPositionAxisRatio, param.ClampPositionVelocityInfluence);
			}
			if (param.ChangedParam(ClothParams.ParamType.RestoreRotation))
			{
				compute.RestoreRotation.ChangeParam(team.TeamId, param.UseRestoreRotation, param.GetRotationPower(), param.RestoreRotationVelocityInfluence);
			}
			if (param.ChangedParam(ClothParams.ParamType.ClampRotation))
			{
				compute.ClampRotation.ChangeParam(team.TeamId, param.UseClampRotation, param.GetClampRotationAngle(), param.ClampRotationVelocityInfluence);
			}
			if (param.ChangedParam(ClothParams.ParamType.AdjustRotation))
			{
				compute.AdjustRotationWorker.ChangeParam(team.TeamId, true, (int)param.AdjustRotationMode, param.AdjustRotationVector);
			}
			if (param.ChangedParam(ClothParams.ParamType.ColliderCollision))
			{
				instance.Team.SetFlag(team.TeamId, 262144u, param.KeepInitialShape);
				compute.Collision.ChangeParam(team.TeamId, param.UseCollision);
			}
			if (param.ChangedParam(ClothParams.ParamType.Spring))
			{
				compute.Spring.ChangeParam(team.TeamId, param.UseSpring, param.GetSpringPower());
			}
			if (param.ChangedParam(ClothParams.ParamType.RotationInterpolation))
			{
				compute.LineWorker.ChangeParam(team.TeamId, param.UseLineAvarageRotation);
				instance.Team.SetFlag(team.TeamId, 4u, param.UseFixedNonRotation);
			}
			if (param.ChangedParam(ClothParams.ParamType.Penetration))
			{
				compute.Penetration.ChangeParam(team.TeamId, param.UsePenetration, param.GetPenetrationDistance(), param.GetPenetrationRadius());
			}
			param.ClearChangeParam();
		}
	}
}
