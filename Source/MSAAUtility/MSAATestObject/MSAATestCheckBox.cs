﻿/********************************************************************
*                      AutoTester     
*                        Wan,Yu
* AutoTester is a free software, you can use it in any commercial work. 
* But you CAN NOT redistribute it and/or modify it.
*--------------------------------------------------------------------
* Component: MSAATestCheckBox.cs
*
* Description: This class define the checkbox object for MSAA.
*
* History: 2008/04/23 wan,yu init version.
*
*********************************************************************/

using System;
using System.Collections.Generic;
using System.Text;

using Accessibility;

using Shrinerain.AutoTester.Core;
using Shrinerain.AutoTester.Win32;

namespace Shrinerain.AutoTester.MSAAUtility
{
    public class MSAATestCheckBox : MSAATestGUIObject, ICheckable, IShowInfo
    {

        #region fields

        #endregion

        #region properties


        #endregion

        #region methods

        #region ctor

        public MSAATestCheckBox(IAccessible iAcc)
            : this(iAcc, 0)
        {
        }

        public MSAATestCheckBox(IAccessible iAcc, int childID)
            : base(iAcc, childID)
        {
        }

        #endregion

        #region public methods

        #region IClickable Members

        public void Click()
        {
            try
            {
                _actionFinished.WaitOne();

                if (!_sendMsgOnly)
                {
                    Hover();

                    MouseOp.Click();
                }
                else
                {
                    _iAcc.accDoDefaultAction(_selfID);
                }

                _actionFinished.Set();

            }
            catch (TestException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CannotPerformActionException("Can not click button: " + ex.Message);
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
            throw new Exception("The method or operation is not implemented.");
        }

        public string GetAction()
        {
            return "Check";
        }

        public void DoAction(object parameter)
        {
            try
            {
                bool isCheck = true;

                if (parameter != null)
                {
                    isCheck = Convert.ToBoolean(parameter);
                }

                if (isCheck)
                {
                    Check();
                }
                else
                {
                    UnCheck();
                }
            }
            catch (TestException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CannotPerformActionException("Can not perform action: " + ex.Message);
            }
        }

        #endregion

        #region IShowInfo Members

        public string GetText()
        {
            return MSAATestObject.GetName(this._iAcc, Convert.ToInt32(this._selfID));
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

        #endregion

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
                throw new CannotPerformActionException("Can not check checkbox: " + ex.Message);
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
                throw new CannotPerformActionException("Can not uncheck checkbox: " + ex.Message);
            }
        }

        public bool IsChecked()
        {
            try
            {
                String state = MSAATestObject.GetState(_iAcc, (int)_selfID);
                return state.IndexOf("checked", StringComparison.CurrentCultureIgnoreCase) >= 0;
            }
            catch
            {
                return false;
            }

        }

        #endregion

        #endregion

        #region private methods


        #endregion

        #endregion

    }
}