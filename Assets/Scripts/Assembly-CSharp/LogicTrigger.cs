public class LogicTrigger
{
	public float m_fAccumilatedTime;

	public float m_fNextGameTime;

	public float m_fFrameLen;

	public float m_fInterpolation;

	public LogicTrigger()
	{
		m_fFrameLen = GameLogicUpdateManager.m_fFrameLen;
		m_fAccumilatedTime = 0f;
		m_fNextGameTime = 0f;
		m_fInterpolation = 0f;
	}

	public bool IsTrigger(float deltaTime, ref int rGameFrame)
	{
		m_fAccumilatedTime += deltaTime;
		m_fInterpolation = (m_fAccumilatedTime + m_fFrameLen - m_fNextGameTime) / m_fFrameLen;
		if (m_fAccumilatedTime > m_fNextGameTime)
		{
			rGameFrame++;
			m_fNextGameTime += m_fFrameLen;
			return true;
		}
		return false;
	}
}
