
namespace TGL.InternetCheck
{
    public interface IInternetCheckResult
    {
        void StartListening();

        void StopListening();


        void UpdatedInternetStatus(InternetCheckData internetReachable);
    }
}