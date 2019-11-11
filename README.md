# OBSCommand is a command line tool to control OBS Studio via obs-websocket-dotnet

Usage:
OBSCommand.exe /server=127.0.0.1:4444 /password=xxxx /delay=0.5 /setdelay=0.05 /profile=myprofile /scene=myscene /hidesource=myscene/mysource /togglesource=myscene/mysource /showsource=myscene/mysource /toggleaudio=myaudio /mute=myaudio /unmute=myaudio /setvolume=mysource,volume,delay /startstream /stopstream /startrecording /stoprecording /command=mycommand,myparam1=myvalue1...


Note: If Server is omitted, default 127.0.0.1:4444 will be used.
Use quotes if your item name includes spaces.
Password can be empty if no password is set in OBS Studio.
You can use the same option multiple times.
If you use Server and Password, those must be the first 2 options!


Examples:
OBSCommand.exe /scene=myscene
OBSCommand.exe /toggleaudio="Desktop Audio"
OBSCommand.exe /mute=myAudioSource
OBSCommand.exe /unmute="my Audio Source"
OBSCommand.exe /setvolume=Mic/Aux,0,50
OBSCommand.exe /setvolume=Mic/Aux,100
OBSCommand.exe /stopstream
OBSCommand.exe /profile=myprofile /scene=myscene /showsource=mysource
OBSCommand.exe /showsource=mysource
OBSCommand.exe /hidesource=myscene/mysource
OBSCommand.exe /togglesource=myscene/mysource
OBSCommand.exe /showsource="my scene" "my source"
OBSCommand.exe /command=SaveReplayBuffer
OBSCommand.exe /command=TakeSourceScreenshot,sourceName=MyScene,PictureFormat=png,saveToFilePath=C:\OBSTest.png
OBSCommand.exe /scene=mysource1 /delay=1.555 /scene=mysource2
OBSCommand.exe /setdelay=1.555 /scene=mysource1 /scene=mysource2

Options:
--------

/server=127.0.0.1:4444            define server address and port
  Note: If Server is omitted, default 127.0.0.1:4444 will be used.
/password=xxxx                    define password (can be omitted)
/delay=n.nnn                      delay in seconds (0.001 = 1 ms)
/setdelay=n.nnn                   global delay in seconds (0.001 = 1 ms)
                                  (set it to 0 to cancel it)
/profile=myprofile                switch to profile "myprofile"
/scene=myscene                    switch to scene "myscene"
/hidesource=myscene/mysource      hide source "scene/mysource"
/showsource=myscene/mysource      show source "scene/mysource"
/togglesource=myscene/mysource    toggle source "scene/mysource"
  Note:  if scene is omitted, current scene is used
/toggleaudio=myaudio              toggle mute from audio source "myaudio"
/mute=myaudio                     mute audio source "myaudio"
/unmute=myaudio                   unmute audio source "myaudio"
/setvolume=myaudio,volume,delay   set volume of audio source "myaudio"
                                  volume is 0-100, delay is in milliseconds
                                  between steps (min. 10, max. 1000) for fading
  Note:  if delay is omitted volume is set instant
/startstream                      starts streaming
/stopstream                       stop streaming
/startrecording                   starts recording
/stoprecording                    stops recording

General User Command syntax:
/command=mycommand,myparam1=myvalue1,myparam2=myvalue2...
                                  issues user command,parameter (optional)
                                  (see list of commands below)

List of commands:
https://github.com/Palakis/obs-websocket/blob/4.x-current/docs/generated/protocol.md