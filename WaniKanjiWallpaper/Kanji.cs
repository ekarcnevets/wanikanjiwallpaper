using Newtonsoft.Json;

namespace WaniKanjiWallpaper
{
	public class Kanji
	{
		public string Character { get; set; }
		public string Meaning { get; set; }
		public string Onyomi { get; set; }
		public string Kunyomi { get; set; }
		public string ImportantReading { get; set; }
		public int Level { get; set; }
		[JsonProperty(PropertyName = "user_specific")]
		public UserSpecific UserSpecificInfo { get; set; }
	}

	public class UserSpecific
	{
		public string Srs;
	}
}
