using PeNet;
using SharpDisasm;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ExeDisassembler
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        public byte[] FileToByteArray(string fileName, PeFile _peFile)
        {
            uint PAddress = 0;
            uint PSize = 0;
            byte[] buff = null;
            FileStream fs = new FileStream(fileName,
                                           FileMode.Open,
                                           FileAccess.Read);
            BinaryReader br = new BinaryReader(fs);
            foreach (var sec in _peFile.ImageSectionHeaders)
            {
                Name = PeNet.Utilities.FlagResolver.ResolveSectionName(sec.Name);
                if (Name == ".text")
                {
                    PAddress = sec.PointerToRawData;
                    PSize = sec.SizeOfRawData;
                    break;
                }

            }


            br.ReadBytes((int)PAddress);
            buff = br.ReadBytes((int)PSize);
            return buff;
        }
        private void button1_Click(object sender, EventArgs e)
        {
            ArchitectureMode mode = ArchitectureMode.x86_32;
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                Disassembler.Translator.IncludeAddress = true;
                Disassembler.Translator.IncludeBinary = true;
                if (!PeFile.IsPEFile(openFileDialog1.FileName))
                {

                    return;
                }

                PeFile _peFile = new PeFile(openFileDialog1.FileName);
                if (_peFile.Is64Bit)
                    mode = ArchitectureMode.x86_64;

                byte[] b = FileToByteArray(openFileDialog1.FileName, _peFile);
                var disasm = new Disassembler(b, mode, 0, true);
                // Disassemble each instruction and output to console
                richTextBox1.Text = "";
                List<SharpDisasm.Instruction> dis = disasm.DisassembleList();

                for (int i = 0; i < dis.Count; i++)
                {
                    richTextBox1.AppendText(dis[i].ToString() + "\n");
                }


            }
        }
    }
}
