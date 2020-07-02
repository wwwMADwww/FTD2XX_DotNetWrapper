using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace FTD2X_DotNetWrapper.Platform
{
    public class PlatformFuncs: IPlatformFuncs
    {


        #if TargetOS_Windows

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


        #elif TargetOS_Linux

        // https://linux.die.net/man/3/dlopen

        // https://code.woboq.org/userspace/glibc/bits/dlfcn.h.html
        const int RTLD_NOW = 0x00002;


        [DllImport("libdl.so")]
        protected static extern IntPtr dlopen(string filename, int flags);


        [DllImport("libdl.so")]
        protected static extern IntPtr dlsym(IntPtr handle, string symbol);


        [DllImport("libdl.so")]
        protected static extern int dlclose(IntPtr handle);

        public IntPtr LoadLibrary(string name)
        {
            return dlopen(name, RTLD_NOW);
        }

        public IntPtr GetSymbol(IntPtr libraryHandle, string symbolName)
        {
            return dlsym(libraryHandle, symbolName);
        }

        public int FreeLibrary(IntPtr libraryHandle)
        {
            return dlclose(libraryHandle);
        }
        
        #else

        #error please check or define TargetOS_ variable in project file, build settings or commandline
        // examples:
        // dotnet build FTD2X_DotNetWrapper.csproj /p:DefineConstants=TargetOS_Windows
        // dotnet build FTD2X_DotNetWrapper.csproj /p:DefineConstants=TargetOS_Linux

        public IntPtr LoadLibrary(string name) => throw new NotImplementedException();
        public IntPtr GetSymbol(IntPtr libraryHandle, string symbolName) => throw new NotImplementedException();
        public int FreeLibrary(IntPtr libraryHandle) => throw new NotImplementedException();

        #endif


    }
}
