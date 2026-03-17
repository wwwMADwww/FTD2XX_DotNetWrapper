using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace FTD2XX_NET
{
    public partial class FTDI : IDisposable
    {
        const string _libpathWindows = "FTD2XX.DLL";

        // path suggested by official FTDI linux installation guide
        const string _libpathLinux = "/usr/local/lib/libftd2xx.so";


        void Init(string libpath = null)
        {
            if (libpath == null)
            {
                libpath = GetLibraryPath();
            }

            if (hFTD2XXDLL == IntPtr.Zero)
            {
                // Load FTD2XX library
                hFTD2XXDLL = LoadLibrary(libpath);

                if (hFTD2XXDLL == IntPtr.Zero)
                {
                    // if failed - try to find library somewhere nearby, besides default OS search paths
                    var libfilename = Path.GetFileName(libpath);
                    var assemblyDir = Path.GetDirectoryName(GetType().Assembly.Location);

                    hFTD2XXDLL = LoadLibrary(Path.Combine(assemblyDir, libfilename));
                }
            }

            // If we have succesfully loaded the library, get the function pointers set up
            if (hFTD2XXDLL != IntPtr.Zero)
            {
                FindFunctionPointers();
            }
            else
            {
                Console.WriteLine("Failed to load FTD2XX library. Are the FTDI drivers installed?");
            }
        }


        private IntPtr LoadLibrary(string dllToLoad)
        {
            return NativeLibrary.Load(dllToLoad);
        }

        private IntPtr GetProcAddress(IntPtr hModule, string procedureName)
        {
            return NativeLibrary.GetExport(hModule, procedureName);
        }

        private bool FreeLibrary(IntPtr hModule)
        {
            NativeLibrary.Free(hModule);
            return true;
        }


        private string GetLibraryPath()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return _libpathWindows;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) return _libpathLinux;
            
            throw new NotImplementedException("Loading FT2XX library not implemented for this OS");
        }


        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                NativeLibrary.Free(hFTD2XXDLL);
                hFTD2XXDLL = IntPtr.Zero;

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~FTDI()
        // {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            GC.SuppressFinalize(this);
        }

        #endregion


        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate FT_STATUS tFT_WriteBufPtr(IntPtr ftHandle, in byte lpBuffer, UInt32 dwBytesToWrite, ref UInt32 lpdwBytesWritten);

        IntPtr pFT_WriteBufPtr = IntPtr.Zero;

        private void FindFunctionPointersExtend()
        {
            pFT_WriteBufPtr = GetProcAddress(hFTD2XXDLL, "FT_Write");
        }


        /// <summary>
        /// Write data to an open FTDI device.
        /// </summary>
        /// <returns>FT_STATUS value from FT_Write in FTD2XX.DLL</returns>
        /// <param name="dataBuffer">An array of bytes which contains the data to be written to the device.</param>
        /// <param name="offset">Buffer offset to start write data from</param>
        /// <param name="numBytesToWrite">The number of bytes to be written to the device.</param>
        /// <param name="numBytesWritten">The number of bytes actually written to the device.</param>
        public FT_STATUS Write(byte[] dataBuffer, Int32 offset, Int32 numBytesToWrite, ref UInt32 numBytesWritten)
        {
            // Initialise ftStatus to something other than FT_OK
            FT_STATUS ftStatus = FT_STATUS.FT_OTHER_ERROR;

            // If the DLL hasn't been loaded, just return here
            if (hFTD2XXDLL == IntPtr.Zero)
                return ftStatus;

            // Check for our required function pointers being set up
            if (pFT_Write != IntPtr.Zero)
            {
                tFT_WriteBufPtr FT_WriteBufPtr = (tFT_WriteBufPtr)Marshal.GetDelegateForFunctionPointer(pFT_WriteBufPtr, typeof(tFT_WriteBufPtr));

                if (ftHandle != IntPtr.Zero)
                {
                    unsafe
                    {
                        var span = dataBuffer.AsSpan(offset, numBytesToWrite);
                        fixed (byte* ptr = span)
                        {
                            ftStatus = FT_WriteBufPtr(ftHandle, Unsafe.AsRef<byte>(ptr), (UInt32)numBytesToWrite, ref numBytesWritten);
                        }
                    }
                }
            }
            else
            {
                if (pFT_Write == IntPtr.Zero)
                {
                    Console.WriteLine("Failed to load function FT_Write.");
                }
            }
            return ftStatus;
        }


        /// <summary>
        /// Write data to an open FTDI device.
        /// </summary>
        /// <returns>FT_STATUS value from FT_Write in FTD2XX.DLL</returns>
        /// <param name="dataBuffer">An array of bytes which contains the data to be written to the device.</param>
        /// <param name="numBytesWritten">The number of bytes actually written to the device.</param>
        public FT_STATUS Write(Span<byte> dataBuffer, ref UInt32 numBytesWritten)
        {
            // Initialise ftStatus to something other than FT_OK
            FT_STATUS ftStatus = FT_STATUS.FT_OTHER_ERROR;

            // If the DLL hasn't been loaded, just return here
            if (hFTD2XXDLL == IntPtr.Zero)
                return ftStatus;

            // Check for our required function pointers being set up
            if (pFT_Write != IntPtr.Zero)
            {
                tFT_WriteBufPtr FT_WriteBufPtr = (tFT_WriteBufPtr)Marshal.GetDelegateForFunctionPointer(pFT_WriteBufPtr, typeof(tFT_WriteBufPtr));

                if (ftHandle != IntPtr.Zero)
                {
                    unsafe
                    {
                        fixed (byte* ptr = dataBuffer)
                        {
                            ftStatus = FT_WriteBufPtr(ftHandle, Unsafe.AsRef<byte>(ptr), (UInt32)dataBuffer.Length, ref numBytesWritten);
                        }
                    }
                }
            }
            else
            {
                if (pFT_Write == IntPtr.Zero)
                {
                    Console.WriteLine("Failed to load function FT_Write.");
                }
            }
            return ftStatus;
        }



    }
}
