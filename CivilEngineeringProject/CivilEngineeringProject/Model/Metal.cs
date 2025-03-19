using System;
using System.ComponentModel;

namespace CivilEngineeringProject.Model
{
    public class Metal : INotifyPropertyChanged
    {
        private double _initialLength;
        private double _usedLength;

        public double InitialLength
        {
            get => _initialLength;
            set
            {
                if (_initialLength != value)
                {
                    _initialLength = value;
                    OnPropertyChanged(nameof(InitialLength));
                    OnPropertyChanged(nameof(RemainingLength));  // Notify when InitialLength changes
                }
            }
        }

        public double UsedLength
        {
            get => _usedLength;
            set
            {
                if (_usedLength != value)
                {
                    _usedLength = value;
                    OnPropertyChanged(nameof(UsedLength));
                    OnPropertyChanged(nameof(RemainingLength));  // Notify when UsedLength changes
                }
            }
        }

        public double RemainingLength => InitialLength - UsedLength;  // Derived property, no need to set directly

        // Method to use the metal
        public void UseMetal(double length)
        {
            if (length <= RemainingLength)
            {
                UsedLength += length; // Increase used length by the specified amount
            }
            else
            {
                // If the requested length is greater than the remaining, throw an exception
                throw new ArgumentException("The requested length exceeds the remaining metal length.");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        // Notify property changes
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
