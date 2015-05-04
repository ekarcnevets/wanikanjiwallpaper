using System.Collections.Generic;
using Newtonsoft.Json;

namespace WaniKanjiWallpaper
{
	public class ApiResponse
	{
		[JsonProperty(PropertyName = "user_information")]
		public UserInformation UserInformation { get; set; }
		[JsonProperty(PropertyName = "requested_information")]
		public List<Kanji> RequestedInformation { get; set; }
		[JsonProperty(PropertyName = "error")]
		public ApiError Error { get; set; }
	}

	public class ApiError
	{
		public string Code { get; set; }
		public string Message { get; set; }
	}
}
