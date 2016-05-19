using UnityEngine;
using OSCsharp.Data;
using TUIOsharp.Entities;

namespace TUIOSimulator.Entities {
	
	public class TUIOObject : TuioObject, ITUIOEntity {

		public OscMessage oscMessage { get; private set; }
		public bool isSendRequired { get; set; }

		public event EntityUpdatedHandler onUpdated;


		public TUIOObject(int id, int classId) : this(id, classId, 0, 0, 0, 0, 0, 0, 0, 0) {}

		public TUIOObject(int id, int classId, float x, float y, float angle, float velocityX, float velocityY, float rotationVelocity, float acceleration, float rotationAcceleration)
			: base(id, classId, x, y, angle, velocityX, velocityY, rotationVelocity, acceleration, rotationAcceleration) {

			// http://www.tuio.org/?specification - Profiles
			// /tuio/2Dobj set s i x y a X Y A m r
			oscMessage = new OscMessage("/tuio/2Dobj");
			oscMessage.Append("set");
			oscMessage.Append(id); // s
			oscMessage.Append(classId); // i
			oscMessage.Append(x); // x
			oscMessage.Append(y); // y
			oscMessage.Append(angle); // a
			oscMessage.Append(velocityX); // X
			oscMessage.Append(velocityY); // Y
			oscMessage.Append(rotationVelocity); // A
			oscMessage.Append(acceleration); // m
			oscMessage.Append(rotationAcceleration); // r

			isSendRequired = true;
		}

		new public void Update(float x, float y, float angle, float velocityX, float velocityY, float rotationVelocity, float acceleration, float rotationAcceleration) {
			bool changed = !Mathf.Approximately(x, X) ||
				!Mathf.Approximately(y, Y) ||
				!Mathf.Approximately(angle, Angle) ||
				!Mathf.Approximately(velocityX, VelocityX) ||
				!Mathf.Approximately(velocityY, VelocityY) ||
				!Mathf.Approximately(rotationVelocity, RotationVelocity) ||
				!Mathf.Approximately(acceleration, Acceleration) ||
				!Mathf.Approximately(rotationAcceleration, RotationAcceleration);

			if (changed) {
				base.Update(x, y, angle, velocityX, velocityY, rotationVelocity, acceleration, rotationAcceleration);

				UpdateOSCMessage();
				isSendRequired = true;

				if (onUpdated != null) onUpdated(this);
			}
		}

		protected void UpdateOSCMessage() {
			oscMessage.UpdateDataAt(3, X);
			oscMessage.UpdateDataAt(4, Y);
			oscMessage.UpdateDataAt(5, Angle);
			oscMessage.UpdateDataAt(6, VelocityX);
			oscMessage.UpdateDataAt(7, VelocityY);
			oscMessage.UpdateDataAt(8, RotationVelocity);
			oscMessage.UpdateDataAt(9, Acceleration);
			oscMessage.UpdateDataAt(10, RotationAcceleration);
		}
	}
}
