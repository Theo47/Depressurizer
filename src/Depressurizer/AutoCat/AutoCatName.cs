﻿using System;
using System.Text.RegularExpressions;
using Depressurizer.Helpers;
using Depressurizer.Model;

namespace Depressurizer
{
    public class AutoCatName : AutoCat
    {
        public override AutoCatType AutoCatType => AutoCatType.Name;

        public bool GroupNonEnglishCharacters { get; set; }
        public string GroupNonEnglishCharactersText { get; set; }
        public bool GroupNumbers { get; set; }
        public string Prefix { get; set; }
        public bool SkipThe { get; set; }

        public AutoCatName(string name, string prefix = "", bool skipThe = true, bool groupNumbers = false, bool groupNonEnglishCharacters = false, string groupNonEnglishCharactersText = "") : base(name)
        {
            Name = name;
            Prefix = prefix;
            SkipThe = skipThe;
            GroupNumbers = groupNumbers;
            GroupNonEnglishCharacters = groupNonEnglishCharacters;
            GroupNonEnglishCharactersText = groupNonEnglishCharactersText;
        }

        //XmlSerializer requires a parameterless constructor
        private AutoCatName() { }

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

            string cat = game.Name.Substring(0, 1);
            cat = cat.ToUpper();
            if (SkipThe && (cat == "T") && (game.Name.Substring(0, 4).ToUpper() == "THE "))
            {
                cat = game.Name.Substring(4, 1).ToUpper();
            }
            if (GroupNumbers && char.IsDigit(cat[0]))
            {
                cat = "#";
            }
            else if (GroupNonEnglishCharacters && !string.IsNullOrEmpty(GroupNonEnglishCharactersText) && Regex.IsMatch(cat, "[^a-z0-9]", RegexOptions.IgnoreCase))
            {
                cat = GroupNonEnglishCharactersText;
            }
            if (Prefix != null)
            {
                cat = Prefix + cat;
            }

            game.AddCategory(games.GetCategory(cat));

            return AutoCatResult.Success;
        }

        public override AutoCat Clone() => new AutoCatName(Name, Prefix, SkipThe, GroupNumbers, GroupNonEnglishCharacters, GroupNonEnglishCharactersText);
    }
}