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
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Net.Cache;
using System.Threading.Tasks;
using System.Xml;

namespace Depressurizer.Helpers
{
    /// <summary>
    ///     Steam Helper Class
    /// </summary>
    public static class Steam
    {
        /// <summary>
        /// </summary>
        public static string BannerFolder
        {
            get
            {
                string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Depressurizer", "Banners");
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                return path;
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="appId"></param>
        /// <returns></returns>
        public static string BannerFile(int appId)
        {
            return Path.Combine(BannerFolder, string.Format(CultureInfo.InvariantCulture, "{0}.jpg", appId));
        }

        /// <summary>
        /// </summary>
        /// <param name="steamId64"></param>
        /// <returns></returns>
        /// TODO: Proper error handling
        public static Image GetAvatar(long steamId64)
        {
            Image avatar = null;

            try
            {
                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.Load(string.Format(CultureInfo.InvariantCulture, "http://steamcommunity.com/profiles/{0}?xml=1", steamId64));

                XmlNode avatarNode = xmlDocument.SelectSingleNode("/profile/avatarIcon");
                if (avatarNode != null)
                {
                    avatar = Utility.GetImage(avatarNode.InnerText, RequestCacheLevel.BypassCache);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            return avatar;
        }

        /// <summary>
        /// </summary>
        /// <param name="games"></param>
        public static async void GrabBanners(List<GameInfo> games)
        {
            await Task.Run(() =>
            {
                Parallel.ForEach(games, game =>
                {
                    if (game.Id < 0)
                    {
                        return;
                    }

                    if (!File.Exists(BannerFile(game.Id)))
                    {
                        FetchBanner(game.Id);
                    }
                });
            });
        }

        /// <summary>
        ///     Runs an application. It will be installed if necessary.
        /// </summary>
        /// <param name="appId"></param>
        public static void LaunchApp(int appId)
        {
            if (appId < 0)
            {
                return;
            }

            ExecuteBrowser("steam://run/{0}", appId);
        }

        /// <summary>
        ///     Opens up the store for an app, if no app is specified then the default one is opened.
        /// </summary>
        /// <param name="appId"></param>
        public static void LaunchStorePage(int appId)
        {
            if (appId < 0)
            {
                return;
            }

            ExecuteBrowser("steam://store/{0}", appId);
        }

        /// <summary>
        /// </summary>
        /// <param name="url"></param>
        /// <param name="args"></param>
        private static void ExecuteBrowser(string url, params object[] args)
        {
            ExecuteBrowser(string.Format(CultureInfo.InvariantCulture, url, args));
        }

        /// <summary>
        /// </summary>
        /// <param name="url"></param>
        private static void ExecuteBrowser(string url)
        {
            Process.Start(url);
        }

        /// <summary>
        /// </summary>
        /// <param name="appId"></param>
        /// <returns></returns>
        private static void FetchBanner(int appId)
        {
            if (appId < 0)
            {
                return;
            }

            if (File.Exists(BannerFile(appId)))
            {
                return;
            }

            string bannerLink = string.Format(CultureInfo.InvariantCulture, "https://steamcdn-a.akamaihd.net/steam/apps/{0}/capsule_sm_120.jpg", appId);
            bool success = Utility.SaveRemoteImageToFile(bannerLink, BannerFile(appId), appId);
            if (!success)
            {
                // TODO: Add error
            }
        }
    }
}