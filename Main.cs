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
using System.Xml;

namespace Rainbows
{
    public class Rainbows
    {
        public static string RemoteUrl = "ssh://localhost:/Users/hbons/TestTest";
        public static string LocalPath = "/Users/hbons/SparkleShare/Rainbows/.sparkleshare";

        public static void Main (string [] args)
        {
            //
            // PLAYING GROUND
            //

            Stopwatch s = new Stopwatch ();
            s.Start ();

            Chunker chunker = new Chunker ("/Users/hbons/SparkleShare/Rainbows/.sparkleshare",
                new Cryptographer ("cGFzc3dvcmQAAAAAAAAAAA=="));

            List<string> chunk_paths = new List<string> ();
            List<string> hashes      = new List<string> ();

            TransferManager transfer_manager = new TransferManager (LocalPath + "/", "/Users/hbons/rsync-test");

            chunker.ChunkCreated += delegate (string chunk_file_path, int chunk_size, string chunk_hash) {
             //   Console.WriteLine ("Created: " + chunk_file_path + " (" + chunk_size + " bytes)");
                chunk_paths.Add (chunk_file_path);
                hashes.Add (chunk_hash);


                transfer_manager.QueueUpload ();
            };

            chunker.FileToChunks ("/Users/hbons/hp2.avi");

          //  Blobs blobs = new Blobs ("Users/hbons/SparkleShare/Rainbows/.sparkleshare");
            //blobs.Store ("hp.avi", hashes.ToArray ());


            s.Stop ();
            Console.WriteLine ("Total time: " + (int) s.Elapsed.TotalSeconds + " seconds");
        }
    }
}
