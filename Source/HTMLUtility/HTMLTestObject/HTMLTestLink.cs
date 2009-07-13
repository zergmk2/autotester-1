/********************************************************************
*                      AutoTester     
*                        Wan,Yu
* AutoTester is a free software, you can use it in any commercial work. 
* But you CAN NOT redistribute it and/or modify it.
*--------------------------------------------------------------------
* Component: HTMLTestLink.cs
*
* Description: This class defines the actions provide by Link.
*              The important actions is "Click". 
*
* History: 2007/09/04 wan,yu Init version
*          2008/01/12 wan,yu update, remove HTMLTestObject[] GetLinkChildren()
* 
*********************************************************************/

using System;
using System.Threading;

using mshtml;

using Shrinerain.AutoTester.Core;
using Shrinerain.AutoTester.Win32;

namespace Shrinerain.AutoTester.HTMLUtility
{
    public class HTMLTestLink : HTMLTestGUIObject, IClickable, IText, IStatus
    {

        #region fields

        //the url of this link
        protected string _href;
        //the text of the link if it is a text link.
        protected string _linkText;
        //the image of the link if it is a image link.
        protected IHTMLImgElement _linkImgElement;
        protected string _linkImgSrc;
        // the HTML element of link.
        protected HTMLAnchorElement _acnchorElement;

        //for link, we can have child image.
        protected object[] _childeren;

        #endregion

        #region properties

        public string LinkText
        {
            get { return _linkText; }
        }

        public string LinkImage
        {
            get { return _linkImgSrc; }
        }

        public string Href
        {
            get { return _href; }
        }

        #endregion

        #region methods

        #region ctor
        public HTMLTestLink(IHTMLElement element)
            : this(element, null)
        {
        }
        public HTMLTestLink(IHTMLElement element, HTMLTestBrowser browser)
            : base(element, browser)
        {
            this._isDelayAfterAction = true;
            this._type = HTMLTestObjectType.Link;
            try
            {
                _acnchorElement = (HTMLAnchorElement)element;
            }
            catch (Exception ex)
            {
                throw new CannotBuildObjectException("Can not convert to HTMLAnchorElement: " + ex.ToString());
            }

            try
            {
                //get the link text
                _linkText = _acnchorElement.innerText;
            }
            catch
            {
                _linkText = "";
            }

            // if the text is null, it maybe a image link, try to get the image.
            if (String.IsNullOrEmpty(_linkText))
            {
                try
                {
                    _linkImgElement = _acnchorElement.firstChild as IHTMLImgElement;
                    if (_linkImgElement != null)
                    {
                        _linkImgSrc = _linkImgElement.src;
                    }
                }
                catch
                {
                }
            }

            try
            {
                //get the url of the link.
                _href = _acnchorElement.href;
            }
            catch (Exception ex)
            {
                throw new CannotBuildObjectException("Can not get href of link: " + ex.ToString());
            }
        }

        #endregion

        #region public methods

        /* void Click()
         * Click on link
         */
        public virtual void Click()
        {
            try
            {
                BeforeAction();

                Thread t = new Thread(new ThreadStart(PerformClick));
                t.Start();
                t.Join(ActionTimeout);
            }
            catch (TestException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CannotPerformActionException("Can not perform click action of link: " + ex.ToString());
            }
            finally
            {
                AfterAction();
            }
        }

        public virtual void DoubleClick()
        {
        }

        public virtual void RightClick()
        {
        }

        public virtual void MiddleClick()
        {
        }

        public virtual string GetAction()
        {
            return "Click";
        }

        public virtual void DoAction(object para)
        {
            Click();
        }

        #region IText Members

        public string GetText()
        {
            return this._linkText;
        }

        public override string GetLabel()
        {
            return GetText();
        }

        public string GetFontFamily()
        {
            return null;
        }

        public string GetFontSize()
        {
            return null;
        }

        public string GetFontColor()
        {
            return null;
        }

        #endregion

        #endregion

        #endregion
    }
}
