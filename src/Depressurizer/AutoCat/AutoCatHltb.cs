/*
This file is part of Depressurizer.
Copyright 2011, 2012, 2013, 2014 Steve Labbe.

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
    public enum TimeType
    {
        Main,
        Extras,
        Completionist
    }

    public class Hltb_Rule
    {
        [XmlElement("Text")]
        public string Name { get; set; }
        public float MinHours { get; set; }
        public float MaxHours { get; set; }
        public TimeType TimeType { get; set; }

        public Hltb_Rule(string name, float minHours, float maxHours, TimeType timeType)
        {
            Name = name;
            MinHours = minHours;
            MaxHours = maxHours;
            TimeType = timeType;
        }

        //XmlSerializer requires a parameterless constructor
        private Hltb_Rule() { }

        public Hltb_Rule(Hltb_Rule other)
        {
            Name = other.Name;
            MinHours = other.MinHours;
            MaxHours = other.MaxHours;
            TimeType = other.TimeType;
        }
    }

    public class AutoCatHltb : AutoCat
    {
        #region Properties

        public string Prefix { get; set; }
        public bool IncludeUnknown { get; set; }
        public string UnknownText { get; set; }
        [XmlElement("Rule")]
        public List<Hltb_Rule> Rules;

        public override AutoCatType AutoCatType
        {
            get { return AutoCatType.Hltb; }
        }

        #endregion

        #region Construction

        public AutoCatHltb(string name, string filter = null, string prefix = null,
            bool includeUnknown = true, string unknownText = "", List<Hltb_Rule> rules = null, bool selected = false)
            : base(name)
        {
            Filter = filter;
            Prefix = prefix;
            IncludeUnknown = includeUnknown;
            UnknownText = unknownText;
            Rules = (rules == null) ? new List<Hltb_Rule>() : rules;
            Selected = selected;
        }

        //XmlSerializer requires a parameterless constructor
        private AutoCatHltb() { }

        public AutoCatHltb(AutoCatHltb other)
            : base(other)
        {
            Filter = other.Filter;
            Prefix = other.Prefix;
            IncludeUnknown = other.IncludeUnknown;
            UnknownText = other.UnknownText;
            Rules = other.Rules.ConvertAll(rule => new Hltb_Rule(rule));
            Selected = other.Selected;
        }

        public override AutoCat Clone()
        {
            return new AutoCatHltb(this);
        }

        #endregion

        #region Autocategorization

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

            string result = null;

            float hltbMain = db.Games[game.Id].HltbMain / 60.0f;
            float hltbExtras = db.Games[game.Id].HltbExtras / 60.0f;
            float hltbCompletionist = db.Games[game.Id].HltbCompletionist / 60.0f;

            if (IncludeUnknown && (hltbMain == 0.0f) && (hltbExtras == 0.0f) && (hltbCompletionist == 0.0f))
            {
                result = UnknownText;
            }
            else
            {
                foreach (Hltb_Rule rule in Rules)
                {
                    if (CheckRule(rule, hltbMain, hltbExtras, hltbCompletionist))
                    {
                        result = rule.Name;
                        break;
                    }
                }
            }

            if (result != null)
            {
                result = GetProcessedString(result);
                game.AddCategory(games.GetCategory(result));
            }
            return AutoCatResult.Success;
        }

        private bool CheckRule(Hltb_Rule rule, float hltbMain, float hltbExtras, float hltbCompletionist)
        {
            float hours = 0.0f;
            if (rule.TimeType == TimeType.Main)
            {
                hours = hltbMain;
            }
            else if (rule.TimeType == TimeType.Extras)
            {
                hours = hltbExtras;
            }
            else if (rule.TimeType == TimeType.Completionist)
            {
                hours = hltbCompletionist;
            }
            if (hours == 0.0f)
            {
                return false;
            }

            return ((hours >= rule.MinHours) && ((hours <= rule.MaxHours) || (rule.MaxHours == 0.0f)));
        }

        private string GetProcessedString(string s)
        {
            if (!string.IsNullOrEmpty(Prefix))
            {
                return Prefix + s;
            }

            return s;
        }

        #endregion
    }
}