using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonROMEditor.Models
{
    public class TypeStrength
    {
        public PokeType AttackType { get; set; }
        public PokeType DefenseType { get; set; }
        public DamageModifier Effectiveness
        {
            get;
            set;
        }
    }
}
