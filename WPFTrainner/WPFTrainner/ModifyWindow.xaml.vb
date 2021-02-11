Imports ITrainerExtension
Imports PVZClass

Public Class ModifyWindow
    Private Function DealKeyDown(sender As Object, e As KeyEventArgs) As Boolean
        If e.Key = Key.Space Then
            sender.Text = "0"
            e.Handled = True
        ElseIf e.Key = Key.V Then
            e.Handled = True
        End If
        Return e.Key = Key.Enter
    End Function
    Private Sub Window_MouseDown(sender As Object, e As MouseButtonEventArgs)
        Try
            DragMove()
        Catch ex As InvalidOperationException
        End Try
    End Sub
    Private Sub Window_Closed(sender As Object, e As EventArgs)
        If Not IsNothing(Application.Current.MainWindow) Then
            CType(Application.Current.MainWindow, MainWindow).BtnModify.IsEnabled = True
        End If
    End Sub
    Private Sub NudAdvStage_ValueChanged(sender As Object, e As EventArgs)
        If IsLoaded Then
            If NudAdvStage.Value = 6 Then
                NudAdvLevel.MaxValue = Int32.MaxValue - 50
            Else
                NudAdvLevel.Value = Math.Min(NudAdvLevel.Value, 10)
                NudAdvLevel.MaxValue = 10
            End If
        End If
    End Sub
    Dim jlText1 As String() = {"非常抱歉,冒险模式1-1不能在关内跳关", "sorry, Adventure mode 1-1 can't jump level while playing"}
    Dim jlText2 As String() = {"跳关已设置,请点击游戏中右上角的菜单按钮", "Function set,Please click the Main menu button in the upper right corner of the game"}
    Private Sub BtnJumpLevel_Click(sender As Object, e As RoutedEventArgs)
        Dim advLevel As Integer = 0
        Dim gameLevel As Byte = 0
        GetLevel(advLevel, gameLevel)
        Dim msg = PVZ.JumpLevel(advLevel, gameLevel)
        If msg = 0 Then
            MessageBox.Show(jlText1(Lang.Id), "Information", MessageBoxButton.OK, MessageBoxImage.Information)
        ElseIf msg = 1 Then
            MessageBox.Show(jlText2(Lang.Id), "Information", MessageBoxButton.OK, MessageBoxImage.Information)
        End If
    End Sub
    Private Sub GetLevel(ByRef advLevel As Integer, ByRef gameLevel As Byte)
        If RBAdventure.IsChecked Then
            advLevel = 10 * (NudAdvStage.Value - 1) + NudAdvLevel.Value
        ElseIf RBSurvival.IsChecked Then
            advLevel = PVZ.SaveData.AdventureLevel
            gameLevel = CBLevelSurvival.SelectedIndex + 1
        ElseIf RBMiniGames.IsChecked Then
            advLevel = PVZ.SaveData.AdventureLevel
            gameLevel = CBLevelMini.SelectedIndex + 16
        ElseIf RBMiniHidden.IsChecked Then
            advLevel = PVZ.SaveData.AdventureLevel
            If CBLevelMiniHidden.SelectedIndex >= 15 Then
                gameLevel = CBLevelMiniHidden.SelectedIndex + 56
            Else
                gameLevel = CBLevelMiniHidden.SelectedIndex + 36
            End If
        ElseIf RBPuzzle.IsChecked Then
            advLevel = PVZ.SaveData.AdventureLevel
            If CBLevelPuzzle.SelectedIndex >= 9 Then
                gameLevel = CBLevelPuzzle.SelectedIndex + 52
            Else
                gameLevel = CBLevelPuzzle.SelectedIndex + 51
            End If
        ElseIf RBEndless.IsChecked Then
            advLevel = PVZ.SaveData.AdventureLevel
            If CBLevelEndless.SelectedIndex = 5 Then
                gameLevel = 60
            ElseIf CBLevelEndless.SelectedIndex = 6 Then
                gameLevel = 70
            Else
                gameLevel = CBLevelEndless.SelectedIndex + 11
            End If
        End If
    End Sub
    Private Sub BtnToLevel_Click(sender As Object, e As RoutedEventArgs)
        Dim advLevel As Integer = 0
        Dim gameLevel As Byte = 0
        GetLevel(advLevel, gameLevel)
        PVZ.AdventureLevel = advLevel
        PVZ.SaveData.AdventureLevel = advLevel
        PVZ.LevelId = gameLevel
    End Sub
    Private Sub BtnGoldenSave_Click(sender As Object, e As RoutedEventArgs)
        PVZ.SaveData.AdventureFinishCount = Math.Max(1, PVZ.SaveData.AdventureFinishCount)
        Dim addr As Integer
        addr = PVZ.SaveData.BaseAddress + &H30
        Dim finishedCount As Integer
        For index = 0 To 4
            finishedCount = PVZ.Memory.ReadInteger(addr + 4 * index)
            PVZ.Memory.WriteInteger(addr + 4 * index, Math.Max(finishedCount, 5))
        Next
        For index = 5 To 9
            finishedCount = PVZ.Memory.ReadInteger(addr + 4 * index)
            PVZ.Memory.WriteInteger(addr + 4 * index, Math.Max(finishedCount, 10))
        Next
        addr = PVZ.SaveData.BaseAddress + &H6C
        For index = 0 To 34
            finishedCount = PVZ.Memory.ReadInteger(addr + 4 * index)
            PVZ.Memory.WriteInteger(addr + 4 * index, Math.Max(finishedCount, 1))
        Next
        addr = PVZ.SaveData.BaseAddress + &HF8
        For index = 0 To 8
            finishedCount = PVZ.Memory.ReadInteger(addr + 4 * index)
            PVZ.Memory.WriteInteger(addr + 4 * index, Math.Max(finishedCount, 1))
        Next
        addr = PVZ.SaveData.BaseAddress + &H120
        For index = 0 To 8
            finishedCount = PVZ.Memory.ReadInteger(addr + 4 * index)
            PVZ.Memory.WriteInteger(addr + 4 * index, Math.Max(finishedCount, 1))
        Next
        finishedCount = PVZ.Memory.ReadInteger(PVZ.SaveData.BaseAddress + &H148)
        PVZ.Memory.WriteInteger(PVZ.SaveData.BaseAddress + &H148, Math.Max(finishedCount, 1))
        finishedCount = PVZ.Memory.ReadInteger(PVZ.SaveData.BaseAddress + &H14C)
        PVZ.Memory.WriteInteger(PVZ.SaveData.BaseAddress + &H14C, Math.Max(finishedCount, 1))
    End Sub
    Private Sub BtnWin_Click(sender As Object, e As RoutedEventArgs)
        If PVZ.ExitingLevelCountDown = -1 Then
            PVZ.Win()
        End If
    End Sub
    Private Sub DaveSelCardNum_Click(sender As Object, e As RoutedEventArgs)
        Dim flag = Lang.Id = 0
        Dim inputDlg = New InputDialog(
            IIf(flag, "请输入数值", "Please enter a value"),
            IIf(flag, "选卡张数", "Number of cards"), 1, 10)
        If inputDlg.ShowDialog() Then
            PVZ.Memory.WriteByte(&H48420B, inputDlg.Value)
        End If
    End Sub
    Private Sub BtnCreatePortal_Click(sender As Object, e As RoutedEventArgs)
        PVZ.CreatePortal(NvdBlack1Y.Value, NvdBlack1X.Value,
                         NvdBlack2Y.Value, NvdBlack2X.Value,
                         NvdBlue1Y.Value, NvdBlue1X.Value,
                         NvdBlue2Y.Value, NvdBlue2X.Value)
    End Sub
    Private Sub GraveAppearWave_Click(sender As Object, e As RoutedEventArgs)
        Dim flag = Lang.Id = 0
        Dim inputDlg = New InputDialog(
            IIf(flag, "请输入数值", "Please enter a value"),
            IIf(flag, "墓碑出现的波数", "Grave appear at wave"), 3, Byte.MaxValue)
        If inputDlg.ShowDialog() Then
            PVZ.Memory.WriteByte(&H426925, inputDlg.Value - 2)
        End If
    End Sub
    Private Sub CBLockIZEFormat_Click(sender As Object, e As RoutedEventArgs)
        If CBLockIZEFormat.IsChecked Then
            CBIZEFormat.IsEnabled = False
            If CBIZEFormat.SelectedIndex = 0 Then
                PVZ.Memory.WriteByteArray(&H42B046, &HF, &H81, &H68, &H1, &H0, &H0)
            ElseIf CBIZEFormat.SelectedIndex = 1 Then
                PVZ.Memory.WriteByteArray(&H42B046, &HF, &H81, &HF9, &H0, &H0, &H0)
            ElseIf CBIZEFormat.SelectedIndex = 2 Then
                PVZ.Memory.WriteByteArray(&H42B046, &HF, &H81, &HBE, &H0, &H0, &H0)
            ElseIf CBIZEFormat.SelectedIndex = 3 Then
                PVZ.Memory.WriteByteArray(&H42B046, &HF, &H81, &H18, &H0, &H0, &H0)
            ElseIf CBIZEFormat.SelectedIndex = 4 Then
                PVZ.Memory.WriteByteArray(&H42B046, &HF, &H81, &H90, &HFD, &HFF, &HFF)
            ElseIf CBIZEFormat.SelectedIndex = 5 Then
                PVZ.Memory.WriteByteArray(&H42B046, &HF, &H81, &H49, &H0, &H0, &H0)
            ElseIf CBIZEFormat.SelectedIndex = 6 Then
                PVZ.Memory.WriteByteArray(&H42B046, &HF, &H81, &H64, &H0, &H0, &H0)
            ElseIf CBIZEFormat.SelectedIndex = 7 Then
                PVZ.Memory.WriteByteArray(&H42B046, &HF, &H81, &H7A, &H0, &H0, &H0)
            End If
        Else
            CBIZEFormat.IsEnabled = True
            PVZ.Memory.WriteByteArray(&H42B046, &HF, &H85, &H90, &H0, &H0, &H0)
        End If
    End Sub
    Private Sub CBPlantStaticProp_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
        If IsLoaded Then
            If CBPlantStaticProp.SelectedIndex = 0 Then
                TBPlantStaicProp.Text = PVZ.Memory.ReadInteger(&H69F2B8 + CBPlantTypes.SelectedIndex * &H24)
            Else
                TBPlantStaicProp.Text = PVZ.Memory.ReadInteger(&H69F2BC + CBPlantStaticProp.SelectedIndex * 4 + CBPlantTypes.SelectedIndex * &H24)
            End If
        End If
    End Sub
    Private Sub TBPlantStaicProp_PreviewKeyDown(sender As Object, e As KeyEventArgs)
        If DealKeyDown(sender, e) Then
            If CBPlantStaticProp.SelectedIndex = 0 Then
                PVZ.Memory.WriteInteger(&H69F2B8 + CBPlantTypes.SelectedIndex * &H24, TBPlantStaicProp.Text)
            Else
                PVZ.Memory.WriteInteger(&H69F2BC + CBPlantStaticProp.SelectedIndex * 4 + CBPlantTypes.SelectedIndex * &H24, TBPlantStaicProp.Text)
            End If
        End If
    End Sub
    Private Sub CBPlantTimeProp_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
        If IsLoaded Then
            If CBPlantTimeProp.SelectedIndex = 0 Then
                TBPlantTimeProp.Text = PVZ.Memory.ReadInteger(&H45E300)
            ElseIf CBPlantTimeProp.SelectedIndex = 1 Then
                TBPlantTimeProp.Text = PVZ.Memory.ReadInteger(&H45E34E)
            ElseIf CBPlantTimeProp.SelectedIndex = 2 Then
                TBPlantTimeProp.Text = PVZ.Memory.ReadInteger(&H4613BC)
            ElseIf CBPlantTimeProp.SelectedIndex = 3 Then
                TBPlantTimeProp.Text = PVZ.Memory.ReadInteger(&H461551)
            ElseIf CBPlantTimeProp.SelectedIndex = 4 Then
                TBPlantTimeProp.Text = PVZ.Memory.ReadInteger(&H45E3F1)
            ElseIf CBPlantTimeProp.SelectedIndex = 5 Then
                TBPlantTimeProp.Text = PVZ.Memory.ReadInteger(&H45FCE3)
            ElseIf CBPlantTimeProp.SelectedIndex = 6 Then
                TBPlantTimeProp.Text = PVZ.Memory.ReadInteger(&H4632B0)
            ElseIf CBPlantTimeProp.SelectedIndex = 7 Then
                TBPlantTimeProp.Text = PVZ.Memory.ReadInteger(&H460DFE)
            ElseIf CBPlantTimeProp.SelectedIndex = 8 Then
                TBPlantTimeProp.Text = PVZ.Memory.ReadInteger(&H460A3D)
            ElseIf CBPlantTimeProp.SelectedIndex = 9 Then
                TBPlantTimeProp.Text = PVZ.Memory.ReadInteger(&H460AF1)
            ElseIf CBPlantTimeProp.SelectedIndex = 10 Then
                TBPlantTimeProp.Text = PVZ.Memory.ReadInteger(&H460B56)
            ElseIf CBPlantTimeProp.SelectedIndex = 11 Then
                TBPlantTimeProp.Text = PVZ.Memory.ReadInteger(&H460C53)
            ElseIf CBPlantTimeProp.SelectedIndex = 12 Then
                TBPlantTimeProp.Text = PVZ.Memory.ReadInteger(&H460D21)
            ElseIf CBPlantTimeProp.SelectedIndex = 13 Then
                TBPlantTimeProp.Text = PVZ.Memory.ReadInteger(&H4600F1)
            ElseIf CBPlantTimeProp.SelectedIndex = 14 Then
                TBPlantTimeProp.Text = PVZ.Memory.ReadInteger(&H45DF05)
            ElseIf CBPlantTimeProp.SelectedIndex = 15 Then
                TBPlantTimeProp.Text = PVZ.Memory.ReadInteger(&H46163A)
            ElseIf CBPlantTimeProp.SelectedIndex = 16 Then
                TBPlantTimeProp.Text = PVZ.Memory.ReadInteger(&H45E521)
            ElseIf CBPlantTimeProp.SelectedIndex = 17 Then
                TBPlantTimeProp.Text = PVZ.Memory.ReadInteger(&H45E560)
            ElseIf CBPlantTimeProp.SelectedIndex = 18 Then
                TBPlantTimeProp.Text = PVZ.Memory.ReadInteger(&H464D4D)
            End If
        End If
    End Sub
    Private Sub TBPlantTimeProp_PreviewKeyDown(sender As Object, e As KeyEventArgs)
        If DealKeyDown(sender, e) Then
            If CBPlantTimeProp.SelectedIndex = 0 Then
                PVZ.Memory.WriteInteger(&H45E300, TBPlantTimeProp.Text)
            ElseIf CBPlantTimeProp.SelectedIndex = 1 Then
                PVZ.Memory.WriteInteger(&H45E34E, TBPlantTimeProp.Text)
            ElseIf CBPlantTimeProp.SelectedIndex = 2 Then
                PVZ.Memory.WriteInteger(&H4613BC, TBPlantTimeProp.Text)
            ElseIf CBPlantTimeProp.SelectedIndex = 3 Then
                PVZ.Memory.WriteInteger(&H461551, TBPlantTimeProp.Text)
            ElseIf CBPlantTimeProp.SelectedIndex = 4 Then
                PVZ.Memory.WriteInteger(&H45E3F1, TBPlantTimeProp.Text)
            ElseIf CBPlantTimeProp.SelectedIndex = 5 Then
                PVZ.Memory.WriteInteger(&H45FCE3, TBPlantTimeProp.Text)
            ElseIf CBPlantTimeProp.SelectedIndex = 6 Then
                PVZ.Memory.WriteInteger(&H4632B0, TBPlantTimeProp.Text)
                Dim mt = PVZ.Memory.ReadInteger(&H45DC5F)
                PVZ.Memory.WriteInteger(&H45DC5F, Math.Max(mt, CInt(TBPlantTimeProp.Text)))
            ElseIf CBPlantTimeProp.SelectedIndex = 7 Then
                PVZ.Memory.WriteInteger(&H460DFE, TBPlantTimeProp.Text)
                Dim mt = PVZ.Memory.ReadInteger(&H45DC5F)
                PVZ.Memory.WriteInteger(&H45DC5F, Math.Max(mt, CInt(TBPlantTimeProp.Text)))
            ElseIf CBPlantTimeProp.SelectedIndex = 8 Then
                PVZ.Memory.WriteInteger(&H460A3D, TBPlantTimeProp.Text)
            ElseIf CBPlantTimeProp.SelectedIndex = 9 Then
                PVZ.Memory.WriteInteger(&H460AF1, TBPlantTimeProp.Text)
            ElseIf CBPlantTimeProp.SelectedIndex = 10 Then
                PVZ.Memory.WriteInteger(&H460B56, TBPlantTimeProp.Text)
            ElseIf CBPlantTimeProp.SelectedIndex = 11 Then
                PVZ.Memory.WriteInteger(&H460C53, TBPlantTimeProp.Text)
            ElseIf CBPlantTimeProp.SelectedIndex = 12 Then
                PVZ.Memory.WriteInteger(&H460D21, TBPlantTimeProp.Text)
            ElseIf CBPlantTimeProp.SelectedIndex = 13 Then
                PVZ.Memory.WriteInteger(&H4600F1, TBPlantTimeProp.Text)
            ElseIf CBPlantTimeProp.SelectedIndex = 14 Then
                PVZ.Memory.WriteInteger(&H45DF05, TBPlantTimeProp.Text)
            ElseIf CBPlantTimeProp.SelectedIndex = 15 Then
                PVZ.Memory.WriteInteger(&H46163A, TBPlantTimeProp.Text)
            ElseIf CBPlantTimeProp.SelectedIndex = 16 Then
                PVZ.Memory.WriteInteger(&H45E521, TBPlantTimeProp.Text)
            ElseIf CBPlantTimeProp.SelectedIndex = 17 Then
                PVZ.Memory.WriteInteger(&H45E560, TBPlantTimeProp.Text)
            ElseIf CBPlantTimeProp.SelectedIndex = 18 Then
                PVZ.Memory.WriteInteger(&H464D4D, TBPlantTimeProp.Text)
            End If
        End If
    End Sub
    Private Sub CBPlantHp_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
        If IsLoaded Then
            If CBPlantHp.SelectedIndex = 0 Then
                TBPlantHp.Text = PVZ.Memory.ReadInteger(&H45DC55)
            ElseIf CBPlantHp.SelectedIndex = 1 Then
                TBPlantHp.Text = PVZ.Memory.ReadInteger(&H45E1A7)
            ElseIf CBPlantHp.SelectedIndex = 2 Then
                TBPlantHp.Text = PVZ.Memory.ReadInteger(&H45E215)
            ElseIf CBPlantHp.SelectedIndex = 3 Then
                TBPlantHp.Text = PVZ.Memory.ReadInteger(&H45E445)
            ElseIf CBPlantHp.SelectedIndex = 4 Then
                TBPlantHp.Text = PVZ.Memory.ReadInteger(&H45E242)
            ElseIf CBPlantHp.SelectedIndex = 5 Then
                TBPlantHp.Text = PVZ.Memory.ReadInteger(&H45E5C3)
            End If
        End If
    End Sub
    Private Sub TBPlantHp_PreviewKeyDown(sender As Object, e As KeyEventArgs)
        If DealKeyDown(sender, e) Then
            If CBPlantHp.SelectedIndex = 0 Then
                PVZ.Memory.WriteInteger(&H45DC55, TBPlantHp.Text)
            ElseIf CBPlantHp.SelectedIndex = 1 Then
                PVZ.Memory.WriteInteger(&H45E1A7, TBPlantHp.Text)
            ElseIf CBPlantHp.SelectedIndex = 2 Then
                PVZ.Memory.WriteInteger(&H45E215, TBPlantHp.Text)
            ElseIf CBPlantHp.SelectedIndex = 3 Then
                PVZ.Memory.WriteInteger(&H45E445, TBPlantHp.Text)
            ElseIf CBPlantHp.SelectedIndex = 4 Then
                PVZ.Memory.WriteInteger(&H45E242, TBPlantHp.Text)
            ElseIf CBPlantHp.SelectedIndex = 5 Then
                PVZ.Memory.WriteInteger(&H45E5C3, TBPlantHp.Text)
            End If
        End If
    End Sub
    Private Sub CBPlantInvincible_Click(sender As Object, e As RoutedEventArgs)
        PlantInvincibleResume()
        If CBPlantInvincible.IsChecked Then
            CBPlantWeak.IsEnabled = False
            If MIPlantBiteProof.IsChecked Then
                PVZ.Memory.WriteByte(&H52FCF3, 0)
            End If
            If MIPlantBlastProof.IsChecked Then
                PVZ.Memory.WriteByte(&H41CC2F, 235)
            End If
            If MIPlantRollProof.IsChecked Then
                PVZ.Memory.WriteByte(&H45EC66, 0)
                PVZ.Memory.WriteByte(&H45EE0A, 112)
                PVZ.Memory.WriteByte(&H52E93B, 235)
                PVZ.Memory.WriteInteger(&H462B80, 1811940546)
            End If
            If MIPlantHitProof.IsChecked Then
                PVZ.Memory.WriteInteger(&H46CFEB, -2087677808)
                PVZ.Memory.WriteInteger(&H46D7A6, -2087677808)
            End If
            If MIPlantBurnProof.IsChecked Then
                PVZ.Memory.WriteByte(&H5276EA, 235)
            End If
        Else
            CBPlantWeak.IsEnabled = True
        End If
    End Sub
    Private Shared Sub PlantInvincibleResume()
        PVZ.Memory.WriteByte(&H41CC2F, 116)
        PVZ.Memory.WriteByte(&H45EC66, 224)
        PVZ.Memory.WriteByte(&H45EE0A, 117)
        PVZ.Memory.WriteInteger(&H46CFEB, -2092937175)
        PVZ.Memory.WriteInteger(&H46D7A6, -2092937687)
        PVZ.Memory.WriteByte(&H5276EA, 117)
        PVZ.Memory.WriteByte(&H52E93B, 116)
        PVZ.Memory.WriteByte(&H52FCF3, 252)
        PVZ.Memory.WriteByte(&H52FCF1, 70)
        PVZ.Memory.WriteInteger(&H462B80, 1821070675)
    End Sub
    Private Sub CBPlantWeak_Click(sender As Object, e As RoutedEventArgs)
        PlantInvincibleResume()
        If CBPlantWeak.IsChecked Then
            CBPlantInvincible.IsEnabled = False
            PVZ.Memory.WriteByte(&H45EE0A, 112)
            PVZ.Memory.WriteByte(&H46CFEC, 64)
            PVZ.Memory.WriteByte(&H46D7A7, 118)
            PVZ.Memory.WriteByte(&H52FCF3, 0)
            PVZ.Memory.WriteByte(&H52FCF1, 102)
        Else
            CBPlantInvincible.IsEnabled = True
        End If
    End Sub
    Private Sub CBZombieStaticProp_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
        If IsLoaded Then
            If CBZombieStaticProp.SelectedIndex = 0 Then
                TBZombieStaicProp.Text = PVZ.Memory.ReadInteger(&H69DA84 + CBZombieTypes.SelectedIndex * &H1C)
            ElseIf CBZombieStaticProp.SelectedIndex = 1 Then
                TBZombieStaicProp.Text = PVZ.Memory.ReadInteger(&H69DA88 + CBZombieTypes.SelectedIndex * &H1C)
            ElseIf CBZombieStaticProp.SelectedIndex = 2 Then
                TBZombieStaicProp.Text = PVZ.Memory.ReadInteger(&H69DA94 + CBZombieTypes.SelectedIndex * &H1C)
            End If
        End If
    End Sub
    Private Sub TBZombieStaicProp_PreviewKeyDown(sender As Object, e As KeyEventArgs)
        If DealKeyDown(sender, e) Then
            If CBZombieStaticProp.SelectedIndex = 0 Then
                PVZ.Memory.WriteInteger(&H69DA84 + CBZombieTypes.SelectedIndex * &H1C, TBZombieStaicProp.Text)
            ElseIf CBZombieStaticProp.SelectedIndex = 1 Then
                PVZ.Memory.WriteInteger(&H69DA88 + CBZombieTypes.SelectedIndex * &H1C, TBZombieStaicProp.Text)
            ElseIf CBZombieStaticProp.SelectedIndex = 2 Then
                PVZ.Memory.WriteInteger(&H69DA94 + CBZombieTypes.SelectedIndex * &H1C, TBZombieStaicProp.Text)
            End If
        End If
    End Sub
    Private Sub CBZombieCardSun_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
        If IsLoaded Then
            If CBZombieCardSun.SelectedIndex < 7 Then
                TBZombieCardSun.Text = PVZ.Memory.ReadInteger(&H467B60 + CBZombieCardSun.SelectedIndex * 6)
            ElseIf CBZombieCardSun.SelectedIndex = 7 Then
                TBZombieCardSun.Text = PVZ.Memory.ReadInteger(&H467B3D)
            ElseIf CBZombieCardSun.SelectedIndex = 8 Then
                TBZombieCardSun.Text = PVZ.Memory.ReadInteger(&H467B48)
            End If
        End If
    End Sub
    Private Sub TBZombieCardSun_PreviewKeyDown(sender As Object, e As KeyEventArgs)
        If DealKeyDown(sender, e) Then
            If (CBZombieCardSun.SelectedIndex < 7) Then
                PVZ.Memory.WriteInteger(&H467B60 + CBZombieCardSun.SelectedIndex * 6, TBZombieCardSun.Text)
            ElseIf CBZombieCardSun.SelectedIndex = 7 Then
                PVZ.Memory.WriteInteger(&H467B3D, TBZombieCardSun.Text)
            ElseIf CBZombieCardSun.SelectedIndex = 8 Then
                PVZ.Memory.WriteInteger(&H467B48, TBZombieCardSun.Text)
            End If
        End If
    End Sub
    Private Sub CBZombieCardId_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
        If IsLoaded Then
            If PVZ.Memory.ReadInteger(&H42A046) = -125582400 Then
                PVZ.Memory.WriteQword(&H42A03E, 202333581277315)
                PVZ.Memory.WriteShort(&H42A046, 0)
                PVZ.Memory.WriteByte(&H4661BE, 124)
                PVZ.Memory.WriteByte(&H42A41A, 32)
            End If
            If CBZombieCardId.SelectedIndex = 0 Then
                CBZombieTypes2.SelectedIndex = PVZ.Memory.ReadInteger(&H42A044)
            Else
                CBZombieTypes2.SelectedIndex = PVZ.Memory.ReadInteger(&H42A04E + (CBZombieCardId.SelectedIndex - 1) * 11)
            End If
        End If
    End Sub
    Private Sub CBZombieTypes2_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
        If CBZombieTypes2.IsMouseOver Then
            If PVZ.Memory.ReadInteger(&H42A046) = -125582400 Then
                PVZ.Memory.WriteQword(&H42A03E, 202333581277315)
                PVZ.Memory.WriteShort(&H42A046, 0)
                PVZ.Memory.WriteByte(&H4661BE, 124)
                PVZ.Memory.WriteByte(&H42A41A, 32)
            End If
            If CBZombieCardId.SelectedIndex = 0 Then
                PVZ.Memory.WriteInteger(&H42A044, CBZombieTypes2.SelectedIndex)
            Else
                PVZ.Memory.WriteInteger(&H42A04E + (CBZombieCardId.SelectedIndex - 1) * 11, CBZombieTypes2.SelectedIndex)
            End If
        End If
    End Sub
    Private Sub CBZombieTimeProp_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
        If IsLoaded Then
            If CBZombieTimeProp.SelectedIndex = 0 Then
                TBZombieTimeProp.Text = PVZ.Memory.ReadInteger(&H52350A)
            ElseIf CBZombieTimeProp.SelectedIndex = 1 Then
                TBZombieTimeProp.Text = PVZ.Memory.ReadInteger(&H528EB7)
            ElseIf CBZombieTimeProp.SelectedIndex = 2 Then
                TBZombieTimeProp.Text = PVZ.Memory.ReadInteger(&H523160)
            ElseIf CBZombieTimeProp.SelectedIndex = 3 Then
                TBZombieTimeProp.Text = PVZ.Memory.ReadInteger(&H522FBD)
            ElseIf CBZombieTimeProp.SelectedIndex = 4 Then
                TBZombieTimeProp.Text = PVZ.Memory.ReadInteger(&H522FDB)
            ElseIf CBZombieTimeProp.SelectedIndex = 5 Then
                TBZombieTimeProp.Text = PVZ.Memory.ReadInteger(&H522FE0)
            ElseIf CBZombieTimeProp.SelectedIndex = 6 Then
                TBZombieTimeProp.Text = PVZ.Memory.ReadInteger(&H528355)
            ElseIf CBZombieTimeProp.SelectedIndex = 7 Then
                TBZombieTimeProp.Text = PVZ.Memory.ReadInteger(&H5232A7)
            ElseIf CBZombieTimeProp.SelectedIndex = 8 Then
                TBZombieTimeProp.Text = PVZ.Memory.ReadInteger(&H525548)
            ElseIf CBZombieTimeProp.SelectedIndex = 9 Then
                TBZombieTimeProp.Text = PVZ.Memory.ReadInteger(&H522978)
            ElseIf CBZombieTimeProp.SelectedIndex = 10 Then
                TBZombieTimeProp.Text = PVZ.Memory.ReadInteger(&H525127)
            ElseIf CBZombieTimeProp.SelectedIndex = 11 Then
                TBZombieTimeProp.Text = PVZ.Memory.ReadInteger(&H525A28)
            ElseIf CBZombieTimeProp.SelectedIndex = 12 Then
                TBZombieTimeProp.Text = PVZ.Memory.ReadInteger(&H525B28)
            ElseIf CBZombieTimeProp.SelectedIndex = 13 Then
                TBZombieTimeProp.Text = PVZ.Memory.ReadInteger(&H523BD0)
            ElseIf CBZombieTimeProp.SelectedIndex = 14 Then
                TBZombieTimeProp.Text = PVZ.Memory.ReadInteger(&H5275B2)
            ElseIf CBZombieTimeProp.SelectedIndex = 15 Then
                TBZombieTimeProp.Text = PVZ.Memory.ReadInteger(&H523A7A)
            ElseIf CBZombieTimeProp.SelectedIndex = 16 Then
                TBZombieTimeProp.Text = PVZ.Memory.ReadInteger(&H523A91)
            ElseIf CBZombieTimeProp.SelectedIndex = 17 Then
                TBZombieTimeProp.Text = PVZ.Memory.ReadInteger(&H523BD0)
            ElseIf CBZombieTimeProp.SelectedIndex = 18 Then
                TBZombieTimeProp.Text = PVZ.Memory.ReadInteger(&H527831)
            ElseIf CBZombieTimeProp.SelectedIndex = 19 Then
                TBZombieTimeProp.Text = PVZ.Memory.ReadInteger(&H527BAE)
            ElseIf CBZombieTimeProp.SelectedIndex = 20 Then
                TBZombieTimeProp.Text = PVZ.Memory.ReadInteger(&H527D20)
            ElseIf CBZombieTimeProp.SelectedIndex = 21 Then
                TBZombieTimeProp.Text = PVZ.Memory.ReadInteger(&H527E4A)
            End If
        End If
    End Sub
    Private Sub TBZombieTimeProp_PreviewKeyDown(sender As Object, e As KeyEventArgs)
        If DealKeyDown(sender, e) Then
            If CBZombieTimeProp.SelectedIndex = 0 Then
                PVZ.Memory.WriteInteger(&H52350A, TBZombieTimeProp.Text)
            ElseIf CBZombieTimeProp.SelectedIndex = 1 Then
                PVZ.Memory.WriteInteger(&H528EB7, TBZombieTimeProp.Text)
            ElseIf CBZombieTimeProp.SelectedIndex = 2 Then
                PVZ.Memory.WriteInteger(&H523160, TBZombieTimeProp.Text)
            ElseIf CBZombieTimeProp.SelectedIndex = 3 Then
                PVZ.Memory.WriteInteger(&H522FBD, TBZombieTimeProp.Text)
            ElseIf CBZombieTimeProp.SelectedIndex = 4 Then
                PVZ.Memory.WriteInteger(&H522FDB, TBZombieTimeProp.Text)
            ElseIf CBZombieTimeProp.SelectedIndex = 5 Then
                PVZ.Memory.WriteInteger(&H522FE0, TBZombieTimeProp.Text)
            ElseIf CBZombieTimeProp.SelectedIndex = 6 Then
                PVZ.Memory.WriteInteger(&H528355, TBZombieTimeProp.Text)
            ElseIf CBZombieTimeProp.SelectedIndex = 7 Then
                PVZ.Memory.WriteInteger(&H5232A7, TBZombieTimeProp.Text)
            ElseIf CBZombieTimeProp.SelectedIndex = 8 Then
                PVZ.Memory.WriteInteger(&H525548, TBZombieTimeProp.Text)
            ElseIf CBZombieTimeProp.SelectedIndex = 9 Then
                PVZ.Memory.WriteInteger(&H522978, TBZombieTimeProp.Text)
            ElseIf CBZombieTimeProp.SelectedIndex = 10 Then
                PVZ.Memory.WriteInteger(&H525127, TBZombieTimeProp.Text)
            ElseIf CBZombieTimeProp.SelectedIndex = 11 Then
                PVZ.Memory.WriteInteger(&H525A28, TBZombieTimeProp.Text)
            ElseIf CBZombieTimeProp.SelectedIndex = 12 Then
                PVZ.Memory.WriteInteger(&H525B28, TBZombieTimeProp.Text)
            ElseIf CBZombieTimeProp.SelectedIndex = 13 Then
                PVZ.Memory.WriteInteger(&H523BD0, TBZombieTimeProp.Text)
            ElseIf CBZombieTimeProp.SelectedIndex = 14 Then
                PVZ.Memory.WriteInteger(&H5275B2, TBZombieTimeProp.Text)
            ElseIf CBZombieTimeProp.SelectedIndex = 15 Then
                PVZ.Memory.WriteInteger(&H523A7A, TBZombieTimeProp.Text)
            ElseIf CBZombieTimeProp.SelectedIndex = 16 Then
                PVZ.Memory.WriteInteger(&H523A91, TBZombieTimeProp.Text)
            ElseIf CBZombieTimeProp.SelectedIndex = 17 Then
                PVZ.Memory.WriteInteger(&H523BD0, TBZombieTimeProp.Text)
            ElseIf CBZombieTimeProp.SelectedIndex = 18 Then
                PVZ.Memory.WriteInteger(&H527831, TBZombieTimeProp.Text)
            ElseIf CBZombieTimeProp.SelectedIndex = 19 Then
                PVZ.Memory.WriteInteger(&H527BAE, TBZombieTimeProp.Text)
            ElseIf CBZombieTimeProp.SelectedIndex = 20 Then
                PVZ.Memory.WriteInteger(&H527D20, TBZombieTimeProp.Text)
            ElseIf CBZombieTimeProp.SelectedIndex = 21 Then
                PVZ.Memory.WriteInteger(&H527E4A, TBZombieTimeProp.Text)
            End If
        End If
    End Sub
    Private ZombieHpList As Integer() = New Integer() {&H5227BB, &H522892, &H522CBF, &H52292B,
        &H52337D, &H522949, &H522BB0, &H523530, &H522DE1, &H523139, &H522D64, &H522FC7, &H522BEF,
        &H523300, &H52296E, &H522A1B, &H52299C, &H522E8D, &H523D26, &H523624, &H52361E, &H52382B,
        &H523A87, &H52395D, &H523E4A, &H5235AC, &H5234BF}
    Private Sub CBZombieHp_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
        If IsLoaded Then
            TBZombieHp.Text = PVZ.Memory.ReadInteger(ZombieHpList(CBZombieHp.SelectedIndex))
        End If
    End Sub
    Private Sub TBZombieHp_PreviewKeyDown(sender As Object, e As KeyEventArgs)
        If DealKeyDown(sender, e) Then
            PVZ.Memory.WriteInteger(ZombieHpList(CBZombieHp.SelectedIndex), TBZombieHp.Text)
        End If
    End Sub
    Private Sub CBZombieInvincible_Click(sender As Object, e As RoutedEventArgs)
        ZombieInvincibleResume()
        If CBZombieInvincible.IsChecked Then
            CBZombieWeak.IsEnabled = False
            If MIZombieChomperProof.IsChecked Then
                PVZ.Memory.WriteByte(&H46144A, 235)
            End If
            If MIZombieBlastEffectProof.IsChecked Then
                PVZ.Memory.WriteByte(&H532BA1, 129)
            End If
            If MIZombieBlastProof.IsChecked Then
                PVZ.Memory.WriteByte(&H41D8FF, 235)
                PVZ.Memory.WriteByte(&H4664F2, 235)
            End If
            If MIZombieSputterProof.IsChecked Then
                PVZ.Memory.WriteByte(&H46D455, 235)
            End If
            If MIZombieBloverProof.IsChecked Then
                PVZ.Memory.WriteInteger(&H466601, -125595504)
            End If
            If MIZombieHypnoshroonProof.IsChecked Then
                PVZ.Memory.WriteByte(&H52FA82, 0)
            End If
            If MIZombieLawnmoverProof.IsChecked Then
                PVZ.Memory.WriteByte(&H458836, 235)
            End If
            If MIZombieBodyDamageProof.IsChecked Then
                PVZ.Memory.WriteInteger(&H53130F, -1869574000)
            End If
            If MIZombieType1DamageProof.IsChecked Then
                PVZ.Memory.WriteByte(&H531045, 192)
            End If
            If MIZombieType2DamageProof.IsChecked Then
                PVZ.Memory.WriteInteger(&H530C9B, -768360397)
            End If
        Else
            CBZombieWeak.IsEnabled = True
        End If
    End Sub
    Private Sub ZombieInvincibleResume()
        PVZ.Memory.WriteByte(&H46144A, 116)
        PVZ.Memory.WriteByte(&H532BA1, 141)
        PVZ.Memory.WriteByte(&H41D8FF, 127)
        PVZ.Memory.WriteByte(&H4664F2, 117)
        PVZ.Memory.WriteByte(&H46D455, 116)
        PVZ.Memory.WriteInteger(&H466601, -125631116)
        PVZ.Memory.WriteByte(&H52FA82, 1)
        PVZ.Memory.WriteByte(&H458836, 116)
        PVZ.Memory.WriteInteger(&H53130F, 539261995)
        PVZ.Memory.WriteByte(&H531045, 200)
        PVZ.Memory.WriteInteger(&H530C9B, -1031077252)
    End Sub
    Private Sub CBZombieWeak_Click(sender As Object, e As RoutedEventArgs)
        ZombieInvincibleResume()
        If CBZombieWeak.IsChecked Then
            CBZombieInvincible.IsEnabled = False
            PVZ.Memory.WriteShort(&H530C9B, -12149)
            PVZ.Memory.WriteByte(&H531045, 201)
            PVZ.Memory.WriteInteger(&H53130F, -1869545685)
        Else
            CBZombieInvincible.IsEnabled = True
        End If
    End Sub
    Private Sub NudAdvStage2_ValueChanged(sender As Object, e As EventArgs)
        If IsLoaded Then
            Dim level = 10 * (NudAdvStage2.Value - 1) + NudAdvLevel2.Value - 1
            For index = 0 To 32
                CType(CZombieSeeds.Children(index), CheckBox).IsChecked = Convert.ToBoolean(PVZ.Memory.ReadInteger(&H6A35B4 + level * 4 + index * &HCC))
            Next
        End If
    End Sub
    Private Sub BtnSetZombieSeed_Click(sender As Object, e As RoutedEventArgs)
        If RBAdventure2.IsChecked Then
            PVZ.Memory.WriteByte(&H40D6A3, 235)
            Dim level = 10 * (NudAdvStage2.Value - 1) + NudAdvLevel2.Value - 1
            For index = 0 To 32
                PVZ.Memory.WriteInteger(&H6A35B4 + level * 4 + index * &HCC, Convert.ToInt32(CType(CZombieSeeds.Children(index), CheckBox).IsChecked))
            Next
        ElseIf RBMini2.IsChecked Then
            Dim count As Integer
            For index = 0 To 32
                Dim cbox As CheckBox = CZombieSeeds.Children(index)
                If cbox.IsChecked Then
                    PVZ.Memory.WriteInteger(ZombieSeedList(CBLevelMiniAll.SelectedIndex) + 9 * count, &H54D4 + index)
                    count += 1
                End If
            Next
        ElseIf RBCurrent.IsChecked Then
            Dim waveNum = NudAdvFlags.Value
            PVZ.Memory.WriteByteArray(&H409301, &HEB, &H23, &H90, &H90, &H90, &H90)
            PVZ.Memory.WriteInteger(&H40932C, waveNum)
            For index = 0 To 32
                PVZ.Memory.WriteByte(PVZ.BaseAddress + &H54D4 + index, 0)
            Next
            Dim cbox As CheckBox = CZombieSeeds.Children(0)
            If CBMaxLimit.IsChecked Then
                Dim zcount As Integer
                For index = 0 To 32
                    cbox = CZombieSeeds.Children(index)
                    If cbox.IsChecked Then
                        zcount += 1
                        PVZ.Memory.WriteByte(PVZ.BaseAddress + &H54D4 + index, 1)
                    End If
                Next
                Dim random = New Random()
                Dim zseeds = PVZ.ZombieSeed
                For index = 0 To waveNum - 1
                    For jndex = 0 To 49
                        PVZ.Memory.WriteInteger(PVZ.BaseAddress + &H6B4 + jndex * 4 + index * 50 * 4, zseeds(random.Next(zseeds.Length)))
                    Next
                Next
                PVZ.ClearZombiePreview()
                PVZ.ShowZombiePreview()
            Else
                If cbox.IsChecked OrElse CType(CZombieSeeds.Children(26), CheckBox).IsChecked Then
                    For index = 0 To 32
                        cbox = CZombieSeeds.Children(index)
                        If cbox.IsChecked Then
                            PVZ.Memory.WriteByte(PVZ.BaseAddress + &H54D4 + index, 1)
                        End If
                    Next
                    PVZ.CallZombieList()
                    PVZ.ClearZombiePreview()
                    PVZ.ShowZombiePreview()
                Else
                    Dim flag = -1
                    For index = 0 To 32
                        cbox = CZombieSeeds.Children(index)
                        If cbox.IsChecked AndAlso flag = -1 Then
                            flag = index
                            PVZ.Memory.WriteByte(PVZ.BaseAddress + &H54D4 + 26, 1)
                        ElseIf cbox.IsChecked Then
                            PVZ.Memory.WriteByte(PVZ.BaseAddress + &H54D4 + index, 1)
                        End If
                    Next
                    PVZ.CallZombieList()
                    For index = 0 To waveNum - 1
                        For jndex = 0 To 49
                            Dim z = PVZ.Memory.ReadInteger(PVZ.BaseAddress + &H6B4 + jndex * 4 + index * 50 * 4)
                            If z = -1 Then
                                Exit For
                            ElseIf z = 26 Then
                                PVZ.Memory.WriteInteger(PVZ.BaseAddress + &H6B4 + jndex * 4 + index * 50 * 4, flag)
                            End If
                        Next
                    Next
                    PVZ.ClearZombiePreview()
                    PVZ.ShowZombiePreview()
                End If
            End If
            PVZ.Memory.WriteByteArray(&H409301, &HF, &H85, &HA6, 0, 0, 0)
            PVZ.Memory.WriteInteger(&H40932C, 8)
        End If
    End Sub
    Private ZombieSeedList As Integer() = New Integer() {&H425C09, &H425A6F, &H42588F, &H4258BD, &H4258D4,
        &H425902, &H425942, &H425974, &H4259A2, &H4259E2, &H425A34, &H425A94, &H425AB0, &H425CEC, &H425AD5,
        &H425B39, &H425B67, &H425BA7, &H425BC3, &H425CC6, &H425C36, &H425C83, &H425CA9}
    Private Sub CBLevelMiniAll_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
        If IsLoaded Then
            For Each cbox As CheckBox In CZombieSeeds.Children
                cbox.IsChecked = False
            Next
            For index = 0 To Integer.Parse(CType(CBLevelMiniAll.SelectedItem, ComboBoxItem).Tag) - 1
                Dim i = PVZ.Memory.ReadInteger(ZombieSeedList(CBLevelMiniAll.SelectedIndex) + 9 * index) - &H54D4
                i = Math.Min(CZombieSeeds.Children.Count, Math.Max(0, i))
                CType(CZombieSeeds.Children(i), CheckBox).IsChecked = True
            Next
        End If
    End Sub
    Private Sub CBZombieSeed_Click(sender As Object, e As RoutedEventArgs)
        If RBMini2.IsChecked Then
            Dim checkedCount As Integer
            For Each cbox As CheckBox In CZombieSeeds.Children
                If cbox.IsChecked Then
                    checkedCount += 1
                End If
            Next
            If checkedCount <> Integer.Parse(CType(CBLevelMiniAll.SelectedItem, ComboBoxItem).Tag) Then
                BtnSetZombieSeed.IsEnabled = False
            Else
                BtnSetZombieSeed.IsEnabled = True
            End If
        End If
    End Sub
    Private Sub RBMini2_Checked(sender As Object, e As RoutedEventArgs)
        CBLevelMiniAll_SelectionChanged(Nothing, Nothing)
    End Sub
    Private Sub RBCurrent_Checked(sender As Object, e As RoutedEventArgs)
        For Each cbox As CheckBox In CZombieSeeds.Children
            cbox.IsChecked = False
        Next
        For Each zseed In PVZ.ZombieSeed
            CType(CZombieSeeds.Children(zseed), CheckBox).IsChecked = True
        Next
    End Sub
    Private Sub BtnWaveManager_Click(sender As Object, e As RoutedEventArgs)
        Dim waveDlg As New WaveManageDialog()
        BtnWaveManager.IsEnabled = False
        waveDlg.Owner = Me
        waveDlg.Show()
    End Sub
    Private Sub BtnZombieList_Click(sender As Object, e As RoutedEventArgs)
        Dim zlistDlg As New ZombieListDialog()
        BtnZombieList.IsEnabled = False
        zlistDlg.Owner = Me
        zlistDlg.Show()
    End Sub
    Private Sub TCMain_PreviewKeyDown(sender As Object, e As KeyEventArgs)
        If e.Key = Key.F5 Then
            Dim selItem As TabItem = TCMain.SelectedItem
            selItem.Tag = TCMain
            TCMain.Items.Remove(selItem)
            Dim separate = New SeparateWindow()
            separate.TCMain.Items.Add(selItem)
            separate.Show()
        End If
    End Sub

    Private Sub RBDamagePlantType_Click(sender As Object, e As RoutedEventArgs)
        NudBound.IgnoreAssign = True
        NudRadius.IgnoreAssign = True
        If RBDamageDoomShroom.IsChecked = True Then
            NudRadius.MaxValue = Integer.MaxValue
            CBIsCinder.IsChecked = Convert.ToInt32(PVZ.Memory.ReadByte(&H466835))
            NudBound.Value = PVZ.Memory.ReadByte(&H466837)
            NudRadius.Value = PVZ.Memory.ReadInteger(&H466839)
        Else
            NudRadius.MaxValue = 127
            If RBDamageExpWallNut.IsChecked = True Then
                CBIsCinder.IsChecked = Convert.ToInt32(PVZ.Memory.ReadByte(&H462E7A))
                NudBound.Value = PVZ.Memory.ReadByte(&H462E7C)
                NudRadius.Value = PVZ.Memory.ReadByte(&H462E7E)
            ElseIf RBDamageCherryBomb.IsChecked = True Then
                CBIsCinder.IsChecked = Convert.ToInt32(PVZ.Memory.ReadByte(&H4667D7))
                NudBound.Value = PVZ.Memory.ReadByte(&H4667D9)
                NudRadius.Value = PVZ.Memory.ReadByte(&H4667DB)
            ElseIf RBDamagePotatoMine.IsChecked = True Then
                CBIsCinder.IsChecked = Convert.ToInt32(PVZ.Memory.ReadByte(&H466A61))
                NudBound.Value = PVZ.Memory.ReadByte(&H466A63)
                NudRadius.Value = PVZ.Memory.ReadByte(&H466A65)
            ElseIf RBDamageCobCannon.IsChecked = True Then
                CBIsCinder.IsChecked = Convert.ToInt32(PVZ.Memory.ReadByte(&H46D839))
                NudBound.Value = PVZ.Memory.ReadByte(&H46D83B)
                NudRadius.Value = PVZ.Memory.ReadByte(&H46D83D)
            ElseIf RBDamageJackH.IsChecked = True Then
                CBIsCinder.IsChecked = Convert.ToInt32(PVZ.Memory.ReadByte(&H526C51))
                NudBound.Value = PVZ.Memory.ReadByte(&H526C53)
                NudRadius.Value = PVZ.Memory.ReadByte(&H526C55)
            ElseIf RBDamageJack.IsChecked = True Then
                CBIsCinder.IsChecked = Convert.ToInt32(PVZ.Memory.ReadByte(&H526C6D))
                NudBound.Value = PVZ.Memory.ReadByte(&H526C6F)
                NudRadius.Value = PVZ.Memory.ReadByte(&H526C71)
            End If
        End If
        NudBound.IgnoreAssign = False
        NudRadius.IgnoreAssign = False
    End Sub

    Private Sub CBIsCinder_Click(sender As Object, e As RoutedEventArgs)
        Dim value As Byte = Convert.ToByte(CBIsCinder.IsChecked.Value)
        If RBDamageExpWallNut.IsChecked = True Then
            PVZ.Memory.WriteByte(&H462E7A, value)
        ElseIf RBDamageCherryBomb.IsChecked = True Then
            PVZ.Memory.WriteByte(&H4667D7, value)
        ElseIf RBDamageDoomShroom.IsChecked = True Then
            PVZ.Memory.WriteByte(&H466835, value)
        ElseIf RBDamagePotatoMine.IsChecked = True Then
            PVZ.Memory.WriteByte(&H466A61, value)
        ElseIf RBDamageCobCannon.IsChecked = True Then
            PVZ.Memory.WriteByte(&H46D839, value)
        ElseIf RBDamageJackH.IsChecked = True Then
            PVZ.Memory.WriteByte(&H526C51, value)
        ElseIf RBDamageJack.IsChecked = True Then
            PVZ.Memory.WriteByte(&H526C6D, value)
        End If
    End Sub

    Private Sub NudBound_ValueChanged(sender As Object, e As EventArgs)
        If IsLoaded Then
            If RBDamageExpWallNut.IsChecked = True Then
                PVZ.Memory.WriteByte(&H462E7C, NudBound.Value)
            ElseIf RBDamageCherryBomb.IsChecked = True Then
                PVZ.Memory.WriteByte(&H4667D9, NudBound.Value)
            ElseIf RBDamageDoomShroom.IsChecked = True Then
                PVZ.Memory.WriteByte(&H466837, NudBound.Value)
            ElseIf RBDamagePotatoMine.IsChecked = True Then
                PVZ.Memory.WriteByte(&H466A63, NudBound.Value)
            ElseIf RBDamageCobCannon.IsChecked = True Then
                PVZ.Memory.WriteByte(&H46D83B, NudBound.Value)
            ElseIf RBDamageJackH.IsChecked = True Then
                PVZ.Memory.WriteByte(&H526C53, NudBound.Value)
            ElseIf RBDamageJack.IsChecked = True Then
                PVZ.Memory.WriteByte(&H526C6F, NudBound.Value)
            End If
        End If
    End Sub

    Private Sub NudRadius_ValueChanged(sender As Object, e As EventArgs)
        If IsLoaded Then
            If RBDamageExpWallNut.IsChecked = True Then
                PVZ.Memory.WriteByte(&H462E7E, NudRadius.Value)
            ElseIf RBDamageCherryBomb.IsChecked = True Then
                PVZ.Memory.WriteByte(&H4667DB, NudRadius.Value)
            ElseIf RBDamageDoomShroom.IsChecked = True Then
                PVZ.Memory.WriteByte(&H466839, NudRadius.Value)
            ElseIf RBDamagePotatoMine.IsChecked = True Then
                PVZ.Memory.WriteByte(&H466A65, NudRadius.Value)
            ElseIf RBDamageCobCannon.IsChecked = True Then
                PVZ.Memory.WriteByte(&H46D83D, NudRadius.Value)
            ElseIf RBDamageJackH.IsChecked = True Then
                PVZ.Memory.WriteByte(&H526C55, NudRadius.Value)
            ElseIf RBDamageJack.IsChecked = True Then
                PVZ.Memory.WriteByte(&H526C71, NudRadius.Value)
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
            Height = 470 * scale / 100
            Width = 400 * scale / 100
        End If
    End Sub

    Private Sub Window_Loaded(sender As Object, e As RoutedEventArgs)
        Lang.ChangeLanguage(Content)
    End Sub
    Private Sub TBDamageProjectile_PreviewKeyDown(sender As Object, e As KeyEventArgs)
        If DealKeyDown(sender, e) Then
            PVZ.Memory.WriteInteger(&H69F1C8 + CBDamageProjectile.SelectedIndex * &HC, TBDamageProjectile.Text)
        End If
    End Sub

    Private Sub CBDamageProjectile_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
        If IsLoaded Then
            TBDamageProjectile.Text = PVZ.Memory.ReadInteger(&H69F1C8 + CBDamageProjectile.SelectedIndex * &HC)
        End If
    End Sub
    Dim Damages As Integer() = {&H532FDC, &H532B9C, &H41D931, &H4614DD, &H532493, &H4607A9, &H45EDEF}
    Private Sub CBDamageSpecial_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
        If IsLoaded Then
            If CBDamageSpecial.SelectedIndex = 3 Or CBDamageSpecial.SelectedIndex = 4 Then
                TBDamageSpecial.Text = PVZ.Memory.ReadByte(Damages(CBDamageSpecial.SelectedIndex))
            Else
                TBDamageSpecial.Text = PVZ.Memory.ReadInteger(Damages(CBDamageSpecial.SelectedIndex))
            End If
        End If
    End Sub

    Private Sub TBDamageSpecial_PreviewKeyDown(sender As Object, e As KeyEventArgs)
        If DealKeyDown(sender, e) Then
            If CBDamageSpecial.SelectedIndex = 3 Or CBDamageSpecial.SelectedIndex = 4 Then
                PVZ.Memory.WriteByte(Damages(CBDamageSpecial.SelectedIndex), TBDamageSpecial.Text)
            Else
                PVZ.Memory.WriteInteger(Damages(CBDamageSpecial.SelectedIndex), TBDamageSpecial.Text)
            End If
        End If
    End Sub

    Private Sub CBDamageZombie_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
        If IsLoaded Then
            If CBDamageZombie.SelectedIndex = 0 Then
                TBDamageZombie.Text = CSByte(PVZ.Memory.ReadByte(&H52FCF3))
            ElseIf CBDamageZombie.SelectedIndex = 1 Then
                TBDamageZombie.Text = PVZ.Memory.ReadByte(&H52FE14)
            ElseIf CBDamageZombie.SelectedIndex = 2 Then
                TBDamageZombie.Text = CSByte(PVZ.Memory.ReadByte(&H45EC66))
            End If
        End If
    End Sub

    Private Sub TBDamageZombie_PreviewKeyDown(sender As Object, e As KeyEventArgs)
        If DealKeyDown(sender, e) Then
            If CBDamageZombie.SelectedIndex = 0 Then
                PVZ.Memory.WriteByte(&H52FCF3, CSByte(TBDamageZombie.Text))
            ElseIf CBDamageZombie.SelectedIndex = 1 Then
                PVZ.Memory.WriteByte(&H52FE14, TBDamageZombie.Text)
            ElseIf CBDamageZombie.SelectedIndex = 2 Then
                PVZ.Memory.WriteByte(&H45EC66, CSByte(TBDamageZombie.Text))
            End If
        End If
    End Sub
    Dim DamageTimes As Integer() = {&H5309C7, &H5309CE, &H532741, &H532400, &H53241C, &H532426, &H53240B, &H532415}
    Private Sub CBDamageTime_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
        If IsLoaded Then
            TBDamageTime.Text = PVZ.Memory.ReadInteger(DamageTimes(CBDamageTime.SelectedIndex))
        End If
    End Sub

    Private Sub TBDamageTime_PreviewKeyDown(sender As Object, e As KeyEventArgs)
        If DealKeyDown(sender, e) Then
            PVZ.Memory.WriteInteger(DamageTimes(CBDamageTime.SelectedIndex), TBDamageTime.Text)
        End If
    End Sub

    Private Sub NudSceneGridRow_ValueChanged(sender As Object, e As EventArgs)
        If IsLoaded Then
            Dim row = NudSceneGridRow.Value
            CBSceneRouteType.SelectedIndex = PVZ.Lawn.GetRouteType(row)
            CBSceneGridType.SelectedIndex = PVZ.Lawn.GetGridType(row, NudSceneGridColumn.Value) - 1
        End If
    End Sub

    Private Sub NudSceneGridColumn_ValueChanged(sender As Object, e As EventArgs)
        If IsLoaded Then
            Dim row = NudSceneGridRow.Value
            CBSceneGridType.SelectedIndex = PVZ.Lawn.GetGridType(row, NudSceneGridColumn.Value) - 1
        End If
    End Sub

    Private Sub CBSceneGridType_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
        If CBSceneGridType.IsMouseOver Then
            PVZ.Lawn.SetGridType(NudSceneGridRow.Value, NudSceneGridColumn.Value, CBSceneGridType.SelectedIndex + 1)
        End If
    End Sub

    Private Sub CBSceneRouteType_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
        If CBSceneRouteType.IsMouseOver Then
            PVZ.Lawn.SetRouteType(NudSceneGridRow.Value, CBSceneRouteType.SelectedIndex)
        End If
    End Sub

    Private Sub RBPoolSceneNormal_Click(sender As Object, e As RoutedEventArgs)
        PVZ.Memory.WriteByte(&H40A668, &HAE)
        PVZ.Memory.WriteByte(&H40A66E, &HAE)
        PVZ.Memory.WriteByte(&H40A674, &H8E)
        PVZ.Memory.WriteByte(&H40A67A, &H8E)
        PVZ.Memory.WriteByte(&H40A680, &HAE)
        PVZ.Memory.WriteByte(&H40A686, &HAE)
    End Sub

    Private Sub RBPoolSceneReverse_Click(sender As Object, e As RoutedEventArgs)
        PVZ.Memory.WriteByte(&H40A668, &H8E)
        PVZ.Memory.WriteByte(&H40A66E, &H8E)
        PVZ.Memory.WriteByte(&H40A674, &HAE)
        PVZ.Memory.WriteByte(&H40A67A, &HAE)
        PVZ.Memory.WriteByte(&H40A680, &H8E)
        PVZ.Memory.WriteByte(&H40A686, &H8E)
    End Sub

    Private Sub RBPoolSceneFlood_Click(sender As Object, e As RoutedEventArgs)
        PVZ.Memory.WriteByte(&H40A668, &H8E)
        PVZ.Memory.WriteByte(&H40A66E, &H8E)
        PVZ.Memory.WriteByte(&H40A674, &H8E)
        PVZ.Memory.WriteByte(&H40A67A, &H8E)
        PVZ.Memory.WriteByte(&H40A680, &H8E)
        PVZ.Memory.WriteByte(&H40A686, &HAE)
    End Sub

    Private Sub RBPoolSceneLand_Click(sender As Object, e As RoutedEventArgs)
        PVZ.Memory.WriteByte(&H40A668, &HAE)
        PVZ.Memory.WriteByte(&H40A66E, &HAE)
        PVZ.Memory.WriteByte(&H40A674, &HAE)
        PVZ.Memory.WriteByte(&H40A67A, &HAE)
        PVZ.Memory.WriteByte(&H40A680, &HAE)
        PVZ.Memory.WriteByte(&H40A686, &HAE)
    End Sub

    Private Sub MIAdvicePot_Click(sender As Object, e As RoutedEventArgs)
        If MIAdvicePot.IsChecked Then
            PVZ.Memory.WriteByte(&H41CD19, 235)
            PVZ.Memory.WriteByte(&H4857A8, 235)
        Else
            PVZ.Memory.WriteByte(&H41CD19, 116)
            PVZ.Memory.WriteByte(&H4857A8, 116)
        End If
    End Sub

    Private Sub CBUnsoddedAsRoof_Click(sender As Object, e As RoutedEventArgs)
        If CBDefaultUnsodded.IsChecked = False Then
            CBDefaultUnsodded.IsChecked = True
            PVZ.DefaultUnsodded()
        End If
    End Sub

    Private GroundPropetys = {&H413BA4, &H413BAC, &H413BBA, &H422BD9, &H422C6B, &H466644, &H426FCE, &H4277D8, &H52A8B6, &H52A8D2, &H41F7A2, &H466887}

    Private Sub TBGroundPropety_PreviewKeyDown(sender As Object, e As KeyEventArgs)
        If DealKeyDown(sender, e) Then
            PVZ.Memory.WriteInteger(GroundPropetys(CBGroundPropety.SelectedIndex), TBGroundPropety.Text)
            If CBGroundPropety.SelectedIndex = 1 Then
                PVZ.Memory.WriteInteger(&H413BB1, TBGroundPropety.Text)
            End If
        End If
    End Sub

    Private Sub CBGroundPropety_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
        If CBGroundPropety.IsMouseOver Then
            TBGroundPropety.Text = PVZ.Memory.ReadInteger(GroundPropetys(CBGroundPropety.SelectedIndex))
        End If
    End Sub

    Private Sub TBWharkaZombieSpawnCount_PreviewKeyDown(sender As Object, e As KeyEventArgs)
        If DealKeyDown(sender, e) Then
            PVZ.Memory.WriteInteger(&H426193, TBWharkaZombieSpawnCount.Text)
        End If
    End Sub

    Private Sub NudWharkaZombieZombieSpeed_ValueChanged(sender As Object, e As EventArgs)
        If IsLoaded Then
            PVZ.Memory.WriteByte(&H42630B, NudWharkaZombieZombieSpeed.Value)
        End If
    End Sub

    Private Sub NudWharkaZombieMinGraveCount_ValueChanged(sender As Object, e As EventArgs)
        If IsLoaded Then
            PVZ.Memory.WriteInteger(&H426044, NudWharkaZombieMinGraveCount.Value)
        End If
    End Sub

    Private Sub TBWharkaZombieSpawnSpeed_PreviewKeyDown(sender As Object, e As KeyEventArgs)
        If DealKeyDown(sender, e) Then
            If PVZ.Memory.ReadInteger(&H426559) = 412001000 Then
                PVZ.Memory.WriteByte(&H426559, 134)
            End If
            PVZ.Memory.WriteInteger(&H42655A, TBWharkaZombieSpawnSpeed.Text)
        End If
    End Sub

    Private Sub SIcetrace_ValueChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of Double))
        If IsLoaded Then
            Select Case CBLockIcetrace.SelectedIndex
                Case 0
                    For index = 0 To 5
                        PVZ.Icetrace.SetX(index, SIcetrace.Value)
                        PVZ.Icetrace.SetDisapperaCountdown(index, 1000)
                    Next
                Case 1
                    For index = 0 To 5
                        If index = 2 OrElse index = 3 Then Continue For
                        PVZ.Icetrace.SetX(index, SIcetrace.Value)
                        PVZ.Icetrace.SetDisapperaCountdown(index, 1000)
                    Next
                Case 2
                    PVZ.Icetrace.SetX(0, SIcetrace.Value)
                    PVZ.Icetrace.SetDisapperaCountdown(0, 1000)
                Case 3
                    PVZ.Icetrace.SetX(1, SIcetrace.Value)
                    PVZ.Icetrace.SetDisapperaCountdown(1, 1000)
                Case 4
                    PVZ.Icetrace.SetX(2, SIcetrace.Value)
                    PVZ.Icetrace.SetDisapperaCountdown(2, 1000)
                Case 5
                    PVZ.Icetrace.SetX(3, SIcetrace.Value)
                    PVZ.Icetrace.SetDisapperaCountdown(3, 1000)
                Case 6
                    PVZ.Icetrace.SetX(4, SIcetrace.Value)
                    PVZ.Icetrace.SetDisapperaCountdown(4, 1000)
                Case 7
                    PVZ.Icetrace.SetX(5, SIcetrace.Value)
                    PVZ.Icetrace.SetDisapperaCountdown(5, 1000)
            End Select
        End If
    End Sub

    Private Sub TBItemValue_PreviewKeyDown(sender As Object, e As KeyEventArgs)
        If DealKeyDown(sender, e) Then
            PVZ.Memory.WriteInteger(ItemValues(CBItemValue.SelectedIndex), TBItemValue.Text)
        End If
    End Sub
    Private ItemValues = {&H430A46, &H430A52, &H430A5C, &H4309F0, &H4309FC, &H430A03}
    Private Sub CBItemValue_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
        If CBItemValue.IsMouseOver Then
            TBItemValue.Text = PVZ.Memory.ReadInteger(ItemValues(CBItemValue.SelectedIndex))
        End If
    End Sub

    Private Function GetCoinTypeValue(value As Integer) As Byte
        Return [Enum].GetValues(GetType(PVZ.CoinType))(value)
    End Function

    Private Sub CBDropItem_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
        If CBDropItem.IsMouseOver Then
            PVZ.Memory.WriteInteger(&H413BD9, GetCoinTypeValue(CBDropItem.SelectedIndex))
        End If
    End Sub

    Private Sub CBSunnyDayDropItem_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
        If CBSunnyDayDropItem.IsMouseOver Then
            PVZ.Memory.WriteInteger(&H413BE0, GetCoinTypeValue(CBSunnyDayDropItem.SelectedIndex))
        End If
    End Sub

    Private Sub CBDropPlantType_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
        PVZ.Memory.WriteInteger(&H42FFB9, CBDropPlantType.SelectedIndex)
    End Sub

    Private Sub RBZombieDropNormal_Click(sender As Object, e As RoutedEventArgs)
        PVZ.Memory.WriteByte(&H530275, 117)
        PVZ.Memory.WriteByte(&H41CF10, -914024332)
    End Sub

    Private Sub RBZombieWillDrop_Click(sender As Object, e As RoutedEventArgs)
        PVZ.Memory.WriteByte(&H530275, 112)
    End Sub

    Private Sub RBZombieMightDrop_Click(sender As Object, e As RoutedEventArgs)
        PVZ.Memory.WriteByte(&H41CF10, -914024213)
    End Sub

    Private Sub CBZombieWillDropItem1_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
        If CBZombieWillDropItem1.IsMouseOver Then
            PVZ.Memory.WriteByte(&H53028D, GetCoinTypeValue(CBZombieWillDropItem1.SelectedIndex))
        End If
    End Sub

    Private Sub CBZombieWillDropItem2_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
        If CBZombieWillDropItem2.IsMouseOver Then
            PVZ.Memory.WriteByte(&H53029B, GetCoinTypeValue(CBZombieWillDropItem2.SelectedIndex))
        End If
    End Sub

    Private Sub CBZombieWillDropItem3_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
        If CBZombieWillDropItem3.IsMouseOver Then
            PVZ.Memory.WriteByte(&H5302AF, GetCoinTypeValue(CBZombieWillDropItem3.SelectedIndex))
        End If
    End Sub

    Private Sub CBZombieWillDropItem4_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
        If CBZombieWillDropItem4.IsMouseOver Then
            PVZ.Memory.WriteByte(&H5302C0, GetCoinTypeValue(CBZombieWillDropItem4.SelectedIndex))
        End If
    End Sub

    Private Sub CBZombieMightDroItem1_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
        If CBZombieMightDroItem1.IsMouseOver Then
            PVZ.Memory.WriteByte(&H41CFE6, GetCoinTypeValue(CBZombieMightDroItem1.SelectedIndex))
        End If
    End Sub

    Private Sub CBZombieMightDroItem2_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
        If CBZombieMightDroItem2.IsMouseOver Then
            PVZ.Memory.WriteByte(&H41CFF6, GetCoinTypeValue(CBZombieMightDroItem2.SelectedIndex))
        End If
    End Sub

    Private Sub CBZombieMightDroItem3_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
        If CBZombieMightDroItem3.IsMouseOver Then
            PVZ.Memory.WriteByte(&H41D006, GetCoinTypeValue(CBZombieMightDroItem3.SelectedIndex))
        End If
    End Sub

    Private Sub CBMarigoldDrop1_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
        If CBMarigoldDrop1.IsMouseOver Then
            PVZ.Memory.WriteByte(&H45FAFC, 218 + GetCoinTypeValue(CBMarigoldDrop1.SelectedIndex))
            PVZ.Memory.WriteByte(&H45FAFF, 100 - GetCoinTypeValue(CBMarigoldDrop1.SelectedIndex))
        End If
    End Sub

    Private Sub CBMarigoldDrop2_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
        If CBMarigoldDrop2.IsMouseOver Then
            PVZ.Memory.WriteByte(&H45FB0B, GetCoinTypeValue(CBMarigoldDrop2.SelectedIndex))
        End If
    End Sub

    Private Sub NudPMarigoldDrop_ValueChanged(sender As Object, e As EventArgs)
        If IsLoaded Then
            PVZ.Memory.WriteByte(&H45FB07, NudPMarigoldDrop.Value)
        End If
    End Sub

    Private ConveyorBeltCards As Integer(,) = {
        {&H422E42, &H422E4E, &H422E6E, &H422E7A, 0, 0, 0},
        {&H422EA0, &H422EB0, &H422EBC, &H422ED0, 0, 0, 0},
        {&H422EF6, &H422F02, &H422F0E, &H422F22, &H422F2E, &H422F3A, 0},
        {&H422F60, &H422F6C, &H422F78, &H422F84, &H422F94, &H422FA0, &H422FAC},
        {&H422FCD, &H422FDD, &H422FF5, &H423001, &H42300D, 0, 0},
        {&H42308F, &H42309F, 0, 0, 0, 0, 0},
        {&H423110, &H423120, &H423130, &H423140, 0, 0, 0},
        {&H4230CB, &H4230D7, &H4230EB, 0, 0, 0, 0},
        {&H423059, &H42306D, &H423075, 0, 0, 0, 0},
        {&H423160, &H423170, &H423180, &H423190, 0, 0, 0},
        {&H4231C4, &H4231D0, &H4231E0, &H4231EC, 0, 0, 0},
        {&H42320A, &H42321A, &H42322E, &H423246, 0, 0, 0},
        {&H42326A, &H423272, &H42327E, &H42328A, &H42329A, 0, 0}
    }

    Private Sub CBConveyorBeltLevel_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
        If IsLoaded Then
            CBBeltCard1.IsEnabled = False
            CBBeltCard2.IsEnabled = False
            CBBeltCard3.IsEnabled = False
            CBBeltCard4.IsEnabled = False
            CBBeltCard5.IsEnabled = False
            CBBeltCard6.IsEnabled = False
            CBBeltCard7.IsEnabled = False

            If CBConveyorBeltLevel.SelectedIndex = 8 Then
                PVZ.Memory.WriteInteger(&H423059, 3)
                PVZ.Memory.WriteByte(&H42305E, &H44)
            End If

            Dim cardcount = CInt(CType(CBConveyorBeltLevel.SelectedItem, ComboBoxItem).Tag)
            Select Case cardcount
                Case 2
                    CBBeltCard1.IsEnabled = True
                    CBBeltCard2.IsEnabled = True
                    CBBeltCard1.SelectedIndex = PVZ.Memory.ReadInteger(ConveyorBeltCards(CBConveyorBeltLevel.SelectedIndex, 0))
                    CBBeltCard2.SelectedIndex = PVZ.Memory.ReadInteger(ConveyorBeltCards(CBConveyorBeltLevel.SelectedIndex, 1))
                Case 3
                    CBBeltCard1.IsEnabled = True
                    CBBeltCard2.IsEnabled = True
                    CBBeltCard3.IsEnabled = True
                    CBBeltCard1.SelectedIndex = PVZ.Memory.ReadInteger(ConveyorBeltCards(CBConveyorBeltLevel.SelectedIndex, 0))
                    CBBeltCard2.SelectedIndex = PVZ.Memory.ReadInteger(ConveyorBeltCards(CBConveyorBeltLevel.SelectedIndex, 1))
                    CBBeltCard3.SelectedIndex = PVZ.Memory.ReadInteger(ConveyorBeltCards(CBConveyorBeltLevel.SelectedIndex, 2))
                Case 4
                    CBBeltCard1.IsEnabled = True
                    CBBeltCard2.IsEnabled = True
                    CBBeltCard3.IsEnabled = True
                    CBBeltCard4.IsEnabled = True
                    CBBeltCard1.SelectedIndex = PVZ.Memory.ReadInteger(ConveyorBeltCards(CBConveyorBeltLevel.SelectedIndex, 0))
                    CBBeltCard2.SelectedIndex = PVZ.Memory.ReadInteger(ConveyorBeltCards(CBConveyorBeltLevel.SelectedIndex, 1))
                    CBBeltCard3.SelectedIndex = PVZ.Memory.ReadInteger(ConveyorBeltCards(CBConveyorBeltLevel.SelectedIndex, 2))
                    CBBeltCard4.SelectedIndex = PVZ.Memory.ReadInteger(ConveyorBeltCards(CBConveyorBeltLevel.SelectedIndex, 3))
                Case 5
                    CBBeltCard1.IsEnabled = True
                    CBBeltCard2.IsEnabled = True
                    CBBeltCard3.IsEnabled = True
                    CBBeltCard4.IsEnabled = True
                    CBBeltCard5.IsEnabled = True
                    CBBeltCard1.SelectedIndex = PVZ.Memory.ReadInteger(ConveyorBeltCards(CBConveyorBeltLevel.SelectedIndex, 0))
                    CBBeltCard2.SelectedIndex = PVZ.Memory.ReadInteger(ConveyorBeltCards(CBConveyorBeltLevel.SelectedIndex, 1))
                    CBBeltCard3.SelectedIndex = PVZ.Memory.ReadInteger(ConveyorBeltCards(CBConveyorBeltLevel.SelectedIndex, 2))
                    CBBeltCard4.SelectedIndex = PVZ.Memory.ReadInteger(ConveyorBeltCards(CBConveyorBeltLevel.SelectedIndex, 3))
                    CBBeltCard5.SelectedIndex = PVZ.Memory.ReadInteger(ConveyorBeltCards(CBConveyorBeltLevel.SelectedIndex, 4))
                Case 6
                    CBBeltCard1.IsEnabled = True
                    CBBeltCard2.IsEnabled = True
                    CBBeltCard3.IsEnabled = True
                    CBBeltCard4.IsEnabled = True
                    CBBeltCard5.IsEnabled = True
                    CBBeltCard6.IsEnabled = True
                    CBBeltCard1.SelectedIndex = PVZ.Memory.ReadInteger(ConveyorBeltCards(CBConveyorBeltLevel.SelectedIndex, 0))
                    CBBeltCard2.SelectedIndex = PVZ.Memory.ReadInteger(ConveyorBeltCards(CBConveyorBeltLevel.SelectedIndex, 1))
                    CBBeltCard3.SelectedIndex = PVZ.Memory.ReadInteger(ConveyorBeltCards(CBConveyorBeltLevel.SelectedIndex, 2))
                    CBBeltCard4.SelectedIndex = PVZ.Memory.ReadInteger(ConveyorBeltCards(CBConveyorBeltLevel.SelectedIndex, 3))
                    CBBeltCard5.SelectedIndex = PVZ.Memory.ReadInteger(ConveyorBeltCards(CBConveyorBeltLevel.SelectedIndex, 4))
                    CBBeltCard6.SelectedIndex = PVZ.Memory.ReadInteger(ConveyorBeltCards(CBConveyorBeltLevel.SelectedIndex, 5))
                Case 7
                    CBBeltCard1.IsEnabled = True
                    CBBeltCard2.IsEnabled = True
                    CBBeltCard3.IsEnabled = True
                    CBBeltCard4.IsEnabled = True
                    CBBeltCard5.IsEnabled = True
                    CBBeltCard6.IsEnabled = True
                    CBBeltCard7.IsEnabled = True
                    CBBeltCard1.SelectedIndex = PVZ.Memory.ReadInteger(ConveyorBeltCards(CBConveyorBeltLevel.SelectedIndex, 0))
                    CBBeltCard2.SelectedIndex = PVZ.Memory.ReadInteger(ConveyorBeltCards(CBConveyorBeltLevel.SelectedIndex, 1))
                    CBBeltCard3.SelectedIndex = PVZ.Memory.ReadInteger(ConveyorBeltCards(CBConveyorBeltLevel.SelectedIndex, 2))
                    CBBeltCard4.SelectedIndex = PVZ.Memory.ReadInteger(ConveyorBeltCards(CBConveyorBeltLevel.SelectedIndex, 3))
                    CBBeltCard5.SelectedIndex = PVZ.Memory.ReadInteger(ConveyorBeltCards(CBConveyorBeltLevel.SelectedIndex, 4))
                    CBBeltCard6.SelectedIndex = PVZ.Memory.ReadInteger(ConveyorBeltCards(CBConveyorBeltLevel.SelectedIndex, 5))
                    CBBeltCard7.SelectedIndex = PVZ.Memory.ReadInteger(ConveyorBeltCards(CBConveyorBeltLevel.SelectedIndex, 6))
            End Select
        End If
    End Sub

    Private Sub CBBeltCard1_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
        If CBBeltCard1.IsMouseOver Then
            PVZ.Memory.WriteInteger(ConveyorBeltCards(CBConveyorBeltLevel.SelectedIndex, 0), CBBeltCard1.SelectedIndex)
        End If
    End Sub

    Private Sub CBBeltCard2_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
        If CBBeltCard2.IsMouseOver Then
            PVZ.Memory.WriteInteger(ConveyorBeltCards(CBConveyorBeltLevel.SelectedIndex, 1), CBBeltCard2.SelectedIndex)
        End If
    End Sub

    Private Sub CBBeltCard3_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
        If CBBeltCard3.IsMouseOver Then
            PVZ.Memory.WriteInteger(ConveyorBeltCards(CBConveyorBeltLevel.SelectedIndex, 2), CBBeltCard3.SelectedIndex)
        End If
    End Sub

    Private Sub CBBeltCard4_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
        If CBBeltCard4.IsMouseOver Then
            PVZ.Memory.WriteInteger(ConveyorBeltCards(CBConveyorBeltLevel.SelectedIndex, 3), CBBeltCard4.SelectedIndex)
        End If
    End Sub

    Private Sub CBBeltCard5_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
        If CBBeltCard5.IsMouseOver Then
            PVZ.Memory.WriteInteger(ConveyorBeltCards(CBConveyorBeltLevel.SelectedIndex, 4), CBBeltCard5.SelectedIndex)
        End If
    End Sub

    Private Sub CBBeltCard6_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
        If CBBeltCard6.IsMouseOver Then
            PVZ.Memory.WriteInteger(ConveyorBeltCards(CBConveyorBeltLevel.SelectedIndex, 5), CBBeltCard6.SelectedIndex)
        End If
    End Sub

    Private Sub CBBeltCard7_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
        If CBBeltCard7.IsMouseOver Then
            PVZ.Memory.WriteInteger(ConveyorBeltCards(CBConveyorBeltLevel.SelectedIndex, 6), CBBeltCard7.SelectedIndex)
        End If
    End Sub

    Private Sub CBBeltCardLilyPad_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
        If CBBeltCardLilyPad.IsMouseOver Then
            PVZ.Memory.WriteByte(&H422E2B, CBBeltCardLilyPad.SelectedIndex)
        End If
    End Sub

    Private Sub CBBeltCardJalapeno_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
        If CBBeltCardJalapeno.IsMouseOver Then
            PVZ.Memory.WriteByte(&H422E2E, CBBeltCardJalapeno.SelectedIndex)
        End If
    End Sub


    Private Sub CBEnableConveyorBelt_Click(sender As Object, e As RoutedEventArgs)
        ResumeConveyorBelt()
        If CBEnableConveyorBelt.IsChecked = True Then
            If CBConveyorBeltLevel.SelectedIndex <> -1 Then
                PVZ.Memory.WriteByte(&H41BECE, &H90)
            End If
            Select Case CBConveyorBeltLevel.SelectedIndex
                Case 0
                    PVZ.Memory.WriteByte(&H422E2F, &H70)
                Case 1
                    PVZ.Memory.WriteByte(&H422E89, &H70)
                Case 2
                    PVZ.Memory.WriteByte(&H422EE3, &H70)
                Case 3
                    PVZ.Memory.WriteByte(&H422F4D, &H70)
                Case 4
                    PVZ.Memory.WriteByte(&H422FC7, &H70)
                Case 5
                    PVZ.Memory.WriteByte(&H423089, &H70)
                Case 6
                    PVZ.Memory.WriteByte(&H423106, &H70)
                Case 7
                    PVZ.Memory.WriteByte(&H4230BC, &H70)
                Case 8
                    PVZ.Memory.WriteByte(&H423051, &H70)
                Case 9
                    PVZ.Memory.WriteByte(&H42315A, &H70)
                Case 10
                    PVZ.Memory.WriteByte(&H4231A9, &H70)
                Case 11
                    PVZ.Memory.WriteByte(&H4231FF, &H70)
                Case 12
                    PVZ.Memory.WriteByte(&H423253, &H70)
            End Select
        Else
            PVZ.Memory.WriteByte(&H41BECE, &HC3)
        End If
    End Sub

    Private Shared Sub ResumeConveyorBelt()
        PVZ.Memory.WriteByte(&H422E2F, &H75)
        PVZ.Memory.WriteByte(&H422E89, &H75)
        PVZ.Memory.WriteByte(&H422EE3, &H75)
        PVZ.Memory.WriteByte(&H422F4D, &H75)
        PVZ.Memory.WriteByte(&H422FC7, &H74)
        PVZ.Memory.WriteByte(&H423051, &H75)
        PVZ.Memory.WriteByte(&H423089, &H74)
        PVZ.Memory.WriteByte(&H4230BC, &H74)
        PVZ.Memory.WriteByte(&H423106, &H74)
        PVZ.Memory.WriteByte(&H42315A, &H74)
        PVZ.Memory.WriteByte(&H4231A9, &H75)
        PVZ.Memory.WriteByte(&H4231FF, &H75)
        PVZ.Memory.WriteByte(&H423253, &H75)
    End Sub

    Private Sub CBLSLimitPlant1_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
        If CBLSLimitPlant1.IsMouseOver Then
            Dim plant = CBLSLimitPlant1.SelectedIndex
            PVZ.Memory.WriteByte(&H482E1A, plant)
            PVZ.Memory.WriteByte(&H482F62, plant)
            PVZ.Memory.WriteByte(&H4832AA, plant)
            PVZ.Memory.WriteByte(&H484A3D, plant)
            PVZ.Memory.WriteByte(&H486B8C, plant)
            PVZ.Memory.WriteByte(&H484622, plant)
        End If
    End Sub

    Private Sub CBLSLimitPlant2_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
        If CBLSLimitPlant2.IsMouseOver Then
            Dim plant = CBLSLimitPlant2.SelectedIndex
            PVZ.Memory.WriteByte(&H482E1F, plant)
            PVZ.Memory.WriteByte(&H482F67, plant)
            PVZ.Memory.WriteByte(&H4832B3, plant)
            PVZ.Memory.WriteByte(&H484A42, plant)
            PVZ.Memory.WriteByte(&H486B95, plant)
            PVZ.Memory.WriteByte(&H484627, plant)
        End If
    End Sub

    Private Sub CBLSLimitPlant3_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
        If CBLSLimitPlant3.IsMouseOver Then
            Dim plant = CBLSLimitPlant3.SelectedIndex
            PVZ.Memory.WriteByte(&H482E24, plant)
            PVZ.Memory.WriteByte(&H482F6C, plant)
            PVZ.Memory.WriteByte(&H4832BC, plant)
            PVZ.Memory.WriteByte(&H484A47, plant)
            PVZ.Memory.WriteByte(&H486B9E, plant)
            PVZ.Memory.WriteByte(&H48462C, plant)
        End If
    End Sub

    Private Sub CBLSLimitPlant4_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
        If CBLSLimitPlant4.IsMouseOver Then
            Dim plant = CBLSLimitPlant4.SelectedIndex
            PVZ.Memory.WriteByte(&H482E29, plant)
            PVZ.Memory.WriteByte(&H482F71, plant)
            PVZ.Memory.WriteByte(&H4832C5, plant)
            PVZ.Memory.WriteByte(&H484A4C, plant)
            PVZ.Memory.WriteByte(&H486BA7, plant)
            PVZ.Memory.WriteByte(&H484631, plant)
        End If
    End Sub

    Private Sub CBLSLimitPlant5_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
        If CBLSLimitPlant5.IsMouseOver Then
            Dim plant = CBLSLimitPlant5.SelectedIndex
            PVZ.Memory.WriteByte(&H482E2E, plant)
            PVZ.Memory.WriteByte(&H482F76, plant)
            PVZ.Memory.WriteByte(&H4832CE, plant)
            PVZ.Memory.WriteByte(&H484A51, plant)
            PVZ.Memory.WriteByte(&H486BB0, plant)
            PVZ.Memory.WriteByte(&H484636, plant)
        End If
    End Sub

    Private Sub NudPlantLimitLine_ValueChanged(sender As Object, e As EventArgs)
        If IsLoaded Then
            Dim line As Integer = NudPlantLimitLine.Value
            PVZ.Memory.WriteByte(&H425583, line - 1)
            PVZ.Memory.WriteInteger(&H425392, 20 + 80 * line)
        End If
    End Sub

    Private Sub NudZombieLimitLine1_ValueChanged(sender As Object, e As EventArgs)
        If IsLoaded Then
            Dim line As Integer = NudZombieLimitLine1.Value
            PVZ.Memory.WriteByte(&H4255C4, line)
            PVZ.Memory.WriteInteger(&H4253C7, 20 + 80 * line)
        End If
    End Sub

    Private Sub NudZombieLimitLine2_ValueChanged(sender As Object, e As EventArgs)
        If IsLoaded Then
            Dim line As Integer = NudZombieLimitLine2.Value
            PVZ.Memory.WriteByte(&H4255DD, line)
            PVZ.Memory.WriteInteger(&H4253F7, 20 + 80 * line)
        End If
    End Sub

    Private Sub NudZombieLimitLine3_ValueChanged(sender As Object, e As EventArgs)
        If IsLoaded Then
            Dim line As Integer = NudZombieLimitLine3.Value
            PVZ.Memory.WriteByte(&H4255A9, line)
            PVZ.Memory.WriteInteger(&H425416, 20 + 80 * line)
        End If
    End Sub

    Private Sub TB95NewsZombieBodyHp_PreviewKeyDown(sender As Object, e As KeyEventArgs)
        If DealKeyDown(sender, e) Then
            PVZ.Memory.WriteInteger(&H4002FF, TB95NewsZombieBodyHp.Text)
        End If
    End Sub

    Private Sub TB95FlagZombieBodyHp_PreviewKeyDown(sender As Object, e As KeyEventArgs)
        If DealKeyDown(sender, e) Then
            PVZ.Memory.WriteInteger(&H40049F, TB95FlagZombieBodyHp.Text)
        End If
    End Sub

    Private Sub TB95NormalZombieBodyHp_PreviewKeyDown(sender As Object, e As KeyEventArgs)
        If DealKeyDown(sender, e) Then
            PVZ.Memory.WriteInteger(&H4004AB, TB95FlagZombieBodyHp.Text)
        End If
    End Sub

    Private Sub TB95HypnotizedZombieHpAdd_PreviewKeyDown(sender As Object, e As KeyEventArgs)
        If DealKeyDown(sender, e) Then
            PVZ.Memory.WriteInteger(&H400384, TB95HypnotizedZombieHpAdd.Text)
            PVZ.Memory.WriteInteger(&H400396, TB95HypnotizedZombieHpAdd.Text)
        End If
    End Sub

    Private Sub TB95TallnutCounterattackHp_PreviewKeyDown(sender As Object, e As KeyEventArgs)
        If DealKeyDown(sender, e) Then
            PVZ.Memory.WriteInteger(&H400464, TB95TallnutCounterattackHp.Text)
        End If
    End Sub

    Private Sub TB95BloverDamage_PreviewKeyDown(sender As Object, e As KeyEventArgs)
        If DealKeyDown(sender, e) Then
            Dim b As Byte = Math.Min(127, Convert.ToDouble(TB95BloverDamage.Text))
            PVZ.Memory.WriteInteger(&H40075E, b)
            PVZ.Memory.WriteByte(&H400766, b)
        End If
    End Sub

    Private Sub TB95IceLevelSunCondition_PreviewKeyDown(sender As Object, e As KeyEventArgs)
        If DealKeyDown(sender, e) Then
            PVZ.Memory.WriteInteger(&H400938, TB95IceLevelSunCondition.Text)
        End If
    End Sub

    Private Sub TB95NewspaperAngrySpeedMultiple_PreviewKeyDown(sender As Object, e As KeyEventArgs)
        If DealKeyDown(sender, e) Then
            PVZ.Memory.WriteDouble(&H4009BA, TB95NewspaperAngrySpeedMultiple.Text)
        End If
    End Sub

    Private Sub TB95JalapenoDamage_PreviewKeyDown(sender As Object, e As KeyEventArgs)
        If DealKeyDown(sender, e) Then
            PVZ.Memory.WriteInteger(&H466520, TB95JalapenoDamage.Text)
        End If
    End Sub
End Class
