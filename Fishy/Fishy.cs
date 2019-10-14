using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Fishy
{
    public partial class Fishy : Form
    {
        private FishyPlayer player1;

        public Fishy()
        {
            InitializeComponent();

            player1 = new FishyPlayer(
                this,
                new Point(200, 200),
                new Size(50, 30),
                Color.DarkCyan,
                0.5
            );
            this.Controls.Add(player1.ObjectControl);
        }

        private void WorldTimer_Tick(object sender, EventArgs e)
        {
            player1.Move();
        }

        private void KeyIsDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Left)
            {
                player1.XAcceleration -= player1.DeltaAcceleration;
            }

            if (e.KeyCode == Keys.Right)
            {
                player1.XAcceleration += player1.DeltaAcceleration;
            }

            if (e.KeyCode == Keys.Up)
            {
                player1.YAcceleration -= player1.DeltaAcceleration;
            }

            if (e.KeyCode == Keys.Down)
            {
                player1.YAcceleration += player1.DeltaAcceleration;
            }
        }

        private void KeyIsUp(object sender, KeyEventArgs e)
        {

        }
    }

    public class GameObject
    {
        // Properties

        public PictureBox ObjectControl { get; set; }
        public Form _form { get; set; }
        
        private int _x;
        private int _y;
        private int _width;
        private int _height;
        private double _xVelocity = 1;
        private double _yVelocity = 1;
        private double _xAcceleration = 0.1;
        private double _yAcceleration = 0.1;
        private double _deltaAcceleration;
        private int _score;

        public int X
        {
            get { return _x; }
            set
            {
                _x = value;
            }
        }

        public int Y
        {
            get { return _y; }
            set { _y = value; }
        }

        public int Width
        {
            get { return _width; }
            set { _width = value; }
        }

        public int Height
        {
            get { return _height; }
            set { _height = value; }
        }

        public double XVelocity
        {
            get { return _xVelocity; }
            set
            {
                if (value > 1.0 || value < -1.0)
                {
                    _xVelocity = value;
                }
            }
        }

        public double YVelocity
        {
            get { return _yVelocity; }
            set
            {
                if (value > 1.0 || value < -1.0)
                {
                    _yVelocity = value;
                }
            }
        }

        public double XAcceleration
        {
            get { return _xAcceleration; }
            set { _xAcceleration = value; }
        }

        public double YAcceleration
        {
            get { return _yAcceleration; }
            set { _yAcceleration = value; }
        }
        public double DeltaAcceleration
        {
            get { return _deltaAcceleration; }
            set { _deltaAcceleration = value; }
        }

        public int Score
        {
            get { return _score; }
            set { _score = value; }
        }

        // Methods

        public GameObject(Form form, Point point, Size size, Color color, double deltaAcceleration)
        {
            _form = form;

            ObjectControl = new PictureBox();
            ObjectControl.Location = point;
            ObjectControl.Size = size;
            ObjectControl.BackColor = color;

            DeltaAcceleration = deltaAcceleration;
        }

        public void Move()
        {
            double newX = X + (XVelocity * XAcceleration);
            double newY = Y + (YVelocity * YAcceleration);

            if (newX >= 0)
            {
                X = Convert.ToInt32(newX);
            }

            if (newY >= 0)
            {
                Y = Convert.ToInt32(newY);
            }

            // Move the picturebox
            ObjectControl.Location = new Point(X, Y);
        }
    }

    public class FishyPlayer : GameObject
    {
        public FishyPlayer(Form form, Point point, Size size, Color color, double deltaAcceleration) : base(form, point, size, color, deltaAcceleration)
        {
            _form = form;

            ObjectControl = new PictureBox();
            ObjectControl.Location = point;
            ObjectControl.Size = size;
            ObjectControl.BackColor = color;
            ObjectControl.Image = Image.FromFile("Images\\");

            DeltaAcceleration = deltaAcceleration;
        }
    }
}
