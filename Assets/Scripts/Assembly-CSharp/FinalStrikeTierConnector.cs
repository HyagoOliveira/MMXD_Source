using UnityEngine;

public class FinalStrikeTierConnector : MonoBehaviour
{
	[SerializeField]
	private OrangeText m_levelNum;

	[SerializeField]
	private GameObject m_connectorEnabled;

	[SerializeField]
	private GameObject m_connectorDisabled;

	private void Start()
	{
	}

	public void SetLevel(int level)
	{
		m_levelNum.text = level.ToString();
	}

	public void SetEnable(bool bEnable)
	{
		m_connectorEnabled.SetActive(bEnable);
		m_connectorDisabled.SetActive(!bEnable);
	}
}
