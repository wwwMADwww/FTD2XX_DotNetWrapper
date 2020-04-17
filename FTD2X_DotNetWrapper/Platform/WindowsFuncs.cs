using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace FTD2X_DotNetWrapper.Platform
{
    public class WindowsFuncs : IPlatformFuncs
    {

        #region LOAD_LIBRARIES
        /// <summary>
        /// Built-in Windows API functions to allow us to dynamically load our own DLL.
        /// Will allow us to use old versions of the DLL that do not have all of these functions available.
        /// </summary>
        [DllImport("kernel32.dll", EntryPoint = "LoadLibrary")]
        private static extern IntPtr LoadLibraryFunc(string dllToLoad);

        [DllImport("kernel32.dll", EntryPoint = "GetProcAddress")]
        private static extern IntPtr GetProcAddressFunc(IntPtr hModule, string procedureName);

        [DllImport("kernel32.dll", EntryPoint = "FreeLibrary")]
        private static extern bool FreeLibraryFunc(IntPtr hModule);
        #endregion

        public IntPtr LoadLibrary(string name)
        {
            return LoadLibraryFunc(name);
        }

        public IntPtr GetSymbol(IntPtr libraryHandle, string symbolName)
        {
            return GetProcAddressFunc(libraryHandle, symbolName);
        }

        public int FreeLibrary(IntPtr libraryHandle)
        {
            return FreeLibraryFunc(libraryHandle) ? 0 : -1;
        }



    }
}
