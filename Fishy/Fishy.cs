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

            this.BackgroundImage = Properties.Resources.background;

            player1 = new FishyPlayer(this);
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
                if (player1.XAcceleration > 0)
                {
                    player1.XAcceleration = player1.XAcceleration / 2;
                }
                player1.XAcceleration -= player1.DeltaAcceleration;
            }

            if (e.KeyCode == Keys.Right)
            {
                if (player1.XAcceleration < 0)
                {
                    player1.XAcceleration = player1.XAcceleration / 2;
                }
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
            player1.XAcceleration = 0;
            player1.YAcceleration = 0;
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
        private double _friction;
        private double _xVelocity;
        private double _yVelocity;
        private double _velocityLimit;
        private double _xAcceleration;
        private double _yAcceleration;
        private double _deltaAcceleration;
        private bool _flippedLeft;

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

        public double Friction
        {
            get { return _friction; }
            set { _friction = value; }
        }

        public double XVelocity
        {
            get { return _xVelocity; }
            set
            {
                if (Math.Abs(value) < VelocityLimit)
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
                if (Math.Abs(value) < VelocityLimit)
                {
                    _yVelocity = value;
                }
            }
        }

        public double VelocityLimit
        {
            get { return _velocityLimit; }
            set { _velocityLimit = value; }
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

        public bool FlippedLeft
        {
            get { return _flippedLeft; }
            set { _flippedLeft = value; }
        }

        public int Score
        {
            get { return _score; }
            set { _score = value; }
        }

        // Methods

        public void Move()
        {
            XVelocity += XAcceleration;
            YVelocity += YAcceleration;

            if (XAcceleration == 0)
            {
                XVelocity *= Friction;
            }

            if (YAcceleration == 0)
            {
                YVelocity *= Friction;
            }

            if (XVelocity < 0 && !FlippedLeft)
            {
                ObjectControl.Image.RotateFlip(RotateFlipType.RotateNoneFlipX);
                FlippedLeft = true;
            }
            else if (XVelocity > 0 && FlippedLeft)
            {
                ObjectControl.Image.RotateFlip(RotateFlipType.RotateNoneFlipX);
                FlippedLeft = false;
            }

            double newX = X + XVelocity;
            double newY = Y + YVelocity;
            
            // Check boundaries
            if (newX >= 0 && newX < (_form.Width - Width))
            {
                X = Convert.ToInt32(newX);
            }

            if (newY >= 0 && newY < (_form.Height - Height))
            {
                Y = Convert.ToInt32(newY);
            }

            // Move the picturebox
            ObjectControl.Location = new Point(X, Y);
        }
    }

    public class FishyPlayer : GameObject
    {
        public FishyPlayer(Form form)
        {
            _form = form;

            ObjectControl = new PictureBox
            {
                Size = new Size(50, 50),
                Location = new Point((_form.Width / 2) - (Width / 2), (_form.Height / 2) - (Height / 2)),
                Image = Properties.Resources.playerfish_raw,
                BackColor = Color.Transparent,
                SizeMode = PictureBoxSizeMode.Zoom
            };

            DeltaAcceleration = 0.9;
            Friction = 0.97;
            VelocityLimit = 3.5;
        }
    }
}
