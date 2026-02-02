#define RELEASE
using System;
using System.Collections.Generic;
using UnityEngine;

namespace MagicaCloth
{
	[HelpURL("https://magicasoft.jp/avatar-parts/")]
	[AddComponentMenu("MagicaCloth/MagicaAvatarParts")]
	public class MagicaAvatarParts : MonoBehaviour, IDataVerify
	{
		private MagicaAvatar parentAvatar;

		private Dictionary<string, Transform> boneDict = new Dictionary<string, Transform>();

		private List<CoreComponent> magicaComponentList;

		public MagicaAvatar ParentAvatar
		{
			get
			{
				return parentAvatar;
			}
			set
			{
				parentAvatar = value;
			}
		}

		public bool HasParent
		{
			get
			{
				return parentAvatar != null;
			}
		}

		public int PartsId
		{
			get
			{
				return GetInstanceID();
			}
		}

		private void OnDestroy()
		{
			Dispose();
		}

		public void Dispose()
		{
			if (parentAvatar != null)
			{
				parentAvatar.DetachAvatarParts(base.gameObject);
				parentAvatar = null;
			}
		}

		public List<Transform> CheckOverlappingTransform()
		{
			HashSet<string> hashSet = new HashSet<string>();
			List<Transform> list = new List<Transform>();
			Transform[] componentsInChildren = GetComponentsInChildren<Transform>();
			Transform transform = base.transform;
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

		public Dictionary<string, Transform> GetBoneDict()
		{
			if (boneDict.Count > 0)
			{
				return boneDict;
			}
			boneDict.Clear();
			Transform[] componentsInChildren = GetComponentsInChildren<Transform>();
			foreach (Transform transform in componentsInChildren)
			{
				if (boneDict.ContainsKey(transform.name))
				{
					Debug.LogWarning(string.Format("{0} [{1}]", Define.GetErrorMessage(Define.Error.OverlappingTransform), transform.name));
				}
				else
				{
					boneDict.Add(transform.name, transform);
				}
			}
			return boneDict;
		}

		public List<CoreComponent> GetMagicaComponentList()
		{
			if (magicaComponentList != null)
			{
				return magicaComponentList;
			}
			magicaComponentList = new List<CoreComponent>(GetComponentsInChildren<CoreComponent>());
			return magicaComponentList;
		}

		public int GetVersion()
		{
			return 1;
		}

		public void CreateVerifyData()
		{
			throw new NotImplementedException();
		}

		public Define.Error VerifyData()
		{
			if (Application.isPlaying)
			{
				return Define.Error.None;
			}
			if (CheckOverlappingTransform().Count > 0)
			{
				return Define.Error.OverlappingTransform;
			}
			return Define.Error.None;
		}

		public string GetInformation()
		{
			StaticStringBuilder.Clear();
			if (Application.isPlaying)
			{
				if ((bool)ParentAvatar)
				{
					StaticStringBuilder.Append("Connection parent avatar:");
					StaticStringBuilder.AppendLine();
					StaticStringBuilder.Append("    [", ParentAvatar.name, "]");
				}
				else
				{
					StaticStringBuilder.Append("No connection.");
				}
			}
			else
			{
				List<Transform> list = CheckOverlappingTransform();
				if (list.Count > 0)
				{
					StaticStringBuilder.Append("There are duplicate game object names.");
					foreach (Transform item in list)
					{
						StaticStringBuilder.AppendLine();
						StaticStringBuilder.Append("* ", item.name);
					}
				}
				else
				{
					StaticStringBuilder.Append("No problem.");
				}
			}
			return StaticStringBuilder.ToString();
		}
	}
}
