# MapChange
On-demand map change plugin for 2v2, 5v5 and 8v8

# Warning

This plugin is not made for public servers.

# Basic commands

/map - takes 3 arguments - mapName, gameMode. Use variables from commands below - changes the map. </br>
/listmaps - lists all map names that can be used in the map/changemap command. </br>
/gamemodes - lists all gamemodes that can be used in the map/changemap command. </br>
/fullgamemodes - full name of gamemodes, for explanation purposes. </br>
/frestart - restarts the round. Requires MapFunctionPriviliges to use it. </br>

# How to install

Local procon layer:
1. Go to releases and download the zip.
2. Extract the MapChange.cs file in (wherever u have procon installed)Procon\Plugins\BF4.

Procon layer hosted by gportal:
1. Go to the server page and find procon layer page.
2. Disable the procon layer.
3. Go to the FTP page and use the credentials from there to connect through any ftp client (example: filezilla).
4. Extract the MapChange.cs file into Plugins\BF4.
5. Start the procon layer.
6. To connect use:
   - IP Address
   - Port that is listed UNDER the IP address (do not use the ip listed next to the port)
   - Username and password as listed.
7. Enable the plugin in parent layer control.

# How do i give MapFunctionPriviliges to user:

1. Open procon, login into procon layer of the server. 
2. Go to the account tab.
3. Manage accounts -> Create new account
4. In username USE your current username that you use ingame
5. Click on the account you just created.
6. Set privilges on all server layers
7. Find "Server state" section -> change current map functions.
