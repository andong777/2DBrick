﻿using UnityEngine;
using System.Collections;

public class SetGame: MonoBehaviour {

    private static SetGame _instance;   // singleton

	// Collider components
	public Transform leftWall;
	public Transform rightWall;
	public Transform topWall;
	public Transform bottomWall;

	// set pad and ball to original position
	private Transform pad = null;
	private Transform ball = null;
	
	// record some values
	private float leftPos;
	private float rightPos;
	private float topPos;
	private float bottomPos;
	
	// the bricks
	public GameObject brick;
	float percentage = 0.875f;	// use how many spaces to generate bricks	
    public Sprite[] brickSprites;

    private SetGame() {}

    public static SetGame Instance
    {
        get
        {
            return _instance;
        }
    }

	void Awake () {
        _instance = this;
		pad = GameObject.Find("Pad").transform;
		ball = GameObject.Find("Ball").transform;
		Random.seed = System.DateTime.Now.Millisecond;
	}

	// Use this for initialization
	void Start () {
		// get the border
        Camera mainCam = Camera.main;
		leftPos = mainCam.ScreenToWorldPoint(new Vector3(0, 0, 0)).x;
        rightPos = mainCam.ScreenToWorldPoint(new Vector3(Screen.width, 0, 0)).x;
        topPos = mainCam.ScreenToWorldPoint(new Vector3(0, Screen.height, 0)).y;
        bottomPos = mainCam.ScreenToWorldPoint(new Vector3(0, 0, 0)).y;
		
		// set walls
		leftWall.position = new Vector3(leftPos - leftWall.gameObject.collider2D.bounds.size.x, 0, 0);  // leave some space
		rightWall.position = new Vector3(rightPos + rightWall.gameObject.collider2D.bounds.size.x / 2, 0, 0);
		topWall.position = new Vector3(0, topPos + topWall.gameObject.collider2D.bounds.size.y / 2, 0);
		bottomWall.position = new Vector3(0, bottomPos - bottomWall.gameObject.collider2D.bounds.size.y / 2, 0);

        Reset();    // reset pad, ball and bricks
	}

    public void Reset()
    {
        // set the pad and the ball
        SetPadAndBall();
        // spawn bricks randomly.
        SpawnBricks();

    }

	public void SetPadAndBall() {
		// set pad position
		pad.position = new Vector3(0, bottomPos + pad.collider2D.bounds.size.y * 2 / 3, 0);
		// set ball position
		ball.position = pad.position + new Vector3(0, ball.gameObject.collider2D.bounds.size.y / 2, 0);
		
		// mark the ball as unreleased
		Manager.Released = false;

		// return pad to normal
		pad.SendMessage("Reset");
		
		// return ball to normal
        ball.SendMessage("Reset");

	}
	
	public void SpawnBricks () {
		// get brick info. add a little distance between them
        float brickWidth = brickSprites[0].bounds.size.x * 1.1f;
        float brickHeight = brickSprites[0].bounds.size.y * 1.1f;

		// set block to spawn bricks
		float distX = brickWidth;
		float distY = (topPos - bottomPos) / 3;
        float width = rightPos - leftPos - 2 * distX;
		float height = distY * 2;
	
		// array to mark if a place is occupied
		int rowNum = (int)(width / brickWidth);
		int colNum = (int)(height / brickHeight);
		bool[,] used = new bool[rowNum, colNum];
		
		int brickCount = 0;	// count how many bricks are generated
		for(int i = 0; i < rowNum * colNum * percentage; i++){
			int row = Random.Range(0, rowNum);
			int col = Random.Range(0, colNum);
			if(!used[row, col]){
				used[row, col] = true;
				brickCount ++;
				float x = leftPos + distX + brickWidth * (row + 0.5f);
				float y = bottomPos + distY + brickHeight * (col + 0.5f);
				var theBrick = Instantiate(brick, new Vector3(x, y, 0), Quaternion.identity) as GameObject;
                int index = Random.Range(0, brickSprites.Length);
                theBrick.GetComponent<SpriteRenderer>().sprite = brickSprites[index];
			}
		}
		
		Manager.SetTargetScoreByBrick(brickCount);	// set target score according to bricks
	}
}