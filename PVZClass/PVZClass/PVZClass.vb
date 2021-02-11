Imports System.ComponentModel
Imports System.Drawing
Imports System.IO
Imports System.Text
Imports System.Text.RegularExpressions
''' <summary>
''' 此类不可实例化,想修改游戏必须先调用<see cref="PVZ.RunGame()"/>,所有设计时间/倒计时的数值单位均为<see langword="厘秒"/><para/>
''' 当前仍然是测试版本，有很多代码在实际应用中并没有作用，甚至影响效率，请酌情删改<para/>
'''将一些类的<see langword="BaseAddress"/>属性初始化在<see langword="New"/>过程里,并删除以下代码，可以避免每次调用都重新读取，提高效率<para/>
'''<see langword="Public Shared ReadOnly Property"/> BaseAddress <see langword="As Integer"/><para/>
'''<see langword="      Get"/><para/>
'''<see langword="              Return "/><see cref="Memory.GetAddress(Integer,Integer)"/><para/>
'''<see langword="      End Get"/><para/>
'''<see langword="End Property"/><para/>
'''如果你不需要运行游戏中某个这个功能，请重写<see cref="PVZ.RunGame()"/>方法并删除相关属性<para/>
'''请务必在程序退出时调用<see cref="PVZ.CloseGame(Boolean)"/>方法，以便处理遗留问题<para/>
'''想使用某些方法，请先调用<see cref="PVZ.InitFunctions()"/>方法<para/>
'''<see cref="Memory.CreateThreadNoWait(Integer)"/>方法效率高但可能不稳定，在功能上和<see cref="Memory.CreateThread(Integer)"/>完全一样<para/>
'''使用事件循环可以大幅提高效率(不会写的可以找我帮忙),并且直到游戏关闭或执行<see langword="Stop"/>方法时一直有效
''' </summary>
Public Class PVZ
#Region "声明"
    Private Declare Auto Function FindWindow Lib "user32" _
        (ByVal lpClassName As String,
         ByVal lpWindowName As String) _
        As Integer
    Private Declare Function GetWindowThreadProcessId Lib "user32" _
        (ByVal hwnd As Integer,
         ByRef lpdwProcessId As Integer) _
         As Integer
    Private Declare Function OpenProcess Lib "kernel32.dll" _
        (ByVal dwDesiredAccess As Integer,
        ByVal bInheritHandle As Integer,
        ByVal dwProcessId As Integer) _
        As Integer
    Private Declare Function CloseHandle Lib "kernel32.dll" _
        (ByVal hObject As Integer) _
        As Boolean
    Private Declare Function SendMessageA Lib "user32" _
        (ByVal hwnd As Integer,
         ByVal wMsg As Integer,
         ByVal wParam As Integer,
         lParam As Integer) _
         As Integer
    Private Declare Function SetForegroundWindow Lib "user32" (ByVal hwnd As Integer) As Integer
    Private Declare Function ReadProcessMemoryInteger Lib "kernel32.dll" Alias "ReadProcessMemory" _
        (ByVal hProcess As Integer,
         ByVal lpBaseAddress As Integer,
         ByRef lpBuffer As Integer,
         ByVal nSize As Integer,
         ByRef lpNumberOfBytesWritten As Integer) _
         As Boolean
    Private Declare Function ReadProcessMemoryQword Lib "kernel32.dll" Alias "ReadProcessMemory" _
        (ByVal hProcess As Integer,
         ByVal lpBaseAddress As Integer,
         ByRef lpBuffer As Long, ByVal nSize As Integer,
         ByRef lpNumberOfBytesWritten As Integer) _
         As Boolean
    Private Declare Function ReadProcessMemoryShort Lib "kernel32.dll" Alias "ReadProcessMemory" _
        (ByVal hProcess As Integer,
         ByVal lpBaseAddress As Integer,
         ByRef lpBuffer As Short,
         ByVal nSize As Integer,
         ByRef lpNumberOfBytesWritten As Integer) _
         As Boolean
    Private Declare Function ReadProcessMemoryFloat Lib "kernel32.dll" Alias "ReadProcessMemory" _
        (ByVal hProcess As Integer,
         ByVal lpBaseAddress As Integer,
         ByRef lpBuffer As Single,
         ByVal nSize As Integer,
         ByRef lpNumberOfBytesWritten As Integer) _
         As Boolean
    Private Declare Function ReadProcessMemoryDouble Lib "kernel32.dll" Alias "ReadProcessMemory" _
         (ByVal hProcess As Integer,
          ByVal lpBaseAddress As Integer,
          ByRef lpBuffer As Double,
          ByVal nSize As Integer,
          ByRef lpNumberOfBytesWritten As Integer) _
          As Boolean
    Private Declare Function ReadProcessMemoryByte Lib "kernel32.dll" Alias "ReadProcessMemory" _
        (ByVal hProcess As Integer,
         ByVal lpBaseAddress As Integer,
         ByRef lpBuffer As Byte,
         ByVal nSize As Integer,
         ByRef lpNumberOfBytesWritten As Integer) _
         As Boolean
    Private Declare Function ReadProcessMemoryBytes Lib "kernel32.dll" Alias "ReadProcessMemory" _
        (ByVal hProcess As Integer,
         ByVal lpBaseAddress As Integer,
         ByVal lpBuffer As Byte(),
         ByVal nSize As Integer,
         ByRef lpNumberOfBytesWritten As Integer) _
         As Boolean
    Private Declare Function WriteProcessMemoryInteger Lib "kernel32.dll" Alias "WriteProcessMemory" _
        (ByVal hProcess As Integer,
         ByVal lpBaseAddress As Integer,
         ByRef lpBuffer As Integer,
         ByVal nSize As Integer,
         ByRef lpNumberOfBytesWritten As Integer) _
         As Boolean
    Private Declare Function WriteProcessMemoryQword Lib "kernel32.dll" Alias "WriteProcessMemory" _
        (ByVal hProcess As Integer,
         ByVal lpBaseAddress As Integer,
         ByRef lpBuffer As Long,
         ByVal nSize As Integer,
         ByRef lpNumberOfBytesWritten As Integer) _
         As Boolean
    Private Declare Function WriteProcessMemoryShort Lib "kernel32.dll" Alias "WriteProcessMemory" _
        (ByVal hProcess As Integer,
         ByVal lpBaseAddress As Integer,
         ByRef lpBuffer As Short,
         ByVal nSize As Integer,
         ByRef lpNumberOfBytesWritten As Integer) _
         As Boolean
    Private Declare Function WriteProcessMemoryFloat Lib "kernel32.dll" Alias "WriteProcessMemory" _
        (ByVal hProcess As Integer,
         ByVal lpBaseAddress As Integer,
         ByRef lpBuffer As Single,
         ByVal nSize As Integer,
         ByRef lpNumberOfBytesWritten As Integer) _
         As Boolean
    Private Declare Function WriteProcessMemoryDouble Lib "kernel32.dll" Alias "WriteProcessMemory" _
        (ByVal hProcess As Integer,
         ByVal lpBaseAddress As Integer,
         ByRef lpBuffer As Double,
         ByVal nSize As Integer,
         ByRef lpNumberOfBytesWritten As Integer) _
         As Boolean
    Private Declare Function WriteProcessMemoryByte Lib "kernel32.dll" Alias "WriteProcessMemory" _
        (ByVal hProcess As Integer,
         ByVal lpBaseAddress As Integer,
         ByRef lpBuffer As Byte,
         ByVal nSize As Integer,
         ByRef lpNumberOfBytesWritten As Integer) _
         As Boolean
    Private Declare Function WriteProcessMemoryBytes Lib "kernel32.dll" Alias "WriteProcessMemory" _
        (ByVal hProcess As Integer,
         ByVal lpBaseAddress As Integer,
         ByVal lpBuffer As Byte(),
         ByVal nSize As Integer,
         ByRef lpNumberOfBytesWritten As Integer) _
         As Boolean
    ''' <summary>
    ''' 在指定地址创建线程,并返回线程句柄
    ''' </summary>
    ''' <returns>线程句柄</returns>
    Private Declare Function CreateRemoteThread Lib "kernel32.dll" _
        (ByVal hProcess As Integer,
         ByVal lpThreadAttributes As Integer,
         ByVal dwStackSize As Integer,
         ByVal lpStartAddress As Integer,
         ByVal lpParameter As Integer,
         ByVal dwCreationFlags As Integer,
         ByRef lpThreadId As Integer) _
         As Integer
    ''' <summary>
    ''' 申请内存页空间
    ''' </summary>
    ''' <returns>申请的内存页首地址</returns>
    Private Declare Function VirtualAllocEx Lib "kernel32.dll" _
        (ByVal hProcess As Integer,
         ByVal lpAddress As Integer,
         ByVal dwSize As Integer,
         ByVal flAllocationType As Integer,
         ByVal flProtect As Integer) _
         As Integer
    ''' <summary>
    ''' 等待线程的运行,并返回线程的运行状态
    ''' </summary>
    ''' <returns>线程运行状态</returns>
    Private Declare Function WaitForSingleObject Lib "kernel32.dll" _
        (ByVal hHandle As Integer,
         ByVal dwMilliseconds As Integer) _
         As Integer
    ''' <summary>
    ''' 释放内存页空间
    ''' </summary>
    Private Declare Function VirtualFreeEx Lib "kernel32.dll" _
        (ByVal hProcess As Integer,
         ByVal lpAddress As Integer,
         ByVal dwSize As Integer,
         ByVal flAllocationType As Integer) _
         As Integer
    ''' <summary>
    ''' 修改内存页的访问权限
    ''' </summary>
    Private Declare Function VirtualProtectEx Lib "kernel32.dll" _
        (ByVal hProcess As Integer,
         ByVal lpAddress As Integer,
         ByVal dwSize As Integer,
         ByVal flNewProtect As Integer,
         ByRef lpflOldProtect As Integer) _
         As Integer
#End Region
#Region "属性和字段"
    ''' <summary>
    ''' 获取游戏的进程对象
    ''' </summary>
    Public Shared Game As Process
    Private Shared hprocess As Integer
    Private Const PROCESS_ALL_ACCESS = &H1F0FFF
    ''' <summary>
    ''' 0-100存放高效创建子弹的函数<para/>
    ''' 100-200存放字符串或者PlantEffect的伪造植物对象<para/>
    ''' 200-300存放子弹的事件循环代码<para/>
    ''' 300-400存放僵尸的事件循环代码<para/>
    ''' 400-500存放植物的事件循环代码<para/>
    ''' 500-600存放各种需要初始化的功能
    ''' </summary>
    Public Shared Variable As Integer
    ''' <summary>
    ''' 游戏版本
    ''' </summary>
    Public Enum PVZVER
        V1_0_0_1051 = 1
        V1_2_0_1065 = 2
        V1_2_0_1073 = 4
        中文年度加强版 = 8
        粘度汗化版 = 16
        NotPVZ = 32
    End Enum
    Private Shared Funinited As Boolean = False
    Private Shared wcode As Integer
    Private Shared logArg As Integer
    Public Shared GamePath As String
    Public Shared Property WarningCode As Integer
        Get
            Return wcode
        End Get
        Set(value As Integer)
            If File.Exists("warning.log") AndAlso New FileInfo("warning.log").Length > 102400 Then
                File.Delete("warning.log")
            End If
            If logArg <> 0 Then
                File.AppendAllText("warning.log", Date.Now.ToString() + vbNewLine + LastWarning + " - " + logArg.ToString() + vbNewLine)
            Else
                File.AppendAllText("warning.log", Date.Now.ToString() + vbNewLine + LastWarning + vbNewLine)
            End If
            wcode = value
        End Set
    End Property
    ''' <summary>
    ''' 默认寻找游戏名称
    ''' </summary>
    Public Shared GameName As String = "PlantsVsZombies"
    ''' ''' <summary>
    ''' 备份寻找游戏名称
    ''' </summary>
    Public Shared BackUpGameName As String = "popcapgame1"
    ''' <summary>
    ''' 默认寻找游戏窗口
    ''' </summary>
    Public Shared GameTitle As String = "植物大战僵尸中文版"
    ''' <summary>
    ''' 获取最后的警告信息
    ''' </summary>
    Public Shared ReadOnly Property LastWarning As String
        Get
            Select Case WarningCode
                Case 0
                    Return "EveryThing Is All Right"
                Case 3
                    Return "GameName Is Wrong"
                Case 4
                    Return "Failed To Find The Game In The ProcessList"
                Case 5
                    Return "Game Version Is Not Compatible"
                Case 6
                    Return "ReadMemory Failed"
                Case 7
                    Return "WriteMemory Failed"
                Case 8
                    Return "InitFunctions() Not Used"
                Case 9
                    Return "Game Process Wrong Or Not Supposed"
                Case Else
                    Return "UnDefined Error"
            End Select
        End Get
    End Property
    ''' <summary>
    ''' 此类的版本
    ''' </summary>
    Public Shared ClassVer As String = "1.2.6.0"
    ''' <summary>
    ''' 获取游戏版本
    ''' </summary>
    Public Shared ReadOnly Property GameVer As PVZVER
        Get
            Dim Ver As Integer = Memory.ReadInteger(&H552013)
            Select Case Ver
                Case &HC35EDB74
                    Return PVZVER.V1_0_0_1051
                Case &H86831977
                    Return PVZVER.V1_2_0_1065
                Case &H3B000001
                    Return PVZVER.V1_2_0_1073
                Case &H878B0000
                    Return PVZVER.中文年度加强版
                Case &HA48F
                    Return PVZVER.粘度汗化版
                Case Else
                    Return PVZVER.NotPVZ
            End Select
        End Get
    End Property
    ''' <summary>
    ''' 获得该类支持的游戏版本,用LogicalInclude方法来判断
    ''' </summary>
    Public Shared ApplicableVer As PVZVER = PVZVER.V1_0_0_1051 Or PVZVER.V1_2_0_1065

    Public Shared Function CheckPeocess() As Boolean?
        If hprocess = 0 Then Return Nothing
        Dim ver = GameVer
        If WarningCode = 6 Then Return Nothing
        If ver = PVZVER.V1_0_0_1051 Then
            Return True
        End If
        If FileVersionInfo.GetVersionInfo(GamePath).FileVersion = "1.0.0.1051" Then
            Return True
        End If
        WarningCode = 9
        logArg = Game.Id
        Return False
    End Function
#Region "伪汇编标识符"
    '用于描述汇编指令的特定字节
    Private Const mov_eax As Byte = &HB8
    Private Const mov_ecx As Byte = &HB9
    Private Const mov_ebx As Byte = &HBB
    Private Const mov_esi As Byte = &HBE
    Private Const mov_edi As Byte = &HBF
    Private Const pushad As Byte = &H60
    Private Const popad As Byte = &H61
    Private Const push As Byte = &H6A
    Private Const pushdw As Byte = &H68
    Private Const call­ As Byte = &HE8
    Private Const jmp As Byte = &HEB
    Private Const jmpfar As Byte = &HE9
    Private Const ret As Byte = &HC3
    Private Const mov_eax_ptr As Byte = &HA1
    Private Const mov_ptr_addr_eax As Byte = &HA3
#End Region
#End Region
#Region "逻辑实现方法"
    Private Sub New()
    End Sub
    ''' <summary>
    ''' 如果不需要使用下列方法可以不调用<para/>
    ''' <see cref="CreateProjectile(ProjectiletType, Integer, Integer, Single, Single)"/><para/>
    ''' <see cref="CreatePortal(Integer, Integer, Integer, Integer, Integer, Integer, Integer, Integer)"/>
    ''' </summary>
    Public Shared Sub InitFunctions()
        If Not Game?.HasExited Then
            '文件头可访问
            Memory.SetExecuteReadWrite(&H400000)
            '创建子弹的函数
            Memory.WriteBytes(Variable + 16, {
                              push, 0,
                              push, 0,
                              push, 0,
                              pushdw, 0, 0, 0, 0,
                              pushdw, 0, 0, 0, 0,
                              mov_eax, 0, 0, 0, 0,
                              call­, 2, 0, 0, 0,
                              jmp, 6,
                              pushdw, &H20, &HD6, &H40, 0,
                              ret,
                              &HC7, &H40, &H58, 7, 0, 0, 0,
                              &HC7, &H40, &H74, 1, 0, 0, 0,
                              &HDB, 5, 0, 0, 0, 0,
                              &HD8, &H35, 0, 0, 0, 0,
                              &HD9, &H58, &H3C,
                              &HDB, 5, 0, 0, 0, 0,
                              &HD8, &H35, 0, 0, 0, 0,
                              &HD9, &H58, &H40,
                              mov_ptr_addr_eax, 0, 0, 0, 0,
                              &HC3
                              })
            Memory.WriteInteger(Variable + 66, Variable + 8)
            Memory.WriteInteger(Variable + 72, Variable + 4)
            Memory.WriteInteger(Variable + 81, Variable + 12)
            Memory.WriteInteger(Variable + 87, Variable + 4)
            Memory.WriteInteger(Variable + 95, Variable)
            '创建传送门的必要修改
            Memory.WriteBytes(&H42706C, {&HB9, 4, 0, 0, 0, &HE9})
            Memory.WriteInteger(&H427072, Variable + 500 - 4 - &H427072)
            Memory.WriteBytes(Variable + 500, {&H89, &H48, &H14, &HC7, &H40, &H1C, &H78, &H4B, 5, 0, &HE9})
            Memory.WriteInteger(Variable + 511, &H427076 - 4 - (Variable + 511))
            '我是僵尸函数
            PVZ.Memory.WriteByteArray(&H750000, &H8B, &H80, &H68, &H7, &H0, &H0, &H85, &HC0, &H75, &H2, &HC3,
                                      &HCC, &H8B, &H80, &H38, &H1, &H0, &H0, &H8B, &H40, &H28, &H83, &HF8,
                                      &H34, &H7F, &H4, &H31, &HC0, &HEB, &HEC, &H31, &HC0, &HB0, &H1, &HEB, &HE6, &HCC)
            Funinited = True
        End If
    End Sub
    Public Shared Function RunGame(pid As Integer) As Boolean
        Funinited = False
        Memory.FreeMemory(Variable)
        CloseHandle(hprocess)
        hprocess = 0
        Game = Process.GetProcessById(pid)
        hprocess = OpenProcess(PROCESS_ALL_ACCESS, False, pid)
        Variable = Memory.AllocMemory()
        AddHandler Game.Exited, AddressOf CloseGame
        GamePath = Game.MainModule.FileName
        If GameVer = PVZVER.NotPVZ Then
            Return False
        End If
        Return True
    End Function

    ''' <summary>
    ''' 注意这里正常是不使用<see langword="FindWindow"/>函数而是找进程名字,成功返回真,失败返回假
    ''' </summary>
    Public Shared Function RunGame() As Boolean
        CloseGame()
        For Each process As Process In Process.GetProcesses()
            If process.ProcessName = GameName Or process.ProcessName = BackUpGameName Then
                Game = process
                AddHandler Game.Exited, AddressOf CloseGame
                GamePath = Game.MainModule.FileName
                hprocess = OpenProcess(PROCESS_ALL_ACCESS, False, Game.Id)
                Variable = Memory.AllocMemory()
                If GameVer = PVZVER.NotPVZ Then
                    CloseGame()
                    Game.Kill()
                    Continue For
                End If
                Return True
            End If
        Next
        Dim hwnd = FindWindow(vbNullString, GameTitle)
        If hwnd Then
            Dim pid As Integer
            GetWindowThreadProcessId(hwnd, pid)
            hprocess = OpenProcess(PROCESS_ALL_ACCESS, False, pid)
            Game = Process.GetProcessById(pid)
            AddHandler Game.Exited, AddressOf CloseGame
            GamePath = Game.MainModule.FileName
            Variable = Memory.AllocMemory()
            Return True
        End If
        WarningCode = 4
        Return False
    End Function
    Private Shared Sub ReplaceBytes(ByRef Self As Byte(), ByVal Index As Integer, ByVal Other As Integer)
        Dim Temp As Byte() = BitConverter.GetBytes(Other)
        For i = Index To Index + Temp.Length - 1
            Self(i) = Temp(i - Index)
        Next
    End Sub
    Private Shared Sub ReplaceBytes(ByRef Self As Byte(), ByVal Index As Integer, ByVal Other As Single)
        Dim Temp As Byte() = BitConverter.GetBytes(Other)
        For i = Index To Index + Temp.Length - 1
            Self(i) = Temp(i - Index)
        Next
    End Sub
    ''' <summary>
    ''' 程序结束或者再次使用<see cref="RunGame()"/>方法之前一定要调用
    ''' </summary>
    ''' <param name="CancelEventLoop">如果你不希望结束事件循环可以设置False(但这意味着申请的内存讲不被释放)</param>
    Public Shared Sub CloseGame(ByVal CancelEventLoop As Boolean)
        If hprocess = 0 Then Return
        Memory.WriteBytes(&H42706C, {&H89, &H58, &H14, &HC7, &H40, &H1C, &H78, &H4B, 5, 0})
        If CancelEventLoop Then
            Memory.FreeMemory(Variable)
        End If
        '恢复传送门代码
        Memory.WriteBytes(&H42706C, {137, 88, 20, 199, 64, 28, 120, 75, 5, 0})
        CloseHandle(hprocess)
        hprocess = 0
        Funinited = False
    End Sub
    Public Shared Sub CloseGame()
        CloseGame(True)
    End Sub
    Private Shared Function IncludeChinese(ByVal Content As String) As Boolean
        For Each i In Content
            If Asc(i) < 0 Then
                Return True
            End If
        Next
        Return False
    End Function
    Private Shared Function MakeLong(wLow As Short, wHigh As Short) As Integer
        Return wHigh * &H10000 + wLow
    End Function
    ''' <summary>
    ''' 判断目标值是否包括指定的值
    ''' </summary>
    ''' <param name="Comparator">要判断的值</param>
    ''' <param name="Value">判断的值</param>
    Public Shared Function LogicalInclude(ByVal Comparator As Integer, ByVal Value As Integer) As Boolean
        Return (Comparator And Value) = Value
    End Function
    ''' <summary>
    ''' 将XY的坐标转换为RC的坐标
    ''' </summary>
    ''' <param name="XY">XY坐标</param>
    Public Shared Sub XYToRC(ByRef XY As Point)
        Dim temp = XY.Y
        XY.Y = (XY.X - 40) / 80
        If SixRoute Then
            XY.X = (temp - 80) / 85
        Else
            XY.X = (temp - 80) / 100
        End If
    End Sub
    ''' <summary>
    ''' 将XY的坐标转换为RC的坐标
    ''' </summary>
    Public Shared Sub XYToRC(ByRef X As Integer, ByRef Y As Integer)
        Dim temp = Y
        Y = (X - 40) / 80
        If SixRoute Then
            X = (temp - 80) / 85
        Else
            X = (temp - 80) / 100
        End If
    End Sub
    ''' <summary>
    ''' 将RC的坐标转换为XY的坐标
    ''' </summary>
    ''' <param name="RC">RC坐标</param>
    Public Shared Sub RCToXY(ByRef RC As Point)
        Dim temp = RC.X
        RC.X = RC.Y * 80 + 40
        If SixRoute Then
            RC.Y = temp * 85 + 80
        Else
            RC.Y = temp * 100 + 80
        End If
    End Sub
    ''' <summary>
    ''' 将RC的坐标转换为XY的坐标
    ''' </summary>
    Public Shared Sub RCToXY(ByRef X As Integer, ByRef Y As Integer)
        Dim temp = X
        X = Y * 80 + 40
        If SixRoute Then
            Y = temp * 85 + 80
        Else
            Y = temp * 100 + 80
        End If
    End Sub
#End Region
#Region "功能实现方法"
#Region "内存类"
    ''' <summary>
    ''' 提高对游戏的内存操作(读写内存,申请/释放内存,创建线程)
    ''' </summary>
    Public Class Memory
        Private Const MEM_COMMIT As Integer = 4096
        Private Const PAGE_EXECUTE_READWRITE As Integer = 4
        Private Const WAIT_TIMEOUT As Integer = &H102
        Private Const MEM_RELEASE As Integer = &H8000
        Private Const MEM_DECOMMIT As Integer = &H4000
        Private Sub New()
        End Sub
        Public Shared Property GlobalSymblos As Dictionary(Of String, Integer) = New Dictionary(Of String, Integer)()
#Region "设置访问"
        ''' <summary>
        ''' 将指定的内存页修改为可执行可写,成功返回真,失败返回假
        ''' </summary>
        ''' <param name="Address">要修改的内存页的首地址</param>
        Public Shared Function SetExecuteReadWrite(ByVal Address As Integer) As Boolean
            Return VirtualProtectEx(hprocess, Address, 1024, &H40, 2)
        End Function
#End Region
#Region "读内存整数型"
        Public Shared Function ReadInteger(ByVal address As Integer) As Integer
            Dim Buffer As Integer
            If Not ReadProcessMemoryInteger(hprocess, address, Buffer, 4, 0&) Then
                WarningCode = 6
                logArg = address
            End If
            Return Buffer
        End Function
#End Region
#Region "读内存长整数型"
        Public Shared Function ReadQword(ByVal address As Integer) As Long
            Dim Buffer As Long
            If Not ReadProcessMemoryQword(hprocess, address, Buffer, 8, 0&) Then
                WarningCode = 6
                logArg = address
            End If
            Return Buffer
        End Function
#End Region
#Region "读内存短整数型"
        Public Shared Function ReadShort(ByVal address As Integer) As Short
            Dim Buffer As Short
            If Not ReadProcessMemoryShort(hprocess, address, Buffer, 2, 0&) Then
                WarningCode = 6
                logArg = address
            End If
            Return Buffer
        End Function
#End Region
#Region "读内存浮点数"
        Public Shared Function ReadFloat(ByVal address As Integer) As Single
            Dim Buffer As Single
            If Not ReadProcessMemoryFloat(hprocess, address, Buffer, 4, 0&) Then
                WarningCode = 6
                logArg = address
            End If
            Return Buffer
        End Function
        Public Shared Function ReadSingle(ByVal address As Integer) As Single
            Return ReadFloat(address)
        End Function
#End Region
#Region "读内存双精度浮点数型"
        Public Shared Function ReadDouble(ByVal address As Integer) As Double
            Dim Buffer As Double
            If Not ReadProcessMemoryDouble(hprocess, address, Buffer, 8, 0&) Then
                WarningCode = 6
                logArg = address
            End If
            Return Buffer
        End Function
#End Region
#Region "读内存字节型"

        Public Shared Function CheckRead(ByVal address As Integer) As Boolean
            Dim Buffer As Byte
            Return ReadProcessMemoryByte(hprocess, address, Buffer, 1, 0&)
        End Function
        Public Shared Function ReadByte(ByVal address As Integer) As Byte
            Dim Buffer As Byte
            If Not ReadProcessMemoryByte(hprocess, address, Buffer, 1, 0&) Then
                WarningCode = 6
                logArg = address
            End If
            Return Buffer
        End Function
#End Region
#Region "读内存字节集"
        Public Shared Function ReadBytes(ByVal address As Integer, ByVal length As Integer) As Byte()
            Dim Buffer(length - 1) As Byte
            If Not ReadProcessMemoryBytes(hprocess, address, Buffer, length, 0&) Then
                WarningCode = 6
                logArg = address
            End If
            Return Buffer
        End Function
#End Region
#Region "读内存文本型"
        Public Shared Function ReadString(ByVal address As Integer, ByVal length As Integer) As String
            Dim re = Encoding.Default.GetString(ReadBytes(address, length))
            Return re.Substring(0, re.IndexOf(vbNullChar))
        End Function
        Public Shared Function ReadToNull(ByVal address As Integer) As Byte()
            Dim buffer As List(Of Byte) = New List(Of Byte)()
            Dim b = ReadByte(address)
            While b <> 0
                buffer.Add(b)
                address += 1
                b = ReadByte(address)
            End While
            Return buffer.ToArray()
        End Function
#End Region
#Region "写内存整数型"
        Public Shared Function WriteInteger(ByVal address As Integer, ByVal write As Integer) As Boolean
            If WriteProcessMemoryInteger(hprocess, address, write, 4, 0&) Then Return True
            WarningCode = 7
            logArg = address
            Return False
        End Function
#End Region
#Region "写内存长整数型"
        Public Shared Function WriteQword(ByVal address As Integer, ByVal write As Long) As Boolean
            If WriteProcessMemoryQword(hprocess, address, write, 8, 0&) Then Return True
            WarningCode = 7
            logArg = address
            Return False
        End Function
        Public Shared Function WriteLong(ByVal address As Integer, ByVal write As Long) As Boolean
            If WriteProcessMemoryQword(hprocess, address, write, 8, 0&) Then Return True
            WarningCode = 7
            logArg = address
            Return False
        End Function
#End Region
#Region "写内存短整数型"
        Public Shared Function WriteShort(ByVal address As Integer, ByVal write As Short) As Boolean
            If WriteProcessMemoryShort(hprocess, address, write, 2, 0&) Then Return True
            WarningCode = 7
            logArg = address
            Return False
        End Function
#End Region
#Region "写内存浮点数"
        Public Shared Function WriteFloat(ByVal address As Integer, ByVal write As Single) As Boolean
            If WriteProcessMemoryFloat(hprocess, address, write, 4, 0&) Then Return True
            WarningCode = 7
            logArg = address
            Return False
        End Function
        Public Shared Function WritSingle(ByVal address As Integer, ByVal write As Single) As Boolean
            Return WriteFloat(address, write)
        End Function
#End Region
#Region "写内存双精度浮点数型"
        Public Shared Function WriteDouble(ByVal address As Integer, ByVal write As Double) As Boolean
            If WriteProcessMemoryDouble(hprocess, address, write, 8, 0&) Then Return True
            WarningCode = 7
            logArg = address
            Return False
        End Function
#End Region
#Region "写内存字节型"
        Public Shared Function WriteByte(ByVal address As Integer, ByVal write As Byte) As Boolean
            If WriteProcessMemoryByte(hprocess, address, write, 1, 0&) Then Return True
            WarningCode = 7
            logArg = address
            Return False
        End Function
#End Region
#Region "写内存字节集"
        Public Shared Function WriteBytes(ByVal address As Integer, ByVal write As Byte()) As Boolean
            If WriteProcessMemoryBytes(hprocess, address, write, write.Count, 0&) Then Return True
            WarningCode = 7
            logArg = address
            Return False
        End Function
        Public Shared Function WriteBytesStrong(ByVal address As Integer, ByVal write As Byte(), count As Integer) As Boolean
            SetExecuteReadWrite(address)
            If WriteProcessMemoryBytes(hprocess, address, write, count, 0&) Then Return True
            WarningCode = 7
            logArg = address
            Return False
        End Function
        Public Shared Function WriteByteArray(ByVal address As Integer, ByVal ParamArray write As Byte()) As Boolean
            If WriteProcessMemoryBytes(hprocess, address, write, write.Count, 0&) Then Return True
            WarningCode = 7
            logArg = address
            Return False
        End Function
#End Region
#Region "写内存文本型"
        Public Shared Function WriteString(ByVal address As Integer, ByVal write As String) As Boolean
            write = Regex.Unescape(write) + vbNullChar
            Dim str As Byte()
            str = Encoding.Default.GetBytes(write)
            Return WriteBytes(address, str)
        End Function
#End Region
#Region "读指针"
        Public Shared Function GetAddress(ByVal BaseAddress As Integer, ByVal Offset As Integer) As Integer
            Return ReadInteger(ReadInteger(BaseAddress) + Offset)
        End Function
        Public Shared Function GetAddress(ByVal BaseAddress As Integer, ByVal Offset1 As Integer, ByVal Offset2 As Integer) As Integer
            Return ReadInteger(ReadInteger(ReadInteger(BaseAddress) + Offset1) + Offset2)
        End Function
        Public Shared Function GetAddress(ByVal BaseAddress As Integer, ByVal Offset1 As Integer, ByVal Offset2 As Integer, ByVal Offset3 As Integer) As Integer
            Return ReadInteger(ReadInteger(ReadInteger(ReadInteger(BaseAddress) + Offset1) + Offset2) + Offset3)
        End Function
        Public Shared Function GetAddress(ByVal BaseAddress As Integer, ByVal Offset1 As Integer, ByVal Offset2 As Integer, ByVal Offset3 As Integer, ByVal Offset4 As Integer) As Integer
            Return ReadInteger(ReadInteger(ReadInteger(ReadInteger(ReadInteger(BaseAddress) + Offset1) + Offset2) + Offset3) + Offset4)
        End Function
#End Region
#Region "申请内存"
        ''' <summary>
        ''' 申请内存页,并返回申请的内存页的首地址
        ''' </summary>
        Public Shared Function AllocMemory() As Integer
            Return VirtualAllocEx(hprocess, 0, 1024, MEM_COMMIT, PAGE_EXECUTE_READWRITE)
        End Function
        ''' <summary>
        ''' 申请全局内存页,并返回申请的内存页的首地址
        ''' </summary>
        Public Shared Function AllocMemoryGlobal(symbol As String) As Integer
            If Not GlobalSymblos.ContainsKey(symbol) Then
                GlobalSymblos(symbol) = AllocMemory()
            End If
            Return GlobalSymblos(symbol)
        End Function
#End Region
#Region "创建线程"
        ''' <summary>
        ''' 在指定地址创建线程并等待运行
        ''' </summary>
        ''' <param name="Address">要创建线程的地址</param>
        Public Shared Sub CreateThread(ByVal Address As Integer)
            Dim threadhwnd As Integer
            Dim ret As Integer
            threadhwnd = CreateRemoteThread(hprocess, 0, 0, Address, 0, 0, Game?.Id)
            Do
                ret = WaitForSingleObject(threadhwnd, 100)
            Loop While ret = WAIT_TIMEOUT
            CloseHandle(threadhwnd)
        End Sub
        ''' <summary>
        ''' 在指定地址创建线程且不等待运行(不稳定)
        ''' </summary>
        ''' <param name="Address">要创建线程的地址</param>
        Public Shared Sub CreateThreadNoWait(ByVal Address As Integer)
            CloseHandle(CreateRemoteThread(hprocess, 0, 0, Address, 0, 0, Game?.Id))
        End Sub
#End Region
#Region "释放内存"
        ''' <summary>
        ''' 释放指定内存页
        ''' </summary>
        ''' <param name="Address">要释放的内存页首地址</param>
        Public Shared Sub FreeMemory(ByVal Address As Integer)
            VirtualFreeEx(hprocess, Address, 1024, MEM_DECOMMIT)
            VirtualFreeEx(hprocess, Address, 0, MEM_RELEASE)
        End Sub
#End Region
#Region "直接运行"
        ''' <summary>
        ''' 执行汇编字节代码,并返回一个值(通常是目标对象的指针)
        ''' </summary>
        ''' <param name="AsmCodes">要运行的汇编字节代码</param>
        Public Shared Function Execute(ByVal AsmCodes As Byte()) As Integer
            If hprocess <> 0 Then
                Dim Address = AllocMemory()
                WriteBytes(Address, AsmCodes)
                WriteByte(&H552014, &HFE)
                CreateThread(Address)
                WriteByte(&H552014, &HDB)
                FreeMemory(Address)
                Return ReadInteger(Variable)
            End If
            Return 0
        End Function
#End Region
    End Class
#End Region
#Region "全局方法"
    ''' <summary>
    ''' 设置音量大小和音效大小
    ''' </summary>
    ''' <param name="Music">音量大小</param>
    ''' <param name="SoundFX">音效大小(默认0.65[最大值])</param>
    Public Shared Sub SetVolume(ByVal Music As Double, Optional SoundFX As Double = 0.65)
        Memory.WriteDouble(Memory.ReadInteger(&H6A9EC0) + &HD0, Music)
        Memory.WriteDouble(Memory.ReadInteger(&H6A9EC0) + &HD8, SoundFX)
        Dim Asmcode As Byte() = {
            push, 0,
            mov_ecx, 0, 0, 0, 0,
            call­, 2, 0, 0, 0,
            jmp, 6,
            pushdw, 0, &H4D, &H55, 0,
            ret,
            ret
        }
        ReplaceBytes(Asmcode, 3, Memory.ReadInteger(&H6A9EC0))
        Memory.Execute(Asmcode)
    End Sub
    ''' <summary>
    ''' 创建一个僵尸,并返回创建的僵尸对象
    ''' </summary>
    ''' <param name="Type">僵尸类型</param>
    ''' <param name="Row">僵尸所在行</param>
    ''' <param name="Column">僵尸所在列</param>
    Public Shared Function CreateZombie(ByVal Type As ZombieType, ByVal Row As Integer, ByVal Column As Byte) As Zombie
        If MainObjectExist Then
            Dim Asmcode As Byte() = {
                &H8B, &HD, 0, 0, 0, 0,
                mov_eax, 0, 0, 0, 0,
                push, Column,
                push, CByte(Type),
                call­, 2, 0, 0, 0,
                jmp, 6,
                pushdw, &HF0, &HA0, &H42, 0,
                ret,
                mov_ptr_addr_eax, 0, 0, 0, 0,
                ret
            }
            Memory.WriteBytes(&H42A209, {&H8B, &HC3, &H5B, &HC2, 8, 0})
            Memory.WriteShort(&H42A1E4, 9195)
            Memory.WriteShort(&H42A196, 29163)
            ReplaceBytes(Asmcode, 2, BaseAddress + &H160)
            ReplaceBytes(Asmcode, 7, Row)
            ReplaceBytes(Asmcode, 29, Variable)
            Return New Zombie(Memory.Execute(Asmcode))
        End If
        Return Nothing
    End Function
    ''' <summary>
    ''' 创建一个植物,并返回创建的植物对象
    ''' </summary>
    ''' <param name="Type">植物类型</param>
    ''' <param name="Row">植物所在行</param>
    ''' <param name="Column">植物所在列</param>
    ''' <param name="Imitative">是否为模仿者,默认为否</param>
    Public Shared Function CreatePlant(ByVal Type As PlantType, ByVal Row As Integer, ByVal Column As Byte, Optional Imitative As Boolean = False) As Plant
        If MainObjectExist Then
            Dim AsmCode As Byte() = {
                push, &HFF,
                push, CByte(Type),
                mov_eax, 0, 0, 0, 0,
                push, Column,
                pushdw, 0, 0, 0, 0,
                call­, 2, 0, 0, 0,
                jmp, 6,
                pushdw, &H20, &HD1, &H40, 0,
                ret,
                mov_ptr_addr_eax, 0, 0, 0, 0,
                ret
            }
            If Imitative Then
                AsmCode(1) = CByte(Type)
                AsmCode(3) = PlantType.Imitater
            End If
            ReplaceBytes(AsmCode, 5, Row)
            ReplaceBytes(AsmCode, 12, BaseAddress)
            ReplaceBytes(AsmCode, 30, Variable)
            Return New Plant(Memory.Execute(AsmCode))
        End If
        Return Nothing
    End Function
    ''' <summary>
    ''' 用常规方法创建一个子弹,并返回创建的子弹对象
    ''' </summary>
    ''' <param name="Type">子弹类型</param>
    ''' <param name="Row">子弹所在行</param>
    ''' <param name="X">子弹横坐标</param>
    Public Shared Function CreateProjectile(ByVal Type As ProjectiletType, ByVal Row As Byte, ByVal X As Integer) As Projectile
        If MainObjectExist Then
            Dim AsmCode As Byte() = {
                push, CByte(Type),
                push, Row,
                push, 0,
                pushdw, 0, 0, 0, 0,
                pushdw, 0, 0, 0, 0,
                mov_eax, 0, 0, 0, 0,
                call­, 2, 0, 0, 0,
                jmp, 6,
                pushdw, &H20, &HD6, &H40, 0,
                ret,
                &HC7, &H40, &H74, &HB, 0, 0, 0,
                mov_ptr_addr_eax, 0, 0, 0, 0,
                ret
            }
            Dim Y As Integer = 90 + Row * 100
            If SixRoute Then
                Y = 94 + 86 * Row
            End If
            ReplaceBytes(AsmCode, 7, Y)
            ReplaceBytes(AsmCode, 12, X)
            ReplaceBytes(AsmCode, 17, BaseAddress)
            ReplaceBytes(AsmCode, 42, Variable)
            If Type = ProjectiletType.FirePea Then
                AsmCode(1) = CInt(ProjectiletType.NormalPea)
                Dim re = New Projectile(Memory.Execute(AsmCode))
                re.OnFire()
                Return re
            End If
            Return New Projectile(Memory.Execute(AsmCode))
        End If
        Return Nothing
    End Function
    ''' <summary>
    ''' 用高效的方法创建一个子弹,并返回创建的子弹对象<para/>
    ''' 需要调用<see cref="PVZ.InitFunctions()"/>
    ''' </summary>
    ''' <param name="Type">子弹类型</param>
    ''' <param name="X">子弹横坐标</param>
    ''' <param name="Y">子弹纵坐标</param>
    ''' <param name="Angle">子弹的移动角度</param>
    ''' <param name="Speed">子弹的移动速度</param>
    Public Shared Function CreateProjectile(ByVal Type As ProjectiletType, ByVal X As Integer, ByVal Y As Integer, ByVal Angle As Single, ByVal Speed As Single) As Projectile
        If Funinited Then
            If MainObjectExist Then
                Angle = Angle / 180 * Math.PI
                Dim XSpeed As Integer = Int(Math.Sin(Angle) * Speed * 10000)
                Dim YSpeed As Integer = Int(Math.Cos(Angle) * -Speed * 10000)
                Memory.WriteFloat(Variable + 4, 10000)
                Memory.WriteInteger(Variable + 8, XSpeed)
                Memory.WriteInteger(Variable + 12, YSpeed)
                Memory.WriteByte(Variable + 17, Type)
                Dim xy As New Point(X, Y)
                XYToRC(xy)
                Memory.WriteByte(Variable + 19, xy.X)
                Memory.WriteInteger(Variable + 23, Y)
                Memory.WriteInteger(Variable + 28, X)
                Memory.WriteInteger(Variable + 33, BaseAddress)
                Memory.WriteByte(&H552014, &HFE)
                Memory.CreateThread(Variable + 16)
                Memory.WriteByte(&H552014, &HDB)
                Return New Projectile(Memory.ReadInteger(Variable))
            End If
        Else
            WarningCode = 8
        End If
        Return Nothing
    End Function
    ''' <summary>
    ''' 创建一个掉落物,并返回创建的掉落物对象
    ''' </summary>
    ''' <param name="Type">掉落物类型</param>
    ''' <param name="X">掉落物横坐标</param>
    ''' <param name="Y">掉落物纵坐标</param>
    ''' <param name="State">掉落物状态</param>
    Public Shared Function CreateCoin(ByVal Type As CoinType, ByVal X As Integer, ByVal Y As Integer, ByVal State As Coin.MotionType, Optional ByVal Card As CardType = CardType.Peashooter) As Coin
        If MainObjectExist Then
            Dim AsmCode As Byte() = {
                mov_ecx, 0, 0, 0, 0,
                push, CByte(State),
                push, CByte(Type),
                pushdw, 0, 0, 0, 0,
                pushdw, 0, 0, 0, 0,
                call­, 2, 0, 0, 0,
                jmp, 6,
                pushdw, &H10, &HCB, &H40, 0,
                ret,
                mov_ptr_addr_eax, 0, 0, 0, 0,
                ret
            }
            ReplaceBytes(AsmCode, 1, BaseAddress)
            ReplaceBytes(AsmCode, 10, X)
            ReplaceBytes(AsmCode, 15, Y)
            ReplaceBytes(AsmCode, 33, Variable)
            Dim coin = New Coin(Memory.Execute(AsmCode))
            If Type = CoinType.PlantCard Then
                coin.CardType = Card
            End If
            Return coin
        End If
        Return Nothing
    End Function
    ''' <summary>
    ''' 恢复小推车
    ''' </summary>
    Public Shared Sub ResumeLawnmover()
        If MainObjectExist Then
            Dim AsmCode As Byte() = {
                pushdw, 0, 0, 0, 0,
                call­, 2, 0, 0, 0,
                jmp, 6,
                pushdw, &H70, &HBC, &H40, 0,
                ret,
                ret
            }
            Memory.SetExecuteReadWrite(&H679BF8)
            Memory.WriteFloat(&H679BF8, -21)
            Memory.WriteShort(&H40BC98, 24811)
            Memory.WriteByte(&H40BD17, 1)
            ReplaceBytes(AsmCode, 1, BaseAddress)
            For Each mover In AllLawnmovers
                mover.Exist = False
            Next
            Memory.Execute(AsmCode)
            Memory.WriteShort(&H40BC98, 2421)
            Memory.WriteByte(&H40BD17, 0)
            Memory.WriteFloat(&H679BF8, -160)
        End If
    End Sub

    ''' <summary>
    ''' 触发小推车
    ''' </summary>
    Public Shared Sub StartLawnmover()
        Dim AsmCode As Byte() = {
            &H8B, &H3D, &HC0, &H9E, &H6A, &H0,
            &H8B, &HB7, &H68, &H7, &H0, &H0,
            &H85, &HF6,
            &H74, &H23,
            &H8B, &H9E, &H4, &H1, &H0, &H0,
            &H8B, &HB6, &H0, &H1, &H0, &H0,
            &H39, &H3E,
            &H75, &HD,
            &HE8, &H2, &H0, &H0, &H0,
            &HEB, &H6,
            &H68, &HA0, &H8D, &H45, &H0,
            &HC3,
            &H83, &HC6, &H48,
            &H4B,
            &H7F, &HE9,
            &HC3
        }
        Memory.Execute(AsmCode)
    End Sub

    ''' <summary>
    ''' 创建一个场地物品,并返回创建的场地物品对象<para/>
    ''' 注意这个对象是所有实际的场地物品的基类,具体参数需要手动设置
    ''' </summary>
    Public Shared Function CreateGriditem() As Griditem
        If MainObjectExist Then
            Dim AsmCode As Byte() = {
                mov_esi, 0, 0, 0, 0,
                call­, 2, 0, 0, 0,
                jmp, 6,
                pushdw, &HC0, &HE1, &H41, 0,
                ret,
                mov_ptr_addr_eax, 0, 0, 0, 0,
                ret
            }
            ReplaceBytes(AsmCode, 1, BaseAddress + &H11C)
            ReplaceBytes(AsmCode, 19, Variable)
            Return New Griditem(Memory.Execute(AsmCode))
        End If
        Return Nothing
    End Function
    ''' <summary>
    ''' 创建一个墓碑
    ''' </summary>
    ''' <param name="Row">墓碑所在行</param>
    ''' <param name="Column">墓碑所在列</param>
    Public Shared Sub CreateGrave(ByVal Row As Integer, ByVal Column As Integer)
        If MainObjectExist Then
            Dim AsmCode As Byte() = {
                &HFF, &H35, 0, 0, 0, 0,
                mov_edi, 0, 0, 0, 0,
                mov_ebx, 0, 0, 0, 0,
                call­, 2, 0, 0, 0,
                jmp, 6,
                pushdw, &H20, &H66, &H42, 0,
                ret,
                ret
            }
            ReplaceBytes(AsmCode, 2, BaseAddress + &H160)
            ReplaceBytes(AsmCode, 7, Row)
            ReplaceBytes(AsmCode, 12, Column)
            Memory.Execute(AsmCode)
        End If
    End Sub
    ''' <summary>
    ''' 创建一个弹坑,并返回创建的弹坑对象
    ''' </summary>
    ''' <param name="Row">弹坑所在行</param>
    ''' <param name="Column">弹坑所在列</param>
    ''' <param name="Timeout">弹坑消失时间</param>
    Public Shared Function CreateCrater(ByVal Row As Integer, ByVal Column As Integer, Optional ByVal Timeout As Integer = 18000) As Crater
        If MainObjectExist Then
            Dim Re = New Crater(CreateGriditem().BaseAddress)
            With Re
                .Row = Row
                .Column = Column
                .Layer = .Column * &H2710 + &H49ED0
                .DisappearCountdown = Timeout
                .Type = GriditemType.Crater
            End With
            Return Re
        End If
        Return Nothing
    End Function
    ''' <summary>
    ''' 创建一个梯子,并返回创建的梯子对象
    ''' </summary>
    ''' <param name="Row">梯子所在行</param>
    ''' <param name="Column">梯子所在列</param>
    Public Shared Function CreateLadder(ByVal Row As Integer, ByVal Column As Byte) As Griditem
        If MainObjectExist Then
            Dim AsmCode As Byte() = {
                mov_eax, 0, 0, 0, 0,
                mov_edi, 0, 0, 0, 0,
                push, Column,
                call­, 2, 0, 0, 0,
                jmp, 6,
                pushdw, &H40, &H8F, &H40, 0,
                ret,
                mov_ptr_addr_eax, 0, 0, 0, 0,
                ret
            }
            ReplaceBytes(AsmCode, 1, BaseAddress)
            ReplaceBytes(AsmCode, 6, Row)
            ReplaceBytes(AsmCode, 26, Variable)
            Return New Griditem(Memory.Execute(AsmCode))
        End If
        Return Nothing
    End Function
    ''' <summary>
    ''' 创建一个花瓶,并返回创建的花瓶对象,建议使用关键字参数的形式为参数赋值
    ''' </summary>
    ''' <param name="Row">花瓶所在行</param>
    ''' <param name="Column">花瓶所在列</param>
    ''' <param name="Content">花瓶内容类型</param>
    ''' <param name="Skin">花瓶皮肤,默认问号</param>
    ''' <param name="Zombie">僵尸花瓶内的僵尸类型</param>
    ''' <param name="Plant">植物花瓶内的植物类型</param>
    ''' <param name="Sun">阳光花瓶内的阳光类型</param>
    Public Shared Function CreateVase(ByVal Row As Integer, ByVal Column As Integer, ByVal Content As VaseContent, Optional ByVal Skin As VaseSkin = VaseSkin.Unknow, Optional Zombie As ZombieType = ZombieType.Zombie, Optional ByVal Plant As PlantType = PlantType.Peashooter, Optional Sun As Integer = 0) As Vase
        If MainObjectExist Then
            Dim Re = New Vase(CreateGriditem().BaseAddress)
            With Re
                .Row = Row
                .Column = Column
                .Layer = .Column * &H2710 + &H49ED0
                .Type = GriditemType.Vase
                .Skin = Skin
                .Zombie = Zombie
                .Plant = Plant
                .Content = Content
                .Sun = Sun
            End With
            Return Re
        End If
        Return Nothing
    End Function
    ''' <summary>
    ''' 创建一个钉耙
    ''' </summary>
    ''' <param name="Row">钉耙所在行</param>
    ''' <param name="Column">钉耙所在列</param>
    Public Shared Sub CreateRake(ByVal Row As Byte, ByVal Column As Byte)
        If MainObjectExist Then
            Dim AsmCode As Byte() = {
                push, Row,
                push, Column,
                pushdw, 0, 0, 0, 0,
                call­, 2, 0, 0, 0,
                jmp, 6,
                pushdw, &HC0, &HB9, &H40, 0,
                ret,
                &H83, &HC4, 8,
                ret
            }
            Memory.WriteByte(&H40B9E3, &H81)
            Memory.WriteShort(&H40B9E4, 166)
            Memory.WriteByte(&H40BB2B, 0)
            Memory.WriteInteger(&H40BB3B, -1878241909)
            Memory.WriteInteger(&H40BB41, -1877981813)
            ReplaceBytes(AsmCode, 5, (BaseAddress))
            Memory.Execute(AsmCode)
            Memory.WriteInteger(&H40BB41, 337921163)
            Memory.WriteInteger(&H40BB3B, 270814347)
            Memory.WriteByte(&H40BB2B, &HFF)
            Memory.WriteShort(&H40B9E4, 633)
            Memory.WriteByte(&H40B9E3, &H84)
        End If
    End Sub
    ''' <summary>
    ''' 创建字幕
    ''' </summary>
    ''' <param name="Content">字幕内容</param>
    ''' <param name="Style">字幕样式</param>
    ''' <param name="Timeout">字幕消失时间,默认500</param>
    Public Shared Sub CreateCaption(ByVal Content As String, ByVal Style As CaptionStyle, Optional ByVal Timeout As Integer = 500)
        If MainObjectExist Then
            Dim AsmCode As Byte() = {
                pushdw, 0, 0, 0, 0,
                &H8D, &H4C, &H24, &H30,
                call­, 2, 0, 0, 0,
                jmp, 6,
                pushdw, &H50, &H44, &H40, 0,
                ret,
                mov_esi, 0, 0, 0, 0,
                mov_ecx, 6, 0, 0, 0,
                &H8D, &H54, &H24, &H2C,
                call­, 2, 0, 0, 0,
                jmp, 6,
                pushdw, &H10, &H90, &H45, 0,
                ret,
                &HC7, &H86, &H88, 0, 0, 0, 0, 0, 0, 0,
                &HC7, &H86, &H8C, 0, 0, 0, 0, 0, 0, 0,
                ret
            }
            Memory.WriteString(Variable + 100, Content)
            ReplaceBytes(AsmCode, 1, (Variable + 100))
            ReplaceBytes(AsmCode, 23, Caption.BaseAddress)
            ReplaceBytes(AsmCode, 55, Timeout)
            ReplaceBytes(AsmCode, 65, Style)
            Memory.Execute(AsmCode)
        End If
    End Sub
    ''' <summary>
    ''' 创建红字字幕,注意必须是全英文字符
    ''' </summary>
    ''' <param name="Content">红字字幕内容</param>
    Public Shared Sub CreateImageCaption(ByVal Content As String)
        If MainObjectExist And Not IncludeChinese(Content) Then
            Dim AsmCode As Byte() = {
                mov_edi, 0, 0, 0, 0,
                pushdw, 0, 0, 0, 0,
                &H8D, &H4C, &H24, &H10,
                call­, 2, 0, 0, 0,
                jmp, 6,
                pushdw, &H50, &H44, &H40, 0,
                ret,
                mov_ecx, &HF, 0, 0, 0,
                &H8D, &H54, &H24, &HC,
                call­, 2, 0, 0, 0,
                jmp, 6,
                pushdw, &H10, &HCA, &H40, 0,
                ret,
                ret
            }
            Memory.WriteString(Variable + 100, Content)
            ReplaceBytes(AsmCode, 1, BaseAddress)
            ReplaceBytes(AsmCode, 6, Variable + 100)
            Memory.Execute(AsmCode)
        End If
    End Sub
    ''' <summary>
    ''' 创建一个植物爆炸效果,和植物类内部的爆炸效果一样,但此函数不创建对象<para/>
    ''' 你可以使用<see cref="RCToXY(ByRef Point)"/>将行列转换成坐标
    ''' </summary>
    ''' <param name="Type">爆炸类型</param>
    ''' <param name="X">爆炸的横坐标</param>
    ''' <param name="Y">爆炸的纵坐标</param>
    Public Shared Sub CreatePlantEffect(ByVal Type As PlantEffectType, ByVal X As Integer, ByVal Y As Integer)
        If MainObjectExist Then
            Memory.WriteInteger(Variable + 100, Memory.ReadInteger(&H6A9EC0))
            Memory.WriteInteger(Variable + 104, BaseAddress)
            Memory.WriteInteger(Variable + 100 + &H24, Type)
            Memory.WriteInteger(Variable + 108, X + 50)
            Memory.WriteInteger(Variable + 100 + &HC, Y + 50)
            Dim tpoint = New Point(X, Y)
            XYToRC(tpoint)
            Memory.WriteInteger(Variable + 100 + &H1C, tpoint.X)
            Memory.WriteInteger(Variable + 100 + &H28, tpoint.Y)
            Dim AsmCode As Byte() = {
                    pushdw, 0, 0, 0, 0,
                    call­, 2, 0, 0, 0,
                    jmp, 6,
                    pushdw, &HA0, &H66, &H46, 0,
                    ret,
                    ret
                }
            ReplaceBytes(AsmCode, 1, Variable + 100)
            Memory.Execute(AsmCode)
        End If
    End Sub
    ''' <summary>
    ''' 创建一个爆炸,注意只有爆炸效果,而没有爆炸的音效和特效<para/>
    ''' 你可以使用<see cref="CreateEffect(Integer, Single, Single)"/>和<see cref="CreateSound(Integer)"/>函数手动创建<para/>
    ''' 对于上下界限,它限制了爆炸的范围在[自身所在行-界限,自身所在行+界限]的范围内,樱桃是1,毁灭菇是3
    ''' </summary>
    ''' <param name="X">爆炸的横坐标</param>
    ''' <param name="Y">爆炸的纵坐标</param>
    ''' <param name="Radius">爆炸的半径</param>
    ''' <param name="Cinder">是否为灰烬伤害</param>
    ''' <param name="Bound">爆炸的上下界限,默认5(不限制)</param>
    ''' <param name="EnemyDamage">是否为敌方的伤害(只炸魅惑僵尸)</param>
    Public Shared Sub CreateExplosion(ByVal X As Integer, ByVal Y As Integer, ByVal Radius As Integer, Optional ByVal Cinder As Boolean = True, Optional ByVal Bound As Byte = 5， Optional ByVal EnemyDamage As Boolean = False)
        If MainObjectExist Then
            Dim Damage As Byte = &H7F
            If EnemyDamage Then
                Damage = &HFF
            End If
            Dim temp As New Point(X, Y)
            XYToRC(temp)
            Dim AsmCode As Byte() = {
                push, Damage,
                push, -Cinder,
                push, Bound,
                pushdw, 0, 0, 0, 0,
                pushdw, 0, 0, 0, 0,
                pushdw, 0, 0, 0, 0,
                push, temp.X,
                pushdw, 0, 0, 0, 0,
                call­, 2, 0, 0, 0,
                jmp, 6,
                pushdw, &HA0, &HD8, &H41, 0,
                ret,
                ret
            }
            ReplaceBytes(AsmCode, 7, Radius)
            ReplaceBytes(AsmCode, 12, Y)
            ReplaceBytes(AsmCode, 17, X)
            ReplaceBytes(AsmCode, 24, BaseAddress)
            Memory.Execute(AsmCode)
        End If
    End Sub
    ''' <summary>
    ''' 创建一个特效,注意某些特效不会自动消失
    ''' </summary>
    ''' <param name="ID">特效类型</param>
    ''' <param name="X">特效横坐标</param>
    ''' <param name="Y">特效纵坐标</param>
    Public Shared Sub CreateEffect(ByVal ID As Integer, ByVal X As Single, ByVal Y As Single)
        If MainObjectExist Then
            Dim AsmCode As Byte() = {
            mov_esi, 0, 0, 0, 0,
            pushdw, 0, 0, 0, 0,
            pushdw, &H80, &H1A, 6, 0,
            pushdw, 0, 0, 0, 0,
            pushdw, 0, 0, 0, 0,
            call­, 2, 0, 0, 0,
            jmp, 6,
            pushdw, &H70, &H8A, &H51, 0,
            ret,
            ret
        }
            ReplaceBytes(AsmCode, 1, Memory.GetAddress(&H6A9EC0, &H820, 0))
            ReplaceBytes(AsmCode, 6, ID)
            ReplaceBytes(AsmCode, 16, Y)
            ReplaceBytes(AsmCode, 21, X)
            Memory.Execute(AsmCode)
        End If
    End Sub
    ''' <summary>
    ''' 创建一个音效,注意部分音效并不会自动停止,请使用<see cref="StopSound(Integer)"/>方法手动停止
    ''' </summary>
    ''' <param name="ID">音效类型</param>
    Public Shared Sub CreateSound(ByVal ID As Integer)
        Dim AsmCode As Byte() = {
            mov_eax, 0, 0, 0, 0,
            mov_ecx, 0, 0, 0, 0,
            push, 0,
            call­, 2, 0, 0, 0,
            jmp, 6,
            pushdw, &H20, &H50, &H51, 0,
            ret,
            ret
        }
        ReplaceBytes(AsmCode, 1, ID)
        ReplaceBytes(AsmCode, 6, Memory.GetAddress(&H6A9EC0, &H784))
        Memory.Execute(AsmCode)
    End Sub
    ''' <summary>
    ''' 冻结全屏,注意你必须至少种下过一个植物(不要求当前存在)
    ''' </summary>
    Public Shared Sub FrozeAll()
        If MainObjectExist And Memory.ReadInteger(BaseAddress + &HB0) > 0 Then
            Dim AsmCode As Byte() = {
            mov_edi, 0, 0, 0, 0,
            call­, 2, 0, 0, 0,
            jmp, 6,
            pushdw, &H20, &H64, &H46, 0,
            ret,
            ret
        }
            ReplaceBytes(AsmCode, 1, Memory.ReadInteger(BaseAddress + &HAC))
            Memory.Execute(AsmCode)
        End If
    End Sub
    ''' <summary>
    ''' 停止一个音效
    ''' </summary>
    ''' <param name="ID">音效类型</param>
    Public Shared Sub StopSound(ByVal ID As Integer)
        Dim AsmCode As Byte() = {
            mov_edi, 0, 0, 0, 0,
            mov_eax, 0, 0, 0, 0,
            call­, 2, 0, 0, 0,
            jmp, 6,
            pushdw, &H90, &H52, &H51, 0,
            ret,
            ret
        }
        ReplaceBytes(AsmCode, 1, Memory.GetAddress(&H6A9EC0, &H784))
        ReplaceBytes(AsmCode, 6, ID)
        Memory.Execute(AsmCode)
    End Sub
    ''' <summary>
    ''' 创建IZ阵型
    ''' </summary>
    ''' <param name="IZlevel">需要创建的关卡</param>
    Public Shared Sub CreateIZombieFormation(ByVal IZlevel As Level)
        If MainObjectExist And IZlevel >= Level.IZombie And IZlevel <= Level.IZombieEndless Then
            Dim temp = LevelId
            LevelId = IZlevel
            Dim AsmCode As Byte() = {
                pushdw, 0, 0, 0, 0,
                call­, 2, 0, 0, 0,
                jmp, 6,
                pushdw, &H90, &HA8, &H42, 0,
                ret,
                ret
            }
            ReplaceBytes(AsmCode, 1, Memory.ReadInteger(BaseAddress + &H160))
            Memory.Execute(AsmCode)
            LevelId = temp
        End If
    End Sub
    ''' <summary>
    ''' 创建VB阵型,不包括4-5的阵型
    ''' </summary>
    ''' <param name="VBlevel">需要创建的关卡</param>
    Public Shared Sub CreateVaseFormation(ByVal VBlevel As Level)
        If MainObjectExist And VBlevel >= Level.Vasebreaker And VBlevel <= Level.VasebreakerEndless Then
            Dim temp = LevelId
            LevelId = VBlevel
            Dim AsmCode As Byte() = {
                mov_esi, 0, 0, 0, 0,
                call­, 2, 0, 0, 0,
                jmp, 6,
                pushdw, &HF0, &H86, &H42, 0,
                ret,
                ret
            }
            ReplaceBytes(AsmCode, 1, Memory.ReadInteger(BaseAddress + &H160))
            Memory.Execute(AsmCode)
            LevelId = temp
        End If
    End Sub
    Private Shared Sub _CreatePortal()
        If MainObjectExist Then
            For Each por In AllGriditems
                If por.Type = GriditemType.PortalBlue Or por.Type = GriditemType.PortalYellow Then
                    por.Exist = False
                End If
            Next
            Dim AsmCode As Byte() = {
                mov_edi, 0, 0, 0, 0,
                call­, 2, 0, 0, 0,
                jmp, 6,
                pushdw, &HC0, &H6F, &H42, 0,
                ret,
                ret
            }
            ReplaceBytes(AsmCode, 1, Memory.ReadInteger(BaseAddress + &H160))
            Memory.Execute(AsmCode)
        End If
    End Sub
    ''' <summary>
    ''' 创建传送门
    ''' </summary>
    ''' <param name="Yellow1Row">第1个黄色传送门所在行</param>
    ''' <param name="Yellow1Column">第1个黄色传送门所在列</param>
    ''' <param name="Yellow2Row">第2个黄色传送门所在行</param>
    ''' <param name="Yellow2Column">第2个黄色传送门所在列</param>
    ''' <param name="Blue1Row">第1个蓝色传送门所在行</param>
    ''' <param name="Blue1Column">第1个蓝色传送门所在列</param>
    ''' <param name="Blue2Row">第2个蓝色传送门所在行</param>
    ''' <param name="Blue2Column">第2个蓝色传送门所在列</param>
    Public Shared Sub CreatePortal(ByVal Yellow1Row As Integer, ByVal Yellow1Column As Integer, ByVal Yellow2Row As Integer, ByVal Yellow2Column As Integer, ByVal Blue1Row As Integer, ByVal Blue1Column As Integer, ByVal Blue2Row As Integer, ByVal Blue2Column As Integer)
        If Funinited Then
            Memory.WriteInteger(&H426FE9, Yellow1Row)
            Memory.WriteInteger(&H426FE2, Yellow1Column)
            Memory.WriteInteger(&H427014, Yellow2Row)
            Memory.WriteInteger(&H42700D, Yellow2Column)
            Memory.WriteInteger(&H427044, Blue1Row)
            Memory.WriteInteger(&H42703D, Blue1Column)
            Memory.WriteInteger(&H42706D, Blue2Row)
            Memory.WriteInteger(&H427068, Blue2Column)
            _CreatePortal()
            Memory.WriteInteger(&H426FE9, 0)
            Memory.WriteInteger(&H426FE2, 2)
            Memory.WriteInteger(&H427014, 1)
            Memory.WriteInteger(&H42700D, 9)
            Memory.WriteInteger(&H427044, 3)
            Memory.WriteInteger(&H42703D, 9)
            Memory.WriteInteger(&H42706D, 4)
            Memory.WriteInteger(&H427068, 2)
        Else
            WarningCode = 8
        End If
    End Sub
    Public Shared Sub ClearZombiePreview()
        If MainObjectExist Then
            Dim AsmCode As Byte() = {
            mov_ebx, 0, 0, 0, 0,
            call­, 2, 0, 0, 0,
            jmp, 6,
            pushdw, &H70, &HDF, &H40, 0,
            ret,
            ret
        }
            ReplaceBytes(AsmCode, 1, BaseAddress)
            Memory.Execute(AsmCode)
        End If
    End Sub
    ''' <summary>
    ''' 生成出怪列表
    ''' </summary>
    Public Shared Sub CallZombieList()
        If MainObjectExist Then
            Dim AsmCode As Byte() = {
            mov_edi, 0, 0, 0, 0,
            call­, 2, 0, 0, 0,
            jmp, 6,
            pushdw, &HE0, &H92, &H40, 0,
            ret,
            ret
        }
            ReplaceBytes(AsmCode, 1, BaseAddress)
            Memory.Execute(AsmCode)
        End If
    End Sub
    ''' <summary>
    ''' 显示出怪预览
    ''' </summary>
    Public Shared Sub ShowZombiePreview()
        If MainObjectExist Then
            Dim AsmCode As Byte() = {
            pushdw, 0, 0, 0, 0,
            call­, 2, 0, 0, 0,
            jmp, 6,
            pushdw, &H40, &HA1, &H43, 0,
            ret,
            ret
        }
            ReplaceBytes(AsmCode, 1, Memory.ReadInteger(BaseAddress + &H15C))
            PVZ.Memory.WriteByte(&H43A153, 128)
            Memory.Execute(AsmCode)
            PVZ.Memory.WriteByte(&H43A153, 133)
        End If
    End Sub
    ''' <summary>
    ''' 创建出怪列表(设置出怪)
    ''' </summary>
    ''' <param name="ZombieSeed">出怪种子</param>
    ''' <param name="Wave">需要设置的波数,默认为0(不干预)</param>
    Public Shared Sub CreateZombieList(ByVal ZombieSeed As ZombieType(), Optional Wave As Integer = 0)
        If Wave > 0 Then
            Memory.WriteInteger(&H4092FD, Wave)
            Memory.WriteInteger(&H4093F2, Wave)
            Memory.WriteInteger(&H409466, Wave)
            Memory.WriteInteger(&H409472, Wave)
            Memory.WriteInteger(&H40947C, Wave)
            Memory.WriteInteger(&H40948A, Wave)
            Memory.WriteInteger(&H409499, Wave)
        End If
        For i = 0 To 32
            Memory.WriteByte(BaseAddress + &H54D4 + i, 0)
        Next
        For Each i In ZombieSeed
            Memory.WriteByte(BaseAddress + &H54D4 + i, 1)
        Next
        CallZombieList()
        ClearZombiePreview()
        ShowZombiePreview()
        Memory.WriteInteger(&H4092FD, 20)
        Memory.WriteInteger(&H4093F2, 12)
        Memory.WriteInteger(&H409466, 40)
        Memory.WriteInteger(&H409472, 30)
        Memory.WriteInteger(&H40947C, 0)
        Memory.WriteInteger(&H40948A, 10)
        Memory.WriteInteger(&H409499, 10)
    End Sub
#End Region
#Region "全局属性"
    ''' <summary>
    ''' 游戏音乐音量
    ''' </summary>
    Public Shared ReadOnly Property MusicVolume As Double
        Get
            Return Memory.ReadDouble(Memory.ReadInteger(&H6A9EC0) + &HD0)
        End Get
    End Property
    ''' <summary>
    ''' 游戏音效音量
    ''' </summary>
    Public Shared ReadOnly Property SoundFXVolume As Double
        Get
            Return Memory.ReadDouble(Memory.ReadInteger(&H6A9EC0) + &HD8)
        End Get
    End Property
    ''' <summary>
    ''' 获取全部僵尸对象,返回一个僵尸对象的数组
    ''' </summary>
    Public Shared ReadOnly Property AllZombies As Zombie()
        Get
            Dim MaxIndex = Memory.ReadInteger(BaseAddress + &H94) - 1
            Dim re(MaxIndex) As Zombie
            Dim Num As Integer = 0
            For i = 0 To MaxIndex
                If Memory.GetAddress(BaseAddress + &H90, &HEC + &H15C * i) = 0 Then
                    re(Num) = New Zombie(i)
                    Num += 1
                End If
            Next
            ReDim Preserve re(Num - 1)
            Return re
        End Get
    End Property
    ''' <summary>
    ''' 当前僵尸数
    ''' </summary>
    Public Shared ReadOnly Property ZombiesCount As Integer
        Get
            Return Memory.ReadInteger(BaseAddress + &HA0)
        End Get
    End Property
    ''' <summary>
    ''' 获取全部植物对象,返回一个植物对象的数组
    ''' </summary>
    Public Shared ReadOnly Property AllPlants As Plant()
        Get
            Dim MaxIndex = Memory.ReadInteger(BaseAddress + &HB0) - 1
            Dim re(MaxIndex) As Plant
            Dim Num As Integer = 0
            For i = 0 To MaxIndex
                If Memory.ReadByte(Memory.ReadInteger(BaseAddress + &HAC) + &H141 + &H14C * i) = 0 Then
                    re(Num) = New Plant(i)
                    Num += 1
                End If
            Next
            ReDim Preserve re(Num - 1)
            Return re
        End Get
    End Property
    ''' <summary>
    ''' 当前植物数
    ''' </summary>
    Public Shared ReadOnly Property PlantsCount As Integer
        Get
            Return Memory.ReadInteger(BaseAddress + &HBC)
        End Get
    End Property
    ''' <summary>
    ''' 获取全部子弹对象,返回一个子弹对象的数组
    ''' </summary>
    Public Shared ReadOnly Property AllProjectiles As Projectile()
        Get
            Dim MaxIndex = Memory.ReadInteger(BaseAddress + &HCC) - 1
            Dim re(MaxIndex) As Projectile
            Dim Num As Integer = 0
            For i = 0 To MaxIndex
                If Memory.GetAddress(BaseAddress + &HC8, &H50 + &H94 * i) = 0 Then
                    re(Num) = New Projectile(i)
                    Num += 1
                End If
            Next
            ReDim Preserve re(Num - 1)
            Return re
        End Get
    End Property
    ''' <summary>
    ''' 当前子弹数
    ''' </summary>
    Public Shared ReadOnly Property ProjectilesCount As Integer
        Get
            Return Memory.ReadInteger(BaseAddress + &HD8)
        End Get
    End Property
    ''' <summary>
    ''' 获取全部掉落物对象,返回一个掉落物对象的数组
    ''' </summary>
    Public Shared ReadOnly Property AllCoins As Coin()
        Get
            Dim MaxIndex = Memory.ReadInteger(BaseAddress + &HE8) - 1
            Dim re(MaxIndex) As Coin
            Dim Num As Integer = 0
            For i = 0 To MaxIndex
                If Memory.GetAddress(BaseAddress + &HE4, &H38 + &HD8 * i) = 0 Then
                    re(Num) = New Coin(i)
                    Num += 1
                End If
            Next
            ReDim Preserve re(Num - 1)
            Return re
        End Get
    End Property
    ''' <summary>
    ''' 当前掉落物数
    ''' </summary>
    Public Shared ReadOnly Property CoinsCount As Integer
        Get
            Return Memory.ReadInteger(BaseAddress + &HF4)
        End Get
    End Property
    ''' <summary>
    ''' 获取全部小推车对象,返回一个小推车对象的数组
    ''' </summary>
    Public Shared ReadOnly Property AllLawnmovers As Lawnmover()
        Get
            Dim MaxIndex = Memory.ReadInteger(BaseAddress + &H104) - 1
            Dim re(MaxIndex) As Lawnmover
            Dim Num As Integer = 0
            For i = 0 To MaxIndex
                If Memory.ReadByte(Memory.ReadInteger(BaseAddress + &H100) + &H30 + &H48 * i) = 0 Then
                    re(Num) = New Lawnmover(i)
                    Num += 1
                End If
            Next
            ReDim Preserve re(Num - 1)
            Return re
        End Get
    End Property
    ''' <summary>
    ''' 当前小推车数
    ''' </summary>
    Public Shared ReadOnly Property LawnmoversCount As Integer
        Get
            Return Memory.ReadInteger(BaseAddress + &H110)
        End Get
    End Property
    ''' <summary>
    ''' 获取全部场地物品对象,返回一个场地物品对象的数组
    ''' </summary>
    Public Shared ReadOnly Property AllGriditems As Griditem()
        Get
            Dim MaxIndex = Memory.ReadInteger(BaseAddress + &H120) - 1
            Dim re(MaxIndex) As Griditem
            Dim Num As Integer = 0
            For i = 0 To MaxIndex
                If Memory.ReadByte(Memory.ReadInteger(BaseAddress + &H11C) + &H20 + &HEC * i) = 0 Then
                    re(Num) = New Griditem(i)
                    Num += 1
                End If
            Next
            ReDim Preserve re(Num - 1)
            Return re
        End Get
    End Property
    ''' <summary>
    ''' 当前场地物品数
    ''' </summary>
    Public Shared ReadOnly Property GriditemsCount As Integer
        Get
            Return Memory.ReadInteger(BaseAddress + &H12C)
        End Get
    End Property
    ''' <summary>
    ''' 关卡枚举
    ''' </summary>
    Public Enum Level
        Adventure
        SurvivalDay
        SurvivalNight
        SurvivalPool
        SurvivalFog
        SurvivalRoof
        SurvivalDayHard
        SurvivalNightHard
        SurvivalPoolHard
        SurvivalFogHard
        SurvivalRoofHard
        SurvivalDayEndless
        SurvivalNightEndless
        SurvivalPoolEndless
        SurvivalFogEndless
        SurvivalRoofEndless
        ZomBotany
        WallnutBowling
        SlotMachine
        ItˆsRainingSeeds
        Beghouled
        Invisighoul
        SeeingStars
        Zombiguarium
        BeghouledTwist
        BigTroubleLittleZombie
        PortalCombat
        ColumnLikeYouSeeˆEm
        BobsiedBonanza
        ZombieNimbleZombieQuick
        WharkaZombie
        LastStand
        Zombotany2
        WallnutBowling2
        PogoParty
        DrZombossˆsRevenge
        ArtChallengeWallnut
        SunnyDay
        Unsodded
        BigTime
        ArtChallengeSunflower
        AirRaid
        IceLevel
        ZenGarden
        HighGravity
        GraveDanger
        CanYouDigIt
        DarkStormyNight
        BungeeBlitz
        Squirrel
        TreeofWisdom
        Vasebreaker
        TotheLeft
        ThirdVase
        ChainRection
        MisforMetal
        ScaryPotter
        HokeyPokey
        AnotherChainRection
        AceofVace
        VasebreakerEndless
        IZombie
        IZombieToo
        CanYouDigIt­
        TotallyNuts
        DeadZeppelin
        MeSmash
        ZomBoogie
        ThreeHitWonder
        Allyourbrainzrbelongtous
        IZombieEndless
        Upsell
        Intro
    End Enum
    ''' <summary>
    ''' 设置或获取当前关卡
    ''' </summary>
    Public Shared Property LevelId As Level
        Get
            Return Memory.GetAddress(&H6A9EC0, &H7F8)
        End Get
        Set(Value As Level)
            Memory.WriteInteger(Memory.ReadInteger(&H6A9EC0) + &H7F8, Value)
        End Set
    End Property
    ''' <summary>
    ''' 游戏状态(关卡状态)枚举
    ''' </summary>
    Public Enum GameState
        <Description("载入中")> Loading
        <Description("主菜单")> MainMenu
        <Description("准备开始")> Prepare
        <Description("游戏进行")> Playing
        <Description("失败")> Losing
        <Description("胜利")> Prize
        <Description("播放MV")> MVPlaying
        <Description("选择关卡")> SelectingLevel
    End Enum
    ''' <summary>
    ''' 设置或获取当前关卡状态,注意大部分时候这并不有效
    ''' </summary>
    Public Shared Property LevelState As GameState
        Get
            Return Memory.GetAddress(&H6A9EC0, &H7FC)
        End Get
        Set(Value As GameState)
            Memory.WriteInteger(Memory.ReadInteger(&H6A9EC0) + &H7FC, Value)
        End Set
    End Property
    ''' <summary>
    ''' 开启或关闭游戏自带的免费种植
    ''' </summary>
    Public Shared Property FreePlantingCheat As Boolean
        Get
            Return Memory.GetAddress(&H6A9EC0, &H814)
        End Get
        Set(Value As Boolean)
            Memory.WriteInteger(Memory.ReadInteger(&H6A9EC0) + &H814, Convert.ToInt32(Value))
        End Set
    End Property
    ''' <summary>
    ''' 设置或获取游戏是否为完全版本
    ''' </summary>
    Public Shared Property FullVersion As Boolean
        Get
            Return Memory.GetAddress(&H6A9EC0, &H8C0)
        End Get
        Set(Value As Boolean)
            Memory.WriteInteger(Memory.ReadInteger(&H6A9EC0) + &H8C0, Convert.ToInt32(Value))
        End Set
    End Property
#End Region
#Region "MainObject对象"
    ''' <summary>
    ''' 鼠标点击状态枚举
    ''' </summary>
    Public Enum MouseClickState
        None
        LButton
        RButton
        LRButton
        MidButton
        LMidButton
        RMidButton
        LRMidButton
    End Enum
    ''' <summary>
    ''' 鼠标对象(控制层面的鼠标)
    ''' </summary>
    Public Class Mouse
        Private Sub New()
        End Sub
        ''' <summary>
        ''' 对象指针
        ''' </summary>
        Public Shared ReadOnly Property BaseAddress As Integer
            Get
                Return Memory.GetAddress(&H6A9EC0, &H320)
            End Get
        End Property
        ''' <summary>
        ''' 鼠标是否在游戏画面内
        ''' </summary>
        Public Shared ReadOnly Property InGameArea As Boolean
            Get
                Return Memory.ReadInteger(BaseAddress + &HDC)
            End Get
        End Property
        ''' <summary>
        ''' 鼠标横坐标
        ''' </summary>
        Public Shared ReadOnly Property X As Integer
            Get
                Return Memory.ReadInteger(BaseAddress + &HE0)
            End Get
        End Property
        ''' <summary>
        ''' 鼠标纵坐标
        ''' </summary>
        Public Shared ReadOnly Property Y As Integer
            Get
                Return Memory.ReadInteger(BaseAddress + &HE4)
            End Get
        End Property
        ''' <summary>
        ''' 鼠标点击状态
        ''' </summary>
        Public Shared ReadOnly Property ClickState As MouseClickState
            Get
                Return Memory.ReadInteger(BaseAddress + &HE8)
            End Get
        End Property
        ''' <summary>
        ''' 通过发送消息左键点击一个坐标
        ''' </summary>
        ''' <param name="X">要点击的横坐标</param>
        ''' <param name="Y">要点击的纵坐标</param>
        Public Shared Sub WMLClick(ByVal X As Short, ByVal Y As Short)
            SetForegroundWindow(Game?.MainWindowHandle)
            SendMessageA(Game?.MainWindowHandle, &H201, 0, MakeLong(X, Y))
            SendMessageA(Game?.MainWindowHandle, &H202, 0, MakeLong(X, Y))
        End Sub
        ''' <summary>
        ''' 通过发送消息右键点击一个坐标
        ''' </summary>
        ''' <param name="X">要点击的横坐标</param>
        ''' <param name="Y">要点击的纵坐标</param>
        Public Shared Sub WMRClick(ByVal X As Short, ByVal Y As Short)
            SetForegroundWindow(Game?.MainWindowHandle)
            SendMessageA(Game?.MainWindowHandle, 516, 0, MakeLong(X, Y))
            SendMessageA(Game?.MainWindowHandle, 517, 0, MakeLong(X, Y))
        End Sub
        ''' <summary>
        ''' 点击一个坐标
        ''' </summary>
        ''' <param name="X">要点击的横坐标</param>
        ''' <param name="Y">要点击的纵坐标</param>
        Public Shared Sub GameClick(ByVal X As Integer, ByVal Y As Integer)
            Dim AsmCode As Byte() = {
                mov_ecx, 0, 0, 0, 0,
                mov_eax, 0, 0, 0, 0,
                mov_ebx, 0, 0, 0, 1,
                pushdw, 0, 0, 0, 0,
                call­, 2, 0, 0, 0,
                jmp, 6,
                pushdw, &H90, &H93, &H53, 0,
                ret,
                ret
            }
            ReplaceBytes(AsmCode, 1, BaseAddress)
            ReplaceBytes(AsmCode, 6, Y)
            ReplaceBytes(AsmCode, 16, X)
            Memory.Execute(AsmCode)
        End Sub
    End Class
    ''' <summary>
    ''' 僵尸类型枚举
    ''' </summary>
    Public Enum ZombieType
        <Description("普通僵尸")> Zombie
        <Description("旗帜僵尸")> FlagZombie
        <Description("路障僵尸")> ConeheadZombie
        <Description("撑杆跳僵尸")> PoleVaultingZombie
        <Description("铁桶僵尸")> BucketheadZombie
        <Description("读报僵尸")> NewspaperZombie
        <Description("铁栅门僵尸")> ScreenDoorZombie
        <Description("橄榄球僵尸")> FootballZombie
        <Description("舞王僵尸")> DancingZombie
        <Description("伴舞僵尸")> BackupDancer
        <Description("救生圈僵尸")> DuckyTubeZombie
        <Description("潜水僵尸")> SnorkedZombie
        <Description("冰车僵尸")> Zomboin
        <Description("雪橇僵尸小队")> ZombieBobsledTeam
        <Description("海豚骑士僵尸")> DolphinRiderZombie
        <Description("玩偶匣僵尸")> JackintheboxZombie
        <Description("气球僵尸")> BalloonZombie
        <Description("矿工僵尸")> DiggerZombie
        <Description("跳跳僵尸")> PogoZombie
        <Description("雪人僵尸")> ZombieYeti
        <Description("蹦极僵尸")> BungeeZombie
        <Description("扶梯僵尸")> LadderZombie
        <Description("投石车僵尸")> CatapultZombie
        <Description("伽刚特尔")> Gargantuar
        <Description("小鬼僵尸")> Imp
        <Description("僵王博士")> DrZomboss
        <Description("豌豆射手僵尸")> PeashooterZombie
        <Description("坚果墙僵尸")> WallnutZombie
        <Description("火爆辣椒僵尸")> JalapenoZombie
        <Description("机枪射手僵尸")> GatlingPeaZombie
        <Description("倭瓜僵尸")> SquashZombie
        <Description("高坚果僵尸")> TallnutZombie
        <Description("暴走伽刚特尔")> Gigagargantuar
    End Enum
    ''' <summary>
    ''' 僵尸对象
    ''' </summary>
    Public Class Zombie
        ''' <summary>
        ''' 对象指针
        ''' </summary>
        Public BaseAddress As Integer
        ''' <summary>
        ''' 新建一个僵尸对象,可以使用僵尸的序号或者对象指针
        ''' </summary>
        ''' <param name="IndexOrAddress">僵尸的序号或者对象指针</param>
        Public Sub New(ByVal IndexOrAddress As Integer)
            If IndexOrAddress > 1024 Then
                BaseAddress = IndexOrAddress
            Else
                BaseAddress = Memory.ReadInteger(PVZ.BaseAddress + &H90) + IndexOrAddress * &H15C
            End If
        End Sub
        ''' <summary>
        ''' 僵尸图像的横坐标,若想修改真实坐标请修改X
        ''' </summary>
        Public Property ImageX As Integer
            Get
                Return Memory.ReadInteger(BaseAddress + 8)

            End Get
            Set(Value As Integer)
                Memory.WriteInteger(BaseAddress + 8, Value)
            End Set
        End Property
        ''' <summary>
        ''' 僵尸图像的纵坐标,若想修改真实坐标请修改Y
        ''' </summary>
        Public Property ImageY As Integer
            Get
                Return Memory.ReadInteger(BaseAddress + &HC)

            End Get
            Set(Value As Integer)
                Memory.WriteInteger(BaseAddress + &HC, Value)
            End Set
        End Property
        ''' <summary>
        ''' 设置或获取僵尸是否可见
        ''' </summary>
        Public Property Visible As Boolean
            Get
                Return Memory.ReadInteger(BaseAddress + &H18)

            End Get
            Set(Value As Boolean)
                Memory.WriteInteger(BaseAddress + &H18, Convert.ToInt32(Value))
            End Set
        End Property
        ''' <summary>
        ''' 设置或获取僵尸所在行
        ''' </summary>
        Public Property Row As Integer
            Get
                Return Memory.ReadInteger(BaseAddress + &H1C)

            End Get
            Set(Value As Integer)
                Memory.WriteInteger(BaseAddress + &H1C, Value)
            End Set
        End Property
        ''' <summary>
        ''' 设置或获取僵尸的图层
        ''' </summary>
        Public Property Layer As Integer
            Get
                Return Memory.ReadInteger(BaseAddress + &H20)

            End Get
            Set(Value As Integer)
                Memory.WriteInteger(BaseAddress + &H20, Value)
            End Set
        End Property
        ''' <summary>
        ''' 设置或获取僵尸的类型(通常不推荐设置)
        ''' </summary>
        Public Property Type As ZombieType
            Get
                Return Memory.ReadInteger(BaseAddress + &H24)
            End Get
            Set(Value As ZombieType)
                Memory.WriteInteger(BaseAddress + &H24, Value)
            End Set
        End Property
        ''' <summary>
        ''' 设置或获取僵尸的状态
        ''' </summary>
        Public Property State As Integer
            Get
                Return Memory.ReadInteger(BaseAddress + &H28)
            End Get
            Set(Value As Integer)
                Memory.WriteInteger(BaseAddress + &H28, Value)
            End Set
        End Property
        ''' <summary>
        ''' 设置或获取僵尸的横坐标
        ''' </summary>
        Public Property X As Single
            Get
                Return Memory.ReadFloat(BaseAddress + &H2C)
            End Get
            Set(Value As Single)
                Memory.WriteFloat(BaseAddress + &H2C, Value)
            End Set
        End Property
        ''' <summary>
        ''' 设置或获取僵尸的纵坐标
        ''' </summary>
        Public Property Y As Single
            Get
                Return Memory.ReadFloat(BaseAddress + &H30)
            End Get
            Set(Value As Single)
                Memory.WriteFloat(BaseAddress + &H30, Value)
            End Set
        End Property
        ''' <summary>
        ''' 僵尸发光(被击中时的效果)
        ''' </summary>
        ''' <param name="CentiSecond">发光的时长(厘秒),默认100</param>
        Public Sub Light(Optional ByVal CentiSecond As Integer = 100)
            Memory.WriteInteger(BaseAddress + &H54, CentiSecond)
        End Sub
        ''' <summary>
        ''' 获取僵尸的已存在时间
        ''' </summary>
        Public ReadOnly Property ExistedTime As Integer
            Get
                Return Memory.ReadInteger(BaseAddress + &H60)
            End Get
        End Property
        ''' <summary>
        ''' 设置或获取僵尸的属性倒计时,具体由僵尸本身的类别区分作用
        ''' </summary>
        Public Property AttributeCountdown As Integer
            Get
                Return Memory.ReadInteger(BaseAddress + &H68)
            End Get
            Set(Value As Integer)
                Memory.WriteInteger(BaseAddress + &H68, Value)
            End Set
        End Property
        ''' <summary>
        ''' 设置或获取僵尸消失倒计时
        ''' </summary>
        Public Property DisappearCountdown As Integer
            Get
                Return Memory.ReadInteger(BaseAddress + &H74)
            End Get
            Set(Value As Integer)
                Memory.WriteInteger(BaseAddress + &H74, Value)
            End Set
        End Property
        ''' <summary>
        ''' 设置或获取僵尸碰撞判定的横坐标(相对自身的坐标)
        ''' </summary>
        Public Property CollisionX As Integer
            Get
                Return Memory.ReadInteger(BaseAddress + &H8C)
            End Get
            Set(Value As Integer)
                Memory.WriteInteger(BaseAddress + &H8C, Value)
            End Set
        End Property
        ''' <summary>
        ''' 设置或获取僵尸碰撞判定的纵坐标(相对自身的坐标)
        ''' </summary>
        Public Property CollisionY As Integer
            Get
                Return Memory.ReadInteger(BaseAddress + &H90)
            End Get
            Set(Value As Integer)
                Memory.WriteInteger(BaseAddress + &H90, Value)
            End Set
        End Property
        ''' <summary>
        ''' 设置或获取僵尸碰撞判定的宽度
        ''' </summary>
        Public Property CollisionLength As Integer
            Get
                Return Memory.ReadInteger(BaseAddress + &H94)
            End Get
            Set(Value As Integer)
                Memory.WriteInteger(BaseAddress + &H94, Value)
            End Set
        End Property
        ''' <summary>
        ''' 设置或获取僵尸碰撞判定的高度
        ''' </summary>
        Public Property CollisionHeight As Integer
            Get
                Return Memory.ReadInteger(BaseAddress + &H98)
            End Get
            Set(Value As Integer)
                Memory.WriteInteger(BaseAddress + &H98, Value)
            End Set
        End Property
        ''' <summary>
        ''' 设置或获取僵尸攻击判定的横坐标(相对自身的坐标)
        ''' </summary>
        Public Property AttackCollisionX As Integer
            Get
                Return Memory.ReadInteger(BaseAddress + &H9C)
            End Get
            Set(Value As Integer)
                Memory.WriteInteger(BaseAddress + &H9C, Value)
            End Set
        End Property
        ''' <summary>
        ''' 设置或获取僵尸攻击判定的纵坐标(相对自身的坐标)
        ''' </summary>
        Public Property AttackCollisionY As Integer
            Get
                Return Memory.ReadInteger(BaseAddress + &HA0)
            End Get
            Set(Value As Integer)
                Memory.WriteInteger(BaseAddress + &HA0, Value)
            End Set
        End Property
        ''' <summary>
        ''' 设置或获取僵尸攻击判定的宽度
        ''' </summary>
        Public Property AttackCollisionLength As Integer
            Get
                Return Memory.ReadInteger(BaseAddress + &HA4)
            End Get
            Set(Value As Integer)
                Memory.WriteInteger(BaseAddress + &HA4, Value)
            End Set
        End Property
        ''' <summary>
        ''' 设置或获取僵尸攻击判定的高度
        ''' </summary>
        Public Property AttackCollisionHeight As Integer
            Get
                Return Memory.ReadInteger(BaseAddress + &HA8)
            End Get
            Set(Value As Integer)
                Memory.WriteInteger(BaseAddress + &HA8, Value)
            End Set
        End Property
        ''' <summary>
        ''' 设置或获取僵尸的减速倒计时(修改此值不会立刻让僵尸减速)
        ''' </summary>
        Public Property DecelerateCountdown As Integer
            Get
                Return Memory.ReadInteger(BaseAddress + &HAC)
            End Get
            Set(Value As Integer)
                Memory.WriteInteger(BaseAddress + &HAC, Value)
            End Set
        End Property
        ''' <summary>
        ''' 设置或获取僵尸的黄油固定倒计时(修改此值不会让僵尸停止活动,但会停止行为)
        ''' </summary>
        Public Property FixedCountdown As Integer
            Get
                Return Memory.ReadInteger(BaseAddress + &HB0)
            End Get
            Set(Value As Integer)
                Memory.WriteInteger(BaseAddress + &HB0, Value)
            End Set
        End Property
        ''' <summary>
        ''' 设置或获取僵尸的冻结倒计时(修改此值不会让僵尸停止活动,但会停止行为)
        ''' </summary>
        Public Property FrozenCountdown As Integer
            Get
                Return Memory.ReadInteger(BaseAddress + &HB4)
            End Get
            Set(Value As Integer)
                Memory.WriteInteger(BaseAddress + &HB4, Value)
            End Set
        End Property
        ''' <summary>
        ''' 设置或获取僵尸是否被魅惑
        ''' </summary>
        Public Property Hypnotized As Boolean
            Get
                Return Memory.ReadByte(BaseAddress + &HB8)
            End Get
            Set(Value As Boolean)
                Memory.WriteByte(BaseAddress + &HB8, Convert.ToInt32(Value))
            End Set
        End Property
        ''' <summary>
        ''' 设置或获取僵尸是否被吹走
        ''' </summary>
        Public Property Blowaway As Boolean
            Get
                Return Memory.ReadByte(BaseAddress + &HB9)
            End Get
            Set(Value As Boolean)
                Memory.WriteByte(BaseAddress + &HB9, Convert.ToInt32(Value))
            End Set
        End Property
        ''' <summary>
        ''' 设置或获取僵尸是否将死(血量低于临界值的状态)
        ''' </summary>
        Public Property Dying As Boolean
            Get
                Return Memory.ReadByte(BaseAddress + &HBA) = 0
            End Get
            Set(Value As Boolean)
                Memory.WriteByte(BaseAddress + &HBA, Convert.ToInt32(Not Value))
            End Set
        End Property
        ''' <summary>
        ''' 僵尸是否过断手
        ''' </summary>
        Public ReadOnly Property HandBroken As Boolean
            Get
                Return Memory.ReadByte(BaseAddress + &HBB) = 0
            End Get
        End Property
        ''' <summary>
        ''' 设置或获取僵尸是否有手持物,雪人是否向左走
        ''' </summary>
        Public Property SthinHandOrYetiLeft As Boolean
            Get
                Return Memory.ReadByte(BaseAddress + &HBC)
            End Get
            Set(Value As Boolean)
                Memory.WriteByte(BaseAddress + &HBC, Convert.ToInt32(Value))
            End Set
        End Property
        ''' <summary>
        ''' 设置或获取僵尸是否在水中
        ''' </summary>
        Public Property InWater As Boolean
            Get
                Return Memory.ReadByte(BaseAddress + &HBD)
            End Get
            Set(Value As Boolean)
                Memory.WriteByte(BaseAddress + &HBD, Convert.ToInt32(Value))
            End Set
        End Property
        ''' <summary>
        ''' 设置或获取僵尸是否吃到了大蒜
        ''' </summary>
        Public Property GarlicBited As Boolean
            Get
                Return Memory.ReadByte(BaseAddress + &HBF)
            End Get
            Set(Value As Boolean)
                Memory.WriteByte(BaseAddress + &HBF, Convert.ToInt32(Value))
            End Set
        End Property
        ''' <summary>
        ''' 僵尸1类饰品(全方位防御)枚举
        ''' </summary>
        Public Enum Accessories1
            <Description("无饰品")> None
            <Description("路障")> RoadCone
            <Description("铁桶")> Bucket
            <Description("橄榄球帽")> FootballCap
            <Description("矿工帽")> MinerHat
            <Description(" 雪橇")> Sled
            <Description("坚果")> Wallnut
            <Description("高坚果")> Tallnut
        End Enum
        ''' <summary>
        ''' 僵尸2类饰品(会直接受到投掷,地刺,后方攻击和僵尸啃咬攻击)枚举
        ''' </summary>
        Public Enum Accessories2
            <Description("无饰品")> None
            <Description("铁栅门")> ScreenDoor
            <Description("报纸")> Newspaper
            <Description("梯子")> Ladder
        End Enum
        ''' <summary>
        ''' 设置或获取僵尸1类饰品
        ''' </summary>
        Public ReadOnly Property AccessoriesType1 As Accessories1
            Get
                Return Memory.ReadInteger(BaseAddress + &HC4)
            End Get
        End Property
        ''' <summary>
        ''' 设置或获取僵尸本体血量
        ''' </summary>
        Public Property BodyHP As Integer
            Get
                Return Memory.ReadInteger(BaseAddress + &HC8)
            End Get
            Set(Value As Integer)
                Memory.WriteInteger(BaseAddress + &HC8, Value)
            End Set
        End Property
        ''' <summary>
        ''' 设置或获取僵尸本体血量上限
        ''' </summary>
        Public Property MaxBodyHP As Integer
            Get
                Return Memory.ReadInteger(BaseAddress + &HCC)
            End Get
            Set(Value As Integer)
                Memory.WriteInteger(BaseAddress + &HCC, Value)
            End Set
        End Property
        ''' <summary>
        ''' 设置或获取僵尸1类饰品血量
        ''' </summary>
        Public Property AccessoriesType1HP As Integer
            Get
                Return Memory.ReadInteger(BaseAddress + &HD0)
            End Get
            Set(Value As Integer)
                Memory.WriteInteger(BaseAddress + &HD0, Value)
            End Set
        End Property
        ''' <summary>
        ''' 设置或获取僵尸1类饰品血量上限
        ''' </summary>
        Public Property MaxAccessoriesType1HP As Integer
            Get
                Return Memory.ReadInteger(BaseAddress + &HD4)
            End Get
            Set(Value As Integer)
                Memory.WriteInteger(BaseAddress + &HD4, Value)
            End Set
        End Property
        ''' <summary>
        ''' 设置或获取僵尸2类饰品
        ''' </summary>
        Public ReadOnly Property AccessoriesType2 As Accessories2
            Get
                Return Memory.ReadInteger(BaseAddress + &HD8)
            End Get
        End Property
        ''' <summary>
        ''' 设置或获取僵尸2类饰品血量
        ''' </summary>
        Public Property AccessoriesType2HP As Integer
            Get
                Return Memory.ReadInteger(BaseAddress + &HDC)
            End Get
            Set(Value As Integer)
                Memory.WriteInteger(BaseAddress + &HDC, Value)
            End Set
        End Property
        ''' <summary>
        ''' 设置或获取僵尸2类饰品血量上限
        ''' </summary>
        Public Property MaxAccessoriesType2HP As Integer
            Get
                Return Memory.ReadInteger(BaseAddress + &HE0)
            End Get
            Set(Value As Integer)
                Memory.WriteInteger(BaseAddress + &HE0, Value)
            End Set
        End Property
        ''' <summary>
        ''' 设置或获取僵尸是否存在
        ''' </summary>
        Public Property Exist As Boolean
            Get
                Return Memory.ReadInteger(BaseAddress + &HEC) = 0
            End Get
            Set(Value As Boolean)
                Memory.WriteInteger(BaseAddress + &HEC, Convert.ToInt32(Not Value))
            End Set
        End Property
        ''' <summary>
        ''' 僵尸的动画对象
        ''' </summary>
        Public ReadOnly Property Animation As Animation
            Get
                Return New Animation(Memory.ReadShort(BaseAddress + &H118))
            End Get
        End Property
        ''' <summary>
        ''' 设置或获取僵尸大小
        ''' </summary>
        Public Property Size As Single
            Get
                Return Memory.ReadFloat(BaseAddress + &H11C)
            End Get
            Set(Value As Single)
                Memory.WriteFloat(BaseAddress + &H11C, Value)
            End Set
        End Property
        ''' <summary>
        ''' 僵尸的ID(可用于子弹的跟踪)
        ''' </summary>
        Public ReadOnly Property Id As Integer
            Get
                Return Memory.ReadInteger(BaseAddress + &H158)
            End Get
        End Property
        ''' <summary>
        ''' 取得这是第几个僵尸
        ''' </summary>
        Public ReadOnly Property Index As Integer
            Get
                Return Id And &HFFFF
            End Get
        End Property
        ''' <summary>
        ''' 伤害类型枚举
        ''' </summary>
        Public Enum DamageType
            <Description("正面")> Direct
            <Description("非正面")> NotDirect
            <Description("溅射")> Sputter
            <Description("正面减速")> DirectDecelerate = 4
            <Description("非正面减速")> NotDirectDecelerate
            <Description("溅射减速")> SputterDecelerate
        End Enum
        ''' <summary>
        ''' 攻击僵尸
        ''' </summary>
        ''' <param name="Damage">伤害值</param>
        ''' <param name="DamageType­">伤害类型,默认正前方普通伤害</param>
        Public Sub Hit(ByVal Damage As Integer, Optional ByVal DamageType­ As DamageType = DamageType.Direct)
            Dim AsmCode As Byte() = {
                mov_esi, 0, 0, 0, 0,
                pushdw, 0, 0, 0, 0,
                mov_eax, 0, 0, 0, 0,
                call­, 2, 0, 0, 0,
                jmp, 6,
                pushdw, &HC0, &H17, &H53, 0,
                ret,
                ret
            }
            ReplaceBytes(AsmCode, 1, BaseAddress)
            ReplaceBytes(AsmCode, 6, Damage)
            ReplaceBytes(AsmCode, 11, DamageType­)
            Memory.Execute(AsmCode)
        End Sub
        ''' <summary>
        ''' 炸死僵尸(给予一个灰烬攻击)
        ''' </summary>
        Public Sub Blast()
            Dim AsmCode As Byte() = {
                mov_ecx, 0, 0, 0, 0,
                call­, 2, 0, 0, 0,
                jmp, 6,
                pushdw, &H70, &H2B, &H53, 0,
                ret,
                ret
            }
            ReplaceBytes(AsmCode, 1, BaseAddress)
            Memory.Execute(AsmCode)
        End Sub
        ''' <summary>
        ''' 黄油固定僵尸
        ''' </summary>
        Public Sub Butter()
            Dim AsmCode As Byte() = {
                mov_eax, 0, 0, 0, 0,
                call­, 2, 0, 0, 0,
                jmp, 6,
                pushdw, &HD0, &H26, &H53, 0,
                ret,
                ret
            }
            ReplaceBytes(AsmCode, 1, BaseAddress)
            Memory.Execute(AsmCode)
        End Sub
        ''' <summary>
        ''' 僵尸的事件循环,即一直会运行的代码
        ''' </summary>
        Class EventLoop
            Private Sub New()
            End Sub
            ''' <summary>
            ''' 初始化应用事件循环的汇编代码
            ''' </summary>
            ''' <param name="Asmcode">以8B 06开始(mov eax,[esi]),C3(ret)结束,其中eax即为所有僵尸对象的指针</param>
            Public Shared Sub Initialize(ByVal Asmcode As Byte())
                Memory.WriteBytes(Variable + 300, Asmcode)
            End Sub
            ''' <summary>
            ''' 开始执行事件循环,一点要先初始化,并且一旦运行,直到调用<see cref="Stop­()"/>方法或者游戏结束为止一直有效,中途不可再调用<see cref="Initialize(Byte())"/>
            ''' </summary>
            Public Shared Sub Start()
                Memory.WriteByte(&H41311B, 232)
                Memory.WriteInteger(&H41311C, Variable + 300 - 4 - &H41311C)
                Memory.WriteByte(&H413137, 227)
            End Sub
            ''' <summary>
            ''' 结束事件循环,随时可以再次调用<see cref="Start()"/>
            ''' </summary>
            Public Shared Sub Stop­()
                Memory.WriteByte(&H41311B, 235)
                Memory.WriteInteger(&H41311C, 4820227)
                Memory.WriteByte(&H413137, 232)
            End Sub
        End Class
    End Class
    ''' <summary>写
    ''' 植物类型枚举
    ''' </summary>
    Public Enum PlantType
        <Description("豌豆射手")> Peashooter
        <Description("向日葵")> Sunflower
        <Description("樱桃炸弹")> CherryBomb
        <Description("坚果墙")> Wallnut
        <Description("土豆雷")> PotatoMine
        <Description("寒冰射手")> SnowPea
        <Description("大嘴花")> Chomper
        <Description("双发射手")> Repeater
        <Description("小喷菇")> Puffshroom
        <Description("阳光菇")> Sunshroom
        <Description("大喷菇")> Fumeshroom
        <Description("墓碑吞噬者")> GraveBuster
        <Description("魅惑菇")> Hypnoshroom
        <Description("胆小菇")> Scaredyshroom
        <Description("寒冰菇")> Iceshroom
        <Description("毁灭菇")> Doomshroom
        <Description("睡莲")> LilyPad
        <Description("倭瓜")> Squash
        <Description("三线射手")> Threepeater
        <Description("缠绕海草")> TangleKelp
        <Description("火爆辣椒")> Jalapeno
        <Description("地刺")> Caltrop
        <Description("火炬树桩")> Torchwood
        <Description("高坚果")> Tallnut
        <Description("海蘑菇")> Seashroom
        <Description("路灯花")> Plantern
        <Description("仙人掌")> Cactus
        <Description("三叶草")> Blover
        <Description("裂荚射手")> SplitPea
        <Description("杨桃")> Starfruit
        <Description("南瓜头")> Pumpkin
        <Description("磁力菇")> Magnetshroom
        <Description("卷心菜投手")> Cabbagepult
        <Description("花盆")> Pot
        <Description("玉米投手")> Cornpult
        <Description("咖啡豆")> CoffeeBean
        <Description("大蒜")> Garlic
        <Description("叶子保护伞")> UmbrellaLeaf
        <Description("金盏花")> Marigold
        <Description("西瓜投手")> Melonpult
        <Description("机枪射手")> GatlingPea
        <Description("双子向日葵")> TwinSunflower
        <Description("忧郁蘑菇")> Gloomshroom
        <Description("香蒲")> Cattail
        <Description("冰瓜")> WinterMelon
        <Description("吸金磁")> GoldMagnet
        <Description("地刺王")> Spikerock
        <Description("玉米加农炮")> CobCannon
        <Description("模仿者")> Imitater
        <Description("爆炸坚果")> Explodenut
        <Description("巨大坚果")> GiantWallnut
        <Description("幼苗")> Sprout
        <Description("左向双发射手")> LeftRepeater
    End Enum
    ''' <summary>
    ''' 植物特效类型枚举
    ''' </summary>
    Public Enum PlantEffectType
        <Description("樱桃爆炸")> CherryBomb = 2
        <Description("土豆雷爆炸")> PotatoMine = 4
        <Description("冰冻全屏")> Iceshroon = 14
        <Description("毁灭菇爆炸")> Doomshroon = 15
        <Description("辣椒爆炸")> Jalapeno = 20
        <Description("三叶草吹风")> Blover = 27
    End Enum
    ''' <summary>
    ''' 植物对象
    ''' </summary>
    Public Class Plant
        ''' <summary>
        ''' 对象指针
        ''' </summary>
        Public BaseAddress As Integer
        ''' <summary>
        ''' 新建一个植物对象,可以使用植物的序号或者对象指针
        ''' </summary>
        ''' <param name="IndexOrAddress">植物的序号或者对象指针</param>
        Public Sub New(ByVal IndexOrAddress As Integer)
            If IndexOrAddress > 1024 Then
                BaseAddress = IndexOrAddress
            Else
                BaseAddress = Memory.ReadInteger(PVZ.BaseAddress + &HAC) + IndexOrAddress * &H14C
            End If
        End Sub
        ''' <summary>
        ''' 设置或获取植物的横坐标
        ''' </summary>
        Public Property X As Integer
            Get
                Return Memory.ReadInteger(BaseAddress + 8)

            End Get
            Set(Value As Integer)
                Memory.WriteInteger(BaseAddress + 8, Value)
            End Set
        End Property
        ''' <summary>
        ''' 设置或获取植物的纵坐标
        ''' </summary>
        Public Property Y As Integer
            Get
                Return Memory.ReadInteger(BaseAddress + &HC)

            End Get
            Set(Value As Integer)
                Memory.WriteInteger(BaseAddress + &HC, Value)
            End Set
        End Property
        ''' <summary>
        ''' 设置或获取植物是否可见
        ''' </summary>
        Public Property Visible As Boolean
            Get
                Return Memory.ReadInteger(BaseAddress + &H18)

            End Get
            Set(Value As Boolean)
                Memory.WriteInteger(BaseAddress + &H18, Convert.ToInt32(Value))
            End Set
        End Property
        ''' <summary>
        ''' 设置或获取植物所在行
        ''' </summary>
        Public Property Row As Integer
            Get
                Return Memory.ReadInteger(BaseAddress + &H1C)

            End Get
            Set(Value As Integer)
                Memory.WriteInteger(BaseAddress + &H1C, Value)
            End Set
        End Property
        ''' <summary>
        ''' 设置或获取植物图层
        ''' </summary>
        Public Property Layer As Integer
            Get
                Return Memory.ReadInteger(BaseAddress + &H20)

            End Get
            Set(Value As Integer)
                Memory.WriteInteger(BaseAddress + &H20, Value)
            End Set
        End Property
        ''' <summary>
        ''' 设置或获取植物类型(通常不推荐设置)
        ''' </summary>
        Public Property Type As PlantType
            Get
                Return Memory.ReadInteger(BaseAddress + &H24)
            End Get
            Set(Value As PlantType)
                Memory.WriteInteger(BaseAddress + &H24, Value)
            End Set
        End Property
        ''' <summary>
        ''' 设置或获取植物所在列
        ''' </summary>
        Public Property Column As Integer
            Get
                Return Memory.ReadInteger(BaseAddress + &H28)
            End Get
            Set(Value As Integer)
                Memory.WriteInteger(BaseAddress + &H28, Value)
            End Set
        End Property
        ''' <summary>
        ''' 设置或获取植物状态
        ''' </summary>
        Public Property State As Integer
            Get
                Return Memory.ReadInteger(BaseAddress + &H3C)
            End Get
            Set(Value As Integer)
                Memory.WriteInteger(BaseAddress + &H3C, Value)
            End Set
        End Property
        ''' <summary>
        ''' 设置或获取植物血量
        ''' </summary>
        Public Property Hp As Integer
            Get
                Return Memory.ReadInteger(BaseAddress + &H40)
            End Get
            Set(Value As Integer)
                Memory.WriteInteger(BaseAddress + &H40, Value)
            End Set
        End Property
        ''' <summary>
        ''' 设置或获取植物血量上限
        ''' </summary>
        Public Property MaxHp As Integer
            Get
                Return Memory.ReadInteger(BaseAddress + &H44)
            End Get
            Set(Value As Integer)
                Memory.WriteInteger(BaseAddress + &H44, Value)
            End Set
        End Property
        ''' <summary>
        ''' 设置或获取植物是否可以攻击
        ''' </summary>
        Public Property Aggressive As Boolean
            Get
                Return Memory.ReadInteger(BaseAddress + &H48)

            End Get
            Set(Value As Boolean)
                Memory.WriteInteger(BaseAddress + &H48, Convert.ToInt32(Value))
            End Set
        End Property
        ''' <summary>
        ''' 设置或获取三叶草消失倒计时
        ''' </summary>
        Public Property BloverDisappearCountdown As Integer
            Get
                Return Memory.ReadInteger(BaseAddress + &H4C)
            End Get
            Set(Value As Integer)
                Memory.WriteInteger(BaseAddress + &H4C, Value)
            End Set
        End Property
        ''' <summary>
        ''' 设置或获取植物产生爆炸效果倒计时
        ''' </summary>
        Public Property EffectiveCountdown As Integer
            Get
                Return Memory.ReadInteger(BaseAddress + &H50)
            End Get
            Set(Value As Integer)
                Memory.WriteInteger(BaseAddress + &H50, Value)
            End Set
        End Property
        ''' <summary>
        ''' 设置或获取植物属性倒计时
        ''' </summary>
        Public Property AttributeCountdown As Integer
            Get
                Return Memory.ReadInteger(BaseAddress + &H54)
            End Get
            Set(Value As Integer)
                Memory.WriteInteger(BaseAddress + &H54, Value)
            End Set
        End Property
        ''' <summary>
        ''' 设置或获取植物射击,产生物品倒计时
        ''' </summary>
        Public Property ShootOrProductCountdown As Integer
            Get
                Return Memory.ReadInteger(BaseAddress + &H58)
            End Get
            Set(Value As Integer)
                Memory.WriteInteger(BaseAddress + &H58, Value)
            End Set
        End Property
        ''' <summary>
        ''' 设置或获取植物射击,产生物品时间间隔
        ''' </summary>
        Public Property ShootOrProductInterval As Integer
            Get
                Return Memory.ReadInteger(BaseAddress + &H5C)
            End Get
            Set(Value As Integer)
                Memory.WriteInteger(BaseAddress + &H5C, Value)
            End Set
        End Property
        ''' <summary>
        ''' 设置或获取植物射击(产生子弹)倒计时
        ''' </summary>
        Public Property ShootingCountdown As Integer
            Get
                Return Memory.ReadInteger(BaseAddress + &H90)
            End Get
            Set(Value As Integer)
                Memory.WriteInteger(BaseAddress + &H90, Value)
            End Set
        End Property
        ''' <summary>
        ''' 植物动画对象1
        ''' </summary>
        Public ReadOnly Property Animation1 As Animation
            Get
                Return New Animation(Memory.ReadShort(BaseAddress + &H94))
            End Get
        End Property
        ''' <summary>
        ''' 植物动画对象2
        ''' </summary>
        Public ReadOnly Property Animation2 As Animation
            Get
                Return New Animation(Memory.ReadShort(BaseAddress + &H98))
            End Get
        End Property
        ''' <summary>
        ''' 植物发光
        ''' </summary>
        ''' <param name="CentiSecond">发光的时长,默认100</param>
        Public Sub Light(Optional ByVal CentiSecond As Integer = 100)
            Memory.WriteInteger(BaseAddress + &HB8, CentiSecond)
        End Sub
        ''' <summary>
        ''' 植物闪光
        ''' </summary>
        ''' <param name="CentiSecond">闪光的时长,默认100</param>
        Public Sub Flash(Optional ByVal CentiSecond As Integer = 100)
            Memory.WriteInteger(BaseAddress + &HBC, CentiSecond)
        End Sub
        ''' <summary>
        ''' 设置或获取植物横坐标偏移
        ''' </summary>
        Public Property ImageXOffset As Single
            Get
                Return Memory.ReadFloat(BaseAddress + &HC0)
            End Get
            Set(Value As Single)
                Memory.WriteFloat(BaseAddress + &HC0, Value)
            End Set
        End Property
        ''' <summary>
        ''' 设置或获取植物纵坐标偏移
        ''' </summary>
        Public Property ImageYOffset As Single
            Get
                Return Memory.ReadFloat(BaseAddress + &HC4)
            End Get
            Set(Value As Single)
                Memory.WriteFloat(BaseAddress + &HC4, Value)
            End Set
        End Property
        ''' <summary>
        ''' 设置或获取植物是否存在
        ''' </summary>
        Public Property Exist As Boolean
            Get
                Return Memory.ReadByte(BaseAddress + &H141) = 0
            End Get
            Set(Value As Boolean)
                Memory.WriteByte(BaseAddress + &H141, Convert.ToInt32(Not Value))
            End Set
        End Property
        ''' <summary>
        ''' 设置或获取植物是否压扁
        ''' </summary>
        Public Property Squash As Boolean
            Get
                Return Memory.ReadByte(BaseAddress + &H142)
            End Get
            Set(Value As Boolean)
                Memory.WriteByte(BaseAddress + &H142, Convert.ToInt32(Value))
            End Set
        End Property
        ''' <summary>
        ''' 设置或获取植物是否睡着
        ''' </summary>
        Public Property Sleeping As Boolean
            Get
                Return Memory.ReadByte(BaseAddress + &H143) = 0
            End Get
            Set(Value As Boolean)
                Memory.WriteByte(BaseAddress + &H143, Convert.ToInt32(Not Value))
            End Set
        End Property
        ''' <summary>
        ''' 植物ID
        ''' </summary>
        Public ReadOnly Property Id As Integer
            Get
                Return Memory.ReadInteger(BaseAddress + &H148)
            End Get
        End Property
        ''' <summary>
        ''' 取得这是第几个植物
        ''' </summary>
        Public ReadOnly Property Index As Integer
            Get
                Return Id And &HFFFF
            End Get
        End Property
        ''' <summary>
        ''' 产生效果,注意只有部分特殊植物可以产生效果
        ''' </summary>
        Public Sub CreateEffect()
            If Type = PlantEffectType.CherryBomb Or
                Type = PlantEffectType.PotatoMine Or
                Type = PlantEffectType.Iceshroon Or
                Type = PlantEffectType.Doomshroon Or
                Type = PlantEffectType.Jalapeno Or
                Type = PlantEffectType.Blover Then
                Dim AsmCode As Byte() = {
                    pushdw, 0, 0, 0, 0,
                    call­, 2, 0, 0, 0,
                    jmp, 6,
                    pushdw, &HA0, &H66, &H46, 0,
                    ret,
                    ret
                }
                ReplaceBytes(AsmCode, 1, BaseAddress)
                Memory.Execute(AsmCode)
            End If
        End Sub
        ''' <summary>
        ''' 固定植物,如同IZ中的植物一样不会运动
        ''' </summary>
        Public Sub Fix()
            Dim AsmCode As Byte() = {
                pushdw, 0, 0, 0, 0,
                mov_eax_ptr, 0, 0, 0, 0,
                call­, 2, 0, 0, 0,
                jmp, 6,
                pushdw, &H30, &HA5, &H42, 0,
                ret,
                ret
            }
            ReplaceBytes(AsmCode, 1, BaseAddress)
            ReplaceBytes(AsmCode, 6, PVZ.BaseAddress + &H160)
            Memory.Execute(AsmCode)
        End Sub
        ''' <summary>
        ''' 使植物射击,并返回射击的子弹对象,注意杨桃对于该方法没有返回值
        ''' </summary>
        ''' <param name="TracktargetId">目标的ID,设置即可跟踪</param>
        ''' <returns></returns>
        Public Function Shoot(Optional TracktargetId As Integer = -1) As Projectile
            Dim AsmCode As Byte() = {
                push, 0,
                push, CByte(Row),
                pushdw, 0, 0, 0, 0,
                pushdw, 0, 0, 0, 0,
                call­, &H2, 0, 0, 0,
                jmp, &H6,
                pushdw, 0, &H6E, &H46, 0,
                ret,
                &H89, &HD, 0, 0, 0, 0,
                ret
            }
            ReplaceBytes(AsmCode, 10, (BaseAddress))
            ReplaceBytes(AsmCode, 29, (Variable))
            If Type = PlantType.Starfruit Then
                Memory.Execute(AsmCode)
                Return Nothing
            End If
            Dim Re = New Projectile(Memory.Execute(AsmCode))
            If TracktargetId = -1 Then Return Re
            Re.TracktargetId = TracktargetId
            Re.Motion = Projectile.MotionType.Track
            Re.Damagable = True
            Return Re
        End Function
        ''' <summary>
        ''' 植物的事件循环,即一直会运行的代码
        ''' </summary>
        Class EventLoop
            Private Sub New()
            End Sub
            ''' <summary>
            ''' 初始化应用事件循环的汇编代码
            ''' </summary>
            ''' <param name="Asmcode">以8B 06开始(mov eax,[esi]),C3(ret)结束,其中eax即为所有植物对象的指针</param>
            Public Shared Sub Initialize(ByVal Asmcode As Byte())
                Memory.WriteBytes(Variable + 400, Asmcode)
            End Sub
            ''' <summary>
            ''' 开始执行事件循环,一点要先初始化,并且一旦运行,直到调用<see cref="Stop­()"/>方法或者游戏结束为止一直有效,中途不可再调用<see cref="Initialize(Byte())"/>
            ''' </summary>
            Public Shared Sub Start()
                Memory.WriteByte(&H4130EB, 232)
                Memory.WriteInteger(&H4130EC, Variable + 400 - 4 - &H4130EC)
                Memory.WriteByte(&H413107, 227)
            End Sub
            ''' <summary>
            ''' 结束事件循环,随时可以再次调用<see cref="Start()"/>
            ''' </summary>
            Public Shared Sub Stop­()
                Memory.WriteByte(&H4131EB, 235)
                Memory.WriteInteger(&H4131EC, 4820227)
                Memory.WriteByte(&H413107, 232)
            End Sub
        End Class
    End Class
    ''' <summary>
    ''' 子弹类型枚举
    ''' </summary>
    Public Enum ProjectiletType
        <Description("豌豆")> NormalPea
        <Description("寒冰豌豆")> SnowPea
        <Description("卷心菜")> Cabbage
        <Description("西瓜")> Melon
        <Description("孢子")> Puff
        <Description("冰瓜")> WinterMelon
        <Description("火焰豌豆")> FirePea
        <Description("星星")> Star
        <Description("尖刺")> Cactus
        <Description("篮球")> Basketball
        <Description("玉米粒")> Kernel
        <Description("玉米大炮")> CobCannon
        <Description("黄油")> Butter
        <Description("僵尸豌豆")> ZombiePea
    End Enum '
    ''' <summary>
    ''' 子弹对象
    ''' </summary>
    Public Class Projectile
        ''' <summary>
        ''' 对象指针
        ''' </summary>
        Private ReadOnly BaseAddress As Integer
        ''' <summary>
        ''' 新建一个植物对象,可以使用植物的序号或者对象指针
        ''' </summary>
        ''' <param name="IndexOrAddress">植物的序号或者对象指针</param>
        Public Sub New(ByVal IndexOrAddress As Integer)
            If IndexOrAddress > 1024 Then
                BaseAddress = IndexOrAddress
            Else
                BaseAddress = Memory.ReadInteger(PVZ.BaseAddress + &HC8) + IndexOrAddress * &H94
            End If
        End Sub
        ''' <summary>
        ''' 子弹图像的横坐标,若想修改真实坐标请修改X
        ''' </summary>
        Public ReadOnly Property ImageX As Integer
            Get
                Return Memory.ReadInteger(BaseAddress + 8)

            End Get
        End Property
        ''' <summary>
        ''' 子弹图像的纵坐标,若想修改真实坐标请修改Y
        ''' </summary>
        Public ReadOnly Property ImageY As Integer
            Get
                Return Memory.ReadInteger(BaseAddress + &HC)

            End Get
        End Property
        ''' <summary>
        ''' 设置或获取子弹是否可见
        ''' </summary>
        Public Property Visible As Boolean
            Get
                Return Memory.ReadInteger(BaseAddress + &H18)

            End Get
            Set(Value As Boolean)
                Memory.WriteInteger(BaseAddress + &H18, Convert.ToInt32(Value))
            End Set
        End Property
        ''' <summary>
        ''' 设置或获取子弹所在行
        ''' </summary>
        Public Property Row As Integer
            Get
                Return Memory.ReadInteger(BaseAddress + &H1C)

            End Get
            Set(Value As Integer)
                Memory.WriteInteger(BaseAddress + &H1C, Value)
            End Set
        End Property
        ''' <summary>
        ''' 设置或获取子弹图层
        ''' </summary>
        Public Property Layer As Integer
            Get
                Return Memory.ReadInteger(BaseAddress + &H20)

            End Get
            Set(Value As Integer)
                Memory.WriteInteger(BaseAddress + &H20, Value)
            End Set
        End Property
        ''' <summary>
        ''' 设置或获取子弹横坐标
        ''' </summary>
        Public Property X As Single
            Get
                Return Memory.ReadFloat(BaseAddress + &H30)
            End Get
            Set(Value As Single)
                Memory.WriteFloat(BaseAddress + &H30, Value)
            End Set
        End Property
        ''' <summary>
        ''' 设置或获取子弹纵坐标
        ''' </summary>
        Public Property Y As Single
            Get
                Return Memory.ReadFloat(BaseAddress + &H34)
            End Get
            Set(Value As Single)
                Memory.WriteFloat(BaseAddress + &H34, Value)
            End Set
        End Property
        ''' <summary>
        ''' 设置或获取子弹相对地面的高度
        ''' </summary>
        Public Property Height As Single
            Get
                Return Memory.ReadFloat(BaseAddress + &H38)
            End Get
            Set(Value As Single)
                Memory.WriteFloat(BaseAddress + &H38, Value)
            End Set
        End Property
        ''' <summary>
        ''' 设置或获取子弹的横向加速度,注意只有子弹运动状态为<see cref="MotionType.Float"/>时有效果
        ''' </summary>
        Public Property XSpeed As Single
            Get
                Return Memory.ReadFloat(BaseAddress + &H3C)
            End Get
            Set(Value As Single)
                Memory.WriteFloat(BaseAddress + &H3C, Value)
            End Set
        End Property
        ''' <summary>
        ''' 设置或获取子弹的纵向加速度,注意只有子弹运动状态为<see cref="MotionType.Float"/>时有效果
        ''' </summary>
        Public Property YSpeed As Single
            Get
                Return Memory.ReadFloat(BaseAddress + &H40)
            End Get
            Set(Value As Single)
                Memory.WriteFloat(BaseAddress + &H40, Value)
            End Set
        End Property
        ''' <summary>
        ''' 设置或获取子弹是否存在
        ''' </summary>
        Public Property Exist As Boolean
            Get
                Return Memory.ReadInteger(BaseAddress + &H50) = 0
            End Get
            Set(Value As Boolean)
                Memory.WriteInteger(BaseAddress + &H50, Convert.ToInt32(Not Value))
            End Set
        End Property
        ''' <summary>
        ''' 子弹运动状态枚举
        ''' </summary>
        Public Enum MotionType
            <Description("正面")> Direct
            <Description("抛掷")> Throw­
            <Description("正面修正")> Slide
            <Description("向左")> Left = 4
            <Description("向左修正")> LeftSlide = 6
            <Description("浮动")> Float
            <Description("慢速")> Slow
            <Description("跟踪")> Track
        End Enum
        ''' <summary>
        ''' 设置或获取子弹运动状态
        ''' </summary>
        Public Property Motion As MotionType
            Get
                Return Memory.ReadInteger(BaseAddress + &H58)
            End Get
            Set(Value As MotionType)
                Memory.WriteInteger(BaseAddress + &H58, Value)
            End Set
        End Property
        ''' <summary>
        ''' 设置或获取子弹类型
        ''' </summary>
        Public Property Type As ProjectiletType
            Get
                Return Memory.ReadInteger(BaseAddress + &H5C)
            End Get
            Set(Value As ProjectiletType)
                Memory.WriteInteger(BaseAddress + &H5C, Value)
            End Set
        End Property
        ''' <summary>
        ''' 子弹已经存在的时间
        ''' </summary>
        Public ReadOnly Property ExistedTime As Integer
            Get
                Return Memory.ReadInteger(BaseAddress + &H60)
            End Get
        End Property
        ''' <summary>
        ''' 设置或获取子弹旋转角度
        ''' </summary>
        Public Property RotationAngle As Single
            Get
                Return Memory.ReadFloat(BaseAddress + &H68)
            End Get
            Set(Value As Single)
                Memory.WriteFloat(BaseAddress + &H68, Value)
            End Set
        End Property
        ''' <summary>
        ''' 设置或获取子弹旋转速度
        ''' </summary>
        Public Property RotationSpeed As Single
            Get
                Return Memory.ReadFloat(BaseAddress + &H6C)
            End Get
            Set(Value As Single)
                Memory.WriteFloat(BaseAddress + &H6C, Value)
            End Set
        End Property
        ''' <summary>
        ''' 设置或获子弹的伤害状压
        ''' </summary>
        Public Property Damagable As Integer
            Get
                Return Memory.ReadInteger(BaseAddress + &H74)
            End Get
            Set(Value As Integer)
                Memory.WriteInteger(BaseAddress + &H74, Value)
            End Set
        End Property
        ''' <summary>
        ''' 设置或获取子弹的跟踪目标,注意只有子弹运动状态为<see cref="MotionType.Track"/>时有效果
        ''' </summary>
        Public Property TracktargetId As Integer
            Get
                Return Memory.ReadInteger(BaseAddress + &H88)
            End Get
            Set(Value As Integer)
                Memory.WriteInteger(BaseAddress + &H88， Value)
            End Set
        End Property
        ''' <summary>
        ''' 子弹ID
        ''' </summary>
        Public ReadOnly Property Id As Integer
            Get
                Return Memory.ReadInteger(BaseAddress + &H90)
            End Get
        End Property
        ''' <summary>
        ''' 取得这是第几个子弹
        ''' </summary>
        Public ReadOnly Property Index As Integer
            Get
                Return Id And &HFFFF
            End Get
        End Property
        ''' <summary>
        ''' 让子弹通过火炬
        ''' </summary>
        Public Sub OnFire()
            Dim AsmCode As Byte() = {
                mov_ecx, 0, 0, 0, 0,
                call­, 2, 0, 0, 0,
                jmp, 6,
                pushdw, &HB0, &HEC, &H46, 0,
                ret,
                ret
            }
            ReplaceBytes(AsmCode, 1, BaseAddress)
            Memory.Execute(AsmCode)
        End Sub
        ''' <summary>
        ''' 子弹的事件循环,即一直会运行的代码
        ''' </summary>
        Class EventLoop
            Private Sub New()
            End Sub
            ''' <summary>
            ''' 初始化应用事件循环的汇编代码
            ''' </summary>
            ''' <param name="Asmcode">以8B 06开始(mov eax,[esi]),C3(ret)结束,其中eax即为所有子弹对象的指针</param>
            Public Shared Sub Initialize(ByVal Asmcode As Byte())
                Memory.WriteBytes(Variable + 200, Asmcode)
            End Sub
            ''' <summary>
            ''' 开始执行事件循环,一点要先初始化,并且一旦运行,直到调用<see cref="Stop­()"/>方法或者游戏结束为止一直有效,中途不可再调用<see cref="Initialize(Byte())"/>
            ''' </summary>
            Public Shared Sub Start()
                Memory.WriteByte(&H41314B, 232)
                Memory.WriteInteger(&H41314C, Variable + 200 - 4 - &H41314C)
                Memory.WriteByte(&H413167, 227)
            End Sub
            ''' <summary>
            ''' 结束事件循环,随时可以再次调用<see cref="Start()"/>
            ''' </summary>
            Public Shared Sub Stop­()
                Memory.WriteByte(&H41314B, 235)
                Memory.WriteInteger(&H41314C, 4820227)
                Memory.WriteByte(&H413167, 232)
            End Sub
        End Class
    End Class
    ''' <summary>
    ''' 掉落物类型枚举
    ''' </summary>
    Public Enum CoinType
        <Description("银币")> SilverDollar = 1
        <Description("金币")> GoldDollar
        <Description("钻石")> Diamond
        <Description("普通阳光")> NormalSun
        <Description("小阳光")> MiniSun
        <Description("大阳光")> LargeSun
        <Description("奖杯")> Trophy = 8
        <Description("纸条")> Note = 15
        <Description("植物卡片")> PlantCard
        <Description("植物礼盒")> PlantPresent
        <Description("钱袋")> MoneyBag
    End Enum
    ''' <summary>
    ''' 掉落物对象
    ''' </summary>
    Public Class Coin
        ''' <summary>
        ''' 对象指针
        ''' </summary>
        Private ReadOnly BaseAddress As Integer
        ''' <summary>
        ''' 新建一个掉落物对象,可以使用掉落物的序号或者对象指针
        ''' </summary>
        ''' <param name="IndexOrAddress">掉落物的序号或者对象指针</param>
        Public Sub New(ByVal IndexOrAddress As Integer)
            If IndexOrAddress > 1024 Then
                BaseAddress = IndexOrAddress
            Else
                BaseAddress = Memory.ReadInteger(PVZ.BaseAddress + &HE4) + IndexOrAddress * &HD8
            End If
        End Sub
        ''' <summary>
        ''' 掉落物图像的横坐标,若想修改真实坐标请修改X
        ''' </summary>
        Public ReadOnly Property ImageX As Integer
            Get
                Return Memory.ReadInteger(BaseAddress + 8)

            End Get
        End Property
        ''' <summary>
        ''' 掉落物图像的纵坐标,若想修改真实坐标请修改X
        ''' </summary>
        Public ReadOnly Property ImageY As Integer
            Get
                Return Memory.ReadInteger(BaseAddress + &HC)

            End Get
        End Property
        ''' <summary>
        ''' 设置或获取掉落物的碰撞宽度
        ''' </summary>
        Public Property CollisionWidth As Integer
            Get
                Return Memory.ReadInteger(BaseAddress + &H10)

            End Get
            Set(Value As Integer)
                Memory.WriteInteger(BaseAddress + &H10, Value)
            End Set
        End Property
        ''' <summary>
        ''' 设置或获取掉落物的碰撞高度
        ''' </summary>
        Public Property CollisionHeight As Integer
            Get
                Return Memory.ReadInteger(BaseAddress + &H14)

            End Get
            Set(Value As Integer)
                Memory.WriteInteger(BaseAddress + &H14, Value)
            End Set
        End Property
        ''' <summary>
        ''' 设置或获取掉落物是否可见
        ''' </summary>
        Public Property Visible As Boolean
            Get
                Return Memory.ReadInteger(BaseAddress + &H18)

            End Get
            Set(Value As Boolean)
                Memory.WriteInteger(BaseAddress + &H18, Convert.ToInt32(Value))
            End Set
        End Property
        ''' <summary>
        ''' 设置或获取掉落物的图层
        ''' </summary>
        Public Property Layer As Integer
            Get
                Return Memory.ReadInteger(BaseAddress + &H20)

            End Get
            Set(Value As Integer)
                Memory.WriteInteger(BaseAddress + &H20, Value)
            End Set
        End Property
        ''' <summary>
        ''' 设置或获取掉落物的横坐标
        ''' </summary>
        Public Property X As Single
            Get
                Return Memory.ReadFloat(BaseAddress + &H24)
            End Get
            Set(Value As Single)
                Memory.WriteFloat(BaseAddress + &H24, Value)
            End Set
        End Property
        ''' <summary>
        ''' 设置或获取掉落物的纵坐标
        ''' </summary>
        Public Property Y As Single
            Get
                Return Memory.ReadFloat(BaseAddress + &H28)
            End Get
            Set(Value As Single)
                Memory.WriteFloat(BaseAddress + &H28, Value)
            End Set
        End Property
        ''' <summary>
        ''' 设置或获取掉落物的大小
        ''' </summary>
        Public Property Size As Single
            Get
                Return Memory.ReadFloat(BaseAddress + &H34)
            End Get
            Set(Value As Single)
                Memory.WriteFloat(BaseAddress + &H34, Value)
            End Set
        End Property
        ''' <summary>
        ''' 设置或获取掉落物是否存在
        ''' </summary>
        Public Property Exist As Boolean
            Get
                Return Memory.ReadInteger(BaseAddress + &H38) = 0
            End Get
            Set(Value As Boolean)
                Memory.WriteInteger(BaseAddress + &H38, Convert.ToInt32(Not Value))
            End Set
        End Property
        ''' <summary>
        ''' 掉落物已存在的时间
        ''' </summary>
        Public ReadOnly Property ExistedTime As Integer
            Get
                Return Memory.ReadInteger(BaseAddress + &H4C)
            End Get
        End Property
        ''' <summary>
        ''' 设置或获取掉落物是否被收集
        ''' </summary>
        Public Property Collected As Boolean
            Get
                Return Memory.ReadInteger(BaseAddress + &H50)
            End Get
            Set(Value As Boolean)
                Memory.WriteInteger(BaseAddress + &H50, Convert.ToInt32(Value))
            End Set
        End Property
        ''' <summary>
        ''' 设置或获取掉落物的类型
        ''' </summary>
        Public Property Type As CoinType
            Get
                Return Memory.ReadInteger(BaseAddress + &H58)
            End Get
            Set(Value As CoinType)
                Memory.WriteInteger(BaseAddress + &H58, Value)
            End Set
        End Property
        ''' <summary>
        ''' 掉落物的运动状态枚举
        ''' </summary>
        Public Enum MotionType
            <Description("快速下落")> Fastfall
            <Description("缓慢下落")> Slowfall
            <Description("生产")> Product
            <Description("喷射")> Spray
            <Description("收集")> Collected
            <Description("延迟收集")> DelayCollected
        End Enum
        ''' <summary>
        ''' 设置或获取掉落物的运动状态
        ''' </summary>
        Public Property Motion As MotionType
            Get
                Return Memory.ReadInteger(BaseAddress + &H5C)
            End Get
            Set(Value As MotionType)
                Memory.WriteInteger(BaseAddress + &H5C, Value)
            End Set
        End Property
        ''' <summary>
        ''' 设置或获取卡片掉落物的卡片内容
        ''' </summary>
        Public Property CardType As CardType
            Get
                Return Memory.ReadInteger(BaseAddress + &H68)
            End Get
            Set(Value As CardType)
                Memory.WriteInteger(BaseAddress + &H68, Value)
            End Set
        End Property
        ''' <summary>
        ''' 设置或获取掉落物是否有光环
        ''' </summary>
        Public Property Halo As Boolean
            Get
                Return Memory.ReadByte(BaseAddress + &HC8)
            End Get
            Set(Value As Boolean)
                Memory.WriteByte(BaseAddress + &HC8, Convert.ToInt32(Value))
            End Set
        End Property
        ''' <summary>
        ''' 掉落物ID
        ''' </summary>
        Public ReadOnly Property Id As Integer
            Get
                Return Memory.ReadInteger(BaseAddress + &HD0)
            End Get
        End Property
        ''' <summary>
        ''' 取得这是第几个掉落物
        ''' </summary>
        Public ReadOnly Property Index As Integer
            Get
                Return Id And &HFFFF
            End Get
        End Property
        Public Sub Collect()
            If Not Type = CoinType.PlantCard Then
                Dim Asmcode As Byte() = {
                    mov_ecx, 0, 0, 0, 0,
                    call­, 2, 0, 0, 0,
                    jmp, 6,
                    pushdw, &H60, &H20, &H43, 0,
                    ret,
                    ret
                }
                ReplaceBytes(Asmcode, 1, BaseAddress)
                Memory.Execute(Asmcode)
            End If
        End Sub
    End Class
    ''' <summary>
    ''' 小推车类型枚举
    ''' </summary>
    Public Enum LawnmowerType
        <Description("草地小推车")> LawnCleaner
        <Description("池塘清洁车")> PoolCleaner
        <Description("屋顶小推车")> RoofCleaner
        <Description("机车小推车")> Trickedout
    End Enum
    ''' <summary>
    ''' 小推车对象
    ''' </summary>
    Public Class Lawnmover
        ''' <summary>
        ''' 对象指针
        ''' </summary>
        Public BaseAddress As Integer
        ''' <summary>
        ''' 新建一个小推车对象,可以使用小推车的序号或者对象指针
        ''' </summary>
        ''' <param name="IndexOrAddress">小推车的序号或者对象指针</param>
        Public Sub New(ByVal IndexOrAddress As Integer)
            If IndexOrAddress > 1024 Then
                BaseAddress = IndexOrAddress
            Else
                BaseAddress = Memory.ReadInteger(PVZ.BaseAddress + &H100) + IndexOrAddress * &H48
            End If
        End Sub
        ''' <summary>
        ''' 设置或获取小推车的横坐标
        ''' </summary>
        Public Property X As Single
            Get
                Return Memory.ReadFloat(BaseAddress + &H8)
            End Get
            Set(Value As Single)
                Memory.WriteFloat(BaseAddress + &H8, Value)
            End Set
        End Property
        ''' <summary>
        ''' 设置或获取小推车的纵坐标
        ''' </summary>
        Public Property Y As Single
            Get
                Return Memory.ReadFloat(BaseAddress + &HC)
            End Get
            Set(Value As Single)
                Memory.WriteFloat(BaseAddress + &HC, Value)
            End Set
        End Property
        ''' <summary>
        ''' 设置或获取小推车的图层
        ''' </summary>
        Public Property Layer As Integer
            Get
                Return Memory.ReadInteger(BaseAddress + &H10)

            End Get
            Set(Value As Integer)
                Memory.WriteInteger(BaseAddress + &H10, Value)
            End Set
        End Property
        ''' <summary>
        ''' 设置或获取小推车所在行
        ''' </summary>
        Public Property Row As Integer
            Get
                Return Memory.ReadInteger(BaseAddress + &H14)

            End Get
            Set(Value As Integer)
                Memory.WriteInteger(BaseAddress + &H14, Value)
            End Set
        End Property
        ''' <summary>
        ''' 设置或获取小推车状态
        ''' </summary>
        Public Property State As Integer
            Get
                Return Memory.ReadInteger(BaseAddress + &H2C)
            End Get
            Set(Value As Integer)
                Memory.WriteInteger(BaseAddress + &H2C, Value)
            End Set
        End Property
        ''' <summary>
        ''' 设置或获取小推车是否存在
        ''' </summary>
        Public Property Exist As Boolean
            Get
                Return Memory.ReadByte(BaseAddress + &H30) = 0
            End Get
            Set(Value As Boolean)
                Memory.WriteByte(BaseAddress + &H30, Convert.ToInt32(Not Value))
            End Set
        End Property
        ''' <summary>
        ''' 设置或获取小推车是否可见
        ''' </summary>
        Public Property Visible As Boolean
            Get
                Return Memory.ReadByte(BaseAddress + &H31)
            End Get
            Set(Value As Boolean)
                Memory.WriteByte(BaseAddress + &H31, Convert.ToInt32(Value))
            End Set
        End Property
        ''' <summary>
        ''' 设置或获取小推车类型
        ''' </summary>
        Public Property Type As LawnmowerType
            Get
                Return Memory.ReadInteger(BaseAddress + &H34)
            End Get
            Set(Value As LawnmowerType)
                Memory.WriteInteger(BaseAddress + &H34, Value)
            End Set
        End Property
        ''' <summary>
        ''' 设置或获取小推车纵坐标偏移
        ''' </summary>
        Public Property YOffset As Single
            Get
                Return Memory.ReadFloat(BaseAddress + &H38)
            End Get
            Set(Value As Single)
                Memory.WriteFloat(BaseAddress + &H38, Value)
            End Set
        End Property
        ''' <summary>
        ''' 小推车ID
        ''' </summary>
        Public ReadOnly Property Id As Integer
            Get
                Return Memory.ReadInteger(BaseAddress + &H44)
            End Get
        End Property
        ''' <summary>
        ''' 取得这是第几个小推车
        ''' </summary>
        Public ReadOnly Property Index As Integer
            Get
                Return Id And &HFFFF
            End Get
        End Property
    End Class
    ''' <summary>
    ''' 场地物品枚举
    ''' </summary>
    Public Enum GriditemType
        <Description("墓碑")> Grave = 1
        <Description("弹坑")> Crater
        <Description("梯子")> Ladder
        <Description("黑色传送门")> PortalBlue
        <Description("黄色传送门")> PortalYellow
        <Description("罐子")> Vase = 7
        <Description("钉耙")> Rake = 11
        <Description("大脑")> Brain
    End Enum
    ''' <summary>
    ''' 场地物品对象
    ''' </summary>
    Public Class Griditem
        ''' <summary>
        ''' 对象指针
        ''' </summary>
        Public BaseAddress As Integer
        ''' <summary>
        ''' 新建一个场地物品对象,可以使用场地物品的序号或者对象指针
        ''' </summary>
        ''' <param name="IndexOrAddress">墓碑的序号或者对象指针</param>
        Public Sub New(ByVal IndexOrAddress As Integer)
            If IndexOrAddress > 1024 Then
                BaseAddress = IndexOrAddress
            Else
                BaseAddress = Memory.ReadInteger(PVZ.BaseAddress + &H11C) + IndexOrAddress * &HEC
            End If
        End Sub
        ''' <summary>
        ''' 设置或获取场地物品的类型
        ''' </summary>
        Public Property Type As GriditemType
            Get
                Return Memory.ReadInteger(BaseAddress + &H8)
            End Get
            Set(Value As GriditemType)
                Memory.WriteInteger(BaseAddress + &H8, Value)
            End Set
        End Property
        ''' <summary>
        ''' 设置或获取场地物品所在列
        ''' </summary>
        Public Property Column As Integer
            Get
                Return Memory.ReadInteger(BaseAddress + &H10)

            End Get
            Set(Value As Integer)
                Memory.WriteInteger(BaseAddress + &H10, Value)
            End Set
        End Property
        ''' <summary>
        ''' 设置或获取场地物品所在行
        ''' </summary>
        Public Property Row As Integer
            Get
                Return Memory.ReadInteger(BaseAddress + &H14)
            End Get
            Set(Value As Integer)
                Memory.WriteInteger(BaseAddress + &H14, Value)
            End Set
        End Property
        ''' <summary>
        ''' 设置或获取场地物品的图层
        ''' </summary>
        Public Property Layer As Integer
            Get
                Return Memory.ReadInteger(BaseAddress + &H1C)

            End Get
            Set(Value As Integer)
                Memory.WriteInteger(BaseAddress + &H1C, Value)
            End Set
        End Property
        ''' <summary>
        ''' 设置或获取场地物品是否存在
        ''' </summary>
        Public Property Exist As Boolean
            Get
                Return Memory.ReadInteger(BaseAddress + &H20) = 0
            End Get
            Set(Value As Boolean)
                Memory.WriteInteger(BaseAddress + &H20, Convert.ToInt32(Not Value))
            End Set
        End Property
        ''' <summary>
        ''' 场地物品ID
        ''' </summary>
        Public ReadOnly Property Id As Integer
            Get
                Return Memory.ReadInteger(BaseAddress + &HE8)
            End Get
        End Property
        ''' <summary>
        ''' 取得这是第几个场地物品
        ''' </summary>
        Public ReadOnly Property Index As Integer
            Get
                Return Id And &HFFFF
            End Get
        End Property
    End Class
    ''' <summary>
    ''' 墓碑对象
    ''' </summary>
    Public Class Grave
        Inherits Griditem
        ''' <summary>
        ''' 新建一个墓碑对象,可以使用墓碑的序号或者对象指针
        ''' </summary>
        ''' <param name="IndexOrAddress">墓碑的序号或者对象指针</param>
        Public Sub New(ByVal IndexOrAddress As Integer)
            MyBase.New(IndexOrAddress)
        End Sub
        ''' <summary>
        ''' 设置或获取墓碑露出地面的百分比
        ''' </summary>
        Public Property AppearedValue As Integer
            Get
                Return Memory.ReadInteger(BaseAddress + &H18)

            End Get
            Set(Value As Integer)
                Memory.WriteInteger(BaseAddress + &H18, Value)
            End Set
        End Property
    End Class
    ''' <summary>
    ''' 弹坑对象
    ''' </summary>
    Public Class Crater
        Inherits Griditem
        ''' <summary>
        ''' 新建一个弹坑对象,可以使用弹坑的序号或者对象指针
        ''' </summary>
        ''' <param name="IndexOrAddress">弹坑的序号或者对象指针</param>
        Public Sub New(ByVal IndexOrAddress As Integer)
            MyBase.New(IndexOrAddress)
        End Sub
        ''' <summary>
        ''' 设置或获取弹坑消失倒计时
        ''' </summary>
        Public Property DisappearCountdown As Integer
            Get
                Return Memory.ReadInteger(BaseAddress + &H18)
            End Get
            Set(Value As Integer)
                Memory.WriteInteger(BaseAddress + &H18, Value)
            End Set
        End Property
    End Class
    ''' <summary>
    ''' 脑子对象
    ''' </summary>
    Public Class Brain
        Inherits Griditem
        ''' <summary>
        ''' 新建一个脑子对象,可以使用脑子的序号或者对象指针
        ''' </summary>
        ''' <param name="IndexOrAddress">脑子的序号或者对象指针</param>
        Public Sub New(ByVal IndexOrAddress As Integer)
            MyBase.New(IndexOrAddress)
        End Sub
        ''' <summary>
        ''' 设置或获取脑子血量
        ''' </summary>
        Public Property Hp As Integer
            Get
                Return Memory.ReadInteger(BaseAddress + &H18)
            End Get
            Set(Value As Integer)
                Memory.WriteInteger(BaseAddress + &H18, Value)
            End Set
        End Property
        ''' <summary>
        ''' 设置或获取脑子纵坐标
        ''' </summary>
        Public Property Y As Single
            Get
                Return Memory.ReadFloat(BaseAddress + &H28)
            End Get
            Set(Value As Single)
                Memory.WriteFloat(BaseAddress + &H28, Value)
            End Set
        End Property
    End Class
    ''' <summary>
    ''' 花瓶皮肤样式枚举
    ''' </summary>
    Public Enum VaseSkin
        <Description("未知")> Unknow = 3
        <Description("绿叶")> Leaf
        <Description("僵尸")> Zombie
    End Enum
    ''' <summary>
    ''' 花瓶内容类型枚举
    ''' </summary>
    Public Enum VaseContent
        <Description("空罐")> None
        <Description("植物")> Plant
        <Description("僵尸")> Zombie
        <Description("阳光")> Sun
    End Enum
    ''' <summary>
    ''' 花瓶对象
    ''' </summary>
    Public Class Vase
        Inherits Griditem
        ''' <summary>
        ''' 新建一个花瓶对象,可以使用花瓶的序号或者对象指针
        ''' </summary>
        ''' <param name="IndexOrAddress">脑子的序号或者对象指针</param>
        Public Sub New(ByVal IndexOrAddress As Integer)
            MyBase.New(IndexOrAddress)
        End Sub
        ''' <summary>
        ''' 设置或获取花瓶的皮肤样式
        ''' </summary>
        Public Property Skin As VaseSkin
            Get
                Return Memory.ReadInteger(BaseAddress + &HC)
            End Get
            Set(Value As VaseSkin)
                Memory.WriteInteger(BaseAddress + &HC, Value)
            End Set
        End Property
        ''' <summary>
        ''' 设置或获取僵尸花瓶的僵尸类型
        ''' </summary>
        Public Property Zombie As ZombieType
            Get
                Return Memory.ReadInteger(BaseAddress + &H3C)

            End Get
            Set(Value As ZombieType)
                Memory.WriteInteger(BaseAddress + &H3C, Value)
            End Set
        End Property
        ''' <summary>
        ''' 设置或获取植物花瓶的植物类型
        ''' </summary>
        Public Property Plant As PlantType
            Get
                Return Memory.ReadInteger(BaseAddress + &H40)

            End Get
            Set(Value As PlantType)
                Memory.WriteInteger(BaseAddress + &H40, Value)
            End Set
        End Property
        ''' <summary>
        ''' 设置或获取花瓶的内容类型
        ''' </summary>
        Public Property Content As VaseContent
            Get
                Return Memory.ReadInteger(BaseAddress + &H44)
            End Get
            Set(Value As VaseContent)
                Memory.WriteInteger(BaseAddress + &H44, Value)
            End Set
        End Property
        ''' <summary>
        ''' 鼠标是否悬停在花瓶上
        ''' </summary>
        Public ReadOnly Property MouseEnter As Boolean
            Get
                Return Memory.ReadInteger(BaseAddress + &H48)
            End Get
        End Property
        ''' <summary>
        ''' 设置或获取花瓶的透视倒计时
        ''' </summary>
        Public Property TransparentCountDown As Integer
            Get
                Return Memory.ReadInteger(BaseAddress + &H4C)
            End Get
            Set(Value As Integer)
                Memory.WriteInteger(BaseAddress + &H4C, Value)
            End Set
        End Property
        ''' <summary>
        ''' 设置或获取阳光花瓶的阳光个数
        ''' </summary>
        Public Property Sun As Integer
            Get
                Return Memory.ReadInteger(BaseAddress + &H50)
            End Get
            Set(Value As Integer)
                Memory.WriteInteger(BaseAddress + &H50, Value)
            End Set
        End Property
    End Class
    ''' <summary>
    ''' 鼠标指针类型枚举
    ''' </summary>
    Public Enum MouseType
        None
        Card
        Shovel = 6
        Crosshair = 8
        Watering
        GoldenWatering
        Fertilizer
        BugSpray
        Phonograph
        Chocolate
        GardeningGlove
        Sell
        WheelBarrow
        TreeFood
    End Enum
    ''' <summary>
    ''' 鼠标对象(游戏内的鼠标对象)
    ''' </summary>
    Public Class MousePointer
        ''' <summary>
        ''' 鼠标对象的指针
        ''' </summary>
        Public Shared ReadOnly Property BaseAddress As Integer
            Get
                Return Memory.ReadInteger(PVZ.BaseAddress + &H138)
            End Get
        End Property
        ''' <summary>
        ''' 设置或获取鼠标当前拿的卡片的索引
        ''' </summary>
        Public Shared Property Index As Integer
            Get
                Return Memory.ReadInteger(BaseAddress + &H24)
            End Get
            Set(Value As Integer)
                Memory.WriteInteger(BaseAddress + &H24, Value)
            End Set
        End Property
        ''' <summary>
        ''' 设置或获取鼠标当前拿的卡片的类型
        ''' </summary>
        Public Shared Property CardType As CardType
            Get
                Return Memory.ReadInteger(BaseAddress + &H28)
            End Get
            Set(Value As CardType)
                Memory.WriteInteger(BaseAddress + &H28, Value)
            End Set
        End Property
        ''' <summary>
        ''' 设置或获取鼠标当前拿的卡片是否为模仿者
        ''' </summary>
        Public Shared Property ImitativeCardType As PlantType
            Get
                Return Memory.ReadInteger(BaseAddress + &H2C)
            End Get
            Set(Value As PlantType)
                Memory.WriteInteger(BaseAddress + &H2C, Value)
            End Set
        End Property
        ''' <summary>
        ''' 设置或获取鼠标类型
        ''' </summary>
        Public Shared Property Type As MouseType
            Get
                Return Memory.ReadInteger(BaseAddress + &H30)
            End Get
            Set(Value As MouseType)
                Memory.WriteInteger(BaseAddress + &H30, Value)
            End Set
        End Property
        ''' <summary>
        ''' 鼠标所在行
        ''' </summary>
        Public Shared ReadOnly Property Row As Integer
            Get
                Return Memory.GetAddress(PVZ.BaseAddress + &H13C, &H28)
            End Get
        End Property
        ''' <summary>
        ''' 鼠标所在列
        ''' </summary>
        Public Shared ReadOnly Property Column As Integer
            Get
                Return Memory.GetAddress(PVZ.BaseAddress + &H13C, &H24)
            End Get
        End Property
    End Class
    ''' <summary>
    ''' 文本字幕的样式枚举
    ''' </summary>
    Public Enum CaptionStyle
        <Description("中下位")> Lowermiddle = 1
        <Description("中下部分")> Lowerpart = 3
        <Description("底部")> Bottom = 6
        <Description("中间")> Center = 12
        <Description("底部白色")> BottomWhite = 14
        <Description("中间红字")> CenterRed
        <Description("顶部黄色")> TopYellow
    End Enum
    ''' <summary>
    ''' 文本字幕对象
    ''' </summary>
    Public Class Caption
        ''' <summary>
        ''' 对象指针
        ''' </summary>
        Public Shared ReadOnly Property BaseAddress As Integer
            Get
                Return Memory.ReadInteger(PVZ.BaseAddress + &H140)
            End Get
        End Property
        ''' <summary>
        ''' 文本字幕的文本内容
        ''' </summary>
        Public Shared Property Text As String
            Get
                Return Memory.ReadString(BaseAddress + 4, &H80)
            End Get
            Set(Value As String)
                Memory.WriteString(BaseAddress + 4, Value)
            End Set
        End Property
        ''' <summary>
        ''' 设置或获取文本字幕的消失倒计时
        ''' </summary>
        Public Shared Property DisappearCountdown As Integer
            Get
                Return Memory.ReadInteger(BaseAddress + &H88)
            End Get
            Set(Value As Integer)
                Memory.WriteInteger(BaseAddress + &H88, Value)
            End Set
        End Property
        ''' <summary>
        ''' 文本字幕的样式
        ''' </summary>
        Public Shared Property Style As CaptionStyle
            Get
                Return Memory.ReadInteger(BaseAddress + &H8C)
            End Get
            Set(Value As CaptionStyle)
                Memory.WriteInteger(BaseAddress + &H8C, Value)
            End Set
        End Property
    End Class
    ''' <summary>
    ''' 卡片类型枚举
    ''' </summary>
    Public Enum CardType
        <Description("豌豆射手")> Peashooter
        <Description("向日葵")> Sunflower
        <Description("樱桃炸弹")> CherryBomb
        <Description("坚果墙")> Wallnut
        <Description("土豆雷")> PotatoMine
        <Description("寒冰射手")> SnowPea
        <Description("大嘴花")> Chomper
        <Description("双发射手")> Repeater
        <Description("小喷菇")> Puffshroom
        <Description("阳光菇")> Sunshroom
        <Description("大喷菇")> Fumeshroom
        <Description("墓碑吞噬者")> GraveBuster
        <Description("魅惑菇")> Hypnoshroom
        <Description("胆小菇")> Scaredyshroom
        <Description("寒冰菇")> Iceshroom
        <Description("毁灭菇")> Doomshroom
        <Description("睡莲")> LilyPad
        <Description("倭瓜")> Squash
        <Description("三线射手")> Threepeater
        <Description("缠绕海草")> TangleKelp
        <Description("火爆辣椒")> Jalapeno
        <Description("地刺")> Caltrop
        <Description("火炬树桩")> Torchwood
        <Description("高坚果")> Tallnut
        <Description("海蘑菇")> Seashroom
        <Description("路灯花")> Plantern
        <Description("仙人掌")> Cactus
        <Description("三叶草")> Blover
        <Description("裂荚射手")> SplitPea
        <Description("杨桃")> Starfruit
        <Description("南瓜头")> Pumpkin
        <Description("磁力菇")> Magnetshroom
        <Description("卷心菜投手")> Cabbagepult
        <Description("花盆")> Pot
        <Description("玉米投手")> Cornpult
        <Description("咖啡豆")> CoffeeBean
        <Description("大蒜")> Garlic
        <Description("叶子保护伞")> UmbrellaLeaf
        <Description("金盏花")> Marigold
        <Description("西瓜投手")> Melonpult
        <Description("机枪射手")> GatlingPea
        <Description("双子向日葵")> TwinSunflower
        <Description("忧郁蘑菇")> Gloomshroom
        <Description("香蒲")> Cattail
        <Description("冰瓜")> WinterMelon
        <Description("吸金磁")> GoldMagnet
        <Description("地刺王")> Spikerock
        <Description("玉米加农炮")> CobCannon
        <Description("模仿者")> Imitater
        <Description("爆炸坚果")> Explodenut
        <Description("巨大坚果")> GiantWallnut
        <Description("幼苗")> Sprout
        <Description("左向双发射手")> LeftRepeater
        _____________
        <Description("刷新卡")> Refresh
        <Description("弹坑卡")> Crater
        <Description("阳光卡")> Sun
        <Description("钻石卡")> Diamond
        <Description("潜水僵尸")> SnorkedZombie
        <Description("奖杯卡")> Trophy
        <Description("普通僵尸")> Zombie
        <Description("路障僵尸")> ConeheadZombie
        <Description("撑杆跳僵尸")> PoleVaultingZombie
        <Description("铁桶僵尸")> BucketheadZombie
        <Description("扶梯僵尸")> LadderZombie
        <Description("矿工僵尸")> DiggerZombie
        <Description("蹦极僵尸")> BungeeZombie
        <Description("橄榄球僵尸")> FootballZombie
        <Description("气球僵尸")> BalloonZombie
        <Description("铁栅门僵尸")> ScreenDoorZombie
        <Description("冰车僵尸")> Zomboin
        <Description("跳跳僵尸")> PogoZombie
        <Description("舞王僵尸")> DancingZombie
        <Description("伽刚特尔")> Gargantuar
        <Description("小鬼僵尸")> Imp
    End Enum
    ''' <summary>
    ''' 卡槽对象
    ''' </summary>
    Public Class CardSlot
        ''' <summary>
        ''' 对象指针
        ''' </summary>
        Public Shared ReadOnly Property BaseAddress As Integer
            Get
                Return Memory.ReadInteger(PVZ.BaseAddress + &H144)
            End Get
        End Property
        ''' <summary>
        ''' 设置或获取卡槽整体的横坐标
        ''' </summary>
        Public Shared Property X As Integer
            Get
                Return Memory.ReadInteger(BaseAddress + 8)
            End Get
            Set(Value As Integer)
                Memory.WriteInteger(BaseAddress + 8, Value)
            End Set
        End Property
        ''' <summary>
        ''' 设置或获取卡槽整体的纵坐标
        ''' </summary>
        Public Shared Property Y As Integer
            Get
                Return Memory.ReadInteger(BaseAddress + &HC)
            End Get
            Set(Value As Integer)
                Memory.WriteInteger(BaseAddress + &HC, Value)
            End Set
        End Property
        ''' <summary>
        ''' 设置或获取卡槽整体的判定长度
        ''' </summary>
        Public Shared Property CollisionLength As Integer
            Get
                Return Memory.ReadInteger(BaseAddress + &H10)
            End Get
            Set(Value As Integer)
                Memory.WriteInteger(BaseAddress + &H10, Value)
            End Set
        End Property
        ''' <summary>
        ''' 设置或获取卡槽整体是否可见
        ''' </summary>
        Public Shared Property Visible As Boolean
            Get
                Return Memory.ReadInteger(BaseAddress + &H18)
            End Get
            Set(Value As Boolean)
                Memory.WriteInteger(BaseAddress + &H18, Convert.ToInt32(Value))
            End Set
        End Property
        ''' <summary>
        ''' 获取卡槽的卡片数量
        ''' </summary>
        Public Shared ReadOnly Property CardNum As Integer
            Get
                Return Memory.ReadInteger(BaseAddress + &H24)
            End Get
        End Property
        ''' <summary>
        ''' 设置卡槽格数
        ''' </summary>
        ''' <param name="Num">卡槽格数</param>
        Public Shared Sub SetCardNum(ByVal Num As Integer)
            If MainObjectExist Then
                Dim AsmCode As Byte() = {
                    mov_eax, 0, 0, 0, 0,
                    mov_esi, 0, 0, 0, 0,
                    call­, 2, 0, 0, 0,
                    jmp, 6,
                    pushdw, &HD0, &H9C, &H48, 0,
                    ret,
                    ret
                }
                ReplaceBytes(AsmCode, 1, Num)
                ReplaceBytes(AsmCode, 6, BaseAddress)
                Memory.WriteByte(&H41BEE0, ret)
                Memory.Execute(AsmCode)
                Memory.WriteByte(&H41BEE0, 86)
            End If
        End Sub
        ''' <summary>
        ''' 取到一个卡片对象
        ''' </summary>
        ''' <param name="Num">要取的卡片的索引(从1开始)</param>
        ''' <returns></returns>
        Public Shared Function GetCard(ByVal Num As Integer) As SeedCard
            If Num >= 0 And Num < 10 Then
                Return New SeedCard(BaseAddress + &H24 + Num * &H50)
            End If
            Return Nothing
        End Function
        ''' <summary>
        ''' 卡片对象
        ''' </summary>
        Public Class SeedCard
            ''' <summary>
            ''' 对象指针
            ''' </summary>
            Public BaseAddress As Integer
            ''' <summary>
            ''' 通过卡片对象的指针,新建一个卡片对象
            ''' </summary>
            ''' <param name="Address">卡片对象的指针</param>
            Public Sub New(ByVal Address As Integer)
                BaseAddress = Address
            End Sub
            ''' <summary>
            ''' 设置或获取卡片的横坐标
            ''' </summary>
            Public Property X As Integer
                Get
                    Return Memory.ReadInteger(BaseAddress + &HC)

                End Get
                Set(Value As Integer)
                    Memory.WriteInteger(BaseAddress + &HC, Value)
                End Set
            End Property
            ''' <summary>
            ''' 设置或获取卡片的纵坐标
            ''' </summary>
            Public Property Y As Integer
                Get
                    Return Memory.ReadInteger(BaseAddress + &H10)

                End Get
                Set(Value As Integer)
                    Memory.WriteInteger(BaseAddress + &H10, Value)
                End Set
            End Property
            ''' <summary>
            ''' 设置或获取卡片的判定宽度
            ''' </summary>
            Public Property CollisionLength As Integer
                Get
                    Return Memory.ReadInteger(BaseAddress + &H14)
                End Get
                Set(Value As Integer)
                    Memory.WriteInteger(BaseAddress + &H14, Value)
                End Set
            End Property
            ''' <summary>
            ''' 设置或获取卡片的判定高度
            ''' </summary>
            Public Property CollisionHeight As Integer
                Get
                    Return Memory.ReadInteger(BaseAddress + &H18)
                End Get
                Set(Value As Integer)
                    Memory.WriteInteger(BaseAddress + &H18, Value)
                End Set
            End Property
            ''' <summary>
            ''' 设置或获取卡片是否可见
            ''' </summary>
            Public Property Visible As Boolean
                Get
                    Return Memory.ReadInteger(BaseAddress + &H1C)

                End Get
                Set(Value As Boolean)
                    Memory.WriteInteger(BaseAddress + &H1C, Convert.ToInt32(Value))
                End Set
            End Property
            ''' <summary>
            ''' 设置或获取卡片的当前冷却时间
            ''' </summary>
            Public Property CoolDown As Integer
                Get
                    Return Memory.ReadInteger(BaseAddress + &H28)
                End Get
                Set(Value As Integer)
                    Memory.WriteInteger(BaseAddress + &H28, Value)
                End Set
            End Property
            ''' <summary>
            ''' 设置或获取卡片的冷却时间
            ''' </summary>
            Public Property CoolDownInterval As Integer
                Get
                    Return Memory.ReadInteger(BaseAddress + &H2C)
                End Get
                Set(Value As Integer)
                    Memory.WriteInteger(BaseAddress + &H2C, Value)
                End Set
            End Property
            ''' <summary>
            ''' 设置或获取卡片在卡槽中的格数
            ''' </summary>
            Public ReadOnly Property Index As Integer
                Get
                    Return Memory.ReadInteger(BaseAddress + &H30)
                End Get
            End Property
            ''' <summary>
            ''' 设置或获取卡片在传送带中的横坐标
            ''' </summary>
            Public Property ConveyorBeltX As Integer
                Get
                    Return Memory.ReadInteger(BaseAddress + &H34)
                End Get
                Set(Value As Integer)
                    Memory.WriteInteger(BaseAddress + &H34, Value)
                End Set
            End Property
            ''' <summary>
            ''' 设置或获取卡片的类型
            ''' </summary>
            Public Property CardType As CardType
                Get
                    Return Memory.ReadInteger(BaseAddress + &H38)
                End Get
                Set(Value As CardType)
                    Memory.WriteInteger(BaseAddress + &H38, Value)
                End Set
            End Property
            ''' <summary>
            ''' 设置或获取模仿者植物卡片的类型
            ''' </summary>
            Public Property ImitativeCardType As PlantType
                Get
                    Return Memory.ReadInteger(BaseAddress + &H3C)
                End Get
                Set(Value As PlantType)
                    Memory.WriteInteger(BaseAddress + &H3C, Value)
                End Set
            End Property
            ''' <summary>
            ''' 设置或获取老虎机卡片停止倒计时
            ''' </summary>
            Public Property SlotCountdown As Integer
                Get
                    Return Memory.ReadInteger(BaseAddress + &H40)
                End Get
                Set(Value As Integer)
                    Memory.WriteInteger(BaseAddress + &H40, Value)
                End Set
            End Property
            ''' <summary>
            ''' 设置或获取老虎机的卡片类型
            ''' </summary>
            Public Property SlotType As CardType
                Get
                    Return Memory.ReadInteger(BaseAddress + &H44)
                End Get
                Set(Value As CardType)
                    Memory.WriteInteger(BaseAddress + &H44, Value)
                End Set
            End Property
            ''' <summary>
            ''' 设置或获取老虎机卡片的位置
            ''' </summary>
            Public Property SlotPosition As Single
                Get
                    Return Memory.ReadFloat(BaseAddress + &H48)
                End Get
                Set(Value As Single)
                    Memory.WriteFloat(BaseAddress + &H48, Value)
                End Set
            End Property
            ''' <summary>
            ''' 设置或获取卡片是否可用
            ''' </summary>
            Public Property Enable As Boolean
                Get
                    Return Memory.ReadByte(BaseAddress + &H4C)
                End Get
                Set(Value As Boolean)
                    Memory.WriteByte(BaseAddress + &H4C, Convert.ToInt32(Value))
                End Set
            End Property
            ''' <summary>
            ''' 设置卡片可用
            ''' </summary>
            Public Property Active As Boolean
                Get
                    Return Memory.ReadByte(BaseAddress + &H4D)
                End Get
                Set(Value As Boolean)
                    Memory.WriteByte(BaseAddress + &H4D, Convert.ToInt32(Value))
                End Set
            End Property
            ''' <summary>
            ''' 卡片的使用次数
            ''' </summary>
            Public ReadOnly Property UsageCount As Integer
                Get
                    Return Memory.ReadInteger(BaseAddress + &H50)
                End Get
            End Property
        End Class
    End Class
    ''' <summary>
    ''' 杂项(宝石迷阵)对象
    ''' </summary>
    Public Class Miscellaneous
        ''' <summary>
        ''' 对象指针
        ''' </summary>
        Public Shared ReadOnly Property BaseAddress As Integer
            Get
                Return Memory.ReadInteger(PVZ.BaseAddress + &H160)
            End Get
        End Property
        ''' <summary>
        ''' 是否在拖动植物
        ''' </summary>
        Public Shared ReadOnly Property DragingPlant As Boolean
            Get
                Return Memory.ReadByte(BaseAddress + 8)
            End Get
        End Property
        ''' <summary>
        ''' 拖动的横坐标
        ''' </summary>
        Public Shared ReadOnly Property DragingX As Integer
            Get
                Return Memory.ReadInteger(BaseAddress + &HC)
            End Get
        End Property
        ''' <summary>
        ''' 拖动的纵坐标
        ''' </summary>
        Public Shared ReadOnly Property DragingY As Integer
            Get
                Return Memory.ReadInteger(BaseAddress + &H10)
            End Get
        End Property
        ''' <summary>
        ''' 指定坐标的位置是否有有弹坑
        ''' </summary>
        ''' <param name="Row">行</param>
        ''' <param name="Colnum">列</param>
        ''' <returns></returns>
        Public Shared Function HaveCrater(ByVal Row As Integer, ByVal Colnum As Integer) As Boolean
            If Row >= 0 And Row < 6 And Colnum >= 0 And Colnum < 9 Then
                Return Memory.ReadByte(BaseAddress + &H14 + 6 * Colnum + Row)
            End If
            Return False
        End Function
        ''' <summary>
        ''' 设置/取消指定坐标弹坑
        ''' </summary>
        ''' <param name="Row">行</param>
        ''' <param name="Colnum">列</param>
        ''' <param name="Switch">是否设置弹坑,默认为设置弹坑</param>
        Public Shared Sub SetCrater(ByVal Row As Integer, ByVal Colnum As Integer, Optional Switch As Boolean = True)
            If Row >= 0 And Row < 6 And Colnum >= 0 And Colnum < 9 Then
                Memory.WriteByte(BaseAddress + &H14 + 6 * Colnum + Row, Convert.ToInt32(Switch))
            End If
        End Sub
        ''' <summary>
        ''' 设置或获取是否升级了双重射手
        ''' </summary>
        Public Shared Property UpgradedRepeater As Boolean
            Get
                Return Memory.ReadByte(BaseAddress + &H4A)
            End Get
            Set(Value As Boolean)
                Memory.WriteByte(BaseAddress + &H4A, Convert.ToInt32(Value))
            End Set
        End Property
        ''' <summary>
        ''' 设置或获取是否升级了大喷菇
        ''' </summary>
        Public Shared Property UpgradedFumeshroon As Boolean
            Get
                Return Memory.ReadByte(BaseAddress + &H4B)
            End Get
            Set(Value As Boolean)
                Memory.WriteByte(BaseAddress + &H4B, Convert.ToInt32(Value))
            End Set
        End Property
        ''' <summary>
        ''' 设置或获取是否升级了高坚果
        ''' </summary>
        Public Shared Property UpgradedTallnut As Boolean
            Get
                Return Memory.ReadByte(BaseAddress + &H4C)
            End Get
            Set(Value As Boolean)
                Memory.WriteByte(BaseAddress + &H4C, Convert.ToInt32(Value))
            End Set
        End Property
        ''' <summary>
        ''' 设置或获取关卡属性倒计时
        ''' </summary>
        Public Shared Property AttributeCountdown As Integer
            Get
                Return Memory.ReadInteger(BaseAddress + &H58)
            End Get
            Set(Value As Integer)
                Memory.WriteInteger(BaseAddress + &H58, Value)
            End Set
        End Property
        ''' <summary>
        ''' 设置或获取关卡进程
        ''' </summary>
        Public Shared Property LevelProcess As Integer
            Get
                Return Memory.ReadInteger(BaseAddress + &H60)
            End Get
            Set(Value As Integer)
                Memory.WriteInteger(BaseAddress + &H60, Value)
            End Set
        End Property
        ''' <summary>
        ''' 设置或获取当前关卡轮数
        ''' </summary>
        Public Shared Property Round As Integer
            Get
                Return Memory.ReadInteger(BaseAddress + &H6C)
            End Get
            Set(Value As Integer)
                Memory.WriteInteger(BaseAddress + &H6C, Value)
            End Set
        End Property
    End Class
    ''' <summary>
    ''' 动画对象修改项枚举
    ''' </summary>
    Public Enum AnimationModify
        ZombieSpeed = 1
        ZombieColor = 2
        ZombieScale = 4
        PlantColor = 8
        PlantScale = 16
    End Enum
    Public Class Animation
        ''' <summary>
        ''' 对象指针
        ''' </summary>
        Private ReadOnly BaseAddress As Integer
        ''' <summary>
        ''' 新建一个动画对象,可以使用动画的ID或者对象指针
        ''' </summary>
        ''' <param name="AnimationIdOrAddress">动画的ID或者对象指针</param>
        Public Sub New(ByVal AnimationIdOrAddress As Integer)
            If AnimationIdOrAddress > 1024 Then
                BaseAddress = AnimationIdOrAddress
            Else
                BaseAddress = Memory.GetAddress(&H6A9EC0, &H820, 8, 0) + AnimationIdOrAddress * &HA0
            End If
        End Sub
        ''' <summary>
        ''' 设置或获取当前动画的图像,游戏从0到1变化,不断循环
        ''' </summary>
        Public Property CycleRate As Single
            Get
                Return Memory.ReadFloat(BaseAddress + 4)
            End Get
            Set(Value As Single)
                Memory.WriteFloat(BaseAddress + 4, Value)
            End Set
        End Property
        ''' <summary>
        ''' 设置或获取动画的速度
        ''' </summary>
        Public Property Speed As Single
            Get
                Return Memory.ReadFloat(BaseAddress + 8)
            End Get
            Set(Value As Single)
                Memory.WriteFloat(BaseAddress + 8, Value)
            End Set
        End Property
        ''' <summary>
        ''' 设置或获取动画是否存在
        ''' </summary>
        Public Property Exist As Boolean
            Get
                Return Memory.ReadInteger(BaseAddress + &H14) = 0
            End Get
            Set(Value As Boolean)
                Memory.WriteInteger(BaseAddress + &H14, Convert.ToInt32(Not Value))
            End Set
        End Property
        ''' <summary>
        ''' 设置或获取动画的起始帧
        ''' </summary>
        Public Property StartFrame As Integer
            Get
                Return Memory.ReadInteger(BaseAddress + &H18)

            End Get
            Set(Value As Integer)
                Memory.WriteInteger(BaseAddress + &H18, Value)
            End Set
        End Property
        ''' <summary>
        ''' 设置或获取动画包含的帧数
        ''' </summary>
        Public Property FrameNum As Integer
            Get
                Return Memory.ReadInteger(BaseAddress + &H1C)

            End Get
            Set(Value As Integer)
                Memory.WriteInteger(BaseAddress + &H1C, Value)
            End Set
        End Property
        ''' <summary>
        ''' 设置或获取动画的横向缩放比
        ''' </summary>
        Public Property XScale As Single
            Get
                Return Memory.ReadFloat(BaseAddress + &H24)
            End Get
            Set(Value As Single)
                Memory.WriteFloat(BaseAddress + &H24, Value)
            End Set
        End Property
        ''' <summary>
        ''' 设置或获取动画的横向倾斜度
        ''' </summary>
        Public Property XSlant As Single
            Get
                Return Memory.ReadFloat(BaseAddress + &H28)
            End Get
            Set(Value As Single)
                Memory.WriteFloat(BaseAddress + &H28, Value)
            End Set
        End Property
        ''' <summary>
        ''' 设置或获取动画的横向位置偏移(应该不能设置)
        ''' </summary>
        Public Property XOffset As Single
            Get
                Return Memory.ReadFloat(BaseAddress + &H2C)
            End Get
            Set(Value As Single)
                Memory.WriteFloat(BaseAddress + &H2C, Value)
            End Set
        End Property
        ''' <summary>
        ''' 设置或获取动画的纵向缩放比
        ''' </summary>
        Public Property YScale As Single
            Get
                Return Memory.ReadFloat(BaseAddress + &H30)
            End Get
            Set(Value As Single)
                Memory.WriteFloat(BaseAddress + &H30, Value)
            End Set
        End Property
        ''' <summary>
        ''' 设置或获取动画的纵向倾斜度
        ''' </summary>
        Public Property YSlant As Single
            Get
                Return Memory.ReadFloat(BaseAddress + &H34)
            End Get
            Set(Value As Single)
                Memory.WriteFloat(BaseAddress + &H34, Value)
            End Set
        End Property
        ''' <summary>
        ''' 设置或获取动画的纵向位置偏移(应该不能设置)
        ''' </summary>
        Public Property YOffset As Single
            Get
                Return Memory.ReadFloat(BaseAddress + &H38)
            End Get
            Set(Value As Single)
                Memory.WriteFloat(BaseAddress + &H38, Value)
            End Set
        End Property
        ''' <summary>
        ''' 设置或获取动画对象的颜色(包括透明度)
        ''' </summary>
        Public Property Colour As Color
            Get
                Dim Red = Memory.ReadInteger(BaseAddress + &H48)
                Dim Green = Memory.ReadInteger(BaseAddress + &H4C)
                Dim Blue = Memory.ReadInteger(BaseAddress + &H50)
                Dim Alpha = Memory.ReadInteger(BaseAddress + &H54)
                Return Color.FromArgb(Alpha, Red, Green, Blue)
            End Get
            Set(Value As Color)
                Memory.WriteInteger(BaseAddress + &H48, Value.R)
                Memory.WriteInteger(BaseAddress + &H4C, Value.G)
                Memory.WriteInteger(BaseAddress + &H50, Value.B)
                Memory.WriteInteger(BaseAddress + &H54, Value.A)
            End Set
        End Property
        ''' <summary>
        ''' 设置或获取动画对象的循环播放次数
        ''' </summary>
        Public Property CycleCount As Integer
            Get
                Return Memory.ReadInteger(BaseAddress + &H5C)
            End Get
            Set(Value As Integer)
                Memory.WriteInteger(BaseAddress + &H5C, Value)
            End Set
        End Property
        ''' <summary>
        ''' 着色方式枚举
        ''' </summary>
        Public Enum PaintState
            Normal = -1
            LightGary
            Gary
            Cover
        End Enum
        ''' <summary>
        ''' 设置或获取动画的着色方式
        ''' </summary>
        Public Property Paint As PaintState
            Get
                Return Memory.ReadInteger(BaseAddress + &H98)

            End Get
            Set(Value As PaintState)
                Memory.WriteInteger(BaseAddress + &H98, Value)
            End Set
        End Property
        ''' <summary>
        ''' 动画对象ID
        ''' </summary>
        Public Property Id As Integer
            Get
                Return Memory.ReadInteger(BaseAddress + &H9C)

            End Get
            Set(Value As Integer)
                Memory.WriteInteger(BaseAddress + &H9C, Value)
            End Set
        End Property
        ''' <summary>
        ''' 启用修改项目,使某些属性可写,一旦启动一直生效,直到调用<see cref="DisableModify()"/>时停止
        ''' </summary>
        '''<param name="Modify">修改的项目，可用Or连接一次修改多个</param>
        Public Shared Sub EnableModify(ByVal Modify As AnimationModify)
            If LogicalInclude(Modify, AnimationModify.ZombieSpeed) Then
                Memory.WriteQword(&H52EFF0, -8029759185035983678)
            End If
            If LogicalInclude(Modify, AnimationModify.ZombieColor) Then
                Memory.WriteInteger(&H52D3EA, -1869598485)
            End If
            If LogicalInclude(Modify, AnimationModify.ZombieScale) Then
                Memory.WriteByte(&H52C57E, &H39)
            End If
            If LogicalInclude(Modify, AnimationModify.PlantColor) Then
                Memory.WriteByte(&H4636B4, &H6B)
            End If
            If LogicalInclude(Modify, AnimationModify.PlantScale) Then
                Memory.WriteInteger(&H463E1E, -1869607701)
            End If

        End Sub
        ''' <summary>
        ''' 恢复所有的修改项目
        ''' </summary>
        Public Shared Sub DisableModify()
            Memory.WriteQword(&H52EFF0, 1205015873675)
            Memory.WriteInteger(&H52D3EA, 807683211)
            Memory.WriteByte(&H52C57E, &H14)
            Memory.WriteByte(&H4636B4, &H2B)
            Memory.WriteInteger(&H463E1E, 405030105)
        End Sub
    End Class
    ''' <summary>
    ''' 存档类(不完整)<para/>
    ''' 如果觉得有必要再叫我补完整吧
    ''' </summary>
    Public Class SaveData
        Private Sub New()
        End Sub
        ''' <summary>
        ''' 对象指针
        ''' </summary>
        Public Shared ReadOnly Property BaseAddress As Integer
            Get
                Return Memory.GetAddress(&H6A9EC0, &H82C)
            End Get
        End Property
        ''' <summary>
        ''' 当前用户名字
        ''' </summary>
        Public Shared ReadOnly Property UserName As String
            Get
                Return Memory.ReadString(BaseAddress + 4, 12)
            End Get
        End Property
        ''' <summary>
        ''' 切换用户的次数
        ''' </summary>
        Public Shared ReadOnly Property UserSwitchCount As Integer
            Get
                Return Memory.ReadInteger(BaseAddress + &H1C)
            End Get
        End Property
        ''' <summary>
        ''' 当前用户在所有用户中的索引
        ''' </summary>
        Public Shared ReadOnly Property UserIndex As Integer
            Get
                Return Memory.ReadInteger(BaseAddress + &H20)
            End Get
        End Property
        ''' <summary>
        ''' 设置或获取冒险模式的关卡
        ''' </summary>
        Public Shared Property AdventureLevel As Integer
            Get
                Return Memory.ReadInteger(BaseAddress + &H24)
            End Get
            Set(Value As Integer)
                Memory.WriteInteger(BaseAddress + &H24, Value)
            End Set
        End Property
        ''' <summary>
        ''' 设置或获取金钱数
        ''' </summary>
        Public Shared Property Money As Integer
            Get
                Return Memory.ReadInteger(BaseAddress + &H28)
            End Get
            Set(Value As Integer)
                Memory.WriteInteger(BaseAddress + &H28, Value)
            End Set
        End Property
        ''' <summary>
        ''' 设置或获取冒险模式完成次数
        ''' </summary>
        Public Shared Property AdventureFinishCount As Integer
            Get
                Return Memory.ReadInteger(BaseAddress + &H2C)
            End Get
            Set(Value As Integer)
                Memory.WriteInteger(BaseAddress + &H2C, Value)
            End Set
        End Property
        ''' <summary>
        ''' 设置或获取智慧树高度
        ''' </summary>
        Public Shared Property TreeHight As Integer
            Get
                Return Memory.ReadInteger(BaseAddress + &HF4)
            End Get
            Set(Value As Integer)
                Memory.WriteInteger(BaseAddress + &HF4, Value)
            End Set
        End Property
        ''' <summary>
        ''' 是否有指定的紫卡
        ''' </summary>
        ''' <param name="PurpleCard">要判断的紫卡</param>
        Public Shared Function HavePurpleCard(ByVal PurpleCard As CardType) As Boolean
            If PurpleCard >= CardType.GatlingPea And PurpleCard <= CardType.CobCannon Then
                Return Memory.ReadInteger(BaseAddress + &H1C0 + (PurpleCard - 40) * 4)
            End If
            Return False
        End Function
        ''' <summary>
        ''' 设置指定的紫卡
        ''' </summary>
        ''' <param name="PurpleCard">要判断的紫卡</param>
        Public Shared Sub SetPurpleCard(ByVal PurpleCard As CardType)
            If PurpleCard >= CardType.GatlingPea And PurpleCard <= CardType.CobCannon Then
                Memory.WriteInteger(BaseAddress + &H1C0 + (PurpleCard - 40) * 4, 1)
            End If
        End Sub
        ''' <summary>
        ''' 设置或获取是否有模仿者
        ''' </summary>
        Public Shared Property HaveImitater As Boolean
            Get
                Return Memory.ReadInteger(BaseAddress + &H1E0)
            End Get
            Set(Value As Boolean)
                Memory.WriteInteger(BaseAddress + &H1E0, Convert.ToInt32(Value))
            End Set
        End Property
        ''' <summary>
        ''' 设置或获取是否有黄金水壶
        ''' </summary>
        Public Shared Property HaveGoldenWatering As Boolean
            Get
                Return Memory.ReadInteger(BaseAddress + &H1F4)
            End Get
            Set(Value As Boolean)
                Memory.WriteInteger(BaseAddress + &H1F4, Convert.ToInt32(Value))
            End Set
        End Property
        ''' <summary>
        ''' 设置或获取化肥数量,注意此值并非实际值
        ''' </summary>
        Public Shared Property Fertilizer As Integer
            Get
                Return Memory.ReadInteger(BaseAddress + &H1F8)
            End Get
            Set(Value As Integer)
                Memory.WriteInteger(BaseAddress + &H1F8, Value)
            End Set
        End Property
        ''' <summary>
        ''' 设置或获取杀虫剂数量,注意此值并非实际值
        ''' </summary>
        Public Shared Property BugSpray As Integer
            Get
                Return Memory.ReadInteger(BaseAddress + &H1FC)
            End Get
            Set(Value As Integer)
                Memory.WriteInteger(BaseAddress + &H1FC, Value)
            End Set
        End Property
        ''' <summary>
        ''' 设置或获取购买卡槽的格数
        ''' </summary>
        Public Shared Property CardSlotNum As Integer
            Get
                Return Memory.ReadInteger(BaseAddress + &H214)
            End Get
            Set(Value As Integer)
                Memory.WriteInteger(BaseAddress + &H214, Value)
            End Set
        End Property
        ''' <summary>
        ''' 设置或获取巧克力数量,注意此值并非实际值
        ''' </summary>
        Public Shared Property Chocolate As Integer
            Get
                Return Memory.ReadInteger(BaseAddress + &H228)
            End Get
            Set(Value As Integer)
                Memory.WriteInteger(BaseAddress + &H228, Value)
            End Set
        End Property
        ''' <summary>
        ''' 设置或获取树肥数量,注意此值并非实际值
        ''' </summary>
        Public Shared Property TreeFood As Integer
            Get
                Return Memory.ReadInteger(BaseAddress + &H230)
            End Get
            Set(Value As Integer)
                Memory.WriteInteger(BaseAddress + &H230, Value)
            End Set
        End Property
        ''' <summary>
        ''' 设置或获取是否有结果包扎术
        ''' </summary>
        Public Shared Property HaveWallnutFirstAid As Boolean
            Get
                Return Memory.ReadInteger(BaseAddress + &H234)
            End Get
            Set(Value As Boolean)
                Memory.WriteInteger(BaseAddress + &H234, Convert.ToInt32(Value))
            End Set
        End Property
        ''' <summary>
        ''' 花园植物数量
        ''' </summary>
        Public Shared ReadOnly Property GardenPlantNum As Integer
            Get
                Return Memory.ReadInteger(BaseAddress + &H350)
            End Get
        End Property
        ''' <summary>
        ''' 获得一个花园植物的对象
        ''' </summary>
        ''' <param name="Num">要获取的花园植物的序号</param>
        Public Shared Function GetGardenPlant(ByVal Num As Integer) As GardenPlant
            If Num > 0 And Num < GardenPlantNum Then
                Return New GardenPlant(BaseAddress + &H350 + (Num - 1) * &H50)
            End If
            Return Nothing
        End Function
        ''' <summary>
        ''' 花园植物位置枚举
        ''' </summary>
        Public Enum GardenScene
            <Description("温室花园")> ZenGarden
            <Description("蘑菇园")> MushroomGarden
            <Description("铲车")> WheelBarrow
            <Description("水族馆")> Aquarium
        End Enum
        ''' <summary>
        ''' 花园植物方向枚举
        ''' </summary>
        Public Enum GardenPlantDirection
            <Description("向右")> Right
            <Description("向左")> Left
        End Enum
        ''' <summary>
        ''' 花园植物状态枚举
        ''' </summary>
        Public Enum GardenPlantState
            <Description("幼苗")> None
            <Description("小")> Small
            <Description("中")> Medium
            <Description("大")> Big
        End Enum
        ''' <summary>
        ''' 花园植物对象
        ''' </summary>
        Public Class GardenPlant
            ''' <summary>
            ''' 对象指针
            ''' </summary>
            Public BaseAddress As Integer
            ''' <summary>
            ''' 通过花园植物的序号,新建一个花园植物对象
            ''' </summary>
            ''' <param name="Address">花园植物的序号</param>
            Public Sub New(ByVal Address As Integer)
                BaseAddress = Address
            End Sub
            ''' <summary>
            ''' 设置或获取花园植物的类型
            ''' </summary>
            Public Property Type As PlantType
                Get
                    Return Memory.ReadInteger(BaseAddress + 8)
                End Get
                Set(Value As PlantType)
                    Memory.WriteInteger(BaseAddress + 8, Value)
                End Set
            End Property
            ''' <summary>
            ''' 设置或获取花园植物的位置
            ''' </summary>
            Public Property Location As GardenScene
                Get
                    Return Memory.ReadInteger(BaseAddress + &HC)
                End Get
                Set(Value As GardenScene)
                    Memory.WriteInteger(BaseAddress + &HC, Value)
                End Set
            End Property
            ''' <summary>
            ''' 设置或获取花园植物所在行
            ''' </summary>
            Public Property Row As Integer
                Get
                    Return Memory.ReadInteger(BaseAddress + &H14)

                End Get
                Set(Value As Integer)
                    Memory.WriteInteger(BaseAddress + &H14, Value)
                End Set
            End Property
            ''' <summary>
            ''' 设置或获取花园植物所在列
            ''' </summary>
            Public Property Column As Integer
                Get
                    Return Memory.ReadInteger(BaseAddress + &H10)
                End Get
                Set(Value As Integer)
                    Memory.WriteInteger(BaseAddress + &H10, Value)
                End Set
            End Property
            ''' <summary>
            ''' 设置或获取花园植物的方向
            ''' </summary>
            Public Property Direction As GardenPlantDirection
                Get
                    Return Memory.ReadInteger(BaseAddress + &H18)
                End Get
                Set(Value As GardenPlantDirection)
                    Memory.WriteInteger(BaseAddress + &H18, Value)
                End Set
            End Property
            ''' <summary>
            ''' 设置或获取花园植物的颜色(金盏花的颜色)
            ''' </summary>
            Public Property Colour As Integer
                Get
                    Return Memory.ReadInteger(BaseAddress + &H28)
                End Get
                Set(Value As Integer)
                    Memory.WriteInteger(BaseAddress + &H28, Value)
                End Set
            End Property
            ''' <summary>
            ''' 设置或获取花园植物的状态
            ''' </summary>
            Public Property State As GardenPlantState
                Get
                    Return Memory.ReadInteger(BaseAddress + &H2C)
                End Get
                Set(Value As GardenPlantState)
                    Memory.WriteInteger(BaseAddress + &H2C, Value)
                End Set
            End Property
        End Class
    End Class
    ''' <summary>
    ''' 背景音乐类型枚举
    ''' </summary>
    Public Enum MusicType
        Grasswalk = 1
        Moongrains
        WaterGraves
        RigorMormist
        CrazytheRoof
        ChooseYourSeeds
        CrazyDave
        ZenGarden
        Cerebrawl
        Loonboon
        UltimateBattle
        BrainiacManiac
    End Enum
    ''' <summary>
    ''' 背景音乐状态枚举
    ''' </summary>
    Public Enum INGAMEState
        None
        Starting
        Finished
        Disappearing
    End Enum
    ''' <summary>
    ''' 背景音乐效果枚举
    ''' </summary>
    Public Enum INGAMEEffectState
        None
        FadeIn
        Completed
        FadeOut
        Disappeared
    End Enum
    ''' <summary>
    ''' 背景音乐对象
    ''' </summary>
    Public Class Music
        ''' <summary>
        ''' 对象指针
        ''' </summary>
        Public Shared ReadOnly Property BaseAddress As Integer
            Get
                Return Memory.GetAddress(&H6A9EC0, &H83C)
            End Get
        End Property
        ''' <summary>
        ''' 设置或获取音乐类型
        ''' </summary>
        Public Shared Property Type As MusicType
            Get
                Return Memory.ReadInteger(BaseAddress + 8)
            End Get
            Set(Value As MusicType)
                Dim AsmCode As Byte() = {
                    mov_edi, 0, 0, 0, 0,
                    mov_eax, 0, 0, 0, 0,
                    call­, 2, 0, 0, 0,
                    jmp, 6,
                    pushdw, &H50, &HB7, &H45, 0,
                    ret,
                    ret
                }
                ReplaceBytes(AsmCode, 1, Value)
                ReplaceBytes(AsmCode, 6, BaseAddress)
                Memory.Execute(AsmCode)
            End Set
        End Property
        ''' <summary>
        ''' 设置或获取当前音乐是否可以开启INGAME
        ''' </summary>
        Public Shared Property INGAMEEnable As Boolean
            Get
                Return Memory.ReadInteger(BaseAddress + &H10) = 2
            End Get
            Set(Value As Boolean)
                If Value Then
                    Memory.WriteInteger(BaseAddress + &H10, 2)
                Else
                    Memory.WriteInteger(BaseAddress + &H10, -1)
                End If
            End Set
        End Property
        ''' <summary>
        ''' 设置或获取当前音乐是否开启了INGAME模式
        ''' </summary>
        Public Shared Property INGAMEStart As Boolean
            Get
                Return Memory.ReadInteger(BaseAddress + &H18) = 1
            End Get
            Set(Value As Boolean)
                If Value Then
                    Memory.WriteInteger(BaseAddress + &H18, 1)
                Else
                    Memory.WriteInteger(BaseAddress + &H18, -1)
                End If
            End Set
        End Property
        ''' <summary>
        ''' 当前音乐的BPM
        ''' </summary>
        Public Shared ReadOnly Property Tempo As Integer
            Get
                Return Memory.ReadInteger(BaseAddress + &H1C)
            End Get
        End Property
        ''' <summary>
        ''' 当前音乐的速度
        ''' </summary>
        Public Shared ReadOnly Property TicksRow As Integer
            Get
                Return Memory.ReadInteger(BaseAddress + &H20)
            End Get
        End Property
        ''' <summary>
        ''' 设置或获取当前音乐状态
        ''' </summary>
        Public Shared Property State As INGAMEState
            Get
                Return Memory.ReadInteger(BaseAddress + &H24)
            End Get
            Set(Value As INGAMEState)
                Memory.WriteInteger(BaseAddress + &H24, Value)
            End Set
        End Property
        ''' <summary>
        ''' 设置或获取当前音乐属性倒计时
        ''' </summary>
        Public Shared Property AttributeCountdown As Integer
            Get
                Return Memory.ReadInteger(BaseAddress + &H28)
            End Get
            Set(Value As Integer)
                Memory.WriteInteger(BaseAddress + &H28, Value)
            End Set
        End Property
        ''' <summary>
        ''' 设置或获取当前音乐效果状态
        ''' </summary>
        Public Shared Property EffectState As INGAMEEffectState
            Get
                Return Memory.ReadInteger(BaseAddress + &H2C)
            End Get
            Set(Value As INGAMEEffectState)
                Memory.WriteInteger(BaseAddress + &H2C, Value)
            End Set
        End Property
    End Class
    ''' <summary>
    ''' 音乐库类(啥都没有)<para/>
    ''' 如果觉得有必要再叫我补完整吧
    ''' </summary>
    Public Class Bass_Dll
        Public Const HMUSIC1 As Integer = &HA0000001
        Public Const HMUSIC2 As Integer = &HA0000002
        Public Const HMUSIC3 As Integer = &HA0000003

        Public Const BASS_MUSIC_ATTRIB_AMPLIFY = 0
        Public Const BASS_MUSIC_ATTRIB_PANSEP = 1
        Public Const BASS_MUSIC_ATTRIB_PSCALER = 2
        Public Const BASS_MUSIC_ATTRIB_BPM = 3
        Public Const BASS_MUSIC_ATTRIB_SPEED = 4
        Public Const BASS_MUSIC_ATTRIB_VOL_GLOBAL = 5
        Public Const BASS_MUSIC_ATTRIB_VOL_CHAN = &H100 ' + channel #
        Public Const BASS_MUSIC_ATTRIB_VOL_INST = &H200 ' + instrument #
        ''' <summary>
        ''' 设置音乐属性<para/>
        ''' </summary>
        ''' <param name="HMUSIC">要设置的音乐</param>
        ''' <param name="Attribute">属性值</param>
        ''' <param name="Value">设置的数值</param>
        Public Shared Sub MusicSetAttribute(ByVal HMUSIC As Integer, ByVal Attribute As Integer, ByVal Value As Integer)
            If IsNothing(Game) Then Return
            Dim AsmCode As Byte() = {
                    pushdw, 0, 0, 0, 0,
                    pushdw, 0, 0, 0, 0,
                    pushdw, 0, 0, 0, 0,
                    call­, 2, 0, 0, 0,
                    jmp, 6,
                    pushdw, 0, 0, 0, 0,
                    ret,
                    ret
                }
            ReplaceBytes(AsmCode, 1, Value)
            ReplaceBytes(AsmCode, 6, Attribute)
            ReplaceBytes(AsmCode, 11, HMUSIC)
            For Each m As ProcessModule In PVZ.Game.Modules
                If m.ModuleName = "bass.dll" Then
                    ReplaceBytes(AsmCode, 23, m.BaseAddress.ToInt32() + &H19648)
                    Memory.Execute(AsmCode)
                    Exit For
                End If
            Next
        End Sub
    End Class
#End Region
#Region "MainObject属性"
    ''' <summary>
    ''' 主要对象是否存在
    ''' </summary>
    Public Shared ReadOnly Property MainObjectExist As Boolean
        Get
            Return BaseAddress
        End Get
    End Property
    ''' <summary>
    ''' 主要对象的指针
    ''' </summary>
    Public Shared ReadOnly Property BaseAddress As Integer
        Get
            Return Memory.GetAddress(&H6A9EC0, &H768)
        End Get
    End Property
    ''' <summary>
    ''' 设置或获取画面横坐标(向左递增,正常游戏时为0)
    ''' </summary>
    Public Shared Property ViewX As Integer
        Get
            Return Memory.ReadInteger(BaseAddress + &H30)
        End Get
        Set(Value As Integer)
            Memory.WriteInteger(BaseAddress + &H30, Value)
        End Set
    End Property
    ''' <summary>
    ''' 设置或获取画面纵坐标(向下递增,正常游戏时为0)
    ''' </summary>
    Public Shared Property ViewY As Integer
        Get
            Return Memory.ReadInteger(BaseAddress + &H34)
        End Get
        Set(Value As Integer)
            Memory.WriteInteger(BaseAddress + &H34, Value)
        End Set
    End Property
    ''' <summary>
    ''' 设置或获取画面可点击的横坐标范围
    ''' </summary>
    Public Shared Property ViewLength As Integer
        Get
            Return Memory.ReadInteger(BaseAddress + &H38)
        End Get
        Set(Value As Integer)
            Memory.WriteInteger(BaseAddress + &H38, Value)
        End Set
    End Property
    ''' <summary>
    ''' 设置或获取画面可点击的纵坐标范围
    ''' </summary>
    Public Shared Property ViewHeight As Integer
        Get
            Return Memory.ReadInteger(BaseAddress + &H3C)
        End Get
        Set(Value As Integer)
            Memory.WriteInteger(BaseAddress + &H3C, Value)
        End Set
    End Property
    ''' <summary>
    ''' 游戏是否暂停
    ''' </summary>
    Public Shared ReadOnly Property GamePaused As Boolean
        Get
            Return Memory.ReadInteger(BaseAddress + 164)
        End Get
    End Property
    ''' <summary>
    ''' 草地格子类型枚举
    ''' </summary>
    Public Enum LawnType
        <Description("草地")> Grass = 1
        <Description("裸地")> Unsodded
        <Description("水池")> Water
    End Enum
    ''' <summary>
    ''' 道路类型枚举
    ''' </summary>
    Public Enum RouteType
        <Description("不出僵尸")> NoZombie
        <Description("陆地")> Land
        <Description("水路")> Pool
    End Enum
    ''' <summary>
    ''' 草地类
    ''' </summary>
    Public Class Lawn
        ''' <summary>
        ''' 获取指定位置的草地类型
        ''' </summary>
        ''' <param name="Row">行</param>
        ''' <param name="Colnum">列</param>
        Public Shared Function GetGridType(ByVal Row As Integer, ByVal Colnum As Integer) As LawnType
            If Row >= 0 And Row < 6 And Colnum >= 0 And Colnum < 9 Then
                Return Memory.ReadInteger(BaseAddress + &H168 + 4 * (6 * Colnum + Row))
            End If
            Return Nothing
        End Function
        ''' <summary>
        ''' 设置指定位置的草地类型
        ''' </summary>
        ''' <param name="Row">行</param>
        ''' <param name="Colnum">列</param>
        ''' <param name="Tyte">要设置的类型</param>
        Public Shared Sub SetGridType(ByVal Row As Integer, ByVal Colnum As Integer, ByVal Tyte As LawnType)
            If Row >= 0 And Row < 6 And Colnum >= 0 And Colnum < 9 Then
                Memory.WriteInteger(BaseAddress + &H168 + 4 * (6 * Colnum + Row), Tyte)
            End If
        End Sub
        ''' <summary>
        ''' 获取指定道路的类型
        ''' </summary>
        ''' <param name="Route">道路</param>
        Public Shared Function GetRouteType(ByVal Route As Integer) As RouteType
            If Route >= 0 And Route < 6 Then
                Return Memory.ReadInteger(BaseAddress + &H5D8 + 4 * Route)
            End If
            Return Nothing
        End Function
        ''' <summary>
        ''' 设置指定道路类型
        ''' </summary>
        ''' <param name="Route">要设置的道路</param>
        ''' <param name="Type">要设置的类型</param>
        Public Shared Sub SetRouteType(ByVal Route As Integer, ByVal Type As RouteType)
            If Route >= 0 And Route < 6 Then
                Memory.WriteInteger(BaseAddress + &H5D8 + 4 * Route, Type)
            End If
        End Sub
    End Class
    ''' <summary>
    ''' 冰道类
    ''' </summary>
    Public Class Icetrace
        ''' <summary>
        ''' 获取指定道路的冰道横坐标
        ''' </summary>
        ''' <param name="Route">要获取的道路</param>
        Public Shared Function GetX(ByVal Route As Integer) As Integer
            If Route >= 0 And Route < 6 Then
                Return Memory.ReadInteger(BaseAddress + &H60C + Route * 4)
            End If
            Return Nothing
        End Function
        ''' <summary>
        ''' 设置指定道路的冰道横坐标
        ''' </summary>
        ''' <param name="Route">要设置的道路</param>
        ''' <param name="X">要设置的横坐标</param>
        Public Shared Sub SetX(ByVal Route As Integer, ByVal X As Integer)
            If Route >= 0 And Route < 6 Then
                Memory.WriteInteger(BaseAddress + &H60C + Route * 4, X)
            End If
        End Sub
        ''' <summary>
        ''' 获取指定道路的冰道消失倒计时
        ''' </summary>
        ''' <param name="Route">要获取的道路</param>
        Public Shared Function GetDisapperaCountdown(ByVal Route As Integer) As Integer
            If Route >= 0 And Route < 6 Then
                Return Memory.ReadInteger(BaseAddress + &H624 + Route * 4)
            End If
            Return Nothing
        End Function
        ''' <summary>
        ''' 设置指定道路的冰道消失倒计时
        ''' </summary>
        ''' <param name="Route">要设置的道路</param>
        ''' <param name="DisappearCountdown">要设置的消失倒计时</param>
        Public Shared Sub SetDisapperaCountdown(ByVal Route As Integer, ByVal DisappearCountdown As Integer)
            If Route >= 0 And Route < 6 Then
                Memory.WriteInteger(BaseAddress + &H624 + Route * 4, DisappearCountdown)
            End If
        End Sub
    End Class
    ''' <summary>
    ''' 僵尸出怪列表对象
    ''' </summary>
    Public Class ZombieList
        ''' <summary>
        ''' 当前波的僵尸出怪对象
        ''' </summary>
        Public Class Wave
            ''' <summary>
            ''' 对象指针
            ''' </summary>
            Public BaseAddress As Integer
            ''' <summary>
            ''' 通过当前波的对象指针新建一个波的僵尸出怪对象
            ''' </summary>
            ''' <param name="Address">当前波的对象指针</param>
            Public Sub New(ByVal Address As Integer)
                BaseAddress = Address
            End Sub
            ''' <summary>
            ''' 设置或获得当前波的所有僵尸,返回一个数组
            ''' </summary>
            Public Property All As ZombieType()
                Get
                    Dim Num
                    Dim re(49) As ZombieType
                    For Num = 0 To 49
                        Dim zombie = Memory.ReadInteger(BaseAddress + Num * 4)
                        If zombie = -1 Then Exit For
                        re(Num) = zombie
                    Next
                    ReDim Preserve re(Num - 1)
                    Return re
                End Get
                Set(Value As ZombieType())
                    Dim Num
                    Dim Times = Value.Length - 1
                    If Times >= 50 Then Times = 49
                    For Num = 0 To Times
                        Memory.WriteInteger(BaseAddress + Num * 4, Value(Num))
                    Next
                    If Not Num = 50 Then
                        Memory.WriteInteger(BaseAddress + Num * 4, -1)
                    End If
                End Set
            End Property
            ''' <summary>
            ''' 获得当前波的僵尸数量
            ''' </summary>
            Public ReadOnly Property Count As Integer
                Get
                    For i = 0 To 49
                        If Memory.ReadInteger(BaseAddress + i * 4) = -1 Then Return i
                    Next
                    Return 50
                End Get
            End Property
            ''' <summary>
            ''' 添加一个僵尸到列表中
            ''' </summary>
            ''' <param name="Zombie">要添加的僵尸</param>
            Public Sub Add(ByVal Zombie As ZombieType)
                Dim Index = Count
                If Index <= 50 Then
                    Memory.WriteInteger(BaseAddress + Index * 4, Zombie)
                    If Not Index = 49 Then Memory.WriteInteger(BaseAddress + (Index + 1) * 4, -1)
                End If
            End Sub
            ''' <summary>
            ''' 添加多个僵尸到到列表中
            ''' </summary>
            ''' <param name="Zombie">要添加的僵尸数组</param>
            Public Sub Add(ByVal Zombie As ZombieType())
                Dim Index = Count
                Dim Inall = Index + Zombie.Length
                If Inall > 50 Then Inall = 50
                For i = Index To Inall - 1
                    Memory.WriteInteger(BaseAddress + i * 4, Zombie(i - Index))
                Next
                If Not Inall = 50 Then Memory.WriteInteger(BaseAddress + Inall * 4, -1)
            End Sub
            ''' <summary>
            ''' 删除指定位置的僵尸
            ''' </summary>
            ''' <param name="Index">要删除的僵尸位置</param>
            Public Sub Del(ByVal Index As Integer)
                Dim Num = Count
                If Index < Num Then
                    For i = Index To Num
                        If i = 49 Then
                            Memory.WriteInteger(BaseAddress + 49 * 4, -1)
                        Else
                            Memory.WriteInteger(BaseAddress + i * 4, Memory.ReadInteger(BaseAddress + (i + 1) * 4))
                        End If
                    Next
                End If
            End Sub
            ''' <summary>
            ''' 获得指定位置的僵尸
            ''' </summary>
            ''' <param name="Index">要删除的僵尸位置</param>
            Public Function Get­(ByVal Index As Integer) As ZombieType
                Dim Num = Count
                If Index = Num And Index >= 0 Then
                    Return Memory.ReadInteger(BaseAddress + Index * 4)
                End If
                Return Nothing
            End Function
            ''' <summary>
            ''' 设置指定位置的僵尸
            ''' </summary>
            ''' <param name="Index">要删除的僵尸位置</param>
            Public Sub Set­(ByVal Index As Integer, type As ZombieType)
                Dim Num = Count
                If Index < Num And Index >= 0 Then
                    Memory.WriteInteger(BaseAddress + Index * 4, type)
                End If
            End Sub
        End Class
        ''' <summary>
        ''' 获得指定波的僵尸出怪列表
        ''' </summary>
        ''' <param name="Num">要获得的波数</param>
        Public Shared Function GetWave(ByVal Num As Integer) As Wave
            If Num > 0 And Num <= WaveNum Then
                Return New Wave(BaseAddress + &H6B4 + (Num - 1) * 200)
            End If
            Return Nothing
        End Function
    End Class
    ''' <summary>
    ''' 获取僵尸出怪种子
    ''' </summary>
    Public Shared ReadOnly Property ZombieSeed As ZombieType()
        Get
            Dim Re(33) As ZombieType
            Dim j As Integer = 0
            For i = 0 To 33
                If Memory.ReadByte(BaseAddress + &H54D4 + i) = 1 Then
                    Re(j) = i
                    j += 1
                End If
            Next
            ReDim Preserve Re(j - 1)
            Return Re
        End Get
    End Property
    ''' <summary>
    ''' 设置或获取阳光掉落倒计时
    ''' </summary>
    Public Shared Property SunDropCountdown As Integer
        Get
            Return Memory.ReadInteger(BaseAddress + &H5538)
        End Get
        Set(Value As Integer)
            Memory.WriteInteger(BaseAddress + &H5538, Value)
        End Set
    End Property
    ''' <summary>
    ''' 设置或获取阳光掉落个数
    ''' </summary>
    Public Shared Property SunDropCount As Integer
        Get
            Return Memory.ReadInteger(BaseAddress + &H553C)
        End Get
        Set(Value As Integer)
            Memory.WriteInteger(BaseAddress + &H553C, Value)
        End Set
    End Property
    ''' <summary>
    ''' 引发一次屏幕震动
    ''' </summary>
    ''' <param name="LateralAmplitude">横向震动幅度,默认2</param>
    ''' <param name="VerticalAmplitude">纵向震动幅度,默认4</param>
    Public Shared Sub ViewShake(Optional LateralAmplitude As Integer = 2, Optional VerticalAmplitude As Integer = 4)
        Memory.WriteInteger(BaseAddress + &H5540, 20)
        Memory.WriteInteger(BaseAddress + &H5544, LateralAmplitude)
        Memory.WriteInteger(BaseAddress + &H5548, VerticalAmplitude)
    End Sub
    ''' <summary>
    ''' 场景类型枚举
    ''' </summary>
    Public Enum SceneType
        <Description("白天")> Day
        <Description("黑夜")> Night
        <Description("水池")> Pool
        <Description("浓雾")> Fog
        <Description("屋顶")> Roof
        <Description("月夜")> MoonNight
        <Description("蘑菇园")> MushroomGarden
        <Description("温室花园")> ZenGarden
        <Description("水族馆")> Aquarium
        <Description("智慧树")> TreeofWisdom
    End Enum
    ''' <summary>
    ''' 设置或获取当前场景类型
    ''' </summary>
    Public Shared Property Scene As SceneType
        Get
            Return Memory.ReadInteger(BaseAddress + &H554C)
        End Get
        Set(Value As SceneType)
            Memory.WriteInteger(BaseAddress + &H554C, Value)
            Dim AsmCode As Byte() = {
                mov_esi, 0, 0, 0, 0,
                call­, 2, 0, 0, 0,
                jmp, 6,
                pushdw, &H60, &HA1, &H40, 0,
                ret,
                ret
            }
            ReplaceBytes(AsmCode, 1, BaseAddress)
            Memory.Execute(AsmCode)
        End Set
    End Property
    ''' <summary>
    ''' 是否有6路地图
    ''' </summary>
    Public Shared ReadOnly Property SixRoute As Boolean
        Get
            Return Scene = SceneType.Pool Or Scene = SceneType.Fog
        End Get
    End Property
    ''' <summary>
    ''' 获取地图有5路还是6路
    ''' </summary>
    Public Shared ReadOnly Property RouteCount As Integer
        Get
            Return IIf(SixRoute, 6, 5)
        End Get
    End Property
    ''' <summary>
    ''' 设置或获取冒险模式关卡
    ''' </summary>
    Public Shared Property AdventureLevel As Integer
        Get
            Return Memory.ReadInteger(BaseAddress + &H5550)
        End Get
        Set(Value As Integer)
            Memory.WriteInteger(BaseAddress + &H5550, Value)
        End Set
    End Property
    ''' <summary>
    ''' 设置或获取当前阳光数
    ''' </summary>
    Public Shared Property Sun As Integer
        Get
            Return Memory.ReadInteger(BaseAddress + &H5560)
        End Get
        Set(Value As Integer)
            Memory.WriteInteger(BaseAddress + &H5560, Value)
        End Set
    End Property
    ''' <summary>
    ''' 获取当前关卡波数
    ''' </summary>
    Public Shared ReadOnly Property WaveNum As Integer
        Get
            Return Memory.ReadInteger(BaseAddress + &H5564)
        End Get
    End Property
    ''' <summary>
    ''' 获取当前关卡的游玩时间
    ''' </summary>
    Public Shared ReadOnly Property RunningTime As Integer
        Get
            Return Memory.ReadInteger(BaseAddress + &H556C)
        End Get
    End Property
    ''' <summary>
    ''' 僵尸突袭(产生三人组)
    ''' </summary>
    Public Shared Sub Assault()
        Memory.WriteInteger(BaseAddress + &H5574, 1)
    End Sub
    ''' <summary>
    ''' 当前所在波数
    ''' </summary>
    Public Shared ReadOnly Property CurrentWave As Integer
        Get
            Return Memory.ReadInteger(BaseAddress + &H557C)
        End Get
    End Property
    ''' <summary>
    ''' 已刷新的波数
    ''' </summary>
    Public Shared ReadOnly Property RefreshedWave As Integer
        Get
            Return Memory.ReadInteger(BaseAddress + &H5580)
        End Get
    End Property
    ''' <summary>
    ''' 1-1提示
    ''' </summary>
    Public Shared Property FirstLevelMsg As Integer
        Get
            Return Memory.ReadInteger(BaseAddress + &H5584)
        End Get
        Set(Value As Integer)
            Memory.WriteInteger(BaseAddress + &H5584, Value)
        End Set
    End Property
    ''' <summary>
    ''' 设置或获取达到刷新条件的血量
    ''' </summary>
    Public Shared Property RefreshHp As Integer
        Get
            Return Memory.ReadInteger(BaseAddress + &H5594)
        End Get
        Set(Value As Integer)
            Memory.WriteInteger(BaseAddress + &H5594, Value)
        End Set
    End Property
    ''' <summary>
    ''' 本波总血量
    ''' </summary>
    Public Shared ReadOnly Property CurrentWaveHp As Integer
        Get
            Return Memory.ReadInteger(BaseAddress + &H5598)
        End Get
    End Property
    ''' <summary>
    ''' 设置或获取下一波刷新僵尸倒计时
    ''' </summary>
    Public Shared Property RefreshWaveCountdown As Integer
        Get
            Return Memory.ReadInteger(BaseAddress + &H559C)
        End Get
        Set(Value As Integer)
            Memory.WriteInteger(BaseAddress + &H559C, Value)
        End Set
    End Property
    ''' <summary>
    ''' 设置或获取下一波刷新僵尸倒计时初始值
    ''' </summary>
    Public Shared Property RefreshHugeWaveCountdown As Integer
        Get
            Return Memory.ReadInteger(BaseAddress + &H55A4)
        End Get
        Set(Value As Integer)
            Memory.WriteInteger(BaseAddress + &H55A4, Value)
        End Set
    End Property
    ''' <summary>
    ''' 设置或获取是否有铲子
    ''' </summary>
    Public Shared Property HaveShovel As Boolean
        Get
            Return Memory.ReadByte(BaseAddress + &H55F1)
        End Get
        Set(Value As Boolean)
            Memory.WriteByte(BaseAddress + &H55F1, Convert.ToInt32(Value))
        End Set
    End Property
    ''' <summary>
    ''' 退出关卡倒计时
    ''' </summary>
    Public Shared Property ExitingLevelCountDown As Integer
        Get
            Return Memory.ReadInteger(BaseAddress + &H5600)
        End Get
        Set(Value As Integer)
            Memory.WriteInteger(BaseAddress + &H5600, Value)
        End Set
    End Property
    ''' <summary>
    ''' 调试模式枚举
    ''' </summary>
    Public Enum DebugModeType
        <Description("无调试模式")> NODEBUG
        <Description("产生")> ZOMBIESPAWNINGDEBUG
        <Description("音乐")> MUSICDEBUG
        <Description("内存")> MEMORYDEBUG
        <Description("碰撞")> COLLISIONDEBUG
    End Enum
    ''' <summary>
    ''' 设置或获取调试模式,只有英文版才可以使用这个属性
    ''' </summary>
    Public Shared WriteOnly Property DebugMode As DebugModeType
        Set(Value As DebugModeType)
            Memory.WriteInteger(BaseAddress + &H55F8, Value)
        End Set
    End Property
    ''' <summary>
    ''' 获胜(直接过关)
    ''' </summary>
    Public Shared Sub Win()
        Dim AsmCode As Byte() = {&HB9, 0, 0, 0, 0, &HE8, 2, 0, 0, 0, &HEB, 6, &H68, &HE0, &HC3, &H40, 0, &HC3, &HC3}
        ReplaceBytes(AsmCode, 1, (BaseAddress))
        If LevelId > 0 And LevelId < 16 Then
            If LevelState = GameState.Playing Then
                Memory.Execute(AsmCode)
            End If
        Else
            Memory.Execute(AsmCode)
        End If
    End Sub
    ''' <summary>
    ''' 设置当前所在波
    ''' </summary>
    ''' <param name="Wave">要设置的波数</param>
    Public Shared Sub SetCurrentWave(ByVal Wave As Integer)
        If Wave >= 0 And Wave <= WaveNum Then
            Memory.WriteInteger(BaseAddress + &H557C, Wave)
            Memory.WriteInteger(BaseAddress + &H5610, Wave * 150 / WaveNum)
        End If
    End Sub
    ''' <summary>
    ''' 发出提示音
    ''' </summary>
    Public Shared Sub Beep()
        Memory.WriteInteger(BaseAddress + &H5750, 1)
    End Sub
    ''' <summary>
    ''' 设置或获取是否开启了Mustache模式
    ''' </summary>
    Public Shared Property Mustache As Boolean
        Get
            Return Memory.ReadByte(BaseAddress + &H5761)
        End Get
        Set(Value As Boolean)
            Memory.WriteByte(BaseAddress + &H5761, Convert.ToInt32(Value))
        End Set
    End Property
    ''' <summary>
    ''' 设置或获取是否开启了Trickedout模式
    ''' </summary>
    Public Shared Property Trickedout As Boolean
        Get
            Return Memory.ReadByte(BaseAddress + &H5762)
        End Get
        Set(Value As Boolean)
            Memory.WriteByte(BaseAddress + &H5762, Convert.ToInt32(Value))
        End Set
    End Property
    ''' <summary>
    ''' 设置或获取是否开启了Future模式
    ''' </summary>
    Public Shared Property Future As Boolean
        Get
            Return Memory.ReadByte(BaseAddress + &H5763)
        End Get
        Set(Value As Boolean)
            Memory.WriteByte(BaseAddress + &H5763, Convert.ToInt32(Value))
        End Set
    End Property
    ''' <summary>
    ''' 设置或获取是否开启了Pinata模式
    ''' </summary>
    Public Shared Property Pinata As Boolean
        Get
            Return Memory.ReadByte(BaseAddress + &H5764)
        End Get
        Set(Value As Boolean)
            Memory.WriteByte(BaseAddress + &H5764, Convert.ToInt32(Value))
        End Set
    End Property
    ''' <summary>
    ''' 设置或获取是否开启了Dance模式
    ''' </summary>
    Public Shared Property Dance As Boolean
        Get
            Return Memory.ReadByte(BaseAddress + &H5765)
        End Get
        Set(Value As Boolean)
            Memory.WriteByte(BaseAddress + &H5765, Convert.ToInt32(Value))
        End Set
    End Property
    ''' <summary>
    ''' 设置或获取是否开启了Daisies模式
    ''' </summary>
    Public Shared Property Daisies As Boolean
        Get
            Return Memory.ReadByte(BaseAddress + &H5766)
        End Get
        Set(Value As Boolean)
            Memory.WriteByte(BaseAddress + &H5766, Convert.ToInt32(Value))
        End Set
    End Property
    ''' <summary>
    ''' 设置或获取是否开启了Sukhbir模式
    ''' </summary>
    Public Shared Property Sukhbir As Boolean
        Get
            Return Memory.ReadByte(BaseAddress + &H5767)
        End Get
        Set(Value As Boolean)
            Memory.WriteByte(BaseAddress + &H5767, Convert.ToInt32(Value))
        End Set
    End Property
    ''' <summary>
    ''' 获得被吃掉的植物数
    ''' </summary>
    Public Shared ReadOnly Property EatenPlants As Integer
        Get
            Return Memory.ReadInteger(BaseAddress + &H5798)
        End Get
    End Property
    ''' <summary>
    ''' 获得被铲掉的植物数
    ''' </summary>
    Public Shared ReadOnly Property ShoveledPlants As Integer
        Get
            Return Memory.ReadInteger(BaseAddress + &H579C)
        End Get
    End Property
#End Region
#End Region
End Class
