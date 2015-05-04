using System.Collections.Generic;
using Newtonsoft.Json;

namespace WaniKanjiWallpaper
{
	[JsonObject(Title = "user_information", Id = "user_information")]
	public class UserInformation
	{
		public string Username { get; set; }
		public int Level { get; set; }
		public string Title { get; set; }
	}
}
