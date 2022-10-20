Imports OBSWebsocketDotNet
Imports Newtonsoft.Json.Linq
Imports System.Text.RegularExpressions
Imports System.Threading

Module Main

    Private _obs As OBSWebsocket = New OBSWebsocket()
    Private isConnected As Boolean = False

    Sub Main()

        Dim args As String() = Environment.GetCommandLineArgs
        Dim password As String = ""
        Dim server As String = "ws://127.0.0.1:4455"

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

        Dim isInitialized As Boolean = False
        Dim skipfirst As Boolean = False
        Dim argsindex As Integer

        AddHandler _obs.Connected, AddressOf Connect
        AddHandler _obs.Disconnected, AddressOf Disconnect

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
                    '_obs = New OBSWebsocket()

                    _obs.WSTimeout = New TimeSpan(0, 0, 0, 3)
                    _obs.ConnectAsync(server, Nothing)
                    'Dim i As Integer = 0
                    'Do While Not _obs.IsConnected
                    '    Threading.Thread.Sleep(10)
                    '    i += 1
                    '    If i > 300 Then
                    '        Console.Write("Error: can't connect to OBS websocket plugin!")
                    '        End
                    '    End If
                    'Loop
                    Do While Not isConnected
                        Dim waitThread As New Thread(
                            Sub()
                                Thread.Sleep(10)
                            End Sub)

                        waitThread.Start()
                        waitThread.Join()
                    Loop
                    'Dim versionInfo As Types.ObsVersion = _obs.GetVersion()
                End If

                If profile <> "" Then
                    Dim fields As New JObject()
                    fields.Add("profileName", profile)
                    _obs.SendRequest("SetCurrentProfile", fields)
                End If

                If scene <> "" Then
                    Dim fields As New JObject
                    fields.Add("sceneName", scene)
                    Dim response As JObject = _obs.SendRequest("SetCurrentProgramScene", fields)
                End If

                ' sendjson
                If sendjson <> "" Then
                    Dim json As New JObject
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

                            Console.WriteLine(_obs.SendRequest(tmp(0), fields))
                        Else
                            Console.WriteLine(_obs.SendRequest(command))
                        End If


                    Catch ex As Exception
                        Console.WriteLine("Error with command """ & command & """: " & ex.Message.ToString())
                    End Try
                End If
                If hidesource <> "" Then
                    If hidesource.Contains("/") Then
                        Dim tmp As String() = hidesource.Split("/")

                        ' scene/source
                        If tmp.Count = 2 Then
                            Dim fields As New JObject
                            fields.Add("sceneName", tmp(0))
                            fields.Add("sceneItemId", GetSceneItemId(tmp(0), tmp(1)))
                            fields.Add("sceneItemEnabled", False)
                            _obs.SendRequest("SetSceneItemEnabled", fields)
                        End If
                    Else
                        Dim CurrentScene = GetCurrentProgramScene()
                        Dim fields As New JObject
                        fields.Add("sceneName", CurrentScene)
                        fields.Add("sceneItemId", GetSceneItemId(CurrentScene, hidesource))
                        fields.Add("sceneItemEnabled", False)
                        _obs.SendRequest("SetSceneItemEnabled", fields)
                    End If
                End If
                If showsource <> "" Then
                    If showsource.Contains("/") Then
                        Dim tmp As String() = showsource.Split("/")

                        ' scene/source
                        If tmp.Count = 2 Then
                            Dim fields As New JObject
                            fields.Add("sceneName", tmp(0))
                            fields.Add("sceneItemId", GetSceneItemId(tmp(0), tmp(1)))
                            fields.Add("sceneItemEnabled", True)
                            _obs.SendRequest("SetSceneItemEnabled", fields)
                        End If
                    Else
                        Dim CurrentScene = GetCurrentProgramScene()
                        Dim fields As New JObject
                        fields.Add("sceneName", CurrentScene)
                        fields.Add("sceneItemId", GetSceneItemId(CurrentScene, showsource))
                        fields.Add("sceneItemEnabled", True)
                        _obs.SendRequest("SetSceneItemEnabled", fields)
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
                    fields.Add("inputName", toggleaudio)
                    _obs.SendRequest("ToggleInputMute", fields)
                End If
                If mute <> "" Then
                    Dim fields As New JObject
                    fields.Add("inputName", mute)
                    fields.Add("inputMuted", True)
                    _obs.SendRequest("SetInputMute", fields)
                End If
                If unmute <> "" Then
                    Dim fields As New JObject
                    fields.Add("inputName", unmute)
                    fields.Add("inputMuted", False)
                    _obs.SendRequest("SetInputMute", fields)
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
                        t.IsBackground = True
                        t.Start()
                    ElseIf tmp.Count = 6 Then
                        If Not IsNumeric(tmp(5)) Then Throw New Exception("Delay value is not nummeric (0-x)!")
                        Dim ExecuteTask As New AsyncSlideSettings(server, password, tmp(0), tmp(1), tmp(2), tmp(3), tmp(4), tmp(5))
                        Dim t As Threading.Thread
                        t = New Threading.Thread(AddressOf ExecuteTask.StartSlide)
                        t.IsBackground = True
                        t.Start()
                    ElseIf tmp.Count = 7 Then
                        If Not IsNumeric(tmp(5)) Then Throw New Exception("Delay value is not nummeric (0-x)!")
                        If Not IsNumeric(tmp(6)) Then Throw New Exception("Slide step value is not nummeric (1-x)!")
                        Dim ExecuteTask As New AsyncSlideSettings(server, password, tmp(0), tmp(1), tmp(2), tmp(3), tmp(4), tmp(5), tmp(6))
                        Dim t As Threading.Thread
                        t = New Threading.Thread(AddressOf ExecuteTask.StartSlide)
                        t.IsBackground = True
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
                    '_obs.SendRequest("StartStreaming")
                    _obs.SendRequest("StartStream")
                End If
                If stopstream = True Then
                    '_obs.SendRequest("StopStreaming")
                    _obs.SendRequest("StopStream")
                End If
                If startrecording = True Then
                    '_obs.SendRequest("StartRecording")
                    _obs.SendRequest("StartRecord")
                End If
                If stoprecording = True Then
                    '_obs.SendRequest("StopRecording")
                    _obs.SendRequest("StopRecord")
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

        'Console.SetOut(myout)
        If errormessage = "" Then
            Console.Write("Ok")
        Else
            Console.Write("Error: " & errormessage)
        End If

    End Sub

    Private Sub Connect(sender As Object, e As EventArgs)
        Debug.WriteLine("Connection established!")
        isConnected = True
        'Console.WriteLine("Connected")
    End Sub

    Private Sub Disconnect(sender As Object, e As Communication.ObsDisconnectionInfo)
        Debug.WriteLine("Connection terminated!")
        'Console.WriteLine("DisConnected")
    End Sub

    Private Function IsNumericOrAsterix(value As String) As Boolean
        If value = "*" Then Return True

        If Not IsNumeric(value) Then Return False

        Return True
    End Function

    Private Function ConvertToType(text As String) As JToken
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

    Private Function GetSceneItemId(sceneName As String, sourceName As String) As Integer
        Dim fields As New JObject
        fields.Add("sceneName", ConvertToType(sceneName))
        fields.Add("sourceName", ConvertToType(sourceName))

        Dim response As JObject = _obs.SendRequest("GetSceneItemId", fields)

        Return response.GetValue("sceneItemId")
    End Function

    Private Function GetCurrentProgramScene() As String
        Dim response As JObject = _obs.SendRequest("GetCurrentProgramScene")

        Return response.GetValue("currentProgramSceneName")
    End Function

    Private Sub OBSToggleSource(source As String, Optional sceneName As String = "")

        If sceneName = "" Then
            sceneName = GetCurrentProgramScene()
        End If

        Dim fields As New JObject
        fields.Add("sceneName", sceneName)
        fields.Add("sceneItemId", GetSceneItemId(sceneName, source))
        Dim sceneItemEnabled As JObject = _obs.SendRequest("GetSceneItemEnabled", fields)
        fields.Add("sceneItemEnabled", Not Boolean.Parse(sceneItemEnabled.GetValue("sceneItemEnabled")))
        _obs.SendRequest("SetSceneItemEnabled", fields)

    End Sub

    Private Sub DoSlideSetting(source As String, filtername As String, settingname As String, fadestart As String, fadeend As String, Optional delay As Integer = 0, Optional fadestep As String = "1")

        If delay < 5 Then delay = 5
        If delay > 1000 Then delay = 1000

        If fadestart = "*" Or fadeend = "*" Then
            Dim tmpfield As JObject = New JObject
            tmpfield.Add("sourceName", source)
            tmpfield.Add("filterName", filtername)
            Dim result As JObject = _obs.SendRequest("GetSourceFilter", tmpfield)
            If fadestart = "*" Then
                Dim tmp As JObject = result.GetValue("filterSettings")
                fadestart = tmp.GetValue(settingname)
            ElseIf fadeend = "*" Then
                Dim tmp As JObject = result.GetValue("filterSettings")
                fadeend = tmp.GetValue(settingname)
            End If
        End If

        Dim haddecimals As Boolean = False

        If fadestep < 1 Then
            haddecimals = True
            fadestart *= 100
            fadeend *= 100
            fadestep *= 100
            delay /= 100
        End If

        Dim fields As JObject
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

    Private Sub OBSSetVolume(source As String, volume As Integer, Optional delay As Integer = 0, Optional steps As Integer = 1)
        volume += 1
        If steps < 1 Then steps = 1
        If steps > 99 Then steps = 99
        If delay = 0 Then
            Dim molvol As Double = volume ^ 3 / 1000000 ' Convert percent to amplitude/mul (approximate, mul is non-linear)
            Dim fields As New JObject
            fields.Add("inputName", source)
            fields.Add("inputVolumeMul", molvol)
            _obs.SendRequest("SetInputVolume", fields)
        Else
            If delay < 5 Then delay = 5
            If delay > 1000 Then delay = 1000
            Dim fields As New JObject
            fields.Add("inputName", source)
            Dim _VolumeInfo As JObject = _obs.SendRequest("GetInputVolume", fields)

            Dim startvolume As Integer = Math.Pow(CDbl(_VolumeInfo.GetValue("inputVolumeMul")), 1.0 / 3) * 100 ' Convert amplitude/mul to percent (approximate, mul is non-linear)

            If startvolume = volume Then
                Exit Sub
            ElseIf startvolume < volume Then
                For a = startvolume To volume Step steps
                    fields = New JObject
                    fields.Add("inputName", source)
                    fields.Add("inputVolumeMul", CDbl(a ^ 3 / 1000000))
                    _obs.SendRequest("SetInputVolume", fields)
                    Threading.Thread.Sleep(delay)
                Next
            ElseIf startvolume > volume Then
                For a = startvolume To volume Step -steps
                    fields = New JObject
                    fields.Add("inputName", source)
                    fields.Add("inputVolumeMul", CDbl(a ^ 3 / 1000000))
                    _obs.SendRequest("SetInputVolume", fields)
                    Threading.Thread.Sleep(delay)
                Next
            End If
        End If
    End Sub

    Private Sub PrintUsage()
        Dim out As List(Of String) = New List(Of String)

        out.Add("OBSCommand v1.6.2 (for OBS Version 28.x.x and above / Websocket 5.x.x and above) ©2018-2022 by FSC-SOFT (http://www.VoiceMacro.net)")
        out.Add(vbCrLf)
        out.Add("Usage:")
        out.Add("------")
        out.Add("OBSCommand /server=127.0.0.1:4455 /password=xxxx /delay=0.5 /setdelay=0.05 /profile=myprofile /scene=myscene /hidesource=myscene/mysource /showsource=myscene/mysource /togglesource=myscene/mysource /toggleaudio=myaudio /mute=myaudio /unmute=myaudio /setvolume=mysource,volume,[delay],[steps] /fadeopacity=mysource,myfiltername,startopacity,endopacity,[fadedelay],[fadestep] /slidesetting=mysource,myfiltername,startvalue,endvalue,[slidedelay],[slidestep] /slideasync=mysource,myfiltername,startvalue,endvalue,[slidedelay],[slidestep] /startstream /stopstream /startrecording /stoprecording /command=mycommand,myparam1=myvalue1... /sendjson=jsonstring")
        out.Add(vbCrLf)
        out.Add("Note: If Server is omitted, default 127.0.0.1:4455 will be used.")
        out.Add("Use quotes if your item name includes spaces.")
        out.Add("Password can be empty if no password is set in OBS Studio.")
        out.Add("You can use the same option multiple times.")
        out.Add("If you use Server and Password, those must be the first 2 options!")
        out.Add(vbCrLf)
        out.Add("This tool uses the obs-websocket plugin to talk to OBS Studio:")
        out.Add("https://github.com/Palakis/obs-websocket/releases")
        out.Add(vbCrLf)
        out.Add("3rd Party Dynamic link libraries used:")
        out.Add("Json.NET ©2021 by James Newton-King")
        out.Add("websocket-sharp ©2010-2022 by BarRaider")
        out.Add("obs-websocket-dotnet ©2022 by Stéphane Lepin.")
        out.Add(vbCrLf)
        out.Add("Examples:")
        out.Add("---------")
        out.Add("OBSCommand /scene=myscene")
        out.Add("OBSCommand /toggleaudio=""Desktop Audio""")
        out.Add("OBSCommand /mute=myAudioSource")
        out.Add("OBSCommand /unmute=""my Audio Source""")
        out.Add("OBSCommand /setvolume=Mic/Aux,0,50,2")
        out.Add("OBSCommand /setvolume=Mic/Aux,100")
        out.Add("OBSCommand /fadeopacity=Mysource,myfiltername,0,100,5,2")
        out.Add("OBSCommand /slidesetting=Mysource,myfiltername,contrast,-2,0,100,0.01")
        out.Add("OBSCommand /slideasync=Mysource,myfiltername,saturation,*,5,100,0.1")
        out.Add("OBSCommand /stopstream")
        out.Add("OBSCommand /profile=myprofile /scene=myscene /showsource=mysource")
        out.Add("OBSCommand /showsource=mysource")
        out.Add("OBSCommand /hidesource=myscene/mysource")
        out.Add("OBSCommand /togglesource=myscene/mysource")
        out.Add("OBSCommand /showsource=""my scene""/""my source""")
        out.Add("")
        out.Add("For most of other simpler requests, use the generalized '/command' feature (see syntax below):")
        out.Add("OBSCommand /command=SaveReplayBuffer")
        out.Add("OBSCommand /command=SaveSourceScreenshot,sourceName=MyScene,imageFormat=png,imageFilePath=C:\OBSTest.png")
        out.Add("OBSCommand /command=SetSourceFilterSettings,sourceName=""Color Correction"",filterName=Opacity,filterSettings=opacity=10")
        out.Add("OBSCommand /command=SetInputSettings,inputName=""Browser"",inputSettings=url='https://www.google.com/search?q=query+goes+there'")
        out.Add("")
        out.Add("For more complex requests, use the generalized '/sendjson' feature:")
        out.Add("OBSCommand.exe /sendjson=SaveSourceScreenshot={'sourceName':'MyScource','imageFormat':'png','imageFilePath':'H:\\OBSScreenShot.png'}")
        out.Add("")
        out.Add("You can combine multiple commands like this:")
        out.Add("OBSCommand /scene=mysource1 /delay=1.555 /scene=mysource2 ...etc")
        out.Add("OBSCommand /setdelay=1.555 /scene=mysource1 /scene=mysource2 ...etc")
        out.Add(vbCrLf)
        out.Add("Options:")
        out.Add("--------")
        out.Add("/server=127.0.0.1:4455            define server address and port")
        out.Add("  Note: If Server is omitted, default 127.0.0.1:4455 will be used.")
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
        out.Add("----------------------------")
        out.Add("/command=mycommand,myparam1=myvalue1,myparam2=myvalue2...")
        out.Add("                                  issues user command,parameter(s) (optional)")
        out.Add("/command=mycommand,myparam1=myvalue1,myparam2=myvalue2,myparam3=mysubparam=mysubparamvalue")
        out.Add("                                  issues user command,parameters and sub-parameters")
        out.Add("")
        out.Add("A full list of commands is available here https://github.com/obsproject/obs-websocket/blob/master/docs/generated/protocol.md")
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

            z += out(i).Length / (Console.WindowWidth / 2)
        Loop

    End Sub

    Private Function SplitWhilePreservingQuotedValues(value As String, delimiter As Char, Optional DeleteQuotes As Boolean = False) As String()
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





    ' Class for Async Slide Settings
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

        Public Sub New(server As String, password As String, source As String, filtername As String, settingname As String, fadestart As String, fadeend As String, Optional delay As Integer = 0, Optional fadestep As String = "1")
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

        Public Sub SlideSetting(server As String, password As String, source As String, filtername As String, settingname As String, fadestart As String, fadeend As String, Optional delay As Integer = 0, Optional fadestep As String = "1")
            Dim obs = New OBSWebsocket()
            obs.WSTimeout = New TimeSpan(0, 0, 0, 3)
            obs.ConnectAsync(server, password)
            Dim i As Integer = 0
            Do While Not obs.IsConnected
                Threading.Thread.Sleep(10)
                i += 1
                If i > 300 Then
                    Console.Write("Error: can't connect to OBS websocket plugin!")
                    End
                End If
            Loop

            If delay < 5 Then delay = 5
            If delay > 1000 Then delay = 1000

            If fadestart = "*" Or fadeend = "*" Then
                Dim tmpfield As JObject = New JObject
                tmpfield.Add("sourceName", source)
                tmpfield.Add("filterName", filtername)
                Dim result As JObject = obs.SendRequest("GetSourceFilter", tmpfield)

                If fadestart = "*" Then
                    Dim tmp As JObject = result.GetValue("filterSettings")
                    fadestart = tmp.GetValue(settingname)
                ElseIf fadeend = "*" Then
                    Dim tmp As JObject = result.GetValue("filterSettings")
                    fadeend = tmp.GetValue(settingname)
                End If
            End If

            Dim haddecimals As Boolean = False

            If fadestep < 1 Then
                haddecimals = True
                fadestart *= 100
                fadeend *= 100
                fadestep *= 100
                delay /= 100
            End If

            Dim fields As JObject
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
