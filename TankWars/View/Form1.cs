// Main form consisting of input controls and a DrawingPanel
// Created Nov 2021 by Sam Christensen and Bryce Gillespie
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TankWars;

namespace TankWars
{
    public partial class TankWars : Form
    {
        private GameController controller;
        private DrawingPanel drawingPanel;
        private const int viewSize = 900;
        private const int menuSize = 40;

        public TankWars(GameController controller)
        {
            InitializeComponent();

            ClientSize = new Size(viewSize, viewSize + menuSize);

            // Set up the drawing panel using the world provided by the controller
            drawingPanel = new DrawingPanel(controller.theWorld);
            drawingPanel = new DrawingPanel(controller.theWorld);
            drawingPanel.Location = new Point(0, menuSize);
            drawingPanel.Size = new Size(viewSize, viewSize);
            drawingPanel.MouseDown += HandleMouseDown;
            drawingPanel.MouseUp += HandleMouseUp;
            drawingPanel.MouseMove += HandleMouseMove;

            // Listeners and event handlers
            controller.UpdateArrived += OnFrame;
            controller.ErrorOccurred += showMessageBoxError;
            controller.Connected += ControllerConnected;

            this.controller = controller;
            this.KeyDown += HandleKeyDown;
            this.KeyUp += HandleKeyUp;
            this.Controls.Add(drawingPanel);
            this.KeyPreview = false;
        }

        /// <summary>
        /// If successfully connected to the server, disables textbox controls
        /// </summary>
        private void ControllerConnected()
        {
            this.KeyPreview = true;
            this.Invoke((MethodInvoker)delegate
            {
                // Disable text controls
                this.serverTextBox.Enabled = false;
                this.playerNameTextBox.Enabled = false;
                this.connectButton.Enabled = false;
                this.drawingPanel.Focus();
            });
            OnFrame();
        }

        /// <summary>
        /// Method to force the form to repaint on each received frame
        /// </summary>
        private void OnFrame()
        {
            try
            {
                MethodInvoker invalidateForm = new MethodInvoker(() => this.Invalidate(true));
                this.Invoke(invalidateForm);
            }
            catch { }
        }

        /// <summary>
        /// Handles keyboard input for tank movement direction
        /// </summary>
        private void HandleKeyDown(object sender, KeyEventArgs e)
        {
            // Close the form if 'Escape' is pressed
            if (e.KeyCode == Keys.Escape)
            {
                this.Close();
            }
            if (e.KeyCode == Keys.W)
                controller.HandleMoveRequest("up");
            if (e.KeyCode == Keys.A)
                controller.HandleMoveRequest("left");
            if (e.KeyCode == Keys.S)
                controller.HandleMoveRequest("down");
            if (e.KeyCode == Keys.D)
                controller.HandleMoveRequest("right");

            // Prevent other key handlers from running
            e.SuppressKeyPress = true;
            e.Handled = true;
        }

        /// <summary>
        /// Handles key up events for movement
        /// </summary>
        private void HandleKeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.W)
                controller.HandleKeyUp("up");
            if (e.KeyCode == Keys.A)
                controller.HandleKeyUp("left");
            if (e.KeyCode == Keys.S)
                controller.HandleKeyUp("down");
            if (e.KeyCode == Keys.D)
                controller.HandleKeyUp("right");
        }

        /// <summary>
        /// Handle mouse button presses for firing modes
        /// </summary>
        private void HandleMouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                controller.HandleMouseRequest("main");
            }
            if (e.Button == MouseButtons.Right)
            {
                controller.HandleMouseRequest("alt");
            }
        }

        /// <summary>
        /// Handle mouse button releases
        /// </summary>
        private void HandleMouseUp(object sender, MouseEventArgs e)
        {
            controller.HandleMouseUp();
        }

        /// <summary>
        /// Handle mouse movement
        /// </summary>
        private void HandleMouseMove(object sender, MouseEventArgs e)
        {
            controller.HandleMouseMoveRequest(e.Location, viewSize);
        }

        /// <summary>
        /// Begins event loop for server connection in the controller
        /// </summary>
        private void connectButton_Click(object sender, EventArgs e)
        {
            if (serverTextBox.Text != "" && playerNameTextBox.Text != "")
            {
                controller.Connect(serverTextBox.Text, playerNameTextBox.Text);
            }
        }

        /// <summary>
        /// Method to display appropriate error messages
        /// </summary>
        /// <param name="message">The message to display</param>
        private void showMessageBoxError(String message)
        {
            MessageBox.Show(message);
        }
    }
}