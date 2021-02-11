Imports ITrainerExtension
Imports PVZClass
Public Class ZombieListDialog
    Private currentWave As PVZ.ZombieList.Wave
    Private Sub Window_MouseDown(sender As Object, e As MouseButtonEventArgs)
        Try
            DragMove()
        Catch ex As InvalidOperationException
        End Try
    End Sub
    Private Sub Window_Closed(sender As Object, e As EventArgs)
        CType(Owner, ModifyWindow).BtnZombieList.IsEnabled = True
    End Sub
    Private Sub Window_Loaded(sender As Object, e As RoutedEventArgs)
        LBZombieSeed.Items.Clear()
        For Each zseed In PVZ.ZombieSeed
            If Lang.Id Then
                LBZombieSeed.Items.Add(zseed.ToString())
            Else
                LBZombieSeed.Items.Add(zseed.GetDescription())
            End If
        Next
        NudWaveNum.MaxValue = PVZ.WaveNum
        Lang.ChangeLanguage(Content)
    End Sub
    Private Sub NudWaveNum_ValueChanged(sender As Object, e As EventArgs)
        If IsLoaded Then
            currentWave = PVZ.ZombieList.GetWave(NudWaveNum.Value)
            FlushListItem()
        End If
    End Sub
    Private Sub LBZombieLst_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
        If IsLoaded AndAlso LBZombieLst.SelectedIndex >= 0 Then
            CBZombieTypes.IsReadOnly = True
            CBZombieTypes.SelectedIndex = CType(LBZombieLst.SelectedItem, ListBoxItem).Tag
            CBZombieTypes.IsReadOnly = False
        End If
    End Sub
    Private Sub CBZombieTypes_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
        If IsLoaded AndAlso CBZombieTypes.IsReadOnly <> True AndAlso LBZombieLst.SelectedIndex >= 0 Then
            Dim type = CType(CBZombieTypes.SelectedIndex, PVZ.ZombieType)
            currentWave?.Set­(LBZombieLst.SelectedIndex, type)
            FlushListItem()
        End If
    End Sub
    Private Sub AddZombie_Click(sender As Object, e As RoutedEventArgs)
        If CBZombieTypesAdd.SelectedIndex >= 0 Then
            Dim type = CType(CBZombieTypesAdd.SelectedIndex, PVZ.ZombieType)
            currentWave?.Add(type)
            FlushListItem()
        End If
    End Sub
    Private Sub DelSelZombie_Click(sender As Object, e As RoutedEventArgs)
        If LBZombieLst.SelectedIndex >= 0 Then
            Dim type = CType(CBZombieTypes.SelectedIndex, PVZ.ZombieType)
            currentWave?.Del(LBZombieLst.SelectedIndex)
            FlushListItem()
        End If
    End Sub
    Private Sub ClearZombies_Click(sender As Object, e As RoutedEventArgs)
        currentWave?.Set­(0, -1)
        FlushListItem()
    End Sub
    Private Sub FlushListItem()
        LBZombieLst.Items.Clear()
        If IsNothing(currentWave) Then Return
        For Each zombie In currentWave.All
            Dim item = New ListBoxItem()
            If Lang.Id Then
                item.Content = zombie.ToString()
            Else
                item.Content = zombie.GetDescription()
            End If
            item.Tag = zombie
            LBZombieLst.Items.Add(item)
        Next
    End Sub
    Private Sub FlushData_Click(sender As Object, e As RoutedEventArgs)
        Window_Loaded(Nothing, Nothing)
        currentWave = PVZ.ZombieList.GetWave(NudWaveNum.Value)
        FlushListItem()
    End Sub
    Private Sub LBZombieLst_PreviewKeyDown(sender As Object, e As KeyEventArgs)
        If e.Key = Key.Delete Then
            DelSelZombie_Click(Nothing, Nothing)
        ElseIf e.Key = Key.A AndAlso Keyboard.Modifiers = ModifierKeys.Control Then
            AddZombie_Click(Nothing, Nothing)
        End If
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
            Height = 400 * scale / 100
            Width = 420 * scale / 100
        End If
    End Sub
End Class
