using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System;
using System.Text;
using TouchScript.InputSources;
using TouchScript.InputSources.InputHandlers;
using Flightless;
using TUIOSimulator;

public class Main : SingletonMonoBehaviour<Main> {

	public Surface surface;
	public TUIOTransmitter tuioTransmitter { get; private set; }

	public StandardInput standardInput;
	public TuioInput tuioInput;

	public Vector2 screenDimensions { get; private set; }
	public Vector2 invScreenDimensions { get; private set; }

	public bool screenDimensionsChanged { get; private set; }

	public Text infoMessage;
	public Text errorMessage;

	private string serverInfo;
	private string cursorInfo = string.Empty;
	private string objectInfo = string.Empty;
	private string blobInfo = string.Empty;


	override protected void Initialise() {
		UpdateScreenDimensions();

		serverInfo = string.Format("Sending: {0}:{1}\nReceiving: {2}", Settings.ipAddress, Settings.port, Settings.listenPort);

		tuioTransmitter = new TUIOTransmitter(Settings.ipAddress, Settings.port);
		tuioTransmitter.Connect();

		if (Settings.listen) {
			tuioInput.TuioPort = Settings.listenPort;
			tuioInput.enabled = true;
		}
	}

	//protected void OnEnable() {}

	protected void Update() {
		CheckScreenDimensions();
	}

	protected void LateUpdate() {
		/*if (Settings.sendCursors)*/ cursorInfo = "\nCursors:" + tuioTransmitter.cursorCount.ToString();
		/*if (Settings.sendObjects)*/ objectInfo = "\nObjects:" + tuioTransmitter.objectCount.ToString();
		/*if (Settings.sendBlobs)*/ blobInfo = "\nBlobs:" + tuioTransmitter.blobCount.ToString();

		infoMessage.text = string.Format("{0}{1}{2}{3}", serverInfo, cursorInfo, objectInfo, blobInfo);

		tuioTransmitter.Send();
		errorMessage.enabled = tuioTransmitter.hasError;
	}

	//protected void OnDisable() {}

	override protected void OnDestroy() {
		tuioTransmitter.Close();
		tuioTransmitter = null;

		base.OnDestroy();
	}


	//
	// screen dimensions
	//

	private void CheckScreenDimensions() {
		screenDimensionsChanged = (Screen.width != screenDimensions.x || Screen.height != screenDimensions.y);
		if (screenDimensionsChanged)
			UpdateScreenDimensions();
	}

	private void UpdateScreenDimensions() {
		screenDimensions = new Vector2(Screen.width, Screen.height);
		invScreenDimensions = new Vector2(1f/Screen.width, 1f/Screen.height);
	}
}
