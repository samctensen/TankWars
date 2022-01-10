// Class for handling inputs to send to the server
// Created Nov 2021 by Sam Christensen and Bryce Gillespie
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TankWars
{
    public class DrawingPanel : Panel
    {
        // Image declarations
        private Image wallSeg;
        private Image background;
        private Image blueTank;
        private Image blueTurret;
        private Image darkPurpleTank;
        private Image darkPurpleTurret;
        private Image greenTank;
        private Image greenTurret;
        private Image lightGreenTank;
        private Image lightGreenTurret;
        private Image orangeTank;
        private Image orangeTurret;
        private Image purpleTank;
        private Image purpleTurret;
        private Image redTank;
        private Image redTurret;
        private Image yellowTank;
        private Image yellowTurret;
        private Image blueShot;
        private Image darkPurpleShot;
        private Image greenShot;
        private Image lightGreenShot;
        private Image purpleShot;
        private Image redShot;
        private Image orangeShot;
        private Image yellowShot;
        private Image powerup;

        // Animation images
        private Bitmap beamImage;
        private Bitmap explosionImage;

        // Dictionaries to track active animations
        private Dictionary<int, BeamAnimation> beamAnimations;
        private Dictionary<int, ExplosionAnimation> explosionAnimations;

        // World declaration
        private World theWorld;

        // Nested class for drawing beam animations
        public class BeamAnimation
        {
            public Beam beam { get; set; } = null;
            public int frameCounter { get; set; } = 0;
            public Bitmap img { get; set; } = null;
            public bool currentlyAnimating { get; set; } = false;

            public BeamAnimation(Beam b, int frame)
            {
                beam = b;
                frameCounter = frame;
            }
        }

        // Nested class for drawing tank explosion animations
        public class ExplosionAnimation
        {
            public Tank tank { get; set; } = null;
            public int frameCounter { get; set; } = 0;
            public Bitmap img { get; set; } = new Bitmap(@"..\..\..\Resources\Images\explosion.gif");
            public bool currentlyAnimating { get; set; } = false;

            public ExplosionAnimation(Tank t, int frame)
            {
                tank = t;
                frameCounter = frame;
            }
        }
        public DrawingPanel(World w)
        {
            // Initial settings and load images
            DoubleBuffered = true;
            theWorld = w;
            beamAnimations = new Dictionary<int, BeamAnimation>();
            explosionAnimations = new Dictionary<int, ExplosionAnimation>();

            beamImage = new Bitmap(@"..\..\..\Resources\Images\beam-cropped.gif");
            explosionImage = new Bitmap(@"..\..\..\Resources\Images\explosion.gif");
            background = Image.FromFile(@"..\..\..\Resources\Images\Background.png");
            wallSeg = Image.FromFile(@"..\..\..\Resources\Images\WallSprite.png");
            blueTank = Image.FromFile(@"..\..\..\Resources\Images\BlueTank.png");
            blueTurret = Image.FromFile(@"..\..\..\Resources\Images\BlueTurret.png");
            darkPurpleTank = Image.FromFile(@"..\..\..\Resources\Images\DarkTank.png");
            darkPurpleTurret = Image.FromFile(@"..\..\..\Resources\Images\DarkTurret.png");
            greenTank = Image.FromFile(@"..\..\..\Resources\Images\GreenTank.png");
            greenTurret = Image.FromFile(@"..\..\..\Resources\Images\GreenTurret.png");
            lightGreenTank = Image.FromFile(@"..\..\..\Resources\Images\LightGreenTank.png");
            lightGreenTurret = Image.FromFile(@"..\..\..\Resources\Images\LightGreenTurret.png");
            orangeTank = Image.FromFile(@"..\..\..\Resources\Images\OrangeTank.png");
            orangeTurret = Image.FromFile(@"..\..\..\Resources\Images\OrangeTurret.png");
            purpleTank = Image.FromFile(@"..\..\..\Resources\Images\PurpleTank.png");
            purpleTurret = Image.FromFile(@"..\..\..\Resources\Images\PurpleTurret.png");
            redTank = Image.FromFile(@"..\..\..\Resources\Images\RedTank.png");
            redTurret = Image.FromFile(@"..\..\..\Resources\Images\RedTurret.png");
            yellowTank = Image.FromFile(@"..\..\..\Resources\Images\YellowTank.png");
            yellowTurret = Image.FromFile(@"..\..\..\Resources\Images\YellowTurret.png");
            blueShot = Image.FromFile(@"..\..\..\Resources\Images\shot_blue.png");
            darkPurpleShot = Image.FromFile(@"..\..\..\Resources\Images\shot-darkpurple.png");
            greenShot = Image.FromFile(@"..\..\..\Resources\Images\shot_green.png");
            lightGreenShot = Image.FromFile(@"..\..\..\Resources\Images\shot_lightgreen.png");
            purpleShot = Image.FromFile(@"..\..\..\Resources\Images\shot_purple.png");
            redShot = Image.FromFile(@"..\..\..\Resources\Images\shot_red.png");
            orangeShot = Image.FromFile(@"..\..\..\Resources\Images\shot-orange.png");
            yellowShot = Image.FromFile(@"..\..\..\Resources\Images\shot-yellow.png");
            powerup = Image.FromFile(@"..\..\..\Resources\Images\powerup.png");
        }

        // A delegate for DrawObjectWithTransform
        // Methods matching this delegate can draw whatever they want using e  
        public delegate void ObjectDrawer(object o, PaintEventArgs e);


        /// <summary>
        /// This method performs a translation and rotation to drawn an object in the world.
        /// </summary>
        /// <param name="e">PaintEventArgs to access the graphics (for drawing)</param>
        /// <param name="o">The object to draw</param>
        /// <param name="worldX">The X coordinate of the object in world space</param>
        /// <param name="worldY">The Y coordinate of the object in world space</param>
        /// <param name="angle">The orientation of the objec, measured in degrees clockwise from "up"</param>
        /// <param name="drawer">The drawer delegate. After the transformation is applied, the delegate is invoked to draw whatever it wants</param>
        private void DrawObjectWithTransform(PaintEventArgs e, object o, double worldX, double worldY, double angle, ObjectDrawer drawer)
        {
            // "push" the current transform
            System.Drawing.Drawing2D.Matrix oldMatrix = e.Graphics.Transform.Clone();

            e.Graphics.TranslateTransform((int)worldX, (int)worldY);
            e.Graphics.RotateTransform((float)angle);
            drawer(o, e);

            // "pop" the transform
            e.Graphics.Transform = oldMatrix;
        }

        /// <summary>
        /// Acts as a drawing delegate for DrawObjectWithTransform
        /// Draws Walls in the correct positions
        /// </summary>
        /// <param name="o">The object to draw</param>
        /// <param name="e">The PaintEventArgs to access the graphics</param>
        private void WallDrawer(object o, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            Wall wall = o as Wall;
            int width = 50;
            Rectangle r = new Rectangle(-(width / 2), -(width / 2), width, width);
            e.Graphics.DrawImage(wallSeg, r);
        }



        /// <summary>
        /// Acts as a drawing delegate for DrawObjectWithTransform
        /// Draws Tanks in the correct positions
        /// </summary>
        /// <param name="o">The object to draw</param>
        /// <param name="e">The PaintEventArgs to access the graphics</param>
        private void TankDrawer(object o, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            Tank tank = o as Tank;
            int width = 60;
            Rectangle r = new Rectangle(-(width / 2), -(width / 2), width, width);

            int team = tank.ID % 8;

            // Use image dependent on team
            switch (team)
            {
                case 0:
                    e.Graphics.DrawImage(blueTank, r);
                    break;
                case 1:
                    e.Graphics.DrawImage(darkPurpleTank, r);
                    break;
                case 2:
                    e.Graphics.DrawImage(greenTank, r);
                    break;
                case 3:
                    e.Graphics.DrawImage(lightGreenTank, r);
                    break;
                case 4:
                    e.Graphics.DrawImage(orangeTank, r);
                    break;
                case 5:
                    e.Graphics.DrawImage(purpleTank, r);
                    break;
                case 6:
                    e.Graphics.DrawImage(redTank, r);
                    break;
                case 7:
                    e.Graphics.DrawImage(yellowTank, r);
                    break;
            }
        }

        /// <summary>
        /// Acts as a drawing delegate for DrawObjectWithTransform
        /// Draws Turrets in the correct positions and with the correct orientation
        /// </summary>
        /// <param name="o">The object to draw</param>
        /// <param name="e">The PaintEventArgs to access the graphics</param>
        private void TurretDrawer(object o, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            Tank tank = o as Tank;
            int width = 60;
            Rectangle r = new Rectangle(-(width / 2), -(width / 2), width, width);

            int team = tank.ID % 8;

            // Use image dependent on team
            switch (team)
            {
                case 0:
                    e.Graphics.DrawImage(blueTurret, r);
                    break;
                case 1:
                    e.Graphics.DrawImage(darkPurpleTurret, r);
                    break;
                case 2:
                    e.Graphics.DrawImage(greenTurret, r);
                    break;
                case 3:
                    e.Graphics.DrawImage(lightGreenTurret, r);
                    break;
                case 4:
                    e.Graphics.DrawImage(orangeTurret, r);
                    break;
                case 5:
                    e.Graphics.DrawImage(purpleTurret, r);
                    break;
                case 6:
                    e.Graphics.DrawImage(redTurret, r);
                    break;
                case 7:
                    e.Graphics.DrawImage(yellowTurret, r);
                    break;
            }
        }

        /// <summary>
        /// Acts as a drawing delegate for DrawObjectWithTransform
        /// Draws Tank HUD (text and health bar) in the correct positions
        /// </summary>
        /// <param name="o">The object to draw</param>
        /// <param name="e">The PaintEventArgs to access the graphics</param>
        private void TankHUDDrawer(object o, PaintEventArgs e)
        {
            Tank tank = o as Tank;

            int width = tank.hitPoints * 20;
            int height = 4;
            Rectangle r = new Rectangle(-30, -40, width, height);

            // Draw rectangle representing health depending on tank hitPoint count
            if (tank.hitPoints == 3)
            {
                using (System.Drawing.SolidBrush greenBrush = new System.Drawing.SolidBrush(System.Drawing.Color.Green))
                {
                    e.Graphics.FillRectangle(greenBrush, r);
                }
            }
            else if (tank.hitPoints == 2)
            {
                using (System.Drawing.SolidBrush yellowBrush = new System.Drawing.SolidBrush(System.Drawing.Color.Yellow))
                {
                    e.Graphics.FillRectangle(yellowBrush, r);
                }
            }
            else if (tank.hitPoints == 1)
            {
                using (System.Drawing.SolidBrush redBrush = new System.Drawing.SolidBrush(System.Drawing.Color.Red))
                {
                    e.Graphics.FillRectangle(redBrush, r);
                }
            }

            // Font options for drawing player name and score
            Font drawFont = new Font("Arial", 13);
            SolidBrush drawBrush = new SolidBrush(Color.Black);
            PointF point = new PointF(0, 30);
            StringFormat drawFormat = new StringFormat();
            drawFormat.Alignment = StringAlignment.Center;
            e.Graphics.DrawString(tank.name + ": " + tank.score, drawFont, drawBrush, point, drawFormat);
        }

        /// <summary>
        /// Acts as a drawing delegate for DrawObjectWithTransform
        /// Draws Powerups in the correct positions
        /// </summary>
        /// <param name="o">The object to draw</param>
        /// <param name="e">The PaintEventArgs to access the graphics</param>
        private void PowerupDrawer(object o, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            Powerup p = o as Powerup;
            int width = 30;
            Rectangle r = new Rectangle(-(width / 2), -(width / 2), width, width);
            e.Graphics.DrawImage(powerup, r);
        }

        /// <summary>
        /// Acts as a drawing delegate for DrawObjectWithTransform
        /// Draws Projectiles in the correct positions
        /// </summary>
        /// <param name="o">The object to draw</param>
        /// <param name="e">The PaintEventArgs to access the graphics</param>
        private void ProjectileDrawer(object o, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            Projectile p = o as Projectile;
            int width = 30;
            Rectangle r = new Rectangle(-(width / 2), -(width / 2), width, width);

            int team = p.ownerID % 8;

            // Use image dependent on projectile owner
            switch (team)
            {
                case 0:
                    e.Graphics.DrawImage(blueShot, r);
                    break;
                case 1:
                    e.Graphics.DrawImage(darkPurpleShot, r);
                    break;
                case 2:
                    e.Graphics.DrawImage(greenShot, r);
                    break;
                case 3:
                    e.Graphics.DrawImage(lightGreenShot, r);
                    break;
                case 4:
                    e.Graphics.DrawImage(orangeShot, r);
                    break;
                case 5:
                    e.Graphics.DrawImage(purpleShot, r);
                    break;
                case 6:
                    e.Graphics.DrawImage(redShot, r);
                    break;
                case 7:
                    e.Graphics.DrawImage(yellowShot, r);
                    break;
            }
        }

        /// <summary>
        /// Acts as a drawing delegate for DrawObjectWithTransform
        /// Draws Beams in the correct positions
        /// </summary>
        /// <param name="o">The object to draw</param>
        /// <param name="e">The PaintEventArgs to access the graphics</param>
        private void BeamDrawer(object o, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            BeamAnimation b = o as BeamAnimation;

            int width = b.img.Width;
            int height = 60;

            Rectangle r = new Rectangle(10, -(height / 2), width, height);
            e.Graphics.DrawImage(b.img, r);
        }

        /// <summary>
        /// Acts as a drawing delegate for DrawObjectWithTransform
        /// Draws Explosion animations in the correct positions
        /// </summary>
        /// <param name="o">The object to draw</param>
        /// <param name="e">The PaintEventArgs to access the graphics</param>
        private void ExplosionDrawer(object o, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            ExplosionAnimation b = o as ExplosionAnimation;

            int width = 80;
            Rectangle r = new Rectangle(-(width / 2), -(width / 2), width, width);
            e.Graphics.DrawImage(b.img, r);
        }

        /// <summary>
        /// Empty method to simulate frame drawing, used in animation event handlers
        /// </summary>
        private void PanelTriggersOnFrame(object o, EventArgs e) { }

        // This method is invoked when the DrawingPanel needs to be re-drawn
        protected override void OnPaint(PaintEventArgs e)
        {
            //Don't paint if we haven't yet received world size from the server
            if (!theWorld.Tanks.ContainsKey(theWorld.player.ID))
            {
                return;
            }

            // Center the view on the middle of the world,
            // since the image and world use different coordinate systems
            double playerX = theWorld.player.location.GetX();
            double playerY = theWorld.player.location.GetY();
            int viewSize = Size.Width;
            e.Graphics.TranslateTransform((float)(-playerX + (viewSize / 2)), (float)(-playerY + (viewSize / 2)));

            // Stores animations to remove after iteration
            List<int> beamRemovals = new List<int>();
            List<int> explosionRemovals = new List<int>();

            lock (theWorld)
            {
                // Draw the background
                e.Graphics.DrawImage(background, (-1 * theWorld.size) / 2, (-1 * theWorld.size) / 2, theWorld.size, theWorld.size);

                // Draw the walls
                foreach (Wall wall in theWorld.Walls.Values)
                {
                    // Find if the wall is vertical or horizontal and draw in the correct direction
                    Vector2D diff = wall.p1 - wall.p2;

                    double yMax = Math.Max(wall.p1.GetY(), wall.p2.GetY());
                    double yMin = Math.Min(wall.p1.GetY(), wall.p2.GetY());
                    double xMax = Math.Max(wall.p1.GetX(), wall.p2.GetX());
                    double xMin = Math.Min(wall.p1.GetX(), wall.p2.GetX());

                    // If vertical
                    if (diff.GetX() == 0)
                    {
                        for (double y = yMin; y <= yMax; y += 50)
                        {
                            DrawObjectWithTransform(e, wall, wall.p1.GetX(), y, 0, WallDrawer);
                        }
                    }

                    // If horizontal
                    else
                    {
                        for (double x = xMin; x <= xMax; x += 50)
                        {
                            DrawObjectWithTransform(e, wall, x, wall.p1.GetY(), 0, WallDrawer);
                        }
                    }
                }

                // Draw the tanks, turrets and HUD
                foreach (Tank tank in theWorld.Tanks.Values)
                {
                    if (tank.hitPoints > 0)
                    {
                        DrawObjectWithTransform(e, tank, tank.location.GetX(), tank.location.GetY(), tank.orientation.ToAngle(), TankDrawer);
                        DrawObjectWithTransform(e, tank, tank.location.GetX(), tank.location.GetY(), tank.aiming.ToAngle(), TurretDrawer);
                        DrawObjectWithTransform(e, tank, tank.location.GetX(), tank.location.GetY(), 0, TankHUDDrawer);
                    }
                    if (tank.died && !explosionAnimations.ContainsKey(tank.ID))
                    {
                        ExplosionAnimation explode = new ExplosionAnimation(tank, 30);
                        explode.img = explosionImage;
                        explosionAnimations[tank.ID] = explode;
                    }
                }

                // Draw the explosion animations
                foreach (ExplosionAnimation explosion in explosionAnimations.Values)
                {
                    if (!explosion.currentlyAnimating)
                    {
                        ImageAnimator.Animate(explosion.img, new EventHandler(this.PanelTriggersOnFrame));
                        explosion.currentlyAnimating = true;
                    }
                    DrawObjectWithTransform(e, explosion, explosion.tank.location.GetX(), explosion.tank.location.GetY(), 0, ExplosionDrawer);

                    if (explosion.frameCounter > 0)
                    {
                        explosion.frameCounter--;
                        ImageAnimator.UpdateFrames(explosion.img);
                    }
                    else
                        explosionRemovals.Add(explosion.tank.ID);
                }

                // Draw the powerups
                foreach (Powerup pow in theWorld.Powerups.Values)
                {
                    DrawObjectWithTransform(e, pow, pow.location.GetX(), pow.location.GetY(), 0, PowerupDrawer);
                }

                // Draw the projectiles
                foreach (Projectile projectile in theWorld.Projectiles.Values)
                {
                    DrawObjectWithTransform(e, projectile, projectile.location.GetX(), projectile.location.GetY(), projectile.direction.ToAngle(), ProjectileDrawer);
                }

                // Add the beams to the dictionary of animations
                foreach (Beam beam in theWorld.Beams.Values)
                {
                    if (!beamAnimations.ContainsKey(beam.ID))
                    {
                        BeamAnimation animation = new BeamAnimation(beam, 15);
                        animation.img = beamImage;
                        beamAnimations[beam.ID] = animation;
                    }
                }

                // Iterate through beam animations, draw the gif in the correct location, and decrement the frame counter
                foreach (BeamAnimation b in beamAnimations.Values)
                {
                    if (!b.currentlyAnimating)
                    {
                        ImageAnimator.Animate(b.img, new EventHandler(this.PanelTriggersOnFrame));
                        b.currentlyAnimating = true;
                    }
                    b.beam.direction.Normalize();
                    DrawObjectWithTransform(e, b, b.beam.origin.GetX(), b.beam.origin.GetY(), b.beam.direction.ToAngle() - 90, BeamDrawer);

                    if (b.frameCounter > 0)
                    {
                        b.frameCounter--;
                        ImageAnimator.UpdateFrames(b.img);
                    }
                    else
                        beamRemovals.Add(b.beam.ID);
                }

                // Remove finished beam animations
                foreach (int i in beamRemovals)
                {
                    // Clean up the animation event and remove the beam from the world
                    ImageAnimator.StopAnimate(beamAnimations[i].img, this.PanelTriggersOnFrame);
                    beamAnimations.Remove(i);
                    theWorld.Beams.Remove(i);
                }

                // Remove finished explosion animations
                foreach (int i in explosionRemovals)
                {
                    // Clean up the animation event and remove the beam from the world
                    ImageAnimator.StopAnimate(explosionAnimations[i].img, this.PanelTriggersOnFrame);
                    explosionAnimations.Remove(i);
                }
            }

            theWorld.RemoveDeadObjects();
            // Do anything that Panel (from which we inherit) needs to do
            base.OnPaint(e);
        }
    }
}