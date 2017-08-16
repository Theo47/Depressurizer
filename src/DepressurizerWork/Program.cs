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
using System.Diagnostics;
using System.Windows.Forms;
using Depressurizer.Helpers;

namespace Depressurizer
{
    /// <summary>
    /// </summary>
    internal static class Program
    {
        /// <summary>
        ///     The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            Logger.Instance.Info("Depressurizer Initialized");

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.ApplicationExit += OnApplicationExit;

            Settings.Instance.Load();

            Application.Run(new FormMainScreen());
        }

        private static void OnApplicationExit(object sender, EventArgs eventArgs)
        {
            Logger.Instance.Info("Depressurizer Exited");
            Logger.Instance.Dispose();
        }
    }
}