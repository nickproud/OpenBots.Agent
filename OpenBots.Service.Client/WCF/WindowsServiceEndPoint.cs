using OpenBots.Agent.Core.Infrastructure;
using OpenBots.Agent.Core.Model;
using OpenBots.Service.Client.Manager;
using OpenBots.Service.Client.Manager.Execution;
using OpenBots.Service.Client.Server;
using System;
using System.Threading.Tasks;

namespace OpenBots.Service.Client
{
    public class WindowsServiceEndPoint : IWindowsServiceEndPoint
    {
        
        public ServerResponse ConnectToServer(ServerConnectionSettings settings)
        {
            return HttpServerClient.Instance.Connect(settings);
        }

        public ServerResponse DisconnectFromServer(ServerConnectionSettings settings)
        {
            return HttpServerClient.Instance.Disconnect(settings);
        }

        public async Task<bool> ExecuteAttendedTask(string projectPath, ServerConnectionSettings settings)
        {
            var task = Task.Factory.StartNew(()=>
            {
                return AttendedExecutionManager.Instance.ExecuteTask(projectPath, settings);
            });
            return await task.ConfigureAwait(false);
        }

        public ServerConnectionSettings GetConnectionSettings()
        {
            return ConnectionSettingsManager.Instance?.ConnectionSettings ?? null;
        }

        public bool IsAlive()
        {
            return ServiceController.Instance.IsServiceAlive;
        }

        public bool IsConnected()
        {
            return ConnectionSettingsManager.Instance?.ConnectionSettings?.ServerConnectionEnabled ?? false;
        }

        public bool IsEngineBusy()
        {
            return ExecutionManager.Instance?.IsEngineBusy ?? false;
        }

        public ServerResponse PingServer(ServerConnectionSettings serverSettings)
        {
            try
            {
                AuthAPIManager.Instance.Initialize(serverSettings);
                var serverIP = AuthAPIManager.Instance.Ping();
                AuthAPIManager.Instance.UnInitialize();

                return new ServerResponse(serverIP);
            }
            catch (Exception ex)
            {
                var errorCode = ex.GetType().GetProperty("ErrorCode")?.GetValue(ex, null)?.ToString() ?? string.Empty;
                var errorMessage = ex.GetType().GetProperty("ErrorContent")?.GetValue(ex, null)?.ToString() ?? ex.Message;

                // Send Response to Agent
                return new ServerResponse(null, errorCode, errorMessage);
            }
        }

        public void SetEnvironmentVariable(string environmentVariable, string settingsFilePath)
        {
            try
            {
                Environment.SetEnvironmentVariable(environmentVariable, settingsFilePath, EnvironmentVariableTarget.Machine);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //public IAsyncResult BeginAsyncAttendedExecution(string projectPath, AsyncCallback callback, object asyncState)
        //{
        //    try
        //    {
        //        AttendedExecutionManager.Instance.ExecuteTask(projectPath);
        //        return new CompletedAsyncResult<bool>(true);
        //    }
        //    catch
        //    {
        //        return new CompletedAsyncResult<bool>(false);
        //    }
        //}
        //public bool EndAsyncAttendedExecution(IAsyncResult res)
        //{
        //    CompletedAsyncResult<bool> result = res as CompletedAsyncResult<bool>;
        //    return result.Data;
        //}
    }
}
