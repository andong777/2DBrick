﻿using UnityEngine;
using System.Collections;

public class DetectFall : MonoBehaviour {
	
	void OnTriggerEnter2D (Collider2D other) {
		
		// if it is the ball
		if (other.gameObject.tag == "Ball") {
			// if it is bottom wall, execute code, else let ConvertBall takes effect			
            audio.Play();
			Manager.LoseLife();	// subtract one life of the player
            SetGame.Instance.SetPadAndBall();   // reset position, zero speed
		}
		// remove other objects to save memory
		else {
			Debug.Log("catch something");
			if(other.gameObject.tag == "FallBrick"){
				Manager.LoseBrick();	// to help game manager count brick number
			}
			Destroy(other.gameObject, 0.5f);
		}
	}
}
