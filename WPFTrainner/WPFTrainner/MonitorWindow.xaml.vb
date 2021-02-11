Imports System.Text.RegularExpressions
Imports System.Windows.Media.Animation
Imports ITrainerExtension
Imports PVZClass
Public Class MonitorWindow
    Private ReadOnly tracker As New HpTrackWindow()
    Private ReadOnly Timers(7) As Threading.DispatcherTimer
    Private ReadOnly CheckBoxes As List(Of CheckBox) = New List(Of CheckBox)
    Private prezombienum As Integer = 0
    Private preselzombieid As Integer = 0
    Private zombie As PVZ.Zombie
    Private preplantnum As Integer = 0
    Private preselplantid As Integer = 0
    Private plant As PVZ.Plant
    Private precoinnum As Integer = 0
    Private preselcoinid As Integer = 0
    Private coin As PVZ.Coin
    Private pregriditemnum As Integer = 0
    Private preselgriditemid As Integer = 0
    Private griditem As PVZ.Griditem
    Private crater As PVZ.Crater
    Private vase As PVZ.Vase
    Private precardmnum As Integer = 0
    Private preselcardid As Integer = 0
    Private card As PVZ.CardSlot.SeedCard
    Private Function DealKeyDown(sender As Object, e As KeyEventArgs) As Boolean
        If e.Key = Key.Space Then
            sender.Text = "0"
            e.Handled = True
        ElseIf e.Key = Key.V Then
            e.Handled = True
        End If
        Return e.Key = Key.Enter
    End Function
    Public Sub New()
        ' 此调用是设计器所必需的。
        InitializeComponent()
        ' 在 InitializeComponent() 调用之后添加任何初始化。
        For i = 0 To 7
            Timers(i) = New Threading.DispatcherTimer()
            Timers(i).Interval = New TimeSpan(100)
        Next
        AddHandler Timers(0).Tick, AddressOf Timer1Tick
        AddHandler Timers(1).Tick, AddressOf Timer2Tick
        AddHandler Timers(2).Tick, AddressOf Timer3Tick
        AddHandler Timers(3).Tick, AddressOf Timer4Tick
        AddHandler Timers(4).Tick, AddressOf Timer5Tick
        AddHandler Timers(5).Tick, AddressOf Timer6Tick
        AddHandler Timers(6).Tick, AddressOf Timer7Tick
        AddHandler Timers(7).Tick, AddressOf Timer8Tick
    End Sub
    Private Sub Window_Loaded(sender As Object, e As RoutedEventArgs)
        For i = 0 To 8
            Dim panel = New StackPanel()
            For j = 0 To 5
                Dim cbox = New DarkStyle.DarkCheckBox()
                AddHandler cbox.Click,
                    Sub(_sender As CheckBox, _e As RoutedEventArgs)
                        Dim index = CheckBoxes.IndexOf(_sender)
                        PVZ.Miscellaneous.SetCrater(index Mod 6, Math.Floor(index / 6), _sender.IsChecked)
                    End Sub
                CheckBoxes.Add(cbox)
                panel.Children.Add(cbox)
            Next
            SPCrater.Children.Add(panel)
        Next
        Lang.ChangeLanguage(Content)
    End Sub
    Private Sub Window_MouseDown(sender As Object, e As MouseButtonEventArgs)
        Try
            DragMove()
        Catch ex As InvalidOperationException
        End Try
    End Sub
    Private Sub Window_Closed(sender As Object, e As EventArgs)
        tracker.Close()
        If Not IsNothing(Application.Current.MainWindow) Then
            CType(Application.Current.MainWindow, MainWindow).BtnMonitor.IsEnabled = True
        End If
    End Sub
    Private Sub TCMain_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
        If Not IsLoaded Then Return
        Try
            For Each item In TCMain.Items
                Timers(item.TabIndex).Stop()
            Next
            Timers(TCMain.SelectedItem.TabIndex).Start()
        Catch ex As Exception
        End Try
    End Sub

    '鼠标
    Private Sub Timer1Tick(sender As Object, e As EventArgs)
        TBMouseInGameArea.Text = IIf(PVZ.Mouse.InGameArea, "是", "否")
        If Lang.Id = 1 Then
            TBMouseInGameArea.Text = PVZ.Mouse.InGameArea
        End If
        TBMouseX.Text = PVZ.Mouse.X
        TBMouseY.Text = PVZ.Mouse.Y
        TBMouseRow.Text = PVZ.MousePointer.Row
        TBMouseColumn.Text = PVZ.MousePointer.Column
        Select Case PVZ.Mouse.ClickState
            Case PVZ.MouseClickState.None
                TBMouseStateLeft.Foreground = Brushes.White
                TBMouseStateMid.Foreground = Brushes.White
                TBMouseStateRight.Foreground = Brushes.White
            Case PVZ.MouseClickState.LButton
                TBMouseStateLeft.Foreground = Brushes.Red
                TBMouseStateMid.Foreground = Brushes.White
                TBMouseStateRight.Foreground = Brushes.White
            Case PVZ.MouseClickState.RButton
                TBMouseStateLeft.Foreground = Brushes.White
                TBMouseStateMid.Foreground = Brushes.White
                TBMouseStateRight.Foreground = Brushes.Red
            Case PVZ.MouseClickState.LRButton
                TBMouseStateLeft.Foreground = Brushes.Red
                TBMouseStateMid.Foreground = Brushes.White
                TBMouseStateRight.Foreground = Brushes.Red
            Case PVZ.MouseClickState.MidButton
                TBMouseStateLeft.Foreground = Brushes.White
                TBMouseStateMid.Foreground = Brushes.Red
                TBMouseStateRight.Foreground = Brushes.White
            Case PVZ.MouseClickState.LMidButton
                TBMouseStateLeft.Foreground = Brushes.Red
                TBMouseStateMid.Foreground = Brushes.Red
                TBMouseStateRight.Foreground = Brushes.White
            Case PVZ.MouseClickState.RMidButton
                TBMouseStateLeft.Foreground = Brushes.White
                TBMouseStateMid.Foreground = Brushes.Red
                TBMouseStateRight.Foreground = Brushes.Red
            Case PVZ.MouseClickState.LRMidButton
                TBMouseStateLeft.Foreground = Brushes.Red
                TBMouseStateMid.Foreground = Brushes.Red
                TBMouseStateRight.Foreground = Brushes.Red
        End Select
    End Sub
    '僵尸
    Private Sub Timer2Tick(sender As Object, e As EventArgs)
        If PVZ.ZombiesCount <> prezombienum Then
            If prezombienum <> 0 Then
                LBZombies_SelectionChanged(Nothing, Nothing)
            End If
            LBZombies.Items.Clear()
            For Each zombie In PVZ.AllZombies
                Dim tb As New TextBlock()
                tb.Text = zombie.Id And &HFFFF
                tb.Foreground = Brushes.White
                tb.HorizontalAlignment = HorizontalAlignment.Center
                LBZombies.Items.Add(tb)
            Next
            prezombienum = PVZ.ZombiesCount
            For Each item As TextBlock In LBZombies.Items
                If item.Text = preselzombieid Then
                    LBZombies.SelectedItem = item
                End If
            Next
        End If
        If LBZombies.SelectedIndex >= 0 Then
            zombie = New PVZ.Zombie(LBZombies.SelectedItem.Text)
            TBZombieType.Text = zombie.Type.GetDescription()
            TBZombieId.Text = "id = " + zombie.Id.ToString()
            If Lang.Id = 1 Then
                TBZombieType.Text = zombie.Type.ToString()
            End If
            If Not TBZombieX.IsFocused Then
                TBZombieX.Text = zombie.X
            End If
            If Not TBZombieY.IsFocused Then
                TBZombieY.Text = zombie.Y
            End If
            If Not NudZombieRow.IsMouseOver Then
                NudZombieRow.IgnoreAssign = True
                NudZombieRow.Value = zombie.Row + 1
                NudZombieRow.IgnoreAssign = False
            End If
            If Not TBZombieState.IsFocused Then
                TBZombieState.Text = zombie.State
            End If
            If Not TBZombieBodyHp.IsFocused Then
                TBZombieBodyHp.Text = zombie.BodyHP
            End If
            If Not TBZombieA1Hp.IsFocused Then
                TBZombieA1Hp.Text = zombie.AccessoriesType1HP
            End If
            If Not TBZombieA2Hp.IsFocused Then
                TBZombieA2Hp.Text = zombie.AccessoriesType2HP
            End If
            If Not CBZombieVisible.IsMouseOver Then
                CBZombieVisible.IsChecked = Not zombie.Visible
            End If
            If Not CBZombieHypnotized.IsMouseOver Then
                CBZombieHypnotized.IsChecked = zombie.Hypnotized
            End If
            If Not CBZombieBlowaway.IsMouseOver Then
                CBZombieBlowaway.IsChecked = zombie.Blowaway
            End If
            If Not CBZombieDying.IsMouseOver Then
                CBZombieDying.IsChecked = zombie.Dying
            End If
            If Not CBZombieGarlicBited.IsMouseOver Then
                CBZombieGarlicBited.IsChecked = zombie.GarlicBited
            End If
            If Not CBZombieExist.IsMouseOver Then
                CBZombieExist.IsChecked = zombie.Exist
            End If
            If Not SZombieDecelerate.IsMouseOver Then
                SZombieDecelerate.Value = zombie.DecelerateCountdown / 100
            End If
            If Not SZombieFixed.IsMouseOver Then
                SZombieFixed.Value = zombie.FixedCountdown / 100
            End If
            If Not SZombieFrozen.IsMouseOver Then
                SZombieFrozen.Value = zombie.FrozenCountdown / 100
            End If
        End If
    End Sub
    '植物
    Private Sub Timer3Tick(sender As Object, e As EventArgs)
        If PVZ.PlantsCount <> preplantnum Then
            If preplantnum <> 0 Then
                LBPlants_SelectionChanged(Nothing, Nothing)
            End If
            LBPlants.Items.Clear()
            For Each plant In PVZ.AllPlants
                Dim tb As New TextBlock()
                tb.Text = plant.Id And &HFFFF
                tb.Foreground = Brushes.White
                tb.HorizontalAlignment = HorizontalAlignment.Center
                LBPlants.Items.Add(tb)
            Next
            preplantnum = PVZ.PlantsCount
            For Each item As TextBlock In LBPlants.Items
                If item.Text = preselplantid Then
                    LBPlants.SelectedItem = item
                End If
            Next
        End If
        If LBPlants.SelectedIndex >= 0 Then
            plant = New PVZ.Plant(LBPlants.SelectedItem.Text)
            TBPlantType.Text = plant.Type.GetDescription()
            If Lang.Id = 1 Then
                TBPlantType.Text = plant.Type.ToString()
            End If
            TBPlantId.Text = "id = " + plant.Id.ToString()
            If Not TBPlantX.IsFocused Then
                TBPlantX.Text = plant.X
            End If
            If Not TBPlantY.IsFocused Then
                TBPlantY.Text = plant.Y
            End If
            If Not TBPlantRow.IsFocused Then
                TBPlantRow.Text = plant.Row
            End If
            If Not TBPlantColumn.IsFocused Then
                TBPlantColumn.Text = plant.Column
            End If
            If Not TBPlantState.IsFocused Then
                TBPlantState.Text = plant.State
            End If
            If Not TBPlantHp.IsFocused Then
                TBPlantHp.Text = plant.Hp
            End If
            If Not CBPlantVisible.IsMouseOver Then
                CBPlantVisible.IsChecked = Not plant.Visible
            End If
            If Not CBPlantAggressive.IsMouseOver Then
                CBPlantAggressive.IsChecked = plant.Aggressive
            End If
            If Not CBPlantSquash.IsMouseOver Then
                CBPlantSquash.IsChecked = plant.Squash
            End If
            If Not CBPlantSleeping.IsMouseOver Then
                CBPlantSleeping.IsChecked = plant.Sleeping
            End If
            If Not CBPlantExist.IsMouseOver Then
                CBPlantExist.IsChecked = plant.Exist
            End If
            If Not SPlantProduct.IsMouseOver Then
                SPlantProduct.Value = plant.ShootOrProductCountdown / 100
            End If
            If Not SPlantAttribute.IsMouseOver Then
                SPlantAttribute.Value = plant.AttributeCountdown / 100
            End If
            If Not SPlantShooting.IsMouseOver Then
                SPlantShooting.Value = plant.ShootingCountdown / 100
            End If
            If Not TBPlantProductInterval.IsFocused Then
                TBPlantProductInterval.Text = plant.Column
            End If
        End If
    End Sub
    '掉落物
    Private Sub Timer4Tick(sender As Object, e As EventArgs)
        If PVZ.CoinsCount <> precoinnum Then
            If precoinnum <> 0 Then
                LBCoins_SelectionChanged(Nothing, Nothing)
            End If
            LBCoins.Items.Clear()
            For Each coin In PVZ.AllCoins
                Dim tb As New TextBlock()
                tb.Text = coin.Id And &HFFFF
                tb.Foreground = Brushes.White
                tb.HorizontalAlignment = HorizontalAlignment.Center
                LBCoins.Items.Add(tb)
            Next
            precoinnum = PVZ.CoinsCount
            For Each item As TextBlock In LBCoins.Items
                If item.Text = preselcoinid Then
                    LBCoins.SelectedItem = item
                End If
            Next
        End If
        If LBCoins.SelectedIndex >= 0 Then
            coin = New PVZ.Coin(LBCoins.SelectedItem.Text)
            TBCoinType.Text = coin.Type.GetDescription()
            If Lang.Id = 1 Then
                TBCoinType.Text = coin.Type.ToString()
            End If
            TBCoinId.Text = "id = " + coin.Id.ToString()
            If Not TBCoinX.IsFocused Then
                TBCoinX.Text = coin.X
            End If
            If Not TBCoinY.IsFocused Then
                TBCoinY.Text = coin.Y
            End If
            If Not TBCoinSize.IsFocused Then
                TBCoinSize.Text = coin.Size
            End If
            If Not CBCoinCard.IsFocused Then
                CBCoinCard.SelectedIndex = coin.CardType
            End If
            If Not CBCoinVisible.IsMouseOver Then
                CBCoinVisible.IsChecked = Not coin.Visible
            End If
            If Not CBCoinCollected.IsMouseOver Then
                CBCoinCollected.IsChecked = coin.Collected
            End If
            If Not CBCoinHalo.IsMouseOver Then
                CBCoinHalo.IsChecked = coin.Halo
            End If

        End If
    End Sub
    '场地物品
    Private Sub Timer5Tick(sender As Object, e As EventArgs)
        If PVZ.GriditemsCount <> pregriditemnum Then
            If pregriditemnum <> 0 Then
                LBGriditems_SelectionChanged(Nothing, Nothing)
            End If
            LBGriditems.Items.Clear()
            For Each griditem In PVZ.AllGriditems
                Dim tb As New TextBlock()
                tb.Text = griditem.Id And &HFFFF
                tb.Foreground = Brushes.White
                tb.HorizontalAlignment = HorizontalAlignment.Center
                LBGriditems.Items.Add(tb)
            Next
            pregriditemnum = PVZ.GriditemsCount
            For Each item As TextBlock In LBGriditems.Items
                If item.Text = preselgriditemid Then
                    LBGriditems.SelectedItem = item
                End If
            Next
        End If
        If LBGriditems.SelectedIndex >= 0 Then
            crater = New PVZ.Crater(LBGriditems.SelectedItem.Text)
            vase = New PVZ.Vase(crater.BaseAddress)
            griditem = crater
            TBGriditemType.Text = griditem.Type.GetDescription()
            If Lang.Id = 1 Then
                TBGriditemType.Text = griditem.Type.ToString()
            End If
            TBGriditemId.Text = "id = " + griditem.Id.ToString()
            If Not TBGriditemRow.IsFocused Then
                TBGriditemRow.Text = griditem.Row
            End If
            If Not TBGriditemColumn.IsFocused Then
                TBGriditemColumn.Text = griditem.Column
            End If
            If Not CBGriditemExist.IsMouseOver Then
                CBGriditemExist.IsChecked = griditem.Exist
            End If
            If Not SCraterDisappear.IsMouseOver Then
                SCraterDisappear.Value = crater.DisappearCountdown / 100
            End If
            If Not CBVaseSkin.IsFocused Then
                CBVaseSkin.SelectedIndex = Math.Max(vase.Skin - 3， 0)
            End If
            If Not CBVaseContent.IsFocused Then
                CBVaseContent.SelectedIndex = vase.Content
            End If
            If Not CBVaseZombie.IsFocused Then
                CBVaseZombie.SelectedIndex = vase.Zombie
            End If
            If Not CBVasePlant.IsFocused Then
                CBVasePlant.SelectedIndex = vase.Plant
            End If
            If Not TBVaseSun.IsFocused Then
                TBVaseSun.Text = vase.Sun
            End If
            If Not SVaseTransparent.IsMouseOver Then
                SVaseTransparent.Value = vase.TransparentCountDown / 100
            End If
        End If
    End Sub
    '卡槽卡片
    Private Sub Timer6Tick(sender As Object, e As EventArgs)
        If PVZ.CardSlot.CardNum <> precardmnum Then
            If precardmnum <> 0 Then
                LBCards_SelectionChanged(Nothing, Nothing)
            End If
            LBCards.Items.Clear()
            For i = 0 To PVZ.CardSlot.CardNum - 1
                card = PVZ.CardSlot.GetCard(i)
                Dim tb As New TextBlock()
                tb.Text = card.Index
                tb.Foreground = Brushes.White
                tb.HorizontalAlignment = HorizontalAlignment.Center
                LBCards.Items.Add(tb)
            Next
            precardmnum = PVZ.CardSlot.CardNum
            For Each item As TextBlock In LBCards.Items
                If item.Text = precardmnum Then
                    LBCards.SelectedItem = item
                End If
            Next
        End If
        If Not CBCardSlotVisible.IsMouseOver Then
            CBCardSlotVisible.IsChecked = PVZ.CardSlot.Visible
        End If
        If Not CBCardNum.IsMouseOver Then
            CBCardNum.SelectedIndex = PVZ.CardSlot.CardNum
        End If
        If LBCards.SelectedIndex >= 0 Then
            card = PVZ.CardSlot.GetCard(LBCards.SelectedItem.Text)
            If Not TBCardX.IsFocused Then
                TBCardX.Text = card.X
            End If
            If Not TBCardY.IsFocused Then
                TBCardY.Text = card.Y
            End If
            If Not CBCardVisible.IsMouseOver Then
                CBCardVisible.IsChecked = card.Visible
            End If
            If Not CBCardEnable.IsMouseOver Then
                CBCardEnable.IsChecked = card.Enable
            End If
            If Not CBCardActive.IsMouseOver Then
                CBCardActive.IsChecked = card.Active
            End If
            If Not TBCoolDown.IsFocused Then
                TBCoolDown.Text = card.CoolDownInterval
            End If
            If Not CBCardType.IsFocused Then
                CBCardType.SelectedIndex = card.CardType
            End If
            If Not CBCardTypeImitative.IsFocused Then
                CBCardTypeImitative.SelectedIndex = card.ImitativeCardType
            End If
            If Not SCardCoolDowm.IsMouseOver Then
                SCardCoolDowm.Maximum = Math.Max(card.CoolDownInterval, SCardCoolDowm.Maximum) / 100 + 5
                SCardCoolDowm.Value = card.CoolDown / 100
            End If
            If Not SCardBeltX.IsMouseOver Then
                SCardBeltX.Value = card.ConveyorBeltX
            End If
        End If
    End Sub
    '杂项，宝石迷阵
    Private Sub Timer7Tick(sender As Object, e As EventArgs)
        For i = 0 To 8
            For j = 0 To 5
                CheckBoxes(i * 6 + j).IsChecked = PVZ.Miscellaneous.HaveCrater(j, i)
            Next
        Next
        If Not CBUpgradedRepeater.IsMouseOver Then
            CBUpgradedRepeater.IsChecked = PVZ.Miscellaneous.UpgradedRepeater
        End If
        If Not CBUpgradedFumeshroon.IsMouseOver Then
            CBUpgradedFumeshroon.IsChecked = PVZ.Miscellaneous.UpgradedFumeshroon
        End If
        If Not CBUpgradedTallnut.IsMouseOver Then
            CBUpgradedTallnut.IsChecked = PVZ.Miscellaneous.UpgradedTallnut
        End If
        If Not SAttributeTime.IsMouseOver Then
            SAttributeTime.Value = PVZ.Miscellaneous.AttributeCountdown / 100
        End If
        If Not TBLevelProcess.IsFocused Then
            TBLevelProcess.Text = PVZ.Miscellaneous.LevelProcess
        End If
        If Not TBLevelRound.IsFocused Then
            TBLevelRound.Text = PVZ.Miscellaneous.Round
        End If
        If Not CBSceneType.IsFocused Then
            CBSceneType.SelectedIndex = PVZ.Scene
        End If
    End Sub
    '背景音乐
    Private Sub Timer8Tick(sender As Object, e As EventArgs)
        If Not CBMusicType.IsMouseOver Then
            CBMusicType.SelectedIndex = Math.Max(0, PVZ.Music.Type - 1)
        End If
        If Not CBINGAMEEnable.IsMouseOver Then
            CBINGAMEEnable.IsChecked = PVZ.Music.INGAMEEnable
        End If
        If Not CBINGAMEStart.IsMouseOver Then
            CBINGAMEStart.IsChecked = PVZ.Music.INGAMEStart
        End If
        TBMusicBPM.Text = PVZ.Music.Tempo
        TBMusivTicksRow.Text = PVZ.Music.TicksRow
    End Sub
#Region "鼠标"
    Private Sub TBoxMouseX_PreviewKeyDown(sender As Object, e As KeyEventArgs)
        DealKeyDown(sender, e)
    End Sub
    Private Sub BtnWMClick_Click(sender As Object, e As RoutedEventArgs)
        Dim ValueX = MainWindow.StrToInt(TBoxMouseX.Text)
        Dim ValueY = MainWindow.StrToInt(TBoxMouseY.Text)
        PVZ.Mouse.WMLClick(ValueX, ValueY)
        TBoxMouseX.Text = CStr(ValueX)
        TBoxMouseY.Text = CStr(ValueY)
    End Sub
#End Region
#Region "僵尸"
    Private Sub TBZombieId_MouseDown(sender As Object, e As MouseButtonEventArgs)
        If TBZombieId.Text <> "id=" Then
            Clipboard.SetText(TBZombieId.Text.Substring(5))
        End If
    End Sub
    Private Sub LBZombies_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
        Try
            preselzombieid = CInt(LBZombies.SelectedItem.Text)
        Catch ex As Exception
        End Try
    End Sub
    Private Sub TBZombieX_PreviewKeyDown(sender As Object, e As KeyEventArgs)
        If DealKeyDown(sender, e) Then
            If Not IsNothing(zombie) Then
                zombie.X = TBZombieX.Text
            End If
        End If
    End Sub
    Private Sub TBZombieY_PreviewKeyDown(sender As Object, e As KeyEventArgs)
        If DealKeyDown(sender, e) Then
            If Not IsNothing(zombie) Then
                zombie.Y = TBZombieY.Text
            End If
        End If
    End Sub
    Private Sub NudZombieRow_ValueChanged(sender As Object, e As EventArgs)
        If Not IsNothing(zombie) Then
            zombie.Row = NudZombieRow.Value - 1
        End If
    End Sub
    Private Sub TBZombieState_PreviewKeyDown(sender As Object, e As KeyEventArgs)
        If DealKeyDown(sender, e) Then
            If Not IsNothing(zombie) Then
                zombie.State = TBZombieState.Text
            End If
        End If
    End Sub
    Private Sub TBZombieBodyHp_PreviewKeyDown(sender As Object, e As KeyEventArgs)
        If DealKeyDown(sender, e) Then
            If Not IsNothing(zombie) Then
                zombie.BodyHP = TBZombieBodyHp.Text
                If zombie.BodyHP > zombie.MaxBodyHP Then
                    zombie.MaxBodyHP = zombie.BodyHP
                End If
            End If
        End If
    End Sub
    Private Sub TBZombieA1Hp_PreviewKeyDown(sender As Object, e As KeyEventArgs)
        If DealKeyDown(sender, e) Then
            If Not IsNothing(zombie) Then
                zombie.AccessoriesType1HP = TBZombieA1Hp.Text
                If zombie.AccessoriesType1HP > zombie.MaxAccessoriesType1HP Then
                    zombie.MaxAccessoriesType1HP = zombie.AccessoriesType1HP
                End If
            End If
        End If
    End Sub
    Private Sub TBZombieA2Hp_PreviewKeyDown(sender As Object, e As KeyEventArgs)
        If DealKeyDown(sender, e) Then
            If Not IsNothing(zombie) Then
                zombie.AccessoriesType2HP = TBZombieA2Hp.Text
                If zombie.AccessoriesType2HP > zombie.MaxAccessoriesType2HP Then
                    zombie.MaxAccessoriesType2HP = zombie.AccessoriesType2HP
                End If
            End If
        End If
    End Sub
    Private Sub CBZombieVisible_Click(sender As Object, e As RoutedEventArgs)
        If Not IsNothing(zombie) Then
            zombie.Visible = Not CBZombieVisible.IsChecked
        End If
    End Sub
    Private Sub CBZombieHypnotized_Click(sender As Object, e As RoutedEventArgs)
        If Not IsNothing(zombie) Then
            zombie.Hypnotized = CBZombieHypnotized.IsChecked
        End If
    End Sub
    Private Sub CBZombieBlowaway_Click(sender As Object, e As RoutedEventArgs)
        If Not IsNothing(zombie) Then
            zombie.Blowaway = CBZombieBlowaway.IsChecked
        End If
    End Sub
    Private Sub CBZombieDying_Click(sender As Object, e As RoutedEventArgs)
        If Not IsNothing(zombie) Then
            zombie.Dying = CBZombieDying.IsChecked
        End If
    End Sub
    Private Sub CBZombieGarlicBited_Click(sender As Object, e As RoutedEventArgs)
        If Not IsNothing(zombie) Then
            zombie.GarlicBited = CBZombieGarlicBited.IsChecked
        End If
    End Sub
    Private Sub CBZombieExist_Click(sender As Object, e As RoutedEventArgs)
        If Not IsNothing(zombie) Then
            zombie.Exist = CBZombieExist.IsChecked
        End If
    End Sub
    Private Sub SZombieDecelerate_ValueChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of Double))
        If SZombieDecelerate.IsMouseOver And Not IsNothing(zombie) Then
            zombie.DecelerateCountdown = SZombieDecelerate.Value * 100
        End If
    End Sub
    Private Sub SZombieFixed_ValueChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of Double))
        If SZombieFixed.IsMouseOver And Not IsNothing(zombie) Then
            zombie.FixedCountdown = SZombieFixed.Value * 100
        End If
    End Sub
    Private Sub SZombieFrozen_ValueChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of Double))
        If SZombieFrozen.IsMouseOver And Not IsNothing(zombie) Then
            zombie.FrozenCountdown = SZombieFrozen.Value * 100
        End If
    End Sub
    Private Sub BtnZombieButter_Click(sender As Object, e As RoutedEventArgs)
        If Not IsNothing(zombie) Then
            zombie.Butter()
        End If
    End Sub
    Private Sub BtnZombieBlast_Click(sender As Object, e As RoutedEventArgs)
        If Not IsNothing(zombie) Then
            zombie.Blast()
        End If
    End Sub
    Private Sub BtnZombieHit_Click(sender As Object, e As RoutedEventArgs)
        If Not IsNothing(zombie) Then
            Dim dt = CBZombieDamageType.SelectedIndex
            If dt >= 3 Then
                dt += 1
            End If
            zombie.Hit(TBZombieDamage.Text, dt)
        End If
    End Sub
#End Region
    '血量窗口显示
    Private Sub CBHpTrack_Click(sender As Object, e As RoutedEventArgs)
        If CBHpTrack.IsChecked Then
            tracker.IsHide = False
            tracker.Show()
        Else
            tracker.Visibility = Visibility.Collapsed
            tracker.IsHide = True
        End If
    End Sub
    '血量颜色
    Private Sub TBColor_PreviewKeyDown(sender As Object, e As KeyEventArgs)
        If e.Key = Key.Enter Then
            Try
                tracker.hpfontcolor = ColorConverter.ConvertFromString(TBColor.Text)
            Catch ex As Exception
                MessageBox.Show("无法转换指定颜色", "错误", MessageBoxButton.OK, MessageBoxImage.Error)
            End Try
            TBColor.Foreground = New SolidColorBrush(tracker.hpfontcolor)
        End If
    End Sub
#Region "植物"
    Private Sub LBPlants_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
        Try
            preselplantid = CInt(LBPlants.SelectedItem.Text)
        Catch ex As Exception
        End Try
    End Sub
    Private Sub TBPlantX_PreviewKeyDown(sender As Object, e As KeyEventArgs)
        If DealKeyDown(sender, e) Then
            If Not IsNothing(plant) Then
                plant.X = TBPlantX.Text
            End If
        End If
    End Sub
    Private Sub TBPlantY_PreviewKeyDown(sender As Object, e As KeyEventArgs)
        If DealKeyDown(sender, e) Then
            If Not IsNothing(plant) Then
                plant.Y = TBPlantY.Text
            End If
        End If
    End Sub
    Private Sub TBPlantRow_PreviewKeyDown(sender As Object, e As KeyEventArgs)
        If DealKeyDown(sender, e) Then
            If Not IsNothing(plant) Then
                plant.Row = TBPlantRow.Text
            End If
        End If
    End Sub
    Private Sub TBPlantColumn_PreviewKeyDown(sender As Object, e As KeyEventArgs)
        If DealKeyDown(sender, e) Then
            If Not IsNothing(plant) Then
                plant.Column = TBPlantColumn.Text
            End If
        End If
    End Sub
    Private Sub TBPlantState_PreviewKeyDown(sender As Object, e As KeyEventArgs)
        If DealKeyDown(sender, e) Then
            If Not IsNothing(plant) Then
                plant.State = TBPlantState.Text
            End If
        End If
    End Sub
    Private Sub TBPlantHp_PreviewKeyDown(sender As Object, e As KeyEventArgs)
        If DealKeyDown(sender, e) Then
            If Not IsNothing(plant) Then
                plant.Hp = TBPlantHp.Text
                If plant.Hp > plant.MaxHp Then
                    plant.MaxHp = plant.Hp
                End If
            End If
        End If
    End Sub
    Private Sub CBPlantVisible_Click(sender As Object, e As RoutedEventArgs)
        If Not IsNothing(plant) Then
            plant.Visible = Not CBPlantVisible.IsChecked
        End If
    End Sub
    Private Sub CBPlantAggressive_Click(sender As Object, e As RoutedEventArgs)
        If Not IsNothing(plant) Then
            plant.Aggressive = CBPlantAggressive.IsChecked
        End If
    End Sub
    Private Sub CBPlantSquash_Click(sender As Object, e As RoutedEventArgs)
        If Not IsNothing(plant) Then
            plant.Squash = CBPlantSquash.IsChecked
        End If
    End Sub
    Private Sub CBPlantSleeping_Click(sender As Object, e As RoutedEventArgs)
        If Not IsNothing(plant) Then
            plant.Sleeping = CBPlantSleeping.IsChecked
        End If
    End Sub
    Private Sub CBPlantExist_Click(sender As Object, e As RoutedEventArgs)
        If Not IsNothing(plant) Then
            plant.Exist = CBPlantExist.IsChecked
        End If
    End Sub
    Private Sub SPlantProduct_ValueChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of Double))
        If SPlantProduct.IsMouseOver And Not IsNothing(plant) Then
            plant.ShootOrProductCountdown = SPlantProduct.Value * 100
        End If
    End Sub
    Private Sub SPlantAttribute_ValueChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of Double))
        If SPlantAttribute.IsMouseOver And Not IsNothing(plant) Then
            plant.AttributeCountdown = SPlantProduct.Value * 100
        End If
    End Sub
    Private Sub SPlantShooting_ValueChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of Double))
        If SPlantShooting.IsMouseOver And Not IsNothing(plant) Then
            plant.ShootingCountdown = SPlantShooting.Value * 100
        End If
    End Sub
    Private Sub TBPlantProductInterval_PreviewKeyDown(sender As Object, e As KeyEventArgs)
        If DealKeyDown(sender, e) Then
            If Not IsNothing(plant) Then
                plant.ShootOrProductInterval = TBPlantProductInterval.Text
            End If
        End If
    End Sub
    Private Sub BtnPlantEffect_Click(sender As Object, e As RoutedEventArgs)
        If Not IsNothing(plant) Then
            plant.CreateEffect()
        End If
    End Sub
    Private Sub BtnPlantFix_Click(sender As Object, e As RoutedEventArgs)
        If Not IsNothing(plant) Then
            plant.Fix()
        End If
    End Sub
    Private Sub BtnPlantFlash_Click(sender As Object, e As RoutedEventArgs)
        If Not IsNothing(plant) Then
            plant.Flash()
        End If
    End Sub
    Private Sub BtnPlantLight_Click(sender As Object, e As RoutedEventArgs)
        If Not IsNothing(plant) Then
            plant.Light(TBPlantLight.Text)
        End If
    End Sub
    Private Sub BtnPlantShoot_Click(sender As Object, e As RoutedEventArgs)
        If Not IsNothing(plant) Then
            plant.Shoot(TBPlantShoot.Text)
        End If
    End Sub
#End Region
#Region "掉落物"
    Private Sub LBCoins_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
        Try
            preselcoinid = CInt(LBCoins.SelectedItem.Text)
        Catch ex As Exception
        End Try
    End Sub
    Private Sub TBCoinX_PreviewKeyDown(sender As Object, e As KeyEventArgs)
        If DealKeyDown(sender, e) Then
            If Not IsNothing(coin) Then
                coin.X = TBCoinX.Text
            End If
        End If
    End Sub
    Private Sub TBCoinY_PreviewKeyDown(sender As Object, e As KeyEventArgs)
        If DealKeyDown(sender, e) Then
            If Not IsNothing(coin) Then
                coin.Y = TBCoinY.Text
            End If
        End If
    End Sub
    Private Sub TBCoinSize_PreviewKeyDown(sender As Object, e As KeyEventArgs)
        If DealKeyDown(sender, e) Then
            If Not IsNothing(coin) Then
                coin.Size = TBCoinSize.Text
            End If
        End If
    End Sub
    Private Sub CBCoinCard_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
        If Not IsNothing(coin) AndAlso CBCardNum.IsMouseOver AndAlso CBCardType.SelectedIndex <> 53 Then
            coin.CardType = CBCoinCard.SelectedIndex
        End If
    End Sub
    Private Sub CBCoinVisible_Click(sender As Object, e As RoutedEventArgs)
        If Not IsNothing(coin) Then
            coin.Visible = Not CBCoinVisible.IsChecked
        End If
    End Sub
    Private Sub CBCoinCollected_Click(sender As Object, e As RoutedEventArgs)
        If Not IsNothing(coin) Then
            coin.Collected = CBCoinCollected.IsChecked
        End If
    End Sub
    Private Sub CBCoinHalo_Click(sender As Object, e As RoutedEventArgs)
        If Not IsNothing(coin) Then
            coin.Halo = CBCoinHalo.IsChecked
        End If
    End Sub
#End Region
#Region "场地物品"
    Private Sub LBGriditems_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
        Try
            preselgriditemid = CInt(LBGriditems.SelectedItem.Text)
        Catch ex As Exception
        End Try
    End Sub
    Private Sub TBGriditemRow_PreviewKeyDown(sender As Object, e As KeyEventArgs)
        If DealKeyDown(sender, e) Then
            If Not IsNothing(griditem) Then
                griditem.Row = TBGriditemRow.Text
            End If
        End If
    End Sub
    Private Sub TBGriditemColumn_PreviewKeyDown(sender As Object, e As KeyEventArgs)
        If DealKeyDown(sender, e) Then
            If Not IsNothing(griditem) Then
                griditem.Column = TBGriditemColumn.Text
            End If
        End If
    End Sub
    Private Sub CBGriditemExist_Click(sender As Object, e As RoutedEventArgs)
        If Not IsNothing(griditem) Then
            griditem.Exist = CBGriditemExist.IsChecked
        End If
    End Sub
    Private Sub SCraterDisappear_ValueChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of Double))
        If SCraterDisappear.IsMouseOver And Not IsNothing(crater) Then
            crater.DisappearCountdown = SCraterDisappear.Value * 100
        End If
    End Sub
    Private Sub CBVaseSkin_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
        If Not IsNothing(vase) Then
            vase.Skin = CBVaseSkin.SelectedIndex + 3
        End If
    End Sub
    Private Sub CBVaseContent_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
        If Not IsNothing(vase) Then
            vase.Content = CBVaseContent.SelectedIndex
        End If
    End Sub
    Private Sub CBVaseZombie_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
        If Not IsNothing(vase) Then
            vase.Zombie = CBVaseZombie.SelectedIndex
        End If
    End Sub
    Private Sub CBVasePlant_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
        If Not IsNothing(vase) Then
            vase.Plant = CBVasePlant.SelectedIndex
        End If
    End Sub
    Private Sub TBVaseSun_PreviewKeyDown(sender As Object, e As KeyEventArgs)
        If DealKeyDown(sender, e) Then
            If Not IsNothing(vase) Then
                vase.Sun = TBVaseSun.Text
            End If
        End If
    End Sub
    Private Sub SVaseTransparent_ValueChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of Double))
        If SVaseTransparent.IsMouseOver And Not IsNothing(crater) Then
            vase.TransparentCountDown = SVaseTransparent.Value * 100
        End If
    End Sub
#End Region
#Region "卡槽"
    Private Sub CBCardSlotVisible_Click(sender As Object, e As RoutedEventArgs)
        PVZ.CardSlot.Visible = CBCardSlotVisible.IsChecked
    End Sub
    Private Sub LBCards_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
        Try
            preselcardid = CInt(LBCards.SelectedItem.Text)
        Catch ex As Exception
        End Try
    End Sub
    Private Sub CBCardNum_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
        If CBCardNum.IsMouseCaptured Then
            PVZ.CardSlot.SetCardNum(CBCardNum.SelectedIndex)
        End If
    End Sub
    Private Sub TBCardX_PreviewKeyDown(sender As Object, e As KeyEventArgs)
        If DealKeyDown(sender, e) Then
            If Not IsNothing(card) Then
                card.X = TBCardX.Text
            End If
        End If
    End Sub
    Private Sub TBCardY_PreviewKeyDown(sender As Object, e As KeyEventArgs)
        If DealKeyDown(sender, e) Then
            If Not IsNothing(card) Then
                card.Y = TBCardY.Text
            End If
        End If
    End Sub
    Private Sub CBCardVisible_Click(sender As Object, e As RoutedEventArgs)
        If Not IsNothing(card) Then
            card.Visible = CBCardVisible.IsChecked
        End If
    End Sub
    Private Sub CBCardEnable_Click(sender As Object, e As RoutedEventArgs)
        If Not IsNothing(card) Then
            card.Enable = CBCardEnable.IsChecked
        End If
    End Sub
    Private Sub CBCardActive_Click(sender As Object, e As RoutedEventArgs)
        If Not IsNothing(card) Then
            card.Active = CBCardActive.IsChecked
        End If
    End Sub
    Private Sub CBCardType_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
        If CBCardType.IsMouseOver AndAlso Not IsNothing(card) AndAlso CBCardType.SelectedIndex <> 53 Then
            card.CardType = CBCardType.SelectedIndex
        End If
    End Sub
    Private Sub CBCardTypeImitative_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
        If CBCardTypeImitative.IsMouseOver AndAlso Not IsNothing(card) AndAlso CBCardType.SelectedIndex <> 53 Then
            card.ImitativeCardType = CBCardTypeImitative.SelectedIndex
        End If
    End Sub
    Private Sub SCardCoolDowm_ValueChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of Double))
        If SCardCoolDowm.IsMouseOver And Not IsNothing(card) Then
            card.CoolDown = SCardCoolDowm.Value * 100
        End If
    End Sub
    Private Sub SCardBeltX_ValueChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of Double))
        If SCardBeltX.IsMouseOver And Not IsNothing(card) Then
            card.ConveyorBeltX = SCardBeltX.Value
        End If
    End Sub
    Private Sub TBCoolDown_PreviewKeyDown(sender As Object, e As KeyEventArgs)
        If DealKeyDown(sender, e) Then
            If Not IsNothing(card) Then
                card.CoolDownInterval = TBCoolDown.Text
            End If
        End If
    End Sub
#End Region
#Region "杂项，宝石迷阵"
    Private Sub CBUpgradedRepeater_Click(sender As Object, e As RoutedEventArgs)
        PVZ.Miscellaneous.UpgradedRepeater = CBUpgradedRepeater.IsChecked
    End Sub
    Private Sub CBUpgradedFumeshroon_Click(sender As Object, e As RoutedEventArgs)
        PVZ.Miscellaneous.UpgradedFumeshroon = CBUpgradedFumeshroon.IsChecked
    End Sub
    Private Sub CBUpgradedTallnut_Click(sender As Object, e As RoutedEventArgs)
        PVZ.Miscellaneous.UpgradedTallnut = CBUpgradedTallnut.IsChecked
    End Sub
    Private Sub SAttributeTime_ValueChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of Double))
        PVZ.Miscellaneous.AttributeCountdown = SAttributeTime.Value * 100
    End Sub
    Private Sub TBLevelProcess_PreviewKeyDown(sender As Object, e As KeyEventArgs)
        If DealKeyDown(sender, e) Then
            PVZ.Miscellaneous.LevelProcess = TBLevelProcess.Text
        End If
    End Sub
    Private Sub TBLevelRound_PreviewKeyDown(sender As Object, e As KeyEventArgs)
        If DealKeyDown(sender, e) Then
            PVZ.Miscellaneous.Round = TBLevelRound.Text
        End If
    End Sub
#End Region
#Region "背景音乐"
    Private Sub CBMusicType_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
        If CBMusicType.IsMouseCaptured Then
            PVZ.Music.Type = CBMusicType.SelectedIndex + 1
        End If
    End Sub
    Private Sub CBInGameEnanle_Click(sender As Object, e As RoutedEventArgs)
        PVZ.Music.INGAMEEnable = CBINGAMEEnable.IsChecked
    End Sub
    Private Sub CBInGameStart_Click(sender As Object, e As RoutedEventArgs)
        PVZ.Music.INGAMEStart = CBINGAMEStart.IsChecked
    End Sub
    Private Sub SMusicSpeed_ValueChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of Double))
        If SMusicSpeed.IsMouseOver Then
            PVZ.Bass_Dll.MusicSetAttribute(PVZ.Bass_Dll.HMUSIC1, PVZ.Bass_Dll.BASS_MUSIC_ATTRIB_SPEED, SMusicSpeed.Value)
        End If
    End Sub
    Private Sub SMusicVolumn_ValueChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of Double))
        If SMusicVolumn.IsMouseOver Then
            PVZ.Bass_Dll.MusicSetAttribute(PVZ.Bass_Dll.HMUSIC1, PVZ.Bass_Dll.BASS_MUSIC_ATTRIB_AMPLIFY, SMusicVolumn.Value)
        End If
    End Sub
    Private Sub SMusicBPM_ValueChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of Double))
        If SMusicBPM.IsMouseOver Then
            PVZ.Bass_Dll.MusicSetAttribute(PVZ.Bass_Dll.HMUSIC1, PVZ.Bass_Dll.BASS_MUSIC_ATTRIB_BPM, SMusicBPM.Value)
        End If
    End Sub
#End Region
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

    Private Sub CBSceneType_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
        If CBSceneType.IsMouseCaptured Then
            PVZ.Scene = CBSceneType.SelectedIndex
        End If
    End Sub
End Class
