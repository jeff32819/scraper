using Microsoft.Data.SqlClient;
using NLog;
using NLog.Common;
using NLog.Config;
using NLog.Targets;
using ScraperCode;

namespace ScraperApp2;

public class NLogSetup
{
    public NLogSetup(bool purgeLogs, bool showNLogInternalLogs = false)
    {
        Config = new LoggingConfiguration();
        PurgeLogs = purgeLogs;
        SetConsoleTarget();
        if (!showNLogInternalLogs)
        {
            return;
        }

        InternalLogger.LogLevel = LogLevel.Trace; // or Debug, Info, etc.
        InternalLogger.LogFile = @"t:\nlog-internal.log";
        InternalLogger.IncludeTimestamp = true;
        InternalLogger.Info("Internal logging initialized.");
    }

    public bool PurgeLogs { get; set; }

    private LoggingConfiguration Config { get; }

    public NLogger GetLogger()
    {
        LogManager.Configuration = Config;
        return new NLogger(LogManager.GetCurrentClassLogger());
    }

    public void SetFileTarget(string fileName)
    {
        if (PurgeLogs && File.Exists(fileName))
        {
            File.Delete(fileName);
        }

        var tmp = new FileTarget("logfile")
        {
            FileName = fileName, // Must be a static path
            Layout = "${message}",
            DeleteOldFileOnStartup = true
        };
        Config.AddRule(LogLevel.Info, LogLevel.Fatal, tmp);
    }

    private void SetConsoleTarget()
    {
        var tmp = new ConsoleTarget
        {
            Layout = "${message}"
        };
        Config.AddRule(LogLevel.Info, LogLevel.Fatal, tmp);
    }

    public void SetDbTarget(string dbConnString)
    {
        if (PurgeLogs)
        {
            DeleteAllLogItems(dbConnString);
        }

        var dbTarget = new DatabaseTarget("database")
        {
            ConnectionString = dbConnString,
            CommandText = "INSERT INTO logTbl (text, addedDateTime) VALUES (@text, @time_stamp);"
        };
        dbTarget.Parameters.Add(new DatabaseParameterInfo("@text", "${message}"));
        dbTarget.Parameters.Add(new DatabaseParameterInfo("@time_stamp", "${date:format=yyyy-MM-dd HH\\:mm\\:ss.fff}"));
        Config.AddRule(LogLevel.Info, LogLevel.Fatal, dbTarget);
    }

    private static void DeleteAllLogItems(string dbConnString)
    {
        using var conn = new SqlConnection(dbConnString);
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "DELETE FROM logTbl";
        cmd.ExecuteNonQuery();
    }
}