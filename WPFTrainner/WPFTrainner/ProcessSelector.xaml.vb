Imports System.ComponentModel
Imports System.Data
Imports System.Diagnostics
Imports ITrainerExtension

Public Class ProcessSelector
    Public Property ProcessId As Integer
    Public Property ProcessName As String
    Public Property WindowName As String

    Private Sub Window_MouseDown(sender As Object, e As MouseButtonEventArgs)
        Try
            DragMove()
        Catch ex As InvalidOperationException
        End Try
    End Sub
    ReadOnly dt As DataTable = New DataTable()
    Private Sub Window_Loaded(sender As Object, e As RoutedEventArgs)
        dt.Columns.Add("ProcessId")
        dt.Columns.Add("ProcessName")
        dt.Columns.Add("WindowName")
        For Each pro In Process.GetProcesses()
            dt.Rows.Add(pro.Id, pro.ProcessName, pro.MainWindowTitle)
        Next
        LVMain.DataContext = dt
        If Lang.Id = 1 Then
            TBTitle.Text = "ProcessSelector(Click the header to sort)"
            GVCPID.Header = "PID"
            GVCName.Header = "Name"
            GVCTitle.Header = "Window Title"
            BtnSelect.Content = "Select"
            BtnRefresh.Content = "Refresh"
            BtnCancel.Content = "Cancel"
        Else
            TBTitle.Text = "进程选择窗口(点击表头可排序)"
            GVCPID.Header = "进程ID"
            GVCName.Header = "进程名"
            GVCTitle.Header = "窗口标题"
            BtnSelect.Content = "选择"
            BtnRefresh.Content = "刷新"
            BtnCancel.Content = "取消"
        End If
    End Sub
    Private Sub ListViewSort(lv As ListView, sortBy As String, direction As ListSortDirection)
        Dim dataView As ICollectionView = CollectionViewSource.GetDefaultView(lv.ItemsSource) '获取数据源视图
        dataView.SortDescriptions.Clear() '清空默认排序描述
        Dim sd As SortDescription = New SortDescription(sortBy, direction)
        dataView.SortDescriptions.Add(sd) '加入新的排序描述
        dataView.Refresh() '刷新视图
    End Sub
    Private Sub LVMain_Click(sender As Object, e As RoutedEventArgs)
        Dim gch As GridViewColumnHeader = e.OriginalSource
        Dim sort As ListSortDirection
        If gch.Tag Then
            gch.Tag = False
            sort = ListSortDirection.Ascending
        Else
            gch.Tag = True
            sort = ListSortDirection.Descending
        End If
        If gch.Content = "进程ID" OrElse gch.Content = "PID" Then
            ListViewSort(LVMain, "ProcessId", sort)
        ElseIf gch.Content = "进程名" OrElse gch.Content = "Name" Then
            ListViewSort(LVMain, "ProcessName", sort)
        ElseIf gch.Content = "窗口标题" OrElse gch.Content = "Window Title" Then
            ListViewSort(LVMain, "WindowName", sort)
        End If

    End Sub
    Private Sub BtnSelect_Click(sender As Object, e As RoutedEventArgs)
        If LVMain.SelectedIndex >= 0 Then
            DialogResult = True
            ProcessId = Integer.Parse(LVMain.SelectedItem.Row(0))
            ProcessName = LVMain.SelectedItem.Row(1)
            WindowName = LVMain.SelectedItem.Row(2)
        End If
    End Sub
    Private Sub BtnRefresh_Click(sender As Object, e As RoutedEventArgs)
        dt.Rows.Clear()
        For Each pro In Process.GetProcesses()
            dt.Rows.Add(pro.Id, pro.ProcessName, pro.MainWindowTitle)
        Next
        LVMain.DataContext = dt
    End Sub
    Private Sub BtnCancel_Click(sender As Object, e As RoutedEventArgs)
        DialogResult = False
        Close()
    End Sub
    Private Sub LVMain_MouseDoubleClick(sender As Object, e As MouseButtonEventArgs)
        If LVMain.SelectedIndex >= 0 Then
            DialogResult = True
            ProcessId = Integer.Parse(LVMain.SelectedItem.Row(0))
            ProcessName = LVMain.SelectedItem.Row(1)
            WindowName = LVMain.SelectedItem.Row(2)
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
            Height = 500 * scale / 100
            Width = 600 * scale / 100
        End If
    End Sub
End Class
