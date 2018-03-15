using System;
using System.Collections.Generic;
using System.Text;

namespace MongoRepository.entities
{
    public class RelatedEntity
    {
        public Type Type { get; set; }
        public string Name { get; set; }
        public double Weight { get; set; }

        public RelatedEntity(Type TypeP, string NameP, double WeightP)
        {
            Type = TypeP;
            Name = NameP;
            Weight = WeightP;
        }
    }
}
