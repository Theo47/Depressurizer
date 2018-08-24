using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Depressurizer
{
    public partial class DlgRandomGame : Form
    {
        private GameInfo game;

        //Constructor
        public DlgRandomGame(GameInfo game)
        {
            this.game = game;

            InitializeComponent();
        }

        private void DlgRandomGame_Load(object sender, EventArgs e)
        {
            DisplayGame();
        }

        private void DisplayGame()
        {
            //game id's less than zero denote a game external to steam.
            if (game.Id > 0)
            {
                //Check to see if the banner is already downloaded, if not, get it now
                String bannerFile = String.Format(Properties.Resources.GameBannerPath, Path.GetDirectoryName(Application.ExecutablePath), game.Id);
                if (!File.Exists(bannerFile))
                {
                    Utility.GrabBanner(game.Id);
                }

                try
                {
                    Image gameBannerImage = Image.FromFile(bannerFile);
                    gameBannerBox.Image = gameBannerImage;
                }
                catch (Exception e)
                {
                    //process the error
                    MessageBox.Show(GlobalStrings.RandomGame_Banner_Error, GlobalStrings.Gen_Warning, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    Program.Logger.WriteException($"Failed to load game banner for random game selection: ", e);
                }

            }

            gameTextBox.Text = game.Name;
        }

        private void btnLaunch_Click(object sender, EventArgs e)
        {
            if (game != null)
            {
                game.LastPlayed = Utility.GetCurrentUTime();
                System.Diagnostics.Process.Start(game.Executable);
            }
        }
    }
}
