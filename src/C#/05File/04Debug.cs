using System;
using System.IO;
using System.Linq;
using System.Diagnostics;

public static class AX {
    public static void CPrint(object X) {
        System.Console.WriteLine(X);
    }
}
namespace CMKZ {
    /// <summary>
    /// Tobo：处理Throw
    /// </summary>
    public static partial class LocalStorage {
#if UNITY_5_3_OR_NEWER || UNITY_5 || UNITY_4 || UNITY_3 || UNITY_2 || UNITY_1
        public static string 堆栈信息 => UnityEngine.StackTraceUtility.ExtractStackTrace();
#else
        public static string 堆栈信息 => new StackTrace().ToString();
#endif
        public static object DefaultSender = "Debug";
        public static CMKZList<LogData> LogList = new();
        public static StreamWriter LogFileWriter;
        public static bool IsLogQuit = false;
        public static int MaxLogFileCount = 1000;
        public static int MinLogFileCount = 100;
        public static LogLevel LogLevel = LogLevel.Debug;
        public static object ConsoleSystem = nameof(ConsoleSystem);
        public static event Action<LogData> OnPrint = t => {
#if UNITY_5_3_OR_NEWER || UNITY_5 || UNITY_4 || UNITY_3 || UNITY_2 || UNITY_1
            if (t.Level == LogLevel.Error) {
                UnityEngine.Debug.LogError(t.Message);
            } else if (t.Level == LogLevel.Warning) {
                UnityEngine.Debug.LogWarning(t.Message);
            } else {
                UnityEngine.Debug.Log(t.Message);
            }
#endif
            AX.CPrint(t.Message);
            LogList.Add(t);
            LogToFile(t);
        };
        public static void Print(object X, object Sender = null) {
            if (LogLevel > LogLevel.Debug) return;
            OnPrint(new LogData {
                Level = LogLevel.Debug,
                Message = X == null ? "null" : X.ToString(),
                Time = DateTime.Now,
                Sender = Sender ?? DefaultSender,
                Stack = 堆栈信息,
            });
        }
        public static string Read() {
            return System.Console.ReadLine();
        }
        public static void PrintWarning(string X, object Sender = null) {
            if (LogLevel > LogLevel.Warning) return;
            OnPrint(new LogData {
                Level = LogLevel.Warning,
                Message = X,
                Time = DateTime.Now,
                Sender = Sender ?? DefaultSender,
                Stack = 堆栈信息,
            });
        }
        public static void PrintError(string X, object Sender = null) {
            if (LogLevel > LogLevel.Error) return;
            OnPrint(new LogData {
                Level = LogLevel.Error,
                Message = X,
                Time = DateTime.Now,
                Sender = Sender ?? DefaultSender,
                Stack = 堆栈信息,
            });
        }
        public static void PrintSystem(string X) {
            Print(X, ConsoleSystem);
        }
        public static void LogToFile(LogData X) {
            if (IsLogQuit) {
#if UNITY_5_3_OR_NEWER || UNITY_5 || UNITY_4 || UNITY_3 || UNITY_2 || UNITY_1
                UnityEngine.Debug.Log("程序已退出，Print无效");
#endif
                return;
            }
            if (LogFileWriter == null) {
                var LogFiles = GetFiles("Log".绝对路径());
                if (LogFiles.Length > MaxLogFileCount) {
                    var A = LogFiles.Select(t => new FileInfo(t)).OrderBy(t => t.CreationTime).ToList_CMKZ();
                    for (int i = 0; i < A.Count - MinLogFileCount; i++) {
                        A[i].Delete();
                    }
                }
                LogFileWriter = GetWriter("Log/" + NowTimeWithUnderLine + ".log");
                PhoneSystemInfo(LogFileWriter);
#if UNITY_5_3_OR_NEWER || UNITY_5 || UNITY_4 || UNITY_3 || UNITY_2 || UNITY_1
                OnAppQuit(() => {
                    IsLogQuit = true;
                    LogFileWriter.Flush();
                    LogFileWriter.Close();
                    LogFileWriter.Dispose();
                });
#else
                AppDomain.CurrentDomain.ProcessExit += (s, e) => {
                    IsLogQuit = true;
                    LogFileWriter.Flush();
                    LogFileWriter.Close();
                    LogFileWriter.Dispose();
                };
#endif
            }
            LogFileWriter.WriteLine(X.Level.ToString() + ":" + X.Message + "\n" + X.Stack);
        }
        public static void PhoneSystemInfo(StreamWriter X) {
            X.WriteLine("********************************");
            X.WriteLine(NowTime);
#if UNITY_5_3_OR_NEWER || UNITY_5 || UNITY_4 || UNITY_3 || UNITY_2 || UNITY_1
            X.WriteLine("CPU：" + UnityEngine.SystemInfo.processorType + " " + UnityEngine.SystemInfo.processorCount + "核");
            X.WriteLine("显卡：" + UnityEngine.SystemInfo.graphicsDeviceName + " " + (UnityEngine.SystemInfo.graphicsMemorySize / 1024 + 1) + "GB");
            X.WriteLine("内存：" + (UnityEngine.SystemInfo.systemMemorySize / 1024 + 1) + "GB");
            X.WriteLine("操作系统：" + UnityEngine.SystemInfo.operatingSystem);
#endif
            foreach (var i in DriveInfo.GetDrives()) {
                if (i.IsReady && i.DriveType == DriveType.Fixed) {
                    var A = BToGB(i.TotalSize);
                    var B = BToGB(i.TotalFreeSpace);
                    var C = (1 - B / A) * 100;
                    X.WriteLine($"硬盘 {i.Name} {A:F0}GB 已用{C:F0}%");
                }
            }
            X.WriteLine("********************************");
            X.WriteLine();
        }
    }
    public enum LogLevel {
        Debug,
        Warning,
        Error,
    }
    public class LogData {
        public LogLevel Level;
        public object Sender;
        public string Message;
        public string Stack;
        public DateTime Time;
    }
}