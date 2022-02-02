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
using System.Security.Cryptography;

namespace Lock_Screen
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            passBox.Top = 1080 - passBox.Size.Height;
            passBox.Left = 1920 - passBox.Size.Width;
            FormBorderStyle = FormBorderStyle.None;
            WindowState = FormWindowState.Maximized;
            try 
            {
                BackgroundImage = Image.FromFile(Path.Combine(Application.StartupPath, @"bg.png"));
                Console.WriteLine("png");
            }
            catch
            {
                try
                {
                    BackgroundImage = Image.FromFile(Path.Combine(Application.StartupPath, @"bg.jpg"));
                    Console.WriteLine("jpg");
                }
                catch
                {
                    BackColor = Color.Black;
                    Console.WriteLine("black");
                }
            }
        }

        private void passBox_KeyDown(object sender, KeyEventArgs e)
        {
            Console.Write(e.KeyCode);
            if (e.KeyCode == Keys.Enter)
            {
                string res = decrypt();
                Console.WriteLine(res.Remove(res.Length-2));
                Console.WriteLine(passBox.Text);
                Console.WriteLine(res.Remove(res.Length-2).Equals(passBox.Text));
                if (passBox.Text.Equals(res.Remove(res.Length-2)))
                {
                    Application.Exit();
                }
                passBox.Text = "";
            }
        }

        private string decrypt()
        {
            try
            {
                using (FileStream fileStream = new FileStream("pw.txt", FileMode.Open))
                {
                    using (Aes aes = Aes.Create())
                    {
                        byte[] iv = new byte[aes.IV.Length];
                        int numBytesToRead = aes.IV.Length;
                        int numBytesRead = 0;
                        while (numBytesToRead > 0)
                        {
                            int n = fileStream.Read(iv, numBytesRead, numBytesToRead);
                            if (n == 0) break;

                            numBytesRead += n;
                            numBytesToRead -= n;
                        }

                        byte[] key =
                        {
                0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08,
                0x09, 0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16
            };

                        using (CryptoStream cryptoStream = new CryptoStream(
                           fileStream,
                           aes.CreateDecryptor(key, iv),
                           CryptoStreamMode.Read))
                        {
                            using (StreamReader decryptReader = new StreamReader(cryptoStream))
                            {
                                return decryptReader.ReadToEnd();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"The decryption failed. {ex}");
                return null;
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
        }

        private void passBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            Console.Write("¦");

            if (e.KeyChar == '¦')
            {
                Application.Exit();
            }
        }
    }
}
