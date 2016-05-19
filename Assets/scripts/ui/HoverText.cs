using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class HoverText : UIBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerUpHandler {

	public Color hoverColour = Color.white;
	private Color originalColour;

	private Text target;


	override protected void Awake() {
		target = GetComponent<Text>();
		originalColour = target.color;
	}

	override protected void OnDisable() {
		UnHover();
	}

	protected void Hover() {
		target.color = hoverColour;
	}

	protected void UnHover() {
		target.color = originalColour;
	}


	//
	// events
	//

	public void OnPointerEnter(PointerEventData eventData) {
		Hover();
	}

	public void OnPointerExit(PointerEventData eventData) {
		UnHover();
	}

	public void OnPointerUp(PointerEventData eventData) {
		UnHover();
	}
}
