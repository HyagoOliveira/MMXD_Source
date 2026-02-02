using System;

namespace MagicaCloth
{
	public abstract class MagicaAvatarAccess : IDisposable
	{
		protected MagicaAvatar owner;

		protected MagicaAvatarRuntime Runtime
		{
			get
			{
				return owner.Runtime;
			}
		}

		public void SetParent(MagicaAvatar avatar)
		{
			owner = avatar;
		}

		public abstract void Create();

		public abstract void Dispose();

		public abstract void Active();

		public abstract void Inactive();
	}
}
