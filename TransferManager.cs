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
using System.Threading;

namespace Rainbows {

    public class TransferManager {

        private bool queued_upload = false;
        private bool busy          = false;
        private Process rsync_process = new Process ();


        public TransferManager (string path, string remote_path)
        {
            this.rsync_process.StartInfo.FileName  = "rsync";
            this.rsync_process.StartInfo.Arguments = "--ignore-existing --bwlimit=500 " +
                "--recursive --whole-file --progress " + path + " " + remote_path;

            Console.WriteLine (this.rsync_process.StartInfo.Arguments);

            this.rsync_process.EnableRaisingEvents              = true;
            this.rsync_process.StartInfo.RedirectStandardOutput = false;
            this.rsync_process.StartInfo.UseShellExecute        = false;
            this.rsync_process.StartInfo.WorkingDirectory       = path;
        }


        public void UploadObjects ()
        {
            this.busy = true;

            this.rsync_process.Start ();
            this.rsync_process.WaitForExit ();

            this.busy = false;
        }


        public void QueueUpload ()
        {
            if (!this.busy) {
                Thread thread = new Thread (
                    new ThreadStart (
                        delegate {
                            UploadObjects ();
                        }
                    )
                );

                thread.Start ();
            }
        }


        public void DownloadObjects (string [] hashes)
        {

        }
    }
}
