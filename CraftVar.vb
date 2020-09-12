Imports System.Data.SQLite
Imports System.Windows.Forms

Module CraftVar
    Public path As String = Application.StartupPath
    Public MainTimer As New Timers.Timer

    Public sqlite_conn_users As SQLiteConnection
    Public sqlite_conn_items As SQLiteConnection

    Public bagdata As New DataTable

    Public alchItemListNovice As New List(Of String)
    Public alchItemListApprentice As New List(Of String)
    Public alchItemListAdept As New List(Of String)
    Public alchItemListExpert As New List(Of String)
    Public alchItemListMaster As New List(Of String)
    Public smithItemListNovice As New List(Of String)
    Public smithItemListApprentice As New List(Of String)
    Public smithItemListAdept As New List(Of String)
    Public smithItemListExpert As New List(Of String)
    Public smithItemListMaster As New List(Of String)

    Public goldIcon As String = "<:skyGold:753385199835938916>"
    Public arrowIcon As String = "<:arrow:753946284464799794>"
End Module
