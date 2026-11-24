using System;
using System.IO;

namespace Rake2D;

public class Logger
{
	private static StreamWriter file;

	private int _logNumber;

	public Logger()
	{
		while (file == null && _logNumber < 20)
		{
			try
			{
				string path = ((_logNumber <= 0) ? Path.Combine(DataUtils.GetAppDataDirectory(), "!game_log.txt") : Path.Combine(DataUtils.GetAppDataDirectory(), $"!game_log_{_logNumber}.txt"));
				file = new StreamWriter(path);
			}
			catch
			{
				_logNumber++;
			}
		}
	}

	public void LogTimeStamp()
	{
		file.WriteLine("- TimeStamp: " + DateTime.Now.ToString("HH:mm:ss"));
		file.Flush();
	}

	public void Log(string message)
	{
		file.WriteLine(message);
		file.Flush();
	}
}
