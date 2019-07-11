using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Http;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Processing;
using TRLogic.WebApi.Task.Models;

namespace TRLogic.WebApi.Task.Helpers
{
	public class MediaConverter
	{
		private const int PREVIEW_WIDTH = 100;
		private const int PREVIEW_HEIGHT = 100;

		private static List<Base64File> Base64Files;
		private static List<string> Urls;
		private static List<IFormFile> Images;

		public MediaConverter(List<Base64File> pBase64List = null, List<string> pUrls = null, List<IFormFile> pImages = null)
		{
			if (pBase64List?.Count > 0)
			{
				Base64Files = pBase64List;
			}
			if (pUrls?.Count > 0)
			{
				Urls = pUrls;
			}
			if (pImages?.Count > 0)
			{
				Images = pImages;
			}
		}

		public ResponseModel<List<ResponseModel<MediaFile>>> DownloadAndCreatePreview()
		{
			var vResult = new ResponseModel<List<ResponseModel<MediaFile>>>();
			try
			{
				if (Base64Files != null)
				{
					var getFilesFromBase64 = Base64Worker(Base64Files);
					return getFilesFromBase64;
				}
				if (Urls != null)
				{
					var getFilesByUrl = UrlWorker(Urls);
					return getFilesByUrl;
				}

				if (Images != null)
				{
					var getFilesByIFile = FilesWorker(Images);
					return getFilesByIFile;
				}
			}
			catch (Exception e)
			{
				return new ResponseModel<List<ResponseModel<MediaFile>>>
				{
					Code = ResponseCodes.TECHNICAL_ERROR,
					Message = e.Message
				};
			}

			return vResult;
		}


		#region Private Methods

		private ResponseModel<List<ResponseModel<MediaFile>>> Base64Worker (List<Base64File> pBase64Files)
		{
			var vResult = new ResponseModel<List<ResponseModel<MediaFile>>>
			{
				ReturnObject = new List<ResponseModel<MediaFile>>()
			};
			var vTasks = new List<System.Threading.Tasks.Task<ResponseModel<MediaFile>>>();

			try
			{
				foreach (var file in pBase64Files)
				{
					vTasks.Add(System.Threading.Tasks.Task<ResponseModel<MediaFile>>.Factory.StartNew(() => WorkWithFileBase64(file)));
				}

				foreach(var task in vTasks)
				{
					vResult.ReturnObject.Add(task.Result);
				}
			}
			catch (Exception e)
			{
				return new ResponseModel<List<ResponseModel<MediaFile>>>
				{
					Code = ResponseCodes.TECHNICAL_ERROR,
					Message = e.Message
				};
			}

			return vResult;
		}

		private ResponseModel<List<ResponseModel<MediaFile>>> UrlWorker(List<string> pUrlList)
		{
			var vResult = new ResponseModel<List<ResponseModel<MediaFile>>>
			{
				ReturnObject = new List<ResponseModel<MediaFile>>()
			};
			var vTasks = new List<System.Threading.Tasks.Task<ResponseModel<MediaFile>>>();

			try
			{
				foreach (var url in pUrlList)
				{
					vTasks.Add(System.Threading.Tasks.Task<ResponseModel<MediaFile>>.Factory.StartNew(() => WorkWithFileUrl(url)));
				}

				foreach (var task in vTasks)
				{
					vResult.ReturnObject.Add(task.Result);
				}

			}
			catch (Exception e)
			{
				return new ResponseModel<List<ResponseModel<MediaFile>>>
				{
					Code = ResponseCodes.TECHNICAL_ERROR,
					Message = e.Message
				};
			}

			return vResult;
		}

		private ResponseModel<List<ResponseModel<MediaFile>>> FilesWorker(List<IFormFile> pImages)
		{
			var vResult = new ResponseModel<List<ResponseModel<MediaFile>>>
			{
				ReturnObject = new List<ResponseModel<MediaFile>>()
			};
			var vTasks = new List<System.Threading.Tasks.Task<ResponseModel<MediaFile>>>();

			try
			{
				foreach (var image in pImages)
				{
					vTasks.Add(System.Threading.Tasks.Task<ResponseModel<MediaFile>>.Factory.StartNew(() => WorkWithIFiles(image)));
				}

				foreach (var task in vTasks)
				{
					vResult.ReturnObject.Add(task.Result);
				}
			}
			catch (Exception e)
			{
				return new ResponseModel<List<ResponseModel<MediaFile>>>
				{
					Code = ResponseCodes.TECHNICAL_ERROR,
					Message = e.Message
				};
			}

			return vResult;
		}

		#region Workers
		private ResponseModel<MediaFile> WorkWithFileBase64(Base64File pBase64File)
		{
			ResponseModel<MediaFile> vResult;

			try
			{
				var convertFile = ConvertBase64ToFile(pBase64File.Base64String);
				if (!convertFile.IsOk)
				{
					throw new CustomException(convertFile.Code, convertFile.Message);
				}

				var getPreview = CreatePreview(convertFile.ReturnObject);
				if (!getPreview.IsOk)
				{
					throw new CustomException(getPreview.Code, getPreview.Message);
				}

				var saveFile = SaveFile(getPreview.ReturnObject);
				if (!saveFile.IsOk)
				{
					throw new CustomException(saveFile.Code, saveFile.Message);
				}

				vResult = getPreview;
			}
			catch (CustomException ce)
			{
				return new ResponseModel<MediaFile>
				{
					Code = ce.ResponseCode,
					Message = ce.Message
				};
			}
			catch (Exception e)
			{
				return new ResponseModel<MediaFile>
				{
					Code = ResponseCodes.TECHNICAL_ERROR,
					Message = e.Message
				};
			}

			return vResult;
		}

		private ResponseModel<MediaFile> WorkWithFileUrl(string pUr)
		{
			ResponseModel<MediaFile> vResult;
			try
			{
				var convertFile = GetFileByUrl(pUr);
				if (!convertFile.IsOk)
				{
					throw new CustomException(convertFile.Code, convertFile.Message);
				}

				var getPreview = CreatePreview(convertFile.ReturnObject);
				if (!getPreview.IsOk)
				{
					throw new CustomException(getPreview.Code, getPreview.Message);
				}

				var saveFile = SaveFile(getPreview.ReturnObject);
				if (!saveFile.IsOk)
				{
					throw new CustomException(saveFile.Code, saveFile.Message);
				}

				vResult = getPreview;
			}
			catch (CustomException ce)
			{
				return new ResponseModel<MediaFile>
				{
					Code = ce.ResponseCode,
					Message = ce.Message
				};
			}
			catch (Exception e)
			{
				return new ResponseModel<MediaFile>
				{
					Code = ResponseCodes.TECHNICAL_ERROR,
					Message = e.Message
				};
			}

			return vResult;
		}

		private ResponseModel<MediaFile> WorkWithIFiles(IFormFile pFile)
		{
			ResponseModel<MediaFile> vResult;
			try
			{
				var convertFile = ConvertIFileToFile(pFile);
				if (!convertFile.IsOk)
				{
					throw new CustomException(convertFile.Code, convertFile.Message);
				}

				var getPreview = CreatePreview(convertFile.ReturnObject);
				if (!getPreview.IsOk)
				{
					throw new CustomException(getPreview.Code, getPreview.Message);
				}

				var saveFile = SaveFile(getPreview.ReturnObject);
				if (!saveFile.IsOk)
				{
					throw new CustomException(saveFile.Code, saveFile.Message);
				}

				vResult = getPreview;
			}
			catch (CustomException ce)
			{
				return new ResponseModel<MediaFile>
				{
					Code = ce.ResponseCode,
					Message = ce.Message
				};
			}
			catch (Exception e)
			{
				return new ResponseModel<MediaFile>
				{
					Code = ResponseCodes.TECHNICAL_ERROR,
					Message = e.Message
				};
			}

			return vResult;
		}


		#endregion

		#region Convertors
		private ResponseModel<MediaFile> ConvertIFileToFile(IFormFile pFile)
		{
			var vResult = new ResponseModel<MediaFile>();
			var vMemoryStream = new MemoryStream();

			try
			{
				if (pFile.Length == 0)
				{
					throw new CustomException(ResponseCodes.INVALID_INPUT_PARAMS, $"Invalid IFormFile Stream length");
				}
				pFile.CopyTo(vMemoryStream);

				vResult.ReturnObject = new MediaFile
				{
					Content = vMemoryStream.ToArray(),
					MimeType = pFile.ContentType,
					Name = pFile.FileName
				};
			}
			catch (CustomException ce)
			{
				return new ResponseModel<MediaFile>
				{
					Code = ce.ResponseCode,
					Message = ce.Message
				};
			}
			catch (Exception e)
			{
				return new ResponseModel<MediaFile>
				{
					Code = ResponseCodes.TECHNICAL_ERROR,
					Message = e.Message
				};
			}

			return vResult;
		}

		private ResponseModel<MediaFile> ConvertBase64ToFile(string pBase64String)
		{
			var vResult = new ResponseModel<MediaFile>();
			var vByteArray = new Span<byte>();

			try
			{
				var vSplitString = pBase64String.Split(";");
				var vMimeType = vSplitString?.FirstOrDefault()?.Split(":")[1];
				if (string.IsNullOrEmpty(vMimeType))
				{
					throw new CustomException(ResponseCodes.INVALID_INPUT_PARAMS, $"Invalid Base64 String");
				}

				var vImageString = vSplitString[1]?.Split(",")[1];
				if (string.IsNullOrEmpty(vImageString))
				{
					throw new CustomException(ResponseCodes.INVALID_INPUT_PARAMS, $"Invalid Base64 String");
				}

				//if (!Convert.TryFromBase64String(pBase64String, vByteArray, out _))
				//{
				//	throw new CustomException(ResponseCodes.INVALID_INPUT_PARAMS, $"Invalid Base64 String");
				//}

				vByteArray = Convert.FromBase64String(vImageString);
				vResult.ReturnObject = new MediaFile
				{
					Content = vByteArray.ToArray(),
					MimeType = vMimeType
				};
			}
			catch (CustomException ce)
			{
				return new ResponseModel<MediaFile>
				{
					Code = ce.ResponseCode,
					Message = ce.Message
				};
			}
			catch (Exception e)
			{
				return new ResponseModel<MediaFile>
				{
					Code = ResponseCodes.TECHNICAL_ERROR,
					Message = e.Message
				};
			}

			return vResult;
		}

		private ResponseModel<MediaFile> GetFileByUrl(string pUrl)
		{
			Stream vResponseStream = null;
			var vResultStream = new MemoryStream();
			var vResult = new ResponseModel<MediaFile>();
			try
			{
				var fileReq = (HttpWebRequest)WebRequest.Create(pUrl);

				var fileResp = (HttpWebResponse)fileReq.GetResponse();

				if (fileResp.StatusCode != HttpStatusCode.OK)
				{
					throw new CustomException(ResponseCodes.INVALID_INPUT_PARAMS, $"Request is unsuccessful it Return StatusCode : {fileResp.StatusCode}");
				}

				vResponseStream = fileResp.GetResponseStream();
				if (vResponseStream is null)
				{
					throw new CustomException(ResponseCodes.INVALID_INPUT_PARAMS, $"Request is unsuccessful it Return empty stream");
				}

				string vMimeType = fileResp.ContentType;
				string vFileName = fileResp.ResponseUri.Segments.LastOrDefault()?.Split('.').FirstOrDefault();
				vResponseStream.CopyTo(vResultStream);

				vResult.ReturnObject = new MediaFile
				{
					Name = vFileName,
					MimeType = vMimeType,
					Content = vResultStream.ToArray()
				};
			}
			catch (CustomException ce)
			{
				return new ResponseModel<MediaFile>
				{
					Code = ce.ResponseCode,
					Message = ce.Message
				};
			}
			catch (Exception e)
			{
				return new ResponseModel<MediaFile>
				{
					Code = ResponseCodes.TECHNICAL_ERROR,
					Message = e.Message
				};
			}
			finally
			{
				vResponseStream?.Close();
			}

			return vResult;
		}
		#endregion
		#endregion

		public ResponseModel<MediaFile> CreatePreview(MediaFile pFile)
		{
			var vResult = new ResponseModel<MediaFile>();
			var vPreviewStream = new MemoryStream();

			try
			{
				using (var image = Image.Load(pFile.Content, out IImageFormat vFormat))
				{
					image.Mutate(x => x
						 .Resize(PREVIEW_WIDTH, PREVIEW_HEIGHT));
					image.Save(vPreviewStream, vFormat);
				}

				pFile.PreviewContent = vPreviewStream.ToArray();

				vResult.ReturnObject = pFile;
			}
			catch (CustomException ce)
			{
				return new ResponseModel<MediaFile>
				{
					Code = ce.ResponseCode,
					Message = ce.Message
				};
			}
			catch (Exception e)
			{
				return new ResponseModel<MediaFile>
				{
					Code = ResponseCodes.TECHNICAL_ERROR,
					Message = e.Message
				};
			}
			finally
			{
				vPreviewStream.Close();
			}

			return vResult;
		}

		public ResponseModel SaveFile(MediaFile pFile)
		{
			var vResult = new ResponseModel();
			try
			{

			}
			catch (CustomException ce)
			{
				return new ResponseModel
				{
					Code = ce.ResponseCode,
					Message = ce.Message
				};
			}
			catch (Exception e)
			{
				return new ResponseModel
				{
					Code = ResponseCodes.TECHNICAL_ERROR,
					Message = e.Message
				};
			}

			return vResult;
		}


	}
}
