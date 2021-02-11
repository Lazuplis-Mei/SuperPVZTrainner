Public Class InputDialog
    Public Property Value As Double
    Public Sub New(title As String, desc As String, Optional minval As Long = 0, Optional maxval As Long = 1000)
        ' 此调用是设计器所必需的。
        InitializeComponent()

        ' 在 InitializeComponent() 调用之后添加任何初始化。
        TBTitle.Text = title
        NudInput.MinValue = minval
        NudInput.MaxValue = maxval
        If ITrainerExtension.Lang.Id = 1 Then
            TBDesc.Text = desc + $"Range({minval}-{maxval})"
            BtnOK.Content = "Ok"
            BtnCancel.Content = "Cancel"
        Else
            TBDesc.Text = desc + $"范围({minval}-{maxval})"
            BtnOK.Content = "确认"
            BtnCancel.Content = "取消"
        End If
    End Sub

    Private Sub Window_MouseDown(sender As Object, e As MouseButtonEventArgs)
        Try
            DragMove()
        Catch ex As InvalidOperationException
        End Try
    End Sub
    Private Sub MyButton_Click(sender As Object, e As RoutedEventArgs)
        Value = NudInput.Value
        DialogResult = True
    End Sub
    Private Sub MyButton_Click_1(sender As Object, e As RoutedEventArgs)
        DialogResult = False
        Close()
    End Sub

    Public scale As Integer = 100

    Private Sub Window_PreviewMouseWheel(sender As Object, e As MouseWheelEventArgs)
        If Keyboard.Modifiers = ModifierKeys.Control Then
            If e.Delta > 0 Then
                scale += 5
            Else
                scale -= 5
            End If
            scale = Math.Max(10, scale)
            scale = Math.Min(300, scale)
            Dim con As UIElement = Content
            con.RenderTransform = New ScaleTransform(scale / 100, scale / 100)
            Height = 180 * scale / 100
            Width = 400 * scale / 100
        End If
    End Sub
End Class
