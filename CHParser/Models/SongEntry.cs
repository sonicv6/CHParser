using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IniParser;
using IniParser.Model;

namespace CHParser.Models
{
    public class SongEntry
    {
        //[NonSerialized]
        //public GClass9 songEnc;

        //[NonSerialized]
        public long charts;

        [NonSerialized] public string iconName = "";

        [NonSerialized] public string folderPath;

        [NonSerialized] public byte[] checksum;

        private string checksumStringCached;


        [NonSerialized] public short playlistTrack;

        [NonSerialized] public short albumTrack;

        [NonSerialized] public int previewStart;

        private bool isMidiChartCache;

        private bool isTypeCached;


        [NonSerialized] public DateTime dateAdded;

        [NonSerialized] public bool videoBackground;

        [NonSerialized] public bool isAvailableOnline = true;

        public string chartName;

        public int songLength;

        public Metadata[] metadata;

        private static string[] metadataCache = new string[7];

        public bool lyrics;

        public bool modchart;

        public bool forceProDrums;

        public bool forceFiveLane;

        public string topLevelPlaylist = "";

        public string subPlaylist = "";

        private sbyte[] intensities;

        private const int INTENSITY_COUNT = 12;

        [NonSerialized] public bool metadataLoaded;

        public bool filtered;

        public bool isEnc;

        public string checksumString;

        private SongCache cache;

        public SongEntry(string path, SongCache cache)
        {
            this.cache = cache;
            FileAttributes attributes = File.GetAttributes(path);
            isEnc = !attributes.HasFlag(FileAttributes.Directory) && path.EndsWith(".sng");
            intensities = new sbyte[12];
            for (int i = 0; i < intensities.Length; i++)
            {
                intensities[i] = -1;
            }

            Array.Clear(metadataCache, 0, metadataCache.Length);
            string iniPath = Path.Combine(path, "song.ini");
            string chartFilePath = Path.Combine(path, "notes.chart");
            folderPath = path;
            if (!ReadINI(iniPath))
            {
                metadataLoaded = false;
                //if (ReadChart(chartFilePath))
                //{
                //    metadataLoaded = true;
                //}
            }
            else
            {
                metadataLoaded = true;
            }
        }

        public SongEntry(BinaryReader binaryReader, SongCache cache)
        {
            this.cache = cache;
            folderPath = binaryReader.ReadString();
            binaryReader.ReadInt64();
            binaryReader.ReadInt64();
            metadata = new Metadata[7];
            intensities = new sbyte[12];
            for (int i = 0; i < intensities.Length; i++)
            {
                intensities[i] = -1;
            }

            read_song(binaryReader);
        }

        private void read_song(BinaryReader binaryReader)
        {
            chartName = binaryReader.ReadString();
            isEnc = binaryReader.ReadBoolean();
            metadata[0] = cache.GetMetadata(MetadataType.Name, binaryReader.ReadInt32());
            metadata[1] = cache.GetMetadata(MetadataType.Artist, binaryReader.ReadInt32());
            metadata[2] = cache.GetMetadata(MetadataType.Album, binaryReader.ReadInt32());
            metadata[3] = cache.GetMetadata(MetadataType.Genre, binaryReader.ReadInt32());
            metadata[4] = cache.GetMetadata(MetadataType.Year, binaryReader.ReadInt32());
            metadata[5] = cache.GetMetadata(MetadataType.Charter, binaryReader.ReadInt32());
            metadata[6] = cache.GetMetadata(MetadataType.Playlist, binaryReader.ReadInt32());
            charts = binaryReader.ReadInt64();
            lyrics = binaryReader.ReadBoolean();
            intensities[8] = binaryReader.ReadSByte();
            intensities[0] = binaryReader.ReadSByte();
            intensities[2] = binaryReader.ReadSByte();
            intensities[3] = binaryReader.ReadSByte();
            intensities[1] = binaryReader.ReadSByte();
            intensities[6] = binaryReader.ReadSByte();
            intensities[9] = binaryReader.ReadSByte();
            intensities[7] = binaryReader.ReadSByte();
            intensities[4] = binaryReader.ReadSByte();
            intensities[5] = binaryReader.ReadSByte();
            intensities[11] = binaryReader.ReadSByte();
            intensities[10] = binaryReader.ReadSByte();
            previewStart = binaryReader.ReadInt32();
            iconName = binaryReader.ReadString();
            albumTrack = binaryReader.ReadInt16();
            playlistTrack = binaryReader.ReadInt16();
            modchart = binaryReader.ReadBoolean();
            videoBackground = binaryReader.ReadBoolean();
            forceProDrums = binaryReader.ReadBoolean();
            forceFiveLane = binaryReader.ReadBoolean();
            songLength = binaryReader.ReadInt32();
            dateAdded = DateTime.FromBinary(binaryReader.ReadInt64());
            topLevelPlaylist = binaryReader.ReadString();
            subPlaylist = binaryReader.ReadString();
            checksum = binaryReader.ReadBytes(16);
        }

        private bool ReadINI(string iniPath)
        {
            try
            {
                var parser = new FileIniDataParser();
                IniData data = parser.ReadFile(iniPath);
                isTypeCached = false;
                if (data.Sections.ContainsSection("Song"))
                {
                    var song = data.Sections["Song"];
                    metadataCache[0] = song["name"];
                    metadataCache[1] = song["artist"];
                    metadataCache[2] = song["album"];
                    metadataCache[3] = song["genre"];
                    metadataCache[4] = song["year"];
                    intensities[8] = (sbyte)(song["diff_band"] == null ? -1:sbyte.Parse(song["diff_band"]));
                    intensities[0] = (sbyte)(song["diff_guitar"] == null ? -1 : sbyte.Parse(song["diff_guitar"]));
                    intensities[2] = (sbyte)(song["diff_rhythm"] == null ? -1 : sbyte.Parse(song["diff_rhythm"]));
                    intensities[3] = (sbyte)(song["diff_guitar_coop"] == null ? -1 : sbyte.Parse(song["diff_guitar_coop"]));
                    intensities[1] = (sbyte)(song["diff_bass"] == null ? -1 : sbyte.Parse(song["diff_bass"]));
                    intensities[6] = (sbyte)(song["diff_drums"] == null ? -1 : sbyte.Parse(song["diff_drums"]));
                    intensities[9] = (sbyte)(song["diff_drums_real"] == null ? -1 : sbyte.Parse(song["diff_drums_real"]));
                    intensities[7] = (sbyte)(song["diff_keys"] == null ? -1 : sbyte.Parse(song["diff_keys"]));
                    intensities[4] = (sbyte)(song["diff_guitarghl"] == null ? -1 : sbyte.Parse(song["diff_guitarghl"]));
                    intensities[5] = (sbyte)(song["diff_bassghl"] == null ? -1 : sbyte.Parse(song["diff_bassghl"]));
                    intensities[11] = (sbyte)(song["diff_guitar_coop_ghl"] == null ? -1 : sbyte.Parse(song["diff_guitar_coop_ghl"]));
                    intensities[10] = (sbyte)(song["diff_rhythm_ghl"] == null ? -1 : sbyte.Parse(song["diff_rhythm_ghl"]));
                    previewStart = (song["preview_start_time"] == null ? -1 : sbyte.Parse(song["preview_start_time"]));
                    iconName = string.Empty;
                    playlistTrack = (short)(song["playlist_track"] == null ? 16000 : sbyte.Parse(song["playlist_track"]));
                    try
                    { modchart = bool.Parse(song["modchart"]); }
                    catch (Exception ex)
                    {
                        modchart = false;
                    }
                    songLength = int.Parse(song["song_length"]);
                    try
                    {
                        forceProDrums = bool.Parse(song["pro_drums"]);

                    }
                    catch (Exception ex)
                    {
                        forceProDrums = false;
                    }
                    try { forceFiveLane = bool.Parse(song["five_lane_drums"]); }
                    catch (Exception ex)
                    {
                        forceFiveLane = false;
                    }
                    topLevelPlaylist = song["playlist"] == null ? "" : song["playlist"].ToLowerInvariant();
                    subPlaylist = song["sub_playlist"] == null ? "" : song["sub_playlist"].ToLowerInvariant();
                    albumTrack = 16000;

                    metadataCache[5] = song["charter"];
                    return true;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        public void GetTopLevelPlaylist()
        {
            metadata = new Metadata[7];
            if (topLevelPlaylist == "")
            {
                metadataCache[6] = string.Empty;
                string text = metadataCache[6];
                if (text != null)
                {
                    if (text.IndexOf("\\") == -1)
                    {
                        topLevelPlaylist = text;
                    }
                    else
                    {
                        topLevelPlaylist = text.Substring(0, text.IndexOf("\\"));
                    }
                }
                subPlaylist = "";
            }
            else
            {
                metadataCache[6] = topLevelPlaylist + ((subPlaylist != "") ? ("\\" + subPlaylist) : "");
            }
            for (int i = 0; i < 7; i++)
            {
                metadata[i] = cache.AddMetadata((MetadataType)i, metadataCache[i]);
            }
        }

    }
}
