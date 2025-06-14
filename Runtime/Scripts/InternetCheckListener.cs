using UnityEngine;

namespace TGL.InternetCheck
{
	public abstract class InternetCheckListener : MonoBehaviour, IInternetCheckResult
	{
		public virtual void StartListening()
		{
			InternetCheck.InternetStatusChanged += UpdatedInternetStatus;
		}

		public virtual void StopListening()
		{
			InternetCheck.InternetStatusChanged -= UpdatedInternetStatus;
		}

		public abstract void UpdatedInternetStatus(InternetCheckData internetReachable);
	}
}