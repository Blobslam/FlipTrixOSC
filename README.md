# FlipTrixOSC

FlipTrixOSC is a simple application that allows you to send spotify metadata to a specified osc endpoint.
You can easily adjust this endpoint in the `appsettings.json` file.

The metadata can contain, but is not limited to track data, artist names or more.
Please adjust the OSCSender class for you needs.

For sending the OSC Packages, FlipTrixOSC uses SharpOSC https://github.com/ValdemarOrn/SharpOSC<br/>
For accessing the SpotifyAPI https://github.com/JohnnyCrazy/SpotifyAPI-NET

**Getting started**
1. Create an application in the Spotify Developer Dashboard https://developer.spotify.com/dashboard/
2. Go to the application settings and set the callback address to http://localhost:5000/callback to ensure authorization with your local spotify client.
3. Insert your client secret and client id from your created spotify application in the appsettings.json file
4. Run the program

Building the program from source:

1. Clone the repoistory
2. Restore the project
3. Add an assembly reference to [SharpOSC](https://github.com/ValdemarOrn/SharpOSC) DLL if not already exist
4. Build the program 
