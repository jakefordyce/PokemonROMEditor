using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace PokemonROMEditor.Models
{
    public class Map
    {
        public Map(string name, TileSet ts)
        {
            MapName = name;
            TileSetID = ts;
        }
        public string MapName { get; }
        public int Height { get; set; }
        public int Width { get; set; }
        public int[] MapBlockValues { get; set; }
        public TileSet TileSetID { get; }
        public ObservableCollection<MapObject> MapObjects { get; set; }
    }

    public class BlockSet
    {        
        public string SourceFile { get; set; }
        public int[] BlockDefinitions { get; set; }
        public ObservableCollection<Bitmap> Tiles { get; set; }
    }

    public class MapTile
    {
        public MapTile(int id, BitmapSource image)
        {
            TileID = id;
            TileImage = image;
        }
        public int TileID { get; set; }
        public BitmapSource TileImage { get; set; }
    }

    public class MapObject
    {
        public MapObject()
        {
            // Setting up some default values. Not all data will be used for every object
            PokemonObj = new EnemyPokemon();
            PokemonObj.PokedexID = 1;
            PokemonObj.Level = 1;
            Item = ItemType.ANTIDOTE;
            TrainerGroupNum = 201;
            TrainerNum = 1;
        }
        public int SpriteID { get; set; }
        public MapObjectType ObjectType { get; set; }
        public int YPosition { get; set; }
        public int XPosition { get; set; }
        public MapObjectMovementType Movement { get; set; }
        public MapObjectFacing Facing { get; set; }
        public ItemType Item { get; set; }
        public EnemyPokemon PokemonObj { get; set; }
        public int TrainerGroupNum { get; set; }
        public int TrainerNum { get; set; }
    }

    public class MapObjectSprite
    {
        public string SpriteName { get; set; }
        public string FileName { get; set; }
        public Bitmap SpriteBitmap { get; set; }
    }

}
