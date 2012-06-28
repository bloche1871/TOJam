using UnityEngine;
using System.Collections;

public class Monster : MonoBehaviour {
	
	public AudioSource eating;
	
	private Vector3 movement = Vector3.zero;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		
		if (Random.value < 0.06f) updateBehavior();
		
		this.transform.position += movement;
		this.transform.position = new Vector3
			(
				this.transform.position.x,
				Terrain.activeTerrain.SampleHeight(this.transform.position),
				this.transform.position.z
			);
		
		foreach (Animal animal in FindObjectsOfType(typeof(Animal)))
		{
			if (!animal.eaten)
			{
				Vector3 diff = animal.transform.position - this.transform.position;
				if (diff.sqrMagnitude < 60.0f)
				{
					animal.getEaten();
					eating.Play();
				}
			}
		}
	
	}
	
	void updateBehavior ()
	{	
		bool on_attack = (Time.timeSinceLevelLoad % 240 > 120);
		
		Vector3 center = Vector3.zero;
		Vector3 avoid = Vector3.zero;
		
		float n = 0;
		foreach (Animal animal in FindObjectsOfType(typeof(Animal)))
		{
			if (!animal.eaten)
			{
				Vector3 diff = animal.transform.position - this.transform.position;
				float sqdiff = diff.sqrMagnitude + 0.00001f;
				
				if (true)
				{
					float inverse_square = 1/sqdiff;
	
					center += diff*inverse_square;
					
					n += inverse_square;
				}
			}
		}

		foreach (GameObject player in GameObject.FindGameObjectsWithTag("Player"))
		{
			Vector3 diff = player.transform.position - this.transform.position;
			float sqdiff = diff.sqrMagnitude;
			
			if (true)
			{
				float inverse_square = 1/sqdiff;
				
				avoid -= diff*inverse_square;
			}
		}		
		
		if (n > 0)
		{
			center /= n;
		}
		
		if (on_attack)
		{
			center *= 0.0025f;
			avoid *= 2.25f;
		}
		else
		{
			center *= 0.0025f;
			avoid *= 500.0f;
		}
		
		movement = movement*0.5f+0.5f*(center+avoid).normalized*0.2f;
	}
}
