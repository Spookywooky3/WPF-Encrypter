using Microsoft.Win32;
using System;
using System.IO;
using System.Collections.Generic;
using System.Windows;
using System.Security.Cryptography;
using System.Threading;

namespace FileEncrypter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
#if DEBUG
        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool AllocConsole();
#endif

        private List<string> filePaths;
        public MainWindow()
        {
            InitializeComponent();
            filePaths = new List<string>();
#if DEBUG
            AllocConsole();
#endif
        }

        private void decryptButton_Click(object sender, RoutedEventArgs e)
        {
            new Thread(new ThreadStart(Decrypt)).Start();
        }

        private void encryptButton_Click(object sender, RoutedEventArgs e)
        {
            new Thread(new ThreadStart(Encrypt)).Start();
        }

        private void selectFilesButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fileDiag = new OpenFileDialog();

            fileDiag.DefaultExt = ".*";
            fileDiag.FileName = "File";
            fileDiag.Filter = "(.*)|*.*";
            fileDiag.Title = "File To Encrypt";
            fileDiag.Multiselect = true;

            bool? result = fileDiag.ShowDialog();
            if (result == true)
            {
                foreach (var fileName in fileDiag.FileNames)
                {
                    filePaths.Add(fileName);
                    listBox.Items.Add(GetFileNameFromPath(fileName));
                }
            }
        }

        private void passwordGenButton_click(object sender, RoutedEventArgs e)
        {
            PasswordGenerator pwGenObj = new PasswordGenerator();
            pwGenObj.Show();
        }

        private void Decrypt()
        {
            OpenFileDialog fileDiag = new OpenFileDialog();

            fileDiag.DefaultExt = ".*";
            fileDiag.FileName = "File";
            fileDiag.Filter = "(.*)|*.*";
            fileDiag.Title = "File To Decrypt";
            fileDiag.Multiselect = true;

            bool? result = fileDiag.ShowDialog();
            if (result == true)
            {
                foreach (var file in fileDiag.FileNames)
                {
                    byte[] decryptedBytes = DecryptBytes(file);
                    File.WriteAllBytes(file.Replace(".clover", string.Empty), decryptedBytes);

#if DEBUG
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine($"{ConsoleDateTag()} {GetFileNameFromPath(file)} decrypted!");
                    Console.ResetColor();
#endif
                }
            }
        }

        private void Encrypt()
        {
            foreach (var file in filePaths)
            {
                byte[] encryptedBytes = EncryptBytes(file);
                File.WriteAllBytes($"{GetFileNameFromPath(file)}.clover", encryptedBytes);

#if DEBUG
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"{ConsoleDateTag()} {GetFileNameFromPath(file)} encrypted!");
                Console.ResetColor();
#endif
            }
            listBox.Items.Clear();
        }

        // Theres probably already a c# function to do this but it was honestly
        // just easier for me to do this rather than search for it
        private string GetFileNameFromPath(string filePath)
        {
            string[] file = filePath.Split('\\');
            return file[file.Length - 1];
        }

        private string ConsoleDateTag()
        {
            string dateTag = $"[{DateTime.Now.ToString("MM:dd-HH:mm")}]";
            return dateTag;
        }

        private byte[] EncryptBytes(string file)
        {
            byte[] encryptedBytes = { 0x0 };
            Dispatcher.Invoke(() =>
            {
                byte[] byteArray = File.ReadAllBytes(file);
                
                using (Aes aes = new AesCng())
                {
                    PasswordDeriveBytes pwDerivedBytes = new PasswordDeriveBytes(passwordTextBox.Text, new byte[] { 0x32, 0xF4, 0x83, 0xC });

                    aes.Key = pwDerivedBytes.GetBytes(aes.KeySize / 8);
                    aes.IV = pwDerivedBytes.GetBytes(aes.BlockSize / 8);

                    using (MemoryStream memStream = new MemoryStream())
                    {
                        using (CryptoStream cryptoStream = new CryptoStream(memStream, aes.CreateEncryptor(), CryptoStreamMode.Write))
                        {
                            cryptoStream.Write(byteArray, 0, byteArray.Length);
                            cryptoStream.Close();
                            encryptedBytes = memStream.ToArray();
#if DEBUG
                            Console.ForegroundColor = ConsoleColor.Cyan;
                            Console.WriteLine($"{ConsoleDateTag()} Encrypting {GetFileNameFromPath(file)} using password {passwordTextBox.Text}");
                            Console.ResetColor();
#endif
                            return memStream.ToArray();
                        }
                    }
                }
            });
            return encryptedBytes;
        }

        private byte[] DecryptBytes(string file)
        {
            byte[] decryptedBytes = { 0x0 };
            Dispatcher.Invoke(() =>
            {
                byte[] byteArray = File.ReadAllBytes(file);

                using (Aes aes = new AesCng())
                {
                    PasswordDeriveBytes pwDerivedBytes = new PasswordDeriveBytes(passwordTextBox.Text, new byte[] { 0x32, 0xF4, 0x83, 0xC });

                    aes.Key = pwDerivedBytes.GetBytes(aes.KeySize / 8);
                    aes.IV = pwDerivedBytes.GetBytes(aes.BlockSize / 8);

                    using (MemoryStream memStream = new MemoryStream())
                    {
                        using (CryptoStream cryptoStream = new CryptoStream(memStream, aes.CreateDecryptor(), CryptoStreamMode.Write))
                        {
                            cryptoStream.Write(byteArray, 0, byteArray.Length);
                            cryptoStream.Close();
#if DEBUG
                            Console.ForegroundColor = ConsoleColor.Cyan;
                            Console.WriteLine($"{ConsoleDateTag()} Decrypting {GetFileNameFromPath(file)} using password {passwordTextBox.Text}");
                            Console.ResetColor();
#endif
                            return memStream.ToArray();
                        }
                    }
                }
            });
            return decryptedBytes;
        }
    }
}
