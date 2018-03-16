using System;
using System.Collections.Generic;
using System.Text;

namespace MongoRepository.entities
{
    public class RelatedEntity
    {
        public type Type { get; set; }
        public string Name { get; set; }
        public double Weight { get; set; }

        public RelatedEntity(type TypeP, string NameP, double WeightP)
        {
            Type = TypeP;
            Name = NameP;
            Weight = WeightP;
        }
    }
}
