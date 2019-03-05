using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace PokemonROMEditor.Models
{
    public class Pokemon: INotifyPropertyChanged
    {        
        public event PropertyChangedEventHandler PropertyChanged;

        public string Name { get; set; }
        public int PokedexID { get; set; }
        private int hp;
        public int HP {
            get { return hp; }
            set { hp = value; OnPropertyChanged(); OnPropertyChanged("StatTotal"); }
        }
        private int attack;
        public int Attack
        {
            get { return attack; }
            set { attack = value; OnPropertyChanged(); OnPropertyChanged("StatTotal"); }
        }
        private int defense;
        public int Defense
        {
            get { return defense; }
            set { defense = value; OnPropertyChanged(); OnPropertyChanged("StatTotal"); }
        }
        private int speed;
        public int Speed
        {
            get { return speed; }
            set { speed = value; OnPropertyChanged(); OnPropertyChanged("StatTotal"); }
        }
        private int special;
        public int Special
        {
            get { return special; }
            set { special = value; OnPropertyChanged(); OnPropertyChanged("StatTotal"); }
        }
        public int StatTotal
        {
            get
            {
                return (HP + Attack + Defense + Speed + Special);
            }
        }
        public int Type1 { get; set; }
        public int Type2 { get; set; }
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

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
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

    public class PokemonView
    {
        public PokemonView(int id, string name)
        {
            PokemonID = id;
            Name = name;
        }
        public int PokemonID { get; set; }
        public string Name { get; set; }
    }
}
