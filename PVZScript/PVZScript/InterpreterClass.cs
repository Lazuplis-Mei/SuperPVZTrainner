using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PVZScript
{
    class Interpreter
    {
        static int? ToInt(string v)
        {
            if (v == "true" || v == "True")
                return 1;
            else if (v == "false" || v == "False")
                return 0;
            else if (v.StartsWith("0x") || v.StartsWith("0X"))
                return Convert.ToInt32(v.Substring(2), 16);
            else if (v == "null")
                return null;
            else return Convert.ToInt32(v);
        }


        public string Dealing;

        public Interpreter(string command)
        {
            int i = command.IndexOf("//");
            if (i>=0)
            {
                Dealing = command.Substring(0,i);
            }
            else
            {
                Dealing = command;
            }
            
        }

        public struct NV
        {
            public readonly string Name;
            public readonly int? Value;
            public readonly bool Valid;
            public NV(string name, string value)
            {
                Name = name;
                int.Parse("0");
                
                Value = ToInt(value);
                Valid = true;
            }
        }

        public struct FA
        {
            public readonly string Name;
            public readonly object[] args;
            public readonly bool Valid;
            public FA(string name,string[] args)
            {
                Name = name;
                if(!(args is null))
                {
                    var temp = new List<object>();
                    for (int i = 0; i < args.Length; i++)
                    {
                        try
                        {
                            temp.Add(ToInt(args[i]));
                        }
                        catch (FormatException)
                        {

                            temp.Add(0);
                        }
                    }
                    this.args = temp.ToArray();
                }
                else
                {
                    this.args = null;
                }
                Valid = true;
            }
        }

        public struct WC
        {
            public readonly Clazz clazz;
            public readonly bool Valid;
            public WC(string name, string arg)
            {
                if(arg is null)
                {
                    clazz = new Clazz(Program.pvzClass, "PVZClass.PVZ+" + name);
                }
                else
                {
                    clazz = new Clazz(Program.pvzClass, "PVZClass.PVZ+" + name).Create(new object[] { ToInt(arg) });
                }
                Valid = true;
            }
        }

        public struct GC
        {
            public readonly object value;
            public readonly bool Valid;
            public GC(object name)
            {
                value = name;
                Valid = true;
            }
        }

        public struct WGC
        {
            public readonly Clazz clazz;
            public readonly bool Valid;
            public WGC(object name)
            {
                clazz = new Clazz(name);
                Valid = true;
            }
        }

        public struct LF
        {
            public readonly object[] objs;
            public readonly bool Valid;
            public LF(object[] name)
            {
                objs = name;
                Valid = true;
            }
        }

        

        private string GSExFormat()
        {
            string process;
            process = Dealing.Replace(" ", "");
            process = process.Substring(0, process.Length - 1);
            return process.Substring(3);
        }

        public NV SetEx()
        {
           if(Dealing.EndsWith(";") && Dealing.StartsWith("Set"))
            {
                string[] Substr = GSExFormat().Split('=');
                if(Substr.Length==2)
                {
                    return new NV(Substr[0], Substr[1]);
                }
            }
            return new NV();
        }

        public string GetEx()
        {
            if (Dealing.EndsWith(";") && Dealing.StartsWith("Get"))
            {
                return GSExFormat();
            }
            return null;
        }

        public bool IsBlank()
        {
            return Dealing.Replace(" ", "") == "";
        }

        public bool IsListProp()
        {
            return Dealing == "-Propertys" || Dealing == "-Props";
        }

        public bool IsListMethod()
        {
            return Dealing == "-Functions" || Dealing == "-Funcs";
        }

        public FA CallEx()
        {
            if (Dealing.EndsWith(";"))
            {
                var process = Dealing.Substring(0, Dealing.Length - 1);
                if (process.StartsWith("Call"))
                {
                    process = process.Replace(" ", "");
                    process = process.Substring(4);
                    process = process.Substring(0, process.Length - 1);
                    int i = process.IndexOf("(");
                    if (i >= 0)
                    {
                        var fn = process.Substring(0, i);
                        process = process.Substring(i + 1);
                        var substr = process.Split(',');
                        if (substr.Length > 1)
                        {
                            var temp = new List<string>();
                            foreach (var str in substr)
                            {
                                temp.Add(str);
                            }
                            return new FA(fn, temp.ToArray());
                        }
                        else
                        {
                            if (substr[0] == "")
                            {
                                return new FA(fn, null);
                            }
                            return new FA(fn, substr);
                        }
                    }
                }
            }
            return new FA();
        }

        public GC GetCallEx(Clazz target)
        {
            if (Dealing.EndsWith(";") && Dealing.StartsWith("Get Call"))
            {
                string process = Dealing.Substring(4);
                var script = new Interpreter(process);
                var fa = script.CallEx();
                if (fa.Valid)
                {
                    return new GC(target.Invokefunction(fa.Name, fa.args));
                }
            }
            return new GC();
        }

        public WC WithEx()
        {
            string process = Dealing;
            if (process.StartsWith("With"))
            {
                process = Dealing.Replace(" ", "");
                process= process.Substring(4);
                int i = process.IndexOf("(");
                if (i >= 0)
                {
                    process = process.Substring(0, process.Length - 1);
                    var cn = process.Substring(0, i);
                    var arg = process.Substring(i + 1, process.Length - i-1);
                    return new WC(cn, arg);
                }
                else
                {
                    return new WC(process, null);
                }
            }
            return new WC();
        }

        public bool IsEWithEx()
        {
            return Dealing == "EndWith";
        }

        public WGC WithGetEx(Clazz target)
        {
            if (Dealing.EndsWith(";") && Dealing.StartsWith("With Get"))
            {
                string process = Dealing.Substring(8);
                process = process.Substring(0, process.Length - 1);
                process = process.Replace(" ", "");
                return new WGC(target.Getvalue(process));
            }
            return new WGC();
        }

        public WGC WithCallEx(Clazz target)
        {
            if(Dealing.EndsWith(";") && Dealing.StartsWith("With Call"))
            {
                string process = Dealing.Substring(5);
                var script = new Interpreter(process);
                var fa = script.CallEx();
                if(fa.Valid)
                {
                    return new WGC(target.Invokefunction(fa.Name, fa.args));
                }
            }
            return new WGC();
        }

        public bool ExitEx()
        {
            return Dealing == "EndScript" || Dealing == "End";
        }

        public LF IsFor(Clazz target)
        {
            if(Dealing.StartsWith("For"))
            {
                string process = Dealing.Substring(4, Dealing.Length - 4);
                return new LF((object[])target.Getvalue(process));
            }
            return new LF();
        }
    }
}
