using System;
using UnityEngine;

namespace AncientMemorial.Map {
	[Serializable]
	public class MapObjects {

		public SpriteRenderer[] leftWallSR;
		public SpriteRenderer[] rightWallSR;
		public SpriteRenderer[] floorSR;
		public SpriteRenderer[] ceilSR;
		public SpriteRenderer[] bgSR;
		
		public Transform     leftWallTR;
		public Transform     rightWallTR;
		public Transform     floorTR;
		public Transform     ceilTR;
		public Transform     bgTR;
		public Transform     cornerRU;
		public Transform     cornerLU;
		public Transform     cornerRD;
		public Transform     cornerLD;
		public BoxCollider2D colliderL;
		public BoxCollider2D colliderR;
		public BoxCollider2D colliderU;
		public BoxCollider2D colliderD;

		public void Move(Vector2 MapSize) {
			
			colliderL.size = new Vector2(0.5f, MapSize.y+1);
			foreach (SpriteRenderer sr in leftWallSR) {
				sr.size = new Vector2(1, MapSize.y);
			}
			
			colliderR.size = new Vector2(0.5f, MapSize.y+1);
			foreach (SpriteRenderer sr in rightWallSR) {
				sr.size = new Vector2(1, MapSize.y);
			}
			
			colliderU.size = new Vector2(MapSize.x+1, 1);
			foreach (SpriteRenderer sr in floorSR) {
				sr.size = new Vector2(MapSize.x, 1);
			}
			
			colliderD.size = new Vector2(MapSize.x+1, 1);
			foreach (SpriteRenderer sr in ceilSR) {
				sr.size = new Vector2(MapSize.x, 1);
			}

			foreach (SpriteRenderer sr in bgSR) {
				sr.size = MapSize + Vector2.one;
			}
			
			float leftWallX  = -MapSize.x /2 - 0.5f;
			float leftWallY  =  MapSize.y /2 + 0.5f;
			float rightWallX =  MapSize.x /2 + 0.5f;
			float rightWallY =  MapSize.y /2 + 0.5f;
			float ceilY      =  MapSize.y    + 1;
			
			leftWallTR.position  = new Vector2(leftWallX,  leftWallY);
			rightWallTR.position = new Vector2(rightWallX, rightWallY);
			floorTR.position     = new Vector2(0,          0);
			ceilTR.position      = new Vector2(0,          ceilY);

			bgTR.position = new Vector2(0, ceilY / 2);
			
			cornerRU.position = new Vector2(rightWallX, ceilY);
			cornerLU.position = new Vector2(leftWallX,  ceilY);
			cornerRD.position = new Vector2(rightWallX, 0);
			cornerLD.position = new Vector2(leftWallX,  0);
		}
	}
}