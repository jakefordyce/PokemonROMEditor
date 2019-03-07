using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
        private TileSet tileSetID;
        public TileSet TileSetID
        {
            get { return tileSetID; }
            set
            {
                tileSetID = value;
                OnTileSetChanged();
            }
        }
        public ObservableCollection<MapObject> MapObjects { get; set; }
        public int Connections { get; set; } //this is temporary until we start working with the connection data.
        public int Signs { get; set; }
        public int Warps { get; set; }

        public event EventHandler TileSetChanged;

        protected virtual void OnTileSetChanged()
        {
            TileSetChanged?.Invoke(this, new EventArgs());
        }
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
        public event EventHandler SpriteChanged;

        public MapObject()
        {
            // Setting up some default values. Not all data will be used for every object
            PokemonObj = new EnemyPokemon();
            PokemonObj.PokedexID = 1;
            PokemonObj.Level = 1;
            Item = ItemType.ANTIDOTE;
            TrainerGroupNum = 0;
            TrainerNum = 1;
        }
        private int spriteID;
        public int SpriteID
        {
            get
            {
                return spriteID;
            }
            set
            {
                spriteID = value;
                OnSpriteChanged();
            }
        }
        public MapObjectType ObjectType { get; set; }
        private int yPosition;
        public int YPosition
        {
            get { return yPosition; }
            set
            {
                yPosition = value;
                OnSpriteChanged();
            }
        }
        private int xPosition;
        public int XPosition
        {
            get { return xPosition; }
            set
            {
                xPosition = value;
                OnSpriteChanged();
            }
        }
        public MapObjectMovementType Movement { get; set; }
        public MapObjectFacing Facing { get; set; }
        public ItemType Item { get; set; }
        public EnemyPokemon PokemonObj { get; set; }
        public int TrainerGroupNum { get; set; }
        public int TrainerNum { get; set; }

        protected virtual void OnSpriteChanged()
        {
            SpriteChanged?.Invoke(this, new EventArgs());
        }
    }

    public class MapObjectSprite
    {
        public string SpriteName { get; set; }
        public string FileName { get; set; }
        public Bitmap SpriteBitmap { get; set; }
    }

}
