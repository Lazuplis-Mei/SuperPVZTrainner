Imports ITrainerExtension
Imports PVZClass

Public Class WaveManageDialog
    Private Sub Window_MouseDown(sender As Object, e As MouseButtonEventArgs)
        Try
            DragMove()
        Catch ex As InvalidOperationException
        End Try
    End Sub
    Private Sub Window_Closed(sender As Object, e As EventArgs)
        CType(Owner, ModifyWindow).BtnWaveManager.IsEnabled = True
    End Sub
    Private Sub NudAdvStage_ValueChanged(sender As Object, e As EventArgs)
        If IsLoaded Then
            Dim level = (NudAdvStage.Value - 1) * 10 + NudAdvLevel.Value - 1
            NudAdvWave.IgnoreAssign = True
            NudAdvWave.Value = PVZ.Memory.ReadInteger(&H6A34E8 + level * 4)
            NudAdvWave.IgnoreAssign = False
        End If
    End Sub
    Private Sub NudAdvWave_ValueChanged(sender As Object, e As EventArgs)
        If IsLoaded Then
            Dim level = (NudAdvStage.Value - 1) * 10 + NudAdvLevel.Value - 1
            PVZ.Memory.WriteInteger(&H6A34E8 + level * 4, NudAdvWave.Value)
        End If
    End Sub
    Private WavesList As Integer() = New Integer() {&H4092FD, &H40932C, &H4093F2, &H409466, &H409472, &H40947E, &H40948A, &H409499}
    Private TypesList As List(Of Integer) = New List(Of Integer)({&H409394, &H409326, &H4093EC, &H409460, &H40946C, &H409478, &H409489, &H409498})
    Private Sub CBWaveTypes_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
        If IsLoaded Then
            NudTypesWave.IgnoreAssign = True
            NudTypesWave.Value = PVZ.Memory.ReadInteger(WavesList(CBWaveTypes.SelectedIndex))
            NudTypesWave.IgnoreAssign = False
        End If
    End Sub
    Private Sub NudTypesWave_ValueChanged(sender As Object, e As EventArgs)
        If IsLoaded Then
            PVZ.Memory.WriteInteger(WavesList(CBWaveTypes.SelectedIndex), NudTypesWave.Value)
        End If
    End Sub
    Private Sub NudAdvWaveCmp_ValueChanged(sender As Object, e As EventArgs)
        If IsLoaded Then
            PVZ.Memory.WriteByte(&H409391, NudAdvWaveCmp.Value)
        End If
    End Sub
    Private Sub NudAdvWaveAdd_ValueChanged(sender As Object, e As EventArgs)
        If IsLoaded Then
            PVZ.Memory.WriteByte(&H4093A1, NudAdvWaveAdd.Value)
        End If
    End Sub
    Private LevelsList As Integer() = New Integer() {&H4093C0, &H4093FE, &H409403, &H409408, &H40940D, &H409412,
        &H409417, &H409420, &H40942D, &H409436, &H40943F, &H409444, &H40944F, &H409454, &H409459, &H40945E}
    Private Sub CBLevels_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
        If IsLoaded Then
            Dim des = PVZ.GetJumpDestination(LevelsList(CBLevels.SelectedIndex))
            CBLevelTypes.Tag = True
            CBLevelTypes.SelectedIndex = TypesList.IndexOf(des)
            CBLevelTypes.Tag = False
        End If
    End Sub
    Private Sub CBLevelTypes_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
        If IsLoaded AndAlso CBLevelTypes.Tag <> True Then
            If Not PVZ.SetJumpDestination(LevelsList(CBLevels.SelectedIndex), TypesList(CBLevelTypes.SelectedIndex)) Then
                If Lang.Id = 1 Then
                    MessageBox.Show("code distance too far,short jump does not support this modification", "Information", MessageBoxButton.OK, MessageBoxImage.Information)
                Else
                    MessageBox.Show("指令距离太远,短程jmp不支持此项修改", "信息", MessageBoxButton.OK, MessageBoxImage.Information)
                End If
            End If
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
            Height = 215 * scale / 100
            Width = 400 * scale / 100
        End If
    End Sub

    Private Sub Window_Loaded(sender As Object, e As RoutedEventArgs)
        Lang.ChangeLanguage(Content)
    End Sub
End Class
