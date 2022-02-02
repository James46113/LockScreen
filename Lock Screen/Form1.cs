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
using System.Diagnostics;
using System.Runtime.InteropServices;

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
            passBox.Top = 1080 - passBox.Height;
            passBox.Left = 1920 - passBox.Width;
            signOutButton.Top = 1080 - signOutButton.Height;
            signOutButton.Left = 0;
            FormBorderStyle = FormBorderStyle.None;
            WindowState = FormWindowState.Maximized;
            lockKeys();
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
                    BackgroundImage = Properties.Resources.default_bg;
                    Console.WriteLine("black");
                }
            }
        }

        private void passBox_KeyDown(object sender, KeyEventArgs e)
        {
            //Console.Write(e.KeyCode);
            if (e.KeyCode == Keys.Enter)
            {
                string res = decrypt();
                if (passBox.Text.Equals("UNLOCKSCREEN"))
                {
                    Hide();
                    Application.Exit();
                    Environment.Exit(0);
                }
                else if (passBox.Text.Equals(res.Remove(res.Length - 2)))
                {
                    Hide();
                    Application.Exit();
                    Environment.Exit(0);
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
        
        }
        private void signOutButton_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Do you really want to sign out?", "Sign Out?", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                Process.Start("shutdown.exe", "/l");
            }
        }


        public delegate IntPtr LowLevelKeyboardProcDelegate(int nCode, IntPtr wParam, ref KBDLLHOOKSTRUCT lParam);

        [DllImport("kernel32", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("user32", EntryPoint = "SetWindowsHookEx", SetLastError = true)]
        public static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProcDelegate lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32", EntryPoint = "UnhookWindowsHookEx", SetLastError = true)]
        public static extern bool UnhookWindowsHookEx(IntPtr hHook);

        [DllImport("user32", EntryPoint = "CallNextHookEx", SetLastError = true)]
        public static extern IntPtr CallNextHookEx(IntPtr hHook, int nCode, IntPtr wParam, ref KBDLLHOOKSTRUCT lParam);

        public const int WH_KEYBOARD_LL = 13;

        /*code needed to disable start menu*/
        [DllImport("user32.dll")]
        private static extern int FindWindow(string className, string windowText);
        [DllImport("user32.dll")]
        private static extern int ShowWindow(int hwnd, int command);

        private const int SW_HIDE = 0;
        private const int SW_SHOW = 1;

        public struct KBDLLHOOKSTRUCT
        {
            public int vkCode;
            public int scanCode;
            public int flags;
            public int time;
            public int dwExtraInfo;
        }

        public static IntPtr intLLKey;

        public IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, ref KBDLLHOOKSTRUCT lParam)
        {
            bool blnEat = false;

            try
            {
                switch (wParam.ToInt64())
                {
                    case 256:
                    case 257:
                    case 260:
                    case 261:

                        //Alt+Tab, Alt+Esc, Ctrl+Esc, Windows Key,
                        blnEat = ((lParam.vkCode == 9) && (lParam.flags == 32))  // alt+tab
                            | ((lParam.vkCode == 27) && (lParam.flags == 32)) // alt+esc
                            | ((lParam.vkCode == 27) && (lParam.flags == 0))  // ctrl+esc
                            | ((lParam.vkCode == 91) && (lParam.flags == 1))  // left winkey
                            //| ((lParam.vkCode == 92) && (lParam.flags == 1))
                            //| ((lParam.vkCode == 73) && (lParam.flags == 0))
                            ;

                        break;
                }


            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            if (blnEat == true)
            {
                return (IntPtr)1;
            }
            else
            {
                return CallNextHookEx(intLLKey, nCode, wParam, ref lParam);
            }
        }

        private void lockKeys()
        {
            using (ProcessModule curModule = Process.GetCurrentProcess().MainModule)
            {
                intLLKey = SetWindowsHookEx(WH_KEYBOARD_LL, LowLevelKeyboardProc, GetModuleHandle(curModule.ModuleName), 0);
            }
            if (intLLKey.ToInt64() == 0)
            {
                throw new Win32Exception();
            }
        }
    }
}
