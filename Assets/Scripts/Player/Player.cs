using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using FlintInputSystem;

namespace SonicFramework
{
	public enum Character
	{
		sonic = 0,
		tails = 1,
		knuckles = 2,
	}
	
	public enum SpringType
	{
		vertical = 0,
		diagonal = 1,
		horizontal = 2,
	}
	
	public enum GroundMode
	{
		floor = 0,
		rightWall = 1,
		ceiling = 2,
		leftWall = 3,
	}
	
	public enum MostlyMoving
	{
		down = 0,
		right = 1,
		up = 2,
		left = 3,
	}

	public enum CollisionLayer
	{
		A = 0,
		B = 1,
	}
	
	public class Player : MonoBehaviour
	{
		/// <Summary>
		/// Debug options.
		/// </Summary>
		[Header("Debug")]
		private bool LastStep;
		
		public bool DebugMode;
		public GameObject hitboxRect;
		[Tooltip("Snaps the characters angle to the nearest 45 degrees.")]
		public bool SnapCharacterAngle;
		
		/// <Summary>
		/// Basic variables.
		/// </Summary>

		[Header("Basic")]
		[Tooltip("The Current position of the player.")]
		[SerializeField] private Vector2 position;		
		[Tooltip("How many pixels the player moves each frame.")]
		[SerializeField] private Vector2 velocity;	
		[Tooltip("Movement calculations will run this many times per frame.")]   
		[SerializeField] private int subSteps;		
		[Tooltip("The time between each sub step, used in place of \"Time.deltaTime\" inside of \"MovementUpdate();\". Based on pixels per frame instead of units per second.")]   
		[System.NonSerialized] public float stepDelta;
		[Tooltip("Whether or not the player is underwater")] 
		[SerializeField] private bool underwater;
		[Tooltip("The layer mask that the player's sensors will detect")] 
		[SerializeField] private LayerMask groundMask;
		[SerializeField] private LayerMask triggerLayer;
		private Vector2Int gridPosition;
		private Vector2Int gridPositionOld;
		public CollisionLayer collisionLayer;
		[Tooltip("The layer mask that the player's sensors will detect for jumping through platforms")] 
		[SerializeField] private LayerMask jumpThroughMask;
		[Tooltip("The Camera that the player is using")] 
		public SonicCamera cam;
		private float GlobalSpeedCap = 16f;
		[System.NonSerialized] public Vector2 movingPlatformVelocity;
		
		public Tilemap cachedTilemap;
		public Collider2D cachedTilemapCollider;
		public SonicObject cachedSonicObject;
		
		[System.NonSerialized] public bool InputJump;
		[System.NonSerialized] public bool InputJumpLastFrame;
		
		[System.NonSerialized] public bool InputRight;
		[System.NonSerialized] public bool InputLeft;
		[System.NonSerialized] public bool InputUp;
		[System.NonSerialized] public bool InputUpLastFrame;
		[System.NonSerialized] public bool InputDown;
		[System.NonSerialized] public bool InputDownLastFrame;
		
		private MostlyMoving mostlyMoving 
		{
			get {
				if(Mathf.Abs (velocity.x) >= Mathf.Abs (velocity.y)) {
					return velocity.x > 0 ? MostlyMoving.right : MostlyMoving.left;
				}
				else {
					return velocity.y > 0 ? MostlyMoving.up : MostlyMoving.down;
				}
			}
		}
		
		private LayerMask collisionLayerMask
		{
			get {
				if(collisionLayer == CollisionLayer.A) return PhysicsManager.Instance.layerAMask;
				else 
					if(collisionLayer == CollisionLayer.B) return PhysicsManager.Instance.layerBMask;
				else 
					return PhysicsManager.Instance.layerAMask;
			}
		}
		
		/// <Summary>
		/// Scriptable objects that hold a list of constants used for movement.
		/// </Summary>

		[Header("Movement Settings")]
		[Tooltip("Movement settings containing movement constants: onland")] 
		[SerializeField] private MovementSettings onlandMovementSettings;
		[Tooltip("Movement settings containing movement constants: underwater")] 
		[SerializeField] private MovementSettings underwaterMovementSettings;
		
		/// <Summary>
		/// Variables defining characteristics.
		/// </Summary>

		[Header("Character")]
		[Tooltip("The current character, used for special abilities like the dropdash or flying")]
		[SerializeField] private Character currentCharacter;
		[Tooltip("Length from the ground to the players center: standing")]
		[SerializeField] private float standingHeight;
		[Tooltip("Length from the ground to the players center: ball")]
		[SerializeField] private float ballHeight;
		[Tooltip("Player collision width: standing")]
		[SerializeField] private float standingWidth;
		[Tooltip("Player collision width: ball")]
		[SerializeField] private float ballWidth;
		[Tooltip("Length for the wall sensors")]
		[SerializeField] private float pushRadius;
		[Tooltip("The offset of the wall sensors")]
		[SerializeField] private float wallSensorVerticalOffset;
		private Vector2 wallSensorOffset;
		[Tooltip("The Object which sprites are attacted to")]
		[SerializeField] public GameObject spriteHolder;
		public bool isCurrentPlayer;
		
		public bool lookingRight = true;
		
		private float winningSensorGroundAngle;
		private float lookingAxis
		{
			get {
				return lookingRight ? 1 : -1;
			}
		}
		
		[SerializeField] private bool rolling;
		private bool jumped;
		private bool jumpedFromGround;
		public bool crouched;
		private bool isBall { get { return rolling || jumped; } }
		
		[Tooltip("The Sensors that define collision")]
		[SerializeField] public Sensor[] Sensors = new Sensor[9];
		
		/// <Summary>
		/// Variables associated with being on the ground.
		/// </Summary>

		[Header("Ground Motion")]
		[Tooltip("The angle of the ground, in degrees, under the player being used for movement calculations.")]
		[SerializeField] private float groundAngle;
		[Tooltip("The velocity that the player has while grounded, defines velocity while grounded.")] 
		[SerializeField] private float groundSpeed;
		public float oldGroundSpeed;
		private float olderGroundSpeed;
		[Tooltip("Whether or not the player is grounded")] 
		[SerializeField] private bool grounded;
		public bool inTunnel;
		private bool oldGrounded;
		private bool olderGrounded;
		[Tooltip("The force that slides you down slopes: not rolling")]
		[SerializeField] private float defaultSlopeFactor = 0.125f;
		[Tooltip("The force that slides you down slopes: rolling uphill")]
		[SerializeField] private float rollUphillSlopeFactor = 0.09f; // 0.078125f
		[Tooltip("The force that slides you down slopes: rolling downhill")]
		[SerializeField] private float rollDownhillSlopeFactor = 0.2f;
		[Tooltip("Whether or not the player is spindashing")]
		public SpringType springType;
		public bool spinDashing;
		public bool skidding;
		public bool oldSkidding;
		public float drpspd;
		float dropDashTimer;
		public bool bouncedOffSpring;
		public bool springBounceStart;
		public bool walldashed;
		float walldashDir;
		public bool walldashKicked;
		float walldashKickDelay;
		public bool walldashCharged;
		public PlayerSprite playerSprite;
		public bool spinDashRelease;
		public float spinrev;
		
		[SerializeField] public float controlLockTimer;
		
		[SerializeField] public float airControlLockTimer;
			
		[SerializeField] private float stickRadius;
		
		[SerializeField] public GroundMode groundMode;
		
		private Vector2 groundModeUp 
		{
			get {
				switch (groundMode) {
					case GroundMode.floor:
						return new Vector2(0, 1);

					case GroundMode.rightWall:
						return new Vector2(-1, 0);

					case GroundMode.ceiling:
						return new Vector2(0, -1);

					case GroundMode.leftWall:
						return new Vector2(1, 0);

					default :
						return new Vector2(0, 1);

				}
			}
		}

		private Vector2 groundModeRight { get { return new Vector2(groundModeUp.y, -groundModeUp.x); } }
		private float groundModeAngle 
		{
			get { 
				switch (groundMode) {
					case GroundMode.floor:
						return 0;

					case GroundMode.rightWall:
						return 90;

					case GroundMode.ceiling:
						return 180;

					case GroundMode.leftWall:
						return -90;

					default :
						return 0;
				}
			} 
		}
		
		public Vector2 GroundModeUp { get { return groundModeUp; } }
		
		public Vector2 GroundModeRight { get { return groundModeRight; } }
		
		// Returns if the player is rolling or not.
		public bool Rolling { get { return rolling; } set { rolling = value; }}

		// Returns if the player has jumped or not.
		public bool Jumped { get { return jumped; } }
		
		// Returns if the player is a ball or not.
		public bool IsBall { get { return isBall; } }
		
		// Returns if the player is a ball or not.
		public bool SpinDashing  { get { return spinDashing; } }
		
		// Returns the player's current height when a ball or not.
		public float heightRadius { get { return isBall? ballHeight/2 : standingHeight/2; } }

		// Returns the player's current width when a ball or not.
		private float widthRadius { get { return isBall? ballWidth/2 : standingWidth/2; } }
			
		// Returns the player's current size.
		public Vector2 sizeRead { get { return new Vector2(widthRadius * 2, heightRadius * 2); } }
		
		// Returns the player's current position.
		public Vector2 Position { get { return position; } set { position = value; }}
		
		// Returns the player's current velocity.
		public Vector2 Velocity { get { return velocity; } set { velocity = value; }}
		
		// Returns if the player is on the ground.
		public bool Grounded 
		{ 
			get { return grounded; } 
			set { grounded = value; } 
		}
		
		// Returns the players ground speed.
		public float GroundSpeed 
		{ 
			get { return groundSpeed; } 
			set { groundSpeed = value; } 
		}
		
		// Returns the players ground angle.
		public float GroundAngle 
		{ 
			get { return groundAngle; } 
			set { groundAngle = value; } 
		}
		
		// Returns if the player's underwater.
		public bool Underwater 
		{ 
			get { return underwater; } 
			set { underwater = value; } 
		}
		
		public Character CurrentCharacter 
		{ 
			get { return currentCharacter; } 
			set { currentCharacter = value; } 
		}
		
		// The current movement settings based on if the player is onland or not.
		private MovementSettings movementSettings { get { return underwater? underwaterMovementSettings : onlandMovementSettings; } }
		
		// Start is called before the first frame update.
		void Start()
		{
			position = new Vector2(transform.position.x, transform.position.y);
		}

		// Update is called once per frame.
		public void PlayerUpdate(float delta)
		{	
			stepDelta = delta;
			subSteps = PhysicsManager.Instance.subSteps;
			MovementUpdate();
		}
		
		float oldGroundAngle;
		
		bool movingForwardsInAir;
		
		public void DebugUpdate()
		{
			if(PhysicsManager.Instance.debugMode) {
				foreach(Sensor sensor in Sensors) {
					Vector2 RayDebugOffset = new Vector2(0, 0);
					float lengthOffset = 0;

					sensor.DrawRay(this, RayDebugOffset, lengthOffset); 
				}
			}
		}
		void MovementUpdate()
		{	
			spinDashRelease = false;
			
			if(grounded) {
				// Run GroundMovement if grounded.
				GroundMovement();
				
				if(!grounded && (movingPlatformVelocity != Vector2.zero)) {
					velocity.x += (movingPlatformVelocity.x / stepDelta) * 16f;
					position += new Vector2(movingPlatformVelocity.x, movingPlatformVelocity.y);
				}	
			}
			else { 
				// Run AirMovment if not grounded.
				AirMovment();
			}
			
			TriggerHandler();
			List<Sensor> s = new List<Sensor>() { Sensors[0], Sensors[1], Sensors[2], Sensors[3], Sensors[4], Sensors[5], Sensors[8]};
			
			foreach(Sensor _s in s) {
				SonicObject obj = _s.info.hitObject;
			
				if(obj != null) {
					obj.CollidePlayerWithObject(this, _s);
				}
			}

			InputJumpLastFrame = InputJump;
			InputUpLastFrame = InputUp;
			InputDownLastFrame = InputDown;
			olderGrounded = oldGrounded;
			oldGrounded = grounded;
			olderGroundSpeed = oldGroundSpeed;
			oldGroundSpeed = groundSpeed;
			gridPositionOld = gridPosition;

			MovementApplier();
		}

		bool groundRollSoundCheck = false;
		bool dropdashStart = false;

		void GroundMovement()
		{
			bool whileNormal = (grounded && !isBall);
			
			bool whileRolling = (grounded && isBall);
			
			float sinGroundAngle = Mathf.Sin(groundAngle * Mathf.Deg2Rad);
			float cosGroundAngle = Mathf.Cos(groundAngle * Mathf.Deg2Rad);

			Sensor winningGroundSensor = GetWinningSensor(Sensors[0], Sensors[1]);

			groundRollSoundCheck = false;
			bouncedOffSpring = false;
			dropdashStart = false;
			springBounceStart = false;
			walldashed = false;
			walldashKicked = false;
			walldashCharged = false;
			jumpedFromGround = false;
			jumped = false;

			if((currentCharacter == Character.sonic) && (drpspd == 8)) {
				float drpmax = 12; 
				if(movingForwardsInAir) {
					groundSpeed = Mathf.Clamp((groundSpeed / 4f) + (drpspd * lookingAxis), -drpmax, drpmax);
				}
				else {
					if(groundAngle == 0) {
						groundSpeed = Mathf.Clamp((drpspd * lookingAxis), -drpmax, drpmax);
					}
					else {
						groundSpeed = Mathf.Clamp((groundSpeed / 2f) + (drpspd * lookingAxis), -drpmax, drpmax);
					}
				}
				SoundManager.Instance.SpindashRelease();
				SoundManager.Instance.dropdash.Stop();
				GameObject dropdashDustObj = Instantiate(playerSprite.dustPrefab) as GameObject;
				Dust dropdashDust = dropdashDustObj.GetComponent<Dust>();
				dropdashDust.currentAnim = "dropdashDust";
				dropdashDust.player = this;
				dropdashDust.instantiate();
				cam.camDelay = 32f - drpspd;
				rolling = true;
				drpspd = 0;
				dropDashTimer = 0;
			}
			
			if(!spinDashing) {
				float slopeFactor = 0;
			
				if(whileNormal)	{	
					// Check for special animations that prevent control (such as balancing).
				
					// Check for starting a spindash while crouched.
					if(crouched) {
						if(InputJump && !InputJumpLastFrame) {
							spinDashing = true;
							spinrev = 2f;
							SoundManager.Instance.SpindashCharge();
						}
					}
				
					// Use default slopeFactor.
					slopeFactor = defaultSlopeFactor;
				}
					
				if(whileRolling) {	
					// Use slope factor rollup and rolldown.
					bool _rollingUpHill = (Mathf.Sign(sinGroundAngle) == Mathf.Sign(groundSpeed));
					slopeFactor = _rollingUpHill? rollUphillSlopeFactor : rollDownhillSlopeFactor;
				}
				
				// Adjust Ground Speed based on current Ground Angle (To simulate gravity on steep slopes).
				groundSpeed -= slopeFactor * sinGroundAngle * stepDelta;
				
				if(Mathf.Abs(groundSpeed) > GlobalSpeedCap) {
					groundSpeed = GlobalSpeedCap * Mathf.Sign(groundSpeed);
				}
				
				// Check for starting a jump
				if(InputJump && !InputJumpLastFrame && !crouched && !spinDashing && !inTunnel) {	
					float extraSenseLength = 0f;
					if(rolling) extraSenseLength = 0.2f;
					// Check the top sensors. If they collide, don't jump.
					if(!(CheckSensor(Sensors[2], extraSenseLength) || CheckSensor(Sensors[3], extraSenseLength))) {
						grounded = false;
						jumped = true;
						jumpedFromGround = true;
						SoundManager.Instance.Jump();
						rolling = true;
						// Jump off in the direction of the ground angle.
						velocity.x += movementSettings.JumpVelocity * -sinGroundAngle;
						velocity.y += movementSettings.JumpVelocity * cosGroundAngle;
						
						position.x += ((1/16f) * -sinGroundAngle);
						position.y += ((1/16f) *  cosGroundAngle); 
					}
				}
				
				// Update Ground Speed based on directional input and apply friction/deceleration.
				GroundInputAndFriction();
				
				if(whileNormal)	{
					if(Mathf.Abs(oldGroundSpeed) < 0.5f && InputDown && Mathf.Abs(groundAngle) <= 26.5f) {
						crouched = true;
						groundSpeed = 0;
					}
					// Check for starting ducking, balancing on ledges, etc.
				}
				
				if(crouched && !InputDown) {
					
					crouched = false;
				}
			}
			else { 
				if(InputJump && !InputJumpLastFrame) {
					SoundManager.Instance.SpindashCharge();
					spinrev += 2;
					spinrev = Mathf.Clamp(spinrev, 0f, 8f);
				}
				
				if(spinrev > 0) {
					spinrev -= ((spinrev * 0.125f) / (60/16f)) * stepDelta; 	
				}
				
				if(!InputDown) { 
					if(spinrev > 0) {
						SoundManager.Instance.SpindashRelease();
						SoundManager.Instance.spindashCharge.Stop();
						spinDashRelease = true;
						groundSpeed = (8 + (spinrev / 2f)) * lookingAxis;
						cam.camDelay = 32f - spinrev;
						rolling = true;
						spinDashRelease = false;
						spinrev = 0;
					}
					
					spinDashing = false;		
				}
			}

			// Wall sensor collision occurs.
			DoHorizontalCollisions();	
			
			if(rolling && grounded) {
				if(groundMode == GroundMode.ceiling && (CheckSensor(Sensors[2], 0f) || CheckSensor(Sensors[3], 0f))) {
					groundMode = GroundMode.floor;
					position -= 4/16f * groundModeUp;	
				}	
			}
			
			if(whileNormal) {
				// Check for starting a roll.
				if(((Mathf.Abs(groundSpeed) >= 1f) && (InputDown && !InputDownLastFrame) && !rolling) || inTunnel) {
					SoundManager.Instance.Roll();
					rolling = true;
					position -= 10/16f * groundModeUp;
				}
			}
			else if((Mathf.Abs(oldGroundSpeed) <= 1f)) {
				// Check if you should uncurl
				if(rolling && !inTunnel) {
					position += 5/16f * groundModeUp;
					rolling = false;
				}	
			}
			
			if(rolling && inTunnel) {
				rolling = true;
				controlLockTimer = 1;
				if((Mathf.Abs(groundSpeed) <= 1f) && (Mathf.Abs(groundAngle) < 45f)) {
					groundSpeed = 4 * -Mathf.Sign(groundSpeed);
				}
				// Check the top sensors. If they collide, don't jump.
				if(!(CheckSensor(Sensors[2], 0.4f) || CheckSensor(Sensors[3], 0.4f))) {
					inTunnel = false;
				}
			}
			
			if(jumped) return;
			
			// Handle camera boundaries (keep the Player inside the view and kill them if they touch the kill plane).	

			// Move the Player object			
			velocity.x = groundSpeed * cosGroundAngle;
			velocity.y = groundSpeed * sinGroundAngle;
			
			UpdatePosition();
				
			// Floor sensor collision occurs.			
			oldGroundAngle = groundAngle;
			
			UpdateGroundMode();
			
			DoVerticalCollisions();
			
			// Collide with any objects detected.
			if(grounded) {
				SonicObject groundedObj = GetWinningSensor(Sensors[0],Sensors[1]).info.hitObject;
			
				if(groundedObj != null) {
					if(groundedObj.isMovingPlatform && (groundMode == GroundMode.floor))
						groundedObj.platformScript.AttachPlayerToPlatform(this);
				}
			}
			
			// Check for falling when Ground Speed is too low on walls/ceilings.
			float maxStickDelta = 50f;
			float deltaAngle = Mathf.Abs(Mathf.DeltaAngle(oldGroundAngle, groundAngle));
			
			if((deltaAngle >= maxStickDelta)) {
				grounded = false;
				controlLockTimer = 30f;
				position += 0.001f * groundModeUp;
			}
			
			if(controlLockTimer == 0) {
				// slip
				if(!inTunnel) {
					if (Mathf.Abs(olderGroundSpeed) < 2.5f && Mathf.Abs(groundAngle) >= 35f) {
						controlLockTimer = 30f;
						
						if(Mathf.Abs(groundAngle) >= 90) {
							grounded = false;
							return;
						}
						else {
							if(groundAngle < 0) {
								groundSpeed += 0.5f;
							}
							else {
								groundSpeed -= 0.5f;
							}
						}
					}
				}
			}
			else
			{
				controlLockTimer -= 1 * stepDelta;
				if(controlLockTimer <= 0) {
					controlLockTimer = 0;
				}
			}
		}
		
		void AirMovment()
		{	
			if(rolling && inTunnel) {
				if(!(CheckSensor(Sensors[2], 0.4f) || CheckSensor(Sensors[3], 0.4f))) {
					inTunnel = false;
				}
			}
			// Check for jump button release (variable jump velocity).
			if(jumped && !InputJump && InputJumpLastFrame) {
				if(velocity.y > 4f) {
					velocity.y = 4f;
				}
				jumped = false;
			}
			
			if(!jumped && InputJump && (currentCharacter == Character.sonic) && !walldashed && rolling && jumpedFromGround) {
				dropDashTimer += stepDelta;
				if(dropDashTimer >= 20) {
					if(!dropdashStart) {
						dropdashStart = true;
						SoundManager.Instance.Dropdash();
					}
					drpspd = 8;
				} 
			}
			else {
				drpspd = 0;
				dropDashTimer = 0;
			}
			
			if(InputDown && !InputDownLastFrame) {
				groundRollSoundCheck = true;
			}
			
			bool canWallDash = false;
			
			if(!jumped && InputJump && (currentCharacter == Character.sonic) && canWallDash) {
				walldashCharged = true;
			}
		
			if(walldashCharged && (mostlyMoving == MostlyMoving.up) && !walldashed && (velocity.y > 2)) {
				if((Sensors[4].info.collided && InputLeft) || (Sensors[5].info.collided && InputRight)) {
					airControlLockTimer = 16f;
					walldashed = true;
					walldashDir = 0;
					
					if(Sensors[4].info.collided && !Sensors[5].info.collided) {
						walldashDir = 1;
						lookingRight = true;
					}

					if(Sensors[5].info.collided && !Sensors[4].info.collided) {
						walldashDir = -1;
						lookingRight = false;
					}
					
					drpspd = 0;
					
					velocity.y = (velocity.y) + 9f;
				
					walldashKickDelay = 3;
				}
			}
			
			if(walldashKickDelay != 0) {
				walldashKickDelay -= 1 * stepDelta;
				if(walldashKickDelay < 0) {
					walldashKickDelay = 0;
				}
			}
			
			if(walldashed && !walldashKicked) {
				if(walldashKickDelay == 0) {
					velocity.x = (3 * walldashDir);
					velocity.y = (velocity.y) - 5f;
					walldashKicked = true;
				}
			}
			
			movingForwardsInAir = (lookingAxis == Mathf.Sign(velocity.x)) || (velocity.x == 0);
			
			// Check for turning Super.
			
			// Update X Speed based on directional input.
			UpdateAirInput();
			
			// Apply air drag.
			if (velocity.y > 0 && velocity.y < 4) {
				float dragFactor = (velocity.x / 0.125f) / 256f;
				velocity.x -= dragFactor * stepDelta;
			}
			
			skidding = false;
			
			// Move the Player object
			// 		-Updates X Position and Y Position based on X Speed and Y Speed.
			UpdatePosition();
			
			// Apply gravity.
			velocity.y -= movementSettings.Gravity * stepDelta;
			// Vertical downwords speed limit.
			
			if(velocity.y < -GlobalSpeedCap) velocity.y = -GlobalSpeedCap;
			
			// Check underwater for reduced gravity.
			
			groundAngle = 0;
			
			// All collision checks occurs here.
			UpdateGroundMode();			
			DoHorizontalCollisions();
			DoVerticalCollisions();
		}
		
		void UpdatePosition()
		{
			position += velocity * stepDelta / 16f;	
		}
		
		void MovementApplier()
		{	
			Vector2 platformMovement = new Vector2(movingPlatformVelocity.x, movingPlatformVelocity.y);
			position += new Vector2(movingPlatformVelocity.x, movingPlatformVelocity.y * PhysicsManager.Instance.subSteps);
			movingPlatformVelocity = Vector2.zero;
			
			// Update the players actual position.
			transform.position = Utilis.Vector2RoundToPixel(new Vector2(position.x + 1/32f, position.y + 1/32f), 16f);
			
			Vector2 spriteHolderOffset = Vector2.zero;
			Vector2 spritePos = (Vector2)transform.position + spriteHolderOffset;
			spriteHolder.transform.position = spritePos;
			float spriteUseAngle = 0f;
			
			spriteUseAngle = Mathf.Round(groundAngle/90f)*90f;
			
			if(groundMode == GroundMode.floor) spriteUseAngle = 0;
			if(groundMode == GroundMode.rightWall) spriteUseAngle = 90;
			if(groundMode == GroundMode.ceiling) spriteUseAngle = 180;
			if(groundMode == GroundMode.leftWall) spriteUseAngle = -90;
			
			float spriteAngle = spriteUseAngle;
			spriteHolder.transform.rotation = Quaternion.AngleAxis(spriteAngle, Vector3.forward);
			
			hitboxRect.transform.localScale = new Vector3(sizeRead.x,sizeRead.y,1f);
		}
		
		void UpdateGroundMode()
		{
			float leftGroundAngle = 0f;
			float rightGroundAngle = 0f;
			
			float maxChangeDelta = 80f;
			
			float leftDeltaAngle = Mathf.Abs(Mathf.DeltaAngle(groundAngle, Sensors[6].info.angle));
			float rightDeltaAngle = Mathf.Abs(Mathf.DeltaAngle(groundAngle, Sensors[7].info.angle));

			if(Sensors[6].info.collided) {
				if(leftDeltaAngle <= maxChangeDelta) {
					leftGroundAngle = Sensors[6].info.angle;
				}
			}
			
			if(Sensors[7].info.collided) {
				if(rightDeltaAngle <= maxChangeDelta) {
					rightGroundAngle = Sensors[7].info.angle;
				}
			}
			
			if((Sensors[6].info.collided || Sensors[7].info.collided) && (groundSpeed != 0)) {
				if(groundSpeed < 0) {
					if(Sensors[6].info.collided) {
						if(leftDeltaAngle <= maxChangeDelta) {
							UpdateGroundModeFromAngle(leftGroundAngle);
						}
					}
					else {
						if(rightDeltaAngle <= maxChangeDelta) {
							UpdateGroundModeFromAngle(rightGroundAngle);
						}
					}
					
					if(Sensors[7].info.collided) {
						UpdateGroundModeFromAngle(groundAngle);	
					}
				}
				else {
					if(Sensors[7].info.collided) {
						if(rightDeltaAngle <= maxChangeDelta) {
							UpdateGroundModeFromAngle(rightGroundAngle);	
						}
					}
					else {
						if(leftDeltaAngle <= maxChangeDelta) {
							UpdateGroundModeFromAngle(leftGroundAngle);
						}
					}
					
					if(Sensors[6].info.collided) {
						UpdateGroundModeFromAngle(groundAngle);	
					}
				}		
			}
			else if(!grounded) {
				UpdateGroundModeFromAngle(groundAngle);
			}
		}
		
		void ShiftGroundModeLeft()
		{	
			switch (groundMode) {
				case GroundMode.floor:
					groundMode = GroundMode.leftWall;
					break;
				case GroundMode.leftWall:
					groundMode = GroundMode.ceiling;
					break;
				case GroundMode.ceiling:
					groundMode = GroundMode.rightWall;
					break;
				case GroundMode.rightWall:
					groundMode = GroundMode.floor;
					break;
				default:
					groundMode = GroundMode.floor;
					break;
			}
		}
		
		void ShiftGroundModeRight()
		{
			switch (groundMode) {
				case GroundMode.floor:
					groundMode = GroundMode.rightWall;
					break;
				case GroundMode.rightWall:
					groundMode = GroundMode.ceiling;
					break;
				case GroundMode.ceiling:
					groundMode = GroundMode.leftWall;
					break;
				case GroundMode.leftWall:
					groundMode = GroundMode.floor;
					break;
				default:
					break;
			}
		}
		
		void UpdateGroundModeFromAngle(float angle, bool flip = false)
		{
			if(grounded) {
				if(angle == 45) {
					if(groundSpeed < 0) groundMode = !flip ? GroundMode.floor     : GroundMode.ceiling;
					else                groundMode = !flip ? GroundMode.rightWall : GroundMode.leftWall;
				}
				
				if(angle == 135) {
					if(groundSpeed < 0) groundMode = !flip ? GroundMode.rightWall : GroundMode.leftWall;
					else                groundMode = !flip ? GroundMode.ceiling   : GroundMode.floor;
				}
				
				if(angle == -135) {
					if(groundSpeed < 0) groundMode = !flip ? GroundMode.ceiling  : GroundMode.floor;
					else                groundMode = !flip ? GroundMode.leftWall : GroundMode.rightWall;
				}
				
				if(angle == -45) {
					if(groundSpeed < 0) groundMode = !flip ? GroundMode.leftWall : GroundMode.rightWall;
					else                groundMode = !flip ? GroundMode.floor    : GroundMode.ceiling;
				}
				
				if(Utilis.inRange(angle, -45, 45, false))
					groundMode = !flip ? GroundMode.floor : GroundMode.ceiling;
					
				if(Utilis.inRange(angle, 45, 135, false)) 
					groundMode = !flip ? GroundMode.rightWall : GroundMode.leftWall;
					
				if(angle > 135f || angle < -135f)
					groundMode = !flip ? GroundMode.ceiling : GroundMode.floor;
					
				if(Utilis.inRange(angle, -135, -45, false)) 
					groundMode = !flip ? GroundMode.leftWall : GroundMode.rightWall;
			}
			else {
				groundMode = GroundMode.floor;
			}
		}
		void DoHorizontalCollisions()
		{
			UpdateSensors(false);
			
			if(grounded) {
				float horzSpeed = 0;
				switch (groundMode) {
					case GroundMode.floor:
						horzSpeed = velocity.x;
						break;
					case GroundMode.rightWall:
						horzSpeed = velocity.y;
						break;
					case GroundMode.ceiling:
						horzSpeed = -velocity.x;
						break;
					case GroundMode.leftWall:
						horzSpeed = -velocity.y;
						break;
					default : break;
				}

				if(Sensors[4].info.collided && Sensors[4].info.flagged) {
					if(groundSpeed < 0) {
						position = Sensors[4].info.point + groundModeRight * pushRadius - wallSensorOffset + (groundModeRight * horzSpeed * stepDelta / 16f);
						groundSpeed = 0;
					}
				}
				
				if(Sensors[5].info.collided && Sensors[5].info.flagged) {
					if(groundSpeed > 0) {
						position = Sensors[5].info.point + groundModeRight * -pushRadius - wallSensorOffset + (groundModeRight * horzSpeed * stepDelta / 16f);
						groundSpeed = 0;
					}
				}
			}
			else {
				if(Sensors[4].info.collided && Sensors[4].info.flagged) {
					if(velocity.x < 0) {
						velocity.x = 0;
						position.x = Sensors[4].info.point.x + pushRadius;
					}
				}
				
				if(Sensors[5].info.collided && Sensors[5].info.flagged) {
					if(velocity.x > 0) {
						velocity.x = 0;
						position.x = Sensors[5].info.point.x - pushRadius;
					}
				}
			}	
		}
		
		void DoVerticalCollisions()
		{
			UpdateSensors(true);
			
			if(grounded) {
				if(!(Sensors[0].info.collided || Sensors[1].info.collided)) {
					grounded = false;
				}
				else {
					DownwardsCollision();
				}
			}
			else { // in the air.	
				if((Sensors[0].info.collided || Sensors[1].info.collided)) {
					grounded = true;
					DownwardsCollision();
					OnGroundLanding();
				}
				else if(!oldGrounded) {
					UpwardsCollision();
				}
			}
			UpdateGroundMode();
		}
		
		void DownwardsCollision()
		{
			if(Sensors[0].info.collided || Sensors[1].info.collided) {
				Sensor winningSensorDown = GetWinningSensor(Sensors[8], GetWinningSensor(Sensors[0], Sensors[1]));
				bool winningSensorAngle = false;

				if(Sensors[0].info.collided && Sensors[1].info.collided) {
					if(Mathf.Abs(Sensors[0].info.distance - Sensors[1].info.distance) <= (1/16f)) {
						UpdateGroundAngle(winningSensorDown);
						winningSensorGroundAngle = groundAngle;
						UpdateGroundAngle(Sensors[8]);
					}
					else { 
						winningSensorAngle = true;
					}
				}
				else {
					winningSensorAngle = true;
				}
						
				if(winningSensorAngle) {
					UpdateGroundAngle(winningSensorDown);
					winningSensorGroundAngle = groundAngle;
				}
				
				Vector2 repositioning = GetSensorRepositioning(winningSensorDown);
				
				float stick = 0;
				
				if(grounded) {
					stick = stickRadius;
				}
				else {
					stick = 0;
				}
				
				position = winningSensorDown.info.point + repositioning + (groundModeUp * (-stick)) ; // Adds stick radius to account for the longer ray length with stick radius.
			}
		}
		
		void UpwardsCollision()
		{
			if(Sensors[2].info.collided || Sensors[3].info.collided) {
				Sensor winningSensorUp = GetWinningSensor(Sensors[2], Sensors[3]);
				
				UpdateGroundAngle(winningSensorUp);
				
				Vector2 repositioning = GetSensorRepositioning(winningSensorUp);
				position = winningSensorUp.info.point + repositioning; // Adds stick radius to account for the longer ray length with stick radius.
				
				OnGroundLanding();
			}
		}
		
		void UpdateGroundAngle(Sensor sensor)
		{
			if(sensor.info.flagged) {
				if(SensorUtilis.GetSensorDirection(sensor.dir) == Direction.down) {
					groundAngle = 0;
				}

				if(SensorUtilis.GetSensorDirection(sensor.dir) == Direction.right) {
					groundAngle = 90;
				}

				if(SensorUtilis.GetSensorDirection(sensor.dir) == Direction.up) {
					groundAngle = 180;
				}

				if(SensorUtilis.GetSensorDirection(sensor.dir) == Direction.left) {
					groundAngle = -90;
				}
			}
			else {
				groundAngle = Utilis.WrapAngleFromNegative180To180(sensor.info.angle);
			}
		}

		Sensor GetWinningSensor(Sensor sensor1, Sensor sensor2)
		{
			Sensor winningSensor;
			
			// If only one sensor hits it will make that the winning sensor, if both hit then the sensor closest to the player is chosen.
			if(sensor1.info.collided && sensor2.info.collided) {
				if(sensor1.info.distance != sensor2.info.distance) {
					winningSensor = sensor1.info.distance < sensor2.info.distance ? sensor1 : sensor2;
				}
				else {
					if( !((sensor1.info.hitObject == null) || !((sensor1.info.hitObject != null) && sensor1.info.hitObject.isMovingPlatform)) &&
						 ((sensor2.info.hitObject == null) || !((sensor2.info.hitObject != null) && sensor2.info.hitObject.isMovingPlatform))
					) return sensor2;
					
					if(  ((sensor1.info.hitObject == null) || !((sensor1.info.hitObject != null) && sensor1.info.hitObject.isMovingPlatform)) &&
						!((sensor2.info.hitObject == null) || !((sensor2.info.hitObject != null) && sensor2.info.hitObject.isMovingPlatform))
					) return sensor1;
					
					if(Mathf.Abs(sensor1.info.angle) <= Mathf.Abs(sensor2.info.angle)) {
						return sensor1;
					}
					else {
						return sensor2;
					}
				}
			}
			else {
				winningSensor = sensor1.info.collided && !sensor2.info.collided ? sensor1 : sensor2;
			}
			
			return winningSensor;
		}
		
		Vector2 GetSensorRepositioning(Sensor sensor)
		{
			Vector2 repositioning = Vector2.zero;
			
			repositioning = -sensor.dir - GetSensorStartPositions()[Array.IndexOf(Sensors, sensor)] + position;
			
			return repositioning;
		}
		
		Vector2[] GetSensorStartPositions()
		{
			Vector2[] posArray = new Vector2[Sensors.Length];
			
			Vector2 collisionOffset = Vector2.zero;
			
			Vector2 wallSenseOff = Vector2.zero;
			
			if(grounded) {
				// Move the wall sensors by velocity because wall collision is ran before updating the position when grounded.
				if((winningSensorGroundAngle % 90) == 0) {
					wallSenseOff += (-heightRadius/2) * groundModeUp;
					if(rolling) wallSenseOff += 2.5f/16f * groundModeUp;
				}
				else {
					if(rolling) wallSenseOff += 2.5f/16f * groundModeUp;
				}
				
				if(oldGrounded) {
					if(groundMode == GroundMode.floor || groundMode == GroundMode.ceiling)
						wallSenseOff.x += (velocity.x * stepDelta / 16f);
					if(groundMode == GroundMode.leftWall || groundMode == GroundMode.rightWall)
						wallSenseOff.y += (velocity.y * stepDelta / 16f);
				}
			}
			else {
				if(oldGrounded) {
					wallSenseOff += (velocity * stepDelta / 16f);
				}

				wallSenseOff += (-heightRadius/2) * groundModeUp;
				if(rolling) wallSenseOff += 2.5f/16f * groundModeUp;
			}
			wallSensorOffset = new Vector2(wallSenseOff.x, wallSenseOff.y);
			// A.
			posArray[0] = collisionOffset + position + groundModeRight * -widthRadius;
			// B.
			posArray[1] = collisionOffset + position + groundModeRight * widthRadius;
			// AB.
			posArray[8] = collisionOffset + position;
			// C.
			posArray[2] = collisionOffset + position + groundModeRight * -widthRadius;
			// D.
			posArray[3] = collisionOffset + position + groundModeRight * widthRadius;
			// E.
			posArray[4] = collisionOffset + position + wallSensorOffset;
			// F.
			posArray[5] = collisionOffset + position + wallSensorOffset;
			// G.
			posArray[6] = collisionOffset + position + new Vector2(-(groundModeRight * -widthRadius).y,  (groundModeRight * -widthRadius).x);
			// H.
			posArray[7] = collisionOffset + position + new Vector2( (groundModeRight *  widthRadius).y, -(groundModeRight *  widthRadius).x);
			
			return posArray;
		}
		
		bool[] UpdateActiveSensors()
		{
			bool[] activeSensors = new bool[Sensors.Length];
			if(Grounded) {
				activeSensors[0] = true;
				activeSensors[1] = true;
				activeSensors[8] = true;

				if(groundSpeed < 0) {
					activeSensors[4] = true;
				}
				if(groundSpeed > 0) {
					activeSensors[5] = true;
				}
			}
			else
			{
				if(velocity.x <= 0) {
					activeSensors[4] = true;
				}

				if(velocity.x >= 0) {
					activeSensors[5] = true;
				}
				
				if(velocity.x > 0) {
					activeSensors[0] = true;
					activeSensors[1] = true;
					activeSensors[8] = true;
					activeSensors[2] = true;
					activeSensors[3] = true;
				}
				
				switch (mostlyMoving) {
					case MostlyMoving.right:
						activeSensors[0] = true;
						activeSensors[1] = true;
						activeSensors[8] = true;
						activeSensors[2] = true;
						activeSensors[3] = true;
						break;
					case MostlyMoving.left:
						activeSensors[0] = true;
						activeSensors[1] = true;
						activeSensors[8] = true;
						activeSensors[2] = true;
						activeSensors[3] = true;
						break;
					case MostlyMoving.up:
						activeSensors[0] = true;
						activeSensors[1] = true;
						activeSensors[8] = true;
						activeSensors[2] = true;
						activeSensors[3] = true;
						break;
					case MostlyMoving.down:
						activeSensors[0] = true;
						activeSensors[1] = true;
						activeSensors[8] = true;
						break;
					default : break;		
				}
			}

			activeSensors[6] = true;
			activeSensors[7] = true;
			
			if(inTunnel) {
				activeSensors[4] = false;
				activeSensors[5] = false;
			}
				
			return activeSensors;
		}
		
		void UpdateSensors(bool vertical)
		{
			Vector2[] SensorPositions = GetSensorStartPositions();
			
			bool[] activeSensors = UpdateActiveSensors();
			
			stickRadius = 0;
			
			if(grounded) {
				stickRadius = Mathf.Min(Mathf.Abs(velocity.x + ((movingPlatformVelocity.magnitude / stepDelta) * 16f)) + 4f, 10f) / 16f;
			}
			else {
				if(mostlyMoving == MostlyMoving.down) {
					stickRadius = (-velocity.y * stepDelta) / 16f;
				}
			}
			
			if(vertical) {
				// Sensor A.
				Sensors[0] = new Sensor(SensorPositions[0], groundModeUp * (-heightRadius), Color.green, this, stickRadius, activeSensors[0], false, PhysicsManager.Instance.groundMask | collisionLayerMask);
				// Sensor B.
				Sensors[1] = new Sensor(SensorPositions[1], groundModeUp * (-heightRadius), Color.cyan, this, stickRadius, activeSensors[1], false, PhysicsManager.Instance.groundMask | collisionLayerMask);
				// Sensor AB.
				Sensors[8] = new Sensor(SensorPositions[8], groundModeUp * (-heightRadius), Color.cyan, this, stickRadius, activeSensors[8], false, PhysicsManager.Instance.groundMask | collisionLayerMask);

				// Sensor C.
				Sensors[2] = new Sensor(SensorPositions[2], groundModeUp * (heightRadius), Color.blue, this, 0, activeSensors[2], false, PhysicsManager.Instance.groundMask | collisionLayerMask);
				// Sensor D.
				Sensors[3] = new Sensor(SensorPositions[3], groundModeUp * (heightRadius), Color.yellow, this, 0, activeSensors[3], false, PhysicsManager.Instance.groundMask | collisionLayerMask);
				
				// Sensor G.
				Sensors[6] = new Sensor(SensorPositions[6], groundModeRight * -heightRadius, Color.gray, this, 0, activeSensors[6], false, PhysicsManager.Instance.groundMask | jumpThroughMask | collisionLayerMask);
				// Sensor H.
				Sensors[7] = new Sensor(SensorPositions[7], groundModeRight * heightRadius, Color.gray, this, 0, activeSensors[7], false, PhysicsManager.Instance.groundMask | jumpThroughMask | collisionLayerMask);
			}
			else {
				// Sensor E.
				Sensors[4] = new Sensor(SensorPositions[4], groundModeRight * -pushRadius, Color.magenta, this, 0, activeSensors[4], false, PhysicsManager.Instance.groundMask | collisionLayerMask);
				// Sensor F.
				Sensors[5] = new Sensor(SensorPositions[5], groundModeRight * pushRadius, Color.red, this, 0, activeSensors[5], false, PhysicsManager.Instance.groundMask | collisionLayerMask);
			}
			
			if(vertical) {
				Sensor[] jumpThroughSensors = new Sensor[2];
			
				jumpThroughSensors[0] = new Sensor(SensorPositions[0], groundModeUp * (-heightRadius), Color.green, this, stickRadius, activeSensors[0], false, jumpThroughMask);
				jumpThroughSensors[1] = new Sensor(SensorPositions[1], groundModeUp * (-heightRadius), Color.cyan, this, stickRadius, activeSensors[1], false, jumpThroughMask);
				
				for(int i = 0; i < jumpThroughSensors.Length; i++) {
					Sensor jumpThroughSensor = jumpThroughSensors[i];
					
					if(jumpThroughSensor.info.collided) {
						SonicObject obj = jumpThroughSensor.info.hitObject;
						if((obj != null) && obj.isJumpThrough) {
							bool isCol = false;
							
							if(jumpThroughSensor.info.distance >= (heightRadius * 0.1f)) {
								if(grounded) {
									isCol = true;
								}
								else {
									if(velocity.y <= 0)
										isCol = true;
								}
							}

							if(isCol) {
								Sensors[i] = jumpThroughSensor;
							}
						}
					}
				}
			}
			
		}
		
		public bool CheckSensor(Sensor sensor, float extraLength)
		{
			Sensor sensorTemp = Sensor.TestCheckDupe(sensor, this, extraLength);
			
			return sensorTemp.info.collided;
		}
		
		void GroundInputAndFriction()
		{
			bool skidAnims = (
				playerSprite.characterAnimator.currentAnimation == "skidTurn" || 
				playerSprite.characterAnimator.currentAnimation == "skidLoop" ||
				playerSprite.characterAnimator.currentAnimation == "skidStart"
			);
			
			if(skidding) {
				if(Mathf.Abs(groundSpeed) <= 0.1f) {
					skidding = false;
				}

				if(InputRight && !InputLeft && groundSpeed > 0) {
					skidding = false;
				}

				if(InputLeft && !InputRight && groundSpeed < 0) {
					skidding = false;
				}

				if(controlLockTimer != 0) {
					skidding = false;
				}
			}
			else if(!skidAnims) {
				if(groundSpeed < 0 && lookingRight) {
					if(!InputRight && InputLeft) {
						lookingRight = false;
					}
				}
				
				if(groundSpeed > 0 && !lookingRight) {
					if(InputRight && !InputLeft) {
						lookingRight = true;
					}
				}
			}
			
			float minSkidSpeed = 2f;
			
			if(controlLockTimer == 0) {
				if(InputLeft && !InputRight) {
					if(groundSpeed > 0) { // if moving to the right
						if(!rolling) {
							groundSpeed -= movementSettings.Deceleration * stepDelta; // decelerate
							
							if(skidding == false && lookingRight) {
								if(groundSpeed >= minSkidSpeed && Mathf.Abs(groundAngle) < 22.5f) {
									skidding = true;
									SoundManager.Instance.Skid();
								}
							}
						}
						else {
							groundSpeed -= movementSettings.RollingDeceleration * stepDelta; // decelerate
						}
						if(groundSpeed <= 0)
							groundSpeed = -0.5f; // emulate deceleration quirk
					}
					else {
						if(groundSpeed > -movementSettings.GroundTopSpeed) { //if moving to the left
							if(!skidAnims)
								lookingRight = false;
							if(!rolling)
								groundSpeed -= movementSettings.GroundAcceleration * stepDelta; // accelerate
							if(groundSpeed <= -movementSettings.GroundTopSpeed) {
								groundSpeed = -movementSettings.GroundTopSpeed; // impose top speed limit
							}
						}
						
						if(groundSpeed == -movementSettings.GroundTopSpeed) {
							if(!skidAnims)
								lookingRight = false;
						}
					}
				}
				
				if(InputRight && !InputLeft) {
					if(groundSpeed < 0) { // if moving to the left	
						if(!rolling) {
							groundSpeed += movementSettings.Deceleration * stepDelta; // decelerate
							
							if(skidding == false && !lookingRight) {
								if(groundSpeed <= -minSkidSpeed && Mathf.Abs(groundAngle) < 22.5f) {
									skidding = true;
									SoundManager.Instance.Skid();
								}	
							}
						}
						else {
							groundSpeed += movementSettings.RollingDeceleration * stepDelta; // decelerate
						}

						if(groundSpeed >= 0)
							groundSpeed = 0.5f * stepDelta; // emulate deceleration quirk
					}
					else {
						if(groundSpeed < movementSettings.GroundTopSpeed) {// if moving to the right
							if(!skidAnims)
								lookingRight = true;
							if(!rolling)
								groundSpeed += movementSettings.GroundAcceleration * stepDelta; // accelerate
							if(groundSpeed >= movementSettings.GroundTopSpeed) {
								groundSpeed = movementSettings.GroundTopSpeed; // impose top speed limit
							}
						}
						
						if(groundSpeed == movementSettings.GroundTopSpeed) {
							if(!skidAnims)
								lookingRight = true;
						}
					}	
				}	
			}
			
			if(!rolling) {
				if((!InputLeft && !InputRight) || (controlLockTimer > 0)) {
					groundSpeed -= Mathf.Min(Mathf.Abs(groundSpeed), movementSettings.Friction) * Mathf.Sign(groundSpeed) * stepDelta;
					if(Mathf.Abs(groundSpeed) <= 0.03f * stepDelta) groundSpeed = 0;
				}
			}
			else {
				if((groundSpeed < 0 && !InputRight) || (controlLockTimer > 0)) {
					groundSpeed -= Mathf.Min(Mathf.Abs(groundSpeed), movementSettings.RollingFriction) * Mathf.Sign(groundSpeed) * stepDelta;
					if(Mathf.Abs(groundSpeed) <= 0.03f * stepDelta) groundSpeed = 0;
				}

				if((groundSpeed > 0 && !InputLeft) || (controlLockTimer > 0)) {
					groundSpeed -= Mathf.Min(Mathf.Abs(groundSpeed), movementSettings.RollingFriction) * Mathf.Sign(groundSpeed) * stepDelta;
					if(Mathf.Abs(groundSpeed) <= 0.03f * stepDelta) groundSpeed = 0;
				}
			}
		}
		
		void UpdateAirInput()
		{
			if(airControlLockTimer == 0) {
				if(InputLeft) {
					lookingRight = false;
					if(velocity.x > -movementSettings.GroundTopSpeed) { // Under the max x speed.
						velocity.x -= movementSettings.AirAcceleration * stepDelta; // accelerate
						if(velocity.x <= -movementSettings.GroundTopSpeed) {
							velocity.x = -movementSettings.GroundTopSpeed; // impose top speed limit
						}
					}
				}
				
				if(InputRight) {
					lookingRight = true;
					if(velocity.x < movementSettings.GroundTopSpeed) { // Under the max x speed.
						velocity.x += movementSettings.AirAcceleration * stepDelta; // accelerate
						if(velocity.x >= movementSettings.GroundTopSpeed) {
							velocity.x = movementSettings.GroundTopSpeed; // impose top speed limit
						}
					}
				}
			}
			else {
				airControlLockTimer -= 1 * stepDelta;
				if(airControlLockTimer < 0) {
					airControlLockTimer = 0;
				}
			}
		}
		
		void OnGroundLanding()
		{	
			float oldGrndSpd = groundSpeed;
			
			// Shallow.	
			if(Utilis.inRange(Mathf.Abs(groundAngle), 0f, 22.5f, false, true)) {
				groundSpeed = velocity.x;	
			}
			
			// Slope.
			if(Utilis.inRange(Mathf.Abs(groundAngle), 22.5f, 45f, false, true)) {
				if(mostlyMoving == MostlyMoving.right || mostlyMoving == MostlyMoving.left)
					groundSpeed = velocity.x;		
				else
					groundSpeed = velocity.y * 0.5f * Mathf.Sign(Mathf.Sin(groundAngle * Mathf.Deg2Rad));	
			}
			
			// Steep Slope.
			if(Utilis.inRange(Mathf.Abs(groundAngle), 45f, 90f, false, true)) {
				groundSpeed = velocity.y * Mathf.Sign(Mathf.Sin(groundAngle * Mathf.Deg2Rad));
				Debug.Log("bruah");
			}
			
			// Up Slope.
			if(Utilis.inRange(Mathf.Abs(groundAngle), 90f, 135f, false, true)) {
				if(mostlyMoving == MostlyMoving.up) {
					grounded = true;
					groundSpeed = velocity.y * Mathf.Sign(Mathf.Sin(groundAngle * Mathf.Deg2Rad));
					
					if(Mathf.Sign(groundAngle) == 1) groundMode = GroundMode.rightWall; 
					else							 groundMode = GroundMode.leftWall;
					
					controlLockTimer = 3f;
				}
				else {
					groundAngle = 0;
					grounded = false;
					position.y -= 0.01f;
				}
				
				return;
			}
			
			// Ceiling.
			if(Utilis.inRange(Mathf.Abs(groundAngle), 135, 200f, false, true)) {
				groundAngle = 0;
				grounded = false;
				position.y -= 0.01f;
			}
			
			UpdateGroundMode();
			
			if(!grounded) {
				if(velocity.y > 0) velocity.y = -0.01f;
				groundSpeed = oldGrndSpd;
				
				return;
			}
			
			if(groundMode == GroundMode.floor) {
				SonicObject groundedObj = GetWinningSensor(Sensors[0],Sensors[1]).info.hitObject;
			
				if(groundedObj != null && groundedObj.isMovingPlatform)
				{
					groundSpeed -= groundedObj.platformScript.velocity.x;
				}
			}
			
			if(InputDown && (Mathf.Abs(oldGroundSpeed) >= 0.5f)) {
				if(groundRollSoundCheck) SoundManager.Instance.Roll();
				
				rolling = true;
			}
			else if(!inTunnel) {
				rolling = false;
			}
			
			if(jumped) {
				jumped = false;
			}
		}
		private Vector3Int oldCell;
		public void TriggerHandler()
		{
			float detectorSize = sizeRead.x;
			
			gridPosition = Utilis.Vector2FloorInt(position);
			Rect triggerDetector = new Rect(position.x - detectorSize/2f, position.y - detectorSize/2f, detectorSize, detectorSize);
			Collider2D triggerCenterPoint = Physics2D.OverlapBox((Vector2)position, Vector2.one * detectorSize, 0f, triggerLayer);
			
			if(triggerCenterPoint != null && triggerCenterPoint.gameObject.tag == "triggers" && (gridPosition != gridPositionOld)) {	
				Tilemap tilemap = triggerCenterPoint.GetComponent<Tilemap>();
				GridLayout gridLayout = tilemap.GetComponentInParent<GridLayout>(); 
				
				var cellBounds = new BoundsInt(
					gridLayout.WorldToCell(triggerDetector.position),
					gridLayout.WorldToCell(triggerDetector.size * 2) + new Vector3Int(0, 0, 1));
						
				List<Vector3Int> cellsInBounds = new List<Vector3Int>();	
				Vector3Int closestCell = Vector3Int.zero;
				
				foreach (var cell in cellBounds.allPositionsWithin) {
					TriggerBase tile = tilemap.GetTile(cell) as TriggerBase;
					if(tile != null) {
						cellsInBounds.Add(cell);
					}
				}
				
				if(cellsInBounds.Count > 0) {
					closestCell = cellsInBounds[0];
					for(int i = 0; i < cellsInBounds.Count; i++) {
						var offsetVector = new Vector2(0.5f, 0.5f);
						if(Vector3.Distance((Vector3)cellsInBounds[i] + (Vector3)offsetVector, position) < Vector3.Distance((Vector3)closestCell + (Vector3)offsetVector, position)) {
							closestCell = cellsInBounds[i];
						}
					}
					
					TriggerBase triggerTile = tilemap.GetTile(closestCell) as TriggerBase;
					if(triggerTile != null && (oldCell != closestCell)) {
						triggerTile.Trigger(this);
					}
					oldCell = closestCell;
				}
			}
			else {
				oldCell = Vector3Int.one * 1000;
			}
		}
	}
}