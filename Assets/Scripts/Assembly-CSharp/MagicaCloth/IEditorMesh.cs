using System.Collections.Generic;
using UnityEngine;

namespace MagicaCloth
{
	public interface IEditorMesh
	{
		int GetEditorPositionNormalTangent(out List<Vector3> wposList, out List<Vector3> wnorList, out List<Vector3> wtanList);

		List<int> GetEditorTriangleList();

		List<int> GetEditorLineList();
	}
}
