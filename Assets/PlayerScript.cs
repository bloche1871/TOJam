using UnityEngine;
using System.Collections;

public class PlayerScript : MonoBehaviour {

	public float noise = 0;

	void Start () 
	{
	}
	
	void OnJump()
	{
		noise = 4.0f;
	}
		
	void Update () 
	{
		noise = noise + 0.03f*(this.GetComponent<CharacterController>().velocity.magnitude/8.0f-noise);
	}
}
