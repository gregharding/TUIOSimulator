using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using TUIOSimulator;

public class SettingsScreen : MonoBehaviour {

	public InputField ipAddressInput;
	public InputField portInput;
	public Text localIP;
	public InputField listenPortInput;

	private string ipAddress;
	private int port;
	private int listenPort;

	private ServerHistory serverHistory;
	private int historyIndex;

	public Color errorColour;
	private Color normalColour;


	protected void Awake() {
		Application.targetFrameRate = 60;

		normalColour = ipAddressInput.textComponent.color;

		(ipAddressInput.placeholder as Text).text = Settings.defaultIPAddress;
		(portInput.placeholder as Text).text = Settings.defaultPort.ToString();
		(listenPortInput.placeholder as Text).text = Settings.defaultListenPort.ToString();

		localIP.text = string.Join(", ", IPHelper.localIPAddresses.ToArray());

		// history
		serverHistory = ServerHistory.Load();
		if (serverHistory.hasHistory) {
			historyIndex = serverHistory.count - 1;
			ShowHistory(historyIndex);
		}
	}

	private void Go() {
		Settings.Use(ipAddress, port, listenPort);

		serverHistory.Add(ipAddress, port, listenPort);
		serverHistory.Save();

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

	public void OnPrevButtonPressed() {
		ShowPrevHistory();
	}

	public void OnNextButtonPressed() {
		ShowNextHistory();
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
	// input, validation
	//

	private void ShowPrevHistory() {
		if (!serverHistory.hasHistory) return;

		historyIndex = (historyIndex-1 + serverHistory.count) % serverHistory.count;
		ShowHistory(historyIndex);
	}

	private void ShowNextHistory() {
		if (!serverHistory.hasHistory) return;

		historyIndex = (historyIndex+1) % serverHistory.count;
		ShowHistory(historyIndex);
	}

	private void ShowHistory(int index) {
		if (!serverHistory.hasHistory) return;

		var entry = (index >= 0) ? serverHistory.servers[index] : serverHistory.mostRecent;
		Show(entry.ipAddress, entry.port, entry.listenPort);
	}

	private void Show(string ipAddress, int port, int listenPort) {
		// show input, leave fields blank if they match the defaults
		ipAddressInput.text = (ipAddress != Settings.defaultIPAddress) ? ipAddress : "" ;
		portInput.text = (port != Settings.defaultPort) ? port.ToString() : "" ;
		listenPortInput.text = (listenPort != Settings.defaultListenPort) ? listenPort.ToString() : "";

		ValidateInput();
	}

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


	//
	// history
	//

	[Serializable]
	public class ServerHistory {

		public const string prefKey = "ServerHistory";
		public const int maxHistory = 10;

		public List<ServerHistoryEntry> servers = new List<ServerHistoryEntry>();

		public int count { get { return servers.Count; } }
		public bool hasHistory { get { return servers.Count > 0; } }
		public ServerHistoryEntry mostRecent { get { return hasHistory ? servers[servers.Count-1] : null; } }


		public static ServerHistory Load() {
			// load history data from player preferences
			var json = PlayerPrefs.GetString(prefKey, "");

			if (!string.IsNullOrEmpty(json)) {
				return JsonUtility.FromJson<ServerHistory>(json);
			} else {
				return new ServerHistory();	
			}		
		}

		public void Add(string ipAddress, int port, int listenPort) {
			// don't add  duplicate of most recent entry or default values to an empty history
			if (hasHistory) {
				var entry = mostRecent;
				if (ipAddress == entry.ipAddress && port == entry.port && listenPort == entry.listenPort)
					return;

			} else if ((ipAddress == "" || ipAddress == Settings.defaultIPAddress) && (port == 0 || port == Settings.defaultPort) && (listenPort == 0 || listenPort == Settings.defaultListenPort)) {
				return;
			}

			servers.Add(new ServerHistoryEntry { ipAddress = ipAddress, port = port, listenPort = listenPort } );			
		}

		public void Save() {
			// cull
			if (servers.Count > maxHistory) {
				servers.RemoveRange(0, servers.Count - maxHistory);
			}

			// save
			var json = JsonUtility.ToJson(this);
			PlayerPrefs.SetString(prefKey, json);
		}
	}


	[Serializable]
	public class ServerHistoryEntry {

		public string ipAddress;
		public int port;
		public int listenPort;
	}
}
