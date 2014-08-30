﻿using UnityEngine;
using System.Collections;

public class BallFly : MonoBehaviour {
	
	const float normalSpeed = 5f;
	
	// if speed smaller or greater than speed threshold, set the speed to value
	float speedMinThreshold;
	float speedMinValue;
	float speedMaxThreshold;
	float speedMaxValue;
	// if velocity angle smaller than angle threshold, add speed at y axis by value
	float angleThreshold;
	float multifyValue;

    Quaternion shootDirection;
	
	// speed will return to normal after resetTime
	private const float resetTime = 3f;
	
	// the probability of generating property
	float propertyProbability = 0.3f;

    public GameObject star;
    public AudioClip winAudio;
	
	// properties
	public GameObject[] properties;

    // in case the ball is shot when user clicks reset
    float startTime;

    bool drunk;
	
	void Start () {
        Reset();
        Random.seed = (int)startTime;
	}
	
	void Update () {
		// Control the ball.
		if (Input.GetButtonUp ("Fire1") && !Manager.Released 
                && Time.timeScale > 0f && Time.time > startTime + 0.5) {
			Debug.Log("Fire");
			// choose a random direction		
			float dirX = Random.Range(-0.8f, 0.8f);
			float dirY = Mathf.Sqrt(1 - dirX * dirX);
			Vector2 direct = new Vector2(dirX, dirY);

			rigidbody2D.velocity = direct * normalSpeed;
			
			// mark as released
			Manager.Released = true;
        }
        // rotating the ball
        if(!Manager.Released)
        {
            transform.RotateAround(Vector3.zero, Vector3.forward, 30 * Time.deltaTime);
        }
	}
	
	/* 	This method is to avoid the situations below:
	 * 	1. where ball flies too slow or too fast after collision
	 * 	2. where ball flies horizontally
	 */
	void FixedUpdate () {
        
		if (Manager.Released) 
        {
			Vector3 velocity = rigidbody2D.velocity;
		
			// to solve problem 1
			float speed = velocity.sqrMagnitude;
			if (speed < speedMinThreshold) {
				velocity.Normalize();
				rigidbody2D.velocity = velocity * speedMinValue;
			}
			else if(speed > speedMaxThreshold) {
				velocity.Normalize();
				rigidbody2D.velocity = velocity * speedMaxValue;
			}
			
			// to solve problem 2
			float angle = Mathf.Atan(velocity.y / velocity.x) * Mathf.Rad2Deg;
			if (Mathf.Abs(angle) < angleThreshold) {
				// Debug.Log("Angle amendment takes effect: "+angle);
				float amend = Mathf.Sign(velocity.y) * multifyValue;
				rigidbody2D.velocity = velocity + new Vector3(0, amend, 0);
			}

            // drunk ball mode
            if (drunk)
            {
                float force = 10f;
                // choose a random direction	
                float dirX = Random.Range(-0.8f, 0.8f);
			    float dirY = Mathf.Sqrt(1 - dirX * dirX);
			    Vector2 direct = new Vector2(dirX, dirY);
                rigidbody2D.AddForce(direct * force);
            }

        }
        else
        {
            rigidbody2D.velocity = Vector2.zero;
            rigidbody2D.angularVelocity = 0f;
        }
	}
	
	// for normal ball
	void OnCollisionEnter2D(Collision2D other)
	{
		if (other.gameObject.tag == "Brick") {
            if(Manager.Released)
                audio.Play();
			// drop brick
			other.rigidbody.isKinematic = false;	// let it fall
			other.collider.isTrigger = true;	// let it be transparent
			other.gameObject.tag = "FallBrick";	// to avoid a second contact
			// drop property
			float probable = Random.Range(0f, 1f);
			if(probable < propertyProbability){
				Debug.Log("ball drops a property");
				int index = Random.Range(0, properties.Length - 1);	// choose a property
				Instantiate(properties[index], transform.position, Quaternion.identity);
			}
		}
	}
	
	// for fireball
	void OnTriggerEnter2D(Collider2D other){
		// Debug.Log("fireball working");
		if (other.gameObject.tag == "Brick") {
			// drop brick
			other.rigidbody2D.isKinematic = false;	// let it fall
			other.collider2D.isTrigger = true;	// let it be transparent
			other.gameObject.tag = "FallBrick";	// to avoid a second contact
			// drop property
			float probable = Random.Range(0f, 1f);
			if(probable < propertyProbability){
				Debug.Log("fireball drops a property");
				int index = Random.Range(0, properties.Length - 1);	// choose a property
				Instantiate(properties[index], transform.position, Quaternion.identity);
			}
		}
	}
	
	void SetVariables () {	

        if (Manager.Released)
        {
            // recover previous speed
            Debug.Log("return to normal");
            rigidbody2D.velocity = rigidbody2D.velocity.normalized * normalSpeed;
        }
        else
        {            
            Debug.Log("zero speed");
            rigidbody2D.velocity = Vector2.zero;    // zero ball speed
            rigidbody2D.angularVelocity = 0f;        // zero angular speed
        }
		// recover thresholds and values
		speedMinValue = normalSpeed - 1;
		speedMinThreshold = speedMinValue * speedMinValue;
		speedMaxValue = normalSpeed + 1;
		speedMaxThreshold = speedMaxValue * speedMaxValue;
		angleThreshold = 10f;
		multifyValue = 3f;
	}

	void SetSpeedByRatio (float ratio) {
		Debug.Log("set speed by "+ratio);

		// change the speed
		rigidbody2D.velocity = rigidbody2D.velocity * ratio;
		// change thresholds and values to avoid speed restriction
		speedMinThreshold *= ratio * ratio;
		speedMinValue *= ratio;
		speedMaxThreshold *= ratio * ratio;
		speedMaxValue *= ratio;
		
		// call reset after some delay
		Invoke("SetVariables", resetTime);
	}
	
	void MakeFireBall (float time){
		collider2D.isTrigger = true;
		ConvertBall.work = true;
        StartCoroutine("FireBallCoroutine");
		Invoke ("LoseFireBall", time);
	}

    IEnumerator FireBallCoroutine()
    {
        var sr = GetComponent<SpriteRenderer>();
        Color color1 = sr.color;
        Color color2 = new Color(color1.r, color1.g, color1.b, 0.6f);
        while (true)
        {
            sr.color = color1;
            yield return new WaitForSeconds(0.3f);
            sr.color = color2;
            yield return new WaitForSeconds(0.1f);
        }
    }
	
	void LoseFireBall (){
        GameUIHelper.Instance.DrawProperty(null);
		collider2D.isTrigger = false;
		ConvertBall.work = false;
        StopCoroutine("FireBallCoroutine");
        GetComponent<SpriteRenderer>().color = Color.white;
	}

    void MakeDrunkBall(float time)
    {
        drunk = true;
        Invoke("LoseDrunkBall", time);
    }

    void LoseDrunkBall()
    {
        GameUIHelper.Instance.DrawProperty(null);
        drunk = false;
    }

    void DropStar()
    {
        Vector2 position = new Vector3(0, 2, 0);
        AudioSource.PlayClipAtPoint(winAudio, position);
        Instantiate(star, position, Quaternion.identity);
    }

    void Reset()
    {
        // disable fireball
        LoseFireBall();
        // disable drunkball
        LoseDrunkBall();
        // reset speed
        SetVariables();        
    }

    void SetStartTime()
    {
        // set start time to now
        startTime = Time.time;
    }

}
