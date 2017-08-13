﻿/*
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
using Rallion;

namespace Depressurizer.Helpers
{
    public class XmlParser
    {
        public static XmlDocument Load(string xmlPath)
        {
            Program.Logger.Write(LoggerLevel.Info, $"Loading: {xmlPath}");

            XmlDocument xmlDocument = new XmlDocument();
            bool parsingSucceeded = false;

            try
            {
                xmlDocument.Load(xmlPath);
                parsingSucceeded = true;
            }
            catch (WebException ex)
            {
                Debug.WriteLine(ex);

                if ((ex.Status == WebExceptionStatus.ProtocolError) && (ex.Response != null))
                {
                    HttpWebResponse resp = (HttpWebResponse)ex.Response;
                    if (resp.StatusCode == HttpStatusCode.NotFound)
                    {
                        Program.Logger.Write(LoggerLevel.Error, $"Invalid XmlPath supplied: {xmlPath}");
                        xmlDocument = null;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);

                if (!parsingSucceeded)
                {
                    Program.Logger.Write(LoggerLevel.Error, $"Error while parsing: {xmlPath}");
                    xmlDocument = null;
                }
                else
                {
                    Program.Logger.Write(LoggerLevel.Error, $"Unknown Exception: {ex}");
                    throw new Exception(ex.Message);
                }
            }

            return xmlDocument;
        }

        public static XmlDocument Load(string xmlPath, params object[] args)
        {
            return Load(string.Format(xmlPath, args));
        }
    }
}