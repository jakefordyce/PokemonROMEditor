using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonROMEditor.Models
{
    public class Move
    {
        public event EventHandler NameChanged;

        private string name;
        public string Name {
            get { return name; }
            set
            {
                name = value;
                OnNameChanged();
            }
        }
        public int ID { get; set; }
        public MoveAnimation AnimationID { get; set; }
        public int Power { get; set; }
        public int MoveType { get; set; }
        public int Accuracy { get; set; }
        public int PP { get; set; }
        public MoveEffect Effect { get; set; }

        protected virtual void OnNameChanged()
        {
            NameChanged?.Invoke(this, new EventArgs());
        }
    }

    public class MoveView
    {
        public MoveView(int id, string name)
        {
            MoveID = id;
            Name = name;
        }
        public int MoveID { get; set; }
        public string Name { get; set; }
    }
}
