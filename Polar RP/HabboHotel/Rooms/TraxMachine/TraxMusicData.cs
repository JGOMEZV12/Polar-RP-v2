using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Polar.HabboHotel.Rooms.TraxMachine
{
    public class TraxMusicData
    {
        public int Id;
        public string CodeName;
        public string Name;
        public string Artist;
        public string Data;
        public int Length;

        public TraxMusicData(int id, string code, string name, string art, string data, int len)
        {
            Id = id;
            CodeName = code;
            Name = name;
            Artist = art;
            Data = data;
            Length = len;
        }

        public static TraxMusicData Parse(DataRow row)
        {
            int id = Convert.ToInt32(row["id"]);
            string codename = row.Table.Columns.Contains("codename") ? row["codename"].ToString() : "track_" + id;
            string name = row.Table.Columns.Contains("name") ? row["name"].ToString() : "Unknown Track";
            string artist = row.Table.Columns.Contains("artist") ? row["artist"].ToString() : "Unknown Artist";
            string data = row.Table.Columns.Contains("song_data") ? row["song_data"].ToString() : (row.Table.Columns.Contains("data") ? row["data"].ToString() : "");
            int length = row.Table.Columns.Contains("length") ? Convert.ToInt32(row["length"]) : 0;

            return new TraxMusicData(id, codename, name, artist, data, length);
        }
    }
}