using System;
using System.Collections.Generic;
using System.Text;

namespace MongoRepository.entities
{
    public class RelatedEntities
    {
        public type Type { get; set; }
        public List<RelatedEntity> RelatedEntitiesList { get; set; }

        public RelatedEntities(type TypeP, List<RelatedEntity> RelatedEntitiesListP)
        {
            Type = TypeP;
            RelatedEntitiesList = RelatedEntitiesListP;
        }
    }
}
