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

		public static PostData.TextData SeparateText(NewsItem post)
		{
			return new PostData.TextData()
			{
				postID = $"{post.SourceId}_{post.PostId}",
				postText = post.Text
			};
		}

		public static PostData.LinksData SeparateLinks(NewsItem post)
		{
			return new PostData.LinksData()
			{
				postID = $"{post.SourceId}_{post.PostId}",
				postLinks = Parser.GetAttachments<Link>(post).ConvertAll(i => i.Uri.ToString())
			};
		}

		public static PostData.ImagesData SeparateImages(NewsItem post)
		{
			return new PostData.ImagesData()
			{
				postID = $"{post.SourceId}_{post.PostId}",
				postImages = Parser.GetAttachments<Photo>(post).ConvertAll(i => i.Sizes[^1].Url.ToString())
			};
		}
	}
}
