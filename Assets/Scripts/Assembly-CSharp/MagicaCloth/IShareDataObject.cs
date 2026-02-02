using System.Collections.Generic;

namespace MagicaCloth
{
	public interface IShareDataObject
	{
		List<ShareDataObject> GetAllShareDataObject();

		ShareDataObject DuplicateShareDataObject(ShareDataObject source);
	}
}
