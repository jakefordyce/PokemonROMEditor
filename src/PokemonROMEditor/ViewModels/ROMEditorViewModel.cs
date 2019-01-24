using Microsoft.Win32;
using PokemonROMEditor.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PokemonROMEditor.ViewModels
{
    public class ROMEditorViewModel: ViewModelBase
    {

        #region variables

        private ROMConverter romConverter;
        private string romFile;
        private int moveByteMax;
        private int trainerByteMax;
        #endregion

        #region Data Properties
        private ObservableCollection<Move> moves;

        public ObservableCollection<Move> Moves
        {
            get { return moves; }
            set
            {
                moves = value;
                MoveNames = new ObservableCollection<MoveView>();
                foreach (var m in Moves)
                {
                    moveNames.Add(new MoveView(m.ID, m.Name));
                    m.NameChanged += UpdateMoveNamesEvent;
                }
                OnPropertyChanged();
            }
        }

        private ObservableCollection<MoveView> moveNames;

        public ObservableCollection<MoveView> MoveNames
        {
            get { return moveNames; }
            set
            {
                moveNames = value;
                OnPropertyChanged();
            }
        }

        private Move selectedMove;

        public Move SelectedMove
        {
            get { return selectedMove; }
            set
            {
                selectedMove = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<Pokemon> pokemons;

        public ObservableCollection<Pokemon> Pokemons
        {
            get { return pokemons; }
            set
            {
                pokemons = value;
                foreach (var p in pokemons)
                {
                    foreach (var e in p.Evolutions)
                    {
                        e.EvolveTypeChanged += UpdateDoesPokemonEvolve;
                    }
                }
                OnPropertyChanged();
            }
        }

        private Pokemon selectedPokemon;

        public Pokemon SelectedPokemon
        {
            get { return selectedPokemon; }
            set
            {
                if(selectedPokemon != null)
                {
                    foreach (var m in SelectedPokemon.LearnedMoves)
                    {
                        m.LevelChanged -= SortLearnedMoves;
                    }
                }
                selectedPokemon = value;
                UpdatePokemonTMNames();
                if(selectedPokemon != null)
                {
                    foreach (var m in SelectedPokemon.LearnedMoves)
                    {
                        m.LevelChanged += SortLearnedMoves;
                    }
                }                
                OnPropertyChanged();
                OnPropertyChanged("IsPokemonSelected");
                OnPropertyChanged("DoesPokemonEvolve");
            }
        }

        private ObservableCollection<TM> allTMs;

        public ObservableCollection<TM> AllTMs
        {
            get
            {
                return allTMs;
            }
            set
            {
                allTMs = value;
                OnPropertyChanged();
            }
        }


        private ObservableCollection<TMCompatible> pokemonTMs;

        public ObservableCollection<TMCompatible> PokemonTMs
        {
            get
            {
                return pokemonTMs;
            }
            set
            {
                pokemonTMs = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<TypeStrength> typeStrengths;

        public ObservableCollection<TypeStrength> TypeStrengths
        {
            get { return typeStrengths; }
            set
            {
                if (value.Count <= 82)
                {
                    typeStrengths = value;
                }
                OnPropertyChanged();
            }
        }

        private TypeStrength selectedTypeStrength;

        public TypeStrength SelectedTypeStrength
        {
            get { return selectedTypeStrength; }
            set
            {
                selectedTypeStrength = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<WildEncounterZone> encounterZones;

        public ObservableCollection<WildEncounterZone> EncounterZones
        {
            get { return encounterZones; }
            set
            {
                encounterZones = value;
                OnPropertyChanged();
            }
        }

        private WildEncounterZone selectedEncounterZone;

        public WildEncounterZone SelectedEncounterZone
        {
            get { return selectedEncounterZone; }
            set
            {
                selectedEncounterZone = value;
                OnPropertyChanged();
                OnPropertyChanged("IsZoneSelected");
            }
        }

        private ObservableCollection<Trainer> trainers;

        public ObservableCollection<Trainer> Trainers
        {
            get { return trainers; }
            set
            {
                trainers = value;
                OnPropertyChanged();
            }
        }

        private Trainer selectedTrainer;

        public Trainer SelectedTrainer
        {
            get { return selectedTrainer; }
            set
            {
                selectedTrainer = value;
                OnPropertyChanged();
                OnPropertyChanged("IsTrainerSelected");
            }
        }

        private ObservableCollection<Shop> shops;

        public ObservableCollection<Shop> Shops
        {
            get
            {
                return shops;
            }
            set
            {
                shops = value;
                OnPropertyChanged();
            }
        }

        private Shop selectedShop;

        public Shop SelectedShop
        {
            get { return selectedShop; }
            set
            {
                selectedShop = value;
                OnPropertyChanged();
                OnPropertyChanged("IsShopSelected");
            }
        }

        #endregion

        #region Behavior Properties
        private bool dataLoaded;

        public bool DataLoaded
        {
            get { return dataLoaded; }
            set
            {
                dataLoaded = value;
                OnPropertyChanged();
            }
        }

        public bool IsPokemonSelected
        {
            get
            {
                return selectedPokemon != null;
            }
        }

        public bool IsZoneSelected
        {
            get
            {
                return selectedEncounterZone != null;
            }
        }

        public bool IsTrainerSelected
        {
            get
            {
                return selectedTrainer != null;
            }
        }

        public bool IsShopSelected
        {
            get
            {
                return selectedShop != null;
            }
        }

        public bool DoesPokemonEvolve
        {
            get
            {
                if (IsPokemonSelected)
                {
                    return (SelectedPokemon.Evolutions.Count > 0);
                }
                else
                {
                    return false;
                }

            }
        }

        public bool CanAddLearnedMove()
        {
            if (extraMoveBytes > 1)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool CanAddTrainerPokemon()
        {
            if(selectedTrainer != null)
            {
                if (extraTrainerBytes > (selectedTrainer.AllSameLevel ? 0 : 1))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }            
            else
            {
                return false;
            }
        }

        private int extraMoveBytes;

        public int ExtraMoveBytes
        {
            get
            {
                return extraMoveBytes;
            }
            set
            {
                extraMoveBytes = value;
                OnPropertyChanged();
            }
        }

        public bool CanAddTypeStrength()
        {
            return (TypeStrengths.Count < 82);
        }

        private int extraTrainerBytes;

        public int ExtraTrainerBytes
        {
            get
            {
                return extraTrainerBytes;
            }
            set
            {
                extraTrainerBytes = value;
                OnPropertyChanged();
            }
        }

        public bool CanSave()
        {
            return (ExtraTrainerBytes >= 0 && ExtraMoveBytes >= 0 && DataLoaded);
        }

        #endregion

        public ROMEditorViewModel()
        {
            DataLoaded = false;
            Moves = new ObservableCollection<Move>();
            Pokemons = new ObservableCollection<Pokemon>();
            romConverter = new ROMConverter();
            PokemonTMs = new ObservableCollection<TMCompatible>();
            AllTMs = new ObservableCollection<TM>();
            TypeStrengths = new ObservableCollection<TypeStrength>();
            EncounterZones = new ObservableCollection<WildEncounterZone>();
            Trainers = new ObservableCollection<Trainer>();
            Shops = new ObservableCollection<Shop>();
        }

        #region Public Methods        

        private ICommand addLearnedMove;

        public ICommand AddLearnedMove
        {
            get
            {
                return addLearnedMove ?? (addLearnedMove = new RelayCommand(
                    x =>
                    {
                        SelectedPokemon.LearnedMoves.Add(new LearnedMove());
                        CountMoveBytes();
                    },
                    x => CanAddLearnedMove()));
            }
        }

        private ICommand deleteLearnedMove;

        public ICommand DeleteLearnedMove
        {
            get
            {
                return deleteLearnedMove ?? (deleteLearnedMove = new RelayCommand(
                    x =>
                    {
                        SelectedPokemon.LearnedMoves.Remove((LearnedMove)x);
                        CountMoveBytes();
                    }
                    ));
            }
        }

        private ICommand openROMFile;

        public ICommand OpenROMFile
        {
            get
            {
                return openROMFile ?? (openROMFile = new RelayCommand(
                    x =>
                    {
                        OpenFile();
                    }));
            }
        }

        private ICommand saveROMFileAs;

        public ICommand SaveROMFileAs
        {
            get
            {
                return saveROMFileAs ?? (saveROMFileAs = new RelayCommand(
                    x =>
                    {
                        SaveFileAs();
                    },
                    x => CanSave()
                    ));
            }
        }

        private ICommand saveROMFile;

        public ICommand SaveROMFile
        {
            get
            {
                return saveROMFile ?? (saveROMFile = new RelayCommand(
                    x =>
                    {
                        SaveFile();
                    },
                    x => CanSave()
                    ));
            }
        }

        private ICommand deleteTypeStrength;

        public ICommand DeleteTypeStrength
        {
            get
            {
                return deleteTypeStrength ?? (deleteTypeStrength = new RelayCommand(
                    x =>
                    {
                        TypeStrengths.Remove((TypeStrength)x);
                    }));
            }
        }

        private ICommand addTypeStrength;

        public ICommand AddTypeStrength
        {
            get
            {
                return addTypeStrength ?? (addTypeStrength = new RelayCommand(
                    x =>
                    {
                        TypeStrengths.Add(new TypeStrength());
                    },
                    x => CanAddTypeStrength()));
            }
        }

        private ICommand addTrainerPokemon;

        public ICommand AddTrainerPokemon
        {
            get
            {
                return addTrainerPokemon ?? (addTrainerPokemon = new RelayCommand(
                    x =>
                    {
                        SelectedTrainer.Pokemons.Add(new EnemyPokemon());
                        CountTrainerBytes();
                    },
                    x => CanAddTrainerPokemon()));
            }
        }

        private ICommand deleteTrainerPokemon;

        public ICommand DeleteTrainerPokemon
        {
            get
            {
                return deleteTrainerPokemon ?? (deleteTrainerPokemon = new RelayCommand(
                    x =>
                    {
                        SelectedTrainer.Pokemons.Remove((EnemyPokemon)x);
                        CountTrainerBytes();
                    }));
            }
        }

        #endregion

        #region Private Methods

        private void OpenFile()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            // for now just looking for regular Gameboy roms since this is designed for Pokemon Red.
            ofd.DefaultExt = ".gb";
            ofd.Filter = "GameBoy Rom Files (*.gb)|*.gb";

            bool? result = ofd.ShowDialog();
            if (result == true)
            {
                romFile = ofd.FileName;
                romConverter.LoadROMDataFromFile(romFile);
                Moves = romConverter.LoadMoves();
                Pokemons = romConverter.LoadPokemon();
                AllTMs = romConverter.LoadTMs();
                moveByteMax = romConverter.GetMaxMoveBytes();
                TypeStrengths = romConverter.LoadTypeStrengths();
                EncounterZones = romConverter.LoadWildEncounters();
                Trainers = romConverter.LoadTrainers();
                trainerByteMax = romConverter.GetMaxTrainerBytes();
                Shops = romConverter.LoadShops();
                DataLoaded = true;
            }
        }

        private void SaveFileAs()
        {
            SaveFileDialog ofd = new SaveFileDialog();
            // for now just looking for regular Gameboy roms since this is designed for Pokemon Red.
            ofd.DefaultExt = ".gb";
            ofd.Filter = "GameBoy Rom Files (*.gb)|*.gb";

            bool? result = ofd.ShowDialog();
            if (result == true)
            {
                romFile = ofd.FileName;
                romConverter.SaveROMDataToFile(romFile, Moves, Pokemons, TypeStrengths, AllTMs, EncounterZones, Trainers, Shops);
                DataLoaded = true;
            }
        }

        private void SaveFile()
        {
            romConverter.SaveROMDataToFile(romFile, Moves, Pokemons, TypeStrengths, AllTMs, EncounterZones, Trainers, Shops);
            DataLoaded = true;
        }

        private void UpdatePokemonTMNames()
        {
            if (SelectedPokemon != null)
            {
                pokemonTMs = SelectedPokemon.TMs;
                foreach (var tm in pokemonTMs)
                {
                    tm.TMMoveName = (from m in moves
                                     join t in AllTMs on m.ID equals t.MoveID
                                     where tm.TMNumber == t.TMNumber
                                     select m.Name).First();
                }
            }
            OnPropertyChanged("PokemonTMs");
        }

        private void UpdateMoveNamesEvent(object sender, EventArgs e)
        {
            UpdateMoveNames();
        }

        private void UpdateMoveNames()
        {
            foreach (var n in moveNames)
            {
                n.Name = (from m in moves
                          where n.MoveID == m.ID
                          select m.Name).First();
            }
            OnPropertyChanged("MoveNames");
        }

        private void UpdateDoesPokemonEvolve(object sender, EventArgs e)
        {
            OnPropertyChanged("DoesPokemonEvolve");
            OnPropertyChanged("ShowStonesCombo");
            OnPropertyChanged("ShowStonesCombo");
            OnPropertyChanged("ShowLevelTextbox");
            CountMoveBytes();
        }

        private void SortLearnedMoves(object sender, EventArgs e)
        {
            var temp = SelectedPokemon.LearnedMoves.OrderBy(x => x.Level).ToList();
            SelectedPokemon.LearnedMoves.Clear();
            foreach( var t in temp)
            {
                SelectedPokemon.LearnedMoves.Add(t);
            }
        }

        private void CountMoveBytes()
        {
            int count = 0;

            foreach (var p in Pokemons)
            {
                count += 2; //every pokemon has at least 2 bytes of data.

                foreach(var e in p.Evolutions)
                {
                    if (e.Evolve == EvolveType.LEVEL || e.Evolve == EvolveType.TRADE)
                    {
                        count += 3;
                    }
                    else if (e.Evolve == EvolveType.STONE)
                    {
                        count += 4;
                    }

                }

                foreach (var m in p.LearnedMoves)
                {
                    count += 2;
                }
            }

            ExtraMoveBytes = moveByteMax - count;
        }

        private void CountTrainerBytes()
        {
            int count = 0;

            foreach(var t in Trainers)
            {
                count += 2; //every trainer has at least 2 bytes
                if (t.AllSameLevel)
                {
                    count += t.Pokemons.Count; //1 byte per pokemon if they are all the same level
                }
                else
                {
                    count += (t.Pokemons.Count * 2); //2 bytes if they each have a different level
                }
            }

            ExtraTrainerBytes = trainerByteMax - count;
        }

        #endregion
    }

}
