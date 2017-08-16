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
using Rallion;
using System.Xml.Serialization;
using Depressurizer.Helpers;
using Depressurizer.Model;

namespace Depressurizer
{
    /// <summary>
    /// Autocategorization scheme that adds and removes manual categories.
    /// </summary>
    public class AutoCatManual : AutoCat
    {
        public override AutoCatType AutoCatType
        {
            get { return AutoCatType.Manual; }
        }

        // Autocat configuration
        [XmlElement("RemoveAll")]
        public bool RemoveAllCategories { get; set; }

        public string Prefix { get; set; }

        [XmlArray("Remove"), XmlArrayItem("Category")]
        public List<string> RemoveCategories { get; set; }
        [XmlArray("Add"), XmlArrayItem("Category")]
        public List<string> AddCategories { get; set; }

        private GameList gamelist;

        /// <summary>
        /// Creates a new AutoCatManual object, which removes selected (or all) categories from one list and then, optionally, assigns categories from another list.
        /// </summary>
        public AutoCatManual(string name, string filter = null, string prefix = null, bool removeAll = false,
            List<string> remove = null, List<string> add = null, bool selected = false)
            : base(name)
        {
            Filter = filter;
            Prefix = prefix;
            RemoveAllCategories = removeAll;
            RemoveCategories = (remove == null) ? new List<string>() : remove;
            AddCategories = (add == null) ? new List<string>() : add;
            Selected = selected;
        }

        //XmlSerializer requires a parameterless constructor
        private AutoCatManual() { }

        protected AutoCatManual(AutoCatManual other)
            : base(other)
        {
            Filter = other.Filter;
            Prefix = other.Prefix;
            RemoveAllCategories = other.RemoveAllCategories;
            RemoveCategories = new List<string>(other.RemoveCategories);
            AddCategories = new List<string>(other.AddCategories);
            Selected = other.Selected;
        }

        public override AutoCat Clone()
        {
            return new AutoCatManual(this);
        }

        /// <summary>
        /// Prepares to categorize games. Prepares a list of genre categories to remove. Does nothing if removeothergenres is false.
        /// </summary>
        public override void PreProcess(GameList games, GameDB db)
        {
            base.PreProcess(games, db);
            gamelist = games;
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

            if (RemoveAllCategories)
            {
                game.ClearCategories();
            }
            else if (RemoveCategories != null)
            {
                List<Category> removed = new List<Category>();

                foreach (string category in RemoveCategories)
                {
                    Category c = gamelist.GetCategory(category);
                    if (game.ContainsCategory(c))
                    {
                        game.RemoveCategory(c);
                        removed.Add(c);
                    }
                }

                foreach (Category c in removed)
                {
                    if (c.Count == 0)
                    {
                        gamelist.RemoveCategory(c);
                    }
                }
            }

            if (AddCategories != null)
            {
                foreach (string category in AddCategories)
                {
                    // add Category, or create it if it doesn't exist
                    game.AddCategory(gamelist.GetCategory(GetProcessedString(category)));
                }
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