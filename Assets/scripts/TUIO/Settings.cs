using System.Collections.Generic;
using System;
using System.Net;
using System.Net.Sockets;

namespace TUIOSimulator {

	public static class Settings {

		// client
		public const string defaultIPAddress = "127.0.0.1";
		public const int defaultPort = 3333;

		public static IPAddress ipAddress { get; private set; }
		public static int port { get; private set; }

		public static bool sendCursors = true;
		public static bool sendObjects = true;
		public static bool sendBlobs = false;

		// server
		public static string localIPAddresses { get; private set; }
		public const int defaultListenPort = 33333;
		public static int listenPort { get; private set; }

		public static bool listen { get; private set; }


		static Settings() {
			ipAddress = IPAddress.Parse(defaultIPAddress);
			port = defaultPort;

			localIPAddresses = IPHelper.localIPAddressesString;
			listenPort = defaultListenPort;
			listen = true;
		}

		public static void Use(string ipAddressString, string portString, string listenPortString) {
			if (string.IsNullOrEmpty(ipAddressString)) ipAddressString = defaultIPAddress;
			if (string.IsNullOrEmpty(portString)) portString = defaultPort.ToString();
			if (string.IsNullOrEmpty(listenPortString)) listenPortString = defaultListenPort.ToString();

			int port = 0;
			if (!int.TryParse(portString, out port)) port = defaultPort;

			int listenPort = 0;
			if (!int.TryParse(listenPortString, out listenPort)) listenPort = defaultListenPort;

			Use(ipAddressString, port, listenPort);
		}

		public static void Use(string ipAddressString, int _port, int _listenPort) {
			ipAddress = IsValidIpAddress(ipAddressString) ? IPAddress.Parse(ipAddressString) : IPAddress.Parse(defaultIPAddress);
			port = IsValidPort(_port) ? _port : defaultPort;
			listenPort = IsValidPort(_listenPort, true) ? _listenPort : defaultListenPort;
			listen = (IsValidPort(listenPort, true) && listenPort > 0);
		}


		//
		// helpers
		//

		public static bool IsValidIpAddress(string ipAddressString) {
			IPAddress ipAddress;
			return IsIPv4(ipAddressString) && IPAddress.TryParse(ipAddressString, out ipAddress);
		}

		public static bool IsIPv4(string ipAddressString) {
			// http://stackoverflow.com/questions/5096780/ip-address-validation
			var quads = ipAddressString.Split('.');

			// if we do not have 4 quads, return false
			if (quads.Length != 4) return false;

			// for each quad
			foreach(var quad in quads) {
				int q;
				// if parse fails 
				// or length of parsed int != length of quad string (i.e.; '1' vs '001')
				// or parsed int < 0
				// or parsed int > 255
				// return false
				if (!int.TryParse(quad, out q) 
					|| !q.ToString().Length.Equals(quad.Length) 
					|| q < 0 
					|| q > 255) { return false; }

			}

			return true;
		}

		public static bool IsValidPort(string portString, bool allow0 = false) {
			int port = 0;
			return (int.TryParse(portString, out port)) ? IsValidPort(port, allow0) : false;
		}

		public static bool IsValidPort(int port, bool allow0 = false) {
			return ((port >= 1024 && port <= ushort.MaxValue) || (port == 0 && allow0));
		}		
	}
}
