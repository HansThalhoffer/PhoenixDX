using PhoenixModel.Program;
using PhoenixWPF.Program;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhoenixWPF.Helper
{
    
    public class SelectionHistory: List<ISelectable>
    {
        int _index = -1;
        public ISelectable? NavigateBack()
        {
            if (_index < 0)
                return null;
            if (_index > 0)
                _index--;
            return _OnSelectionChange(Current);
        }

        protected ISelectable? _OnSelectionChange(ISelectable? selected)
        {
            if (selected != null)
            {
                Main.Instance.PropertyDisplay?.Display(selected.Eigenschaften);
            }
            return selected;
        }

        public ISelectable? NavigateForward()
        {
            if (_index < this.Count-1)
                _index++;
            return _OnSelectionChange(Current);
        }

        public ISelectable? Current
        { 
            get
            {   if (_index < 0)
                    return null;
                return this[_index];
            }
            set
            {
                if (Current != value)
                {
                    if (_index >= 0)
                    {
                        // stimmt die Rechnung?
                        while (this.Count > _index + 1)
                            this.RemoveAt(this.Count-1);
                    }
                    if (value != null)
                    {
                        this.Add(value);
                        NavigateForward();
                    }
                }
                else
                {
                    _OnSelectionChange(Current);
                }
            }
        }
    }
}
