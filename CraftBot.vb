Imports System.Runtime.InteropServices
Imports System.Text
Imports System.Text.RegularExpressions
Imports Discord
Imports Discord.WebSocket
Module CraftBot
    Sub Main(args As String())
        handler = New ConsoleEventDelegate(AddressOf ConsoleEventCallback)
        SetConsoleCtrlHandler(handler, True)
        ' Start main as an async sub
        MainAsync.GetAwaiter.GetResult()
    End Sub

    Public clientconfig As DiscordSocketConfig = New DiscordSocketConfig With {
        .TotalShards = 1
    }
    Public _client As DiscordShardedClient = New DiscordShardedClient(clientconfig)

    Sub New()
        ' Set console encoding for names with symbols like ♂️ and ♀️
        Console.OutputEncoding = Text.Encoding.UTF8
        ' Set our log, ready, timer, and message received functions
        AddHandler _client.Log, AddressOf LogAsync
        AddHandler _client.ShardReady, AddressOf ReadAsync
        AddHandler _client.MessageReceived, AddressOf MessageReceivedAsync
        AddHandler _client.ShardConnected, AddressOf ShardConnectedAsync
        AddHandler _client.ShardReady, AddressOf ShardReadyAsync
        AddHandler _client.UserJoined, AddressOf UserJoinedAsync
        AddHandler MainTimer.Elapsed, AddressOf tickAsync

        MainTimer.Interval = "60000"
        MainTimer.Start()
    End Sub

    <STAThread()>
    Public Async Function MainAsync() As Task
        ' Set thread
        Threading.Thread.CurrentThread.SetApartmentState(Threading.ApartmentState.STA)

        Await _client.LoginAsync(TokenType.Bot, "")

        ' Wait for the client to start
        Await _client.StartAsync
        Await Task.Delay(-1)
    End Function

    Private Async Function LogAsync(ByVal log As LogMessage) As Task(Of Task)
        ' Once loginasync and startasync finish we get the log message of "Ready" once we get that, we load everything else
        If log.ToString.Contains("Ready") Then
            Colorize("[GATEWAY]   " & log.ToString)

            LoadCraftRanks(1, "Smithing")
            LoadCraftRanks(2, "Smithing")
            LoadCraftRanks(3, "Smithing")
            LoadCraftRanks(4, "Smithing")
            LoadCraftRanks(5, "Smithing")
            LoadCraftRanks(1, "Alchemy")
            LoadCraftRanks(2, "Alchemy")
            LoadCraftRanks(3, "Alchemy")
            LoadCraftRanks(4, "Alchemy")
            LoadCraftRanks(5, "Alchemy")

            Await _client.SetGameAsync(" on " & _client.Guilds.Count & " servers.")
        ElseIf log.ToString.Contains("gateway") Or log.ToString.Contains("unhandled") Then
        Else
            Colorize("[GATEWAY]   " & log.ToString) ' update console
        End If
        Return Task.CompletedTask
    End Function

    ' Async reader
    Private Async Function ReadAsync() As Task(Of Task)
        Return Task.CompletedTask
    End Function

    Private Async Function ShardConnectedAsync(ByVal shard As DiscordSocketClient) As Task(Of Task)
        Colorize("[SHARD]     #" & shard.ShardId + 1 & " connected! Guilds: " & shard.Guilds.Count & " Users: " & shard.Guilds.Sum(Function(x) x.MemberCount))
        Return Task.CompletedTask
    End Function

    Private Async Function ShardReadyAsync(ByVal shard As DiscordSocketClient) As Task(Of Task)
        Colorize("[SHARD]     #" & shard.ShardId + 1 & " ready! Guilds: " & shard.Guilds.Count & " Users: " & shard.Guilds.Sum(Function(x) x.MemberCount))
        Return Task.CompletedTask
    End Function

    Private Async Function UserJoinedAsync(ByVal member As SocketGuildUser) As Task
        Try
            Dim guild As SocketGuild = member.Guild
            Dim d As Date = member.CreatedAt.DateTime
            Dim t As Date = Date.Now
            Dim avatarurl As String = ""
            If member.GetAvatarUrl IsNot Nothing Then
                avatarurl = member.GetAvatarUrl
            Else
                avatarurl = member.GetDefaultAvatarUrl
            End If

            If guild.Id = "733499986695684219" Then
                AddUser(member.Id, member.Username)

                Colorize("[INFO]      UserID: " & member.Id & " Joined the server.")

                Dim builder As EmbedBuilder = New EmbedBuilder
                builder.WithAuthor("SS Craft Bot", "https://imgur.com/VLGngdh.png")
                builder.WithThumbnailUrl("https://imgur.com/VLGngdh.png")
                builder.WithColor(219, 172, 69)

                builder.WithDescription(member.Mention & " **Joined** the server!" & Environment.NewLine &
                                        My.Settings.welcomeMsg)

                '                builder2.AddField("Vote:", "[Click Here!](https://top.gg/bot/681247510366650436/vote)", True)

                Await _client.GetGuild("733499986695684219").GetTextChannel("733505555150274610").SendMessageAsync("", False, builder.Build)

                'Dim role = _client.GetGuild("733499986695684219").Roles.FirstOrDefault(Function(x) x.Name = "💎 Alpha Tester")
                'Await member.AddRoleAsync(role)
            End If
        Catch ex As Exception
            Colorize("[ERROR]     " & ex.ToString)
        End Try
    End Function

    Private Async Function MessageReceivedAsync(ByVal message As SocketMessage) As Task
        If message.Author.IsBot = False Then
            Dim content As String = message.Content
            Dim author As IUser = message.Author
            Dim channel As IChannel = message.Channel
            Dim chn As SocketGuildChannel = message.Channel
            Dim guild As IGuild = chn.Guild
            Dim member As SocketGuildUser = author

            Dim prefix As String = My.Settings.prefix

            If content.ToLower.StartsWith(prefix) Then
                If CheckUser(author.Id) = False Then
                    AddUser(member.Id, member.Username)
                End If
                If content.ToLower.StartsWith(prefix & "ping") Then
                    Dim ping1 As String = _client.Shards(0).Latency
                    ' Dim ping2 As String = _client.Shards(1).Latency
                    Dim builder As EmbedBuilder = New EmbedBuilder
                    builder.WithAuthor("SS Craft Bot", "https://imgur.com/VLGngdh.png")
                    builder.WithThumbnailUrl("https://imgur.com/VLGngdh.png")
                    builder.WithColor(219, 172, 69)

                    builder.WithDescription("Shard #1: " & ping1 & "ms | Guilds: " & _client.Shards(0).Guilds.Count & " Users: " & _client.Shards(0).Guilds.Sum(Function(x) x.MemberCount))

                    Await message.Channel.SendMessageAsync("", False, builder.Build)

                    Colorize("[INFO]      Shard #1: " & ping1 & "ms | Guilds: " & _client.Shards(0).Guilds.Count & " Users: " & _client.Shards(0).Guilds.Sum(Function(x) x.MemberCount))
                ElseIf content.ToLower.StartsWith(prefix & "help") Then
                    Dim split As String() = content.Split(" ")
                    If split.Count > 1 Then
                        If split(1).ToLower = "settings" Then
                            Dim builder As EmbedBuilder = New EmbedBuilder
                            builder.WithAuthor("SS Craft Bot", "https://imgur.com/VLGngdh.png")
                            builder.WithThumbnailUrl("https://imgur.com/VLGngdh.png")
                            builder.WithColor(219, 172, 69)

                            builder.WithDescription("Please use `" & prefix & "help` to see the main help menu any time." & Environment.NewLine & Environment.NewLine &
                                                    "**Current Prefix:** `" & prefix & "`")

                            builder.AddField("**Change bot prefix:**", "`" & prefix & "prefix [new prefix]`", True)
                            builder.AddField("**Change bot welcome message:**", "`" & prefix & "wm [new message]`", True)
                            builder.AddField("**Give user bot mod/admin:**", "`" & prefix & "giveadmin [@user]`" & Environment.NewLine &
                                             "`" & prefix & "givemod [@user]`", True)
                            builder.AddField("**Remove user bot mod/admin:**", "`" & prefix & "takeadmin [@user]`" & Environment.NewLine &
                                             "`" & prefix & "takemod [@user]`", True)
                            builder.AddField("**Check Latency:**", "`" & prefix & "ping`", True)

                            Await message.Channel.SendMessageAsync("", False, builder.Build)
                        ElseIf split(1).ToLower = "player" Then
                            Dim builder As EmbedBuilder = New EmbedBuilder
                            builder.WithAuthor("SS Craft Bot", "https://imgur.com/VLGngdh.png")
                            builder.WithThumbnailUrl("https://imgur.com/VLGngdh.png")
                            builder.WithColor(219, 172, 69)

                            builder.WithDescription("Please use `" & prefix & "help` to see the main help menu any time." & Environment.NewLine & Environment.NewLine &
                                                    "**Current Prefix:** `" & prefix & "`")

                            builder.AddField("**View profile:**", "`" & prefix & "profile`", True)
                            builder.AddField("**Quick balance:**", "`" & prefix & "bal`", True)
                            builder.AddField("**Check inventory:**", "`" & prefix & "bag [type] [page]`", True)

                            Await message.Channel.SendMessageAsync("", False, builder.Build)

                        ElseIf split(1).ToLower = "resource" Then
                            Dim builder As EmbedBuilder = New EmbedBuilder
                            builder.WithAuthor("SS Craft Bot", "https://imgur.com/VLGngdh.png")
                            builder.WithThumbnailUrl("https://imgur.com/VLGngdh.png")
                            builder.WithColor(219, 172, 69)

                            builder.WithDescription("Please use `" & prefix & "help` to see the main help menu any time." & Environment.NewLine & Environment.NewLine &
                                                    "**Current Prefix:** `" & prefix & "`")

                            builder.AddField("**Get daily rewards:**", "`" & prefix & "daily`", True)
                            builder.AddField("**Search for smithing materials:**", "`" & prefix & "smith`", True)
                            builder.AddField("**Search for alchemy materials:**", "`" & prefix & "alch`", True)
                            builder.AddField("**Shop for materials:**", "`" & prefix & "shop [type] [page]`" & Environment.NewLine &
                                             "`" & prefix & "shop smith 1`" & Environment.NewLine &
                                             "`" & prefix & "shop alch 1`", True)

                            Await message.Channel.SendMessageAsync("", False, builder.Build)
                        ElseIf split(1).ToLower = "items" Then
                            Dim builder As EmbedBuilder = New EmbedBuilder
                            builder.WithAuthor("SS Craft Bot", "https://imgur.com/VLGngdh.png")
                            builder.WithThumbnailUrl("https://imgur.com/VLGngdh.png")
                            builder.WithColor(219, 172, 69)

                            builder.WithDescription("Please use `" & prefix & "help` to see the main help menu any time." & Environment.NewLine & Environment.NewLine &
                                                    "**Current Prefix:** `" & prefix & "`")

                            builder.AddField("**Get item information:**", "`" & prefix & "iteminfo [item name]`", True)
                            builder.AddField("**Get craft list:**", "`" & prefix & "craft list [type] [page]`" & Environment.NewLine &
                                             "`" & prefix & "craft list weapon 1`" & Environment.NewLine &
                                             "`" & prefix & "craft list armor 1`" & Environment.NewLine &
                                             "`" & prefix & "craft list potion 1`", True)
                            builder.AddField("**Craft an item:**", "`" & prefix & "craft item [item name]`", True)
                            builder.AddField("**Buy Materials:**", "`" & prefix & "buy [item name] [qty]`", True)
                            builder.AddField("**Sell Items:**", "`" & prefix & "sell [item name] [qty]`", True)

                            Await message.Channel.SendMessageAsync("", False, builder.Build)
                        End If
                    Else
                        Dim builder As EmbedBuilder = New EmbedBuilder
                        builder.WithAuthor("SS Craft Bot", "https://imgur.com/VLGngdh.png")
                        builder.WithThumbnailUrl("https://imgur.com/VLGngdh.png")
                        builder.WithColor(219, 172, 69)

                        builder.WithDescription("Please use `" & prefix & "help` to see this menu any time." & Environment.NewLine & Environment.NewLine &
                                                "**Current Prefix:** `" & prefix & "`")

                        builder.AddField("**Settings Help:**", "`" & prefix & "help settings`", True)
                        builder.AddField("**Player Help:**", "`" & prefix & "help player`", True)
                        builder.AddField("**Resource Gain Help:**", "`" & prefix & "help resource`", True)
                        builder.AddField("**General Item Help:**", "`" & prefix & "help items`", True)

                        Await message.Channel.SendMessageAsync("", False, builder.Build)
                    End If
                ElseIf content.ToLower.StartsWith(prefix & "prefix") Then
                    If member.Roles.Any(Function(role) role.Permissions.Administrator.Equals(True)) Then
                        Dim newprefix As String() = content.Split(" ")
                        If newprefix.Count > 0 Then
                            My.Settings.prefix = newprefix(1)
                            My.Settings.Save()

                            Dim builder As EmbedBuilder = New EmbedBuilder
                            builder.WithAuthor("SS Craft Bot", "https://imgur.com/VLGngdh.png")
                            builder.WithThumbnailUrl("https://imgur.com/VLGngdh.png")
                            builder.WithColor(219, 172, 69)

                            builder.WithDescription("The bot prefix has been updated!")

                            builder.AddField("**Current prefix: **", "`" & My.Settings.prefix & "`", True)

                            Await message.Channel.SendMessageAsync("", False, builder.Build)
                        End If
                    ElseIf CheckPerm(author.Id) = 100 Then
                        Dim newprefix As String() = content.Split(" ")
                        If newprefix.Count > 0 Then
                            My.Settings.prefix = newprefix(1)
                            My.Settings.Save()

                            Dim builder As EmbedBuilder = New EmbedBuilder
                            builder.WithAuthor("SS Craft Bot", "https://imgur.com/VLGngdh.png")
                            builder.WithThumbnailUrl("https://imgur.com/VLGngdh.png")
                            builder.WithColor(219, 172, 69)

                            builder.WithDescription("The bot prefix has been updated!")

                            builder.AddField("**Current prefix: **", "`" & My.Settings.prefix & "`", True)

                            Await message.Channel.SendMessageAsync("", False, builder.Build)
                        End If
                    Else
                        Dim builder As EmbedBuilder = New EmbedBuilder
                        builder.WithAuthor("SS Craft Bot", "https://imgur.com/VLGngdh.png")
                        builder.WithThumbnailUrl("https://imgur.com/VLGngdh.png")
                        builder.WithColor(111, 33, 39)

                        builder.WithDescription("I'm sorry, but you do not have permission to do that.")

                        Await message.Channel.SendMessageAsync("", False, builder.Build)
                    End If
                ElseIf content.ToLower.StartsWith(prefix & "exprate") Then
                    If member.Roles.Any(Function(role) role.Permissions.Administrator.Equals(True)) Then
                        Dim newexp As Integer = Num(content)
                        If newexp > 0 Then
                            My.Settings.exprate = newexp
                            My.Settings.Save()

                            Dim builder As EmbedBuilder = New EmbedBuilder
                            builder.WithAuthor("SS Craft Bot", "https://imgur.com/VLGngdh.png")
                            builder.WithThumbnailUrl("https://imgur.com/VLGngdh.png")
                            builder.WithColor(219, 172, 69)

                            builder.WithDescription("The bot exp rate has been updated!")

                            builder.AddField("**Current exp rate: **", "`" & My.Settings.exprate & "`", True)

                            Await message.Channel.SendMessageAsync("", False, builder.Build)
                        End If
                    ElseIf CheckPerm(author.Id) = 100 Then
                        Dim newexp As Integer = Num(content)
                        If newexp > 0 Then
                            My.Settings.exprate = newexp
                            My.Settings.Save()

                            Dim builder As EmbedBuilder = New EmbedBuilder
                            builder.WithAuthor("SS Craft Bot", "https://imgur.com/VLGngdh.png")
                            builder.WithThumbnailUrl("https://imgur.com/VLGngdh.png")
                            builder.WithColor(219, 172, 69)

                            builder.WithDescription("The bot exp rate has been updated!")

                            builder.AddField("**Current exp rate: **", "`" & My.Settings.exprate & "`", True)

                            Await message.Channel.SendMessageAsync("", False, builder.Build)
                        End If
                    Else
                        Dim builder As EmbedBuilder = New EmbedBuilder
                        builder.WithAuthor("SS Craft Bot", "https://imgur.com/VLGngdh.png")
                        builder.WithThumbnailUrl("https://imgur.com/VLGngdh.png")
                        builder.WithColor(111, 33, 39)

                        builder.WithDescription("I'm sorry, but you do not have permission to do that.")

                        Await message.Channel.SendMessageAsync("", False, builder.Build)
                    End If
                ElseIf content.ToLower.StartsWith(prefix & "wc") Then
                    If member.Roles.Any(Function(role) role.Permissions.Administrator.Equals(True)) Then
                        Dim newmsg As String() = content.Split(" ")
                        If newmsg.Count > 0 Then
                            My.Settings.welcomeMsg = newmsg(1)
                            My.Settings.Save()

                            Dim builder As EmbedBuilder = New EmbedBuilder
                            builder.WithAuthor("SS Craft Bot", "https://imgur.com/VLGngdh.png")
                            builder.WithThumbnailUrl("https://imgur.com/VLGngdh.png")
                            builder.WithColor(219, 172, 69)

                            builder.WithDescription("The bot welcome message has been updated!")

                            builder.AddField("**Current welcome message: **", "`" & My.Settings.welcomeMsg & "`", True)

                            Await message.Channel.SendMessageAsync("", False, builder.Build)
                        End If
                    ElseIf CheckPerm(author.Id) = 100 Then
                        Dim newmsg As String() = content.Split(" ")
                        If newmsg.Count > 0 Then
                            My.Settings.welcomeMsg = newmsg(1)
                            My.Settings.Save()

                            Dim builder As EmbedBuilder = New EmbedBuilder
                            builder.WithAuthor("SS Craft Bot", "https://imgur.com/VLGngdh.png")
                            builder.WithThumbnailUrl("https://imgur.com/VLGngdh.png")
                            builder.WithColor(219, 172, 69)

                            builder.WithDescription("The bot welcome message has been updated!")

                            builder.AddField("**Current welcome message: **", "`" & My.Settings.welcomeMsg & "`", True)

                            Await message.Channel.SendMessageAsync("", False, builder.Build)
                        End If
                    Else
                        Dim builder As EmbedBuilder = New EmbedBuilder
                        builder.WithAuthor("SS Craft Bot", "https://imgur.com/VLGngdh.png")
                        builder.WithThumbnailUrl("https://imgur.com/VLGngdh.png")
                        builder.WithColor(111, 33, 39)

                        builder.WithDescription("I'm sorry, but you do not have permission to do that.")

                        Await message.Channel.SendMessageAsync("", False, builder.Build)
                    End If
                ElseIf content.ToLower.StartsWith(prefix & "giveadmin") Then
                    If member.Roles.Any(Function(role) role.Permissions.Administrator.Equals(True)) Then
                        If message.MentionedUsers.Count > 0 Then
                            UpdatePermission(message.MentionedUsers.FirstOrDefault.Id, "100")
                            Dim builder As EmbedBuilder = New EmbedBuilder
                            builder.WithAuthor("SS Craft Bot", "https://imgur.com/VLGngdh.png")
                            builder.WithThumbnailUrl("https://imgur.com/VLGngdh.png")
                            builder.WithColor(219, 172, 69)

                            builder.WithDescription("Permissions updated!")

                            builder.AddField("**Admin Granted:**", "`" & message.MentionedUsers.FirstOrDefault.Username & "`", True)

                            Await message.Channel.SendMessageAsync("", False, builder.Build)
                        End If
                    ElseIf CheckPerm(author.Id) = 100 Then
                        If message.MentionedUsers.Count > 0 Then
                            UpdatePermission(message.MentionedUsers.FirstOrDefault.Id, "100")
                            Dim builder As EmbedBuilder = New EmbedBuilder
                            builder.WithAuthor("SS Craft Bot", "https://imgur.com/VLGngdh.png")
                            builder.WithThumbnailUrl("https://imgur.com/VLGngdh.png")
                            builder.WithColor(219, 172, 69)

                            builder.WithDescription("Permissions updated!")

                            builder.AddField("**Admin Granted:**", "`" & message.MentionedUsers.FirstOrDefault.Username & "`", True)

                            Await message.Channel.SendMessageAsync("", False, builder.Build)
                        End If
                    Else
                        Dim builder As EmbedBuilder = New EmbedBuilder
                        builder.WithAuthor("SS Craft Bot", "https://imgur.com/VLGngdh.png")
                        builder.WithThumbnailUrl("https://imgur.com/VLGngdh.png")
                        builder.WithColor(111, 33, 39)

                        builder.WithDescription("I'm sorry, but you do not have permission to do that.")

                        Await message.Channel.SendMessageAsync("", False, builder.Build)
                    End If
                ElseIf content.ToLower.StartsWith(prefix & "givemod") Then
                    If member.Roles.Any(Function(role) role.Permissions.Administrator.Equals(True)) Then
                        If message.MentionedUsers.Count > 0 Then
                            UpdatePermission(message.MentionedUsers.FirstOrDefault.Id, "50")
                            Dim builder As EmbedBuilder = New EmbedBuilder
                            builder.WithAuthor("SS Craft Bot", "https://imgur.com/VLGngdh.png")
                            builder.WithThumbnailUrl("https://imgur.com/VLGngdh.png")
                            builder.WithColor(219, 172, 69)

                            builder.WithDescription("Permissions updated!")

                            builder.AddField("**Mod Granted:**", "`" & message.MentionedUsers.FirstOrDefault.Username & "`", True)

                            Await message.Channel.SendMessageAsync("", False, builder.Build)
                        End If
                    ElseIf CheckPerm(author.Id) = 100 Then
                        If message.MentionedUsers.Count > 0 Then
                            UpdatePermission(message.MentionedUsers.FirstOrDefault.Id, "50")
                            Dim builder As EmbedBuilder = New EmbedBuilder
                            builder.WithAuthor("SS Craft Bot", "https://imgur.com/VLGngdh.png")
                            builder.WithThumbnailUrl("https://imgur.com/VLGngdh.png")
                            builder.WithColor(219, 172, 69)

                            builder.WithDescription("Permissions updated!")

                            builder.AddField("**Mod Granted:**", "`" & message.MentionedUsers.FirstOrDefault.Username & "`", True)

                            Await message.Channel.SendMessageAsync("", False, builder.Build)
                        End If
                    Else
                        Dim builder As EmbedBuilder = New EmbedBuilder
                        builder.WithAuthor("SS Craft Bot", "https://imgur.com/VLGngdh.png")
                        builder.WithThumbnailUrl("https://imgur.com/VLGngdh.png")
                        builder.WithColor(111, 33, 39)

                        builder.WithDescription("I'm sorry, but you do not have permission to do that.")

                        Await message.Channel.SendMessageAsync("", False, builder.Build)
                    End If
                ElseIf content.ToLower.StartsWith(prefix & "takeadmin") Then
                    If member.Roles.Any(Function(role) role.Permissions.Administrator.Equals(True)) Then
                        If message.MentionedUsers.Count > 0 Then
                            UpdatePermission(message.MentionedUsers.FirstOrDefault.Id, "0")
                            Dim builder As EmbedBuilder = New EmbedBuilder
                            builder.WithAuthor("SS Craft Bot", "https://imgur.com/VLGngdh.png")
                            builder.WithThumbnailUrl("https://imgur.com/VLGngdh.png")
                            builder.WithColor(219, 172, 69)

                            builder.WithDescription("Permissions updated!")

                            builder.AddField("**Admin Stripped:**", "`" & message.MentionedUsers.FirstOrDefault.Username & "`", True)

                            Await message.Channel.SendMessageAsync("", False, builder.Build)
                        End If
                    ElseIf CheckPerm(author.Id) = 100 Then
                        If message.MentionedUsers.Count > 0 Then
                            UpdatePermission(message.MentionedUsers.FirstOrDefault.Id, "0")
                            Dim builder As EmbedBuilder = New EmbedBuilder
                            builder.WithAuthor("SS Craft Bot", "https://imgur.com/VLGngdh.png")
                            builder.WithThumbnailUrl("https://imgur.com/VLGngdh.png")
                            builder.WithColor(219, 172, 69)

                            builder.WithDescription("Permissions updated!")

                            builder.AddField("**Admin Stripped:**", "`" & message.MentionedUsers.FirstOrDefault.Username & "`", True)

                            Await message.Channel.SendMessageAsync("", False, builder.Build)
                        End If
                    Else
                        Dim builder As EmbedBuilder = New EmbedBuilder
                        builder.WithAuthor("SS Craft Bot", "https://imgur.com/VLGngdh.png")
                        builder.WithThumbnailUrl("https://imgur.com/VLGngdh.png")
                        builder.WithColor(111, 33, 39)

                        builder.WithDescription("I'm sorry, but you do not have permission to do that.")

                        Await message.Channel.SendMessageAsync("", False, builder.Build)
                    End If
                ElseIf content.ToLower.StartsWith(prefix & "takemod") Then
                    If member.Roles.Any(Function(role) role.Permissions.Administrator.Equals(True)) Then
                        If message.MentionedUsers.Count > 0 Then
                            UpdatePermission(message.MentionedUsers.FirstOrDefault.Id, "0")
                            Dim builder As EmbedBuilder = New EmbedBuilder
                            builder.WithAuthor("SS Craft Bot", "https://imgur.com/VLGngdh.png")
                            builder.WithThumbnailUrl("https://imgur.com/VLGngdh.png")
                            builder.WithColor(219, 172, 69)

                            builder.WithDescription("Permissions updated!")

                            builder.AddField("**Mod Stripped:**", "`" & message.MentionedUsers.FirstOrDefault.Username & "`", True)

                            Await message.Channel.SendMessageAsync("", False, builder.Build)
                        End If
                    ElseIf CheckPerm(author.Id) = 100 Then
                        If message.MentionedUsers.Count > 0 Then
                            UpdatePermission(message.MentionedUsers.FirstOrDefault.Id, "0")
                            Dim builder As EmbedBuilder = New EmbedBuilder
                            builder.WithAuthor("SS Craft Bot", "https://imgur.com/VLGngdh.png")
                            builder.WithThumbnailUrl("https://imgur.com/VLGngdh.png")
                            builder.WithColor(219, 172, 69)

                            builder.WithDescription("Permissions updated!")

                            builder.AddField("**Mod Stripped:**", "`" & message.MentionedUsers.FirstOrDefault.Username & "`", True)

                            Await message.Channel.SendMessageAsync("", False, builder.Build)
                        End If
                    Else
                        Dim builder As EmbedBuilder = New EmbedBuilder
                        builder.WithAuthor("SS Craft Bot", "https://imgur.com/VLGngdh.png")
                        builder.WithThumbnailUrl("https://imgur.com/VLGngdh.png")
                        builder.WithColor(111, 33, 39)

                        builder.WithDescription("I'm sorry, but you do not have permission to do that.")

                        Await message.Channel.SendMessageAsync("", False, builder.Build)
                    End If
                ElseIf content.ToLower.StartsWith(prefix & "bal") Then
                    Dim bal As Integer = UserQuery(author.Id, "gold")
                    Dim builder As EmbedBuilder = New EmbedBuilder
                    builder.WithAuthor("SS Craft Bot", "https://imgur.com/VLGngdh.png")
                    builder.WithThumbnailUrl("https://imgur.com/VLGngdh.png")
                    builder.WithColor(219, 172, 69)

                    builder.WithDescription(author.Mention & " your current balance is: " & goldIcon & " " & bal)

                    Await message.Channel.SendMessageAsync("", False, builder.Build)
                ElseIf content.ToLower.StartsWith(prefix & "profile") Then
                    Dim bal As Integer = UserQuery(author.Id, "gold")
                    Dim userName As String = UserQuery(author.Id, "userName")
                    Dim userLvl As Integer = UserQuery(author.Id, "lvl")
                    Dim userExp As Integer = UserQuery(author.Id, "exp")
                    Dim smithLvl As Integer = UserQuery(author.Id, "smithLvl")
                    Dim smithExp As Integer = UserQuery(author.Id, "smithExp")
                    Dim alchLvl As Integer = UserQuery(author.Id, "alchemyLvl")
                    Dim alchExp As Integer = UserQuery(author.Id, "alchemyExp")
                    Dim dailyClaimed As String = UserQuery(author.Id, "daily")
                    Dim accessLvl As Integer = UserQuery(author.Id, "accessLvl")
                    Dim avatar As String = ""

                    Dim builder As EmbedBuilder = New EmbedBuilder
                    builder.WithAuthor(userName & "'s Profile", "https://imgur.com/VLGngdh.png")

                    If Not author.GetAvatarUrl Is Nothing Then
                        avatar = author.GetAvatarUrl
                    Else
                        avatar = author.GetDefaultAvatarUrl
                    End If

                    builder.WithThumbnailUrl(avatar)
                    builder.WithColor(219, 172, 69)

                    Dim expString As String
                    Dim expPercent As Double = Math.Round(userExp / CalcExp(userLvl + 1) * 100, 2)
                    If expPercent <= 9.9 Then
                        expString = "▱▱▱▱▱▱▱▱▱▱ " & expPercent & "% EXP"
                    ElseIf expPercent >= 10.0 AndAlso expPercent <= 19.99 Then
                        expString = "▰▱▱▱▱▱▱▱▱▱ " & expPercent & "% EXP"
                    ElseIf expPercent >= 20.0 AndAlso expPercent <= 29.99 Then
                        expString = "▰▰▱▱▱▱▱▱▱▱ " & expPercent & "% EXP"
                    ElseIf expPercent >= 30.0 AndAlso expPercent <= 39.99 Then
                        expString = "▰▰▰▱▱▱▱▱▱▱ " & expPercent & "% EXP"
                    ElseIf expPercent >= 40.0 AndAlso expPercent <= 49.99 Then
                        expString = "▰▰▰▰▱▱▱▱▱▱ " & expPercent & "% EXP"
                    ElseIf expPercent >= 50.0 AndAlso expPercent <= 59.99 Then
                        expString = "▰▰▰▰▰▱▱▱▱▱ " & expPercent & "% EXP"
                    ElseIf expPercent >= 60.0 AndAlso expPercent <= 69.99 Then
                        expString = "▰▰▰▰▰▰▱▱▱▱ " & expPercent & "% EXP"
                    ElseIf expPercent >= 70.0 AndAlso expPercent <= 79.99 Then
                        expString = "▰▰▰▰▰▰▰▱▱▱ " & expPercent & "% EXP"
                    ElseIf expPercent >= 80.0 AndAlso expPercent <= 89.99 Then
                        expString = "▰▰▰▰▰▰▰▰▱▱ " & expPercent & "% EXP"
                    ElseIf expPercent >= 90.0 AndAlso expPercent <= 99.99 Then
                        expString = "▰▰▰▰▰▰▰▰▰▱ " & expPercent & "% EXP"
                    Else
                        expString = "▰▰▰▰▰▰▰▰▰▰ " & expPercent & "% EXP"
                    End If

                    Dim smithExpString As String
                    Dim smithExpPercent As Double = Math.Round(smithExp / CalcExp(smithLvl + 1) * 100, 2)
                    If smithExpPercent <= 9.9 Then
                        smithExpString = "▱▱▱▱▱▱▱▱▱▱ " & smithExpPercent & "% EXP"
                    ElseIf smithExpPercent >= 10.0 AndAlso smithExpPercent <= 19.99 Then
                        smithExpString = "▰▱▱▱▱▱▱▱▱▱ " & smithExpPercent & "% EXP"
                    ElseIf smithExpPercent >= 20.0 AndAlso smithExpPercent <= 29.99 Then
                        smithExpString = "▰▰▱▱▱▱▱▱▱▱ " & smithExpPercent & "% EXP"
                    ElseIf smithExpPercent >= 30.0 AndAlso smithExpPercent <= 39.99 Then
                        smithExpString = "▰▰▰▱▱▱▱▱▱▱ " & smithExpPercent & "% EXP"
                    ElseIf smithExpPercent >= 40.0 AndAlso smithExpPercent <= 49.99 Then
                        smithExpString = "▰▰▰▰▱▱▱▱▱▱ " & smithExpPercent & "% EXP"
                    ElseIf smithExpPercent >= 50.0 AndAlso smithExpPercent <= 59.99 Then
                        smithExpString = "▰▰▰▰▰▱▱▱▱▱ " & smithExpPercent & "% EXP"
                    ElseIf smithExpPercent >= 60.0 AndAlso smithExpPercent <= 69.99 Then
                        smithExpString = "▰▰▰▰▰▰▱▱▱▱ " & smithExpPercent & "% EXP"
                    ElseIf smithExpPercent >= 70.0 AndAlso smithExpPercent <= 79.99 Then
                        smithExpString = "▰▰▰▰▰▰▰▱▱▱ " & smithExpPercent & "% EXP"
                    ElseIf smithExpPercent >= 80.0 AndAlso smithExpPercent <= 89.99 Then
                        smithExpString = "▰▰▰▰▰▰▰▰▱▱ " & smithExpPercent & "% EXP"
                    ElseIf smithExpPercent >= 90.0 AndAlso smithExpPercent <= 99.99 Then
                        smithExpString = "▰▰▰▰▰▰▰▰▰▱ " & smithExpPercent & "% EXP"
                    Else
                        smithExpString = "▰▰▰▰▰▰▰▰▰▰ " & smithExpPercent & "% EXP"
                    End If

                    Dim alchExpString As String
                    Dim alchExpPercent As Double = Math.Round(alchExp / CalcExp(alchLvl + 1) * 100, 2)
                    If alchExpPercent <= 9.9 Then
                        alchExpString = "▱▱▱▱▱▱▱▱▱▱ " & alchExpPercent & "% EXP"
                    ElseIf alchExpPercent >= 10.0 AndAlso alchExpPercent <= 19.99 Then
                        alchExpString = "▰▱▱▱▱▱▱▱▱▱ " & alchExpPercent & "% EXP"
                    ElseIf alchExpPercent >= 20.0 AndAlso alchExpPercent <= 29.99 Then
                        alchExpString = "▰▰▱▱▱▱▱▱▱▱ " & alchExpPercent & "% EXP"
                    ElseIf alchExpPercent >= 30.0 AndAlso alchExpPercent <= 39.99 Then
                        alchExpString = "▰▰▰▱▱▱▱▱▱▱ " & alchExpPercent & "% EXP"
                    ElseIf alchExpPercent >= 40.0 AndAlso alchExpPercent <= 49.99 Then
                        alchExpString = "▰▰▰▰▱▱▱▱▱▱ " & alchExpPercent & "% EXP"
                    ElseIf alchExpPercent >= 50.0 AndAlso alchExpPercent <= 59.99 Then
                        alchExpString = "▰▰▰▰▰▱▱▱▱▱ " & alchExpPercent & "% EXP"
                    ElseIf alchExpPercent >= 60.0 AndAlso alchExpPercent <= 69.99 Then
                        alchExpString = "▰▰▰▰▰▰▱▱▱▱ " & alchExpPercent & "% EXP"
                    ElseIf alchExpPercent >= 70.0 AndAlso alchExpPercent <= 79.99 Then
                        alchExpString = "▰▰▰▰▰▰▰▱▱▱ " & alchExpPercent & "% EXP"
                    ElseIf alchExpPercent >= 80.0 AndAlso alchExpPercent <= 89.99 Then
                        alchExpString = "▰▰▰▰▰▰▰▰▱▱ " & alchExpPercent & "% EXP"
                    ElseIf alchExpPercent >= 90.0 AndAlso alchExpPercent <= 99.99 Then
                        alchExpString = "▰▰▰▰▰▰▰▰▰▱ " & alchExpPercent & "% EXP"
                    Else
                        alchExpString = "▰▰▰▰▰▰▰▰▰▰ " & alchExpPercent & "% EXP"
                    End If

                    builder.WithDescription("**Level: " & userLvl & "**" & Environment.NewLine &
                                            "`" & expString & "`")

                    builder.AddField("**Smithing:**", "**Level: " & smithLvl & "**" & Environment.NewLine &
                                            "`" & smithExpString & "`", True)

                    builder.AddField("**Alchemy:**", "**Level: " & alchLvl & "**" & Environment.NewLine &
                                            "`" & alchExpString & "`", True)

                    builder.AddField("**Balance:**", goldIcon & " " & bal, False)

                    Await message.Channel.SendMessageAsync("", False, builder.Build)
                ElseIf content.ToLower.StartsWith(prefix & "daily") Then
                    If UserQuery(author.Id, "daily") = "false" Then
                        Dim ranSmith As Integer = 0
                        Dim ranAlch As Integer = 0
                        Dim smithbuilder As New StringBuilder
                        Dim alchbuilder As New StringBuilder
                        Dim listSmith As New List(Of String)
                        Dim listalch As New List(Of String)
                        Dim smithRank As Integer = 0
                        Dim userSmith As Integer = UserQuery(author.Id, "smithLvl")
                        Dim alchRank As Integer = 0
                        Dim userAlch As Integer = UserQuery(author.Id, "alchemyLvl")
                        If userSmith <= 9 Then
                            smithRank = 1
                        ElseIf userSmith >= 10 AndAlso userSmith <= 19 Then
                            smithRank = 2
                        ElseIf userSmith >= 20 AndAlso userSmith <= 49 Then
                            smithRank = 3
                        ElseIf userSmith >= 50 AndAlso userSmith <= 99 Then
                            smithRank = 4
                        ElseIf userSmith = 100 Then
                            smithRank = 5
                        End If
                        If userAlch <= 9 Then
                            alchRank = 1
                        ElseIf userAlch >= 10 AndAlso userAlch <= 19 Then
                            alchRank = 2
                        ElseIf userAlch >= 20 AndAlso userAlch <= 49 Then
                            alchRank = 3
                        ElseIf userAlch >= 50 AndAlso userAlch <= 99 Then
                            alchRank = 4
                        ElseIf userAlch = 100 Then
                            alchRank = 5
                        End If
                        For i As Integer = 0 To 4
                            Select Case smithRank
                                Case 1
                                    ranSmith = RandomNumber(0, smithItemListNovice.Count - 1)
                                    listSmith.Add(smithItemListNovice(ranSmith))
                                Case 2
                                    ranSmith = RandomNumber(0, smithItemListApprentice.Count - 1)
                                    listSmith.Add(smithItemListApprentice(ranSmith))
                                Case 3
                                    ranSmith = RandomNumber(0, smithItemListAdept.Count - 1)
                                    listSmith.Add(smithItemListAdept(ranSmith))
                                Case 4
                                    ranSmith = RandomNumber(0, smithItemListExpert.Count - 1)
                                    listSmith.Add(smithItemListExpert(ranSmith))
                                Case 5
                                    ranSmith = RandomNumber(0, smithItemListMaster.Count - 1)
                                    listSmith.Add(smithItemListMaster(ranSmith))
                            End Select
                            Select Case alchRank
                                Case 1
                                    ranAlch = RandomNumber(0, alchItemListNovice.Count - 1)
                                    listalch.Add(alchItemListNovice(ranAlch))
                                Case 2
                                    ranAlch = RandomNumber(0, alchItemListApprentice.Count - 1)
                                    listalch.Add(alchItemListApprentice(ranAlch))
                                Case 3
                                    ranAlch = RandomNumber(0, alchItemListAdept.Count - 1)
                                    listalch.Add(alchItemListAdept(ranAlch))
                                Case 4
                                    ranAlch = RandomNumber(0, alchItemListExpert.Count - 1)
                                    listalch.Add(alchItemListExpert(ranAlch))
                                Case 5
                                    ranAlch = RandomNumber(0, alchItemListMaster.Count - 1)
                                    listalch.Add(alchItemListMaster(ranAlch))
                            End Select
                        Next

                        Dim smithGroupedNames = listSmith.GroupBy(Function(x) x)
                        Dim alchGroupedNames = listalch.GroupBy(Function(x) x)
                        If smithGroupedNames IsNot Nothing AndAlso smithGroupedNames.Count > 0 Then
                            For Each item In smithGroupedNames
                                smithbuilder.Append("[" & item.Count.ToString & "] " & item.Key).AppendLine()
                                If CheckInventory(author.Id, item.Key) Then
                                    Dim userinvqty As Integer = InventoryQuery(author.Id, item.Key, "itemQty")
                                    UpdateInventory(author.Id, item.Key, userinvqty + item.Count)
                                Else
                                    AddItem(author.Id, item.Key, item.Count, ItemQuery(item.Key, "itemType"))
                                End If
                            Next
                        End If
                        If alchGroupedNames IsNot Nothing AndAlso alchGroupedNames.Count > 0 Then
                            For Each item In alchGroupedNames
                                alchbuilder.Append("[" & item.Count.ToString & "] " & item.Key).AppendLine()

                                If CheckInventory(author.Id, item.Key) Then
                                    Dim userinvqty As Integer = InventoryQuery(author.Id, item.Key, "itemQty")
                                    UpdateInventory(author.Id, item.Key, userinvqty + item.Count)
                                Else
                                    AddItem(author.Id, item.Key, item.Count, ItemQuery(item.Key, "itemType"))
                                End If
                            Next
                        End If

                        Dim builder As EmbedBuilder = New EmbedBuilder
                        builder.WithAuthor("SS Craft Bot", "https://imgur.com/VLGngdh.png")
                        builder.WithThumbnailUrl("https://imgur.com/VLGngdh.png")
                        builder.WithColor(219, 172, 69)

                        builder.WithDescription(author.Mention & " you have gained 10 daily resources.")

                        builder.AddField("Smithing Rewards:", "```fix" & Environment.NewLine &
                                         smithbuilder.ToString & Environment.NewLine &
                                         "```", True)
                        builder.AddField("Alchemy Rewards:", "```fix" & Environment.NewLine &
                                         alchbuilder.ToString & Environment.NewLine &
                                         "```", True)

                        UpdateUser(author.Id, "daily", "true")

                        Await message.Channel.SendMessageAsync("", False, builder.Build)
                    Else
                        Dim builder As EmbedBuilder = New EmbedBuilder
                        builder.WithAuthor("SS Craft Bot", "https://imgur.com/VLGngdh.png")
                        builder.WithThumbnailUrl("https://imgur.com/VLGngdh.png")
                        builder.WithColor(111, 33, 39)

                        builder.WithDescription("I'm sorry, but you already claimed this daily, please wait until the reset time.")

                        Dim seconds As Integer = Math.Round((Date.Now.AddDays(1).Date - Date.Now).TotalSeconds)
                        Dim minutes As Integer = Math.Round(seconds / 60)
                        Dim hours As Integer = Math.Round(minutes / 60)

                        If hours > 0 Then
                            builder.AddField("Cooldown Remaining:", hours & " hr", True)
                        Else
                            If minutes > 0 Then
                                builder.AddField("Cooldown Remaining:", minutes & " min", True)
                            Else
                                builder.AddField("Cooldown Remaining:", seconds & " sec", True)
                            End If
                        End If

                        Await message.Channel.SendMessageAsync("", False, builder.Build)
                    End If
                ElseIf content.ToLower.StartsWith(prefix & "smith") Then
                    Dim ls As Date = UserQuery(author.Id, "lastSmith")
                    If Math.Round((Date.Now - ls).TotalMinutes) >= 5 Then
                        Dim ranSmith As Integer = 0
                        Dim smithbuilder As New StringBuilder
                        Dim listSmith As New List(Of String)
                        Dim smithRank As Integer = 0
                        Dim userSmith As Integer = UserQuery(author.Id, "smithLvl")
                        If userSmith <= 9 Then
                            smithRank = 1
                        ElseIf userSmith >= 10 AndAlso userSmith <= 19 Then
                            smithRank = 2
                        ElseIf userSmith >= 20 AndAlso userSmith <= 49 Then
                            smithRank = 3
                        ElseIf userSmith >= 50 AndAlso userSmith <= 99 Then
                            smithRank = 4
                        ElseIf userSmith = 100 Then
                            smithRank = 5
                        End If
                        For i As Integer = 0 To 4
                            Select Case smithRank
                                Case 1
                                    ranSmith = RandomNumber(0, smithItemListNovice.Count - 1)
                                    listSmith.Add(smithItemListNovice(ranSmith))
                                Case 2
                                    ranSmith = RandomNumber(0, smithItemListApprentice.Count - 1)
                                    listSmith.Add(smithItemListApprentice(ranSmith))
                                Case 3
                                    ranSmith = RandomNumber(0, smithItemListAdept.Count - 1)
                                    listSmith.Add(smithItemListAdept(ranSmith))
                                Case 4
                                    ranSmith = RandomNumber(0, smithItemListExpert.Count - 1)
                                    listSmith.Add(smithItemListExpert(ranSmith))
                                Case 5
                                    ranSmith = RandomNumber(0, smithItemListMaster.Count - 1)
                                    listSmith.Add(smithItemListMaster(ranSmith))
                            End Select
                        Next

                        Dim smithGroupedNames = listSmith.GroupBy(Function(x) x)
                        If smithGroupedNames IsNot Nothing AndAlso smithGroupedNames.Count > 0 Then
                            For Each item In smithGroupedNames
                                smithbuilder.Append("[" & item.Count.ToString & "] " & item.Key).AppendLine()
                                If CheckInventory(author.Id, item.Key) Then
                                    Dim userinvqty As Integer = InventoryQuery(author.Id, item.Key, "itemQty")
                                    UpdateInventory(author.Id, item.Key, userinvqty + item.Count)
                                Else
                                    AddItem(author.Id, item.Key, item.Count, ItemQuery(item.Key, "itemType"))
                                End If
                            Next
                        End If

                        Dim builder As EmbedBuilder = New EmbedBuilder
                        builder.WithAuthor("SS Craft Bot", "https://imgur.com/VLGngdh.png")
                        builder.WithThumbnailUrl("https://imgur.com/VLGngdh.png")
                        builder.WithColor(219, 172, 69)

                        builder.WithDescription(author.Mention & " you have gained smithing resources.")

                        builder.AddField("Smithing Rewards:", "```fix" & Environment.NewLine &
                                         smithbuilder.ToString & Environment.NewLine &
                                         "```", True)

                        UpdateUser(author.Id, "lastSmith", Date.Now)

                        Await message.Channel.SendMessageAsync("", False, builder.Build)
                    Else
                        Dim builder As EmbedBuilder = New EmbedBuilder
                        builder.WithAuthor("SS Craft Bot", "https://imgur.com/VLGngdh.png")
                        builder.WithThumbnailUrl("https://imgur.com/VLGngdh.png")
                        builder.WithColor(111, 33, 39)

                        builder.WithDescription("I'm sorry, this command is on a cooldown.")

                        builder.AddField("Cooldown Remaining:", Math.Round((Date.Now - ls).Minutes, 0) - 5 & " minutes", True)

                        Await message.Channel.SendMessageAsync("", False, builder.Build)
                    End If
                ElseIf content.ToLower.StartsWith(prefix & "alch") Then
                    Dim ls As Date = UserQuery(author.Id, "lastAlch")
                    If Math.Round((Date.Now - ls).TotalMinutes) >= 5 Then
                        Dim ranAlch As Integer = 0
                        Dim alchbuilder As New StringBuilder
                        Dim listalch As New List(Of String)
                        Dim alchRank As Integer = 0
                        Dim userAlch As Integer = UserQuery(author.Id, "alchemyLvl")
                        If userAlch <= 9 Then
                            alchRank = 1
                        ElseIf userAlch >= 10 AndAlso userAlch <= 19 Then
                            alchRank = 2
                        ElseIf userAlch >= 20 AndAlso userAlch <= 49 Then
                            alchRank = 3
                        ElseIf userAlch >= 50 AndAlso userAlch <= 99 Then
                            alchRank = 4
                        ElseIf userAlch = 100 Then
                            alchRank = 5
                        End If
                        For i As Integer = 0 To 4
                            Select Case alchRank
                                Case 1
                                    ranAlch = RandomNumber(0, alchItemListNovice.Count - 1)
                                    listalch.Add(alchItemListNovice(ranAlch))
                                Case 2
                                    ranAlch = RandomNumber(0, alchItemListApprentice.Count - 1)
                                    listalch.Add(alchItemListApprentice(ranAlch))
                                Case 3
                                    ranAlch = RandomNumber(0, alchItemListAdept.Count - 1)
                                    listalch.Add(alchItemListAdept(ranAlch))
                                Case 4
                                    ranAlch = RandomNumber(0, alchItemListExpert.Count - 1)
                                    listalch.Add(alchItemListExpert(ranAlch))
                                Case 5
                                    ranAlch = RandomNumber(0, alchItemListMaster.Count - 1)
                                    listalch.Add(alchItemListMaster(ranAlch))
                            End Select
                        Next

                        Dim alchGroupedNames = listalch.GroupBy(Function(x) x)
                        If alchGroupedNames IsNot Nothing AndAlso alchGroupedNames.Count > 0 Then
                            For Each item In alchGroupedNames
                                alchbuilder.Append("[" & item.Count.ToString & "] " & item.Key).AppendLine()

                                If CheckInventory(author.Id, item.Key) Then
                                    Dim userinvqty As Integer = InventoryQuery(author.Id, item.Key, "itemQty")
                                    UpdateInventory(author.Id, item.Key, userinvqty + item.Count)
                                Else
                                    AddItem(author.Id, item.Key, item.Count, ItemQuery(item.Key, "itemType"))
                                End If
                            Next
                        End If

                        Dim builder As EmbedBuilder = New EmbedBuilder
                        builder.WithAuthor("SS Craft Bot", "https://imgur.com/VLGngdh.png")
                        builder.WithThumbnailUrl("https://imgur.com/VLGngdh.png")
                        builder.WithColor(219, 172, 69)

                        builder.WithDescription(author.Mention & " you have gained alchemy resources.")

                        builder.AddField("Alchemy Rewards:", "```fix" & Environment.NewLine &
                                         alchbuilder.ToString & Environment.NewLine &
                                         "```", True)

                        UpdateUser(author.Id, "lastAlch", Date.Now)

                        Await message.Channel.SendMessageAsync("", False, builder.Build)
                    Else
                        Dim builder As EmbedBuilder = New EmbedBuilder
                        builder.WithAuthor("SS Craft Bot", "https://imgur.com/VLGngdh.png")
                        builder.WithThumbnailUrl("https://imgur.com/VLGngdh.png")
                        builder.WithColor(111, 33, 39)

                        builder.WithDescription("I'm sorry, this command is on a cooldown.")

                        builder.AddField("Cooldown Remaining:", Math.Round((Date.Now - ls).Minutes, 0) - 5 & " minutes", True)

                        Await message.Channel.SendMessageAsync("", False, builder.Build)
                    End If
                ElseIf content.ToLower.StartsWith(prefix & "bag") Then
                    Dim split As String() = content.ToLower.Split(" ")
                    If split.Count > 1 Then
                        Dim page As Integer = 1
                        If content.Any(Function(c) Char.IsDigit(c)) Then
                            page = Num(content)
                        End If
                        Dim sbuilder As New StringBuilder
                        sbuilder = BagList(author.Id, page, split(1))

                        Dim builder As EmbedBuilder = New EmbedBuilder
                        builder.WithAuthor(UserQuery(author.Id, "userName") & "'s Bag", "https://imgur.com/VLGngdh.png")
                        builder.WithThumbnailUrl("https://imgur.com/VLGngdh.png")
                        builder.WithColor(219, 172, 69)

                        builder.WithDescription("Use `" & prefix & "iteminfo [item name]` for detailed descriptions." & Environment.NewLine & Environment.NewLine &
                                                            sbuilder.ToString)

                        Await message.Channel.SendMessageAsync("", False, builder.Build)
                    End If
                ElseIf content.ToLower.StartsWith(prefix & "iteminfo") Then
                    Dim split As String() = content.Split(" ")
                    If split.Count > 0 Then
                        Dim itemname As String = content.Remove(0, 12)

                        Dim itemDesc As String = ItemQuery(itemname, "itemDesc")
                        Dim itemClass As String = ItemQuery(itemname, "itemClass")
                        Dim itemStat As String = ItemQuery(itemname, "itemStat")
                        Dim itemCost As String = ItemQuery(itemname, "itemCost")
                        Dim itemType As String = ItemQuery(itemname, "itemType")
                        Dim itemRecipe As String = ItemQuery(itemname, "itemRecipe")
                        Dim craftRank As String = ItemQuery(itemname, "craftRank")
                        Dim craftable As Boolean = True
                        If itemType = "material" Then craftable = False

                        Dim builder As EmbedBuilder = New EmbedBuilder
                        Select Case craftRank
                            Case 1
                                builder.WithAuthor("[Novice] " & itemname, "https://imgur.com/VLGngdh.png")
                            Case 2
                                builder.WithAuthor("[Apprentice] " & itemname, "https://imgur.com/VLGngdh.png")
                            Case 3
                                builder.WithAuthor("[Adept] " & itemname, "https://imgur.com/VLGngdh.png")
                            Case 4
                                builder.WithAuthor("[Expert] " & itemname, "https://imgur.com/VLGngdh.png")
                            Case 5
                                builder.WithAuthor("[Master] " & itemname, "https://imgur.com/VLGngdh.png")
                        End Select
                        builder.WithThumbnailUrl("https://imgur.com/VLGngdh.png")
                        builder.WithColor(219, 172, 69)

                        builder.WithDescription(itemDesc)

                        builder.AddField("Item Class:", "`" & itemClass & "`", True)
                        If itemType = "weapon" Then
                            builder.AddField("Attack:", "`" & itemStat & "`", True)
                        ElseIf itemType = "armor" Then
                            builder.AddField("Defense:", "`" & itemStat & "`", True)
                        End If

                        builder.AddField("Cost:", goldIcon & " " & itemCost, True)

                        If craftable = True Then
                            builder.AddField("Recipe:", "```" & Environment.NewLine &
                                                             itemRecipe & Environment.NewLine &
                                                             "```", False)
                        End If

                        Await message.Channel.SendMessageAsync("", False, builder.Build)
                    End If
                ElseIf content.ToLower.StartsWith(prefix & "craft") Then
                    Dim split As String() = content.Split
                    If split.Count > 0 Then
                        If split(1) = "list" Then
                            If split.Count > 1 Then
                                Dim page As Integer = Num(content)
                                If page > 0 Then
                                    Dim userSmith As Integer = UserQuery(author.Id, "smithLvl")
                                    Dim userAlch As Integer = UserQuery(author.Id, "alchemyLvl")
                                    Dim smithRank As Integer = 1
                                    Dim alchRank As Integer = 1
                                    If userSmith <= 9 Then
                                        smithRank = 1
                                    ElseIf userSmith >= 10 AndAlso userSmith <= 19 Then
                                        smithRank = 2
                                    ElseIf userSmith >= 20 AndAlso userSmith <= 49 Then
                                        smithRank = 3
                                    ElseIf userSmith >= 50 AndAlso userSmith <= 99 Then
                                        smithRank = 4
                                    ElseIf userSmith = 100 Then
                                        smithRank = 5
                                    End If
                                    If userAlch <= 9 Then
                                        alchRank = 1
                                    ElseIf userAlch >= 10 AndAlso userAlch <= 19 Then
                                        alchRank = 2
                                    ElseIf userAlch >= 20 AndAlso userAlch <= 49 Then
                                        alchRank = 3
                                    ElseIf userAlch >= 50 AndAlso userAlch <= 99 Then
                                        alchRank = 4
                                    ElseIf userAlch = 100 Then
                                        alchRank = 5
                                    End If

                                    Dim builder As EmbedBuilder = New EmbedBuilder
                                    builder.WithAuthor("SS Craft Bot", "https://imgur.com/VLGngdh.png")
                                    builder.WithThumbnailUrl("https://imgur.com/VLGngdh.png")
                                    builder.WithColor(219, 172, 69)

                                    If split(2) = "weapon" Then
                                        Dim weapbuilder As New StringBuilder
                                        weapbuilder = LoadCraftList(smithRank, "weapon", page)
                                        builder.WithDescription(weapbuilder.ToString)
                                    ElseIf split(2) = "armor" Then
                                        Dim armorbuilder As New StringBuilder
                                        armorbuilder = LoadCraftList(smithRank, "armor", page)
                                        builder.WithDescription(armorbuilder.ToString)
                                    ElseIf split(2) = "potion" Then
                                        Dim potionbuilder As New StringBuilder
                                        potionbuilder = LoadCraftList(alchRank, "potion", page)
                                        builder.WithDescription(potionbuilder.ToString)
                                    End If

                                    builder.AddField("Item Recipe:", "`" & prefix & "iteminfo [item name]`", True)

                                    Await message.Channel.SendMessageAsync("", False, builder.Build)
                                End If
                            End If
                        ElseIf split(1) = "item" Then
                            Try
                                Dim itemnameraw As String = message.Content.Remove(0, 14)
                                Dim recipe As String() = ItemQuery(itemnameraw, "itemRecipe").Split(", ")
                                Dim recipeCount As Integer = recipe.Count
                                Dim itemCheck As Integer = 0
                                Dim itemType As String = ItemQuery(itemnameraw, "itemType")

                                For Each item As String In recipe
                                    Dim itemQty As Integer = Num(item)
                                    Dim itemName As String = Regex.Replace(item, "[\d-]", String.Empty)
                                    itemName = itemName.Remove(0, 1)
                                    If Char.IsWhiteSpace(itemName, 0) Then itemName = itemName.Remove(0, 1)
                                    If CheckInventory(author.Id, itemName) Then
                                        If InventoryQuery(author.Id, itemName, "itemQty") >= itemQty Then
                                            itemCheck += 1
                                        End If
                                    End If
                                Next

                                If itemCheck = recipeCount Then
                                    For Each item As String In recipe
                                        Dim itemQty As Integer = Num(item)
                                        Dim itemName As String = Regex.Replace(item, "[\d-]", String.Empty)
                                        itemName = itemName.Remove(0, 1)
                                        If Char.IsWhiteSpace(itemName, 0) Then itemName = itemName.Remove(0, 1)
                                        Dim curQty As Integer = InventoryQuery(author.Id, itemName, "itemQty")
                                        UpdateInventory(author.Id, itemName, curQty - itemQty)
                                    Next

                                    If CheckInventory(author.Id, itemnameraw) Then
                                        Dim newqty As Integer = InventoryQuery(author.Id, itemnameraw, "itemQty")
                                        UpdateInventory(author.Id, itemnameraw, newqty + 1)
                                    Else
                                        AddItem(author.Id, itemnameraw, 1, itemType)
                                    End If

                                    Dim craftLevelup As Boolean = False
                                    Dim smithLvl As Integer = UserQuery(author.Id, "smithLvl")
                                    Dim smithExp As Integer = UserQuery(author.Id, "smithExp")
                                    Dim alchemyLvl As Integer = UserQuery(author.Id, "alchemyLvl")
                                    Dim alchemyExp As Integer = UserQuery(author.Id, "alchemyExp")

                                    Dim builder As EmbedBuilder = New EmbedBuilder
                                    builder.WithAuthor("SS Craft Bot", "https://imgur.com/VLGngdh.png")
                                    builder.WithThumbnailUrl("https://imgur.com/VLGngdh.png")
                                    builder.WithColor(219, 172, 69)

                                    builder.WithDescription(author.Mention & " you have crafted [1] " & itemnameraw & "!")

                                    Dim expGain As Integer = 10
                                    If itemType = "weapon" Then
                                        expGain = expGain * smithLvl
                                        If smithLvl <= 99 Then
                                            If expGain + smithExp >= CalcExp(smithLvl + 1) Then
                                                craftLevelup = True
                                                UpdateUser(author.Id, "smithLvl", smithLvl + 1)
                                                UpdateUser(author.Id, "smithExp", (expGain + smithExp) - CalcExp(smithLvl))
                                            End If
                                        End If
                                    ElseIf itemType = "armor" Then
                                        expGain = expGain * smithLvl
                                        If smithLvl <= 99 Then
                                            If expGain + smithExp >= CalcExp(smithLvl + 1) Then
                                                craftLevelup = True
                                                UpdateUser(author.Id, "smithLvl", smithLvl + 1)
                                                UpdateUser(author.Id, "smithExp", (expGain + smithExp) - CalcExp(smithLvl))
                                            End If
                                        End If
                                    ElseIf itemType = "potion" Then
                                        expGain = expGain * alchemyLvl
                                        If alchemyLvl <= 99 Then
                                            If expGain + alchemyExp >= CalcExp(alchemyLvl + 1) Then
                                                craftLevelup = True
                                                UpdateUser(author.Id, "alchemyLvl", alchemyLvl + 1)
                                                UpdateUser(author.Id, "alchemyExp", (expGain + alchemyExp) - CalcExp(alchemyLvl))
                                            End If
                                        End If
                                    End If
                                    If craftLevelup = True Then
                                        Select Case itemType
                                            Case "weapon"
                                                Dim expString As String
                                                Dim expPercent As Double = Math.Round((expGain + smithExp) - CalcExp(smithLvl) / CalcExp(smithLvl + 1) * 100, 2)
                                                If expPercent <= 9.9 Then
                                                    expString = "▱▱▱▱▱▱▱▱▱▱ " & expPercent & "% EXP"
                                                ElseIf expPercent >= 10.0 AndAlso expPercent <= 19.99 Then
                                                    expString = "▰▱▱▱▱▱▱▱▱▱ " & expPercent & "% EXP"
                                                ElseIf expPercent >= 20.0 AndAlso expPercent <= 29.99 Then
                                                    expString = "▰▰▱▱▱▱▱▱▱▱ " & expPercent & "% EXP"
                                                ElseIf expPercent >= 30.0 AndAlso expPercent <= 39.99 Then
                                                    expString = "▰▰▰▱▱▱▱▱▱▱ " & expPercent & "% EXP"
                                                ElseIf expPercent >= 40.0 AndAlso expPercent <= 49.99 Then
                                                    expString = "▰▰▰▰▱▱▱▱▱▱ " & expPercent & "% EXP"
                                                ElseIf expPercent >= 50.0 AndAlso expPercent <= 59.99 Then
                                                    expString = "▰▰▰▰▰▱▱▱▱▱ " & expPercent & "% EXP"
                                                ElseIf expPercent >= 60.0 AndAlso expPercent <= 69.99 Then
                                                    expString = "▰▰▰▰▰▰▱▱▱▱ " & expPercent & "% EXP"
                                                ElseIf expPercent >= 70.0 AndAlso expPercent <= 79.99 Then
                                                    expString = "▰▰▰▰▰▰▰▱▱▱ " & expPercent & "% EXP"
                                                ElseIf expPercent >= 80.0 AndAlso expPercent <= 89.99 Then
                                                    expString = "▰▰▰▰▰▰▰▰▱▱ " & expPercent & "% EXP"
                                                ElseIf expPercent >= 90.0 AndAlso expPercent <= 99.99 Then
                                                    expString = "▰▰▰▰▰▰▰▰▰▱ " & expPercent & "% EXP"
                                                Else
                                                    expString = "▰▰▰▰▰▰▰▰▰▰ " & expPercent & "% EXP"
                                                End If

                                                builder.AddField("Skill Level Up!", "Your smith skill has increased! `" & smithLvl & "` " & arrowIcon & " `" & smithLvl + 1 & "`!")
                                            Case "armor"
                                                Dim expString As String
                                                Dim expPercent As Double = Math.Round((expGain + smithExp) - CalcExp(smithLvl) / CalcExp(smithLvl + 1) * 100, 2)
                                                If expPercent <= 9.9 Then
                                                    expString = "▱▱▱▱▱▱▱▱▱▱ " & expPercent & "% EXP"
                                                ElseIf expPercent >= 10.0 AndAlso expPercent <= 19.99 Then
                                                    expString = "▰▱▱▱▱▱▱▱▱▱ " & expPercent & "% EXP"
                                                ElseIf expPercent >= 20.0 AndAlso expPercent <= 29.99 Then
                                                    expString = "▰▰▱▱▱▱▱▱▱▱ " & expPercent & "% EXP"
                                                ElseIf expPercent >= 30.0 AndAlso expPercent <= 39.99 Then
                                                    expString = "▰▰▰▱▱▱▱▱▱▱ " & expPercent & "% EXP"
                                                ElseIf expPercent >= 40.0 AndAlso expPercent <= 49.99 Then
                                                    expString = "▰▰▰▰▱▱▱▱▱▱ " & expPercent & "% EXP"
                                                ElseIf expPercent >= 50.0 AndAlso expPercent <= 59.99 Then
                                                    expString = "▰▰▰▰▰▱▱▱▱▱ " & expPercent & "% EXP"
                                                ElseIf expPercent >= 60.0 AndAlso expPercent <= 69.99 Then
                                                    expString = "▰▰▰▰▰▰▱▱▱▱ " & expPercent & "% EXP"
                                                ElseIf expPercent >= 70.0 AndAlso expPercent <= 79.99 Then
                                                    expString = "▰▰▰▰▰▰▰▱▱▱ " & expPercent & "% EXP"
                                                ElseIf expPercent >= 80.0 AndAlso expPercent <= 89.99 Then
                                                    expString = "▰▰▰▰▰▰▰▰▱▱ " & expPercent & "% EXP"
                                                ElseIf expPercent >= 90.0 AndAlso expPercent <= 99.99 Then
                                                    expString = "▰▰▰▰▰▰▰▰▰▱ " & expPercent & "% EXP"
                                                Else
                                                    expString = "▰▰▰▰▰▰▰▰▰▰ " & expPercent & "% EXP"
                                                End If

                                                builder.AddField("Skill Level Up!", "Your smith skill has increased! `" & smithLvl & "` " & arrowIcon & " `" & smithLvl + 1 & "`!")
                                            Case "potion"
                                                Dim expString As String
                                                Dim expPercent As Double = Math.Round((expGain + alchemyExp) - CalcExp(alchemyLvl) / CalcExp(alchemyExp + 1) * 100, 2)
                                                If expPercent <= 9.9 Then
                                                    expString = "▱▱▱▱▱▱▱▱▱▱ " & expPercent & "% EXP"
                                                ElseIf expPercent >= 10.0 AndAlso expPercent <= 19.99 Then
                                                    expString = "▰▱▱▱▱▱▱▱▱▱ " & expPercent & "% EXP"
                                                ElseIf expPercent >= 20.0 AndAlso expPercent <= 29.99 Then
                                                    expString = "▰▰▱▱▱▱▱▱▱▱ " & expPercent & "% EXP"
                                                ElseIf expPercent >= 30.0 AndAlso expPercent <= 39.99 Then
                                                    expString = "▰▰▰▱▱▱▱▱▱▱ " & expPercent & "% EXP"
                                                ElseIf expPercent >= 40.0 AndAlso expPercent <= 49.99 Then
                                                    expString = "▰▰▰▰▱▱▱▱▱▱ " & expPercent & "% EXP"
                                                ElseIf expPercent >= 50.0 AndAlso expPercent <= 59.99 Then
                                                    expString = "▰▰▰▰▰▱▱▱▱▱ " & expPercent & "% EXP"
                                                ElseIf expPercent >= 60.0 AndAlso expPercent <= 69.99 Then
                                                    expString = "▰▰▰▰▰▰▱▱▱▱ " & expPercent & "% EXP"
                                                ElseIf expPercent >= 70.0 AndAlso expPercent <= 79.99 Then
                                                    expString = "▰▰▰▰▰▰▰▱▱▱ " & expPercent & "% EXP"
                                                ElseIf expPercent >= 80.0 AndAlso expPercent <= 89.99 Then
                                                    expString = "▰▰▰▰▰▰▰▰▱▱ " & expPercent & "% EXP"
                                                ElseIf expPercent >= 90.0 AndAlso expPercent <= 99.99 Then
                                                    expString = "▰▰▰▰▰▰▰▰▰▱ " & expPercent & "% EXP"
                                                Else
                                                    expString = "▰▰▰▰▰▰▰▰▰▰ " & expPercent & "% EXP"
                                                End If

                                                builder.AddField("Skill Level Up!", "Your alchemy skill has increased! `" & alchemyLvl & "` " & arrowIcon & " `" & alchemyLvl + 1 & "`!")
                                        End Select

                                    End If
                                    Await message.Channel.SendMessageAsync("", False, builder.Build)
                                Else
                                    Dim builder As EmbedBuilder = New EmbedBuilder
                                    builder.WithAuthor("SS Craft Bot", "https://imgur.com/VLGngdh.png")
                                    builder.WithThumbnailUrl("https://imgur.com/VLGngdh.png")
                                    builder.WithColor(111, 33, 39)

                                    builder.WithDescription("I'm sorry, you dont have enough materials to craft this item.")

                                    Await message.Channel.SendMessageAsync("", False, builder.Build)
                                End If
                            Catch ex As Exception
                                Console.WriteLine(ex.ToString)
                            End Try
                        End If
                    End If
                ElseIf content.ToLower.StartsWith(prefix & "buy") Then
                    Dim split As String() = content.Split(" ")
                    Dim qty As Integer = Num(content)
                    Dim itemName As String = content.Remove(0, 7)
                    itemName = Regex.Replace(itemName, "[\d-]", String.Empty)
                    itemName = itemName.Remove(itemName.Length - 1, 1)
                    Dim userGold As Integer = UserQuery(author.Id, "gold")
                    Dim itemCost As Integer = ItemQuery(itemName, "itemCost")
                    Dim itemType As String = ItemQuery(itemName, "itemType")
                    Dim itemClass As String = ItemQuery(itemName, "itemClass")
                    Dim craftRank As Integer = ItemQuery(itemName, "craftRank")
                    Dim userSmith As Integer = UserQuery(author.Id, "smithLvl")
                    Dim userAlch As Integer = UserQuery(author.Id, "alchemyLvl")
                    Dim smithRank As Integer = 1
                    Dim alchRank As Integer = 1
                    If userSmith <= 9 Then
                        smithRank = 1
                    ElseIf userSmith >= 10 AndAlso userSmith <= 19 Then
                        smithRank = 2
                    ElseIf userSmith >= 20 AndAlso userSmith <= 49 Then
                        smithRank = 3
                    ElseIf userSmith >= 50 AndAlso userSmith <= 99 Then
                        smithRank = 4
                    ElseIf userSmith = 100 Then
                        smithRank = 5
                    End If
                    If userAlch <= 9 Then
                        alchRank = 1
                    ElseIf userAlch >= 10 AndAlso userAlch <= 19 Then
                        alchRank = 2
                    ElseIf userAlch >= 20 AndAlso userAlch <= 49 Then
                        alchRank = 3
                    ElseIf userAlch >= 50 AndAlso userAlch <= 99 Then
                        alchRank = 4
                    ElseIf userAlch = 100 Then
                        alchRank = 5
                    End If
                    If itemType = "material" Then
                        If itemClass = "Smithing" Then
                            If smithRank >= craftRank Then
                                If qty > 0 Then
                                    If (itemCost * qty) <= (userGold * qty) Then
                                        UpdateUser(author.Id, "gold", userGold - (itemCost * qty))
                                        If CheckInventory(author.Id, itemName) Then
                                            Dim userinvqty As Integer = InventoryQuery(author.Id, itemName, "itemQty")
                                            UpdateInventory(author.Id, itemName, userinvqty + qty)
                                        Else
                                            AddItem(author.Id, itemName, qty, itemType)
                                        End If

                                        Dim builder As EmbedBuilder = New EmbedBuilder
                                        builder.WithAuthor("SS Craft Bot", "https://imgur.com/VLGngdh.png")
                                        builder.WithThumbnailUrl("https://imgur.com/VLGngdh.png")
                                        builder.WithColor(219, 172, 69)

                                        builder.WithDescription(author.Mention & " you have made a purchase!")

                                        builder.AddField("Items Received:", "```fix" & Environment.NewLine &
                                                         "[" & qty & "] " & itemName & Environment.NewLine &
                                                         "```", True)

                                        Await message.Channel.SendMessageAsync("", False, builder.Build)
                                    End If
                                End If
                            End If
                        ElseIf itemClass = "Alchemy" Then
                            If alchRank >= craftRank Then
                                If qty > 0 Then
                                    If (itemCost * qty) <= (userGold * qty) Then
                                        UpdateUser(author.Id, "gold", userGold - (itemCost * qty))
                                        If CheckInventory(author.Id, itemName) Then
                                            Dim userinvqty As Integer = InventoryQuery(author.Id, itemName, "itemQty")
                                            UpdateInventory(author.Id, itemName, userinvqty + qty)
                                        Else
                                            AddItem(author.Id, itemName, qty, itemType)
                                        End If

                                        Dim builder As EmbedBuilder = New EmbedBuilder
                                        builder.WithAuthor("SS Craft Bot", "https://imgur.com/VLGngdh.png")
                                        builder.WithThumbnailUrl("https://imgur.com/VLGngdh.png")
                                        builder.WithColor(219, 172, 69)

                                        builder.WithDescription(author.Mention & " you have made a purchase!")

                                        builder.AddField("Items Received:", "```fix" & Environment.NewLine &
                                                         "[" & qty & "] " & itemName & Environment.NewLine &
                                                         "```", True)

                                        Await message.Channel.SendMessageAsync("", False, builder.Build)
                                    End If
                                End If
                            End If
                        End If
                    End If
                ElseIf content.ToLower.StartsWith(prefix & "sell") Then
                    Dim split As String() = content.Split(" ")
                    Dim qty As Integer = Num(content)
                    Dim itemName As String = content.Remove(0, 8)
                    itemName = Regex.Replace(itemName, "[\d-]", String.Empty)
                    itemName = itemName.Remove(itemName.Length - 1, 1)
                    Dim userGold As Integer = UserQuery(author.Id, "gold")
                    Dim itemCost As Integer = ItemQuery(itemName, "itemCost")
                    Dim itemType As String = ItemQuery(itemName, "itemType")
                    Dim itemClass As String = ItemQuery(itemName, "itemClass")
                    Dim itemQty As String = InventoryQuery(author.Id, itemName, "itemQty")
                    If CheckInventory(author.Id, itemName) Then
                        If qty > 0 Then
                            If itemQty >= qty Then
                                UpdateUser(author.Id, "gold", userGold + ((itemCost / 2) * qty))
                                UpdateInventory(author.Id, itemName, itemQty - qty)

                                Dim builder As EmbedBuilder = New EmbedBuilder
                                builder.WithAuthor("SS Craft Bot", "https://imgur.com/VLGngdh.png")
                                builder.WithThumbnailUrl("https://imgur.com/VLGngdh.png")
                                builder.WithColor(219, 172, 69)

                                builder.WithDescription(author.Mention & " you have sold something(s)!")

                                builder.AddField("Items Sold:", "```fix" & Environment.NewLine &
                                                             "[" & qty & "] " & itemName & Environment.NewLine &
                                                             "```", True)

                                Await message.Channel.SendMessageAsync("", False, builder.Build)
                            End If
                        End If
                    End If
                ElseIf content.ToLower.StartsWith(prefix & "shop") Then
                    Dim page As Integer = Num(content)
                    Dim split As String() = content.Split(" ")
                    If split.Count > 0 Then
                        If page > 0 Then
                            Dim iType As String = split(1)
                            Dim sbuilder As New StringBuilder
                            Dim userSmith As Integer = UserQuery(author.Id, "smithLvl")
                            Dim userAlch As Integer = UserQuery(author.Id, "alchemyLvl")
                            Dim smithRank As Integer = 1
                            Dim alchRank As Integer = 1
                            If userSmith <= 9 Then
                                smithRank = 1
                            ElseIf userSmith >= 10 AndAlso userSmith <= 19 Then
                                smithRank = 2
                            ElseIf userSmith >= 20 AndAlso userSmith <= 49 Then
                                smithRank = 3
                            ElseIf userSmith >= 50 AndAlso userSmith <= 99 Then
                                smithRank = 4
                            ElseIf userSmith = 100 Then
                                smithRank = 5
                            End If
                            If userAlch <= 9 Then
                                alchRank = 1
                            ElseIf userAlch >= 10 AndAlso userAlch <= 19 Then
                                alchRank = 2
                            ElseIf userAlch >= 20 AndAlso userAlch <= 49 Then
                                alchRank = 3
                            ElseIf userAlch >= 50 AndAlso userAlch <= 99 Then
                                alchRank = 4
                            ElseIf userAlch = 100 Then
                                alchRank = 5
                            End If

                            If iType = "smith" Then
                                sbuilder = LoadShopList(smithRank, "smithing", page)
                            ElseIf iType = "alch" Then
                                sbuilder = LoadShopList(alchRank, "alchemy", page)
                            End If

                            Dim builder As EmbedBuilder = New EmbedBuilder
                            builder.WithAuthor("SS Craft Bot", "https://imgur.com/VLGngdh.png")
                            builder.WithThumbnailUrl("https://imgur.com/VLGngdh.png")
                            builder.WithColor(219, 172, 69)

                            builder.WithDescription("Use `" & prefix & "buy [item name] [qty]` to purchase an item.")

                            builder.AddField("For Sale:", sbuilder.ToString, True)

                            Await message.Channel.SendMessageAsync("", False, builder.Build)
                        End If

                    End If

                End If
            Else
                If CheckUser(author.Id) = False Then
                    AddUser(member.Id, member.Username)
                End If

                If message.Author.IsBot = False Then
                    Dim msgexp As Integer = message.Content.Count * My.Settings.exprate
                    Dim exp As Integer = UserQuery(author.Id, "exp")
                    Dim lvl As Integer = UserQuery(author.Id, "lvl")

                    If lvl <= 99 Then
                        If exp + msgexp >= CalcExp(lvl + 1) Then
                            UpdateUser(author.Id, "exp", (msgexp + exp) - CalcExp(lvl))
                            UpdateUser(author.Id, "lvl", lvl + 1)

                            Dim builder As EmbedBuilder = New EmbedBuilder
                            builder.WithAuthor("SS Craft Bot", "https://imgur.com/VLGngdh.png")
                            builder.WithThumbnailUrl("https://imgur.com/VLGngdh.png")
                            builder.WithColor(219, 172, 69)

                            builder.WithDescription(author.Mention & " you have leveled up! `" & lvl & "` " & arrowIcon & " `" & lvl + 1 & "`.")

                            Await message.Channel.SendMessageAsync("", False, builder.Build)
                        Else
                            UpdateUser(author.Id, "exp", msgexp + exp)
                        End If
                    End If
                End If
            End If
        End If
    End Function

    Private Function ConsoleEventCallback(ByVal eventType As Integer) As Boolean
        Select Case eventType
            Case 0
                Colorize("[INFO]      Bot Closing | Prepairing final tasks and saving.")
                _client.Dispose()
            Case 1
                Colorize("[INFO]      Bot Closing | Prepairing final tasks and saving.")
                _client.Dispose()
            Case 2
                Colorize("[INFO]      Bot Closing | Prepairing final tasks and saving.")
                _client.Dispose()
            Case 5
                Colorize("[INFO]      Bot Closing | Prepairing final tasks and saving.")
                _client.Dispose()
            Case 6
                Colorize("[INFO]      Bot Closing | Prepairing final tasks and saving.")
                _client.Dispose()
        End Select
        Return False
    End Function

    Dim handler As ConsoleEventDelegate
    Private Delegate Function ConsoleEventDelegate(ByVal eventType As Integer) As Boolean
    <DllImport("kernel32.dll", SetLastError:=True)>
    Private Function SetConsoleCtrlHandler(ByVal callback As ConsoleEventDelegate, ByVal add As Boolean) As Boolean
    End Function
End Module