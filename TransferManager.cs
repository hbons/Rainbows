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
using System.Diagnostics;
using System.IO;
using System.Threading;

using Renci.SshNet;
using Renci.SshNet.Common;
using Renci.SshNet.Sftp;

namespace Rainbows {

    public class TransferManager {

        public readonly string DatabasePath;
        public readonly string RemotePath;

        private TransferPlugin transfer_plugin;
        private List<string> upload_queue = new List<string> ();
        private bool busy                 = false;

        public delegate void TransferSpeedChangedHandler (int bytes_per_second_up, int bytes_per_second_down);
        public event TransferSpeedChangedHandler TransferSpeedChanged;


        public TransferManager (string database_path, Uri uri)
        {
            if (!database_path.EndsWith ("" + Path.DirectorySeparatorChar))
                database_path += Path.DirectorySeparatorChar;

            DatabasePath = database_path;
            RemotePath   = uri.AbsolutePath;

            ConnectionData connection_data = new ConnectionData (uri, "/home/hbons/test.txt");

            if (connection_data.Scheme.Equals ("ssh"))
                this.transfer_plugin = new TransferPluginSftp (connection_data);
        }


        public bool InitRepository ()
        {
            // Check whether the remote path is a repository.
            // If so, copy it. If not, create it
            if (this.transfer_plugin.Exists (RemotePath + "/" + "HEAD")) {



            }

                if (!InitRepositoryLocal ())
                    return false;

                if (!InitRepositoryRemote ())
                    return false;

/*

                // Create structure remotely
                StreamWriter writer = this.sftp.CreateText (RemotePath + "/" + "HEAD");
                writer.WriteLine ("0:0"); // TODO repo id (ID file?)
                writer.Close ();

                if (!this.sftp.Exists (RemotePath + "/" + "objects"))
                    this.sftp.CreateDirectory (RemotePath + "/" + "objects");


             */

            Stream stream;

            return true;
        }


        public bool InitRepositoryLocal ()
        {
            // Error out if the target folder is not empty
            if (!Directory.Exists (DatabasePath)) {
                Directory.CreateDirectory (DatabasePath);

            } else {
                Utils.Log ("Could not init local: " + DatabasePath + " already exists");
                return false;
            }

            // Create structure locally
            Directory.CreateDirectory (Path.Combine (DatabasePath, "objects"));
            StreamWriter writer = new StreamWriter (Path.Combine (DatabasePath, "HEAD"));
            writer.WriteLine ("0:0");
            writer.Close ();

            // Create a randomish ID
            writer = new StreamWriter (Path.Combine (DatabasePath, "ID"));
            string random = DateTime.Now.ToString ("YYDDMMhhmmssFFF") + new Random ().Next (0, 1000);
            ASCIIEncoding encoding = new ASCIIEncoding ();
            writer.WriteLine (Utils.SHA1 (encoding.GetBytes (random)));
            writer.Close ();

            return true;
        }



        public bool DownloadRepository ()
        {
            string [] object_folder_names = this.transfer_plugin.List (RemotePath + "/" + "objects");

            // Copy over all of the objects
            foreach (string object_folder_name in object_folder_names) {
                Directory.CreateDirectory (Path.Combine (DatabasePath, "objects", object_folder_name));

                string [] object_names = this.transfer_plugin.List (RemotePath + "/" +
                    "objects" + "/" + object_folder_name);

                foreach (string object_name in object_names) {
                    Utils.Log ("Downloading objects/" + object_folder_name + "/" + object_name);

                    this.transfer_plugin.Download (
                        RemotePath + "/" + "objects" + "/" + object_folder_name + "/" + object_name,
                        Path.Combine (DatabasePath, "objects", object_folder_name, object_name)
                    );
                }
            }

            // Copy over the HEAD file
            Utils.Log ("Downloading HEAD");
            this.transfer_plugin.Download (RemotePath + "/" + "HEAD", Path.Combine (DatabasePath, "HEAD"));

            // Copy over the ID file
            Utils.Log ("Downloading ID");
            this.transfer_plugin.Download (RemotePath + "/" + "ID", Path.Combine (DatabasePath, "ID"));

            return true;
        }




        public bool InitRepositoryRemote ()
        {
           // if (!this.transfer_plugin.Upload (
             //   Path.Combine (DatabasePath, "HEAD"), RemotePath + "/" + "HEAD")
            return false;
        }


        public bool UploadObject (string hash)
        {
            this.upload_queue.Add (hash);
            string hash_path = "objects/" + hash.Substring (0, 2) + "/" + hash.Substring (2);

            if (this.busy) {
                Utils.Log ("Queueing upload " + hash_path);
                return true;
            }

            this.busy = true;

            while (this.upload_queue.Count > 0) {
                string next_hash = this.upload_queue [0];
                hash_path = "objects/" + next_hash.Substring (0, 2) + "/" + next_hash.Substring (2);

                string target_object_path = RemotePath + "/" + hash_path;
                string tmp_object_path    = RemotePath + "/" + hash_path + ".tmp";

                string local_object_path = Path.Combine (DatabasePath, "objects",
                    next_hash.Substring (0, 2), next_hash.Substring (2));


                if (!this.transfer_plugin.Exists (target_object_path)) {
                    Utils.Log ("Uploading " + hash_path);

                    try {
                        // Save the object to a temporary file first, then move it.
                        // This prevents ending up with a corrupted database
                        this.transfer_plugin.Upload (local_object_path, tmp_object_path);
                        this.transfer_plugin.Move (tmp_object_path, target_object_path);

                    } catch (TransferPluginException e) {
                        this.busy = false;

                        throw new TransferManagerException ("Failed uploading " + hash_path +
                            Environment.NewLine + e.Message);
                    }

                } else {
                    Utils.Log ("Remote " + hash_path + "  exists");
                }


                this.upload_queue.Remove (next_hash);
            }

            this.busy = false;
            return true;
        }


        public bool DownloadObject (string hash)
        {
            string hash_path = "objects/" + hash.Substring (0, 2) + "/" + hash.Substring (2);

            string remote_object_path = RemotePath + "/" + hash_path;

            string target_object_path = Path.Combine (DatabasePath, "objects",
                hash.Substring (0, 2), hash.Substring (2));

            string tmp_object_path = Path.Combine (DatabasePath, "objects",
                hash.Substring (0, 2), hash.Substring (2) + ".tmp");

            if (!this.transfer_plugin.Exists (remote_object_path)) {
                Console.WriteLine ("Remote object " + hash + " not found");
                return false;
            }

            if (File.Exists (target_object_path)) {
                Utils.Log ("Deleting existing " + hash_path);
                File.Delete (target_object_path);
            }

            Utils.Log ("Downloading " + hash_path);

            try {
                // Save the object to a temporary file first, then move it.
                // This prevents ending up with a corrupted database
                this.transfer_plugin.Download (remote_object_path, tmp_object_path);
                File.Move (tmp_object_path, target_object_path);

            } catch (TransferPluginException e) {
                throw new TransferManagerException ("Failed downloading " + hash_path +
                    Environment.NewLine + e.Message);
            }

            return true;
        }
    }
}
