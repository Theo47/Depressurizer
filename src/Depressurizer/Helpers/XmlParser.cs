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
using System.Net;
using System.Xml;

namespace Depressurizer.Helpers
{
    public sealed class XmlParser
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="xmlPath"></param>
        /// <returns></returns>
        public static XmlDocument Load(string xmlPath)
        {
            Logger.Instance.Write(LogLevel.Trace, $"XmlParser.Load({xmlPath}) Called");

            XmlDocument xmlDocument = new XmlDocument();
            bool parsingSucceeded = false;

            try
            {
                xmlDocument.Load(xmlPath);
                parsingSucceeded = true;
            }
            catch (WebException webException)
            {
                Logger.Instance.WriteException(webException);

                if ((webException.Status == WebExceptionStatus.ProtocolError) && (webException.Response != null))
                {
                    HttpWebResponse resp = (HttpWebResponse)webException.Response;
                    if (resp.StatusCode == HttpStatusCode.NotFound)
                    {
                        Logger.Instance.Write(LogLevel.Error, $"Invalid XmlPath supplied: {xmlPath}");
                        xmlDocument = null;
                    }
                }
            }
            catch (Exception ex)
            {
                if (!parsingSucceeded)
                {
                    Logger.Instance.Write(LogLevel.Error, $"Error while parsing: {xmlPath}");
                }
                else
                {
                    Logger.Instance.WriteException(ex);
                }
                xmlDocument = null;
            }

            return xmlDocument;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="xmlPath"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static XmlDocument Load(string xmlPath, params object[] args)
        {
            Logger.Instance.Write(LogLevel.Trace, $"XmlParser.Load({xmlPath}, {args}) Called");

            return Load(string.Format(xmlPath, args));
        }
    }
}