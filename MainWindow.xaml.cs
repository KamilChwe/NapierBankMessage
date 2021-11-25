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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NapierBankMessage
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string messageType = "None";

        public MainWindow()
        {
            InitializeComponent();
        }

        private void txtHeader_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Setup the file for HeaderManager
            HeaderManager header = new HeaderManager();

            // Call the function to chec the type of the message
            string messageType = header.DetectType(txtHeader.Text);

            // Tell the user what type of message they just detected
            txtMessageType.Content = messageType + " Message ID Detected!";
        }

        private void btnSubmit_Click(object sender, RoutedEventArgs e)
        {
            // Check if all of the fields are correctly filled in
            if(messageType == "Tweet" || messageType == "SMS" && txtBody.Text.Length > 140)
            {
                MessageBox.Show(messageType + " Messages can only be 140 characters long!");
            }
            else if(messageType == "EMail" && txtBody.Text.Length > 1028)
            {
                MessageBox.Show(messageType = " Messages can only be 1028 characters long!");
            }
            else if(txtHeader.Text.Length > 10 || txtHeader.Text.Length < 9)
            {
                MessageBox.Show("The header must be 10 characters long!\n (Example: T123456789)");
            }
        }
    }
}
