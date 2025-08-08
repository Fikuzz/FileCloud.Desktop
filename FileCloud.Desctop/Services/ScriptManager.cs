using System;
using System.IO;

public static class ScriptManager
{
    public static string ScriptsFolder
    {
        get
        {
            string path = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "FileCloud.Desktop", "Scripts"
            );
            Directory.CreateDirectory(path);
            return path;
        }
    }

    public static void EnsureDefaultScripts()
    {
        string defaultScriptsPath = Path.Combine(AppContext.BaseDirectory, "Scripts");

        foreach (var scriptFile in Directory.GetFiles(defaultScriptsPath, "*.lua"))
        {
            string fileName = Path.GetFileName(scriptFile);
            string targetPath = Path.Combine(ScriptsFolder, fileName);

            // Copy only if not already customized
            if (!File.Exists(targetPath))
            {
                File.Copy(scriptFile, targetPath);
            }
        }
    }
}
