using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonROMEditor.Models
{
    public class WildEncounterZone
    {
        public string ZoneName { get; set; }
        public int EncounterRate { get; set; }
        public EnemyPokemon Enc1 { get; set; }
        public EnemyPokemon Enc2 { get; set; }
        public EnemyPokemon Enc3 { get; set; }
        public EnemyPokemon Enc4 { get; set; }
        public EnemyPokemon Enc5 { get; set; }
        public EnemyPokemon Enc6 { get; set; }
        public EnemyPokemon Enc7 { get; set; }
        public EnemyPokemon Enc8 { get; set; }
        public EnemyPokemon Enc9 { get; set; }
        public EnemyPokemon Enc10 { get; set; }
    }

    public class Trainer
    {
        public string TrainerName { get; set; }
        public int GroupNum { get; set; }
        public int TrainerNum { get; set; }
        public bool AllSameLevel { get; set; }
        public int PartyLevel { get; set; }
        public ObservableCollection<EnemyPokemon> Pokemons { get; set; }
    }

    public class TrainerGroup
    {
        public TrainerGroup(string name, int num)
        {
            GroupName = name;
            GroupNum = num;
        }
        public string GroupName { get; }
        public int GroupNum { get; }
    }

}
