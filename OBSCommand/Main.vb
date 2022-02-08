Imports System.IO
Imports System.Text
Imports OBSWebsocketDotNet
Imports Newtonsoft.Json.Linq
Imports System.Text.RegularExpressions

Module Main

    Private _obs As OBSWebsocket
    Dim myout As TextWriter = Console.Out

    Sub Main()

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
        Dim slidesetting As String = ""
        Dim slideasync As String = ""
        Dim setvolume As String = ""
        Dim stopstream As Boolean = False
        Dim startstream As Boolean = False
        Dim startrecording As Boolean = False
        Dim stoprecording As Boolean = False
        Dim sendjson As String = False
        Dim command As String = ""
        Dim setdelay As Double

        Dim errormessage As String = ""

        If args.Count = 1 Then
            PrintUsage()
            End
        End If

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
            sendjson = ""
            hidesource = ""
            showsource = ""
            togglesource = ""
            toggleaudio = ""
            mute = ""
            unmute = ""
            setvolume = ""
            fadeopacity = ""
            slidesetting = ""
            slideasync = ""
            stopstream = False
            startstream = False
            startrecording = False
            stoprecording = False

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
                tmp = tmp.Replace(",", ".")
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
                tmp = tmp.Replace(",", ".")
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
            If arg.StartsWith("/sendjson=") Then
                sendjson = arg.Replace("/sendjson=", "")
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
            If arg.StartsWith("/slidesetting=") Then
                slidesetting = arg.Replace("/slidesetting=", "")
            End If
            If arg.StartsWith("/slideasync=") Then
                slideasync = arg.Replace("/slideasync=", "")
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
                    Dim fields As New JObject()
                    fields.Add("profile-name", profile)
                    _obs.SendRequest("SetCurrentProfile", fields)
                End If

                If scene <> "" Then
                    Dim fields As New JObject
                    fields.Add("scene-name", scene)
                    Dim response As JObject = _obs.SendRequest("SetCurrentScene", fields)
                End If

                ' sendjson
                If sendjson <> "" Then
                    Dim json As New JObject
                    'sendjson = """ReorderSceneItems={'scene': 'Game', 'items': [{'name': 'Image'}, {'name': 'Spielaufnahme'}]}"""
                    If Not sendjson.Contains("=") Then
                        errormessage = "sendjson missing ""="" after command"
                        Exit For
                    End If
                    Dim tmp As String() = {"", ""}
                    Try
                        tmp(0) = sendjson.Substring(0, sendjson.IndexOf("="))
                        tmp(1) = sendjson.Substring(sendjson.IndexOf("=") + 1)
                        tmp(1) = tmp(1).Replace("'", Chr(34))
                        json = JObject.Parse(tmp(1))
                        Console.WriteLine(_obs.SendRequest(tmp(0), json))
                    Catch ex As Exception
                        errormessage = "sendjson error:" & vbCrLf & ex.Message.ToString()
                    End Try

                End If

                If command <> "" Then
                    command = command.Replace("'", """")

                    Dim myParameter As Object = Nothing

                    Try

                        If command.Contains(",") Then
                            Dim tmp As String() = command.Split(",")

                            Dim fields As New JObject
                            For a = 1 To tmp.Count - 1
                                Dim tmpsplit As String() = SplitWhilePreservingQuotedValues(tmp(a), "=", True)
                                If tmpsplit.Count < 2 Then
                                    Console.SetOut(myout)
                                    Console.WriteLine("Error with command """ & command & """: " & "Missing a = in Name=Type")
                                End If

                                If tmpsplit.Count > 2 Then
                                    Dim subfield As New JObject
                                    subfield.Add(ConvertToType(tmpsplit(1)), ConvertToType(tmpsplit(2)))
                                    fields.Add(ConvertToType(tmpsplit(0)), subfield)
                                Else
                                    fields.Add(ConvertToType(tmpsplit(0)), ConvertToType(tmpsplit(1)))
                                End If
                            Next

                            Console.SetOut(myout)
                            Console.WriteLine(_obs.SendRequest(tmp(0), fields))

                        Else
                            Console.SetOut(myout)
                            Console.WriteLine(_obs.SendRequest(command))
                        End If


                    Catch ex As Exception
                        Console.SetOut(myout)
                        Console.WriteLine("Error with command """ & command & """: " & ex.Message.ToString())
                    End Try
                End If
                If hidesource <> "" Then
                    If hidesource.Contains("/") Then
                        Dim tmp As String() = hidesource.Split("/")

                        ' scene/source
                        If tmp.Count = 2 Then
                            Dim fields As New JObject
                            fields.Add("source", tmp(1))
                            fields.Add("render", False)
                            fields.Add("scene-name", tmp(0))
                            _obs.SendRequest("SetSourceRender", fields)
                        End If
                    Else
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
                            Dim fields As New JObject
                            fields.Add("source", tmp(1))
                            fields.Add("render", True)
                            fields.Add("scene-name", tmp(0))
                            _obs.SendRequest("SetSourceRender", fields)
                        End If
                    Else
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
                    Dim fields As New JObject
                    fields.Add("source", toggleaudio)
                    _obs.SendRequest("ToggleMute", fields)
                End If
                If mute <> "" Then
                    Dim fields As New JObject
                    fields.Add("source", mute)
                    fields.Add("mute", True)
                    _obs.SendRequest("SetMute", fields)
                End If
                If unmute <> "" Then
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
                    If Not IsNumericOrAsterix(tmp(2)) Or Not IsNumericOrAsterix(tmp(3)) Then Throw New Exception("Opacity start or end value is not nummeric (0-100)!")
                    If tmp.Count = 4 Then
                        Call DoSlideSetting(tmp(0), tmp(1), "opacity", tmp(2), tmp(3))
                    ElseIf tmp.Count = 5 Then
                        If Not IsNumeric(tmp(4)) Then Throw New Exception("Delay value is not nummeric (0-x)!")
                        Call DoSlideSetting(tmp(0), tmp(1), "opacity", tmp(2), tmp(3), tmp(4))
                    ElseIf tmp.Count = 6 Then
                        If Not IsNumeric(tmp(4)) Then Throw New Exception("Delay value is not nummeric (0-x)!")
                        If Not IsNumeric(tmp(5)) Then Throw New Exception("Fadestep value is not nummeric (1-x)!")
                        Call DoSlideSetting(tmp(0), tmp(1), "opacity", tmp(2), tmp(3), tmp(4), tmp(5))
                    End If
                End If

                If slidesetting <> "" Then
                    ' source,filtername,settingname,startvalue,endvalue,[slidedelay],[slidestep]
                    Dim tmp As String() = slidesetting.Split(",")
                    If tmp.Count < 5 Then
                        Throw New Exception("/slideSetting is missing required parameters!")
                    End If
                    If Not IsNumericOrAsterix(tmp(3)) Or Not IsNumericOrAsterix(tmp(4)) Then Throw New Exception("Slide start or end value is not nummeric (0-100)!")
                    If tmp.Count = 5 Then
                        DoSlideSetting(tmp(0), tmp(1), tmp(2), tmp(3), tmp(4))
                    ElseIf tmp.Count = 6 Then
                        If Not IsNumeric(tmp(5)) Then Throw New Exception("Delay value is not nummeric (0-x)!")
                        DoSlideSetting(tmp(0), tmp(1), tmp(2), tmp(3), tmp(4), tmp(5))
                    ElseIf tmp.Count = 7 Then
                        If Not IsNumeric(tmp(5)) Then Throw New Exception("Delay value is not nummeric (0-x)!")
                        If Not IsNumeric(tmp(6)) Then Throw New Exception("Slide step value is not nummeric (1-x)!")
                        DoSlideSetting(tmp(0), tmp(1), tmp(2), tmp(3), tmp(4), tmp(5), tmp(6))
                    End If
                End If

                If slideasync <> "" Then
                    ' source,filtername,settingname,startvalue,endvalue,[slidedelay],[slidestep]
                    Dim tmp As String() = slideasync.Split(",")
                    If tmp.Count < 5 Then
                        Throw New Exception("/slideSetting is missing required parameters!")
                    End If
                    If Not IsNumericOrAsterix(tmp(3)) Or Not IsNumericOrAsterix(tmp(4)) Then Throw New Exception("Slide start or end value is not nummeric (0-100)!")
                    If tmp.Count = 5 Then
                        Dim ExecuteTask As New AsyncSlideSettings(server, password, tmp(0), tmp(1), tmp(2), tmp(3), tmp(4))
                        Dim t As Threading.Thread
                        t = New Threading.Thread(AddressOf ExecuteTask.StartSlide)
                        t.Start()
                    ElseIf tmp.Count = 6 Then
                        If Not IsNumeric(tmp(5)) Then Throw New Exception("Delay value is not nummeric (0-x)!")
                        Dim ExecuteTask As New AsyncSlideSettings(server, password, tmp(0), tmp(1), tmp(2), tmp(3), tmp(4), tmp(5))
                        Dim t As Threading.Thread
                        t = New Threading.Thread(AddressOf ExecuteTask.StartSlide)
                        t.Start()
                    ElseIf tmp.Count = 7 Then
                        If Not IsNumeric(tmp(5)) Then Throw New Exception("Delay value is not nummeric (0-x)!")
                        If Not IsNumeric(tmp(6)) Then Throw New Exception("Slide step value is not nummeric (1-x)!")
                        Dim ExecuteTask As New AsyncSlideSettings(server, password, tmp(0), tmp(1), tmp(2), tmp(3), tmp(4), tmp(5), tmp(6))
                        Dim t As Threading.Thread
                        t = New Threading.Thread(AddressOf ExecuteTask.StartSlide)
                        t.Start()
                    End If
                End If

                If setvolume <> "" Then
                    ' source,volume,[delay],[steps]
                    Dim tmp As String() = setvolume.Split(",")
                    If Not IsNumeric(tmp(1)) Then Throw New Exception("Volume value is not nummeric (0-100)!")
                    If tmp.Count = 2 Then
                        OBSSetVolume(tmp(0), tmp(1))
                    ElseIf tmp.Count = 3 Then
                        If Not IsNumeric(tmp(2)) Then Throw New Exception("Delay value is not nummeric (5-1000)!")
                        OBSSetVolume(tmp(0), tmp(1), tmp(2))
                    ElseIf tmp.Count = 4 Then
                        If Not IsNumeric(tmp(2)) Then Throw New Exception("Delay value is not nummeric (5-1000)!")
                        If Not IsNumeric(tmp(3)) Then Throw New Exception("Step value is not nummeric (1-99)!")
                        OBSSetVolume(tmp(0), tmp(1), tmp(2), tmp(3))
                    End If
                End If
                If startstream = True Then
                    _obs.SendRequest("StartStreaming")

                End If
                If stopstream = True Then
                    _obs.SendRequest("StopStreaming")
                End If
                If startrecording = True Then
                    _obs.SendRequest("StartRecording")
                End If
                If stoprecording = True Then
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

    Private Function IsNumericOrAsterix(ByVal value As String) As Boolean
        If value = "*" Then Return True

        If Not IsNumeric(value) Then Return False

        Return True
    End Function

    Private Function ConvertToType(ByVal text As String) As JToken
        If IsNumeric(text) Then
            If text.Contains(".") Then
                Return Double.Parse(text, System.Globalization.CultureInfo.InvariantCulture)
            Else
                If CLng(text) > Integer.MaxValue Or CLng(text) < Integer.MinValue Then
                    Return CLng(text)
                End If
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

    Private Sub DoSlideSetting(ByVal source As String, ByVal filtername As String, ByVal settingname As String, ByVal fadestart As String, ByVal fadeend As String, Optional ByVal delay As Integer = 0, Optional ByVal fadestep As String = "1")

        If delay < 5 Then delay = 5
        If delay > 1000 Then delay = 1000
        Dim fields As New JObject

        If fadestart = "*" Or fadeend = "*" Then
            Dim tmpfield As JObject = New JObject
            tmpfield.Add("sourceName", source)
            tmpfield.Add("filterName", filtername)
            Dim result As JObject = _obs.SendRequest("GetSourceFilterInfo", tmpfield)
            If fadestart = "*" Then
                Dim tmp As JObject = result.GetValue("settings")
                fadestart = tmp.GetValue(settingname)
            ElseIf fadeend = "*" Then
                Dim tmp As JObject = result.GetValue("settings")
                fadeend = tmp.GetValue(settingname)
            End If
        End If

        Dim haddecimals As Boolean = False

        If fadestep < 1 Then
            haddecimals = True
            fadestart = fadestart * 100
            fadeend = fadeend * 100
            fadestep = fadestep * 100
            delay = delay / 100
        End If

        If fadestart < fadeend Then
            For a As Integer = fadestart To fadeend Step fadestep
                fields = New JObject
                fields.Add("sourceName", source)
                fields.Add("filterName", filtername)
                Dim tmpfield As JObject = New JObject
                If haddecimals = True Then
                    tmpfield.Add(settingname, ConvertToType(a / 100))
                Else
                    tmpfield.Add(settingname, ConvertToType(a))
                End If
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
                If haddecimals = True Then
                    tmpfield.Add(settingname, ConvertToType(a / 100))
                Else
                    tmpfield.Add(settingname, ConvertToType(a))
                End If
                fields.Add("filterSettings", tmpfield)
                _obs.SendRequest("SetSourceFilterSettings", fields)
                Threading.Thread.Sleep(delay)
            Next
        End If
    End Sub

    Private Sub OBSSetVolume(ByVal source As String, ByVal volume As Integer, Optional ByVal delay As Integer = 0, Optional ByVal steps As Integer = 1)
        If steps < 1 Then steps = 1
        If steps > 99 Then steps = 99
        If delay = 0 Then
            Dim molvol As Double = volume ^ 3 / 1000000 ' Convert percent to amplitude/mul (approximate, mul is non-linear)
            Dim fields As New JObject
            fields.Add("source", source)
            fields.Add("volume", molvol)
            _obs.SendRequest("SetVolume", fields)
        Else
            If delay < 5 Then delay = 5
            If delay > 1000 Then delay = 1000
            Dim fields As New JObject
            fields.Add("source", source)
            Dim _VolumeInfo As JObject = _obs.SendRequest("GetVolume", fields)

            Dim startvolume As Integer = Math.Pow(CDbl(_VolumeInfo.GetValue("volume")), 1.0 / 3) * 100 ' Convert amplitude/mul to percent (approximate, mul is non-linear)

            If startvolume = volume Then
                Exit Sub
            ElseIf startvolume < volume Then
                For a = startvolume To volume Step steps
                    fields = New JObject
                    fields.Add("source", source)
                    fields.Add("volume", CDbl(a ^ 3 / 1000000))
                    _obs.SendRequest("SetVolume", fields)
                    Threading.Thread.Sleep(delay)
                Next
            ElseIf startvolume > volume Then
                For a = startvolume To volume Step -steps
                    fields = New JObject
                    fields.Add("source", source)
                    fields.Add("volume", CDbl(a ^ 3 / 1000000))
                    _obs.SendRequest("SetVolume", fields)
                    Threading.Thread.Sleep(delay)
                Next
            End If
        End If
    End Sub

    Private Sub PrintUsage()
        Dim out As List(Of String) = New List(Of String)

        out.Add("OBSCommand v1.5.7 ©2018-2022 by FSC-SOFT (http://www.VoiceMacro.net)")
        out.Add(vbCrLf)
        out.Add("Usage:")
        out.Add("------")
        out.Add("OBSCommand.exe /server=127.0.0.1:4444 /password=xxxx /delay=0.5 /setdelay=0.05 /profile=myprofile /scene=myscene /hidesource=myscene/mysource /showsource=myscene/mysource /togglesource=myscene/mysource /toggleaudio=myaudio /mute=myaudio /unmute=myaudio /setvolume=mysource,volume,[delay],[steps] /fadeopacity=mysource,myfiltername,startopacity,endopacity,[fadedelay],[fadestep] /slidesetting=mysource,myfiltername,startvalue,endvalue,[slidedelay],[slidestep] /slideasync=mysource,myfiltername,startvalue,endvalue,[slidedelay],[slidestep] /startstream /stopstream /startrecording /stoprecording /command=mycommand,myparam1=myvalue1... /sendjson=jsonstring")
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
        out.Add("OBSCommand.exe /setvolume=Mic/Aux,0,50,2")
        out.Add("OBSCommand.exe /setvolume=Mic/Aux,100")
        out.Add("OBSCommand.exe /fadeopacity=Mysource,myfiltername,0,100,5,2")
        out.Add("OBSCommand.exe /slidesetting=Mysource,myfiltername,contrast,-2,0,100,0.01")
        out.Add("OBSCommand.exe /slideasync=Mysource,myfiltername,saturation,*,5,100,0.1")
        out.Add("OBSCommand.exe /stopstream")
        out.Add("OBSCommand.exe /profile=myprofile /scene=myscene /showsource=mysource")
        out.Add("OBSCommand.exe /showsource=mysource")
        out.Add("OBSCommand.exe /hidesource=myscene/mysource")
        out.Add("OBSCommand.exe /togglesource=myscene/mysource")
        out.Add("OBSCommand.exe /showsource=""my scene""/""my source""")
        out.Add("OBSCommand.exe /command=SaveReplayBuffer")
        out.Add("OBSCommand.exe /command=TakeSourceScreenshot,sourceName=MyScene,PictureFormat=png,saveToFilePath=C:\OBSTest.png")
        out.Add("OBSCommand.exe /command=SetSourceFilterSettings,sourceName=""Color Correction"",filterName=Opacity,filterSettings=opacity=10")
        out.Add("OBSCommand.exe /sendjson=""ReorderSceneItems={'scene': 'MyScene', 'items': [{'name': 'Image'}, {'name': 'Gamecapture'}]}""")
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
        out.Add("/setvolume=myaudio,volume,delay,  set volume of audio source ""myaudio""")
        out.Add("steps                             volume is 0-100, delay is in milliseconds")
        out.Add("                                  between steps (min. 5, max. 1000) for fading")
        out.Add("                                  steps is (1-99), default step is 1")
        out.Add("  Note:  if delay is omitted volume is set instant")
        out.Add("/fadeopacity=mysource,myfiltername,startopacity,endopacity,[fadedelay],[fadestep]")
        out.Add("                                  start/end opacity is 0-100, 0=fully transparent")
        out.Add("                                  delay is in milliseconds, step 0-100")
        out.Add("             Note: Use * for start- or endopacity for fade from/to current value")
        out.Add("/slidesetting=mysource,myfiltername,settingname,startvalue,endvalue,[slidedelay],[slidestep]")
        out.Add("                                  start/end value min/max depends on setting!")
        out.Add("                                  delay is in milliseconds")
        out.Add("                                  step depends on setting (can be x Or 0.x Or 0.0x)")
        out.Add("             Note: Use * for start- or end value to slide from/to current value")
        out.Add("/slideasync")
        out.Add("            The same as slidesetting, only this one runs asynchron!")
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
            If z > Console.WindowHeight - 1 Then
                Console.Write("Press any key to continue...")
                Console.ReadKey()
                ClearCurrentConsoleLine()
                z = 0
            End If
            i += 1
            z += 1
            If i >= out.Count Then Exit Do

            z = z + out(i).Length / (Console.WindowWidth / 2)
        Loop

    End Sub

    Private Function SplitWhilePreservingQuotedValues(value As String, delimiter As Char, Optional ByVal DeleteQuotes As Boolean = False) As String()
        Dim csvPreservingQuotedStrings As New Regex(String.Format("(""[^""]*""|[^{0}])+", delimiter))
        Dim values = csvPreservingQuotedStrings.Matches(value).Cast(Of Match)().[Select](Function(m) m.Value.TrimStart(" ")).Where(Function(v) Not String.IsNullOrEmpty(v))

        Dim tmp() As String = values.ToArray()

        If DeleteQuotes = False Then Return tmp

        For a = 0 To tmp.Length - 1
            If tmp(a) <> Chr(34) AndAlso tmp(a).StartsWith(Chr(34)) And tmp(a).EndsWith(Chr(34)) Then
                tmp(a) = tmp(a).Substring(1, tmp(a).Length - 2)
            End If
        Next

        Return tmp
    End Function

    Public Sub ClearCurrentConsoleLine()
        Dim currentLineCursor As Integer = Console.CursorTop
        Console.SetCursorPosition(0, Console.CursorTop)
        Console.Write(New String(" "c, Console.WindowWidth))
        Console.SetCursorPosition(0, currentLineCursor)
    End Sub

    Class AsyncSlideSettings
        Dim _server As String
        Dim _password As String
        Dim _source As String
        Dim _filtername As String
        Dim _settingname As String
        Dim _fadestart As String
        Dim _fadeend As String
        Dim _delay As Integer
        Dim _fadestep As String

        Public Sub New(ByVal server As String, ByVal password As String, ByVal source As String, ByVal filtername As String, ByVal settingname As String, ByVal fadestart As String, ByVal fadeend As String, Optional ByVal delay As Integer = 0, Optional ByVal fadestep As String = "1")
            _server = server
            _password = password
            _source = source
            _filtername = filtername
            _settingname = settingname
            _fadestart = fadestart
            _fadeend = fadeend
            _delay = delay
            _fadestep = fadestep
        End Sub

        Public Sub StartSlide()
            SlideSetting(_server, _password, _source, _filtername, _settingname, _fadestart, _fadeend, _delay, _fadestep)
        End Sub

        Public Sub SlideSetting(ByVal server As String, ByVal password As String, ByVal source As String, ByVal filtername As String, ByVal settingname As String, ByVal fadestart As String, ByVal fadeend As String, Optional ByVal delay As Integer = 0, Optional ByVal fadestep As String = "1")
            Dim obs = New OBSWebsocket()
            obs.WSTimeout = New TimeSpan(0, 0, 0, 3)
            obs.Connect(server, password)

            If delay < 5 Then delay = 5
            If delay > 1000 Then delay = 1000
            Dim fields As New JObject

            If fadestart = "*" Or fadeend = "*" Then
                Dim tmpfield As JObject = New JObject
                tmpfield.Add("sourceName", source)
                tmpfield.Add("filterName", filtername)
                Dim result As JObject = obs.SendRequest("GetSourceFilterInfo", tmpfield)
                If fadestart = "*" Then
                    Dim tmp As JObject = result.GetValue("settings")
                    fadestart = tmp.GetValue(settingname)
                ElseIf fadeend = "*" Then
                    Dim tmp As JObject = result.GetValue("settings")
                    fadeend = tmp.GetValue(settingname)
                End If
            End If

            Dim haddecimals As Boolean = False

            If fadestep < 1 Then
                haddecimals = True
                fadestart = fadestart * 100
                fadeend = fadeend * 100
                fadestep = fadestep * 100
                delay = delay / 100
            End If

            If fadestart < fadeend Then
                For a As Integer = fadestart To fadeend Step fadestep
                    fields = New JObject
                    fields.Add("sourceName", source)
                    fields.Add("filterName", filtername)
                    Dim tmpfield As JObject = New JObject
                    If haddecimals = True Then
                        tmpfield.Add(settingname, ConvertToType(a / 100))
                    Else
                        tmpfield.Add(settingname, ConvertToType(a))
                    End If
                    fields.Add("filterSettings", tmpfield)
                    obs.SendRequest("SetSourceFilterSettings", fields)
                    Threading.Thread.Sleep(delay)
                Next
            ElseIf fadestart > fadeend Then
                For a As Integer = fadestart To fadeend Step -fadestep
                    fields = New JObject
                    fields.Add("sourceName", source)
                    fields.Add("filterName", filtername)
                    Dim tmpfield As JObject = New JObject
                    If haddecimals = True Then
                        tmpfield.Add(settingname, ConvertToType(a / 100))
                    Else
                        tmpfield.Add(settingname, ConvertToType(a))
                    End If
                    fields.Add("filterSettings", tmpfield)
                    obs.SendRequest("SetSourceFilterSettings", fields)
                    Threading.Thread.Sleep(delay)
                Next
            End If

            _obs.Disconnect()
        End Sub
    End Class
End Module
