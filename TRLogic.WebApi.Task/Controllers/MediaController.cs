using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TRLogic.WebApi.Task.Helpers;
using TRLogic.WebApi.Task.Models;

namespace TRLogic.WebApi.Task.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MediaController : ControllerBase
    {
        
        // POST: api/Media
        [HttpPost]
        public IActionResult  Post([ModelBinder(BinderType = typeof(JsonModelBinder))]RequestMediaJsonModel JsonModel, List<IFormFile> Images)
        {
	        ResponseModel<List<ResponseModel<MediaFile>>> vResult;

			try
			{
				var workWithFiles = new MediaConverter(JsonModel.Base64Files, JsonModel.FileUrlList, Images).DownloadAndCreatePreview();
				if (!workWithFiles.IsOk)
				{
					throw  new CustomException(workWithFiles.Code, workWithFiles.Message);
				}

				vResult = workWithFiles;
			}
			catch (CustomException ce)
			{
				return BadRequest(ce.Message);
			}
			catch (Exception e)
			{
				return BadRequest(e.Message);
			}

			return Ok(vResult);
		}
	
    }
}
