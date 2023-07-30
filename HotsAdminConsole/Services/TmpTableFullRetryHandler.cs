using MySqlConnector;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace HotsAdminConsole.Services;

public static class TmpTableFullRetryHandler
{
    public static async Task RetryIfTmpTableFull(Func<Task> act, Action notifier, CancellationToken token = default)
    {
        var retry = true;
        while (!token.IsCancellationRequested && retry)
        {
            retry = false;
            try
            {
                await act();
            }
            catch (MySqlException e) when (e.Message.StartsWith("The table '/hotslogs/tmpdir/#sql"))
            {
                notifier();
                retry = true;
            }
        }
    }

    public static void RetryIfTmpTableFull(Action act, Action notifier, CancellationToken token = default)
    {
        var retry = true;
        while (!token.IsCancellationRequested && retry)
        {
            retry = false;
            try
            {
                act();
            }
            catch (MySqlException e) when (e.Message.StartsWith("The table '/hotslogs/tmpdir/#sql"))
            {
                notifier();
                retry = true;
            }
        }
    }
}
