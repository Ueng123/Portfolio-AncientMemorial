using UnityEngine;

namespace UengSystem.Objects {
	public class ObjectInitializeData {
		public Vector3         initialPosition;
		public Quaternion      initialRotation;
		public Vector3         initialScale;
		public Sprite          initialSprite;
		public Color           initialColor;
		public bool            initialFlipX;
		public bool            initialFlipY;
		public Vector2         initialSize;
		public RigidbodyType2D initialRigidBodyType;
		public float           initialLinearDamping;
		public float           initialAngularDamping;
		public float           initialGravityScale;

		public Transform      transform;
		public SpriteRenderer spriteRenderer;
		public Rigidbody2D    rigidbody2D;

		public ObjectInitializeData() {
			
		}
	}
}