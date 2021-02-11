Imports ITrainerExtension
Imports PVZClass

Public Class OperationWindow
    Private Sub Window_Loaded(sender As Object, e As RoutedEventArgs)
        Lang.ChangeLanguage(Content)
    End Sub

    Public scale As Integer = 100
    Private random As Random = New Random()
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
            Height = 670 * scale / 100
            Width = 600 * scale / 100
        End If
    End Sub

    Private Sub Window_MouseDown(sender As Object, e As MouseButtonEventArgs)
        Try
            DragMove()
        Catch ex As InvalidOperationException
        End Try
    End Sub

    Private Sub Window_Closed(sender As Object, e As EventArgs)
        If Not IsNothing(Application.Current.MainWindow) Then
            CType(Application.Current.MainWindow, MainWindow).BtnOperate.IsEnabled = True
        End If
    End Sub

    Private Sub BtnResumeLawnmover_Click(sender As Object, e As RoutedEventArgs)
        PVZ.ResumeLawnmover()
    End Sub

    Private Sub BtnStartLawnmover_Click(sender As Object, e As RoutedEventArgs)
        PVZ.StartLawnmover()
    End Sub

    Private Sub CreateBatch(action As Action(Of Integer, Integer))
        Dim row = NudRow.Value
        Dim column = NudColumn.Value
        If row = 0 And column > 0 Then
            For index = 0 To PVZ.RouteCount - 1
                action(index, column - 1)
            Next
        ElseIf column = 0 And row > 0 Then
            For jndex = 0 To 8
                action(row - 1, jndex)
            Next
        ElseIf column = 0 And row = 0 Then
            For index = 0 To PVZ.RouteCount - 1
                For jndex = 0 To 8
                    action(index, jndex)
                Next
            Next
        Else
            action(row - 1, column - 1)
        End If
    End Sub

    Private Sub BtnCreatePlant_Click(sender As Object, e As RoutedEventArgs)
        CreateBatch(AddressOf CreateSinglePlant)
    End Sub

    Private Sub CreateSinglePlant(row As Integer, column As Integer)
        Dim plant As PVZ.Plant = PVZ.CreatePlant(CBPlantTypes.SelectedIndex, row, column, CBPlantIsImitate.IsChecked.Value)
        Dim hp As Integer = Convert.ToDouble(TBPlantHp.Text)
        If hp > 0 Then
            plant.MaxHp = hp
            plant.Hp = hp
        End If
        If CBPlantIsFixed.IsChecked = True Then
            plant.Fix()
        End If
    End Sub

    Private Sub BtnCreateZombie_Click(sender As Object, e As RoutedEventArgs)
        CreateBatch(AddressOf CreateSingleZombie)
    End Sub

    Private Sub CreateSingleZombie(row As Integer, column As Integer)
        Dim zombie As PVZ.Zombie = PVZ.CreateZombie(CBZombieTypes.SelectedIndex, row, column)
        Dim hp As Integer = Convert.ToDouble(TBZombieBodyHp.Text)
        If hp > 0 Then
            zombie.MaxBodyHP = hp
            zombie.BodyHP = hp
        End If
        hp = Convert.ToDouble(TBZombieHatHp.Text)
        If hp > 0 Then
            zombie.MaxAccessoriesType1HP = hp
            zombie.AccessoriesType1HP = hp
        End If
        hp = Convert.ToDouble(TBZombieShieldHp.Text)
        If hp > 0 Then
            zombie.MaxAccessoriesType2HP = hp
            zombie.AccessoriesType2HP = hp
        End If
        If CBZombieIsHypnotized.IsChecked = True Then
            zombie.Hypnotized = True
        End If
    End Sub

    Private Sub BtnCreateLadder_Click(sender As Object, e As RoutedEventArgs)
        CreateBatch(AddressOf PVZ.CreateLadder)
    End Sub

    Private Sub BtnCreateGrave_Click(sender As Object, e As RoutedEventArgs)
        CreateBatch(AddressOf PVZ.CreateGrave)
    End Sub

    Private Sub BtnAutoLadder_Click(sender As Object, e As RoutedEventArgs)
        For Each griditem In PVZ.AllGriditems
            If griditem.Type = PVZ.GriditemType.Ladder Then
                griditem.Exist = False
            End If
        Next
        For Each plant In PVZ.AllPlants
            If plant.Type = PVZ.PlantType.Pumpkin Then
                PVZ.CreateLadder(plant.Row, plant.Column)
            End If
        Next
    End Sub

    Private Sub BtnCreatRake_Click(sender As Object, e As RoutedEventArgs)
        CreateBatch(AddressOf PVZ.CreateRake)
    End Sub

    Private Sub BtnCreateCoin_Click(sender As Object, e As RoutedEventArgs)
        If CBCoinGrid.IsChecked = True Then
            CreateBatch(AddressOf CreateGridCoin)
        ElseIf CBCoinGrid.IsChecked = False Then
            PVZ.CreateCoin(GetEnumTypeValue(Of PVZ.CoinType)(CBCoinTypes.SelectedIndex), NudColumn.Value, NudRow.Value, CBCoinMotionTypes.SelectedIndex, CBCardTypes.SelectedIndex)
        End If
    End Sub

    Private Function GetEnumTypeValue(Of TEnum)(value As Integer) As TEnum
        Return [Enum].GetValues(GetType(TEnum))(value)
    End Function

    Private Sub CreateGridCoin(row As Integer, column As Integer)
        PVZ.RCToXY(row, column)
        PVZ.CreateCoin(GetEnumTypeValue(Of PVZ.CoinType)(CBCoinTypes.SelectedIndex), column, row, CBCoinMotionTypes.SelectedIndex, CBCardTypes.SelectedIndex)
    End Sub

    Private Sub CreateSingleVase(row As Integer, column As Integer)
        Dim vaseContent As PVZ.VaseContent = CBVaseContent.SelectedIndex
        Dim vaseSkin As PVZ.VaseSkin = CBVaseSkin.SelectedIndex + 3
        Dim zombie As PVZ.ZombieType = CBVaseZombie.SelectedIndex
        Dim plant As PVZ.PlantType = CBVasePlant.SelectedIndex
        Dim sun As Integer = Convert.ToDouble(TBVaseSun.Text)
        If CBVaseRandom.IsChecked = True Then
            Dim per = random.Next(62)
            If per = 0 Then
                vaseContent = PVZ.VaseContent.None
            ElseIf per < 30 Then
                vaseContent = PVZ.VaseContent.Plant
            ElseIf per < 60 Then
                vaseContent = PVZ.VaseContent.Zombie
            Else
                vaseContent = PVZ.VaseContent.Sun
            End If
            vaseSkin = PVZ.VaseSkin.Unknow
            Select Case vaseContent
                Case PVZ.VaseContent.Zombie
                    zombie = random.Next(IIf(MIVaseRandomExclude.IsChecked, 25, 33))
                    If random.Next(40) = 0 Then
                        vaseSkin = PVZ.VaseSkin.Zombie
                    End If
                Case PVZ.VaseContent.Plant
                    plant = random.Next(IIf(MIVaseRandomExclude.IsChecked, 40, 53))
                    If random.Next(40) = 0 Then
                        vaseSkin = PVZ.VaseSkin.Leaf
                    End If
                Case PVZ.VaseContent.Sun
                    sun = random.Next(5)
            End Select
        End If
        PVZ.CreateVase(row, column, vaseContent, vaseSkin, zombie, plant, sun)
    End Sub
    Private Sub BtnCreateVase_Click(sender As Object, e As RoutedEventArgs)
        CreateBatch(AddressOf CreateSingleVase)
    End Sub

    Private Sub BtnCreateCaption_Click(sender As Object, e As RoutedEventArgs)
        If CBCaptionImageData.IsChecked = True Then
            PVZ.CreateImageCaption(TBCaptionText.Text)
        ElseIf CBCaptionImageData.IsChecked = False Then
            PVZ.CreateCaption(TBCaptionText.Text, GetEnumTypeValue(Of PVZ.CaptionStyle)(CBCaptionStyle.SelectedIndex))
        End If
    End Sub

    Private Sub BtnCreatePlantEffectType_Click(sender As Object, e As RoutedEventArgs)
        PVZ.CreatePlantEffect(GetEnumTypeValue(Of PVZ.PlantEffectType)(CBPlantEffectType.SelectedIndex), NudColumn.Value, NudRow.Value)
    End Sub

    Private Sub BtnCreateEffect_Click(sender As Object, e As RoutedEventArgs)
        PVZ.CreateEffect(Convert.ToDouble(TBEffectType.Text), NudColumn.Value, NudRow.Value)
    End Sub

    Private Sub BtnCreateSound_Click(sender As Object, e As RoutedEventArgs)
        If BtnCreateSound.IsChecked = True Then
            PVZ.CreateSound(Convert.ToDouble(TBSoundType.Text))
        Else
            PVZ.StopSound(Convert.ToDouble(TBSoundType.Text))
        End If
    End Sub

    Private Sub BtnCreateExplosion_Click(sender As Object, e As RoutedEventArgs)
        PVZ.CreateExplosion(NudColumn.Value, NudRow.Value, Convert.ToDouble(TBExplosionRadius.Text), CBIsCinder.IsChecked.Value, NudExplosionBound.Value, CBIsEnemy.IsChecked.Value)
    End Sub
End Class
