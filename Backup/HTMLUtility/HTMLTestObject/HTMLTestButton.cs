using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using mshtml;

using Shrinerain.AutoTester.Win32;
using Shrinerain.AutoTester.Function;
using Shrinerain.AutoTester.Function.Interface;

namespace Shrinerain.AutoTester.HTMLUtility
{
    public class HTMLTestButton : HTMLGuiTestObject, IClickable, IShowInfo
    {

        #region fields

        protected string _currentStr;

        protected IHTMLInputElement _inputElement;

        #endregion

        #region properties


        #endregion

        #region methods

        #region ctor

        public HTMLTestButton(IHTMLElement element)
            : base(element)
        {
            try
            {
                this._currentStr = element.getAttribute("value", 0).ToString();
            }
            catch
            {
                this._currentStr = "";
            }

            try
            {
                this._inputElement = (IHTMLInputElement)element;
            }
            catch (Exception e)
            {
                throw new CanNotBuildObjectException("Can not build test button: " + e.Message);
            }
        }

        #endregion

        #region public methods

        public virtual void Click()
        {
            try
            {
                _actionFinished.WaitOne();

                Hover();
                MouseOp.Click();

                _actionFinished.Set();

            }
            catch (Exception e)
            {
                throw new CanNotPerformActionException("Can not perform click action: " + e.Message);
            }
        }

        public virtual void DoubleClick()
        {
            try
            {
                _actionFinished.WaitOne();

                Hover();
                MouseOp.DoubleClick();

                _actionFinished.Set();
            }
            catch
            {
                throw new CanNotPerformActionException("Can not perform double click action.");
            }
        }

        public virtual void RightClick()
        {
            try
            {
                _actionFinished.WaitOne();

                Hover();
                MouseOp.RightClick();

                _actionFinished.Set();
            }
            catch
            {
                throw new CanNotPerformActionException("Can not perform right click action.");
            }
        }

        public virtual void MiddleClick()
        {
            throw new CanNotPerformActionException("Can not perform middle click.");
        }

        public virtual void Focus()
        {
            Click();
        }

        public virtual object GetDefaultAction()
        {
            return "Click";
        }

        public virtual void PerformDefaultAction()
        {
            Click();
        }


        public virtual string GetText()
        {
            return this._currentStr;
        }

        public virtual string GetFontStyle()
        {
            throw new PropertyNotFoundException();
        }

        public virtual string GetFontFamily()
        {
            throw new PropertyNotFoundException();
        }

        #endregion

        #region private methods


        #endregion

        #endregion


    }
}