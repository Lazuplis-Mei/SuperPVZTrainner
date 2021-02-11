using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace TrainnerExpend
{
    public class NotDefinedAttribute : Attribute { }

    public class FolderBrowserDialog : IDisposable
    {

        #region DllImports

        [DllImport("shell32.dll")]
        private static extern int SHILCreateFromPath([MarshalAs(UnmanagedType.LPWStr)] string pszPath, out IntPtr ppIdl, ref uint rgflnOut);

        [DllImport("shell32.dll")]
        private static extern int SHCreateShellItem(IntPtr pidlParent, IntPtr psfParent, IntPtr pidl, out IShellItem ppsi);

        [DllImport("user32.dll")]
        private static extern IntPtr GetActiveWindow();

        #endregion

        #region ComImports

        [ComImport, Guid("43826D1E-E718-42EE-BC55-A1E261C37BFE")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IShellItem
        {
            [NotDefined]
            void BindToHandler();
            [NotDefined]
            void GetParent();
            void GetDisplayName(SIGDN sigdnName, [MarshalAs(UnmanagedType.LPWStr)] out string ppszName);
            [NotDefined]
            void GetAttributes();
            [NotDefined]
            void Compare();
        }

        [ComImport, Guid("42f85136-db7e-439c-85f1-e4075d135fc8")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IFileDialog
        {
            [PreserveSig]
            uint Show(IntPtr parent);
            [NotDefined]
            void SetFileTypes();
            [NotDefined]
            void SetFileTypeIndex();
            [NotDefined]
            void GetFileTypeIndex();
            [NotDefined]
            void Advise();
            [NotDefined]
            void Unadvise();
            void SetOptions(FOS fos);
            void GetOptions(out FOS pfos);
            [NotDefined]
            void SetDefaultFolder();
            void SetFolder(IShellItem psi);
            void GetFolder(out IShellItem ppsi);
            [NotDefined]
            void GetCurrentSelection();
            void SetFileName([MarshalAs(UnmanagedType.LPWStr)] string pszName);
            [NotDefined]
            void GetFileName();
            void SetTitle([MarshalAs(UnmanagedType.LPWStr)] string pszTitle);
            void SetOkButtonLabel([MarshalAs(UnmanagedType.LPWStr)] string pszText);
            void SetFileNameLabel([MarshalAs(UnmanagedType.LPWStr)] string pszLabel);
            void GetResult(out IShellItem ppsi);
            void AddPlace(IShellItem psi, int fdap);
            [NotDefined]
            void SetDefaultExtension();
            [NotDefined]
            void Close();
            [NotDefined]
            void SetClientGuid();
            [NotDefined]
            void ClearClientData();
            [NotDefined]
            void SetFilter();
        }

        [ComImport, Guid("DC1C5A9C-E88A-4dde-A5A1-60F82A20AEF7")]
        private class FileOpenDialog { }

        #endregion

        #region Consts

        private const uint ERROR_CANCELLED = 0x800704C7;

        private enum SIGDN : uint
        {
            SIGDN_DESKTOPABSOLUTEEDITING = 0x8004C000,
            SIGDN_DESKTOPABSOLUTEPARSING = 0x80028000,
            SIGDN_FILESYSPATH = 0x80058000,
            SIGDN_NORMALDISPLAY = 0,
            SIGDN_PARENTRELATIVE = 0x80080001,
            SIGDN_PARENTRELATIVEEDITING = 0x80031001,
            SIGDN_PARENTRELATIVEFORADDRESSBAR = 0x8007C001,
            SIGDN_PARENTRELATIVEPARSING = 0x80018001,
            SIGDN_URL = 0x80068000
        }

        [Flags]
        private enum FOS
        {
            FOS_ALLNONSTORAGEITEMS = 0x80,
            FOS_ALLOWMULTISELECT = 0x200,
            FOS_CREATEPROMPT = 0x2000,
            FOS_DEFAULTNOMINIMODE = 0x20000000,
            FOS_DONTADDTORECENT = 0x2000000,
            FOS_FILEMUSTEXIST = 0x1000,
            FOS_FORCEFILESYSTEM = 0x40,
            FOS_FORCESHOWHIDDEN = 0x10000000,
            FOS_HIDEMRUPLACES = 0x20000,
            FOS_HIDEPINNEDPLACES = 0x40000,
            FOS_NOCHANGEDIR = 8,
            FOS_NODEREFERENCELINKS = 0x100000,
            FOS_NOREADONLYRETURN = 0x8000,
            FOS_NOTESTFILECREATE = 0x10000,
            FOS_NOVALIDATE = 0x100,
            FOS_OVERWRITEPROMPT = 2,
            FOS_PATHMUSTEXIST = 0x800,
            FOS_PICKFOLDERS = 0x20,
            FOS_SHAREAWARE = 0x4000,
            FOS_STRICTFILETYPES = 4
        }

        #endregion

        /// <summary>
        /// 获取或设置一个字符串，其中包含在选择对话框中选定的文件夹的完整路径。
        /// </summary>
        public string DirectoryPath { get; set; }

        /// <summary>
        /// 获取或设置一个字符串，其中包含在选择对话框中选定的文件夹的名字。
        /// </summary>
        public string DirectoryName { get; set; }

        /// <summary>
        /// 获取或设置一个字符串，其中包含在选择对话框中选定的文件夹的完整Url。
        /// </summary>
        public string UrlPath { get; private set; }

        /// <summary>
        /// 获取或设置一个值，该值指示如果用户指定不存在的路径，对话框是否显示警告。
        /// </summary>
        public bool CheckPathExists
        {
            get => Options.HasFlag(FOS.FOS_PATHMUSTEXIST);
            set => Options = value ?
                Options | FOS.FOS_PATHMUSTEXIST :
                Options ^ FOS.FOS_PATHMUSTEXIST;
        }

        /// <summary>
        /// 获取或设置一个值，该值指定是否改变当前工作目录.
        /// </summary>
        public bool ChangeDirectory
        {
            get => !Options.HasFlag(FOS.FOS_NOCHANGEDIR);
            set => Options = value ?
                Options ^ FOS.FOS_NOCHANGEDIR :
                Options | FOS.FOS_NOCHANGEDIR;
        }

        /// <summary>
        /// 获取或设置一个值，该值指定是否提示创建不存在的路径。
        /// </summary>
        public bool CreatePrompt
        {
            get => Options.HasFlag(FOS.FOS_CREATEPROMPT);
            set => Options = value ?
                Options | FOS.FOS_CREATEPROMPT :
                Options ^ FOS.FOS_CREATEPROMPT;
        }

        /// <summary>
        /// 获取或设置一个值，该值指定是否隐藏标准的导航窗格。
        /// </summary>
        public bool HidePinnedPlaces
        {
            get => Options.HasFlag(FOS.FOS_HIDEPINNEDPLACES);
            set => Options = value ?
                Options | FOS.FOS_HIDEPINNEDPLACES :
                Options ^ FOS.FOS_HIDEPINNEDPLACES;
        }

        /// <summary>
        /// 获取或设置一个值，该值指示是否添加条目被打开或保存到最近文件列表。
        /// </summary>
        public bool AddToRecent
        {
            get => !Options.HasFlag(FOS.FOS_DONTADDTORECENT);
            set => Options = value ?
                Options ^ FOS.FOS_DONTADDTORECENT :
                Options | FOS.FOS_DONTADDTORECENT;
        }

        /// <summary>
        /// 获取或设置一个值，该值指示是否显示隐藏项目和系统项目。
        /// </summary>
        public bool ShowHidden
        {
            get => Options.HasFlag(FOS.FOS_FORCESHOWHIDDEN);
            set => Options = value ?
                Options | FOS.FOS_FORCESHOWHIDDEN :
                Options ^ FOS.FOS_FORCESHOWHIDDEN;
        }

        /// <summary>
        /// 设置在对话框的标题栏中显示的文本。
        /// </summary>
        public string Title { set => Dialog.SetTitle(value); }

        /// <summary>
        /// 设置对话框中选择文件夹按钮显示的文本。
        /// </summary>
        public string SelectButtonText { set => Dialog.SetOkButtonLabel(value); }

        /// <summary>
        /// 设置对话框中文件夹标签显示的文本。
        /// </summary>
        public string DirectoryNameText { set => Dialog.SetFileNameLabel(value); }

        /// <summary>
        /// 添加文件夹对话框的自定义空间
        /// </summary>
        /// <param name="folder">文件夹路径</param>
        /// <param name="addtotop">添加在已有项的顶部</param>
        public void AddCustomPlace(string folder, bool addtotop = false)
        {
            Dialog.AddPlace(GetShellItem(folder), addtotop ? 1 : 0);
        }

        private FOS Options
        {
            get
            {
                Dialog.GetOptions(out FOS fos);
                return fos;
            }
            set
            {
                Dialog.SetOptions(value);
            }
        }
        private readonly IFileDialog Dialog;

        public FolderBrowserDialog()
        {
            Dialog = new FileOpenDialog() as IFileDialog;
            Options |= FOS.FOS_PICKFOLDERS;
        }

        void IDisposable.Dispose()
        {
            Marshal.ReleaseComObject(Dialog);
        }

        private IShellItem GetShellItem(string path)
        {
            uint atts = 0;
            if (SHILCreateFromPath(path, out IntPtr idl, ref atts) == 0 &&
                (SHCreateShellItem(IntPtr.Zero, IntPtr.Zero, idl, out IShellItem shellItem) == 0))
                return shellItem;
            return null;
        }

        /// <summary>
        /// 向用户显示 FolderBrowser 的对话框
        /// </summary>
        public bool? ShowDialog()
        {
            return ShowDialog(GetActiveWindow());
        }

        /// <summary>
        /// 向用户显示 FolderBrowser 的对话框
        /// </summary>
        public bool? ShowDialog(IntPtr owner)
        {
            if (!string.IsNullOrWhiteSpace(DirectoryPath))
                Dialog.SetFolder(GetShellItem(DirectoryPath));

            if (!string.IsNullOrWhiteSpace(DirectoryName))
                Dialog.SetFileName(DirectoryName);

            switch (Dialog.Show(owner))
            {
                case ERROR_CANCELLED:
                    return false;
                case 0:
                    Dialog.GetResult(out IShellItem shellItem);
                    shellItem.GetDisplayName(SIGDN.SIGDN_FILESYSPATH, out string path);
                    DirectoryPath = path;
                    shellItem.GetDisplayName(SIGDN.SIGDN_NORMALDISPLAY, out path);
                    DirectoryName = path;
                    shellItem.GetDisplayName(SIGDN.SIGDN_URL, out path);
                    UrlPath = path;
                    return true;
                default:
                    return null;
            }
        }

    }
}
