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
using Depressurizer.Helpers;
using Depressurizer.Model;

namespace Depressurizer
{
    public enum AutoCatYear_Grouping
    {
        None,
        Decade,
        HalfDecade
    }

    public class AutoCatYear : AutoCat
    {
        // Meta properies
        public override AutoCatType AutoCatType => AutoCatType.Year;

        public AutoCatYear_Grouping GroupingMode { get; set; }

        public bool IncludeUnknown { get; set; }

        // Autocat configuration properties
        public string Prefix { get; set; }

        public string UnknownText { get; set; }

        public AutoCatYear(string name, string filter = null, string prefix = null, bool includeUnknown = true, string unknownText = null, AutoCatYear_Grouping groupMode = AutoCatYear_Grouping.None, bool selected = false) : base(name)
        {
            Filter = filter;
            Prefix = prefix;
            IncludeUnknown = includeUnknown;
            UnknownText = unknownText;
            GroupingMode = groupMode;
            Selected = selected;
        }

        //XmlSerializer requires a parameterless constructor
        private AutoCatYear() { }

        protected AutoCatYear(AutoCatYear other) : base(other)
        {
            Filter = other.Filter;
            Prefix = other.Prefix;
            IncludeUnknown = other.IncludeUnknown;
            UnknownText = other.UnknownText;
            GroupingMode = other.GroupingMode;
            Selected = other.Selected;
        }

        public override AutoCat Clone() => new AutoCatYear(this);

        public override AutoCatResult CategorizeGame(GameInfo game, Filter filter)
        {
            if (games == null)
            {
                Logger.Instance.Error(GlobalStrings.Log_AutoCat_GamelistNull);
                throw new ApplicationException(GlobalStrings.AutoCatGenre_Exception_NoGameList);
            }

            if (db == null)
            {
                Logger.Instance.Error(GlobalStrings.Log_AutoCat_DBNull);
                throw new ApplicationException(GlobalStrings.AutoCatGenre_Exception_NoGameDB);
            }

            if (game == null)
            {
                Logger.Instance.Error(GlobalStrings.Log_AutoCat_GameNull);
                return AutoCatResult.Failure;
            }

            if (!db.Contains(game.Id))
            {
                return AutoCatResult.NotInDatabase;
            }

            if (!game.IncludeGame(filter))
            {
                return AutoCatResult.Filtered;
            }

            int year = db.GetReleaseYear(game.Id);
            if ((year > 0) || IncludeUnknown)
            {
                game.AddCategory(games.GetCategory(GetProcessedString(year)));
            }

            return AutoCatResult.Success;
        }

        private string GetProcessedString(int year)
        {
            string result = string.Empty;
            if (year <= 0)
            {
                result = UnknownText;
            }
            else
            {
                switch (GroupingMode)
                {
                    case AutoCatYear_Grouping.Decade:
                        result = GetRangeString(year, 10);
                        break;
                    case AutoCatYear_Grouping.HalfDecade:
                        result = GetRangeString(year, 5);
                        break;
                    default:
                        result = year.ToString();
                        break;
                }
            }

            if (string.IsNullOrEmpty(Prefix))
            {
                return result;
            }

            return Prefix + result;
        }

        private string GetRangeString(int year, int rangeSize)
        {
            int first = year - (year % rangeSize);
            return string.Format("{0}-{1}", first, (first + rangeSize) - 1);
        }
    }
}