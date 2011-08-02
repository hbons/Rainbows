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

            return File.ReadAllBytes (file_path);;
        }


        protected void WriteHashObject (string hash, byte [] buffer)
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

        public string UserName;
        public string UserEmail;
        public DateTime Timestamp;


        public Commit (string hash) : base (hash)
        {
        }


        public Tree Tree {
            get {
                return null;
            }
        }
    }


    public class Tree : HashObject {

        public string Path;


        public Tree (string hash) : base (hash)
        {
        }


        // Key:   blob hash
        // Value: file name
        public Hashtable Blobs {
            get {
                return null;
            }
        }


        // Key:   tree hash
        // Value: folder name
        public Hashtable Trees {
            get {
                return null;
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
        }
    }


    public class Chunk : HashObject {

        public Chunk (string hash) : base (hash)
        {
        }


        public byte [] Bytes {
            get {
                return ReadHashObject (Hash);
            }
        }
    }
}
