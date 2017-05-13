using UnityEngine;
using UnityEngine.Serialization;
using System.Collections;
using System.Collections.Generic;
using TUIOSimulator.Entities;
using TouchScript.Gestures;

public class SurfaceObject : MonoBehaviour, ISurfaceEntity {

	public Surface surface { get; protected set; }
	public ITUIOEntity tuioEntity { get { return tuioObject; } }

	public TUIOObject tuioObject { get; protected set; }

	public int id { get { return _id; } }
	[SerializeField]
	[Range(0,7)]
	private int _id;

	public List<Sprite> sprites;
	public List<Color> colors;

	public SpriteRenderer spriteRenderer { get; protected set; }
	public Collider2D collider2d { get; protected set; }

	private PressGesture pressGesture;


	protected void Awake() {
		spriteRenderer = GetComponent<SpriteRenderer>();
		collider2d = GetComponent<Collider2D>();
		pressGesture = GetComponent<PressGesture>();
	}

	protected void OnEnable() {
		Init(_id);
		pressGesture.Pressed += OnPressed;
	}

	protected void Update() {
		if (surface != null)
			UpdateObject();
	}

	protected void OnDisable() {
		pressGesture.Pressed -= OnPressed;
		RemoveFromSurface();
	}


	//
	// meta
	//

	public void Init(int id) {
		_id = id;
		name = "object-" + _id.ToString();

		if (_id < sprites.Count) spriteRenderer.sprite = sprites[_id];
		if (_id < colors.Count) spriteRenderer.color = colors[_id];
	}


	//
	// surface
	//

	public void AddToSurface() {
		if (surface != null)
			RemoveFromSurface();

		surface = GetComponentInParent<Surface>();

		Vector2 normalisedPosition = surface.GetNormalisedPosition(transform);
		float angleRads = (transform.localEulerAngles.z < 0f) ? ((transform.localEulerAngles.z % 360f + 360f) * Mathf.Deg2Rad) : ((transform.localEulerAngles.z % 360f) * Mathf.Deg2Rad) ;
		tuioObject = new TUIOObject(surface.NextSessionId(), _id, normalisedPosition.x, normalisedPosition.y, -angleRads, 0f, 0f, 0f, 0f, 0f);
		surface.Add(this);
		surface.ShowSurfaceObjectOnTop(this);
	}

	public void UpdateObject() {
		Vector2 tuioPosition = surface.GetNormalisedPosition(transform);
		float angleRads = (transform.localEulerAngles.z < 0f) ? ((transform.localEulerAngles.z % 360f + 360f) * Mathf.Deg2Rad) : ((transform.localEulerAngles.z % 360f) * Mathf.Deg2Rad) ;
		tuioObject.Update(tuioPosition.x, tuioPosition.y, -angleRads, 0f, 0f, 0f, 0f, 0f);
	}

	public void RemoveFromSurface() {
		if (surface == null) return;

		surface.Remove(this);
		surface = null;
		tuioObject = null;
	}

	private void OnPressed(object sender, System.EventArgs e) {
		if (surface != null)
			surface.ShowSurfaceObjectOnTop(this);
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
