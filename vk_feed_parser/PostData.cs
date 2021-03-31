using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using VkNet.Model;
using VkNet.Model.Attachments;

namespace vk_feed_parser
{
	public class PostData
	{
		public string postId;
		public List<string> postContent;

		public override string ToString()
		{
			string content = string.Empty;
			foreach (var i in postContent)
			{
				content += i + Environment.NewLine;
			}

			return $"\"Id\": {postId}\n" +
				$"\"Content\": {content}\n";
		}

		public static PostData SeparateText(NewsItem post) => new PostData()
		{
			postId = $"{post.SourceId}_{post.PostId}",
			postContent = new List<string>() { post.Text }
		};

		public static PostData SeparateLinks(NewsItem post) => new PostData()
		{
			postId = $"{post.SourceId}_{post.PostId}",
			postContent = Parser.GetAttachments<Link>(post).ConvertAll(i => i.Uri.ToString())
		};

		public static PostData SeparateImages(NewsItem post) => new PostData()
		{
			postId = $"{post.SourceId}_{post.PostId}",
			postContent = Parser.GetAttachments<Photo>(post).ConvertAll(i => i.Sizes[^1].Url.ToString())
		};
	}

	class PostDataEqualityComparer : IEqualityComparer<PostData>
	{
		public bool Equals([AllowNull] PostData x, [AllowNull] PostData y) =>
			x.postId.Equals(y.postId);

		public int GetHashCode([DisallowNull] PostData obj) => 
			obj.postId.GetHashCode();
	}
}
