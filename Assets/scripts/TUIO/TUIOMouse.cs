using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TUIOSimulator.Entities;

namespace TUIOSimulator {
	
	public class TUIOMouse : MonoBehaviour {

		//public Surface surface { get; protected set; }
		public TUIOCursor tuioCursor { get; protected set; }

		protected Vector2 lastPosition;
		protected Vector2WMA velocity;


		protected void Awake() {
			//surface = GetComponentInParent<Surface>();

			velocity = new Vector2WMA(5);
		}

		//protected void OnEnable() {}

		protected void Update() {
			if (Input.GetMouseButtonDown(0)) CreateCursor();
			else if (Input.GetMouseButtonUp(0)) DestroyCursor();
			else if (tuioCursor != null) UpdateCursor(Time.deltaTime);
		}

		protected void OnDisable() {
			DestroyCursor();
		}

		protected void CreateCursor() {
			if (tuioCursor != null)
				DestroyCursor();

			// raycast against surface
			//Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			//RaycastHit2D hit = Physics2D.GetRayIntersection(ray);
			//if (hit == null || hit.collider != surface.boxCollider2D) return;

			Vector2 pos = Vector2.Scale(Input.mousePosition, Main.instance.invScreenDimensions);
			tuioCursor = new TUIOCursor(Main.instance.tuioTransmitter.NextSessionId(), pos.x, pos.y, 0f, 0f, 0f);
			lastPosition = pos;

			Main.instance.tuioTransmitter.Add(tuioCursor);
		}

		protected void UpdateCursor(float dt) {
			if (Mathf.Approximately(dt, 0f)) return;
			float invDt = 1 / dt;

			Vector2 pos = Vector2.Scale(Input.mousePosition, Main.instance.invScreenDimensions);
			Vector2 vel = (pos - lastPosition) * invDt;

			Vector2 previousVelocity = velocity.value;
			velocity.AddSample(vel);
			Vector2 currentVelocity = velocity.value;
			float acceleration = currentVelocity.magnitude - previousVelocity.magnitude;

			tuioCursor.Update(pos.x, pos.y, currentVelocity.x, currentVelocity.y, acceleration);

			lastPosition = pos;
		}

		protected void DestroyCursor() {
			if (tuioCursor == null) return;

			Main.instance.tuioTransmitter.Remove(tuioCursor);
			tuioCursor = null;
			velocity.Clear();
		}
	}
}