/*
    This file is part of Depressurizer.
    Copyright (C) 2017 Martijn Vegter

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using Depressurizer.Properties;

namespace Depressurizer.Helpers
{
    public sealed class Steam
    {
        /// <summary>
        /// </summary>
        /// <param name="appId"></param>
        /// TODO Add proper error handling
        /// TODO Add unit test
        public static void LaunchStorePage(int appId)
        {
            Logger.Instance.Write(LogLevel.Trace, $"Steam.LaunchStorePage({appId}) Called");

            Process steamProcess = new Process();
            try
            {
                steamProcess.StartInfo.UseShellExecute = true;
                steamProcess.StartInfo.FileName = string.Format(Resources.UrlSteamStoreApp, appId);
                steamProcess.Start();
            }
            catch (Exception exception)
            {
                Logger.Instance.WriteException(exception);
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="steamId64"></param>
        /// <returns>
        ///     Success: Returns user avatar
        ///     Failure: Returns null
        /// </returns>
        /// TODO Add proper error handling
        /// TODO Add unit test
        public static Image GetAvatar(long steamId64)
        {
            Logger.Instance.Write(LogLevel.Trace, $"Steam.GetAvatar({steamId64}) Called");

            Image steamAvatar = null;

            XmlDocument xmlDocument = XmlParser.Load(Resources.UrlSteamProfile, steamId64);
            if (xmlDocument.DocumentElement != null)
            {
                XmlNode xmlNode = xmlDocument.DocumentElement.SelectSingleNode(Resources.XmlNodeAvatar);
                if (xmlNode != null)
                {
                    string steamAvatarLink = xmlNode.InnerText;
                    steamAvatar = Web.ImageFromStream(steamAvatarLink);
                }
            }

            return steamAvatar;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// TODO: Add proper error handling
        /// TODO: Improve / extend existing Unit Tests
        public static XmlDocument FetchAppList()
        {
            Logger.Instance.Write(LogLevel.Trace, "Steam.FetchAppList() Called");

            return XmlParser.Load(@"http://api.steampowered.com/ISteamApps/GetAppList/v0002/?format=xml");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="appId"></param>
        /// <returns></returns>
        public static bool FetchBanner(int appId)
        {
            Logger.Instance.Write(LogLevel.Trace, $"Steam.FetchBanner({appId}) Called");

            bool success = false;

            string bannerUrl = string.Format(Resources.UrlGameBanner, appId);
            string bannerPath = string.Format(Resources.GameBannerPath, Path.GetDirectoryName(Application.ExecutablePath), appId);

            try
            {
                if (!Directory.Exists(Path.GetDirectoryName(bannerPath)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(bannerPath));
                }

                success = Web.SaveImageFromStream(bannerUrl, bannerPath, appId);
            }
            catch (Exception e)
            {
                Logger.Instance.WriteException(e);
                success = false;
            }

            return success;
        }
    }
}