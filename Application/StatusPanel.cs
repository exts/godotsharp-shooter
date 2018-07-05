using Godot;

namespace SpaceShooter.Application
{
    public class StatusPanel : Node2D
    {
        [Signal]
        public delegate void ResetGame();

        private Label _panelLabel;
        private Button _panelButton;
        
        public override void _Ready()
        {
            _panelLabel = (Label) GetNode("Control/Panel/Title");
            
            _panelButton = (Button) GetNode("Control/Panel/Button");
            _panelButton.Connect("pressed", this, nameof(Restart));
        }
        
        public void ShowStartGamePanel()
        {
            _panelLabel.Text = "New Game";
            _panelButton.Text = "Start";
            Show();
        }

        public void ShowGameOverPanel()
        {
            _panelLabel.Text = "Game Over";
            _panelButton.Text = "Play Again";
            Show();
        }

        public void Restart()
        {
            EmitSignal(nameof(ResetGame));
        }
    }
}