public class WantedAttrDataParam
{
	public int Condition { get; private set; }

	public int ParamX { get; private set; }

	public int ParamY { get; private set; }

	public WantedAttrDataParam(int condition, int paramX, int paramY)
	{
		Condition = condition;
		ParamX = paramX;
		ParamY = paramY;
	}
}
