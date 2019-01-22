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

        int shopsStartByte = 9282; //The data for the pokemarts inventories starts at byte 0x2442
        int mewStartByte = 16987; // Mew's stats start at byte 0x425B
        int wildEncountersByte = 53471; //The data for wild encounters start at 0xD0DE but the first one is empty so I skip it.
        int wildEncountersEndByte = 54727;
        int tmStartByte = 79731; //The TM info starts at byte 0x13773.
        int pokemonNameStartByte = 115230; //Pokemon names start at byte 0x1c21e and also run in Index order.
        int pointerBase = 212992; //this is the value that is added to the pointers to get the location of evolutions/moves and trainer data
        int movesStartingByte = 229376; //The move data starts 0x38000 bytes into the file which is 229376 in Decimal.
        int pokemonStartByte = 230366; //Pokemon data starts at byte 0x383DE. It goes in Pokedex order, Bulbasaur through Mewtwo.
        int trainerPointersByte = 236859; // The pointers to the trainer groups start at byte 0x39D3B
        int trainerStartByte = 236953; //The data for trainers starts at 0x39D99
        int trainerEndByte = 238893; //The last byte for trainers is 0x3A52D
        int pokemonEvosPointersByte = 241756; //The pointers to the pokemon Evolutions and learned moves start at byte 0x3B05C.
        int pokemonEvosMovesByte = 242136; //Pokemon evolutions and moves learned through leveling are stored together starting at byte 0x3B1D8.
        int typeChartByte = 255092; //The types' strengths start at byte 0x3E474.
        int pokedexStartByte = 266276; //List of pokedex IDs start at byte 0x41024 and run in Index order, Rhydon through Victreebel.
        int tmPricesStartByte = 507815; //TM prices start at byte 0x7BFA7
        int moveNamesByte = 720896; //The data for move names starts at 0xB0000 bytes into the file which is 720896 in Decimal.

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
                                        ObservableCollection<TM> tms, ObservableCollection<WildEncounterZone> zones, ObservableCollection<Trainer> trainers, ObservableCollection<Shop> shops)
        {            
            // first update the byte array with the data that the user has modified.
            SaveMoves(moves);
            SavePokemon(pokemons);
            SaveTypeStrengths(str);
            SaveTMs(tms);
            SaveWildEncounters(zones);
            SaveTrainers(trainers);
            SaveShops(shops);
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
                typeStrengthToAdd = new TypeStrength();
                typeStrengthToAdd.AttackType = (PokeType)romData[typeChartByte + (i * 3)]; //first byte is the attacking type
                typeStrengthToAdd.DefenseType = (PokeType)romData[typeChartByte + (i * 3) + 1]; //second byte is the defending type
                typeStrengthToAdd.Effectiveness = (DamageModifier)romData[typeChartByte + (i * 3) + 2]; //third byte is effectiveness X 10. So double damage = 20, half damage = 5.
                typeStrengths.Add(typeStrengthToAdd);
            }

            return typeStrengths;
        }

        private void SaveTypeStrengths(ObservableCollection<TypeStrength> str)
        {
            for(int i = 0; i < 82; i++)
            {
                romData[typeChartByte + (i * 3)] = (byte)str.ElementAt(i).AttackType;
                romData[typeChartByte + (i * 3) + 1] = (byte)str.ElementAt(i).DefenseType;
                romData[typeChartByte + (i * 3) + 2] = (byte)str.ElementAt(i).Effectiveness;
            }
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
            int trainerNameTracker = 0; // tells which name to pull from the dictionary
            int numOfTrainersWithName = 0;            
            string trainerName = "";
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
                trainerName = TrainerNames[trainerNameTracker];
                if (unusedTrainers.Contains(trainers.Count))
                {
                    trainerName += " (unused)";
                }
                trainerToAdd.TrainerName = trainerName;

                numOfTrainersWithName++;
                if(numOfTrainersWithName == trainerCounts[trainerNameTracker])
                {
                    trainerNameTracker++;
                    numOfTrainersWithName = 0;
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
            int currentByte = shopsStartByte;
            for (int i = 0; i < 16; i++)
            {
                currentByte += 2; //Skip the first 2 bytes. The first byte is always 0xFE and the 2nd is the number of items for sale.

                foreach(var item in shops.ElementAt(i).Items)
                {
                    romData[currentByte++] = (byte)item.ItemID;
                }
                currentByte++;
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
            { 227, "-" },
            { 232, "." },
            { 224, "'" },
            { 239, "M" },
            { 245, "F" }
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

        int[] trainerCounts = { 13, 14, 18, 8, 9, 24, 7, 12, 14, 15, 9, 3, 11, 15, 9, 7, 15, 4, 2, 8, 6, 17, 9, 9, 3, 13, 3, 41, 10, 8, 1, 1, 1, 1, 1, 1, 1, 1, 5, 12, 3, 1, 24, 1, 1};
        int[] unusedTrainers = {12, 24, 58, 65, 98, 99, 100, 134, 135, 136, 143, 186, 198, 214, 222, 234, 235, 258, 259, 260, 261, 298, 321, 323, 324, 325, 331, 333, 334, 335, 347, 365, 366, 367, 368, 371, 375, 377, 379 };
    }
}
