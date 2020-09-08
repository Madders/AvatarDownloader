using System;
using System.IO;
using System.Text.RegularExpressions;

namespace AvatarDownloader
{
    class Program
    {
        static void Main(string[] args)
        {
			CheckFolder("Downloads");

			if(args == null || args.Length != 1)
            {
				Console.WriteLine("Expected command line parameter of the avatars marketplace id.");
				return;
            }

			DownloadAvatar(args[0] + "-v1");
		}

		static void CheckFolder(string path)
		{
			if (!Directory.Exists(path))
			{
				Directory.CreateDirectory(path);
			}
		}

		static void DownloadAndSave(string url)
		{
			Console.WriteLine(url);
			string savePath = "Downloads\\" + url.Replace("http://mpassets.highfidelity.com/", "").Replace("/", "\\");

			Console.WriteLine(savePath);
			if (!File.Exists(savePath))
			{
				CheckFolder(Path.GetDirectoryName(savePath));

				Console.WriteLine("  Download file to savePath");

				using (var wc = new System.Net.WebClient())
					wc.DownloadFile(url, savePath);
			}
		}

        static void DownloadAvatar(string avatarId)
        {
			// Replace the following variable with your new URL
			var avatarUrl = "http://mpassets.highfidelity.com/" + avatarId + "/avatar/baked/avatar.baked.fst";
			var bakedPath = avatarUrl.Replace("avatar.baked.fst", "");
			var originalPath = bakedPath.Replace("/baked/", "/original/");

			DownloadAndSave(avatarUrl);

			string bakedFstContents;
			using (var wc = new System.Net.WebClient())
				bakedFstContents = wc.DownloadString(avatarUrl);

			string bakedFbxFilename = Regex.Match(bakedFstContents, "filename = (.*?.baked.fbx)").Groups[1].ToString();

			DownloadAndSave(bakedPath + bakedFbxFilename);

			string avatarFileGuid = bakedFbxFilename.Replace(".baked.fbx", "");

			DownloadAndSave(bakedPath + avatarFileGuid + ".baked.json");
			//Console.Write(bakedFstContents);

			string allMaterialsJsonContents;
			using (var wc = new System.Net.WebClient())
				allMaterialsJsonContents = wc.DownloadString(bakedPath + avatarFileGuid + ".baked.json");

			foreach (var match in Regex.Matches(allMaterialsJsonContents, "materialTextures/0/.*?.texmeta.json"))
			{
				string materialJsonUrl = bakedPath + match.ToString();
				DownloadAndSave(materialJsonUrl);

				string materialsJsonContents;
				using (var wc = new System.Net.WebClient())
					materialsJsonContents = wc.DownloadString(materialJsonUrl);

				foreach (Match matMatch in Regex.Matches(materialsJsonContents, ": \"(.*?\\..*?)\""))
				{
					DownloadAndSave(bakedPath + "materialTextures/0/" + matMatch.Groups[1].ToString());
				}
			}

			// finally output original file links
			DownloadAndSave(originalPath + "avatar.fst");
			DownloadAndSave(originalPath + avatarFileGuid + ".fbx");
		}
    }
}
