using System.Windows.Input;
using Xamarin.Forms;
using Groundsman.Services;
using System.Collections.Generic;

namespace Groundsman.ViewModels
{
    public class LoggerViewModel : BaseViewModel
    {
        public ICommand ToggleButtonClickCommand { set; get; }
        public ICommand ClearButtonClickCommand { set; get; }
        public ICommand ShareButtonClickCommand { set; get; }
        public bool isLogging;

        private string _textEntry;
        public string TextEntry
        {
            get { return _textEntry; }
            set
            {
                _textEntry = value;
                OnPropertyChanged();
            }
        }

        private List<string> _UnitItems = new List<string> { "Seconds", "Minutes", "Hours" };
        public List<string> UnitItems
        {
            get { return _UnitItems; }
            set
            {
                _UnitItems = value;
                OnPropertyChanged();
            }
        }

        private int _UnitEntry = 0;
        public int UnitEntry
        {
            get { return _UnitEntry; }
            set
            {
                _UnitEntry = value;
                OnPropertyChanged();
            }
        }

        private string _ToggleButtonLabel = "Start";
        public string ToggleButtonLabel
        {
            get { return _ToggleButtonLabel; }
            set
            {
                _ToggleButtonLabel = value;
                OnPropertyChanged();
            }
        }

        private int _intervalEntry = 1;
        public int IntervalEntry
        {
            get { return _intervalEntry; }
            set
            {
                int temp = UnitEntry;
                if (value == 1)
                {
                    UnitItems = new List<string> { "Second", "Minute", "Hour" };
                }
                else
                {
                    UnitItems = new List<string> { "Seconds", "Minutes", "Hours" };
                }
                UnitEntry = temp;
                _intervalEntry = value;
                OnPropertyChanged();
            }
        }

        public LoggerViewModel()
        {
            Title = "Logger";
            TextEntry = LogStore.LogString;
            ToggleButtonClickCommand = new Command(() =>
            {
                if (isLogging)
                {
                    ToggleButtonLabel = "Start";
                    LogStore.StopLogging();
                }
                else
                {
                    ToggleButtonLabel = "Stop";
                    if (IntervalEntry < 1)
                    {
                        IntervalEntry = 1;
                    }
                    switch (UnitEntry)
                    {
                        case 0:
                            LogStore.StartLogging(IntervalEntry);
                            break;
                        case 1:
                            LogStore.StartLogging(IntervalEntry * 60);
                            break;
                        case 2:
                            LogStore.StartLogging(IntervalEntry * 3600);
                            break;
                    }
                }
                isLogging = !isLogging;
            });

            ClearButtonClickCommand = new Command(() => { LogStore.ClearLog(); });

            ShareButtonClickCommand = new Command(async () => { await LogStore.ExportLogFile(); });

            MessagingCenter.Subscribe<LogStore>(this, "LogUpdated", (sender) => { TextEntry = LogStore.LogString; });
        }
    }
}
