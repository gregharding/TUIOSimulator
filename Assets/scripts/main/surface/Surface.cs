using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using TUIOSimulator;
using TUIOSimulator.Entities;
using TouchScript;
using TouchScript.InputSources;

public class Surface : MonoBehaviour {

	public TUIOTransmitter tuioTransmitter { get; private set; }

	private Vector2 bottomLeft;
	private Vector2 invSize;

	public SurfaceCursor surfaceCursorPrefab;
	private Dictionary<int, SurfaceCursor> surfaceCursors;

	public SurfaceObject surfaceObjectPrefab;

	public int spawnObjectCount = 8;
	private List<SurfaceObject> surfaceObjects;

	public BoxCollider2D boxCollider2D;

	private List<RaycastResult> raycastResults = new List<RaycastResult>();


	protected void Awake() {
		tuioTransmitter = Main.instance.tuioTransmitter;

		bottomLeft = new Vector2(boxCollider2D.offset.x - 0.5f * boxCollider2D.size.x, boxCollider2D.offset.y - 0.5f * boxCollider2D.size.y);
		invSize = new Vector2(1/boxCollider2D.size.x, 1/boxCollider2D.size.y);
	}

	protected void OnEnable() {
		Init(spawnObjectCount);

		if (TouchManager.Instance != null) {
			TouchManager.Instance.TouchesBegan += OnTouchesBegan;
			TouchManager.Instance.TouchesMoved += OnTouchesMoved;
			TouchManager.Instance.TouchesEnded += OnTouchesEnded;
			TouchManager.Instance.TouchesCancelled += OnTouchesCancelled;
		}
	}

	protected void OnDisable() {
		if (TouchManager.Instance != null) {
			TouchManager.Instance.TouchesBegan -= OnTouchesBegan;
			TouchManager.Instance.TouchesMoved -= OnTouchesMoved;
			TouchManager.Instance.TouchesEnded -= OnTouchesEnded;
			TouchManager.Instance.TouchesCancelled -= OnTouchesCancelled;
		}

		RemoveSurfaceCursors();
	}


	//
	// init surface entities
	//

	private void Init(int spawnObjectCount) {
		surfaceCursors = new Dictionary<int, SurfaceCursor>();

		Vector3 spawnPosition = new Vector3(6f, 4.375f, 0f);
		surfaceObjects = new List<SurfaceObject>(spawnObjectCount);

		for (int i=0; i<spawnObjectCount; i++) {
			SurfaceObject so = Instantiate<SurfaceObject>(surfaceObjectPrefab);
			surfaceObjects.Add(so);

			so.transform.localPosition = spawnPosition;
			so.transform.SetParent(transform, false);

			so.Init(i);
		}

		LayoutSurfaceObjects();
		SortSurfaceObjects();
	}

	private void LayoutSurfaceObjects() {
		Vector3 startPosition = new Vector3(6f, 4.375f, 0f);
		Vector3 offset = new Vector3(1.25f, -1.25f, 0f);
		int rows = 8;

		for (int i=0; i<surfaceObjects.Count; i++) {
			SurfaceObject so = surfaceObjects[i];
			so.transform.localPosition = startPosition + Vector3.Scale(new Vector3(i / rows, i % rows, 1f), offset);
		}
	}

	private void SortSurfaceObjects() {
		for (int i=0; i<surfaceObjects.Count; i++) {
			surfaceObjects[i].spriteRenderer.sortingOrder = i;
		}
	}

	public void ResetSurfaceObjects() {
		for (int i=0; i<surfaceObjects.Count; i++) {
			surfaceObjects[i].RemoveFromSurface();
			surfaceObjects[i].transform.eulerAngles = Vector3.zero;
		}

		LayoutSurfaceObjects();
		SortSurfaceObjects();
	}


	//
	// surface entities
	//

	public int NextSessionId() {
		return tuioTransmitter.NextSessionId();
	}

	public void Add(ISurfaceEntity entity) {
		tuioTransmitter.Add(entity.tuioEntity);
	}

	public void Remove(ISurfaceEntity entity) {
		tuioTransmitter.Remove(entity.tuioEntity);
	}

	//public void RemoveAll() {
	//	tuioTransmitter.RemoveAll();
	//}


	//
	// surface cursors
	//

	private void CreateSurfaceCursor(TouchPoint touch) {
		if (surfaceCursors.ContainsKey(touch.Id)) RemoveSurfaceCursor(touch);

		// only create cursor if not over UI (no normal event system tracking mouse and pointer being injected does not exist for IsPointerOverGameObject check)
		if (IsScreenPositionOverUI(touch.Position)) return;

		// raycast, only create cursor if no other entities are hit
		Ray ray = Camera.main.ScreenPointToRay(touch.Position);
		RaycastHit2D hit = Physics2D.GetRayIntersection(ray);
		if (hit.collider != null && hit.collider != boxCollider2D) return;
			
		SurfaceCursor sc = Instantiate<SurfaceCursor>(surfaceCursorPrefab);

		Vector2 position = Camera.main.ScreenToWorldPoint(touch.Position);
		sc.transform.localPosition = position;
		sc.transform.SetParent(transform, false);

		sc.spriteRenderer.sortingOrder = surfaceObjects.Count; // ie. above surface objects
			
		surfaceCursors[touch.Id] = sc;
	}

	private void UpdateSurfaceCursor(TouchPoint touch) {
		SurfaceCursor sc;
		if (surfaceCursors.TryGetValue(touch.Id, out sc)) {
			Vector2 position = Camera.main.ScreenToWorldPoint(touch.Position);
			sc.transform.localPosition = position;
		}
	}

	private void RemoveSurfaceCursor(TouchPoint touch) {
		SurfaceCursor sc;
		if (surfaceCursors.TryGetValue(touch.Id, out sc)) {
			surfaceCursors.Remove(touch.Id);
			Destroy(sc.gameObject);
		}
	}

	private void RemoveSurfaceCursors() {
		foreach (var keyValue in surfaceCursors) {
			SurfaceCursor sc = keyValue.Value;
			Destroy(sc.gameObject);
		}
	}


	//
	// surface objects
	//

	private void UpdateSurfaceObject(TouchPoint touch) {
		SurfaceObject so = surfaceObjects.Find(s => s.id == (int)(touch.Properties["ObjectId"]));
		if (so == null) return;

		Vector2 position = Camera.main.ScreenToWorldPoint(touch.Position);
		float angle = -(float)(touch.Properties["Angle"]) * Mathf.Rad2Deg;

		so.transform.localPosition = position;
		so.transform.eulerAngles = new Vector3(0f, 0f, angle);
	}


	//
	// local mouse, local touches, remote touches, remote objects
	//

	private void OnTouchesBegan(object sender, TouchEventArgs e) {
		foreach (TouchPoint touch in e.Touches) {
			if (touch.Tags.HasTag(TuioInput.SOURCE_TUIO)) {
				// tuio
				if (touch.Tags.HasTag(Tags.INPUT_TOUCH)) {
					CreateSurfaceCursor(touch);
				} else
				if (touch.Tags.HasTag(Tags.INPUT_OBJECT)) {
					UpdateSurfaceObject(touch);
				}
			} else {
				// normal touch
				CreateSurfaceCursor(touch);
			}
		}
	}

	private void OnTouchesMoved(object sender, TouchEventArgs e) {
		foreach (TouchPoint touch in e.Touches) {
			if (touch.Tags.HasTag(TuioInput.SOURCE_TUIO)) {
				// tuio
				if (touch.Tags.HasTag(Tags.INPUT_TOUCH)) {
					UpdateSurfaceCursor(touch);
				} else if (touch.Tags.HasTag(Tags.INPUT_OBJECT)) {
					UpdateSurfaceObject(touch);
				}
			} else {
				// normal touch
				UpdateSurfaceCursor(touch);
			}
		}
	}

	private void OnTouchesEnded(object sender, TouchEventArgs e) {
		foreach (TouchPoint touch in e.Touches) {
			if (touch.Tags.HasTag(TuioInput.SOURCE_TUIO)) {
				// tuio
				if (touch.Tags.HasTag(Tags.INPUT_TOUCH)) {
					RemoveSurfaceCursor(touch);
				} else if (touch.Tags.HasTag(Tags.INPUT_OBJECT)) {
					UpdateSurfaceObject(touch);
				}
			} else {
				// normal touch
				RemoveSurfaceCursor(touch);
			}
		}
	}

	private void OnTouchesCancelled(object sender, TouchEventArgs e) {
		foreach (TouchPoint touch in e.Touches) {
			if (touch.Tags.HasTag(TuioInput.SOURCE_TUIO)) {
				// tuio
				if (touch.Tags.HasTag(Tags.INPUT_TOUCH)) {
					RemoveSurfaceCursor(touch);
				} else if (touch.Tags.HasTag(Tags.INPUT_OBJECT)) {
					UpdateSurfaceObject(touch);
				}
			} else {
				// normal touch
				RemoveSurfaceCursor(touch);
			}
		}
	}


	//
	// helpers
	//

	public Vector2 GetNormalisedPosition(Transform other) {
		// return normalised position on surface, clamped 0..1
		Vector2 position = (Vector2) transform.InverseTransformPoint(other.position) - bottomLeft;
		position.x = Mathf.Clamp01(position.x * invSize.x);
		position.y = 1f - Mathf.Clamp01(position.y * invSize.y); // nb. tuio top left=0,0, bottom right=1,1
		return position;
	}


	//
	// events
	//

	public void OnSettingsButtonPressed() {
		SceneManager.LoadScene("launch");
	}

	public void OnResetSurfaceObjectsButtonPressed() {
		ResetSurfaceObjects();
	}


	//
	// ui graphic raycaster
	//

	private bool IsScreenPositionOverUI(Vector2 position) {
		PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
		pointerEventData.position = position;
		EventSystem.current.RaycastAll(pointerEventData, raycastResults);
		return (FindFirstRaycast(raycastResults).gameObject != null);
	}

	private RaycastResult FindFirstRaycast(List<RaycastResult> candidates) {
		for (var i = 0; i < candidates.Count; ++i) {
			if (candidates[i].gameObject == null)
				continue;

			return candidates[i];
		}
		return new RaycastResult();
	}
}
