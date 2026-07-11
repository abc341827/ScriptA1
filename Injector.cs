using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace WinFormsApp1
{
    public class Injector
    {
        // 基础API
        [DllImport("kernel32.dll")]
        private static extern IntPtr OpenProcess(uint dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll")]
        private static extern bool CloseHandle(IntPtr hObject);

        [DllImport("kernel32.dll")]
        private static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, uint dwSize,
            uint flAllocationType, uint flProtect);

        [DllImport("kernel32.dll")]
        private static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress,
            byte[] lpBuffer, uint nSize, out IntPtr lpNumberOfBytesWritten);

        [DllImport("kernel32.dll")]
        private static extern IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr lpThreadAttributes,
            uint dwStackSize, IntPtr lpStartAddress, IntPtr lpParameter, uint dwCreationFlags,
            out IntPtr lpThreadId);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern int WaitForSingleObject(IntPtr hHandle, int dwMilliseconds);

        [DllImport("kernel32.dll")]
        private static extern bool VirtualFreeEx(IntPtr hProcess, IntPtr lpAddress, uint dwSize, uint dwFreeType);

        [DllImport("advapi32.dll")]
        private static extern bool OpenProcessToken(IntPtr ProcessHandle, uint DesiredAccess, out IntPtr TokenHandle);

        [DllImport("advapi32.dll")]
        private static extern bool LookupPrivilegeValue(string lpSystemName, string lpName, out long lpLuid);

        [DllImport("advapi32.dll")]
        private static extern bool AdjustTokenPrivileges(IntPtr TokenHandle, bool DisableAllPrivileges,
            ref TOKEN_PRIVILEGES NewState, int BufferLength, IntPtr PreviousState, IntPtr ReturnLength);
        [DllImport("kernel32.dll")]
        private static extern IntPtr CreateToolhelp32Snapshot(uint dwFlags, uint th32ProcessID);

        [DllImport("kernel32.dll")]
        private static extern bool Module32First(IntPtr hSnapshot, ref MODULEENTRY32 lpme);

        [DllImport("kernel32.dll")]
        private static extern bool Module32Next(IntPtr hSnapshot, ref MODULEENTRY32 lpme);
        // 声明 Unicode 版本的 API
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern bool Module32FirstW(IntPtr hSnapshot, ref MODULEENTRY32 lpme);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern bool Module32NextW(IntPtr hSnapshot, ref MODULEENTRY32 lpme);
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct MODULEENTRY32
        {
            public uint dwSize;
            public uint th32ModuleID;
            public uint th32ProcessID;
            public uint GlblcntUsage;
            public uint ProccntUsage;
            public IntPtr modBaseAddr;
            public uint modBaseSize;
            public IntPtr hModule;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            public string szModule;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szExePath;
        }

        private const uint PROCESS_ALL_ACCESS = 0x001F0FFF;
        private const uint MEM_COMMIT = 0x1000;
        private const uint MEM_RESERVE = 0x2000;
        private const uint PAGE_READWRITE = 0x04;
        private const uint TH32CS_SNAPMODULE = 0x00000008;
        private const uint TH32CS_SNAPMODULE32 = 0x00000010;

        /// <summary>
        /// 在目标进程中调用 SetGameWindow 导出函数
        /// </summary>
        /// <param name="processId">目标进程ID</param>
        /// <param name="dllName">DLL文件名（如 "MouseHookDLL.dll"）</param>
        /// <param name="hwnd">要传递的游戏窗口句柄</param>
        /// <returns>是否成功</returns>
        public static bool CallSetGameWindow(int processId, string dllName,string dllfullPath, IntPtr hwnd)
        {
            // 1. 获取目标进程中 DLL 的基址
            IntPtr remoteDllBase = GetRemoteModuleBase(processId, dllName);
            if (remoteDllBase == IntPtr.Zero)
            {
                Console.WriteLine($"未在进程 {processId} 中找到模块 {dllName}");
                return false;
            }

            // 2. 计算 SetGameWindow 函数在 DLL 中的偏移
            // 首先获取本地进程中 SetGameWindow 的地址
            IntPtr localDllBase = GetModuleHandle(dllName); // 注意：当前进程可能未加载该 DLL，如果未加载则此方法返回0
            if (localDllBase == IntPtr.Zero)
            {
                // 如果当前进程没有加载该 DLL，可以临时加载一下再卸载
                localDllBase = LoadLibrary(dllfullPath);
                if (localDllBase == IntPtr.Zero)
                {
                    Console.WriteLine("无法在本地加载 DLL 以获取函数地址");
                    return false;
                }
            }

            IntPtr localFuncAddr = GetProcAddress(localDllBase, "SetGameWindow");
            if (localFuncAddr == IntPtr.Zero)
            {
                Console.WriteLine("无法找到 SetGameWindow 函数地址");
                if (localDllBase != IntPtr.Zero && localDllBase != GetModuleHandle(dllName))
                    FreeLibrary(localDllBase);
                return false;
            }

            // 计算偏移：本地函数地址 - 本地 DLL 基址
            long funcOffset = localFuncAddr.ToInt64() - localDllBase.ToInt64();
            // 远程函数地址 = 远程 DLL 基址 + 偏移
            IntPtr remoteFuncAddr = new IntPtr(remoteDllBase.ToInt64() + funcOffset);

            // 如果临时加载了 DLL，释放它
            if (localDllBase != GetModuleHandle(dllName))
                FreeLibrary(localDllBase);

            // 3. 打开目标进程
            IntPtr hProcess = OpenProcess(PROCESS_ALL_ACCESS, false, processId);
            if (hProcess == IntPtr.Zero)
            {
                Console.WriteLine($"打开进程失败，错误码：{Marshal.GetLastWin32Error()}");
                return false;
            }

            try
            {
                // 4. 在目标进程中分配内存以存放参数（hwnd 本身是一个指针，可以直接作为参数传递）
                // 我们只需要将 hwnd 的值作为线程参数传递，所以可以直接使用 IntPtr 参数，无需额外分配内存
                // 但如果函数期望的是指针，我们可以直接传递 hwnd 的值（它本身就是一个指针）
                // 注意：SetGameWindow 接受 HWND，即窗口句柄，可以直接作为线程参数传递（32/64位系统都适配）

                // 5. 创建远程线程调用 SetGameWindow
                IntPtr hThread = CreateRemoteThread(hProcess, IntPtr.Zero, 0, remoteFuncAddr, hwnd, 0, out _);
                if (hThread == IntPtr.Zero)
                {
                    Console.WriteLine($"创建远程线程失败，错误码：{Marshal.GetLastWin32Error()}");
                    return false;
                }

                // 等待线程结束（最多5秒）
                WaitForSingleObject(hThread, 5000);
                CloseHandle(hThread);
                Console.WriteLine("已成功调用 SetGameWindow");
                return true;
            }
            finally
            {
                CloseHandle(hProcess);
            }
        }

        /// <summary>
        /// 获取目标进程中指定模块的基址
        /// </summary>
        private static IntPtr GetRemoteModuleBase(int processId, string moduleName)
        {
            IntPtr hSnapshot = CreateToolhelp32Snapshot(TH32CS_SNAPMODULE | TH32CS_SNAPMODULE32, (uint)processId);
            if (hSnapshot == IntPtr.Zero || hSnapshot == (IntPtr)(-1))
            {
                int error = Marshal.GetLastWin32Error();
                Console.WriteLine($"快照创建失败，错误码：{error}");
                return IntPtr.Zero;
            }

            try
            {
                MODULEENTRY32 me = new MODULEENTRY32();
                me.dwSize = (uint)Marshal.SizeOf(typeof(MODULEENTRY32));

                // 使用 W 版本的函数
                if (Module32FirstW(hSnapshot, ref me))
                {
                    do
                    {
                        Console.WriteLine($"模块名: {me.szModule}，基址: 0x{me.modBaseAddr.ToInt64():X}");
                        if (me.szModule.Equals(moduleName, StringComparison.OrdinalIgnoreCase))
                            return me.modBaseAddr;
                    } while (Module32NextW(hSnapshot, ref me));
                }
                else
                {
                    int error = Marshal.GetLastWin32Error();
                    Console.WriteLine($"Module32FirstW 失败，错误码：{error}");
                }

                Console.WriteLine($"未找到模块: {moduleName}");
                return IntPtr.Zero;
            }
            finally
            {
                CloseHandle(hSnapshot);
            }
        }

        // 如果需要临时加载 DLL
        [DllImport("kernel32.dll")]
        private static extern IntPtr LoadLibrary(string lpFileName);

        [DllImport("kernel32.dll")]
        private static extern bool FreeLibrary(IntPtr hModule);

        /// <summary>
        /// 从目标进程卸载指定名称的DLL
        /// </summary>
        /// <param name="processId">进程ID</param>
        /// <param name="dllName">DLL文件名（如 "MouseHookDLL.dll"）</param>
        public static bool UnloadDLL(int processId, string dllName)
        {
            // 1. 获取目标进程中该DLL的模块基址
            IntPtr hModule = GetRemoteModuleHandle(processId, dllName);
            if (hModule == IntPtr.Zero)
            {
                Console.WriteLine($"在进程 {processId} 中未找到模块 {dllName}");
                return false;
            }

            // 2. 获取FreeLibrary函数地址
            IntPtr pFreeLibrary = GetProcAddress(GetModuleHandle("kernel32.dll"), "FreeLibrary");
            if (pFreeLibrary == IntPtr.Zero)
            {
                Console.WriteLine("获取FreeLibrary地址失败");
                return false;
            }

            // 3. 打开目标进程（需要足够权限）
            IntPtr hProcess = OpenProcess(PROCESS_CREATE_THREAD | PROCESS_QUERY_INFORMATION |
                                          PROCESS_VM_OPERATION | PROCESS_VM_WRITE | PROCESS_VM_READ,
                                          false, processId);
            if (hProcess == IntPtr.Zero)
            {
                Console.WriteLine($"打开进程失败，错误码：{Marshal.GetLastWin32Error()}");
                return false;
            }

            try
            {
                // 4. 创建远程线程执行FreeLibrary
                IntPtr hThread = CreateRemoteThread(hProcess, IntPtr.Zero, 0, pFreeLibrary, hModule, 0, out _);
                if (hThread == IntPtr.Zero)
                {
                    Console.WriteLine($"创建远程线程失败，错误码：{Marshal.GetLastWin32Error()}");
                    return false;
                }

                // 5. 等待线程结束（最多5秒）
                WaitForSingleObject(hThread, 5000);
                CloseHandle(hThread);
                Console.WriteLine($"已从进程 {processId} 卸载 {dllName}");
                return true;
            }
            finally
            {
                CloseHandle(hProcess);
            }
        }

        /// <summary>
        /// 获取目标进程中指定模块的基址
        /// </summary>
        private static IntPtr GetRemoteModuleHandle(int processId, string moduleName)
        {
            IntPtr hSnapshot = CreateToolhelp32Snapshot(TH32CS_SNAPMODULE | TH32CS_SNAPMODULE32, (uint)processId);
            if (hSnapshot == IntPtr.Zero || hSnapshot == (IntPtr)(-1))
                return IntPtr.Zero;

            try
            {
                MODULEENTRY32 me = new MODULEENTRY32();
                me.dwSize = (uint)Marshal.SizeOf(typeof(MODULEENTRY32));

                if (Module32First(hSnapshot, ref me))
                {
                    do
                    {
                        if (me.szModule.Equals(moduleName, StringComparison.OrdinalIgnoreCase))
                            return me.hModule; // 注意：这是模块在目标进程中的句柄，即模块基址
                    } while (Module32Next(hSnapshot, ref me));
                }
                return IntPtr.Zero;
            }
            finally
            {
                CloseHandle(hSnapshot);
            }
        }
        // 常量定义
        private const uint PROCESS_CREATE_THREAD = 0x0002;
        private const uint PROCESS_QUERY_INFORMATION = 0x0400;
        private const uint PROCESS_VM_OPERATION = 0x0008;
        private const uint PROCESS_VM_WRITE = 0x0020;
        private const uint PROCESS_VM_READ = 0x0010;
        private const uint PROCESS_TERMINATE = 0x0001;


        private const uint MEM_RELEASE = 0x8000;

        private const uint TOKEN_QUERY = 0x0008;
        private const uint TOKEN_ADJUST_PRIVILEGES = 0x0020;
        private const string SE_DEBUG_NAME = "SeDebugPrivilege";

        [StructLayout(LayoutKind.Sequential)]
        private struct TOKEN_PRIVILEGES
        {
            public uint PrivilegeCount;
            public long Luid;
            public uint Attributes;
        }

        /// <summary>
        /// 提升进程权限为调试权限 [citation:1]
        /// </summary>
        private static bool EnableDebugPrivilege()
        {
            IntPtr hToken;
            if (!OpenProcessToken(Process.GetCurrentProcess().Handle, TOKEN_ADJUST_PRIVILEGES | TOKEN_QUERY, out hToken))
                return false;

            try
            {
                long luid;
                if (!LookupPrivilegeValue(null, SE_DEBUG_NAME, out luid))
                    return false;

                TOKEN_PRIVILEGES tp = new TOKEN_PRIVILEGES
                {
                    PrivilegeCount = 1,
                    Luid = luid,
                    Attributes = 0x00000002  // SE_PRIVILEGE_ENABLED
                };

                return AdjustTokenPrivileges(hToken, false, ref tp, 0, IntPtr.Zero, IntPtr.Zero);
            }
            finally
            {
                CloseHandle(hToken);
            }
        }

        /// <summary>
        /// 注入DLL到目标进程 [citation:1][citation:6]
        /// </summary>
        public static bool InjectDLL(int processId, string dllPath)
        {
            if (!EnableDebugPrivilege())
                Console.WriteLine("警告：权限提升失败，可能无法注入系统进程");

            IntPtr hProcess = OpenProcess(PROCESS_ALL_ACCESS, false, processId);
            if (hProcess == IntPtr.Zero)
            {
                Console.WriteLine($"打开进程失败，错误码：{Marshal.GetLastWin32Error()}");
                return false;
            }

            try
            {
                // 1. 在目标进程中分配内存
                byte[] dllPathBytes = System.Text.Encoding.Unicode.GetBytes(dllPath);
                uint bytesSize = (uint)dllPathBytes.Length;

                IntPtr pRemoteMemory = VirtualAllocEx(hProcess, IntPtr.Zero, bytesSize,
                    MEM_COMMIT | MEM_RESERVE, PAGE_READWRITE);
                if (pRemoteMemory == IntPtr.Zero)
                {
                    Console.WriteLine("分配内存失败");
                    return false;
                }

                // 2. 写入DLL路径
                if (!WriteProcessMemory(hProcess, pRemoteMemory, dllPathBytes, bytesSize, out _))
                {
                    Console.WriteLine("写入内存失败");
                    return false;
                }

                // 3. 获取LoadLibraryW函数地址
                IntPtr pLoadLibrary = GetProcAddress(GetModuleHandle("kernel32.dll"), "LoadLibraryW");
                if (pLoadLibrary == IntPtr.Zero)
                {
                    Console.WriteLine("获取LoadLibrary地址失败");
                    return false;
                }

                // 4. 创建远程线程执行LoadLibrary
                IntPtr hThread = CreateRemoteThread(hProcess, IntPtr.Zero, 0, pLoadLibrary,
                    pRemoteMemory, 0, out _);
                if (hThread == IntPtr.Zero)
                {
                    Console.WriteLine("创建远程线程失败");
                    return false;
                }

                // 5. 等待线程结束
                WaitForSingleObject(hThread, 5000);
                CloseHandle(hThread);

                // 6. 释放分配的内存（可选，进程退出时会自动释放）
                // VirtualFreeEx(hProcess, pRemoteMemory, 0, MEM_RELEASE);

                Console.WriteLine("DLL注入成功");
                return true;
            }
            finally
            {
                CloseHandle(hProcess);
            }
        }

        /// <summary>
        /// 在目标进程中调用带一个指针参数的导出函数
        /// </summary>
        public static bool CallRemoteFunction(int processId, string dllName, string funcName, IntPtr parameter)
        {
            return CallRemoteFunction_(processId, dllName, funcName, parameter);
        }

        /// <summary>
        /// 通用远程调用（支持带参数）
        /// </summary>
        /// <param name="processId">目标进程ID</param>
        /// <param name="dllFullName">DLL文件名（如 "MouseHookDLL.dll"）</param>
        /// <param name="funcName">导出函数名</param>
        /// <param name="parameter">参数值（如果是无参函数，传 IntPtr.Zero）</param>
        /// <param name="paramSize">参数大小（字节），无参传0</param>
        /// <returns>是否成功</returns>
        private static bool CallRemoteFunction_(int processId, string dllFullName, string funcName, IntPtr parameter)
        {
            // 1. 获取远程 DLL 基址
            var file = new FileInfo(dllFullName);
            IntPtr remoteDllBase = GetRemoteModuleBase(processId, file.Name);
            if (remoteDllBase == IntPtr.Zero)
            {
                Console.WriteLine($"未找到模块 {dllFullName}");
                return false;
            }

            // 2. 获取函数在本地的偏移
            IntPtr localFuncAddr = GetLocalFunctionOffset(dllFullName, funcName);
            if (localFuncAddr == IntPtr.Zero)
            {
                Console.WriteLine($"无法获取函数 {funcName} 的本地地址");
                return false;
            }

            // 3. 计算远程函数地址
            IntPtr remoteFuncAddr = new IntPtr(remoteDllBase.ToInt64() + localFuncAddr.ToInt64());

            // 4. 打开目标进程
            IntPtr hProcess = OpenProcess(PROCESS_ALL_ACCESS, false, processId);
            if (hProcess == IntPtr.Zero)
            {
                Console.WriteLine($"打开进程失败，错误码：{Marshal.GetLastWin32Error()}");
                return false;
            }

            try
            {
                // 直接传递 parameter 作为线程参数
                IntPtr hThread = CreateRemoteThread(hProcess, IntPtr.Zero, 0, remoteFuncAddr, parameter, 0, out _);
                if (hThread == IntPtr.Zero)
                {
                    Console.WriteLine($"创建远程线程失败，错误码：{Marshal.GetLastWin32Error()}");
                    return false;
                }

                WaitForSingleObject(hThread, 5000);
                CloseHandle(hThread);
                Console.WriteLine($"成功调用 {funcName}");
                return true;
            }
            finally
            {
                CloseHandle(hProcess);
            }
        }

        /// <summary>
        /// 获取本地 DLL 中函数的偏移量（函数地址 - 模块基址）
        /// </summary>
        private static IntPtr GetLocalFunctionOffset(string dllName, string funcName)
        {
            // 方法1：临时加载 DLL 获取函数地址，然后卸载
            IntPtr hModule = LoadLibrary(dllName);
            if (hModule == IntPtr.Zero)
            {
                Console.WriteLine($"无法加载本地 DLL：{dllName}");
                return IntPtr.Zero;
            }

            IntPtr funcAddr = GetProcAddress(hModule, funcName);
            if (funcAddr == IntPtr.Zero)
            {
                FreeLibrary(hModule);
                Console.WriteLine($"本地 DLL 中找不到函数 {funcName}");
                return IntPtr.Zero;
            }

            // 计算偏移
            long offset = funcAddr.ToInt64() - hModule.ToInt64();
            FreeLibrary(hModule);
            return new IntPtr(offset);
        }

        /// <summary>
        /// 获取进程ID
        /// </summary>
        public static int GetProcessIdByName(string processName)
        {
            Process[] processes = Process.GetProcessesByName(processName);
            if (processes.Length > 0)
                return processes[0].Id;
            return 0;
        }
    }
}
