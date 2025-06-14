
namespace TGL.InternetCheck
{
    public interface IInternetCheckResult
    {
        virtual void StartListening()
        {
            InternetCheck.InternetStatusChanged += UpdatedInternetStatus;
        }

        virtual void StopListening()
        {
            InternetCheck.InternetStatusChanged -= UpdatedInternetStatus;
        }


        void UpdatedInternetStatus(InternetCheckData internetReachable);
    }
}