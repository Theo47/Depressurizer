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
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

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
            Logger.Instance.Write(LogLevel.Info, $"Loading image: {url}");

            Image image = null;

            using (WebClient webClient = new WebClient())
            {
                using (MemoryStream imageStream = new MemoryStream(webClient.DownloadData(url)))
                {
                    image = Image.FromStream(imageStream);
                }
            }

            return image;
        }
    }
}
