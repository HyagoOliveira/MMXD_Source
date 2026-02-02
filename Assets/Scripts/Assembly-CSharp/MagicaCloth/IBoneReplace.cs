using System.Collections.Generic;
using UnityEngine;

namespace MagicaCloth
{
	public interface IBoneReplace
	{
		void ReplaceBone(Dictionary<Transform, Transform> boneReplaceDict);
	}
}
