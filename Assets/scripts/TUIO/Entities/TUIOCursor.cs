using UnityEngine;
using OSCsharp.Data;
using TUIOsharp.Entities;

namespace TUIOSimulator.Entities {
	
	public class TUIOCursor : TuioCursor, ITUIOEntity {

		public OscMessage oscMessage { get; private set; }
		public bool isSendRequired { get; set; }

		public event EntityUpdatedHandler onUpdated;


		public TUIOCursor(int id) : this(id, 0, 0, 0, 0, 0) {}

		public TUIOCursor(int id, float x, float y, float velocityX, float velocityY, float acceleration)
			: base(id, x, y, velocityX, velocityY, acceleration) {

			// http://www.tuio.org/?specification - Profiles
			// /tuio/2Dcur set s x y X Y m
			oscMessage = new OscMessage("/tuio/2Dcur");
			oscMessage.Append("set");
			oscMessage.Append(id); // s
			oscMessage.Append(x); // x
			oscMessage.Append(y); // y
			oscMessage.Append(velocityX); // X
			oscMessage.Append(velocityY); // Y
			oscMessage.Append(acceleration); // m

			isSendRequired = true;
		}

		new public void Update(float x, float y, float velocityX, float velocityY, float acceleration) {
			bool changed = !Mathf.Approximately(x, X) ||
				!Mathf.Approximately(y, Y) ||
				!Mathf.Approximately(velocityX, VelocityX) ||
				!Mathf.Approximately(velocityY, VelocityY) ||
				!Mathf.Approximately(acceleration, Acceleration);

			if (changed) {
				base.Update(x, y, velocityX, velocityY, acceleration);

				UpdateOSCMessage();
				isSendRequired = true;

				if (onUpdated != null) onUpdated(this);
			}
		}

		protected void UpdateOSCMessage() {
			oscMessage.UpdateDataAt(2, X);
			oscMessage.UpdateDataAt(3, Y);
			oscMessage.UpdateDataAt(4, VelocityX);
			oscMessage.UpdateDataAt(5, VelocityY);
			oscMessage.UpdateDataAt(6, Acceleration);
		}
	}
}
