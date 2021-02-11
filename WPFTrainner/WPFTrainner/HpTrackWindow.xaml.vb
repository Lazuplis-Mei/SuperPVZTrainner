Imports PVZClass
Imports System.Linq
Imports System.Runtime.InteropServices
Imports System.Windows.Interop

Public Class HpTrackWindow
    Public hpfontcolor As Color = Colors.White
    Private Structure Spoint
        Dim x As Integer
        Dim y As Integer
    End Structure
    Private Declare Function ClientToScreen Lib "user32.dll" (ByVal hwnd As Integer, ByRef lppoint As Spoint) As Boolean
    <DllImport("user32.dll", SetLastError:=True)>
    Private Shared Function SetWindowLong(ByVal hWnd As IntPtr, ByVal nIndex As Integer, ByVal dwNewLong As Integer) As Integer
    End Function
    Private Declare Auto Function IsIconic Lib "user32.dll" (ByVal hwnd As IntPtr) As Boolean
    Private prezombienum As Integer = 0
    Private Timer As Threading.DispatcherTimer
    Private Sub Window_Loaded(sender As Object, e As RoutedEventArgs)
        IsHitTestVisible = False
        dpi = GetDpiFromVisual()
        SetWindowLong(New WindowInteropHelper(Me).Handle, -20, &H20)
        Timer = New Threading.DispatcherTimer()
        Timer.Interval = TimeSpan.FromMilliseconds(30)
        AddHandler Timer.Tick, New EventHandler(AddressOf TimerTick)
        Timer.Start()
    End Sub
    Dim dpi As Double
    Private Function GetDpiFromVisual() As Double
        Dim source = PresentationSource.FromVisual(Me)
        If Not IsNothing(source?.CompositionTarget) Then
            Return source.CompositionTarget.TransformToDevice.M11
        End If
        Return 1
    End Function
    Public Property IsHide As Boolean
    Private Sub TimerTick(sender As Object, e As EventArgs)
        Dim p As New Spoint()
        If Not IsNothing(PVZ.Game) Then
            ClientToScreen(PVZ.Game.MainWindowHandle, p)
            If Not IsHide Then
                If IsIconic(PVZ.Game.MainWindowHandle) Then
                    Visibility = Visibility.Collapsed
                Else
                    Visibility = Visibility.Visible
                End If
            End If
        End If
        Top = p.y / dpi
        Left = p.x / dpi
        Height = 600 / dpi
        Width = 800 / dpi
        If PVZ.ZombiesCount <> prezombienum Then
            Canvas1.Children.Clear()
            For Each zombie In PVZ.AllZombies
                Dim tb As New TextBlock()
                '颜色
                tb.Foreground = New SolidColorBrush(hpfontcolor)
                tb.HorizontalAlignment = HorizontalAlignment.Center
                tb.FontWeight = FontWeights.Bold
                tb.Margin = New Thickness(3, 0, 3, 0)
                tb.IsHitTestVisible = False
                Dim b = New Border()
                b.BorderBrush = New SolidColorBrush(hpfontcolor)
                b.Child = tb
                Canvas1.Children.Add(b)
            Next
            prezombienum = PVZ.ZombiesCount
        End If
        Dim zblist = PVZ.AllZombies
        Dim blist = Canvas1.Children.Cast(Of Border)
        Dim tblist As List(Of TextBlock) = New List(Of TextBlock)()
        For Each b In blist
            tblist.Add(b.Child)
        Next
        If zblist.Length = tblist.Count Then
            For i = 0 To zblist.Length - 1
                tblist(i).Text = ""
                CType(tblist(i).Parent, Border).BorderThickness = New Thickness(0)
                If zblist(i).BodyHP <> zblist(i).MaxBodyHP And zblist(i).BodyHP <> 0 Then
                    tblist(i).Text += zblist(i).BodyHP.ToString()
                    tblist(i).Text += "/"
                    tblist(i).Text += zblist(i).MaxBodyHP.ToString()
                    CType(tblist(i).Parent, Border).BorderThickness = New Thickness(1)
                End If
                If zblist(i).AccessoriesType1HP <> zblist(i).MaxAccessoriesType1HP And zblist(i).AccessoriesType1HP <> 0 Then
                    If zblist(i).BodyHP <> zblist(i).MaxBodyHP Then
                        tblist(i).Text += vbCrLf
                    End If
                    tblist(i).Text += zblist(i).AccessoriesType1HP.ToString()
                    tblist(i).Text += "/"
                    tblist(i).Text += zblist(i).MaxAccessoriesType1HP.ToString()
                    CType(tblist(i).Parent, Border).BorderThickness = New Thickness(1)
                End If
                If zblist(i).AccessoriesType2HP <> zblist(i).MaxAccessoriesType2HP And zblist(i).AccessoriesType2HP <> 0 Then
                    If zblist(i).BodyHP <> zblist(i).MaxBodyHP Then
                        tblist(i).Text += vbCrLf
                    End If
                    If zblist(i).AccessoriesType1HP <> zblist(i).MaxAccessoriesType1HP Then
                        tblist(i).Text += vbCrLf
                    End If
                    tblist(i).Text += zblist(i).AccessoriesType2HP.ToString()
                    tblist(i).Text += "/"
                    tblist(i).Text += zblist(i).MaxAccessoriesType2HP.ToString()
                    CType(tblist(i).Parent, Border).BorderThickness = New Thickness(1)
                End If
                Canvas1.Children.Item(i).SetValue(Canvas.LeftProperty, Convert.ToDouble(zblist(i).ImageX + 10) / dpi)
                Canvas1.Children.Item(i).SetValue(Canvas.TopProperty, Convert.ToDouble(zblist(i).ImageY - 10) / dpi)
            Next
        End If
    End Sub

    Private Sub Window_Closed(sender As Object, e As EventArgs)
        Timer?.Stop()
    End Sub
End Class
