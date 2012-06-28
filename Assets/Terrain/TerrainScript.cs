using UnityEngine;
using System.Collections;

public class TerrainScript : MonoBehaviour {
	
	public Light thelight;
	
	private Terrain terrain;
	static public float[,,] alphas;
	static private float[,,] original_alphas;
	
	
	void Start () {
		
		Screen.showCursor = false;
		
		terrain = this.GetComponent<Terrain>();
		
        foreach (AnimationState state in thelight.animation) {
			state.time = 1.5f;
        	state.speed *= 1/120.0f;			
    	}
		
		alphas = terrain.terrainData.GetAlphamaps(0,0,512,512);
		restoreTerrain();
		original_alphas = terrain.terrainData.GetAlphamaps(0,0,512,512);		
		
		doDetails();
		terrain.Flush();
	}

	void FixedUpdate () {
		
		
		if (Input.GetKeyDown (KeyCode.Return) || Input.GetKeyDown (KeyCode.R)) 
		{  
			Application.LoadLevel (0);  
		}
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			Application.Quit();
		}
		
		Camera.mainCamera.backgroundColor = new Color(0.25f*thelight.color.r,
			0.66f*Mathf.Max(0.01f,thelight.color.g+0.01f),
			Mathf.Max(thelight.color.b+0.15f, 0.15f));
		
		for (int i = 0; i <1; ++i)
		{
			int x = Random.Range(0, 512);
			int y = Random.Range(0, 512);
			
			if (original_alphas[x,y,1] > alphas[x,y,1] +0.001f)
			{
				//Debug.Log("regrow! "+x+","+y+" - "+alphas[x,y,1]+" "+original_alphas[x,y,1]);
				alphas[x,y,1] = original_alphas[x,y,1];
				
				int[,] temp_details = terrain.terrainData.GetDetailLayer(y,x,1,1,0);
				temp_details[0,0] = (int)(1+8*(alphas[x,y,1]-0.75f));
				terrain.terrainData.SetDetailLayer(y,x,0,temp_details);				
			}
		}
	}
	
	void restoreTerrain()
	{
		for (int x = 0; x < 512; ++x)
		for (int y = 0; y < 512; ++y)
		{
			alphas[x,y,1] = 1.0f-alphas[x,y,0];
		}

		terrain.terrainData.SetAlphamaps(0,0,alphas);
	}
	
	void doDetails()
	{
		int[,] details = terrain.terrainData.GetDetailLayer(0,0,512,512,1);
		
		for (int x = 0; x < 512; ++x)
		for (int y = 0; y < 512; ++y)
		{
			if (alphas[x,y,1] > 0.75f)
			{
				details[x,y] = (int)(1+8*(alphas[x,y,1]-0.75f));
			}
		}
		
		terrain.terrainData.SetDetailLayer(0,0,0,details);
	}
}
