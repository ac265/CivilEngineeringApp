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
        public ObservableCollection<Metal> RemainingParts { get; set; } = new ObservableCollection<Metal>();
        public ObservableCollection<Metal> UsedParts { get; set; } = new ObservableCollection<Metal>();

        private ObservableCollection<Metal> _metals = new ObservableCollection<Metal>();

        public ObservableCollection<Metal> Metals
        {
            get { return _metals; }
            set
            {
                if (_metals != value)
                {
                    _metals = value;
                    OnPropertyChanged(nameof(Metals));
                    OnPropertyChanged(nameof(RemainingUnusedPartsCount));
                    OnPropertyChanged(nameof(RemainingUnusedPartsTotalLength));
                }
            }
        }

        private int _metalCount = 1;

        public int MetalCount
        {
            get { return _metalCount; }
            set
            {
                if (_metalCount != value) // De�er de�i�ti�inde
                {
                    _metalCount = value;
                    OnPropertyChanged(nameof(MetalCount)); // Burada property ad� 'MetalCount' olmal�
                }
            }
        }

        // JSON'dan al�nan verilerde kullan�lmayan metallerin say�s�n� d�nd�ren �zellik
        public int RemainingUnusedPartsCount
        {
            get
            {
                return Metals.Count(m => m.RemainingLength > 0);
            }
            set
            {
                // E�er set metodu ile de�i�iklik yap�l�rsa, burada koleksiyona m�dahale edebiliriz.
                // Ancak, genellikle hesaplanan de�erler d��ar�dan set edilmez, burada sadece �rnek g�sterilmektedir.
                // Bu �ekilde, say�y� de�i�tirerek koleksiyonu g�ncelleyebilirsiniz.
                OnPropertyChanged(nameof(RemainingUnusedPartsCount));
            }
        }

        // JSON'dan al�nan verilerdeki kullan�lmayan metallerin toplam uzunlu�unu d�nd�ren �zellik
        public double RemainingUnusedPartsTotalLength
        {
            get
            {
                return Metals.Where(m => m.RemainingLength > 0).Sum(m => m.RemainingLength);
            }
            set
            {
                // Burada da, total length de�eri de�i�irse, koleksiyondaki metallerin RemainingLength de�eri
                // g�ncellenebilir. Ancak genellikle bu tarz veriler, do�rudan d��ar�dan set edilmez.
                OnPropertyChanged(nameof(RemainingUnusedPartsTotalLength));
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
            // MetalCount de�erine g�re metal ekliyoruz
            for (int i = 0; i < MetalCount; i++)
            {
                // Yeni bir metal nesnesi olu�turuyoruz
                var newMetal = new Metal { InitialLength = 12, UsedLength = 0 };

                // Yeni metali Metals koleksiyonuna ekliyoruz
                Metals.Add(newMetal);

                // Kalan metali RemainingParts listesine ekliyoruz
                RemainingParts.Add(newMetal);
            }

            // Yeni eklenen metal sonras� her iki hesaplanan de�eri g�ncelleyerek UI'yi yeniden tetikliyoruz
            OnPropertyChanged(nameof(RemainingUnusedPartsCount));
            OnPropertyChanged(nameof(RemainingUnusedPartsTotalLength));

            // Veriyi kaydediyoruz
            SaveData();
        }

        // Kalan metal kullan�m i�lemi
        // Kalan metal kullan�m i�lemi
        public void UseRemainingMetal()
        {
            // Kalan metaller k���kten b�y��e s�ralan�r
            var metalToUse = RemainingParts
                .Where(m => m.RemainingLength >= LengthToUse) // Yaln�zca yeterli uzunlu�a sahip metaller
                .OrderBy(m => m.RemainingLength) // K���kten b�y��e s�ralama
                .FirstOrDefault(); // �lk uygun metal se�ilir

            if (metalToUse != null)
            {
                // Se�ilen metal kullan�l�r
                metalToUse.UseMetal(LengthToUse);

                // E�er metalin kalan uzunlu�u s�f�rsa, metal listesinden ��kar
                if (metalToUse.RemainingLength == 0)
                {
                    RemainingParts.Remove(metalToUse);
                }

                // Metalin kullan�lm�� halini UsedParts listesine ekliyoruz
                UsedParts.Add(metalToUse);  // Kullan�lan metal eklenir
                // Veriyi kaydet
                SaveData();

                // Verileri g�ncelle
                OnPropertyChanged(nameof(RemainingUnusedPartsCount)); // Kalan metal say�s� g�ncellenmeli
                OnPropertyChanged(nameof(RemainingUnusedPartsTotalLength)); // Kalan metal uzunlu�u g�ncellenmeli

            }
            else
            {
                MessageBox.Show("Yeterli metal bulunamad�!", "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
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

            // Veri y�klendikten sonra RemainingPartsCount ve RemainingPartsTotalLength'i g�ncelle
            OnPropertyChanged(nameof(RemainingUnusedPartsCount));
            OnPropertyChanged(nameof(RemainingUnusedPartsTotalLength));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}