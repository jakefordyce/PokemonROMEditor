using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonROMEditor.Models
{
    public class ROMConverter
    {
        private byte[] romData;
        private Dictionary<int, int> PokedexIDs;
        private Dictionary<int, int> IndexIDs;

        int mapHeaderPointersByte = 430; // 0x1AE 
        int shopsStartByte = 9282; //The data for the pokemarts inventories starts at byte 0x2442
        int mewStartByte = 16987; // Mew's stats start at byte 0x425B
        int itemPricesStartByte = 17928; // The prices for items start at byte 0x4608
        int mapBanksByte = 49725; // 0xC23D 
        int tilesetHeadersByte = 51134; //0xC7BE
        int wildEncountersByte = 53471; //The data for wild encounters start at 0xD0DE but the first one is empty so I skip it.
        int wildEncountersEndByte = 54727;
        int tmStartByte = 79731; //The TM info starts at byte 0x13773.
        int pokemonNameStartByte = 115230; //Pokemon names start at byte 0x1c21e and also run in Index order.
        int pointerBase = 212992; //this is the value that is added to the pointers to get the location of evolutions/moves and trainer data
        int movesStartingByte = 229376; //The move data starts 0x38000 bytes into the file which is 229376 in Decimal.
        int pokemonStartByte = 230366; //Pokemon data starts at byte 0x383DE. It goes in Pokedex order, Bulbasaur through Mewtwo.
        int trainerNamesByte = 236031; //The names of the trainer groups start at byte 0x399FF
        int trainerPointersByte = 236859; // The pointers to the trainer groups start at byte 0x39D3B
        int trainerStartByte = 236953; //The data for trainers starts at 0x39D99
        int trainerEndByte = 238893; //The last byte for trainers is 0x3A52D
        int pokemonEvosPointersByte = 241756; //The pointers to the pokemon Evolutions and learned moves start at byte 0x3B05C.
        int pokemonEvosMovesByte = 242136; //Pokemon evolutions and moves learned through leveling are stored together starting at byte 0x3B1D8.
        int typeChartByte = 255092; //The types' strengths start at byte 0x3E474.
        int pokedexStartByte = 266276; //List of pokedex IDs start at byte 0x41024 and run in Index order, Rhydon through Victreebel.
        int tmPricesStartByte = 507815; //TM prices start at byte 0x7BFA7
        int moveNamesByte = 720896; //The data for move names starts at 0xB0000 bytes into the file which is 720896 in Decimal.

        string imageFolder = "..\\..\\SourceImages\\";

        //The locations of the pointers for each shop.
        //The first 0x00 is the bike shop. The script for the bike shop menu doesn't actually look at the bike shop's items.
        //The second 0x00 is an unused shop. 
        int[] shopPointerBytes = { 0x1D4EA, 0x74CB6, 0x5C898, 0x00, 0x5C9E4, 0x5692F, 0x560F8, 0x560FA, 0x48359, 0x49070, 0x49072, 0x1DD8B, 0x00, 0x75E81, 0x5D40C, 0x19C85};

        public ROMConverter() { }

        public ROMConverter(string fileName)
        {
            LoadROMDataFromFile(fileName);
        }

        public void LoadROMDataFromFile(string fileName)
        {
            romData = File.ReadAllBytes(fileName);
        }

        public void SaveROMDataToFile(string fileName, ObservableCollection<Move> moves, ObservableCollection<Pokemon> pokemons, ObservableCollection<TypeStrength> str,
                                        ObservableCollection<TM> tms, ObservableCollection<WildEncounterZone> zones, ObservableCollection<Trainer> trainers, ObservableCollection<Shop> shops,
                                        ObservableCollection<Item> items, ObservableCollection<Map> maps)
        {            
            // first update the byte array with the data that the user has modified.
            SaveMoves(moves);
            SavePokemon(pokemons);
            SaveTypeStrengths(str);
            SaveTMs(tms);
            SaveWildEncounters(zones);
            SaveTrainers(trainers);
            SaveShops(shops);
            SaveItems(items);
            SaveMaps(maps);
            // then just write the new byte array to file.
            File.WriteAllBytes(fileName, romData);
        }

        public ObservableCollection<Move> LoadMoves()
        {
            var moves = new ObservableCollection<Move>();
            int currentMoveNameByte = moveNamesByte;
            Move moveToAdd;
            string moveName;

            moveToAdd = new Move(); //The ROM uses 0x00 for blank starter moves in the pokemon base stats section.
            moveToAdd.ID = 0;
            moveToAdd.Name = "nothing";
            moves.Add(moveToAdd);

            for (int i = 0; i < 165; i++) //165 because there are 165 moves in the game.
            {
                moveName = "";

                //Luckily the names for the moves are stored in the same order as the moves, just in a different spot in memory.
                while (romData[currentMoveNameByte] != 0x50) //0x50 is the deliminator for the end of a name.
                {
                    moveName += Letters[romData[currentMoveNameByte]];
                    currentMoveNameByte++;
                }
                currentMoveNameByte++;

                moveToAdd = new Move();
                moveToAdd.ID = i + 1;
                moveToAdd.Name = moveName;
                //Each Move uses 6 bytes. i = the current move so we take the starting point and add 6 for each move
                // that we have already read and then add 0-5 as we read through the data fields for that move.
                // we skip the first byte because it is basically the move ID.
                moveToAdd.AnimationID = (MoveAnimation)romData[movesStartingByte + (i * 6)];
                moveToAdd.Effect = (MoveEffect)romData[movesStartingByte + (i * 6) + 1];
                moveToAdd.Power = romData[movesStartingByte + (i * 6) + 2];
                moveToAdd.MoveType = (PokeType)romData[movesStartingByte + (i * 6) + 3];
                moveToAdd.Accuracy = romData[movesStartingByte + (i * 6) + 4];
                moveToAdd.PP = romData[movesStartingByte + (i * 6) + 5];

                moves.Add(moveToAdd);
            }

            return moves;
        }

        private void SaveMoves(ObservableCollection<Move> moves)
        {
            int currentMoveNameByte = moveNamesByte;

            for(int i = 0; i < 165; i++)
            {
                romData[movesStartingByte + (i * 6)] = (byte)moves.ElementAt(i + 1).AnimationID;
                romData[movesStartingByte + (i * 6) + 1] = (byte)moves.ElementAt(i+1).Effect;
                romData[movesStartingByte + (i * 6) + 2] = (byte)moves.ElementAt(i+1).Power;
                romData[movesStartingByte + (i * 6) + 3] = (byte)moves.ElementAt(i+1).MoveType;
                romData[movesStartingByte + (i * 6) + 4] = (byte)moves.ElementAt(i+1).Accuracy;
                romData[movesStartingByte + (i * 6) + 5] = (byte)moves.ElementAt(i+1).PP;

                foreach(var s in moves.ElementAt(i + 1).Name)
                {
                    romData[currentMoveNameByte] = (byte)LetterValues[s.ToString().ToUpper()];
                    currentMoveNameByte++;
                }
                romData[currentMoveNameByte] = 0x50;
                currentMoveNameByte++;
            }

        }

        public ObservableCollection<Pokemon> LoadPokemon()
        {
            var pokemon = new ObservableCollection<Pokemon>();
            PokedexIDs = new Dictionary<int, int>();
            IndexIDs = new Dictionary<int, int>();
            int currentEvosMovesByte = pokemonEvosMovesByte;
            
            Pokemon pokeToAdd;
            Evolution evo;

            string pokemonName;
            LearnedMove moveToAdd;
            byte[] tmArray;

            for (int i = 0; i < 150; i++) //There are 151 pokemon in the game but Mew is stored separately from the others.
            {
                pokeToAdd = new Pokemon();
                pokeToAdd.PokedexID = romData[pokemonStartByte + (i * 28)]; // 28 because there are 28 bytes of data per Pokemon.
                pokeToAdd.HP = romData[pokemonStartByte + (i * 28) + 1];
                pokeToAdd.Attack = romData[pokemonStartByte + (i * 28) + 2];
                pokeToAdd.Defense = romData[pokemonStartByte + (i * 28) + 3];
                pokeToAdd.Speed = romData[pokemonStartByte + (i * 28) + 4];
                pokeToAdd.Special = romData[pokemonStartByte + (i * 28) + 5];
                pokeToAdd.Type1 = (PokeType)romData[pokemonStartByte + (i * 28) + 6];
                pokeToAdd.Type2 = (PokeType)romData[pokemonStartByte + (i * 28) + 7];
                pokeToAdd.CatchRate = romData[pokemonStartByte + (i * 28) + 8];
                pokeToAdd.ExpYield = romData[pokemonStartByte + (i * 28) + 9];
                pokeToAdd.Move1 = romData[pokemonStartByte + (i * 28) + 15];
                pokeToAdd.Move2 = romData[pokemonStartByte + (i * 28) + 16];
                pokeToAdd.Move3 = romData[pokemonStartByte + (i * 28) + 17];
                pokeToAdd.Move4 = romData[pokemonStartByte + (i * 28) + 18];
                pokeToAdd.GrowthRate = (GrowthType)romData[pokemonStartByte + (i * 28) + 19];
                tmArray = new byte[7]; //The next 7 bytes tell us which TMs/HMs the pokemon can learn.
                tmArray[0] = romData[pokemonStartByte + (i * 28) + 20];
                tmArray[1] = romData[pokemonStartByte + (i * 28) + 21];
                tmArray[2] = romData[pokemonStartByte + (i * 28) + 22];
                tmArray[3] = romData[pokemonStartByte + (i * 28) + 23];
                tmArray[4] = romData[pokemonStartByte + (i * 28) + 24];
                tmArray[5] = romData[pokemonStartByte + (i * 28) + 25];
                tmArray[6] = romData[pokemonStartByte + (i * 28) + 26];
                pokeToAdd.TMs = GetTMCompatibles(new BitArray(tmArray));
                pokeToAdd.LearnedMoves = new ObservableCollection<LearnedMove>();
                pokeToAdd.Evolutions = new ObservableCollection<Evolution>();
                pokemon.Add(pokeToAdd);
            }

            // Mew
            pokeToAdd = new Pokemon();            
            pokeToAdd.PokedexID = romData[mewStartByte];
            pokeToAdd.HP = romData[mewStartByte + 1];
            pokeToAdd.Attack = romData[mewStartByte + 2];
            pokeToAdd.Defense = romData[mewStartByte + 3];
            pokeToAdd.Speed = romData[mewStartByte + 4];
            pokeToAdd.Special = romData[mewStartByte + 5];
            pokeToAdd.Type1 = (PokeType)romData[mewStartByte + 6];
            pokeToAdd.Type2 = (PokeType)romData[mewStartByte + 7];
            pokeToAdd.CatchRate = romData[mewStartByte + 8];
            pokeToAdd.ExpYield = romData[mewStartByte + 9];
            pokeToAdd.Move1 = romData[mewStartByte + 15];
            pokeToAdd.Move2 = romData[mewStartByte + 16];
            pokeToAdd.Move3 = romData[mewStartByte + 17];
            pokeToAdd.Move4 = romData[mewStartByte + 18];
            pokeToAdd.GrowthRate = (GrowthType)romData[mewStartByte + 19];
            tmArray = new byte[7]; //The next 7 bytes tell us which TMs/HMs the pokemon can learn.
            tmArray[0] = romData[mewStartByte + 20];
            tmArray[1] = romData[mewStartByte + 21];
            tmArray[2] = romData[mewStartByte + 22];
            tmArray[3] = romData[mewStartByte + 23];
            tmArray[4] = romData[mewStartByte + 24];
            tmArray[5] = romData[mewStartByte + 25];
            tmArray[6] = romData[mewStartByte + 26];
            pokeToAdd.TMs = GetTMCompatibles(new BitArray(tmArray));
            pokeToAdd.LearnedMoves = new ObservableCollection<LearnedMove>();
            pokeToAdd.Evolutions = new ObservableCollection<Evolution>();
            pokemon.Add(pokeToAdd);

            for (int i = 0; i < 190; i++) //There are 190 pokemon index IDs. 39 of which are missingno, and 151 are Pokemon.
            {
                //Creating a couple dictionaries to make it easier to reference between pokedex IDs and index IDs.
                PokedexIDs[i + 1] = (romData[pokedexStartByte + i] - 1);
                IndexIDs[romData[pokedexStartByte + i]] = i + 1;

                //Set the Pokemon names, evolution data, and learned moves skipping missingno
                if (romData[pokedexStartByte + i] != 0) //0 = missingno
                {

                    int pokeIndex = romData[pokedexStartByte + i] - 1;
                    int dataRead = romData[currentEvosMovesByte];
                    //Evolutions.
                    while(romData[currentEvosMovesByte] != 0)
                    {
                        evo = new Evolution();
                        evo.Evolve = (EvolveType)romData[currentEvosMovesByte];

                        if (romData[currentEvosMovesByte] == 1) //Level up
                        {
                            evo.EvolveLevel = romData[++currentEvosMovesByte];
                            evo.EvolveTo = romData[++currentEvosMovesByte];
                            currentEvosMovesByte++;
                        }
                        else if (romData[currentEvosMovesByte] == 2) //Stone
                        {
                            evo.EvolutionStone = (EvolveStone)romData[++currentEvosMovesByte];
                            currentEvosMovesByte++; // Stone evolutions have an extra 1 thrown in for some reason. We are just ignoring that.
                            evo.EvolveTo = romData[++currentEvosMovesByte];
                            currentEvosMovesByte++;
                        }
                        else if (romData[currentEvosMovesByte] == 3) //Trade
                        {
                            //pokemon.ElementAt(romData[pokedexStartByte + i] - 1).EvolveLevel = 1; //the 2nd byte for trade evolutions is always 1;
                            currentEvosMovesByte++;
                            evo.EvolveTo = romData[++currentEvosMovesByte];
                            currentEvosMovesByte++;
                        }
                        pokemon.ElementAt(romData[pokedexStartByte + i] - 1).Evolutions.Add(evo);
                    }
                    currentEvosMovesByte++;//Move to the next byte after reading all of the evolution data.

                    //Moves learned while leveling up.
                    while (romData[currentEvosMovesByte] != 0) //0 marks the end of move data
                    {
                        moveToAdd = new LearnedMove();
                        moveToAdd.Level = romData[currentEvosMovesByte++]; //for each move the level learned is the first byte.
                        moveToAdd.MoveID = romData[currentEvosMovesByte++]; //move ID is 2nd byte.
                        pokemon.ElementAt(romData[pokedexStartByte + i] - 1).LearnedMoves.Add(moveToAdd);
                    }
                    currentEvosMovesByte++;


                    pokemonName = "";
                    for (int j = 0; j < 10; j++) //Each name is 10 bytes
                    {
                        // hex 50 is blank. This is different than a space and only used at the end of names that are less than 10 characters.
                        // EF is the male sign. F5 is the female sign. These 2 are only used by the Nidorans
                        if (romData[pokemonNameStartByte + (i * 10) + j] != 0x50)// && romData[pokemonNameStartByte + (i * 10) + j] != 0xEF && romData[pokemonNameStartByte + (i * 10) + j] != 0xF5)
                        {
                            pokemonName += Letters[romData[pokemonNameStartByte + (i * 10) + j]];
                        }
                    }
                    // -1 is because the pokemon collection created above starts at 0 but the pokedex numbers start at 1.
                    // so Bulbasaur's pokedex ID is 1 but he is at the 0 position in the collection.
                    pokemon.ElementAt(romData[pokedexStartByte + i] - 1).Name = pokemonName;
                }
                else
                {
                    currentEvosMovesByte += 2;
                }
            }

            //Here I swap the EvolveTo IDs from Index IDs to Pokedex IDs. The collection of pokemon I return here is in pokedex order 
            // but the evolutions use Index IDs. In order to make the pokemon appear neatly in the evolution combo box 
            // I change the evolutions to use Pokedex IDs. They will need to be changed back during saving.
            foreach (var p in pokemon)
            {
                foreach(var e in p.Evolutions)
                {
                    if (e.Evolve != EvolveType.NONE)
                    {
                        e.EvolveTo = PokedexIDs[e.EvolveTo];
                    }
                }
                
            }

            return pokemon;
        }

        private void SavePokemon(ObservableCollection<Pokemon> pokemons)
        {
            byte[] tmArray;
            int currentEvosMovesByte = pokemonEvosMovesByte;
            int currentPointerByte = pokemonEvosPointersByte;
            for (int i = 0; i < 150; i++) //There are 151 pokemon in the game but Mew is stored separately from the others.
            {
                //basically just write all of the modify-able data back into its location in the ROM
                romData[pokemonStartByte + (i * 28) + 1] = (byte)pokemons.ElementAt(i).HP;
                romData[pokemonStartByte + (i * 28) + 2] = (byte)pokemons.ElementAt(i).Attack;
                romData[pokemonStartByte + (i * 28) + 3] = (byte)pokemons.ElementAt(i).Defense;
                romData[pokemonStartByte + (i * 28) + 4] = (byte)pokemons.ElementAt(i).Speed;
                romData[pokemonStartByte + (i * 28) + 5] = (byte)pokemons.ElementAt(i).Special;
                romData[pokemonStartByte + (i * 28) + 6] = (byte)pokemons.ElementAt(i).Type1;
                romData[pokemonStartByte + (i * 28) + 7] = (byte)pokemons.ElementAt(i).Type2;
                romData[pokemonStartByte + (i * 28) + 8] = (byte)pokemons.ElementAt(i).CatchRate;
                romData[pokemonStartByte + (i * 28) + 9] = (byte)pokemons.ElementAt(i).ExpYield;
                romData[pokemonStartByte + (i * 28) + 15] = (byte)pokemons.ElementAt(i).Move1;
                romData[pokemonStartByte + (i * 28) + 16] = (byte)pokemons.ElementAt(i).Move2;
                romData[pokemonStartByte + (i * 28) + 17] = (byte)pokemons.ElementAt(i).Move3;
                romData[pokemonStartByte + (i * 28) + 18] = (byte)pokemons.ElementAt(i).Move4;
                romData[pokemonStartByte + (i * 28) + 19] = (byte)pokemons.ElementAt(i).GrowthRate;
                tmArray = GetTMArray(pokemons.ElementAt(i).TMs);
                romData[pokemonStartByte + (i * 28) + 20] = tmArray[0];
                romData[pokemonStartByte + (i * 28) + 21] = tmArray[1];
                romData[pokemonStartByte + (i * 28) + 22] = tmArray[2];
                romData[pokemonStartByte + (i * 28) + 23] = tmArray[3];
                romData[pokemonStartByte + (i * 28) + 24] = tmArray[4];
                romData[pokemonStartByte + (i * 28) + 25] = tmArray[5];
                romData[pokemonStartByte + (i * 28) + 26] = tmArray[6];
            }

            // Mew            
            romData[mewStartByte + 1] = (byte)pokemons.ElementAt(150).HP;
            romData[mewStartByte + 2] = (byte)pokemons.ElementAt(150).Attack;
            romData[mewStartByte + 3] = (byte)pokemons.ElementAt(150).Defense;
            romData[mewStartByte + 4] = (byte)pokemons.ElementAt(150).Speed;
            romData[mewStartByte + 5] = (byte)pokemons.ElementAt(150).Special;
            romData[mewStartByte + 6] = (byte)pokemons.ElementAt(150).Type1;
            romData[mewStartByte + 7] = (byte)pokemons.ElementAt(150).Type2;
            romData[mewStartByte + 8] = (byte)pokemons.ElementAt(150).CatchRate;
            romData[mewStartByte + 9] = (byte)pokemons.ElementAt(150).ExpYield;
            romData[mewStartByte + 15] = (byte)pokemons.ElementAt(150).Move1;
            romData[mewStartByte + 16] = (byte)pokemons.ElementAt(150).Move2;
            romData[mewStartByte + 17] = (byte)pokemons.ElementAt(150).Move3;
            romData[mewStartByte + 18] = (byte)pokemons.ElementAt(150).Move4;
            romData[mewStartByte + 19] = (byte)pokemons.ElementAt(150).GrowthRate;
            tmArray = GetTMArray(pokemons.ElementAt(150).TMs);
            romData[mewStartByte + 20] = tmArray[0];
            romData[mewStartByte + 21] = tmArray[1];
            romData[mewStartByte + 22] = tmArray[2];
            romData[mewStartByte + 23] = tmArray[3];
            romData[mewStartByte + 24] = tmArray[4];
            romData[mewStartByte + 25] = tmArray[5];
            romData[mewStartByte + 26] = tmArray[6];

            for(int i = 0; i < 190; i++)
            {
                //update pointers so the game knows where to find the updated pokemon evos/learned moves.
                // The data in the pointers is how many bytes to move past byte 0x34000 in order to find the start of that pokemon data.
                // 2 bytes per pokemon.
                int firstPointerByte;
                int secondPointerByte = (currentEvosMovesByte - pointerBase) / 256;
                firstPointerByte = (currentEvosMovesByte - pointerBase) - (secondPointerByte * 256);
                romData[currentPointerByte++] = (byte)firstPointerByte;
                romData[currentPointerByte++] = (byte)secondPointerByte;

                if (PokedexIDs[i+1] != -1) //lookup the pokedex ID so we can skip missingnos
                {
                    //evolutions
                    int indexfound = romData[pokedexStartByte + i] - 1;
                    foreach (var e in pokemons.ElementAt(romData[pokedexStartByte + i] - 1).Evolutions)
                    {
                        if (e.Evolve == EvolveType.LEVEL)
                        {
                            romData[currentEvosMovesByte++] = 1;
                            romData[currentEvosMovesByte++] = (byte)e.EvolveLevel;
                            romData[currentEvosMovesByte++] = (byte)IndexIDs[e.EvolveTo +1];
                        }
                        else if (e.Evolve == EvolveType.STONE)
                        {
                            romData[currentEvosMovesByte++] = 2;
                            romData[currentEvosMovesByte++] = (byte)e.EvolutionStone;
                            romData[currentEvosMovesByte++] = 1;
                            romData[currentEvosMovesByte++] = (byte)IndexIDs[e.EvolveTo +1];
                        }
                        else if (e.Evolve == EvolveType.TRADE)
                        {
                            romData[currentEvosMovesByte++] = 3;
                            romData[currentEvosMovesByte++] = 1;
                            romData[currentEvosMovesByte++] = (byte)IndexIDs[e.EvolveTo +1];
                        }
                    }
                    romData[currentEvosMovesByte++] = 0;

                    foreach(var m in pokemons.ElementAt(romData[pokedexStartByte + i] - 1).LearnedMoves)
                    {
                        romData[currentEvosMovesByte++] = (byte)m.Level;
                        romData[currentEvosMovesByte++] = (byte)m.MoveID;
                    }
                    romData[currentEvosMovesByte++] = 0;
                }
                else
                {
                    //write 2 zeroes for missingnos. 1 for no evolution, 1 for no moves
                    romData[currentEvosMovesByte++] = 0;
                    romData[currentEvosMovesByte++] = 0;
                }
            }
        }

        public ObservableCollection<TypeStrength> LoadTypeStrengths()
        {
            var typeStrengths = new ObservableCollection<TypeStrength>();
            
            TypeStrength typeStrengthToAdd;
            for (int i = 0; i < 82; i++) //There are 82 type strengths. Each is 3 bytes and the group is terminated by the byte FF.
            {
                if(romData[typeChartByte + (i * 3)] != 0xFF)
                {
                    typeStrengthToAdd = new TypeStrength();
                    typeStrengthToAdd.AttackType = (PokeType)romData[typeChartByte + (i * 3)]; //first byte is the attacking type
                    typeStrengthToAdd.DefenseType = (PokeType)romData[typeChartByte + (i * 3) + 1]; //second byte is the defending type
                    typeStrengthToAdd.Effectiveness = (DamageModifier)romData[typeChartByte + (i * 3) + 2]; //third byte is effectiveness X 10. So double damage = 20, half damage = 5.
                    typeStrengths.Add(typeStrengthToAdd);
                }
                else
                {
                    break;
                }
            }

            return typeStrengths;
        }

        private void SaveTypeStrengths(ObservableCollection<TypeStrength> str)
        {
            int currentByte = typeChartByte;
            
            foreach(var ts in str)
            {
                romData[currentByte++] = (byte)ts.AttackType;
                romData[currentByte++] = (byte)ts.DefenseType;
                romData[currentByte++] = (byte)ts.Effectiveness;
            }
            romData[currentByte++] = 0xFF; //writing the ending byte manually in case they removed some type strengths.
        }

        public ObservableCollection<TM> LoadTMs()
        {
            var tms = new ObservableCollection<TM>();
            
            TM newTM;

            for (int i = 0; i < 55; i++) //There are 50 TMs and 5 HMs. Each is 1 byte which is the moveID
            {
                newTM = new TM();
                newTM.TMNumber = i;
                newTM.MoveID = romData[tmStartByte + i];
                if (i < 50)
                {
                    newTM.TMName = $"TM{i + 1}";
                }
                else
                {
                    newTM.TMName = $"HM{i - 49}";
                }
                tms.Add(newTM);
            }
            //The prices for TMs are stored separately from the other items. There are 25 bytes for the 50 TMs. Each TM is 1 nibble. The value is in thousands.
            // for example: TM1 is 3000 and TM2 is 2000. The first byte is 0x32.
            for (int i = 0; i < 25; i++) 
            {
                tms.ElementAt(i * 2).Price = romData[tmPricesStartByte + i] / 16;
                tms.ElementAt(i * 2 + 1).Price = romData[tmPricesStartByte + i] % 16;
            }

            return tms;
        }

        private void SaveTMs(ObservableCollection<TM> tms)
        {
            for (int i = 0; i < 55; i++) //There are 50 TMs and 5 HMs. Each is 1 byte which is the moveID
            {
                romData[tmStartByte + i] = (byte)tms.ElementAt(i).MoveID;
            }
            for(int i = 0; i < 25; i++)
            {
                int price = tms.ElementAt(i * 2).Price * 16 + tms.ElementAt(i * 2 + 1).Price;
                romData[tmPricesStartByte + i] = (byte)price;
            }
        }

        public ObservableCollection<WildEncounterZone> LoadWildEncounters()
        {
            var encounters = new ObservableCollection<WildEncounterZone>();
            int currentByte = wildEncountersByte;
            WildEncounterZone newZone;

            while (currentByte < wildEncountersEndByte)
            {
                if(encounters.Count == 36) // There's an extra byte at the beginning of this data.
                {
                    currentByte++;
                }                
                if(romData[currentByte] != 0) //The zones that dont have encounters are marked with 0 for the first byte.
                {
                    newZone = new WildEncounterZone();
                    newZone.EncounterRate = romData[currentByte];
                    currentByte++;
                    newZone.ZoneName = ZoneNames[encounters.Count];
                    // If the zone has encounters it has 10 slots, each with 2 bytes. The slot determines the chance of the pokemon appearing in a random encounter
                    // The first byte is the pokemon's level and the 2nd is the index ID. We are converting to pokedex IDs for display purposes.
                    newZone.Enc1 = new EnemyPokemon(romData[currentByte++], PokedexIDs[romData[currentByte++]]);
                    newZone.Enc2 = new EnemyPokemon(romData[currentByte++], PokedexIDs[romData[currentByte++]]);
                    newZone.Enc3 = new EnemyPokemon(romData[currentByte++], PokedexIDs[romData[currentByte++]]);
                    newZone.Enc4 = new EnemyPokemon(romData[currentByte++], PokedexIDs[romData[currentByte++]]);
                    newZone.Enc5 = new EnemyPokemon(romData[currentByte++], PokedexIDs[romData[currentByte++]]);
                    newZone.Enc6 = new EnemyPokemon(romData[currentByte++], PokedexIDs[romData[currentByte++]]);
                    newZone.Enc7 = new EnemyPokemon(romData[currentByte++], PokedexIDs[romData[currentByte++]]);
                    newZone.Enc8 = new EnemyPokemon(romData[currentByte++], PokedexIDs[romData[currentByte++]]);
                    newZone.Enc9 = new EnemyPokemon(romData[currentByte++], PokedexIDs[romData[currentByte++]]);
                    newZone.Enc10 = new EnemyPokemon(romData[currentByte++], PokedexIDs[romData[currentByte++]]);
                    if (encounters.Count != 36 && encounters.Count != 46 && encounters.Count != 47) //There's no ending byte at the end of these 3 zones.
                    {
                        currentByte++;
                    }
                    encounters.Add(newZone);
                }
                else // Zones with no encounters have only 2 bytes
                {
                    currentByte += 2;
                }
            }
            return encounters;
        }

        private void SaveWildEncounters(ObservableCollection<WildEncounterZone> zones)
        {
            int currentByte = wildEncountersByte;
            for (int i = 0; i < 57; i++)
            {
                if(i == 36) //There's an extra byte at the beginning of this zone
                {
                    romData[currentByte++] = 0;
                }
                romData[currentByte++] = (byte)zones.ElementAt(i).EncounterRate;
                romData[currentByte++] = (byte)zones.ElementAt(i).Enc1.Level;
                romData[currentByte++] = (byte)IndexIDs[zones.ElementAt(i).Enc1.PokedexID +1];
                romData[currentByte++] = (byte)zones.ElementAt(i).Enc2.Level;
                romData[currentByte++] = (byte)IndexIDs[zones.ElementAt(i).Enc2.PokedexID +1];
                romData[currentByte++] = (byte)zones.ElementAt(i).Enc3.Level;
                romData[currentByte++] = (byte)IndexIDs[zones.ElementAt(i).Enc3.PokedexID +1];
                romData[currentByte++] = (byte)zones.ElementAt(i).Enc4.Level;
                romData[currentByte++] = (byte)IndexIDs[zones.ElementAt(i).Enc4.PokedexID +1];
                romData[currentByte++] = (byte)zones.ElementAt(i).Enc5.Level;
                romData[currentByte++] = (byte)IndexIDs[zones.ElementAt(i).Enc5.PokedexID +1];
                romData[currentByte++] = (byte)zones.ElementAt(i).Enc6.Level;
                romData[currentByte++] = (byte)IndexIDs[zones.ElementAt(i).Enc6.PokedexID +1];
                romData[currentByte++] = (byte)zones.ElementAt(i).Enc7.Level;
                romData[currentByte++] = (byte)IndexIDs[zones.ElementAt(i).Enc7.PokedexID +1];
                romData[currentByte++] = (byte)zones.ElementAt(i).Enc8.Level;
                romData[currentByte++] = (byte)IndexIDs[zones.ElementAt(i).Enc8.PokedexID +1];
                romData[currentByte++] = (byte)zones.ElementAt(i).Enc9.Level;
                romData[currentByte++] = (byte)IndexIDs[zones.ElementAt(i).Enc9.PokedexID +1];
                romData[currentByte++] = (byte)zones.ElementAt(i).Enc10.Level;
                romData[currentByte++] = (byte)IndexIDs[zones.ElementAt(i).Enc10.PokedexID +1];
                if(i != 36 && i != 46 && i != 47) //there's no ending byte for these 3
                {
                    romData[currentByte++] = 0;
                }
                if(i == 20) //These 4 bytes are the first 2 floors of the pokemon tower. The data for them comes after Route 7.
                {
                    romData[currentByte++] = 0;
                    romData[currentByte++] = 0;
                    romData[currentByte++] = 0;
                    romData[currentByte++] = 0;
                }
            }
        }

        public ObservableCollection<Trainer> LoadTrainers()
        {
            var trainers = new ObservableCollection<Trainer>();
            int currentByte = trainerStartByte;
            int trainerGroupTracker = 0; // tells which name to pull from the dictionary
            int numOfTrainersInGroup = 0;
            //string trainerName = "";
            Trainer trainerToAdd;

            while (currentByte < trainerEndByte)
            {
                trainerToAdd = new Trainer();
                trainerToAdd.Pokemons = new ObservableCollection<EnemyPokemon>();
                if(romData[currentByte] == 255) // trainers who have pokemon of different levels are marked with 0xFF as the 1st byte
                {
                    trainerToAdd.AllSameLevel = false;
                    currentByte++;
                    trainerToAdd.PartyLevel = romData[currentByte];
                    while(romData[currentByte] != 0)
                    {
                        trainerToAdd.Pokemons.Add(new EnemyPokemon(romData[currentByte++], PokedexIDs[romData[currentByte++]]));
                    }
                }
                else // if the first byte isn't 0xFF then it is the level for all of the pokemon in the team.
                {
                    trainerToAdd.AllSameLevel = true;
                    trainerToAdd.PartyLevel = romData[currentByte++];
                    while(romData[currentByte] != 0) //list of pokemon ending with 0.
                    {
                        trainerToAdd.Pokemons.Add(new EnemyPokemon(trainerToAdd.PartyLevel, PokedexIDs[romData[currentByte++]]));
                    }
                }
                currentByte++;

                //trainerToAdd.TrainerName = TrainerNames[trainerNameTracker];
                //trainerName = $"{TrainerNames[trainerNameTracker]} {numOfTrainersWithName +1}";
                //if (unusedTrainers.Contains(trainers.Count))
                //{
                //    trainerName += " (unused)";
                //}
                //trainerToAdd.TrainerName = trainerName;

                trainerToAdd.GroupNum = trainerGroupTracker + 201;
                trainerToAdd.TrainerNum = numOfTrainersInGroup + 1;

                numOfTrainersInGroup++;
                while(numOfTrainersInGroup == trainerCounts[trainerGroupTracker] && trainerGroupTracker < 46)
                {                    
                    trainerGroupTracker++;
                    numOfTrainersInGroup = 0;                    
                }
                trainers.Add(trainerToAdd);
            }

            return trainers;
        }

        private void SaveTrainers(ObservableCollection<Trainer> trainers)
        {
            int currentByte = trainerStartByte;
            int currentTrainerGroup = 0;
            int numOfTrainers = 0;
            int currentPointerByte = trainerPointersByte;
            int firstPointerByte;
            int secondPointerByte;

            foreach (var t in trainers)
            {
                if(numOfTrainers == 0)
                {
                    //update pointers so the game knows where to find the updated trainer groups.
                    // The data in the pointers is how many bytes to move past byte 0x34000 in order to find the start of that trainer group.
                    // 2 bytes per trainer.
                    secondPointerByte = (currentByte - pointerBase) / 256;
                    firstPointerByte = (currentByte - pointerBase) - (secondPointerByte * 256);
                    romData[currentPointerByte++] = (byte)firstPointerByte;
                    romData[currentPointerByte++] = (byte)secondPointerByte;

                    //There are 2 trainer groups that aren't used. they have the same pointers as the groups that come after them so I'm just writing those groups twice.
                    if (currentTrainerGroup == 12 || currentTrainerGroup == 25) 
                    {
                        romData[currentPointerByte++] = (byte)firstPointerByte;
                        romData[currentPointerByte++] = (byte)secondPointerByte;
                    }
                }

                if (t.AllSameLevel)
                {
                    romData[currentByte++] = (byte)t.PartyLevel;
                    foreach(var p in t.Pokemons)
                    {
                        romData[currentByte++] = (byte)IndexIDs[p.PokedexID + 1];
                    }
                    romData[currentByte++] = 0;
                }
                else
                {
                    romData[currentByte++] = 0xFF;
                    foreach (var p in t.Pokemons)
                    {
                        romData[currentByte++] = (byte)p.Level;
                        romData[currentByte++] = (byte)IndexIDs[p.PokedexID + 1];
                    }
                    romData[currentByte++] = 0;
                }

                numOfTrainers++;
                if(numOfTrainers == trainerCounts[currentTrainerGroup])
                {
                    currentTrainerGroup++;
                    numOfTrainers = 0;
                }
            }

        }

        public ObservableCollection<TrainerGroup> LoadTrainerGroups()
        {
            ObservableCollection<TrainerGroup> groups = new ObservableCollection<TrainerGroup>();
            int trainerNum = 201;
            int currentByte = trainerNamesByte;
            TrainerGroup newGroup;
            string groupName;
            int currentLetter = 0;

            for(int i = 0; i < 47; i++) // There are 47 trainer groups
            {
                groupName = "";
                while(romData[currentByte] != 0x50)
                {
                    currentLetter = romData[currentByte];
                    groupName += Letters[romData[currentByte++]];
                }
                currentByte++;

                newGroup = new TrainerGroup(groupName, trainerNum + i);
                groups.Add(newGroup);
            }

            return groups;
        }

        public ObservableCollection<Shop> LoadShops()
        {
            var shops = new ObservableCollection<Shop>();
            Shop newShop;
            int currentByte = shopsStartByte;

            for (int i = 0; i < 16; i++)
            {
                newShop = new Shop();
                newShop.ShopName = ShopNames[i];
                newShop.Items = new ObservableCollection<Item>();
                currentByte += 2; //Skip the first 2 bytes. The first byte is always 0xFE and the 2nd is the number of items for sale.
                while(romData[currentByte] != 0xFF) //the end of the shop is marked by 0xFF
                {
                    newShop.Items.Add(new Item((ItemType)romData[currentByte++]));
                }
                shops.Add(newShop);
                currentByte++;
            }

            return shops;
        }

        private void SaveShops(ObservableCollection<Shop> shops)
        {
            int firstPointerByte;
            int secondPointerByte;

            int currentByte = shopsStartByte;
            for (int i = 0; i < 16; i++)
            {                
                //first update pointers to the new location of the shop's data.
                //the game stores these pointers along with the pointers to the text data.
                if(shopPointerBytes[i] != 0x00) //there are 2 pointers that aren't used. We don't need to update them
                {                    
                    secondPointerByte = (currentByte) / 256;
                    firstPointerByte = (currentByte) - (secondPointerByte * 256);
                    romData[shopPointerBytes[i]] = (byte)firstPointerByte;
                    romData[shopPointerBytes[i]+1] = (byte)secondPointerByte;
                }

                //next update the shop data.
                //The first byte is always 0xFE and the 2nd is the number of items for sale.
                romData[currentByte++] = 0xFE;
                romData[currentByte++] = (byte)shops.ElementAt(i).Items.Count;

                foreach (var item in shops.ElementAt(i).Items)
                {                    
                    romData[currentByte++] = (byte)item.ItemID;
                }
                romData[currentByte++] = 0xFF; //mark end of shop
            }
        }

        public ObservableCollection<Item> LoadItems()
        {
            ObservableCollection<Item> items = new ObservableCollection<Item>();
            Item itemToAdd;

            for(int i = 0; i < 83; i++) // there are 97 items but the last 14 are floors?
            {
                itemToAdd = new Item((ItemType)i+1, GetDecPrice(itemPricesStartByte + (i * 3)));
                items.Add(itemToAdd);
            }

            return items;
        }

        private void SaveItems(ObservableCollection<Item> items)
        {
            int currentByte = itemPricesStartByte;
            int firstPriceByte = 0;
            int secondPriceByte = 0;
            int thirdPriceByte = 0;
            int price = 0;

            foreach( var i in items)
            {
                price = i.Price;
                firstPriceByte = DecToHex(price / 10000);
                price -= (HexToDec(firstPriceByte) * 10000);
                secondPriceByte = DecToHex(price / 100);
                price -= (HexToDec(secondPriceByte) * 100);
                thirdPriceByte = DecToHex(price);

                romData[currentByte++] = (byte)firstPriceByte;
                romData[currentByte++] = (byte)secondPriceByte;
                romData[currentByte++] = (byte)thirdPriceByte;
            }
        }

        public ObservableCollection<BlockSet> LoadBlockSets()
        {
            var blockSets = new ObservableCollection<BlockSet>();
            BlockSet bs;
            int currentHeaderByte;
            int currentBank;
            int currentBlocksetByte;
            int blocksetPointer1;
            int blocksetPointer2;

            for(int i = 0; i < blocksetSizes.Count(); i++)
            {
                currentHeaderByte = tilesetHeadersByte + (i * 12); // each tileset header is 12 bytes
                currentBank = (romData[currentHeaderByte] - 1) * 0x4000; //the first byte is the map. each map is 0x4000 bytes. We subtract 1 because the first bank starts at 0.
                blocksetPointer1 = romData[currentHeaderByte + 1]; // the pointer to the block data is stored in 2 bytes, smaller byte first
                blocksetPointer2 = romData[currentHeaderByte + 2] * 256;
                currentBlocksetByte = currentBank + blocksetPointer2 + blocksetPointer1; // add the bank byte to the pointer bytes and we get the start of the blocks

                bs = new BlockSet();
                bs.Tiles = new ObservableCollection<System.Drawing.Bitmap>();
                bs.SourceFile = imageFolder + blocksetPngNames[i]; //this is the name of the png file that is used to create the block images.
                bs.BlockDefinitions = new int[blocksetSizes[i]]; //blocksetSizes is a list of how many bytes are used for each blockset
                for(int j = 0; j < blocksetSizes[i]; j++)
                {
                    //from the starting point we calculated above we copy each byte
                    bs.BlockDefinitions[j] = romData[currentBlocksetByte + j];
                }
                blockSets.Add(bs);
                
            }           

            return blockSets;
        }

        public ObservableCollection<MapObjectSprite> LoadSprites()
        {
            ObservableCollection<MapObjectSprite> sprites = new ObservableCollection<MapObjectSprite>();
            
            for(int i = 0; i < spritePngNames.Count(); i++)
            {
                MapObjectSprite newSprite = new MapObjectSprite();
                newSprite.SpriteName = spritePngNames[i];
                newSprite.FileName = imageFolder + spritePngNames[i] + ".png";
                sprites.Add(newSprite);
            }

            return sprites;
        }

        public ObservableCollection<Map> LoadMaps()
        {
            var maps = new ObservableCollection<Map>();
            Map newMap;
            int currentHeaderByte;
            int currentBank;
            int headerPointer1;
            int headerPointer2;
            int currentBlocksByte;
            int blocksPointer1;
            int blocksPointer2;
            int numOfConnections;
            int objectsPointer1;
            int objectsPointer2;
            int currentObjectsByte;
            MapObject newMapObject;
            int numOfObjects;
            int numOfExtraBytes;

            for (int i = 0; i < 248; i++) //There are somehow 248 maps in the game.
            {
                // need to calculate the location of the map header.
                currentBank = (romData[mapBanksByte + i] - 1) * 0x4000; //this bank value is used for the header location as well as the location of the map's blocks
                headerPointer1 = romData[mapHeaderPointersByte + (i * 2)]; //again the pointer is stored as 2 bytes, smaller first.
                headerPointer2 = romData[mapHeaderPointersByte + (i * 2) + 1] * 256;
                currentHeaderByte = currentBank + headerPointer1 + headerPointer2;

                if(romData[currentHeaderByte] < 24 && !(unusedMaps.Contains(i))) // Make sure we are reading from good maps. several maps have bad data.
                {
                    // read some info about the map from the header
                    newMap = new Map($"map {i}", (TileSet)romData[currentHeaderByte]);
                    newMap.Height = romData[currentHeaderByte + 1];
                    newMap.Width = romData[currentHeaderByte + 2];
                    newMap.MapBlockValues = new int[newMap.Height * newMap.Width];

                    // uses the header info to get the location of the map's blocks.
                    blocksPointer1 = romData[currentHeaderByte + 3];
                    blocksPointer2 = romData[currentHeaderByte + 4] * 256;
                    currentBlocksByte = currentBank + blocksPointer1 + blocksPointer2;

                    // read the map's blocks
                    for (int m = 0; m < newMap.Height * newMap.Width; m++)
                    {
                        newMap.MapBlockValues[m] = romData[currentBlocksByte + m];
                    }

                    numOfConnections = 0;
                    // find number of connections
                    if ((romData[currentHeaderByte + 9] & 1) != 0)
                    {
                        numOfConnections++;
                    }
                    if ((romData[currentHeaderByte + 9] & 2) != 0)
                    {
                        numOfConnections++;
                    }
                    if ((romData[currentHeaderByte + 9] & 4) != 0)
                    {
                        numOfConnections++;
                    }
                    if ((romData[currentHeaderByte + 9] & 8) != 0)
                    {
                        numOfConnections++;
                    }
                    //each connection is 11 bytes. There can be 0 to 4 connections in a map. The pointer to the object data is after the connections.
                    objectsPointer1 = romData[currentHeaderByte + 10 + (numOfConnections * 11)];
                    objectsPointer2 = romData[currentHeaderByte + 11 + (numOfConnections * 11)] * 256;
                    currentObjectsByte = currentBank + objectsPointer1 + objectsPointer2;

                    currentObjectsByte++; // the first byte is the border block. We are skipping that for now.
                    numOfExtraBytes = romData[currentObjectsByte] * 4 + 1;
                    currentObjectsByte += numOfExtraBytes; // warps data. Skipping for now
                    numOfExtraBytes = romData[currentObjectsByte] * 3 + 1;
                    currentObjectsByte += numOfExtraBytes; // Signs data. Skipping for now

                    newMap.MapObjects = new ObservableCollection<MapObject>();
                    numOfObjects = romData[currentObjectsByte++];

                    for (int objs = 0; objs < numOfObjects; objs++)
                    {
                        newMapObject = new MapObject();
                        newMapObject.SpriteID = romData[currentObjectsByte++] - 1; // -1 because it will be referencing the index of a collection that is 0 based where the sprites in the game are 1 based.
                        newMapObject.YPosition = romData[currentObjectsByte++] - 4;
                        newMapObject.XPosition = romData[currentObjectsByte++] - 4;
                        newMapObject.Movement = (MapObjectMovementType)romData[currentObjectsByte++];
                        newMapObject.Facing = (MapObjectFacing)romData[currentObjectsByte++];

                        //the next byte is the textstringID. This is how you determine what type of object you are dealing with.
                        if (romData[currentObjectsByte] > 0x80) // item
                        {
                            newMapObject.ObjectType = MapObjectType.Item;
                            currentObjectsByte++; //move past the textstringID
                            newMapObject.Item = (ItemType)romData[currentObjectsByte++];
                        }
                        else if(romData[currentObjectsByte] > 0x40) // trainer or pokemon
                        {
                            currentObjectsByte++; //move past the textstringID
                            if(romData[currentObjectsByte] > 200) //ID over 200 means trainer ID
                            {
                                newMapObject.ObjectType = MapObjectType.Trainer;
                                newMapObject.TrainerGroupNum = romData[currentObjectsByte++] - 201;
                                newMapObject.TrainerNum = romData[currentObjectsByte++];
                            }
                            else // ID under 200 means pokedex ID
                            {
                                newMapObject.ObjectType = MapObjectType.Pokemon;
                                int foundID = romData[currentObjectsByte++];
                                newMapObject.PokemonObj.PokedexID = PokedexIDs[foundID]; //the pokemon are stored by indexID. Switch it here to pokedexID
                                //newMapObject.PokemonObj.PokedexID = romData[currentObjectsByte++];
                                newMapObject.PokemonObj.Level = romData[currentObjectsByte++];
                            }
                        }
                        else
                        {
                            newMapObject.ObjectType = MapObjectType.Person;
                            currentObjectsByte++; //move past the textstringID
                        }
                        newMap.MapObjects.Add(newMapObject);
                    }

                    maps.Add(newMap);
                }
                
            }


            return maps;
        }

        private void SaveMaps(ObservableCollection<Map> maps)
        {
            int currentHeaderByte;
            int currentBank;
            int headerPointer1;
            int headerPointer2;
            int currentBlocksByte;
            int blocksPointer1;
            int blocksPointer2;
            int currentMap = 0; //using this to keep track of which map index to use since we skipped loading 11 of the 247 maps.

            for (int i = 0; i < 248; i++) //There are somehow 248 maps in the game.
            {
                // need to calculate the location of the map header.
                currentBank = (romData[mapBanksByte + i] - 1) * 0x4000; //this bank value is used for the header location as well as the location of the map's blocks
                headerPointer1 = romData[mapHeaderPointersByte + (i * 2)]; //again the pointer is stored as 2 bytes, smaller first.
                headerPointer2 = romData[mapHeaderPointersByte + (i * 2) + 1] * 256;
                currentHeaderByte = currentBank + headerPointer1 + headerPointer2;

                if (romData[currentHeaderByte] < 24 && !(unusedMaps.Contains(i))) // Make sure we are reading from good maps. several maps have bad data.
                {
                    // uses the header info to get the location of the map's blocks.
                    blocksPointer1 = romData[currentHeaderByte + 3];
                    blocksPointer2 = romData[currentHeaderByte + 4] * 256;
                    currentBlocksByte = currentBank + blocksPointer1 + blocksPointer2;

                    // save the map's blocks
                    for (int m = 0; m < maps.ElementAt(currentMap).MapBlockValues.Count(); m++)
                    {
                        romData[currentBlocksByte + m] = (byte)maps.ElementAt(currentMap).MapBlockValues[m];
                    }
                    currentMap++;
                }

            }
        }

        public int GetMaxMoveBytes()
        {
            //2068 bytes total. Take out the 39 missingnos at 2 bytes each for total left.
            return 1990;
        }

        public int GetMaxTrainerBytes()
        {

            return 1941;
        }

        public int GetMaxShopItems()
        {
            return 100;
        }

        private ObservableCollection<TMCompatible> GetTMCompatibles(BitArray tmBits)
        {
            var tms = new ObservableCollection<TMCompatible>();
            TMCompatible newTM;
            for (int i = 0; i < 55; i++)
            {
                newTM = new TMCompatible();
                newTM.TMNumber = i;
                newTM.CanLearn = tmBits.Get(i);
                tms.Add(newTM);
            }
            return tms;
        }

        private byte[] GetTMArray(ObservableCollection<TMCompatible> tms)
        {
            byte[] tmArray = new byte[7];
            BitArray tmBits = new BitArray(56);
            int counter = 0;
            foreach(var tm in tms)
            {
                tmBits.Set(counter, tm.CanLearn);
                counter++;
            }
            tmBits.CopyTo(tmArray, 0);
            return tmArray;
        }

        private int GetDecPrice(int currentByte)
        {
            int decPrice = 0;

            decPrice += (HexToDec(romData[currentByte]) * 10000);
            decPrice += (HexToDec(romData[currentByte+1]) * 100);
            decPrice += (HexToDec(romData[currentByte+2]) * 1);

            return decPrice;
        }        

        private int DecToHex(int decNum)
        {
            int hexsNum = 0;
            int onesNum = 0;

            hexsNum = decNum / 10;
            onesNum = decNum % 10;

            return (hexsNum * 16) + onesNum;
        }

        private int HexToDec(int hexNum)
        {
            int tensNum = 0;
            int onesNum = 0;

            tensNum = hexNum / 16;
            onesNum = hexNum % 16;

            return (tensNum * 10) + onesNum;
        }

        //This is for finding a letter based on the given byte (loading from the ROM)
        Dictionary<int, string> Letters = new Dictionary<int, string>
        {
            { 127, " " },
            { 128, "A"},
            { 129, "B" },
            { 130, "C" },
            { 131, "D" },
            { 132, "E" },
            { 133, "F" },
            { 134, "G" },
            { 135, "H" },
            { 136, "I" },
            { 137, "J" },
            { 138, "K" },
            { 139, "L" },
            { 140, "M"},
            { 141, "N" },
            { 142, "O" },
            { 143, "P" },
            { 144, "Q" },
            { 145, "R" },
            { 146, "S" },
            { 147, "T" },
            { 148, "U" },
            { 149, "V" },
            { 150, "W" },
            { 151, "X" },
            { 152, "Y"},
            { 153, "Z" },
            { 186, "E" },
            { 227, "-" },
            { 232, "." },
            { 224, "'" },
            { 239, "M" },
            { 245, "F" },
            { 246, "0" },
            { 247, "1" },
            { 248, "2" },
            { 249, "3" },
            { 250, "4" },
            { 251, "5" },
            { 252, "6" },
            { 253, "7" },
            { 254, "8" },
            { 255, "9" }
        };

        //This is for when you need the byte value for a letter (saving to the ROM)
        Dictionary<string, int> LetterValues = new Dictionary<string, int>
        {
            { " ", 127 },
            { "A", 128 },
            { "B", 129 },
            { "C", 130 },
            { "D", 131 },
            { "E", 132 },
            { "F", 133 },
            { "G", 134 },
            { "H", 135 },
            { "I", 136 },
            { "J", 137 },
            { "K", 138 },
            { "L", 139 },
            { "M", 140 },
            { "N", 141 },
            { "O", 142 },
            { "P", 143 },
            { "Q", 144 },
            { "R", 145 },
            { "S", 146 },
            { "T", 147 },
            { "U", 148 },
            { "V", 149 },
            { "W", 150 },
            { "X", 151 },
            { "Y", 152 },
            { "Z", 153 },
            { "-", 227 },
            { ".", 232 },
            { "'", 224 }
        };

        Dictionary<int, string> ZoneNames = new Dictionary<int, string>
        {
            {0, "Route 1" },
            {1, "Route 2" },
            {2, "Route 22" },
            {3, "Viridian Forest" },
            {4, "Route 3" },
            {5, "MT. Moon 1" },
            {6, "MT. Moon B1" },
            {7, "MT. Moon B2" },
            {8, "Route 4" },
            {9, "Route 24" },
            {10, "Route 25" },
            {11, "Route 9" },
            {12, "Route 5" },
            {13, "Route 6" },
            {14, "Route 11" },
            {15, "Rock Tunnel 1" },
            {16, "Rock Tunnel 2" },
            {17, "Route 10" },
            {18, "Route 12" },
            {19, "Route 8" },
            {20, "Route 7" },            
            {21, "Pokemon Tower 3" },
            {22, "Pokemon Tower 4" },
            {23, "Pokemon Tower 5" },
            {24, "Pokemon Tower 6" },
            {25, "Pokemon Tower 7" },
            {26, "Route 13" },
            {27, "Route 14" },
            {28, "Route 15" },
            {29, "Route 16" },
            {30, "Route 17" },
            {31, "Route 18" },
            {32, "Safari Zone Center" },
            {33, "Safari Zone 1" },
            {34, "Safari Zone 2" },
            {35, "Safari Zone 3" },
            {36, "Water Pokemon" },
            {37, "Seafoam Island 1" },
            {38, "Seafoam Island B1" },
            {39, "Seafoam Island B2" },
            {40, "Seafoam Island B3" },
            {41, "Seafoam Island B4" },
            {42, "Mansion 1" },
            {43, "Mansion 2" },
            {44, "Mansion 3" },
            {45, "Mansion B1" },
            {46, "Route 21 Grass" },
            {47, "Route 21 Surf" },
            {48, "Unknown Dungeon 1" },
            {49, "Unknown Dungeon 2" },
            {50, "Unknown Dungeon B1" },
            {51, "Power Plant" },
            {52, "Route 23" },
            {53, "Victory Road 2" },
            {54, "Victory Road 3" },
            {55, "Victory Road 1" },
            {56, "Digletts Cave" }
        };

        Dictionary<int, string> TrainerNames = new Dictionary<int, string>
        {
            {0, "Youngster" },
            {1, "Bug Catcher" },
            {2, "Lass" },
            {3, "Sailor" },
            {4, "Jr Trainer M" },
            {5, "Jr Trainer F" },
            {6, "Pokemaniac" },
            {7, "Super Nerd" },
            {8, "Hiker" },
            {9, "Biker" },
            {10, "Burglar" },
            {11, "Engineer" },
            {12, "Fisher" },
            {13, "Swimmer" },
            {14, "Cue Ball" },
            {15, "Gambler" },
            {16, "Beauty" },
            {17, "Psychic" },
            {18, "Rocker" },
            {19, "Juggler" },
            {20, "Tamer" },
            {21, "Bird Keeper" },
            {22, "Blackbelt" },
            {23, "Rival" },
            {24, "Prof Oak" },
            {25, "Scientist" },
            {26, "Giovanni" },
            {27, "Rocket" },
            {28, "Cool Trainer M" },
            {29, "Cool Trainer F" },
            {30, "Bruno" },
            {31, "Brock" },
            {32, "Misty" },
            {33, "Lt. Surge" },
            {34, "Erika" },
            {35, "Koga" },
            {36, "Blaine" },
            {37, "Sabrina" },
            {38, "Gentleman" },
            {39, "Rival" },
            {40, "Rival" },
            {41, "Lorelei" },
            {42, "Channeler" },
            {43, "Agatha" },
            {44, "Lance" }
        };

        Dictionary<int, string> ShopNames = new Dictionary<int, string>
        {
            {0, "Viridian City" },
            {1, "Pewter City" },
            {2, "Cerulean City" },
            {3, "Bike Shop" },
            {4, "Vermilion City" },
            {5, "Lavender City" },
            {6, "Celadon Floor 2 Clerk 1" },
            {7, "Celadon Floor 2 Clerk 2" },
            {8, "Celadon Floor 4" },
            {9, "Celadon Floor 5 Clerk 1" },
            {10, "Celadon Floor 5 Clerk 2" },
            {11, "Fushia City" },
            {12, "unused" },
            {13, "Cinnabar Island" },
            {14, "Saffron City" },
            {15, "Indigo Plateau" }
        };

        int[] trainerCounts = { 13, 14, 18, 8, 9, 24, 7, 12, 14, 15, 9, 3, 0, 11, 15, 9, 7, 15, 4, 2, 8, 6, 17, 9, 9, 3, 0, 13, 3, 41, 10, 8, 1, 1, 1, 1, 1, 1, 1, 1, 5, 12, 3, 1, 24, 1, 1};
        //int[] unusedTrainers = {12, 24, 58, 65, 98, 99, 100, 134, 135, 136, 143, 186, 198, 214, 222, 234, 235, 258, 259, 260, 261, 298, 321, 323, 324, 325, 331, 333, 334, 335, 347, 365, 366, 367, 368, 371, 375, 377, 379 };
        int[] blocksetSizes = { 2048, 304, 592, 2048, 304, 1856, 592, 1856, 560, 1872, 1872, 272, 1872, 992, 368, 1760, 928, 2048, 1264, 1152, 928, 576, 2048, 1200};
        string[] blocksetPngNames = { "overworld.png", "reds_house.png", "pokecenter.png", "forest.png", "reds_house.png", "gym.png", "pokecenter.png", "gym.png", "house.png", "gate.png", "gate.png",
            "underground.png", "gate.png", "ship.png", "ship_port.png", "cemetery.png", "interior.png", "cavern.png", "lobby.png", "mansion.png", "lab.png", "club.png", "facility.png", "plateau.png"};

        string[] spritePngNames = {"red", "blue", "oak", "bug_catcher", "slowbro", "lass", "black_hair_boy_1", "little_girl", "bird", "fat_bald_guy", "gambler",
            "black_hair_boy_2", "girl", "hiker", "foulard_woman", "gentleman", "daisy", "biker", "sailor", "cook", "bike_shop_guy", "mr_fuji", "giovanni",
            "rocket", "medium", "waiter", "erika", "mom_geisha", "brunette_girl", "lance","oak_aide","oak_aide","rocker","swimmer","white_player","gym_helper","old_person",
            "mart_guy","fisher","old_medium_woman","nurse","cable_club_woman","mr_masterball","lapras_giver","warden","ss_captain","fisher2","blackbelt","guard",
            "guard", "mom","balding_guy","young_boy","gameboy_kid","gameboy_kid","clefairy","agatha","bruno","lorelei","seel","ball","omanyte","boulder","paper_sheet",
            "book_map_dex","clipboard","snorlax","old_amber","old_amber","lying_old_man","lying_old_man","lying_old_man"};

        int[] unusedMaps = { 11, 105, 106, 107, 109, 110, 111, 112, 114, 115, 116, 117, 204, 205, 206, 231, 237, 238, 241, 242, 243, 244 };
    }
}
