Imports System.Data.SQLite
Imports System.Text

Module CraftSQL
    Public Function CheckUser(ByVal userID As String) As Boolean
        Dim userexists As Boolean = False

        sqlite_conn_users = New SQLiteConnection("Data Source=" & path & "\CraftUsers.db;Version=3;")
        Dim sqlite_cmd As SQLiteCommand

        ' open the connection:
        sqlite_conn_users.Open()

        sqlite_cmd = sqlite_conn_users.CreateCommand()

        sqlite_cmd.CommandText = "SELECT count(*) FROM players WHERE userID = @userID;"
        sqlite_cmd.Parameters.Add("@userID", SqlDbType.VarChar, 50).Value = userID

        Dim hasrows = Convert.ToInt32(sqlite_cmd.ExecuteScalar())

        If hasrows >= 1 Then
            userexists = True
        End If

        Colorize("[SQL]       [" & userID & "] | " & sqlite_cmd.CommandText)

        Return userexists
    End Function

    Public Function CheckPerm(ByVal userID As String) As Integer
        sqlite_conn_users = New SQLiteConnection("Data Source=" & path & "\CraftUsers.db;Version=3;")
        Dim sqlite_cmd As SQLiteCommand

        ' open the connection:
        sqlite_conn_users.Open()

        sqlite_cmd = sqlite_conn_users.CreateCommand()

        sqlite_cmd.CommandText = "SELECT count(*) FROM players WHERE userID = @userID;"
        sqlite_cmd.Parameters.Add("@userID", SqlDbType.VarChar, 50).Value = userID

        Dim accessLvl As Integer = sqlite_cmd.ExecuteScalar()

        Colorize("[SQL]       [" & userID & "] | " & sqlite_cmd.CommandText)

        Return accessLvl
    End Function

    Public Sub UpdatePermission(ByVal userid As String, ByVal rank As String)
        Try
            sqlite_conn_users = New SQLiteConnection("Data Source=" & path & "\CraftUsers.db;Version=3;")
            Dim sqlite_cmd As SQLiteCommand

            ' open the connection:
            sqlite_conn_users.Open()

            sqlite_cmd = sqlite_conn_users.CreateCommand()

            sqlite_cmd.CommandText = "UPDATE players SET accessLvl = @value WHERE userID = @user;"
            sqlite_cmd.Parameters.Add("@value", SqlDbType.Int).Value = rank
            sqlite_cmd.Parameters.Add("@user", SqlDbType.VarChar, 50).Value = userid
            sqlite_cmd.ExecuteNonQuery()

            Colorize("[SQL]       [" & userid & "] | " & sqlite_cmd.CommandText)
        Catch ex As Exception
            Colorize("[ERROR]     [" & userid & "] | " & ex.ToString)
        End Try
    End Sub

    Public Sub AddUser(ByVal userID As String, ByVal nick As String)
        sqlite_conn_users = New SQLiteConnection("Data Source=" & path & "\CraftUsers.db;Version=3;")
        Dim sqlite_cmd As SQLiteCommand
        nick = nick.Replace("'", " ")

        ' open the connection:
        sqlite_conn_users.Open()

        sqlite_cmd = sqlite_conn_users.CreateCommand()

        sqlite_cmd.CommandText = "INSERT INTO players (userID, userName, lastSmith, lastAlch) VALUES (@userID, @nick, @smith, @alch);"
        sqlite_cmd.Parameters.Add("@userID", SqlDbType.VarChar, 50).Value = userID
        sqlite_cmd.Parameters.Add("@nick", SqlDbType.VarChar, 50).Value = nick
        sqlite_cmd.Parameters.Add("@smith", SqlDbType.VarChar, 50).Value = Date.Now
        sqlite_cmd.Parameters.Add("@alch", SqlDbType.VarChar, 50).Value = Date.Now
        sqlite_cmd.ExecuteNonQuery()

        Colorize("[SQL]       [" & userID & "] | " & sqlite_cmd.CommandText)
    End Sub

    Public Function UserQuery(ByVal userID As String, ByVal clm As String) As String
        Try
            sqlite_conn_users = New SQLiteConnection("Data Source=" & path & "\CraftUsers.db;Version=3;")
            Dim sqlite_cmd As SQLiteCommand

            ' open the connection:
            sqlite_conn_users.Open()

            sqlite_cmd = sqlite_conn_users.CreateCommand()

            sqlite_cmd.CommandText = "SELECT " & clm & " FROM players WHERE userID = @userID;"
            sqlite_cmd.Parameters.Add("@userID", SqlDbType.VarChar, 50).Value = userID

            Dim profile As String = sqlite_cmd.ExecuteScalar()

            Colorize("[SQL]       [" & userID & "] | " & sqlite_cmd.CommandText)

            Return profile
        Catch ex As Exception
            Console.WriteLine(ex.ToString)
        End Try
    End Function

    Public Sub AddItem(ByVal userID As String, ByVal itemName As String, ByVal qty As Integer, ByVal itemType As String)
        sqlite_conn_users = New SQLiteConnection("Data Source=" & path & "\CraftUsers.db;Version=3;")
        Dim sqlite_cmd As SQLiteCommand

        ' open the connection:
        sqlite_conn_users.Open()

        sqlite_cmd = sqlite_conn_users.CreateCommand()

        sqlite_cmd.CommandText = "INSERT INTO inventory (userID, itemName, itemQty, itemType) VALUES (@userID, @itemName, @itemQty, @itemType);"
        sqlite_cmd.Parameters.Add("@userID", SqlDbType.VarChar, 50).Value = userID
        sqlite_cmd.Parameters.Add("@itemName", SqlDbType.VarChar, 50).Value = itemName
        sqlite_cmd.Parameters.Add("@itemType", SqlDbType.VarChar, 50).Value = itemType
        sqlite_cmd.Parameters.Add("@itemQty", SqlDbType.Int).Value = qty
        sqlite_cmd.ExecuteNonQuery()

        Colorize("[SQL]       [" & userID & "] | " & sqlite_cmd.CommandText)
    End Sub

    Public Sub RemoveItem(ByVal userID As String, ByVal itemName As String)
        sqlite_conn_users = New SQLiteConnection("Data Source=" & path & "\CraftUsers.db;Version=3;")
        Dim sqlite_cmd As SQLiteCommand

        ' open the connection:
        sqlite_conn_users.Open()

        sqlite_cmd = sqlite_conn_users.CreateCommand()

        sqlite_cmd.CommandText = "DELETE FROM inventory WHERE userID = @userID AND itemName = @itemName;"
        sqlite_cmd.Parameters.Add("@userID", SqlDbType.VarChar, 50).Value = userID
        sqlite_cmd.Parameters.Add("@itemName", SqlDbType.VarChar, 50).Value = itemName
        sqlite_cmd.ExecuteNonQuery()

        Colorize("[SQL]       [" & userID & "] | " & sqlite_cmd.CommandText)
    End Sub

    Public Sub UpdateInventory(ByVal userID As String, ByVal itemName As String, ByVal qty As Integer)
        Try
            sqlite_conn_users = New SQLiteConnection("Data Source=" & path & "\CraftUsers.db;Version=3;")
            Dim sqlite_cmd As SQLiteCommand

            ' open the connection:
            sqlite_conn_users.Open()

            sqlite_cmd = sqlite_conn_users.CreateCommand()

            If qty <= 0 Then
                sqlite_cmd.CommandText = "DELETE FROM inventory WHERE userID = @userID AND itemName = @itemName;"
                sqlite_cmd.Parameters.Add("@userID", SqlDbType.VarChar, 50).Value = userID
                sqlite_cmd.Parameters.Add("@itemName", SqlDbType.VarChar, 50).Value = itemName
                sqlite_cmd.ExecuteNonQuery()
            Else
                sqlite_cmd.CommandText = "UPDATE inventory SET itemQty = @itemQty WHERE userID = @userID AND itemName = @itemName;"
                sqlite_cmd.Parameters.Add("@userID", SqlDbType.VarChar, 50).Value = userID
                sqlite_cmd.Parameters.Add("@itemName", SqlDbType.VarChar, 50).Value = itemName
                sqlite_cmd.Parameters.Add("@itemQty", SqlDbType.Int).Value = qty
                sqlite_cmd.ExecuteNonQuery()
            End If

            Colorize("[SQL]       [" & userID & "] | " & sqlite_cmd.CommandText)
        Catch ex As Exception
            Colorize("[ERROR]     [" & userID & "] | " & ex.ToString)
        End Try
    End Sub

    Public Function CheckInventory(ByVal userID As String, ByVal itemname As String) As Boolean
        Dim itemexists As Boolean = False

        sqlite_conn_users = New SQLiteConnection("Data Source=" & path & "\CraftUsers.db;Version=3;")
        Dim sqlite_cmd As SQLiteCommand

        ' open the connection:
        sqlite_conn_users.Open()

        sqlite_cmd = sqlite_conn_users.CreateCommand()

        sqlite_cmd.CommandText = "SELECT count(*) FROM inventory WHERE userID = @userID AND itemName = @itemName;"
        sqlite_cmd.Parameters.Add("@userID", SqlDbType.VarChar, 50).Value = userID
        sqlite_cmd.Parameters.Add("@itemName", SqlDbType.VarChar, 50).Value = itemname

        Dim hasrows = Convert.ToInt32(sqlite_cmd.ExecuteScalar())

        If hasrows >= 1 Then
            itemexists = True
        End If

        Colorize("[SQL]       [" & userID & "] | " & sqlite_cmd.CommandText)

        Return itemexists
    End Function

    Public Sub UpdateUser(ByVal userID As String, ByVal clm As String, ByVal cval As String)
        Try
            sqlite_conn_users = New SQLiteConnection("Data Source=" & path & "\CraftUsers.db;Version=3;")
            Dim sqlite_cmd As SQLiteCommand

            ' open the connection:
            sqlite_conn_users.Open()

            sqlite_cmd = sqlite_conn_users.CreateCommand()

            sqlite_cmd.CommandText = "UPDATE players SET " & clm & " = @cval WHERE userID = @userID;"
            sqlite_cmd.Parameters.Add("@userID", SqlDbType.VarChar, 50).Value = userID
            sqlite_cmd.Parameters.Add("@cval", SqlDbType.VarChar, 50).Value = cval
            sqlite_cmd.ExecuteNonQuery()

            Colorize("[SQL]       [" & userID & "] | " & sqlite_cmd.CommandText)
        Catch ex As Exception
            Colorize("[ERROR]     [" & userID & "] | " & ex.ToString)
        End Try
    End Sub

    Public Function ItemQuery(ByVal itemName As String, ByVal clm As String) As String
        Try
            sqlite_conn_items = New SQLiteConnection("Data Source=" & path & "\CraftItems.db;Version=3;")
            Dim sqlite_cmd As SQLiteCommand

            ' open the connection:
            sqlite_conn_items.Open()

            sqlite_cmd = sqlite_conn_items.CreateCommand()

            sqlite_cmd.CommandText = "SELECT " & clm & " FROM items WHERE itemName = @itemName;"
            sqlite_cmd.Parameters.Add("@itemName", SqlDbType.VarChar, 50).Value = itemName

            Dim profile As String = sqlite_cmd.ExecuteScalar()

            Colorize("[SQL]       " & sqlite_cmd.CommandText)

            Return profile
        Catch ex As Exception
            Console.WriteLine(ex.ToString)
        End Try
    End Function

    Public Function InventoryQuery(ByVal userID As String, ByVal itemName As String, ByVal clm As String) As String
        Try
            sqlite_conn_users = New SQLiteConnection("Data Source=" & path & "\CraftUsers.db;Version=3;")
            Dim sqlite_cmd As SQLiteCommand

            ' open the connection:
            sqlite_conn_users.Open()

            sqlite_cmd = sqlite_conn_users.CreateCommand()

            sqlite_cmd.CommandText = "SELECT " & clm & " FROM inventory WHERE itemName = @itemName AND userID = @userID;"
            sqlite_cmd.Parameters.Add("@itemName", SqlDbType.VarChar, 50).Value = itemName
            sqlite_cmd.Parameters.Add("@userID", SqlDbType.VarChar, 50).Value = userID

            Dim profile As String = sqlite_cmd.ExecuteScalar()

            Colorize("[SQL]       " & sqlite_cmd.CommandText)

            Return profile
        Catch ex As Exception
            Console.WriteLine(ex.ToString)
        End Try
    End Function

    Public Sub GetInventory(ByVal userID As String, ByVal limit As String, ByVal con As String)
        sqlite_conn_users = New SQLiteConnection("Data Source=" & path & "\CraftUsers.db;Version=3;")
        Dim sqlite_cmd As SQLiteCommand

        ' open the connection:
        sqlite_conn_users.Open()

        sqlite_cmd = sqlite_conn_users.CreateCommand()

        Dim da As New SQLiteDataAdapter("SELECT * FROM inventory WHERE userID = '" & userID & "' AND itemType = '" & con & "' ORDER BY itemQTY DESC LIMIT " & limit & ";", sqlite_conn_users)
        bagdata.Clear()
        da.Fill(bagdata)
        da.Dispose()

        Colorize("[SQL]       [" & userID & "] | " & sqlite_cmd.CommandText)
    End Sub

    Public Sub GetInventoryPage(ByVal userID As String, ByVal limit As String, ByVal con As String)
        sqlite_conn_users = New SQLiteConnection("Data Source=" & path & "\CraftUsers.db;Version=3;")
        Dim sqlite_cmd As SQLiteCommand

        ' open the connection:
        sqlite_conn_users.Open()

        sqlite_cmd = sqlite_conn_users.CreateCommand()

        Dim da As New SQLiteDataAdapter("SELECT * FROM inventory WHERE userID = '" & userID & "' AND itemType = '" & con & "' ORDER BY itemQTY DESC LIMIT " & limit & ",10;", sqlite_conn_users)
        bagdata.Clear()
        da.Fill(bagdata)
        da.Dispose()

        Colorize("[SQL]       [" & userID & "] | " & sqlite_cmd.CommandText)
    End Sub

    Public Sub LoadCraftRanks(ByVal craftRank As Integer, ByVal craftType As String)
        Try
            sqlite_conn_items = New SQLiteConnection("Data Source=" & path & "\CraftItems.db;Version=3;")
            Dim sqlite_cmd As SQLiteCommand

            ' open the connection:
            sqlite_conn_items.Open()

            sqlite_cmd = sqlite_conn_items.CreateCommand()

            sqlite_cmd.CommandText = "SELECT itemName FROM items WHERE craftRank = @craft AND itemClass = @type;"
            sqlite_cmd.Parameters.Add("@craft", SqlDbType.Int).Value = craftRank
            sqlite_cmd.Parameters.Add("@type", SqlDbType.VarChar, 50).Value = craftType

            Dim dr = sqlite_cmd.ExecuteReader()
            While dr.Read
                If craftType = "Smithing" Then
                    Select Case craftRank
                        Case 1
                            smithItemListNovice.Add(dr("itemName").ToString)
                        Case 2
                            smithItemListApprentice.Add(dr("itemName").ToString)
                        Case 3
                            smithItemListAdept.Add(dr("itemName").ToString)
                        Case 4
                            smithItemListExpert.Add(dr("itemName").ToString)
                        Case 5
                            smithItemListMaster.Add(dr("itemName").ToString)
                    End Select
                Else
                    Select Case craftRank
                        Case 1
                            alchItemListNovice.Add(dr("itemName").ToString)
                        Case 2
                            alchItemListApprentice.Add(dr("itemName").ToString)
                        Case 3
                            alchItemListAdept.Add(dr("itemName").ToString)
                        Case 4
                            alchItemListExpert.Add(dr("itemName").ToString)
                        Case 5
                            alchItemListMaster.Add(dr("itemName").ToString)
                    End Select
                End If
            End While

            Colorize("[SQL]       " & sqlite_cmd.CommandText)
        Catch ex As Exception
            Colorize("[ERROR]     " & ex.ToString)
        End Try
    End Sub

    Public Function LoadCraftList(ByVal craftRank As Integer, ByVal craftType As String, ByVal limit As Integer) As StringBuilder
        Try
            sqlite_conn_items = New SQLiteConnection("Data Source=" & path & "\CraftItems.db;Version=3;")
            Dim sqlite_cmd As SQLiteCommand

            ' open the connection:
            sqlite_conn_items.Open()

            sqlite_cmd = sqlite_conn_items.CreateCommand()

            limit = 10 * (limit - 1)
            If craftType = "weapon" Then
                sqlite_cmd.CommandText = "SELECT itemName FROM items WHERE craftRank <= @craft AND craftRank > 0 AND itemType = 'weapon' AND itemRecipe != 'Uncraftable' LIMIT " & limit & ",10;"
            ElseIf craftType = "armor" Then
                sqlite_cmd.CommandText = "SELECT itemName FROM items WHERE craftRank <= @craft AND craftRank > 0 AND itemType = 'armor' AND itemRecipe != 'Uncraftable' LIMIT " & limit & ",10;"
            ElseIf craftType = "potion" Then
                sqlite_cmd.CommandText = "SELECT itemName FROM items WHERE craftRank <= @craft AND craftRank > 0 AND itemType = 'potion' AND itemRecipe != 'Uncraftable' LIMIT " & limit & ",10;"
            End If

            sqlite_cmd.Parameters.Add("@craft", SqlDbType.Int).Value = craftRank

            Dim dr = sqlite_cmd.ExecuteReader()
            Dim sbuilder As New StringBuilder
            While dr.Read
                sbuilder.Append(dr("itemName").ToString).AppendLine()
            End While

            Return sbuilder
            Colorize("[SQL]       " & sqlite_cmd.CommandText)
        Catch ex As Exception
            Colorize("[ERROR]     " & ex.ToString)
        End Try
    End Function

    Public Function LoadShopList(ByVal craftRank As Integer, ByVal craftType As String, ByVal limit As Integer) As StringBuilder
        Try
            sqlite_conn_items = New SQLiteConnection("Data Source=" & path & "\CraftItems.db;Version=3;")
            Dim sqlite_cmd As SQLiteCommand

            ' open the connection:
            sqlite_conn_items.Open()

            sqlite_cmd = sqlite_conn_items.CreateCommand()

            limit = 10 * (limit - 1)
            If craftType = "smithing" Then
                sqlite_cmd.CommandText = "SELECT * FROM items WHERE craftRank <= @craft AND craftRank > 0 AND itemType = 'material' AND itemClass = 'Smithing' LIMIT " & limit & ",10;"
            ElseIf craftType = "alchemy" Then
                sqlite_cmd.CommandText = "SELECT * FROM items WHERE craftRank <= @craft AND craftRank > 0 AND itemType = 'material' AND itemClass = 'Alchemy' LIMIT " & limit & ",10;"
            End If

            sqlite_cmd.Parameters.Add("@craft", SqlDbType.Int).Value = craftRank

            Dim dr = sqlite_cmd.ExecuteReader()
            Dim sbuilder As New StringBuilder
            While dr.Read
                sbuilder.Append("[" & goldIcon & " " & dr("itemCost").ToString & "] | " & dr("itemName").ToString).AppendLine()
            End While

            Return sbuilder
            Colorize("[SQL]       " & sqlite_cmd.CommandText)
        Catch ex As Exception
            Colorize("[ERROR]     " & ex.ToString)
        End Try
    End Function

    Public Sub ResetDaily()
        Try
            sqlite_conn_users = New SQLiteConnection("Data Source=" & path & "\CraftUsers.db;Version=3;")
            Dim sqlite_cmd As SQLiteCommand

            ' open the connection:
            sqlite_conn_users.Open()

            sqlite_cmd = sqlite_conn_users.CreateCommand()

            sqlite_cmd.CommandText = "UPDATE players SET daily = @value;"
            sqlite_cmd.Parameters.Add("@value", SqlDbType.VarChar, 50).Value = "false"
            sqlite_cmd.ExecuteNonQuery()

            Colorize("[SQL]       " & sqlite_cmd.CommandText)
        Catch ex As Exception
            Colorize("[ERROR]     " & ex.ToString)
        End Try
    End Sub
End Module
