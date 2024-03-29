﻿using System;
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
using System.Text.RegularExpressions;

namespace NapierBankMessage
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string messageType = "None";
        string header, body;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void txtHeader_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Setup the file for HeaderManager
            HeaderManager header = new HeaderManager();

            // Call the function to chec the type of the message
            messageType = header.DetectType(txtHeader.Text);

            // Tell the user what type of message they just detected
            txtMessageType.Content = messageType + " Message ID Detected!";
        }

        private void btnEnd_Click(object sender, RoutedEventArgs e)
        {
            EndOfSession newScreen = new EndOfSession();

            newScreen.Show();
            this.Close();
        }

        private void btnFile_Click(object sender, RoutedEventArgs e)
        {
            InputFileManager file = new InputFileManager();

            file.ImportFile("input");
        }

        private void btnSubmit_Click(object sender, RoutedEventArgs e)
        {
            header = txtHeader.Text;
            body = txtBody.Text;

            // Check if all of the fields are correctly filled in
            if(messageType == "Tweet" && body.Length > 140 || messageType == "SMS" && body.Length > 140)
            {
                MessageBox.Show(messageType + " Messages can only be 140 characters long!");
            }
            else if(messageType == "EMail" && body.Length > 1028)
            {
                MessageBox.Show(messageType = " Messages can only be 1028 characters long!");
            }
            else if(txtHeader.Text.Length > 10 || header.Length < 9)
            {
                MessageBox.Show("The header must be 10 characters long!\n (Example: T123456789)");
            }
            else
            {
                MessageManager processing = new MessageManager();

                processing.StartProcessing(messageType, header, body);
            }

        }
    }
}
