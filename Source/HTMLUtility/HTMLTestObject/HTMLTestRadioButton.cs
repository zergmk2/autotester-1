/********************************************************************
*                      AutoTester     
*                        Wan,Yu
* AutoTester is a free software, you can use it in any commercial work. 
* But you CAN NOT redistribute it and/or modify it.
*--------------------------------------------------------------------
* Component: HTMLTestRadioButton.cs
*
* Description: This class defines the actions provide by Radio Button.
*              The important actions include "Check" , and "IsChecked".
*
* History: 2007/09/04 wan,yu Init version.
*          2008/01/24 wan,yu update, GetLabelForRadioBox(). 
*
*********************************************************************/

using System;
using System.Collections.Generic;
using System.Text;

using mshtml;

using Shrinerain.AutoTester.Interface;
using Shrinerain.AutoTester.Core;
using Shrinerain.AutoTester.Win32;

namespace Shrinerain.AutoTester.HTMLUtility
{
    public class HTMLTestRadioButton : HTMLTestGUIObject, ICheckable, IShowInfo, IStatus
    {
        #region fields

        protected IHTMLInputElement _radioElement;

        #endregion

        #region properties


        #endregion

        #region methods

        #region ctor

        public HTMLTestRadioButton(IHTMLElement element)
            : base(element)
        {

            this._type = HTMLTestObjectType.RadioButton;

            this._isDelayAfterAction = false;

            try
            {
                this._radioElement = (IHTMLInputElement)element;
            }
            catch (Exception ex)
            {
                throw new CannotBuildObjectException("Can not convert to IHTMLInputElement: " + ex.Message);
            }
        }

        #endregion

        #region public methods

        #region ICheckable Members

        public void Check()
        {
            try
            {
                if (!IsChecked())
                {
                    Click();
                }
            }
            catch (TestException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CannotPerformActionException("Can not perform Check action on radio button: " + ex.Message);
            }

        }

        public void UnCheck()
        {
            try
            {
                if (IsChecked())
                {
                    Click();
                }
            }
            catch (TestException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CannotPerformActionException("Can not perform UnCheck action on radio button: " + ex.Message);
            }
        }

        public bool IsChecked()
        {
            try
            {
                return _radioElement.@checked;
            }
            catch (Exception ex)
            {
                throw new CannotPerformActionException("Can not get status of radio button: " + ex.Message);
            }
        }

        #endregion

        #region IClickable Members

        public void Click()
        {
            try
            {
                if (!IsReady() || !_isEnable || !_isVisible || _isReadOnly)
                {
                    throw new CannotPerformActionException("Radiobutton is not enabled.");
                }

                _actionFinished.WaitOne();

                Hover();

                if (_sendMsgOnly)
                {
                    this._radioElement.@checked = true;
                }
                else
                {
                    MouseOp.Click();
                }

                if (_isDelayAfterAction)
                {
                    System.Threading.Thread.Sleep(_delayTime * 1000);
                }

                _actionFinished.Set();
            }
            catch (TestException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CannotPerformActionException("Can not click on the radio button: " + ex.Message);
            }
        }

        public void DoubleClick()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void RightClick()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void MiddleClick()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        #endregion

        #region IInteractive Members

        public void Focus()
        {
            try
            {
                Hover();
                MouseOp.Click();
            }
            catch (Exception ex)
            {
                throw new CannotPerformActionException("Can not focus on radiobox: " + ex.Message);
            }
        }

        public object GetDefaultAction()
        {
            return "Check";
        }

        public void PerformDefaultAction(object para)
        {
            Check();
        }

        #endregion


        #region IStatus Members

        /* object GetCurrentStatus()
         * get the readystate of element. 
         */
        public virtual object GetCurrentStatus()
        {
            try
            {
                if (_radioElement != null)
                {
                    return _radioElement.readyState;
                }
                else
                {
                    throw new CannotPerformActionException("Can not get status: Element can not be null.");
                }
            }
            catch (TestException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CannotPerformActionException("Can not get status: " + ex.Message);
            }
        }

        /* bool IsReady()
         * return true if the object is ready.
         */
        public virtual bool IsReady()
        {
            try
            {
                if (_radioElement != null)
                {
                    return _radioElement.readyState == null ||
                        _radioElement.readyState == "interactive" ||
                        _radioElement.readyState == "complete";
                }
                else
                {
                    throw new CannotPerformActionException("Can not get ready status: InputElement can not be null.");
                }
            }
            catch (TestException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CannotPerformActionException("Can not get ready status: " + ex.Message);
            }
        }

        #endregion

        #region IShowInfo Members

        public string GetText()
        {
            return LabelText;
        }

        public string GetFontFamily()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public string GetFontSize()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public string GetFontColor()
        {
            throw new Exception("The method or operation is not implemented.");
        }


        /* string GetLabelForRadioBox(IHTMLElement element)
         * return the text around radio button, firstly we will try current cell, then try right cell and up cell.
         */
        public static string GetLabelForRadioBox(IHTMLElement element)
        {
            try
            {
                //firstly, try to get text in the same cell/span/div/label
                string label = GetAroundText(element);

                //for radio button, we think the text on the right is it's label.
                if (!String.IsNullOrEmpty(label))
                {
                    if (!String.IsNullOrEmpty(label.Split(new string[] { _labelSplitter }, StringSplitOptions.RemoveEmptyEntries)[1]))
                    {
                        return label.Split(new string[] { _labelSplitter }, StringSplitOptions.RemoveEmptyEntries)[1];
                    }
                    else if (!String.IsNullOrEmpty(label.Split(new string[] { _labelSplitter }, StringSplitOptions.RemoveEmptyEntries)[0]))
                    {
                        return label.Split(new string[] { _labelSplitter }, StringSplitOptions.RemoveEmptyEntries)[0];
                    }
                }


                //we will search right cell and up cell.
                int[] searchDirs = new int[] { 1, 3, 0 };
                foreach (int currentDir in searchDirs)
                {
                    for (int deepth = 1; deepth < 4; deepth++)
                    {
                        label = GetAroundCellText(element, currentDir, deepth);

                        if (!String.IsNullOrEmpty(label.Trim()))
                        {
                            return label;//.Split(' ')[0];
                        }
                    }
                }


                return null;
            }
            catch
            {
                return null;
            }

        }
        #endregion


        #region private methods

        /* string GetAroundText()
         * return the text around the radio button.
         */
        protected override string GetLabelText()
        {
            return GetLabelForRadioBox(this._sourceElement);
        }

        #endregion

        #endregion

        #endregion
    }
}
