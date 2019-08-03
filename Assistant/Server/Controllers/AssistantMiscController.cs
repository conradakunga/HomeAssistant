using Assistant.AssistantCore;
using Assistant.Extensions;
using Assistant.Server.Responses;
using Assistant.Weather;
using Microsoft.AspNetCore.Mvc;
using System;
using Assistant.Geolocation;
using Assistant.Server.Authentication;

namespace Assistant.Server.Controllers {
	[Route("api/misc/")]
	public class AssistantMiscController : Controller {
		[HttpPost("weather")]
		public ActionResult<GenericResponse<WeatherData>> GetWeatherInfo(AuthPostData auth, int pinCode, string countryCode) {
			if (auth == null) {
				return BadRequest(new GenericResponse<string>(
					"Please provide the specified authentication information!",
					Enums.HttpStatusCodes.BadRequest, DateTime.Now));
			}

			if (!Core.CoreInitiationCompleted) {
				return BadRequest(new GenericResponse<string>(
					$"{Core.AssistantName} core initiation isn't completed yet, please be patient while it is completed. retry after 20 seconds.",
					Enums.HttpStatusCodes.BadRequest, DateTime.Now));
			}

			if (!KestrelServer.Authentication.IsAllowedToExecute(auth)) {
				return BadRequest(new GenericResponse<string>("You are not authenticated with the assistant. Please use the authentication endpoint to authenticate yourself!", Enums.HttpStatusCodes.BadRequest, DateTime.Now));
			}

			if (pinCode <= 0) {
				return BadRequest(new GenericResponse<string>($"The specified pin code is invalid.",
					Enums.HttpStatusCodes.BadRequest, DateTime.Now));
			}

			if (Helpers.IsNullOrEmpty(countryCode)) {
				return BadRequest(new GenericResponse<string>($"The specified country code is invalid.",
					Enums.HttpStatusCodes.BadRequest, DateTime.Now));
			}

			(bool status, WeatherData response) = Core.WeatherApi.GetWeatherInfo(Core.Config.OpenWeatherApiKey, pinCode, countryCode);

			if (status) {
				return Ok(new GenericResponse<WeatherData>(response, Enums.HttpStatusCodes.OK, DateTime.Now));
			}
			else {
				return BadRequest(new GenericResponse<string>("Failed to fetch weather info.",
					Enums.HttpStatusCodes.BadRequest, DateTime.Now));
			}
		}

		[HttpPost("zip")]
		public ActionResult<GenericResponse<ZipLocationResult>> LocationFromZipCode(AuthPostData auth, long zipCode) {
			if (auth == null) {
				return BadRequest(new GenericResponse<string>(
					"Please provide the specified authentication information!",
					Enums.HttpStatusCodes.BadRequest, DateTime.Now));
			}

			if (!Core.CoreInitiationCompleted) {
				return BadRequest(new GenericResponse<string>(
					$"{Core.AssistantName} core initiation isn't completed yet, please be patient while it is completed. retry after 20 seconds.",
					Enums.HttpStatusCodes.BadRequest, DateTime.Now));
			}

			if (!KestrelServer.Authentication.IsAllowedToExecute(auth)) {
				return BadRequest(new GenericResponse<string>("You are not authenticated with the assistant. Please use the authentication endpoint to authenticate yourself!", Enums.HttpStatusCodes.BadRequest, DateTime.Now));
			}

			if (zipCode <= 0) {
				return BadRequest(new GenericResponse<string>($"The specified pin code is invalid.",
					Enums.HttpStatusCodes.BadRequest, DateTime.Now));
			}

			(bool status, ZipLocationResult apiResult) = Core.ZipCodeLocater.GetZipLocationInfo(zipCode);

			if (status) {
				return Ok(new GenericResponse<ZipLocationResult>(apiResult, Enums.HttpStatusCodes.OK, DateTime.Now));
			}
			else {
				return BadRequest(new GenericResponse<string>("Failed to fetch zip code info.",
					Enums.HttpStatusCodes.BadRequest, DateTime.Now));
			}
		}
	}
}