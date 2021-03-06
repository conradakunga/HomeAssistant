using System;

namespace Assistant.Server.CoreServer.EventArgs {
	public class OnClientConnectedEventArgs {
		public readonly string? IpAddress;
		public readonly DateTime ConnectionTime;
		public readonly string? ClientUid;

		public OnClientConnectedEventArgs(string? _ip, DateTime _connTime, string? _uid) {
			IpAddress = _ip;
			ConnectionTime = _connTime;
			ClientUid = _uid;
		}
	}
}
