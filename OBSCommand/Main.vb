Imports System.IO
Imports System.Text
Imports OBSWebsocketDotNet
Imports Newtonsoft.Json.Linq

Module Main

    Private _obs As OBSWebsocket

    Sub Main()

        'AddHandler _obs.EventReceived, AddressOf EventReceived

        Dim args As String() = Environment.GetCommandLineArgs
        Dim password As String = ""
        Dim server As String = "ws://127.0.0.1:4444"

        Dim profile As String = ""
        Dim scene As String = ""
        Dim hidesource As String = ""
        Dim showsource As String = ""
        Dim togglesource As String = ""
        Dim toggleaudio As String = ""
        Dim mute As String = ""
        Dim unmute As String = ""
        Dim fadeopacity As String = ""
        Dim setvolume As String = ""
        Dim stopstream As Boolean = False
        Dim startstream As Boolean = False
        Dim startrecording As Boolean = False
        Dim stoprecording As Boolean = False
        Dim command As String = ""
        Dim delay As String = ""
        Dim setdelay As Double

        Dim errormessage As String = ""

        If args.Count = 1 Then
            PrintUsage()
            End
        End If

        Dim myout As TextWriter = Console.Out
        Dim builder As StringBuilder = New StringBuilder()
        Dim writer As TextWriter = New StringWriter(builder)
        Console.SetOut(writer)

        Dim isInitialized As Boolean = False
        Dim skipfirst As Boolean = False
        Dim argsindex As Integer

        For Each arg As String In args
            argsindex += 1
            If skipfirst = False Then
                skipfirst = True
                Continue For
            End If

            ' Clear variables
            profile = ""
            scene = ""
            command = ""
            hidesource = ""
            showsource = ""
            togglesource = ""
            toggleaudio = ""
            mute = ""
            unmute = ""
            setvolume = ""
            fadeopacity = ""
            stopstream = False
            startstream = False
            startrecording = False
            stoprecording = False
            delay = ""

            If arg = "?" Or arg = "/?" Or arg = "-?" Or arg = "help" Or arg = "/help" Or arg = "-help" Then
                PrintUsage()
                End
            End If

            If arg.StartsWith("/server=") Then
                server = "ws://" & arg.Replace("/server=", "")
                Continue For ' get credentials first before trying to connect!
            End If
            If arg.StartsWith("/password=") Then
                password = arg.Replace("/password=", "")
                Continue For ' get credentials first before trying to connect!
            End If

            If arg.StartsWith("/setdelay=") Then
                Dim tmp As String = arg.Replace("/setdelay=", "")
                If Not IsNumeric(tmp) Then
                    Console.SetOut(myout)
                    Console.WriteLine("Error: setdelay is not numeric")
                    Continue For
                Else
                    setdelay = Double.Parse(tmp, System.Globalization.CultureInfo.InvariantCulture)
                    Continue For
                End If
            End If

            If arg.StartsWith("/delay=") Then
                Dim tmp As String = arg.Replace("/delay=", "")
                If Not IsNumeric(tmp) Then
                    Console.SetOut(myout)
                    Console.WriteLine("Error: delay is not numeric")
                    Continue For
                Else
                    Threading.Thread.Sleep(Double.Parse(tmp, System.Globalization.CultureInfo.InvariantCulture) * 1000)
                    Continue For
                End If
            End If

            If arg.StartsWith("/profile=") Then
                profile = arg.Replace("/profile=", "")
            End If
            If arg.StartsWith("/scene=") Then
                scene = arg.Replace("/scene=", "")
            End If
            If arg.StartsWith("/command=") Then
                command = arg.Replace("/command=", "")
            End If
            If arg.StartsWith("/hidesource=") Then
                hidesource = arg.Replace("/hidesource=", "")
            End If
            If arg.StartsWith("/showsource=") Then
                showsource = arg.Replace("/showsource=", "")
            End If
            If arg.StartsWith("/togglesource=") Then
                togglesource = arg.Replace("/togglesource=", "")
            End If
            If arg.StartsWith("/toggleaudio=") Then
                toggleaudio = arg.Replace("/toggleaudio=", "")
            End If
            If arg.StartsWith("/mute=") Then
                mute = arg.Replace("/mute=", "")
            End If
            If arg.StartsWith("/unmute=") Then
                unmute = arg.Replace("/unmute=", "")
            End If
            If arg.StartsWith("/setvolume=") Then
                setvolume = arg.Replace("/setvolume=", "")
            End If
            If arg.StartsWith("/fadeopacity=") Then
                fadeopacity = arg.Replace("/fadeopacity=", "")
            End If
            If arg = "/startstream" Then
                startstream = True
            End If
            If arg = "/stopstream" Then
                stopstream = True
            End If
            If arg = "/startrecording" Then
                startrecording = True
            End If
            If arg = "/stoprecording" Then
                stoprecording = True
            End If
            Try

                If isInitialized = False Then
                    isInitialized = True
                    _obs = New OBSWebsocket()
                    _obs.WSTimeout = New TimeSpan(0, 0, 0, 3)
                    _obs.Connect(server, password)
                End If

                If profile <> "" Then
                    '_obs.SetCurrentProfile(profile)
                    Dim fields As New JObject()
                    fields.Add("profile-name", profile)
                    _obs.SendRequest("SetCurrentProfile", fields)
                End If
                If scene <> "" Then
                    '_obs.SetCurrentScene(scene)
                    Dim fields As New JObject
                    fields.Add("scene-name", scene)
                    Dim response As JObject = _obs.SendRequest("SetCurrentScene", fields)
                End If
                If command <> "" Then
                    Dim myParameter As Object = Nothing

                    Try

                        If command.Contains(",") Then
                            Dim tmp As String() = command.Split(",")

                            'If tmp.Count = 2 Then
                            '    command = tmp(0)
                            '    myParameter = tmp(1)
                            'End If
                            Dim fields As New JObject
                            For a = 1 To tmp.Count - 1
                                Dim tmpsplit As String() = tmp(a).Split("=")
                                If tmpsplit.Count < 2 Then
                                    Console.SetOut(myout)
                                    Console.WriteLine("Error with command """ & command & """: " & "Missing a = in Name=Type")
                                End If
                                'If IsNumeric(tmpsplit(1)) Then
                                '    If tmpsplit(1).Contains(".") Then
                                '        fields.Add(tmpsplit(0), Double.Parse(tmpsplit(1), System.Globalization.CultureInfo.InvariantCulture))
                                '    Else
                                '        fields.Add(tmpsplit(0), CInt(tmpsplit(1)))
                                '    End If
                                'ElseIf tmpsplit(1).ToUpper = "TRUE" Or tmpsplit(1).ToUpper = "FALSE" Then
                                '    fields.Add(tmpsplit(0), CBool(tmpsplit(1)))
                                'Else
                                If tmpsplit.Count > 2 Then
                                    Dim subfield As New JObject
                                    subfield.Add(ConvertToType(tmpsplit(1)), ConvertToType(tmpsplit(2)))
                                    fields.Add(ConvertToType(tmpsplit(0)), subfield)
                                Else
                                    fields.Add(ConvertToType(tmpsplit(0)), ConvertToType(tmpsplit(1)))
                                End If

                                'End If

                            Next
                            Console.SetOut(myout)
                            Console.WriteLine(_obs.SendRequest(tmp(0), fields))

                        Else
                            Console.SetOut(myout)
                            Console.WriteLine(_obs.SendRequest(command))
                        End If

                        'Dim myType As System.Type = GetType(OBSWebsocket)
                        'Dim myInfo As System.Reflection.MethodInfo = myType.GetMethod(command)
                        'If myInfo = Nothing Then
                        '    Console.SetOut(myout)
                        '    Console.WriteLine("Error! Command """ & command & """ not found.")
                        'Else
                        '    Try
                        '        Dim returnvalue As String = myInfo.Invoke(_obs, myParameter)

                        '        If returnvalue <> "" Then
                        '            Console.SetOut(myout)
                        '            Console.WriteLine(returnvalue)
                        '        End If
                    Catch ex As Exception
                        Console.SetOut(myout)
                        Console.WriteLine("Error with command """ & command & """: " & ex.Message.ToString())
                    End Try
                    'End If
                End If
                If hidesource <> "" Then
                    If hidesource.Contains("/") Then
                        Dim tmp As String() = hidesource.Split("/")

                        ' scene/source
                        If tmp.Count = 2 Then
                            '_obs.SetSourceRender(tmp(1), False, tmp(0))
                            Dim fields As New JObject
                            fields.Add("source", tmp(1))
                            fields.Add("render", False)
                            fields.Add("scene-name", tmp(0))
                            _obs.SendRequest("SetSourceRender", fields)
                        End If
                    Else
                        '_obs.SetSourceRender(hidesource, False)
                        Dim fields As New JObject
                        fields.Add("source", hidesource)
                        fields.Add("render", False)
                        _obs.SendRequest("SetSourceRender", fields)
                    End If
                End If
                If showsource <> "" Then
                    If showsource.Contains("/") Then
                        Dim tmp As String() = showsource.Split("/")

                        ' scene/source
                        If tmp.Count = 2 Then
                            '_obs.SetSourceRender(tmp(1), True, tmp(0))
                            Dim fields As New JObject
                            fields.Add("source", tmp(1))
                            fields.Add("render", True)
                            fields.Add("scene-name", tmp(0))
                            _obs.SendRequest("SetSourceRender", fields)
                        End If
                    Else
                        '_obs.SetSourceRender(showsource, True)
                        Dim fields As New JObject
                        fields.Add("source", showsource)
                        fields.Add("render", True)
                        _obs.SendRequest("SetSourceRender", fields)
                    End If

                End If
                If togglesource <> "" Then
                    If togglesource.Contains("/") Then
                        Dim tmp As String() = togglesource.Split("/")

                        ' scene/source
                        If tmp.Count = 2 Then OBSToggleSource(tmp(1), tmp(0))
                    Else
                        OBSToggleSource(togglesource)
                    End If
                End If
                If toggleaudio <> "" Then
                    '_obs.ToggleMute(toggleaudio)
                    Dim fields As New JObject
                    fields.Add("source", toggleaudio)
                    _obs.SendRequest("ToggleMute", fields)
                End If
                If mute <> "" Then
                    '_obs.SetMute(mute, True)
                    Dim fields As New JObject
                    fields.Add("source", mute)
                    fields.Add("mute", True)
                    _obs.SendRequest("SetMute", fields)
                End If
                If unmute <> "" Then
                    '_obs.SetMute(unmute, False)
                    Dim fields As New JObject
                    fields.Add("source", unmute)
                    fields.Add("mute", False)
                    _obs.SendRequest("SetMute", fields)
                End If

                If fadeopacity <> "" Then
                    ' source,filtername,startopacity,endopacity,[fadedelay],[fadestep]
                    Dim tmp As String() = fadeopacity.Split(",")
                    If tmp.Count < 4 Then
                        Throw New Exception("/fadeopacity is missing required parameters!")
                    End If
                    If Not IsNumeric(tmp(2)) Or Not IsNumeric(tmp(3)) Then Throw New Exception("Opacity value is not nummeric (0-100)!")
                    If tmp.Count = 4 Then
                        Opacityfade(tmp(0), tmp(1), tmp(2), tmp(3))
                    ElseIf tmp.Count = 5 Then
                        If Not IsNumeric(tmp(4)) Then Throw New Exception("Delay value is not nummeric (0-x)!")
                        Opacityfade(tmp(0), tmp(1), tmp(2), tmp(3), tmp(4))
                    ElseIf tmp.Count = 6 Then
                        If Not IsNumeric(tmp(4)) Then Throw New Exception("Delay value is not nummeric (0-x)!")
                        If Not IsNumeric(tmp(5)) Then Throw New Exception("Fadestep value is not nummeric (1-x)!")
                        Opacityfade(tmp(0), tmp(1), tmp(2), tmp(3), tmp(4), tmp(5))
                    End If
                End If

                If setvolume <> "" Then
                    ' source/volume,[delay]
                    Dim tmp As String() = setvolume.Split(",")
                    If Not IsNumeric(tmp(1)) Then Throw New Exception("Volume value is not nummeric (0-100)!")
                    If tmp.Count = 2 Then
                        OBSSetVolume(tmp(0), tmp(1))
                    ElseIf tmp.Count = 3 Then
                        If Not IsNumeric(tmp(2)) Then Throw New Exception("Delay value is not nummeric (0-x)!")
                        OBSSetVolume(tmp(0), tmp(1), tmp(2))
                    End If
                End If
                If startstream = True Then
                    '_obs.StartStreaming()
                    _obs.SendRequest("StartStreaming")

                End If
                If stopstream = True Then
                    '_obs.StopStreaming()
                    _obs.SendRequest("StopStreaming")
                End If
                If startrecording = True Then
                    '_obs.StartRecording()
                    _obs.SendRequest("StartRecording")
                End If
                If stoprecording = True Then
                    '_obs.StopRecording()
                    _obs.SendRequest("StopRecording")
                End If

                If setdelay > 0 And argsindex < args.Count And argsindex > 1 Then
                    Threading.Thread.Sleep(setdelay * 1000)
                End If

            Catch ex As Exception
                errormessage = ex.Message.ToString()
            End Try
        Next
        Try
            _obs.Disconnect()

        Catch ex As Exception

        End Try

        Console.SetOut(myout)
        If errormessage = "" Then
            Console.WriteLine("Ok")
        Else
            Console.WriteLine("Error: " & errormessage)
        End If

    End Sub

    Private Function ConvertToType(ByVal text As String) As JToken
        If IsNumeric(text) Then
            If text.Contains(".") Then
                Return Double.Parse(text, System.Globalization.CultureInfo.InvariantCulture)
            Else
                Return CInt(text)
            End If
        ElseIf text.ToUpper = "TRUE" Or text.ToUpper = "FALSE" Then
            Return CBool(text)
        Else
            Return text
        End If
    End Function

    Private Sub OBSToggleSource(ByVal source As String, Optional ByVal sceneName As String = "")

        Dim fields As New JObject
        If sceneName <> "" Then fields.Add("scene-name", sceneName)
        fields.Add("item", source)
        Dim response As JObject = _obs.SendRequest("GetSceneItemProperties", fields)

        fields = New JObject
        If sceneName <> "" Then fields.Add("scene-name", sceneName)
        fields.Add("item", source)
        fields.Add("visible", Not Convert.ToBoolean(response.GetValue("visible")))
        _obs.SendRequest("SetSceneItemProperties", fields)

    End Sub

    Private Sub Opacityfade(ByVal source As String, ByVal filtername As String, ByVal fadestart As Integer, ByVal fadeend As Integer, Optional ByVal delay As Integer = 0, Optional ByVal fadestep As Integer = 1)

        If delay < 5 Then delay = 5
        If delay > 1000 Then delay = 1000
        Dim fields As New JObject

        If fadestart < fadeend Then
            For a As Integer = fadestart To fadeend Step fadestep
                fields = New JObject
                fields.Add("sourceName", source)
                fields.Add("filterName", filtername)
                Dim tmpfield As JObject = New JObject
                tmpfield.Add("opacity", ConvertToType(a))
                fields.Add("filterSettings", tmpfield)
                _obs.SendRequest("SetSourceFilterSettings", fields)
                Threading.Thread.Sleep(delay)
            Next
        ElseIf fadestart > fadeend Then
            For a As Integer = fadestart To fadeend Step -fadestep
                fields = New JObject
                fields.Add("sourceName", source)
                fields.Add("filterName", filtername)
                Dim tmpfield As JObject = New JObject
                tmpfield.Add("opacity", ConvertToType(a))
                fields.Add("filterSettings", tmpfield)
                _obs.SendRequest("SetSourceFilterSettings", fields)
                Threading.Thread.Sleep(delay)
            Next
        End If
    End Sub

    Private Sub OBSSetVolume(ByVal source As String, ByVal volume As Integer, Optional ByVal delay As Integer = 0)
        If delay = 0 Then
            '_obs.SetVolume(source, volume / 100)
            _obs.SendRequest("SetVolume", New JObject(source, volume / 100))

        Else
            If delay < 5 Then delay = 5
            If delay > 1000 Then delay = 1000
            'Dim _VolumeInfo As VolumeInfo = _obs.GetVolume(source)
            Dim fields As New JObject
            fields.Add("source", source)

            Dim _VolumeInfo As JObject = _obs.SendRequest("GetVolume", fields)

            Dim startvolume As Integer = CInt(_VolumeInfo.GetValue("volume")) * 100

            If startvolume = volume Then
                Exit Sub
            ElseIf startvolume < volume Then
                For a = startvolume To volume
                    '_obs.SetVolume(source, (a / 100))
                    fields = New JObject
                    fields.Add("source", source)
                    fields.Add("volume", a / 100)
                    _obs.SendRequest("SetVolume", fields)
                    Threading.Thread.Sleep(delay)
                Next
            ElseIf startvolume > volume Then
                For a = startvolume To volume Step -1
                    '_obs.SetVolume(source, (a / 100))
                    fields = New JObject
                    fields.Add("source", source)
                    fields.Add("volume", a / 100)
                    _obs.SendRequest("SetVolume", fields)
                    Threading.Thread.Sleep(delay)
                Next
            End If
        End If
    End Sub

    Private Sub PrintUsage()
        Dim out As List(Of String) = New List(Of String)

        out.Add("OBSCommand v1.5.0 ©2018-2020 by FSC-SOFT (http://www.VoiceMacro.net)")
        out.Add(vbCrLf)
        out.Add("Usage:")
        out.Add("------")
        out.Add("OBSCommand.exe /server=127.0.0.1:4444 /password=xxxx /delay=0.5 /setdelay=0.05 /profile=myprofile /scene=myscene /hidesource=myscene/mysource /showsource=myscene/mysource /togglesource=myscene/mysource /toggleaudio=myaudio /mute=myaudio /unmute=myaudio /setvolume=mysource,volume,delay /fadeopacity=mysource,myfiltername,startopacity,endopacity,[fadedelay],[fadestep] /startstream /stopstream /startrecording /stoprecording /command=mycommand,myparam1=myvalue1...")
        out.Add(vbCrLf)
        out.Add("Note: If Server is omitted, default 127.0.0.1:4444 will be used.")
        out.Add("Use quotes if your item name includes spaces.")
        out.Add("Password can be empty if no password is set in OBS Studio.")
        out.Add("You can use the same option multiple times.")
        out.Add("If you use Server and Password, those must be the first 2 options!")
        out.Add(vbCrLf)
        out.Add("This tool uses the obs-websocket plugin to talk to OBS Studio:")
        out.Add("https://github.com/Palakis/obs-websocket/releases")
        out.Add(vbCrLf)
        out.Add("Dynamic link libraries used:")
        out.Add("Json.NET ©2008 by James Newton-King")
        out.Add("websocket-sharp ©2010-2016 by sta.blockhead")
        out.Add("obs-websocket-dotnet ©2017 by Stéphane Lepin.")
        out.Add(vbCrLf)
        out.Add("Examples:")
        out.Add("---------")
        out.Add("OBSCommand.exe /scene=myscene")
        out.Add("OBSCommand.exe /toggleaudio=""Desktop Audio""")
        out.Add("OBSCommand.exe /mute=myAudioSource")
        out.Add("OBSCommand.exe /unmute=""my Audio Source""")
        out.Add("OBSCommand.exe /setvolume=Mic/Aux,0,50")
        out.Add("OBSCommand.exe /setvolume=Mic/Aux,100")
        out.Add("OBSCommand.exe /fadeopacity=Mysource,myfiltername,0,100,5,2")
        out.Add("OBSCommand.exe /stopstream")
        out.Add("OBSCommand.exe /profile=myprofile /scene=myscene /showsource=mysource")
        out.Add("OBSCommand.exe /showsource=mysource")
        out.Add("OBSCommand.exe /hidesource=myscene/mysource")
        out.Add("OBSCommand.exe /togglesource=myscene/mysource")
        out.Add("OBSCommand.exe /showsource=""my scene""/""my source""")
        out.Add("OBSCommand.exe /command=SaveReplayBuffer")
        out.Add("OBSCommand.exe /command=TakeSourceScreenshot,sourceName=MyScene,PictureFormat=png,saveToFilePath=C:\OBSTest.png")
        out.Add("OBSCommand.exe /command=SetSourceFilterSettings,sourceName=""Color Correction"",filterName=Opacity,filterSettings=opacity=10")
        out.Add("OBSCommand.exe /scene=mysource1 /delay=1.555 /scene=mysource2")
        out.Add("OBSCommand.exe /setdelay=1.555 /scene=mysource1 /scene=mysource2")
        out.Add(vbCrLf)
        out.Add("Options:")
        out.Add("--------")
        out.Add("/server=127.0.0.1:4444            define server address and port")
        out.Add("  Note: If Server is omitted, default 127.0.0.1:4444 will be used.")
        out.Add("/password=xxxx                    define password (can be omitted)")
        out.Add("/delay=n.nnn                      delay in seconds (0.001 = 1 ms)")
        out.Add("/setdelay=n.nnn                   global delay in seconds (0.001 = 1 ms)")
        out.Add("                                  (set it to 0 to cancel it)")
        out.Add("/profile=myprofile                switch to profile ""myprofile""")
        out.Add("/scene=myscene                    switch to scene ""myscene""")
        out.Add("/hidesource=myscene/mysource      hide source ""scene/mysource""")
        out.Add("/showsource=myscene/mysource      show source ""scene/mysource""")
        out.Add("/togglesource=myscene/mysource    toggle source ""scene/mysource""")
        out.Add("  Note:  if scene is omitted, current scene is used")
        out.Add("/toggleaudio=myaudio              toggle mute from audio source ""myaudio""")
        out.Add("/mute=myaudio                     mute audio source ""myaudio""")
        out.Add("/unmute=myaudio                   unmute audio source ""myaudio""")
        out.Add("/setvolume=myaudio,volume,delay   set volume of audio source ""myaudio""")
        out.Add("                                  volume is 0-100, delay is in milliseconds")
        out.Add("                                  between steps (min. 10, max. 1000) for fading")
        out.Add("  Note:  if delay is omitted volume is set instant")
        out.Add("/fadeopacity=mysource,myfiltername,startopacity,endopacity,[fadedelay],[fadestep]")
        out.Add("                                  start/end opacity is 0-100, 0=fully transparent")
        out.Add("                                  delay is in milliseconds, step 0-100")
        out.Add("/startstream                      starts streaming")
        out.Add("/stopstream                       stop streaming")
        out.Add("/startrecording                   starts recording")
        out.Add("/stoprecording                    stops recording")
        out.Add("")
        out.Add("General User Command syntax:")
        out.Add("/command=mycommand,myparam1=myvalue1,myparam2=myvalue2...")
        out.Add("                                  issues user command,parameter(s) (optional)")
        out.Add("/command=mycommand,myparam1=myvalue1,myparam2=myvalue2,myparam3=mysubparam=mysubparamvalue")
        out.Add("                                  issues user command,parameters and sub-parameters")
        out.Add("")
        out.Add("A full list of commands is available here https://github.com/Palakis/obs-websocket/blob/4.x-current/docs/generated/protocol.md")
        out.Add("")

        Dim i As Integer = 0
        Dim z As Integer = 0

        Do While True

            Console.WriteLine(out(i))
            If z = Console.WindowHeight - 6 Then
                Console.Write("Press any key to continue...")
                Console.ReadKey()
                ClearCurrentConsoleLine()
                z = 0
            End If
            i += 1
            z += 1
            If i >= out.Count Then Exit Do

            If out(i).Length > Console.WindowWidth Then
                z += 1
            End If
            If out(i).Length > Console.WindowWidth * 2 Then
                z += 1
            End If
        Loop

    End Sub


    Public Sub ClearCurrentConsoleLine()
        Dim currentLineCursor As Integer = Console.CursorTop
        Console.SetCursorPosition(0, Console.CursorTop)
        Console.Write(New String(" "c, Console.WindowWidth))
        Console.SetCursorPosition(0, currentLineCursor)
    End Sub
End Module
