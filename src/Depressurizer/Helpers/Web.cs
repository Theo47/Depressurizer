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
using System.Drawing;
using System.IO;
using System.Net;
using Rallion;

namespace Depressurizer.Helpers
{
    public sealed class Web
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static Image ImageFromStream(string url)
        {
            Program.Logger.Write(LoggerLevel.Trace, $"Web.ImageFromStream({url}) Called");

            return Image.FromStream(GetRemoteImageStream(url));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="url"></param>
        /// <param name="appId"></param>
        /// <returns></returns>
        public static Stream GetRemoteImageStream(string url, int appId = 0)
        {
            Program.Logger.Write(LoggerLevel.Trace, $"Web.GetRemoteImageStream({url}, {appId}) Called");

            Stream imageStream = null;
            try
            {
                using (WebClient webClient = new WebClient())
                {
                    imageStream = new MemoryStream(webClient.DownloadData(url));
                }
            }
            catch (WebException webException)
            {
                if ((webException.Status == WebExceptionStatus.ProtocolError) && (webException.Response != null))
                {
                    HttpWebResponse resp = (HttpWebResponse)webException.Response;
                    if (resp.StatusCode == HttpStatusCode.NotFound)
                    {
                        if (appId != 0)
                        {
                            Program.Logger.Write(LoggerLevel.Warning, $"No Game Banner for: {appId}");
                        }
                        Program.Logger.Write(LoggerLevel.Warning, $"Page not found: {url}");
                        imageStream = null;
                    }
                }
                else
                {
                    Program.Logger.WriteException(url, webException);
                    imageStream = null;
                }
            }
            catch (Exception e)
            {
                Program.Logger.WriteException("Web.GetRemoteImageStream: ", e);
                imageStream = null;
            }

            return imageStream;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="url"></param>
        /// <param name="localPath"></param>
        /// <param name="appId"></param>
        /// <returns></returns>
        public static bool SaveImageFromStream(string url, string localPath, int appId = 0)
        {
            Program.Logger.Write(LoggerLevel.Trace, $"Web.SaveImageFromStream({url}, {localPath}, {appId}) Called");
            bool success = false;

            try
            {
                using (Stream imageStream = GetRemoteImageStream(url, appId))
                {
                    if (imageStream != null)
                    {
                        using (Stream outputStream = File.OpenWrite(localPath))
                        {
                            byte[] buffer = new byte[4096];
                            int bytesRead;

                            do
                            {
                                bytesRead = imageStream.Read(buffer, 0, buffer.Length);
                                outputStream.Write(buffer, 0, bytesRead);
                            } while (bytesRead != 0);
                        }
                    }
                    success = true;
                }
            }
            catch (Exception exception)
            {
                Program.Logger.WriteException("Web.SaveImageFromStream: ", exception);
                success = false;
            }

            return success;
        }
    }
}
