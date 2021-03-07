using System;
using System.Collections.Generic;
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
	}
}
