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
using Newtonsoft.Json;
using System.IO;

namespace NapierBankMessage
{
    /// <summary>
    /// Interaction logic for EndOfSession.xaml
    /// </summary>
    public partial class EndOfSession : Window
    {
        public EndOfSession()
        {
            InitializeComponent();
            PopulateLists();
        }

        void PopulateLists()
        {
            // Populate Trending List
            HashList trendingList = JsonConvert.DeserializeObject<HashList>(File.ReadAllText("Hashtags.json"));

            foreach(var hashtag in trendingList.Hashtags)
            {
                lstTrending.Items.Add(hashtag.hashtag + " " + "(" + hashtag.hashtagCount + ")");
            }

            // Populate Mentions List
            MentionsList mentions = JsonConvert.DeserializeObject<MentionsList>(File.ReadAllText("Mentions.json"));

            foreach(var mention in mentions.mentionsList)
            {
                lstMentions.Items.Add(mention.mention);
            }

            // Populate the SIR List
            SIRList sir = JsonConvert.DeserializeObject<SIRList>(File.ReadAllText("SIR.json"));

            foreach(var item in sir.SIR)
            {
                lstSir.Items.Add("Sort Code: " + item.sortCode + "\nNoI: " + item.natureOfIncident);
            }    
        }
    }
}
