using UnityEngine;

namespace TGL.InternetCheck.Sample
{
    public class InternetCheckResult : MonoBehaviour, IInternetCheckResult
    {
        private void Start()
        {
            ((IInternetCheckResult)this).StartListening();
        }

        void IInternetCheckResult.StartListening()
        {
            InternetCheck.InternetStatusChanged += UpdatedInternetStatus;
        }
        
        void IInternetCheckResult.StopListening()
        {
            InternetCheck.InternetStatusChanged -= UpdatedInternetStatus;
        }

        public void UpdatedInternetStatus(InternetCheckData internetReachable)
        {
            Debug.Log($"[Internet Access] - status updated - ::[{internetReachable.speedInKbps.ToString("00")} kbps]:: {internetReachable.isInternetConnected} - {internetReachable.connectMsg}");
        }

        private void OnDestroy()
        {
            ((IInternetCheckResult)this).StopListening();
        }
    }
}