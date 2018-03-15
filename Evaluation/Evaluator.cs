using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Configuration;
using ConfigurationJSON;

namespace Evaluation
{
    public class Evaluator
    {
        public static void Evaluate(DiseasesData PredictionData, DiseasesData RealData, string wantedFileName = "")
        {
            //Object to write in JSON
            Results results = new Results();

            int RP = 0;//RealPositive general
            int FP = 0;//FalsePositive general
            int FN = 0;//FalseNegative general

            //For each existent rare disease
            foreach(string orphaNumber in PredictionData.DiseaseDataList.Select(x => x.Disease.OrphaNumber))
            {
                //Find THE diseaseData of ONE disease (real and predicted data)
                DiseaseData RealDiseaseData = RealData.DiseaseDataList.Where(x => x.Disease.OrphaNumber == orphaNumber).FirstOrDefault();
                DiseaseData PredictionDiseaseData = PredictionData.DiseaseDataList.Where(x => x.Disease.OrphaNumber == orphaNumber).FirstOrDefault();

                //If we don't find the disease in both dataset, we shoud pass to another disease
                if(RealDiseaseData != null && PredictionDiseaseData != null)
                {
                    int RP_Disease = 0;//RealPositive of one disease
                    int FP_Disease = 0;//FalsePositive of one disease
                    int FN_Disease = 0;//FalseNegative of one disease

                    //Compute RP and FP
                    List<string> RelatedEntitiesNamesReal =
                        RealDiseaseData
                        .RelatedEntities.RelatedEntitiesList
                        .Select(x => x.Name)
                        .ToList();

                    for (int j = 0; j < PredictionDiseaseData.RelatedEntities.RelatedEntitiesList.Count; j++)
                    {
                        //Is my predicted related entity is present in the real data?
                        if (RelatedEntitiesNamesReal.IndexOf(PredictionDiseaseData.RelatedEntities.RelatedEntitiesList[j].Name) != -1)
                        {
                            RP++;
                            RP_Disease++;
                        }
                        else
                        {
                            FP++;
                            FP_Disease++;
                        }
                    }

                    //Compute FN
                    List<string> RelatedEntitiesNamesPred =
                        PredictionDiseaseData
                        .RelatedEntities.RelatedEntitiesList
                        .Select(x => x.Name)
                        .ToList();
                    for (int j = 0; j < RealDiseaseData.RelatedEntities.RelatedEntitiesList.Count; j++)
                    {
                        //Is my real related entity is present in the predicted data?
                        if (RelatedEntitiesNamesPred.IndexOf(RealDiseaseData.RelatedEntities.RelatedEntitiesList[j].Name) == -1)
                        {
                            FN++;
                            FN_Disease++;
                        }
                    }

                    //Compute Precision/recall and F_score
                    double PrecisionDisease = Math.Round((double)RP_Disease / (double)(RP_Disease + FP_Disease), 4);
                    double RecallDisease = Math.Round((double)RP_Disease / (double)(RP_Disease + FN_Disease), 4);
                    double F_ScoreDisease = Math.Round(2 * PrecisionDisease * RecallDisease / (PrecisionDisease + RecallDisease), 4);

                    //Construct results object
                    PerDisease OnePerDisease = new PerDisease(orphaNumber,
                        RealDiseaseData.Disease.NumberOfPublications, 
                        PredictionData.Type.ToString(),
                        RP_Disease, 
                        FP_Disease, 
                        FN_Disease,
                        PrecisionDisease,//Precision
                        RecallDisease, //Recall
                        F_ScoreDisease
                        );

                    results.perDisease.Add(OnePerDisease);
                }

            }

            //Compute Precision/recall and F_score
            double Precision = Math.Round((double)RP / (double)(RP + FP), 4);
            double Recall = Math.Round((double)RP / (double)(RP + FN), 4);
            double F_Score = Math.Round(2 * Precision * Recall / (Precision + Recall), 4);

            //Construct results object
            results.general = new General(DateTime.Now, 
                PredictionData.Type.ToString(), RP, FP, FN,  Precision,  Recall, F_Score);

            //Write JSON FILE
            WriteJSONFile(results, wantedFileName);
        }

        public static void WriteJSONFile(Results results, string wantedFileName = "")
        {
            string output = JsonConvert.SerializeObject(results, Formatting.Indented);

            //Choose the name
            string fileName = "";
            if (wantedFileName != "")
            {
                fileName = wantedFileName;
            }
            else
            {

                fileName = "results_" + results.general.TimeStamp.ToString("yyyy-MM-dd_HH-mm-ss") + ".json";
            }

            File.WriteAllText(ConfigurationManager.GetSetting("ResultsFolder") + fileName, output);

        }
    }
}
