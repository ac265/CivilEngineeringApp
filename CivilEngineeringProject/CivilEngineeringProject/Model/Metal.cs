using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;

namespace CivilEngineeringProject.Model
{
    public class Metal : INotifyPropertyChanged
    {
        private double _initialLength;
        private double _usedLength;

        // Properties for InitialLength and UsedLength
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

        // Remaining Length calculated from InitialLength and UsedLength
        public double RemainingLength => InitialLength - UsedLength;

        // Method to use metal
        public void UseMetal(double lengthToUse)
        {
            // Check if the length to use is valid
            if (lengthToUse <= 0)
            {
                MessageBox.Show("Lütfen geçerli bir uzunluk girin.", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Ensure there's enough remaining length to use
            if (RemainingLength >= lengthToUse)
            {
                UsedLength += lengthToUse;  // Increase the used length by the requested amount
            }
            else
            {
                MessageBox.Show("Yeterli metal bulunamadı!", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // PropertyChanged event to notify UI of changes
        public event PropertyChangedEventHandler PropertyChanged;

        // Notify property changes
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
