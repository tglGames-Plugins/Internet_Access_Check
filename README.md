# TGL Internet Access Check

A simple internet access check plugin for Unity. We check if we can access internet and the speed of reaching it in kbps.
Add [InternetCheck.cs](./Runtime/Scripts/InternetCheck.cs) to your scene, and it will start monitoring weather you are online.
the variables allow you to decide how often you want to check accessing online.  
- ***timeOutSeconds*** : how much time do we wait for all ping request before they time out
- ***pingFrequencyOnline*** : when you are online, how often should we check for internet connectivity
- ***pingFrequencyOffline*** : when you are offline, how often should we check for internet connectivity - try to keep it less than 'pingFrequencyOnline' as we are offline
- ***waitTimeBeforeDisconnection*** : If we set this, we are giving our system some time to re-connect in the background before we mention that we are offline.
- ***endPointsToTest*** : What end points to test for internet. It is adviced to add your server's ping URL to this and any public urls you want to ping to confirm the access to internet.


Abstract class [InternetCheckListener.cs](./Runtime/Scripts/InternetCheckListener.cs) allows you to add listener to detect status has changed.
- ***StartListening*** : Call this if you are in a phase of your app, where you want to track internet status
- ***StopListening*** : Call this if you are in a phase of your app, where you do not want to track internet status
- ***UpdatedInternetStatus*** : this abstract method can be overritten to use as a source to detect the current status
A sample script 'InternetCheckResult' is available to get an idea. How you deal with current status is up to you.

## How to add this package?
- Open unity package manaegr 
- On top right, there is a button to add a package
- add a git package (from git URL)
- fill the Https link for the package, in this case, 'https://github.com/tglGames-Plugins/Internet_Access_Check.git'
- Add
The package will be added under 'TGL Internet Access Check' in packages, use as needed.