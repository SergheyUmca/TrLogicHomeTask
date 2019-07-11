using System.Collections.Generic;

namespace TRLogic.WebApi.Task
{
	public class RequestMediaJsonModel
	{
		public List<Base64File> Base64Files { get; set; }
		public List<string> FileUrlList { get; set; }
		
	}

	public class Base64File
	{
		public string FileName { get; set; }
		public string Base64String { get; set; }
	}

	public class MediaFile
	{
		public string Name { get; set; }
		public string MimeType { get; set; }
		public byte[] Content { get; set; }
		public byte[] PreviewContent { get; set; }
	}
}
