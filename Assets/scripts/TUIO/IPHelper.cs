using System.Collections.Generic;
using System;
using System.Net;
using System.Net.Sockets;

namespace TUIOSimulator {

	public static class IPHelper {

		public static List<string> localIPAddresses { get; private set; }
		public static string localIPAddressesString { get; private set; }


		static IPHelper() {
			InitLocalIPAddresses();
		}

		private static void InitLocalIPAddresses() {
			try {
				localIPAddresses = new List<string>();
				localIPAddressesString = "";

				IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());

				foreach (IPAddress ip in host.AddressList) {
					if (ip.AddressFamily == AddressFamily.InterNetwork) {
						string ipString = ip.ToString();
						if (!localIPAddresses.Contains(ipString)) localIPAddresses.Add(ipString);
					}
				}

				// filter out some addresses
				localIPAddresses.RemoveAll(ip => ip == "127.0.0.1"); // just in case
				localIPAddresses.RemoveAll(ip => ip.StartsWith("0.0.0.")); // 0.0.0.0, 0.0.0.1 show up on iOS

				if (localIPAddresses.Count > 0) localIPAddressesString = string.Join(",", localIPAddresses.ToArray());
			}
			catch {}
			//catch(Exception e) {
			//	Debug.LogWarning(e);
			//}
		}
	}
}