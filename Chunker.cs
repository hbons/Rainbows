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
using System.IO;

namespace Rainbows {

    public class Chunker {

        public string OutputDirectory;

        public delegate void ChunkCreatedHandler (string chunk_file_path, int chunk_size, string chunk_hash);
        public event ChunkCreatedHandler ChunkCreated;

        public delegate void ChunkingFinishedHandler (string [] chunk_paths);
        public event ChunkingFinishedHandler ChunkingFinished;

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


        public void FileToChunks (string [] source_file_paths)
        {
            int chunk_size = 1024 * 1024 * 4;
            List <string> chunk_paths = new List<string> ();

            // TODO: threadpool and block
            foreach (string source_file_path in source_file_paths) {

                using (FileStream stream = File.OpenRead (source_file_path))
                {
                    stream.Lock (0, stream.Length);
                    List<string> new_chunk_paths = new List<string> ();

                    try {
                        int current_chunk_size       = 0;
                        var buffer                   = new byte [chunk_size];
                        int chunk_number             = 1;

                        while ((current_chunk_size = stream.Read (buffer, 0, buffer.Length)) > 0)
                        {
                            string hash                 = Cryptographer.SHA1 (buffer);
                            string chunk_file_name      = hash.Substring (2);
                            string chunk_container      = hash.Substring (0, 2);
                            string chunk_container_path = Path.Combine (OutputDirectory, chunk_container);
                            string chunk_file_path      = Path.Combine (chunk_container_path, chunk_file_name);

                            // TODO: Calculate SHA1 hash of the full file here too

                            if (!File.Exists (chunk_file_path)) {
                                if (!Directory.Exists (chunk_container_path))
                                    Directory.CreateDirectory (chunk_container_path);

                                if (this.cryptographer != null) {
                                    byte [] crypto_buffer = this.cryptographer.Encrypt (buffer);
                                    File.WriteAllBytes (chunk_file_path, crypto_buffer);

                                } else {
                                    File.WriteAllBytes (chunk_file_path, buffer);
                                    new_chunk_paths.Add (chunk_file_path);
                                }

                                chunk_paths.Add (chunk_file_path);

                                if (ChunkCreated != null) // TODO: return full file hash too
                                    ChunkCreated (chunk_file_path, current_chunk_size, hash);

                                Console.WriteLine ("Chunk " + hash + " created");

                            } else {
                                Console.WriteLine ("Chunk " + hash + " exists");
                            }

                            chunk_number++;
                        }

                    } catch (IOException) {
                        foreach (string new_chunk_path in new_chunk_paths) {
                            if (File.Exists (new_chunk_path))
                                File.Delete (new_chunk_path); // TODO: what to do with ongoing transfers?
                        }

                    } finally {
                        stream.Unlock (0, stream.Length);
                    }
                }
            }

            if (ChunkingFinished != null)
                ChunkingFinished (chunk_paths.ToArray ());
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
