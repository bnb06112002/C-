using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using UkrPochta.Data.Entity;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace UkrPochta
{
    public delegate void SelectChangedEventHandler(object sender, object e);
    public delegate void VisibleChangedEventHandler(object sender, bool visible);

    public sealed partial class ListDataControl : UserControl
    {
        public event SelectChangedEventHandler SelectChanged;
        public event VisibleChangedEventHandler VisibleChanged;
        private void OnVisibleChanged(bool visible)
        {
            VisibleChanged?.Invoke(this, visible);
        }
        private void OnSelectedChanged(object e)
        {
            SelectChanged?.Invoke(this, e);
            if (e==null)
                OnVisibleChanged(false);
            else
                OnVisibleChanged(true);
        }
        private IEnumerable<Base> _obj;
        private Type _t;
        public Type TypeObj {
            get { return this._t; }
            set { this._t = value; }
        }
        public IEnumerable<Base> Obj
        {
            get { return this._obj; }
            set {
                this._obj = value;
                ListAddr.ItemsSource = _obj;
                if(_obj==null || _obj.Count() == 0)                
                    OnSelectedChanged(null);
                   
                   

            }
        }
        public ListDataControl()
        {
            this.InitializeComponent();
        }

        private void CloseList(object sender, TappedRoutedEventArgs e)
        {
            var index = ListAddr.SelectedIndex;            
            ListAddr.ItemsSource = null;
            if (index==-1)
                OnSelectedChanged(null);            
        }

        private void ChengeClick(object sender, SelectionChangedEventArgs e)
        {
          /*  Base item=null;
            if ((sender as ListView).Items.Count > 0&& e.AddedItems.Count>0)
               item = (Base)e.AddedItems[0];
            OnSelectedChanged(item); */          

        }

        private void ClickItem(object sender, ItemClickEventArgs e)
        {
            Base item = (Base)e.ClickedItem;  
            OnSelectedChanged(item);
        }
    }
}
