using System;
using TRLogic.WebApi.Task.Models;

namespace TRLogic.WebApi.Task.Helpers
{
	public class CustomException : Exception
	{
		public ResponseCodes ResponseCode { get; }
		public string ResponseMessage { get; }
		public string Description { get; set; }

		public CustomException(ResponseCodes pResponseCode, string pMessage, string pDescription)
		: base(pMessage)
		{
			ResponseCode = pResponseCode;
			ResponseMessage = pMessage;
			Description = pDescription;
		}

		public CustomException(ResponseCodes pResponseCode, string pMessage)
		: base(pMessage)
		{
			ResponseCode = pResponseCode;
			ResponseMessage = pMessage;
		}
	}
}
