//   Rainbows, an experimental backend for SparkleShare
//   Copyright (C) 2011  Hylke Bons <hylkebons@gmail.com>
//
//   This program is free software: you can redistribute it and/or modify
//   it under the terms of the GNU General Public License as published by
//   the Free Software Foundation, either version 3 of the License, or
//   (at your option) any later version.
//
//   This program is distributed in the hope that it will be useful,
//   but WITHOUT ANY WARRANTY; without even the implied warranty of
//   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//   GNU General Public License for more details.
//
//   You should have received a copy of the GNU General Public License
//   along with this program. If not, see <http://www.gnu.org/licenses/>.


using System;
using System.IO;

namespace Rainbows {

    public class ConnectionData : Uri {

        public readonly string PrivateKey;
        public readonly string User;
        public readonly string Password;


        public ConnectionData (Uri uri, string key_path) : base (uri.ToString ())
        {
            StreamReader reader = new StreamReader (key_path);
            PrivateKey = reader.ReadToEnd ();

            if (UserInfo.Contains (":")) {
                Password = UserInfo.Substring (UserInfo.IndexOf (":"));
                User     = UserInfo.Substring (0, UserInfo.IndexOf (":"));

            } else {
                User = UserInfo;
            }
        }
    }
}
