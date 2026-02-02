using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class ImageSpriteSwitcher : MonoBehaviour
{
	[SerializeField]
	private int _currentIndex;

	[SerializeField]
	private Sprite[] _sprites;

	private Image _image;

	public int CurrentIndex
	{
		get
		{
			return _currentIndex;
		}
		private set
		{
			_currentIndex = value;
		}
	}

	public void OnValidate()
	{
		ChangeImage(_currentIndex);
	}

	public void ChangeImage(int index)
	{
		_currentIndex = index;
		if (_image == null)
		{
			_image = GetComponent<Image>();
		}
		if (_sprites != null && 0 <= index && index < _sprites.Length)
		{
			_image.sprite = _sprites[index];
		}
	}
}
