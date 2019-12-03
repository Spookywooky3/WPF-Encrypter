using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace FileEncrypter
{
    /// <summary>
    /// Interaction logic for PasswordGenerator.xaml
    /// </summary>
    public partial class PasswordGenerator : Window
    {
        public PasswordGenerator()
        {
            InitializeComponent();
        }

        private void slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {

        }

        private void pwGenButton_Click(object sender, RoutedEventArgs e)
        {
            // Thanks stackoverflow
            Random rand = new Random();

            const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!@#$%^&*()_+.,<>";
            passwordTextBox.Text = new string(Enumerable.Repeat(chars, Convert.ToInt32(slider.Value)).Select(s => s[rand.Next(s.Length)]).ToArray());
        }
    }
}
