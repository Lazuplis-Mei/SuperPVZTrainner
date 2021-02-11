Imports System.Net
Imports System.Net.Mail
Imports System.Runtime.InteropServices
Imports System.Text.RegularExpressions
Imports System.Threading
Imports System.Windows.Markup
Imports Microsoft.Win32

Class Application
    Public Shared Function IsChineseSystem() As Boolean
        Dim lang = Thread.CurrentThread.CurrentCulture.Name
        Return lang = "zh-CN" OrElse lang = "en-US"
    End Function

    Private Sub Application_Startup(sender As Object, e As StartupEventArgs)
        If Not IsChineseSystem() Then
            ITrainerExtension.Lang.Id = 1
        End If
    End Sub

    ' 应用程序级事件(例如 Startup、Exit 和 DispatcherUnhandledException)
    ' 可以在此文件中进行处理。
    Public Sub _InitializeComponent() Implements IComponentConnector.InitializeComponent
        InitializeComponent()
    End Sub

    Shared Sub SendToAuthor(title As String, message As String)
        Dim msg = String.Format("mailto:lazuplismei@163.com?subject={0}&body={1}", title, message)
        Process.Start(msg)
    End Sub

    <STAThread>
    <Obsolete>
    Public Shared Sub Main()
        AppDomain.CurrentDomain.AppendPrivatePath("Extension")
        Dim app As Application = New Application()
        If Debugger.IsAttached Then
            app.Run()
        Else
            Try
                app.Run()
            Catch ex As Exception
                Dim output = New ExpendWindow()
                output.TBTitle.Text = ex.Message.Replace(vbCrLf, vbNullString)
                If output.TBTitle.GetTextRect().Width > 480 Then
                    While output.TBTitle.GetTextRect().Width > 470
                        output.TBTitle.Text = output.TBTitle.Text.Substring(0, output.TBTitle.Text.Length - 1)
                    End While
                    output.TBTitle.Text += "..."
                End If
                Dim text = New TextBox With {
                    .Width = 578,
                    .Height = 295,
                    .Background = Brushes.Transparent,
                    .Foreground = Brushes.White,
                    .IsReadOnly = True,
                    .TextWrapping = TextWrapping.Wrap
                }
                Canvas.SetTop(text, 56)
                Canvas.SetLeft(text, 10)
                text.Text = ex.ToString()
                Dim btn = New DarkStyle.DarkButton With {
                    .Width = 200,
                    .Content = IIf(ITrainerExtension.Lang.Id = 1, "Restart", "重启程序"),
                    .FontSize = 20
                }
                Canvas.SetBottom(btn, 10)
                Canvas.SetLeft(btn, 80)
                AddHandler btn.Click, Sub()
                                          Forms.Application.Restart()
                                          app.Shutdown()
                                      End Sub
                output.MainCanvas.Children.Add(btn)
                btn = New DarkStyle.DarkButton With {
                    .Width = 200,
                    .Content = IIf(ITrainerExtension.Lang.Id = 1, "SendToAuthor", "发给作者"),
                    .FontSize = 20
                }
                Canvas.SetBottom(btn, 10)
                Canvas.SetRight(btn, 80)
                AddHandler btn.Click, Sub()
                                          Dim input = New InputDialog(
                                          IIf(ITrainerExtension.Lang.Id = 1, "Are you sure to send an email?", "确认要发送邮件?(万一得到回复了呢?)"),
                                          IIf(ITrainerExtension.Lang.Id = 1, "Your QQ number(if you have)", "请输入您的QQ号"),
                                          1, 99999999999)
                                          If input.ShowDialog() Then
                                              SendToAuthor($"[{input.Value.ToString()}]" + ex.Message.Replace(vbCrLf, vbNullString), ex.ToString())
                                          End If
                                          app.Shutdown()
                                      End Sub
                output.MainCanvas.Children.Add(btn)
                output.MainCanvas.Children.Add(text)
                output.ShowDialog()
            End Try
        End If
    End Sub
End Class
