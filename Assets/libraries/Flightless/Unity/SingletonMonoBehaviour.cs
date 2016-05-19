/*

	Singleton MonoBehaviour

	Generic Unity MonoBehaviour Singleton

	Usage:
		public class MusicManager : SingletonMonobehaviour<MusicManager> {
			public void PlaySong(string name) {}
		}

	To survive scene loads use DontDestroyOnLoad(this) in subclasses or use SingletonMonobehaviourNoDestroy.
	nb. this ONLY works if any parents of the gameobject aren't themselves destroyed! ie. should be at root level or nested in another non-destructible gameobject.

	Updated 24/5/2015
	Copyright Flightless 2014. All rights reserved.

*/

using UnityEngine;
using System.Collections.Generic;

namespace Flightless {
		
	public abstract class SingletonMonoBehaviour<T> : MonoBehaviour where T : SingletonMonoBehaviour<T> {

		private static T _instance;
		public static T instance { get { return _instance ?? (!isApplicationQuitting ? new GameObject("_" + typeof(T)).AddComponent<T>() : null ); } }
		public static T CreateInstance() { return instance; }
		public static bool hasInstance { get { return _instance != null; } }

		public static bool isApplicationQuitting { get; protected set; }


		virtual protected void Awake() {
			if (_instance != null) {
				Debug.LogError(name + ".Awake() error: already initialised as " + _instance.name);
				Destroy(gameObject);
				return;
			}

			_instance = (T)this;
			Initialise();
		}
		
		virtual protected void Initialise() {}

		virtual protected void OnApplicationQuit() {
			isApplicationQuitting = true;
		}

		virtual protected void OnDestroy() {
			if (_instance == this) _instance = null;
		}
	}


	public abstract class SingletonMonoBehaviourNoDestroy<T> : SingletonMonoBehaviour<T> where T : SingletonMonoBehaviourNoDestroy<T> {

		override protected void Awake() {
			base.Awake();
			DontDestroyOnLoad(gameObject);
		}
	}
}
