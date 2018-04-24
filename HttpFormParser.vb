Public Class HttpFormParser
    Implements IDisposable

    Dim currentFormContext As Net.HttpListenerContext
    Public Property currentMemorystream As New IO.MemoryStream

    Public Property FormFields As New SortedList
    Public Property Files As New SortedList

    Public Property CurrentContext As Net.HttpListenerContext
        Get
            Return currentFormContext
        End Get
        Set(value As Net.HttpListenerContext)
            currentFormContext = value

            While currentMemorystream.Position < currentFormContext.Request.ContentLength64
                currentMemorystream.WriteByte(currentFormContext.Request.InputStream.ReadByte)
            End While
            'parseFormAsync()
        End Set
    End Property


    Public Sub DumpFormSave(path As String)
        If currentFormContext IsNot Nothing Then
            Using srk As New IO.FileStream(path, IO.FileMode.Create)
                currentMemorystream.Position = 0
                currentMemorystream.CopyTo(srk)

                For Each itm As String In currentFormContext.Request.Headers.Keys
                    Dim cbuff As Byte() = Text.UTF8Encoding.UTF8.GetBytes(itm & " : " & currentFormContext.Request.Headers(itm) & vbCrLf)
                    srk.Write(cbuff, 0, cbuff.Length - 1)
                Next
                srk.Flush()
                srk.Close()
            End Using
        End If
    End Sub

    Public Sub parseForm()
        If currentFormContext Is Nothing Then
            Exit Sub

        End If
        FormFields = New SortedList
        Files = New SortedList
        currentMemorystream.Position = 0
        Dim txr As New IO.StreamReader(currentMemorystream)
        Dim parsestr As String = txr.ReadToEnd

        Dim contregex As New Text.RegularExpressions.Regex("((?:\-+[\d]+)\n*([\s\S]+?)\n*(?:\-+[\d]+))|((?:\-+[\d]+)\n*|([\s\S]+?)(?:\-+[\d]+))")
        Dim fnamereg As New Text.RegularExpressions.Regex("filename=""(.+?)""")
        Dim namereg As New Text.RegularExpressions.Regex("name=""(.+?)""")
        For Each itm As Text.RegularExpressions.Match In contregex.Matches(parsestr)
            If itm.Groups(2).Value <> "" Then
                Dim fileinteger As Integer = 0
                Dim ssplit As String() = itm.Groups(2).Value.Split(New String() {vbCrLf, vbLf}, StringSplitOptions.None)

                Dim filename As String = fnamereg.Match(ssplit(1)).Groups(1).Value
                Dim idname As String = namereg.Match(ssplit(1)).Groups(1).Value

                While Files.ContainsKey(fileinteger & idname)
                    fileinteger += 1
                End While

                Dim scrk(ssplit.Length - 4) As String

                For i As Integer = 4 To ssplit.Length - 1
                    scrk(i - 4) = ssplit(i)
                Next

                Dim joinedfile As String = String.Join("", scrk)
                Dim tpx As New IO.MemoryStream
                Dim fileasbyte As Byte() = Text.UTF8Encoding.UTF8.GetBytes(joinedfile)
                tpx.Write(fileasbyte, 0, fileasbyte.Length - 1)

                Dim outcont As New SortedList
                outcont.Add("filename", filename)
                outcont.Add("iostream", tpx)
                Files.Add(fileinteger & idname, outcont)
            Else
                If itm.Groups(4).Value <> "" Then
                    Dim ssplit As String() = itm.Groups(4).Value.Split(New String() {vbCrLf, vbLf}, StringSplitOptions.None)
                    Dim idname As String = namereg.Match(ssplit(1)).Groups(1).Value
                    Dim data As String = ssplit(3)

                    FormFields.Add(idname, data)
                End If
            End If

        Next



        RaiseEvent FormParsed(Me, FormFields)
    End Sub

    Public Sub parseFormAsync()
        Dim crf As New Threading.Thread(AddressOf parseForm)
        crf.Start()
    End Sub

    Public Event FormParsed(sender As Object, formfield As SortedList)

#Region "IDisposable Support"
    Private disposedValue As Boolean ' To detect redundant calls


    Protected Overridable Sub Dispose(disposing As Boolean)
        If Not disposedValue Then
            If disposing Then

            End If


        End If
        disposedValue = True
    End Sub


    Public Sub Dispose() Implements IDisposable.Dispose
        Dispose(True)
    End Sub
#End Region

End Class
