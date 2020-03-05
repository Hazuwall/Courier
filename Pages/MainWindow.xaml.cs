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

namespace Courier
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : System.Windows.Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            World world = new WorldGenerator()
                .GenerateBuilding()
                .GenerateFurniture()
                .GenerateNPC()
                .GenerateRobot(out Robot robot)
                .World;

            foreach (var obj in world.Objects)
                FloorCanvas.Children.Add(obj.View);

            System.Windows.Window window = new ProbabilityWindow(robot.NavigationSystem);
            window.Show();
            var sim = new Simulation(world);
            await sim.SimulateAsync();
        }
    }
}
