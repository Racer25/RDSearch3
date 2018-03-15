using System;
using System.Collections.Generic;
using System.Text;

namespace MongoRepository.entities
{
    public class RelatedEntities
    {
        public Type Type { get; set; }
        public List<RelatedEntity> RelatedEntitiesList { get; set; }

        public RelatedEntities(Type TypeP, List<RelatedEntity> RelatedEntitiesListP)
        {
            Type = TypeP;
            RelatedEntitiesList = RelatedEntitiesListP;
        }
    }
}
