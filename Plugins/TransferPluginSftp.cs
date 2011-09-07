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

using Renci.SshNet;
using Renci.SshNet.Common;
using Renci.SshNet.Sftp;


namespace Rainbows {

    public class TransferPluginSftp : TransferPlugin {

        private SftpClient sftp;


        public TransferPluginSftp (ConnectionData data) : base (data)
        {
            if (data.Password != null)
                this.sftp = new SftpClient (data.Host, data.Port, data.User, data.Password);
            else
                this.sftp = new SftpClient (data.Host, data.Port, data.User, data.PrivateKey);
        }


        public override void Connect ()
        {
            try {
                if (!this.sftp.IsConnected)
                    this.sftp.Connect ();

            } catch (SshException e) {
                throw new TransferPluginException (e.Message);
            }
        }


        public override bool Exists (string remote_path)
        {
            try {
                Connect ();

                if (this.sftp.Exists (remote_path))
                    return true;
                else
                    return false;

            } catch (SshException e) {
                throw new TransferPluginException (e.Message);
            }
        }


        public override void Upload (string local_path, string remote_path)
        {
            try {
                Connect ();

                FileStream stream = new FileStream (local_path, FileMode.Open);
                this.sftp.UploadFile (stream, remote_path);
                stream.Close ();

            } catch (SshException e) {
                throw new TransferPluginException (e.Message);
            }
        }


        public override void Download (string remote_path, string local_path)
        {
            try {
                Connect ();

                FileStream stream = new FileStream (local_path, FileMode.Create);
                this.sftp.DownloadFile (remote_path, stream);
                stream.Close ();

            } catch (SshException e) {
                throw new TransferPluginException (e.Message);
            }
        }


        public override void Move (string remote_path, string new_remote_path)
        {
            try {
                Connect ();
                this.sftp.RenameFile (remote_path, new_remote_path);

            } catch (SshException e) {
                throw new TransferPluginException (e.Message);
            }
        }


        public override string [] List (string remote_path)
        {
            try {
                Connect ();
                List<string> list = new List<string> ();

                foreach (SftpFile item in this.sftp.ListDirectory (remote_path)) {
                    if (item.Name.Equals (".") || item.Name.Equals (".."))
                        continue;

                    list.Add (item.Name);
                }

                return list.ToArray ();

            } catch (SshException e) {
                throw new TransferPluginException (e.Message);
            }
        }
    }
}
