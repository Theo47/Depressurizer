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
using System.Xml;
using System.Xml.Serialization;
using Depressurizer.Helpers;
using Rallion;

namespace Depressurizer
{
    /// <summary>
    /// Autocategorization scheme that adds developer and publisher categories.
    /// </summary>
    public class AutoCatDevPub : AutoCat
    {
        public override AutoCatType AutoCatType
        {
            get { return AutoCatType.DevPub; }
        }

        // Autocat configuration
        public bool AllDevelopers { get; set; }

        public bool AllPublishers { get; set; }
        public string Prefix { get; set; }
        public bool OwnedOnly { get; set; }
        public int MinCount { get; set; }
        [XmlArrayItem("Developer")]
        public List<string> Developers { get; set; }
        [XmlArrayItem("Publisher")]
        public List<string> Publishers { get; set; }

        private IEnumerable<Tuple<string, int>> devList;
        private IEnumerable<Tuple<string, int>> pubList;

        private GameList gamelist;

        /// <summary>
        /// Creates a new AutoCatManual object, which removes selected (or all) categories from one list and then, optionally, assigns categories from another list.
        /// </summary>
        public AutoCatDevPub(string name, string filter = null, string prefix = null, bool owned = true, int count = 0,
            bool developersAll = false, bool publishersAll = false, List<string> developers = null,
            List<string> publishers = null, bool selected = false)
            : base(name)
        {
            Filter = filter;
            Prefix = prefix;
            OwnedOnly = owned;
            MinCount = count;
            AllDevelopers = developersAll;
            AllPublishers = publishersAll;
            Developers = (developers == null) ? new List<string>() : developers;
            Publishers = (publishers == null) ? new List<string>() : publishers;
            Selected = selected;
        }

        //XmlSerializer requires a parameterless constructor
        private AutoCatDevPub() { }

        protected AutoCatDevPub(AutoCatDevPub other)
            : base(other)
        {
            Filter = other.Filter;
            Prefix = other.Prefix;
            OwnedOnly = other.OwnedOnly;
            MinCount = other.MinCount;
            AllDevelopers = other.AllDevelopers;
            AllPublishers = other.AllPublishers;
            Developers = new List<string>(other.Developers);
            Publishers = new List<string>(other.Publishers);
            Selected = other.Selected;
        }

        public override AutoCat Clone()
        {
            return new AutoCatDevPub(this);
        }

        /// <summary>
        /// Prepares to categorize games. Prepares a list of genre categories to remove. Does nothing if removeothergenres is false.
        /// </summary>
        public override void PreProcess(GameList games, GameDB db)
        {
            base.PreProcess(games, db);
            gamelist = games;
            devList = Program.GameDatabase.CalculateSortedDevList(OwnedOnly ? gamelist : null, MinCount);
            pubList = Program.GameDatabase.CalculateSortedPubList(OwnedOnly ? gamelist : null, MinCount);
        }

        public override void DeProcess()
        {
            base.DeProcess();
            gamelist = null;
        }

        public override AutoCatResult CategorizeGame(GameInfo game, Filter filter)
        {
            if (games == null)
            {
                Logger.Instance.Write(LogLevel.Error, GlobalStrings.Log_AutoCat_GamelistNull);
                throw new ApplicationException(GlobalStrings.AutoCatGenre_Exception_NoGameList);
            }
            if (db == null)
            {
                Logger.Instance.Write(LogLevel.Error, GlobalStrings.Log_AutoCat_DBNull);
                throw new ApplicationException(GlobalStrings.AutoCatGenre_Exception_NoGameDB);
            }
            if (game == null)
            {
                Logger.Instance.Write(LogLevel.Error, GlobalStrings.Log_AutoCat_GameNull);
                return AutoCatResult.Failure;
            }

            if (!db.Contains(game.Id) || (db.Games[game.Id].LastStoreScrape == 0))
            {
                return AutoCatResult.NotInDatabase;
            }

            if (!game.IncludeGame(filter))
            {
                return AutoCatResult.Filtered;
            }

            List<string> devs = db.GetDevelopers(game.Id);

            if (devs != null)
            {
                for (int index = 0; index < devs.Count; index++)
                {
                    if (Developers.Contains(devs[index]) || AllDevelopers)
                    {
                        if (DevCount(devs[index]) >= MinCount)
                        {
                            game.AddCategory(games.GetCategory(GetProcessedString(devs[index])));
                        }
                    }
                }
            }

            List<string> pubs = db.GetPublishers(game.Id);

            if (pubs != null)
            {
                for (int index = 0; index < pubs.Count; index++)
                {
                    if (Publishers.Contains(pubs[index]) || AllPublishers)
                    {
                        if (PubCount(pubs[index]) >= MinCount)
                        {
                            game.AddCategory(games.GetCategory(GetProcessedString(pubs[index])));
                        }
                    }
                }
            }

            return AutoCatResult.Success;
        }

        private int DevCount(string name)
        {
            foreach (Tuple<string, int> dev in devList)
            {
                if (dev.Item1 == name)
                {
                    return dev.Item2;
                }
            }
            return 0;
        }

        private int PubCount(string name)
        {
            foreach (Tuple<string, int> pub in pubList)
            {
                if (pub.Item1 == name)
                {
                    return pub.Item2;
                }
            }
            return 0;
        }

        private string GetProcessedString(string baseString)
        {
            if (string.IsNullOrEmpty(Prefix))
            {
                return baseString;
            }
            return Prefix + baseString;
        }
    }
}