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
using System.IO;
using System.Threading;

namespace Rainbows {

    public class TransferManager {

        public delegate void UploadFinishedHandler ();
        public event UploadFinishedHandler UploadFinished;

        private bool queued_upload    = false;
        private bool busy             = false;
        private Process upload_process = new Process ();
        private Process download_process = new Process ();


        public TransferManager (string path, string remote_path)
        {
            if (!path.EndsWith ("" + Path.DirectorySeparatorChar))
                path += Path.DirectorySeparatorChar;

            this.upload_process.EnableRaisingEvents              = true;
            this.upload_process.StartInfo.RedirectStandardOutput = false;
            this.upload_process.StartInfo.UseShellExecute        = false;
            this.upload_process.StartInfo.WorkingDirectory       = path;
            this.upload_process.StartInfo.FileName               = "rsync";
            this.upload_process.StartInfo.Arguments              = "--ignore-existing " +
                "--bwlimit=1000 --recursive --whole-file --progress --exclude=HEAD " + path +
                " " + remote_path;

            this.download_process.EnableRaisingEvents              = true;
            this.download_process.StartInfo.RedirectStandardOutput = false;
            this.download_process.StartInfo.UseShellExecute        = false;
            this.download_process.StartInfo.WorkingDirectory       = path;
            this.download_process.StartInfo.FileName  = "rsync";
            this.download_process.StartInfo.Arguments = "--ignore-existing --bwlimit=1000 " +
                "--recursive --whole-file --progress " + path + " " + remote_path;
        }


        public void UploadObjects ()
        {
            this.busy = true;

            this.upload_process.Start ();
            this.upload_process.WaitForExit ();

            this.busy = false;

            if (this.queued_upload) {
                this.queued_upload = false;
                UploadObjects ();
            }
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

            } else {
                this.queued_upload = true;
            }
        }


        public void DownloadObjects (string [] hashes)
        {

        }
    }
}
