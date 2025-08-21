using System;
using System.IO;

public static class ScriptManager
{
    public static string ScriptsFolder
    {
        get
        {
            string path = Path.Combine(AppContext.BaseDirectory, "Scripts");
            Directory.CreateDirectory(path);
            return path;
        }
    }

    public static void EnsureDefaultScripts()
    {
        foreach (var scriptFile in Directory.GetFiles(ScriptsFolder, "*.lua"))
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
