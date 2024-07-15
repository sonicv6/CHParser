using CHParser.Models;

namespace CHViewer
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            SongCache cache = new SongCache("songcache.bin");
            cache.AddSong("Obituary - Turned Inside Out (Roadsounder99)");
            dataGridView1.ColumnCount = 23;
            dataGridView1.Columns[0].Name = "Name";
            dataGridView1.Columns[1].Name = "Artist";
            dataGridView1.Columns[2].Name = "Album";
            dataGridView1.Columns[3].Name = "Genre";
            dataGridView1.Columns[4].Name = "Year";
            dataGridView1.Columns[5].Name = "Charter";
            dataGridView1.Columns[6].Name = "Playlist";
            dataGridView1.Columns[7].Name = "isEnc";
            for (int i = 0; i < cache.songs.Count; i++)
            {
                SongEntry song = cache.songs[i];
                dataGridView1.Rows.Add(new string[] { song.metadata[0].value, song.metadata[1].value, song.metadata[2].value, song.metadata[3].value, song.metadata[4].value, song.metadata[5].value, song.metadata[6].value, song.isEnc.ToString() });
            }
        }
    }
}