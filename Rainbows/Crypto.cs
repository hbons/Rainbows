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
using System.Security.Cryptography;
using System.Text;

namespace Rainbows {

    public class Crypto {

        private RijndaelManaged aes = new RijndaelManaged () {
            KeySize   = 256,
            BlockSize = 128,
            Mode      = CipherMode.CBC,
            Padding   = PaddingMode.PKCS7
        };


        public Crypto (string password)
        {
            this.aes.IV  = Convert.FromBase64String (password);
            this.aes.Key = Convert.FromBase64String (password);
        }


        public byte [] Encrypt (byte [] buffer)
        {
            ICryptoTransform crypto = this.aes.CreateEncryptor ();
            return crypto.TransformFinalBlock (buffer, 0, buffer.Length);
        }


        public byte [] Decrypt (byte [] buffer)
        {
            ICryptoTransform crypto = this.aes.CreateDecryptor ();
            return crypto.TransformFinalBlock (buffer, 0, buffer.Length);
        }
    }
}
