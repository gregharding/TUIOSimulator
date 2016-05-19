using UnityEngine;
using TUIOSimulator.Entities;

public interface ISurfaceEntity {

	Surface surface { get; }
	ITUIOEntity tuioEntity { get; }
}
