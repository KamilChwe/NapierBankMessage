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

            #region Old Input Attempt
            /* Old Input Attempt

            // The CSV doesn't have a header so we must specify this
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = false,
            };

            // Use the textword CSV
            using (var reader = new StreamReader("input.csv"))
            {
                // Open the CSV with the configs set
                using (var csv = new CsvReader(reader, config))
                {
                    var records = csv.GetRecords<InputFile>().ToList();

                    for(int i = 0; i < records.Count; i++)
                    {
                        header = records[i].Header;
                        body = records[i].Body;

                        string messageType = headerMan.DetectType(header);
                        messageMan.StartProcessing(messageType, header, body);
                    }
                }
            }
            */
            #endregion
        }
    }


    public class InputFile
    {
        public string Header { get; set; }
        public string Body { get; set; }
    }
}
