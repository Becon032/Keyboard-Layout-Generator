using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

namespace Keyboard_layout_generator {

    public partial class MainWindow : Window {

        readonly int keyPixelSize = 50;

        Key selectedKey;
        bool isMoveModeEnabled = true;
        List<Key> keys = new List<Key>();


        public MainWindow() {
            InitializeComponent();

            //Culture for parsing "." instead "," in raw data when parsing double to string
            System.Globalization.CultureInfo customCulture = (System.Globalization.CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
            customCulture.NumberFormat.NumberDecimalSeparator = ".";

            System.Threading.Thread.CurrentThread.CurrentCulture = customCulture;
        }

        

        #region Key selection

        private void UiKey_Click(object sender, RoutedEventArgs e) {
            if (selectedKey == e.OriginalSource)
                return;

            foreach (var key in keys) {
                if ((Button)e.OriginalSource == key.uiButton) {
                    SelectKey(key);
                }
            }
        }
        private void KeyboardCanvas_Click(object sender, MouseButtonEventArgs e) {
            SelectKey(null);
        }
        private void SelectKey(Key key) {
            //unselects key before
            if(selectedKey != null)
                selectedKey.uiButton.BorderBrush = Brushes.Gray;
            selectedKey = null;
            keyLegendTextBoxTop.IsEnabled = false;
            keyLegendTextBoxTop.Text = null;
            keyLegendTextBoxBottom.IsEnabled = false;
            keyLegendTextBoxBottom.Text = null;

            if (key == null)
                return;

            //selects new key
            selectedKey = key;
            keyLegendTextBoxTop.IsEnabled = true;
            keyLegendTextBoxTop.Text = key.defaultLegend;
            keyLegendTextBoxBottom.IsEnabled = true;
            keyLegendTextBoxBottom.Text = key.additionalLegend;
            key.uiButton.BorderBrush = Brushes.Red;
            
        }

        #endregion

        #region Key Modification

        private void UpdateCanvas() {
            foreach (Key key in keys) {
                UpdateCanvas(key);
            }
        }
        private void UpdateCanvas(Key key) {

            Canvas.SetLeft(key.uiButton, key.X * keyPixelSize);
            Canvas.SetTop(key.uiButton, key.Y * keyPixelSize);

            key.uiButton.Width = key.Width * keyPixelSize;
            key.uiButton.Height = key.Height * keyPixelSize;

            if (key.additionalLegend == "" || key.additionalLegend == null)
                key.uiButton.Content = key.defaultLegend;
            else
                key.uiButton.Content = key.additionalLegend + "\n" + key.defaultLegend;

        }

        private void Nav_Click(object sender, RoutedEventArgs e) {
            if (selectedKey == null)
                return;

            Point vector = new Point(0, 0);

            switch ((e.Source as Button).Name.ToString()) {
                case "up1u": vector.Y = -1; break;
                case "up025u": vector.Y = -0.25; break;
                case "down1u": vector.Y = 1; break;
                case "down025u": vector.Y = 0.25; break;
                case "left1u": vector.X = -1; break;
                case "left025u": vector.X = -0.25; break;
                case "right1u": vector.X = 1; break;
                case "right025u": vector.X = 0.25; break;
                default: return;
            }

            if (isMoveModeEnabled)
                selectedKey.Move(vector);
            else
                selectedKey.Resize(vector);

            UpdateCanvas(selectedKey);

        }
        private void Add_Click(object sender, RoutedEventArgs e) {
            Key key = NewKey(0, 0, 1, 1, null, null);

            SelectKey(key);

            UpdateCanvas(key);
        }
        private Key NewKey(double x, double y, double width, double height, string defaultLegend, string additionalLegend) {
            Key key = new Key(x, y, width, height);

            Button uiKey = new Button() { HorizontalContentAlignment = HorizontalAlignment.Left };
            uiKey.Click += UiKey_Click;

            keyboardCanvas.Children.Add(uiKey);

            key.uiButton = uiKey;

            key.defaultLegend = defaultLegend;
            key.additionalLegend = additionalLegend;

            keys.Add(key);

            return key;
        }
        private void Del_Click(object sender, RoutedEventArgs e) {
            if (selectedKey != null) {
                keyboardCanvas.Children.Remove(selectedKey.uiButton);
                keys.Remove(selectedKey);
                SelectKey(null);
            }
        }
        private void DeleteAllButtons() {
            SelectKey(null);
            keyboardCanvas.Children.Clear();
            keys.Clear();
        }
        private void MoveOrResize_Click(object sender, RoutedEventArgs e) {
            isMoveModeEnabled = !isMoveModeEnabled;
            move.IsEnabled = !move.IsEnabled;
            resize.IsEnabled = !resize.IsEnabled;
        }
        private void Key_Legend_Changed(object sender, KeyEventArgs e) {
            selectedKey.defaultLegend = keyLegendTextBoxTop.Text;
            selectedKey.additionalLegend = keyLegendTextBoxBottom.Text;
            UpdateCanvas(selectedKey);

        }
        private void Preset_Click(object sender, RoutedEventArgs e) {

            //unfinished
            switch((e.Source as Button).Name.ToString()) {
                case "JD40": break;
            }
        }
        #endregion

        #region Raw Data

        private int CompareKeysByCoordinates(Key x, Key y) {

            if (x.Y > y.Y) return 1;          //Compare by Y
            else if (x.Y < y.Y) return -1;    //
            else if (x.X > y.X) return 1;     //Compare by X when Y are equal
            else if (x.X < y.X) return -1;    //
            else return 0;                    //Return 0 when X and Y are equal

        }
        private void CreateRawData() {
            keys.Sort(CompareKeysByCoordinates);
            output.Text = "";

            double previousY = -1;
            double previousX = 0;
            double previousWidth = 0;

            Queue<string> CurrentRow = new Queue<string>();
            string currentRow = "";
            Queue<string> Rows = new Queue<string>();

            for (int i = 0; i <= keys.Count; i++) {

                if (keys.Count == i || keys[i].Y != previousY) {
                    if (CurrentRow.Count > 0) {
                        currentRow += "[";
                        while (CurrentRow.Count > 0) {
                            currentRow += CurrentRow.Dequeue();

                            if (CurrentRow.Count > 0)
                                currentRow += ",";
                        }
                        currentRow += "]";

                        Rows.Enqueue(currentRow);
                        currentRow = "";
                        CurrentRow.Clear();
                        previousX = 0;
                        previousWidth = 0;
                    }
                    if (keys.Count == i)
                        break;
                }

                Queue<string> Modifiers = new Queue<string>();
                string modifiers = "";

                if (keys[i].Y != previousY + 1 && !(keys[i].Y == previousY))
                    Modifiers.Enqueue("y:" + (keys[i].Y - (previousY + 1)).ToString());

                if (keys[i].X != previousX + previousWidth)
                    Modifiers.Enqueue("x:" + (keys[i].X - (previousX + previousWidth)).ToString());

                if (keys[i].Width != 1)
                    Modifiers.Enqueue("w:" + keys[i].Width.ToString());

                if (keys[i].Height != 1)
                    Modifiers.Enqueue("h:" + keys[i].Height.ToString());

                if (Modifiers.Count > 0) {
                    modifiers += "{";
                    while (Modifiers.Count > 0) {
                        modifiers += Modifiers.Dequeue();

                        if (Modifiers.Count > 0)
                            modifiers += ",";
                    }
                    modifiers += "},";
                }

                if (keys[i].additionalLegend != null && keys[i].additionalLegend != "")
                    CurrentRow.Enqueue(modifiers + "\"" + keys[i].additionalLegend + "\\n" + keys[i].defaultLegend + "\"");
                else
                    CurrentRow.Enqueue(modifiers + "\"" + keys[i].defaultLegend + "\"");

                previousY = keys[i].Y;
                previousX = keys[i].X;
                previousWidth = keys[i].Width;
            }

            while (Rows.Count > 0) {
                output.Text += Rows.Dequeue();

                if (Rows.Count > 0)
                    output.Text += ",\n";
            }
        }
        private void TryCompileFromRaw() {
            string textOutput = output.Text;
            char? currentlyReadedMode = null;

            //variables for key creation 
            string defaultLegend = null;
            string aditionalLegend = null;
            string tempLegend = "";

            //variables for modifiers for next new key
            double xOffset = 0;
            double yOffset = 0;
            double width = 1;
            double height = 1;

            //variables of last created key button propeties
            double lastX = 0;
            double lastY = 0;
            double lastWidth = 1;

            DeleteAllButtons();

            if (textOutput.Length > 0)
                if (textOutput[0] != '[') {
                    return;
                    //wrong syntax at 0
                }

            for (int i = 1; i <= textOutput.Length; i++) {

                if (currentlyReadedMode == null) {
                    //check if there is need to change reading mode
                    switch (textOutput[i]) {
                        case '"': currentlyReadedMode = textOutput[i]; break;
                        case '{': currentlyReadedMode = textOutput[i]; break;
                        case ']': currentlyReadedMode = textOutput[i]; break;
                        //default: return;
                    }

                } else if (currentlyReadedMode == '"') {

                    if (textOutput[i] != '"') {
                        //scan for key legends inside " "

                        if (textOutput[i] == '\\') {
                            //skips \ char and reads special char, if it is not last char
                            if (textOutput.Length < i)
                                return;
                            i++;

                            if (textOutput[i] == 'n') {
                                //n - aditional legend is present in key.
                                defaultLegend = tempLegend;
                                tempLegend = "";

                            } else if (false) {
                                //to do funcionality of different chars
                            } else {
                                //break if special char after \ is not special
                            }
                            continue;
                        }

                        tempLegend += textOutput[i];
                        //finally add char to temp legend
                    } else {
                        //if on ending "
                        if (defaultLegend == null)
                            defaultLegend = tempLegend;
                        else
                            aditionalLegend = tempLegend;

                        //finally add new key
                        NewKey(xOffset + lastX, yOffset + lastY, width, height, defaultLegend, aditionalLegend);



                        lastWidth = width;
                        lastX += xOffset + lastWidth;
                        lastY += yOffset;

                        xOffset = 0;
                        yOffset = 0;
                        width = 1;
                        height = 1;

                        defaultLegend = null;
                        aditionalLegend = null;
                        tempLegend = "";

                        currentlyReadedMode = null;
                    }

                } else if (currentlyReadedMode == '{') {

                    //special modifiers before new key
                    if (textOutput[i] != '}') {
                        //scan for modifiers inside { }

                        char lastSpecialChar;
                        string number = "";

                        lastSpecialChar = textOutput[i];

                        if (++i < textOutput.Length)
                            if (textOutput[i] != ':')
                                return;

                        while (++i < textOutput.Length) {

                            if (textOutput[i] == ',' || textOutput[i] == '}') {
                                double temp = double.Parse(number); //to do: add tryparse
                                switch (lastSpecialChar) {
                                    case 'w': width = temp; break;
                                    case 'h': height = temp; break;
                                    case 'x': xOffset = temp; break;
                                    case 'y': yOffset = temp; break;
                                    default: return;
                                }
                                if (textOutput[i] == '}')
                                    currentlyReadedMode = null;

                                break;
                            }

                            number += textOutput[i];

                        }
                    }

                } else if (currentlyReadedMode == ']') {

                    if (i >= textOutput.Length) {
                        UpdateCanvas();
                        return;
                    }

                    if (textOutput[i] == '[') {
                        //new line

                        lastX = 0;
                        lastY++;
                        lastWidth = 1;

                        currentlyReadedMode = null;
                    }
                }
            }
        }

        private void Raw_Click(object sender, RoutedEventArgs e) {

            if ((e.Source as Button).Name.ToString() == "RawFromButtons")
                CreateRawData();
            else if ((e.Source as Button).Name.ToString() == "ButtonsFromRaw")
                TryCompileFromRaw();
        }

        #endregion



    }
}
