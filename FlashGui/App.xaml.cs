using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace FlashGui
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        private DevicesModel devicesModel;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            devicesModel = MainFabric.createDevicesModel();
            devicesModel.start();

            DevicesView devicesView = (DevicesView)devicesModel.getDevicesView();
            devicesView.Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            devicesModel.stop();

            base.OnExit(e);
        }
    
    }
}
