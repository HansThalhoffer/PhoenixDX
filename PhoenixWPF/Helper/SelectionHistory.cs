using PhoenixModel.dbErkenfara;
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
                Main.Instance.PropertyDisplay?.Display(selected);
            }
            return selected;
        }

        public ISelectable? NavigateForward()
        {
            if (_index < this.Count-1)
                _index++;
            return _OnSelectionChange(Current);
        }

        /// <summary>
        /// gibt das aktuell ausgewählte Objekt zurück
        /// beim Setzen des Objektes wird die Queue abgeschnitten, wenn bereits zurück nagigiert wurde
        /// </summary>
        public ISelectable? Current
        { 
            get
            {   if (_index < 0)
                    return null;
                return this[_index];
            }
            set
            {
                if (value != null && value.Select() &&  Current != value)
                {
                    if (_index >= 0)
                    {
                        // hiser soll abgeschnitten werden bis zum aktuellen Eintrag, falls bereits zurücknavigiert wurde
                        while (this.Count > _index + 1)
                            this.RemoveAt(this.Count-1);
                    }
                    this.Add(value);
                    NavigateForward();
                    _OnSelectionChange(Current);
                }
            }
        }
    }
}
