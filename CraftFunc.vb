﻿Imports System.Text
Imports System.Text.RegularExpressions

Module CraftFunc
    Function RemoveWhitespace(fullString As String) As String
        Return New String(fullString.Where(Function(x) Not Char.IsWhiteSpace(x)).ToArray())
    End Function

    Public Function Num(ByVal value As String) As Integer
        Dim returnVal As String = String.Empty
        Dim collection As MatchCollection = Regex.Matches(value, "\d+")
        For Each m As Match In collection
            returnVal += m.ToString()
        Next
        Return Convert.ToInt32(returnVal)
    End Function

    Private _rnd As New Random()
    Function RandomNumber(ByVal low As Integer, ByVal high As Integer) As Integer
        Return _rnd.Next(low, high)
    End Function

    Public Function Scramble(ByVal phrase As String) As String
        Static rand As New Random()
        Return New String(phrase.ToLower.ToCharArray.OrderBy(Function(r) rand.Next).ToArray)
    End Function

    Function Cap(ByVal val As String) As String
        If String.IsNullOrEmpty(val) Then
            Return val
        End If
        Dim array() As Char = val.ToCharArray
        array(0) = Char.ToUpper(array(0))
        Return New String(array)
    End Function

    Function CapWords(ByVal val As String) As String
        If String.IsNullOrEmpty(val) Then
            Return val
        End If
        Dim capText As String = Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(val)
        Return capText
    End Function

    Public Sub Colorize(ByVal msg As String)
        ' Checks the message for particular string and changes the color, then updates the log
        Select Case True
            Case msg.Contains("ERROR") ' error message
                Console.ForegroundColor = ConsoleColor.DarkRed ' errors are red
                Console.WriteLine(msg) ' update console
                Console.ResetColor() ' reset the color
            Case msg.Contains("INFO") ' repeat
                Console.ForegroundColor = ConsoleColor.DarkYellow
                Console.WriteLine(msg)
                Console.ResetColor()
            Case msg.Contains("GATEWAY")
                Console.ForegroundColor = ConsoleColor.DarkMagenta
                Console.WriteLine(msg)
                Console.ResetColor()
            Case msg.Contains("SQL")
                Console.ForegroundColor = ConsoleColor.DarkCyan
                Console.WriteLine(msg)
                Console.ResetColor()
            Case msg.Contains("SHARD")
                Console.ForegroundColor = ConsoleColor.DarkMagenta
                Console.WriteLine(msg)
                Console.ResetColor()
            Case msg.Contains("[SPAWN]")
                Console.ForegroundColor = ConsoleColor.Green
                Console.WriteLine(msg)
                Console.ResetColor()
            Case Else
                Console.ForegroundColor = ConsoleColor.White
                Console.WriteLine(msg)
                Console.ResetColor()
        End Select
    End Sub
    Public Function CalcExp(ByVal level As Integer) As Integer
        Dim exptolevel As Integer
        exptolevel = Math.Round((4 * (level ^ 3)) / 12)
        Return exptolevel
    End Function

    Public Function BagList(ByVal userID As String, ByVal page As Integer, ByVal filter As String) As StringBuilder
        Dim sBuilder As New StringBuilder
        Dim amt As Integer = 10
        Dim limit As Integer = amt * (page - 1)

        If page = 1 Then
            GetInventory(userID, "10", filter)
        Else
            GetInventoryPage(userID, limit, filter)
        End If

        For c As Integer = 0 To bagdata.Rows.Count - 1
            Dim itemName As String = bagdata.Rows(c)("itemName")
            Dim itemQty As Integer = bagdata.Rows(c)("itemQty")

            sBuilder.Append("[" & itemQty & "] " & itemName).AppendLine()
        Next

        Return sBuilder
    End Function

    Public Sub tickAsync(ByVal sender As Object, ByVal e As Timers.ElapsedEventArgs)
        If Date.Now.Hour = 0 Then
            ResetDaily
        End If
    End Sub
End Module
