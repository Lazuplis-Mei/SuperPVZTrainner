Imports System.Text.RegularExpressions
Imports Microsoft.Win32
Imports System.IO
Imports System.Reflection
Imports ITrainerExtension
Imports System.Globalization
Imports PVZClass
Imports System.Runtime.InteropServices

Class MainWindow
    '特定的转换字符串为数字
    Public Shared Function StrToInt(ByVal Value As String) As Integer
        Try
            StrToInt = CInt(Value)
        Catch ex As OverflowException
            StrToInt = Integer.MaxValue
        Catch ex As InvalidCastException
            StrToInt = 0
        End Try
    End Function
    '窗口载入
    Private Sub Window_Loaded(sender As Object, e As RoutedEventArgs)
        openFileDlg = New OpenFileDialog()
        openFileDlg.DefaultExt = ".dll"
        openFileDlg.Multiselect = True
        LBMain.Tag = -1
        ListPlugIns.Tag = -1
        FindGame()
        If Directory.Exists("Extension") Then
            For Each f In Directory.GetFiles("Extension", "*.dll")
                AddExtension(Path.GetFullPath(f))
            Next
        End If
        If Directory.Exists("Scripts") Then
            For Each f In Directory.GetFiles("Scripts", "*.pvzs")
                AddLBIScr(Path.GetFullPath(f))
            Next
        End If
        Lang.ChangeLanguage(Content)
    End Sub
    '寻找游戏
    Private Sub FindGame()
        If PVZ.RunGame() Then
            Dim check = PVZ.CheckPeocess()
            If check.HasValue Then
                If check.Value Then
                    TBStatus.Text = StatusTextFound(Lang.Id)
                    PVZ.InitFunctions()
                    PVZ.Game.EnableRaisingEvents = True
                    AddHandler PVZ.Game.Exited, AddressOf GameExited
                Else
                    TBStatus.Text = StatusTextNotSuppost(Lang.Id)
                End If
            Else
                TBStatus.Text = StatusTextOpenFailed(Lang.Id)
            End If
        Else
            TBStatus.Text = StatusTextNotFound(Lang.Id)
        End If
    End Sub

    Private Sub GameExited()
        Dispatcher.Invoke(Sub() TBStatus.Text = StatusTextNotFound(Lang.Id))
    End Sub

    '窗口关闭
    Private Sub Window_Closed(sender As Object, e As EventArgs)
        PVZ.CloseGame()
        Dim temp = Environment.GetEnvironmentVariable("Temp")
        For Each f In Directory.GetFiles(temp, "PlantsVsZombies_Temp*.exe")
            Try
                File.Delete(f)
            Catch
                Continue For
            End Try
        Next
        Application.Current.Shutdown()
        Forms.Application.Exit()
    End Sub
    '添加pvz脚本
    Private Sub AddLBIScr(ByVal scriptfile As String)
        If ChecckPlugIns(scriptfile) Then Exit Sub
        Dim ALBIStr = File.ReadAllText(scriptfile)
        ALBIStr = ALBIStr.TrimEnd(vbCr, " ", vbLf)
        If ALBIStr.EndsWith("End") Or ALBIStr.EndsWith("EndScript") Then
            Dim Btn = New DarkStyle.DarkButton()
            Dim substr = scriptfile.Split("\\")
            Dim name = substr(substr.Length - 1)
            Btn.Content = name.Substring(0, name.Length - 5)
            Btn.Tag = scriptfile
            Btn.Style = FindResource("LBIBtnnStyle1")
            Btn.Width = 175
            AddHandler Btn.Click, AddressOf ButtonScript_Click
            ListPlugIns.Items.Add(Btn)
        End If
    End Sub
    '运行脚本
    Private Sub ButtonScript_Click(sender As Object, e As RoutedEventArgs)
        Dim pvzscript = New Process()
        pvzscript.StartInfo.FileName = "PVZScript.exe"
        pvzscript.StartInfo.WindowStyle = ProcessWindowStyle.Hidden
        pvzscript.StartInfo.Arguments = """" + CType(sender, FrameworkElement).Tag + """"
        Try
            pvzscript.Start()
        Catch ex As ComponentModel.Win32Exception
            If Lang.Id = 1 Then
                MessageBox.Show("The program PVZScriptNoConsole.exe was not found." + vbCrLf + "Please place the program in the PVZScript directory", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning)
            Else
                MessageBox.Show("没有找到程序PVZScriptNoConsole.exe,请将程序放置到PVZScript目录下", "警告", MessageBoxButton.OK, MessageBoxImage.Warning)
            End If
        End Try
    End Sub
    '检查插件重复
    Private Function ChecckPlugIns(file As String) As Boolean
        For Each lbi In ListPlugIns.Items
            If CType(lbi, Control).Tag = file Then
                If Lang.Id = 1 Then
                    MessageBox.Show($"{file} is already added", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning)
                Else
                    MessageBox.Show($"项目{file}已经添加", "警告", MessageBoxButton.OK, MessageBoxImage.Warning)
                End If
                Return True
            End If
        Next
        Return False
    End Function
    '添加插件
    Private Sub AddExtension(ByVal extensionfile As String)
        If ChecckPlugIns(extensionfile) Then Exit Sub
        Dim asm As Assembly
        Try
            asm = Assembly.LoadFile(extensionfile)
        Catch ex As BadImageFormatException
            Return
        End Try
        Dim types() = asm.GetExportedTypes()
        For Each type In types
            If type.IsClass Then
                Dim ifaces() = type.GetInterfaces()
                For Each iface In ifaces
                    If iface = GetType(ITrainerExtensionButton) Then
                        AddButtonPlugIn(extensionfile, type)
                    ElseIf iface = GetType(IITrainerExtensionCheckBox) Then
                        AddCheckBoxPlugIn(extensionfile, type)
                    ElseIf iface = GetType(ITrainerExtensionTextBox) Then
                        AddTextBoxPlugIn(extensionfile, type)
                    ElseIf iface = GetType(ITrainerExtensionUserControl) Then
                        AddUserControlPlugIn(extensionfile, type)
                    End If
                Next
            End If
        Next
    End Sub
    '添加控件插件
    Private Sub AddUserControlPlugIn(extensionfile As String, type As Type)
        Dim usercon As ITrainerExtensionUserControl = Activator.CreateInstance(type)
        Dim mi As ListBoxItem = New ListBoxItem With {
            .Content = usercon.Text,
            .Resources = New ResourceDictionary() From {{“Lang”, usercon.TextLang}},
            .Tag = extensionfile,
            .Foreground = Brushes.White,
            .HorizontalAlignment = HorizontalAlignment.Stretch,
            .HorizontalContentAlignment = HorizontalAlignment.Center
        }
        If Not IsNothing(usercon.ToolTip) Then
            mi.ToolTip = New MyToolTip() With {
            .Content = usercon.ToolTip,
            .Resources = New ResourceDictionary() From {{“Lang”, usercon.ToolTipLang}}
        }
        End If
        AddHandler mi.MouseDoubleClick,
            Sub()
                expanderPlugIns.IsExpanded = False
                Dim expend As ExpendWindow = New ExpendWindow()
                expend.Tag = mi
                expend.TBTitle.Text = usercon.Text
                expend.TBTitle.Resources = New ResourceDictionary() From {{“Lang”, usercon.TextLang}}
                usercon.Layout(expend, expend.MainCanvas)
                Lang.ChangeLanguage(expend.MainCanvas)
                expend.Show()
                mi.IsEnabled = False
            End Sub
        ListPlugIns.Items.Add(mi)
    End Sub

    '添加文本框插件
    Private Sub AddTextBoxPlugIn(extensionfile As String, type As Type)
        Dim lbibtnwithcb As ITrainerExtensionTextBox = Activator.CreateInstance(type)
        Dim TBlock = New TextBlock With {
            .Foreground = Brushes.White,
            .Text = lbibtnwithcb.Text,
            .Resources = New ResourceDictionary() From {{“Lang”, lbibtnwithcb.TextLang}}
        }
        Dim TBox = New TextBox With {
            .VerticalAlignment = VerticalAlignment.Center,
            .Margin = New Thickness(5, 0, 0, 0),
            .Width = 125,
            .Foreground = Brushes.White,
            .Background = New SolidColorBrush(Color.FromArgb(&HFF, &H25, &H25, &H26)),
            .AllowDrop = False,
            .BorderThickness = New Thickness(0)
        }
        InputMethod.SetIsInputMethodEnabled(TBox, False)
        TBox.ContextMenu = Nothing
        AddHandler TBox.PreviewKeyDown, AddressOf TBSun_PreviewKeyDown
        Dim lbi = New ListBoxItem()
        AddHandler lbi.MouseDoubleClick, Sub() lbibtnwithcb.FunctionOnCall(TBox.Text)
        Dim g = New Grid With {
            .Margin = New Thickness(0, 0, -4, 0)
        }
        Dim cd1 = New ColumnDefinition With {
            .Width = New GridLength(1, GridUnitType.Star)
        }
        Dim cd2 = New ColumnDefinition With {
            .Width = New GridLength(1, GridUnitType.Star)
        }
        g.ColumnDefinitions.Add(cd1)
        g.ColumnDefinitions.Add(cd2)
        Grid.SetColumn(TBox, 1)
        g.Children.Add(TBlock)
        g.Children.Add(TBox)
        lbi.Content = g
        lbi.Tag = extensionfile
        If Not IsNothing(lbibtnwithcb.ToolTip) Then
            lbi.ToolTip = New MyToolTip() With
                {
                .Content = lbibtnwithcb.ToolTip,
                .Resources = New ResourceDictionary() From {{“Lang”, lbibtnwithcb.ToolTipLang}}
            }
        End If
        ListPlugIns.Items.Add(lbi)
    End Sub
    '添加选择框插件
    Private Sub AddCheckBoxPlugIn(extensionfile As String, type As Type)
        Dim lbicheckbox As IITrainerExtensionCheckBox = Activator.CreateInstance(type)
        Dim Cbox = New MyCheckBox With {
            .Content = lbicheckbox.Text,
            .Resources = New ResourceDictionary() From {{“Lang”, lbicheckbox.TextLang}},
            .Tag = extensionfile,
            .Style = FindResource("CheckBoxStyle1")
        }
        If Not IsNothing(lbicheckbox.ToolTip) Then
            Cbox.ToolTip = New MyToolTip() With {
                .Content = lbicheckbox.ToolTip,
                .Resources = New ResourceDictionary() From {{“Lang”, lbicheckbox.ToolTipLang}}
            }
        End If
        AddHandler Cbox.Click,
            Sub(sender As CheckBox, e As RoutedEventArgs)
                lbicheckbox.CheckBoxOnClick(sender.IsChecked)
            End Sub
        ListPlugIns.Items.Add(Cbox)
    End Sub
    '添加按钮插件
    Private Sub AddButtonPlugIn(extensionfile As String, type As Type)
        Dim lbibutton As ITrainerExtensionButton = Activator.CreateInstance(type)
        Dim Btn = New DarkStyle.DarkButton With {
            .Content = lbibutton.Text,
            .Resources = New ResourceDictionary() From {{“Lang”, lbibutton.TextLang}},
            .Tag = extensionfile,
            .Style = FindResource("LBIBtnnStyle1"),
            .Width = 175
        }
        If Not IsNothing(lbibutton.ToolTip) Then
            Btn.ToolTip = New MyToolTip() With {
                .Content = lbibutton.ToolTip,
                .Resources = New ResourceDictionary() From {{“Lang”, lbibutton.ToolTipLang}}
            }
        End If
        AddHandler Btn.Click, AddressOf lbibutton.ButtonOnClick
        ListPlugIns.Items.Add(Btn)
    End Sub
    '载入功能
    Private Sub BtnLoadLBMain_Click(sender As Object, e As RoutedEventArgs)
        If Lang.Id = 1 Then
            openFileDlg.Filter = "extension plugin|*.dll|pvz script file|*.pvzs"
        Else
            openFileDlg.Filter = "扩展插件|*.dll|pvz脚本文件|*.pvzs"
        End If
        openFileDlg.Title = IIf(Lang.Id = 1, "Load PlugIn", "载入插件")
        If openFileDlg.ShowDialog() Then
            If openFileDlg.FilterIndex = 1 Then
                For Each f In openFileDlg.FileNames
                    AddExtension(f)
                Next
            ElseIf openFileDlg.FilterIndex = 2 Then
                For Each f In openFileDlg.FileNames
                    AddLBIScr(f)
                Next
            End If
        End If
    End Sub
    '主列表框按键
    Private Sub LBMain_KeyDown(sender As Object, e As KeyEventArgs)
        Dim listbox As ListBox = sender
        If e.Key = Key.Delete AndAlso listbox.Name = "ListPlugIns" Then
            listbox.Items.Remove(listbox.SelectedItem)
        End If
        If Keyboard.GetKeyStates(Key.LeftCtrl) Then
            If e.Key = Key.C Then
                listbox.Tag = listbox.SelectedIndex
                Try
                    Clipboard.SetText(listbox.Items.GetItemAt(listbox.Tag).Tag)
                Catch ex As ArgumentNullException
                    Clipboard.SetText("+ <=> -")
                End Try
            End If
            If e.Key = Key.V Then
                If listbox.Tag <> -1 Then
                    Dim temp = listbox.Items.GetItemAt(listbox.Tag)
                    listbox.Items.Remove(temp)
                    listbox.Items.Insert(listbox.SelectedIndex + 1, temp)
                    listbox.Tag = -1
                End If
            End If
        End If
    End Sub
    '运行拖动窗口
    Private Sub Window_MouseDown(sender As Object, e As MouseButtonEventArgs)
        Try
            DragMove()
        Catch ex As InvalidOperationException
        End Try
    End Sub
    '文本框响应
    Private Sub TBSun_PreviewKeyDown(sender As Object, e As KeyEventArgs)
        If e.Key = Key.Enter Then
            Dim tb = CType(sender, TextBox)
            If TypeOf tb.Parent Is Grid Then
                Dim grand = CType(tb.Parent, Grid).Parent
                If TypeOf grand Is ListBoxItem Then
                    Dim dce = New MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Left) With {
                        .RoutedEvent = MouseDoubleClickEvent
                    }
                    CType(grand, ListBoxItem).RaiseEvent(dce)
                End If
            End If
        ElseIf e.Key = Key.Space Then
            TBSun.Text = "0"
            e.Handled = True
        ElseIf e.Key = Key.V Then
            e.Handled = True
        End If
    End Sub
    Private Sub LBISun_MouseDoubleClick(sender As Object, e As MouseButtonEventArgs)
        Dim Value = StrToInt(TBSun.Text)
        PVZ.Sun = Value
        TBSun.Text = CStr(Value)
    End Sub
    Private Sub LBIMoney_MouseDoubleClick(sender As Object, e As MouseButtonEventArgs)
        Dim Value = Int(StrToInt(TBMoney.Text) / 10)
        PVZ.SaveData.Money = Value
        TBMoney.Text = CStr(Value * 10)
    End Sub
    Private Sub LBISunMax_MouseDoubleClick(sender As Object, e As MouseButtonEventArgs)
        Dim Value = StrToInt(TBSunMax.Text)
        PVZ.SunMax = Value
        TBSunMax.Text = CStr(Value)
    End Sub
    Private Sub CBBGRunable_Click(sender As Object, e As RoutedEventArgs)
        PVZ.BGRunable(CBBGRunable.IsChecked)
    End Sub
    Private Sub CBFreePlanting_Click(sender As Object, e As RoutedEventArgs)
        PVZ.FreePlantingCheat = CBFreePlanting.IsChecked
    End Sub
    Private Sub CBShowHidden_Click(sender As Object, e As RoutedEventArgs)
        PVZ.ShowHiddenLevel(CBShowHidden.IsChecked)
    End Sub
    Private Sub CBOverlapPlanting_Click(sender As Object, e As RoutedEventArgs)
        PVZ.OverlapPlanting(CBOverlapPlanting.IsChecked)
    End Sub
    Private Sub CBIgnoreRes_Click(sender As Object, e As RoutedEventArgs)
        PVZ.IgnoreRes(CBIgnoreRes.IsChecked)
    End Sub
    Private Sub CBNoCD_Click(sender As Object, e As RoutedEventArgs)
        PVZ.NoCD(CBNoCD.IsChecked)
    End Sub
    Private Sub CBConveyorBeltNoDelay_Click(sender As Object, e As RoutedEventArgs)
        PVZ.ConveyorBeltNoDelay(CBConveyorBeltNoDelay.IsChecked)
    End Sub
    Private Sub CBFullScreenFog_Click(sender As Object, e As RoutedEventArgs)
        PVZ.FullScreenFog(CBFullScreenFog.IsChecked)
    End Sub
    Private Sub CBBlockZombie_Click(sender As Object, e As RoutedEventArgs)
        PVZ.BlockZombie(CBBlockZombie.IsChecked)
    End Sub
    Private Sub CBNoUpperLimit_Click(sender As Object, e As RoutedEventArgs)
        PVZ.NoUpperLimit(CBNoUpperLimit.IsChecked)
    End Sub
    Private Sub CBVasePerspect_Click(sender As Object, e As RoutedEventArgs)
        PVZ.VasePerspect(CBVasePerspect.IsChecked)
    End Sub
    Private Sub CBFogPerspect_Click(sender As Object, e As RoutedEventArgs)
        PVZ.FogPerspect(CBFogPerspect.IsChecked)
    End Sub
    Private Sub CBLockShovel_Click(sender As Object, e As RoutedEventArgs)
        PVZ.LockShovel(CBLockShovel.IsChecked)
    End Sub
    Private Sub CBAutoCollect_Click(sender As Object, e As RoutedEventArgs)
        PVZ.AutoCollect(CBAutoCollect.IsChecked)
    End Sub
    Private Sub BtnKillAllZombies_Click(sender As Object, e As RoutedEventArgs)
        For Each zombie In PVZ.AllZombies
            zombie.State = 3
        Next
    End Sub
    Private Sub BtnHypnotizeAllZombies_Click(sender As Object, e As RoutedEventArgs)
        For Each zombie In PVZ.AllZombies
            zombie.Hypnotized = True
        Next
    End Sub
    Private Sub BtnKillAllPlants_Click(sender As Object, e As RoutedEventArgs)
        For Each plant In PVZ.AllPlants
            plant.Exist = False
        Next
    End Sub
    Private Sub BtnMonitor_Click(sender As Object, e As RoutedEventArgs)
        BtnMonitor.IsEnabled = False
        Dim monitor As New MonitorWindow()
        monitor.Show()
    End Sub
    Private Sub BtnModify_Click(sender As Object, e As RoutedEventArgs)
        Dim modify As New ModifyWindow()
        BtnModify.IsEnabled = False
        modify.Show()
    End Sub
    Private Sub BtnOperate_Click(sender As Object, e As RoutedEventArgs)
        Dim operate As New OperationWindow()
        BtnOperate.IsEnabled = False
        operate.Show()
    End Sub

    Private Sub Window_PreviewKeyDown(sender As Object, e As KeyEventArgs)
        If e.Key = Key.F2 Then
            If Lang.Id = 1 Then
                MessageBox.Show(PVZ.LastWarning, "You got a warning", MessageBoxButton.OK, MessageBoxImage.Warning)
            Else
                MessageBox.Show(PVZ.LastWarning, "你得到如下警告", MessageBoxButton.OK, MessageBoxImage.Warning)
            End If
        ElseIf e.Key = Key.F5 Then
            OpenGame()
        ElseIf e.Key = Key.F6 Then
            OpenMuiti()
        End If
    End Sub

    Private Sub OpenMuiti()
        If File.Exists(PVZ.GamePath) Then
            Try
                Dim temp = Path.Combine(Environment.GetEnvironmentVariable("Temp"), $"PlantsVsZombies_Temp{Rnd() * 1000 * Rnd() * 1000 * Rnd()}.exe")
                File.Copy(PVZ.GamePath, temp)
                Using f = File.OpenWrite(temp)
                    f.Seek(&H153F1B, SeekOrigin.Begin)
                    f.WriteByte(&HEB)
                End Using
                Dim pro = New Process()
                Dim startInfo = New ProcessStartInfo()
                startInfo.FileName = temp
                startInfo.WorkingDirectory = Path.GetDirectoryName(PVZ.GamePath)
                pro.StartInfo = startInfo
                pro.Start()
            Catch
            End Try
        End If
    End Sub

    Private Sub OpenGame()
        Try
            If Not IsNothing(PVZ.Game) AndAlso PVZ.Game.HasExited Then
                Dim startInfo = New ProcessStartInfo()
                startInfo.FileName = PVZ.GamePath
                startInfo.WorkingDirectory = Path.GetDirectoryName(PVZ.GamePath)
                Process.Start(startInfo)
                FindGame()
            End If
        Catch
        End Try
    End Sub

    Private Sub expanderPlugIns_Expanded(sender As Object, e As RoutedEventArgs)
        BtnMonitor.Visibility = Visibility.Collapsed
        BtnModify.Visibility = Visibility.Collapsed
        BtnOperate.Visibility = Visibility.Collapsed
    End Sub
    Private Sub expanderPlugIns_Collapsed(sender As Object, e As RoutedEventArgs)
        BtnMonitor.Visibility = Visibility.Visible
        BtnModify.Visibility = Visibility.Visible
        BtnOperate.Visibility = Visibility.Visible
    End Sub

    Dim StatusTextFound = {"已找到游戏", "Game Found"}
    Dim StatusTextNotSuppost = {"不支持的版本", "NotSuppost"}
    Dim StatusTextOpenFailed = {"打开游戏失败", "OpenFailed"}
    Dim StatusTextNotFound = {"没有找到游戏", "NotFound"}
    Private openFileDlg As OpenFileDialog

    Private Sub BtnFindGame_Click(sender As Object, e As RoutedEventArgs)
        FindGame()
    End Sub
    Private Sub BtnFindGame_MouseDoubleClick(sender As Object, e As MouseButtonEventArgs)
        If e.LeftButton = MouseButtonState.Pressed Then
            Dim processSelector = New ProcessSelector()
            If processSelector.ShowDialog() Then
                If PVZ.RunGame(processSelector.ProcessId) Then
                    Dim check = PVZ.CheckPeocess()
                    If check.HasValue Then
                        If check.Value Then
                            TBStatus.Text = StatusTextFound(Lang.Id)
                            PVZ.InitFunctions()
                            PVZ.Game.EnableRaisingEvents = True
                            AddHandler PVZ.Game.Exited, AddressOf GameExited
                        Else
                            TBStatus.Text = StatusTextNotSuppost(Lang.Id)
                        End If
                    Else
                        TBStatus.Text = StatusTextOpenFailed(Lang.Id)
                    End If
                Else
                    TBStatus.Text = StatusTextNotFound(Lang.Id)
                End If
            End If
        End If
    End Sub

    Private Sub textBlock_MouseDown(sender As Object, e As MouseButtonEventArgs)
        If e.ClickCount = 2 Then
            If e.LeftButton = MouseButtonState.Pressed AndAlso e.RightButton = MouseButtonState.Released Then
                Lang.Id += 1
                Lang.Id = Lang.Id Mod Lang.Count
                BtnFindGame_Click(Nothing, Nothing)
                For Each win As Window In Application.Current.Windows
                    Lang.ChangeLanguage(win.Content)
                Next
            ElseIf e.RightButton = MouseButtonState.Pressed AndAlso e.LeftButton = MouseButtonState.Released Then
                Dim output = New ExpendWindow()
                output.TBTitle.Text = IIf(Lang.Id = 1, "About", "关于")
                Dim text = New TextBox With {
                    .Width = 578,
                    .Height = 295,
                    .Background = Brushes.Transparent,
                    .Foreground = Brushes.White,
                    .IsReadOnly = True,
                    .TextWrapping = TextWrapping.Wrap,
                    .FontSize = 16
                }
                Canvas.SetTop(text, 56)
                Canvas.SetLeft(text, 10)
                If Lang.Id = 1 Then
                    text.Text = "This procedure is made by 冥谷川恋(email: lazuplismei@163.com )
It can be used to modify various contents of Plants vs.zombies,it's a modifier which provide powerful functions of monitoring, modification and operation
You can even manually plug it in to extend its capabilities(Just implement the interface in ITrainerExtension)
Attentions:
The program is always free and can be used at will.
It only supposted 1.0.0.1051 version of Plants vs.zombies.
For later versions of 1.2.0.1063,1.2.0.1073 and the version from steam all invalid."
                Else
                    text.Text = "本程序由冥谷川恋制作（QQ398833450，邮箱lazuplismei@163.com）
可用于修改植物大战僵尸的各项内容，是一个提供监视，修改，操作等强大功能的修改器
甚至可以手动为其编写插件来扩展它的功能（实现ITrainerExtension中的接口即可）
注意事项：
程序完全免费可任意使用
程序仅对1.0.0.1051版本的植物大战僵尸有效
对于更高版本的1.2.0.1063，1.2.0.1073以及来源于Steam上的版本均无效"
                End If
                output.MainCanvas.Children.Add(text)
                Dim btn = New DarkStyle.DarkButton With {
                    .BorderThickness = New Thickness(1),
                    .Width = 200,
                    .Content = IIf(Lang.Id = 1, "Close", "关闭"),
                    .FontSize = 20
                }
                Canvas.SetBottom(btn, 10)
                Canvas.SetLeft(btn, 195)
                AddHandler btn.Click, AddressOf output.Close
                output.MainCanvas.Children.Add(btn)
                output.ShowDialog()
            End If
        End If
    End Sub


    Private Sub BtnFindGame_MouseDown(sender As Object, e As MouseButtonEventArgs)
        If e.MiddleButton = MouseButtonState.Pressed Then
            OpenMuiti()
        ElseIf e.RightButton = MouseButtonState.Pressed Then
            OpenGame()
        End If
    End Sub
End Class
