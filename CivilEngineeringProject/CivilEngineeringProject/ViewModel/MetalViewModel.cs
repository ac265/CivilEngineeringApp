using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Newtonsoft.Json;
using CivilEngineeringProject.Model;

namespace CivilEngineeringProject.ViewModel
{
    public class MetalViewModel : INotifyPropertyChanged
    {
        private const string FilePath = "metals.json";  // File path for saving metals data
        public ObservableCollection<Metal> Metals { get; set; } = new ObservableCollection<Metal>();
        public ObservableCollection<Metal> RemainingParts { get; set; } = new ObservableCollection<Metal>();

        public ICommand AddMetalCommand { get; set; }
        public ICommand UseRemainingMetalCommand { get; set; }

        private double _lengthToUse;
        public double LengthToUse
        {
            get => _lengthToUse;
            set
            {
                if (_lengthToUse != value)
                {
                    _lengthToUse = value;
                    OnPropertyChanged(nameof(LengthToUse));
                    OnPropertyChanged(nameof(CanUseMetal));  // Re-evaluate CanExecute
                    CommandManager.InvalidateRequerySuggested();  // Forces command re-evaluation
                }
            }
        }

        public MetalViewModel()
        {
            AddMetalCommand = new RelayCommand(AddMetal);
            UseRemainingMetalCommand = new RelayCommand(UseRemainingMetal, CanUseMetal);
            LoadData();
        }

        // Method to add new metal (12 meters by default)
        public void AddMetal()
        {
            // Yeni bir metal nesnesi olu�turuyoruz
            var newMetal = new Metal { InitialLength = 12, UsedLength = 0 };

            // Yeni metali Metals koleksiyonuna ekliyoruz
            Metals.Add(newMetal);
            RemainingParts.Add(newMetal); // Kalan metali de RemainingParts listesine ekliyoruz

            // Veriyi kaydediyoruz
            SaveData();
        }

        // Method to use remaining metal from the collection
        public void UseRemainingMetal()
        {
            if (LengthToUse <= 0)
            {
                MessageBox.Show("Please enter a valid length.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var availableMetal = RemainingParts.FirstOrDefault(m => m.RemainingLength >= LengthToUse);

            if (availableMetal != null)
            {
                availableMetal.UseMetal(LengthToUse);

                // If metal is used up (remaining length is 0), remove it from the list
                if (availableMetal.RemainingLength == 0)
                {
                    RemainingParts.Remove(availableMetal);
                }
                else
                {
                    // Otherwise, add the remaining part to the list
                    var remainingPart = new Metal { InitialLength = availableMetal.RemainingLength };

                    if (!RemainingParts.Any(m => m.RemainingLength == remainingPart.RemainingLength))
                    {
                        RemainingParts.Add(remainingPart);
                    }
                }

                SaveData();  // Save data to file after use
            }
            else
            {
                MessageBox.Show("Not enough metal available!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Method to check if metal can be used based on remaining lengths
        public bool CanUseMetal()
        {
            return RemainingParts.Any(m => m.RemainingLength >= LengthToUse);
        }

        // Save data to JSON
        // Save data to JSON
        public void SaveData()
        {
            try
            {
                var json = JsonConvert.SerializeObject(Metals, Formatting.Indented);
                File.WriteAllText(FilePath, json);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Veri kaydedilirken bir hata olu�tu: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Load data from JSON
        private void LoadData()
        {
            if (File.Exists(FilePath))
            {
                var json = File.ReadAllText(FilePath);
                var metalsList = JsonConvert.DeserializeObject<ObservableCollection<Metal>>(json);
                if (metalsList != null)
                {
                    Metals.Clear();
                    RemainingParts.Clear();

                    foreach (var metal in metalsList)
                    {
                        Metals.Add(metal);
                        if (metal.RemainingLength > 0)
                        {
                            RemainingParts.Add(metal);
                        }
                    }
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}