using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Fishy
{
    public partial class Fishy : Form
    {
        private System.Windows.Forms.Timer GameTimer;
        private Bitmap Backbuffer;
        private System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();

        private long tick = 0;

        private FishyPlayer player1;
        private List<RandomFishy> fishList = new List<RandomFishy>();
        private List<RandomFishy> eatenFishList = new List<RandomFishy>();

        private List<Crab> crabList = new List<Crab>();
        private List<Crab> eatenCrabList = new List<Crab>();

        private bool gameOver = false;
        private bool pause = false;
        private bool midGame = false;
        private bool lateGame = false;
        private int addFishEveryNTick = 200;
        private int addCrabEveryNTick = 400;
        private Font fontXS = new Font("Trebuchet MS", 24);
        private Font fontS = new Font("Trebuchet MS", 36);
        private Font fontM = new Font("Trebuchet MS", 48);
        private Font fontL = new Font("Trebuchet MS", 72);
        private Font fontXL = new Font("Trebuchet MS", 108);

        private WMPLib.WindowsMediaPlayer bgMusic = new WMPLib.WindowsMediaPlayer();
        //private SoundPlayer backgroundSound = new SoundPlayer(Properties.Resources.Retro_style_synth_music_loop);
        private SoundPlayer eatSound = new SoundPlayer(Properties.Resources.comical_bite);
        private SoundPlayer deadSound = new SoundPlayer(Properties.Resources.oh_no);

        private Rectangle stage;

        public Fishy()
        {
            // Init
            components = new System.ComponentModel.Container();
            GameTimer = new System.Windows.Forms.Timer(this.components);
            SuspendLayout();

            SetStyle(
            ControlStyles.UserPaint |
            ControlStyles.AllPaintingInWmPaint |
            ControlStyles.DoubleBuffer, true);

            // Timer
            GameTimer = new Timer
            {
                Enabled = true,
                Interval = 20
            };
            GameTimer.Tick += new System.EventHandler(GameTimer_Tick);
            
            // Form properties
            AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(800, 600);
            Name = "Fishy";
            Text = "Fishy!";
            Icon = Properties.Resources.FishyIcon1;
            ResumeLayout(false);
            BackgroundImage = Properties.Resources.background;

            stage = new Rectangle(-500, -500, this.Width + 1000, this.Height + 1000);

            // Events
            ResizeEnd += new EventHandler(FormCreateBackBuffer);
            Load += new EventHandler(FormCreateBackBuffer);
            Paint += new PaintEventHandler(FormPaint);
            KeyDown += new KeyEventHandler(KeyIsDown);
            KeyUp += new KeyEventHandler(KeyIsUp);

            // Add player
            player1 = new FishyPlayer(this);
            
            watch.Start();

            String URi = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "background.wav");
            //bgMusic.URL = URi;
            WMPLib.IWMPPlaylist playlist = bgMusic.playlistCollection.newPlaylist("Fishy");
            for (var i = 0;i < 10; i++)
            {
                playlist.appendItem(bgMusic.newMedia(URi));
            }
            bgMusic.currentPlaylist = playlist;
            bgMusic.controls.play();
        }

        private void FormCreateBackBuffer(object sender, EventArgs e)
        {
            if (Backbuffer != null)
            {
                Backbuffer.Dispose();
            }

            Backbuffer = new Bitmap(ClientSize.Width, ClientSize.Height);
        }

        private void FormPaint(object sender, PaintEventArgs e)
        {
            if (Backbuffer != null)
            {
                e.Graphics.DrawImageUnscaled(Backbuffer, Point.Empty);
            }
        }

        private void GameTimer_Tick(object sender, EventArgs e)
        {
            if (!pause && !gameOver)
            {
                if ((tick % addFishEveryNTick) == 0)
                {
                    fishList.Add(new RandomFishy(this, midGame, lateGame));
                    if (addFishEveryNTick > 50)
                    {
                        addFishEveryNTick -= 5;
                    }

                    if ((addFishEveryNTick % 10) == 0)
                    {
                        fishList.Add(new RandomFishy(this, midGame, lateGame));
                    }

                    midGame = (addFishEveryNTick < 150) ? true : false;
                    lateGame = (addFishEveryNTick < 90) ? true : false;
                }

                if ((tick % addCrabEveryNTick) == 0)
                { 
                    crabList.Add(new Crab(this, midGame, lateGame));
                }

                player1.Move();

                foreach (RandomFishy fish in fishList)
                {
                    fish.Move();
                }

                foreach (Crab crab in crabList)
                {
                    crab.Move();
                }

                CheckCollision();
                CheckOutOfBound();
            }

            Draw();
            tick++;
        }

        private void CheckCollision()
        {
            eatenFishList.Clear();
            eatenCrabList.Clear();

            foreach (RandomFishy fish in fishList)
            {
                if (player1.Bounds.IntersectsWith(fish.Bounds))
                {
                    if (player1.Score >= fish.Score)
                    {
                        eatSound.Play();

                        var diffScore = (int)Math.Ceiling(((decimal)fish.Score) / 10);
                        player1.Score += (diffScore < 2) ? 2 : diffScore;
                        eatenFishList.Add(fish);
                    }
                    else
                    {
                        deadSound.Play();
                        gameOver = true;
                        player1.Img = Properties.Resources.fish_skeleton_2;
                        if (player1.FlippedLeft)
                        {
                            player1.Img.RotateFlip(RotateFlipType.RotateNoneFlipX);
                        }
                    }
                }
            }

            foreach (Crab crab in crabList)
            {
                if (player1.Bounds.IntersectsWith(crab.Bounds))
                {
                    if (player1.Score >= crab.Score)
                    {
                        eatSound.Play();

                        var diffScore = (int)Math.Ceiling(((decimal)crab.Score) / 5);
                        player1.Score += (diffScore < 2) ? 2 : diffScore;
                        eatenCrabList.Add(crab);
                    }
                    else
                    {
                        deadSound.Play();
                        gameOver = true;
                        player1.Img = Properties.Resources.fish_skeleton_1;
                    }
                }
            }

            foreach (RandomFishy fish in eatenFishList)
            {
                fishList.Remove(fish);
            }

            foreach (Crab crab in eatenCrabList)
            {
                crabList.Remove(crab);
            }
        }

        private void CheckOutOfBound()
        {
            eatenFishList.Clear();
            eatenCrabList.Clear();

            foreach (RandomFishy fish in fishList)
            {
                if (!stage.IntersectsWith(fish.Bounds))
                {
                    eatenFishList.Add(fish);
                }
            }

            foreach (Crab crab in crabList)
            {
                if (!stage.IntersectsWith(crab.Bounds))
                {
                    eatenCrabList.Add(crab);
                }
            }

            foreach (RandomFishy fish in eatenFishList)
            {
                fishList.Remove(fish);
            }

            foreach (Crab crab in eatenCrabList)
            {
                crabList.Remove(crab);
            }
        }

        private void Draw()
        {
            if (Backbuffer != null)
            {
                using (var g = Graphics.FromImage(Backbuffer))
                {
                    g.Clear(Color.Transparent);
                    g.TextRenderingHint = TextRenderingHint.AntiAlias;

                    g.DrawImage(player1.Img, player1.Bounds);

                    foreach (RandomFishy fish in fishList)
                    {
                        g.DrawImage(fish.Img, fish.Bounds);
                        //g.DrawString(fish.Score + "", new Font("Segoe UI", 16), Brushes.White, new Point(fish.Bounds.X, fish.Bounds.Y));
                    }

                    foreach (Crab crab in crabList)
                    {
                        g.DrawImage(crab.Img, crab.Bounds);
                        //g.DrawString(crab.Score + "", new Font("Segoe UI", 16), Brushes.White, new Point(crab.Bounds.X, crab.Bounds.Y));
                    }

                    g.DrawString("Score: " + player1.Score, fontXS, Brushes.White, new PointF(20, 20));

                    if (pause)
                    {
                        g.FillRectangle(new SolidBrush(Color.FromArgb(128, 0, 0, 0)), 0, 0, this.Width, this.Height);
                        g.DrawString("Pause", fontM, Brushes.DarkOrange, new PointF((this.Width / 2) - 100, 150));
                        g.DrawString("Press 'P' to resume", fontXS, Brushes.White, new PointF((this.Width / 2) - 150, 240));
                    }
                    else if (gameOver)
                    {
                        g.FillRectangle(new SolidBrush(Color.FromArgb(128, 0, 0, 0)), 0, 0, this.Width, this.Height);
                        g.DrawString("Game Over", fontL, Brushes.Red, new PointF((this.Width / 2) - 260, 150));
                        g.DrawString("Press 'R' to restart", fontXS, Brushes.White, new PointF((this.Width / 2) - 150, 270));
                    }
                }

                Invalidate();
            }
        }

        private void Restart()
        {
            gameOver = false;
            pause = false;
            midGame = false;
            lateGame = false;
            player1 = new FishyPlayer(this);
            fishList.Clear();
            crabList.Clear();
            watch.Restart();
            GameTimer = new Timer
            {
                Enabled = true,
                Interval = 20
            };
            GameTimer.Tick += new System.EventHandler(GameTimer_Tick);
            tick = 0;
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
            if (e.KeyCode == Keys.Down || e.KeyCode == Keys.Up || e.KeyCode == Keys.Left || e.KeyCode == Keys.Right)
            {
                player1.XAcceleration = 0;
                player1.YAcceleration = 0;
            }

            if (e.KeyCode == Keys.P)
            {
                pause = !pause;
            }

            if (gameOver && e.KeyCode == Keys.R)
            {
                Restart();
            }
        }
    }

    public class GameObject
    {
        // Properties

        public Form _form { get; set; }
        private Bitmap _img;
        public int X;
        public int Y;
        public int Width;
        public int Height;
        public double Friction;
        private double _xVelocity;
        private double _yVelocity;
        public double MinVelocityLimit;
        public double MaxVelocityLimit;
        public double XAcceleration;
        public double YAcceleration;
        public double DeltaAcceleration;
        public bool FlippedLeft;
        public Rectangle Bounds;
        private int _score;
        public double WidthHeightRatio = 1;

        // Property methods

        public Bitmap Img
        {
            get { return _img; }
            set
            {
                _img = value;
                WidthHeightRatio = Convert.ToDouble(Img.Height) / Convert.ToDouble(Img.Width);
            }
        }

        public double XVelocity
        {
            get { return _xVelocity; }
            set
            {
                if (Math.Abs(value) < MaxVelocityLimit)
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
                if (Math.Abs(value) < MaxVelocityLimit)
                {
                    _yVelocity = value;
                }
            }
        }

        public int Score
        {
            get { return _score; }
            set
            {
                Width = 10 + Convert.ToInt32(Convert.ToDouble(value) * 1.5);
                Height = 10 + Convert.ToInt32(Convert.ToDouble(value) * 1.5 * WidthHeightRatio);
                _score = value; 
            }
        }
    }

    public class FishyPlayer : GameObject
    {
        public FishyPlayer(Form form)
        {
            _form = form;

            Score = 10;

            X = (_form.Width / 2) - (Width / 2);
            Y = (_form.Height / 2) - (Height / 2);

            Bounds = new Rectangle(X, Y, Width, Height);

            Img = Properties.Resources.playerfish_raw;

            DeltaAcceleration = 0.9; // Added to the velocity per tick, when accelerating
            Friction = 0.97; // When not accelerating, reduce velocity per tick by a multiplication of this value
            MinVelocityLimit = 1.0; // Speed limit (per tick)
            MaxVelocityLimit = 3.5; // Speed limit (per tick)
        }

        public void Move()
        {

            XVelocity += XAcceleration;
            YVelocity += YAcceleration;

            if (XAcceleration == 0 && Math.Abs(XVelocity) > MinVelocityLimit)
            {
                XVelocity *= Friction;
            }

            if (YAcceleration == 0 && Math.Abs(YVelocity) > MinVelocityLimit)
            {
                YVelocity *= Friction;
            }

            if (XVelocity < 0 && !FlippedLeft)
            {
                FlippedLeft = true;
                Img.RotateFlip(RotateFlipType.RotateNoneFlipX);
            }
            else if (XVelocity > 0 && FlippedLeft)
            {
                FlippedLeft = false;
                Img.RotateFlip(RotateFlipType.RotateNoneFlipX);
            }

            int newX = Convert.ToInt32(X + XVelocity);
            int newY = Convert.ToInt32(Y + YVelocity);

            // Check boundaries
            if (newX >= 10 && newX < (_form.Width - Width - 10))
            {
                X = newX;
                Bounds = new Rectangle(newX, Y, Width, Height);
            }

            if (newY >= 10 && newY < (_form.Height - Height - 10))
            {
                Y = newY;
                Bounds = new Rectangle(X, newY, Width, Height);
            }
        }
    }

    public class RandomFishy : GameObject
    {
        public RandomFishy(Form form, bool midGame, bool lateGame)
        {
            _form = form;

            Random rnd = new Random();

            List<Bitmap> smallFish = new List<Bitmap>() {
                Properties.Resources.fish_1, Properties.Resources.fish_2, Properties.Resources.fish_3, Properties.Resources.fish_4, Properties.Resources.piranha
            };

            List<Bitmap> bigFish = new List<Bitmap>() {
                Properties.Resources.shark, Properties.Resources.whale, Properties.Resources.dolphin
            };

            // Choosing a size
            int sizeSeed = rnd.Next(0, 1000);
            if (sizeSeed < 300)
            {
                Img = smallFish[rnd.Next(0, smallFish.Count)];
                Score = rnd.Next(0, 10);
            }
            else if (sizeSeed < 600 && midGame)
            {
                Img = smallFish[rnd.Next(0, smallFish.Count)];
                Score = rnd.Next(10, 50);
            }
            else if (sizeSeed < 800 && (midGame || lateGame))
            {
                Img = smallFish[rnd.Next(0, smallFish.Count)];
                Score = rnd.Next(50, 100);
            }
            else if (sizeSeed < 950 && lateGame)
            {
                Img = bigFish[rnd.Next(0, bigFish.Count)];
                Score = rnd.Next(100, 150);
            }
            else if (sizeSeed < 1000 && lateGame)
            {
                Img = bigFish[rnd.Next(0, bigFish.Count)];
                Score = rnd.Next(150, 250);
            }
            else
            {
                Img = smallFish[rnd.Next(0, smallFish.Count)];
                Score = rnd.Next(10, 40);
            }

            MinVelocityLimit = -5;
            MaxVelocityLimit = 5;

            Y = rnd.Next(0, _form.Height - Height - 200);

            int startPosition = rnd.Next(1, 3);
            switch (startPosition)
            {
                case 1: // Start left, swim right
                    X = -100;
                    XVelocity = rnd.Next(1, 4);
                    break;
                case 2: // Start right, swim left
                    X = _form.Width + 100;
                    XVelocity = rnd.Next(1, 4) * -1;
                    Img.RotateFlip(RotateFlipType.RotateNoneFlipX);
                    break;
            }

            Bounds = new Rectangle(X, Y, Width, Height);
        }

        public void Move()
        {
            X += Convert.ToInt32(XVelocity);
            Bounds = new Rectangle(X, Y, Width, Height);
        }
    }

    public class Crab : GameObject
    {
        public Crab(Form form, bool midGame, bool lateGame)
        {
            _form = form;

            Random rnd = new Random();

            Img = Properties.Resources.crab;

            MinVelocityLimit = -8;
            MaxVelocityLimit = 8;

            if (lateGame)
            {
                Score = rnd.Next(80, 120);
                XVelocity = 3;
            }
            else if (midGame)
            {
                Score = rnd.Next(40, 80);
                XVelocity = 2;
            }
            else
            {
                Score = rnd.Next(10, 40);
                XVelocity = 1;
            }

            X = 1;
            Y = _form.Height - Height - 10;

            Bounds = new Rectangle(X, Y, Width, Height);
        }

        public void Move()
        {
            int newX = X + Convert.ToInt32(XVelocity);
            if (newX > (_form.Width + 100))
            {
                XVelocity *= -1;
                X -= 10;
            }
            else if (newX < -100)
            {
                XVelocity *= -1;
                X += 10;
            }
            else
            {
                X += Convert.ToInt32(XVelocity);
            }

            Bounds = new Rectangle(X, Y, Width, Height);
        }
    }
}
