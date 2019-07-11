using System;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace TRLogic.WebApi.Task.Helpers
{
	public class JsonModelBinder : IModelBinder
	{
		public System.Threading.Tasks.Task BindModelAsync(ModelBindingContext bindingContext)
		{
			if (bindingContext == null)
			{
				throw new ArgumentNullException(nameof(bindingContext));
			}

			// Check the value sent in
			var valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
			if (valueProviderResult != ValueProviderResult.None)
			{
				bindingContext.ModelState.SetModelValue(bindingContext.ModelName, valueProviderResult);

				// Attempt to convert the input value
				var valueAsString = valueProviderResult.FirstValue;
				var result = Newtonsoft.Json.JsonConvert.DeserializeObject(valueAsString, bindingContext.ModelType);
				if (result != null)
				{
					bindingContext.Result = ModelBindingResult.Success(result);
					return System.Threading.Tasks.Task.CompletedTask;
				}
			}

			return System.Threading.Tasks.Task.CompletedTask;
		}
	}
}
