using UnityEngine;
using UnityEngine.Serialization;
using System.Collections;
using System.Collections.Generic;
using TUIOSimulator.Entities;

public class SurfaceCursor : MonoBehaviour, ISurfaceEntity {

	public Surface surface { get; protected set; }
	public ITUIOEntity tuioEntity { get { return tuioCursor; } }

	public TUIOCursor tuioCursor { get; protected set; }

	public SpriteRenderer spriteRenderer { get; protected set; }
	public Collider2D collider2d { get; protected set; }


	protected void Awake() {
		spriteRenderer = GetComponent<SpriteRenderer>();
		collider2d = GetComponent<Collider2D>();
	}

	protected void Update() {
		//UpdatePosition(); // dev, mouse

		if (surface != null)
			UpdateCursor();
	}

	protected void OnDisable() {
		RemoveFromSurface();
	}


	//
	// input
	//

	/* dev, mouse
	public void UpdatePosition() {
		Vector3 position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
		position.z = 0f;
		transform.position = position;
	}
	*/

	//public void UpdatePosition(Vector2 position) {
	//	transform.position = position;
	//}


	//
	// surface
	//

	public void AddToSurface() {
		if (surface != null)
			RemoveFromSurface();

		surface = GetComponentInParent<Surface>();

		Vector2 normalisedPosition = surface.GetNormalisedPosition(transform);
		tuioCursor = new TUIOCursor(surface.NextSessionId(), normalisedPosition.x, normalisedPosition.y, 0f, 0f, 0f);
		surface.Add(this);
	}

	public void UpdateCursor() {
		Vector2 tuioPosition = surface.GetNormalisedPosition(transform);
		tuioCursor.Update(tuioPosition.x, tuioPosition.y, 0f, 0f, 0f);
	}

	public void RemoveFromSurface() {
		if (surface == null) return;

		surface.Remove(this);
		surface = null;
		tuioCursor = null;
	}


	//
	// triggers
	//

	protected void OnTriggerEnter2D(Collider2D other) {
		AddToSurface();
	}

	protected void OnTriggerExit2D(Collider2D other) {
		RemoveFromSurface();
	}
}
