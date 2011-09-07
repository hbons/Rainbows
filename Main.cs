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
using System.Diagnostics;
using System.IO;
using System.Text;

using Renci.SshNet;
using Renci.SshNet.Sftp;

namespace Rainbows {

    public class Rainbows {

        public static void Main (string [] args)
        {
            User user = new User {
                Name  = "Hylke Bons",
                Email = "hylkebons@gmail.com"
            };

            Index index = new Index ("/Users/hbons/SparkleShare/Rainbows/.sparkleshare",
                "/Users/hbons/SparkleShare/Rainbows", user);

            //index.Status ();
            //index.Commit ();


            //Index.Clone (new Uri ("ssh://bomahy@bomahy.nl/home/sites/webhosting/bomahy/bomahy/test.sparkleshare"),
              //  "/home/hbons/TEST");

            TransferManager t = new TransferManager ("/home/hbons/TEST/db",
                new Uri ("ssh://bomahy@bomahy.nl/home/sites/webhosting/bomahy/bomahy/test.sparkleshare"));

            t.UploadObject ("12124");
            t.UploadObject ("12125");
            t.UploadObject ("12126");
            t.UploadObject ("12127");
            t.UploadObject ("12128");
            t.UploadObject ("12129");
            t.UploadObject ("12130");
            t.UploadObject ("12131");
            t.UploadObject ("12132");
            t.UploadObject ("12133");
            t.UploadObject ("12134");
            t.UploadObject ("12135");
            t.UploadObject ("12136");
            t.UploadObject ("12137");
            t.UploadObject ("12138");
            t.UploadObject ("12139");
            t.UploadObject ("12140");
            t.DownloadObject ("12126");


        }
    }
}
