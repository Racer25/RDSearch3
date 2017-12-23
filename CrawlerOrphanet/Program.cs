using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoRepository.entities;
using WebCrawler;
using CrawlerOrphanet.tools;
using System.Diagnostics;

namespace CrawlerOrphanet
{
    class Program
    {
        static void Main(string[] args)
        {
            /*
            //Orphanet
            OrphaEngine engine = new OrphaEngine("http://www.orphadata.org/data/export/en_product1.json");
            engine.GetLastUpdateDateFromURL();
            */
            List<Disease> lst_diseases = new List<Disease>();
            /*
            using (var configRepository = new MongoRepository.ConfigRepository())
            using (var diseaseRepository = new MongoRepository.DiseaseRepository())
            {
                if (configRepository.selectConfig() == null || engine.lastUpdateDateFromURL > configRepository.selectConfig()?.lastUpdateDateOfDiseasesList)
                {
                    //We clear the collection
                    diseaseRepository.removeAll();

                    Console.WriteLine("Starting request for OrphaData/Orphanet this can take some time...");
                    Console.WriteLine("Saving diseases information...");
                    engine.Start();
                    lst_diseases = engine.Diseases.ToList();

                    //We stock the retrieved diseases in DB
                    diseaseRepository.insertList(lst_diseases);

                    //Update update date
                    configRepository.deleteAll();
                    Config conf = new Config();
                    conf.lastUpdateDateOfDiseasesList = engine.lastUpdateDateFromURL;
                    configRepository.insert(conf);
                }
                else
                {
                    Console.WriteLine("Last update date from DB is: "+ configRepository.selectConfig()?.lastUpdateDateOfDiseasesList);
                    Console.WriteLine("No update detected, using offline disease information only");
                }
            }*/
            

            lst_diseases = new List<Disease>();
            using (var db = new MongoRepository.DiseaseRepository())
            {
                //lst_diseases = db.selectAll().Take(500).ToList();
                lst_diseases = db.selectAll();
            }
            /*
            //Pubmed
            PubmedEngine pubmedEngine = new PubmedEngine();

            Console.WriteLine("Starting requests for Pubmed this can take some time...");
            pubmedEngine.Start(lst_diseases);
            */


            TextMiningEngine textMiningEngine = new TextMiningEngine("http://purl.obolibrary.org/obo/hp.obo");

            //BatchConfig
            int batchSize = 200;
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
                Console.WriteLine("Extraction Symptomes...");

                TimeLeft.Instance.Reset();
                TimeLeft.Instance.operationsToDo = publicationsPerDisease.Count;

                //foreach(List<Publication> pubs in publicationsPerDisease)
                Parallel.ForEach(publicationsPerDisease, (pubs) =>
                    {
                        Stopwatch diffTime = new Stopwatch();
                        diffTime.Start();
                        
                        List<Symptom> symptomsExtracted = textMiningEngine.getSymptomsFromPublications(pubs);
                        using (var symptomRepository = new MongoRepository.SymptomRepository())
                        {
                            symptomRepository.deleteByOrphaNumber(pubs[0].orphaNumberOfLinkedDisease);
                            if(symptomsExtracted.Count != 0)
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
                                catch(Exception e)
                                {
                                    Console.WriteLine("symptomsExtracted.Count= " + symptomsExtracted.Count);
                                    Console.WriteLine(e);

                                    foreach(Symptom symptom in symptomsExtracted)
                                    {
                                        symptom.Show();
                                    }
                                }
                            }
                        }

                        diffTime.Stop();

                        TimeLeft.Instance.IncrementOfXOperations(TimeSpan.FromMilliseconds(diffTime.ElapsedMilliseconds).Seconds, 1);
                        TimeLeft.Instance.CalcAndShowTimeLeft(i+1, nombreBatch);
                    }
                );
            }

            Console.WriteLine("Fini :)"); 
            Console.ReadLine();

        }
    }
}
