using System;
using System.Collections.Generic;
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

        [DllImport("kernel32.dll")]
        private static extern bool QueryInformationJobObject(IntPtr hJob, JobObjectInfoType infoType, IntPtr lpJobObjectInfo, uint cbJobObjectInfoLength, out uint lpReturnLength);

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

        /// <summary>
        /// 获取作业中所有进程的总 CPU 内核时间（100纳秒为单位）
        /// </summary>
        public long GetCpuTime()
        {
            var info = new JOBOBJECT_BASIC_ACCOUNTING_INFORMATION();
            int length = Marshal.SizeOf(info);
            IntPtr ptr = Marshal.AllocHGlobal(length);
            try
            {
                if (!QueryInformationJobObject(_handle, JobObjectInfoType.BasicAccountingInformation, ptr, (uint)length, out _))
                {
                    return 0;
                }
                info = (JOBOBJECT_BASIC_ACCOUNTING_INFORMATION)Marshal.PtrToStructure(ptr, typeof(JOBOBJECT_BASIC_ACCOUNTING_INFORMATION));
                return info.TotalUserTime + info.TotalKernelTime;
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }
        }

        /// <summary>
        /// 获取作业中所有活跃进程的 PID 列表
        /// </summary>
        public List<int> GetProcessIds()
        {
            var pids = new List<int>();
            
            // 初始分配一个可以容纳 100 个进程的缓冲区
            int maxProcesses = 100;
            int length = Marshal.SizeOf(typeof(JOBOBJECT_BASIC_PROCESS_ID_LIST)) + (IntPtr.Size * (maxProcesses - 1));
            IntPtr ptr = Marshal.AllocHGlobal(length);

            try
            {
                if (QueryInformationJobObject(_handle, JobObjectInfoType.BasicProcessIdList, ptr, (uint)length, out _))
                {
                    var list = (JOBOBJECT_BASIC_PROCESS_ID_LIST)Marshal.PtrToStructure(ptr, typeof(JOBOBJECT_BASIC_PROCESS_ID_LIST));
                    for (int i = 0; i < list.NumberOfProcessIdsInList; i++)
                    {
                        IntPtr pidPtr = new IntPtr(ptr.ToInt64() + Marshal.OffsetOf<JOBOBJECT_BASIC_PROCESS_ID_LIST>("ProcessIdList").ToInt64() + (i * IntPtr.Size));
                        pids.Add((int)Marshal.ReadIntPtr(pidPtr));
                    }
                }
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }

            return pids;
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
        BasicAccountingInformation = 1,
        BasicLimitInformation = 2,
        BasicProcessIdList = 3,
        BasicUIRestrictions = 4,
        SecurityLimitInformation = 5,
        EndOfJobTimeInformation = 6,
        AssociateCompletionPortInformation = 7,
        ExtendedLimitInformation = 9,
        GroupInformation = 11
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct JOBOBJECT_BASIC_ACCOUNTING_INFORMATION
    {
        public long TotalUserTime;
        public long TotalKernelTime;
        public long ThisPeriodTotalUserTime;
        public long ThisPeriodTotalKernelTime;
        public uint TotalPageFaultCount;
        public uint TotalProcesses;
        public uint ActiveProcesses;
        public uint TotalTerminatedProcesses;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct JOBOBJECT_BASIC_PROCESS_ID_LIST
    {
        public uint NumberOfAssignedProcesses;
        public uint NumberOfProcessIdsInList;
        public UIntPtr ProcessIdList; // 这是一个占位符，实际上是连续的 IntPtr
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
