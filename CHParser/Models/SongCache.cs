﻿using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace CHParser.Models
{
    public class SongCache
    {
        private int version;
        private byte[] hash;
        private Dictionary<MetadataType, List<Metadata>> metadataDict = new();
        public List<SongEntry> songs = new();
        private string[] video_formats = ".mp4|.avi|.webm|.vp8|.ogv|.mpeg".Split("|");
        private static string[] default_metas = new string[7] { "Unknown Name", "Unknown Artist", "Unknown Album", "Unknown Genre", "Unknown Year", "Unknown Charter", "Unknown Playlist" };

        public SongCache(string path)
        {
            using Stream input = File.OpenRead(path);
            using BinaryReader binaryReader = new BinaryReader(input);
            version = binaryReader.ReadInt32();
            hash = binaryReader.ReadBytes(16);
            ReadMetadata(binaryReader);
            ReadSongs(binaryReader);
        }

        private void ReadMetadata(BinaryReader binaryReader)
        {
            for (int i = 0; i < 7; i++)
            {
                MetadataType metadataType = (MetadataType)binaryReader.ReadByte();
                int count = binaryReader.ReadInt32();
                List<Metadata> metadataList = new List<Metadata>();
                for (int j = 0; j < count; j++)
                {
                    Metadata metadata = new Metadata(metadataType, binaryReader.ReadString(), j);
                    metadataList.Add(metadata);
                }
                metadataDict.Add(metadataType, metadataList);
            }
        }

        private void ReadSongs(BinaryReader binaryReader)
        {
            int count = binaryReader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                SongEntry song = new SongEntry(binaryReader, this);
                songs.Add(song);
            }
        }

        public Metadata GetMetadata(MetadataType type, int index)
        {
            return metadataDict[type][index];
        }

        public bool AddSong(string path)
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(path);
            bool isMidi = false;
            bool isChart = false;
            bool hasIni = false;
            bool videoBackground = false;
            string chartFileName = null;
            FileInfo[] files = directoryInfo.GetFiles();
            foreach (FileInfo fileInfo in files)
            {
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileInfo.Name);
                string extension = fileInfo.Extension;
                if (fileNameWithoutExtension.Equals("notes", StringComparison.OrdinalIgnoreCase))
                {
                    if (extension.Equals(".mid", StringComparison.OrdinalIgnoreCase))
                    {
                        isMidi = true;
                        chartFileName = fileInfo.Name;
                    }
                    else if (extension.Equals(".chart", StringComparison.OrdinalIgnoreCase))
                    {
                        isChart = true;
                        chartFileName = fileInfo.Name;
                    }
                }
                else if (fileNameWithoutExtension.Equals("song", StringComparison.OrdinalIgnoreCase) && extension.Equals(".ini", StringComparison.OrdinalIgnoreCase))
                {
                    hasIni = true;
                }
                else if (fileNameWithoutExtension.Equals("video", StringComparison.OrdinalIgnoreCase) && video_formats.Contains(extension, StringComparer.OrdinalIgnoreCase))
                {
                    videoBackground = true;
                }
            }
            bool noChart = chartFileName == null && !isMidi && !isChart;
            if (noChart)
            {
                Console.WriteLine("No chart file found in " + path);
                return false;
            }
            var song = new SongEntry(path, this)
            {
                videoBackground = videoBackground,
                chartName = chartFileName
            };
            song.ReadCharts();
            song.GetTopLevelPlaylist();
            songs.Add(song);
            compute_hash();
            return true;
        }


        private void compute_hash()
        {
            byte[] array = new byte[songs.Count * 16];
            for (int i = 0; i < songs.Count; i++)
            {
                songs[i].checksum.CopyTo(array, i * 16);
            }

            using MD5 md5 = MD5.Create();
            hash = md5.ComputeHash(array);
        }

        private bool fix_song_length(SongEntry song)
        {
            if (song.songLength > 0) return true;
            int songLength = song.songLength;
            return false;
        }

        public Metadata AddMetadata(MetadataType type, string value)
        {
            if (string.IsNullOrWhiteSpace(value) || string.IsNullOrEmpty(value))
            {
                value = default_metas[(int)type];
            }
            List<Metadata> list = metadataDict[type];
            Metadata meta = list.Find((Metadata x) => x.value.Equals(value));
            if (meta.value == null)
            {
                meta = new Metadata(type, value, list.Count);
                list.Add(meta);
            }

            return meta;
        }

        public void WriteToFile(string path)
        {
            if (songs.Count == 0)
            {
                return;
            }
            using Stream output = File.Create(path);
            using BinaryWriter binaryWriter = new BinaryWriter(output);
            binaryWriter.Write(20221120);
            binaryWriter.Write(hash);
            WriteMetadata(binaryWriter);
            binaryWriter.Write(songs.Count);
            foreach (SongEntry item in songs)
            {
                FileInfo fileInfo = new FileInfo(item.iniPath);
                FileInfo fileInfo2 = new FileInfo(item.chartPath);
                if (fileInfo.Exists && fileInfo2.Exists)
                {
                    binaryWriter.Write(item.folderPath);
                    binaryWriter.Write(fileInfo.LastWriteTime.ToBinary());
                    binaryWriter.Write(fileInfo2.LastWriteTime.ToBinary());
                    item.WriteToFile(binaryWriter);
                }
            }
        }

        private void WriteMetadata(BinaryWriter binaryWriter)
        {
            foreach (KeyValuePair<MetadataType, List<Metadata>> item in metadataDict)
            {
                binaryWriter.Write((byte)item.Key);
                binaryWriter.Write(item.Value.Count);
                foreach (Metadata item2 in item.Value)
                {
                    binaryWriter.Write(item2.value);
                }
            }
        }
    }
}
