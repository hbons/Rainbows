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


        public Commit Head {
            get {
                string head_file_path = Path.Combine (DatabasePath, "HEAD");
                string hash           = File.ReadAllText (head_file_path).Trim ();

                return new Commit (hash);
            }

            set {
                Commit head_commit    = value;
                string head_file_path = Path.Combine (DatabasePath, "HEAD");

                File.WriteAllText (head_file_path, head_commit.Hash);
            }
        }


        public Index (string database_path, string checkout_path, User user)
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
            Chunker chunker = new Chunker (DatabasePath);
            chunker.ChunkCrypto = new Crypto ("cGFzc3dvcmQAAAAAAAAAAA==");

            // Tell the chunker to save in our database format
            chunker.NameChunk = delegate (string chunk_file_name) {
                string hash            = chunk_file_name;
                string chunk_container = hash.Substring (0, 2);
                string chunk_file_name = hash.Substring (2);

                return Path.Combine (chunker.OutputDirectory, chunk_container, chunk_file_name);
            };

            // TransferManager transfer_manager = new TransferManager (
            // DatabasePath, "/Users/hbons/rsync-test");

            chunker.ChunkCreated += delegate (string chunk_file_path, int chunk_size,
                                              string chunk_hash) {
              // add hash to "todo" transfer list
              transfer_manager.UploadObject (chunk_file_path);
              // remove hash to "todo" transfer list
            };

            // TODO: needs to block
            chunker.FileToChunks ("/Users/hbons/hp2.avi", 4 * 1024 * 1024);

            // TODO: Walk the new tree and create blobs, trees, commit
            //
            // foreach
            //   Hashtable trees = new Hashtable ();
            //   Hashtable blobs = new Hashtable ();
            //   trees.Add ("", new Tree ());
            //   blobs.Add ("", new Blob ());
            // - Upload all trees and blobs
            //   Tree tree = new Tree ("123456");
            // transfer_manager.UploadObject ("12345");
            //
            // tree.Trees = trees;
            // tree.Blobs = blobs;
            //
            // Commit commit    = new Commit ("abcdef");
            // commit.Tree      = tree;
            // commit.Author    = user.Name;
            // commit.Email     = user.Email;
            // commit.Timestamp = DateTime.UtcNow;
            //
            // transfer_manager.UploadObject ("abcdef");
            // - Update HEAD file
        }


        public void Checkout (string commit_hash)
        {
            Chunker chunker = new Chunker (DatabasePath);
            chunker.ChunkCrypto = new Crypto ("cGFzc3dvcmQAAAAAAAAAAA==");

            // TODO: walk the HEAD tree
            //chunker.ChunksToFile (chunks, path);
        }


        public bool Push ()
        {
            // TODO: check if the remote HEAD doesn't conflict

            return true;
        }


        public bool Pull ()
        {
            // TODO: 1. Get the latest HEAD commit from the server
            // 2. Parse the tree and download all the objects we don't have
            return true;
        }


        public bool Rebase ()
        {
            // TODO: 1. Get the latest HEAD commit from the server
            // 2. Parse the tree and download all the objects we don't have

           // TransferManager transfer_manager = new TransferManager (
             //   DatabasePath, "/Users/hbons/rsync-test");

            string [] new_remote_objects = new string [0];
            //foreach (string new_remote_object in new_remote_objects)
               // transfer_manager.DownloadObject (new_remote_object);

            // TODO: rebase
            return false;
        }


        public void CollectGarbage (int days)
        {
            // TODO: Remove all objects that are not in the current HEAD and are older than a week
        }


        public static bool Init (Uri uri, string target_database_path)
        {
            TransferManager transfer_manager = new TransferManager (target_database_path, uri);
            transfer_manager.InitRepository ();

            return true;
        }
    }


    // TODO: integrate into Objects
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
            string file_store_name = Utils.SHA1 (Encoding.ASCII.GetBytes (hash));

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
