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
using System.Diagnostics;

namespace Rainbows {

    public class TransferManager {

        Process rsync_process = new Process ();


        public TransferManager (string path, string remote_path)
        {
            this.rsync_process.StartInfo.FileName  = "rsync";
            this.rsync_process.StartInfo.Arguments = "--ignore-existing " +
                "--recursive " +
                "--whole-file " + // Don't do delta sync
                "--progress " +
                path + " " + remote_path;

            this.rsync_process.EnableRaisingEvents = true;

            this.rsync_process.StartInfo.RedirectStandardOutput = false;
            this.rsync_process.StartInfo.UseShellExecute        = false;
            this.rsync_process.StartInfo.WorkingDirectory       = path;
        }


        public void UploadObjects ()
        {
            Console.WriteLine (this.rsync_process.StartInfo.Arguments);
            this.rsync_process.Start ();
            this.rsync_process.WaitForExit ();
        }


        public void DownloadObjects (string [] hashes)
        {

        }
    }
}
