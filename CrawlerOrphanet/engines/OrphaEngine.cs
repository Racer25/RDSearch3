using HtmlAgilityPack;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using MongoRepository.entities;

namespace WebCrawler
{
    public class OrphaEngine
    {
        public string OrphaURL { get; set; }

        public OrphaData Datas { get; set; }

        public ConcurrentBag<Disease> Diseases { get; set; }

        public DateTime lastUpdateDateFromURL { get; set; }

        public OrphaEngine(string orphaUrl)
        {
            if (orphaUrl == null || orphaUrl == "")
            {
                throw new ArgumentNullException(nameof(orphaUrl));
            }

            OrphaURL = orphaUrl;
            Diseases = new ConcurrentBag<Disease>();
        }

        public void Start()
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            GetRareDiseases();
            /*
            Disease test;
            bool tryTest = Diseases.TryPeek(out test);
            if (tryTest)
            {
                var searchResult = PubMedCrawlerSearch(test.Name);
                PubMedCrawler(searchResult.WebEnv, searchResult.Count, searchResult.QueryKey, (int)searchResult.Count);
            }*/
            
            stopWatch.Stop();
            TimeSpan ts = stopWatch.Elapsed;
            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:000}",
            ts.Hours, ts.Minutes, ts.Seconds,
            ts.Milliseconds);
            Console.WriteLine("RunTime " + elapsedTime);
        }

        private void ParseExpertData(Disease d, ref HtmlDocument doc)
        {
            var divDefinition = doc.DocumentNode
                                        .SelectNodes("//*[contains(@class,'definition')]");
            var datas = new List<HtmlNode>();
            try
            {
                datas = doc.DocumentNode
                                    .SelectNodes("//*[contains(@class,'idData')]")
                                    .FirstOrDefault()
                                    .ChildNodes
                                    .Where(y => y.Name == "li")
                                    .ToList();
            }
            catch
            {
                //Console.WriteLine("OrphaNumber: "+d.OrphaNumber);
            }
            


            if (datas.Count == 10)
            {
                d.Prevalence = datas[1]
                    .ChildNodes
                    .Where(x => x.Name == "strong")
                    .Select(y => y.InnerText)
                    .Where(z => z.Trim() != "\n" && z.Trim() != "" && z.Trim() != "-")
                    .FirstOrDefault();

                d.Inheritance = datas[2]
                    .ChildNodes
                    .Where(x => x.Name == "strong")
                    .Select(y => y.InnerText)
                    .Where(z => z.Trim() != "\n" && z.Trim() != "" && z.Trim() != "-")
                    .FirstOrDefault();

                d.AgesOnSet = datas[3]
                    .ChildNodes
                    .Where(x => x.Name == "strong")
                    .Select(y => y.InnerText)
                    .Where(z => z.Trim() != "\n" && z.Trim() != "" && z.Trim() != "-")
                    .ToList();

                d.ICDTen = datas[4]
                    .ChildNodes
                    .Where(x => x.Name == "strong")
                    .Select(y => y.InnerText)
                    .Where(z => z.Trim() != "\n" && z.Trim() != "" && z.Trim() != "-")
                    .FirstOrDefault();

                d.OMIMID = datas[5]
                    .ChildNodes
                    .Where(x => x.Name == "strong")
                    .Select(y => y.InnerText)
                    .Where(z => z.Trim() != "\n" && z.Trim() != "" && z.Trim() != "-")
                    .FirstOrDefault();

                d.OMIMUrl = datas[5]
                    .ChildNodes
                    .Where(x => x.Name == "strong")
                    .FirstOrDefault()
                    .ChildNodes
                    .Where(y => y.Name == "a")
                    .Select(z => z.Attributes["href"].Value)
                    .Where(a => a.Trim() != "\n" && a.Trim() != "" && a.Trim() != "-")
                    .FirstOrDefault();

                d.UMLS = datas[6]
                    .ChildNodes
                    .Where(x => x.Name == "strong")
                    .Select(y => y.InnerText)
                    .Where(z => z.Trim() != "\n" && z.Trim() != "" && z.Trim() != "-")
                    .FirstOrDefault();

                d.MeSH = datas[7]
                    .ChildNodes
                    .Where(x => x.Name == "strong")
                    .Select(y => y.InnerText)
                    .Where(z => z.Trim() != "\n" && z.Trim() != "" && z.Trim() != "-")
                    .FirstOrDefault();

                d.GARD = datas[8]
                    .ChildNodes
                    .Where(x => x.Name == "strong")
                    .Select(y => y.InnerText)
                    .Where(z => z.Trim() != "\n" && z.Trim() != "" && z.Trim() != "-")
                    .FirstOrDefault();

                d.MedDRA = datas[9]
                    .ChildNodes
                    .Where(x => x.Name == "strong")
                    .Select(y => y.InnerText)
                    .Where(z => z.Trim() != "\n" && z.Trim() != "" && z.Trim() != "-")
                    .FirstOrDefault();
            }
            var articleInfo = new List<string>();
            try
            {

                articleInfo = doc.DocumentNode
                                        .SelectNodes("//*[contains(@class,'articleInfo')]")
                                        .FirstOrDefault()
                                        .ChildNodes
                                        .Where(y => y.Name == "section")
                                        .Select(z => z.InnerText)
                                        .ToList();
            }
            catch/*(Exception e)*/
            {
                //Console.WriteLine("Maladie: " + d.Name + ", OrphaNumber: " + d.OrphaNumber);
                //Console.WriteLine(e);
            }

            if (articleInfo.Count == 4)
            {
                d.Epidemiology = articleInfo[0];
                d.ClinicalDescription = articleInfo[1];
                d.Etiology = articleInfo[2];
                d.GeneticCounseling = articleInfo[3];
            }
        }

        private void GetRareDiseases()
        {
            var request = (HttpWebRequest)WebRequest.Create(OrphaURL);
            request.AutomaticDecompression = DecompressionMethods.GZip;

            using (var response = (HttpWebResponse)request.GetResponse())
            using (var stream = response.GetResponseStream())
            using (var reader = new StreamReader(stream))
            {
                //Console.WriteLine("Parsing request this can take some time...");
                Datas = JsonConvert.DeserializeObject<OrphaData>(reader.ReadToEnd());
                
                var disorders = Datas.JDBOR[0].DisorderList[0].Disorder;

                LaunchBatchs(disorders, 0, 150);

                Console.WriteLine("I found {0} diseases", Diseases.Count);
            }
        }

        public void GetLastUpdateDateFromURL()
        {
            var request = (HttpWebRequest)WebRequest.Create(OrphaURL);
            request.AutomaticDecompression = DecompressionMethods.GZip;

            using (var response = (HttpWebResponse)request.GetResponse())
            using (var stream = response.GetResponseStream())
            using (var reader = new StreamReader(stream))
            {
                //Console.WriteLine("Parsing request this can take some time...");
                Datas = JsonConvert.DeserializeObject<OrphaData>(reader.ReadToEnd());

                lastUpdateDateFromURL = DateTime.ParseExact(Datas.JDBOR[0].date, "yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);

                Console.WriteLine("Last update from url is: "+ lastUpdateDateFromURL);
            }
        }

        private void LaunchBatchs(Disorder[] disorders, int indexDisorder, int maxSize)
        {
            int size = maxSize;
            if(disorders.Length - indexDisorder <= 0)
            {
                return;
            }
            else if(disorders.Length - indexDisorder < maxSize)
            {
                size = disorders.Length - indexDisorder;
            }

            //Parallel.for(int i = indexDisorder; i < indexDisorder + size; i++)
            Parallel.For(indexDisorder, indexDisorder + size, (i) =>
            {
                ProgressBar.Instance.Report((double)indexDisorder / disorders.Length);

                //var disorder = Datas.JDBOR[0].DisorderList[0].Disorder[i];

                Boolean isARealDisorder =
            disorders[i].DisorderType[0].Name[0].label == "Disease" ||
            disorders[i].DisorderType[0].Name[0].label == "Clinical syndrome" ||
            disorders[i].DisorderType[0].Name[0].label == "Malformation syndrome" ||
            disorders[i].DisorderType[0].Name[0].label == "Biological anomaly" ||
            disorders[i].DisorderType[0].Name[0].label == "Morphological anomaly" ||
            disorders[i].DisorderType[0].Name[0].label == "Particular clinical situation in a disease or syndrome";
                if (isARealDisorder)
                {
                    Disease d = new Disease();

                    d.OrphaNumber = disorders[i].OrphaNumber;
                    d.Name = disorders[i].Name[0].label;

                    var synonymsList = disorders[i].SynonymList[0].Synonym;
                    if (synonymsList != null)
                    {
                        foreach (var synonym in synonymsList)
                        {
                            d.Synonyms.Add(synonym.label);
                        }
                    }

                    var textualList = disorders[i].TextualInformationList[0].TextualInformation;
                    if (textualList != null)
                    {
                        foreach (var textualInformation in textualList)
                        {
                            var textualSections = textualInformation.TextSectionList[0].TextSection;
                            if (textualSections != null)
                            {
                                foreach (var textSection in textualSections)
                                {
                                    TextualInformation textual = new TextualInformation();
                                    textual.title = textSection.TextSectionType[0].Name[0].label;
                                    textual.content = textSection.Contents;
                                    d.TextualInformationList.Add(textual);
                                }
                            }
                        }
                    }
                    //Console.WriteLine("Saving diseases information from orphanet website...");
                    var url = disorders[i].ExpertLink[0].link;
                    var web = new HtmlWeb();
                    var doc = web.Load(url);

                    if(doc != null)
                    {
                        ParseExpertData(d, ref doc);
                    }

                    //Console.WriteLine("OrphaNumber: "+d.OrphaNumber);

                    Diseases.Add(d);
                }
            });

            LaunchBatchs(disorders, indexDisorder + maxSize, maxSize);
        }

            public void Stop()
        {
        }
    }
}