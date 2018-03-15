using System;
using System.Collections.Generic;
using System.Text;

namespace Evaluation
{
    public class DiseaseData
    {
        public Disease Disease { get; set; }
        public RelatedEntities RelatedEntities { get; set; }

        public DiseaseData(Disease DiseaseP, RelatedEntities RelatedEntitiesP)
        {
            Disease = DiseaseP;
            RelatedEntities = RelatedEntitiesP;
        }
    }
}
