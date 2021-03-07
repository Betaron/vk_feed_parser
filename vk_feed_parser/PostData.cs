using System;
using System.Collections.Generic;
using System.Text;

namespace vk_feed_parser
{
	class PostData
	{
		public struct TextData
		{
			public ulong? postID;
			public string postText;
		}
		
		public struct ImagesData
		{
			public ulong? postID;
			public string[] postImages; 
		}

		public struct LinksData
		{
			public ulong? postID;
			public string[] postLinks;
		}
	}
}
