using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoundPanel
{
    public class Data
    {
        //private string dataPath;
        //public string DataPath
        //{
        //    get => dataPath;
        //    set => dataPath = value;
        //}

        //private object buttonContent;
        //public object ButtonContent
        //{
        //    get => buttonContent;
        //    set => buttonContent = value;
        //}

        //private string keyBinding;
        //public string KeyBinding
        //{
        //    get => keyBinding;
        //    set => keyBinding = value;
        //}



        private List<string> dataPaths;
        public List<string> DataPaths
        {
            get => dataPaths;
            set => dataPaths = value;
        }

        private List<object> buttonContentList;
        public List<object> ButtonContentList
        {
            get => buttonContentList;
            set => buttonContentList = value;
        }

        private ObservableCollection<string> keyBindingList;
        public ObservableCollection<string> KeyBindingList
        {
            get => keyBindingList;
            set => keyBindingList = value;
        }
    }
}
