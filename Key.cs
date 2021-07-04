using System.Windows;
using System.Windows.Controls;

namespace Keyboard_layout_generator {
    class Key {

        public string defaultLegend;
        public string additionalLegend;

        public double Width { get; private set; }
        public double Height { get; private set; }

        public double X { get; private set; }
        public double Y { get; private set; }

        public Button uiButton;

        public Key(double x, double y, double width, double height) {
            this.X = x;
            this.Y = y;
            this.Width = width;
            this.Height = height;
        }

        public void Move(Point vector) {

            if (vector.X != 0) {
                //move left and right

                double temp = vector.X + X;

                if (temp > 0)
                    X = temp;
                else
                    X = 0;
            }

            if (vector.Y != 0) {
                //move down and up

                double temp = vector.Y + Y;

                if (temp > 0)
                    Y = temp;
                else
                    Y = 0;
            }
        }

        public void Resize(Point vector) {

            if (vector.X != 0) {
                //resize width

                double temp = vector.X + Width;

                if (temp > 1) 
                    Width = temp;
                else 
                    Width = 1;
            }

            if (vector.Y != 0) {
                //resize height

                double temp = vector.Y + Height;

                if (temp > 1)
                    Height = temp;
                else
                    Height = 1;
            }
        }
    }
}
