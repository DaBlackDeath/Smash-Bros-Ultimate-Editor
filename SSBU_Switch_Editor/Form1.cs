using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SSBU_Switch_Editor
{
    public partial class Form1 : Form
    {
        public string Savegame;

        public Form1()
        {
            InitializeComponent();
        }

        public static byte[] BLKDTH_STR2BA(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                .Where(x => x % 2 == 0)
                .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                .ToArray();
        }

        public void BLKDTH_PatchFile(string filename, int offset, byte[] value)
        {
            var fs = new FileStream(filename, FileMode.Open);
            var br = new BinaryWriter(fs);
            fs.Position = offset;
            br.Write(value);
            fs.Close();
            br.Close();
        }

        private void BLKDTH_get_openfile()
        {
            var openFd = new OpenFileDialog();
            if (openFd.ShowDialog() == DialogResult.OK) Savegame = openFd.FileName;
        }

        private void BLKDTH_get_data()
        {
            {
                var savegameFs = new FileStream(Savegame, FileMode.Open);
                var savegameBr = new BinaryReader(savegameFs);
                var length = savegameFs.Length;

                // Gold
                savegameBr.BaseStream.Position = offsets.Gold;
                gold_box.Text = BitConverter.ToUInt32(savegameBr.ReadBytes(4), 0).ToString();

                // SP
                savegameBr.BaseStream.Position = offsets.Sp;
                sp_box.Text = BitConverter.ToUInt32(savegameBr.ReadBytes(4), 0).ToString();

                // Tickets
                savegameBr.BaseStream.Position = offsets.Tickets;
                tickets_box.Text = int.Parse(BitConverter.ToString(savegameBr.ReadBytes(1), 0), NumberStyles.HexNumber).ToString();

                // Snacks [S]
                savegameBr.BaseStream.Position = offsets.Snack[0];
                Snack_S_Box.Text = BitConverter.ToUInt16(savegameBr.ReadBytes(2), 0).ToString();

                // Snacks [M]
                savegameBr.BaseStream.Position = offsets.Snack[1];
                Snack_M_Box.Text = BitConverter.ToUInt16(savegameBr.ReadBytes(2), 0).ToString();

                // Snacks [L]
                savegameBr.BaseStream.Position = offsets.Snack[2];
                Snack_L_Box.Text = BitConverter.ToUInt16(savegameBr.ReadBytes(2), 0).ToString();

                // Hammers
                savegameBr.BaseStream.Position = offsets.Hammers;
                Hammers_Box.Text = int.Parse(BitConverter.ToString(savegameBr.ReadBytes(1), 0), NumberStyles.HexNumber).ToString();

                // Skill Points
                savegameBr.BaseStream.Position = offsets.SkillPoints;
                SkillPoints_Box.Text = BitConverter.ToUInt16(savegameBr.ReadBytes(2), 0).ToString();

                savegameBr.Close();
            }
        }

        private void BLKDTH_set_data()
        {
            FileStream updateSaveOpen = null;
            BinaryWriter updateSaveWrite = null;
            updateSaveOpen = new FileStream(Savegame, FileMode.Open);
            updateSaveWrite = new BinaryWriter(updateSaveOpen);
            
            //Gold
            updateSaveOpen.Position = offsets.Gold;
            updateSaveWrite.Write(BitConverter.GetBytes(uint.Parse(gold_box.Text)));

            //SP
            updateSaveOpen.Position = offsets.Sp;
            updateSaveWrite.Write(BitConverter.GetBytes(uint.Parse(sp_box.Text)));

            //Tickets
            updateSaveOpen.Position = offsets.Tickets;
            updateSaveWrite.Write(BitConverter.GetBytes(uint.Parse(tickets_box.Text)));

            //Snacks [S]
            updateSaveOpen.Position = offsets.Snack[0];
            updateSaveWrite.Write(BitConverter.GetBytes(ushort.Parse(Snack_S_Box.Text)));

            //Snacks [M]
            updateSaveOpen.Position = offsets.Snack[1];
            updateSaveWrite.Write(BitConverter.GetBytes(ushort.Parse(Snack_M_Box.Text)));

            //Snacks [L]
            updateSaveOpen.Position = offsets.Snack[2];
            updateSaveWrite.Write(BitConverter.GetBytes(ushort.Parse(Snack_L_Box.Text)));

            //Hammers
            updateSaveOpen.Position = offsets.Hammers;
            updateSaveWrite.Write(BLKDTH_STR2BA(int.Parse(Hammers_Box.Text).ToString("X2")));

            //Skill Points
            updateSaveOpen.Position = offsets.SkillPoints;
            updateSaveWrite.Write(BitConverter.GetBytes(uint.Parse(SkillPoints_Box.Text))); 
            
            updateSaveOpen.Close();
        }

        private void BLKDTH_check_max()
        {
            if (int.Parse(SkillPoints_Box.Text) > 3000)
            {
                SkillPoints_Box.Text = "3000";
            }
            if (int.Parse(Snack_S_Box.Text) > 65535)
            {
                Snack_S_Box.Text = "65535";
            }
            if (int.Parse(Snack_M_Box.Text) > 65535)
            {
                Snack_M_Box.Text = "65535";
            }
            if (int.Parse(Snack_L_Box.Text) > 65535)
            {
                Snack_L_Box.Text = "65535";
            } 

        }

        private void mass_unlock()
        {
            byte[] b = { 0xFF };
            for (int i = 0; i < 0x2A4; i++)
            {
                BLKDTH_PatchFile(Savegame, 0 + i, b);
            }
        }

        private void milestones_unlock()
        {
            byte[] b = { 0xFF };
            for (int i = 0; i < 0x30; i++)
            {
                BLKDTH_PatchFile(Savegame, 0 + i, b);
            }
        }

        private void open_file_Click(object sender, EventArgs e)
        {
            BLKDTH_get_openfile();
            if (string.IsNullOrEmpty(Savegame))
            {
                MessageBox.Show("no savegame selected");
            }
            else
            {
                BLKDTH_get_data();
            }


        }

        private void save_file_Click(object sender, EventArgs e)
        {
             
            BLKDTH_check_max();
            BLKDTH_set_data();
            if (unlock_roster_checkbox.Checked == true)
            {
                BLKDTH_PatchFile(Savegame, offsets.Roster, offsets.UnlockChars);
            }
            if (unlock_cores_checkbox.Checked == true)
            {
                BLKDTH_PatchFile(Savegame, offsets.Cores, offsets.UnlockCores);
            }
             if (unlock_spirits_checkbox.Checked == true)
             {
                 BLKDTH_PatchFile(Savegame, offsets.Spirits, offsets.UnlockSpirits);
             }
             if (mass_unlock_checkbox.Checked == true)
             {
                 mass_unlock();
             }
             if (unlock_milestones_checkbox.Checked == true)
             {
                 milestones_unlock();
             }
             if (unlock_miiclothes_checkbox.Checked == true)
             {
                 BLKDTH_PatchFile(Savegame, offsets.MiiClothes, offsets.UnlockMiiClothes);
             }
             if (unlock_fullsize_checkbox.Checked == true)
             {
                 BLKDTH_PatchFile(Savegame, offsets.FullSizeSpiritBoard, offsets.UnlockFullSizeSpiritBoard);
             }
             if (unlock_stickers_checkbox.Checked == true)
             {
                 BLKDTH_PatchFile(Savegame, offsets.AllSpiritsandCollectionStickers, offsets.UnlockAllSpiritsandCollectionStickers);
             }

            MessageBox.Show("New Data Saved ...");
        }

        private void max_all_Click(object sender, EventArgs e)
        {
            gold_box.Text = "9999999";
            sp_box.Text = "9999999";
            SkillPoints_Box.Text = "3000";
            Snack_S_Box.Text = "65535";
            Snack_M_Box.Text = "65535";
            Snack_L_Box.Text = "65535";
            Hammers_Box.Text = "99";
            tickets_box.Text = "99";


        }

    }
}