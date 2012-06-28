#pragma strict
//MovieTexture VideoTexture;
 var movieTexture = new MovieTexture();
 var goToScene : String;
 
function Start(){
	//MovieTexture.Play();
	movieTexture.Play();
 	while (!movieTexture.isReadyToPlay)
        yield;

    // Initialize gui texture to be 1:1 resolution centered on screen
    guiTexture.texture = movieTexture;

    transform.localScale = Vector3 (0,0,0);
    transform.position = Vector3 (0.5,0.5,0);
    print("H"+movieTexture.height);
    print("W"+movieTexture.width);
    print(1.0*movieTexture.height/movieTexture.width);
    
    //Keep Ratio (ex. 16:9)
	guiTexture.pixelInset.xMin = -Screen.width / 2;
    guiTexture.pixelInset.xMax = Screen.width / 2;
    guiTexture.pixelInset.yMin = -(1.0*movieTexture.height/movieTexture.width*Screen.width) / 2.0;
    guiTexture.pixelInset.yMax = (1.0*movieTexture.height/movieTexture.width*Screen.width) / 2.0;

    //Stretch video to full screen
    /*guiTexture.pixelInset.xMin = -movieTexture.width / 2;
    guiTexture.pixelInset.xMax = movieTexture.width / 2;
    guiTexture.pixelInset.yMin = -movieTexture.height / 2;
    guiTexture.pixelInset.yMax = movieTexture.height / 2;*/

    // Assign clip to audio source
    // Sync playback with audio
    audio.clip = movieTexture.audioClip;

    // Play both movie & sound
    movieTexture.Play();
    audio.Play();
}

function Update(){
	if(!movieTexture.isPlaying){
		Application.LoadLevel(goToScene);
	}
}

// Make sure we have gui texture and audio source
@script RequireComponent (GUITexture)
@script RequireComponent (AudioSource)


