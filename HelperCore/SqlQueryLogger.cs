using MySqlConnector;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

namespace HelperCore;

public class SqlQueryLogger
{
    private readonly Action<string, bool> _log;
    private string _lastCommandLogged;
    private List<string> _lastParameters = new();

    public SqlQueryLogger(Action<string, bool> log)
    {
        _log = log;
    }

    public bool IsActive { get; set; } = true;

    public void LogSqlCommand(
        MySqlCommand mySqlCommand,
        string[] ignoreParams = null,
        [CallerFilePath] string callerFilePath = "",
        [CallerLineNumber] int callerLineNumber = 0,
        [CallerMemberName] string callerMemberName = "")
    {
        if (!IsActive)
        {
            return;
        }

        var fp = Path.GetFileName(callerFilePath);

        bool IsIgnoredParam(string prm)
        {
            return ignoreParams != null && ignoreParams.Contains(prm);
        }

        var ps = mySqlCommand.Parameters
            .Where(x => !IsIgnoredParam(x.ParameterName))
            .Select(x => $"   {x.ParameterName}={x.Value}").ToList();
        var commandText = mySqlCommand.CommandText;
        if (commandText != _lastCommandLogged)
        {
            _lastCommandLogged = commandText;
            _lastParameters = ps;
            var psMsg = string.Join(Environment.NewLine, ps);
            _log?.Invoke($"Running query ({fp}:{callerLineNumber}): {commandText}{Environment.NewLine}{psMsg}", true);
        }
        else
        {
            var psDiff = ps.Except(_lastParameters).ToList();
            if (psDiff.Any())
            {
                _lastParameters = ps;
                var psMsg = string.Join(Environment.NewLine, psDiff);
                _log?.Invoke($"Running same query{Environment.NewLine}{psMsg}", true);
            }
        }
    }
}
