#region GNU GENERAL PUBLIC LICENSE

// 
// This file is part of Depressurizer.
// Copyright (C) 2017 Martijn Vegter
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.If not, see<http://www.gnu.org/licenses/>.
// 

#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace Depressurizer.Helpers
{
    /// <summary>
    /// </summary>
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

    /// <summary>
    /// </summary>
    internal sealed class Logger
    {
        /// <summary>
        /// </summary>
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

        /// <summary>
        /// </summary>
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

        /// <summary>
        /// </summary>
        public string LogFile => $"Depressurizer-({DateTime.Now:dd-MM-yyyy}).log";

        /// <summary>
        /// </summary>
        public string ActiveLogFile => Path.Combine(LogPath, LogFile);

        /// <summary>
        /// </summary>
        private FileStream _outputStream;

        /// <summary>
        /// </summary>
        private Logger()
        {
            Info("Logger Instance Initialized");
            _outputStream = new FileStream(ActiveLogFile, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
        }

        /// <summary>
        /// </summary>
        private static readonly Queue<string> LogQueue = new Queue<string>();

        /// <summary>
        /// </summary>
        private static volatile Logger _instance;

        /// <summary>
        /// </summary>
        private static readonly object SyncRoot = new object();

        /// <summary>
        /// </summary>
        private static DateTime _lastFlushed = DateTime.Now;

        /// <summary>
        /// </summary>
        /// <param name="logMessage"></param>
        public void Debug(string logMessage)
        {
            Write(LogLevel.Debug, logMessage);
        }

        /// <summary>
        /// </summary>
        /// <param name="logMessage"></param>
        /// <param name="args"></param>
        public void Debug(string logMessage, params object[] args)
        {
            Write(LogLevel.Debug, string.Format(logMessage, args));
        }

        /// <summary>
        /// </summary>
        /// <param name="logMessage"></param>
        public void Info(string logMessage)
        {
            Write(LogLevel.Info, logMessage);
        }

        /// <summary>
        /// </summary>
        /// <param name="logMessage"></param>
        /// <param name="args"></param>
        public void Info(string logMessage, params object[] args)
        {
            Write(LogLevel.Info, string.Format(logMessage, args));
        }

        /// <summary>
        /// </summary>
        /// <param name="logLevel"></param>
        /// <param name="logMessage"></param>
        private void Write(LogLevel logLevel, string logMessage)
        {
            lock (SyncRoot)
            {
                string logEntry = $"{DateTime.Now}  {logLevel,-7} | {logMessage}";
                System.Diagnostics.Debug.WriteLine(logEntry);
                LogQueue.Enqueue(logEntry);

                if ((LogQueue.Count >= 100) || DoPeriodicFlush())
                {
                    FlushLog();
                }
            }
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        private static bool DoPeriodicFlush()
        {
            bool doPeriodicFlush = false;

            TimeSpan logAge = DateTime.Now - _lastFlushed;
            if (logAge.TotalSeconds >= 60)
            {
                _lastFlushed = DateTime.Now;
                doPeriodicFlush = true;
            }

            return doPeriodicFlush;
        }

        /// <summary>
        /// </summary>
        /// TODO: Handle Exception
        public void FlushLog()
        {
            lock (SyncRoot)
            {
                try
                {
                    while (LogQueue.Count > 0)
                    {
                        string logEntry = LogQueue.Dequeue();
                        byte[] output = new UTF8Encoding().GetBytes(logEntry + Environment.NewLine);
                        _outputStream.Write(output, 0, output.Length);
                    }
                }
                catch (Exception exception)
                {
                    System.Diagnostics.Debug.WriteLine(exception);
                    throw;
                }
            }
        }

        /// <summary>
        /// </summary>
        public void Dispose()
        {
            lock (SyncRoot)
            {
                FlushLog();

                byte[] output = new UTF8Encoding().GetBytes(Environment.NewLine);
                _outputStream.Write(output, 0, output.Length);

                _outputStream.Flush();
                _outputStream.Flush(true);
                _outputStream.Dispose();
                _outputStream.Close();
                _outputStream = null;
            }
        }
    }
}