using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using MongoRepository.entities;
using System.Net;
using System.IO;
using System.Xml.Serialization;
using System.Xml;

namespace SymptomsEval
{
    public class Program
    {
        private static readonly Random random = new Random();

        private static readonly object syncLock = new object();

        static void Main(string[] args)
        {
            //Symptoms from Orphanet
            Console.WriteLine("allSymptomsFromOrphanet recup ...");
            Dictionary<string, List<Symptom>> allSymptomsFromOrphanet = GetSymptomsFromOrphanetFromDiseases();
            Console.WriteLine("allSymptomsFromOrphanet recup finished");

            //To test one particular disease
            ////////////////////////////////
            int sampleSize = allSymptomsFromOrphanet.Count;
            ////////////////////////////////

            //int sampleSize = 50;
            int nombreEssais = 3;
            for(int i =0; i < nombreEssais; i++)
            {
                Console.WriteLine("");
                Console.WriteLine("/////////////////");
                Console.WriteLine("////Essai n°"+(i+1)+"////");
                Console.WriteLine("/////////////////");


                //Random selection of diseases
                Console.WriteLine(sampleSize+" List<Symptom> from random diseases from Orphanet ...");

                //Dictionary<string, List<Symptom>> symptomsFromOrphanet = PickRandom(allSymptomsFromOrphanet, sampleSize);

                //To test one particular disease
                //////////////////////////////////////////
                Dictionary<string, List<Symptom>> symptomsFromOrphanet = new Dictionary<string, List<Symptom>>();
                KeyValuePair<string, List<Symptom>> keyVal = allSymptomsFromOrphanet.Where(x => x.Key.Equals("166119")).First(); ;
                symptomsFromOrphanet.Add(keyVal.Key, keyVal.Value);
                ////////////////////////////////////////////////////////////

                Console.WriteLine(sampleSize + " List<Symptom> from random diseases from Orphanet finished");

                //OrphaNumberList
                List<string> orphaNumbers = symptomsFromOrphanet.Keys.ToList();


                Dictionary<string, List<Symptom>> symptomsFromDB = new Dictionary<string, List<Symptom>>();

                using (var symptomRepository = new MongoRepository.SymptomRepository())
                {

                    //Symptoms from DB
                    Console.WriteLine(sampleSize + " List<Symptom> from random diseases from DB ...");
                    foreach (string orphaNumber in orphaNumbers)
                    {
                        symptomsFromDB[orphaNumber] = symptomRepository.getByOrphaNumber(orphaNumber);
                    }
                    Console.WriteLine(sampleSize + " List<Symptom> from random diseases from DB finished");


                }

                //Control of samples
                if (symptomsFromDB.Count != symptomsFromOrphanet.Count)
                {
                    Console.WriteLine("Les échantillons ne sont pas de même taille!");
                    Console.WriteLine("Length symptomsFromDB: " + symptomsFromDB.Count);
                    Console.WriteLine("Length symptomsFromOrphanet: " + symptomsFromOrphanet.Count);
                    return;
                }
                else
                {
                    List<Symptom>[] results = evaluate(symptomsFromDB, symptomsFromOrphanet);

                    int RPCount = results[0].Count;
                    int FPCount = results[1].Count;
                    int FNCount = results[2].Count;

                    Console.WriteLine("RP:" + RPCount + ", FP:" + FPCount + ", FN:" + FNCount);

                    double precision = Math.Round(100.0 * (double)(RPCount) / (double)(RPCount + FPCount), 2);
                    double recall = Math.Round(100.0 * (double)(RPCount) / (double)(RPCount + FNCount), 2);
                    double fMeasure = Math.Round(2.0 * (precision * recall) / (precision + recall), 2);

                    Console.WriteLine("Precision:" + precision + "%, Recall:" + recall+ "%, F-Measure:"+ fMeasure+"%");

                    //To compute mean square error
                    //Preparing for Weight error calculation with only Real positive symptoms

                    //Make a copie
                    var symptomsFromDB_RP = symptomsFromDB.ToDictionary(entry => entry.Key, entry => entry.Value);

                    //Filter
                    foreach (KeyValuePair<string, List<Symptom>> keyValue in symptomsFromDB)
                    {
                        //Filter: Symptom must be abstent of FP list to only have RP
                        symptomsFromDB_RP[keyValue.Key] = symptomsFromDB[keyValue.Key].Where(x => results[1].IndexOf(x) == -1).ToList();
                    }

                    var symptomsFromOrphanet_RP = symptomsFromOrphanet.ToDictionary(entry => entry.Key, entry => entry.Value);
                    foreach (KeyValuePair<string, List<Symptom>> keyValue in symptomsFromOrphanet)
                    {
                        //Filter: Symptom must be abstent of FN list to only have RP
                        symptomsFromOrphanet_RP[keyValue.Key] = symptomsFromOrphanet[keyValue.Key].Where(x => results[2].IndexOf(x) == -1).ToList();
                    }

                    double ecartType = Math.Sqrt(evaluateWeightError(symptomsFromDB_RP, symptomsFromOrphanet_RP));

                    Console.WriteLine("ecartType: " + Math.Round(ecartType, 2));
                    
                }
            }

            Console.WriteLine("");
            Console.WriteLine("Fini :) ");
            Console.ReadLine();

        }

        static public Dictionary<string, List<Symptom>> GetSymptomsFromOrphanetFromDiseases()
        {
            Dictionary<string, List<Symptom>> symptomsFromOrphanet = new Dictionary<string, List<Symptom>>();

            var request = (HttpWebRequest)WebRequest.Create("http://www.orphadata.org/data/xml/en_product4_HPO.xml");
            request.AutomaticDecompression = DecompressionMethods.GZip;
            XmlSerializer serializer = new XmlSerializer(typeof(JDBOR));
            JDBOR result = new JDBOR();

            using (var response = (HttpWebResponse)request.GetResponse())
            using (var stream = response.GetResponseStream())
            using (var reader = XmlReader.Create(stream, new XmlReaderSettings { DtdProcessing = DtdProcessing.Ignore }))
            {
                result = serializer.Deserialize(reader) as JDBOR;
            }

            var disorders = result.DisorderList[0].Disorder;

            foreach(var disorder in disorders)
            {
                List<Symptom> phenotypesList = new List<Symptom>();
                string orphanetID = disorder.OrphaNumber;
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

                    Symptom symptom = new Symptom();
                    symptom.Name = symptomName;
                    symptom.OrphaNumber = orphanetID;
                    symptom.Weight = weight;

                    phenotypesList.Add(symptom);
                }
                symptomsFromOrphanet[orphanetID]=phenotypesList;
            }

            return symptomsFromOrphanet;
        }

        public Disease getOneSpecialDisease(string orphaNumber)
        {
            var diseaseRepo = new MongoRepository.DiseaseRepository();
            return diseaseRepo.getByOrphaNumber(orphaNumber);
            
        }

        static public Dictionary<string, T> PickRandom<T>(Dictionary<string, T> fullDict, int num_values)
        {
            // Don't exceed the array's length.
            if (num_values >= fullDict.Count)
                num_values = fullDict.Count - 1;

            // Make an array of indexes 0 through values.Length - 1.
            int[] indexes =
                Enumerable.Range(0, fullDict.Count).ToArray();

            // Build the return list.
            Dictionary<string, T> results = new Dictionary<string, T>();

            // Randomize the first num_values indexes.
            for (int i = 0; i < num_values; i++)
            {
                // Pick a random entry between i and values.Length - 1.
                int j = RandomNumber(i, fullDict.Count);

                // Swap the values.
                int temp = indexes[i];
                indexes[i] = indexes[j];
                indexes[j] = temp;

                // Save the ith value.
                var keyValue = fullDict.ElementAt(j);
                results[keyValue.Key] = keyValue.Value;
            }

            // Return the selected items.
            return results;
        }

        static public List<Symptom>[] evaluate(Dictionary<string, List<Symptom>> symptomsPrediction, Dictionary<string, List<Symptom>> realSymptoms)
        {
            List<Symptom> RP = new List<Symptom>();//RealPositive
            List<Symptom> FP = new List<Symptom>();//FalsePositive
            List<Symptom> FN = new List<Symptom>();//FalseNegative

            foreach(string orphaNumber in symptomsPrediction.Keys.ToList())
            {
                //Compute RP and FP lists
                List<string> symptomsNamesReal = realSymptoms[orphaNumber].Select(x => x.Name).ToList();
                for (int j = 0; j < symptomsPrediction[orphaNumber].Count; j++)
                {
                    if (symptomsNamesReal.IndexOf(symptomsPrediction[orphaNumber][j].Name) != -1)
                    {
                        RP.Add(symptomsPrediction[orphaNumber][j]);
                    }
                    else
                    {
                        FP.Add(symptomsPrediction[orphaNumber][j]);
                    }
                }

                //Compute FN list
                List<string> symptomsNamesPred = symptomsPrediction[orphaNumber].Select(x => x.Name).ToList();
                for (int j = 0; j < realSymptoms[orphaNumber].Count; j++)
                {
                    if (symptomsNamesPred.IndexOf(realSymptoms[orphaNumber][j].Name) == -1)
                    {
                        FN.Add(realSymptoms[orphaNumber][j]);
                    }
                }
            }

            List<Symptom>[] results = new List<Symptom>[3];
            results[0] = RP;
            results[1] = FP;
            results[2] = FN;

            return results;

        }

        static public double evaluateWeightError(Dictionary<string, List<Symptom>> symptomsPrediction, Dictionary<string, List<Symptom>> realSymptoms)
        {

            double meanSquareError = 0.0;

            var keys = symptomsPrediction.Keys.ToList();
            double compteur = 0.0;
            foreach (var orphaNumber in keys)
            {
                List<Symptom> symptomsPredictionValue = symptomsPrediction[orphaNumber];
                List<Symptom> realSymptomsValue = realSymptoms[orphaNumber];
                
                //Parcours des 2 Listes en même temps
                for (int i = 0; i < symptomsPredictionValue.Count; i++)
                {
                    double dist = symptomsPredictionValue[i].Weight - realSymptomsValue[i].Weight;
                    meanSquareError += Math.Pow(dist, 2);
                    compteur++;
                }
            }

            meanSquareError = meanSquareError / compteur;

            return meanSquareError;
        }


            public static int RandomNumber(int min, int max)
        {
            lock (syncLock)
            { // synchronize
                return random.Next(min, max);
            }
        }
    }

}
