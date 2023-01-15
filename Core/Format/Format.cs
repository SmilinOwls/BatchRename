using System;
using System.ComponentModel;

namespace Core.Format
{
    public class Format : INotifyPropertyChanged, ICloneable
    {
        public string Name { get; set; }
        public string ReName { get; set; }
        public string Path { get; set; }
        public string Status { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;

        public object Clone()
        {
            return (Format)this.MemberwiseClone();
        }
    }
}