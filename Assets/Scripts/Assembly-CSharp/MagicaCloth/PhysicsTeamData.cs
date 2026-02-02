using System;
using System.Collections.Generic;
using UnityEngine;

namespace MagicaCloth
{
	[Serializable]
	public class PhysicsTeamData : IDataHash
	{
		[SerializeField]
		private List<ColliderComponent> colliderList = new List<ColliderComponent>();

		[SerializeField]
		private List<ColliderComponent> penetrationIgnoreColliderList = new List<ColliderComponent>();

		[SerializeField]
		private bool mergeAvatarCollider = true;

		private List<ColliderComponent> addColliderList = new List<ColliderComponent>();

		public int ColliderCount
		{
			get
			{
				return colliderList.Count;
			}
		}

		public List<ColliderComponent> ColliderList
		{
			get
			{
				return colliderList;
			}
		}

		public List<ColliderComponent> PenetrationIgnoreColliderList
		{
			get
			{
				return penetrationIgnoreColliderList;
			}
		}

		public bool MergeAvatarCollider
		{
			get
			{
				return mergeAvatarCollider;
			}
		}

		public int GetDataHash()
		{
			return colliderList.GetDataHash();
		}

		public void Init(int teamId)
		{
			foreach (ColliderComponent collider in colliderList)
			{
				if ((bool)collider)
				{
					collider.CreateColliderParticle(teamId);
				}
			}
		}

		public void Dispose(int teamId)
		{
			if (!CreateSingleton<MagicaPhysicsManager>.IsInstance())
			{
				return;
			}
			foreach (ColliderComponent collider in colliderList)
			{
				if ((bool)collider)
				{
					collider.RemoveColliderParticle(teamId);
				}
			}
			foreach (ColliderComponent addCollider in addColliderList)
			{
				if ((bool)addCollider)
				{
					addCollider.RemoveColliderParticle(teamId);
				}
			}
			addColliderList.Clear();
		}

		public void AddCollider(ColliderComponent collider)
		{
			if ((bool)collider && !addColliderList.Contains(collider))
			{
				addColliderList.Add(collider);
			}
		}

		public void RemoveCollider(ColliderComponent collider)
		{
			if ((bool)collider && addColliderList.Contains(collider))
			{
				addColliderList.Remove(collider);
			}
		}

		public void ValidateColliderList()
		{
			ShareDataObject.RemoveNullAndDuplication(colliderList);
		}
	}
}
