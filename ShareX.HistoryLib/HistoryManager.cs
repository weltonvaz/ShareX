﻿#region License Information (GPL v3)

/*
    ShareX - A program that allows you to take screenshots and share any file type
    Copyright (c) 2007-2019 ShareX Team

    This program is free software; you can redistribute it and/or
    modify it under the terms of the GNU General Public License
    as published by the Free Software Foundation; either version 2
    of the License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program; if not, write to the Free Software
    Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.

    Optionally you can also view the license at <http://www.gnu.org/licenses/>.
*/

#endregion License Information (GPL v3)

using ShareX.HelpersLib;
using ShareX.HistoryLib.Properties;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;

namespace ShareX.HistoryLib
{
    public abstract class HistoryManager
    {
        public string FilePath { get; private set; }
        public string BackupFolder { get; set; }
        public bool CreateBackup { get; set; }
        public bool CreateWeeklyBackup { get; set; }

        public HistoryManager(string filePath)
        {
            FilePath = filePath;
        }

        public List<HistoryItem> GetHistoryItems()
        {
            try
            {
                return Load();
            }
            catch (Exception e)
            {
                DebugHelper.WriteException(e);

                MessageBox.Show(string.Format(Resources.HistoryManager_GetHistoryItems_Error_occured_while_reading_XML_file___0_, FilePath) + "\r\n\r\n" + e,
                    "ShareX - " + Resources.HistoryManager_GetHistoryItems_Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return new List<HistoryItem>();
        }

        public bool AppendHistoryItem(HistoryItem historyItem)
        {
            try
            {
                if (IsValidHistoryItem(historyItem))
                {
                    return Append(historyItem);
                }
            }
            catch (Exception e)
            {
                DebugHelper.WriteException(e);
            }

            return false;
        }

        private bool IsValidHistoryItem(HistoryItem historyItem)
        {
            return historyItem != null && !string.IsNullOrEmpty(historyItem.FileName) && historyItem.DateTime != DateTime.MinValue &&
                (!string.IsNullOrEmpty(historyItem.URL) || !string.IsNullOrEmpty(historyItem.FilePath));
        }

        public List<HistoryItem> Load()
        {
            return Load(FilePath);
        }

        public abstract List<HistoryItem> Load(string filePath);

        public bool Append(params HistoryItem[] historyItems)
        {
            return Append(FilePath, historyItems);
        }

        public abstract bool Append(string filePath, params HistoryItem[] historyItems);

        protected void Backup(string filePath)
        {
            if (!string.IsNullOrEmpty(BackupFolder))
            {
                if (CreateBackup)
                {
                    Helpers.CopyFile(filePath, BackupFolder);
                }

                if (CreateWeeklyBackup)
                {
                    Helpers.BackupFileWeekly(filePath, BackupFolder);
                }
            }
        }

        public void Test(int itemCount)
        {
            Test(FilePath, itemCount);
        }

        public void Test(string filePath, int itemCount)
        {
            HistoryItem historyItem = new HistoryItem()
            {
                FileName = "Example.png",
                FilePath = @"C:\ShareX\Screenshots\Example.png",
                DateTime = DateTime.Now,
                Type = "Image",
                Host = "Imgur",
                URL = "https://example.com/Example.png",
                ThumbnailURL = "https://example.com/Example.png",
                DeletionURL = "https://example.com/Example.png",
                ShortenedURL = "https://example.com/Example.png"
            };

            HistoryItem[] historyItems = new HistoryItem[itemCount];
            for (int i = 0; i < itemCount; i++)
            {
                historyItems[i] = historyItem;
            }

            Thread.Sleep(1000);

            DebugTimer saveTimer = new DebugTimer($"Saved {itemCount} items");
            Append(filePath, historyItems);
            saveTimer.WriteElapsedMilliseconds();

            Thread.Sleep(1000);

            DebugTimer loadTimer = new DebugTimer($"Loaded {itemCount} items");
            Load(filePath);
            loadTimer.WriteElapsedMilliseconds();
        }
    }
}