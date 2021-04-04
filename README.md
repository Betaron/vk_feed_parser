# vk_feed_parser

<h1>Welcome to VkFeedParser 3000!</h1>

  This is a small project, which in its final form should be able to authorize in *Vk*, receive data about *posts* from the *Newsfeed* and write this data to different files in different threads.

During authorization, none of your personal login data is saved, only an unlimited token is saved for subsequent authorization.
<h6>All data stored personal data is stored locally in the config.json file.</h6>

Currently, the main program *vk_feed_parser* gets data from the logged in user's newsfeed, extracts information about images, links and text, and saves the data to three different files.

There is also a Windows service in the repository. The service is able to view the contents of data files every 5 seconds and count the number of parsed posts and write these amounts to the file.
