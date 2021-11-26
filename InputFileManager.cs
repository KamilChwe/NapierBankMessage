using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;

namespace NapierBankMessage
{
    class InputFileManager
    {
        public void ImportFile(string fileName)
        {
            string header, body;
            string input = File.ReadAllText("input.txt");
            string[] array = input.Split(',');

            MessageManager messageMan = new MessageManager();
            HeaderManager headerMan = new HeaderManager();

            for (int i = 0; i < array.Length-1; i++)
            {
                header = array[i];

                // make sur
                body = array[i+1];

                string messageType = headerMan.DetectType(header);
                messageMan.StartProcessing(messageType, header, body);
            }
        }
    }


    public class InputFile
    {
        public string Header { get; set; }
        public string Body { get; set; }
    }
}
