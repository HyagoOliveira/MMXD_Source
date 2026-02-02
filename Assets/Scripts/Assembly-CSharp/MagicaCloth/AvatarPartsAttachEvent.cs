using System;
using UnityEngine.Events;

namespace MagicaCloth
{
	[Serializable]
	public class AvatarPartsAttachEvent : UnityEvent<MagicaAvatar, MagicaAvatarParts>
	{
	}
}
