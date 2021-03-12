using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Text;
using VkNet.Model;
using VkNet.Model.Attachments;

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
				$"\"Links\": {links}\n" +
				$"\"Images\": {imgs}\n";
		}

		public static PostData SeparatePost(NewsItem post)
		{
			string id = $"{post.SourceId}_{post.PostId}";
			List<string> Images = Parser.GetAttachments<Photo>(post).ConvertAll(i => i.Sizes[^1].Url.ToString());
			List<string> Links = Parser.GetAttachments<Link>(post).ConvertAll(i => i.Uri.ToString());

			PostData postData = new PostData()
			{
				textData = {
					postID = id,
					postText = post.Text
				},
				imagesData =
				{
					postID = id,
					postImages = Images
				},
				linksData =
				{
					postID = id,
					postLinks = Links
				}
			};
			return postData;
		}
	}
}
