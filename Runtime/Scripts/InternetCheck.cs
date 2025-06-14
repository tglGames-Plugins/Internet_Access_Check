using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TGL.InternetCheck
{
    public class InternetCheck : MonoBehaviour
    {
        /// <summary>
        /// The maximum time(in seconds), for which all endpoints attempt to connect online
        /// </summary>
        [Tooltip("The maximum time(in seconds), for which all endpoints attempt to connect online")]
        [Range(1, 10)] public int timeOutSeconds = 8;

        /// <summary>
        /// The time(in seconds), after which we re-check the internet availiblity status when online
        /// </summary>
        [Tooltip("The time(in seconds), after which we re-check the internet availiblity status when online")]
        [Range(1, 10)] public int pingFrequencyOnline = 5;

        /// <summary>
        /// The time(in seconds), after which we re-check the internet availiblity status when offline
        /// </summary>
        [Tooltip("The time(in seconds), after which we re-check the internet availiblity status when offline")]
        [Range(1, 10)] public int pingFrequencyOffline = 1;

        /// <summary>
        /// The time(in seconds), after which we inform the system we are offline
        /// </summary>
        [Tooltip("The time(in seconds), after which we inform the system we are offline")]
        [Range(0, 10)] public int waitTimeBeforeDisconnection = 10;

        /// <summary>
        /// endpoints which we attampt to reach out to confirm internet is reachable
        /// </summary>
        [Tooltip("endpoints which we attampt to reach out to confirm internet is available")]
        public List<string> endPointsToTest = new List<string>
        {
            "https://www.cloudflare.com",
            "https://captive.apple.com",
            "http://google.com/generate_204",
            "http://www.msftncsi.com/ncsi.txt"
        };

        private NetworkMonitor networkMonitor;
        private Coroutine internetCheckRoutine;

        public static Action<InternetCheckData> InternetStatusChanged;
        private bool lastCheckStatus;

        private void Awake()
        {
            networkMonitor = new NetworkMonitor(timeOutSeconds, pingFrequencyOnline, pingFrequencyOffline, endPointsToTest, TaskUpdatesStatus);
            StartMonitoring();
        }

        private void StartMonitoring()
        {
            networkMonitor?.StartMonitoring();
        }

        private void TaskUpdatesStatus(InternetCheckData internetCheckData)
        {
            if (lastCheckStatus != internetCheckData.isInternetConnected)
            {
                if (internetCheckData.isInternetConnected)
                {
                    lastCheckStatus = internetCheckData.isInternetConnected;
                    if (internetCheckRoutine != null)
                    {
                        StopCoroutine(internetCheckRoutine);
                        internetCheckRoutine = null;
                    }
                    InternetStatusChanged?.Invoke(internetCheckData);
                }
                else if (internetCheckRoutine == null)
                {
                    internetCheckRoutine = StartCoroutine(BadInternectConnectionRoutine());
                }
            }
        }

        private IEnumerator BadInternectConnectionRoutine()
        {
            if (waitTimeBeforeDisconnection != 0)
            {
                yield return new WaitForSeconds(waitTimeBeforeDisconnection);
            }
            InternetStatusChanged?.Invoke(new InternetCheckData(false, $"could not connect - from [{nameof(BadInternectConnectionRoutine)}]", 0));
            lastCheckStatus = false;
        }

        private void StopMonitoring()
        {
            networkMonitor?.StopMonitoring();
        }

        void OnDestroy()
        {
            StopMonitoring();
        }
    }
}