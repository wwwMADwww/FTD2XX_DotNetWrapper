using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace FTD2X_DotNetWrapper.Platform
{
    public class LinuxFuncs : IPlatformFuncs
    {
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
    }
}
