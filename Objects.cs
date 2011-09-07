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
                return null;
            }
        }


        protected void WriteHashObject (string hash, string lines)
        {
        }


        protected string [] ToLines (byte [] buffer)
        {
            string line = ASCIIEncoding.ASCII.GetString (buffer);
            return line.Split ("\n".ToCharArray (), StringSplitOptions.RemoveEmptyEntries);
        }


        protected byte [] ToBytes (string line)
        {
            return ASCIIEncoding.ASCII.GetBytes (line);
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


        public Tree Tree {
            get {
                return null;
            }

            set {
                Tree tree = value;
                // WriteHashObject
            }
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
                    string hash = columns [0];
                    string name = columns [1];

                    if (name.EndsWith ("/"))
                        trees.Add (name, new Tree (hash));
                }

                if (trees.Count > 0)
                    return trees;
                else
                    return null;
            }

            set {
                IDictionaryEnumerator dictionary = value.GetEnumerator ();
                List<string> names = new List<string> ();
                List<Tree> trees   = new List<Tree> ();

                while (dictionary.MoveNext ()) {
                    string name = (string) dictionary.Key;
                    Tree tree   = (Tree) dictionary.Value;

                    names.Add (name);
                    trees.Add (tree);
                }

                byte [] buffer = ReadHashObject (Hash);
                List<string> lines;

                if (buffer == null) {
                    lines = new List<string> ();

                    // write trees

                } else {
                    lines = new List<string> (ToLines (buffer));

                    for (int i = 0; i < names.Count; i++) {
                        string line = trees [i].Hash + " " + names [i] + "\n";
                        lines.Add (line);
                    }
                    // append trees
                }

                WriteHashObject (Hash, string.Join ("", lines.ToArray ()));
            }
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

            set {
                Chunk [] chunks = value;

                string lines = "";
                foreach (Chunk chunk in chunks)
                    lines += chunk + "\n";

                WriteHashObject (Hash, lines);
            }
        }
    }


    public class Chunk : HashObject {

        public Chunk (string hash) : base (hash)
        {
        }


        public byte [] Bytes {
            get {
                // TODO: return null if file doesn't exist so it can be downloaded
                return ReadHashObject (Hash);
            }

            set {
                // Saving chunks is done by the Chunker,
                // so we don't need to implement it
            }
        }
    }
}
