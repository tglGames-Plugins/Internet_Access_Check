using UnityEngine;

public class InternetCheckData
{
    public bool isInternetConnected;
    public string connectMsg;
    public float speedInKbps;

    public InternetCheckData(bool isConnected, string connectionMsg, float internetSpeed)
    {
        isInternetConnected = isConnected;
        connectMsg = connectionMsg;
        speedInKbps = internetSpeed;
    }

    public static readonly InternetCheckData HardwareBlocked =
        new InternetCheckData(false, "Hardware setting does not allow us to connect to internet", 0);

    public static readonly InternetCheckData CheckCancelled =
        new InternetCheckData(false, "cancelled the internet check", 0);
        
    public static readonly InternetCheckData PingCancelled =
        new InternetCheckData(false, "cancelled the Ping request", 0);
        
    public static readonly InternetCheckData PingFailed =
        new InternetCheckData(false, "all Ping requests failed", 0);
        
    public static readonly InternetCheckData PingTimedOut =
        new InternetCheckData(false, "The Ping request timed out", 0);
}
