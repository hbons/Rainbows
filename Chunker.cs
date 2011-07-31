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

    public class Chunker {

        public string OutputDirectory;

        public delegate void ChunkCreatedHandler (string chunk_file_path, int chunk_size, string chunk_hash);
        public event ChunkCreatedHandler ChunkCreated;

        private Cryptographer cryptographer;


        public Chunker (string output_directory)
        {
            this.cryptographer = null;
            this.Init (output_directory);
        }


        public Chunker (string output_directory, Cryptographer cryptographer)
        {
            this.cryptographer = cryptographer;
            this.Init (output_directory);
        }


        private void Init (string output_directory)
        {
            OutputDirectory = Path.Combine (output_directory, "objects");

            if (!Directory.Exists (OutputDirectory))
                Directory.CreateDirectory (OutputDirectory);
        }


        public void FileToChunks (string source_file_path)
        {
            int chunk_size = 1024 * 1024 * 4;

            using (FileStream stream = File.OpenRead (source_file_path))
            {
                int current_chunk_size = 0;
                var buffer             = new byte [chunk_size];
                int chunk_number       = 1;

                while ((current_chunk_size = stream.Read (buffer, 0, buffer.Length)) > 0)
                {
                    string hash                 = Cryptographer.SHA1 (buffer);
                    string chunk_file_name      = hash;
                    string chunk_container      = chunk_file_name.Substring (6, 2);
                    string chunk_container_path = Path.Combine (OutputDirectory, chunk_container);
                    string chunk_file_path      = Path.Combine (chunk_container_path, chunk_file_name);

                    if (!File.Exists (chunk_file_path)) {
                        if (!Directory.Exists (chunk_container_path))
                            Directory.CreateDirectory (chunk_container_path);

                        if (this.cryptographer != null)
                            buffer = this.cryptographer.Encrypt (buffer);

                        File.WriteAllBytes (chunk_file_path, buffer);
                    }

                    if (ChunkCreated != null)
                        ChunkCreated (chunk_file_path, current_chunk_size, hash);

                    chunk_number++;
                }
            }
        }


        public void ChunksToFile (string [] chunk_file_paths, string target_file_path)
        {
            using (FileStream stream = File.OpenWrite (target_file_path))
            {
                Console.WriteLine ("Creating file from chunks...");
                byte [] buffer;

                foreach (string chunk_path in chunk_file_paths) {
                    buffer = File.ReadAllBytes (chunk_path);

                    if (this.cryptographer != null)
                        buffer = this.cryptographer.Decrypt (buffer);

                    stream.Write (buffer, 0, buffer.Length);
                }
            }
        }
    }
}
