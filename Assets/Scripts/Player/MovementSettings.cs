using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SonicFramework
{
	[CreateAssetMenu(menuName = "sonicFramework/Movement Settings", fileName = "MovementSettings")]
	public class MovementSettings : ScriptableObject
	{
		[Header("Ground Movement")]
		[SerializeField] private float groundAcceleration = 0.046875f;
		[SerializeField] private float groundTopSpeed = 6f;
		[SerializeField] private float friction = 0.046875f;
		[SerializeField] private float rollingFriction = 0.0234375f;
		[SerializeField] private float deceleration = 0.5f;
		[SerializeField] private float rollingDeceleration = 0.125f;

		[Header("Air Movement")]
		[SerializeField] private float airAcceleration = 0.09375f;
		[SerializeField] private float jumpVelocity = 6.5f;
		[SerializeField] private float gravity = 0.21875f;
		[SerializeField] private float topYSpeed = 16f;
		[SerializeField] private float airDrag = 0.125f;
		[SerializeField] private float jumpRelease = 4f;
		
		public float GroundAcceleration { get { return groundAcceleration; } }
		public float GroundTopSpeed { get { return groundTopSpeed; } }
		public float Friction { get { return friction; } }
		public float RollingFriction { get { return rollingFriction; } }
		public float Deceleration { get { return deceleration; } }
		public float RollingDeceleration { get { return rollingDeceleration; } }
		public float AirAcceleration { get { return airAcceleration; } }
		public float JumpVelocity { get { return jumpVelocity; } }
		public float Gravity { get { return gravity; } }
		public float TopYSpeed { get { return topYSpeed; } }
		public float AirDrag { get { return airDrag; } }
		public float JumpRelease { get { return jumpRelease; } }
	}	
}

