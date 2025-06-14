using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace TGL.InternetCheck
{
    public class NetworkMonitor
    {
        #region Variables
        /// <summary>
        /// The time(in milliseconds) interval, after which we check internet connection again when we were Offline
        /// </summary>
        private readonly int ping_time_when_offline;

        /// <summary>
        /// The time(in milliseconds) interval, after which we check internet connection again when we were Online
        /// </summary>
        private readonly int ping_time_when_online;

        /// <summary>
        /// The maximum time(in seconds), for which all endpoints attempt to connect online
        /// </summary>
        private readonly int connection_check_timeout;

        /// <summary>
        /// endpoints which we attampt to reach out to confirm internet is reachable
        /// </summary>
        public List<string> checkEndPoints;

        private event Action<InternetCheckData> ConnectionStatusChanged;

        private static readonly HttpClient httpClient = new HttpClient();
        private readonly List<Task<InternetCheckData>> endpointsTasks;

        private Task mainMonitoringTask;
        private CancellationTokenSource ctsMainMonitoring;
        private NetworkReachability hardwareReachable;
        private InternetCheckData lastCheckInternetConnected = null, currentInternetStatus;
        private bool isMonitoring;
        #endregion Variables

        #region Constructors
        public NetworkMonitor(int timeout, int pingIntervalOnline, int pingIntervalOffline, List<string> endpointsList, Action<InternetCheckData> OnConnectionStatusChange = null)
        {
            connection_check_timeout = timeout;
            ping_time_when_online = pingIntervalOnline * 1000;
            ping_time_when_offline = pingIntervalOffline * 1000;
            checkEndPoints = endpointsList;
            ConnectionStatusChanged = OnConnectionStatusChange;

            endpointsTasks = new List<Task<InternetCheckData>>(checkEndPoints.Count);
        }
        #endregion Constructors

        #region PublicMethods
        public void StartMonitoring()
        {
            if (isMonitoring)
            {
                Debug.LogWarning($"We are already monitoring, but someone called to start monitoring the internet check");
                return;
            }

            isMonitoring = true;
            ctsMainMonitoring = new CancellationTokenSource();

            if (mainMonitoringTask == null || mainMonitoringTask.IsCompleted)
            {
                mainMonitoringTask = MonitorInternetReachable(ctsMainMonitoring.Token);
            }
            else
            {
                Debug.LogError($"{nameof(mainMonitoringTask)} was made to track this task, it is not null");
            }
        }

        public void StopMonitoring()
        {
            if (!isMonitoring)
            {
                Debug.LogWarning($"We have stopped monitoring, but someone called to stop monitoring the internet check");
                return;
            }

            isMonitoring = false;
            ctsMainMonitoring?.Cancel();
            ctsMainMonitoring?.Dispose();
            ctsMainMonitoring = null;
        }
        #endregion PublicMethods

        #region PrivateMethods
        private async Task MonitorInternetReachable(CancellationToken ctMainMonitoring)
        {
            try
            {
                while (isMonitoring && !ctMainMonitoring.IsCancellationRequested)
                {
                    hardwareReachable = Application.internetReachability;
                    if (hardwareReachable == NetworkReachability.NotReachable)
                    {
                        currentInternetStatus = InternetCheckData.HardwareBlocked;
                    }
                    else
                    {
                        currentInternetStatus = await PerformInternetCheckAsync(ctMainMonitoring);
                    }

                    if (lastCheckInternetConnected == null || currentInternetStatus.isInternetConnected != lastCheckInternetConnected.isInternetConnected)
                    {
                        Debug.Log($"Sending the status, as currently connected internet status is {currentInternetStatus.isInternetConnected}");
                        lastCheckInternetConnected = currentInternetStatus;
                        ConnectionStatusChanged?.Invoke(currentInternetStatus);
                    }
                    else
                    {
                        lastCheckInternetConnected = currentInternetStatus;
                        Debug.Log($"Not sending the status, but currently connected internet status is {currentInternetStatus.isInternetConnected} - [{currentInternetStatus.speedInKbps.ToString("00")} kbps]");
                    }

                    try
                    {
                        await Task.Delay(currentInternetStatus.isInternetConnected ? ping_time_when_online : ping_time_when_offline, ctMainMonitoring);
                    }
                    catch (OperationCanceledException)
                    {
                        Debug.Log("NetworkMonitor: Delay was cancelled. Exiting monitoring loop.");
                        break;
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // This exception is expected when cancellationToken.ThrowIfCancellationRequested() is called
                // or when a Task.Delay with a CancellationToken is cancelled.
                Debug.Log("Internet monitoring task was cancelled.");
            }
            catch (Exception ex)
            {
                // This catches any other unexpected exceptions during the task's execution
                Debug.LogException(ex);
            }
            finally
            {
                Debug.Log("Internet monitoring task finished or exited.");
                isMonitoring = false;
            }
        }

        private async Task<InternetCheckData> PerformInternetCheckAsync(CancellationToken ctMainMonitoring)
        {
            using CancellationTokenSource ctsPingTaskTimeout = CancellationTokenSource.CreateLinkedTokenSource(ctMainMonitoring);
            ctsPingTaskTimeout.CancelAfter(TimeSpan.FromSeconds(connection_check_timeout));
            CancellationToken combinedToken = ctsPingTaskTimeout.Token;

            endpointsTasks.Clear();
            foreach (string url in checkEndPoints)
            {
                endpointsTasks.Add(TryToPingEndPoint(url, connection_check_timeout, combinedToken));
            }

            while (endpointsTasks.Count > 0)
            {
                try
                {
                    Task<InternetCheckData> completedTask = await Task.WhenAny(endpointsTasks);

                    if (combinedToken.IsCancellationRequested)
                    {
                        // Debug.LogWarning($"Connection timeout or external cancel");
                        ctsPingTaskTimeout.Cancel();
                        return InternetCheckData.PingCancelled;
                    }

                    if (completedTask.Result.isInternetConnected)
                    {
                        // stop all other ping requests
                        ctsPingTaskTimeout?.Cancel();
                        return completedTask.Result;
                    }
                    endpointsTasks.Remove(completedTask);
                }
                catch (OperationCanceledException)
                {
                    ctsPingTaskTimeout?.Cancel();
                    return InternetCheckData.PingCancelled;
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                    ctsPingTaskTimeout?.Cancel();
                    return InternetCheckData.PingFailed;
                }
            }
            return InternetCheckData.PingFailed;
        }

        private async Task<InternetCheckData> TryToPingEndPoint(string url, int endPointTimeout, CancellationToken ctPing)
        {
            if (ctPing.IsCancellationRequested)
            {
                return InternetCheckData.CheckCancelled;
            }

            try
            {
                using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Head, url);
                // using HttpClientHandler handler = new HttpClientHandler();
                // using HttpClient client = new HttpClient(handler);

                // set timeout and send request
                // httpClient.Timeout = TimeSpan.FromSeconds(endPointTimeout);

                System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();
                HttpResponseMessage responseMsg = await httpClient.SendAsync(request, ctPing);
                stopwatch.Stop();

                long headersSize = responseMsg.Headers.ToString().Length + responseMsg.Content.Headers.ToString().Length;
                double elapsedSeconds = stopwatch.Elapsed.TotalSeconds;

                double kbps = 0;
                if (elapsedSeconds > 0)
                {
                    kbps = (headersSize * 8) / elapsedSeconds / 1024;
                }

                return new InternetCheckData(responseMsg.IsSuccessStatusCode, "Connected", (float)Math.Round(kbps, 1));
            }
            catch (OperationCanceledException)
            {
                // Debug.LogWarning($"ping operation cancelled");
                return InternetCheckData.PingCancelled;
            }
            catch (TimeoutException)
            {
                // Debug.LogWarning($"ping request timed out");
                return InternetCheckData.PingTimedOut;
            }
            catch (HttpRequestException httpEx)
            {
                // Debug.LogWarning($"HTTP error reaching [{url}]: {httpEx}");
                return new InternetCheckData(false, httpEx.Message, 0);
            }
            catch (Exception ex)
            {
                // Debug.LogWarning($"Error checking [{url}]: {ex.Message}");
                return new InternetCheckData(false, ex.Message, 0);
            }
        }
        #endregion PrivateMethods
    }
}