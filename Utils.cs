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
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Rainbows {

    public static class Utils {

        public static void Log (string message)
        {
            message = DateTime.Now.ToString ("HH:mm:ss") + " " + message;
            Console.WriteLine (message);
        }


        // Creates a SHA-1 hash of input
        public static string SHA1 (byte [] buffer)
        {
            SHA1 sha1             = new SHA1CryptoServiceProvider ();
            byte [] encoded_bytes = sha1.ComputeHash (buffer);
            return BitConverter.ToString (encoded_bytes).ToLower ().Replace ("-", "");
        }
    }
}
