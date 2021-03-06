﻿/********************************************************************
*                      AutoTester     
*                        Wan,Yu
* AutoTester is a free software, you can use it in any commercial work. 
* But you CAN NOT redistribute it and/or modify it.
*--------------------------------------------------------------------
* Component: TestAppException.cs
*
* Description: This component defines expcetions used for desktop application.
*
* History: 2007/12/28 wan,yu Init version
*
*********************************************************************/


using System;
using System.Collections.Generic;
using System.Text;

namespace Shrinerain.AutoTester.Core.TestExceptions
{

    public class TestAppException : TestException
    {
        public TestAppException()
            : this("Error of application under test.")
        {

        }
        public TestAppException(string message)
            : base(message)
        {

        }

        public TestAppException(string appName, string message)
            : base(message)
        {
            this._app = appName;
        }

    };

    public class AppNotFoundExpcetion : TestAppException
    {
        public AppNotFoundExpcetion()
            : this("Can not find test app.")
        {
        }

        public AppNotFoundExpcetion(String message)
            : base(message)
        {
        }

        public AppNotFoundExpcetion(string appName, string message)
            : base(appName, message)
        {
        }
    }

    public class CannotStartAppException : TestAppException
    {
        public CannotStartAppException()
            : this("Can not start test application.")
        {
        }
        public CannotStartAppException(string message)
            : base(message)
        {
        }

    }

    public class CannotStopAppException : TestAppException
    {
        public CannotStopAppException()
            : this("Can not stop test application.")
        {
        }

        public CannotStopAppException(string message)
            : base(message)
        {
        }
    }

    public class CannotActiveAppException : TestAppException
    {
        public CannotActiveAppException()
            : this("Can not active test application.")
        {
        }

        public CannotActiveAppException(string message)
            : base(message)
        {
        }
    }

    public class CannotAttachAppException : TestAppException
    {
        public CannotAttachAppException()
            : this("Can not attach test application.")
        {

        }

        public CannotAttachAppException(string message)
            : base(message)
        {
        }
    }

    public class CannotMoveAppException : TestAppException
    {
        public CannotMoveAppException()
            : this("Can not move test application.")
        {
        }

        public CannotMoveAppException(string message)
            : base(message)
        {

        }
    }

    public class CannotResizeAppException : TestAppException
    {
        public CannotResizeAppException()
            : this("Can not resize test application.")
        {
        }

        public CannotResizeAppException(string message)
            : base(message)
        {

        }
    }

    public class CannotMaxAppException : TestAppException
    {
        public CannotMaxAppException()
            : this("Can not maxsize test application.")
        {
        }

        public CannotMaxAppException(string message)
            : base(message)
        {
        }
    }

    public class CannotWaitAppException : TestAppException
    {
        public CannotWaitAppException()
            : this("Can not wait test application.")
        {
        }

        public CannotWaitAppException(string message)
            : base(message)
        {
        }
    }

    public class CannotGetAppStatusException : TestAppException
    {
        public CannotGetAppStatusException()
            : this("Can not get status of test application.")
        {
        }

        public CannotGetAppStatusException(string message)
            : base(message)
        {
        }
    }

    public class CannotGetAppProcessException : TestAppException
    {
        public CannotGetAppProcessException()
            : this("Can not get process of test application.")
        {
        }

        public CannotGetAppProcessException(string message)
            : base(message)
        {
        }

    }

    public class CannotGetAppNetworkException : TestAppException
    {
        public CannotGetAppNetworkException()
            : this("Can not get netowork information of test application.")
        {
        }

        public CannotGetAppNetworkException(string message)
            : base(message)
        {
        }
    }

    public class CannotGetAppInfoException : TestAppException
    {
        public CannotGetAppInfoException()
            : this("Can not get information of test application.")
        {
        }

        public CannotGetAppInfoException(string message)
            : base(message)
        {
        }
    }
}
