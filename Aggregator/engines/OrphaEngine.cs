﻿using HtmlAgilityPack;
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
using ConfigurationJSON;

namespace WebCrawler
{
    public class OrphaEngine
    {
        public OrphaData Datas { get; set; }

        public ConcurrentBag<Disease> Diseases { get; set; }

        public DiseasesData RealData { get; set; }

        public DateTime LastUpdateDateFromURL { get; set; }

        public OrphaEngine()
        {
            Diseases = new ConcurrentBag<Disease>();
        }

        public void Start()
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            GetRareDiseases();
            SaveDiseasesOnDB();

            GetRealData();
            SaveRealDataOnDB();
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
            var request = (HttpWebRequest)WebRequest.Create(ConfigurationManager.GetSetting("URL_Rare_Diseases"));
            request.AutomaticDecompression = DecompressionMethods.GZip;

            using (var response = (HttpWebResponse)request.GetResponse())
            using (var stream = response.GetResponseStream())
            using (var reader = new StreamReader(stream))
            {
                //Console.WriteLine("Parsing request this can take some time...");
                Datas = JsonConvert.DeserializeObject<OrphaData>(reader.ReadToEnd());
                
                var disorders = Datas.JDBOR[0].DisorderList[0].Disorder;

                LaunchBatchs(disorders, 0, ConfigurationManager.GetSetting("BatchSizeDiseases"));

                Console.WriteLine("I found {0} diseases", Diseases.Count);
            }
        }

        public void GetLastUpdateDateFromURL()
        {
            var request = (HttpWebRequest)WebRequest.Create(ConfigurationManager.GetSetting("URL_Rare_Diseases"));
            request.AutomaticDecompression = DecompressionMethods.GZip;

            using (var response = (HttpWebResponse)request.GetResponse())
            using (var stream = response.GetResponseStream())
            using (var reader = new StreamReader(stream))
            {
                //Console.WriteLine("Parsing request this can take some time...");
                Datas = JsonConvert.DeserializeObject<OrphaData>(reader.ReadToEnd());

                LastUpdateDateFromURL = DateTime.ParseExact(Datas.JDBOR[0].date, "yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);

                Console.WriteLine("Last update from url is: "+ LastUpdateDateFromURL);
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

        public void SaveDiseasesOnDB()
        {
            using (var configRepository = new MongoRepository.ConfigRepository())
            using (var diseaseRepository = new MongoRepository.DiseaseRepository())
            {
                if (configRepository.selectConfig() == null || 
                    LastUpdateDateFromURL > configRepository.selectConfig()?.lastUpdateDateOfDiseasesList)
                {
                    Console.WriteLine("Update detected!!");
                    Console.WriteLine("Saving diseases information on DB ..");
                    //We clear the collection
                    diseaseRepository.removeAll();

                    var lst_diseases = Diseases.ToList();

                    //We stock the retrieved diseases in DB
                    diseaseRepository.insertList(lst_diseases);

                    //Update update date
                    configRepository.deleteAll();
                    MongoRepository.entities.Config conf = new MongoRepository.entities.Config();
                    conf.lastUpdateDateOfDiseasesList = LastUpdateDateFromURL;
                    configRepository.insert(conf);
                }
                else
                {
                    Console.WriteLine("Last update date from DB is: " + configRepository.selectConfig()?.lastUpdateDateOfDiseasesList);
                    Console.WriteLine("No update detected, using offline disease information only");
                }
            }
        }

        public void GetRealData()
        {
            RealData = new DiseasesData(type.Symptom, new List<DiseaseData>());

            var request = (HttpWebRequest)WebRequest.Create(ConfigurationManager.GetSetting("URL_RealSymptomsByDisease"));
            request.AutomaticDecompression = DecompressionMethods.GZip;
            XmlSerializer serializer = new XmlSerializer(typeof(SymptomsEval.JDBOR));
            SymptomsEval.JDBOR result = new SymptomsEval.JDBOR();

            using (var response = (HttpWebResponse)request.GetResponse())
            using (var stream = response.GetResponseStream())
            using (var reader = XmlReader.Create(stream, new XmlReaderSettings { DtdProcessing = DtdProcessing.Ignore }))
            {
                result = serializer.Deserialize(reader) as SymptomsEval.JDBOR;
            }

            var disorders = result.DisorderList[0].Disorder;

            foreach (var disorder in disorders)
            {
                //Constructing DiseaseData
                DiseaseData myDiseaseData = new DiseaseData(
                    Diseases.Where(x => x.OrphaNumber == disorder.OrphaNumber).FirstOrDefault(),
                    new RelatedEntities(type.Symptom, new List<RelatedEntity>()));


                
                var hpoPhenotypes = disorder.HPODisorderAssociationList[0].HPODisorderAssociation.ToList();
                for (int j = 0; j < hpoPhenotypes.Count; j++)
                {
                    string symptomName = hpoPhenotypes[j].HPO[0].HPOTerm.ToLower();

                    //Frequency
                    var frequency = hpoPhenotypes[j].HPOFrequency[0].Name[0].Value;
                    double weight = 0;
                    if (frequency.Equals("Obligate (100%)"))
                    {
                        weight = 100.0;
                    }
                    else if (frequency.Equals("Very frequent (99-80%)"))
                    {
                        weight = 90.0;
                    }
                    else if (frequency.Equals("Frequent (79-30%)"))
                    {
                        weight = 55.0;
                    }
                    else if (frequency.Equals("Occasional (29-5%)"))
                    {
                        weight = 17.5;
                    }
                    else if (frequency.Equals("Very rare (<4-1%)"))
                    {
                        weight = 2.5;
                    }

                    RelatedEntity symptom = new RelatedEntity(type.Symptom, symptomName, weight);

                    myDiseaseData.RelatedEntities.RelatedEntitiesList.Add(symptom);
                }

                RealData.DiseaseDataList.Add(myDiseaseData);
            }
        }

        public void SaveRealDataOnDB()
        {
            using (var realDataRepository = new MongoRepository.RealDataRepository())
            {
                //We clear the collection
                realDataRepository.removeAll();

                //We stock the retrieved diseases in DB
                realDataRepository.insert(RealData);
            }
        }

            public void Stop()
        {
        }
    }
}