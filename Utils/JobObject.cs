using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace NodeDock.Utils
{
    /// <summary>
    /// Windows 作业对象封装类，用于自动清理子进程树
    /// </summary>
    public class JobObject : IDisposable
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr CreateJobObject(IntPtr lpJobAttributes, string lpName);

        [DllImport("kernel32.dll")]
        private static extern bool SetInformationJobObject(IntPtr hJob, JobObjectInfoType infoType, IntPtr lpJobObjectInfo, uint cbJobObjectInfoLength);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool AssignProcessToJobObject(IntPtr hJob, IntPtr hProcess);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CloseHandle(IntPtr hObject);

        private IntPtr _handle;
        private bool _disposed;

        public JobObject()
        {
            _handle = CreateJobObject(IntPtr.Zero, null);

            var info = new JOBOBJECT_BASIC_LIMIT_INFORMATION
            {
                LimitFlags = JOBOBJECTLIMIT.JOB_OBJECT_LIMIT_KILL_ON_JOB_CLOSE
            };

            var extendedInfo = new JOBOBJECT_EXTENDED_LIMIT_INFORMATION
            {
                BasicLimitInformation = info
            };

            int length = Marshal.SizeOf(extendedInfo);
            IntPtr extendedInfoPtr = Marshal.AllocHGlobal(length);
            try
            {
                Marshal.StructureToPtr(extendedInfo, extendedInfoPtr, false);

                if (!SetInformationJobObject(_handle, JobObjectInfoType.ExtendedLimitInformation, extendedInfoPtr, (uint)length))
                {
                    throw new Exception("设置作业对象信息失败。");
                }
            }
            finally
            {
                Marshal.FreeHGlobal(extendedInfoPtr);
            }
        }

        public void AddProcess(IntPtr processHandle)
        {
            if (!AssignProcessToJobObject(_handle, processHandle))
            {
                throw new Exception("将进程添加到作业对象失败。");
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            if (_handle != IntPtr.Zero)
            {
                CloseHandle(_handle);
                _handle = IntPtr.Zero;
            }
        }
    }

    #region Win32 P/Invoke Structures

    public enum JobObjectInfoType
    {
        AssociateCompletionPortInformation = 7,
        BasicLimitInformation = 2,
        BasicUIRestrictions = 4,
        EndOfJobTimeInformation = 6,
        ExtendedLimitInformation = 9,
        SecurityLimitInformation = 5,
        GroupInformation = 11
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct JOBOBJECT_BASIC_LIMIT_INFORMATION
    {
        public Int64 PerProcessUserTimeLimit;
        public Int64 PerJobUserTimeLimit;
        public JOBOBJECTLIMIT LimitFlags;
        public UIntPtr MinimumWorkingSetSize;
        public UIntPtr MaximumWorkingSetSize;
        public UInt32 ActiveProcessLimit;
        public Int64 Affinity;
        public UInt32 PriorityClass;
        public UInt32 SchedulingClass;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct IO_COUNTERS
    {
        public UInt64 ReadOperationCount;
        public UInt64 WriteOperationCount;
        public UInt64 OtherOperationCount;
        public UInt64 ReadTransferCount;
        public UInt64 WriteTransferCount;
        public UInt64 OtherTransferCount;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct JOBOBJECT_EXTENDED_LIMIT_INFORMATION
    {
        public JOBOBJECT_BASIC_LIMIT_INFORMATION BasicLimitInformation;
        public IO_COUNTERS IoCounters;
        public UIntPtr ProcessMemoryLimit;
        public UIntPtr JobMemoryLimit;
        public UIntPtr PeakProcessMemoryLimit;
        public UIntPtr PeakJobMemoryLimit;
    }

    public enum JOBOBJECTLIMIT : uint
    {
        JOB_OBJECT_LIMIT_KILL_ON_JOB_CLOSE = 0x2000
    }

    #endregion
}
