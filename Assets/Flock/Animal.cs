using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Animal : MonoBehaviour {
	
	static private Terrain terrain;
	
	public AudioSource munching;

	public AudioSource goat1;
	public AudioSource goat2;
	public AudioSource goat3;
	
	public float speed;
	public float traction;
	public float rotation_speed;
			
	private Vector3 target_velocity = Vector3.zero;
	private Vector3 delta_v = Vector3.zero;
	
	private CharacterController controller;
	
	private Vector3 groundNormal = Vector3.zero;
	
	private float hunger;
	private bool dead = false;
	
	public bool eaten = false;
	
	public Object splotch;
	
	void Start () 
	{		
		this.transform.position = new Vector3
			(
				this.transform.position.x,
				Terrain.activeTerrain.SampleHeight(this.transform.position) +1.0f,
				this.transform.position.z
			);
		
		controller = this.GetComponent<CharacterController>();
		if (!terrain) terrain = Terrain.activeTerrain;
		
		hunger = 1.5f + Random.value;
	}
	
	void FixedUpdate () 
	{
		if (hunger <= 0 && !dead) die ();
		else hunger -= 0.00015f;
		
		if (!dead)
		{
			if (Random.value < 0.04f) updateBehavior();
			if (Random.value < 0.02f) doGrazing();
			
			if (Random.value < 0.0002f)
			{
				float test = Random.value;
				if (test < 1/3.0f)goat2.Play();
				else if (test < 2/3.0f)goat3.Play();
				else goat1.Play();
			}
			
			doMovement();
		}
		
		if (this.eaten)
		{
			this.transform.position += new Vector3(0,-0.025f,0);
		}
	}
	
	public void getEaten()
	{
		this.die();
		this.eaten = true;		
		GameObject daobject = GameObject.Instantiate(splotch, this.transform.position, Quaternion.Euler(new Vector3(270.0f,0,0))) as GameObject;
		Destroy(this.gameObject);
	}
	
	void die()
	{
		Debug.Log("death");
		dead = true;
		target_velocity = Vector3.zero;

		Destroy(controller);
		this.eaten = true;	
	}
		
	void doGrazing()
	{
		if (controller.velocity.magnitude < 2.0f && hunger < 3.0f)
		{
			int posx = (int)((512*this.transform.position.x)/500.0f);
			int posz = (int)((512*this.transform.position.z)/500.0f);
	
			bool graze = false;

			float[,,] temp_alphas = terrain.terrainData.GetAlphamaps(posx,posz,2,2);
			int[,] temp_details = terrain.terrainData.GetDetailLayer(posx,posz,2,2,0);
			
			for (int x = 0; x < 2; ++x)
			for (int z = 0; z < 2; ++z)
			{
				if (posx+x >= 0 && posz + z >= 0 && posx+x < 512 && posz+z <512)
				{
					if (TerrainScript.alphas[posz+z,posx+x,1] > 0.70f)
					{
						TerrainScript.alphas[posz+z,posx+x,1] -= 0.5f;
						hunger += (temp_alphas[z,x,1]-0.70f)*0.5f;
						temp_alphas[z,x,1] -= 0.5f;
						temp_details[z,x] = (int)(1+8*(temp_alphas[z,x,1]-0.75f));
						graze = true;						
					}
				}
			}
			if (graze)
			{
				munching.Play();
				terrain.terrainData.SetDetailLayer(posx,posz,0,temp_details);
				//terrain.terrainData.SetAlphamaps(posx,posz,temp_alphas);
			}
		}
	}
	
	void updateBehavior ()
	{
		Vector3 center = Vector3.zero;
		Vector3 avoid = Vector3.zero;
		Vector3 direction = Vector3.zero;
		Vector3 grass = Vector3.zero;
		Vector3 forward = controller.transform.forward;

		Vector3 monster = Vector3.zero;

		int posx = (int)((512*this.transform.position.x)/500.0f);
		int posz = (int)((512*this.transform.position.z)/500.0f);
		
		int startx = Mathf.Max(0,posx-80);
		int startz = Mathf.Max(0,posz-80);
		int stopx = Mathf.Min(512,posx+80);
		int stopz = Mathf.Min(512,posz+80);
				
		float t = 0;
		for (int x = startx; x < stopx; ++x)
		for (int z = startz; z < stopz; ++z)
		{
			//float inverse_square = 1/sqdiff;
			if (TerrainScript.alphas[z,x,1] > 0.75f)
			{
				float sqdiff = (x-posx)*(x-posx)+(z-posz)*(z-posz)+0.001f;
				float inverse_square = 1/sqdiff;
//				if (sqdiff < grass.sqrMagnitude || grass == Vector3.zero)
//				{
//					grass = new Vector3((500*(x-posx))/512.0f,0,(500*(z-posz))/512.0f);
//				}
				
				Vector3 delta = new Vector3((500*(x-posx))/512.0f,0,(500*(z-posz))/512.0f);
				delta *= (TerrainScript.alphas[z,x,1]-0.75f*4) * inverse_square;
				
				grass -= delta;
				t+=inverse_square;
			}
		}


		float n = 0;
		foreach (Animal animal in FindObjectsOfType(typeof(Animal)))
		{
			if (animal != this && !animal.dead)
			{
				Vector3 diff = animal.transform.position - this.transform.position;
				float sqdiff = diff.sqrMagnitude + 15.0f;
				
				if (sqdiff < 2000.0f)
				{
					float inverse_square = 1/sqdiff;
	
					center += diff*inverse_square;
					
					avoid -= diff.normalized*inverse_square;
					
					direction += animal.controller.velocity*inverse_square;
					
					n += inverse_square;
				}
			}
		}
		
		foreach (GameObject player in GameObject.FindGameObjectsWithTag("Player"))
		{
			Vector3 diff = player.transform.position - this.transform.position;
			float sqdiff = diff.sqrMagnitude;
			
			if (sqdiff < 2000.0f)
			{
				float inverse_square = 1/sqdiff;
				
				avoid -= diff.normalized*inverse_square*100.0f*player.GetComponent<PlayerScript>().noise;
				
				n += inverse_square;
			}
		}
		
		foreach (GameObject player in GameObject.FindGameObjectsWithTag("Monster"))
		{
			Vector3 diff = player.transform.position - this.transform.position;
			float sqdiff = diff.sqrMagnitude+0.1f;
			
			if (true)
			{
				float inverse_square = 1/sqdiff;
				
				monster -= diff*inverse_square;
			}
		}
		
		if (n > 0)
		{
			center = center/n;
			direction = direction/n;
		}
		if (t > 0)
		{
			grass = grass/t;
		}
		
		center *= 0.025f;
		avoid *= 1.0f;
		direction *= 0.2f;
		float grass_motivation = 2.0f - Mathf.Min (hunger,2.0f);
		grass = grass.normalized*grass_motivation;
		grass *= 0.4f;
		forward *= 0.01f;
		monster *= 10.0f;
		Debug.Log(monster);
				
		//Debug.DrawLine(this.transform.position, this.transform.position + center*10.0f);
		//Debug.DrawLine(this.transform.position, this.transform.position + avoid*10.0f);
		//Debug.DrawLine(this.transform.position, this.transform.position + direction*10.0f);
		
		target_velocity = center+avoid+direction+grass+forward+monster;
		target_velocity *= 0.66f;
		}
	
	void doMovement ()
	{
		Vector3 velocity = controller.velocity;
		if (controller.isGrounded)
		{
			Vector3 movement = Vector3.zero;
			
			float dot_product = Vector3.Dot(target_velocity, transform.forward);
			if (dot_product > 0)
			{
				dot_product = Mathf.Min(1.0f,dot_product);
				movement = transform.forward * dot_product;
			}
			
			float corrected_speed = speed * Mathf.Min(1.0f, 0.25f+hunger);

			movement *= corrected_speed;
			
	        foreach (AnimationState state in this.animation) {
            	state.speed = movement.magnitude*0.5f;
        	}
			
			Vector3 difference = movement - velocity;
			
			difference.y = 0;
						
			velocity += difference.normalized * (Mathf.Min(difference.magnitude, traction));

			velocity -= 0.5f*Physics.gravity.y * Time.deltaTime * new Vector3(groundNormal.x, 0, groundNormal.z);
		}
		else
		{
			velocity += Physics.gravity * Time.deltaTime;
		}
		velocity += delta_v;
		delta_v = Vector3.zero;
		controller.Move(velocity*Time.deltaTime);
		
		if (target_velocity != Vector3.zero)
		{
			this.transform.rotation = Quaternion.RotateTowards(this.transform.rotation, 
				Quaternion.LookRotation(new Vector3(target_velocity.x,0,target_velocity.z), new Vector3(0,1,0)), rotation_speed);
		}
	}
	
	void OnControllerColliderHit (ControllerColliderHit hit) 
	{
		if (hit.normal.y > 0 && hit.moveDirection.y < 0) {
			groundNormal = hit.normal;
			delta_v -= 0.25f*controller.velocity.y * new Vector3(groundNormal.x, 0, groundNormal.z);
		}
	}
}
