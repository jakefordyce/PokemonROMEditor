using System;
using System.Collections.Generic;
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
    }

    public class BlockSet
    {        
        public string SourceFile { get; set; }
        public int[] BlockDefinitions { get; set; }
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
}
