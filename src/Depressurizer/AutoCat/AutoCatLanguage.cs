/*
    This file is part of Depressurizer.
    Original work Copyright 2011, 2012, 2013 Steve Labbe.
    Modified work Copyright 2017 Theodoros Dimos.

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
    public class AutoCatLanguage : AutoCat
    {
        public override AutoCatType AutoCatType => AutoCatType.Language;

        // AutoCat configuration
        public string Prefix { get; set; }

        public bool IncludeTypePrefix { get; set; }

        public bool TypeFallback { get; set; }

        public LanguageSupport IncludedLanguages;

        public AutoCatLanguage(string name, string filter = null, string prefix = null, bool includeTypePrefix = false, bool typeFallback = false, List<string> interfaceLanguage = null,
            List<string> subtitles = null, List<string> fullAudio = null, bool selected = false) : base(name)
        {
            Filter = filter;
            Prefix = prefix;
            IncludeTypePrefix = includeTypePrefix;
            TypeFallback = typeFallback;

            IncludedLanguages.Interface = interfaceLanguage ?? new List<string>();
            IncludedLanguages.Subtitles = subtitles ?? new List<string>();
            IncludedLanguages.FullAudio = fullAudio ?? new List<string>();
            Selected = selected;
        }

        //XmlSerializer requires a parameterless constructor
        private AutoCatLanguage() { }

        protected AutoCatLanguage(AutoCatLanguage other) : base(other)
        {
            Filter = other.Filter;
            Prefix = other.Prefix;
            IncludeTypePrefix = other.IncludeTypePrefix;
            TypeFallback = other.TypeFallback;
            IncludedLanguages = other.IncludedLanguages;
            Selected = other.Selected;
        }

        public override AutoCat Clone() => new AutoCatLanguage(this);

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

            LanguageSupport Language = db.Games[game.Id].languageSupport;

            Language.Interface = Language.Interface ?? new List<string>();
            Language.Subtitles = Language.Subtitles ?? new List<string>();
            Language.FullAudio = Language.FullAudio ?? new List<string>();

            IEnumerable<string> interfaceLanguage = Language.Interface.Intersect(IncludedLanguages.Interface);
            foreach (string catString in interfaceLanguage)
            {
                Category c = games.GetCategory(GetProcessedString(catString, "Interface"));
                game.AddCategory(c);
            }

            foreach (string catString in IncludedLanguages.Subtitles)
            {
                if (Language.Subtitles.Contains(catString) || ((Language.Subtitles.Count == 0) && Language.FullAudio.Contains(catString)) || ((Language.FullAudio.Count == 0) && Language.Interface.Contains(catString)))
                {
                    game.AddCategory(games.GetCategory(GetProcessedString(catString, "Subtitles")));
                }
            }

            foreach (string catString in IncludedLanguages.FullAudio)
            {
                if (Language.FullAudio.Contains(catString) || ((Language.FullAudio.Count == 0) && Language.Subtitles.Contains(catString)) || ((Language.Subtitles.Count == 0) && Language.Interface.Contains(catString)))
                {
                    game.AddCategory(games.GetCategory(GetProcessedString(catString, "Full Audio")));
                }
            }

            return AutoCatResult.Success;
        }

        private string GetProcessedString(string baseString, string type="")
        {
            string result = baseString;

            if (IncludeTypePrefix && !string.IsNullOrEmpty(type))
            {
                result = "(" + type + ") " + result;
            }

            if (!string.IsNullOrEmpty(Prefix))
            {
                result = Prefix + result;
            }

            return result;
        }
    }
}