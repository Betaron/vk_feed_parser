using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Text;

namespace vk_feed_parser
{
	public class PostData
	{
		public struct TextData
		{
			public string postID;
			public string postText;
		}
		
		public struct ImagesData
		{
			public string postID;
			public List<string> postImages; 
		}

		public struct LinksData
		{
			public string postID;
			public List<string> postLinks;
		}

		public TextData textData;
		public ImagesData imagesData;
		public LinksData linksData;

		public override string ToString()
		{
			string imgs = string.Empty;
			string links = string.Empty;
			foreach (var i in imagesData.postImages)
			{
				imgs += i + Environment.NewLine;
			}
			foreach (var i in linksData.postLinks)
			{
				links += i + Environment.NewLine;
			}

			return $"\"Id\": {textData.postID}\n" +
				$"\"Text\": {textData.postText}\n" +
				$"\"Images\": {imgs}\n" +
				$"\"Links\": {links}\n";
		}
	}
}
