using Assistant.Server.CoreServer.Responses;
using System;

namespace Assistant.Client.EventArgs {
	public class OnResponseReceivedEventArgs {
		public readonly DateTime ReceivedTime;
		public readonly BaseResponse ReceivedResponse;
		public readonly string ReceivedResponseUnparsed;

		public OnResponseReceivedEventArgs(DateTime dt, BaseResponse resp, string respUnparsed) {
			ReceivedTime = dt;
			ReceivedResponse = resp;
			ReceivedResponseUnparsed = respUnparsed;
		}
	}
}
