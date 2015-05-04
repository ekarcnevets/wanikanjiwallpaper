using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Net;
using System.Text;
using Windows.System;
using Newtonsoft.Json;
using System.Drawing;
using System.Reflection;
using Encoder = System.Drawing.Imaging.Encoder;

namespace WaniKanjiWallpaper
{
	class Program
	{
		public static readonly string UrlBase = "https://www.wanikani.com/api/user/{0}/{1}{2}";
		public static string ApiKey;
		public static string TempPath;

		static void Main(string[] args)
		{
			//TODO: Change command line args to App.config variables
			if (args.Length != 2)
			{
				Console.WriteLine("USAGE: WaniKanjiWallpaper [ApiKey] [Path to save temp output]");
			}

			ApiKey = args[0];
			TempPath = args[1];

			ApiResponse apiResponse;
			
			using (var webClient = new WebClient())
			{
				// Need to set encoding or the kanji come out mangled
				webClient.Encoding = Encoding.UTF8;

				// Get user info - need level to make primary call
				var rawJson = webClient.DownloadString(GetUri("user-information"));
				
				// Now parse with JSON.Net
				apiResponse = JsonConvert.DeserializeObject<ApiResponse>(rawJson);

				// Get kanji from current level
				var levelString = apiResponse.UserInformation.Level.ToString(CultureInfo.InvariantCulture);
				rawJson = webClient.DownloadString(GetUri("kanji", levelString));

				apiResponse = JsonConvert.DeserializeObject<ApiResponse>(rawJson);
			}

			var image = DrawKanji(apiResponse);

			SavePng(@"D:\wk.jpg", image, 100L);

			//var lockScreen = Windows.System.UserProfile.LockScreen.SetImageStreamAsync();
		}

		private static Bitmap DrawKanji(ApiResponse apiResponse)
		{
			if (apiResponse.RequestedInformation.Count > 40)
			{
				Console.WriteLine("WaniKanjiWallpaper fetch - too many kanji! ({0})", apiResponse.RequestedInformation.Count);
				Console.ReadLine();
			}

			if (apiResponse.RequestedInformation == null)
			{
				Environment.Exit(1);
			}
			else if (apiResponse.Error != null)
			{
				Console.WriteLine("WaniKanjiWallpaper fetch failed: {0}", apiResponse.Error.Message);
				Console.ReadLine();
				Environment.Exit(2);
			}

			var myAssembly = Assembly.GetExecutingAssembly();
			var myStream = myAssembly.GetManifestResourceStream("WaniKanjiWallpaper.wk.png");

			var image = new Bitmap(myStream);
			var graphics = Graphics.FromImage(image);
			graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

			const int x = 1350, y = 130, w = 900, h = 1245;
			const int boxDimension = 125;
			const int whitespaceDimension = 50;
			const int cols = 5;
			
			var rows = (apiResponse.RequestedInformation.Count / cols) + 1;

			var xOff = (w - cols*(boxDimension + whitespaceDimension) + whitespaceDimension)/2;
			var yOff = (h - rows*(boxDimension + whitespaceDimension) + whitespaceDimension)/2;

			for (var i = 0; i < rows; i++)
			{
				for (var j = 0; j < cols; j++)
				{
					var xPos = x + xOff + j*(boxDimension + whitespaceDimension);
					var yPos = y + yOff + i*(boxDimension + whitespaceDimension);

					var kanjiIndex = (i*cols) + j;

					var srsBrush = GetSrsBrush(apiResponse.RequestedInformation[kanjiIndex].UserSpecificInfo);
					graphics.DrawString(apiResponse.RequestedInformation[kanjiIndex].Character, new Font("Ms Mincho", 100, FontStyle.Bold), srsBrush, xPos - xOff, yPos);

					//If we have run out of kanji, return early
					if (kanjiIndex + 1 >= apiResponse.RequestedInformation.Count)
						return image;
				}
			}

			return image;
		}

		public static Uri GetUri(string requestedInfo, string additionalInfo = null)
		{
			var additionalInfoString = string.IsNullOrWhiteSpace(additionalInfo)
				? String.Empty
				: String.Format("/{0}", additionalInfo);

			return new Uri(String.Format(UrlBase, ApiKey, requestedInfo, additionalInfoString));
		}

		public static void SavePng(string path, Bitmap image, long quality)
		{
			// Encoder parameter for image quality
			var qualityParam = new EncoderParameter(Encoder.Quality, quality);

			// Jpeg image codec
			var pngCodec = GetEncoderInfo("image/jpeg");

			if (pngCodec == null)
				return;

			var encoderParams = new EncoderParameters(1);
			encoderParams.Param[0] = qualityParam;

			image.Save(path, pngCodec, encoderParams);
		}

		private static ImageCodecInfo GetEncoderInfo(string mimeType)
		{
			// Get image codecs for all image formats
			var codecs = ImageCodecInfo.GetImageEncoders();

			// Find the correct image codec
			return codecs.FirstOrDefault(t => t.MimeType == mimeType);
		}

		private static SolidBrush GetSrsBrush(UserSpecific userSpecificInfo)
		{
			Color srsColor;
			var srs = userSpecificInfo == null ? "" : userSpecificInfo.Srs;

			switch(srs)
			{
				case "apprentice":
					srsColor = Color.FromArgb(255, 221, 0, 147);
					break;
				case "guru":
					srsColor = Color.FromArgb(255, 136, 45, 158);
					break;
				case "master":
					srsColor = Color.FromArgb(255, 41, 77, 219);
					break;
				case "enlightened":
					srsColor = Color.FromArgb(255, 0, 147, 221);
					break;
				case "burned":
					srsColor = Color.FromArgb(255, 255, 255, 255);
					break;
				default: //unseen
					srsColor = Color.FromArgb(255, 48, 48, 48);
					break;
			}

			return new SolidBrush(srsColor);
		}
	}
}
