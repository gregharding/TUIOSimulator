using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Net;
using TUIOSimulator.Entities;
using TUIOsharp.Entities;
using OSCsharp.Net;
using OSCsharp.Data;

namespace TUIOSimulator {
	
	public class TUIOTransmitter {

		// network
		public IPAddress ipAddress { get; private set; }
		public int port { get; private set; }

		public string localIPAddresses { get; private set; }

		private UDPTransmitter udpTransmitter;

		// entities
		public IList<ITUIOEntity> entities {
			get { return _entities.AsReadOnly(); }
		}
		private List<ITUIOEntity> _entities;

		public int cursorCount { get; private set; }
		public int objectCount { get; private set; }
		public int blobCount { get; private set; }

		public int cursorsChangedFseq { get; private set; }
		public int objectsChangedFseq { get; private set; }
		public int blobsChangedFseq { get; private set; }

		private const int keepAliveInterval = 60;
		private const int keepAliveCursorsOffset = 0;
		private const int keepAliveObjectsOffset = 1;
		private const int keepAliveBlobsOffset = 2;

		public bool cursorsEnabled { get; private set; }
		public bool objectsEnabled { get; private set; }
		public bool blobsEnabled { get; private set; }

		// helpers
		public int lastSessionId { get; private set; }
		public int fseq { get; private set; }

		private OscMessage sourceMessage2Dcur;
		private OscMessage aliveMessage2Dcur;
		private OscMessage fseqMessage2Dcur;

		private OscMessage sourceMessage2Dobj;
		private OscMessage aliveMessage2Dobj;
		private OscMessage fseqMessage2Dobj;

		private OscMessage sourceMessage2Dblb;
		private OscMessage aliveMessage2Dblb;
		private OscMessage fseqMessage2Dblb;

		// errors
		public string errorMessage { get; private set; }
		public bool hasError { get; private set; }


		public TUIOTransmitter(string ipAddress, int port) : this(IPAddress.Parse(ipAddress), port) {}

		public TUIOTransmitter(IPAddress ipAddress, int port) {
			this.ipAddress = ipAddress;
			this.port = port;

			cursorsEnabled = true;
			objectsEnabled = true;
			//sendBlobs = true; // not supported yet

			localIPAddresses = IPHelper.localIPAddressesString;

			_entities = new List<ITUIOEntity>();

			// helper messages
			sourceMessage2Dcur = CreateSourceMessage("2Dcur");
			aliveMessage2Dcur = CreateAliveMessage("2Dcur");
			fseqMessage2Dcur = CreateFSeqMessage("2Dcur");

			sourceMessage2Dobj = CreateSourceMessage("2Dobj");
			aliveMessage2Dobj = CreateAliveMessage("2Dobj");
			fseqMessage2Dobj = CreateFSeqMessage("2Dobj");

			sourceMessage2Dblb = CreateSourceMessage("2Dblb");
			aliveMessage2Dblb = CreateAliveMessage("2Dblb");
			fseqMessage2Dblb = CreateFSeqMessage("2Dblb");
		}

		public void Connect() {
			if (udpTransmitter != null) Close();
			udpTransmitter = new UDPTransmitter(ipAddress, port);
			udpTransmitter.Connect();

			fseq = 0;
		}

		public void Close() {
			udpTransmitter.Close();
			udpTransmitter = null;
		}

		public void Send() {
			try {
				// bundle and send tuio data

				//Debug.LogFormat("TUIO Cursors:{0} Objects:{1} Blobs:{2}", cursorCount, objectCount, blobCount);

				bool sendKeepAliveCursors = (fseq % keepAliveInterval == keepAliveCursorsOffset);
				bool sendKeepAliveObjects = (fseq % keepAliveInterval == keepAliveObjectsOffset);
				bool sendKeepAliveBlobs = (fseq % keepAliveInterval == keepAliveBlobsOffset);

				// send?
				bool sendCursors = cursorsEnabled && (fseq == cursorsChangedFseq || sendKeepAliveCursors);
				bool sendObjects = objectsEnabled && (fseq == objectsChangedFseq || sendKeepAliveObjects);
				bool sendBlobs = blobsEnabled && (fseq == blobsChangedFseq || sendKeepAliveBlobs);

				if (sendCursors || sendObjects || sendBlobs) {
					OscBundle oscBundle = new OscBundle();
					if (sendCursors) AppendToBundle<TUIOCursor>(oscBundle, sourceMessage2Dcur, aliveMessage2Dcur, fseqMessage2Dcur, sendKeepAliveCursors);
					if (sendObjects) AppendToBundle<TUIOObject>(oscBundle, sourceMessage2Dobj, aliveMessage2Dobj, fseqMessage2Dobj, sendKeepAliveObjects);
					if (sendBlobs) AppendToBundle<TUIOBlob>(oscBundle, sourceMessage2Dblb, aliveMessage2Dblb, fseqMessage2Dblb, sendKeepAliveBlobs);
					udpTransmitter.Send(oscBundle);

					// reset any previous error
					hasError = false;
				}
			}
			catch {
				//throw new Exception(string.Format("Error sending TUIO bundle to {0}:{1}", ipAddress, port));
				hasError = true;
				errorMessage = string.Format("Error sending TUIO bundle to {0}:{1}", ipAddress, port);
			}
			finally {
				fseq++;
			}
		}

		private void AppendToBundle<T>(OscBundle oscBundle, OscMessage sourceMsg, OscMessage aliveMsg, OscMessage fseqMsg, bool forceSet = false) where T : ITUIOEntity {
			// http://www.tuio.org/?specification - Message & Bundle Format
			//
			// /tuio/2Dcur source application@address
			// /tuio/2Dcur alive s_id0 ... s_idN
			// /tuio/2Dcur set s x y X Y m
			// /tuio/2Dcur fseq f_id
			//
			// nb. TUIOsharp does not enforce an order for alive/set messages so long as they're before fseq

			oscBundle.Append(sourceMsg);

			// entities: alive, set
			aliveMsg.ClearData(); // nb. also clears address
			aliveMsg.Append("alive");

			oscBundle.Append(aliveMsg);

			for (int i=0; i < _entities.Count; i++) {
				ITUIOEntity e = _entities[i];
				if (e.GetType() != typeof(T)) continue;

				aliveMsg.Append(e.Id);
				if (forceSet || e.isSendRequired) {
					oscBundle.Append(e.oscMessage); // set
					e.isSendRequired = false;
				}
			}

			// frame sequence
			fseqMsg.UpdateDataAt(1, fseq);
			oscBundle.Append(fseqMsg);
		}


		//
		// message helpers
		//

		private OscMessage CreateSourceMessage(string type) {
			OscMessage msg = new OscMessage("/tuio/"+type);
			msg.Append("source");
			msg.Append("TUIO Simulator@" + localIPAddresses);
			return msg;
		}

		private OscMessage CreateAliveMessage(string type) {
			OscMessage msg = new OscMessage("/tuio/"+type);
			msg.Append("alive");
			return msg;
		}

		private OscMessage CreateFSeqMessage(string type) {
			OscMessage msg = new OscMessage("/tuio/"+type);
			msg.Append("fseq");
			msg.Append(fseq);
			return msg;
		}


		//
		// entities
		//

		public int NextSessionId() {
			return ++lastSessionId;
		}

		public void Add(ITUIOEntity entity) {
			if (entity == null || _entities.Contains(entity)) return;

			_entities.Add(entity);

			entity.onUpdated += OnEntityUpdated;

			if (entity is TUIOCursor) { cursorCount++; cursorsChangedFseq = fseq; }
			else if (entity is TUIOObject) { objectCount++; objectsChangedFseq = fseq; }
			else if (entity is TUIOBlob) { blobCount++; blobsChangedFseq = fseq; }
		}

		public void Remove(ITUIOEntity entity) {
			if (entity == null || !_entities.Remove(entity)) return;

			entity.onUpdated -= OnEntityUpdated;

			if (entity is TUIOCursor) { cursorCount--; cursorsChangedFseq = fseq; }
			else if (entity is TUIOObject) { objectCount--; objectsChangedFseq = fseq; }
			else if (entity is TUIOBlob) { blobCount--; blobsChangedFseq = fseq; }
		}

		public void RemoveAll() {
			_entities.ForEach(e => e.onUpdated -= OnEntityUpdated);
			_entities.Clear();

			cursorCount = objectCount = blobCount = 0;
			cursorsChangedFseq = objectsChangedFseq = blobsChangedFseq = fseq;
		}

		private void OnEntityUpdated(ITUIOEntity entity) {
			if (entity is TUIOCursor) cursorsChangedFseq = fseq;
			else if (entity is TUIOObject) objectsChangedFseq = fseq;
			else if (entity is TUIOBlob) blobsChangedFseq = fseq;
		}
	}
}