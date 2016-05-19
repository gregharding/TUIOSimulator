using OSCsharp.Data;

namespace TUIOSimulator.Entities {

	public delegate void EntityUpdatedHandler (ITUIOEntity entity);

	public interface ITUIOEntity {

		int Id { get; }

		OscMessage oscMessage { get; }
		bool isSendRequired { get; set; }

		event EntityUpdatedHandler onUpdated;
	}
}
