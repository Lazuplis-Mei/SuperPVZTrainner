using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PVZTools
{
    /// <summary>
    /// 全局键盘钩子
    /// </summary>
    class GlobalKeyboardHook
    {
        public event KeyEventHandler KeyDownEvent;
        public event KeyPressEventHandler KeyPressEvent;
        public event KeyEventHandler KeyUpEvent;

        public delegate int HookProc(int nCode, Int32 wParam, IntPtr lParam);
        static int hKeyboardHook = 0;

        public const int WH_KEYBOARD_LL = 13;
        HookProc keyboardHookProcedure;
        /// <summary>
        /// 键盘结构
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public class KeyboardHookStruct
        {
            /// <summary>
            /// 定一个虚拟键码。该代码必须有一个价值的范围1至254
            /// </summary>
            public int vkCode;
            /// <summary>
            /// 指定的硬件扫描码的关键
            /// </summary>
            public int scanCode;
            /// <summary>
            /// 键标志
            /// </summary>
            public int flags;
            /// <summary>
            /// 指定的时间戳记的这个讯息
            /// </summary>
            public int time;
            /// <summary>
            /// 指定额外信息相关的信息
            /// </summary>
            public int dwExtraInfo;
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern int SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hInstance, int threadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern bool UnhookWindowsHookEx(int idHook);

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern int CallNextHookEx(int idHook, int nCode, Int32 wParam, IntPtr lParam);

        [DllImport("kernel32.dll")]
        static extern int GetCurrentThreadId();

        [DllImport("kernel32.dll")]
        public static extern IntPtr GetModuleHandle(string name);

        /// <summary>
        /// 安装键盘钩子
        /// </summary>
        public void Install()
        {
            if(hKeyboardHook == 0)
            {
                keyboardHookProcedure = new HookProc(KeyboardHookProc);
                hKeyboardHook = SetWindowsHookEx(WH_KEYBOARD_LL, keyboardHookProcedure, GetModuleHandle(System.Diagnostics.Process.GetCurrentProcess().MainModule.ModuleName), 0);
                if(hKeyboardHook == 0)
                {
                    UnInstall();
                    throw new Exception("安装键盘钩子失败");
                }
            }
        }
        public void UnInstall()
        {
            bool retKeyboard = true;


            if(hKeyboardHook != 0)
            {
                retKeyboard = UnhookWindowsHookEx(hKeyboardHook);
                hKeyboardHook = 0;
            }

            if(!(retKeyboard))
                throw new Exception("卸载钩子失败！");
        }
        /// <summary>
        /// 转换指定的虚拟键码和键盘状态的相应字符或字符
        /// </summary>
        /// <param name="uVirtKey">指定虚拟关键代码进行翻译</param>
        /// <param name="uScanCode">指定的硬件扫描码的关键须翻译成英文。高阶位的这个值设定的关键，如果是（不压）</param>
        /// <param name="lpbKeyState">指针，以256字节数组，包含当前键盘的状态。
        /// 每个元素（字节）的数组包含状态的一个关键。如果高阶位的字节是一套，关键是下跌（按下）。
        /// 在低比特，如果设置表明，关键是对切换。在此功能，只有肘位的CAPS LOCK键是相关的。
        /// 在切换状态的NUM个锁和滚动锁定键被忽略。</param>
        /// <param name="lpwTransKey">指针的缓冲区收到翻译字符或字符</param>
        /// <param name="fuState">Specifies whether a menu is active. This parameter must be 1 if a menu is active, or 0 otherwise.</param>
        /// <returns></returns>
        [DllImport("user32")]
        public static extern int ToAscii(int uVirtKey, int uScanCode, byte[] lpbKeyState, byte[] lpwTransKey, int fuState);

        [DllImport("user32")]
        public static extern int GetKeyboardState(byte[] pbKeyState);


        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        private static extern short GetKeyState(int vKey);

        private const int WM_KEYDOWN = 0x100;
        private const int WM_KEYUP = 0x101;
        private const int WM_SYSKEYDOWN = 0x104;
        private const int WM_SYSKEYUP = 0x105;

        private int KeyboardHookProc(int nCode, Int32 wParam, IntPtr lParam)
        {
            if((nCode >= 0) && (KeyDownEvent != null || KeyUpEvent != null || KeyPressEvent != null))
            {
                KeyboardHookStruct MyKeyboardHookStruct = (KeyboardHookStruct)Marshal.PtrToStructure(lParam, typeof(KeyboardHookStruct));
                // raise KeyDown
                if(KeyDownEvent != null && (wParam == WM_KEYDOWN || wParam == WM_SYSKEYDOWN))
                {
                    Keys keyData = (Keys)MyKeyboardHookStruct.vkCode;
                    KeyEventArgs e = new KeyEventArgs(keyData);
                    KeyDownEvent(this, e);
                }

                if(KeyPressEvent != null && wParam == WM_KEYDOWN)
                {
                    byte[] keyState = new byte[256];
                    GetKeyboardState(keyState);

                    byte[] inBuffer = new byte[2];
                    if(ToAscii(MyKeyboardHookStruct.vkCode, MyKeyboardHookStruct.scanCode, keyState, inBuffer, MyKeyboardHookStruct.flags) == 1)
                    {
                        KeyPressEventArgs e = new KeyPressEventArgs((char)inBuffer[0]);
                        KeyPressEvent(this, e);
                    }
                }

                if(KeyUpEvent != null && (wParam == WM_KEYUP || wParam == WM_SYSKEYUP))
                {
                    Keys keyData = (Keys)MyKeyboardHookStruct.vkCode;
                    KeyEventArgs e = new KeyEventArgs(keyData);
                    KeyUpEvent(this, e);
                }

            }
            return CallNextHookEx(hKeyboardHook, nCode, wParam, lParam);
        }
        ~GlobalKeyboardHook()
        {
            UnInstall();
        }
    }
}
