using System;
using System.Runtime.InteropServices;

namespace OdysseyServer_Project
{
    [StructLayout(LayoutKind.Sequential, Size = 1)]
    public struct ErrorCodes
    {
        public const int INVALID_HANDLE_VALUE = -1;
    }
}
