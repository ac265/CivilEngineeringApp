using CivilEngineeringProject.Model;
using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace CivilEngineeringProject.ViewModel
{
    public class MetalViewModel : INotifyPropertyChanged
    {
        private const string FilePath = "metals.json"; // Path for the JSON file

        public ObservableCollection<Metal> Metals { get; set; }  // Metal collection
        public ObservableCollection<Metal> UsedMetals { get; set; } // Used metals collection
        public ObservableCollection<Metal> RemainingParts { get; set; } // Remaining parts collection
        // Ensure PropertyChanged for LengthToUse to notify UI and command reevaluation
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
        public ICommand AddMetalCommand { get; set; } // Command to add metal
        public ICommand UseMetalCommand { get; set; } // Command to use metal

        public MetalViewModel()
        {
            Metals = new ObservableCollection<Metal>();
            UsedMetals = new ObservableCollection<Metal>();
            RemainingParts = new ObservableCollection<Metal>();
            AddMetalCommand = new RelayCommand(AddMetal);
            UseMetalCommand = new RelayCommand(UseMetal, CanUseMetal);

            // Listen to PropertyChanged to trigger command reevaluation when LengthToUse changes
            PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(LengthToUse))
                {
                    CommandManager.InvalidateRequerySuggested();  // Forces CanExecute to be rechecked
                }
            };

            LoadData(); // Load saved data from file
        }

        // Method to check if metal can be used based on remaining lengths
        public bool CanUseMetal()
        {
            return RemainingParts.Any(m => m.RemainingLength >= LengthToUse);
        }

        // Method to use metal
        public void UseMetal()
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

                SaveData(); // Save data to file after use
            }
            else
            {
                MessageBox.Show("Not enough metal available!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Add new metal of 12 meters
        public void AddMetal()
        {
            var newMetal = new Metal { InitialLength = 12 };  // Default length of new metal is 12 meters
            Metals.Add(newMetal);

            // Add to remaining parts as well
            RemainingParts.Add(new Metal { InitialLength = newMetal.InitialLength });

            SaveData(); // Save data after adding new metal
        }

        // Load saved data from JSON file
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
                        if (metal.InitialLength > 0)
                        {
                            Metals.Add(metal);

                            if (metal.RemainingLength > 0)
                            {
                                RemainingParts.Add(new Metal { InitialLength = metal.RemainingLength });
                            }
                        }
                    }
                }
            }
            else
            {
                // Create an empty file if none exists
                File.WriteAllText(FilePath, "[]");
            }
        }

        // Save the current state of metals to JSON file
        private void SaveData()
        {
            try
            {
                var allMetals = Metals.Concat(RemainingParts).ToList();
                var json = JsonConvert.SerializeObject(allMetals);

                File.WriteAllText(FilePath, json); // Write to the file

                // Debug: Output to console
                Console.WriteLine("Data saved: " + json);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error saving file: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
