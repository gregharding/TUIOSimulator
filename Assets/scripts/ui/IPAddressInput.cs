using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class IPAddressInput : UIBehaviour {

	protected InputField inputField;


	override protected void Awake() {
		inputField = GetComponent<InputField>();
		inputField.characterValidation = InputField.CharacterValidation.None;
		inputField.onValidateInput = OnValidateInput;
	}

	private char OnValidateInput(string text, int charIndex, char addedChar) {
		//Debug.LogFormat("\"{0}\" {1} '{2}'", text, charIndex, addedChar);

		// adjust working text to remove any current selection (allows overwrites)
		if (inputField.selectionAnchorPosition != inputField.selectionFocusPosition) {
			int start = Mathf.Min(inputField.selectionAnchorPosition, inputField.selectionFocusPosition);
			int end = Mathf.Max(inputField.selectionAnchorPosition, inputField.selectionFocusPosition);
			text = text.Remove(start, end-start);
			charIndex = start;
		}

		// determine roughly what's allowed at current position
		// xxx.xxx.xxx.xxx

		bool atStart = (charIndex == 0);
		bool isPreviousDot = atStart ? false : text[charIndex-1] == '.';
		int dotCount = text.Split('.').Length - 1;
		int previousDigitCount = 0;
		for (int i=charIndex-1; i>=(charIndex-1-3) && i>-1; i--) {
			if (text[i] >= '0' && text[i] <= '9')
				previousDigitCount++;
			else
				break;
		}

		if ((atStart || isPreviousDot) && addedChar >= '0' && addedChar <= '9') return addedChar;
		if (previousDigitCount > 0 && previousDigitCount < 3 && (addedChar >= '0' && addedChar <= '9' || (dotCount < 3 && addedChar == '.'))) return addedChar;
		if (previousDigitCount == 3 && dotCount < 3 && addedChar == '.') return addedChar;

		return (char)0;
	}
}
