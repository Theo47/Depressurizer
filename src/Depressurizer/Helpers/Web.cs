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
            Logger.Instance.Write(LogLevel.Info, $"Loading: {url}");

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
