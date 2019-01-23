﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonROMEditor.Models
{
    public class Pokemon
    {
        

        public string Name { get; set; }
        public int PokedexID { get; set; }
        public int HP { get; set; }
        public int Attack { get; set; }
        public int Defense { get; set; }
        public int Speed { get; set; }
        public int Special { get; set; }
        public int StatTotal
        {
            get
            {
                return (HP + Attack + Defense + Speed + Special);
            }
        }
        public PokeType Type1 { get; set; }
        public PokeType Type2 { get; set; }
        public int CatchRate { get; set; }
        public int ExpYield { get; set; }
        public int Move1 { get; set; }
        public int Move2 { get; set; }
        public int Move3 { get; set; }
        public int Move4 { get; set; }
        public GrowthType GrowthRate { get; set; }
        public ObservableCollection<Evolution> Evolutions { get; set; }
        public ObservableCollection<LearnedMove> LearnedMoves { get; set; }
        public ObservableCollection<TMCompatible> TMs { get; set; }

        
    }

    public class Evolution
    {
        public event EventHandler EvolveTypeChanged;

        private EvolveType evolve;
        public EvolveType Evolve
        {
            get
            {
                return evolve;
            }
            set
            {
                evolve = value;
                OnEvolveTypeChanged();
            }
        }
        public int EvolveLevel { get; set; }
        public EvolveStone EvolutionStone { get; set; }
        public int EvolveTo { get; set; }

        protected virtual void OnEvolveTypeChanged()
        {
            EvolveTypeChanged?.Invoke(this, new EventArgs());
        }
    }

    public class LearnedMove
    {
        public event EventHandler LevelChanged;
        private int level;
        public int Level
        {
            get
            {
                return level;
            }
            set
            {
                level = value;
                OnLevelChanged();
            }
        }
        public int MoveID
        {
            get;
            set;
        }

        protected virtual void OnLevelChanged()
        {
            LevelChanged?.Invoke(this, new EventArgs());
        }
    }

    public class EnemyPokemon
    {
        public EnemyPokemon()
        {
            Level = 1;
            PokedexID = 1;
        }
        public EnemyPokemon(int lvl, int ind)
        {
            Level = lvl;
            PokedexID = ind;
        }
        public int Level { get; set; }
        public int PokedexID { get; set; }
    }
}