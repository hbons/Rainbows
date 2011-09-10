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

namespace Rainbows {

    public class Cache {

        public readonly string Path;


        public Cache (string cache_file_path)
        {
            Path = cache_file_path;
        }


        public void Update ()
        {

        }


        public static Cache Create (string cache_file_path)
        {
            string parent_path = Path.GetDirectoryName (cache_file_path);

            if (File.Exists (cache_file_path))
                File.Delete (cache_file_path);

            if (!Directory.Exists (parent_path))
                Directory.CreateDirectory (parent_path);

            File.Create (cache_file_path);

            return new Cache (cache_file_path);
        }
    }
}

