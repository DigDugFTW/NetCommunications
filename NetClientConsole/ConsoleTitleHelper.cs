using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace NetClientConsole
{
    public class ConsoleTitleHelper : INotifyPropertyChanged
    {
        public ConsoleTitleHelper()
        {

        }

        private double _ping = 0;
        public double Ping { get => _ping; set { _ping = value; CallInpc(this); } }
        
        private int _uploadBytes = 0;
        public int UploadBytes { get => _uploadBytes; set { _uploadBytes = value; CallInpc(this); } }
        
        private int _downloadBytes = 0;
        public int DownloadBytes
        {
            get { return _downloadBytes; }
            set { _downloadBytes = value; CallInpc(this); }
        }

        private string _sessionInformation = "";
        public string SessionInformation
        {
            get { return _sessionInformation; }
            set { _sessionInformation = value; CallInpc(this); }
        }


        public override string ToString()
        {
            return $"Ping:{Ping}, UP:{UploadBytes} ({(UploadBytes/1024)/1024} MB), DOWN:{DownloadBytes} ({(DownloadBytes/1024)/1024} MB) | {SessionInformation}";
        }

        private void CallInpc(object sender, [CallerMemberName]string propertyName = "")
        {
            PropertyChanged?.Invoke(sender, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
