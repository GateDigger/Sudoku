using System;
using System.Windows.Forms;

namespace Sudoku.GUI
{
    public abstract class Controller<ContainerType, ContaineeType>
        where ContainerType : Control
        where ContaineeType : Control
    {
        ContainerType container;
        public event ControlEventHandler ContainerChanged;
        public ContainerType Container
        {
            get
            {
                return container;
            }
            set
            {
                if (containee != null)
                {
                    if (container != null)
                        container.Controls.Remove(containee);
                    container = value;
                    if (value != null)
                        value.Controls.Add(containee);
                }
                else
                {
                    container = value;
                }
                OnContainerChanged(new ControlEventArgs(value));
            }
        }
        protected virtual void OnContainerChanged(ControlEventArgs e)
        {
            if (ContainerChanged != null)
                ContainerChanged(this, e);
        }

        ContaineeType containee;
        public event ControlEventHandler ContaineeChanged;
        public ContaineeType Containee
        {
            get
            {
                return containee;
            }
            set
            {
                if (container != null)
                {
                    if (containee != null)
                        container.Controls.Remove(containee);
                    containee = value;
                    if (value != null)
                        container.Controls.Add(value);
                }
                else
                {
                    containee = value;
                }
                OnContaineeChanged(new ControlEventArgs(value));
            }
        }
        protected virtual void OnContaineeChanged(ControlEventArgs e)
        {
            if (ContaineeChanged != null)
                ContaineeChanged(this, e);
        }
    }

    public delegate void ControlEventHandler(object sender, ControlEventArgs e);
    public class ControlEventArgs : EventArgs
    {
        public ControlEventArgs(Control control)
        {
            Control = control;
        }
        public Control Control
        {
            get;
            protected set;
        }
    }
}