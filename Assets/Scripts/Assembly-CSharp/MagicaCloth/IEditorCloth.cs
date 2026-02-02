using System.Collections.Generic;

namespace MagicaCloth
{
	public interface IEditorCloth
	{
		List<int> GetSelectionList();

		List<int> GetUseList();
	}
}
