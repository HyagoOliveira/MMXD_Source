#define RELEASE
using System.Collections.Generic;
using UnityEngine;

namespace MagicaCloth
{
	public class MagicaAvatarRuntime : MagicaAvatarAccess
	{
		private Dictionary<string, Transform> boneDict = new Dictionary<string, Transform>();

		private Dictionary<Transform, int> boneReferenceDict = new Dictionary<Transform, int>();

		private List<MagicaAvatarParts> avatarPartsList = new List<MagicaAvatarParts>();

		private List<ColliderComponent> colliderList = new List<ColliderComponent>();

		public int AvatarPartsCount
		{
			get
			{
				return avatarPartsList.Count;
			}
		}

		public override void Create()
		{
			CreateBoneDict();
			CreateColliderList();
		}

		public override void Dispose()
		{
		}

		public override void Active()
		{
		}

		public override void Inactive()
		{
		}

		public MagicaAvatarParts GetAvatarParts(int index)
		{
			return avatarPartsList[index];
		}

		private void CreateBoneDict()
		{
			Transform[] componentsInChildren = owner.GetComponentsInChildren<Transform>();
			foreach (Transform transform in componentsInChildren)
			{
				if (boneDict.ContainsKey(transform.name))
				{
					Debug.LogWarning(string.Format("{0} [{1}]", Define.GetErrorMessage(Define.Error.OverlappingTransform), transform.name));
					continue;
				}
				boneDict.Add(transform.name, transform);
				boneReferenceDict.Add(transform, 1);
			}
		}

		private void CreateColliderList()
		{
			ColliderComponent[] componentsInChildren = owner.GetComponentsInChildren<ColliderComponent>();
			if (componentsInChildren != null && componentsInChildren.Length != 0)
			{
				colliderList.AddRange(componentsInChildren);
			}
		}

		public int GetColliderCount()
		{
			if (Application.isPlaying)
			{
				return colliderList.Count;
			}
			return owner.GetComponentsInChildren<ColliderComponent>().Length;
		}

		public List<Transform> CheckOverlappingTransform()
		{
			HashSet<string> hashSet = new HashSet<string>();
			List<Transform> list = new List<Transform>();
			Transform[] componentsInChildren = owner.GetComponentsInChildren<Transform>();
			Transform transform = owner.transform;
			Transform[] array = componentsInChildren;
			foreach (Transform transform2 in array)
			{
				if (!(transform2 == transform))
				{
					if (hashSet.Contains(transform2.name))
					{
						list.Add(transform2);
					}
					else
					{
						hashSet.Add(transform2.name);
					}
				}
			}
			return list;
		}

		public int AddAvatarParts(MagicaAvatarParts parts)
		{
			if (parts == null)
			{
				return 0;
			}
			if (parts.HasParent)
			{
				return parts.PartsId;
			}
			if (!parts.gameObject.activeSelf)
			{
				parts.gameObject.SetActive(true);
			}
			owner.Init();
			SkinnedMeshRenderer[] componentsInChildren = parts.GetComponentsInChildren<SkinnedMeshRenderer>();
			List<CoreComponent> magicaComponentList = parts.GetMagicaComponentList();
			Transform transform = owner.transform;
			Transform transform2 = parts.transform;
			parts.transform.SetParent(transform, false);
			parts.transform.localPosition = Vector3.zero;
			parts.transform.localRotation = Quaternion.identity;
			parts.ParentAvatar = owner;
			avatarPartsList.Add(parts);
			Dictionary<string, Transform> dictionary = parts.GetBoneDict();
			foreach (Transform value in dictionary.Values)
			{
				if (value != transform2)
				{
					AddBone(transform, transform2, value);
				}
			}
			foreach (Transform value2 in dictionary.Values)
			{
				if (value2 != transform2)
				{
					Transform key = boneDict[value2.name];
					boneReferenceDict[key]++;
				}
			}
			Dictionary<Transform, Transform> dictionary2 = new Dictionary<Transform, Transform>();
			foreach (Transform value3 in dictionary.Values)
			{
				if (value3 != transform2)
				{
					dictionary2.Add(value3, boneDict[value3.name]);
				}
				else
				{
					dictionary2.Add(value3, transform);
				}
			}
			SkinnedMeshRenderer[] array = componentsInChildren;
			foreach (SkinnedMeshRenderer skinRenderer in array)
			{
				ReplaceSkinMeshRenderer(skinRenderer, dictionary2);
			}
			foreach (CoreComponent item in magicaComponentList)
			{
				ReplaceMagicaComponent(item, dictionary2);
			}
			if (colliderList.Count > 0)
			{
				foreach (CoreComponent item2 in magicaComponentList)
				{
					BaseCloth baseCloth = item2 as BaseCloth;
					if (!baseCloth || !baseCloth.TeamData.MergeAvatarCollider)
					{
						continue;
					}
					baseCloth.Init();
					foreach (ColliderComponent collider in colliderList)
					{
						baseCloth.AddCollider(collider);
					}
				}
			}
			parts.gameObject.SetActive(false);
			owner.OnAttachParts.Invoke(owner, parts);
			return parts.PartsId;
		}

		private void AddBone(Transform root, Transform croot, Transform bone)
		{
			if (boneDict.ContainsKey(bone.name))
			{
				return;
			}
			Transform parent = root;
			Transform transform = bone;
			Transform parent2 = bone.parent;
			while ((bool)parent2 && parent2 != croot)
			{
				if (boneDict.ContainsKey(parent2.name))
				{
					parent = boneDict[parent2.name];
					break;
				}
				transform = parent2;
				parent2 = parent2.parent;
			}
			transform.SetParent(parent, false);
			Transform[] componentsInChildren = transform.GetComponentsInChildren<Transform>();
			foreach (Transform transform2 in componentsInChildren)
			{
				if (boneDict.ContainsKey(transform2.name))
				{
					Debug.LogWarning(string.Format("{0} [{1}]", Define.GetErrorMessage(Define.Error.AddOverlappingTransform), transform2.name));
					continue;
				}
				boneDict.Add(transform2.name, transform2);
				boneReferenceDict.Add(transform2, 0);
			}
		}

		private void ReplaceSkinMeshRenderer(SkinnedMeshRenderer skinRenderer, Dictionary<Transform, Transform> boneReplaceDict)
		{
			skinRenderer.rootBone = MeshUtility.GetReplaceBone(skinRenderer.rootBone, boneReplaceDict);
			Transform[] bones = skinRenderer.bones;
			for (int i = 0; i < bones.Length; i++)
			{
				bones[i] = MeshUtility.GetReplaceBone(bones[i], boneReplaceDict);
			}
			skinRenderer.bones = bones;
		}

		private void ReplaceMagicaComponent(CoreComponent comp, Dictionary<Transform, Transform> boneReplaceDict)
		{
			comp.ChangeAvatar(boneReplaceDict);
		}

		public void RemoveAvatarParts(MagicaAvatarParts parts)
		{
			if (parts == null || !avatarPartsList.Contains(parts))
			{
				return;
			}
			parts.ParentAvatar = null;
			avatarPartsList.Remove(parts);
			List<Transform> list = new List<Transform>();
			Transform transform = parts.transform;
			foreach (Transform value in parts.GetBoneDict().Values)
			{
				if (!(value == null) && value != transform)
				{
					Transform transform2 = boneDict[value.name];
					boneReferenceDict[transform2]--;
					if (boneReferenceDict[transform2] == 0)
					{
						boneReferenceDict.Remove(transform2);
						boneDict.Remove(transform2.name);
						list.Add(transform2);
					}
				}
			}
			foreach (Transform item in list)
			{
				if ((bool)item)
				{
					Object.Destroy(item.gameObject);
				}
			}
			if (colliderList.Count > 0)
			{
				foreach (CoreComponent magicaComponent in parts.GetMagicaComponentList())
				{
					BaseCloth baseCloth = magicaComponent as BaseCloth;
					if (!baseCloth)
					{
						continue;
					}
					foreach (ColliderComponent collider in colliderList)
					{
						baseCloth.RemoveCollider(collider);
					}
				}
			}
			Object.Destroy(parts.gameObject);
			owner.OnDetachParts.Invoke(owner);
		}

		public void RemoveAvatarParts(int partsId)
		{
			MagicaAvatarParts parts = avatarPartsList.Find((MagicaAvatarParts p) => p.PartsId == partsId);
			RemoveAvatarParts(parts);
		}
	}
}
