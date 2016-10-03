using System;
using System.Collections.Generic;
using System.Linq;
using UkrPochta.Data.Entity;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;


// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace UkrPochta
{
    public delegate void SelectChangedEventHandler(object sender, object e);
    public delegate void VisibleChangedEventHandler(object sender, bool visible);
    public delegate void ExpandChangedEventHandler(object sender, bool expand);

    public sealed partial class ListDataControl : UserControl
    {
        public event SelectChangedEventHandler SelectChanged;
        public event VisibleChangedEventHandler VisibleChanged;
        public event ExpandChangedEventHandler ExpandChanged;
        public bool IsExpand
        {
            get { return _isExpand; }
            set { _isExpand = value; }
        }
        private void OnExpandChanged()
        {
            IsExpand = !IsExpand;    
            ExpandChanged?.Invoke(this, IsExpand);

        }
        private void OnVisibleChanged(bool visible)
        {
            VisibleChanged?.Invoke(this, visible);
        }
        private void OnSelectedChanged(object e)
        {
            SelectChanged?.Invoke(this, e);
            IsExpand = false;
            if (e==null)
                OnVisibleChanged(false);
            else
                OnVisibleChanged(true);
        }
        private IEnumerable<Base> _obj;
        private bool _isExpand;

        /* private Type _t;
public Type TypeObj {
get { return this._t; }
set { this._t = value; }
}*/
        public IEnumerable<Base> Obj
        {
            get { return this._obj; }
            set {
                this._obj = value;
                ListAddr.ItemsSource = _obj;
                if (_obj == null || _obj.Count() == 0)
                {
                    OnSelectedChanged(null);
                    IsExpand = false;
                }
                else
                    IsExpand = true;              
                   
            }
        }
        public ListDataControl()
        {
            this.InitializeComponent();
        }

        private void CloseList(object sender, TappedRoutedEventArgs e)
        {                    
            Obj = null;                               
        }


        private void ClickItem(object sender, ItemClickEventArgs e)
        {
            Base item = (Base)e.ClickedItem;
            TxtDict.Text = "Справочник: " + item.Name;
            OnSelectedChanged(item);
        }

        private void ExpandList(object sender, TappedRoutedEventArgs e)
        {                     
            OnExpandChanged();
        }
    }
}
