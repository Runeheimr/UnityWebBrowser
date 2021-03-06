using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityWebBrowser;

public static class CopyCEFFiles
{
	private static string[] fileTypes = new[] {".pak", ".exe", ".dat", ".bin"};

	[PostProcessBuild(1)]
	public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
	{
		pathToBuiltProject = Path.GetDirectoryName(pathToBuiltProject);
		string buildPluginsDir =
			Path.GetFullPath($"{pathToBuiltProject}/{Application.productName}_Data/Plugins/x86_64/");
		string cefProcessDir = WebBrowserUtils.GetCefProcessPath();
		string cefProcessDirParent = Directory.GetParent(cefProcessDir).Name;

		IEnumerable<string> files = 
			Directory.EnumerateFiles(cefProcessDir, "*.*", SearchOption.AllDirectories).Where(file => fileTypes.Any(x => file.EndsWith(x, StringComparison.OrdinalIgnoreCase)));
		foreach (string file in files)
		{
			string parentDirectory = "";
			if (Directory.GetParent(file).Name != cefProcessDirParent)
			{
				parentDirectory = $"{Directory.GetParent(file).Name}/";

				if (!Directory.Exists($"{buildPluginsDir}{parentDirectory}"))
					Directory.CreateDirectory($"{buildPluginsDir}{parentDirectory}");
			}

			string destFileName = Path.GetFileName(file);

			File.Copy(file, $"{buildPluginsDir}{parentDirectory}{destFileName}");
		}
	}
}
