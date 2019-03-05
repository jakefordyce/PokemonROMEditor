using Microsoft.Win32;
using PokemonROMEditor.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace PokemonROMEditor.ViewModels
{
    public class ROMEditorViewModel: ViewModelBase
    {

        #region variables

        private ROMConverter romConverter;        
        private int moveByteMax;
        private int trainerByteMax;
        private int shopItemsMax;        
        private string resourceBase = "PokemonROMEditor.SourceImages.";
        #endregion

        #region Data Properties

        private string romFile;

        public string RomFile
        {
            get { return romFile; }
        }

        public string ToggleButtonText
        {
            get
            {
                if (showFullPokemonView)
                {
                    return "Toggle Grid View";
                }
                else
                {
                    return "Toggle Full View";
                }
                
            }
            
        }

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
                PokeNames = new ObservableCollection<PokemonView>();
                foreach (var p in pokemons)
                {
                    PokeNames.Add(new PokemonView(p.PokedexID, p.Name));                    
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

        private ObservableCollection<PokemonView> pokeNames;

        public ObservableCollection<PokemonView> PokeNames
        {
            get
            {
                return pokeNames;
            }
            set
            {
                pokeNames = value;
                OnPropertyChanged();
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

        private ObservableCollection<PokeType> pokeTypes;

        public ObservableCollection<PokeType> PokeTypes
        {
            get
            {
                return pokeTypes;
            }
            set
            {
                pokeTypes = value;
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

        private ObservableCollection<TrainerGroup> trainerGroups;

        public ObservableCollection<TrainerGroup> TrainerGroups
        {
            get
            {
                return trainerGroups;
            }
            set
            {
                trainerGroups = value;
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

        private ObservableCollection<Item> items;

        public ObservableCollection<Item> Items
        {
            get
            {
                return items;
            }
            set
            {
                items = value;
                OnPropertyChanged();
            }
        }
        
        // these are the images from the current blockset
        private ObservableCollection<BitmapSource> tiles;

        public ObservableCollection<BitmapSource> Tiles
        {
            get
            {
                return tiles;
            }
            set
            {
                tiles = value;
                OnPropertyChanged();
            }
        }

        // these are for selecting a tile to update the map
        private int selectedTileID;

        public int SelectedTileID
        {
            get { return selectedTileID; }
            set
            {
                selectedTileID = value;
                OnPropertyChanged();
                OnPropertyChanged("SelectedTileImage");
            }
        }

        public BitmapSource SelectedTileImage
        {
            get
            {
                if(Tiles.Count > 0)
                {
                    return tiles.ElementAt(selectedTileID);
                }
                else
                {
                    return null;
                }
            }
        }

        // these are the images for displaying the selected map
        private ObservableCollection<MapTile> selectedMapTiles;

        public ObservableCollection<MapTile> SelectedMapTiles
        {
            get { return selectedMapTiles; }
            set
            {
                selectedMapTiles = value;
                OnPropertyChanged();
            }
        }

        // these are the definitions of the blocks
        private ObservableCollection<BlockSet> blockSets;

        public ObservableCollection<BlockSet> BlockSets
        {
            get { return blockSets; }
            set { blockSets = value; OnPropertyChanged(); }
        }

        private ObservableCollection<Map> maps;

        public ObservableCollection<Map> Maps
        {
            get
            {
                return maps;
            }
            set
            {
                maps = value;
                OnPropertyChanged();
            }
        }

        private Map selectedMap;

        public Map SelectedMap
        {
            get
            {
                return selectedMap;
            }
            set
            {
                if (selectedMap != null)
                {
                    foreach (var ob in SelectedMap.MapObjects)
                    {
                        ob.SpriteChanged -= UpdateMapSprites;
                    }
                }
                selectedMap = value;
                if (selectedMap != null)
                {
                    foreach (var ob in SelectedMap.MapObjects)
                    {
                        ob.SpriteChanged += UpdateMapSprites;
                    }
                }
                //LoadTileset();
                LoadSelectedMapTilesImages();
                LoadSelectedMapImages();
                OnPropertyChanged();
            }
        }

        private ObservableCollection<MapObjectSprite> sprites;

        public ObservableCollection<MapObjectSprite> Sprites
        {
            get
            {
                return sprites;
            }
            set
            {
                sprites = value;
                OnPropertyChanged();
            }
        }

        /*
        private ObservableCollection<BitmapSource> spriteImages;

        public ObservableCollection<BitmapSource> SpriteImages
        {
            get
            {
                return spriteImages;
            }
            set
            {
                spriteImages = value;
                OnPropertyChanged();
            }
        }
        //*/

        private MapObject selectedMapObject;

        public MapObject SelectedMapObject
        {
            get
            {
                return selectedMapObject;
            }
            set
            {
                selectedMapObject = value;
                LoadSelectedMapImages(); //should eventually find a way that doesn't require redrawing the entire map.
                OnPropertyChanged();
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

        private int extraShopItems;

        public int ExtraShopItems
        {
            get { return extraShopItems; }
            set
            {
                extraShopItems = value;
                OnPropertyChanged();
            }
        }

        public bool CanAddShopItem()
        {
            if (ExtraShopItems > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool CanSave()
        {
            return (ExtraTrainerBytes >= 0 && ExtraMoveBytes >= 0 && DataLoaded && (TypeStrengths.Count <= 82));
        }

        private bool showFullPokemonView;

        public bool ShowFullPokemonView
        {
            get
            {
                return (dataLoaded && showFullPokemonView);               
            }
            
        }

        public bool ShowGridPokemonView
        {
            get
            {
                return (dataLoaded && !showFullPokemonView);
            }
            
        }

        #endregion

        public ROMEditorViewModel()
        {
            DataLoaded = false;
            showFullPokemonView = true;
            Moves = new ObservableCollection<Move>();
            Pokemons = new ObservableCollection<Pokemon>();
            romConverter = new ROMConverter();
            PokemonTMs = new ObservableCollection<TMCompatible>();
            AllTMs = new ObservableCollection<TM>();
            PokeTypes = new ObservableCollection<PokeType>();
            TypeStrengths = new ObservableCollection<TypeStrength>();
            EncounterZones = new ObservableCollection<WildEncounterZone>();
            Trainers = new ObservableCollection<Trainer>();
            Shops = new ObservableCollection<Shop>();
            Items = new ObservableCollection<Item>();
            Tiles = new ObservableCollection<BitmapSource>();
            SelectedMapTiles = new ObservableCollection<MapTile>();
            BlockSets = new ObservableCollection<BlockSet>();
            Maps = new ObservableCollection<Map>();
            TrainerGroups = new ObservableCollection<TrainerGroup>();
            Sprites = new ObservableCollection<MapObjectSprite>();
            //SpriteImages = new ObservableCollection<BitmapSource>();
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

        private ICommand addShopItem;

        public ICommand AddShopItem
        {
            get
            {
                return addShopItem ?? (addShopItem = new RelayCommand(
                    x =>
                    {
                        SelectedShop.Items.Add(new Item(ItemType.ULTRA_BALL));
                        CountShopItems();
                    },
                    x => CanAddShopItem()));
            }
        }

        private ICommand deleteShopItem;

        public ICommand DeleteShopItem
        {
            get
            {
                return deleteShopItem ?? (deleteShopItem = new RelayCommand(
                    x =>
                    {
                        SelectedShop.Items.Remove((Item)x);
                        CountShopItems();
                    }));
            }
        }

        private ICommand togglePokemonView;

        public ICommand TogglePokemonView
        {
            get
            {
                return togglePokemonView ?? (togglePokemonView = new RelayCommand(
                    x =>
                    {
                        showFullPokemonView = !showFullPokemonView;
                        OnPropertyChanged("ToggleButtonText");
                        OnPropertyChanged("ShowFullPokemonView");
                        OnPropertyChanged("ShowGridPokemonView");
                    }));
            }
        }

        private ICommand selectTile;

        public ICommand SelectTile
        {
            get
            {
                return selectTile ?? (selectTile = new RelayCommand(
                    x =>
                    {
                        SelectedTileID = Tiles.IndexOf((BitmapSource)x);
                    }));
            }
        }

        private ICommand updateMapTile;

        public ICommand UpdateMapTile
        {
            get
            {
                return updateMapTile ?? (updateMapTile = new RelayCommand(
                    x =>
                    {
                        var clickedTile = (MapTile)x;
                        SelectedMap.MapBlockValues[clickedTile.TileID] = SelectedTileID;
                        LoadSelectedMapImages();
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
                OnPropertyChanged("RomFile");
                //load data from ROM
                romConverter.LoadROMDataFromFile(romFile);
                PokeTypes = romConverter.LoadPokeTypes();
                Moves = romConverter.LoadMoves();
                Pokemons = romConverter.LoadPokemon();
                AllTMs = romConverter.LoadTMs();
                moveByteMax = romConverter.GetMaxMoveBytes();
                TypeStrengths = romConverter.LoadTypeStrengths();
                EncounterZones = romConverter.LoadWildEncounters();
                TrainerGroups = romConverter.LoadTrainerGroups();
                Trainers = romConverter.LoadTrainers();
                trainerByteMax = romConverter.GetMaxTrainerBytes();
                Shops = romConverter.LoadShops();
                Items = romConverter.LoadItems();
                shopItemsMax = romConverter.GetMaxShopItems();                
                BlockSets = romConverter.LoadBlockSets();
                Maps = romConverter.LoadMaps();
                Sprites = romConverter.LoadSprites();

                //Initialize counts
                CountMoveBytes();
                CountTrainerBytes();
                CountShopItems();

                //LoadTileset();
                LoadBlocksetTiles();
                LoadSpriteBitmaps();
                //LoadSpriteImages();
                UpdateTrainerNames();


                DataLoaded = true;
                OnPropertyChanged("ShowFullPokemonView");
                OnPropertyChanged("ShowGridPokemonView");
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
                OnPropertyChanged("RomFile");
                SaveFile();                
            }
        }

        private void SaveFile()
        {
            romConverter.SaveROMDataToFile(romFile, Moves, Pokemons, TypeStrengths, AllTMs, EncounterZones, Trainers, Shops, Items, Maps, PokeTypes);
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

        private void CountShopItems()
        {
            int count = 0;

            foreach(var shop in Shops)
            {
                foreach(var item in shop.Items)
                {
                    count++;
                }
            }

            ExtraShopItems = shopItemsMax - count;
        }

        private void LoadBlocksetTiles()
        {
            // sourceBitmap is our 1 image that is being chopped up and made into the blocks that are used to create the tilesets.
            Bitmap sourceBitmap;

            // createdBitMap is one of the 32x32 usable blocks that we have created from our source image.
            Bitmap createdBitmap;

            // chunkOfBitmap will be our 8x8 building blocks that are cut from the source image.
            Bitmap chunkOfBitmap;

            int numOfTiles;
            int xpos;
            int ypos;
            int currentByte;            

            foreach (var b in BlockSets)
            {
                b.Tiles.Clear();
                
                sourceBitmap = new Bitmap( Assembly.GetEntryAssembly().GetManifestResourceStream(resourceBase + b.SourceFile));

                numOfTiles = b.BlockDefinitions.Count() / 16;

                for (int i = 0; i < numOfTiles; i++)
                {
                    createdBitmap = new Bitmap(32, 32); //the actual images are 32x32 but that's hard to see so I stretched it out to 64x64
                    using (Graphics g = Graphics.FromImage(createdBitmap))
                    {
                        for (int y = 0; y < 4; y++)
                        {
                            for (int x = 0; x < 4; x++)
                            {
                                currentByte = (i * 16) + (y * 4) + x;

                                //these are used to mark where we start our crop from the source image.
                                xpos = (b.BlockDefinitions[currentByte] % 16);
                                ypos = (b.BlockDefinitions[currentByte] / 16);

                                if (ypos > 7)
                                {
                                    ypos = 0;
                                    xpos = 0;
                                }

                                xpos *= 8;
                                ypos *= 8;

                                // take the proper chunk from the source image...
                                chunkOfBitmap = sourceBitmap.Clone(new Rectangle(xpos, ypos, 8, 8), sourceBitmap.PixelFormat);

                                // and add it to our final image. The 8x8 block is being stretched to 16x16 to make it easier to see.
                                g.DrawImage(chunkOfBitmap, x * 8, y * 8, 8, 8);
                                chunkOfBitmap.Dispose();
                            }
                        }
                    }
                    b.Tiles.Add(createdBitmap);
                    //createdBitmap.Dispose();
                }
                sourceBitmap.Dispose();

            }
        }

        private void LoadSpriteBitmaps()
        {
            Bitmap sourceBitmap;
            Bitmap createdBitmap;
            Bitmap chunkOfBitmap;            

            foreach (var s in Sprites)
            {                
                sourceBitmap = new Bitmap(Assembly.GetEntryAssembly().GetManifestResourceStream(resourceBase + s.FileName));

                createdBitmap = new Bitmap(16, 16);

                using(Graphics g = Graphics.FromImage(createdBitmap))
                {
                    chunkOfBitmap = sourceBitmap.Clone(new Rectangle(0, 0, 16, 16), sourceBitmap.PixelFormat);

                    g.DrawImage(chunkOfBitmap, 0, 0, 16, 16);
                    chunkOfBitmap.Dispose();
                }
                s.SpriteBitmap = createdBitmap;
                sourceBitmap.Dispose();
            }
        }

        /*
        private void LoadSpriteImages()
        {
            SpriteImages.Clear();
            foreach(var s in Sprites)
            {
                SpriteImages.Add(Bitmap2BitmapSource(s.SpriteBitmap));
            }
        }
        //*/

        private void LoadSelectedMapTilesImages()
        {
            Tiles.Clear();
            foreach(var t in BlockSets.ElementAt((int)selectedMap.TileSetID).Tiles)
            {
                Tiles.Add(Bitmap2BitmapSource(t));
            }
        }        

        private BitmapSource Bitmap2BitmapSource(Bitmap bitmap)
        {
            var hbitmap = bitmap.GetHbitmap();
            BitmapSource i = Imaging.CreateBitmapSourceFromHBitmap(
                               hbitmap,
                               IntPtr.Zero,
                               Int32Rect.Empty,
                               BitmapSizeOptions.FromEmptyOptions());
            DeleteObject(hbitmap);
            return i;
        }

        //Need this to free up memory after using GetHbitmap()
        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        private static extern bool DeleteObject(IntPtr hObject);

        private void LoadSelectedMapImages()
        {
            SelectedMapTiles = new ObservableCollection<MapTile>();

            Bitmap createdBitmap;

            Bitmap sourceBitmap;

            Bitmap chunkOfBitmap;

            int blockToLoad;

            for (int i = 0; i < SelectedMap.MapBlockValues.Count(); i++)
            {
                blockToLoad = SelectedMap.MapBlockValues[i];

                if(blockToLoad >= BlockSets.ElementAt((int)selectedMap.TileSetID).Tiles.Count())
                {
                    blockToLoad = 0;
                }

                sourceBitmap = BlockSets.ElementAt((int)selectedMap.TileSetID).Tiles.ElementAt(blockToLoad);

                createdBitmap = new Bitmap(32, 32);

                using(Graphics g = Graphics.FromImage(createdBitmap))
                {
                    g.DrawImage(sourceBitmap, 0, 0);
                    foreach (var ob in selectedMap.MapObjects)
                    {
                        if (i == MatchesBlock(ob.XPosition, ob.YPosition, selectedMap.Width))
                        {
                            chunkOfBitmap = Sprites.ElementAt(ob.SpriteID).SpriteBitmap.Clone(new Rectangle(0, 0, 16, 16), Sprites.ElementAt(ob.SpriteID).SpriteBitmap.PixelFormat);
                            int xpos = (ob.XPosition % 2) * 16;
                            int ypos = (ob.YPosition % 2) * 16;                            
                            g.DrawImage(chunkOfBitmap, xpos, ypos, 16, 16);

                            if(selectedMapObject != null) //highlite the selected object.
                            {
                                if (selectedMapObject.XPosition == ob.XPosition && selectedMapObject.YPosition == ob.YPosition)
                                {
                                    g.DrawRectangle(new Pen(Color.Red, 2), new Rectangle(xpos, ypos, 16, 16));
                                }
                            }
                        }
                    }
                }

                SelectedMapTiles.Add(new MapTile(i, Bitmap2BitmapSource(createdBitmap)));
            }
        }

        private int MatchesBlock(int x, int y, int width)
        {
            return ((y / 2 * width) + x / 2);
        }

        private void UpdateTrainerNames()
        {
            foreach ( var t in Trainers)
            {
                t.TrainerName = (from g in TrainerGroups
                                 where t.GroupNum == g.GroupNum
                                 select g.GroupName).First() + " " + t.TrainerNum;
            }
            OnPropertyChanged("Trainers");
        }

        private void UpdateMapSprites(object sender, EventArgs e)
        {
            LoadSelectedMapImages();
        }

        #endregion
    }

}
