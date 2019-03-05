using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonROMEditor.Models
{
    public class PokeType
    {
        public string TypeName { get; set; }
        public bool TypeUsed { get; set; }
    }

    public class TypeStrength
    {
        public int AttackType { get; set; }
        public int DefenseType { get; set; }
        public DamageModifier Effectiveness
        {
            get;
            set;
        }
    }
}
