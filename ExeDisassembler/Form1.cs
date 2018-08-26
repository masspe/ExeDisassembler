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
        PeFile _peFile;

        public Form1()
        {
            InitializeComponent();
        }
        public byte[] FileToByteArray(string fileName, PeFile _peFile,int PAddress, int PSize)
        {
            
            byte[] buff = null;
            FileStream fs = new FileStream(fileName,
                                           FileMode.Open,
                                           FileAccess.Read);
            BinaryReader br = new BinaryReader(fs);
           


            br.ReadBytes(PAddress);
            buff = br.ReadBytes(PSize);
            return buff;
        }

        public void readTextSection()
        {
            uint PAddress = 0;
            uint PSize = 0;
            ArchitectureMode mode = ArchitectureMode.x86_32;
            if (_peFile.Is64Bit)
                mode = ArchitectureMode.x86_64;
            foreach (var sec in _peFile.ImageSectionHeaders)
            {
                Name = PeNet.Utilities.FlagResolver.ResolveSectionName(sec.Name);
                if (Name == ".text")
                {
                    PAddress = sec.PointerToRawData;
                    PSize = sec.SizeOfRawData;

                    byte[] b = FileToByteArray(openFileDialog1.FileName, _peFile, (int)PAddress, (int)PSize);

                    var disasm = new Disassembler(b, mode, 0, true);
                    // Disassemble each instruction and output to console
                    richTextBox1.Text = "";
                    List<SharpDisasm.Instruction> dis = disasm.DisassembleList();
                    richTextBox1.Text = "";
                    progressBar1.Visible = true;
                    progressBar1.Minimum = 0;
                    progressBar1.Maximum = dis.Count;
                    for (int i = 0; i < dis.Count; i++)
                    {
                        richTextBox1.AppendText(string.Format("{0:x16}  ", (long)_peFile.ImageNtHeaders.OptionalHeader.ImageBase + sec.VirtualAddress + (long) dis[i].Offset) + dis[i].ToString() + "\n");
                        progressBar1.Value = i;
                    }
                    progressBar1.Visible = false;
                    break;
                }

            }
           
        }

        public void readDataSection(string sname, RichTextBox r)
        {
            uint PAddress = 0;
            uint PSize = 0;
          
            foreach (var sec in _peFile.ImageSectionHeaders)
            {
                Name = PeNet.Utilities.FlagResolver.ResolveSectionName(sec.Name);
                r.Text = "";
                if (Name == sname)
                {
                    PAddress = sec.PointerToRawData;
                    PSize = sec.SizeOfRawData;

                    byte[] b = FileToByteArray(openFileDialog1.FileName, _peFile, (int)PAddress, (int)PSize);

                    string hstring = "";
                    string rstring = "";
                    progressBar1.Visible = true;
                    progressBar1.Minimum = 0;
                    progressBar1.Maximum = b.Length;
                    for (int i = 0; i < b.Length; i++)
                    {
                        hstring += string.Format("{0:x2}:", b[i]);
                        if (b[i] > 0)
                            if (b[i] != 13 && b[i] != 10)
                                rstring += (char)b[i];
                            else
                                rstring += "";
                        

                        if ((i+1) % 16 == 0 )
                        {
                            r.AppendText(string.Format("{0:x16}  ", (long) _peFile.ImageNtHeaders.OptionalHeader.ImageBase + sec.VirtualAddress +  (i-16)) + hstring +"  "+ rstring + "\n");
                            hstring = "";
                            rstring = "";
                        }
                        progressBar1.Value = i;
                    }
                    progressBar1.Visible = false;

                    break;
                }

            }
            
        }
        private void button1_Click(object sender, EventArgs e)
        {
            
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                this.Cursor = Cursors.WaitCursor;
                Disassembler.Translator.IncludeAddress = true;
                Disassembler.Translator.IncludeBinary = true;
                if (!PeFile.IsPEFile(openFileDialog1.FileName))
                {

                    return;
                }

                _peFile = new PeFile(openFileDialog1.FileName);


                readTextSection();
                readDataSection(".rdata", richTextBox2);
                readDataSection(".data", richTextBox3);
                readDataSection(".rsrc", richTextBox2);
                this.Cursor = Cursors.Default;

            }
        }
    }
}
