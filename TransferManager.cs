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

//        public delegate void UploadFinishedHandler ();
//        public event UploadFinishedHandler UploadFinished;

        public delegate void DownloadFinishedHandler ();
        public event DownloadFinishedHandler DownloadFinished;

        public int BandwidthLimitKbps = 1500;
        public readonly string DatabasePath;
        public readonly string RemotePath;

        private bool queued_upload    = false;
        private bool busy             = false;
        private Process upload_process = new Process ();
        private Process download_process = new Process ();


        public TransferManager (string database_path, string remote_path)
        {
            if (!database_path.EndsWith ("" + Path.DirectorySeparatorChar))
                database_path += Path.DirectorySeparatorChar;

            DatabasePath = database_path;
            RemotePath   = remote_path;

            this.upload_process.EnableRaisingEvents              = true;
            this.upload_process.StartInfo.RedirectStandardOutput = false;
            this.upload_process.StartInfo.UseShellExecute        = false;
            this.upload_process.StartInfo.WorkingDirectory       = database_path;
            this.upload_process.StartInfo.FileName               = "rsync";

            this.download_process.EnableRaisingEvents              = true;
            this.download_process.StartInfo.RedirectStandardOutput = false;
            this.download_process.StartInfo.UseShellExecute        = false;
            this.download_process.StartInfo.WorkingDirectory       = database_path;
            this.download_process.StartInfo.FileName  = "rsync";
        }


        public void UploadObjects ()
        {
            this.upload_process.StartInfo.Arguments              = "--ignore-existing " +
                "--bwlimit=" + BandwidthLimitKbps + " --recursive --whole-file --progress " +
                "--exclude=HEAD " + DatabasePath + " " + RemotePath;

            this.busy = true;

            this.upload_process.Start ();
            this.upload_process.WaitForExit ();

            this.busy = false;

            if (this.queued_upload) {
                this.queued_upload = false;
                UploadObjects ();
            }

//            if (UploadFinished != null)
//                UploadFinished ();
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
            foreach (string hash in hashes) {
                string remote_object_path = RemotePath + "objects" + "/" +
                    hash.Substring (0, 2) + "/" + hash.Substring (2);

                string target_object_path = DatabasePath + "objects" + "/" +
                    hash.Substring (0, 2) + "/" + hash.Substring (2);

                this.download_process.StartInfo.Arguments = "--bwlimit=" + BandwidthLimitKbps +
                    " --recursive --whole-file --progress " + remote_object_path + " " +
                    target_object_path;

                this.download_process.Start ();
                this.download_process.WaitForExit ();
            }

            if (DownloadFinished != null)
                DownloadFinished ();
        }
    }
}
