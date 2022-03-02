using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Media;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
//using Microsoft.VisualBasic.Logging;
using NAudio;
using NAudio.Midi;
using Serilog;

namespace SoundPanel
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;


            // There are 16 sample pads
            var padCount = 16;

            InitializeButtons(padCount);


            // Mark all learnButtons as not active
            for (int i = 0; i < padCount; i++)
            {
                activeButtons.Add(false);
            }

            AddAllLearnButtonsToList();

            for (int i = 0; i < padCount; i++)
            {
                midiAssignmentList.Add("");
            }


            // Enumerating MIDI Controllers
            for (int device = 0; device < MidiIn.NumberOfDevices; device++)
            {
                comboBoxMidiInDevices.Items.Add(MidiIn.DeviceInfo(device).ProductName);
            }
            if (comboBoxMidiInDevices.Items.Count > 0)
            {
                comboBoxMidiInDevices.SelectedIndex = 0;
            }

            // Arm selected MIDI controller to receive MIDI signals from
            // Listen to MIDI messages
            //var midiIn2 = midiIn;
            midiIn = new MidiIn(comboBoxMidiInDevices.SelectedIndex);
            midiIn.MessageReceived += midiIn_MessageReceived;
            midiIn.Start();
        }


        ////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////////



        private MidiIn midiIn;

        private List<Button> sampleButtonList = new List<Button>();

        private ObservableCollection<string> midiAssignmentList = new ObservableCollection<string>();
        public ObservableCollection<string> MIDIAssignmentList
        {
            get => midiAssignmentList;
            set
            {
                midiAssignmentList = value;
                this.RaisePropertyChanged();
            }
        }

        private List<bool> activeButtons = new List<bool>();
        private List<Button> allLearnButtons = new List<Button>();

        private List<string> dataPaths = new List<string>();

        private void AddAllLearnButtonsToList()
        {
            allLearnButtons.Add(LearnButton_0);
            allLearnButtons.Add(LearnButton_1);
            allLearnButtons.Add(LearnButton_2);
            allLearnButtons.Add(LearnButton_3);
            allLearnButtons.Add(LearnButton_4);
            allLearnButtons.Add(LearnButton_5);
            allLearnButtons.Add(LearnButton_6);
            allLearnButtons.Add(LearnButton_7);
            allLearnButtons.Add(LearnButton_8);
            allLearnButtons.Add(LearnButton_9);
            allLearnButtons.Add(LearnButton_10);
            allLearnButtons.Add(LearnButton_11);
            allLearnButtons.Add(LearnButton_12);
            allLearnButtons.Add(LearnButton_13);
            allLearnButtons.Add(LearnButton_14);
            allLearnButtons.Add(LearnButton_15);
        }

        private string midiState = "false";
        public string MIDIState
        {
            get => midiState;
            set
            {
                midiState = value;
                this.RaisePropertyChanged();
            }
        }

        private NoteEvent ne;

        // Helper to just use NoteOn signals to trigger further functions
        private bool midiNoteState = false;





        //private void StartMonitoringMidi()
        //{
        //    MidiIn midiDevice = new MidiIn(comboBoxMidiInDevices.SelectedIndex);
        //    //midiDevice.MessageReceived += new EventHandler<MidiInMessageEventArgs>(midiDevice.MessageReceived);
        //    midiDevice.Start();
        //}

        private void InitializeButtons(int padCount)
        {
            // Add all buttons to a list
            for (int i = 0; i < padCount; i++)
            {
                var btn = (Button)FindName($"btn_{i}");
                if (btn is Button)
                {
                    sampleButtonList.Add(btn);
                }
            }



            // Add content to each button
            foreach (var button in sampleButtonList)
            {
                // Create placeholder for a new dataPath which is gonna be changed after choosing a sample
                dataPaths.Add("");

                // In case I'll implement startup-presets some day it should check if the button content is empty (= has no sample loaded)
                if (button.Content.Equals(""))
                {
                    button.Content = "Drop .wav file here";
                    button.Opacity = 0.5;
                }

            }
        }

        private void DropFile(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] directories = (string[])e.Data.GetData(DataFormats.FileDrop);
                string pathName = System.IO.Path.GetFullPath(directories[0]);
                string directoryName = System.IO.Path.GetDirectoryName(directories[0]);
                Button srcButton = e.Source as Button;

                // Check which button is pressed and assign the datapath of the dropped sample to this button
                for (int i = 0; i < sampleButtonList.Count; i++)
                {
                    if (srcButton.Equals(sampleButtonList[i]))
                    {
                        dataPaths[i] = pathName;

                        // Change the appearence of the learn button below the given sample button
                        allLearnButtons[i].Opacity = 1.0;
                    }
                }


                //// Get the index out of the Tag and convert it to an integer
                //var buttonIndex = Convert.ToInt32(srcButton.Tag);
                //dataPaths[buttonIndex] = pathName;


                // Change Button content to the file name without its' path
                srcButton.Content = pathName.Remove(0, directoryName.Length + 1);
                srcButton.Opacity = 1.0;


            }      
        }

        private void LoadSample(int index)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog();
            openFileDialog.DefaultExt = ".wav";
            openFileDialog.Filter = "WAV files (.wav)|*.wav";
            bool? result = openFileDialog.ShowDialog();

            if (result == true)
            {
                dataPaths[index] = openFileDialog.FileName;
                sampleButtonList[index].Opacity = 1.0;
                sampleButtonList[index].Content = System.IO.Path.GetFileName(openFileDialog.FileName);

                // Change the appearence of the learn button below the given sample button
                allLearnButtons[index].Opacity = 1.0;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < sampleButtonList.Count; i++)
            {
                if (sender.Equals(sampleButtonList[i]))
                {
                    if (dataPaths[i].Equals("") || dataPaths[i].Equals(null))
                    {
                        LoadSample(i);
                    }
                    HandleKeyBoardPlaying(i);
                }
            }
        }

        private void Learn_Button_Click(object sender, RoutedEventArgs e)
        {
            // MAYBE SOLVABLE LIKE INITALIZEBUTTON
            if (sender.Equals(LearnButton_0))
            {
                var index = 0;
                HandleButton(index);
            }

            if (sender.Equals(LearnButton_1))
            {
                var index = 1;
                HandleButton(index);
            }

            if (sender.Equals(LearnButton_2))
            {
                var index = 2;
                HandleButton(index);
            }

            if (sender.Equals(LearnButton_3))
            {
                var index = 3;
                HandleButton(index);
            }

            if (sender.Equals(LearnButton_4))
            {
                var index = 4;
                HandleButton(index);
            }

            if (sender.Equals(LearnButton_5))
            {
                var index = 5;
                HandleButton(index);
            }

            if (sender.Equals(LearnButton_6))
            {
                var index = 6;
                HandleButton(index);
            }

            if (sender.Equals(LearnButton_7))
            {
                var index = 7;
                HandleButton(index);
            }

            if (sender.Equals(LearnButton_8))
            {
                var index = 8;
                HandleButton(index);
            }

            if (sender.Equals(LearnButton_9))
            {
                var index = 9;
                HandleButton(index);
            }

            if (sender.Equals(LearnButton_10))
            {
                var index = 10;
                HandleButton(index);
            }

            if (sender.Equals(LearnButton_11))
            {
                var index = 11;
                HandleButton(index);
            }

            if (sender.Equals(LearnButton_12))
            {
                var index = 12;
                HandleButton(index);
            }

            if (sender.Equals(LearnButton_13))
            {
                var index = 13;
                HandleButton(index);
            }

            if (sender.Equals(LearnButton_14))
            {
                var index = 14;
                HandleButton(index);
            }

            if (sender.Equals(LearnButton_15))
            {
                var index = 15;
                HandleButton(index);
            }
        }

        // Activate the learn button and make it change its' color by pressing it
        private void HandleButton(int index)
        {
            for (int i = 0; i < allLearnButtons.Count; i++)
            {
                if (i != index)
                {
                    allLearnButtons[i].Background = Brushes.BlueViolet;
                    activeButtons[i] = false;
                }
            }

            // Activate the learn button if it's not activated. When active you can assign a key to play the appropriate sample
            if (activeButtons[index] == false)
            {
                activeButtons[index] = true;
                //allLearnButtons[index].Background = Brushes.Cyan;
                allLearnButtons[index].Foreground = Brushes.Cyan;
            }
            else
            {
                activeButtons[index] = false;
                //allLearnButtons[index].Background = Brushes.BlueViolet;
                allLearnButtons[index].Foreground = Brushes.White;
            }

            //midiIn = new MidiIn(comboBoxMidiInDevices.SelectedIndex);
            midiIn.Stop();
            midiIn.MessageReceived += midiIn_MessageReceived;
            midiIn.Start();
        }

        // TODO: MAKE IT AUTOMATED (BUTTONS AS SENDERS ARE UNIQUE THOUGH)
        private void LearnButton_KeyDown(object sender, KeyEventArgs e)
        {
            if (sender.Equals(LearnButton_0))
            {
                var index = 0;
                HandleKeyDown(index, e);
            }

            if (sender.Equals(LearnButton_1))
            {
                var index = 1;
                HandleKeyDown(index, e);
            }

            if (sender.Equals(LearnButton_2))
            {
                var index = 2;
                HandleKeyDown(index, e);
            }

            if (sender.Equals(LearnButton_3))
            {
                var index = 3;
                HandleKeyDown(index, e);
            }

            if (sender.Equals(LearnButton_4))
            {
                var index = 4;
                HandleKeyDown(index, e);
            }

            if (sender.Equals(LearnButton_5))
            {
                var index = 5;
                HandleKeyDown(index, e);
            }

            if (sender.Equals(LearnButton_6))
            {
                var index = 6;
                HandleKeyDown(index, e);
            }

            if (sender.Equals(LearnButton_7))
            {
                var index = 7;
                HandleKeyDown(index, e);
            }

            if (sender.Equals(LearnButton_8))
            {
                var index = 8;
                HandleKeyDown(index, e);
            }

            if (sender.Equals(LearnButton_9))
            {
                var index = 9;
                HandleKeyDown(index, e);
            }

            if (sender.Equals(LearnButton_10))
            {
                var index = 10;
                HandleKeyDown(index, e);
            }

            if (sender.Equals(LearnButton_11))
            {
                var index = 11;
                HandleKeyDown(index, e);
            }

            if (sender.Equals(LearnButton_12))
            {
                var index = 12;
                HandleKeyDown(index, e);
            }

            if (sender.Equals(LearnButton_13))
            {
                var index = 13;
                HandleKeyDown(index, e);
            }

            if (sender.Equals(LearnButton_14))
            {
                var index = 14;
                HandleKeyDown(index, e);
            }

            if (sender.Equals(LearnButton_15))
            {
                var index = 15;
                HandleKeyDown(index, e);
            }
        }

        private void HandleKeyDown(int index, KeyEventArgs e)
        {
            if (activeButtons[index] == true)
            {
                if (!midiAssignmentList.Contains(e.Key.ToString()))
                {
                    MIDIAssignmentList[index] = e.Key.ToString();
                    // TODO ADD MIDI
                    activeButtons[index] = false;
                    this.Dispatcher.Invoke(new dResetLearnButtonColors(resetLearnButtonColors), allLearnButtons[index]);
                    //this.Dispatcher.Invoke(new dChangeColorOnPlay(ChangeColorOnPlay), index);
                    //allLearnButtons[index].Background = Brushes.BlueViolet;
                    //allLearnButtons[index].Foreground = Brushes.White;
                }
                else
                {
                    var message = "This key is already used for another button! Do you want to remove the other key assignment?";
                    var caption = "Key Bindings";
                    var result = MessageBox.Show(message, caption, MessageBoxButton.YesNo);

                    if (result == MessageBoxResult.Yes)
                    {
                        // Find already taken key binding, reset it and take over new key binding
                        for (int i = 0; i < MIDIAssignmentList.Count; i++)
                        {
                            if (MIDIAssignmentList[i] == e.Key.ToString())
                            {
                                MIDIAssignmentList[i] = "";
                            }
                        }

                        //var assignedKey = MIDIAssignmentList.Where(x => x == e.Key.ToString()).ToString();
                        //assignedKey = "";

                        MIDIAssignmentList[index] = e.Key.ToString();
                        activeButtons[index] = false;
                        this.Dispatcher.Invoke(new dResetLearnButtonColors(resetLearnButtonColors), allLearnButtons[index]);
                    }
                    else
                    {
                        activeButtons[index] = false;
                        this.Dispatcher.Invoke(new dResetLearnButtonColors(resetLearnButtonColors), allLearnButtons[index]);
                    }
                    // TODO YES NO RESULTS
                }
            }
        }

        private void PlayWithKeyboard(object sender, KeyEventArgs e)
        {
            // Only play when no learn button is activated
            if (!activeButtons.Contains(true))
            {
                for (int i = 0; i < MIDIAssignmentList.Count; i++)
                {
                    if (e.Key.ToString() == MIDIAssignmentList[i])
                    {
                        HandleKeyBoardPlaying(i);
                    }
                }
            }
        }

        private void HandleKeyBoardPlaying(int index)
        {
            if (!dataPaths[index].Equals("") && !dataPaths[index].Equals(null))
            {
                SoundPlayer soundplayer = new SoundPlayer(soundLocation: dataPaths[index]);
                soundplayer.Play();
            }
            this.Dispatcher.Invoke(new dChangeColorOnPlay(ChangeColorOnPlay), index);
        }

        private delegate void dChangeColorOnPlay(int index);

        public async void ChangeColorOnPlay(int index)
        {
            sampleButtonList[index].Background = Brushes.MediumSlateBlue;
            //sampleButtonList[index].Foreground = Brushes.Black;
            //sampleButtonList[index].Opacity = 0.7;
            await Task.Delay(120);
            sampleButtonList[index].Background = Brushes.BlueViolet;
            //sampleButtonList[index].Foreground = Brushes.White;
            //sampleButtonList[index].Opacity = 1;
        }

        private void midiIn_MessageReceived(object sender, MidiInMessageEventArgs e)
        {
            // Just some logging
            Trace.WriteLine(String.Format("Time {0} Message 0x{1:X8} Event {2}",
                e.Timestamp, e.RawMessage, e.MidiEvent));

            if (e.MidiEvent != null && e.MidiEvent.CommandCode == MidiCommandCode.AutoSensing)
            {
                return;
            }

            // Toggle to prevent triggering a sample by releasing a MIDI-Controller key/pad - DOESN'T WORK SOMEHOW
            if (midiNoteState == false)
            {
                if (e.MidiEvent.CommandCode == MidiCommandCode.NoteOn)
                {
                    NoteOn(e, midiNoteState);
                    midiNoteState = true;
                }
            }
            else
            {
                midiNoteState = false;
            }
            MIDIState = midiNoteState.ToString();
        }

        private void NoteOn(MidiInMessageEventArgs e, bool midiNoteState)
        {

            ne = (NoteEvent)e.MidiEvent;
            NoteOnEvent no = new NoteOnEvent(0, 1, ne.NoteNumber, 127, 0);

            // Learn MIDI
            if (activeButtons.Contains(true))
            {
                for (int i = 0; i < activeButtons.Count; i++)
                {
                    if (activeButtons[i] == true)
                    {
                        if (!MIDIAssignmentList[i].Contains(no.NoteNumber.ToString()))
                        {
                            MIDIAssignmentList[i] = $"MIDI {no.NoteNumber.ToString()}";
                            activeButtons[i] = false;
                            this.Dispatcher.Invoke(new dResetLearnButtonColors(resetLearnButtonColors), allLearnButtons[i]);
                        }
                    }
                }
            }

            // Only play when no learn button is activated
            if (!activeButtons.Contains(true))
            {
                for (int i = 0; i < MIDIAssignmentList.Count; i++)
                {
                    if ($"MIDI {no.NoteNumber.ToString()}" == MIDIAssignmentList[i])
                    {
                        HandleKeyBoardPlaying(i);
                    }
                }
            }
        }

        private delegate void dResetLearnButtonColors(Button learnButton);

        private void resetLearnButtonColors(Button learnButton)
        {
            learnButton.Background = Brushes.BlueViolet;
            learnButton.Foreground = Brushes.White;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (!string.IsNullOrEmpty(propertyName))
            {
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            } 
        }

        private void SavePreset_Click(object sender, RoutedEventArgs e)
        {
            string fileName;
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "XML files (.xml)|*.xml";
            saveFileDialog.Title = "Save Preset";
            saveFileDialog.ShowDialog();

            if (saveFileDialog.FileName != "")
            {
                fileName = saveFileDialog.FileName;

                Data buttonPresets = new Data();
                buttonPresets.DataPaths = dataPaths;
                buttonPresets.KeyBindingList = MIDIAssignmentList;

                var sampleButtonContentList = new List<object>();
                for (int i = 0; i < sampleButtonList.Count; i++)
                {
                    sampleButtonContentList.Add(sampleButtonList[i].Content);
                }
                buttonPresets.ButtonContentList = sampleButtonContentList;

                XmlManager.XmlDataWriter(buttonPresets, fileName);
            }
        }

        private void LoadPreset_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog();
            openFileDialog.DefaultExt = ".xml";
            openFileDialog.Filter = "XML files (.xml)|*.xml";
            openFileDialog.Title = "Open Preset";
            bool? result = openFileDialog.ShowDialog();

            var fileName = openFileDialog.FileName;

            if (File.Exists(fileName))
            {
                Data buttonPresets = XmlManager.XmlDataReader(fileName);
                dataPaths = buttonPresets.DataPaths;
                MIDIAssignmentList = buttonPresets.KeyBindingList;

                for (int i = 0; i < sampleButtonList.Count; i++)
                {
                    sampleButtonList[i].Content = buttonPresets.ButtonContentList[i];
                    
                    // Change the look of the buttons to make it look "activated"
                    if (!sampleButtonList[i].Content.Equals("Drop .wav file here"))
                    {
                        sampleButtonList[i].Opacity = 1.0;
                        allLearnButtons[i].Opacity = 1.0;
                    }
                }
            }
        }

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            var window = Window.GetWindow(this);
            window.KeyDown += PlayWithKeyboard;
        }
    }
}
