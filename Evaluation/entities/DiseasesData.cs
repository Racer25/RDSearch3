using System;
using System.Collections.Generic;
using System.Text;

namespace Evaluation
{
    public class DiseasesData
    {
        public Type Type { get; set; }
        public List<DiseaseData> DiseaseDataList { get; set; }

        public DiseasesData(Type TypeP, List<DiseaseData> DiseaseDataListP)
        {
            Type = Type;
            DiseaseDataList = DiseaseDataListP;
        }
    }
}
