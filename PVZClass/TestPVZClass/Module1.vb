
Imports PVZClass
Imports System.Threading
Module Module1
    Public Sub Main()
        If PVZ.RunGame() Then
            PVZ.BGRunable()
            PVZ.Plantf(PVZ.PlantType.Blover, 0, 5)
            PVZ.CloseGame()
        End If
    End Sub
    Public Function Randint(ByVal v1, Optional ByVal v2 = Nothing) As Integer
        Randomize()
        If IsNothing(v2) Then
            Return Int((v1 + 1) * Rnd())
        Else
            If v1 < v2 Then
                Return Int((v1 - v2 - 1) * Rnd() + v2 + 1)
            ElseIf v1 = v2 Then
                Return v1
            Else
                Return 0
            End If
        End If
    End Function
End Module
#If 0 Then
        If PVZ.RunGame() Then
                PVZ.InitFunctions()
                PVZ.BGRunable() '后台运行

                If PVZ.MainObjectExist Then
                    Dim Delay As Integer = 0
                    '子弹事件循环实现碰壁反弹
                    PVZ.Projectile.EventLoop.Initialize({&H8B, 6, &H83, &H78, &H5C, 0, &H75, &H41, &H81, &H78, &H30,
0, 0, &H3E, &H44, &H7F, &H19, &H83, &H78, &H30, 0, &H7C, &H13, &H81, &H78, &H34, 0, 0, 7, &H44, &H7F, &H1A, &H81,
&H78, &H34, 0, 0, &H70, &H42, &H7C, &H11, &HC3, &HD9, &H40, &H3C, &HD9, &HE0, &HD9, &H58, &H3C, &HC7, &H40, &H5C,
1, 0, 0, 0, &HC3, &HD9, &H40, &H40, &HD9, &HE0, &HD9, &H58, &H40, &HC7, &H40, &H5C, 1, 0, 0, 0, &HC3})
                    PVZ.Projectile.EventLoop.Start()
                    While True
                        If Delay Mod 20 = 0 Then
                            Dim x = Randint(100, 400)
                            Dim y = Randint(50, 300)
                            For i = 0 To 40
                                PVZ.CreateProjectile(PVZ.ProjectiletType.NormalPea, x, y, i * 9, 1)
                            Next
                        End If
                        Delay += 1
                        For Each i In PVZ.AllProjectiles
                            If PVZ.Mouse.X > i.X - 8 And PVZ.Mouse.X < i.X + 8 And PVZ.Mouse.Y > i.Y - 8 And PVZ.Mouse.Y < i.Y + 8 Then
                                PVZ.LevelState = PVZ.GameState.Losing
                                Exit While
                            End If
                        Next
                        Thread.Sleep(100)
                    End While
                    PVZ.Projectile.EventLoop.Stop­()
                End If
                PVZ.CloseGame()
                Console.WriteLine("You Lose")
                Thread.Sleep(1000)
            End If
#End If
