Imports System.ComponentModel
Imports ITrainerExtension

Public Class SeparateWindow
    Private Sub Window_MouseDown(sender As Object, e As MouseButtonEventArgs)
        Try
            DragMove()
        Catch ex As InvalidOperationException
        End Try
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
            Height = 470 * scale / 100
            Width = 400 * scale / 100
        End If
    End Sub
    Private Sub Window_Loaded(sender As Object, e As RoutedEventArgs)
        Lang.ChangeLanguage(Content)
    End Sub

    Private Sub Window_Closing(sender As Object, e As CancelEventArgs)
        Dim item As TabItem = TCMain.Items(0)
        TCMain.Items.RemoveAt(0)
        Dim tcicollection = CType(item.Tag, TabControl).Items
        tcicollection.Add(item)
        tcicollection.SortDescriptions.Clear()
        tcicollection.SortDescriptions.Add(New SortDescription("TabIndex", ListSortDirection.Ascending))
        tcicollection.Refresh()
    End Sub
End Class
