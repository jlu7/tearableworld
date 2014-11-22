using UnityEngine;
//using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

public class TWCharacterController : MonoBehaviour{
	private GVariables globalVariables;

	private Vector3 playerRespawnPoint;
	/// <summary>
	/// The death effects particle emitter object.
	/// </summary>
	//public GameObject DeathEffects;
	
	/// <summary>
	/// The death effects sub emitter object.
	/// </summary>
//	public GameObject DeathEffectsSubEmitter;
	
	/// <summary>
	/// The player graphics.
	/// </summary>
	public GameObject PlayerGraphics;
	
	/// <summary>
	/// The death animation start time, representing the total
	/// amount of seconds the death particale animation is 
	/// vesible for
	/// </summary>
	private float deathAnimationStartTime = 1.25f;
	
	/// <summary>
	/// The total character death animation time is used to play the
	/// animation of the player dieing. For now, 0
	/// </summary>
	private float totalCharacterDeathAnimationTime = 0; 
	
	/// <summary>
	/// The death animation timer controls how long the death animation plays for
	/// before the new level is loaded
	/// </summary>
	private float deathAnimationTimer;
	
	/// <summary>
	/// This represents the speed of the player once they have died
	/// </summary>
	private float deathSpeed = 0.5f;
	
	/// <summary>
	/// The death audio clip played when player falls off paper
	/// </summary>
	//public AudioClip DeathAudio;
	private LevelGoal myGoal;
	private Quaternion z0Angle = Quaternion.Euler (0,0,0f);
	private Quaternion z90Angle = Quaternion.Euler(0,0,90f);
	private Quaternion z180Angle = Quaternion.Euler(0, 0, 180f);
	private Quaternion z270Angle = Quaternion.Euler(0,0,270f);
	DeviceOrientation currOrient;
	DeviceOrientation oldOrient;
	/// <summary>
	/// The have played death audio flag ensures the player only triggers their
	/// death 'scream' only once
	/// </summary>
	private bool havePlayedDeathAudio = false;
    /// <summary>
    /// Watch that allows for respawn buffer time
    /// to occur w/o anything interfering 
    /// </summary>
    public Stopwatch spawnWatch = new Stopwatch();
	public GameObject myCamera;
    /// <summary>
    /// Time in ms that the player has to respawn
    /// w/o anything death code effecting him
    /// </summary>
    private int SPAWN_LIMIT = 1000;
	public bool tearLevel = false;
	public bool foldLevel = false;

    // horizontal movement variables.
    private float baseSpeed = 10F, speed, baseMaxSpeed = 2, maxSpeed = 3F, hardCap = 8, airControl = 1.0F;
    //private Vector3		horizontalDirection = Vector3.zero;
	
	// CHECK VERTICAL SPEED VARIABLES
	private int uberVelTimeCurr = 0;
	private int yVelTimeMax = 5;
	private float uberPosOld = -1000f;
	private float uberVelOld = -1000f;
	public bool playerIsDead = false;
	// Velocity corrections
	float oldUpVelocity = 0;
	Vector3 oldUpDirection = Vector3.zero;
	bool killPlayer = false;
    // jumping variables. 
    private float jumpSpeed = 6f;
    private Vector3 verticalDirection = Vector3.zero;
    public bool collidingOnBottom = false, jumpReset = true,
						qDown = false, eDown = false;
	
	public enum playerStates {isGrounded, falling, rising, isSliding};
	public playerStates currentPlayerState
    {
        get { return currentState; }
        set { currentState = value; }
    }
	
	public playerStates currentState = playerStates.falling;
	public bool getGrounded()
    {
		if (currentState == playerStates.isGrounded)
        {
			return true;
		}
		else return false;
	}
	public bool getRising()
    {
		if (currentState == playerStates.rising)
        {
			return true;
		}
		else return false;
	}
	public bool getFalling()
    {
		if (currentState == playerStates.falling)
        {
			return true;
		}
		else return false;
	}
	public bool getSliding()
    {
		if (currentState == playerStates.isSliding)
        {
			return true;
		}
		else return false;
	}
    /// <summary>
    /// Made this one public so we could reference
    /// it in AnimationManager
    /// </summary>
    public bool platformCollision = false;
	/// <summary>
	/// The amount seconds player has been in death zone for
	/// </summary>
	private float stillDying;

	/// <summary>
	/// Amount of time player needs to be in place that will kill him til he dies.
	/// </summary>
	private const float SECONDS_TIL_DEATH = 0.5f;

    // gravity variables
    private float forceOfGravity = 750.0F, fallSpeed = 200.0F;
    private Vector3 gravity = new Vector3(0, 750.0F, 0);
    Vector3 horizontalDirection = new Vector3(1, 1, 1);
	/// <summary>
	/// This variables checks how long character has been rising.
	/// </summary>
	private int beenRising = 0;
    private float debugTest = 0f;
    // storing the script that is attached to the collider on the bottom of the player. 
    triggerHitting playerhitbottom;
	triggerHitting playerhitleft;
	triggerHitting playerhitright;
    /// <summary>
    /// Input manager reference
    /// </summary>
    InputManager inputManagerRef;

    /// <summary>
    /// Gamestate Manager reference
    /// </summary>
    GameStateManager gameStateManagerRef;
	
	// references to TearManager and Fold
	TearManager tearManager;
	Fold fold;
    /// <summary>
    /// AnimationManager reference
    /// </summary>
    AnimationManager animationManagerRef;
	
	// used in determining if a tear has occurred
	private bool tearOccurred = false;
	private bool foldOccurred= false;
	
	//using these for debugging
	public float yvel = 0;
	public float xvel = 0;
	
	UnfoldCollision unfoldCollision;
	GameObject background;
    // function to move the character horizontally. 
    void Movement()
    {
		checkUp();
		
		//checks if rising is true, if it is adds to beenRising
        if (getRising())
        {
            beenRising++;
        }
        //if beenrising is equal to 15 then it sets rising to false, and resets beenRising.
		if (beenRising == 15)
		{
			currentState = playerStates.falling;
			beenRising = 0;
		}

		// used for debugging
		yvel = this.rigidbody.velocity.y;
		xvel = this.rigidbody.velocity.x;
				
		float rotZ = Mathf.Abs(this.transform.rotation.z);

        if (!getGrounded())
        {
            speed = baseSpeed * airControl;
        }
        else
        {
            speed = baseSpeed;
        }
        // if the platform we are on is not too steep
		// check for horizontal movement.
		if (playerhitleft.getHitting() && playerhitright.getHitting() 
			&& Mathf.Abs(this.rigidbody.velocity.x) < .4f && Mathf.Abs(this.rigidbody.velocity.y) < .4f && getSliding())
        {
			currentState = playerStates.isGrounded;
		}
        
        if (!fold.currentlyFolding && !tearManager.PlayerMovingPlatformState)
        {
            playerMovements();
        }
        
        if (!getGrounded())
        {
            applyGravity();
        }
	    
        recordUp();
		
		if (((rotZ == z0Angle.z)&&(this.rigidbody.velocity.y <= 0 && !collidingOnBottom && !bottomHitting))
			|| ((rotZ == z90Angle.z) && (this.rigidbody.velocity.y >= 0 && !collidingOnBottom && !bottomHitting))
			|| ((rotZ == z180Angle.z)&&(this.rigidbody.velocity.x >= 0 && !collidingOnBottom&& !bottomHitting))
			|| ((rotZ == z270Angle.z && !collidingOnBottom && !bottomHitting) && (this.rigidbody.velocity.x <= 0)))
        {
				//Debug.Log("from max height check");
				currentState = playerStates.falling;
				//jumpReset = true;
				//UnityEngine.Debug.Log("just got set to falling");
				beenRising = 0;
		}
    }
	
	/// <summary>
	/// Checks if the player is moving to the right, across screen.
	/// </summary>
	/// <returns>
	/// The right.
	/// </returns>
	public bool MovingRight()
	{
		//this checks moving in a positive x with the basic orientation
		if (desiredAngle == 0f || desiredAngle == 360f)
		{
            if (rigidbody.velocity.x >= 0)
            {
                return true;
            }
            else
            {
                return false;
            }
		}//checks if moving up y, for the sideways orientation
		else if (desiredAngle == 90f)
		{
            if (rigidbody.velocity.y >= 0)
            {
                return true;
            }
            else
            {
                return false;
            }
		}//checks if moving down x in the flipped orientation
		else if (desiredAngle == 180f)
		{
            if (rigidbody.velocity.x <= 0)
            {
                return true;
            }
            else
            {
                return false;
            }
		}//checks if moving down y in the other sideways orientation
		else if (desiredAngle == 270f)
		{
            if (rigidbody.velocity.y <= 0)
            {
                return true;
            }
            else
            {
                return false;
            }
		}
		return true;
	}
	
	void playerMovements()
    {
        if (!getSliding())
        {
            horizontalMoveKeyboard();
        }
        // check for jump
        verticalMove();
	}
	
	void recordUp()
	{
		Vector3 upVector = rigidbody.transform.up;
		oldUpDirection = upVector;
		if (upVector.x != 0)
		{
			// "Up" is along X AXIS
			oldUpVelocity = this.rigidbody.velocity.x;
		}
		else
		{
			// "Up" is along Y AXIS
			oldUpVelocity = this.rigidbody.velocity.y;
		}
	}
	
	void checkUp()
	{
		if (oldUpVelocity != 0 && getRising())
		{
			Vector3 upVector = rigidbody.transform.up;
			if (oldUpDirection == upVector)
			{
				if (upVector.x != 0)
				{
					if (Mathf.Abs (this.rigidbody.velocity.x) > Mathf.Abs (oldUpVelocity))
					{
						this.rigidbody.velocity = new Vector3(oldUpVelocity, this.rigidbody.velocity.y, 0);
					}
				}
				else
				{
					if (Mathf.Abs (this.rigidbody.velocity.y) > Mathf.Abs (oldUpVelocity))
					{
						this.rigidbody.velocity = new Vector3(this.rigidbody.velocity.x, oldUpVelocity, 0);
					}
				}
			}
		}
	}

    // handle the vertical jump
    void verticalMove()
    {
        //checks to see if the player pressed the jump button and the player is on a platform, ie touching the ground.
        if ((Input.GetButton("Jump") || inputManagerRef.PlayerJumping()) &&  getGrounded())
        {
            Jump();
        }
    }
	
	int count = 0;
    public void Jump()
    {
		if (getGrounded() && jumpReset)
		{
			//Vector3 force = rigidbody.transform.up * jumpSpeed * this.transform.localScale.y;
	        // add the jump force that propells the player upward. 
	        //rigidbody.AddForce(force);
			Vector3 upVector = rigidbody.transform.up * jumpSpeed;
			if ((int)upVector.x != 0) 
            {
				this.rigidbody.velocity = new Vector3(upVector.x, this.rigidbody.velocity.y, 0);
			}
			else 
            {
				this.rigidbody.velocity = new Vector3(this.rigidbody.velocity.x, upVector.y, 0);
			}
			currentState = playerStates.rising;
			jumpReset = false;
			//UnityEngine.Debug.Log("now rising");
		}
    }

    /// <summary>
    /// Limit beyond 90 degrees (straight up swipe)
    /// that the player will jump at angle instead
    /// of straight vertical.
    /// </summary>
    int VERTICAL_SWIPE_DEGREE_LIMIT = 10;


    /// <summary>
    /// Degree angle on a standard unit circle
    /// A 90 degree (before device re-orientation) swipe is a pure vertical (no horizontal)
    /// jump.
    /// </summary>
    public int verticalSwipeDegreeAngle = 90;

    void horizontalMoveKeyboard()
    {
        float horizontal = 0;
        if (gameStateManagerRef.OnMobileDevice())
        {
			float fingSwipeAngle = inputManagerRef.fingerSwipeAngle;
			if (!getGrounded() && !playerIsDead && spawnWatch.ElapsedMilliseconds > SPAWN_LIMIT)
            {
                if (verticalSwipeDegreeAngle != 0)
                {
                    if (fingSwipeAngle >= verticalSwipeDegreeAngle + VERTICAL_SWIPE_DEGREE_LIMIT)
                    {
                       // if (Camera.mainCamera.WorldToScreenPoint(gameObject.transform.position).x > gameStateManagerRef.GetInputManager().LastPressPos().x)
                        {
                            horizontal = -1.0f;
                        }
                    }

                    else if (fingSwipeAngle < verticalSwipeDegreeAngle - VERTICAL_SWIPE_DEGREE_LIMIT)
                    {
                       // if (Camera.mainCamera.WorldToScreenPoint(gameObject.transform.position).x < gameStateManagerRef.GetInputManager().LastPressPos().x)
                        {
                            horizontal = 1.0f;
                        }
                    }
                }

                // if the vertical swipe angle is current at zero, then we have to check
                // for the special case on the unit circle where the degrees loop back to zero
                else
                {
                    if (fingSwipeAngle >= 0 && 
                        fingSwipeAngle < 90 - VERTICAL_SWIPE_DEGREE_LIMIT)
                    {
                        horizontal = -1.0f;
                        //UnityEngine.Debug.Log("ANGLE " + inputManagerRef.fingerSwipeAngle);
                    }

                    else if (fingSwipeAngle <= 360 - VERTICAL_SWIPE_DEGREE_LIMIT 
					         && fingSwipeAngle >= 270)
                    {
                        horizontal = 1.0f;
                        //UnityEngine.Debug.Log("ANGLE " + inputManagerRef.fingerSwipeAngle);
                    }
                }
            }
            else
            {
                horizontal = inputManagerRef.GetHorizontalTouchMovement();  
            }
        }
        else
        {
            horizontal = Input.GetAxisRaw("Horizontal");
        }

//        UnityEngine.Debug.Log("HORIZ " + horizontal);
        horizontalMove(horizontal);
    }

    // Moves the player horizontally
    public void horizontalMove(float horizontal)
    {
		if (((playerhitright.getHitting() && horizontal == 1) || (playerhitleft.getHitting() && horizontal == -1)) && (getRising() || getFalling()))
        {
			//UnityEngine.Debug.Log("Horizontal: " + horizontal + " not supposed to be moving: " + currentState);
		}
		else if (this.rigidbody.isKinematic == false)
        {
			//UnityEngine.Debug.Log("Horizontal: " + "moving in a direction: " + currentState);
		    //Debug.Log(this.transform.rotation.z);
			bool positive;
			float rotZ = Mathf.Abs(this.transform.rotation.z);
			// this is determining if we are pressing the horizontal in the opposite direction of what we are moving.
			if(rotZ == z0Angle.z)
            {
				positive =  this.rigidbody.velocity.x*horizontal >= 0;
			}
			else if (rotZ == z180Angle.z)
            {
				positive = this.rigidbody.velocity.x*horizontal*(-1) >= 0;
			}
			// this is determining if we are pressing the horizontal in the opposite direction of what we are moving.
			else 
            {
				//Debug.Log( transform.rotation.z + " i am on the right wall");
				positive = this.rigidbody.velocity.y*horizontal*this.transform.right.y >=0;
			}
			
		    if (rotZ == z0Angle.z || rotZ == z180Angle.z)
		    {
				// if we are trying to go in the opposite direction that that which we are moving set the velocity to zero and start moving in the direction we want to go.
				if (!positive)
                {
					this.rigidbody.velocity = new Vector3(0, this.rigidbody.velocity.y, 0);
				}
				//Debug.Log("Horizontal: " + horizontal);
                if (horizontal == 0)
                {
                    Vector3 newVec = new Vector3(rigidbody.velocity.x * slowDownRate * transform.localScale.x, rigidbody.velocity.y, 0);
                    rigidbody.velocity = newVec;
                }

                else
                {
                    // calculate direction of force based on the normal force of the platform you are currently standing on.
                    Vector3 force = new Vector3(this.transform.right.x * horizontal * speed * transform.localScale.x, 0, 0);
                    Vector3 degreeRotationNormal = z90Angle * groundNormal;
                    if (groundNormal != Vector3.zero && !getFalling() && !getRising())
                    {
                        force = Vector3.Project(force, degreeRotationNormal);
                    }
                    //Debug.Log("Force: " + force);
                    // add that force to the rigidbody
                    rigidbody.AddForce(force);
                    if (rigidbody.velocity.x > maxSpeed)
                    {
                        rigidbody.velocity = new Vector3(maxSpeed, rigidbody.velocity.y, 0);
                    }
                    else if (rigidbody.velocity.x < -maxSpeed)
                    {
                        rigidbody.velocity = new Vector3(-maxSpeed, rigidbody.velocity.y, 0);
                    }
                }
		    }
		    else
		    {	
				// if we are trying to go in the opposite direction than that which we are moving set the velocity to zero and start moving in the direction we want to go.
				if (!positive)
                {
					this.rigidbody.velocity = new Vector3(this.rigidbody.velocity.x, 0, 0);
				}
				//Debug.Log("Horizontal: " + horizontal);
		        if (horizontal == 0)
		        {
		            Vector3 newVec = new Vector3(rigidbody.velocity.x, rigidbody.velocity.y * slowDownRate * transform.localScale.x, 0);
		            rigidbody.velocity = newVec;
		        }
		
		        else
		        {
					// calculate direction of force based on the normal force of the platform you are currently standing on
					Vector3 force = new Vector3(0,this.transform.right.y * horizontal * speed * transform.localScale.x, 0);
					Vector3 degreeRotationNormal = z90Angle * groundNormal;
					if(groundNormal != Vector3.zero && !getFalling() && !getRising()) force = Vector3.Project(force, degreeRotationNormal);
					// add that force to the rigidbody
		            rigidbody.AddForce(force);
					// if we are travelling too fast limit that velocity to acceptable speeds.
					if(rigidbody.velocity.y > maxSpeed) rigidbody.velocity = new Vector3(rigidbody.velocity.x, maxSpeed, 0);
					else if(rigidbody.velocity.y < -maxSpeed) rigidbody.velocity = new Vector3(rigidbody.velocity.x, -maxSpeed, 0);
		        }
		    }
		}
    }

    float slowDownRate = 0.0f;

    // Apply Gravity 
    void applyGravity()
    {
        // Apply gravity in the down direction relative to the player.
        gravity = rigidbody.transform.up * forceOfGravity;
        // if we are jumping and begin falling change the force of gravity to be stronger so that we fall faster. 
        if (getFalling())
        {
            gravity += (rigidbody.transform.up * fallSpeed);
        }
        rigidbody.AddForce((-1)*gravity * Time.deltaTime * this.transform.localScale.y);
		//UnityEngine.Debug.Log("Gravity is getting applied");
    }

	// Check the screen orientation and rotate the character accordingly
	public float desiredAngle = 0f;
	float startingAngle;
	DeviceOrientation currentDeviceOrientation;
	
	void checkOrientation()
	{
#if UNITY_IPHONE
			currentDeviceOrientation = Input.deviceOrientation;
#endif
#if UNITY_ANDROID
			currentDeviceOrientation = Input.deviceOrientation;
#endif
		//UnityEngine.Debug.Log("eulerangle: "+this.rigidbody.rotation.eulerAngles.z);
		//!important! this is making us rotate in smaller increments so that we don't get stuck in colliders(hopefully)
		if (this.rigidbody.rotation.eulerAngles.z != desiredAngle)
        {
			//UnityEngine.Debug.Log("Desired Angle in CheckOrientation(): " + desiredAngle);
			// if our current rotation is less than the desiredAngle -1 then rotate in positive direction

			if (this.rigidbody.rotation.eulerAngles.z < desiredAngle-1.0) 
            {
				rigidbody.MoveRotation(Quaternion.Euler(0.0f, 0.0f, this.rigidbody.rotation.eulerAngles.z + 5.0f));
#if UNITY_STANDALONE
				myCamera.transform.rotation =Quaternion.Euler(0.0f, 0.0f, myCamera.transform.rotation.eulerAngles.z +5.0f);
#endif
			}
			// if our current rotation is greater than the desiredAngle +1 then rotate in the negative direction
			else if (this.rigidbody.rotation.eulerAngles.z > desiredAngle+1.0)
            {
				rigidbody.MoveRotation(Quaternion.Euler(0.0f, 0.0f, this.rigidbody.rotation.eulerAngles.z - 5.0f));
#if UNITY_STANDALONE
				myCamera.transform.rotation = Quaternion.Euler(0.0f, 0.0f, myCamera.transform.rotation.eulerAngles.z -5.0f);
#endif
			}
			
			// special case for rotating from 270 -> 360 because rotating to 360 puts the eulerangle equal to 0. 
			if (this.rigidbody.rotation.eulerAngles.z < 1 && this.rigidbody.rotation.eulerAngles.z > -1 && desiredAngle == 360)
            {
				rigidbody.MoveRotation(z0Angle);
#if UNITY_STANDALONE
				myCamera.transform.rotation = Quaternion.Euler(0.0f, 0.0f,0.0f);
#endif
				desiredAngle = 0.0f;
			}
			// if our current rotation is with in desiredAngle +1/-1 then set it directly to the desired angle.
			else if (this.rigidbody.rotation.eulerAngles.z < desiredAngle + 1.0 && this.rigidbody.rotation.eulerAngles.z > desiredAngle -1.0)
            {
				//UnityEngine.Debug.Log("I am calling this for desiredAngle: " + desiredAngle);
				if (desiredAngle == 360.0)
                {
					//UnityEngine.Debug.Log("calling inside here");
					rigidbody.MoveRotation(z0Angle);
#if UNITY_STANDALONE
					myCamera.transform.rotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
#endif
					desiredAngle = 0.0f;
				}
				else
                {
					//UnityEngine.Debug.Log("setting the rotation to desired angle: " + desiredAngle);
					rigidbody.MoveRotation(Quaternion.Euler(0.0f, 0.0f, desiredAngle));
#if UNITY_STANDALONE
					myCamera.transform.rotation = Quaternion.Euler(0.0f, 0.0f, desiredAngle);
#endif
				}
			}
		}
        if (!gameStateManagerRef.unityRemote)
        {
            if (Input.GetKeyDown(KeyCode.Q))
            {
                qDown = true;
            }
            else if (Input.GetKeyDown(KeyCode.E))
            {
                eDown = true;
            }
        }		
		if (getGrounded() && Mathf.Abs(this.rigidbody.rotation.eulerAngles.z - desiredAngle) < 0.2)
		{
			//UnityEngine.Debug.Log("I am grounded and at the desiredAngle");
			if (Input.GetKeyUp(KeyCode.E) && eDown)
			{
				eDown = false;
				if (this.transform.rotation == z0Angle)
	            {
#if UNITY_STANDALONE
					myCamera.camera.orthographicSize = myCamera.camera.orthographicSize + 0.5f;
//					GameObject.FindGameObjectWithTag("MainCamera").transform.rotation = Quaternion.Euler(0f, 0f, 90f);
#endif
					//desiredAngle = 90.0F;
					currentDeviceOrientation = DeviceOrientation.Portrait;
	                inputManagerRef.initPlayerNonGroundedZRot = 90;
	                gameStateManagerRef.GetScreenManager().SetDeviceOrientation(DeviceOrientation.Portrait);
	                verticalSwipeDegreeAngle = 180;
				}
	
				else if(this.transform.rotation == z90Angle)
	            {
#if UNITY_STANDALONE
				    myCamera.camera.orthographicSize =myCamera.camera.orthographicSize - 0.5f;
#endif
					//GameObject.FindGameObjectWithTag("MainCamera").transform.rotation = Quaternion.Euler(0f, 0f, 180f);
					//this.GetComponent<Camera>().transform.rotation = Quaternion.Euler(0f, 0f, 180f);
					//desiredAngle = 180.0F;
					currentDeviceOrientation = DeviceOrientation.LandscapeRight;
	                gameStateManagerRef.GetScreenManager().SetDeviceOrientation(DeviceOrientation.LandscapeRight);
	                inputManagerRef.initPlayerNonGroundedZRot = 180;
	                verticalSwipeDegreeAngle = 270;
				}
	
				else if(this.transform.rotation == z180Angle)
	            {
#if UNITY_STANDALONE
				    myCamera.camera.orthographicSize = myCamera.camera.orthographicSize + 0.5f;
////					GameObject.FindGameObjectWithTag("MainCamera").transform.rotation = Quaternion.Euler(0f, 0f, 270f);
#endif
					//desiredAngle = 270.0F;
					currentDeviceOrientation = DeviceOrientation.PortraitUpsideDown;
	                inputManagerRef.initPlayerNonGroundedZRot = 270;
	                gameStateManagerRef.GetScreenManager().SetDeviceOrientation(DeviceOrientation.PortraitUpsideDown);
	                verticalSwipeDegreeAngle = 0;
				}
	
				else 
	            {
#if UNITY_STANDALONE
				    myCamera.camera.orthographicSize =myCamera.camera.orthographicSize - 0.5f;
#endif
					//desiredAngle = 360.0F;
					currentDeviceOrientation = DeviceOrientation.LandscapeLeft;
	                inputManagerRef.initPlayerNonGroundedZRot = 0;
	                gameStateManagerRef.GetScreenManager().SetDeviceOrientation(DeviceOrientation.LandscapeLeft);
	                verticalSwipeDegreeAngle = 90;
				}
	
	            inputManagerRef.initPlayerNonGroundedPos = gameObject.transform.position;
	            inputManagerRef.hasHorizontalCollision = false;
				//Debug.Log(this.transform.rotation.z);
			}
			else if(Input.GetKeyUp(KeyCode.Q) && qDown)
			{
				qDown = false;
	
				if(this.transform.rotation == z0Angle)
	            {
#if UNITY_STANDALONE
					myCamera.camera.orthographicSize = myCamera.camera.orthographicSize + 0.5f;
#endif
					//this.rigidbody.rotation = Quaternion.Euler(0f, 0f, 359.9f);
					//desiredAngle = 270.0F;
					currentDeviceOrientation = DeviceOrientation.PortraitUpsideDown;
	                inputManagerRef.initPlayerNonGroundedZRot = 270;
	                gameStateManagerRef.GetScreenManager().SetDeviceOrientation(DeviceOrientation.PortraitUpsideDown);
	                verticalSwipeDegreeAngle = 0;
				}
	
				else if(this.transform.rotation == z90Angle)
	            {
#if UNITY_STANDALONE
					myCamera.camera.orthographicSize = myCamera.camera.orthographicSize - 0.5f;
#endif
					//desiredAngle = 0.0F;
					currentDeviceOrientation = DeviceOrientation.LandscapeLeft;
	                inputManagerRef.initPlayerNonGroundedZRot = 0;
	                gameStateManagerRef.GetScreenManager().SetDeviceOrientation(DeviceOrientation.LandscapeLeft);
	                verticalSwipeDegreeAngle = 90;
				}
	
				else if(this.transform.rotation == z180Angle)
	            {
#if UNITY_STANDALONE
				    myCamera.camera.orthographicSize =myCamera.camera.orthographicSize + 0.5f;
#endif
					//desiredAngle = 90.0F;
					currentDeviceOrientation = DeviceOrientation.Portrait;
	                inputManagerRef.initPlayerNonGroundedZRot = 90;
	                gameStateManagerRef.GetScreenManager().SetDeviceOrientation(DeviceOrientation.Portrait);
	                verticalSwipeDegreeAngle = 180;
				}
	
				else 
	            {
#if UNITY_STANDALONE
				    myCamera.camera.orthographicSize =myCamera.camera.orthographicSize - 0.5f;
#endif
					//desiredAngle = 180.0F;
					//UnityEngine.Debug.Log("calling F when rot is equal to 270");
					currentDeviceOrientation = DeviceOrientation.LandscapeRight;
	                inputManagerRef.initPlayerNonGroundedZRot = 180;
	                gameStateManagerRef.GetScreenManager().SetDeviceOrientation(DeviceOrientation.LandscapeRight);
	                verticalSwipeDegreeAngle = 270;
				}
	
	            inputManagerRef.initPlayerNonGroundedPos = gameObject.transform.position;
	            inputManagerRef.hasHorizontalCollision = false;
			}
//			UnityEngine.Debug.Log("this is my transform.z: " + this.transform.rotation.z);
//			UnityEngine.Debug.Log("this is 270.z: " + Quaternion.Euler(0f, 0f, 270.0f).z);
//			UnityEngine.Debug.Log("this is the check for landscapeRight: " + (Mathf.Abs(Mathf.Abs(Quaternion.Euler(0f, 0f, 270.0f).z) - Mathf.Abs(this.transform.rotation.z)) < 0.2f));
			if ((Input.deviceOrientation == DeviceOrientation.Portrait || currentDeviceOrientation == DeviceOrientation.Portrait)
				&& currentDeviceOrientation != oldOrient && ((Mathf.Abs(this.transform.rotation.z - z0Angle.z) < 0.2f) || 
				(Mathf.Abs(this.transform.rotation.z - z180Angle.z) < 0.2f)))
			{
				oldOrient = Input.deviceOrientation;
	            inputManagerRef.initPlayerNonGroundedZRot = 90;
	            inputManagerRef.initPlayerNonGroundedPos = gameObject.transform.position;
	            inputManagerRef.hasHorizontalCollision = false;
	            gameStateManagerRef.GetScreenManager().SetDeviceOrientation(DeviceOrientation.Portrait);
	            verticalSwipeDegreeAngle = 180;
				desiredAngle = 90.0F;
			}
	
			else if ((Input.deviceOrientation == DeviceOrientation.LandscapeRight || currentDeviceOrientation == DeviceOrientation.LandscapeRight)
			&& currentDeviceOrientation != oldOrient	&& ((Mathf.Abs(this.transform.rotation.z - z90Angle.z) < 0.2f) || 
				(Mathf.Abs(Mathf.Abs(this.transform.rotation.z) - Mathf.Abs(z270Angle.z)) < 0.2f)))
	        {
				oldOrient = Input.deviceOrientation;
				//UnityEngine.Debug.Log("this is getting called");
	            inputManagerRef.initPlayerNonGroundedZRot = 180;
	            inputManagerRef.initPlayerNonGroundedPos = gameObject.transform.position;
	            inputManagerRef.hasHorizontalCollision = false;
				desiredAngle = 180.0F;
	            gameStateManagerRef.GetScreenManager().SetDeviceOrientation(DeviceOrientation.LandscapeRight);
	            verticalSwipeDegreeAngle = 270;
			}
	
			else if ((Input.deviceOrientation == DeviceOrientation.PortraitUpsideDown || currentDeviceOrientation == DeviceOrientation.PortraitUpsideDown)
				&&currentDeviceOrientation != oldOrient&& (desiredAngle == 180f || desiredAngle == 0.0f || desiredAngle == 360.0f))
//				&& ((Mathf.Abs(this.transform.rotation.z - Quaternion.Euler (0f, 0f, 180f).z) < 0.2f) || 
//				(Mathf.Abs(this.transform.rotation.z - Quaternion.Euler (0f, 0f, 0f).z) < 0.2f)))
			{
				oldOrient = Input.deviceOrientation;
				inputManagerRef.initPlayerNonGroundedZRot = 270;
	            inputManagerRef.initPlayerNonGroundedPos = gameObject.transform.position;
	            gameStateManagerRef.GetScreenManager().SetDeviceOrientation(DeviceOrientation.PortraitUpsideDown);
	            verticalSwipeDegreeAngle = 0;
	            inputManagerRef.hasHorizontalCollision = false;
				desiredAngle = 270f;
				//UnityEngine.Debug.Log("this is getting called");
				/*if((Mathf.Abs(this.transform.rotation.z - z0Angle.z) < 0.2f)){
					this.rigidbody.rotation = Quaternion.Euler(0f, 0f, 359.9f);
					desiredAngle = 270f;
				}
				else if((Mathf.Abs(this.transform.rotation.z - z180Angle.z) < 0.2f)){
					desiredAngle = 270f;
				}*/
	
			}
	
			else if((Input.deviceOrientation == DeviceOrientation.LandscapeLeft || currentDeviceOrientation == DeviceOrientation.LandscapeLeft)
			&& currentDeviceOrientation != oldOrient	&& ((Mathf.Abs(this.transform.rotation.z - z90Angle.z) < 0.2f) || 
				(Mathf.Abs(Mathf.Abs(this.transform.rotation.z) - Mathf.Abs(z270Angle.z)) < 0.2f)))
			{
				oldOrient = Input.deviceOrientation;
	            inputManagerRef.initPlayerNonGroundedZRot = 0;
	            inputManagerRef.initPlayerNonGroundedPos = gameObject.transform.position;
	            inputManagerRef.hasHorizontalCollision = false;
	            gameStateManagerRef.GetScreenManager().SetDeviceOrientation(DeviceOrientation.LandscapeLeft);
	            verticalSwipeDegreeAngle = 90;
				//desiredAngle = 0.0f;
				//commented out by Doug
				if (this.transform.rotation == z90Angle)
                {
					desiredAngle = 0.0F;
				}
				else if (Mathf.Abs(Mathf.Abs(this.transform.rotation.z) - Mathf.Abs(z270Angle.z))< 0.2f)
                {
					desiredAngle = 360.0F;
				}
			}
		}
	}
	
    // gets the script attached to the bottom of the player. 
    void Start()
    {
		stillDying = 0;
		// attempt to fix the slowing jump when next to wall. 
		this.rigidbody.collider.material.dynamicFriction = 0;
        this.rigidbody.collider.material.dynamicFriction2 = 0;
        this.rigidbody.collider.material.staticFriction = 0;
        this.rigidbody.collider.material.staticFriction2 = 0;
		this.collider.material.dynamicFriction = 0;
		this.collider.material.dynamicFriction2 = 0;
		this.collider.material.staticFriction = 0;
		this.collider.material.staticFriction2 = 0;
		
		// Get the global variables reference.
		GameObject gVar = GameObject.FindGameObjectsWithTag("globalVariables")[0];
		globalVariables = gVar.GetComponent<GVariables>();

        // the following is now needed
        // due to the prefab of 'MainObject'
        GameObject mainObject = GameObject.FindGameObjectsWithTag("MainObject")[0];
		
		// get a reference to the tear manager
		tearManager = GameObject.FindGameObjectWithTag("TearManager").GetComponent<TearManager>();
        if (tearManager)
        {
            tearLevel = true;
        }
        // get a reference to fold
		fold = GameObject.FindGameObjectWithTag("FoldObject").GetComponent<Fold>();
        if (fold)
        {
            foldLevel = true;
        }
        
        if (GameObject.FindGameObjectsWithTag("MainObject").Length > 1)
        {
            GameObject[] mainObjectList = GameObject.FindGameObjectsWithTag("MainObject");
            for (int i = 0; i < mainObjectList.Length; ++i)
            {
                if (mainObjectList[i].GetComponent<GameStateManager>().objectSaved)
                    mainObject = mainObjectList[i];
            }
        }
        if (foldLevel)
        {
            unfoldCollision = GameObject.Find("Graphics").GetComponent<UnfoldCollision>();
        }
        // Ensures all necessary scripts are added for the MainObject
        gameStateManagerRef = mainObject.GetComponent<GameStateManager>();
        gameStateManagerRef.EnsureCoreScriptsAdded();
        inputManagerRef = mainObject.GetComponent<InputManager>();
		animationManagerRef = mainObject.GetComponent<AnimationManager>();

		maxSpeed = baseMaxSpeed*this.transform.localScale.x;
		
		// get all the collision triggers for checking directions. 
		triggerHitting[] triggers = this.gameObject.GetComponentsInChildren<triggerHitting>();
		//Debug.Log("trigger.length: " + triggers.Length);
		foreach (triggerHitting trigger in triggers)
        {
			if (trigger.placementOf == "bottom")
            {
				//Debug.Log("bottom");
				playerhitbottom = trigger;
			}
			else if (trigger.placementOf == "left")
            {
				//Debug.Log("left");
				playerhitleft = trigger;
			}
			else if (trigger.placementOf == "right")
            {
				//Debug.Log("right");
				playerhitright = trigger;
			}
		}
		
		//Set the death animation timer total length
		deathAnimationTimer = deathAnimationStartTime;
		
		playerRespawnPoint = this.transform.position;

        spawnWatch.Start();
     	desiredAngle = this.rigidbody.rotation.eulerAngles.z;
		startingAngle = this.rigidbody.rotation.eulerAngles.z;
		currentDeviceOrientation = Input.deviceOrientation;
		
		myCamera = GameObject.FindGameObjectWithTag("MainCamera");
		background = GameObject.FindGameObjectWithTag("background");
		oldOrient = Input.deviceOrientation;
		myGoal = GameObject.FindGameObjectWithTag("EndGoal").GetComponent<LevelGoal>();
    }
	
	string printList(List<Collision> currList)
    {
		string output = "";
		foreach (Collision current in currList)
        {
			output += " " + current.gameObject.name + " ";
		}
		return output;
	}
	
	string printListNormals(List<Collision> currList)
    {
		string output = "";
		foreach (Collision current in currList)
        {
			output += " " + current.gameObject.name + ":" + current.contacts[0].normal.x + " ";
		}
		return output;
	}
   
    private void PlayerRespawn()
    {
#if UNITY_STANDALONE
		myCamera.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
		myCamera.camera.orthographicSize = 7f;
#endif
		stillDying = 0;
		desiredAngle = 0.0f;
		currentDeviceOrientation = DeviceOrientation.LandscapeLeft;
		oldOrient = currentDeviceOrientation;
		eDown = false;
		qDown = false;
        animationManagerRef.SetDirection(AnimationManager.AnimationDirection.RIGHT);
		//HUGE ISSUES WITH RESTARTING GAME, NOTHING IS RESET IS WORKING CORRECTLY - J.C.
		//Application.LoadLevel(Application.loadedLevel);
		//return;
		if(globalVariables.keys > 0)
        {
			globalVariables.keys = 0;
			
			GameObject[] keysInLevel = GameObject.FindGameObjectsWithTag("Key");
			for(int i = 0; i < keysInLevel.Length; ++i)
            {
				keysInLevel[i].collider.enabled = true;
				keysInLevel[i].renderer.enabled = true;
			}
		}
		if(globalVariables.coins > 0)
        {
			globalVariables.coins = 0;
			
			GameObject[] coinsInLevel = GameObject.FindGameObjectsWithTag("Coin");
			for(int i = 0; i < coinsInLevel.Length; ++i)
            {
				coinsInLevel[i].collider.enabled = true;
				coinsInLevel[i].renderer.enabled = true;
			}
		}
		myGoal.Reset();
		playerhitbottom.Reset();
		playerhitleft.Reset();
		playerhitright.Reset();
        if (foldLevel)
        {
            fold.ResetFold();
        }
        if (tearLevel)
        {
            tearManager.DeathReset();
        }
        // if(foldLevel)
			// unfoldCollision.restart();
        this.transform.position = playerRespawnPoint;
        this.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        spawnWatch.Reset();
        spawnWatch.Start();
        deathPos = new Vector3(-1000, -1000, -1000);
        firstDeathLoop = true;
        playerIsDead = false;
		tearOccurred = false;
		foldOccurred= false;
		currentState = playerStates.falling;
        inputManagerRef.fingerSwipeAngle = 0;
        gameObject.transform.position = playerRespawnPoint;
        inputManagerRef.initPlayerNonGroundedZRot = 0;	
        inputManagerRef.initPlayerNonGroundedPos = playerRespawnPoint;
		desiredAngle = startingAngle;
		this.rigidbody.velocity = Vector3.zero;
        gameStateManagerRef.GetScreenManager().SetDeviceOrientation(DeviceOrientation.LandscapeLeft);
        verticalSwipeDegreeAngle = 90;
    }
	
    private Vector3 deathPos = new Vector3(-1000, -1000, -1000);
    private bool firstDeathLoop = true;
	public bool bottomHitting = false;
	/// <summary>
	/// Update this instance.
	/// </summary>
    private void Update()
    {
		/*Check if the player has gone off the paper, then determine if dead or not.*/
        //UnityEngine.Debug.Log("POS " + Camera.mainCamera.WorldToScreenPoint(gameObject.transform.position));
        if (Input.GetKeyDown(KeyCode.P))
        {
            UnityEngine.Debug.Break();
        }
        Vector3 fwd = transform.TransformDirection(Vector3.forward * 3.0f);
		//Vector3 side = new Vector3(gameObject.collider.bounds.max.x, 0.0f, 0.0f);
		
		
        if (!playerIsDead && 
            spawnWatch.ElapsedMilliseconds > SPAWN_LIMIT)
        {
            //UnityEngine.Debug.Log("CHECK");
			if(CheckForPlayerDeath()) stillDying += Time.deltaTime;
			else stillDying = 0f;
			if(stillDying > SECONDS_TIL_DEATH) playerIsDead = true;
        }

		//Check if the player dies to trigger death Effects and level restart
        if (playerIsDead)
        {
            if (firstDeathLoop)
            {
				gameStateManagerRef.GetSoundManager().PlayAudio("deathWoosh", "SFX");
                deathPos = gameObject.transform.position;
                firstDeathLoop = false;
            }

            gameObject.transform.position = deathPos;
            if (gameStateManagerRef.GetAnimationManager().DeathComplete())
            {
                PlayerRespawn();
            }
            //UnityEngine.Debug.Log("DEAD");
            return;
        }

        
		//
		// TODO -- ADD LOGIC HERE TO ONLY ASSIGN IFF NOT == 0 !!!! PLEASE, J.C.
		//
		// attempt to fix the slowing jump when next to wall. 
		
		// start, code added to detect sliding better and remove the stuck to walls bug (Shawn)
		
		
		bottomHitting = playerhitbottom.getHitting();
		//UnityEngine.Debug.Log("bottomHitting: " + bottomHitting);
		collidingOnBottom = false;
				//added by douglas to check if level is fold or tear level before doing 
		//tear and fold specific checks
		handleFold();
		handleTear();
		//UnityEngine.Debug.Log(printList(currentCollisions));
		// if currentCollisions is not empty
		if(currentCollisions.Count > 0){
			//UnityEngine.Debug.Log(printListNormals(currentCollisions));
			Vector3 bestGroundNormal = Vector3.zero;
			// loop through each element
			foreach (Collision current in currentCollisions)
            {
				//UnityEngine.Debug.Log(current.gameObject.name + " normal.x: " + current.contacts[0].normal.x + " rotation: " + this.rigidbody.rotation.eulerAngles.z);
				// this is horrible i know XD : checks to see if colliding on bottom according to rotation
				//UnityEngine.Debug.Log("Checking gameobject: " + current.gameObject.name);

				if((current.contacts[0].normal.y > 0 && Mathf.Abs(this.rigidbody.rotation.eulerAngles.z) < 0.2) || 
					(current.contacts[0].normal.y < 0 && Mathf.Abs(this.rigidbody.rotation.eulerAngles.z - 180f) < 0.2) ||
					(current.contacts[0].normal.x < 0 && Mathf.Abs(this.rigidbody.rotation.eulerAngles.z - 90f) < 0.2)||
					(current.contacts[0].normal.x > 0 && Mathf.Abs(this.rigidbody.rotation.eulerAngles.z - 270f) < 0.2))
                {
					//UnityEngine.Debug.Log("Colliding on the bottom with " + current.gameObject.name);
					// if we are not grounded while colliding with something below then we are sliding
//					UnityEngine.Debug.Log(current.gameObject.name + " : normaly: " + current.contacts[0].normal.y);
					if(Mathf.Abs(this.rigidbody.rotation.eulerAngles.z - 90f) < 0.2 || Mathf.Abs(this.rigidbody.rotation.eulerAngles.z - 270f) < 0.2)
                    {
						if(Mathf.Abs(current.contacts[0].normal.x)>Mathf.Abs(bestGroundNormal.x) && Mathf.Abs(current.contacts[0].normal.x) > 0.2) 
                        {
							bestGroundNormal = current.contacts[0].normal;
							collidingOnBottom = true;
						}
					}
					else if(Mathf.Abs(this.rigidbody.rotation.eulerAngles.z) < 0.2 || Mathf.Abs(this.rigidbody.rotation.eulerAngles.z - 180f) < 0.2)
                    {
						if(Mathf.Abs(current.contacts[0].normal.y) > Mathf.Abs(bestGroundNormal.y) && Mathf.Abs(current.contacts[0].normal.y) > 0.2) 
                        {
							bestGroundNormal = current.contacts[0].normal;
							collidingOnBottom = true;
						}
					}
				}
			}
			groundNormal = bestGroundNormal;
			//UnityEngine.Debug.Log("groundNormal = " + groundNormal);
			
		}
		// if current collisions is empty reset sliding variables.
		else 
        {
			//Debug.Log("currentCollisions empty");
			//isSliding = false;
			groundNormal = Vector3.zero;
		}
		
		if(!bottomHitting && collidingOnBottom)
        {
			//UnityEngine.Debug.Log("We are setting sliding here!");
			currentState = playerStates.isSliding;
		}
		else if(bottomHitting && collidingOnBottom && !getRising())
        {
			currentState = playerStates.isGrounded;
			jumpReset = true;
		}
		else if(!bottomHitting && !collidingOnBottom && !getRising())
        {
			currentState = playerStates.falling;
			jumpReset = true;
		}
		// end, code added to detect sliding better (Shawn)
        if (foldLevel && tearLevel)
        {
            if (!fold.currentlyFolding && !tearManager.PlayerMovingPlatformState)
            {
                checkOrientation();
            }
        }
        else if (foldLevel)
        {
            if (!fold.currentlyFolding)
            {
                checkOrientation();
            }
        }
        else if (tearLevel)
        {
            if (!tearManager.PlayerMovingPlatformState)
            {
                checkOrientation();
            }
        }
        else
        {
            checkOrientation();
        }
	//	if(!fold.currentlyFolding && !tearManager.PlayerMovingPlatformState) checkOrientation();
        // call movement functions.
       // if (!inputManagerRef.GetPieceMoving())
	
		if (!fold.currentlyFolding && !tearManager.PlayerMovingPlatformState)
        {
            Movement();
        }
		
		if (fold.currentlyFolding || tearManager.PlayerMovingPlatformState)
		{
			this.rigidbody.velocity = new Vector3(0, 0, 0);
		}
		// hardcap code that makes it so the player cannot go faster than specified velocity hardCap
		if (this.rigidbody.velocity.x > hardCap)
        {
			this.rigidbody.velocity = new Vector3(hardCap, this.rigidbody.velocity.y, 0);
		}
		if (this.rigidbody.velocity.x < -hardCap)
        {
			this.rigidbody.velocity = new Vector3(-hardCap, this.rigidbody.velocity.y, 0);
		}
		if (this.rigidbody.velocity.y > hardCap)
        {
			this.rigidbody.velocity = new Vector3(this.rigidbody.velocity.x, hardCap, 0);
		}
		if (this.rigidbody.velocity.y < -hardCap)
        {
			this.rigidbody.velocity = new Vector3(this.rigidbody.velocity.x, -hardCap, 0);
		}
    }
	
	/// <summary>
	/// The only check platform masking once.
	/// </summary>
	private bool onlyCheckPlatformMaskingOnce = false;
	
//	/// <summary>
//	/// Players the death effects.
//	/// </summary>
//	private void PlayerDeathEffects()
//	{
//		if(deathAnimationTimer < 0)
//		{
//         //   inputManagerRef.isDead = true;
//			fold.ResetFold();
//			UnityEngine.Debug.Log("CHECK");
//			DeathEffects.GetComponent<ParticleSystem>().enableEmission = false;
//			DeathEffects.GetComponent<ParticleSystem>().Clear();
//			DeathEffectsSubEmitter.GetComponent<ParticleSystem>().enableEmission = false;
//			DeathEffectsSubEmitter.GetComponent<ParticleSystem>().Clear();
//					PlayerGraphics.GetComponent<MeshRenderer>().enabled = true;
//			this.transform.position = playerRespawnPoint;
//			deathAnimationTimer = 1.1f;
//			playerIsDead = false;
//			this.transform.rotation = Quaternion.Euler (0f, 0f, 0f);
//			
//         //   Application.LoadLevel(Application.loadedLevel);
//		}
//		else
//		{
//			//Keep track of time since death
//			deathAnimationTimer -= Time.deltaTime;
//			
//			//Creath death movement - For now, a slowed speed of their current direction
//			Vector3 newDirecton = rigidbody.velocity.normalized;
//			rigidbody.velocity = new Vector3((float)(deathSpeed * newDirecton.x), 
//											 (float)(deathSpeed * newDirecton.y), 
//											 0);
//			
//			if(deathAnimationTimer <= (deathAnimationStartTime - totalCharacterDeathAnimationTime))
//			{
//				
//				
//				//make suer audio has been triggered
//				if(Camera.main.GetComponent<AudioSource>().clip != DeathAudio)
//				{
//					Camera.main.GetComponent<AudioSource>().clip = DeathAudio;
//					Camera.main.GetComponent<AudioSource>().priority = 0;
//					Camera.main.GetComponent<AudioSource>().pitch = 1.1f;
//					Camera.main.GetComponent<AudioSource>().volume = 100.0f;
//				}
//				//Play audio from Camera (TODO - Integrate Doug's audio work -> J.C.)
//				if(!Camera.main.GetComponent<AudioSource>().isPlaying && !havePlayedDeathAudio)
//				{
//					Camera.main.GetComponent<AudioSource>().Play();
//					havePlayedDeathAudio = true;
//				}
//				
//			}
//		}
//	}
	
	/// <summary>
	/// Checks for player death when colliding with desk to trigger death effects
	/// </summary>
	private bool CheckForPlayerDeath()
	{
        /** TONTON NOTICE 
         * 
         * I changed the following
         * When the entire piece is still intact,
         * I use the bounds approximation to save performance.
         * Then once we've torn and made two pieces then I use
         * your ray-casting method. 
         ********* This could pose some complications with fold but it works *****
         * pretty well in terms of performance.  I went from ~30 to ~60 FPS.
         */
		if(!tearManager.HaveTornOnce)
		{
            if (gameStateManagerRef.GetWorldCollision().PointInsideObject(background,
                Camera.main.WorldToScreenPoint(gameObject.transform.position)))
            {
                return false;
            }
            else
            {
                if (background.GetComponent<MeshRenderer>().isVisible)
                {
                    return true;
                }
            }
		}
		else 
        {
            //Cast ray to see if player is hitting desk
            Vector3 fwd = transform.TransformDirection(Vector3.forward);// * 3.0f);
	        Vector3 max = gameObject.collider.bounds.max;
	        Vector3 min = gameObject.collider.bounds.min;
            RaycastHit hit;

	        if(!tearManager.PlayerCollidingMaterialChange())
	        {
		        if (Physics.Raycast(new Vector3(max.x+0.01f, gameObject.transform.position.y, (max.z+min.z)/2.0f), fwd, out hit, 2.0f))
		        {
		            if (hit.transform.gameObject.tag == "DeadSpace")//|| hit.transform.gameObject.name == "rayTraceBlocker")
		            {
		                return true;
		            }
		            //Debug.Log(hit.transform.gameObject.name);
		        }
		        if (Physics.Raycast(new Vector3(gameObject.transform.position.x, max.y+0.01f, (max.z+min.z)/2), 
			        fwd, out hit, 2.0f))
		        {
		            if (hit.transform.gameObject.tag == "DeadSpace")
		            {
		                return true;
		            }
		        }
		        if (Physics.Raycast(new Vector3(min.x-0.01f, gameObject.transform.position.y, (max.z+min.z)/2), 
			        fwd, out hit, 2.0f))
		        {
		            if (hit.transform.gameObject.tag == "DeadSpace")
			        {
		        //	UnityEngine.Debug.Log(hit.transform.gameObject.name);
			        //				UnityEngine.Debug.Log("HI");
		
		                return true;
		            }
		        }
		        if (Physics.Raycast(new Vector3(gameObject.transform.position.x, min.y-0.01f, (max.z+min.z)/2), 
			        fwd, out hit, 2.0f))
		        {
		            if (hit.transform.gameObject.tag == "DeadSpace")
		            {
		        //	UnityEngine.Debug.Log(hit.transform.gameObject.name);
			        //				UnityEngine.Debug.Log("HI");
		                return true;
		            }
		        }

                if (fold.isFolded)
                {
                    if (/*!tearManager.PlayerCollidingWithTornPieceCheck() &&*/ (PointInTriangle(
                    gameObject.transform.position, fold.changeMeshScript.coverMaxX,
                    fold.changeMeshScript.coverMaxY, fold.changeMeshScript.coverMinX) ||
                    PointInTriangle(new Vector3(gameObject.transform.position.x, gameObject.transform.position.y,
                    gameObject.transform.position.z), fold.changeMeshScript.coverMaxX,
                    fold.changeMeshScript.coverMinY, fold.changeMeshScript.coverMinX)))
                    {
                        return true;
                    }
                }
	        }
        }
        return false;
	}
	
	// clears list after delay
	IEnumerator delayClearList()
    {
		yield return new WaitForSeconds(1);
		currentCollisions.Clear();
		//Debug.Log("Clear has been called");
	}
	
	// handles checking if the currentCollisions list needs to be cleared when a tear happens
	void handleTear()
    {
		// logic for finding out if a tear has started and when that torn piece gets placed we need to clear the currentCollision list
		if (tearManager.TornPieceCurrentlyMaskingCollision && !tearOccurred)
        {
			//UnityEngine.Debug.Log("Need to clear list from tear");
			//StartCoroutine(delayClearList());
			currentCollisions.Clear();
			tearOccurred = true;
		}
		// if PlayerMovingPlatformState is true then we need to set tearOccurred = false signalling that a new tear is happening. 
		else if (tearManager.PlayerMovingPlatformState)
        {
			tearOccurred = false;
		}
	}
	
	// handles checking if the currentCollisions list needs to be cleared when a fold happens
	void handleFold()
    {
		//UnityEngine.Debug.Log("fold.currentlyFolding: " + fold.currentlyFolding + " this.state: " + currentState);
		// checks if a fold has not occured and if isFolded is true then we need to clear the list because a fold just occurred
		if (!fold.currentlyFolding && !foldOccurred)
        {
			//UnityEngine.Debug.Log("clearing list in fold");
			currentCollisions.Clear();
			foldOccurred = true;
		}
		// if currentlyFolding is true then a new fold has just been initiated and we need to set foldOccurred back to reset
		else if (fold.currentlyFolding && foldOccurred)
        {
			foldOccurred = false;
		}
	}
	
	// used in finding the correct horizontal direction to travel
	public Vector3 groundNormal = Vector3.zero;
	// list of objects that the player is currently colliding with
	public List<Collision> currentCollisions = new List<Collision>();
	
	// returns true or false if a Collision with the same name as hit exists
	bool ListContainsElementOfName(List<Collision> currList, Collision hit)
    {
		foreach (Collision current in currList)
        {
			if((current != null) && (current.gameObject != null) && (hit.gameObject != null))
            {
				if(current.gameObject.name.Equals(hit.gameObject.name))
                {
					return true;
				}
			}
		}
		return false;
	}
				
	// returns true or false if a Collision with the same name as string name exists
	bool ListContainsElementOfName(List<Collision> currList, string name)
    {
		foreach (Collision current in currList)
        {
			if((current != null) && (current.gameObject != null) && (name != null))
            {
				if(current.gameObject.name.Equals(name))
                {
					return true;
				}
			}
		}
		return false;
	}
	
		// returns true or false if a Collision with the same name as string name exists
	bool ListContainsElementOfGameObject(List<Collision> currList, GameObject obj)
    {
		foreach (Collision current in currList)
        {
			if((current != null) && (current.gameObject != null) && (obj != null))
            {
				if(current.gameObject.Equals(obj))
                {
					return true;
				}
			}
		}
		return false;
	}
	
	// search for an Collision element in a list by matching their names, returns the element
	Collision GetElementByName(List<Collision> currList, string name)
    {
		foreach(Collision current in currList)
        {
			if(current.gameObject.name.Equals(name))
            {
				//Debug.Log("Removing " + current.gameObject.name);
				return current;
			}
		}
		return null;
	}
	
	Collision GetElementByGameObject(List<Collision> currList, GameObject obj)
    {
		foreach(Collision current in currList)
        {
			if(current.gameObject.Equals(obj))
            {
					return current;
			}
		}
		return null;
	}
		
	
	// upon entering collision with an object add it to teh currentCollisions list if it is not already there. 
	void OnCollisionEnter(Collision hit)
    {
		//Debug.Log("Colliding with : " + hit.gameObject.name);
		if(hit != null && hit.gameObject.CompareTag("Platform") || hit.gameObject.CompareTag("FoldPlatform"))
        {
			if(!ListContainsElementOfGameObject (currentCollisions, hit.gameObject))
            {
				//Debug.Log("not in currentCollisions");
				//Debug.Log("Adding(enter): " + hit.gameObject.name);
				currentCollisions.Add(hit);
			}
		}
	}
	
	// keep checking if a platform we are on is in the list just incase it got removed
	void OnCollisionStay(Collision hit)
    {
		if(hit != null && hit.gameObject.CompareTag("Platform") || hit.gameObject.CompareTag("FoldPlatform"))
        {
			if(!ListContainsElementOfGameObject(currentCollisions, hit.gameObject))
            {
				//Debug.Log("Adding(stay): " + hit.gameObject.name);
				currentCollisions.Add(hit);
			}
			else 
            {
				currentCollisions.Remove(GetElementByGameObject(currentCollisions, hit.gameObject));
				currentCollisions.Add(hit);
			}
		}
	}
	
	// upon exiting a platform remove the Collision from currentCollisions
	void OnCollisionExit(Collision hit)
    {
		if(hit.gameObject.CompareTag("Platform") || hit.gameObject.CompareTag("FoldPlatform"))
        {
			if(ListContainsElementOfGameObject(currentCollisions, hit.gameObject))
            {
				//Debug.Log("Removing(exit): " + hit.gameObject.name);
				currentCollisions.Remove(GetElementByGameObject(currentCollisions, hit.gameObject));
			}
		}
	}

    private bool PointInTriangle(Vector3 p, Vector3 a,Vector3 b,Vector3 c)
	{
//	    if(SameSide(p,a, b,c) && SameSide(p,b, a,c)
//	        && SameSide(p,c, a,b)) return true;
//	    else return false;
		
		// Compute vectors        
		Vector3 v0 = c - a;
		Vector3 v1 = b - a;
		Vector3 v2 = p - a;
		// Compute dot products
		float dot00 = Vector3.Dot(v0, v0);
		float dot01 = Vector3.Dot(v0, v1);
		float dot02 = Vector3.Dot(v0, v2);
		float dot11 = Vector3.Dot(v1, v1);
		float dot12 = Vector3.Dot(v1, v2);
		
		// Compute barycentric coordinates
		float invDenom = 1 / (dot00 * dot11 - dot01 * dot01);
		float u = (dot11 * dot02 - dot01 * dot12) * invDenom;
		float v = (dot00 * dot12 - dot01 * dot02) * invDenom;

        if ((u >= 0) && (v >= 0) && (u + v < 1))
        {
            return true;
        }
        else
        {
            return false;
        }
	}
	/// <summary>
	/// Stops the players x velocity
	/// </summary>
	public void StopPlayer()
	{
		rigidbody.velocity = new Vector3(0f, rigidbody.velocity.y, rigidbody.velocity.z);
	}
//	void OnTriggerEnter(Collider other)
//	{
//		//need to find a better spot for this check
//		if(foldLevel)
//		{
//			if(!fold.currentlyFolding && !fold.needsToUnfold){
//		        if (other.transform.name == "coverup")
//		        {
//		            playerIsDead = true;
//		        }
//			}
//		}
//	}
}