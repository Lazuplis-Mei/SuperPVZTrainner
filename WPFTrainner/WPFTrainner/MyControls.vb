Imports System.Globalization
Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports System.Text.RegularExpressions
Imports System.Windows.Media.Animation
Imports ITrainerExtension
Imports PVZClass


Module ControlExtension
    <Extension>
    Public Function GetTextRect(con As Control, str As String) As Rect
        Dim typeface As Typeface = New Typeface(con.FontFamily, con.FontStyle, con.FontWeight, con.FontStretch)
        Dim formattedText As FormattedText = New FormattedText(
        str, CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, typeface, con.FontSize, con.Foreground, 1)
        Return New Rect(0, 0, formattedText.Width, formattedText.Height)
    End Function
    <Extension>
    Public Function GetTextRect(tb As TextBlock) As Rect
        Dim typeface As Typeface = New Typeface(tb.FontFamily, tb.FontStyle, tb.FontWeight, tb.FontStretch)
        Dim formattedText As FormattedText = New FormattedText(
        tb.Text, CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, typeface, tb.FontSize, tb.Foreground, 1)
        Return New Rect(0, 0, formattedText.Width, formattedText.Height)
    End Function
End Module


Public Class MyCheckBox
    Inherits DarkStyle.DarkCheckBox
    Dim isTipSet As Boolean = False
    Public Sub New()
        AddHandler Click, AddressOf MyCheckBox_Click
        AddHandler Loaded, AddressOf MyCheckBox_Load
    End Sub
    Private Sub MyCheckBox_Click(sender As Object, e As RoutedEventArgs)
        If IsChecked.HasValue Then
            Foreground = Brushes.White
        Else
            Foreground = Brushes.Red
        End If
        If Not IsNothing(Tag) Then
            GetType(PVZ).GetMethod(Tag)?.Invoke(Nothing, New Object() {IsChecked})
        End If
    End Sub
    Dim CBoxText1 As String() = {"变红代表取消或变化原有的效果", "Reddening means canceling or changing the original effect"}
    Dim CBoxText2 As String() = {"附带右键菜单额外功能", "Additional functions of right-click menu"}
    Private Sub MyCheckBox_Load(sender As Object, e As RoutedEventArgs)
        If isTipSet Then Exit Sub
        Dim extra As Boolean = Not IsNothing(Content) AndAlso Content.ToString().Contains("*")
        If IsNothing(ToolTip) Then
            Dim tip = New MyToolTip()
            If IsThreeState And Not extra Then
                tip.Content = CBoxText1(Lang.Id)
                tip.Resources.Add("Lang", CBoxText1)
            ElseIf extra And Not IsThreeState Then
                tip.Content = CBoxText2(Lang.Id)
                tip.Resources.Add("Lang", CBoxText2)
            ElseIf IsThreeState And extra Then
                tip.Content = CBoxText1(Lang.Id) + "," + CBoxText2(Lang.Id)
                Dim res = {CBoxText1(0) + "," + CBoxText2(0), CBoxText1(1) + "," + CBoxText2(1)}
                tip.Resources.Add("Lang", res)
            Else
                Exit Sub
            End If
            ToolTip = tip
            isTipSet = True
        End If
    End Sub
End Class

Public Class MySlider
    Inherits Slider
    Public Sub New()
        Maximum = 20
        AutoToolTipPlacement = Primitives.AutoToolTipPlacement.BottomRight
        Width = 80
    End Sub
End Class

Public Class MyComboBox
    Inherits DarkStyle.DarkComboBox
    Public Sub New()
        Width = 75
        AddHandler Loaded, AddressOf MyComboBox_Load
    End Sub
    Private Sub MyComboBox_Load(sender As Object, e As RoutedEventArgs)
        If Not IsNothing(Tag) AndAlso Items.Count = 0 Then
            Dim tEnum = Tag.ToString().Split({"."c}, StringSplitOptions.RemoveEmptyEntries)
            Dim enumType As Type
            If tEnum.Length = 1 Then
                enumType = GetType(PVZ).GetMember(tEnum(0))(0)
            Else
                enumType = GetType(PVZ).GetNestedType(tEnum(0)).GetMember(tEnum(1))(0)
            End If
            For Each value As [Enum] In [Enum].GetValues(enumType)
                Dim item = New DarkStyle.DarkComboBoxItem()
                Dim res As String() = {value.GetDescription(), value.ToString()}
                item.Resources.Add("Lang", res)
                item.Content = item.Resources("Lang")(Lang.Id)
                Items.Add(item)
            Next
        End If
    End Sub
End Class

Public Class NegateConverter
    Implements IValueConverter
    Public Function Convert(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.Convert
        If GetType(Boolean) = value.GetType() Then
            Return Not value
        End If
        Return value
    End Function
    Public Function ConvertBack(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.ConvertBack
        If GetType(Boolean) = value.GetType() Then
            Return Not value
        End If
        Return value
    End Function
End Class

Public Class IndexValueConverter
    Implements IValueConverter
    Property IndexValue As Integer

    Public Function Convert(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.Convert
        If GetType(Integer) = value.GetType() Then
            Return IndexValue = value
        End If
        Return True
    End Function
    Public Function ConvertBack(value As Object, targetType As Type, parameter As Object, culture As CultureInfo) As Object Implements IValueConverter.ConvertBack
        If GetType(Integer) = value.GetType() Then
            Return IndexValue = value
        End If
        Return True
    End Function
End Class

Public Class MyToolTip
    Inherits ToolTip
    Public Sub New()
        Foreground = Brushes.White
        Background = New SolidColorBrush(Color.FromArgb(&HFF, &H25, &H25, &H26))
        BorderBrush = Brushes.Crimson
        AddHandler Loaded, AddressOf MyToolTip_Loaded
    End Sub
    Sub MyToolTip_Loaded(sender As Object, e As RoutedEventArgs)
        Dim win = Window.GetWindow(Me)
        Dim scaleinfo = RuntimeReflectionExtensions.GetRuntimeField(win.GetType(), "scale")
        Dim openAnim = New DoubleAnimation(0, 1, New Duration(TimeSpan.FromMilliseconds(300)))
        If Not IsNothing(scaleinfo) Then
            Dim scale = CInt(scaleinfo.GetValue(win))
            If scale >= 200 Then
                FontSize = 22
                BorderThickness = New Thickness(3)
            ElseIf scale >= 150 Then
                FontSize = 18
                BorderThickness = New Thickness(2)
            Else
                FontSize = 12
                BorderThickness = New Thickness(1)
            End If
        End If
        Dim rect = GetTextRect(Content.ToString())
        RenderTransform = New ScaleTransform(0, 0, rect.Width / 2, rect.Height / 2)
        RenderTransform.BeginAnimation(ScaleTransform.ScaleXProperty, openAnim)
        RenderTransform.BeginAnimation(ScaleTransform.ScaleYProperty, openAnim)
    End Sub
End Class