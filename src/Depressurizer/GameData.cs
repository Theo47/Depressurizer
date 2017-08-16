using System;

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

namespace Depressurizer
{
    /// <summary>
    ///     Listing of the different ways to find out about a game.
    ///     The higher values take precedence over the lower values. If a game already exists with a PackageFree type, it
    ///     cannot change to SteamConfig.
    /// </summary>
    public enum GameListingSource
    {
        Unknown,
        SteamConfig,
        WebProfile,
        PackageFree,
        PackageNormal,
        Manual
    }

    internal class ProfileAccessException : ApplicationException
    {
        public ProfileAccessException(string m) : base(m) { }
    }
}