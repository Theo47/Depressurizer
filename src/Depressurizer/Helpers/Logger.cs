/*
    This file is part of Depressurizer.
    Original work Copyright 2017 Martijn Vegter.

    Depressurizer is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    Depressurizer is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with Depressurizer.  If not, see <http://www.gnu.org/licenses/>.
*/

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using Rallion;

namespace Depressurizer.Helpers
{
    public enum LogLevel
    {
        Invalid = 0,
        Trace = 1,
        Debug = 2,
        Info = 3,
        Warn = 4,
        Error = 5,
        Fatal = 6
    }

    public sealed class Logger
    {
        public string LogPath
        {
            get
            {
                string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Depressurizer", "Logs");
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                return path;
            }
        }

        public string LogFile => $"Depressurizer-({DateTime.Now:dd-MM-yyyy}).log";

        public string CurrentLogFile => Path.Combine(LogPath, LogFile);

        public int CurrentFileRecords => new DirectoryInfo(LogPath).GetFiles().Length;

        public static Logger Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (SyncRoot)
                    {
                        if (_instance == null)
                        {
                            _instance = new Logger();
                        }
                    }
                }

                return _instance;
            }
        }

        private static readonly EventWaitHandle WaitHandle = new EventWaitHandle(true, EventResetMode.AutoReset, "Depressurizer");
        private static volatile Logger _instance;
        private static readonly object SyncRoot = new object();

        private Logger()
        {
            foreach (FileInfo file in new DirectoryInfo(LogPath).GetFiles().OrderByDescending(x => x.LastWriteTime).Skip(7))
            {
                file.Delete();
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="logLevel"></param>
        /// <param name="logMessage"></param>
        /// <param name="args"></param>
        public void Write(LogLevel logLevel, string logMessage, params object[] args)
        {
            Instance.Write(logLevel, string.Format(logMessage, args));
        }

        /// <summary>
        /// </summary>
        /// <param name="logLevel"></param>
        /// <param name="logMessage"></param>
        public void Write(LogLevel logLevel, string logMessage)
        {
            lock (SyncRoot)
            {
                WaitHandle.WaitOne();

                StackTrace stackTrace = new StackTrace();

                string senderMethod = stackTrace.GetFrame(2) != null ? (stackTrace.GetFrame(2).GetMethod().Name + ", " + stackTrace.GetFrame(1).GetMethod().Name) : stackTrace.GetFrame(1).GetMethod().Name;
                

                using (FileStream fileStream = new FileStream(CurrentLogFile, FileMode.Append, FileAccess.Write, FileShare.Read))
                {
                    Debug.WriteLine($"{DateTime.Now}  {logLevel,-7} | ({senderMethod}) {logMessage}");

                    byte[] output = new UTF8Encoding().GetBytes($"{DateTime.Now} {logLevel,-7} | {logMessage} {Environment.NewLine}");
                    fileStream.Write(output, 0, output.Length);

                    fileStream.Flush();
                }

                WaitHandle.Set();
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="logMessage"></param>
        /// <param name="e"></param>
        public void WriteException(string logMessage, Exception e)
        {
            Instance.Write(LogLevel.Error, $"{logMessage} ({e})");
        }

        public void WriteException(Exception e)
        {
            Instance.Write(LogLevel.Error, $"Unhandled Exception Thrown ({e})");
        }

        /// <summary>
        /// </summary>
        /// <param name="logLevel"></param>
        /// <param name="logObject"></param>
        /// <param name="logPrefix"></param>
        public void WriteObject(LogLevel logLevel, object logObject, string logPrefix = "")
        {
            Instance.Write(logLevel, $"{logPrefix}{0}", JsonConvert.SerializeObject(logObject));
        }
    }
}