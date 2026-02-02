using UnityEngine;
using UnityEngine.UI;

public class DebguDevMenu : MonoBehaviour
{
	public GameObject windows;

	public bool isOpen;

	public Text Svn_rev;

	public GameObject AccountButton;

	[SerializeField]
	private InputField inputFieldDomain;

	private void Awake()
	{
		Object.Destroy(base.gameObject);
	}
}
