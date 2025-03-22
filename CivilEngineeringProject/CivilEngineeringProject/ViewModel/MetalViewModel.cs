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
        private int _metalCount = 1;

        public int MetalCount
        {
            get { return _metalCount; }
            set
            {
                if (_metalCount != value) // Deðer deðiþtiðinde
                {
                    _metalCount = value;
                    OnPropertyChanged(nameof(MetalCount)); // Burada property adý 'MetalCount' olmalý
                }
            }
        }

        public ICommand AddMetalCommand { get; set; }
        public ICommand UseMetalCommand { get; set; }

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
            UseMetalCommand = new RelayCommand(UseRemainingMetal, CanUseMetal);

            LoadData();
        }

        // Method to add new metal (12 meters by default)
        public void AddMetal()
        {
            // MetalCount deðerine göre metal ekliyoruz
            for (int i = 0; i < MetalCount; i++)
            {
                // Yeni bir metal nesnesi oluþturuyoruz
                var newMetal = new Metal { InitialLength = 12, UsedLength = 0 };

                // Yeni metali Metals koleksiyonuna ekliyoruz
                Metals.Add(newMetal);

                // Kalan metali RemainingParts listesine ekliyoruz
                RemainingParts.Add(newMetal);
            }

            // Veriyi kaydediyoruz
            SaveData();
        }

        // Kalan metal kullaným iþlemi
        // Kalan metal kullaným iþlemi
        public void UseRemainingMetal()
        {
            // Kalan metaller küçükten büyüðe sýralanýr
            var metalToUse = RemainingParts
                .Where(m => m.RemainingLength >= LengthToUse) // Yalnýzca yeterli uzunluða sahip metaller
                .OrderBy(m => m.RemainingLength) // Küçükten büyüðe sýralama
                .FirstOrDefault(); // Ýlk uygun metal seçilir

            if (metalToUse != null)
            {
                // Seçilen metal kullanýlýr
                metalToUse.UseMetal(LengthToUse);

                // Eðer metalin kalan uzunluðu sýfýrsa, metal listesinden çýkar
                if (metalToUse.RemainingLength == 0)
                {
                    RemainingParts.Remove(metalToUse);
                }

                // Veriyi kaydet
                SaveData();
            }
            else
            {
                MessageBox.Show("Yeterli metal bulunamadý!", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
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
                MessageBox.Show($"Veri kaydedilirken bir hata oluþtu: {ex.Message}", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
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