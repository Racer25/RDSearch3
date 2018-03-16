using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoRepository.entities;
using WebCrawler;
using CrawlerOrphanet.tools;
using System.Diagnostics;
using ConfigurationJSON;
using Evaluation;

namespace CrawlerOrphanet
{
    class Program
    {
        static void Main(string[] args)
        {
            //Update Orphanet (diseases/real datasets TODO)
            OrphaEngine orphaEngine = new OrphaEngine();
            orphaEngine.Start();

            //Retrieving diseases from DB
            List<Disease> lst_diseases = new List<Disease>();
            using (var db = new MongoRepository.DiseaseRepository())
            {
                //lst_diseases = db.selectAll().Take(500).ToList();
                lst_diseases = db.selectAll();
            }
            
            //Update Publications
            PubmedEngine pubmedEngine = new PubmedEngine();
            Console.WriteLine("Starting requests at PMC this can take some time...");
            pubmedEngine.Start(lst_diseases);

            //Retrieving related entities by disease AND TextMine
            TextMiningEngine textMiningEngine = new TextMiningEngine();
            RecupSymptomsAndTextMine(lst_diseases, textMiningEngine);
            //RecupLinkedDiseasesAndTextMine(lst_diseases, textMiningEngine);
           // RecupDrugsAndTextMine(lst_diseases, textMiningEngine);

            //Retrieving PredictionData and RealData from DB (DiseasesData with type Symptom)
            DiseasesData PredictionData = null;
            DiseasesData RealData = null;
            using (var dbPred = new MongoRepository.PredictionDataRepository())
            using (var dbReal = new MongoRepository.RealDataRepository())
            {
                PredictionData = dbPred.selectByType(type.Symptom);
                RealData = dbReal.selectByType(type.Symptom);
            }


            //Evaluation...
            if(PredictionData != null && RealData != null)
            {
                Evaluator.Evaluate(PredictionData, RealData);
            }


            Console.WriteLine("Finished :)");
            Console.ReadLine();
        }

        static void RecupLinkedDiseasesAndTextMine(List<Disease> lst_diseases, TextMiningEngine textMiningEngine)
        {
            throw new NotImplementedException();
        }

        static void RecupDrugsAndTextMine(List<Disease> lst_diseases, TextMiningEngine textMiningEngine)
        {
            throw new NotImplementedException();
        }

        static void RecupSymptomsAndTextMine(List<Disease> lst_diseases, TextMiningEngine textMiningEngine)
        {
            //Use TextMiningEngine....

            //BatchConfig
            int batchSize = ConfigurationManager.GetSetting("BatchSizePMC");
            int nombreBatch = (lst_diseases.Count / batchSize) + 1;
            if ((nombreBatch - 1) * batchSize == lst_diseases.Count)
            {
                nombreBatch--;
            }
            for (int i = 0; i < nombreBatch; i++)
            {
                //BatchSize adjustement
                int realBatchSize = batchSize;
                if ((i + 1) * realBatchSize > lst_diseases.Count)
                {
                    realBatchSize = lst_diseases.Count - i * realBatchSize;
                }
                var selectedDiseases = lst_diseases.GetRange(i * realBatchSize, realBatchSize);



                //TimeLeft initialization
                TimeLeft.Instance.Reset();
                TimeLeft.Instance.operationsToDo = selectedDiseases.Count;

                //REAL Process
                //Publication recup
                Console.WriteLine("Publications recup...");
                List<List<Publication>> publicationsPerDisease = new List<List<Publication>>();
                using (var publicationRepository = new MongoRepository.PublicationRepository())
                {
                    //Parallel.ForEach(lst_diseases, (disease) =>
                    foreach (Disease disease in selectedDiseases)
                    {
                        Stopwatch diffTime = new Stopwatch();
                        diffTime.Start();

                        List<Publication> pubs = publicationRepository.getByOrphaNumberOfLinkedDisease(disease.OrphaNumber);
                        if (pubs.Count != 0)
                        {
                            publicationsPerDisease.Add(pubs);
                        }
                        diffTime.Stop();

                        TimeLeft.Instance.IncrementOfXOperations(TimeSpan.FromMilliseconds(diffTime.ElapsedMilliseconds).Seconds, 1);
                        TimeLeft.Instance.CalcAndShowTimeLeft(i + 1, nombreBatch);
                    }
                    //);
                }

                Console.WriteLine("Publications recup finished!");

                //Extraction Symptomes
                Console.WriteLine("Extraction Symptoms...");

                TimeLeft.Instance.Reset();
                TimeLeft.Instance.operationsToDo = publicationsPerDisease.Count;

                //foreach(List<Publication> pubs in publicationsPerDisease)
                Parallel.ForEach(publicationsPerDisease, (pubs) =>
                {
                    Stopwatch diffTime = new Stopwatch();
                    diffTime.Start();

                    List<Symptom> symptomsExtracted = textMiningEngine.GetSymptomsFromPublications(pubs);
                    using (var symptomRepository = new MongoRepository.SymptomRepository())
                    {
                        symptomRepository.deleteByOrphaNumber(pubs[0].orphaNumberOfLinkedDisease);
                        if (symptomsExtracted.Count != 0)
                        {
                            try
                            {
                                symptomRepository.insertList(symptomsExtracted);
                                /*
                                List<Symptom> lst_symptoms = new List<Symptom>();
                                for(int k = 0; k < 42; k++)
                                {
                                    Symptom symptom = new Symptom();
                                    symptom.Name = "Paul";
                                    symptom.OrphaNumber = "caca";
                                    symptom.Weight = 42;
                                    lst_symptoms.Add(symptom);
                                }
                                symptomRepository.insertList(lst_symptoms);*/
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine("symptomsExtracted.Count= " + symptomsExtracted.Count);
                                Console.WriteLine(e);

                                foreach (Symptom symptom in symptomsExtracted)
                                {
                                    symptom.Show();
                                }
                            }
                        }
                    }

                    diffTime.Stop();

                    TimeLeft.Instance.IncrementOfXOperations(TimeSpan.FromMilliseconds(diffTime.ElapsedMilliseconds).Seconds, 1);
                    TimeLeft.Instance.CalcAndShowTimeLeft(i + 1, nombreBatch);
                }
                );
            }
        }
    }
}
