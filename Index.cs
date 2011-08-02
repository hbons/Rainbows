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

using Rainbows.Objects;

namespace Rainbows {

    // TODO: all methods here need to block
    public class Index {

        public readonly string DatabasePath;
        public readonly string CheckoutPath;


        public Commit Current {
            get {
                return null;
            }

            set {

            }
        }


        public Index (string database_path, string checkout_path)
        {
            DatabasePath = database_path;
            CheckoutPath = checkout_path;

            HashObject.DatabasePath = DatabasePath;
        }


        public void Status ()
        {
            // TODO compare the checkout with the HEAD tree
        }


        public void Commit ()
        {
            Chunker chunker = new Chunker (DatabasePath,
                new Cryptographer ("cGFzc3dvcmQAAAAAAAAAAA=="));

            TransferManager transfer_manager = new TransferManager (
                DatabasePath, "/Users/hbons/rsync-test");

            chunker.ChunkCreated += delegate (string chunk_file_path, int chunk_size,
                                              string chunk_hash) {

                transfer_manager.QueueUpload ();
            };

            chunker.ChunkingFinished += delegate {
                transfer_manager.QueueUpload ();
            };


            // TODO: needs to block
            chunker.FileToChunks (new string [] {"/Users/hbons/hp2.avi"});

            // TODO: Walk the new tree and create blobs, trees, commit
            // - Update HEAD file
        }


        public void Checkout (string commit_hash)
        {
            Chunker chunker = new Chunker (DatabasePath,
                new Cryptographer ("cGFzc3dvcmQAAAAAAAAAAA=="));

            // TODO: walk the HEAD tree
            //chunker.ChunksToFile (chunks, path);
        }


        public bool Push ()
        {
            // TODO: check if the remote HEAD doesn't conflict

            return true;
        }


        public void PullAndRebase ()
        {
            // TODO: 1. Get the latest HEAD commit from the server
            // 2. Parse the tree and download all the objects we don't have

            TransferManager transfer_manager = new TransferManager (
                DatabasePath, "/Users/hbons/rsync-test");

            string [] new_remote_objects = new string [0];
            transfer_manager.DownloadObjects (new_remote_objects);

            // TODO: rebase
        }


        public void CollectGarbage ()
        {
            // TODO: Remove all objects that are not in the current HEAD and are older than a week
        }


        public static Index Init (string path)
        {
            if (!Directory.Exists (path))
                Directory.CreateDirectory (path);

            string database_path = Path.Combine (path, ".sparkleshare");

            if (!Directory.Exists (database_path))
                Directory.CreateDirectory (database_path);

            return new Index (database_path, path);
        }
    }


    // TODO: integrate into Index
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
            // TODO: we really need the file hash
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
