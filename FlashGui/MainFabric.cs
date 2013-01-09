using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlashGui
{
    class MainFabric
    {
        static public DevicesModel createDevicesModel()
        {
            IDevicesView dv = new DevicesView();
            DevicesModel dm = new DevicesModel();

            dm.setDevicesView(dv);
            dv.setModel(dm);

            return dm;
        }
    }
}
