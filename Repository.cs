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
using System.Collections;
using System.Text;

namespace Rainbows {

    public class Repository {

        public Repository (string path)
        {

        }


        public void Commit ()
        {
            // - Walk the new tree and create blobs, trees
            // - Create new commit object: 'current' file is ParentHash,
            // - Update 'current' file
        }


        public void Push ()
        {

        }


        public void Pull ()
        {

        }
    }

    // TODO: integrate into Repository
     public class Blobs {

        public readonly string OutputDirectory;


        public Blobs (string output_directory)
        {
            OutputDirectory = Path.Combine (output_directory, "objects");

            if (!Directory.Exists (output_directory))
                Directory.CreateDirectory (output_directory);
        }


        public void Store (string file_name, string [] chunk_hashes)
        {
            string hash            = string.Join ("", chunk_hashes);
            string file_store_name = Cryptographer.SHA1 (Encoding.ASCII.GetBytes (hash));

            string file_store_container      = file_store_name.Substring (5, 2);
            string file_store_container_path = Path.Combine (OutputDirectory, file_store_container);
            string file_store_path           = Path.Combine (file_store_container_path, file_store_name);

            if (!File.Exists (file_store_path)) {
                if (!Directory.Exists (file_store_container_path))
                    Directory.CreateDirectory (file_store_container_path);

                using (FileStream stream = File.OpenWrite (file_store_path))
                {
                    foreach (string chunk_hash in chunk_hashes) {
                        byte [] buffer = Encoding.ASCII.GetBytes (chunk_hash + "\n");
                        stream.Write (buffer, 0, buffer.Length);
                    }
                    Console.WriteLine ("Created: " + file_store_path);
                }
            }
        }
    }

}
