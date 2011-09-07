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

namespace Rainbows {

    public abstract class TransferPlugin {

        public TransferPlugin (ConnectionData data)
        {
        }

        public abstract void Connect ();
        public abstract bool Exists (string remote_path);
        public abstract void Upload (string local_path, string remote_path);
        public abstract void Download (string remote_path, string local_path);
        public abstract void Move (string remote_path, string new_remote_path);
        public abstract string [] List (string remote_path);
    }
}
