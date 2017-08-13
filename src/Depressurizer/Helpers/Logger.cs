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
using System.Threading;
using Newtonsoft.Json;

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

    public class Logger
    {
        public string LogPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Depressurizer", "Logs");
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
            if (!Directory.Exists(LogPath))
            {
                Directory.CreateDirectory(LogPath);
            }

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
              
                Debug.WriteLine($"{logLevel,-7} | {logMessage}");

                using (StreamWriter streamWriter = new StreamWriter(CurrentLogFile, true))
                {
                    streamWriter.WriteLine($"{DateTime.Now} {logLevel,-7} | {logMessage}");
                    streamWriter.Close();
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