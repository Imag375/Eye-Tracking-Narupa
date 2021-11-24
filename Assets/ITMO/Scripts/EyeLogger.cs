using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class EyeLogger
{
    private string directoryPath = ".\\logs";

    private string fileName;

    private string fileNameFormat = "dd_MM_yy_HH_mm_ss";
    private string fileType = "txt";

    private string timeLogFormat = "HH:mm:ss.fff";

    public EyeLogger()
    {
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        fileName = $"{directoryPath}\\{System.DateTime.Now.ToString(fileNameFormat)}.{fileType}";

        if (!File.Exists(fileName))
        {
            File.Create(fileName);
        }
    }

    public void AddInfo(string infoToLog)
    {
        using (StreamWriter sw = new StreamWriter(File.Open(fileName, FileMode.Append, FileAccess.Write), System.Text.Encoding.UTF8))
        {
            sw.WriteLine($"[{System.DateTime.Now.ToString(timeLogFormat)}]: {infoToLog}");
            sw.Close();
        }
    }
   
}
