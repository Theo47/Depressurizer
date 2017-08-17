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
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Xml;
using Depressurizer.Helpers;

namespace Depressurizer.Model
{
    /// <summary>
    /// </summary>
    public class GameDatabase
    {
        /// <summary>
        /// </summary>
        public SortedSet<string> AllStoreDevelopers
        {
            get
            {
                if (_allStoreDevelopers == null)
                {
                    _allStoreDevelopers = new SortedSet<string>();
                }

                return _allStoreDevelopers;
            }
        }

        /// <summary>
        /// </summary>
        public SortedSet<string> AllStoreFlags
        {
            get
            {
                if (_allStoreFlags == null)
                {
                    _allStoreFlags = new SortedSet<string>();
                }

                return _allStoreFlags;
            }
        }

        /// <summary>
        /// </summary>
        public SortedSet<string> AllStoreGenres
        {
            get
            {
                if (_allStoreGenres == null)
                {
                    _allStoreGenres = new SortedSet<string>();
                }

                return _allStoreGenres;
            }
        }

        /// <summary>
        /// </summary>
        public SortedSet<string> AllStorePublishers
        {
            get
            {
                if (_allStorePublishers == null)
                {
                    _allStorePublishers = new SortedSet<string>();
                }

                return _allStorePublishers;
            }
        }

        public StoreLanguage DatabaseLanguage { get; set; } = StoreLanguage.en;

        /// <summary>
        /// </summary>
        public Dictionary<int, GameDBEntry> Games
        {
            get
            {
                if (_games == null)
                {
                    _games = new Dictionary<int, GameDBEntry>();
                }

                return _games;
            }
        }

        /// <summary>
        /// </summary>
        /// <see cref="AllStoreDevelopers" />
        private SortedSet<string> _allStoreDevelopers;

        /// <summary>
        /// </summary>
        /// <see cref="AllStoreFlags" />
        private SortedSet<string> _allStoreFlags;

        /// <summary>
        /// </summary>
        /// <see cref="AllStoreGenres" />
        private SortedSet<string> _allStoreGenres;

        /// <summary>
        /// </summary>
        /// <see cref="AllStorePublishers" />
        private SortedSet<string> _allStorePublishers;

        /// <summary>
        /// </summary>
        /// <see cref="Games" />
        private Dictionary<int, GameDBEntry> _games;

        public int LastHltbUpdate;

        private const int VERSION = 1;
        private const string XmlName_Version = "version";
        private const string XmlName_LastHltbUpdate = "lastHltbUpdate";
        private const string XmlName_dbLanguage = "dbLanguage";
        private const string XmlName_GameList = "gamelist";
        private const string XmlName_Game = "game";
        private const string XmlName_Game_Id = "id";
        private const string XmlName_Game_Name = "name";
        private const string XmlName_Game_LastStoreUpdate = "lastStoreUpdate";
        private const string XmlName_Game_LastAppInfoUpdate = "lastAppInfoUpdate";
        private const string XmlName_Game_Type = "type";
        private const string XmlName_Game_Platforms = "platforms";
        private const string XmlName_Game_Parent = "parent";
        private const string XmlName_Game_Genre = "genre";
        private const string XmlName_Game_Tag = "tag";
        private const string XmlName_Game_Achievements = "achievements";
        private const string XmlName_Game_Developer = "developer";
        private const string XmlName_Game_Publisher = "publisher";
        private const string XmlName_Game_Flag = "flag";
        private const string XmlName_Game_ReviewTotal = "reviewTotal";
        private const string XmlName_Game_ReviewPositivePercent = "reviewPositiveP";
        private const string XmlName_Game_MCUrl = "mcUrl";
        private const string XmlName_Game_Date = "steamDate";
        private const string XmlName_Game_HltbMain = "hltbMain";
        private const string XmlName_Game_HltbExtras = "hltbExtras";
        private const string XmlName_Game_HltbCompletionist = "hltbCompletionist";
        private const string XmlName_Game_vrSupport = "vrSupport";
        private const string XmlName_Game_vrSupport_Headsets = "Headset";
        private const string XmlName_Game_vrSupport_Input = "Input";
        private const string XmlName_Game_vrSupport_PlayArea = "PlayArea";
        private const string XmlName_Game_languageSupport = "languageSupport";
        private const string XmlName_Game_languageSupport_Interface = "Headset";
        private const string XmlName_Game_languageSupport_FullAudio = "Input";
        private const string XmlName_Game_languageSupport_Subtitles = "PlayArea";

        /// <summary>
        /// </summary>
        /// <returns></returns>
        public SortedSet<string> CalculateAllGenres()
        {
            lock (AllStoreGenres)
            {
                AllStoreGenres.Clear();

                foreach (GameDBEntry gameEntry in Games.Values)
                {
                    if (gameEntry.Genres != null)
                    {
                        AllStoreGenres.UnionWith(gameEntry.Genres);
                    }
                }
            }

            return AllStoreGenres;
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        public SortedSet<string> CalculateAllPublishers()
        {
            lock (AllStorePublishers)
            {
                AllStorePublishers.Clear();

                foreach (GameDBEntry gameEntry in Games.Values)
                {
                    if (gameEntry.Publishers != null)
                    {
                        AllStorePublishers.UnionWith(gameEntry.Publishers);
                    }
                }
            }

            return AllStorePublishers;
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        public SortedSet<string> CalculateAllStoreFlags()
        {
            lock (AllStoreFlags)
            {
                AllStoreFlags.Clear();

                foreach (GameDBEntry gameEntry in Games.Values)
                {
                    if (gameEntry.Flags != null)
                    {
                        AllStoreFlags.UnionWith(gameEntry.Flags);
                    }
                }
            }

            return AllStoreFlags;
        }

        /// <summary>
        /// </summary>
        private void ClearAll()
        {
            lock (this)
            {
                AllStoreDevelopers.Clear();
                AllStoreFlags.Clear();
                AllStoreGenres.Clear();
                AllStorePublishers.Clear();

                Games.Clear();
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public int UpdateFromAppInfo(string path)
        {
            int appsUpdated = 0;

            Dictionary<int, AppInfo> appInfos = LoadApps(path);
            foreach (AppInfo appInfo in appInfos.Values)
            {
                GameDBEntry gameEntry;
                if (Contains(appInfo.Id))
                {
                    gameEntry = Games[appInfo.Id];
                }
                else
                {
                    gameEntry = new GameDBEntry
                    {
                        Id = appInfo.Id
                    };
                    Games.Add(gameEntry.Id, gameEntry);
                }

                gameEntry.LastAppInfoUpdate = Utility.GetCurrentUTime();

                if (appInfo.AppType != AppTypes.Unknown)
                {
                    gameEntry.AppType = appInfo.AppType;
                }

                if (!string.IsNullOrEmpty(appInfo.Name))
                {
                    gameEntry.Name = appInfo.Name;
                }

                if ((gameEntry.Platforms == AppPlatforms.None) || ((gameEntry.LastStoreScrape == 0) && (appInfo.Platforms > AppPlatforms.None)))
                {
                    gameEntry.Platforms = appInfo.Platforms;
                }

                if (appInfo.Parent > 0)
                {
                    gameEntry.ParentId = appInfo.Parent;
                }

                appsUpdated++;
            }

            return appsUpdated;
        }

        /// <summary>
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private static Dictionary<int, AppInfo> LoadApps(string path)
        {
            Dictionary<int, AppInfo> result = new Dictionary<int, AppInfo>();

            try
            {
                using (FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read))
                {
                    using (BinaryReader bReader = new BinaryReader(fileStream))
                    {
                        long fileLength = bReader.BaseStream.Length;

                        // seek to common: start of a new entry
                        byte[] start =
                        {
                            0x00, 0x00, 0x63, 0x6F, 0x6D, 0x6D, 0x6F, 0x6E, 0x00
                        }; // 0x00 0x00 c o m m o n 0x00

                        VdfFileNode.ReadBin_SeekTo(bReader, start, fileLength);

                        VdfFileNode node = VdfFileNode.LoadFromBinary(bReader, fileLength);
                        while (node != null)
                        {
                            AppInfo app = AppInfo.Create(node);
                            if (app != null)
                            {
                                result.Add(app.Id, app);
                            }
                            VdfFileNode.ReadBin_SeekTo(bReader, start, fileLength);
                            node = VdfFileNode.LoadFromBinary(bReader, fileLength);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                result = null;
                Console.WriteLine(e);
                throw;
            }

            return result;
        }

        /// <summary>
        /// </summary>
        /// <param name="appId"></param>
        /// <returns></returns>
        public bool Contains(int appId) => Games.ContainsKey(appId);

        /// <summary>
        /// </summary>
        /// TODO: Change path
        public bool Save()
        {
            Logger.Instance.Info("Saving GameDatabase");

            try
            {
                using (Stream fileStream = new FileStream("GameDB.xml.gz", FileMode.Create))
                {
                    using (Stream zipStream = new GZipStream(fileStream, CompressionMode.Compress))
                    {
                        XmlWriterSettings settings = new XmlWriterSettings
                        {
                            Indent = true,
                            CloseOutput = true
                        };

                        using (XmlWriter writer = XmlWriter.Create(zipStream, settings))
                        {
                            writer.WriteStartDocument();
                            writer.WriteStartElement(XmlName_GameList);

                            writer.WriteElementString(XmlName_Version, VERSION.ToString());

                            writer.WriteElementString(XmlName_LastHltbUpdate, LastHltbUpdate.ToString());

                            writer.WriteElementString(XmlName_dbLanguage, Enum.GetName(typeof(StoreLanguage), DatabaseLanguage));

                            foreach (GameDBEntry gameEntry in Games.Values)
                            {
                                writer.WriteStartElement(XmlName_Game);

                                writer.WriteElementString(XmlName_Game_Id, gameEntry.Id.ToString());

                                if (!string.IsNullOrEmpty(gameEntry.Name))
                                {
                                    writer.WriteElementString(XmlName_Game_Name, gameEntry.Name);
                                }

                                if (gameEntry.LastStoreScrape > 0)
                                {
                                    writer.WriteElementString(XmlName_Game_LastStoreUpdate, gameEntry.LastStoreScrape.ToString());
                                }
                                if (gameEntry.LastAppInfoUpdate > 0)
                                {
                                    writer.WriteElementString(XmlName_Game_LastAppInfoUpdate, gameEntry.LastAppInfoUpdate.ToString());
                                }

                                writer.WriteElementString(XmlName_Game_Type, gameEntry.AppType.ToString());

                                writer.WriteElementString(XmlName_Game_Platforms, gameEntry.Platforms.ToString());

                                if (gameEntry.ParentId >= 0)
                                {
                                    writer.WriteElementString(XmlName_Game_Parent, gameEntry.ParentId.ToString());
                                }

                                if (gameEntry.Genres != null)
                                {
                                    foreach (string str in gameEntry.Genres)
                                    {
                                        writer.WriteElementString(XmlName_Game_Genre, str);
                                    }
                                }

                                if (gameEntry.Tags != null)
                                {
                                    foreach (string str in gameEntry.Tags)
                                    {
                                        writer.WriteElementString(XmlName_Game_Tag, str);
                                    }
                                }

                                if (gameEntry.Developers != null)
                                {
                                    foreach (string str in gameEntry.Developers)
                                    {
                                        writer.WriteElementString(XmlName_Game_Developer, str);
                                    }
                                }

                                if (gameEntry.Publishers != null)
                                {
                                    foreach (string str in gameEntry.Publishers)
                                    {
                                        writer.WriteElementString(XmlName_Game_Publisher, str);
                                    }
                                }

                                if (gameEntry.Flags != null)
                                {
                                    foreach (string s in gameEntry.Flags)
                                    {
                                        writer.WriteElementString(XmlName_Game_Flag, s);
                                    }
                                }

                                //vr support
                                writer.WriteStartElement(XmlName_Game_vrSupport);
                                if (gameEntry.vrSupport.Headsets != null)
                                {
                                    foreach (string str in gameEntry.vrSupport.Headsets)
                                    {
                                        writer.WriteElementString(XmlName_Game_vrSupport_Headsets, str);
                                    }
                                }

                                if (gameEntry.vrSupport.Input != null)
                                {
                                    foreach (string str in gameEntry.vrSupport.Input)
                                    {
                                        writer.WriteElementString(XmlName_Game_vrSupport_Input, str);
                                    }
                                }

                                if (gameEntry.vrSupport.PlayArea != null)
                                {
                                    foreach (string str in gameEntry.vrSupport.PlayArea)
                                    {
                                        writer.WriteElementString(XmlName_Game_vrSupport_PlayArea, str);
                                    }
                                }

                                writer.WriteEndElement();

                                //language support
                                writer.WriteStartElement(XmlName_Game_languageSupport);
                                if (gameEntry.languageSupport.Interface != null)
                                {
                                    foreach (string str in gameEntry.languageSupport.Interface)
                                    {
                                        writer.WriteElementString(XmlName_Game_languageSupport_Interface, str);
                                    }
                                }

                                if (gameEntry.languageSupport.FullAudio != null)
                                {
                                    foreach (string str in gameEntry.languageSupport.FullAudio)
                                    {
                                        writer.WriteElementString(XmlName_Game_languageSupport_FullAudio, str);
                                    }
                                }

                                if (gameEntry.languageSupport.Subtitles != null)
                                {
                                    foreach (string str in gameEntry.languageSupport.Subtitles)
                                    {
                                        writer.WriteElementString(XmlName_Game_languageSupport_Subtitles, str);
                                    }
                                }

                                writer.WriteEndElement();

                                if (gameEntry.Achievements > 0)
                                {
                                    writer.WriteElementString(XmlName_Game_Achievements, gameEntry.Achievements.ToString());
                                }

                                if (gameEntry.ReviewTotal > 0)
                                {
                                    writer.WriteElementString(XmlName_Game_ReviewTotal, gameEntry.ReviewTotal.ToString());
                                    writer.WriteElementString(XmlName_Game_ReviewPositivePercent, gameEntry.ReviewPositivePercentage.ToString());
                                }

                                if (!string.IsNullOrEmpty(gameEntry.MC_Url))
                                {
                                    writer.WriteElementString(XmlName_Game_MCUrl, gameEntry.MC_Url);
                                }

                                if (!string.IsNullOrEmpty(gameEntry.SteamReleaseDate))
                                {
                                    writer.WriteElementString(XmlName_Game_Date, gameEntry.SteamReleaseDate);
                                }

                                if (gameEntry.HltbMain > 0)
                                {
                                    writer.WriteElementString(XmlName_Game_HltbMain, gameEntry.HltbMain.ToString());
                                }

                                if (gameEntry.HltbExtras > 0)
                                {
                                    writer.WriteElementString(XmlName_Game_HltbExtras, gameEntry.HltbExtras.ToString());
                                }

                                if (gameEntry.HltbCompletionist > 0)
                                {
                                    writer.WriteElementString(XmlName_Game_HltbCompletionist, gameEntry.HltbCompletionist.ToString());
                                }

                                writer.WriteEndElement();
                            }

                            writer.WriteEndElement();
                            writer.WriteEndDocument();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            Logger.Instance.Info("Saved GameDatabase");

            return true;
        }

        /// <summary>
        /// </summary>
        /// TODO: Change path
        /// TODO: Improve null handling
        public bool Load()
        {
            Logger.Instance.Info("Loading GameDatabase");

            try
            {
                if (!File.Exists("GameDB.xml.gz"))
                {
                    // TODO
                }

                XmlDocument doc = new XmlDocument();
                using (Stream fileStream = new FileStream("GameDB.xml.gz", FileMode.Open))
                {
                    using (Stream zipStream = new GZipStream(fileStream, CompressionMode.Decompress))
                    {
                        doc.Load(zipStream);
                    }
                }

                Logger.Instance.Info("Parsed GameDatabase");

                XmlNode gameListNode = doc.SelectSingleNode("/" + XmlName_GameList);
                if (gameListNode == null)
                {
                    Logger.Instance.Warn("Empty or Invalid format GameDatabase");
                }
                else
                {
                    XmlNodeList xmlNodeList = gameListNode.SelectNodes(XmlName_Game);
                    if (xmlNodeList != null)
                    {
                        lock (this)
                        {
                            ClearAll();

                            LastHltbUpdate = XmlUtil.GetIntFromNode(gameListNode[XmlName_LastHltbUpdate], 0);
                            DatabaseLanguage = (StoreLanguage)Enum.Parse(typeof(StoreLanguage), XmlUtil.GetStringFromNode(gameListNode[XmlName_dbLanguage], "en"), true);

                            int fileVersion = XmlUtil.GetIntFromNode(gameListNode[XmlName_Version], 0);

                            foreach (XmlNode gameNode in xmlNodeList)
                            {
                                if (!XmlUtil.TryGetIntFromNode(gameNode[XmlName_Game_Id], out int id) || Games.ContainsKey(id))
                                {
                                    continue;
                                }

                                GameDBEntry g = new GameDBEntry
                                {
                                    Id = id,
                                    Name = XmlUtil.GetStringFromNode(gameNode[XmlName_Game_Name], null)
                                };

                                if (fileVersion < 1)
                                {
                                    g.AppType = AppTypes.Unknown;
                                    if (XmlUtil.TryGetStringFromNode(gameNode[XmlName_Game_Type], out string typeString))
                                    {
                                        switch (typeString)
                                        {
                                            case "DLC":
                                                g.AppType = AppTypes.DLC;
                                                break;
                                            case "Game":
                                                g.AppType = AppTypes.Game;
                                                break;
                                            case "NonApp":
                                                g.AppType = AppTypes.Other;
                                                break;
                                            default:
                                                g.AppType = AppTypes.Unknown;
                                                break;
                                        }
                                    }
                                }
                                else
                                {
                                    g.AppType = XmlUtil.GetEnumFromNode(gameNode[XmlName_Game_Type], AppTypes.Unknown);
                                }

                                g.Platforms = XmlUtil.GetEnumFromNode(gameNode[XmlName_Game_Platforms], AppPlatforms.All);

                                g.ParentId = XmlUtil.GetIntFromNode(gameNode[XmlName_Game_Parent], -1);

                                if (fileVersion < 1)
                                {
                                    List<string> genreList = new List<string>();
                                    string genreString = XmlUtil.GetStringFromNode(gameNode["genre"], null);
                                    if (genreString != null)
                                    {
                                        string[] genStrList = genreString.Split(',');
                                        genreList.AddRange(genStrList.Select(s => s.Trim()));
                                    }

                                    g.Genres = genreList;
                                }
                                else
                                {
                                    g.Genres = XmlUtil.GetStringsFromNodeList(gameNode.SelectNodes(XmlName_Game_Genre));
                                }

                                g.Tags = XmlUtil.GetStringsFromNodeList(gameNode.SelectNodes(XmlName_Game_Tag));

                                XmlNodeList nodeList = gameNode.SelectNodes(XmlName_Game_vrSupport);
                                if (nodeList != null)
                                {
                                    foreach (XmlNode vrNode in nodeList)
                                    {
                                        g.vrSupport.Headsets = XmlUtil.GetStringsFromNodeList(vrNode.SelectNodes(XmlName_Game_vrSupport_Headsets));
                                        g.vrSupport.Input = XmlUtil.GetStringsFromNodeList(vrNode.SelectNodes(XmlName_Game_vrSupport_Input));
                                        g.vrSupport.PlayArea = XmlUtil.GetStringsFromNodeList(vrNode.SelectNodes(XmlName_Game_vrSupport_PlayArea));
                                    }
                                }

                                XmlNodeList selectNodes = gameNode.SelectNodes(XmlName_Game_languageSupport);
                                if (selectNodes != null)
                                {
                                    foreach (XmlNode langNode in selectNodes)
                                    {
                                        g.languageSupport.Interface = XmlUtil.GetStringsFromNodeList(langNode.SelectNodes(XmlName_Game_languageSupport_Interface));
                                        g.languageSupport.FullAudio = XmlUtil.GetStringsFromNodeList(langNode.SelectNodes(XmlName_Game_languageSupport_FullAudio));
                                        g.languageSupport.Subtitles = XmlUtil.GetStringsFromNodeList(langNode.SelectNodes(XmlName_Game_languageSupport_Subtitles));
                                    }
                                }

                                g.Developers = XmlUtil.GetStringsFromNodeList(gameNode.SelectNodes(XmlName_Game_Developer));

                                if (fileVersion < 1)
                                {
                                    List<string> pubList = new List<string>();
                                    string pubString = XmlUtil.GetStringFromNode(gameNode["publisher"], null);
                                    if (pubString != null)
                                    {
                                        string[] pubStrList = pubString.Split(',');
                                        pubList.AddRange(pubStrList.Select(s => s.Trim()));
                                    }

                                    g.Publishers = pubList;
                                }
                                else
                                {
                                    g.Publishers = XmlUtil.GetStringsFromNodeList(gameNode.SelectNodes(XmlName_Game_Publisher));
                                }

                                if (fileVersion < 1)
                                {
                                    int steamDate = XmlUtil.GetIntFromNode(gameNode["steamDate"], 0);
                                    g.SteamReleaseDate = steamDate > 0 ? DateTime.FromOADate(steamDate).ToString("MMM d, yyyy") : null;
                                }
                                else
                                {
                                    g.SteamReleaseDate = XmlUtil.GetStringFromNode(gameNode[XmlName_Game_Date], null);
                                }

                                g.Flags = XmlUtil.GetStringsFromNodeList(gameNode.SelectNodes(XmlName_Game_Flag));

                                g.Achievements = XmlUtil.GetIntFromNode(gameNode[XmlName_Game_Achievements], 0);

                                g.ReviewTotal = XmlUtil.GetIntFromNode(gameNode[XmlName_Game_ReviewTotal], 0);
                                g.ReviewPositivePercentage = XmlUtil.GetIntFromNode(gameNode[XmlName_Game_ReviewPositivePercent], 0);

                                g.MC_Url = XmlUtil.GetStringFromNode(gameNode[XmlName_Game_MCUrl], null);

                                g.LastAppInfoUpdate = XmlUtil.GetIntFromNode(gameNode[XmlName_Game_LastAppInfoUpdate], 0);
                                g.LastStoreScrape = XmlUtil.GetIntFromNode(gameNode[XmlName_Game_LastStoreUpdate], 0);

                                g.HltbMain = XmlUtil.GetIntFromNode(gameNode[XmlName_Game_HltbMain], 0);
                                g.HltbExtras = XmlUtil.GetIntFromNode(gameNode[XmlName_Game_HltbExtras], 0);
                                g.HltbCompletionist = XmlUtil.GetIntFromNode(gameNode[XmlName_Game_HltbCompletionist], 0);

                                Games.Add(id, g);
                            }
                        }
                    }
                }

                Logger.Instance.Info("Processed GameDatabase");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            Logger.Instance.Info("Loaded GameDatabase");

            return true;
        }
    }
}