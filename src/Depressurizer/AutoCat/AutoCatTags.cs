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
using Depressurizer.Model;
using Rallion;

namespace Depressurizer
{
    public class AutoCatTags : AutoCat
    {
        public override AutoCatType AutoCatType
        {
            get { return AutoCatType.Tags; }
        }

        public string Prefix { get; set; }
        public int MaxTags { get; set; }
        [XmlArray("Tags"), XmlArrayItem("Tag")]
        public HashSet<string> IncludedTags { get; set; }

        public bool ListOwnedOnly { get; set; }
        public float ListWeightFactor { get; set; }
        public int ListMinScore { get; set; }
        public int ListTagsPerGame { get; set; }
        public bool ListScoreSort { get; set; }
        public bool ListExcludeGenres { get; set; }

        public AutoCatTags(string name, string filter = null, string prefix = null,
            HashSet<string> tags = null, int maxTags = 0,
            bool listOwnedOnly = true, float listWeightFactor = 1, int listMinScore = 0, int listTagsPerGame = 0,
            bool listScoreSort = true, bool listExcludeGenres = false, bool selected = false)
            : base(name)
        {
            Filter = filter;
            Prefix = prefix;

            if (tags == null)
            {
                IncludedTags = new HashSet<string>();
            }
            else
            {
                IncludedTags = tags;
            }

            MaxTags = maxTags;
            ListOwnedOnly = listOwnedOnly;
            ListWeightFactor = listWeightFactor;
            ListMinScore = listMinScore;
            ListTagsPerGame = listTagsPerGame;
            ListScoreSort = listScoreSort;
            ListExcludeGenres = listExcludeGenres;
            Selected = selected;
        }

        //XmlSerializer requires a parameterless constructor
        private AutoCatTags() { }

        protected AutoCatTags(AutoCatTags other)
            : base(other)
        {
            Filter = other.Filter;
            Prefix = other.Prefix;
            IncludedTags = new HashSet<string>(other.IncludedTags);
            MaxTags = other.MaxTags;

            ListOwnedOnly = other.ListOwnedOnly;
            ListWeightFactor = other.ListWeightFactor;
            ListMinScore = other.ListMinScore;
            ListTagsPerGame = other.ListTagsPerGame;
            ListScoreSort = other.ListScoreSort;
            ListExcludeGenres = other.ListExcludeGenres;
            Selected = other.Selected;
        }

        public override AutoCat Clone()
        {
            return new AutoCatTags(this);
        }

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

            if (!db.Contains(game.Id) || (db.Games[game.Id].LastStoreScrape == 0))
            {
                return AutoCatResult.NotInDatabase;
            }

            if (!game.IncludeGame(filter))
            {
                return AutoCatResult.Filtered;
            }

            List<string> gameTags = db.GetTagList(game.Id);

            if (gameTags != null)
            {
                int added = 0;
                for (int index = 0; (index < gameTags.Count) && ((MaxTags == 0) || (added < MaxTags)); index++)
                {
                    if (IncludedTags.Contains(gameTags[index]))
                    {
                        game.AddCategory(games.GetCategory(GetProcessedString(gameTags[index])));
                        added++;
                    }
                }
            }

            return AutoCatResult.Success;
        }

        public string GetProcessedString(string s)
        {
            if (string.IsNullOrEmpty(Prefix))
            {
                return s;
            }
            return Prefix + s;
        }
    }
}