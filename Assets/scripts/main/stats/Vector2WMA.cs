//
// basic https://en.wikipedia.org/wiki/Moving_average
//

using UnityEngine;
using System.Collections.Generic;

public class Vector2WMA {

	public int size { get; private set; }
	public List<Vector2> data { get; private set; }

	public Vector2 value { get; private set; }

	private Vector2 numerator;
	private float invDenominator;


	public Vector2WMA() : this(5) {}

	public Vector2WMA(int size) {
		this.size = size;
		int denominator = (size * size + 1) / 2;
		invDenominator = 1f / denominator;

		data = new List<Vector2>(size);
		Clear();
	}

	public void Clear() {
		data.Clear();
		while (data.Count < size) {
			data.Add(Vector2.zero);
		}

		numerator = Vector2.zero;
		value = Vector2.zero;
	}

	public void AddSample(Vector2 newSample) {
		data.RemoveAt(size-1);
		data.Insert(0, newSample);

		numerator = Vector2.zero;
		for (int i=0; i<size; i++) {
			numerator += (size-i) * data[i];
		}

		value = invDenominator * numerator;
	}
}
