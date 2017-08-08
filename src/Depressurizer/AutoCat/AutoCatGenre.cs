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
using Rallion;

namespace Depressurizer
{
    /// <summary>
    /// Autocategorization scheme that adds genre categories.
    /// </summary>
    public class AutoCatGenre : AutoCat
    {
        public override AutoCatType AutoCatType
        {
            get { return AutoCatType.Genre; }
        }

        // Autocat configuration
        public int MaxCategories { get; set; }

        [XmlElement("RemoveOthers")]
        public bool RemoveOtherGenres { get; set; }
        public bool TagFallback { get; set; }
        public string Prefix { get; set; }

        [XmlArray("Ignored"), XmlArrayItem("Ignore")]
        public List<string> IgnoredGenres { get; set; }

        const int MAX_PARENT_DEPTH = 3;

        private SortedSet<Category> genreCategories;

        /// <summary>
        /// Creates a new AutoCatGenre object, which autocategorizes games based on the genres in the Steam store.
        /// </summary>
        /// <param name="db">Reference to GameDB to use</param>
        /// <param name="games">Reference to the GameList to act on</param>
        /// <param name="maxCategories">Maximum number of categories to assign per game. 0 indicates no limit.</param>
        /// <param name="removeOthers">If true, removes any OTHER genre-named categories from each game processed. Will not remove categories that do not match a genre found in the database.</param>
        public AutoCatGenre(string name, string filter = null, string prefix = null, int maxCategories = 0,
            bool removeOthers = false, bool tagFallback = true, List<string> ignore = null, bool selected = false)
            : base(name)
        {
            Filter = filter;
            MaxCategories = maxCategories;
            RemoveOtherGenres = removeOthers;
            TagFallback = tagFallback;
            Prefix = prefix;
            IgnoredGenres = (ignore == null) ? new List<string>() : ignore;
            Selected = selected;
        }

        //XmlSerializer requires a parameterless constructor
        private AutoCatGenre() { }

        protected AutoCatGenre(AutoCatGenre other)
            : base(other)
        {
            Filter = other.Filter;
            MaxCategories = other.MaxCategories;
            RemoveOtherGenres = other.RemoveOtherGenres;
            TagFallback = other.TagFallback;
            Prefix = other.Prefix;
            IgnoredGenres = new List<string>(other.IgnoredGenres);
            Selected = other.Selected;
        }

        public override AutoCat Clone()
        {
            return new AutoCatGenre(this);
        }

        /// <summary>
        /// Prepares to categorize games. Prepares a list of genre categories to remove. Does nothing if removeothergenres is false.
        /// </summary>
        public override void PreProcess(GameList games, GameDB db)
        {
            base.PreProcess(games, db);
            if (RemoveOtherGenres)
            {
                SortedSet<string> genreStrings = db.GetAllGenres();
                genreCategories = new SortedSet<Category>();

                foreach (string cStr in genreStrings)
                {
                    if (games.CategoryExists(String.IsNullOrEmpty(Prefix) ? (cStr) : (Prefix + cStr)) &&
                        !IgnoredGenres.Contains(cStr))
                    {
                        genreCategories.Add(games.GetCategory(cStr));
                    }
                }
            }
        }

        public override void DeProcess()
        {
            base.DeProcess();
            genreCategories = null;
        }

        public override AutoCatResult CategorizeGame(GameInfo game, Filter filter)
        {
            if (games == null)
            {
                Program.Logger.Write(LoggerLevel.Error, GlobalStrings.Log_AutoCat_GamelistNull);
                throw new ApplicationException(GlobalStrings.AutoCatGenre_Exception_NoGameList);
            }
            if (db == null)
            {
                Program.Logger.Write(LoggerLevel.Error, GlobalStrings.Log_AutoCat_DBNull);
                throw new ApplicationException(GlobalStrings.AutoCatGenre_Exception_NoGameDB);
            }
            if (game == null)
            {
                Program.Logger.Write(LoggerLevel.Error, GlobalStrings.Log_AutoCat_GameNull);
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

            if (RemoveOtherGenres && (genreCategories != null))
            {
                game.RemoveCategory(genreCategories);
            }

            List<string> genreList = db.GetGenreList(game.Id, depth: MAX_PARENT_DEPTH, tagFallback: TagFallback);
            if ((genreList != null) && (genreList.Count > 0))
            {
                List<Category> categories = new List<Category>();
                int max = MaxCategories;
                for (int i = 0; (i < genreList.Count) && ((MaxCategories == 0) || (i < max)); i++)
                {
                    if (!IgnoredGenres.Contains(genreList[i]))
                    {
                        categories.Add(games.GetCategory(GetProcessedString(genreList[i])));
                    }
                    else
                    {
                        max++; // ignored genres don't contribute to max
                    }
                }

                game.AddCategory(categories);
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