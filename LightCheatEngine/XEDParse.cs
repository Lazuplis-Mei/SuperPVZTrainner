using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace LightCheatEngine
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 8)]
    public struct XEDPARSE
    {
        public int x64;
        public ulong cip;
        public int dest_size;
        public int cbUnknown;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = XEDParse.XEDPARSE_MAXASMSIZE)]
        public byte[] dest;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = XEDParse.XEDPARSE_MAXBUFSIZE)]
        public string instr;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = XEDParse.XEDPARSE_MAXBUFSIZE)]
        public string error;
    };
    public static class XEDParse
    {
        public const int XEDPARSE_MAXASMSIZE = 16;
        public const int XEDPARSE_MAXBUFSIZE = 256;
        [DllImport("XEDParse.dll", EntryPoint = "XEDParseAssemble", CallingConvention = CallingConvention.Cdecl)]
        public static extern int XEDParseAssemble(ref XEDPARSE xedparse);
    }
}
