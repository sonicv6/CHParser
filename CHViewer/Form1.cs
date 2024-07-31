using CHParser.Models;

namespace CHViewer
{
    public partial class Form1 : Form
    {
        private SongCache cache;
        public Form1()
        {
            InitializeComponent();
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            dataGridView1.ColumnCount = 23;
            dataGridView1.Columns[0].Name = "Name";
            dataGridView1.Columns[1].Name = "Artist";
            dataGridView1.Columns[2].Name = "Album";
            dataGridView1.Columns[3].Name = "Genre";
            dataGridView1.Columns[4].Name = "Year";
            dataGridView1.Columns[5].Name = "Charter";
            dataGridView1.Columns[6].Name = "Playlist";
            dataGridView1.Columns[7].Name = "isEnc";
        }

        private void ReloadCacheDisplay()
        {
            for (int i = 0; i < cache.songs.Count; i++)
            {
                SongEntry song = cache.songs[i];
                dataGridView1.Rows.Add(song.metadata[0].value, song.metadata[1].value, song.metadata[2].value, song.metadata[3].value, song.metadata[4].value, song.metadata[5].value, song.metadata[6].value, song.isEnc.ToString());
            }
        }

        private void loadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FileDialog fileDialog = new OpenFileDialog();
            fileDialog.Filter = "Cache Files (*.bin)|*.bin";
            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                cache = new SongCache(fileDialog.FileName);
                ReloadCacheDisplay();
            }
        }

        private void addSongToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (cache != null)
            {
                FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
                if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
                {
                    cache.AddSong(folderBrowserDialog.SelectedPath);
                    ReloadCacheDisplay();
                }
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Cache Files (*.bin)|*.bin";
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                cache.WriteToFile(saveFileDialog.FileName);
            }
        }
    }
}