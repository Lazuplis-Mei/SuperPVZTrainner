using System;
using System.Text;
using PVZScript.Properties;
using System.Reflection;
using System.IO;
using PVZClass;
using System.Diagnostics;

namespace PVZScript
{
    class Program
    {
        public static Assembly pvzClass = typeof(PVZ).Assembly;
        static Clazz pvz = new Clazz(pvzClass, "PVZClass.PVZ");
        static void Main(string[] args)
        {
            Clazz target = pvz;
            bool isfile = false;
            StreamReader sr = null;
            if (args.Length >= 1 && args[0].ToUpper().EndsWith(".PVZS"))
            {
                isfile = true;
                sr = new StreamReader(args[0], Encoding.Default);
                if (args.Length >= 2)
                    PVZ.GameName = args[1];
                if (args.Length >= 3)
                    PVZ.GameTitle = args[2];
            }
            if (PVZ.RunGame())
            {
                Console.WriteLine("PVZScript [版本 2.0.0.5]");
                Console.WriteLine("With PVZClass By 冥谷川恋(Meiyagawa Koishi) [版本 1.0.4.9]");
                Console.WriteLine("StartScript");
                while (true)
                {
                    Interpreter script;
                    if(isfile)
                    {
                        script = new Interpreter(sr.ReadLine());
                    }
                    else
                    {
                        Console.Write(target.Instance.Name + ">>");
                        script = new Interpreter(Console.ReadLine());
                    }
                    var lf = script.IsFor(target);
                    if (lf.Valid)
                    {
                        while (true)
                        {
                            var temp = new Clazz(lf.objs);
                            if (isfile)
                            {
                                script = new Interpreter(sr.ReadLine());
                            }
                            else
                            {
                                Console.Write(temp.Instance.Name + ">>");
                                script = new Interpreter(Console.ReadLine());
                            }
                            if(script.Dealing=="EndFor")
                            {
                                target = pvz;
                                break;
                            }
                            foreach (var item in lf.objs)
                            {
                                target = new Clazz(item);
                                if (!Mainloop(script, ref target, isfile))
                                {
                                    break;
                                }
                            }
                        }
                    }
                    else if (!Mainloop(script,ref target, isfile))
                    {
                        break;
                    }
                }
                PVZ.CloseGame();
            }
        }
        static bool Mainloop(Interpreter script,ref Clazz target,bool op)
        {

            //结束
            if (script.ExitEx()) return false;
            //文件指令
            if (op && script.Dealing == "Stop")
            {
                Console.ReadLine();
                return true;
            }

            //继续
            if (script.IsBlank()) return true;

            //赋值表达式
            var nv = script.SetEx();
            if (nv.Valid)
            {
                try
                {
                    target.Setvalue(nv.Name, nv.Value);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                return true;
            }

            //取函数返回值表达式
            try
            {
                var gc = script.GetCallEx(target);
                if (gc.Valid)
                {
                    Console.WriteLine(gc.value.ToString());
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return true;
            }

            //取值表达式
            var n = script.GetEx();
            if (!(n == null))
            {
                try
                {
                    Console.WriteLine(target.Getvalue(n));
                }
                catch (NullReferenceException)
                {
                    Console.WriteLine(script.Dealing + "属性无法访问");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

                return true;
            }

            //列举属性表达式
            if (script.IsListProp())
            {
                foreach (string name in target.Propertys)
                {
                    Console.WriteLine(name);
                }
                return true;
            }

            //列举方法表达式
            if (script.IsListMethod())
            {
                foreach (string name in target.Methods)
                {
                    Console.WriteLine(name);
                }
                return true;
            }

            //调用函数表达式
            var fa = script.CallEx();
            if (fa.Valid)
            {
                try
                {
                    target.Invokefunction(fa.Name, fa.args);
                }
                catch (Exception ex)
                {

                    Console.WriteLine(ex.Message);
                }
                return true;
            }

            //访问属性类表达式
            try
            {
                var wgc = script.WithGetEx(target);
                if (wgc.Valid)
                {
                    target = wgc.clazz;
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return true;
            }

            //访问方法类表达式
            try
            {
                var wgc = script.WithCallEx(target);
                if (wgc.Valid)
                {
                    target = wgc.clazz;
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return true;
            }

            //访问类表达式
            try
            {
                var wc = script.WithEx();
                if (wc.Valid)
                {
                    target = wc.clazz;
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return true;
            }

            //回到主类
            if (script.IsEWithEx())
            {
                target = pvz;
                return true;
            }

            //以上表达式均不满足时，直接将值作为属性取得
            try
            {
                Console.WriteLine(target.Getvalue(script.Dealing));
            }
            catch (NullReferenceException)
            {
                Console.WriteLine(script.Dealing + "属性无法访问");
            }
            catch (Exception ex)
            {
                Console.WriteLine("表达式错误");
                Console.WriteLine(ex.Message);
            }
            return true;
        }
    }
}
