
Imports System.Globalization

Public Class ExpendWindow
    Private Sub Window_MouseMove(sender As Object, e As MouseEventArgs)
        If e.GetPosition(Me).Y < 35 Then
            If e.LeftButton = MouseButtonState.Pressed Then
                DragMove()
            End If
        End If
    End Sub
    Private Sub Window_Loaded(sender As Object, e As RoutedEventArgs)
        TBEgg.Text =
"---------[>--------<+]>---.<------[>------<+]>.
++.++++++++.----.-.+.<--------[>>----<<+]>>.>--
---[<-------->+]<---.<------.++++++.<-----[>--<
+]>.<-----[>++<+]>+.-.>>------[<+++++>+]<+.>---
---[<----->+]<+.>--------[<+++++>+]<+.<---.---.
-------.<-----[>+++<+]>++.>.<<----[>-----<+]>.<
--[>+++++<+]>.------.>-...>------[<++++>+]<-."
    End Sub

    Private Sub Window_Closed(sender As Object, e As EventArgs)
        If Not IsNothing(Tag) Then
            CType(Tag, ListBoxItem).IsEnabled = True
        End If
    End Sub

    <Obsolete>
    Private Sub Window_SizeChanged(sender As Object, e As SizeChangedEventArgs)
        TBSep.Text = ""
        While GetTextDisplayWidthHelper.GetTextDisplayWidth(TBSep) + Canvas.GetLeft(TBSep) < e.NewSize.Width - 15
            TBSep.Text += "-"
        End While
    End Sub

End Class

Class GetTextDisplayWidthHelper
    <Obsolete>
    Public Shared Function GetTextDisplayWidth(textblock As TextBlock) As Double

        Return GetTextDisplayWidth(textblock.Text, textblock.FontFamily, textblock.FontStyle, textblock.FontWeight, textblock.FontStretch, textblock.FontSize)
    End Function

    <Obsolete>
    Public Shared Function GetTextDisplayWidth(str As String, fontFamily As FontFamily, fontStyle As FontStyle, fontWeight As FontWeight, fontStretch As FontStretch, FontSize As Double) As Double

        Dim formattedText = New FormattedText(
                                str,
                                CultureInfo.CurrentUICulture,
                                FlowDirection.LeftToRight,
                                New Typeface(fontFamily, fontStyle, fontWeight, fontStretch),
                                FontSize,
                                Brushes.Black
                                )
        Dim s = New Size(formattedText.Width, formattedText.Height)
        Return s.Width
    End Function
End Class