using System;
using System.Collections.Generic;
using UnityEngine;

namespace MagicaCloth
{
	[HelpURL("https://magicasoft.jp/avatar/")]
	[AddComponentMenu("MagicaCloth/MagicaAvatar")]
	public class MagicaAvatar : CoreComponent
	{
		private const int DATA_VERSION = 1;

		[SerializeField]
		private bool dataReset;

		private MagicaAvatarRuntime runtime = new MagicaAvatarRuntime();

		public AvatarPartsAttachEvent OnAttachParts = new AvatarPartsAttachEvent();

		public AvatarPartsDetachEvent OnDetachParts = new AvatarPartsDetachEvent();

		public bool DataReset
		{
			get
			{
				return dataReset;
			}
			set
			{
				dataReset = value;
			}
		}

		public MagicaAvatarRuntime Runtime
		{
			get
			{
				runtime.SetParent(this);
				return runtime;
			}
		}

		public int AttachAvatarParts(GameObject avatarPartsPrefab, Action<GameObject> instanceAction = null)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(avatarPartsPrefab);
			if (instanceAction != null)
			{
				instanceAction(gameObject);
			}
			return Runtime.AddAvatarParts(gameObject.GetComponent<MagicaAvatarParts>());
		}

		public void DetachAvatarParts(int partsId)
		{
			Runtime.RemoveAvatarParts(partsId);
		}

		public void DetachAvatarParts(GameObject avatarPartsObject)
		{
			Runtime.RemoveAvatarParts(avatarPartsObject.GetComponent<MagicaAvatarParts>());
		}

		public void DetachAvatarParts(MagicaAvatarParts parts)
		{
			Runtime.RemoveAvatarParts(parts);
		}

		public override int GetDataHash()
		{
			return 0;
		}

		private void Reset()
		{
			DataReset = true;
		}

		private void OnValidate()
		{
		}

		protected override void OnInit()
		{
			Runtime.Create();
		}

		protected override void OnDispose()
		{
			Runtime.Dispose();
		}

		protected override void OnUpdate()
		{
		}

		protected override void OnActive()
		{
			Runtime.Active();
		}

		protected override void OnInactive()
		{
			Runtime.Inactive();
		}

		public override int GetVersion()
		{
			return 1;
		}

		public override int GetErrorVersion()
		{
			return 0;
		}

		public override void CreateVerifyData()
		{
			base.CreateVerifyData();
		}

		public override Define.Error VerifyData()
		{
			if (Application.isPlaying)
			{
				return Define.Error.None;
			}
			if (Runtime.CheckOverlappingTransform().Count > 0)
			{
				return Define.Error.OverlappingTransform;
			}
			return Define.Error.None;
		}

		public override string GetInformation()
		{
			StaticStringBuilder.Clear();
			if (Application.isPlaying)
			{
				if (Runtime.AvatarPartsCount > 0)
				{
					StaticStringBuilder.Append("Connection avatar parts:");
					int avatarPartsCount = Runtime.AvatarPartsCount;
					for (int i = 0; i < avatarPartsCount; i++)
					{
						StaticStringBuilder.AppendLine();
						StaticStringBuilder.Append("    [", Runtime.GetAvatarParts(i).name, "]");
					}
				}
				else
				{
					StaticStringBuilder.Append("No avatar parts connected.");
				}
			}
			else
			{
				List<Transform> list = Runtime.CheckOverlappingTransform();
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
				StaticStringBuilder.AppendLine();
				StaticStringBuilder.Append("Collider : ", Runtime.GetColliderCount());
			}
			return StaticStringBuilder.ToString();
		}

		public override List<ShareDataObject> GetAllShareDataObject()
		{
			return base.GetAllShareDataObject();
		}

		public override ShareDataObject DuplicateShareDataObject(ShareDataObject source)
		{
			return null;
		}
	}
}
