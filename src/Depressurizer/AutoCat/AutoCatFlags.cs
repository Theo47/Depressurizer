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
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using Depressurizer.Helpers;
using Rallion;

namespace Depressurizer
{
    public class AutoCatFlags : AutoCat
    {
        public override AutoCatType AutoCatType
        {
            get { return AutoCatType.Flags; }
        }

        // AutoCat configuration
        public string Prefix { get; set; }

        [XmlArray("Flags"), XmlArrayItem("Flag")]
        public List<string> IncludedFlags { get; set; }

        public AutoCatFlags(string name, string filter = null, string prefix = null, List<string> flags = null,
            bool selected = false)
            : base(name)
        {
            Filter = filter;
            Prefix = prefix;
            IncludedFlags = (flags == null) ? (new List<string>()) : flags;
            Selected = selected;
        }

        //XmlSerializer requires a parameterless constructor
        private AutoCatFlags() { }

        protected AutoCatFlags(AutoCatFlags other)
            : base(other)
        {
            Filter = other.Filter;
            Prefix = other.Prefix;
            IncludedFlags = new List<string>(other.IncludedFlags);
            Selected = other.Selected;
        }

        public override AutoCat Clone()
        {
            return new AutoCatFlags(this);
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

            List<string> gameFlags = db.GetFlagList(game.Id);
            if (gameFlags == null)
            {
                gameFlags = new List<string>();
            }
            IEnumerable<string> categories = gameFlags.Intersect(IncludedFlags);

            foreach (string catString in categories)
            {
                Category c = games.GetCategory(GetProcessedString(catString));
                game.AddCategory(c);
            }
            return AutoCatResult.Success;
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