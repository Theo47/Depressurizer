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
using System.IO;
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
        public string LogPath { get; set; }
        public string LogFile { get; set; }
        public string ActiveLogFile => Path.Combine(LogPath, LogFile);

        // TODO: Make use of these properties
        public int MaxBackup { get; set; }

        public int MaxFileSize { get; set; }
        public int MaxFileRecords { get; set; }
        public int CurrentFileRecords { get; set; }

        private int FileIndex { get; }

        private Logger()
        {
            FileIndex = 1;

            LogPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Depressurizer");
            LogFile = $"Depressurizer-({DateTime.Now:dd-MM-yyyy})-{FileIndex}.log";
        }

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

        private static volatile Logger _instance;
        private static readonly object SyncRoot = new object();

        /// <summary>
        /// </summary>
        /// <param name="logLevel"></param>
        /// <param name="logMessage"></param>
        /// <param name="args"></param>
        public void Write(LogLevel logLevel, string logMessage, params object[] args)
        {
            Write(logLevel, string.Format(logMessage, args));
        }

        /// <summary>
        /// </summary>
        /// <param name="logLevel"></param>
        /// <param name="logMessage"></param>
        public void Write(LogLevel logLevel, string logMessage)
        {
            if (!Directory.Exists(LogPath))
            {
                Directory.CreateDirectory(LogPath);
            }

            using (StreamWriter streamWriter = new StreamWriter(ActiveLogFile, true))
            {
                streamWriter.WriteLine($"{DateTime.Now} {logLevel} | {logMessage}");
                streamWriter.Close();
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="logMessage"></param>
        /// <param name="e"></param>
        public void WriteException(string logMessage, Exception e)
        {
            Write(LogLevel.Error, $"{logMessage} ({e})");
        }

        /// <summary>
        /// </summary>
        /// <param name="logLevel"></param>
        /// <param name="logObject"></param>
        /// <param name="logPrefix"></param>
        public void WriteObject(LogLevel logLevel, object logObject, string logPrefix = "")
        {
            Write(logLevel, $"{logPrefix}{0}", JsonConvert.SerializeObject(logObject));
        }
    }
}