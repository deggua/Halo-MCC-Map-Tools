using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace Forge_Map_Viewer
{
    public partial class frmMain : Form
    {
        const int MapTitleOffset   = 0xC0;
        const int MapTitleLen      = 0x100;
        const int MapDescOffset    = 0x1C0;
        const int MapDescLen       = 0x130;

        byte offset;
        byte[] fmap;
        byte[] mapTitleSection;
        byte[] mapDescSection;

        public frmMain()
        {
            InitializeComponent();
        }

        private void FrmMain_Load(object sender, EventArgs e)
        {
            string[] args = Environment.GetCommandLineArgs();


            if (args.Length > 1)
            {
                if (File.Exists(args[1]))
                {
                    fmap = File.ReadAllBytes(args[1]);
                }
                else
                {
                    MessageBox.Show("Cannot locate the map file:\n" + args[1], "Error");
                    Environment.Exit(0);
                    return;
                } 
            }
            else
            {
                MessageBox.Show("No map file provided. Open mvar file with Forge Map Viewer.", "Error");
                Environment.Exit(0);
                return;
            }

            if (HeaderValid(fmap))
            {
                //find offset
                if (fmap[MapTitleOffset] == 0)
                {
                    offset = 0x1;
                }
                else
                {
                    offset = 0x0;
                }
                //process map title string
                mapTitleSection = fmap.Skip(MapTitleOffset + offset).Take(MapTitleLen).ToArray();

                string mapTitle = string.Empty;
                for (int ii = 0; ii < MapTitleLen; ii += 2)
                    mapTitle += BitConverter.ToChar(mapTitleSection, ii);

                //process map description string
                mapDescSection = fmap.Skip(MapDescOffset + offset).Take(MapDescLen).ToArray();

                string mapDesc = string.Empty;
                for (int ii = 0; ii < MapDescLen; ii += 2)
                    mapDesc += BitConverter.ToChar(mapDescSection, ii);

                textName.Text = mapTitle;
                textDesc.Text = mapDesc;
            }
            else
            {
                MessageBox.Show("Map file is corrupt or invalid.", "Error");
            }
        }

        private void BtnRevEnd_Click(object sender, EventArgs e)
        {
            //process map title string
            mapTitleSection = ReverseEndian(mapTitleSection);

            string mapTitle = string.Empty;
            for (int ii = 0; ii < MapTitleLen; ii += 2)
                mapTitle += BitConverter.ToChar(mapTitleSection, ii);

            //process map description string
            mapDescSection = ReverseEndian(mapDescSection);

            string mapDesc = string.Empty;
            for (int ii = 0; ii < MapDescLen; ii += 2)
                mapDesc += BitConverter.ToChar(mapDescSection, ii);

            textName.Text = mapTitle;
            textDesc.Text = mapDesc;
        }

        private bool HeaderValid(byte[] mapfile)
        {
            byte[] HeaderTemplate = {
                0x5F, 0x62, 0x6C, 0x66, 0x00, 0x00, 0x00, 0x30, 0x00, 0x01, 0x00, 0x02,
                0xFF, 0xFE, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                0x63, 0x68, 0x64, 0x72, 0x00, 0x00, 0x02, 0xC0, 0x00, 0x0A, 0x00, 0x02
            };

            if (mapfile.Length < HeaderTemplate.Length)
                return false;

            bool isValid = true;
            for (int ii = 0; ii < HeaderTemplate.Length; ii++)
                isValid &= (HeaderTemplate[ii] == mapfile[ii]);

            return isValid;
        }

        private byte[] ReverseEndian(byte[] input)
        {
            byte[] output = new byte[input.Length];

            if (input.Length % 2 != 0)
                return output;

            for (int ii = 0; ii < input.Length; ii += 2)
            {
                output[ii] = input[ii + 1];
                output[ii + 1] = input[ii];
            }

            return output;
        }
    }
}
