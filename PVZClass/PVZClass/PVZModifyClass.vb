Imports System.ComponentModel
Imports System.Runtime.CompilerServices

Public Module ExtensionModule
    <Extension()>
    Public Function GetDescription(ByVal this As [Enum], Optional lang As Integer = 0) As String
        If lang = 0 Then
            Try
                Return CType(this.GetType().GetField(this.ToString()).GetCustomAttributes(GetType(DescriptionAttribute), True)(0), DescriptionAttribute).Description
            Catch ex As Exception
            End Try
        End If
        Return this.ToString()
    End Function
End Module


Partial Public Class PVZ
#Region "修改功能"
    ''' <summary>
    ''' 设置后台运行
    ''' </summary>
    ''' <param name="Switch">开启还是关闭,默认开启</param>
    Public Shared Sub BGRunable(Optional ByVal Switch As Boolean = True)
        If Switch Then
            Memory.WriteByte(&H54EBA8, 112)
        Else
            Memory.WriteByte(&H54EBA8, 116)
        End If
    End Sub
    ''' <summary>
    ''' 显示隐藏关卡
    ''' </summary>
    ''' <param name="Switch">开启还是关闭,默认开启</param>
    Public Shared Sub ShowHiddenLevel(Optional ByVal Switch As Boolean = True)
        If Switch Then
            Memory.WriteByte(&H42DF5D, 56)
        Else
            Memory.WriteByte(&H42DF5D, 136)
        End If
    End Sub
    ''' <summary>
    ''' 启动传送门,注意该方法会自动创建默认的传送门
    ''' </summary>
    ''' <param name="Switch">开启还是关闭,默认开启</param>
    Public Shared Sub EnablePortal(Optional ByVal Switch As Boolean = True)
        If Switch Then
            _CreatePortal()
            Memory.WriteByte(&H467665, 112)
            Memory.WriteByte(&H41FFB4, 112)
            Memory.WriteByte(&H4248CE, 112)
        Else
            Memory.WriteByte(&H467665, 117)
            Memory.WriteByte(&H41FFB4, 117)
            Memory.WriteByte(&H4248CE, 117)
        End If
    End Sub
    ''' <summary>
    ''' 固定传送门
    ''' </summary>
    ''' <param name="Switch">开启还是关闭,默认开启</param>
    Public Shared Sub FixPortal(Optional ByVal Switch As Boolean = True)
        If Switch Then
            Memory.WriteByte(&H4276DD, 0)
        Else
            Memory.WriteByte(&H4276DD, 255)
        End If
    End Sub
    ''' <summary>
    ''' 阳光上限
    ''' </summary>
    Public Shared WriteOnly Property SunMax As Integer
        Set(Value As Integer)
            Memory.WriteInteger(&H430A1F, Value)
            Memory.WriteInteger(&H430A2B, Value)
        End Set
    End Property
    ''' <summary>
    ''' 去除上限
    ''' </summary>
    ''' <param name="Switch">开启还是关闭,默认开启</param>
    Public Shared Sub NoUpperLimit(Optional ByVal Switch As Boolean = True)
        If Switch Then
            Memory.WriteByte(&H430A23, 235)
            Memory.WriteByte(&H430A78, 235)
            Memory.WriteByte(&H48CAB0, 235)
        Else
            Memory.WriteByte(&H430A23, 126)
            Memory.WriteByte(&H430A78, 126)
            Memory.WriteByte(&H48CAB0, 126)
        End If
    End Sub
    ''' <summary>
    ''' 重叠放置
    ''' </summary>
    ''' <param name="Switch">开启还是关闭,默认开启</param>
    Public Shared Sub OverlapPlanting(Optional ByVal Switch As Boolean = True)
        If Switch Then
            Memory.WriteByte(&H40FE30, 129)
            Memory.WriteByte(&H42A2D9, 141)
            Memory.WriteByte(&H438E40, 235)
        Else
            Memory.WriteByte(&H40FE30, 132)
            Memory.WriteByte(&H42A2D9, 132)
            Memory.WriteByte(&H438E40, 116)
        End If
    End Sub
    ''' <summary>
    ''' 跳关
    ''' </summary>
    ''' <param name="advLevel">冒险模式关卡</param>
    ''' <param name="gameLevel">其他关卡</param>
    Public Shared Function JumpLevel(advLevel As Integer, gameLevel As Byte) As Integer
        Dim pageIndex = Memory.GetAddress(&H6A9EC0, &H320, &H88, &H1C8)
        If LevelState = GameState.Playing Then
            If AdventureLevel = 0 And LevelId = 0 Then
                Return 0
            Else
                SaveData.AdventureLevel = advLevel
                Memory.WriteByte(&H41264F, gameLevel)
                FirstLevelMsg = 27
                Return 1
            End If
        ElseIf LevelState = GameState.MainMenu OrElse LevelState = GameState.SelectingLevel Then
            SaveData.AdventureLevel = advLevel
            Memory.WriteByte(&H44F587, 235)
            Memory.WriteByte(&H44B403, gameLevel)
            Select Case pageIndex
                Case 1
                    gameLevel -= 15
                Case 3
                    gameLevel -= 50
                Case 0
                    gameLevel = gameLevel
                Case 2
                    gameLevel -= 35
            End Select
            Memory.WriteByte(&H42F7B5, gameLevel)
            If LevelState = GameState.SelectingLevel Then
                Mouse.WMLClick(85, 140)
            Else
                Mouse.WMLClick(555, 140)
            End If
            Dim thread = New Threading.Thread(
                Sub()
                    Threading.Thread.Sleep(5000)
                    Memory.WriteByte(&H44F587, 116)
                    Memory.WriteByte(&H44B403, 0)
                    Memory.WriteByte(&H42F7B5, 1)
                End Sub)
            thread.Start()
        End If
        Return -1
    End Function
    ''' <summary>
    ''' 无视资源
    ''' </summary>
    Public Shared Sub IgnoreRes(Optional ByVal Switch As Boolean = True)
        If Switch Then
            Memory.WriteInteger(&H41BA72, -214234000)
            Memory.WriteByte(&H41BAC0, 145)
            Memory.WriteByte(&H42487F, 235)
            Memory.WriteByte(&H427A92, 128)
            Memory.WriteByte(&H427DFD, 128)
            Memory.WriteByte(&H48CAA5, 57)
            Memory.WriteQword(&H48C7A0, 174109865281658857)
        Else
            Memory.WriteInteger(&H41BA72, -215282561)
            Memory.WriteByte(&H41BAC0, 158)
            Memory.WriteByte(&H42487F, 116)
            Memory.WriteByte(&H427A92, 143)
            Memory.WriteByte(&H427DFD, 143)
            Memory.WriteByte(&H48CAA5, 41)
            Memory.WriteQword(&H48C7A0, 173951535625964815)
        End If
    End Sub
    ''' <summary>
    ''' 取消冷却
    ''' </summary>
    Public Shared Sub NoCD(Optional ByVal Switch As Boolean = True)
        If Switch Then
            Memory.WriteByte(&H487296, 112)
            Memory.WriteByte(&H488250, 235)
            Memory.WriteByte(&H488E76, 1)
        Else
            Memory.WriteByte(&H487296, 126)
            Memory.WriteByte(&H488250, 117)
            Memory.WriteByte(&H488E76, 0)
        End If
    End Sub
    ''' <summary>
    ''' 传送带无延迟
    ''' </summary>
    Public Shared Sub ConveyorBeltNoDelay(Optional ByVal Switch As Boolean = True)
        If Switch Then
            Memory.WriteByte(&H422D20, 128)
            Memory.WriteByte(&H489CA1, 51)
        Else
            Memory.WriteByte(&H422D20, 143)
            Memory.WriteByte(&H489CA1, 133)
        End If
    End Sub
    ''' <summary>
    ''' 全屏浓雾
    ''' </summary>
    Public Shared Sub FullScreenFog(Optional ByVal Switch As Boolean = True)
        If Switch Then
            Memory.WriteShort(&H41A476, 16363)
            Memory.WriteInteger(&H41C1C0, 12828723)
            Memory.WriteByte(&H41A4BA, 0)
        Else
            Memory.WriteShort(&H41A476, 1397)
            Memory.WriteInteger(&H41C1C0, 9208203)
            Memory.WriteByte(&H41A4BA, 4)
            For i = 0 To 63
                Memory.WriteInteger(BaseAddress + &H4C8 + 4 * i, 0)
            Next
        End If
    End Sub
    ''' <summary>
    ''' 暂停僵尸刷新
    ''' </summary>
    Public Shared Sub BlockZombie(Optional ByVal Switch As Boolean = True)
        If Switch Then
            Memory.WriteByte(&H4265DC, 235)
        Else
            Memory.WriteByte(&H4265DC, 116)
        End If
    End Sub
    ''' <summary>
    ''' 罐子透视
    ''' </summary>
    Public Shared Sub VasePerspect(Optional ByVal Switch As Boolean = True)
        If Switch Then
            Memory.WriteBytes(&H44E5CC, {&HC7, &H47, &H4C, &H64, 0, 0, 0, &H5E, &H59, &HC3})
        Else
            Memory.WriteBytes(&H44E5CC, {&H85, &HC0, &H7E, 6, &H83, &HC0, &HFF, &H89, &H47, &H4C})
        End If
    End Sub
    ''' <summary>
    ''' 浓雾透视
    ''' </summary>
    Public Shared Sub FogPerspect(Optional ByVal Switch As Boolean = True)
        If Switch Then
            Memory.WriteInteger(&H41A67A, 0)
            Memory.WriteInteger(&H41A681, 0)
        Else
            Memory.WriteInteger(&H41A67A, 255)
            Memory.WriteInteger(&H41A681, 255)
        End If
    End Sub
    ''' <summary>
    ''' 锁定铲子
    ''' </summary>
    Public Shared Sub LockShovel(Optional ByVal Switch As Boolean = True)
        If Switch Then
            MousePointer.Type = MouseType.Shovel
            Memory.WriteQword(&H41233D, -8029759805927192901)
        Else
            Memory.WriteQword(&H41233D, 586669480753)
        End If
    End Sub
    ''' <summary>
    ''' 自动收集资源
    ''' </summary>
    Public Shared Sub AutoCollect(Optional ByVal Switch As Boolean = True)
        If Switch Then
            For Each coin In AllCoins
                coin.Collect()
            Next
            Memory.WriteByte(&H40CCDA, jmpfar)
            Memory.WriteInteger(&H40CCDB, Variable + 520 - 4 - &H40CCDB)
            Memory.WriteBytes(Variable + 520, {
                              pushad,
                              &H8B, &HC8,
                              &H83, &H79, &H58, &H10,
                              &H74, 5,
                              call­, 2, 0, 0, 0,
                              jmp, 6,
                              pushdw, &H60, &H20, &H43, 0,
                              ret,
                              popad,
                              &HC2, &H10, 0
            })
        Else
            Memory.WriteBytes(&H40CCDA, {&HC2, &H10, 0})
        End If
    End Sub
    ''' <summary>
    ''' 全模式解锁
    ''' </summary>
    Public Shared Sub UnlockAllLevel(Optional ByVal Switch As Boolean = True)
        If Switch Then
            PVZ.Memory.WriteByte(&H454109, 112)
            PVZ.Memory.WriteByte(&H44A514, 112)
            PVZ.Memory.WriteByteArray(&H42E440, &H31, &HC0, &HC3)
            PVZ.Memory.WriteByte(&H449E9D, 235)
            PVZ.Memory.WriteByteArray(&H48AAD0, &H30, &HC0, &HC3)
            PVZ.Memory.WriteByte(&H48A54C, 235)
            PVZ.Memory.WriteByte(&H48D32B, 235)
            PVZ.Memory.WriteByte(&H48C491, 235)
            PVZ.Memory.WriteByte(&H449E7A, 235)
            PVZ.Memory.WriteByte(&H453AD1, 235)
            PVZ.Memory.WriteInteger(&H403A10, 1542128048)
            PVZ.Memory.WriteInteger(&H69DCA0, 0)
            PVZ.Memory.WriteByteArray(&H403B30, &HB0, &H1, &HC3)
        Else
            PVZ.Memory.WriteByte(&H454109, 126)
            PVZ.Memory.WriteByte(&H44A514, 126)
            PVZ.Memory.WriteByteArray(&H42E440, &H51, &H53, &H55)
            PVZ.Memory.WriteByte(&H449E9D, 127)
            PVZ.Memory.WriteByteArray(&H48AAD0, &H53, &H8B, &HD9)
            PVZ.Memory.WriteByte(&H48A54C, 127)
            PVZ.Memory.WriteByte(&H48D32B, 127)
            PVZ.Memory.WriteByte(&H48C491, 127)
            PVZ.Memory.WriteByte(&H449E7A, 127)
            PVZ.Memory.WriteByte(&H453AD1, 127)
            PVZ.Memory.WriteInteger(&H403A10, 1821070673)
            PVZ.Memory.WriteInteger(&H69DCA0, 40)
            PVZ.Memory.WriteByteArray(&H403B30, &H8B, &H80, &H6C)
        End If
    End Sub
    ''' <summary>
    ''' 戴夫选卡
    ''' </summary>
    Public Shared Sub DaveSelectYourCard(Optional ByVal Switch As Boolean? = True)
        ThreeStateCheck(&H483F1A, Switch)
    End Sub
    Private Shared Sub ThreeStateCheck(Address As Integer, Switch As Boolean?)
        If Switch.HasValue Then
            If Switch Then
                Memory.WriteByte(Address, 112)
            Else
                Memory.WriteByte(Address, 117)
            End If
        Else
            Memory.WriteByte(Address, 235)
        End If
    End Sub
    Private Shared Sub BinaryCheck(Address As Integer, Switch As Boolean)
        If Switch Then
            Memory.WriteByte(Address, 235)
        Else
            Memory.WriteByte(Address, 116)
        End If
    End Sub
    ''' <summary>
    ''' 保龄球模式
    ''' </summary>
    Public Shared Sub BowlingMode(Optional ByVal Switch As Boolean = True)
        If Switch Then
            PVZ.Memory.WriteByte(&H46325B, 112)
            PVZ.Memory.WriteByte(&H467FB2, 235)
            PVZ.Memory.WriteByte(&H419B91, 235)
            PVZ.Memory.WriteByte(&H45DDA7, 235)
            PVZ.Memory.WriteByte(&H52E4DE, 235)
        Else
            PVZ.Memory.WriteByte(&H46325B, 116)
            PVZ.Memory.WriteByte(&H467FB2, 116)
            PVZ.Memory.WriteByte(&H419B91, 116)
            PVZ.Memory.WriteByte(&H45DDA7, 116)
            PVZ.Memory.WriteByte(&H52E4DE, 116)

        End If
    End Sub
    ''' <summary>
    ''' 下雨天
    ''' </summary>
    Public Shared Sub Rainning(Optional ByVal Switch As Boolean = True)
        If Switch Then
            PVZ.Memory.WriteByte(&H426B4E, 112)
            PVZ.Memory.WriteByte(&H416F07, 112)
            PVZ.Memory.WriteByte(&H424715, 235)
            PVZ.Memory.WriteByte(&H41F38E, 112)
        Else
            PVZ.Memory.WriteByte(&H426B4E, 117)
            PVZ.Memory.WriteByte(&H416F07, 117)
            PVZ.Memory.WriteByte(&H424715, 116)
            PVZ.Memory.WriteByte(&H41F38E, 117)
        End If
    End Sub
    ''' <summary>
    ''' 天上掉卡片
    ''' </summary>
    Public Shared Sub DropCard(Optional ByVal Switch As Boolean = True)
        If Switch Then
            Miscellaneous.AttributeCountdown = 100
            PVZ.Memory.WriteShort(&H41F1D9, &H6877)
            PVZ.Memory.WriteByte(&H41F1E9, &H58)
            PVZ.Memory.WriteInteger(&H41F1EA, 300)
            PVZ.Memory.WriteByte(&H4248BD, 112)
        Else
            PVZ.Memory.WriteByte(&H4248BD, 117)
        End If
    End Sub
    ''' <summary>
    ''' 隐形僵尸
    ''' </summary>
    Public Shared Sub Invisighoul(Optional ByVal Switch As Boolean? = True)
        ThreeStateCheck(&H52E357, Switch)
        ThreeStateCheck(&H53402B, Switch)
    End Sub
    ''' <summary>
    ''' 宝石迷阵转转看
    ''' </summary>
    Public Shared Sub BeghouledTwist(Optional ByVal Switch As Boolean = True)
        If Switch Then
            PVZ.Memory.WriteByte(&H420260, 7)
            PVZ.Memory.WriteByte(&H420307, 7)
            PVZ.Memory.WriteByte(&H4201A4, 7)
            If PVZ.SixRoute Then
                PVZ.Memory.WriteByte(&H420265, 4)
                PVZ.Memory.WriteByte(&H420310, 4)
                PVZ.Memory.WriteByte(&H4201A9, 4)
            End If
            PVZ.Memory.WriteByte(&H421FE1, 112)
            PVZ.Memory.WriteByte(&H421FE8, 128)
            PVZ.Memory.WriteByte(&H420533, 129)
            PVZ.Memory.WriteByte(&H424520, 113)
            PVZ.Memory.WriteByteArray(&H424520, 144, 144, 144, 144, 144, 144)
            PVZ.Memory.WriteByte(&H424773, 235)
            PVZ.Memory.WriteByte(&H422BC9, 113)
        Else
            PVZ.Memory.WriteByte(&H420260, 6)
            PVZ.Memory.WriteByte(&H420307, 6)
            PVZ.Memory.WriteByte(&H4201A4, 6)
            PVZ.Memory.WriteByte(&H420265, 3)
            PVZ.Memory.WriteByte(&H420310, 3)
            PVZ.Memory.WriteByte(&H4201A9, 3)
            PVZ.Memory.WriteByte(&H421FE1, 117)
            PVZ.Memory.WriteByte(&H421FE8, 133)
            PVZ.Memory.WriteByte(&H420533, 133)
            PVZ.Memory.WriteByte(&H424520, 117)
            PVZ.Memory.WriteByteArray(&H424520, 137, 129, 16, 86, 0, 0)
            PVZ.Memory.WriteByte(&H424773, 116)
            PVZ.Memory.WriteByte(&H422BC9, 117)
        End If
    End Sub
    ''' <summary>
    ''' 小僵尸
    ''' </summary>
    Public Shared Sub LittleZombie(Optional ByVal Switch As Boolean = True)
        BinaryCheck(&H523ED5, Switch)
    End Sub
    ''' <summary>
    ''' 柱子一样
    ''' </summary>
    Public Shared Sub ColumnLike(Optional ByVal Switch As Boolean = True)
        If Switch Then
            PVZ.Memory.WriteByte(&H410AE7, 128)
            PVZ.Memory.WriteByte(&H43903D, 128)
        Else
            PVZ.Memory.WriteByte(&H410AE7, 133)
            PVZ.Memory.WriteByte(&H43903D, 133)
        End If
    End Sub
    ''' <summary>
    ''' 僵尸快跑
    ''' </summary>
    Public Shared Sub ZombieQuick(Optional ByVal Switch As Boolean? = True)
        If Switch.HasValue Then
            If Switch Then
                PVZ.Memory.WriteByteArray(&H4248A3, &H83, &HB9, &HF8, &H7, &H0, &H0, &H1D)
                PVZ.Memory.WriteShort(&H4248AA, 2165)
                PVZ.Memory.WriteByteArray(&H4248AC, &H8B, &H4E, &H4)
                PVZ.Memory.WriteByteArray(&H4248AF, &HE8, &H6C, &H10, &HFF, &HFF)
                PVZ.Memory.WriteByte(&H4248AA, 112)
            Else
                PVZ.Memory.WriteByteArray(&H4248A3, &H83, &HB9, &HF8, &H7, &H0, &H0, &H1D)
                PVZ.Memory.WriteShort(&H4248AA, 2165)
                PVZ.Memory.WriteByteArray(&H4248AC, &H8B, &H4E, &H4)
                PVZ.Memory.WriteByteArray(&H4248AF, &HE8, &H6C, &H10, &HFF, &HFF)
                PVZ.Memory.WriteByte(&H4248AA, 117)
            End If
        Else
            PVZ.Memory.WriteByteArray(&H4248A3, &H8B, &H4E, &H4)
            PVZ.Memory.WriteByteArray(&H4248A6, &HE8, &H75, &H10, &HFF, &HFF)
            PVZ.Memory.WriteByteArray(&H4248AB, &H8B, &H4E, &H4)
            PVZ.Memory.WriteByteArray(&H4248AE, &HE8, &H6D, &H10, &HFF, &HFF)
            PVZ.Memory.WriteByte(&H4248B3, 144)
        End If
    End Sub
    ''' <summary>
    ''' 锤僵尸
    ''' </summary>
    Public Shared Sub WharkaZombie(Optional ByVal Switch As Boolean = True)
        If Switch Then
            PVZ.Memory.WriteByte(&H4222B1, 235)
            PVZ.Memory.WriteByte(&H4538A2, 112)
        Else
            PVZ.Memory.WriteByte(&H4222B1, 116)
            PVZ.Memory.WriteByte(&H4538A2, 117)
        End If
    End Sub
    ''' <summary>
    ''' 地心引力
    ''' </summary>
    Public Shared Sub HighGravity(Optional ByVal Switch As Boolean = True)
        If Switch Then
            PVZ.Memory.WriteByte(&H46D546, 112)
            PVZ.Memory.WriteByte(&H46DC28, 112)
            PVZ.Memory.WriteByte(&H525BED, 112)
        Else
            PVZ.Memory.WriteByte(&H46D546, 117)
            PVZ.Memory.WriteByte(&H46DC28, 117)
            PVZ.Memory.WriteByte(&H525BED, 117)
        End If
    End Sub
    ''' <summary>
    ''' 地里冒僵尸
    ''' </summary>
    Public Shared Sub ZombieDrillOut(Optional ByVal Switch As Boolean = True)
        If Switch Then
            PVZ.Memory.WriteByte(&H426594, 235)
        Else
            PVZ.Memory.WriteByte(&H426594, 116)
        End If
    End Sub
    ''' <summary>
    ''' 坟墓模式
    ''' </summary>
    Public Shared Sub GraveDanger(Optional ByVal Switch As Boolean = True)
        If Switch Then
            PVZ.Memory.WriteByte(&H4268FA, 112)
        Else
            PVZ.Memory.WriteByte(&H4268FA, 117)
        End If
    End Sub
    ''' <summary>
    ''' 黑暗暴风雨
    ''' </summary>
    Public Shared Sub DarkStormyNight(Optional ByVal Switch As Boolean = True)
        If Switch Then
            PVZ.Memory.WriteByte(&H416EEF, 235)
            PVZ.Memory.WriteByte(&H426B6B, 235)
            PVZ.Memory.WriteByte(&H426B35, 235)
            PVZ.Memory.WriteByte(&H4246C9, 235)
            PVZ.Memory.WriteByte(&H424729, 235)
        Else
            PVZ.Memory.WriteByte(&H416EEF, 116)
            PVZ.Memory.WriteByte(&H426B6B, 116)
            PVZ.Memory.WriteByte(&H426B35, 116)
            PVZ.Memory.WriteByte(&H4246C9, 116)
            PVZ.Memory.WriteByte(&H424729, 116)
        End If
    End Sub
    ''' <summary>
    ''' 蹦极闪电战
    ''' </summary>
    Public Shared Sub BungeeBlitz(Optional ByVal Switch As Boolean = True)
        If Switch Then
            PVZ.Memory.WriteByte(&H412F16, 117)
        Else
            PVZ.Memory.WriteByte(&H412F16, 116)
        End If
    End Sub
    ''' <summary>
    ''' 无尽模式
    ''' </summary>
    Public Shared Sub EndlessMode(Optional ByVal Switch As Boolean = True)
        If Switch Then
            PVZ.Memory.WriteByte(&H4139F7, 144)
            PVZ.Memory.WriteByte(&H4139F8, 144)
            PVZ.Memory.WriteByte(&H41F959, 144)
            PVZ.Memory.WriteByte(&H41F95A, 144)
            PVZ.Memory.WriteByte(&H41F95B, 144)
            PVZ.Memory.WriteByte(&H41F95C, 144)
            PVZ.Memory.WriteByte(&H41F95D, 144)
            PVZ.Memory.WriteByte(&H41F95E, 144)
            PVZ.Memory.WriteByte(&H40C030, 144)
            PVZ.Memory.WriteByte(&H40C031, 144)
            PVZ.Memory.WriteByte(&H40C03C, 144)
            PVZ.Memory.WriteByte(&H40C03D, 144)
            PVZ.Memory.WriteByte(&H4525A8, 144)
            PVZ.Memory.WriteByte(&H4525A9, 144)
            PVZ.Memory.WriteByte(&H459EE7, 144)
            PVZ.Memory.WriteByte(&H459EE8, 144)
            PVZ.Memory.WriteByte(&H459EE9, 144)
            PVZ.Memory.WriteByte(&H459EEA, 144)
            PVZ.Memory.WriteByte(&H459EEB, 144)
            PVZ.Memory.WriteByte(&H459EEC, 144)
            PVZ.Memory.WriteByte(&H45C6D2, 144)
            PVZ.Memory.WriteByte(&H45C6D3, 144)
            PVZ.Memory.WriteByte(&H48397B, 144)
            PVZ.Memory.WriteByte(&H48397C, 144)
            PVZ.Memory.WriteByte(&H43A720, 144)
            PVZ.Memory.WriteByte(&H43A721, 144)
            PVZ.Memory.WriteByte(&H40C47D, 144)
            PVZ.Memory.WriteByte(&H40C47E, 144)
            PVZ.Memory.WriteShort(&H43A7CF, 15851)
            PVZ.Memory.WriteByte(&H43BE4E, 144)
            PVZ.Memory.WriteByte(&H43BE4F, 144)
            PVZ.Memory.WriteByteArray(&H417EEA, &HE9, &H38, &H1, &H0, &H0)
            PVZ.Memory.WriteByte(&H417EEF, 144)
            PVZ.Memory.WriteByteArray(&H41DA16, &H8B, &H81, &H68, &H7, &H0, &H0)
            PVZ.Memory.WriteByteArray(&H41DA1C, &H8B, &H80, &H64, &H55, &H0, &H0)
            PVZ.Memory.WriteByte(&H41DA22, 195)
        Else
            PVZ.Memory.WriteByteArray(&H41DAD3, &HF, &H96, &HC0)
            PVZ.Memory.WriteByteArray(&H41DAD6, &HC2, &H4, &H0)
            PVZ.Memory.WriteShort(&H41DB25, 3191)
            PVZ.Memory.WriteShort(&H4139F7, 3455)
            PVZ.Memory.WriteByteArray(&H41F959, &HF, &H8F, &H6C, &H1, &H0, &H0)
            PVZ.Memory.WriteShort(&H40C030, 4735)
            PVZ.Memory.WriteShort(&H40C03C, 1663)
            PVZ.Memory.WriteShort(&H4525A8, 20863)
            PVZ.Memory.WriteByteArray(&H459EE7, &HF, &H8F, &H52, &H1, &H0, &H0)
            PVZ.Memory.WriteShort(&H45C6D2, 5759)
            PVZ.Memory.WriteShort(&H48397B, 6015)
            PVZ.Memory.WriteShort(&H43A720, 7039)
            PVZ.Memory.WriteShort(&H40C47D, 3455)
            PVZ.Memory.WriteShort(&H43A7CF, 15742)
            PVZ.Memory.WriteShort(&H43BE4E, 7295)
            PVZ.Memory.WriteByteArray(&H417EEA, &HF, &H8E, &H37, &H1, &H0, &H0)
            PVZ.Memory.WriteByteArray(&H41DA16, &H8B, &H81, &HF8, &H7, &H0, &H0)
            PVZ.Memory.WriteByteArray(&H41DA1C, &H83, &HF8, &H1F)
            PVZ.Memory.WriteShort(&H41DA1F, 1653)
            PVZ.Memory.WriteByteArray(&H41DA21, &HB8, &HA, &H0, &H0, &H0)
        End If
    End Sub
    ''' <summary>
    ''' 紫卡阳光增加
    ''' </summary>
    Public Shared Sub PurpleSunRaise(Optional ByVal Switch As Boolean? = True)
        If Switch.HasValue Then
            If Switch Then
                PVZ.Memory.WriteShort(&H41DAD3, 432)
                PVZ.Memory.WriteByteArray(&H41DAD5, &HC2, &H4, &H0)
                PVZ.Memory.WriteByte(&H41DB25, 144)
                PVZ.Memory.WriteByte(&H41DB26, 144)
                PVZ.Memory.WriteShort(&H41DB11, 8816)
            Else
                PVZ.Memory.WriteByteArray(&H41DAD3, &HF, &H96, &HC0)
                PVZ.Memory.WriteByteArray(&H41DAD6, &HC2, &H4, &H0)
                PVZ.Memory.WriteShort(&H41DB25, 3191)
                PVZ.Memory.WriteShort(&H41DB11, 8821)
            End If
        Else
            PVZ.Memory.WriteByteArray(&H41DAD3, &H30, &HC0, &H90)
            PVZ.Memory.WriteByteArray(&H41DAD6, &HC2, &H4, &H0)
            PVZ.Memory.WriteShort(&H41DB25, 3191)
            PVZ.Memory.WriteShort(&H41DB11, 8821)
        End If
    End Sub
    ''' <summary>
    ''' 全部紫卡判定
    ''' </summary>
    Public Shared Sub AllPurpleCard(Optional ByVal Switch As Boolean = True)
        If Switch Then
            PVZ.Memory.WriteInteger(&H41DAB8, 79823280)
        Else
            PVZ.Memory.WriteInteger(&H41DAB8, 79872050)
        End If
    End Sub
    ''' <summary>
    ''' 砸罐子模式
    ''' </summary>
    Public Shared Sub VaseCanBreak(Optional ByVal Switch As Boolean = True)
        If Switch Then
            PVZ.Memory.WriteShort(&H424791, 1392)
            PVZ.Memory.WriteShort(&H424796, 4075)
            PVZ.Memory.WriteShort(&H422183, 8304)
            PVZ.Memory.WriteShort(&H40EB36, 1392)
            PVZ.Memory.WriteShort(&H40EB3B, 4331)
            PVZ.Memory.WriteByteArray(&H411AE4, &HF, &H80, &HAE, &H0, &H0, &H0)
        Else
            PVZ.Memory.WriteShort(&H424791, 1404)
            PVZ.Memory.WriteShort(&H424796, 3966)
            PVZ.Memory.WriteShort(&H422183, 8308)
            PVZ.Memory.WriteShort(&H40EB36, 1404)
            PVZ.Memory.WriteShort(&H40EB3B, 4222)
            PVZ.Memory.WriteByteArray(&H411AE4, &HF, &H84, &HAE, &H0, &H0, &H0)
        End If
    End Sub
    ''' <summary>
    ''' 我是僵尸模式
    ''' </summary>
    Public Shared Sub IZombieMode(Optional ByVal Switch As Boolean = True)
        If Switch Then
            PVZ.Memory.WriteInteger(&H40FDC7, 3408437)
            PVZ.Memory.WriteInteger(&H438CFF, 3240701)
            PVZ.Memory.WriteInteger(&H438FAD, 3240015)
        Else
            PVZ.Memory.WriteInteger(&H40FDC7, 276997)
            PVZ.Memory.WriteInteger(&H438CFF, 109261)
            PVZ.Memory.WriteInteger(&H438FAD, 108575)
        End If
    End Sub
    ''' <summary>
    ''' 植物被压炸
    ''' </summary>
    Public Shared Sub PlantRollBlast(Optional ByVal Switch As Boolean? = True)
        If Switch.HasValue Then
            If Switch = True Then
                Memory.WriteShort(&H462B8E, 28139)
                Memory.WriteByteArray(&H466ADD, &HF, &H85, &H30, &HFC, &HFF, &HFF)
            Else
                Memory.WriteShort(&H462B8E, 7797)
                Memory.WriteByteArray(&H466ADD, &HF, &H85, &H8A, &H0, &H0, &H0)
            End If
        Else
            Memory.WriteShort(&H462B8E, 31467)
            Memory.WriteByteArray(&H466ADD, &HF, &H85, &H8A, &H0, &H0, &H0)
        End If
    End Sub
    ''' <summary>
    ''' 植物可以搭梯
    ''' </summary>
    Public Shared Sub PlantCanPutLadder(Optional ByVal Switch As Boolean? = True)
        If Switch.HasValue Then
            If Switch = True Then
                Memory.WriteByte(&H52E62E, 1)
                Memory.WriteByte(&H52E63F, 1)
            Else
                Memory.WriteByte(&H52E62E, 0)
                Memory.WriteByte(&H52E63F, 1)
            End If
        Else
            Memory.WriteByte(&H52E62E, 0)
            Memory.WriteByte(&H52E63F, 0)
        End If
    End Sub
    ''' <summary>
    ''' 植物可以搭梯
    ''' </summary>
    Public Shared Sub MushroomNoSleep(Optional ByVal Switch As Boolean? = True)
        If Switch.HasValue Then
            If Switch = True Then
                PVZ.Memory.WriteByte(&H45DE8E, 235)
            Else
                PVZ.Memory.WriteByte(&H45DE8E, 116)
                PVZ.Memory.WriteByte(&H45DE93, 116)
                PVZ.Memory.WriteByte(&H45DE98, 116)
                PVZ.Memory.WriteByte(&H45DE9D, 116)
                PVZ.Memory.WriteByte(&H45DEA2, 116)
            End If
        Else
            PVZ.Memory.WriteByte(&H45DE8E, 112)
            PVZ.Memory.WriteByte(&H45DE93, 112)
            PVZ.Memory.WriteByte(&H45DE98, 112)
            PVZ.Memory.WriteByte(&H45DE9D, 112)
            PVZ.Memory.WriteByte(&H45DEA2, 112)
        End If
    End Sub
    ''' <summary>
    ''' 植物魅惑菇
    ''' </summary>
    Public Shared Sub AllHypnoshroon(Optional ByVal Switch As Boolean? = True)
        If Switch.HasValue Then
            If Switch = True Then
                PVZ.Memory.WriteQword(&H52B96A, -4719772409470746609)
                PVZ.Memory.WriteByte(&H52FC0A, 12)
            Else
                PVZ.Memory.WriteQword(&H52B96A, -4719772409470745329)
                PVZ.Memory.WriteByte(&H52FC0A, 12)
            End If
        Else
            PVZ.Memory.WriteQword(&H52B96A, -4719614079809826839)
            PVZ.Memory.WriteByte(&H52FC0A, 255)
        End If
    End Sub
    ''' <summary>
    ''' 植物火炬
    ''' </summary>
    Public Shared Sub AllTochwood(Optional ByVal Switch As Boolean = True)
        If Switch = True Then
            PVZ.Memory.WriteByte(&H4633A4, 112)
        Else
            PVZ.Memory.WriteByte(&H4633A4, 117)
        End If
    End Sub
    ''' <summary>
    ''' 植物高坚果
    ''' </summary>
    Public Shared Sub AllTallNut(Optional ByVal Switch As Boolean? = True)
        ThreeStateCheck(&H525FA8, Switch)
    End Sub
    ''' <summary>
    ''' 植物路灯花
    ''' </summary>
    Public Shared Sub AllPlantern(Optional ByVal Switch As Boolean? = True)
        ThreeStateCheck(&H41A70A, Switch)
        If Switch.HasValue Then
            Memory.WriteByteArray(&H41A705, &HEB, &H8)
        Else
            Memory.WriteByteArray(&H41A705, &H90, &H90)
        End If
    End Sub
    ''' <summary>
    ''' 毁灭菇无痕
    ''' </summary>
    Public Shared Sub NoCrater(Optional ByVal Switch? As Boolean = True)
        If Switch.HasValue Then
            If Switch Then
                PVZ.Memory.WriteByte(&H41D79E, 112)
                PVZ.Memory.WriteByte(&H41D799, 255)
            Else
                PVZ.Memory.WriteByte(&H41D79E, 117)
                PVZ.Memory.WriteByte(&H41D799, 255)
            End If
        Else
            PVZ.Memory.WriteByte(&H41D79E, 117)
            PVZ.Memory.WriteByte(&H41D799, 0)
        End If
    End Sub
    ''' <summary>
    ''' 强力三叶草
    ''' </summary>
    Public Shared Sub StrongBlover(Optional ByVal Switch As Boolean = True)

        If Switch Then
            PVZ.Memory.WriteByteArray(&H4665FE, &H83, &HF8, &H49)
            PVZ.Memory.WriteShort(&H466601, 1515)
        Else
            PVZ.Memory.WriteByteArray(&H4665FE, &H83, &HF8, &H49)
            PVZ.Memory.WriteShort(&H466601, 1396)
        End If
    End Sub
    ''' <summary>
    ''' 植物大蒜
    ''' </summary>
    Public Shared Sub AllGarlic(Optional ByVal Switch? As Boolean = True)
        If Switch.HasValue Then
            If Switch Then
                PVZ.Memory.WriteByte(&H52BA41, 112)
            Else
                PVZ.Memory.WriteByte(&H52BA41, 117)
            End If
        Else
            PVZ.Memory.WriteByte(&H52BA41, 235)
        End If
    End Sub
    ''' <summary>
    ''' 植物保护伞
    ''' </summary>
    Public Shared Sub AllUmbrellaLeaf(Optional ByVal Switch? As Boolean = True)
        If Switch.HasValue Then
            If Switch Then
                PVZ.Memory.WriteByte(&H41D3F8, 112)
            Else
                PVZ.Memory.WriteByte(&H41D3F8, 117)
            End If
        Else
            PVZ.Memory.WriteByte(&H41D3F8, 235)
        End If
    End Sub
    ''' <summary>
    ''' 香蒲子弹触碰伤害
    ''' </summary>
    Public Shared Sub CattailNoWaste(Optional ByVal Switch As Boolean = True)
        If Switch Then
            PVZ.Memory.WriteByte(&H46CECC, 129)
        Else
            PVZ.Memory.WriteByte(&H46CECC, 133)
        End If
    End Sub
    ''' <summary>
    ''' 锁定黄油
    ''' </summary>
    Public Shared Sub LockButter(Optional ByVal Switch? As Boolean = True)
        If Switch.HasValue Then
            If Switch Then
                PVZ.Memory.WriteByte(&H45F1EC, 112)
            Else
                PVZ.Memory.WriteByte(&H45F1EC, 117)
            End If
        Else
            PVZ.Memory.WriteByte(&H45F1EC, 235)
        End If
    End Sub
    ''' <summary>
    ''' 超级保龄球
    ''' </summary>
    Public Shared Sub SuperBowling(Optional ByVal Switch As Boolean = True)
        If Switch Then
            PVZ.Memory.WriteByte(&H462D30, 112)
            PVZ.Memory.WriteByte(&H4630D4, 235)
        Else
            PVZ.Memory.WriteByte(&H462D30, 117)
            PVZ.Memory.WriteByte(&H4630D4, 116)
        End If
    End Sub
    ''' <summary>
    ''' 植物反向攻击
    ''' </summary>
    Public Shared Sub PlantFaceLeft(Optional ByVal Switch As Boolean = True)
        If Switch Then
            PVZ.Memory.WriteInteger(&H45DCDC, 0)
            PVZ.Memory.WriteByte(&H468017, 112)
            PVZ.Memory.WriteByte(&H467526, 4)
            PVZ.Memory.WriteByte(&H467373, 6)
            PVZ.Memory.WriteByte(&H46736E, 112)
            PVZ.Memory.WriteByte(&H463B92, 112)
            PVZ.Memory.WriteByte(&H466190, 112)
            PVZ.Memory.WriteByte(&H467306, 4)
            PVZ.Memory.WriteByte(&H467332, 6)
            PVZ.Memory.WriteByte(&H467428, 6)
            PVZ.Memory.WriteByte(&H467373, 6)
        Else
            PVZ.Memory.WriteInteger(&H45DCDC, -1)
            PVZ.Memory.WriteByte(&H468017, 117)
            PVZ.Memory.WriteByte(&H467526, 1)
            PVZ.Memory.WriteByte(&H467373, 2)
            PVZ.Memory.WriteByte(&H46736E, 117)
            PVZ.Memory.WriteByte(&H463B92, 117)
            PVZ.Memory.WriteByte(&H466190, 117)
            PVZ.Memory.WriteByte(&H467306, 2)
            PVZ.Memory.WriteByte(&H467332, 2)
            PVZ.Memory.WriteByte(&H467428, 5)
            PVZ.Memory.WriteByte(&H467373, 2)
        End If
    End Sub
    ''' <summary>
    ''' 禁止种植植物
    ''' </summary>
    Public Shared Sub PlantForbidden(Optional ByVal Switch As Boolean = True)
        If Switch Then
            PVZ.Memory.WriteByte(&H41D823, 112)
        Else
            PVZ.Memory.WriteByte(&H41D823, 117)
        End If
    End Sub
    ''' <summary>
    ''' 植物持续攻击
    ''' </summary>
    Public Shared Sub PlantAlwaysFight(Optional ByVal Switch? As Boolean = True)
        If Switch.HasValue Then
            If Switch Then
                PVZ.Memory.WriteShort(&H46040B, 1392)
                PVZ.Memory.WriteShort(&H45F6E9, 10096)
                PVZ.Memory.WriteByteArray(&H45F31F, &HF, &H80, &H37, &H1, &H0, &H0)
                PVZ.Memory.WriteByteArray(&H45F330, &HF, &H80, &H26, &H1, &H0, &H0)
                PVZ.Memory.WriteShort(&H45EF2C, 2795)
                PVZ.Memory.WriteShort(&H45F2C2, 29301)
            Else
                PVZ.Memory.WriteShort(&H46040B, 1396)
                PVZ.Memory.WriteShort(&H45F6E9, 10100)
                PVZ.Memory.WriteByteArray(&H45F31F, &HF, &H84, &H37, &H1, &H0, &H0)
                PVZ.Memory.WriteByteArray(&H45F330, &HF, &H84, &H26, &H1, &H0, &H0)
                PVZ.Memory.WriteShort(&H45EF2C, 2677)
                PVZ.Memory.WriteShort(&H45F2C2, 29301)
            End If
        Else
            PVZ.Memory.WriteShort(&H46040B, 1515)
            PVZ.Memory.WriteShort(&H45F6E9, 10219)
            PVZ.Memory.WriteByteArray(&H45F31F, &HE9, &H38, &H1, &H0, &H0)
            PVZ.Memory.WriteByte(&H45F324, 144)
            PVZ.Memory.WriteByteArray(&H45F330, &HE9, &H27, &H1, &H0, &H0)
            PVZ.Memory.WriteByte(&H45F335, 144)
            PVZ.Memory.WriteShort(&H45EF2C, 2672)
            PVZ.Memory.WriteShort(&H45F2C2, 29296)
        End If
    End Sub
    ''' <summary>
    ''' 灰烬全屏
    ''' </summary>
    Public Shared Sub FullScreenBlast(Optional ByVal Switch? As Boolean = True)
        If Switch.HasValue Then
            If Switch Then
                PVZ.Memory.WriteShort(&H41D8FF, 8427)
                PVZ.Memory.WriteByte(&H4664F2, 112)
            Else
                PVZ.Memory.WriteShort(&H41D8FF, 15999)
                PVZ.Memory.WriteByte(&H4664F2, 117)
            End If
        Else
            PVZ.Memory.WriteShort(&H41D8FF, 15985)
            PVZ.Memory.WriteByte(&H4664F2, 235)
        End If
    End Sub
    ''' <summary>
    ''' 倭瓜全屏
    ''' </summary>
    Public Shared Sub FullScreenSquash(Optional ByVal Switch? As Boolean = True)
        If Switch.HasValue Then
            If Switch Then
                PVZ.Memory.WriteShort(&H460929, 17899)
                PVZ.Memory.WriteQword(&H460837, -4935945191574831089)
                PVZ.Memory.WriteShort(&H460747, 24555)
            Else
                PVZ.Memory.WriteShort(&H460929, 28799)
                PVZ.Memory.WriteQword(&H460837, -4935945191574829809)
                PVZ.Memory.WriteShort(&H460747, 29301)
            End If
        Else
            PVZ.Memory.WriteShort(&H460929, 28785)
            PVZ.Memory.WriteQword(&H460837, -4935786861923572759)
            PVZ.Memory.WriteShort(&H460747, 29296)
        End If
    End Sub
    ''' <summary>
    ''' 磁力菇全屏
    ''' </summary>
    Public Shared Sub FullScreenMagnetshroom(Optional ByVal Switch? As Boolean = True)
        If Switch.HasValue Then
            If Switch Then
                PVZ.Memory.WriteQword(&H4620A2, -539587530339155953)
                PVZ.Memory.WriteByte(&H462124, 112)
            Else
                PVZ.Memory.WriteLong(&H4620A2, -539587530339154161)
                PVZ.Memory.WriteByte(&H462124, 116)
            End If
        Else
            PVZ.Memory.WriteLong(&H4620A2, -539429200679868183)
            PVZ.Memory.WriteByte(&H462124, 235)
        End If
    End Sub
    ''' <summary>
    ''' 溅射全屏
    ''' </summary>
    Public Shared Sub FullScreenSputter(Optional ByVal Switch? As Boolean = True)
        If Switch.HasValue Then
            If Switch Then
                PVZ.Memory.WriteByte(&H46D455, 112)
            Else
                PVZ.Memory.WriteByte(&H46D455, 116)
            End If
        Else
            PVZ.Memory.WriteByte(&H46D455, 235)
        End If
    End Sub
    ''' <summary>
    ''' IZ纸板样式
    ''' </summary>
    Public Shared Sub IZPlantStyle(Optional ByVal Switch? As Boolean = True)
        If Switch.HasValue Then
            If Switch Then
                PVZ.Memory.WriteByte(&H465DF2, 112)
            Else
                PVZ.Memory.WriteByte(&H465DF2, 116)
            End If
        Else
            PVZ.Memory.WriteByte(&H465DF2, 235)
        End If
    End Sub
    ''' <summary>
    ''' 撑杆无限跳
    ''' </summary>
    Public Shared Sub ZombieInfinitePole(Optional ByVal Switch As Boolean = True)
        If Switch Then
            PVZ.Memory.WriteInteger(&H52613D, 11)
        Else
            PVZ.Memory.WriteInteger(&H52613D, 13)
        End If
    End Sub
    ''' <summary>
    ''' 二爷不愤怒
    ''' </summary>
    Public Shared Sub ErYeNoAngry(Optional ByVal Switch As Boolean = True)
        If Switch Then
            PVZ.Memory.WriteInteger(&H525D21, 29)
        Else
            PVZ.Memory.WriteInteger(&H525D21, 31)
        End If
    End Sub
    ''' <summary>
    ''' 僵尸召唤舞伴
    ''' </summary>
    Public Shared Sub AllDancer(Optional ByVal Switch As Boolean = True)
        If Switch Then
            Memory.WriteByte(&H52B222, &H70)
        Else
            Memory.WriteByte(&H52B222, &H75)
        End If
    End Sub
    ''' <summary>
    ''' 僵尸跳舞
    ''' </summary>
    Public Shared Sub AllDance(Optional ByVal Switch As Boolean = True)
        If Switch Then
            Memory.WriteByte(&H52B22F, &H70)
        Else
            Memory.WriteByte(&H52B22F, &H75)
        End If
    End Sub
    ''' <summary>
    ''' 僵尸留冰道
    ''' </summary>
    Public Shared Sub AllIceTrace(Optional ByVal Switch As Boolean = True)
        If Switch Then
            Memory.WriteByte(&H52B1FD, &H70)
        Else
            Memory.WriteByte(&H52B1FD, &H75)
        End If
    End Sub
    ''' <summary>
    ''' 僵尸匀速前进
    ''' </summary>
    Public Shared Sub ZombieUniformSpeed(Optional ByVal Switch As Boolean = True)
        If Switch Then
            PVZ.Memory.WriteByte(&H52AAAD, 235)
        Else
            PVZ.Memory.WriteByte(&H52AAAD, 116)
        End If
    End Sub
    ''' <summary>
    ''' 小丑不炸
    ''' </summary>
    Public Shared Sub JackNoBoom(Optional ByVal Switch As Boolean = True)
        If Switch Then
            PVZ.Memory.WriteByte(&H526AFC, 129)
        Else
            PVZ.Memory.WriteByte(&H526AFC, 143)
        End If
    End Sub
    ''' <summary>
    ''' 气球自爆
    ''' </summary>
    Public Shared Sub BalloonBoom(Optional ByVal Switch As Boolean = True)
        If Switch Then
            PVZ.Memory.WriteByte(&H525CAB, 112)
        Else
            PVZ.Memory.WriteByte(&H525CAB, 117)
            PVZ.Memory.WriteByte(&H525CC6, 116)
        End If
    End Sub
    ''' <summary>
    ''' 矿工出土向前
    ''' </summary>
    Public Shared Sub DiggerForword(Optional ByVal Switch As Boolean = True)
        If Switch Then
            PVZ.Memory.WriteInteger(&H528348, 35)
        Else
            PVZ.Memory.WriteInteger(&H528348, 33)
        End If
    End Sub
    ''' <summary>
    ''' 跳跳无判定
    ''' </summary>
    Public Shared Sub PogoNoPogo(Optional ByVal Switch As Boolean = True)
        If Switch Then
            PVZ.Memory.WriteByte(&H5232F9, 0)
        Else
            PVZ.Memory.WriteByte(&H5232F9, 1)
        End If
    End Sub
    ''' <summary>
    ''' 僵尸速度加快
    ''' </summary>
    Public Shared Sub ZombieSpeedUp(Optional ByVal Switch As Boolean = True)
        If Switch Then
            Memory.WriteByte(&H52B215, &H70)
        Else
            Memory.WriteByte(&H52B215, &H75)
        End If
    End Sub
    ''' <summary>
    ''' 僵尸速度更快
    ''' </summary>
    Public Shared Sub ZombieSpeedUpUp(Optional ByVal Switch As Boolean = True)
        If Switch Then
            PVZ.Memory.WriteShort(&H52F103, 21621)
        Else
            PVZ.Memory.WriteShort(&H52F103, 21620)
        End If
    End Sub
    ''' <summary>
    ''' 逆向僵尸
    ''' </summary>
    Public Shared Sub ZombieReverse(Optional ByVal Switch As Boolean = True)
        If Switch Then
            PVZ.Memory.WriteByte(&H52BF4F, 112)
        Else
            PVZ.Memory.WriteByte(&H52BF4F, 117)
        End If
    End Sub
    ''' <summary>
    ''' 无限搭梯
    ''' </summary>
    Public Shared Sub InfiniteLadder(Optional ByVal Switch As Boolean = True)
        If Switch Then
            PVZ.Memory.WriteByteArray(&H52AA1A, &H90, &H90, &H90, &H90, &H90)
        Else
            PVZ.Memory.WriteByteArray(&H52AA1A, &HE8, &HC1, &H86, &H0, &H0)
        End If
    End Sub
    ''' <summary>
    ''' 僵尸变巨人
    ''' </summary>
    Public Shared Sub AllGargantuar(Optional ByVal Switch As Boolean = True)
        If Switch Then
            Memory.WriteByte(&H52B1DF, 235)
        Else
            Memory.WriteByte(&H52B1DF, &H74)
        End If
    End Sub
    ''' <summary>
    ''' 横向降维打击
    ''' </summary>
    Public Shared Sub HRollAttack(Optional ByVal Switch? As Boolean = True)
        If Switch.HasValue Then
            If Switch Then
                PVZ.Memory.WriteByteArray(&H52E953, &H3B, &H51, &H28)
                PVZ.Memory.WriteShort(&H52E94D, 12661)
                PVZ.Memory.WriteByteArray(&H52E953, &H90, &H90, &H90)
            Else
                PVZ.Memory.WriteShort(&H52E94D, 12661)
            End If
        Else
            PVZ.Memory.WriteShort(&H52E94D, 12661)
            PVZ.Memory.WriteByteArray(&H52E953, &H3B, &H51, &H28)
            PVZ.Memory.WriteShort(&H52E94D, -28528)
        End If
    End Sub
    ''' <summary>
    ''' 僵尸全屏判定
    ''' </summary>
    Public Shared Sub ZombieAllCollison(Optional ByVal Switch As Boolean = True)
        If Switch Then
            PVZ.Memory.WriteByte(&H46761E, 112)
        Else
            PVZ.Memory.WriteByte(&H46761E, 117)
        End If
    End Sub
    ''' <summary>
    ''' 僵尸发射豌豆
    ''' </summary>
    Public Shared Sub ZombiePea(Optional ByVal Switch As Boolean = True)
        If Switch Then
            PVZ.Memory.WriteByte(&H52B249, 112)
        Else
            PVZ.Memory.WriteByte(&H52B249, 117)
        End If
    End Sub
    ''' <summary>
    ''' 辣椒不炸
    ''' </summary>
    Public Shared Sub JalapenoNoBoom(Optional ByVal Switch As Boolean = True)
        If Switch Then
            PVZ.Memory.WriteByte(&H5275DD, 129)
        Else
            PVZ.Memory.WriteByte(&H5275DD, 133)
        End If
    End Sub
    ''' <summary>
    ''' 僵尸机枪豌豆
    ''' </summary>
    Public Shared Sub ZombieGatling(Optional ByVal Switch As Boolean = True)
        If Switch Then
            PVZ.Memory.WriteByte(&H52B263, 112)
        Else
            PVZ.Memory.WriteByte(&H52B263, 117)
        End If
    End Sub
    ''' <summary>
    ''' 免疫减速
    ''' </summary>
    Public Shared Sub IceProof(Optional ByVal Switch As Boolean = True)
        If Switch Then
            PVZ.Memory.WriteByte(&H53095C, 42)
        Else
            PVZ.Memory.WriteByte(&H53095C, 132)
        End If
    End Sub
    ''' <summary>
    ''' 免疫黄油
    ''' </summary>
    Public Shared Sub ButterProof(Optional ByVal Switch As Boolean = True)
        If Switch Then
            PVZ.Memory.WriteByte(&H531A1A, 235)
        Else
            PVZ.Memory.WriteByte(&H531A1A, 116)
        End If
    End Sub
    ''' <summary>
    ''' 免疫磁力菇
    ''' </summary>
    Public Shared Sub MagnetProof(Optional ByVal Switch As Boolean = True)
        If Switch Then
            PVZ.Memory.WriteByte(&H4620C5, 255)
            PVZ.Memory.WriteByte(&H4620CA, 255)
            PVZ.Memory.WriteByte(&H4620D5, 255)
            PVZ.Memory.WriteByte(&H4620DA, 255)
            PVZ.Memory.WriteByte(&H4620DF, 255)
            PVZ.Memory.WriteByte(&H4620ED, 1)
        Else
            PVZ.Memory.WriteByte(&H4620C5, 2)
            PVZ.Memory.WriteByte(&H4620CA, 3)
            PVZ.Memory.WriteByte(&H4620D5, 1)
            PVZ.Memory.WriteByte(&H4620DA, 3)
            PVZ.Memory.WriteByte(&H4620DF, 15)
            PVZ.Memory.WriteByte(&H4620ED, 0)
        End If
    End Sub
    ''' <summary>
    ''' 僵尸不前进
    ''' </summary>
    Public Shared Sub ZombieNoWork(Optional ByVal Switch As Boolean = True)
        If Switch Then
            PVZ.Memory.WriteByte(&H52AB2B, 84)
            PVZ.Memory.WriteByte(&H52AB34, 84)
        Else
            PVZ.Memory.WriteByte(&H52AB2B, 100)
            PVZ.Memory.WriteByte(&H52AB34, 68)
        End If
    End Sub
    ''' <summary>
    ''' 无限制
    ''' </summary>
    Public Shared Sub AppearNoLimit(Optional ByVal Switch As Boolean = True)
        If Switch Then
            PVZ.Memory.WriteInteger(&H40D91A, -125605653)
            PVZ.Memory.WriteInteger(&H41C078, -125619221)
        Else
            PVZ.Memory.WriteInteger(&H40D91A, -125605772)
            PVZ.Memory.WriteInteger(&H41C078, -125619340)
        End If
    End Sub
    ''' <summary>
    ''' 传送带
    ''' </summary>
    Public Shared Sub AppearVeryMuch(Optional ByVal Switch As Boolean = True)
        If Switch Then
            PVZ.Memory.WriteLong(&H40988A, -539429200679874071)
            PVZ.Memory.WriteByte(&H40AB5A, 235)
            PVZ.Memory.WriteByte(&H40D487, 235)
            PVZ.Memory.WriteByte(&H4112EC, 139)
            PVZ.Memory.WriteByte(&H419B91, 235)
            PVZ.Memory.WriteByte(&H41C058, 235)
            PVZ.Memory.WriteByte(&H41D149, 235)
            PVZ.Memory.WriteByte(&H41F7D7, 235)
            PVZ.Memory.WriteByte(&H41F831, 235)
        Else
            PVZ.Memory.WriteLong(&H40988A, -539587530340662257)
            PVZ.Memory.WriteByte(&H40AB5A, 116)
            PVZ.Memory.WriteByte(&H40D487, 116)
            PVZ.Memory.WriteByte(&H4112EC, 116)
            PVZ.Memory.WriteByte(&H419B91, 116)
            PVZ.Memory.WriteByte(&H41C058, 116)
            PVZ.Memory.WriteByte(&H41D149, 116)
            PVZ.Memory.WriteByte(&H41F7D7, 116)
            PVZ.Memory.WriteByte(&H41F831, 116)
        End If
    End Sub
    ''' <summary>
    ''' 柱子
    ''' </summary>
    Public Shared Sub AppearLikeColumn(Optional ByVal Switch As Boolean = True)
        If Switch Then
            PVZ.Memory.WriteByte(&H40983A, 112)
        Else
            PVZ.Memory.WriteByte(&H40983A, 117)
        End If
    End Sub
    ''' <summary>
    ''' 小丑反水
    ''' </summary>
    Public Shared Sub JackBetray(Optional ByVal Switch As Boolean = True)
        If Switch Then
            PVZ.Memory.WriteByte(&H526C46, 112)
        Else
            PVZ.Memory.WriteByte(&H526C46, 116)
        End If
    End Sub
    ''' <summary>
    ''' 攻击加速
    ''' </summary>
    Public Shared Sub AttackAccelerate(Optional ByVal Switch As Boolean = True)
        If Switch Then
            PVZ.Memory.WriteInteger(&H45F8BF, 9657)
            PVZ.Memory.WriteByte(&H45F8C3, 0)
        Else
            PVZ.Memory.WriteInteger(&H45F8BF, 693915275)
            PVZ.Memory.WriteByte(&H45F8C3, 193)
        End If
    End Sub
    ''' <summary>
    ''' 攻击重叠
    ''' </summary>
    Public Shared Sub AttackOverlap(Optional ByVal Switch As Boolean = True)
        If Switch Then
            PVZ.Memory.WriteByte(&H464A97, 132)
        Else
            PVZ.Memory.WriteByte(&H464A97, 133)
        End If
    End Sub
    ''' <summary>
    ''' 攻击额外
    ''' </summary>
    Public Shared Sub AttackAddition(Optional ByVal Switch As Boolean = True)
        If Switch Then
            PVZ.Memory.WriteByte(&H45F926, 112)
        Else
            PVZ.Memory.WriteByte(&H45F926, 117)
        End If
    End Sub
    ''' <summary>
    ''' 攻击多元
    ''' </summary>
    Public Shared Sub MuitiAttack(Optional ByVal Switch As Boolean = True)
        If Switch Then
            PVZ.Memory.WriteByte(&H464A99, 255)
        Else
            PVZ.Memory.WriteByte(&H464A99, 254)
        End If
    End Sub
    ''' <summary>
    ''' 默认裸地
    ''' </summary>
    Public Shared Sub DefaultUnsodded(Optional ByVal Switch As Boolean = True)
        If Switch Then
            PVZ.Memory.WriteByte(&H407EEB, 2)
        Else
            PVZ.Memory.WriteByte(&H407EEB, 1)
        End If
    End Sub
    ''' <summary>
    ''' 裸地视作屋顶
    ''' </summary>
    Public Shared Sub UnsoddedAsRoof(Optional ByVal Switch As Boolean = True)
        If Switch Then

            PVZ.Memory.WriteByte(&H40E204, 128)
            PVZ.Memory.WriteByteArray(&H41C0B0, &H51, &H52, &H8B, &H88, &H3C, &H1, &H0, &H0, &H8B, &H51, &H28, &H8B, &H49, &H24, &H8D, &HC, &H49, &H1, &HC9, &H1, &HD1, &H83, &HBC, &H88, &H68, &H1, &H0, &H0, &H2, &HEB, &H18, &HCC)
            PVZ.Memory.WriteByteArray(&H41C0E6, &HCC, &HF, &H94, &HC0, &H5A, &H59, &HC3, &HCC, &HCC, &HCC)
        Else

            PVZ.Memory.WriteByte(&H40E204, 132)
            PVZ.Memory.WriteByteArray(&H41C0B0, &H8B, &H80, &H4C, &H55, &H0, &H0, &H83, &HF8, &H4, &H74, &H8, &H83, &HF8, &H5, &H74, &H3, &H32, &HC0, &HC3, &HB0, &H1, &HC3, &HCC, &HCC, &HCC, &HCC, &HCC, &HCC, &HCC, &HCC, &HCC, &HCC)
            PVZ.Memory.WriteByteArray(&H41C0E6, &HCC, &HCC, &HCC, &HCC, &HCC, &HCC, &HCC, &HCC, &HCC, &HCC)
        End If
    End Sub
    ''' <summary>
    ''' 锁定冰道
    ''' </summary>
    Public Shared Sub LockIcetrace(Optional ByVal Switch As Boolean = True)
        If Switch Then
            PVZ.Memory.WriteByte(&H414131, 0)
        Else
            PVZ.Memory.WriteByte(&H414131, 255)
        End If
    End Sub
    ''' <summary>
    ''' 修正物品价值代码
    ''' </summary>
    Public Shared Sub FixeItemValueCode(Optional ByVal Switch As Boolean = True)
        If Switch Then
            PVZ.Memory.WriteByteArray(&H430A50, &H7, &HBA, &H5, &H0, &H0, &H0, &HEB, &HC, &H83, &HF8, &H3, &HBA, &H64, &H0, &H0, &H0, &H90, &H90, &H90, &H90)
            PVZ.Memory.WriteByteArray(&H4309FA, &H7, &HB9, &HF, &H0, &H0, &H0, &HEB, &HC, &HB9, &H32, &H0, &H0, &H0, &H90, &H90, &H90, &H90, &H90, &H90, &H90)
        Else
            PVZ.Memory.WriteByteArray(&H430A50, &H5, &H8D, &H50, &H3, &HEB, &HE, &H33, &HD2, &H83, &HF8, &H3, &HF, &H95, &HC2, &H83, &HEA, &H1, &H83, &HE2, &H64)
            PVZ.Memory.WriteByteArray(&H4309FA, &H5, &H8D, &H48, &HA, &HEB, &HE, &H33, &HC9, &H83, &HF8, &H6, &HF, &H95, &HC1, &H83, &HE9, &H1, &H83, &HE1, &H32)
        End If
    End Sub
    ''' <summary>
    ''' 不掉落物品
    ''' </summary>
    Public Shared Sub NoCoinDrop(Optional ByVal Switch As Boolean = True)
        If Switch Then
            PVZ.Memory.WriteByte(&H42FF96, 77)
        Else
            PVZ.Memory.WriteByte(&H42FF96, 93)
        End If
    End Sub
    ''' <summary>
    ''' 物品不消失
    ''' </summary>
    Public Shared Sub CoinNoDisappear(Optional ByVal Switch As Boolean = True)
        If Switch Then
            PVZ.Memory.WriteByte(&H430DD1, 0)
        Else
            PVZ.Memory.WriteByte(&H430DD1, 1)
        End If
    End Sub
    ''' <summary>
    ''' 传送带需要阳光
    ''' </summary>
    Public Shared Sub ConveyorBeltNeedSun(Optional ByVal Switch As Boolean = True)
        If Switch Then
            PVZ.Memory.WriteByte(&H40F8AD, 112)
            PVZ.Memory.WriteByte(&H410860, 112)
            PVZ.Memory.WriteByte(&H413B55, 128)
            PVZ.Memory.WriteByte(&H488391, 112)
            PVZ.Memory.WriteByte(&H48882E, 128)
            PVZ.Memory.WriteByte(&H4896D4, 235)
            PVZ.Memory.WriteByte(&H489820, 128)
        Else
            PVZ.Memory.WriteByte(&H40F8AD, 117)
            PVZ.Memory.WriteByte(&H410860, 117)
            PVZ.Memory.WriteByte(&H413B55, 133)
            PVZ.Memory.WriteByte(&H488391, 117)
            PVZ.Memory.WriteByte(&H48882E, 133)
            PVZ.Memory.WriteByte(&H4896D4, 116)
            PVZ.Memory.WriteByte(&H489820, 133)

        End If
    End Sub

    ''' <summary>
    ''' 启动卡片限制
    ''' </summary>
    Public Shared Sub EnableCardLimit(Optional ByVal Switch As Boolean = True)
        If Switch Then
            PVZ.Memory.WriteByte(&H482E16, &H70)
            PVZ.Memory.WriteByte(&H482F5E, &H70)
            PVZ.Memory.WriteByte(&H4832A6, &H70)
            PVZ.Memory.WriteByte(&H484A39, &H70)
            PVZ.Memory.WriteByte(&H486B88, &H70)
            PVZ.Memory.WriteByte(&H4850FC, &H80)
            PVZ.Memory.WriteByte(&H4830F6, &H70)
            PVZ.Memory.WriteByte(&H486372, &H70)
        Else
            PVZ.Memory.WriteByte(&H482E16, &H75)
            PVZ.Memory.WriteByte(&H482F5E, &H75)
            PVZ.Memory.WriteByte(&H4832A6, &H75)
            PVZ.Memory.WriteByte(&H484A39, &H75)
            PVZ.Memory.WriteByte(&H486B88, &H75)
            PVZ.Memory.WriteByte(&H4850FC, &H85)
            PVZ.Memory.WriteByte(&H4830F6, &H75)
            PVZ.Memory.WriteByte(&H486372, &H75)
        End If
    End Sub
    ''' <summary>
    ''' 启动植物限制线
    ''' </summary>
    Public Shared Sub EnablePlantLineLimit(Optional ByVal Switch As Boolean = True)
        If Switch Then
            PVZ.Memory.WriteByte(&H42536C, 235)
            PVZ.Memory.WriteByte(&H42556A, 235)
            PVZ.Memory.WriteByte(&H425387, &H70)
        Else
            PVZ.Memory.WriteByte(&H42536C, 116)
            PVZ.Memory.WriteByte(&H42556A, 116)
            PVZ.Memory.WriteByte(&H425387, &H74)
        End If
    End Sub
    ''' <summary>
    ''' 启动僵尸放置限制线
    ''' </summary>
    Public Shared Sub EnableZombieLineLimit(Optional ByVal Switch As Boolean = True)
        If Switch Then
            PVZ.Memory.WriteByte(&H42559D, 235)
            PVZ.Memory.WriteByte(&H4253A8, 235)
        Else
            PVZ.Memory.WriteByte(&H42559D, 116)
            PVZ.Memory.WriteByte(&H4253A8, 116)
        End If
    End Sub
    ''' <summary>
    ''' 铁桶铁门僵尸
    ''' </summary>
    Public Shared Sub BucketheadWithScreenDoorZombie(Optional ByVal Switch As Boolean = True)
        If Switch Then
            PVZ.Memory.WriteInteger(&H52295A, 4294967115)
        Else
            PVZ.Memory.WriteInteger(&H52295A, 5360)
        End If
    End Sub
#End Region
#Region "用户功能"
    Public Shared Sub Plantf(ByVal Type As PlantType, ByVal Row As Integer, ByVal Column As Integer)
        If Row = 0 And Column = 0 Then
            For i = 0 To 8
                If SixRoute Then
                    For j = 0 To 5
                        CreatePlant(Type, j, i)
                    Next
                Else
                    For j = 0 To 4
                        CreatePlant(Type, j, i)
                    Next
                End If
            Next
        ElseIf Row = 0 And Column > 0 Then
            If SixRoute Then
                For i = 0 To 5
                    CreatePlant(Type, i, Column - 1)
                Next
            Else
                For i = 0 To 4
                    CreatePlant(Type, i, Column - 1)
                Next
            End If
        ElseIf Row > 0 And Column = 0 Then
            For i = 0 To 8
                CreatePlant(Type, Row - 1, i)
            Next
        Else
            CreatePlant(Type, Row - 1, Column - 1)
        End If
    End Sub
    Public Shared Sub Zombief(ByVal Type As ZombieType, ByVal Row As Integer, ByVal Column As Integer)
        If Row = 0 And Column = 0 Then
            For i = 0 To 8
                If SixRoute Then
                    For j = 0 To 5
                        CreateZombie(Type, j, i)
                    Next
                Else
                    For j = 0 To 4
                        CreateZombie(Type, j, i)
                    Next
                End If
            Next
        ElseIf Row = 0 And Column > 0 Then
            If SixRoute Then
                For i = 0 To 5
                    CreateZombie(Type, i, Column - 1)
                Next
            Else
                For i = 0 To 4
                    CreateZombie(Type, i, Column - 1)
                Next
            End If
        ElseIf Row > 0 And Column = 0 Then
            For i = 0 To 8
                CreateZombie(Type, Row - 1, i)
            Next
        Else
            CreateZombie(Type, Row - 1, Column - 1)
        End If
    End Sub
    Public Shared Function GetJumpDestination(address As Integer) As Integer
        Dim b = Memory.ReadByte(address)
        If b = &HF Then
            Dim par = Memory.ReadInteger(address + 2)
            Return address + par + 6
        Else
            Dim par As SByte = Memory.ReadByte(address + 1)
            Return address + par + 2
        End If
    End Function
    Public Shared Function SetJumpDestination(address As Integer, des As Integer) As Boolean
        Dim b = Memory.ReadByte(address)
        If b = &HF Then
            Memory.WriteInteger(address + 2, des - address - 6)
        Else
            Dim v As Integer = des - address - 2
            If v > SByte.MaxValue OrElse v < SByte.MinValue Then Return False
            Memory.WriteByte(address + 1, v)
        End If
        Return True
    End Function
#End Region
End Class
