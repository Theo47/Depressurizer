﻿/*
This file is part of Depressurizer.
Copyright 2011, 2012, 2013 Steve Labbe.

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
using System.Collections.Generic;
using System.Windows.Forms;
using Depressurizer.Helpers;
using NDesk.Options;
using Rallion;

namespace Depressurizer
{
    internal static class Program
    {
        public static GameDB GameDatabase
        {
            get => _gameDatabase;
            set
            {
                Logger.Instance.Info("New value for Program.GameDatabase");
                _gameDatabase = value;
            }
        }

        private static GameDB _gameDatabase;

        /// <summary>
        ///     The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main(string[] args)
        {
            Logger.Instance.Info("Depressurizer Initialized");

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.ApplicationExit += OnApplicationExit;
            //Application.Run(new Form1());

            /* */

            FatalError.InitializeHandler();
            Settings.Instance.Load();

            AutomaticModeOptions autoOpts = ParseAutoOptions(args);

            if (autoOpts != null)
            {
                Logger.Instance.Info("Automatic mode set, loading automatic mode form.");
                //TODO Logger.Instance.WriteObject(LogLevel.Debug, autoOpts, "Automatic Mode Options:");
                Application.Run(new AutomaticModeForm(autoOpts));
            }
            else
            {
                Logger.Instance.Info("Automatic mode not set, loading main form.");
                Application.Run(new FormMain());
            }
            Settings.Instance.Save();

            Logger.Instance.Info(GlobalStrings.Program_ProgramClosing);
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        private static void OnApplicationExit(object sender, EventArgs eventArgs)
        {
            Logger.Instance.Info("Depressurizer Exited");
            Logger.Instance.Dispose();
        }

        /// <summary>
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        private static AutomaticModeOptions ParseAutoOptions(IEnumerable<string> args)
        {
            Logger.Instance.Trace($"ParseAutoOptions({args})");

            AutomaticModeOptions config = new AutomaticModeOptions();
            bool auto = false;

            OptionSet opts = new OptionSet
            {
                {
                    "auto", v => auto = true
                },
                {
                    "p|profile=", v => config.CustomProfile = v
                },
                {
                    "checksteam", v => config.CheckSteam = v != null
                },
                {
                    "closesteam", v => config.CloseSteam = v != null
                },
                {
                    "updatelib", v => config.UpdateGameList = v != null
                },
                {
                    "import", v => config.ImportSteamCategories = v != null
                },
                {
                    "updatedblocal", v => config.UpdateAppInfo = v != null
                },
                {
                    "updatedbhltb", v => config.UpdateHltb = v != null
                },
                {
                    "updatedbweb", v => config.ScrapeUnscrapedGames = v != null
                },
                {
                    "savedb", v => config.SaveDBChanges = v != null
                },
                {
                    "saveprofile", v => config.SaveProfile = v != null
                },
                {
                    "export", v => config.ExportToSteam = v != null
                },
                {
                    "launch", v => config.SteamLaunch = SteamLaunchType.Normal
                },
                {
                    "launchbp", v => config.SteamLaunch = SteamLaunchType.BigPicture
                },
                {
                    "tolerant", v => config.TolerateMinorErrors = v != null
                },
                {
                    "quiet", v => config.AutoClose = AutoCloseType.UnlessError
                },
                {
                    "silent", v => config.AutoClose = AutoCloseType.Always
                },
                {
                    "all", v => config.ApplyAllAutoCats = v != null
                },
                {
                    "<>", v => config.AutoCats.Add(v)
                }
            };

            opts.Parse(args);

            return auto ? config : null;
        }
    }
}