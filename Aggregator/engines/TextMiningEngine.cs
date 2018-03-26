using MongoRepository.entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using com.aliasi.chunk;
using com.aliasi.dict;
using com.aliasi.spell;
using com.aliasi.tokenizer;
using java.lang;
using java.util;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections;
using CrawlerOrphanet.tools;
using ConfigurationJSON;
using System.Net.Http;

namespace CrawlerOrphanet
{
    class TextMiningEngine
    {
        private List<Symptom> symptomsList;
        private HttpClient client;

        public TextMiningEngine()
        {
            Console.WriteLine("TextMiningEngine initialization ...");
            client = new HttpClient();
            symptomsList = new List<Symptom>();
            GetSymptomsList();
            //getSymptomsListBeta();
            Console.WriteLine("TextMiningEngine initialization finished");
        }

        public List<Symptom> GetSymptomsFromPublications(List<Publication> publications)
        {
            List<Symptom> extractedSymptoms = new List<Symptom>();

            //Construct dictionnary
            List<Symptom> phenotypes = symptomsList;
            TrieDictionary dict = new  TrieDictionary();
            foreach (Symptom pheno in phenotypes)
            {
                dict.addEntry(new com.aliasi.dict.DictionaryEntry(pheno.Name, "PHENOTYPE"));
                foreach(string synonym in pheno.Synonyms)
                {
                    dict.addEntry(new com.aliasi.dict.DictionaryEntry(synonym, "PHENOTYPE"));
                }
            }

            TokenizerFactory tokenizerFactory = IndoEuropeanTokenizerFactory.INSTANCE;
            WeightedEditDistance editDistance = new FixedWeightEditDistance(0, -1, -1, -1, System.Double.NaN);

            double maxDistance = 0.0;
            ApproxDictionaryChunker chunker = new ApproxDictionaryChunker(dict, tokenizerFactory, editDistance, maxDistance);

            List<System.String> texts = new List<System.String>();

            foreach (Publication publication in publications)
            {
                string text = publication.title + " " + publication.abstractText + " "+publication.fullText;

                //Text preprocessing
                text = text.ToLower();

                Chunking chunking = chunker.chunk(text);
                CharSequence cs = chunking.charSequence();
                Set chunkSet = chunking.chunkSet();
                Iterator iterator = chunkSet.iterator();
                while (iterator.hasNext())
                {
                    Chunk chunk = (Chunk)iterator.next();
                    int start = chunk.start();
                    int end = chunk.end();
                    string str = cs.subSequence(start, end).toString();

                    int index = extractedSymptoms.FindIndex(symptom => symptom.Name.Equals(str) || symptom.Synonyms.IndexOf(str) != -1);
                    if (index != -1)
                    {
                        extractedSymptoms[index].Weight++;
                    }
                    else
                    {
                        //Find infos from phentypes lists
                        Symptom symptomFromPhetotypes = phenotypes.Where(x => x.Name.Equals(str) || x.Synonyms.IndexOf(str) != -1).FirstOrDefault() ;

                        //Create the real Symptom
                        Symptom symptom = new Symptom();
                        symptom.Name = symptomFromPhetotypes.Name;
                        symptom.Synonyms = symptomFromPhetotypes.Synonyms;
                        symptom.Weight = 1.0;
                        symptom.OrphaNumber = publication.orphaNumberOfLinkedDisease;
                        extractedSymptoms.Add(symptom);
                    }
                }

            }
            
            //Symptom Weight Normalization from 0 to 100
            for (int i = 0; i < extractedSymptoms.Count; i++)
            {
                //Find Min and Max for Normalization
                double max = extractedSymptoms.Max(x => x.Weight);
                double min = extractedSymptoms.Min(x => x.Weight);

                //Normalization
                if (max == min)//If size==1
                {
                    if (extractedSymptoms[i].Weight > 100.0)
                    {
                        extractedSymptoms[i].Weight = 100.0;
                    }
                }
                else
                {
                    extractedSymptoms[i].Weight = 100 * (extractedSymptoms[i].Weight - min) / (max - min);
                }
            }
            /*
            ///TEEEEEEEEEEEST
            extractedSymptoms = new List<Symptom>();
            for (int k = 0; k < 42; k++)
            {
                Symptom symptom = new Symptom();
                symptom.Name = "Paul";
                symptom.OrphaNumber = "caca";
                symptom.Weight = 42;
                extractedSymptoms.Add(symptom);
            }*/

            return extractedSymptoms;
        }

        public void GetSymptomsList()
        {
            Console.WriteLine("Retriveing symptomsNamesList ...");
            using (HttpResponseMessage res = client.GetAsync($"{ConfigurationManager.Instance.config.URL_SymptomsList}").Result)
            using (HttpContent content = res.Content)
            using (var stringReader = new StringReader(content.ReadAsStringAsync().Result))
            {
                List<PotentialSymptom> potentialSymptoms = new List<PotentialSymptom>();

                string line;

                //Iterate throught all lines
                while ((line = stringReader.ReadLine()) != null)
                {
                    if (line.Length >= 6 && line.Substring(0, 6).Equals("[Term]"))
                    {
                        //Loop for one symptom
                        PotentialSymptom myPotentialSymptom = new PotentialSymptom();
                        while ((line = stringReader.ReadLine()) != null && !line.Equals("") && !line.Equals("\n"))
                        {
                            string name = "";
                            //idHPO
                            if(line.Length > 4 && line.Substring(0, 4).Equals("id: "))
                            {
                                myPotentialSymptom.IdHPO = line.Substring(4, 10);
                            }
                            else if (line.Length >= 6 && line.Substring(0, 6).Equals("name: "))
                            {
                                name = line.Substring(6).ToLower();
                                if (!name.Equals(""))
                                {
                                    Regex rgx = new Regex("/[^A-Za-z0-9\\s]/g");
                                    myPotentialSymptom.Name = rgx.Replace(name, "").ToLower();
                                }
                            }
                            //synonyms
                            else if (line.Length > 10 && line.Substring(0, 10).Equals("synonym: \""))
                            {
                                int index = 10;
                                Char monChar = line[10];
                                do
                                {
                                    monChar = line[index];
                                    if(!monChar.Equals('\"'))
                                    {
                                        name += monChar;
                                    }
                                    
                                    index++;
                                } while (!monChar.Equals('\"') && index < line.Length);//Char different from "

                                if (!name.Equals(""))
                                {
                                    Regex rgx = new Regex("/[^A-Za-z0-9\\s]/g");
                                    myPotentialSymptom.Synonyms.Add(rgx.Replace(name, "").ToLower());
                                }
                            }//superClass
                            else if(line.Length > 6 && line.Substring(0, 6).Equals("is_a: "))
                            {
                                myPotentialSymptom.SuperClassId = line.Substring(6, 10);
                            }
                        }

                        //PreTest
                        if(myPotentialSymptom.Name != null && !myPotentialSymptom.Name.Equals(""))
                        {
                            potentialSymptoms.Add(myPotentialSymptom);
                        }
                    }
                    
                }


                //Good id
                string goodIdInParentNode = "HP:0000118";//Phenotypic abnormality

                //Filtration from bad ids
                List<Symptom> goodSymptoms = new List<Symptom>();
                foreach (PotentialSymptom myPotentialSymptom in potentialSymptoms)
                {
                    //Good id is not a valid symptom (only his children), so we avoid it
                    if(!myPotentialSymptom.IdHPO.Equals(goodIdInParentNode))
                    {
                        //Copy potential symptom to initiate iterating
                        PotentialSymptom temporaryPotentialSymptom = new PotentialSymptom();
                        temporaryPotentialSymptom.IdHPO = myPotentialSymptom.IdHPO;
                        temporaryPotentialSymptom.Name = myPotentialSymptom.Name;
                        temporaryPotentialSymptom.SuperClassId = myPotentialSymptom.SuperClassId;
                        temporaryPotentialSymptom.Synonyms = myPotentialSymptom.Synonyms;

                        //Iterate throught Parents and Check if it's a Valid symptom
                        bool hasASuperClass = temporaryPotentialSymptom.SuperClassId != null;
                        bool validSymptom = false;
                        while (hasASuperClass && !validSymptom)
                        {
                            if (temporaryPotentialSymptom.SuperClassId == null)
                            {
                                hasASuperClass = false;
                            }
                            else
                            {
                                //If valid id
                                if (temporaryPotentialSymptom.SuperClassId.Equals(goodIdInParentNode))
                                {
                                    validSymptom = true;
                                }
                                else
                                {
                                    //Contniue searching
                                    temporaryPotentialSymptom = potentialSymptoms.Where(x => x.IdHPO.Equals(temporaryPotentialSymptom.SuperClassId)).First();
                                }
                            }
                        }

                        if (validSymptom)
                        {
                            //Conversion from potential to real symptom
                            Symptom symptom = new Symptom();
                            symptom.Name = myPotentialSymptom.Name;
                            symptom.Synonyms = myPotentialSymptom.Synonyms;
                            symptomsList.Add(symptom);
                        }
                    }
                }

                Console.WriteLine("SymptomsNamesList retrieved!!");
            }
        }
    }
}
