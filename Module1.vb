Module Module1
    Property runningthreads As Integer = 0
    Property imglist As New SortedList
    Property allowonly As New ArrayList
    Property grabcoloralphas As Boolean = True
    Property copyonlyoriginals As Boolean = False
    Property gnozip As Boolean = False
    Property skipuricheck As Boolean = False
    Property pixelcount As Long = 0
    Property oldrunningthreads As Integer = 0
    Property maxrunthreads As Integer = 0
    Property usethreshold As Boolean = False
    Property minthresh As Integer = 0
    Property maxthresh As Integer = 255
    Property usecolormatrix As Boolean = False
    Property colorizeby As Drawing.Color = Drawing.Color.Black
    Property colorshiftby As Drawing.Color = Drawing.Color.Black
    Property usenegativecolorshift As Boolean = False
    Property usenegativecolorize As Boolean = False
    Property floatbrightadjust As Double = 0.0
    Property floatbrighttolerance As Double = 0.015
    Property linereachrange As Integer = 30
    Property rotationdegree As Single = 0
    Property onlyoneimageset As Boolean = False
    Property oneimageused As Boolean = False
    Property imgtransparency As Double = 0
    Property alphanoiserange As Integer = 2
    Property scanlinepower As Double = 0.25
    Property scanlinewidth As Integer = 1
    Property scanlineeveryrow As Integer = 3
    Property scanlinerotation As Integer = 0
    Property mustbeusedalone As New ArrayList
    Property slidefadestart As Double = 0.5
    Property slidefadeend As Double = 1.0
    Property slidefadestarttransparency As Double = 0.0
    Property slidefadeendtransparency As Double = 1.0
    Property fadetransparencyrotation As Single = 0.0

    Sub Main()

        AddHandler Console.CancelKeyPress, AddressOf controlcpressed
        mustbeusedalone.Add(ColorChannels.linedraw)
        mustbeusedalone.Add(ColorChannels.scanlines)
        mustbeusedalone.Add(ColorChannels.fade)


        If My.Application.CommandLineArgs.Count = 0 Then
            writearghelp()
            Exit Sub
        End If

        If My.Application.CommandLineArgs.Count >= 1 Then
            Dim ctime As Date = Date.Now
            Console.WriteLine((ctime - ctime).ToString("mm\:ss"))
            If My.Application.CommandLineArgs.Contains("-h") Or My.Application.CommandLineArgs.Contains("/h") Or My.Application.CommandLineArgs.Contains("-?") Or My.Application.CommandLineArgs.Contains("/?") Then
                writearghelp()
                Exit Sub
            End If

            dostartercleanup()
            Threading.ThreadPool.QueueUserWorkItem(AddressOf processCommandLineargs, My.Application.CommandLineArgs.ToArray)


            Threading.Thread.Sleep(8000)
            maxrunthreads = runningthreads

            While runningthreads > 0
                If maxrunthreads < runningthreads Then
                    maxrunthreads = runningthreads
                End If

                If runningthreads <> oldrunningthreads Then
                    Console.WriteLine("Progress :  images processed " & (maxrunthreads - runningthreads) & " of " & maxrunthreads)
                    oldrunningthreads = runningthreads
                End If
                Threading.Thread.Sleep(250)
            End While
            Console.WriteLine((Date.Now - ctime).ToString("mm\:ss"))

        End If
        dostartercleanup()
    End Sub

    Sub controlcpressed(sender As Object, e As ConsoleCancelEventArgs)
        Environment.Exit(0)
    End Sub

    Enum ColorChannels
        redblack
        blueblack
        greenblack
        redwhite
        bluewhite
        greenwhite
        alpha
        alphanoise
        yellow
        cyan
        black
        magenta
        cyanwhite
        yellowwhite
        magentawhite
        blackwhite
        grayscale
        inverted
        saturation
        brightness
        hue
        threshold
        colorize
        colorshift
        linedraw
        exposureadjust
        colorgut
        rotateimage
        transparency
        scanlines
        blinds
        fade
    End Enum

    Sub writearghelp()
        Dim outstring As String = My.Resources.argtext.Replace("%PROGNAME%", My.Application.Info.AssemblyName).Replace("%PROGVERSION%", My.Application.Info.Version.ToString).Replace("%APPLOCATION%", My.Application.Info.DirectoryPath)
        Console.WriteLine(outstring)

    End Sub

    Sub dostartercleanup()
        Dim dirpath As New IO.DirectoryInfo(My.Computer.FileSystem.SpecialDirectories.Temp)
        For Each file As IO.FileInfo In dirpath.GetFiles("*.tmp")
            Try
                file.Delete()
            Catch ex As Exception

            End Try
        Next
    End Sub

    Public Function getRealTempfile() As String
        While True
            Dim fxv As String = My.Computer.FileSystem.GetTempFileName
            Threading.Thread.Sleep(200)
            If IO.File.Exists(fxv) Then
                Return fxv
            Else
                Try
                    IO.File.Delete(fxv)
                Catch ex As Exception

                End Try
            End If
        End While
        Return Nothing
    End Function

    Function convertToCMYKpercent(rgba As Drawing.Color) As SortedList
        Dim returnval As New SortedList
        Dim degreered As Double = Convert.ToDouble(rgba.R) / 255
        Dim degreeblue As Double = Convert.ToDouble(rgba.B) / 255
        Dim degreegreen As Double = Convert.ToDouble(rgba.G) / 255

        returnval("black") = 1 - mathmax(New Double() {degreeblue, degreegreen, degreered})
        If returnval("black") <> 1 Then


            Try
                returnval("cyan") = (1 - degreered - returnval("black")) / (1 - returnval("black"))
                If Double.IsNaN(returnval("cyan")) Then
                    returnval("cyan") = 0
                End If
            Catch ex As Exception
                returnval("cyan") = 0
            End Try

            Try
                returnval("magenta") = (1 - degreegreen - returnval("black")) / (1 - returnval("black"))
                If Double.IsNaN(returnval("magenta")) Then
                    returnval("magenta") = 0
                End If
            Catch ex As Exception
                returnval("magenta") = 0
            End Try
            Try
                returnval("yellow") = (1 - degreeblue - returnval("black")) / (1 - returnval("black"))
                If Double.IsNaN(returnval("yellow")) Then
                    returnval("yellow") = 0
                End If
            Catch ex As Exception
                returnval("yellow") = 0
            End Try
        Else
            returnval("yellow") = 0
            returnval("magenta") = 0
            returnval("cyan") = 0
        End If



        returnval("black") = Convert.ToInt32(returnval("black") * 255)
        returnval("yellow") = Convert.ToInt32(returnval("yellow") * 255)
        returnval("cyan") = Convert.ToInt32(returnval("cyan") * 255)
        returnval("magenta") = Convert.ToInt32(returnval("magenta") * 255)


        Return returnval
    End Function

    Function convertToinvert(rgba As Drawing.Color) As Drawing.Color
        Dim redcolor As Integer = rgba.R - 255
        Dim bluecolor As Integer = rgba.B - 255
        Dim greencolor As Integer = rgba.G - 255

        If redcolor < 0 Then
            redcolor *= -1
        End If

        If bluecolor < 0 Then
            bluecolor *= -1
        End If

        If greencolor < 0 Then
            greencolor *= -1
        End If

        Return Drawing.Color.FromArgb(rgba.A, redcolor, bluecolor, greencolor)
    End Function

    Function convertToWhiteBalance(rgba As Drawing.Color) As Drawing.Color
        Dim redaddj As Integer = 255 - rgba.R
        Dim blueaddj As Integer = 255 - rgba.B
        Dim greenaddj As Integer = 255 - rgba.G

        Return Drawing.Color.FromArgb(rgba.A, redaddj, greenaddj, blueaddj)

    End Function

    Function getColorOFHUE(num As Integer) As Drawing.Color
        Dim redcolor As Double = 1
        Dim bluecolor As Double = 1
        Dim greencolor As Double = 1
        Dim redfullrange() As Integer = {0, 60, 300, 360}
        Dim redfallingrange() As Integer = {60, 120}
        Dim redrisingrange() As Integer = {240, 300}
        Dim bluefullrange() As Integer = {180, 300}
        Dim bluefallingrange() As Integer = {301, 360}
        Dim bluerisingrange() As Integer = {121, 180}
        Dim greenfullrange() As Integer = {61, 180}
        Dim greenfallingrange() As Integer = {181, 240}
        Dim greenrisingrange() As Integer = {0, 120}

        If (num >= redfullrange(0) And num <= redfullrange(1)) Or (num >= redfullrange(2) And num <= redfullrange(3)) Then
            redcolor = 1
        Else

            If (num >= redfallingrange(0) And num <= redfallingrange(1)) Then
                redcolor = 1 - ((num - redfallingrange(0)) / (redfallingrange(1) - redfallingrange(0)))
            Else
                If (num >= redrisingrange(0) And num <= redrisingrange(1)) Then
                    redcolor = ((num - redrisingrange(0)) / (redrisingrange(1) - redrisingrange(0)))
                Else
                    redcolor = 0

                End If
            End If
        End If


        If (num >= bluefullrange(0) And num <= bluefullrange(1)) Then
            bluecolor = 1
        Else

            If (num >= bluefallingrange(0) And num <= bluefallingrange(1)) Then
                bluecolor = 1 - ((num - bluefallingrange(0)) / (bluefallingrange(1) - bluefallingrange(0)))
            Else
                If (num >= bluerisingrange(0) And num <= bluerisingrange(1)) Then
                    bluecolor = ((num - bluerisingrange(0)) / (bluerisingrange(1) - bluerisingrange(0)))
                Else
                    bluecolor = 0

                End If
            End If
        End If

        If (num >= greenfullrange(0) And num <= greenfullrange(1)) Then
            greencolor = 1
        Else

            If (num >= greenfallingrange(0) And num <= greenfallingrange(1)) Then
                greencolor = 1 - ((num - greenfallingrange(0)) / (greenfallingrange(1) - greenfallingrange(0)))
            Else
                If (num >= greenrisingrange(0) And num <= greenrisingrange(1)) Then
                    greencolor = ((num - greenrisingrange(0)) / (greenrisingrange(1) - greenrisingrange(0)))
                Else
                    greencolor = 0

                End If
            End If
        End If
        Return Drawing.Color.FromArgb(255, redcolor * 255, greencolor * 255, bluecolor * 255)
    End Function

    Function mathmax(nums As Double()) As Double
        Dim check As Double = -1
        For Each num As Double In nums
            If check < num Then
                check = num
            End If
        Next
        Return check
    End Function

    Private Function GetEncoder(ByVal format As Drawing.Imaging.ImageFormat) As Drawing.Imaging.ImageCodecInfo


        For Each codec As Drawing.Imaging.ImageCodecInfo In Drawing.Imaging.ImageCodecInfo.GetImageDecoders()
            If codec.FormatID = format.Guid Then
                Return codec
            End If
        Next
        Return Nothing

    End Function

    Function regexreplace(searchpattern As String, inputstring As String, replacement As String, regexflags As Text.RegularExpressions.RegexOptions) As String
        Dim txr As New Text.RegularExpressions.Regex(searchpattern, regexflags)
        Return txr.Replace(inputstring, replacement)
    End Function

    Public Function getMime(file As IO.Stream) As SortedList
        Dim gr As New SortedList
        Dim cbyt(20) As Byte
        Dim mimestring As String = ""
        gr.Add("mime", "")
        gr.Add("ext", "")
        file.Position = 0
        file.Read(cbyt, 0, cbyt.Length - 1)



        Using xrx As New IO.MemoryStream(cbyt)
            Using rxik As New IO.StreamReader(xrx)
                mimestring = rxik.ReadToEnd
            End Using
        End Using

        file.Position = 0

        If System.Text.RegularExpressions.Regex.IsMatch(mimestring, "jfif", System.Text.RegularExpressions.RegexOptions.IgnoreCase) Then
            gr("mime") = "JPEG"
            gr("ext") = ".jpg"
        End If
        If System.Text.RegularExpressions.Regex.IsMatch(mimestring, "png", System.Text.RegularExpressions.RegexOptions.IgnoreCase) Then
            gr("mime") = "png"
            gr("ext") = ".png"
        End If
        If System.Text.RegularExpressions.Regex.IsMatch(mimestring, "gif", System.Text.RegularExpressions.RegexOptions.IgnoreCase) Then
            gr("mime") = "gif"
            gr("ext") = ".gif"
        End If
        If System.Text.RegularExpressions.Regex.IsMatch(mimestring, "svg", System.Text.RegularExpressions.RegexOptions.IgnoreCase) Then
            gr("mime") = "svg"
            gr("ext") = ".svg"
        End If

        Return gr
    End Function

    Public Function getMime(file As String) As SortedList
        Dim gr As New SortedList
        If IO.File.Exists(file) Then
            Using rxp As New IO.FileStream(file, IO.FileMode.Open)
                gr = getMime(rxp)
                rxp.Close()
            End Using
        End If
        Return gr
    End Function

    Sub processCommandLineargs(args As String())

        Dim rex As New Text.RegularExpressions.Regex("^[A-z]\-?\#?[a-fA-F0-9]+\%?")
        If args(0).IndexOf("-") = 0 Then
            Dim allowstring As String = args(0)
            skipuricheck = True
            grabcoloralphas = False
            Try

                If Windows.Forms.Clipboard.ContainsImage And args(0).IndexOf("-") = 0 And args(0).IndexOf("C") > -1 Then


                    If Not IO.Directory.Exists(My.Application.Info.DirectoryPath & "\imgdownloads") Then
                        Try
                            IO.Directory.CreateDirectory(My.Application.Info.DirectoryPath & "\imgdownloads")
                        Catch ex As Exception

                        End Try

                    End If

                    Dim curim As Integer = 0
                    While (IO.File.Exists(My.Application.Info.DirectoryPath & "\imgdownloads\ClipIMG" & curim & ".bmp"))
                        curim += 1
                    End While
                    Dim stxb As New Drawing.Bitmap(Windows.Forms.Clipboard.GetImage)
                    stxb.Save(My.Application.Info.DirectoryPath & "\imgdownloads\ClipIMG" & curim & ".bmp")
                    stxb.Dispose()
                    allowstring = allowstring.Replace("C", "")
                    Dim stp As New ArrayList
                    stp.Add(allowstring)
                    For fxo As Integer = 1 To args.Count - 1
                        stp.Add(args(fxo))
                    Next
                    stp.Add(My.Application.Info.DirectoryPath & "\imgdownloads\ClipIMG" & curim & ".bmp")
                    processCommandLineargs(stp.ToArray(GetType(String)))
                    Exit Sub
                End If

                If Windows.Forms.Clipboard.ContainsText And args(0).IndexOf("-") = 0 And args(0).IndexOf("C") > -1 Then
                    Dim cliptext As String = Windows.Forms.Clipboard.GetText
                    If cliptext.ToUpper.IndexOf("BASE64") > -1 Then
                        Dim datastart As String = cliptext.Substring(cliptext.IndexOf(","))
                        Dim bstring As Byte() = Convert.FromBase64CharArray(datastart, 0, datastart.Length - 1)
                        Dim stxp As New IO.MemoryStream(bstring)
                        Dim mimetype As SortedList = getMime(stxp)
                    End If
                End If
            Catch ex As Exception
                Exit Sub
            End Try


            If allowstring.IndexOf("f") > -1 Then
                gnozip = True
                allowstring = allowstring.Replace("f", "")
            End If
            If allowstring.IndexOf("A") > -1 Then
                grabcoloralphas = True
                allowstring = allowstring.Replace("A", "")
            End If

            For Each itm As String In allowstring

                Select Case itm

                    Case "M"
                        usecolormatrix = True
                    Case "d"
                        allowonly.Add(GetType(ColorChannels).GetEnumName(ColorChannels.alphanoise))
                    Case "a"
                        allowonly.Add(GetType(ColorChannels).GetEnumName(ColorChannels.alpha))

                    Case "c"
                        allowonly.Add(GetType(ColorChannels).GetEnumName(ColorChannels.cyan))
                        allowonly.Add(GetType(ColorChannels).GetEnumName(ColorChannels.cyanwhite))
                    Case "L"
                        allowonly.Add(GetType(ColorChannels).GetEnumName(ColorChannels.colorize))
                    Case "m"
                        allowonly.Add(GetType(ColorChannels).GetEnumName(ColorChannels.magenta))
                        allowonly.Add(GetType(ColorChannels).GetEnumName(ColorChannels.magentawhite))
                    Case "y"
                        allowonly.Add(GetType(ColorChannels).GetEnumName(ColorChannels.yellow))
                        allowonly.Add(GetType(ColorChannels).GetEnumName(ColorChannels.yellowwhite))
                    Case "k"
                        allowonly.Add(GetType(ColorChannels).GetEnumName(ColorChannels.black))
                        allowonly.Add(GetType(ColorChannels).GetEnumName(ColorChannels.blackwhite))
                    Case "r"
                        allowonly.Add(GetType(ColorChannels).GetEnumName(ColorChannels.redblack))
                        allowonly.Add(GetType(ColorChannels).GetEnumName(ColorChannels.redwhite))

                    Case "g"
                        allowonly.Add(GetType(ColorChannels).GetEnumName(ColorChannels.greenblack))
                        allowonly.Add(GetType(ColorChannels).GetEnumName(ColorChannels.greenwhite))
                    Case "b"
                        allowonly.Add(GetType(ColorChannels).GetEnumName(ColorChannels.blueblack))
                        allowonly.Add(GetType(ColorChannels).GetEnumName(ColorChannels.bluewhite))
                    Case "u"
                        allowonly.Add(GetType(ColorChannels).GetEnumName(ColorChannels.hue))
                    Case "t"
                        allowonly.Add(GetType(ColorChannels).GetEnumName(ColorChannels.brightness))
                    Case "T"
                        allowonly.Add(GetType(ColorChannels).GetEnumName(ColorChannels.saturation))
                    Case "e"
                        allowonly.Add(GetType(ColorChannels).GetEnumName(ColorChannels.grayscale))

                    Case "i"
                        allowonly.Add(GetType(ColorChannels).GetEnumName(ColorChannels.inverted))
                    Case "o"
                        copyonlyoriginals = True
                    Case "S"
                        allowonly.Add(GetType(ColorChannels).GetEnumName(ColorChannels.threshold))
                    Case "F"
                        allowonly.Add(GetType(ColorChannels).GetEnumName(ColorChannels.colorshift))
                    Case "n"
                        If allowstring.IndexOf("n") = 1 And allowstring.Length = 2 Then
                            allowonly.Add(GetType(ColorChannels).GetEnumName(ColorChannels.linedraw))
                            grabcoloralphas = True
                        Else
                            Console.WriteLine("Draw line must be used alone, ignoring command")

                        End If
                    Case "R"
                        If allowstring.IndexOf("R") = 1 And allowstring.Length = 2 Then
                            allowonly.Add(GetType(ColorChannels).GetEnumName(ColorChannels.rotateimage))
                        Else
                            Console.WriteLine("Rotate must be used alone, ignoring command")

                        End If
                    Case "x"
                        allowonly.Add(GetType(ColorChannels).GetEnumName(ColorChannels.exposureadjust))
                    Case "G"
                        allowonly.Add(GetType(ColorChannels).GetEnumName(ColorChannels.colorgut))
                    Case "z"
                        Dim fx As New Random
                        colorizeby = Drawing.Color.FromArgb(fx.Next(0, 255), fx.Next(0, 255), fx.Next(0, 255))
                        colorshiftby = colorizeby
                    Case "p"

                        allowonly.Add(GetType(ColorChannels).GetEnumName(ColorChannels.transparency))


                    Case "s"
                        If allowstring.IndexOf("s") = 1 And allowstring.Length = 2 Then
                            allowonly.Add(GetType(ColorChannels).GetEnumName(ColorChannels.scanlines))
                        Else
                            Console.WriteLine("Scanlines must be used alone, ignoring command")

                        End If

                    Case "K"
                        allowonly.Add(GetType(ColorChannels).GetEnumName(ColorChannels.blinds))
                    Case "D"
                        If allowstring.IndexOf("D") = 1 And allowstring.Length = 2 Then
                            allowonly.Add(GetType(ColorChannels).GetEnumName(ColorChannels.fade))
                        Else
                            Console.WriteLine("Fade must be used alone, ignoring command")

                        End If

                End Select
            Next

        End If

        For Each itm As String In args


            If rex.IsMatch(itm) Then
                Dim bsub As String = itm.Substring(1)
                bsub = bsub.Replace("%", "")
                Dim firstlet As String = itm(0)
                If bsub.IndexOf("#") = -1 Then


                    Select Case firstlet

                        Case "b"
                            Try
                                floatbrightadjust = Double.Parse(bsub) / 100

                            Catch ex As Exception

                            End Try


                        Case "r"
                            Try
                                linereachrange = Integer.Parse(bsub)

                                If linereachrange > 200 Then
                                    onlyoneimageset = True
                                    Console.WriteLine("warning: Reach exceeded safe limits for system stabilty, capping at one image only")
                                End If

                                If linereachrange < 1 Then
                                    linereachrange = 10
                                End If
                            Catch ex As Exception
                                linereachrange = 10
                            End Try
                        Case "t"
                            Try
                                floatbrighttolerance = Double.Parse(bsub) / 100

                            Catch ex As Exception

                            End Try
                        Case "H"
                            Try
                                Try
                                    maxthresh = Byte.Parse(bsub)
                                Catch ex As Exception

                                End Try
                            Catch ex As Exception

                            End Try
                        Case "h"
                            Try
                                minthresh = Byte.Parse(bsub)
                            Catch ex As Exception

                            End Try
                        Case "R"
                            Try
                                rotationdegree = Single.Parse(bsub)
                            Catch ex As Exception

                            End Try
                        Case "o"
                            Try
                                imgtransparency = Double.Parse(bsub) / 100
                            Catch ex As Exception

                            End Try
                        Case "n"
                            Try
                                alphanoiserange = Integer.Parse(bsub)
                            Catch ex As Exception

                            End Try
                        Case "s"
                            Try
                                scanlinepower = Double.Parse(bsub) / 100
                            Catch ex As Exception

                            End Try
                        Case "S"
                            Try
                                scanlineeveryrow = Integer.Parse(bsub)
                            Catch ex As Exception

                            End Try
                        Case "w"
                            Try
                                scanlinewidth = Integer.Parse(bsub)
                            Catch ex As Exception

                            End Try
                    End Select
                Else



                    bsub = bsub.Replace("-", "")

                    Dim bcolor As Drawing.Color = Drawing.Color.Black
                    Try
                        Dim rval As Integer = Integer.Parse(bsub.Substring(1, 2), Globalization.NumberStyles.HexNumber)
                        Dim gval As Integer = Integer.Parse(bsub.Substring(3, 2), Globalization.NumberStyles.HexNumber)
                        Dim bval As Integer = Integer.Parse(bsub.Substring(5, 2), Globalization.NumberStyles.HexNumber)
                        bcolor = Drawing.Color.FromArgb(rval, gval, bval)
                    Catch ex As Exception

                    End Try


                    Select Case firstlet
                        Case "c"
                            usenegativecolorize = IIf(itm.IndexOf("-") > -1, True, False)
                            colorizeby = bcolor
                        Case "C"
                            usenegativecolorshift = IIf(itm.IndexOf("-") > -1, True, False)
                            colorshiftby = bcolor


                    End Select
                End If

                Continue For

            End If

            If IO.Directory.Exists(itm) Then
                skipuricheck = True
                Dim cdir As New IO.DirectoryInfo(itm)
                For Each dfile As IO.FileInfo In cdir.GetFiles("*.*")
                    Select Case dfile.Extension.ToLower
                        Case ".png", ".jpg", ".bmp"
                            If dfile.Exists Then
                                Try

                                    Threading.ThreadPool.QueueUserWorkItem(AddressOf splitImage, New Object() {dfile.FullName, allowonly, gnozip})
                                Catch ex As Exception

                                End Try

                            End If
                        Case ".arg"
                            Threading.ThreadPool.QueueUserWorkItem(AddressOf processCommandLineargs, IO.File.ReadAllLines(dfile.FullName))
                    End Select

                Next
            End If
            If IO.File.Exists(itm) Then
                skipuricheck = True
                Dim ctxo As New IO.FileInfo(itm)
                Select Case ctxo.Extension.ToLower
                    Case ".png", ".jpg", ".bmp"
                        Try

                            Threading.ThreadPool.QueueUserWorkItem(AddressOf splitImage, New Object() {itm, allowonly, gnozip})
                        Catch ex As Exception

                        End Try
                    Case ".arg"
                        Try
                            Dim ostring As String() = IO.File.ReadAllLines(itm)
                            Dim carlist As New ArrayList
                            For Each itmx As String In ostring
                                If itmx.IndexOf("-") <> 0 Then
                                    carlist.Add(itmx)
                                End If
                            Next
                            processCommandLineargs(carlist.ToArray(GetType(String)))
                        Catch ex As Exception

                        End Try
                        Continue For
                End Select


            End If
            If skipuricheck Then
                skipuricheck = False
                Continue For
            End If
            Try
                Dim sct As New Uri(itm)
                Dim tfile As String = getRealTempfile()
                Using srp As New Net.WebClient
                    srp.BaseAddress = sct.ToString
                    Dim outfilename As String = sct.OriginalString.Substring(sct.OriginalString.LastIndexOf("/") + 1)
                    outfilename = regexreplace("[\W]+", outfilename, "-", Text.RegularExpressions.RegexOptions.IgnoreCase)

                    Try
                        srp.DownloadFile(sct.ToString, tfile)

                        Dim mimety As SortedList = getMime(tfile)
                        If Not mimety.ContainsKey("ext") Then
                            Try
                                IO.File.Delete(tfile)

                            Catch ex As Exception

                            End Try
                            srp.Dispose()
                            Continue For
                        End If
                        outfilename = outfilename & mimety("ext")
                        If Not IO.Directory.Exists(My.Application.Info.DirectoryPath & "\imgdownloads") Then
                            IO.Directory.CreateDirectory(My.Application.Info.DirectoryPath & "\imgdownloads")
                        End If
                        My.Computer.FileSystem.MoveFile(tfile, My.Application.Info.DirectoryPath & "\imgdownloads\" & outfilename, True)
                        Console.WriteLine(My.Application.Info.DirectoryPath & "\imgdownloads\" & outfilename & " created from url")
                        If Not copyonlyoriginals Then


                            Threading.ThreadPool.QueueUserWorkItem(AddressOf splitImage, New Object() {My.Application.Info.DirectoryPath & "\imgdownloads\" & outfilename, allowonly, gnozip})
                        End If
                    Catch ex As Exception
                        Console.WriteLine("ERROR: " & ex.Message)
                    End Try

                End Using
            Catch ex As Exception

            End Try

        Next
    End Sub

    Sub processCommandLineargs(args As Object)

        Dim strarg As String() = args
        processCommandLineargs(strarg)
    End Sub

    Sub splitImage(imgob As Object)

        If onlyoneimageset Then
            If oneimageused Then
                Console.WriteLine("Cap reached ignoring process of " & imgob(0))
                Exit Sub
            Else
                oneimageused = True
            End If
        End If

        Dim img As String = imgob(0)
        Dim allowances As ArrayList = imgob(1)
        Dim nozip As Boolean = imgob(2)
        runningthreads += 1
        Dim imgtosplit As New Drawing.Bitmap(img)
        Dim crk As New IO.FileInfo(img)
        Dim curimglist As New SortedList
        Dim roughimgcount As Integer = 1

        curimglist.Add(crk.Name.Replace(crk.Extension, " original " & crk.Extension), getRealTempfile())


        Try
            While IO.File.Exists(curimglist.GetByIndex(0))
                curimglist(curimglist.GetKey(0)) = getRealTempfile()
                Try
                    IO.File.Delete(curimglist.GetByIndex(0))
                Catch ex As Exception

                End Try
            End While

            My.Computer.FileSystem.CopyFile(img, curimglist.GetByIndex(0))
        Catch ex As Exception

        End Try

        Console.WriteLine("Processing " & img)
        For Each eval As Integer In GetType(ColorChannels).GetEnumValues

            If allowances.Count > 0 Then
                If Not allowances.Contains(GetType(ColorChannels).GetEnumName(eval)) Then
                    Continue For
                End If
                If allowances.Count > 1 Then
                    Select Case eval
                        Case ColorChannels.scanlines, ColorChannels.linedraw, ColorChannels.rotateimage
                            Continue For
                    End Select
                End If
            Else
                Select Case eval
                    Case ColorChannels.scanlines, ColorChannels.linedraw, ColorChannels.rotateimage
                        Continue For
                End Select
            End If



            Dim cthread As New Threading.Thread(AddressOf splitIntochannels)

            If usecolormatrix Then
                cthread = New Threading.Thread(AddressOf splitIntoChannelsWithMatrix)
            End If


            Console.WriteLine("Processing " & crk.Name.Replace(crk.Extension, "") & "  " & GetType(ColorChannels).GetEnumName(eval) & " &  alphachannels")
            roughimgcount += 1
            cthread.Start(New Object() {imgtosplit.Clone, crk.Name.Replace(crk.Extension, "") & " " & GetType(ColorChannels).GetEnumName(eval) & ".png", eval, False, curimglist})
            If grabcoloralphas And Not usecolormatrix Then
                roughimgcount += 1

                Threading.ThreadPool.QueueUserWorkItem(AddressOf splitIntochannels, New Object() {imgtosplit.Clone, crk.Name.Replace(crk.Extension, "") & " " & GetType(ColorChannels).GetEnumName(eval) & " coloralpha.png", eval, True, curimglist})
            End If


        Next

        While ((curimglist.Count <> roughimgcount) Or (roughimgcount = 1))
            Threading.Thread.Sleep(250)
        End While


        If curimglist.Count > 1 Then
            If Not nozip Then
                Console.WriteLine("Writing " & crk.Name.Replace(crk.Extension, "") & "Zip")
                Using pzip As New IO.FileStream(crk.FullName.Replace(crk.Extension, ".zip"), IO.FileMode.Create)
                    Dim zfile As New IO.Compression.ZipArchive(pzip, IO.Compression.ZipArchiveMode.Create)
                    Dim cim As IEnumerator = curimglist.GetEnumerator
                    While cim.MoveNext
                        If cim.Current.value Is Nothing Then
                            Continue While
                        End If

                        Dim crx As IO.Compression.ZipArchiveEntry = zfile.CreateEntry(cim.Current.key)

                        Using crtfile As IO.FileStream = IO.File.OpenRead(cim.Current.value)
                            Dim ffilestr As IO.Stream = crx.Open
                            crtfile.CopyTo(ffilestr)
                            ffilestr.Flush()
                            ffilestr.Close()
                        End Using
                        Try
                            IO.File.Delete(cim.Current.value)
                        Catch ex As Exception

                        End Try

                    End While
                    zfile.Dispose()
                    pzip.Close()
                    pzip.Dispose()
                End Using
                Console.WriteLine(crk.Name.Replace(crk.Extension, "") & "Zip Done")
            Else
                Dim cim As IEnumerator = curimglist.GetEnumerator
                Console.WriteLine("copying files of " & crk.Name.Replace(crk.Extension, ""))
                While cim.MoveNext
                    Try
                        IO.File.Move(cim.Current.value, crk.FullName.Replace(crk.Name, cim.Current.key))
                    Catch ex As Exception

                    End Try
                End While
            End If
        End If

        runningthreads -= 1
    End Sub

    Sub splitIntochannels(cob As Object)

        Dim pictosplit As Drawing.Bitmap = cob(0)
        Dim savename As String = cob(1)
        Dim chanselect As ColorChannels = cob(2)
        Dim usecoloralpha As Boolean = cob(3)
        Dim imglist As SortedList = cob(4)
        Dim alphadetected As Boolean = False
        Dim writecolor As New Drawing.Bitmap(pictosplit.Width, pictosplit.Height)
        Dim tfile As String = getRealTempfile()
        Dim blackcount As Integer = 0
        Dim dgui As Drawing.Graphics = Drawing.Graphics.FromImage(writecolor)
        Select Case chanselect
            Case ColorChannels.colorgut, ColorChannels.rotateimage, ColorChannels.transparency, ColorChannels.alphanoise, ColorChannels.blinds
                writecolor.MakeTransparent()
            Case Else
                If usecoloralpha Then
                    writecolor.MakeTransparent()
                Else
                    dgui.FillRectangle(Drawing.Brushes.White, 0, 0, writecolor.Width, writecolor.Height)
                End If
        End Select



        Dim txrand As New Random
        For y As Integer = 0 To pictosplit.Height - 1
                For x As Integer = 0 To pictosplit.Width - 1

                    Dim cpix As Drawing.Color = pictosplit.GetPixel(x, y)
                    Dim colorcmyk As SortedList = convertToCMYKpercent(cpix)
                    Dim colorwhitebalance As Drawing.Color = convertToWhiteBalance(cpix)
                    Dim colorhue As Drawing.Color = getColorOFHUE(cpix.GetHue)
                    Dim invertedpix As Drawing.Color = convertToinvert(cpix)


                    Select Case chanselect
                        Case ColorChannels.alpha
                            writecolor.SetPixel(x, y, Drawing.Color.FromArgb(255, cpix.A, cpix.A, cpix.A))

                            If cpix.A = 0 Then
                                blackcount += 1
                            End If
                            If blackcount / (pictosplit.Height * pictosplit.Width) >= 0.98 Then
                                imglist.Add(savename, Nothing)
                                writecolor.Dispose()
                                dgui.Dispose()
                                Exit Sub
                            End If
                        Case ColorChannels.redblack
                            writecolor.SetPixel(x, y, IIf(Not usecoloralpha, Drawing.Color.FromArgb(255, cpix.R, cpix.R, cpix.R), Drawing.Color.FromArgb(cpix.R, 255, 0, 0)))

                        Case ColorChannels.redwhite
                            writecolor.SetPixel(x, y, IIf(Not usecoloralpha, Drawing.Color.FromArgb(255, colorwhitebalance.R, colorwhitebalance.R, colorwhitebalance.R), Drawing.Color.FromArgb(colorwhitebalance.R, 255, 0, 0)))

                        Case ColorChannels.greenblack
                            writecolor.SetPixel(x, y, IIf(Not usecoloralpha, Drawing.Color.FromArgb(255, cpix.G, cpix.G, cpix.G), Drawing.Color.FromArgb(cpix.G, 0, 255, 0)))

                        Case ColorChannels.greenwhite
                            writecolor.SetPixel(x, y, IIf(Not usecoloralpha, Drawing.Color.FromArgb(255, colorwhitebalance.G, colorwhitebalance.G, colorwhitebalance.G), Drawing.Color.FromArgb(colorwhitebalance.G, 0, 255, 0)))


                        Case ColorChannels.blueblack
                            writecolor.SetPixel(x, y, IIf(Not usecoloralpha, Drawing.Color.FromArgb(255, cpix.B, cpix.B, cpix.B), Drawing.Color.FromArgb(cpix.G, 0, 0, 255)))

                        Case ColorChannels.bluewhite
                            writecolor.SetPixel(x, y, IIf(Not usecoloralpha, Drawing.Color.FromArgb(255, colorwhitebalance.B, colorwhitebalance.B, colorwhitebalance.B), Drawing.Color.FromArgb(colorwhitebalance.B, 0, 0, 255)))



                        Case ColorChannels.yellow
                            writecolor.SetPixel(x, y, IIf(Not usecoloralpha, Drawing.Color.FromArgb(255, colorcmyk("yellow"), colorcmyk("yellow"), colorcmyk("yellow")), Drawing.Color.FromArgb(colorcmyk("yellow"), 255, 255, 0)))

                        Case ColorChannels.black
                            writecolor.SetPixel(x, y, IIf(Not usecoloralpha, Drawing.Color.FromArgb(255, colorcmyk("black"), colorcmyk("black"), colorcmyk("black")), Drawing.Color.FromArgb(colorcmyk("black"), 0, 0, 0)))

                        Case ColorChannels.cyan
                            writecolor.SetPixel(x, y, IIf(Not usecoloralpha, Drawing.Color.FromArgb(255, colorcmyk("cyan"), colorcmyk("cyan"), colorcmyk("cyan")), Drawing.Color.FromArgb(colorcmyk("cyan"), 0, 255, 255)))

                        Case ColorChannels.magenta
                            writecolor.SetPixel(x, y, IIf(Not usecoloralpha, Drawing.Color.FromArgb(255, colorcmyk("magenta"), colorcmyk("magenta"), colorcmyk("magenta")), Drawing.Color.FromArgb(colorcmyk("magenta"), 255, 0, 255)))
                        Case ColorChannels.yellow
                            writecolor.SetPixel(x, y, IIf(Not usecoloralpha, Drawing.Color.FromArgb(255, colorcmyk("yellow"), colorcmyk("yellow"), colorcmyk("yellow")), Drawing.Color.FromArgb(colorcmyk("yellow"), 255, 255, 0)))

                        Case ColorChannels.blackwhite
                            writecolor.SetPixel(x, y, IIf(Not usecoloralpha, Drawing.Color.FromArgb(255, 255 - colorcmyk("black"), 255 - colorcmyk("black"), 255 - colorcmyk("black")), Drawing.Color.FromArgb(255 - colorcmyk("black"), 0, 0, 0)))

                        Case ColorChannels.cyanwhite
                            writecolor.SetPixel(x, y, IIf(Not usecoloralpha, Drawing.Color.FromArgb(255, 255 - colorcmyk("cyan"), 255 - colorcmyk("cyan"), 255 - colorcmyk("cyan")), Drawing.Color.FromArgb(255 - colorcmyk("cyan"), 0, 0, 0)))

                        Case ColorChannels.magentawhite
                            writecolor.SetPixel(x, y, IIf(Not usecoloralpha, Drawing.Color.FromArgb(255, 255 - colorcmyk("magenta"), 255 - colorcmyk("magenta"), 255 - colorcmyk("magenta")), Drawing.Color.FromArgb(255 - colorcmyk("magenta"), 0, 0, 0)))
                        Case ColorChannels.yellowwhite
                            writecolor.SetPixel(x, y, IIf(Not usecoloralpha, Drawing.Color.FromArgb(255, 255 - colorcmyk("yellow"), 255 - colorcmyk("yellow"), 255 - colorcmyk("yellow")), Drawing.Color.FromArgb(255 - colorcmyk("yellow"), 0, 0, 0)))

                        Case ColorChannels.saturation
                            writecolor.SetPixel(x, y, IIf(Not usecoloralpha, Drawing.Color.FromArgb(255, cpix.GetSaturation * 255, cpix.GetSaturation * 255, cpix.GetSaturation * 255), Drawing.Color.FromArgb(cpix.GetSaturation * 255, 0, 0, 0)))
                        Case ColorChannels.brightness, ColorChannels.grayscale
                            writecolor.SetPixel(x, y, IIf(Not usecoloralpha, Drawing.Color.FromArgb(255, cpix.GetBrightness * 255, cpix.GetBrightness * 255, cpix.GetBrightness * 255), Drawing.Color.FromArgb(cpix.GetBrightness * 255, 0, 0, 0)))
                        Case ColorChannels.colorize
                            If colorizeby = Drawing.Color.Black Then
                                imglist.Add(savename, Nothing)
                                writecolor.Dispose()
                                dgui.Dispose()
                                Exit Sub
                            End If
                            Dim cpixsat As Integer = cpix.GetBrightness * 255
                            If Not usenegativecolorize Then
                                If Not usecoloralpha Then
                                    Dim colorizered As Integer = IIf(cpixsat + colorizeby.R > 255, 255, cpixsat + colorizeby.R)
                                    Dim colorizegreen As Integer = IIf(cpixsat + colorizeby.G > 255, 255, cpixsat + colorizeby.G)
                                    Dim colorizeblue As Integer = IIf(cpixsat + colorizeby.B > 255, 255, cpixsat + colorizeby.B)
                                    dgui.DrawRectangle(New Drawing.Pen(Drawing.Color.FromArgb(colorizered, colorizegreen, colorizeblue)), x, y, 1, 1)
                                    dgui.DrawRectangle(New Drawing.Pen(Drawing.Color.FromArgb(255 - cpixsat, colorizeby)), x, y, 1, 1)


                                Else
                                    writecolor.SetPixel(x, y, Drawing.Color.FromArgb(255 - cpixsat, colorizeby))
                                End If
                            Else
                                If Not usecoloralpha Then
                                    Dim colorizered As Integer = IIf(cpixsat - colorizeby.R < 0, 0, cpixsat - colorizeby.R)
                                    Dim colorizegreen As Integer = IIf(cpixsat - colorizeby.G < 0, 0, cpixsat - colorizeby.G)
                                    Dim colorizeblue As Integer = IIf(cpixsat - colorizeby.B < 0, 0, cpixsat - colorizeby.B)
                                    dgui.DrawRectangle(New Drawing.Pen(Drawing.Color.FromArgb(colorizered, colorizegreen, colorizeblue)), x, y, 1, 1)
                                    dgui.DrawRectangle(New Drawing.Pen(Drawing.Color.FromArgb(255 - cpixsat, colorizeby)), x, y, 1, 1)


                                Else
                                    writecolor.SetPixel(x, y, Drawing.Color.FromArgb(255 - cpixsat, colorizeby))
                                End If

                            End If
                    Case ColorChannels.colorshift
                        If colorizeby = Drawing.Color.Black Then
                            imglist.Add(savename, Nothing)
                            writecolor.Dispose()
                            dgui.Dispose()
                            Exit Sub
                        End If
                        If usecoloralpha Then
                                imglist.Add(savename, Nothing)
                                writecolor.Dispose()
                                dgui.Dispose()
                                Exit Sub
                            End If

                            If Not usenegativecolorshift Then
                                Dim redc As Integer = IIf(Convert.ToInt32(cpix.R) + Convert.ToInt32(colorshiftby.R) > 255, 255, Convert.ToInt32(cpix.R) + Convert.ToInt32(colorshiftby.R))
                                Dim greenc As Integer = IIf(Convert.ToInt32(cpix.G) + Convert.ToInt32(colorshiftby.G) > 255, 255, Convert.ToInt32(cpix.G) + Convert.ToInt32(colorshiftby.G))
                                Dim bluec As Integer = IIf(Convert.ToInt32(cpix.B) + Convert.ToInt32(colorshiftby.B) > 255, 255, Convert.ToInt32(cpix.B) + Convert.ToInt32(colorshiftby.B))
                                writecolor.SetPixel(x, y, Drawing.Color.FromArgb(redc, greenc, bluec))
                                dgui.DrawRectangle(New Drawing.Pen(Drawing.Color.FromArgb(255 - (cpix.GetBrightness * 255), Drawing.Color.Black)), x, y, 1, 1)
                            Else
                                Dim redc As Integer = IIf(Convert.ToInt32(cpix.R) - Convert.ToInt32(colorshiftby.R) < 0, 0, Convert.ToInt32(cpix.R) - Convert.ToInt32(colorshiftby.R))
                                Dim greenc As Integer = IIf(Convert.ToInt32(cpix.G) - Convert.ToInt32(colorshiftby.G) < 0, 0, Convert.ToInt32(cpix.G) - Convert.ToInt32(colorshiftby.G))
                                Dim bluec As Integer = IIf(Convert.ToInt32(cpix.B) - Convert.ToInt32(colorshiftby.B) < 0, 0, Convert.ToInt32(cpix.B) - Convert.ToInt32(colorshiftby.B))
                                writecolor.SetPixel(x, y, Drawing.Color.FromArgb(redc, greenc, bluec))
                                dgui.DrawRectangle(New Drawing.Pen(Drawing.Color.FromArgb(255 - (cpix.GetBrightness * 255), Drawing.Color.Black)), x, y, 1, 1)
                            End If


                        Case ColorChannels.hue
                            writecolor.SetPixel(x, y, IIf(Not usecoloralpha, colorhue, colorhue))
                        Case ColorChannels.inverted
                            writecolor.SetPixel(x, y, IIf(Not usecoloralpha, invertedpix, invertedpix))
                        Case ColorChannels.threshold
                            If maxthresh = 255 And minthresh = 0 Then
                                imglist.Add(savename, Nothing)
                                writecolor.Dispose()

                                Exit Sub
                            End If
                            Dim bright As Integer = cpix.GetBrightness * 255
                            If bright >= minthresh And bright <= maxthresh Then
                                If Not usecoloralpha Then
                                    writecolor.SetPixel(x, y, Drawing.Color.White)
                                End If
                            Else
                                writecolor.SetPixel(x, y, Drawing.Color.Black)
                            End If

                        Case ColorChannels.linedraw

                            Dim cbright As Double = cpix.GetBrightness
                            Dim brightoffset As Double = floatbrighttolerance
                            Dim offsetx As Integer = linereachrange
                            Dim offsety As Integer = linereachrange
                            Dim offsetdiagnal As Integer = linereachrange
                            Dim offset As Integer = linereachrange
                            Dim abletograb As Integer = 0
                            Dim combinedbrightness As Double = 0

                            While offset > 1

                                If y - offset > 0 Then
                                    If x - offset > 0 Then
                                        combinedbrightness += pictosplit.GetPixel(x - offset, y - offset).GetBrightness
                                        abletograb += 1
                                    End If
                                    If x + offset < pictosplit.Width Then
                                        combinedbrightness += pictosplit.GetPixel(x + offset, y - offset).GetBrightness
                                        abletograb += 1
                                    End If
                                End If
                                If y + offset < pictosplit.Height Then
                                    If x - offset > 0 Then
                                        combinedbrightness += pictosplit.GetPixel(x - offset, y + offset).GetBrightness
                                        abletograb += 1
                                    End If
                                    If x + offset < pictosplit.Width Then
                                        combinedbrightness += pictosplit.GetPixel(x + offset, y + offset).GetBrightness
                                        abletograb += 1
                                    End If
                                End If
                                offset -= 1
                            End While




                            If abletograb >= 1 Then

                                If (combinedbrightness / abletograb) - pictosplit.GetPixel(x, y).GetBrightness <= brightoffset Then
                                    Continue For
                                End If
                            End If

                            writecolor.SetPixel(x, y, Drawing.Color.Black)
                        Case ColorChannels.exposureadjust
                            If usecoloralpha Then
                                imglist.Add(savename, Nothing)
                                writecolor.Dispose()
                                dgui.Dispose()
                                Exit Sub
                            End If
                            Dim rj As Integer = Integer.Parse(cpix.R) + (floatbrightadjust * 255)
                            Dim bj As Integer = Integer.Parse(cpix.B) + (floatbrightadjust * 255)
                            Dim gj As Integer = Integer.Parse(cpix.G) + (floatbrightadjust * 255)

                            Select Case rj
                                Case > 255
                                    rj = 255
                                Case < 0
                                    rj = 0
                            End Select
                            Select Case bj
                                Case > 255
                                    bj = 255
                                Case < 0
                                    bj = 0
                            End Select
                            Select Case gj
                                Case > 255
                                    gj = 255
                                Case < 0
                                    gj = 0
                            End Select
                            writecolor.SetPixel(x, y, Drawing.Color.FromArgb(rj, gj, gj))
                        Case ColorChannels.colorgut
                            If usecoloralpha Then
                                imglist.Add(savename, Nothing)
                                writecolor.Dispose()
                                dgui.Dispose()
                                Exit Sub
                            End If

                            Dim cbright As Double = cpix.GetBrightness
                            Dim thmax As Double = maxthresh / 255
                            Dim thmin As Double = minthresh / 255

                            If (cbright >= thmin) And (cbright <= thmax) Then
                                writecolor.SetPixel(x, y, cpix)
                            Else
                                writecolor.SetPixel(x, y, Drawing.Color.FromArgb(0, cpix))
                            End If

                    Case ColorChannels.rotateimage
                        Dim floatsize As New Drawing.PointF(pictosplit.Width, pictosplit.Height)
                        Dim center As New Drawing.Point(Convert.ToSingle(floatsize.X / 2), Convert.ToInt32(floatsize.Y / 2))
                        writecolor.Dispose()
                        writecolor = New Drawing.Bitmap(Convert.ToInt32(floatsize.X * 1.5), Convert.ToInt32(floatsize.Y * 1.5))
                        dgui.Dispose()
                        dgui = Drawing.Graphics.FromImage(writecolor)

                        writecolor.MakeTransparent()
                        dgui.TranslateTransform(writecolor.Width / 2, writecolor.Height / 2)
                        dgui.RotateTransform(rotationdegree)
                        dgui.DrawImage(pictosplit, New Drawing.Point(-1 * ((writecolor.Width / 2) - floatsize.X), -1 * ((writecolor.Height / 2) - floatsize.Y)))

                        y = pictosplit.Height
                        Exit For
                    Case ColorChannels.transparency
                        If usecoloralpha Then
                            imglist.Add(savename, Nothing)
                            writecolor.Dispose()
                            dgui.Dispose()
                            Exit Sub
                        End If

                        Dim cpixAlpha As Integer = cpix.A
                        Dim imgtran As Integer = -255 * imgtransparency
                        cpixAlpha += imgtran
                        If cpixAlpha > 255 Then
                            cpixAlpha = 255
                        End If
                        If cpixAlpha < 0 Then
                            cpixAlpha = 0
                        End If

                        writecolor.SetPixel(x, y, Drawing.Color.FromArgb(cpixAlpha, cpix.R, cpix.G, cpix.B))
                    Case ColorChannels.alphanoise
                        If usecoloralpha Then
                            imglist.Add(savename, Nothing)
                            writecolor.Dispose()
                            dgui.Dispose()
                            Exit Sub
                        End If

                        Dim maxskip As Integer = 0

                        maxskip = txrand.Next(0, alphanoiserange)
                        Try
                            writecolor.SetPixel(x, y, cpix)
                        Catch ex As Exception

                        End Try


                        x += maxskip
                    Case ColorChannels.scanlines
                        If usecoloralpha Then
                            imglist.Add(savename, Nothing)
                            writecolor.Dispose()
                            dgui.Dispose()
                            Exit Sub
                        End If
                        Dim dxip As Drawing.Bitmap = pictosplit.Clone
                        dgui.DrawImage(dxip, New Drawing.PointF(0, 0))

                        For yline As Integer = 0 To writecolor.Height - 1
                            If yline Mod scanlineeveryrow = 0 And yline > scanlineeveryrow - 1 Then
                                dgui.DrawLine(New Drawing.Pen(Drawing.Color.FromArgb(255 * (scanlinepower / 2), 255, 255, 255), scanlinewidth), New Drawing.Point(0, yline - scanlinewidth), New Drawing.Point(writecolor.Width - 1, yline - scanlinewidth))
                                dgui.DrawLine(New Drawing.Pen(Drawing.Color.FromArgb(255 * scanlinepower, 0, 0, 0), scanlinewidth), New Drawing.Point(0, yline), New Drawing.Point(writecolor.Width - 1, yline))
                            End If

                        Next
                        dxip.Dispose()
                        y = writecolor.Height
                        Exit For
                    Case ColorChannels.blinds
                        If usecoloralpha Then
                            imglist.Add(savename, Nothing)
                            writecolor.Dispose()
                            dgui.Dispose()
                            Exit Sub
                        End If
                        Dim gxp As New Random
                        Try
                            writecolor.SetPixel(x, y, cpix)
                        Catch ex As Exception

                        End Try
                        x += gxp.Next(0, 2)

                    Case ColorChannels.fade
                        Dim cxp As New Drawing.Drawing2D.LinearGradientBrush(New Drawing.PointF(0, 0), New Drawing.Point(writecolor.Width, writecolor.Height), Drawing.Color.FromArgb(255, 0, 0, 0), Drawing.Color.FromArgb(0, 0, 0, 0))

                        dgui.DrawImage(pictosplit, New Drawing.PointF(0, 0))
                        cxp.MultiplyTransform(dgui.Transform)
                        dgui.FillRectangle(cxp, pictosplit.GetBounds(Drawing.GraphicsUnit.Pixel))

                        y = writecolor.Height
                        Exit For

                    Case Else
                            imglist.Add(savename, Nothing)
                            writecolor.Dispose()
                            dgui.Dispose()
                            Exit Sub

                    End Select

                Next
            Next
            dgui.Dispose()


        Try
            pictosplit.Dispose()
            Dim myEncoderParameters As New Drawing.Imaging.EncoderParameters(1)
            Dim myEncoder As System.Drawing.Imaging.Encoder = System.Drawing.Imaging.Encoder.Quality
            Dim myEncoderParameter As New System.Drawing.Imaging.EncoderParameter(myEncoder, 100&)
            myEncoderParameters.Param(0) = myEncoderParameter
            Dim pngencoder As Drawing.Imaging.ImageCodecInfo = GetEncoder(Drawing.Imaging.ImageFormat.Png)
            writecolor.Save(tfile, pngencoder, myEncoderParameters)
            writecolor.Dispose()
            Console.WriteLine("Wrote " & savename)
            imglist.Add(savename, tfile)
        Catch ex As Exception
            Try
                pictosplit.Dispose()
            Catch ex2 As Exception

            End Try
            imglist.Add(savename, Nothing)
        End Try

    End Sub

    Sub splitIntoChannelsWithMatrix(cob As Object)
        Dim pictosplit As Drawing.Bitmap = cob(0)
        Dim savename As String = cob(1)
        Dim chanselect As ColorChannels = cob(2)
        Dim usecoloralpha As Boolean = cob(3)
        Dim imglist As SortedList = cob(4)
        Dim alphadetected As Boolean = False
        Dim writecolor As New Drawing.Bitmap(pictosplit.Width, pictosplit.Height)
        Dim tfile As String = getRealTempfile()
        Dim cmatrix As New Drawing.Imaging.ColorMatrix
        Dim imgatt As New Drawing.Imaging.ImageAttributes

        Using dgui As Drawing.Graphics = Drawing.Graphics.FromImage(writecolor)
            Select Case chanselect

                Case ColorChannels.redblack
                    cmatrix.Matrix00 = 1
                    cmatrix.Matrix11 = 0
                    cmatrix.Matrix22 = 0

                Case ColorChannels.greenblack
                    cmatrix.Matrix00 = 0
                    cmatrix.Matrix11 = 1
                    cmatrix.Matrix22 = 0


                Case ColorChannels.blueblack
                    cmatrix.Matrix00 = 0
                    cmatrix.Matrix11 = 0
                    cmatrix.Matrix22 = 1

                Case ColorChannels.grayscale, ColorChannels.brightness
                    cmatrix.Matrix00 = 0.2627
                    cmatrix.Matrix01 = 0.2627
                    cmatrix.Matrix02 = 0.2627
                    cmatrix.Matrix10 = 0.678
                    cmatrix.Matrix11 = 0.678
                    cmatrix.Matrix12 = 0.678
                    cmatrix.Matrix20 = 0.0593
                    cmatrix.Matrix21 = 0.0593
                    cmatrix.Matrix22 = 0.0593
                Case Else
                    imglist.Add(savename, Nothing)
                    writecolor.Dispose()
                    dgui.Dispose()
                    Exit Sub

            End Select
            imgatt.SetColorMatrix(cmatrix)
            dgui.DrawImage(pictosplit, New Drawing.Rectangle(New Drawing.Point(0, 0), pictosplit.Size), 0, 0, pictosplit.Width, pictosplit.Height, Drawing.GraphicsUnit.Pixel, imgatt)
        End Using
        Try
            pictosplit.Dispose()
            Dim myEncoderParameters As New Drawing.Imaging.EncoderParameters(1)
            Dim myEncoder As System.Drawing.Imaging.Encoder = System.Drawing.Imaging.Encoder.Quality
            Dim myEncoderParameter As New System.Drawing.Imaging.EncoderParameter(myEncoder, 100&)
            myEncoderParameters.Param(0) = myEncoderParameter
            Dim pngencoder As Drawing.Imaging.ImageCodecInfo = GetEncoder(Drawing.Imaging.ImageFormat.Png)
            writecolor.Save(tfile, pngencoder, myEncoderParameters)
            writecolor.Dispose()
            Console.WriteLine("Wrote " & savename)
            imglist.Add(savename, tfile)
        Catch ex As Exception
            imglist.Add(savename, Nothing)
        End Try
    End Sub

End Module
