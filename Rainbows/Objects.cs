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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Rainbows.Objects {

    public abstract class HashObject {

        public static string DatabasePath;
        public string Hash;

        public HashObject (string hash)
        {
            Hash = hash;
        }


        protected byte [] ReadHashObject (string hash)
        {
            string file_path = Path.Combine (DatabasePath, "objects",
                hash.Substring (0, 2), hash.Substring (2));

            try {
                return File.ReadAllBytes (file_path);

            } catch (IOException) {
                Utils.Log ("Could not read object " + hash);
                return null;
            }
        }


        public static void WriteHashObject (string hash, byte [] buffer)
        {
            string file_path = Path.Combine (DatabasePath, "objects",
                hash.Substring (0, 2), hash.Substring (2));

            try {
                File.WriteAllBytes (file_path, buffer);

            } catch (IOException) {
                Utils.Log ("Could not write object " + hash);
            }
        }


        public static string [] ToLines (byte [] buffer)
        {
            string line = ASCIIEncoding.ASCII.GetString (buffer);
            return line.Split ("\n".ToCharArray (), StringSplitOptions.RemoveEmptyEntries);
        }


        public static byte [] ToBytes (string line)
        {
            return UnicodeEncoding.Unicode.GetBytes (line);
        }
    }


    public class Commit : HashObject {

        public string ParentHash;
        public string Author;
        public string Email;
        public DateTime Timestamp;


        public Commit (string hash) : base (hash)
        {
        }


        public Tree Root {
            get {
                byte [] buffer = ReadHashObject (Hash);
                string line    = ToLines (buffer) [0];
                string hash    = line.Substring (0, line.IndexOf (" "));

                return new Tree (hash);
            }
        }


        public static Commit Write (User author, DateTime timestamp,
            Commit parent, Tree root)
        {
            int seconds_since_epoch = (int) (timestamp - new DateTime (1970, 1, 1)).TotalSeconds;

            string line = root.Hash + " " + seconds_since_epoch + " " +
                author.Name + " " + "<" + author.Email + ">" + "\n";

            string hash = Utils.SHA1 (line);
            HashObject.WriteHashObject (hash, ToBytes (line));

            return new Commit (hash);
        }
    }


    public class Tree : HashObject {

        public Tree (string hash) : base (hash)
        {
        }


        public Hashtable Trees {
            get {
                Hashtable trees = new Hashtable ();

                byte [] buffer  = ReadHashObject (Hash);
                string [] lines = ToLines (buffer);

                foreach (string line in lines) {
                    string [] columns = line.Split (" ".ToCharArray ());
                    string name       = columns [1];

                    if (name.EndsWith ("/")) {
                        string hash = columns [0];
                        trees.Add (name, new Tree (hash));
                    }
                }

                return trees;
            }
         }


        public Hashtable Blobs {
            get {
                Hashtable blobs = new Hashtable ();

                byte [] buffer  = ReadHashObject (Hash);
                string [] lines = ToLines (buffer);

                foreach (string line in lines) {
                    string [] columns = line.Split (" ".ToCharArray ());
                    string name       = columns [1];

                    if (!name.EndsWith ("/")) {
                        string hash = columns [0];
                        blobs.Add (name, new Blob (hash));
                    }
                }

                return blobs;
            }
         }


         public static Tree Write (Hashtable trees_and_blobs)
         {
            IDictionaryEnumerator dictionary = trees_and_blobs.GetEnumerator ();
            string tree_lines = "";
            string blob_lines = "";

            while (dictionary.MoveNext ()) {
                string name = (string) dictionary.Key;

                if (name.EndsWith ("/")) {
                    Tree tree = (Tree) dictionary.Value;
                    tree_lines += tree.Hash + " " + name + "\n";

                } else {
                    Blob blob = (Blob) dictionary.Value;
                    blob_lines += blob.Hash + " " + name + "\n";
                }
            }

            string lines = tree_lines + blob_lines;

            string hash = Utils.SHA1 (lines);
            HashObject.WriteHashObject (hash, ToBytes (lines));

            return new Tree (hash);
        }
    }


    public class Blob : HashObject {

        public Blob (string hash) : base (hash)
        {
        }


        public Chunk [] Chunks {
            get {
                byte [] buffer         = ReadHashObject (Hash);
                string [] chunk_hashes = ToLines (buffer);
                List<Chunk> chunks     = new List<Chunk> ();

                foreach (string chunk_hash in chunk_hashes)
                    chunks.Add (new Chunk (chunk_hash));

                return chunks.ToArray ();
            }
        }


        public static Blob Write (Chunk [] chunks)
        {
            string lines = "";

            foreach (Chunk chunk in chunks)
                lines += chunk.Hash + "\n";

            string hash = Utils.SHA1 (lines);
            HashObject.WriteHashObject (hash, ToBytes (lines));

            return new Blob (hash);
        }
    }


    public class Chunk : HashObject {

        public Chunk (string hash) : base (hash)
        {
        }


        public byte [] Bytes {
            get {
                byte [] buffer = ReadHashObject (Hash);

                if (buffer != null)
                    return buffer;
                else
                    return null;
            }

            set {
                // Saving chunks is done by the Chunker,
                // so we don't need to implement it
            }
        }
    }
}
