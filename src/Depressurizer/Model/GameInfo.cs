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

using System.Collections.Generic;
using System.Linq;
using Depressurizer.Properties;

namespace Depressurizer.Model
{
    /// <summary>
    /// </summary>
    public sealed class GameInfo
    {
        /// <summary>
        /// </summary>
        public SortedSet<Category> Categories { get; set; }

        /// <summary>
        /// </summary>
        public string Executable
        {
            get
            {
                if (string.IsNullOrEmpty(_executable))
                {
                    _executable = string.Format(Constants.RunSteam, Id);
                }

                return _executable;
            }
            set => _executable = value;
        }

        /// <summary>
        /// </summary>
        /// <see cref="GameList" />
        public Category FavoriteCategory
        {
            get
            {
                if (GameList == null)
                {
                    return null;
                }

                return GameList.FavoriteCategory;
            }
        }

        /// <summary>
        /// </summary>
        public GameList GameList { get; set; }

        /// <summary>
        ///     Positive ID matches to a Steam ID, negative means it's a non-steam game (= -1 - shortcut ID)
        /// </summary>
        public int Id { get; set; }

        public bool IsFavorite => Contains(FavoriteCategory);

        /// <summary>
        /// </summary>
        public bool IsHidden { get; set; }

        /// <summary>
        /// </summary>
        public int LastPlayed { get; set; }

        /// <summary>
        /// </summary>
        public string LaunchString
        {
            get
            {
                if (Id > 0)
                {
                    return Id.ToString();
                }

                if (!string.IsNullOrEmpty(_launchString))
                {
                    return _launchString;
                }

                return null;
            }
            set => _launchString = value;
        }

        /// <summary>
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// </summary>
        public GameListingSource Source { get; set; }

        /// <summary>
        /// </summary>
        /// <see cref="Executable" />
        private string _executable;

        /// <summary>
        /// </summary>
        /// <see cref="LaunchString" />
        private string _launchString;

        /// <summary>
        /// </summary>
        /// <param name="id"></param>
        /// <param name="name"></param>
        /// <param name="list"></param>
        /// <param name="executable"></param>
        public GameInfo(int id, string name, GameList list, string executable = null)
        {
            Id = id;
            Name = name;
            IsHidden = false;
            Categories = new SortedSet<Category>();
            GameList = list;
            Executable = executable;
        }

        /// <summary>
        /// </summary>
        /// <param name="src"></param>
        public void ApplySource(GameListingSource src)
        {
            if (Source < src)
            {
                Source = src;
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="f"></param>
        /// <returns></returns>
        public bool IncludeGame(Filter f)
        {
            if (f == null)
            {
                return true;
            }

            bool isCategorized = false;
            bool isHidden = false;
            bool isVr = false;

            if (f.Uncategorized != (int)AdvancedFilterState.None)
            {
                isCategorized = HasCategories();
            }
            if (f.Hidden != (int)AdvancedFilterState.None)
            {
                isHidden = IsHidden;
            }
            if (f.VR != (int)AdvancedFilterState.None)
            {
                isVr = Program.GameDatabase.SupportsVr(Id);
            }

            if ((f.Uncategorized == (int)AdvancedFilterState.Require) && isCategorized)
            {
                return false;
            }
            if ((f.Hidden == (int)AdvancedFilterState.Require) && !isHidden)
            {
                return false;
            }
            if ((f.VR == (int)AdvancedFilterState.Require) && !isVr)
            {
                return false;
            }

            if ((f.Uncategorized == (int)AdvancedFilterState.Exclude) && !isCategorized)
            {
                return false;
            }
            if ((f.Hidden == (int)AdvancedFilterState.Exclude) && isHidden)
            {
                return false;
            }
            if ((f.VR == (int)AdvancedFilterState.Exclude) && isVr)
            {
                return false;
            }

            if ((f.Uncategorized == (int)AdvancedFilterState.Allow) || (f.Hidden == (int)AdvancedFilterState.Allow) || (f.VR == (int)AdvancedFilterState.Allow) || (f.Allow.Count > 0))
            {
                if ((f.Uncategorized != (int)AdvancedFilterState.Allow) || isCategorized)
                {
                    if ((f.Hidden != (int)AdvancedFilterState.Allow) || !isHidden)
                    {
                        if ((f.VR != (int)AdvancedFilterState.Allow) || !isVr)
                        {
                            if (!Categories.Overlaps(f.Allow))
                            {
                                return false;
                            }
                        }
                    }
                }
            }

            if (!Categories.IsSupersetOf(f.Require))
            {
                return false;
            }

            if (Categories.Overlaps(f.Exclude))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// </summary>
        /// <param name="newCat"></param>
        public void AddCategory(Category newCat)
        {
            if ((newCat != null) && Categories.Add(newCat) && !IsHidden)
            {
                newCat.Count++;
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="newCats"></param>
        public void AddCategory(ICollection<Category> newCats)
        {
            foreach (Category cat in newCats)
            {
                if (!Categories.Contains(cat))
                {
                    AddCategory(cat);
                }
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="remCat"></param>
        public void RemoveCategory(Category remCat)
        {
            if (Categories.Remove(remCat) && !IsHidden)
            {
                remCat.Count--;
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="remCats"></param>
        public void RemoveCategory(ICollection<Category> remCats)
        {
            foreach (Category cat in remCats)
            {
                if (!Categories.Contains(cat))
                {
                    RemoveCategory(cat);
                }
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="alsoClearFavorite"></param>
        public void ClearCategories(bool alsoClearFavorite = false)
        {
            foreach (Category cat in Categories)
            {
                if (!IsHidden)
                {
                    cat.Count--;
                }
            }

            Categories.Clear();
            if (IsFavorite && !alsoClearFavorite)
            {
                Categories.Add(FavoriteCategory);

                if (!IsHidden)
                {
                    FavoriteCategory.Count++;
                }
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="cats"></param>
        /// <param name="preserveFavorite"></param>
        public void SetCategories(ICollection<Category> cats, bool preserveFavorite)
        {
            ClearCategories(!preserveFavorite);
            AddCategory(cats);
        }

        /// <summary>
        /// </summary>
        /// <param name="fav"></param>
        public void SetFavorite(bool fav)
        {
            if (fav)
            {
                AddCategory(FavoriteCategory);
            }
            else
            {
                RemoveCategory(FavoriteCategory);
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="hide"></param>
        public void SetHidden(bool hide)
        {
            if (IsHidden == hide)
            {
                return;
            }

            if (hide)
            {
                foreach (Category cat in Categories)
                {
                    cat.Count--;
                }
            }
            else
            {
                foreach (Category cat in Categories)
                {
                    cat.Count++;
                }
            }

            IsHidden = hide;
        }

        /// <summary>
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public bool Contains(Category c) => Categories.Contains(c);

        /// <summary>
        /// </summary>
        /// <param name="includeFavorite"></param>
        /// <returns></returns>
        public bool HasCategories(bool includeFavorite = false)
        {
            if (Categories.Count == 0)
            {
                return false;
            }

            return !(!includeFavorite && (Categories.Count == 1) && Categories.Contains(FavoriteCategory));
        }

        /// <summary>
        /// </summary>
        /// <param name="except"></param>
        /// <returns></returns>
        public bool HasCategoriesExcept(ICollection<Category> except)
        {
            return (Categories.Count != 0) && Categories.Any(c => !except.Contains(c));
        }

        /// <summary>
        /// </summary>
        /// <param name="ifEmpty"></param>
        /// <param name="includeFavorite"></param>
        /// <returns></returns>
        public string GetCatString(string ifEmpty = "", bool includeFavorite = false)
        {
            string result = "";
            bool first = true;

            foreach (Category c in Categories)
            {
                if (includeFavorite || (c != FavoriteCategory))
                {
                    if (!first)
                    {
                        result += ", ";
                    }
                    result += c.Name;
                    first = false;
                }
            }

            return first ? ifEmpty : result;
        }
    }
}