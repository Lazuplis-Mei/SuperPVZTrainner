using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace PVZScript
{
    class Method
    {
        readonly MethodInfo Info;

        public Method(MethodInfo method)
        {
            Info = method;
            if(Info==null)
            {
                throw new Exception("访问错误");
            }
        }

        public string[] GetArgType(bool includeName = false)
        {
            List<string> temp = new List<string>();
            foreach (ParameterInfo parameter in Info.GetParameters())
            {
                temp.Add(GetTypeName( parameter.ParameterType));
                if (includeName)
                    temp[temp.Count - 1] += " " + parameter.Name;
            }
            return temp.ToArray();

        }


        public string GetTypeName(Type type)
        {
            if (type == typeof(int))
            {
                return "int";
            }
            else if (type == typeof(short))
            {
                return "short";
            }
            else if (type == typeof(byte))
            {
                return "byte";
            }
            else if (type == typeof(string))
            {
                return "string";
            }
            else if (type == typeof(bool))
            {
                return "bool";
            }
            else if (type == typeof(float))
            {
                return "float";
            }
            else if (type == typeof(double))
            {
                return "double";
            }
            else if (type == typeof(void))
            {
                return "void";
            }
            else if (type.BaseType == typeof(Array))
            {
                return type.Name;
            }
            else if (type.BaseType == typeof(Enum))
            {
                return type.Name + "(enum)";
            }
            else if (type.IsGenericType)
            {
                if (type.Name.StartsWith("Nullable"))
                    return GetTypeName(type.GenericTypeArguments[0]) + '?';
                return type.Name.Substring(0, type.Name.Length - 2) + '<' +
                    GetTypeName(type.GenericTypeArguments[0]) + '>';
            }
            else
            {
                return type.Name + "(object)";
            }
        }



        public string GetRetuenType()
        {
            return GetTypeName(Info.ReturnType);
        }

        public object[] FixArgs(object[] args)
        {
            if(!(args is null))
            {
                var argtypes = new Method(Info).GetArgType();
                for (int i = 0; i < args.Length; i++)
                {
                    switch (argtypes[i])
                    {
                        case "short":
                            args[i] = Convert.ToInt16(args[i]);
                            break;
                        case "byte":
                            args[i] = Convert.ToByte(args[i]);
                            break;
                        case "bool":
                            args[i] = Convert.ToBoolean(args[i]);
                            break;
                        case "bool?":
                            if (args[i] != null)
                                args[i] = (bool?)(Convert.ToBoolean(args[i]));
                            break;
                        case "float":
                            args[i] = Convert.ToSingle(args[i]);
                            break;
                        case "double":
                            args[i] = Convert.ToDouble(args[i]);
                            break;
                    }
                }
            }
            return args;
        }

    }
    class Clazz
    {
        public readonly Type Instance;

        private readonly bool isobj;

        readonly object curobj;

        public Clazz(Assembly assembly, string classname)
        {
           isobj = false;
           Instance = assembly.GetType(classname);
           if (Instance is null)
           {
                throw new ArgumentException("\"" + classname + "\"类未定义");
           }
        }

        public Clazz Create(params object[] obj)
        {
            return new Clazz(Activator.CreateInstance(Instance, obj));
        }

        public Clazz(object obj)
        {
            isobj = true;
            curobj = obj;
            Instance = obj.GetType();
        }

        public object Invokefunction(string functionname, object[] args)
        {
            var function = Instance.GetMethod(functionname);
            if(function==null)
            {
                throw new Exception("\"" + functionname + "\"方法未定义");
            }
            args = new Method(function).FixArgs(args);
            if (!isobj)
            {
                return function.Invoke(Instance, args);
            }
            else
            {
                return function.Invoke(curobj, args);
            }
        }

        public object Getvalue(string propertyname)
        {
            var property = Instance.GetProperty(propertyname);
            if (property == null)
            {
                var field = Instance.GetField(propertyname);
                if (field == null)
                    throw new Exception("\"" + propertyname + "\"属性未定义");
                if (!isobj)
                {
                    return field.GetValue(Instance);
                }
                else
                {
                    return field.GetValue(curobj);
                }
            }
            if (!isobj)
            {

                return property.GetGetMethod().Invoke(Instance, null);
            }
            else
            {
                return property.GetGetMethod().Invoke(curobj, null);
            }
        }

        public void Setvalue(string propertyname, object value)
        {
            var property = Instance.GetProperty(propertyname);
            if (property == null)
            {
                var field  = Instance.GetField(propertyname);
                if (field == null)
                    throw new Exception("\"" + propertyname + "\"属性未定义");
                if (!isobj)
                {
                    field.SetValue(Instance, value);
                }
                else
                {
                    field.SetValue(curobj, value);
                }
                return;
            }
            var function = property.GetSetMethod();
            object[] args = { value };
            args = new Method(function).FixArgs(args);
            if (!isobj)
            {
                function.Invoke(Instance, args);
            }
            else
            {
                function.Invoke(curobj, args);
            }
        }

        public string[] Propertys {
            get
            {
                List<string> vs = new List<string>();
                foreach (PropertyInfo property in Instance.GetProperties())
                {
                    string returnvaluename;
                    try
                    {
                        returnvaluename = new Method(property.GetGetMethod()).GetRetuenType().ToString();
                    }
                    catch (Exception ex)
                    {
                        if (ex.Message== "访问错误")
                        {
                            returnvaluename = new Method(property.GetSetMethod()).GetArgType()[0].ToString();
                        }
                        else
                        {
                            returnvaluename = ex.Message;
                        }
                    }
                    try
                    {
                        vs.Add(property.Name + '(' + returnvaluename + ')');
                    }
                    catch (ArgumentException)
                    {
                        //忽略这个异常
                    }
                }
                return vs.ToArray();
            }
        }

        public string[] Methods
        {
            get
            {
                List<string> vs = new List<string>();
                foreach (MethodInfo method in Instance.GetMethods())
                {
                    if(method.Name.StartsWith("set_")|| method.Name.StartsWith("get_"))
                    {
                        continue;
                    }
                    var temp = new Method(method);
                    StringBuilder arglist = new StringBuilder();
                    foreach (string arg in temp.GetArgType(true))
                    {
                        arglist.Append(arg);
                        arglist.Append(", ");
                    }
                    if (arglist.Length > 0) arglist.Remove(arglist.Length - 2, 2);
                    vs.Add(temp.GetRetuenType() + " " + method.Name + "(" + arglist.ToString() + ")");
                }
                return vs.ToArray();
            }
        }

    }
}
