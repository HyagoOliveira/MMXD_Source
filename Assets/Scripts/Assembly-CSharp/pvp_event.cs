using UnityEngine;

public class pvp_event : MonoBehaviour
{
	public ParticleSystem Hexagon;

	public ParticleSystem Pole;

	public ParticleSystem Stars;

	public void SetColor(Color col)
	{
		ParticleSystem.MainModule main = Hexagon.main;
		ParticleSystem.MainModule main2 = Pole.main;
		ParticleSystem.MainModule main3 = Stars.main;
		main.startColor = col;
		main2.startColor = col;
		main3.startColor = col;
	}

	private void Start()
	{
	}

	private void Update()
	{
	}
}
