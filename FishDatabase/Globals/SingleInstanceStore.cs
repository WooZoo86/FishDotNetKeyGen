using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using FishDatabase.Forms;

namespace FishDatabase.Globals
{
    internal sealed class SingleInstanceStore
    {
        private static readonly SigleInstanceTemplate<MainApp> _mainApp = new SigleInstanceTemplate<MainApp>();
        private static readonly SigleInstanceTemplate<FormStore> _frmStore = new SigleInstanceTemplate<FormStore>();
        internal static MainApp theApp => _mainApp.Instance;
        internal static FormStore theStore => _frmStore.Instance;


        internal sealed class MainApp : ApplicationContext
        {
            public MainApp()
            {
                MainForm = frmMain.GetInstance();
            }

            protected override void OnMainFormClosed(object sender, EventArgs e)
            {
                base.OnMainFormClosed(sender, e);
            }
        }

        internal sealed class SigleInstanceTemplate<T> where T : new()
        {
            private static T _instance;

            internal T Instance
            {
                get
                {
                    if (null == _instance)
                        _instance = Activator.CreateInstance<T>();

                    return _instance;
                }
            }
        }
    }
}
