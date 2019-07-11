
namespace TRLogic.WebApi.Task.Models
{
	public class ResponseModel
	{
		public bool IsOk => Code == ResponseCodes.SUCCESS;
		public ResponseCodes Code { get; set; }
		public string Message { get; set; }
	}

	public class ResponseModel<T> : ResponseModel
	{
		public new bool IsOk => Code == ResponseCodes.SUCCESS && ReturnObject != null;
		public T ReturnObject {get;set;}
	}


	public enum ResponseCodes
	{
		SUCCESS = 0,
		TECHNICAL_ERROR = 1,
		CANT_CONVERT_FILE = 2,
		CANT_SAVE_FILE = 3,
		CANT_CREATE_PREVIEW = 4,
		LONG_QUERY  = 5,
		INVALID_INPUT_PARAMS = 6
	}
}
