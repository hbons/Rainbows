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

namespace Rainbows {

    public class Commit {

        public string Hash;
        public string ParentHash;

        public string UserName;
        public string UserEmail;
        public DateTime Timestamp;

        public Tree Tree;
    }


    public class Tree {

        public string Hash;
        public string Path;


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


    public class Blob {

        public string Hash;

        public Chunk [] Chunks {
            get {
                return null;
            }
        }
    }


    public class Chunk {

        public string Hash;
    }
}
