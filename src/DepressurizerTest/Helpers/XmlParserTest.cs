/*
    This file is part of Depressurizer.
    Original work Copyright 2017 Martijn Vegter.

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

using System.Xml;
using Depressurizer.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DepressurizerTest.Helpers
{
    [TestClass]
    public class XmlParserTest
    {
        [TestMethod]
        public void LoadNoConnection()
        {
            XmlDocument xmlDocument = XmlParser.Load("http://clients3.google.com/generate_204");
            Assert.IsNull(xmlDocument);
        }

        [TestMethod]
        public void LoadInvalidUrl()
        {
            XmlDocument xmlDocument = XmlParser.Load("http://clients3.google.com/g");
            Assert.IsNull(xmlDocument);
        }
    }
}