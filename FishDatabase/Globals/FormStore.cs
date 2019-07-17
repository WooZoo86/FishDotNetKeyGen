using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FishDatabase.Globals
{
    class FormStore
    {
        private static Hashtable _htbExistedForm;

        static FormStore()
        {

        }

        public FormStore()
        {

        }

        private void FreeInstance<T>(ref T frmObj) where T : Form
        {
            frmObj.Dispose();
            frmObj = default(T);
        }

        private static T GetInstance<T>(T frmObj) where T : Form, new()
        {
            T result;
            if (null != frmObj && !frmObj.IsDisposed)
            {
                result = frmObj;
            }
            else
            {
                if (null != _htbExistedForm)
                {
                    if (_htbExistedForm.ContainsKey(typeof(T)))
                        throw new InvalidOperationException("");
                }
                else
                    _htbExistedForm = new Hashtable();

                _htbExistedForm.Add(typeof(T), null);
                try
                {
                    result = Activator.CreateInstance<T>();
                }
                catch (TargetInvocationException ex) when(null != ex)
                {
                    throw new InvalidOperationException("");
                }
                finally
                {
                    _htbExistedForm.Remove(typeof(T));
                }
            }

            return result;
        }
    }
}
