﻿using UnityEngine;
using System.Collections;

public class SpeedUp : MonoBehaviour {

	float ratio = 1.5f;
	
	void OnTriggerEnter2D (Collider2D other) {
		if (other.gameObject.tag == "Pad"){
			Debug.Log("get speed up");
            GameUIHelper.Instance.DrawProperty(GetComponent<SpriteRenderer>().sprite);
			GameObject.Find("Ball").SendMessage("SetSpeedByRatio", ratio);
		}
	}
}