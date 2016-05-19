using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TUIOSimulator;

public class Launch : MonoBehaviour {

	public InputField ipAddressInput;
	public InputField portInput;
	public Text localIP;
	public InputField listenPortInput;

	private string ipAddress;
	private int port;
	private int listenPort;

	public Color errorColour;
	private Color normalColour;


	protected void Awake() {
		Application.targetFrameRate = 60;

		normalColour = ipAddressInput.textComponent.color;

		(ipAddressInput.placeholder as Text).text = Settings.defaultIPAddress;
		(portInput.placeholder as Text).text = Settings.defaultPort.ToString();
		(listenPortInput.placeholder as Text).text = Settings.defaultListenPort.ToString();

		localIP.text = string.Join(", ", IPHelper.localIPAddresses.ToArray());
	}

	private void Go() {
		Settings.Use(ipAddress, port, listenPort, listenPort > 0);
		SceneManager.LoadScene("main");
	}


	//
	// events
	//

	public void OnIpAddressEndEdit() {
		ValidateInput();
	}

	public void OnPortEndEdit() {
		ValidateInput();
	}

	public void OnListenPortEndEdit() {
		ValidateInput();
	}

	public void OnStartButtonPressed() {
		if (ValidateInput()) Go();
	}

	public void OnCreditsButtonPressed() {
		Application.OpenURL("http://www.flightless.co.nz/");
	}

	public void OnProjectButtonPressed() {
		Application.OpenURL("https://github.com/gregharding/TUIOSimulator/");
	}


	//
	// validation
	//

	private bool ValidateInput() {
		ipAddressInput.text = ipAddressInput.text.Trim();
		portInput.text = portInput.text.Trim();
		listenPortInput.text = listenPortInput.text.Trim();

		ipAddress = !string.IsNullOrEmpty(ipAddressInput.text) ? ipAddressInput.text : Settings.defaultIPAddress;
		port = !string.IsNullOrEmpty(portInput.text) ? int.Parse(portInput.text) : Settings.defaultPort;
		listenPort = !string.IsNullOrEmpty(listenPortInput.text) ? int.Parse(listenPortInput.text) : Settings.defaultListenPort;

		// ip
		bool ipAddressError = !Settings.IsValidIpAddress(ipAddress);
		ipAddressInput.textComponent.color = ipAddressError ? errorColour : normalColour;

		// ports
		bool shouldListen = (listenPort > 0);
		bool isIpAddressLocal = (ipAddress == Settings.defaultIPAddress) || IPHelper.localIPAddresses.Contains(ipAddressInput.text);
		bool portsClash = shouldListen && isIpAddressLocal && (port == listenPort);

		bool portError = !Settings.IsValidPort(port) || portsClash;
		bool listenPortError = !Settings.IsValidPort(listenPort, true) || portsClash;

		portInput.textComponent.color = portError ? errorColour : normalColour;
		listenPortInput.textComponent.color = listenPortError ? errorColour : normalColour;

		return !(ipAddressError || portError || listenPortError);
	}
}
