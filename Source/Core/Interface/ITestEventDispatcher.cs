﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Shrinerain.AutoTester.Core
{
    //this interface defines event we need to support.
    //for different modules, like .Net or Native Win32, we all need to fire these events.
    //Recorders will handle these event, to generate correct actions.
    public interface ITestEventDispatcher
    {
        bool Start(ITestApp app);
        void Stop();

        event TestObjectEventHandler OnCheck;
        event TestObjectEventHandler OnUncheck;

        event TestObjectEventHandler OnClick;

        event TestObjectEventHandler OnDrag;

        event TestObjectEventHandler OnKeyDown;
        event TestObjectEventHandler OnKeyUp;

        event TestObjectEventHandler OnMouseDown;
        event TestObjectEventHandler OnMouseUp;
        event TestObjectEventHandler OnMouseClick;

        event TestObjectEventHandler OnTextChange;

        event TestObjectEventHandler OnFocus;

        event TestObjectEventHandler OnSelectIndexChange;
        event TestObjectEventHandler OnSelect;
        event TestObjectEventHandler OnUnselect;

        event TestObjectEventHandler OnStatusChange;

        event TestObjectEventHandler OnShow;
    }
}
