Module Module1
    Dim runningthreads As Integer = 0
    Dim imglist As New SortedList
    Dim allowonly As New ArrayList
    Dim grabcoloralphas As Boolean = True
    Dim gnozip As Boolean = False
    Sub Main()
        If My.Application.CommandLineArgs.Count = 0 Then
            writearghelp()

            Exit Sub
        End If

        If My.Application.CommandLineArgs.Count >= 1 Then
            If My.Application.CommandLineArgs.Contains("-h") Or My.Application.CommandLineArgs.Contains("/h") Or My.Application.CommandLineArgs.Contains("-?") Or My.Application.CommandLineArgs.Contains("/?") Then
                writearghelp()
                Exit Sub
            End If

            If My.Application.CommandLineArgs(0).IndexOf("-") = 0 Then
                Dim allowstring As String = My.Application.CommandLineArgs(0)
                grabcoloralphas = False
                For Each itm As String In allowstring
                    Select Case itm
                        Case "a"
                            allowonly.Add(GetType(ColorChannels).GetEnumName(ColorChannels.alpha))
                        Case "A"
                            grabcoloralphas = True
                        Case "c"
                            allowonly.Add(GetType(ColorChannels).GetEnumName(ColorChannels.cyan))
                        Case "m"
                            allowonly.Add(GetType(ColorChannels).GetEnumName(ColorChannels.magenta))
                        Case "y"
                            allowonly.Add(GetType(ColorChannels).GetEnumName(ColorChannels.yellow))
                        Case "k"
                            allowonly.Add(GetType(ColorChannels).GetEnumName(ColorChannels.black))
                        Case "r"
                            allowonly.Add(GetType(ColorChannels).GetEnumName(ColorChannels.redblack))
                        Case "R"
                            allowonly.Add(GetType(ColorChannels).GetEnumName(ColorChannels.redwhite))

                        Case "g"
                            allowonly.Add(GetType(ColorChannels).GetEnumName(ColorChannels.greenblack))
                        Case "G"
                            allowonly.Add(GetType(ColorChannels).GetEnumName(ColorChannels.greenwhite))
                        Case "b"
                            allowonly.Add(GetType(ColorChannels).GetEnumName(ColorChannels.blueblack))
                        Case "B"
                            allowonly.Add(GetType(ColorChannels).GetEnumName(ColorChannels.bluewhite))
                        Case "u"
                            allowonly.Add(GetType(ColorChannels).GetEnumName(ColorChannels.hue))
                        Case "t"
                            allowonly.Add(GetType(ColorChannels).GetEnumName(ColorChannels.brightness))
                        Case "T"
                            allowonly.Add(GetType(ColorChannels).GetEnumName(ColorChannels.saturation))
                        Case "e"
                            allowonly.Add(GetType(ColorChannels).GetEnumName(ColorChannels.grayscale))
                        Case "f"
                            gnozip = True
                        Case "i"
                            allowonly.Add(GetType(ColorChannels).GetEnumName(ColorChannels.inverted))
                    End Select
                Next

            End If

            For Each itm As String In My.Application.CommandLineArgs
                If IO.Directory.Exists(itm) Then
                    Dim cdir As New IO.DirectoryInfo(itm)
                    For Each dfile As IO.FileInfo In cdir.GetFiles("*.*")
                        Select Case dfile.Extension.ToLower
                            Case ".png", ".jpg", ".bmp"
                                If dfile.Exists Then
                                    Try
                                        Dim gxr As New Threading.Thread(AddressOf splitImage)
                                        gxr.Start(New Object() {dfile.FullName, allowonly, gnozip})
                                    Catch ex As Exception

                                    End Try

                                End If

                        End Select

                    Next

                Else
                    If IO.File.Exists(itm) Then
                        Try
                            Dim gxr As New Threading.Thread(AddressOf splitImage)
                            gxr.Start(New Object() {itm, allowonly, gnozip})
                        Catch ex As Exception

                        End Try

                    End If
                End If
            Next

            While runningthreads > 0
                Threading.Thread.Sleep(250)
            End While

        End If
    End Sub

    Enum ColorChannels
        redblack
        blueblack
        greenblack
        redwhite
        bluewhite
        greenwhite
        alpha
        yellow
        cyan
        black
        magenta
        grayscale
        inverted
        saturation
        brightness
        hue


    End Enum

    Sub writearghelp()
        Dim barra As New ArrayList
        Dim commanlist As String = "aAcmykrRgGbButTe"
        Console.WriteLine(My.Application.Info.AssemblyName & " " & My.Application.Info.Version.ToString)
        Console.WriteLine("Useage (-options) arg1-argx ")
        Console.WriteLine("Args may be individual image files or directories, zipfiles if chosen or individual files will be placed in the same folder as original image")
        Console.WriteLine("--- OPTIONS ---")
        Console.WriteLine("-a Copies Alpha Channel")
        Console.WriteLine("-A Creates a color overlay in the color chosen")
        barra.Add(ColorChannels.alpha)
        barra.Add("")
        barra.Add(ColorChannels.cyan)
        barra.Add(ColorChannels.magenta)
        barra.Add(ColorChannels.yellow)
        barra.Add(ColorChannels.black)
        barra.Add(ColorChannels.redblack)
        barra.Add(ColorChannels.redwhite)
        barra.Add(ColorChannels.greenblack)
        barra.Add(ColorChannels.greenwhite)
        barra.Add(ColorChannels.blueblack)
        barra.Add(ColorChannels.bluewhite)
        barra.Add(ColorChannels.hue)
        barra.Add(ColorChannels.brightness)
        barra.Add(ColorChannels.saturation)
        barra.Add(ColorChannels.grayscale)

        For i As Integer = 2 To commanlist.Length - 1
            Console.WriteLine("-" & commanlist(i) & " Copies " & GetType(ColorChannels).GetEnumName(barra(i)) & " Channel")
        Next
        Console.WriteLine("-f copy files rather than zip them up")
        Console.WriteLine("-i invert image color")
        Console.WriteLine()
        Console.WriteLine("EX: " & My.Application.Info.AssemblyName & " -AcR image.jpg ""C:\imgdir"" ....")

    End Sub


    Sub splitImage(imgob As Object)

        Dim img As String = imgob(0)
        Dim allowances As ArrayList = imgob(1)
        Dim nozip As Boolean = imgob(2)
        runningthreads += 1
        Dim imgtosplit As New Drawing.Bitmap(img)
        Dim crk As New IO.FileInfo(img)
        Dim curimglist As New SortedList
        Dim roughimgcount As Integer = 1

        curimglist.Add(crk.Name.Replace(crk.Extension, " original " & crk.Extension), My.Computer.FileSystem.GetTempFileName)


        Try
            While IO.File.Exists(curimglist.GetByIndex(0))
                curimglist(curimglist.GetKey(0)) = My.Computer.FileSystem.GetTempFileName
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
            End If

            Dim cthread As New Threading.Thread(AddressOf splitIntochannels)

            Console.WriteLine("Processing " & crk.Name.Replace(crk.Extension, "") & "  " & GetType(ColorChannels).GetEnumName(eval) & " &  alphachannels")
            roughimgcount += 1
            cthread.Start(New Object() {imgtosplit.Clone, crk.Name.Replace(crk.Extension, "") & " " & GetType(ColorChannels).GetEnumName(eval) & ".png", eval, False, curimglist})
            If grabcoloralphas Then
                roughimgcount += 1
                Dim cthread2 As New Threading.Thread(AddressOf splitIntochannels)
                cthread2.Start(New Object() {imgtosplit.Clone, crk.Name.Replace(crk.Extension, "") & " " & GetType(ColorChannels).GetEnumName(eval) & " coloralpha.png", eval, True, curimglist})
            End If


        Next

        While (curimglist.Count <> roughimgcount) Or (roughimgcount = 1)
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

    Function convertToCMYKpercent(rgba As Drawing.Color) As SortedList
        Dim returnval As New SortedList
        Dim degreered As Long = rgba.R / 255
        Dim degreeblue As Long = rgba.B / 255
        Dim degreegreen As Long = rgba.G / 255

        returnval("black") = 1 - mathmax(New Double() {degreeblue, degreegreen, degreered})

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

    Function convertToGrayscale(rgba As Drawing.Color) As Drawing.Color

        Dim adjpercent As Double = 0.49
        Dim redcolor As Double = (rgba.R * (adjpercent - 0.2126))
        Dim bluecolor As Double = (rgba.B * (adjpercent - 0.0722))
        Dim greencolor As Double = (rgba.G * (adjpercent - 0.0752))
        Dim finalcolor As Double = redcolor + bluecolor + greencolor

        If finalcolor > 255 Then
            finalcolor = 255
        End If
        If finalcolor < 0 Then
            finalcolor = 0
        End If
        Return Drawing.Color.FromArgb(255, finalcolor, finalcolor, finalcolor)


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
        Dim bluefallingrange() As Integer = {300, 360}
        Dim bluerisingrange() As Integer = {120, 180}
        Dim greenfullrange() As Integer = {60, 180}
        Dim greenfallingrange() As Integer = {180, 240}
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
    Sub splitIntochannels(cob As Object)

        Dim pictosplit As Drawing.Bitmap = cob(0)
        Dim savename As String = cob(1)
        Dim chanselect As ColorChannels = cob(2)
        Dim usecoloralpha As Boolean = cob(3)
        Dim imglist As SortedList = cob(4)
        Dim alphadetected As Boolean = False
        Dim writecolor As New Drawing.Bitmap(pictosplit.Width, pictosplit.Height)
        Dim tfile As String = My.Computer.FileSystem.GetTempFileName

        Using dgui As Drawing.Graphics = Drawing.Graphics.FromImage(writecolor)
            If usecoloralpha Then
                writecolor.MakeTransparent()
            Else
                dgui.FillRectangle(Drawing.Brushes.White, 0, 0, writecolor.Width, writecolor.Height)
            End If

            For y As Integer = 0 To pictosplit.Height - 1
                For x As Integer = 0 To pictosplit.Width - 1
                    Dim cpix As Drawing.Color = pictosplit.GetPixel(x, y)
                    Dim colorcmyk As SortedList = convertToCMYKpercent(cpix)
                    Dim bwcolor As Drawing.Color = convertToGrayscale(cpix)
                    Dim colorwhitebalance As Drawing.Color = convertToWhiteBalance(cpix)
                    Dim colorhue As Drawing.Color = getColorOFHUE(cpix.GetHue)
                    Dim invertedpix As Drawing.Color = convertToinvert(cpix)
                    Select Case chanselect
                        Case ColorChannels.alpha
                            writecolor.SetPixel(x, y, Drawing.Color.FromArgb(255, cpix.A, cpix.A, cpix.A))

                            If cpix.A > 0 Then
                                alphadetected = True
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
                        Case ColorChannels.grayscale
                            writecolor.SetPixel(x, y, IIf(Not usecoloralpha, bwcolor, Drawing.Color.FromArgb(bwcolor.R, 0, 0, 0)))
                        Case ColorChannels.saturation
                            writecolor.SetPixel(x, y, IIf(Not usecoloralpha, Drawing.Color.FromArgb(255, cpix.GetSaturation * 255, cpix.GetSaturation * 255, cpix.GetSaturation * 255), Drawing.Color.FromArgb(cpix.GetSaturation * 255, 0, 0, 0)))
                        Case ColorChannels.brightness
                            writecolor.SetPixel(x, y, IIf(Not usecoloralpha, Drawing.Color.FromArgb(255, cpix.GetBrightness * 255, cpix.GetBrightness * 255, cpix.GetBrightness * 255), Drawing.Color.FromArgb(cpix.GetBrightness * 255, 0, 0, 0)))
                        Case ColorChannels.hue
                            writecolor.SetPixel(x, y, IIf(Not usecoloralpha, colorhue, colorhue))
                        Case ColorChannels.inverted
                            writecolor.SetPixel(x, y, IIf(Not usecoloralpha, invertedpix, invertedpix))
                        Case Else
                            imglist.Add(savename, Nothing)
                            writecolor.Dispose()

                            Exit Sub

                    End Select

                Next
            Next
        End Using

        Try

            Dim myEncoderParameters As New Drawing.Imaging.EncoderParameters(1)
            Dim myEncoder As System.Drawing.Imaging.Encoder = System.Drawing.Imaging.Encoder.Quality
            Dim myEncoderParameter As New System.Drawing.Imaging.EncoderParameter(myEncoder, 100&)
            myEncoderParameters.Param(0) = myEncoderParameter
            Dim pngencoder As Drawing.Imaging.ImageCodecInfo = GetEncoder(Drawing.Imaging.ImageFormat.Png)
            writecolor.Save(tfile, pngencoder, myEncoderParameters)
            Console.WriteLine("Wrote " & savename)
            imglist.Add(savename, tfile)
        Catch ex As Exception
            imglist.Add(savename, Nothing)
        End Try

    End Sub
    Private Function GetEncoder(ByVal format As Drawing.Imaging.ImageFormat) As Drawing.Imaging.ImageCodecInfo

        Dim codecs As Drawing.Imaging.ImageCodecInfo() = Drawing.Imaging.ImageCodecInfo.GetImageDecoders()

        Dim codec As Drawing.Imaging.ImageCodecInfo
        For Each codec In codecs
            If codec.FormatID = format.Guid Then
                Return codec
            End If
        Next codec
        Return Nothing

    End Function
End Module
