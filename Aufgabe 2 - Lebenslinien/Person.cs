using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;

namespace PLG
{
    [DebuggerDisplay("ID = {ID}, Name = {Name}")]
    class Person : INotifyPropertyChanged
    {
        private string name;

        public readonly int ID;
        public int Birth, Death, Position = -1, Distance = -1; //Because of the arrays being based on zero, -1 means not in the list
        public List<Person> Neighbors;
        public bool Flagged;

        public string Name
        {
            get { return name; }
            set
            {
                name = value;
                OnPropertyChanged("Name");
            }
        }

        public Person(int ID)
        {
            this.ID = ID;
            Neighbors = new List<Person>();
            Name = (ID + 1).ToString();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(name));
        }
    }
}
