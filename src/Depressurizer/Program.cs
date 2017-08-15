/*
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
using NDesk.Options;
using Rallion;

namespace Depressurizer
{
    internal static class Program
    {
        public static AppLogger Logger = new AppLogger();
        private static GameDB _gameDatabase;
        public static GameDB GameDatabase
        {
            get => _gameDatabase;
            set
            {
                Logger.Write(LoggerLevel.Info, "New value for Program.GameDatabase");
                _gameDatabase = value;
            }
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main(string[] args)
        {
            Logger.Write(LoggerLevel.Trace, $"Main({args})");

            Logger.Write(LoggerLevel.Info, GlobalStrings.Program_ProgramInitialized, Logger.Level);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            //Application.Run(new Form1());

            /* */

            FatalError.InitializeHandler();
            Settings.Instance.Load();

            AutomaticModeOptions autoOpts = ParseAutoOptions(args);

            if (autoOpts != null)
            {
                Logger.Write(LoggerLevel.Info, "Automatic mode set, loading automatic mode form.");
                Logger.WriteObject(LoggerLevel.Verbose, autoOpts, "Automatic Mode Options:");
                Application.Run(new AutomaticModeForm(autoOpts));
            }
            else
            {
                Logger.Write(LoggerLevel.Info, "Automatic mode not set, loading main form.");
                Application.Run(new FormMain());
            }
            Settings.Instance.Save();

            Logger.Write(LoggerLevel.Info, GlobalStrings.Program_ProgramClosing);
            Logger.EndSession();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        private static AutomaticModeOptions ParseAutoOptions(IEnumerable<string> args)
        {
            Logger.Write(LoggerLevel.Trace, $"ParseAutoOptions({args})");

            AutomaticModeOptions config = new AutomaticModeOptions();
            bool auto = false;

            OptionSet opts = new OptionSet
            {
                {"auto", v => auto = true},
                {"p|profile=", v => config.CustomProfile = v},
                {"checksteam", v => config.CheckSteam = (v != null)},
                {"closesteam", v => config.CloseSteam = (v != null)},
                {"updatelib", v => config.UpdateGameList = (v != null)},
                {"import", v => config.ImportSteamCategories = (v != null)},
                {"updatedblocal", v => config.UpdateAppInfo = (v != null)},
                {"updatedbhltb", v => config.UpdateHltb = (v != null)},
                {"updatedbweb", v => config.ScrapeUnscrapedGames = (v != null)},
                {"savedb", v => config.SaveDBChanges = (v != null)},
                {"saveprofile", v => config.SaveProfile = (v != null)},
                {"export", v => config.ExportToSteam = (v != null)},
                {"launch", v => config.SteamLaunch = SteamLaunchType.Normal},
                {"launchbp", v => config.SteamLaunch = SteamLaunchType.BigPicture},
                {"tolerant", v => config.TolerateMinorErrors = (v != null)},
                {"quiet", v => config.AutoClose = AutoCloseType.UnlessError},
                {"silent", v => config.AutoClose = AutoCloseType.Always},
                {"all", v => config.ApplyAllAutoCats = (v != null)},
                {"<>", v => config.AutoCats.Add(v)}
            };

            opts.Parse(args);

            return auto ? config : null;
        }
    }
}