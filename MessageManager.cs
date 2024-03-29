﻿using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using System.IO;
using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;
using System.Windows;

namespace NapierBankMessage
{
    // This class is responsible for all of the Body (Message) processing
    class MessageManager
    {
        // Setting up Global Vars
        string header;
        string sender;
        string subject;
        string body;
        string messageType;
        string natureOfIncident;

        // Basically acts like a "Master" functions, gets all the variables in place
        public void StartProcessing(string messageTypeIn, string headerIn, string bodyIn)
        {
            messageType = messageTypeIn;
            header = headerIn;
            body = bodyIn;

            FindDetails();
        }
        

        public void FindDetails()
        {
            // setup the Nature of Incident array
            string[] natureOfIncidentList = { "Theft", "Staff Attack", "ATM Theft", "Raid", "Customer Attack", "Staff Abuse", "Bomb Threat", "Terrorism", "Suspicious Incident", "Intelligence", "Cash Loss" };

            // RegEx really came in handy since I could easily hack my way through each string extracing anything I needed.
            // Thus I have not only learned this script but also heavily used it...
            #region RegEx
            // Using RegEx to find the details in the body

            // Regex for finding an international phone number (Example: +44 1234567890)
            Regex smsRegex = new Regex(@"\+((?:9[679]|8[035789]|6[789]|5[90]|42|3[578]|2[1-689])|9[0-58]|8[1246]|6[0-6]|5[1-8]|4[013-9]|3[0-469]|2[70]|7|1)(?:\W*\d){0,13}\d");
            // Regex for finding Twitter Handle (Example: @TestUser)
            Regex twitterRegex = new Regex(@"@([\w]{1,15})");
            // Regex for Hashtags (Example: #SoCool)
            Regex hashtagRegex = new Regex(@"#([\w]+)");
            // Regex for finding an Email Address (Example: TestUser@gmail.com)
            Regex emailRegex = new Regex(@"([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)");
            // Regex for finding Links (Example: www.youtube.com)
            Regex urlRegex = new Regex(@"www\.([\w]+)\.([\w]{2,3})");
            // Regex for finding the Sort Code
            Regex SortCodeRegex = new Regex(@"([\d]{2})\-([\d]{2})\-([\d]{2})");
            Regex subjectRegex = new Regex(@"Subject: ([\w]{1,20})");


            // Getting the matches for Regex
            Match smsMatch = smsRegex.Match(body);

            // Tweets may contain multiple handles so we need a collection of them
            MatchCollection twitterMatches = twitterRegex.Matches(body);
            // Same goes for Hashtags
            MatchCollection hashtagMatches = hashtagRegex.Matches(body);

            Match emailMatch = emailRegex.Match(body);
            MatchCollection urlMatches = urlRegex.Matches(body);
            Match SortCodeMatch = SortCodeRegex.Match(body);
            Match subjectMatch = subjectRegex.Match(body);
            #endregion

            // Preparing the Header, Sender, Subject, and Body
            switch (messageType)
            {
                case "SMS":
                    // The Sender is the Phone Number in the body
                    sender = smsMatch.Value;

                    // Call these functions to check the Body and save any necessary information
                    ConvertAbbreviations(body);
                    AddMessages();
                    break;

                case "EMail":
                    // Sender is the only email in the body
                    sender = emailMatch.Value;
                    subject = subjectMatch.Value;

                    // Censor URLs with <Blocked Link>
                    for(int i = 0; i < urlMatches.Count; i++)
                    {
                        // Use StringBuilder to replace the old string with the new one
                        StringBuilder builder = new StringBuilder(body);
                        builder.Replace(urlMatches[i].Value, "<Quarantined URL>");
                        // Save this modified message as the body
                        body = builder.ToString();
                    }

                    // Checks if the EMail is a SIR email or SEM
                    // If the Body contains a sort code, assume its a SIR
                    if (body.Contains("Sort Code"))
                    {
                        foreach (string incident in natureOfIncidentList)
                        {
                            if(body.Contains(incident))
                            {
                                natureOfIncident = incident;
                            }
                        }
                        AddSIR(SortCodeMatch.Value, natureOfIncident);
                    }
                    // Otherwise, assume its SEM
                    else
                    {
                        MessageBox.Show("This is a SEM email");
                    }
                    AddMessages();
                    AddURLs(urlMatches);
                    break;

                case "Tweet":
                    // Sender must be the first Handle in the Body
                    sender = twitterMatches[0].ToString();

                    // Call these functions to check the Body and save any necessary information
                    ConvertAbbreviations(body);
                    AddMentions(twitterMatches);
                    AddHashtags(hashtagMatches);
                    AddMessages();
                    break;
            }
        }

        public void ConvertAbbreviations(string body)
        {
            // The CSV doesn't have a header so we must specify this
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = false,
            };

            // Use the textword CSV
            using (var reader = new StreamReader(@"textwords.csv"))
            {
                // Open the CSV with the configs set
                using (var csv = new CsvReader(reader, config))
                {
                    var records = csv.GetRecords<TextWords>().ToList();
                    
                    // Go through each TextWord
                    for(int i = 0; i < 255; i++)
                    {
                        // If the current TextWord is found in the body
                        var abr = records[i].shortVer;
                        if (body.ToUpper().Contains(abr))
                        {
                            // Add <> in at the start and end of the Long Version of the TextWord
                            string longVer = "<" + records[i].longVer + ">";
                            // Combine the Short TextWord and the Long TextWord
                            string processedMessage = abr + " " + longVer;
                            // Use StringBuilder to replace the old string with the new one
                            StringBuilder builder = new StringBuilder(body);
                            builder.Replace(abr, processedMessage);
                            // Save this modified message as the body
                            body = builder.ToString();
                        }
                    }
                }
            }
        }

        public void AddMentions(MatchCollection mentions)
        {
            // First we have to find the mentions in the Body

            // We can assume that the first mention we find is the Sender

            // Check if the Mentions JSON already exists
            if (File.Exists("Mentions.json"))
            {
                MentionsList mentList = JsonConvert.DeserializeObject<MentionsList>(File.ReadAllText("Mentions.json"));

                // Go through each Mention the Tweet Body
                foreach(var Mention in mentions)
                {
                    // Create an entry in a JSON file
                    Mention newMention = new Mention(Mention.ToString());
                    mentList.mentionsList.Add(newMention);

                    // Write the mention to the JSON
                    File.WriteAllText("Mentions.json", JsonConvert.SerializeObject(mentList, Formatting.Indented) + Environment.NewLine);
                }
            }
            // Otherwise if it doesn't exist then make a new one
            else
            {
                // Create a new JSON file
                File.WriteAllText("Mentions.json", "{\"mentionsList\": []}");
                MentionsList mentList = JsonConvert.DeserializeObject<MentionsList>(File.ReadAllText("Mentions.json"));

                // Go through each Mention the Tweet Body
                foreach (var Mention in mentions)
                {
                    // Create an entry in a JSON file
                    Mention newMention = new Mention(Mention.ToString());
                    mentList.mentionsList.Add(newMention);

                    // Write the mention to the JSON
                    File.WriteAllText("Mentions.json", JsonConvert.SerializeObject(mentList, Formatting.Indented) + Environment.NewLine);
                }
            }
        }

        public void AddHashtags(MatchCollection hashtags)
        {
            // Check if the file for Hashtags exists
            if (File.Exists("Hashtags.json"))
            {
                HashList hashtagList = JsonConvert.DeserializeObject<HashList>(File.ReadAllText("Hashtags.json"));

                // Go through each Hashtag in the MatchCollection
                foreach(var hashtag in hashtags)
                {
                    // Check if the hashtag already is present in the list
                    bool hashtagPresent = true;
                    
                    // Go through each Hashtag in the list
                    foreach(Hashtag currentHashtag in hashtagList.Hashtags.ToList())
                    {
                        if(currentHashtag.hashtag == hashtag.ToString())
                        {
                            // Increase the counter every time this hashtag is found in the messages
                            currentHashtag.hashtagCount++;

                            // Tell the program that hashtag is present in the list
                            hashtagPresent = true;

                            // Add this count to the JSON
                            File.WriteAllText("Hashtags.json", JsonConvert.SerializeObject(hashtagList, Formatting.Indented) + Environment.NewLine);

                            // Exit this loop
                            goto FoundHash;
                        }
                        else
                        {
                            // This hashtag is not on the List
                            hashtagPresent = false;
                        }
                    }
                    if(hashtagPresent == false)
                    {
                        // Create an entry for this new Hashtag
                        Hashtag newHashtag = new Hashtag(hashtag.ToString(), 1);
                        hashtagList.Hashtags.Add(newHashtag);

                        // Add this hashtag to the JSON
                        File.WriteAllText("Hashtags.json", JsonConvert.SerializeObject(hashtagList, Formatting.Indented) + Environment.NewLine);
                    }

                    FoundHash:
                        continue;
                }
            }
            // Otherwise if the JSON doesn't exist, make a new one
            else
            {
                File.WriteAllText("Hashtags.json", "{\"Hashtags\": []}");
                HashList hashtagList = JsonConvert.DeserializeObject<HashList>(File.ReadAllText("Hashtags.json"));

                foreach (var hashtag in hashtags)
                {
                    // Create an entry for the hashtag
                    Hashtag newHashtag = new Hashtag(hashtag.ToString(), 1);
                    hashtagList.Hashtags.Add(newHashtag);
                }

                // Add it to the new JSON file
                File.WriteAllText("Hashtags.json", JsonConvert.SerializeObject(hashtagList, Formatting.Indented) + Environment.NewLine);
            }
        }

        public void AddURLs(MatchCollection urls)
        {
            // Check if the file exists
            if(File.Exists("BlockedURLs.json"))
            {
                URLList blockedURLs = JsonConvert.DeserializeObject<URLList>(File.ReadAllText("BlockedURLs.json"));

                foreach(var url in urls)
                {
                    // Create and populate List
                    BlockedURL newUrl = new BlockedURL(url.ToString());
                    blockedURLs.URLs.Add(newUrl);

                    // Write the url to the JSON
                    File.WriteAllText("BlockedURLs.json", JsonConvert.SerializeObject(blockedURLs, Formatting.Indented) + Environment.NewLine);
                }
            }
            else
            {
                // Create a new JSON file
                File.WriteAllText("BlockedURLs.json", "{\"URLs\": []}");
                
                URLList blockedURLs = JsonConvert.DeserializeObject<URLList>(File.ReadAllText("BlockedURLs.json"));

                foreach (var url in urls)
                {
                    // Create and populate the URL list
                    BlockedURL newUrl = new BlockedURL(url.ToString());
                    blockedURLs.URLs.Add(newUrl);

                    // Write the url to the JSON
                    File.WriteAllText("BlockedURLs.json", JsonConvert.SerializeObject(blockedURLs, Formatting.Indented) + Environment.NewLine);
                }
            }
        }

        public void AddSIR(string sortCode, string natureOfIncident)
        {
            SIR sir = new SIR(sortCode, natureOfIncident);

            // Check if the JSON file exists
            if (File.Exists("SIR.json"))
            {
                // Create and populate the SIR list
                var SIRList = JsonConvert.DeserializeObject<SIRList>(File.ReadAllText("SIR.json"));
                SIRList.SIR.Add(sir);

                // Write the SIR to a JSON
                File.WriteAllText("SIR.json", JsonConvert.SerializeObject(SIRList, Formatting.Indented) + Environment.NewLine);
            }
            // If not then make a new JSON file
            else
            {
                // Create the JSON file
                File.WriteAllText("SIR.json", "{\"SIR\": []}");

                // Create and populate the SIR list
                var SIRList = JsonConvert.DeserializeObject<SIRList>(File.ReadAllText("SIR.json"));
                SIRList.SIR.Add(sir);

                // Write the SIR to a JSON
                File.WriteAllText("SIR.json", JsonConvert.SerializeObject(SIRList, Formatting.Indented) + Environment.NewLine);
            }
        }

        public void AddMessages()
        {
            // Check if the JSON file exists
            if(File.Exists("Messages.json"))
            {
                // Create and populate the message list
                Message message = new Message(header, sender, subject, body);
                MessageList messageList = JsonConvert.DeserializeObject<MessageList>(File.ReadAllText("Messages.json"));
                messageList.Messages.Add(message);

                // Write the Message to a JSON
                File.WriteAllText("Messages.json", JsonConvert.SerializeObject(messageList, Formatting.Indented) + Environment.NewLine);
            }
            // If not then make a new JSON file
            else
            {
                //Create a JSON file
                File.WriteAllText("Messages.json", "{\"Messages\": []}");

                // Create and populate the message list
                Message message = new Message(header, sender, subject, body);
                MessageList messageList = JsonConvert.DeserializeObject<MessageList>(File.ReadAllText("Messages.json"));
                messageList.Messages.Add(message);

                // Write the Message to a JSON
                File.WriteAllText("Messages.json", JsonConvert.SerializeObject(messageList, Formatting.Indented) + Environment.NewLine);
            }
        }
    }
}
